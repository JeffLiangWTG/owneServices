using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Xml.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.IE.Business;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration.Customs.IE;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using CusEntryHeader = Enterprise.Customs.IE.Business.Declaration.CusEntryHeader;
using EDIMessage = Enterprise.Messaging.Business.EDIMessage;
using GlbCompanyWrapper = Enterprise.Customs.IE.Business.GlbCompanyWrapper;

namespace Enterprise.Customs.IE.Messaging.Testing
{
	class MessageCreatorTest : TestCaseWithFactory
	{
		const string CustomsSchemaUrl = @"http://www.ros.ie/schemas/customs";
		const string CustomsCollectResponseSchemaUrl = @"http://www.ros.ie/schemas/customs/collectresponse/v1";
		const string CustomsIM416SchemaUrl = @"http://www.ros.ie/schemas/customs/IM416";
		const string CustomsIM415VH7SchemaUrl = @"http://www.ros.ie/schemas/customs/IM415VH7";

		public void TestMessageNumber()
		{
			InterchangeProcessorTestHelper.CreateInboundEDIMessage<AISInboundEDIMessage>(Factory, AISInterchangeTypeList.Codes.IM917, "<MailboxId>MAILBOXID0012B</MailboxId>", "TRANSACTIONID002");
			InterchangeProcessorTestHelper.CreateInboundEDIMessage<AISInboundEDIMessage>(Factory, AISInterchangeTypeList.Codes.IM917, "<MailboxId>MAILBOXID003</MailboxId>", "TRANSACTIONID003B");
			InterchangeProcessorTestHelper.CreateInboundEDIMessage<AESInboundEDIMessage>(Factory, AISInterchangeTypeList.Codes.IM917, "<MailboxId>MAILBOXID004</MailboxId>", "TRANSACTIONID004");
			var interchange = InterchangeProcessorTestHelper.CreateIncomingInterchange(Factory, EDIInterchange.ApplicationCodes.IECustomsCommon, CommonInterchangeTypeList.Codes.MailboxRequest,
				InterchangeProcessorTestHelper.GetMailboxCollectResponseMessage(
					AISInterchangeProcessorTestHelper.GetStandardIM917InterchangeText("TRANSACTIONID002", "MAILBOXID002", includeResponseWrap: false, includeEncoding: false),
					AISInterchangeProcessorTestHelper.GetStandardIM917InterchangeText("TRANSACTIONID003", "MAILBOXID003", includeResponseWrap: false, includeEncoding: false),
					AISInterchangeProcessorTestHelper.GetStandardIM917InterchangeText("TRANSACTIONID004", "MAILBOXID004", includeResponseWrap: false, includeEncoding: false)));
			interchange.EI_InterchangeNum = "EI_InterchangeNum_LongerThan_EM_MessageNumMaxLength";
			Factory.Save();
			((IInboundMessageCreator)new MessageCreator(logger)).CreateMessagesForInterchange(interchange);
			AssertEquals("interchange.ContainedMessages.Count", 3, interchange.ContainedMessages.Count);
			AssertEquals("Message Number", "LongerThan_EM_MessageNumMaxLength01", interchange.ContainedMessages[0].EM_MessageNum);
			AssertEquals("Message Number", "LongerThan_EM_MessageNumMaxLength02", interchange.ContainedMessages[1].EM_MessageNum);
			AssertEquals("Message Number", "LongerThan_EM_MessageNumMaxLength03", interchange.ContainedMessages[2].EM_MessageNum);
		}

		public void TestMailboxAcknowledgeIsCreated()
		{
			var company = Factory.New<GlbCompany>();
			company.GC_Code = "GC7";
			company.GC_RN_NKCountryCode = Core.Constants.CountryCodes.Ireland;
			var branch = company.Branches.AddNew();
			var wrapper = (IIEGlbCompanyWrapper)GlbCompanyWrapper.GetWrapper<GlbCompanyWrapper>(company);
			var certificationData = wrapper.GetGlbExternalPasswordOrCreateNew();
			var outgoingInterchange = InterchangeCreator.CreateOutgoingInterchange(Factory, EDIInterchange.ApplicationCodes.IECustomsCommon, CommonInterchangeTypeList.Codes.MailboxRequest, branch.PK, "https://www.endpoint.com", "<GREETING>HELLO</GREETING>");
			outgoingInterchange.EI_Status = EDIInterchange.Status.Sent;
			var incomingInterchange = InterchangeProcessorTestHelper.CreateIncomingInterchange(Factory, EDIInterchange.ApplicationCodes.IECustomsCommon, CommonInterchangeTypeList.Codes.MailboxRequest, InterchangeProcessorTestHelper.GetMailboxCollectResponseMessage(
				AISInterchangeProcessorTestHelper.GetAISVersion2_0IM415VInterchangeText("TRAN001", "A", "LRN001", "MRN001", new ZDateTime(2023, 08, 11), mailboxId: "MAILBOXID1", includeResponseWrap: false, includeEncoding: false),
				AISInterchangeProcessorTestHelper.GetStandardIM917InterchangeText("TRAN002", mailboxId: "MAILBOXID2", includeResponseWrap: false, includeEncoding: false)));
			incomingInterchange.EI_TransportType = EDIInterchangeTransportTypeList.Codes.xT;
			incomingInterchange.EI_SessionGUID = outgoingInterchange.EI_SessionGUID;
			incomingInterchange.EI_Status = EDIInterchange.Status.Queued;
			incomingInterchange.EI_GB = branch.PK;
			incomingInterchange.EI_InterchangeNum = "IE342";
			var helper = new WebServiceEndPointProviderTestHelper(Factory);
			var url = helper.SetupMailboxAcknowledgeURL();
			Factory.Save();

			var logger = new LoggingInformation();
			((IInboundMessageCreator)new MessageCreator(logger)).CreateMessagesForInterchange(incomingInterchange);
			CombineAssertions(() =>
			{
				var query = new ZQuery(EDIInterchangeSchema.EI_ApplicationCode, EDIInterchange.ApplicationCodes.IECustomsCommon);
				query.AddToFilter(EDIInterchangeSchema.EI_InterchangeType, CommonInterchangeTypeList.Codes.MailboxAcknowledge);
				query.AddToFilter(EDIInterchangeSchema.EI_ReceiveTransmit, EDIInterchange.Direction.Transmit);
				query.AddToFilter(EDIInterchangeSchema.EI_TransportType, EDIInterchangeTransportTypeList.Codes.xT);
				query.AddToFilter(EDIInterchangeSchema.EI_Status, EDIInterchange.Status.Queued);
				query.AddToFilter(EDIInterchangeSchema.EI_GP, certificationData.PK);
				query.AddToFilter(EDIInterchangeSchema.EI_GB, branch.PK);
				var acknowledgeInterchange = Factory.Load<EDIInterchange>(query).Single();
				AssertEquals("acknowledgeInterchange.EI_HeaderText", $@"{{""custom.IE.Endpoint"":""{url}""}}", acknowledgeInterchange.EI_HeaderText);
				AssertNotEquals("acknowledgeInterchange.EI_SessionGUID", ZGuid.Empty, acknowledgeInterchange.EI_SessionGUID);
				var acknowledgeRequestUrl = @"http://www.ros.ie/schemas/customs/acknowledgerequest/v1";
				var nsMap = new Dictionary<string, string>
				{
					{ "MailboxAcknowledgeRequest",  acknowledgeRequestUrl },
					{ "MailboxId", acknowledgeRequestUrl }
				};

				AssertXmlPathValue(
					acknowledgeInterchange.EI_BodyText,
					["MailboxAcknowledgeRequest", "MailboxId"],
					nsMap,
					expectedValue: "MAILBOXID1");

				AssertXmlPathValue(
					acknowledgeInterchange.EI_BodyText,
					["MailboxAcknowledgeRequest", "MailboxId"],
					nsMap,
					expectedValue: "MAILBOXID2",
					numberOfMatchedElementsToSkip: 1);

				AssertContains("Logs", "\tProcessing Interchange (Type:MBR, Number:IE342).\r\n\tProcessing 2 mailbox items.\r\n\tCreated message (IEI, 15V, RCV, IE34201, TRAN001).\r\n\tCreated message (IEI, 917, RCV, IE34202, TRAN002).\r\n\tCreated MBA Interchange 'IE342A'.", string.Join("\r\n", logger.UserLogStrings.Cast<string>()));
			});
		}

		public void TestProcessingMessageAcknowledgement()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var eun = helper.CreateNewOrGetExistingDataGrouping(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, "European Union");
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Ireland, parent: eun);
			helper.CreateNewOrGetExistingCusCodeType(UniversalReferenceConstants.RefCusCodeListTypes.RevenueErrorType, "IEROS");
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Ireland, UniversalReferenceConstants.RefCusCodeListTypes.RevenueErrorType, "ROS-212003", "Test Error DESCRIPTION", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			Factory.Save();

			var testInterchange = Factory.New<EDIInterchange>();
			testInterchange.EI_InterchangeType = CommonInterchangeTypeList.Codes.MailboxRequest;
			testInterchange.EI_InterchangeNum = "IE342";
			testInterchange.EI_BodyText = @"<env:Envelope xmlns:env=""http://www.w3.org/2003/05/soap-envelope""><env:Header/><env:Body><ns2:MessageAcknowledgement xmlns:ns2=""http://www.ros.ie/schemas/customs/messageacknowledgement/v1""><ns2:ErrorReference><ns2:ErrorCode>ROS-212003</ns2:ErrorCode></ns2:ErrorReference></ns2:MessageAcknowledgement></env:Body></env:Envelope>";
			Factory.Save();

			var logger = new LoggingInformation();
			((IInboundMessageCreator)new MessageCreator(logger)).CreateMessagesForInterchange(testInterchange);
			CombineAssertions(() =>
			{
				AssertEquals("No ediMessage generated", 0, testInterchange.ContainedMessages.Count);
				AssertEquals("EI_Status", EDIInterchange.Status.Error, testInterchange.EI_Status);
				var log = testInterchange.Logs.GetAllLogs()[0];

				AssertEquals("SL_SE_NKEvent", Events.ErrorReport.Code, log.SL_SE_NKEvent);
				AssertEquals("SL_Reference", "Error submitting mailbox request. Error Code: ROS-212003 - Test Error DESCRIPTION", log.SL_Reference);
				AssertContains("Logs", "Processing Interchange (Type:MBR, Number:IE342).\r\n\tInterchange #IE342: Status set to 'ERR' due to the following error: Error submitting mailbox request. Error Code: ROS-212003 - Test Error DESCRIPTION", string.Join("\r\n", logger.UserLogStrings.Cast<string>()));
			});
		}

		public void TestCreateMessagesForInterchange_EmptyBodyText()
		{
			var testInterchange = Factory.New<EDIInterchange>();
			testInterchange.EI_InterchangeNum = "IE342";
			var logger = new LoggingInformation();
			((IInboundMessageCreator)new MessageCreator(logger)).CreateMessagesForInterchange(testInterchange);
			CombineAssertions(() =>
			{
				AssertEquals("No ediMessage generated", 0, testInterchange.ContainedMessages.Count);
				AssertEquals("EI_Status", EDIInterchange.Status.Error, testInterchange.EI_Status);
				var log = testInterchange.Logs.GetAllLogs()[0];

				AssertEquals("SL_SE_NKEvent", Events.ErrorReport.Code, log.SL_SE_NKEvent);
				AssertEquals("SL_Reference", "EI_BodyText does not contain a valid SOAP Envelope Body or the Body section is empty.", log.SL_Reference);
				AssertContains("Logs", "Processing Interchange (Type:, Number:IE342).\r\n\tInterchange #IE342: Status set to 'ERR' due to the following error: EI_BodyText does not contain a valid SOAP Envelope Body or the Body section is empty.", string.Join("\r\n", logger.UserLogStrings.Cast<string>()));
			});
		}

		public void TestFetchHintIsAddedForDuplicateCheck()
		{
			var factory1 = new BusinessObjectFactory() { RefreshEnabled = false };
			var factory2 = new BusinessObjectFactory() { RefreshEnabled = false };
			var mailItems = new List<string>();
			for (var i = 1; i < 11; i++)
			{
				var mailboxId = "MAILBOXID" + i.ToString();
				var transactionId = "TRANSACTIONID" + i.ToString();
				InterchangeProcessorTestHelper.CreateInboundEDIMessage<AISInboundEDIMessage>(factory1, AISInterchangeTypeList.Codes.IM917, $"<MailboxId>{mailboxId}</MailboxId>", transactionId);
				mailItems.Add(AISInterchangeProcessorTestHelper.GetStandardIM917InterchangeText(transactionId, mailboxId, includeResponseWrap: false, includeEncoding: false));
			}
			factory1.Save();
			var incomingInterchage = InterchangeProcessorTestHelper.CreateIncomingInterchange(factory2, EDIInterchange.ApplicationCodes.IECustomsCommon, CommonInterchangeTypeList.Codes.MailboxRequest, InterchangeProcessorTestHelper.GetMailboxCollectResponseMessage(mailItems.ToArray()));
			((IInboundMessageCreator)new MessageCreator(new LoggingInformation())).CreateMessagesForInterchange(incomingInterchage);
			var messageHitCout = factory2.GetTableHitCount(EDIMessage.Schema.TableName);
			AssertLessThan("Missing fetch hints for EDIMessage", messageHitCout, 10);
		}

		public void TestDoNotCreateEDIMessageIfAlreadyExists()
		{
			var matchedMessage = InterchangeProcessorTestHelper.CreateInboundEDIMessage<AISInboundEDIMessage>(Factory, AISInterchangeTypeList.Codes.IM917, "<MailboxId>MAILBOXID001</MailboxId>", "TRANSACTIONID001");
			matchedMessage.EM_MessageNum = "EN1";
			var differentMailboxIDMessage = InterchangeProcessorTestHelper.CreateInboundEDIMessage<AISInboundEDIMessage>(Factory, AISInterchangeTypeList.Codes.IM917, "<MailboxId>MAILBOXID0012B</MailboxId>", "TRANSACTIONID002");
			differentMailboxIDMessage.EM_MessageNum = "EN2";
			var differentTransactionIDMessage = InterchangeProcessorTestHelper.CreateInboundEDIMessage<AISInboundEDIMessage>(Factory, AISInterchangeTypeList.Codes.IM917, "<MailboxId>MAILBOXID003</MailboxId>", "TRANSACTIONID003B");
			differentTransactionIDMessage.EM_MessageNum = "EN3";
			var differentApplicationCodeMessage = InterchangeProcessorTestHelper.CreateInboundEDIMessage<AESInboundEDIMessage>(Factory, AISInterchangeTypeList.Codes.IM917, "<MailboxId>MAILBOXID004</MailboxId>", "TRANSACTIONID004");
			differentApplicationCodeMessage.EM_MessageNum = "EN4";
			var incomingInterchage = InterchangeProcessorTestHelper.CreateIncomingInterchange(Factory, EDIInterchange.ApplicationCodes.IECustomsCommon, CommonInterchangeTypeList.Codes.MailboxRequest,
				InterchangeProcessorTestHelper.GetMailboxCollectResponseMessage(
					AISInterchangeProcessorTestHelper.GetStandardIM917InterchangeText("TRANSACTIONID001", "MAILBOXID001", includeResponseWrap: false, includeEncoding: false),
					AISInterchangeProcessorTestHelper.GetStandardIM917InterchangeText("TRANSACTIONID002", "MAILBOXID002", includeResponseWrap: false, includeEncoding: false),
					AISInterchangeProcessorTestHelper.GetStandardIM917InterchangeText("TRANSACTIONID003", "MAILBOXID003", includeResponseWrap: false, includeEncoding: false),
					AISInterchangeProcessorTestHelper.GetStandardIM917InterchangeText("TRANSACTIONID004", "MAILBOXID004", includeResponseWrap: false, includeEncoding: false)));
			incomingInterchage.EI_InterchangeNum = "IE6546";
			Factory.Save();
			var logger = new LoggingInformation();
			((IInboundMessageCreator)new MessageCreator(logger)).CreateMessagesForInterchange(incomingInterchage);
			CombineAssertions(() =>
			{
				AssertEquals("incomingInterchage.ContainedMessages.Count", 3, incomingInterchage.ContainedMessages.Count);
				EDIMessage message1 = null;
				EDIMessage message2 = null;
				EDIMessage message3 = null;
				EDIMessage message4 = null;
				foreach (EDIMessage message in incomingInterchage.ContainedMessages)
				{
					switch (message.EM_ApplicationReference)
					{
						case "TRANSACTIONID001":
							throw new InvalidOperationException("Should not create EDIMessage with TRANSACTIONID001");
						case "TRANSACTIONID002":
							message2 = message;
							break;
						case "TRANSACTIONID003":
							message3 = message;
							break;
						case "TRANSACTIONID004":
							message4 = message;
							break;
						default:
							throw new NotSupportedException(message.EM_ApplicationReference);
					}
				}
				AssertNull("Should not have created as there is already an existing EDIMessage", message1);
				AssertNotNull("Should have been created as Mailbox ID wasn't matched", message2);
				AssertNotNull("Should have been created as Transaction ID wasn't matched", message3);
				AssertNotNull("Should have been created as EM_ApplicationCode wasn't matched", message4);
				AssertContains("Logs", "\tProcessing Interchange (Type:MBR, Number:IE6546).\r\n\tProcessing 4 mailbox items.\r\n\tSystem already created message 'EN1' with matching details (IEI, 917, RCV, TRANSACTIONID001).\r\n\tCreated message (IEI, 917, RCV, IE654602, TRANSACTIONID002).\r\n\tCreated message (IEI, 917, RCV, IE654603, TRANSACTIONID003).\r\n\tCreated message (IEI, 917, RCV, IE654604, TRANSACTIONID004).\r\n\tCreated MBA Interchange 'IE6546A'.", string.Join("\r\n", logger.UserLogStrings.Cast<string>()));
			});
		}
		public void TestCreateMessagesForInterchange()
		{
			var testInterchange = AISInterchangeProcessorTestHelper.CreateAISVersion2_0IM415VInterchange(Factory, "TRAN001", "A", "LRN001", "MRN001", new ZDateTime(2023, 08, 11));
			Factory.Save();
			var logger = new LoggingInformation();
			((IInboundMessageCreator)new MessageCreator(logger)).CreateMessagesForInterchange(testInterchange);
			CombineAssertions(() =>
			{
				var createdMessage = testInterchange.ContainedMessages[0];
				AssertEquals("EM_ApplicationCode", "IEI", createdMessage.EM_ApplicationCode);
				AssertEquals("EM_ApplicationReference", "TRAN001", createdMessage.EM_ApplicationReference);
				AssertEquals("EM_MessageType", "15V", createdMessage.EM_MessageType);
				AssertEquals("EM_ReceiveTransmit", "RCV", createdMessage.EM_ReceiveTransmit);
				AssertEquals("EM_Status", "QUE", createdMessage.EM_Status);
				var nsMap = new Dictionary<string, string>
				{
					{ "MailboxItem", CustomsCollectResponseSchemaUrl },
					{ "MailboxId", CustomsCollectResponseSchemaUrl },
					{ "TransactionId", CustomsCollectResponseSchemaUrl },
					{ "Message", CustomsCollectResponseSchemaUrl },
					{ "IM415V", CustomsSchemaUrl },
					{ "ImportOperation", string.Empty },
					{ "additionalDeclarationType", string.Empty },
					{ "LRN", string.Empty },
					{ "MRN", string.Empty },
					{ "DeclarationAcknowledgementDate", string.Empty }
				};

				AssertXmlPathValue(createdMessage.EM_MessageText,
					["MailboxItem", "MailboxId"],
					nsMap,
					expectedValue: "ce45c655-c780-43be-94f8-69ef936ea871");

				AssertXmlPathValue(createdMessage.EM_MessageText,
					["MailboxItem", "TransactionId"],
					nsMap,
					expectedValue: "TRAN001");

				AssertXmlPathValue(
					createdMessage.EM_MessageText,
					["MailboxItem", "Message", "IM415V", "ImportOperation", "MRN"],
					nsMap,
					expectedValue: "MRN001");

				AssertXmlPathValue(createdMessage.EM_MessageText,
					["MailboxItem", "Message", "IM415V", "ImportOperation", "DeclarationAcknowledgementDate"],
					nsMap,
					expectedValue: "2023-08-11");

				AssertContains("Logs", "Processing Interchange (Type:MBR, Number:1).\r\n\tProcessing 1 mailbox item.\r\n\tCreated message (IEI, 15V, RCV, 101, TRAN001).\r\n\tCreated MBA Interchange '1A'.", string.Join("\r\n", logger.UserLogStrings.Cast<string>()));
			});
		}

		public void TestCreateMessagesForInterchange_H7V1()
		{
			var testInterchange = AISInterchangeProcessorTestHelper.CreateAIS_H7V1_IM415VInterchange(Factory, "TRAN001", "A", "LRN001", "MRN001", new ZDateTime(2023, 08, 11));
			Factory.Save();
			var logger = new LoggingInformation();
			((IInboundMessageCreator)new MessageCreator(logger)).CreateMessagesForInterchange(testInterchange);
			CombineAssertions(() =>
			{
				var createdMessage = testInterchange.ContainedMessages[0];
				AssertEquals("EM_ApplicationCode", "IEI", createdMessage.EM_ApplicationCode);
				AssertEquals("EM_ApplicationReference", "TRAN001", createdMessage.EM_ApplicationReference);
				AssertEquals("EM_MessageType", "15V", createdMessage.EM_MessageType);
				AssertEquals("EM_ReceiveTransmit", "RCV", createdMessage.EM_ReceiveTransmit);
				AssertEquals("EM_Status", "QUE", createdMessage.EM_Status);
				var nsMap = new Dictionary<string, string>
				{
					{ "MailboxItem", CustomsCollectResponseSchemaUrl },
					{ "MailboxId", CustomsCollectResponseSchemaUrl },
					{ "TransactionId", CustomsCollectResponseSchemaUrl },
					{ "Message", CustomsCollectResponseSchemaUrl },
					{ "IM415V", CustomsIM415VH7SchemaUrl },
					{ "Declaration", CustomsIM415VH7SchemaUrl },
					{ "AdditionalDeclarationType", CustomsIM415VH7SchemaUrl },
					{ "LRN", CustomsIM415VH7SchemaUrl },
					{ "MRN", CustomsIM415VH7SchemaUrl },
					{ "DeclarationAcknowledgementDate", CustomsIM415VH7SchemaUrl }
				};

				AssertXmlPathValue(
					createdMessage.EM_MessageText,
					["MailboxItem", "MailboxId"],
					nsMap,
					expectedValue: "ce45c655-c780-43be-94f8-69ef936ea871");

				AssertXmlPathValue(
					createdMessage.EM_MessageText,
					["MailboxItem", "TransactionId"],
					nsMap,
					expectedValue: "TRAN001");

				AssertXmlPathValue(
					createdMessage.EM_MessageText,
					["MailboxItem", "Message", "IM415V", "Declaration", "AdditionalDeclarationType"],
					nsMap,
					expectedValue: "A");

				AssertXmlPathValue(
					createdMessage.EM_MessageText,
					["MailboxItem", "Message", "IM415V", "Declaration", "LRN"],
					nsMap,
					expectedValue: "LRN001");

				AssertXmlPathValue(
					createdMessage.EM_MessageText,
					["MailboxItem", "Message", "IM415V", "Declaration", "MRN"],
					nsMap,
					expectedValue: "MRN001");

				AssertXmlPathValue(
					createdMessage.EM_MessageText,
					["MailboxItem", "Message", "IM415V", "Declaration", "DeclarationAcknowledgementDate"],
					nsMap,
					expectedValue: "2023-08-11");

				AssertContains("Logs", "Processing Interchange (Type:MBR, Number:1).\r\n\tProcessing 1 mailbox item.\r\n\tCreated message (IEI, 15V, RCV, 101, TRAN001).\r\n\tCreated MBA Interchange '1A'.", string.Join("\r\n", logger.UserLogStrings.Cast<string>()));
			});
		}

		public void TestCreateMessagesForInterchange_UCC5()
		{
			var testInterchange = AISInterchangeProcessorTestHelper.CreateAISVersion1_0IM416Interchange(Factory, "TRANUCC5");
			Factory.Save();
			var logger = new LoggingInformation();
			((IInboundMessageCreator)new MessageCreator(logger)).CreateMessagesForInterchange(testInterchange);
			CombineAssertions(() =>
			{
				var createdMessage = testInterchange.ContainedMessages[0];
				AssertEquals("EM_ApplicationCode", "IE5", createdMessage.EM_ApplicationCode);
				AssertEquals("EM_ApplicationReference", "TRANUCC5", createdMessage.EM_ApplicationReference);
				AssertEquals("EM_MessageType", "416", createdMessage.EM_MessageType);
				AssertEquals("EM_ReceiveTransmit", "RCV", createdMessage.EM_ReceiveTransmit);
				AssertEquals("EM_Status", "QUE", createdMessage.EM_Status);

				var nsMap = new Dictionary<string, string>
				{
					{ "MailboxItem", CustomsCollectResponseSchemaUrl },
					{ "MailboxId", CustomsCollectResponseSchemaUrl },
					{ "TransactionId", CustomsCollectResponseSchemaUrl },
					{ "Message", CustomsCollectResponseSchemaUrl },
					{ "IM416", CustomsIM416SchemaUrl },
					{ "Declaration", CustomsIM416SchemaUrl },
					{ "DeclarationType_1_1", CustomsIM416SchemaUrl },
					{ "AdditionalDeclarationType_1_2", CustomsIM416SchemaUrl },
					{ "LRN_2_5", CustomsIM416SchemaUrl },
					{ "RejectionDate", CustomsIM416SchemaUrl },
					{ "RejectionMotivationText", CustomsIM416SchemaUrl },
					{ "CustomsOffices", CustomsIM416SchemaUrl },
					{ "CustomsOfficeLodgement", CustomsIM416SchemaUrl }
				};

				AssertXmlPathValue(
					createdMessage.EM_MessageText,
					["MailboxItem", "MailboxId"],
					nsMap,
					expectedValue: "ce45c655-c780-43be-94f8-69ef936ea871");

				AssertXmlPathValue(
					createdMessage.EM_MessageText,
					["MailboxItem", "TransactionId"],
					nsMap,
					expectedValue: "TRANUCC5");

				AssertXmlPathValue(
					createdMessage.EM_MessageText,
					["MailboxItem", "Message", "IM416", "Declaration", "DeclarationType_1_1"],
					nsMap,
					expectedValue: "EX");

				AssertXmlPathValue(
					createdMessage.EM_MessageText,
					["MailboxItem", "Message", "IM416", "Declaration", "AdditionalDeclarationType_1_2"],
					nsMap,
					expectedValue: "A");

				AssertXmlPathValue(
					createdMessage.EM_MessageText,
					["MailboxItem", "Message", "IM416", "Declaration", "LRN_2_5"],
					nsMap,
					expectedValue: "LRN001");

				AssertXmlPathValue(
					createdMessage.EM_MessageText,
					["MailboxItem", "Message", "IM416", "Declaration", "RejectionDate"],
					nsMap,
					expectedValue: "20230810");

				AssertXmlPathValue(
					createdMessage.EM_MessageText,
					["MailboxItem", "Message", "IM416", "Declaration", "RejectionMotivationText"],
					nsMap,
					expectedValue: "Rejection Motivation Text");

				AssertXmlPathValue(
					createdMessage.EM_MessageText,
					["MailboxItem", "Message", "IM416", "Declaration", "CustomsOffices", "CustomsOfficeLodgement"],
					nsMap,
					expectedValue: "IEDUB100");

				AssertContains("Logs", "Processing Interchange (Type:MBR, Number:1).\r\n\tProcessing 1 mailbox item.\r\n\tCreated message (IE5, 416, RCV, 101, TRANUCC5).\r\n\tCreated MBA Interchange '1A'.", string.Join("\r\n", logger.UserLogStrings.Cast<string>()));
			});
		}

		public void TestCreateMessagesForInterchange_IM917()
		{
			var testInterchange = AISInterchangeProcessorTestHelper.CreateStandardIM917Interchange(Factory, "TRAN001");
			Factory.Save();
			((IInboundMessageCreator)new MessageCreator(new LoggingInformation())).CreateMessagesForInterchange(testInterchange);
			CombineAssertions(() =>
			{
				var createdMessage = testInterchange.ContainedMessages[0];
				AssertEquals("EM_ApplicationCode", "IEI", createdMessage.EM_ApplicationCode);
				AssertEquals("EM_ApplicationReference", "TRAN001", createdMessage.EM_ApplicationReference);
				AssertEquals("EM_MessageType", "917", createdMessage.EM_MessageType);
				AssertEquals("EM_ReceiveTransmit", "RCV", createdMessage.EM_ReceiveTransmit);
				AssertEquals("EM_Status", "QUE", createdMessage.EM_Status);
				var nsMap = new Dictionary<string, string>
				{
					{ "MailboxItem", CustomsCollectResponseSchemaUrl },
					{ "MailboxId", CustomsCollectResponseSchemaUrl },
					{ "TransactionId", CustomsCollectResponseSchemaUrl },
					{ "Message", CustomsCollectResponseSchemaUrl },
					{ "IM917", CustomsSchemaUrl },
					{ "XmlNegativeAcknowledgement", string.Empty },
					{ "ErrorLineNumber", string.Empty },
					{ "ErrorReason", string.Empty },
					{ "ErrorColumnNumber", string.Empty }
				};

				AssertXmlPathValue(
					createdMessage.EM_MessageText,
					["MailboxItem", "MailboxId"],
					nsMap,
					expectedValue: "ce45c655-c780-43be-94f8-69ef936ea871");

				AssertXmlPathValue(
					createdMessage.EM_MessageText,
					["MailboxItem", "TransactionId"],
					nsMap,
					expectedValue: "TRAN001");

				AssertXmlPathValue(
					createdMessage.EM_MessageText,
					["MailboxItem", "Message", "IM917", "XmlNegativeAcknowledgement", "ErrorLineNumber"],
					nsMap,
					expectedValue: "1");

				AssertXmlPathValue(
					createdMessage.EM_MessageText,
					["MailboxItem", "Message", "IM917", "XmlNegativeAcknowledgement", "ErrorReason"],
					nsMap,
					expectedValue: "cvc-pattern-valid: Value '' is not facet-valid with respect to pattern '.{1,17}' for type 'TraderIdentification_type'.");

				AssertXmlPathValue(
					createdMessage.EM_MessageText,
					["MailboxItem", "Message", "IM917", "XmlNegativeAcknowledgement", "ErrorColumnNumber"],
					nsMap,
					expectedValue: "437");
			});
		}

		public void TestSetProcessData()
		{
			var entryHeader = Factory.NewWithValidTestData<CusEntryHeader>();
			var jeGb = entryHeader.Declaration.JE_GB;
			var outgoingMessage = entryHeader.Messages.AddNew();
			outgoingMessage.EM_GB = jeGb;
			outgoingMessage.EM_MessageNum = "ABC12312";

			var incomingMessage = InterchangeProcessorTestHelper.CreateMessageAcknowledgementMessage<AESInboundEDIMessage>(Factory, "IEE", "TID", "TID123");
			MessageCreator.SetPreProcessData(incomingMessage, outgoingMessage);

			CombineAssertions(() =>
			{
				AssertEquals("EM_GB", outgoingMessage.EM_GB, incomingMessage.EM_GB);
				AssertEquals("EM_LinkTable", outgoingMessage.EM_LinkTable, incomingMessage.EM_LinkTable);
				AssertEquals("EM_LinkUniqueID", outgoingMessage.EM_LinkUniqueID, incomingMessage.EM_LinkUniqueID);
				AssertEquals("EM_Status", EDIMessage.Status.PreProcessedOK, incomingMessage.EM_Status);
				AssertEquals("EM_MessageNum", ZString.Empty, incomingMessage.EM_MessageNum);
			});
		}

		public void TestSetProcessData_EMCS()
		{
			var entryHeader = Factory.NewWithValidTestData<CusEntryHeader>();
			var jeGb = entryHeader.Declaration.JE_GB;
			var outgoingMessage = entryHeader.Messages.AddNew();
			outgoingMessage.EM_GB = jeGb;
			outgoingMessage.EM_MessageNum = "ABC12312";

			var incomingMessage = InterchangeProcessorTestHelper.CreateMessageAcknowledgementMessage<EDIMessage>(Factory, "IEM", "TID", "TID123");
			MessageCreator.SetPreProcessData(incomingMessage, outgoingMessage);

			CombineAssertions(() =>
			{
				AssertEquals("EM_GB", outgoingMessage.EM_GB, incomingMessage.EM_GB);
				AssertEquals("EM_LinkTable", outgoingMessage.EM_LinkTable, incomingMessage.EM_LinkTable);
				AssertEquals("EM_LinkUniqueID", outgoingMessage.EM_LinkUniqueID, incomingMessage.EM_LinkUniqueID);
				AssertEquals("EM_Status", EDIMessage.Status.PreProcessedOK, incomingMessage.EM_Status);
				AssertEquals("EM_MessageNum", ZString.Empty, incomingMessage.EM_MessageNum);
			});
		}

		public void TestPreProcessData_WithOriginOutGoingMessage()
		{
			var entryHeader = Factory.NewWithValidTestData<CusEntryHeader>();
			var jeGb = entryHeader.Declaration.JE_GB;
			var outGoingMessag = Factory.New<AESOutboundEDIMessage>();
			outGoingMessag.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			outGoingMessag.EM_ApplicationCode = EDIMessage.ApplicationCodes.IECustomsExport;
			outGoingMessag.EM_ApplicationReference = "TRAN001";

			outGoingMessag.EM_GB = jeGb;
			entryHeader.Messages.Add(outGoingMessag);

			var incomingInterchange = AISInterchangeProcessorTestHelper.CreateAISVersion2_0IM415VInterchange(Factory, "TRAN001", "A", "LRN001", "MRN001", new ZDateTime(2023, 08, 11));
			Factory.Save();
			var logger = new LoggingInformation();
			((IInboundMessageCreator)new MessageCreator(logger)).CreateMessagesForInterchange(incomingInterchange);
			var createdMessage = incomingInterchange.ContainedMessages[0];
			CombineAssertions(() =>
			{
				AssertSame("Should have been linked to correct BizObj.", entryHeader, createdMessage.EM_LinkedObject);
				AssertEquals("EM_GB", jeGb, createdMessage.EM_GB);
				AssertEquals("EM_Status", EDIMessage.Status.PreProcessedOK, createdMessage.EM_Status);
				AssertEquals("EM_ApplicationCode", EDIMessage.ApplicationCodes.IECustomsImport, createdMessage.EM_ApplicationCode);
				AssertContains("Logs", "\tProcessing Interchange (Type:MBR, Number:1).\r\n\tProcessing 1 mailbox item.\r\n\tCreated message (IEI, 15V, RCV, 101, TRAN001).\r\n\tCreated MBA Interchange '1A'.", string.Join("\r\n", logger.UserLogStrings.Cast<string>()));
			});
		}

		public void TestPreProcessData_WithOriginOutGoingMessage_UCC5Import()
		{
			var entryHeader = Factory.NewWithValidTestData<CusEntryHeader>();
			var jeGb = entryHeader.Declaration.JE_GB;
			var outGoingMessag = Factory.New<AISUCC5OutboundEDIMessage>();
			outGoingMessag.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			outGoingMessag.EM_ApplicationCode = EDIMessage.ApplicationCodes.IECustomsUCC5Import;
			outGoingMessag.EM_ApplicationReference = "TRAN001";

			outGoingMessag.EM_GB = jeGb;
			entryHeader.Messages.Add(outGoingMessag);

			var incomingInterchange = AISInterchangeProcessorTestHelper.CreateAISVersion1_0IM416Interchange(Factory, "TRAN001");
			Factory.Save();
			var logger = new LoggingInformation();
			((IInboundMessageCreator)new MessageCreator(logger)).CreateMessagesForInterchange(incomingInterchange);
			var createdMessage = incomingInterchange.ContainedMessages[0];
			CombineAssertions(() =>
			{
				AssertSame("Should have been linked to correct BizObj.", entryHeader, createdMessage.EM_LinkedObject);
				AssertEquals("EM_GB", jeGb, createdMessage.EM_GB);
				AssertEquals("EM_Status", EDIMessage.Status.PreProcessedOK, createdMessage.EM_Status);
				AssertEquals("EM_ApplicationCode", EDIMessage.ApplicationCodes.IECustomsUCC5Import, createdMessage.EM_ApplicationCode);
				AssertContains("Logs", "\tProcessing Interchange (Type:MBR, Number:1).\r\n\tProcessing 1 mailbox item.\r\n\tCreated message (IE5, 416, RCV, 101, TRAN001).\r\n\tCreated MBA Interchange '1A'.", string.Join("\r\n", logger.UserLogStrings.Cast<string>()));
			});
		}

		public void TestPreProcessData_WithOriginOutGoingMessage_NCTS()
		{
			var entryHeader = Factory.NewWithValidTestData<CusEntryHeader>();
			var jeGb = entryHeader.Declaration.JE_GB;
			var outGoingMessag = Factory.New<AESOutboundEDIMessage>();
			outGoingMessag.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			outGoingMessag.EM_ApplicationCode = EDIMessage.ApplicationCodes.IECustomsNCTS;
			outGoingMessag.EM_ApplicationReference = "TRAN001";

			outGoingMessag.EM_GB = jeGb;
			entryHeader.Messages.Add(outGoingMessag);

			var incomingInterchange = AESInterchangeProcessorTestHelper.CreateStandardCC917CInterchange_NCTS(Factory, "TRAN001");
			Factory.Save();
			var logger = new LoggingInformation();
			((IInboundMessageCreator)new MessageCreator(logger)).CreateMessagesForInterchange(incomingInterchange);
			var createdMessage = incomingInterchange.ContainedMessages[0];
			CombineAssertions(() =>
			{
				AssertSame("Should have been linked to correct BizObj.", entryHeader, createdMessage.EM_LinkedObject);
				AssertEquals("EM_GB", jeGb, createdMessage.EM_GB);
				AssertEquals("EM_Status", EDIMessage.Status.PreProcessedOK, createdMessage.EM_Status);
				AssertEquals("EM_ApplicationCode", EDIMessage.ApplicationCodes.IECustomsNCTS, createdMessage.EM_ApplicationCode);
				AssertContains("Logs", "\tProcessing Interchange (Type:MBR, Number:1).\r\n\tProcessing 1 mailbox item.\r\n\tCreated message (IEN, 917, RCV, 101, TRAN001).\r\n\tCreated MBA Interchange '1A'.", string.Join("\r\n", logger.UserLogStrings.Cast<string>()));
			});
		}

		public void TestPreProcessData_WithoutOriginOutGoingMessage_EMCS()
		{
			var branch = Factory.NewWithValidTestData<GlbBranch>();
			var outgoingInterchange = InterchangeCreator.CreateOutgoingInterchange(Factory, EDIInterchange.ApplicationCodes.IECustomsEMCS, CommonInterchangeTypeList.Codes.MailboxRequest, branch.PK, "", "");
			var incomingInterchange = InterchangeProcessorTestHelper.CreateIncomingInterchange(Factory, EDIInterchange.ApplicationCodes.IECustomsEMCS, CommonInterchangeTypeList.Codes.MailboxRequest, InterchangeProcessorTestHelper.GetMailboxItemText("TRAN001", GetEMCSIE801MessageText()), branchPK: branch.PK, sessionGUID: outgoingInterchange.EI_SessionGUID);
			Factory.Save();
			var logger = new LoggingInformation();
			((IInboundMessageCreator)new MessageCreator(logger)).CreateMessagesForInterchange(incomingInterchange);
			var createdMessage = incomingInterchange.ContainedMessages[0];
			CombineAssertions(() =>
			{
				AssertEquals("EM_GB", branch.PK, createdMessage.EM_GB);
				AssertEquals("EM_ApplicationCode", EDIMessage.ApplicationCodes.IECustomsEMCS, createdMessage.EM_ApplicationCode);
				AssertContains("Logs", "\tProcessing Interchange (Type:MBR, Number:1).\r\n\tProcessing 1 mailbox item.\r\n\tCreated message (IEM, 801, RCV, 101, TRAN001).\r\n\tCreated MBA Interchange '1A'.", string.Join("\r\n", logger.UserLogStrings.Cast<string>()));
			});
		}

		public void TestPreProcessData_EMCS_AlternateRecipient()
		{
			var branch2 = Factory.NewWithValidTestData<GlbBranch>();
			var transactionId = "0ee8bd2f-f40c-409d-80c6-bbcdcaae80f9";

			var entryHeader = Factory.NewWithValidTestData<CusEntryHeader>();
			var declaration = entryHeader.Declaration;
			var branch1PK = declaration.JE_GB;
			var outGoingMessage = Factory.New<EMCSOutboundMessageForTest>();
			outGoingMessage.EM_ApplicationCode = EDIMessage.ApplicationCodes.IECustomsEMCS;
			outGoingMessage.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			outGoingMessage.EM_ApplicationReference = transactionId;

			outGoingMessage.EM_GB = branch1PK;
			declaration.Messages.Add(outGoingMessage);

			var outgoingInterchange = InterchangeCreator.CreateOutgoingInterchange(Factory, EDIInterchange.ApplicationCodes.IECustomsEMCS, CommonInterchangeTypeList.Codes.MailboxRequest, branch1PK, "", "", transactionID: transactionId);
			var incomingInterchange1 = InterchangeProcessorTestHelper.CreateIncomingInterchange(Factory, EDIInterchange.ApplicationCodes.IECustomsEMCS, CommonInterchangeTypeList.Codes.MailboxRequest, InterchangeProcessorTestHelper.GetMailboxItemText(transactionId, GetEMCSIE801MessageText(), mailboxId: "4187DFCF-8F7D-46E0-A4AE-9FF1A011DC23"), branchPK: branch1PK);
			var incomingInterchange2 = InterchangeProcessorTestHelper.CreateIncomingInterchange(Factory, EDIInterchange.ApplicationCodes.IECustomsEMCS, CommonInterchangeTypeList.Codes.MailboxRequest, InterchangeProcessorTestHelper.GetMailboxItemText(transactionId, GetEMCSIE801MessageText(), mailboxId: "9C6879D7-D243-485F-A18A-6B9854A5A8FE"), branchPK: branch2.PK);
			Factory.Save();

			AssertNotEquals("Branch 1 and Branch 2 are from different companies", declaration.JE_GC, branch2.GB_GC);
			AssertNotEquals("Interchanges have different SessionGUID", outgoingInterchange.EI_SessionGUID, incomingInterchange2.EI_SessionGUID);
			AssertNotEquals("Interchanges have different SessionGUID", incomingInterchange1.EI_SessionGUID, incomingInterchange2.EI_SessionGUID);

			var logger1 = new LoggingInformation();
			((IInboundMessageCreator)new MessageCreator(logger1)).CreateMessagesForInterchange(incomingInterchange1);
			var createdMessage1 = incomingInterchange1.ContainedMessages[0];
			CombineAssertions(() =>
			{
				AssertEquals("EM_ApplicationCode", EDIMessage.ApplicationCodes.IECustomsEMCS, createdMessage1.EM_ApplicationCode);
				AssertEquals("EM_Status", EDIMessage.Status.PreProcessedOK, createdMessage1.EM_Status);
				AssertEquals("EM_GB", branch1PK, createdMessage1.EM_GB);
				AssertSame("Should have been linked to correct BizObj.", declaration, createdMessage1.EM_LinkedObject);
				AssertContains("Logs", $"\tProcessing Interchange (Type:MBR, Number:1).\r\n\tProcessing 1 mailbox item.\r\n\tCreated message (IEM, 801, RCV, 101, {transactionId}).\r\n\tCreated MBA Interchange '1A'.", string.Join("\r\n", logger1.UserLogStrings.Cast<string>()));
			});

			var logger2 = new LoggingInformation();
			((IInboundMessageCreator)new MessageCreator(logger2)).CreateMessagesForInterchange(incomingInterchange2);
			var createdMessage2 = incomingInterchange2.ContainedMessages[0];
			CombineAssertions(() =>
			{
				AssertEquals("EM_ApplicationCode", EDIMessage.ApplicationCodes.IECustomsEMCS, createdMessage2.EM_ApplicationCode);
				AssertEquals("EM_Status", EDIMessage.Status.Queued, createdMessage2.EM_Status);
				AssertEquals("EM_GB", branch2.PK, createdMessage2.EM_GB);
				AssertContains("Logs", $"\tProcessing Interchange (Type:MBR, Number:2).\r\n\tProcessing 1 mailbox item.\r\n\tCreated message (IEM, 801, RCV, 201, {transactionId}).\r\n\tCreated MBA Interchange '2A'.", string.Join("\r\n", logger2.UserLogStrings.Cast<string>()));
			});
		}

		string GetEMCSIE801MessageText() => @"<q1:IE801 xmlns:q1=""urn:publicid:-:EC:DGTAXUD:EMCS:PHASE4:IE801:V3.13""></q1:IE801>";

		static void AssertXmlPathValue(
			string xml,
			string[] elementPath,
			Dictionary<string, string> namespaceMap,
			string expectedValue,
			int numberOfMatchedElementsToSkip = 0)
		{
			var doc = XDocument.Parse(xml);

			XElement current = doc.Root;

			// Validate root matches first element in path
			var rootName = elementPath[0];
			if (!namespaceMap.TryGetValue(rootName, out var rootNsUri))
			{
				throw new ArgumentException($"Missing namespace mapping for root element '{rootName}'");
			}

			XNamespace rootNs = rootNsUri;
			if (current?.Name != rootNs + rootName)
			{
				throw new Exception($"Expected root '{rootNs + rootName}' but found '{current?.Name}'");
			}

			// Walk down the rest of the path
			for (var i = 1; i < elementPath.Length; i++)
			{
				var name = elementPath[i];

				if (!namespaceMap.TryGetValue(name, out var nsUri))
				{
					throw new ArgumentException($"Missing namespace mapping for element '{name}'");
				}

				XNamespace ns = nsUri;
				var elements = current.Elements().ToList();
				current = numberOfMatchedElementsToSkip > 0 && elements.Count > numberOfMatchedElementsToSkip ? elements[numberOfMatchedElementsToSkip] : elements.FirstOrDefault(e => e.Name == ns + name);

				if (current == null)
				{
					throw new Exception($"Element not found at path: {string.Join("/", elementPath.Take(i + 1))}\r\n");
				}
			}

			// Assert value
			if (!string.Equals(current.Value?.Trim(), expectedValue, StringComparison.Ordinal))
			{
				throw new Exception($"Expected value '{expectedValue}' but found '{current.Value}' at path: {string.Join("/", elementPath)}");
			}
		}

		internal sealed class EMCSOutboundMessageForTest : OutboundEDIMessage
		{
			public EMCSOutboundMessageForTest(BusinessObjectFactory factory, DataRow row) : base(factory, row)
			{
			}

			protected override string GetMessageReferenceNumber() => EM_MessageNum;
		}

		protected override void SetUp()
		{
			base.SetUp();
			logger = new LoggingInformation();
		}
		LoggingInformation logger;
	}
}
