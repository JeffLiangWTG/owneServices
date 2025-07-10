using System;
using System.Linq;
using System.Xml.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.IE.Business.Declaration;
using Enterprise.Customs.IE.Messaging;
using Enterprise.Customs.IE.Messaging.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration.Customs.IE;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using Moq;
using BaseEDIMessage = Enterprise.Messaging.Business.EDIMessage;

namespace Enterprise.Customs.IE.Business.Testing
{
	class MessageRetrievingProcessorTest : TestCaseWithFactory
	{
		public void TestProcessingIEECommunicationErrorResponse() => AssertProcessingCommunicationErrorResponse(EDIInterchange.ApplicationCodes.IECustomsExport, AESOutgoingMessageTypeList.Codes.ExportOriginal);

		public void TestProcessingIEICommunicationErrorResponse() => AssertProcessingCommunicationErrorResponse(EDIInterchange.ApplicationCodes.IECustomsImport, AISInterchangeTypeList.Codes.IM415V);

		void AssertProcessingCommunicationErrorResponse(ZString applicationCode, ZString interchangeType)
		{
			(var company1, var branch1) = InterchangeProcessorTestHelper.CreateCompanyAndBranch(Factory, Core.Constants.CountryCodes.Ireland, "I1");
			var companyCredential = InterchangeProcessorTestHelper.CreateValidCredential(company1);
			(var company2, var branch2) = InterchangeProcessorTestHelper.CreateCompanyAndBranch(Factory, Core.Constants.CountryCodes.Ireland, "I2");
			InterchangeProcessorTestHelper.CreateValidCredential(company2);
			const string transactionID = "A02A8604-2D0B-4FEE-8EDF-61DB6AFC7639";
			const string messageText = "<GREETING>HELLO</GREETING>";
			var outgoingInterchange = InterchangeCreator.CreateOutgoingInterchange(Factory, applicationCode, interchangeType, branch1.PK, messageText, "https://www.where.com");
			outgoingInterchange.EI_GP = companyCredential.PK;
			outgoingInterchange.EI_Status = EDIInterchange.Status.Sent;
			var outgoingMessage = Factory.New<BaseEDIMessage>();
			outgoingMessage.EM_ApplicationCode = "OM#";
			outgoingMessage.EM_ApplicationReference = transactionID;
			outgoingMessage.EM_MessageType = "OM@";
			outgoingMessage.EM_MessageText = messageText;
			outgoingMessage.EM_Status = EDIMessage.Status.Acknowledged;
			outgoingMessage.EM_GB = branch2.PK;
			outgoingMessage.EM_EI = outgoingInterchange.PK;
			var messageNumberStrategyMock = new Mock<IMessageNumberStrategy>();
			messageNumberStrategyMock.Setup(x => x.GetMessageReferenceNumber()).Returns("ENT1234");
			outgoingMessage.MessageNumberStrategy = messageNumberStrategyMock.Object;
			const string bodyText = "Failure Notification for 480036.";
			var incomingInterchange = InterchangeProcessorTestHelper.CreateIncomingInterchange(Factory, applicationCode, "Z1Z", bodyText, sessionGUID: outgoingInterchange.EI_SessionGUID, branchPK: branch1.PK, wrapInSOAPEnvelope: false);
			incomingInterchange.EI_HeaderText = @"{""custom.ErrorType"":""ERR"",""custom.NotificationType"":""Failure"",""custom.ErrorDescription"":""MsgId:xt-msg:3a582ce4-fd5b-4fb4-b777-787a5927f899\r\nErrorCode:0\r\nMessage Attributes:\r\n\tacklevel:0\r\n\tackprot:0\r\n\tacktimeout:0\r\n\tarchiveflags:0\r\n\tcfgversion:58242\r\n\tcontractobj:xt-contract:/Interfaces/Customs/DxT/IE/IE Contract\r\n\tcreationtime:1653258660\r\n\tcurracklevel:0\r\n\tcustom.ApplicationCode:IEC\r\n\tcustom.DestinationParty:IECustomsTest\r\n\tcustom.IE.EndPoint:https://softwaretestnextversion.ros.ie/customs/webservice/v1/soap/mailboxCollect\r\n\tcustom.MessageTrackingID:e3fe9ff8-eca4-4ccf-9ada-cc12c4810c9f\r\n\tcustom.MessageType:MBR\r\n\tcustom.SourceParty:HYEIRECMT\r\n\tdatahashin:a5462f4d449eb83931dfa003bd88cc45\r\n\tdatahashout:\r\n\tdatasizein:94\r\n\tdatasizeout:94\r\n\tfilenamein:\r\n\tfilenameout:480036\r\n\tflags:16\r\n\tflagstext:MF_PROC\r\n\tfolderin:\r\n\tfromobj:xt-application:/Direct xT/HYECMT/HYECMT\r\n\tfromparty:\r\n\tfromprot:903\r\n\tinternalid:480036\r\n\tlaststate:1027\r\n\tmsgid:480036\r\n\tmsginfo:\r\n\tmsgtype:1\r\n\tmsguuid:3a582ce4-fd5b-4fb4-b777-787a5927f899\r\n\towner:255\r\n\tparseinfoin.std.sender:HYECMT\r\n\tpriority:1\r\n\tseqidin:0\r\n\tseqidout:0\r\n\tseqnoin:0\r\n\tseqnoout:0\r\n\tseqtypein:0\r\n\tseqtypeout:0\r\n\tsequuidin:00000000-0000-0000-0000-000000000000\r\n\tsequuidout:00000000-0000-0000-0000-000000000000\r\n\tstate:5123\r\n\tstatename:ST_ERR_PROC\r\n\tsyncreply:0\r\n\ttoobj:xt-node:/Interfaces/Customs/DxT/IE/IE Out-port\r\n\ttoparty:\r\n\ttoprot:1282\r\n\tversion:2\r\n\twfinst:00000000-0000-0000-0000-000000000000\r\nMessage Events:\r\nEvent :\r\n\ttransfer.text:\r\n\tlogevent:1\r\n\ttransfer.dir:In\r\n\tlogtext:\r\n\ttime:2022-05-22T22:31:00Z\r\n\ttransfer.module:38914\r\nEvent :Obj:xt-routeentry:{c33df2a3-e750-4c05-b6e2-0048cb8b8244}\r\n\tlogevent:55\r\n\ttime:2022-05-22T22:31:00Z\r\n\tlogtext:\r\nEvent :\r\n\tlogevent:7\r\n\ttime:2022-05-22T22:31:00Z\r\n\tlogtext:\r\nEvent :Obj:xt-procbatch:{d449340f-56f1-4fa9-b71b-3ded31922627}\r\n\tlogevent:52\r\n\ttime:2022-05-22T22:31:00Z\r\n\tlogtext:\r\nEvent :Obj:xt-procstep:{ae6b6283-c1cf-4776-99ec-330094a052fe}\r\n\tlogevent:268435457\r\n\ttime:2022-05-22T22:31:04Z\r\n\tlogtext:Wrap and Sign IE Message\r\n""}";
			incomingInterchange.EI_InterchangeNum = "IN8659";
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_GB = branch1.PK;
			var entry = declaration.CustomsEntryHeaders.AddNew();
			entry.Messages.Add(outgoingMessage);
			Factory.Save();
			var processor = new MessageRetrievingProcessor();
			processor.ExecuteBatch();
			var logger = processor.Logger;

			CombineAssertions(() =>
			{
				var anotherFactory = new BusinessObjectFactory();
				var processedInterchange = anotherFactory.Load<EDIInterchange>(incomingInterchange.PK);
				AssertEquals("EI_Status of processed EDIInterchange.", "RCV", processedInterchange.EI_Status);
				var processedMessage = processedInterchange.ContainedMessages.Cast<BaseEDIMessage>().Single();
				AssertEquals("EM_ApplicationCode", "OM#", processedMessage.EM_ApplicationCode);
				AssertEquals("EM_MessageType", "OM@", processedMessage.EM_MessageType);
				AssertEquals("EM_LinkUniqueID", entry.PK, processedMessage.EM_LinkUniqueID);
				AssertEquals("LogicalStatus", "FAL", (processedMessage.EM_LinkedObject as CusEntryHeader).CH_Status);
				AssertEquals("EM_LinkTable", CusEntryHeader.Schema.TableName, processedMessage.EM_LinkTable);
				AssertEquals("EM_GB", branch2.PK, processedMessage.EM_GB);
				AssertEquals("EM_Status", "PPS", processedMessage.EM_Status);
				AssertEquals("EM_MessageSubType ", "ERR", processedMessage.EM_MessageSubType);
				AssertEquals("EM_MessageText", bodyText, processedMessage.EM_MessageText);
				AssertContains("Logs", $"\tProcessing Interchange (Type:Z1Z, Number:IN8659).\r\n\tCreated error message (OM#, OM@, RCV, IN865901, A02A8604-2D0B-4FEE-8EDF-61DB6AFC7639).\r\n\tInterchange 'IN8659' has been processed successfully.", string.Join("\r\n", logger.UserLogStrings.Cast<string>()));
			});
		}

		public void TestProcessingMailboxAcknowledgeReponse()
		{
			var interchange = InterchangeProcessorTestHelper.CreateMailboxAcknowledgeResponse(Factory, "MAILBOXID001");
			Factory.Save();

			var processor = new MessageRetrievingProcessor();
			processor.ExecuteBatch();
			var logger = processor.Logger;

			CombineAssertions(() =>
			{
				var anotherFactory = new BusinessObjectFactory();
				var processedInterchange = anotherFactory.Load<EDIInterchange>(interchange.PK);
				AssertEquals("EI_Status of processed EDIInterchange.", "RCV", processedInterchange.EI_Status);
				AssertEquals("processedInterchange.ContainedMessages.Count", 0, processedInterchange.ContainedMessages.Count);
				AssertContains("Logs", "Interchange '1' has been processed successfully.", string.Join("\r\n", logger.UserLogStrings.Cast<string>()));
			});
		}

		public void TestProcessingMessageAcknowledgement_Export()
		{
			var interchange = InterchangeProcessorTestHelper.CreateMessageAcknowledgementResponse(Factory, "TRANSACTIONID 001");
			Factory.Save();

			var processor = new MessageRetrievingProcessor();
			processor.ExecuteBatch();
			var logger = processor.Logger;

			CombineAssertions(() =>
			{
				var anotherFactory = new BusinessObjectFactory();
				var processedInterchange = anotherFactory.Load<EDIInterchange>(interchange.PK);
				AssertEquals("EI_Status of processed EDIInterchange.", "RCV", processedInterchange.EI_Status);
				var processedMessage = processedInterchange.ContainedMessages.Cast<EDIMessage>().Single();
				AssertEquals("EM_Status", "QUE", processedMessage.EM_Status);
				AssertEquals("EM_MessageSubType ", "ACK", processedMessage.EM_MessageSubType);
				AssertContains("Logs", "\tProcessing Interchange (Type:ACK, Number:1).\r\n\tCreated message (IEE, ACK, RCV, 101, TRANSACTIONID 001).\r\n\tInterchange '1' has been processed successfully.", string.Join("\r\n", logger.UserLogStrings.Cast<string>()));
			});
		}

		public void TestProcessingMessageAcknowledgement_ServiceError()
		{
			(var company1, var branch1) = InterchangeProcessorTestHelper.CreateCompanyAndBranch(Factory, Core.Constants.CountryCodes.Ireland, "I1");
			var outgoingInterchange = InterchangeCreator.CreateOutgoingInterchange(Factory, EDIInterchange.ApplicationCodes.IECustomsExport, AESOutgoingMessageTypeList.Codes.ExportOriginal, branch1.PK, "", "https://www.where.com");
			var companyCredential = InterchangeProcessorTestHelper.CreateValidCredential(company1);
			outgoingInterchange.EI_GP = companyCredential.PK;
			outgoingInterchange.EI_Status = EDIInterchange.Status.Sent;
			var outgoingMessage = Factory.New<BaseEDIMessage>();
			outgoingMessage.EM_ApplicationCode = EDIInterchange.ApplicationCodes.IECustomsExport;
			outgoingMessage.EM_ApplicationReference = "UNAVAILABLETID";
			outgoingMessage.EM_MessageType = AESOutgoingMessageTypeList.Codes.ExportOriginal;
			outgoingMessage.EM_MessageText = "";
			outgoingMessage.EM_Status = EDIMessage.Status.Sent;
			var messageNumberStrategyMock = new Mock<IMessageNumberStrategy>();
			messageNumberStrategyMock.Setup(x => x.GetMessageReferenceNumber()).Returns("1234");
			outgoingMessage.MessageNumberStrategy = messageNumberStrategyMock.Object;
			outgoingMessage.EM_GB = branch1.PK;
			outgoingMessage.EM_EI = outgoingInterchange.PK;
			var incomingInterchange = InterchangeProcessorTestHelper.CreateMessageAcknowledgementServiceErrorResponse(Factory, "ROS-222000", sessionGUID: outgoingInterchange.EI_SessionGUID);
			incomingInterchange.EI_InterchangeNum = "EI_InterchangeNum_LongerThan_EM_MessageNumMaxLength";
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_GB = branch1.PK;
			var entry = declaration.CustomsEntryHeaders.AddNew();
			entry.Messages.Add(outgoingMessage);
			Factory.Save();

			var processor = new MessageRetrievingProcessor();
			processor.ExecuteBatch();
			var logger = processor.Logger;

			CombineAssertions(() =>
			{
				var anotherFactory = new BusinessObjectFactory();
				var processedInterchange = anotherFactory.Load<EDIInterchange>(incomingInterchange.PK);
				AssertEquals("Incoming interchange is processed", "RCV", processedInterchange.EI_Status);
				var processedMessage = processedInterchange.ContainedMessages.Cast<BaseEDIMessage>().Single();
				AssertEquals("Incoming message linked to entry header", entry.PK, processedMessage.EM_LinkUniqueID);
				AssertEquals("Incoming message link correct type", CusEntryHeader.Schema.TableName, processedMessage.EM_LinkTable);
				AssertEquals("Incoming message EM_MessageNum", "LongerThan_EM_MessageNumMaxLength01", processedMessage.EM_MessageNum);
				AssertContains("Logs", "\tProcessing Interchange (Type:ACK, Number:EI_InterchangeNum_LongerThan_EM_MessageNumMaxLength).\r\n\tCreated message (IEE, ACK, RCV, LongerThan_EM_MessageNumMaxLength01, UNAVAILABLETID).\r\n\tInterchange 'EI_InterchangeNum_LongerThan_EM_MessageNumMaxLength' has been processed successfully.", string.Join("\r\n", logger.UserLogStrings.Cast<string>()));
			});
		}

		public void TestProcessingMessageAcknowledgement_Import()
		{
			var interchange = InterchangeProcessorTestHelper.CreateMessageAcknowledgementResponse(Factory, "TRANSACTIONID 001");
			interchange.EI_ApplicationCode = EDIInterchange.ApplicationCodes.IECustomsImport;
			Factory.Save();

			var processor = new MessageRetrievingProcessor();
			processor.ExecuteBatch();
			var logger = processor.Logger;

			CombineAssertions(() =>
			{
				var anotherFactory = new BusinessObjectFactory();
				var processedInterchange = anotherFactory.Load<EDIInterchange>(interchange.PK);
				AssertEquals("EI_Status of processed EDIInterchange.", "RCV", processedInterchange.EI_Status);
				var processedMessage = processedInterchange.ContainedMessages.Cast<EDIMessage>().Single();
				AssertEquals("EM_Status", "QUE", processedMessage.EM_Status);
				AssertEquals("EM_MessageSubType ", "ACK", processedMessage.EM_MessageSubType);
				AssertContains("Logs", "\tProcessing Interchange (Type:ACK, Number:1).\r\n\tCreated message (IEI, ACK, RCV, 101, TRANSACTIONID 001).\r\n\tInterchange '1' has been processed successfully.", string.Join("\r\n", logger.UserLogStrings.Cast<string>()));
			});
		}

		public void TestProcessingMessageAcknowledgement_ImportUCC5()
		{
			var interchange = InterchangeProcessorTestHelper.CreateMessageAcknowledgementResponse(Factory, "TRANSACTIONID 001");
			interchange.EI_ApplicationCode = EDIInterchange.ApplicationCodes.IECustomsUCC5Import;
			Factory.Save();

			var processor = new MessageRetrievingProcessor();
			processor.ExecuteBatch();
			var logger = processor.Logger;

			CombineAssertions(() =>
			{
				var anotherFactory = new BusinessObjectFactory();
				var processedInterchange = anotherFactory.Load<EDIInterchange>(interchange.PK);
				AssertEquals("EI_Status of processed EDIInterchange.", "RCV", processedInterchange.EI_Status);
				var processedMessage = processedInterchange.ContainedMessages.Cast<EDIMessage>().Single();
				AssertEquals("EM_Status", "QUE", processedMessage.EM_Status);
				AssertEquals("EM_MessageSubType ", "ACK", processedMessage.EM_MessageSubType);
				AssertContains("Logs", "\tProcessing Interchange (Type:ACK, Number:1).\r\n\tCreated message (IE5, ACK, RCV, 101, TRANSACTIONID 001).\r\n\tInterchange '1' has been processed successfully.", string.Join("\r\n", logger.UserLogStrings.Cast<string>()));
			});
		}

		public void TestProcessingMessageAcknowledgement_NCTS()
		{
			var interchange = InterchangeProcessorTestHelper.CreateMessageAcknowledgementResponse(Factory, "TRANSACTIONID 001", applicationCode: EDIInterchange.ApplicationCodes.IECustomsNCTS);
			Factory.Save();

			var processor = new MessageRetrievingProcessor();
			processor.ExecuteBatch();
			var logger = processor.Logger;

			CombineAssertions(() =>
			{
				var anotherFactory = new BusinessObjectFactory();
				var processedInterchange = anotherFactory.Load<EDIInterchange>(interchange.PK);
				AssertEquals("EI_Status of processed EDIInterchange.", "RCV", processedInterchange.EI_Status);
				var processedMessage = processedInterchange.ContainedMessages.Cast<EDIMessage>().Single();
				AssertEquals("EM_ApplicationCode", EDIMessage.ApplicationCodes.IECustomsNCTS, processedMessage.EM_ApplicationCode);
				AssertEquals("EM_Status", "QUE", processedMessage.EM_Status);
				AssertEquals("EM_MessageSubType ", "ACK", processedMessage.EM_MessageSubType);
				AssertContains("Logs", "\tProcessing Interchange (Type:ACK, Number:1).\r\n\tCreated message (IEN, ACK, RCV, 101, TRANSACTIONID 001).\r\n\tInterchange '1' has been processed successfully.", string.Join("\r\n", logger.UserLogStrings.Cast<string>()));
			});
		}

		public void TestProcessingMessageAcknowledgement_EMCS()
		{
			var interchange = InterchangeProcessorTestHelper.CreateMessageAcknowledgementResponse(Factory, "TRANSACTIONID 001", applicationCode: EDIInterchange.ApplicationCodes.IECustomsEMCS);
			Factory.Save();

			var processor = new MessageRetrievingProcessor();
			processor.ExecuteBatch();
			var logger = processor.Logger;

			CombineAssertions(() =>
			{
				var anotherFactory = new BusinessObjectFactory();
				var processedInterchange = anotherFactory.Load<EDIInterchange>(interchange.PK);
				AssertEquals("EI_Status of processed EDIInterchange.", "RCV", processedInterchange.EI_Status);
				var processedMessage = processedInterchange.ContainedMessages.Cast<EDIMessage>().Single();
				AssertEquals("EM_Status", "QUE", processedMessage.EM_Status);
				AssertEquals("EM_MessageSubType ", "ACK", processedMessage.EM_MessageSubType);
				AssertContains("Logs", "\tProcessing Interchange (Type:ACK, Number:1).\r\n\tCreated message (IEM, ACK, RCV, 101, TRANSACTIONID 001).\r\n\tInterchange '1' has been processed successfully.", string.Join("\r\n", logger.UserLogStrings.Cast<string>()));
			});
		}

		public void TestProcessingMailBox_EMCS()
		{
			var company = Factory.New<GlbCompany>();
			company.GC_Code = "GC7";
			company.GC_RN_NKCountryCode = Core.Constants.CountryCodes.Ireland;
			var branch = company.Branches.AddNew();
			var wrapper = (IIEGlbCompanyWrapper)GlbCompanyWrapper.GetWrapper<GlbCompanyWrapper>(company);
			var certificationData = wrapper.GetEMCSGlbExternalPasswordCollectionOrCreateNew().AddNew();
			var outgoingInterchange = InterchangeCreator.CreateOutgoingInterchange(
										Factory,
										EDIInterchange.ApplicationCodes.IECustomsEMCS,
										CommonInterchangeTypeList.Codes.MailboxRequest,
										branch.PK, "", "", credentialPK: certificationData.PK);
			var outgoingMessage = Factory.New<AESOutboundEDIMessage>();
			outgoingInterchange.ContainedMessages.Add(outgoingMessage);
			outgoingMessage.EM_ApplicationCode = EDIMessage.ApplicationCodes.IECustomsEMCS;
			outgoingMessage.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			outgoingMessage.EM_MessageType = "803";
			outgoingMessage.EM_GB = branch.PK;
			outgoingMessage.EM_GP = certificationData.PK;
			outgoingMessage.EM_ApplicationReference = "TRAN001";
			outgoingMessage.EM_MessageNum = "EN1231";
			var testInterchange = InterchangeProcessorTestHelper.CreateIncomingInterchange(
									Factory,
									EDIInterchange.ApplicationCodes.IECustomsEMCS,
									CommonInterchangeTypeList.Codes.MailboxRequest,
									InterchangeProcessorTestHelper.GetMailboxItemText("TRAN001", GetEMCSIE803MessageText(),
									mailboxId: "ce45c655-c780-43be-94f8-69ef936ea871"),
									branchPK: branch.PK,
									sessionGUID: outgoingInterchange.EI_SessionGUID);
			testInterchange.EI_InterchangeNum = "IE8966";
			Factory.Save();

			var processor = new MessageRetrievingProcessor();
			processor.ExecuteBatch();
			var logger = processor.Logger;
			CombineAssertions(() =>
			{
				var newFactory = new BusinessObjectFactory();
				testInterchange = newFactory.Load<EDIInterchange>(testInterchange.PK);
				var createdMessage = testInterchange.ContainedMessages[0];
				AssertEquals("EM_ApplicationCode", "IEM", createdMessage.EM_ApplicationCode);
				AssertEquals("EM_ApplicationReference", "TRAN001", createdMessage.EM_ApplicationReference);
				AssertEquals("EM_MessageType", "803", createdMessage.EM_MessageType);
				AssertEquals("EM_ReceiveTransmit", "RCV", createdMessage.EM_ReceiveTransmit);
				AssertEquals("EM_MessageNum", "IE896601", createdMessage.EM_MessageNum);
				AssertEquals("EM_Status", "PPS", createdMessage.EM_Status);
				AssertEquals("EM_GP", certificationData.PK, createdMessage.EM_GP);
				AssertEquals("EI_GP incoming", certificationData.PK, testInterchange.EI_GP);
				var query = new ZQuery(EDIInterchangeSchema.EI_ApplicationCode, EDIInterchange.ApplicationCodes.IECustomsEMCS);
				query.AddToFilter(EDIInterchangeSchema.EI_InterchangeType, CommonInterchangeTypeList.Codes.MailboxAcknowledge);
				query.AddToFilter(EDIInterchangeSchema.EI_ReceiveTransmit, EDIInterchange.Direction.Transmit);
				query.AddToFilter(EDIInterchangeSchema.EI_TransportType, EDIInterchangeTransportTypeList.Codes.xT);
				query.AddToFilter(EDIInterchangeSchema.EI_Status, EDIInterchange.Status.Queued);
				query.AddToFilter(EDIInterchangeSchema.EI_GP, certificationData.PK);
				query.AddToFilter(EDIInterchangeSchema.EI_GB, branch.PK);
				var acknowledgeInterchange = newFactory.Load<EDIInterchange>(query).Single();
				AssertXmlElementValue(
					acknowledgeInterchange.EI_BodyText,
					@"http://www.ros.ie/schemas/customs/acknowledgerequest/v1",
					["MailboxId"],
					expectedValue: "ce45c655-c780-43be-94f8-69ef936ea871"
				);
				AssertContains("Logs", "\tProcessing Interchange (Type:MBR, Number:IE8966).\r\n\tProcessing 1 mailbox item.\r\n\tCreated message (IEM, 803, RCV, IE896601, TRAN001).\r\n\tCreated MBA Interchange 'IE8966A'.\r\n\tInterchange 'IE8966' has been processed successfully.", string.Join("\r\n", logger.UserLogStrings.Cast<string>()));
			});
		}

		static void AssertXmlElementValue(string xml, string namespaceUri, string[] elementPath, string expectedValue)
		{
			var doc = XDocument.Parse(xml);
			XNamespace ns = namespaceUri;

			XElement current = doc.Root;
			foreach (var name in elementPath)
			{
				current = current?.Element(ns + name);
				if (current == null)
				{
					throw new Exception($"Missing expected element: {string.Join("/", elementPath)}");
				}
			}

			if (!string.Equals(current.Value?.Trim(), expectedValue, StringComparison.Ordinal))
			{
				throw new Exception($"Expected value '{expectedValue}' but found '{current.Value}' at path: {string.Join("/", elementPath)}");
			}
		}

		public void TestProcessingMailBox_EMCS_OtherCompany801Response()
		{
			var company1 = Factory.New<GlbCompany>();
			company1.GC_Code = "GC7";
			company1.GC_RN_NKCountryCode = Core.Constants.CountryCodes.Ireland;
			var branchC1 = company1.Branches.AddNew();
			branchC1.GB_Code = "GB7";
			var wrapperC1 = (IIEGlbCompanyWrapper)GlbCompanyWrapper.GetWrapper<GlbCompanyWrapper>(company1);
			var certificationDataC1 = wrapperC1.GetEMCSGlbExternalPasswordCollectionOrCreateNew().AddNew();

			var company2 = Factory.New<GlbCompany>();
			company2.GC_Code = "GC8";
			company2.GC_RN_NKCountryCode = Core.Constants.CountryCodes.Ireland;
			var branchC2 = company2.Branches.AddNew();
			branchC2.GB_Code = "GB8";

			var outgoingInterchange = InterchangeCreator.CreateOutgoingInterchange(
										Factory,
										EDIInterchange.ApplicationCodes.IECustomsEMCS,
										CommonInterchangeTypeList.Codes.MailboxRequest,
										branchC1.PK, "", "", credentialPK: certificationDataC1.PK);
			var outgoingMessage = Factory.New<AESOutboundEDIMessage>();
			outgoingInterchange.ContainedMessages.Add(outgoingMessage);
			outgoingMessage.EM_ApplicationCode = EDIMessage.ApplicationCodes.IECustomsEMCS;
			outgoingMessage.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			outgoingMessage.EM_MessageType = "815";
			outgoingMessage.EM_GB = branchC1.PK;
			outgoingMessage.EM_GP = certificationDataC1.PK;
			outgoingMessage.EM_ApplicationReference = "TRAN001";
			outgoingMessage.EM_MessageNum = "EN1231";

			var responseInterchangeForOtherCompany = InterchangeProcessorTestHelper.CreateIncomingInterchange(
														Factory,
														EDIInterchange.ApplicationCodes.IECustomsEMCS,
														CommonInterchangeTypeList.Codes.MailboxRequest,
														InterchangeProcessorTestHelper.GetMailboxItemText("TRAN001", GetEMCSIE801MessageText(),
														mailboxId: "ce45c655-c780-43be-94f8-69ef936ea871"),
														branchPK: branchC2.PK,
														sessionGUID: Guid.NewGuid());
			responseInterchangeForOtherCompany.EI_InterchangeNum = "IE8966";
			Factory.Save();

			var processor = new MessageRetrievingProcessor();
			processor.ExecuteBatch();
			var logger = processor.Logger;
			CombineAssertions(() =>
			{
				var newFactory = new BusinessObjectFactory();
				responseInterchangeForOtherCompany = newFactory.Load<EDIInterchange>(responseInterchangeForOtherCompany.PK);
				AssertEquals("EI_GP not set as origin interchange is in a different session", ZGuid.Empty, responseInterchangeForOtherCompany.EI_GP);
				var createdMessage = responseInterchangeForOtherCompany.ContainedMessages[0];
				AssertEquals("EM_ApplicationCode", "IEM", createdMessage.EM_ApplicationCode);
				AssertEquals("EM_ApplicationReference", "TRAN001", createdMessage.EM_ApplicationReference);
				AssertEquals("EM_MessageType", "801", createdMessage.EM_MessageType);
				AssertEquals("EM_ReceiveTransmit", "RCV", createdMessage.EM_ReceiveTransmit);
				AssertEquals("EM_MessageNum", "IE896601", createdMessage.EM_MessageNum);
				AssertEquals("EM_Status", "QUE", createdMessage.EM_Status);
				AssertEquals("EM_GB", branchC2.PK, createdMessage.EM_GB);
				AssertEquals("EM_GP", ZGuid.Empty, createdMessage.EM_GP);

				var query = new ZQuery(EDIInterchangeSchema.EI_ApplicationCode, EDIInterchange.ApplicationCodes.IECustomsEMCS);
				query.AddToFilter(EDIInterchangeSchema.EI_InterchangeType, CommonInterchangeTypeList.Codes.MailboxAcknowledge);
				query.AddToFilter(EDIInterchangeSchema.EI_ReceiveTransmit, EDIInterchange.Direction.Transmit);
				query.AddToFilter(EDIInterchangeSchema.EI_TransportType, EDIInterchangeTransportTypeList.Codes.xT);
				query.AddToFilter(EDIInterchangeSchema.EI_Status, EDIInterchange.Status.Queued);
				query.AddToFilter(EDIInterchangeSchema.EI_GB, branchC2.PK);
				var acknowledgeInterchange = newFactory.Load<EDIInterchange>(query).Single();

				AssertXmlElementValue(
					acknowledgeInterchange.EI_BodyText,
					@"http://www.ros.ie/schemas/customs/acknowledgerequest/v1",
					["MailboxId"],
					expectedValue: "ce45c655-c780-43be-94f8-69ef936ea871"
				);

				var expectedLogEntries = "\tProcessing Interchange (Type:MBR, Number:IE8966).\r\n"
					+ "\tProcessing 1 mailbox item.\r\n"
					+ "\tCreated message (IEM, 801, RCV, IE896601, TRAN001).\r\n"
					+ "\tCreated MBA Interchange 'IE8966A'.\r\n"
					+ "\tInterchange 'IE8966' has been processed successfully.";
				AssertContains("Log Entries", expectedLogEntries, string.Join("\r\n", logger.UserLogStrings.Cast<string>()));
			});
		}

		string GetEMCSIE801MessageText() => @"<q1:IE801 xmlns:q1=""urn:publicid:-:EC:DGTAXUD:EMCS:PHASE4:IE801:V3.13""></q1:IE801>";
		string GetEMCSIE803MessageText() => @"<q1:IE803 xmlns:q1=""urn:publicid:-:EC:DGTAXUD:EMCS:PHASE4:IE803:V3.13""></q1:IE803>";

		public void TestProcessingInvalidMessageAcknowledgement()
		{
			var interchange = InterchangeProcessorTestHelper.CreateMessageAcknowledgementResponse(Factory, "TRANSACTIONID 002", valid: false);
			Factory.Save();

			var processor = new MessageRetrievingProcessor();
			processor.ExecuteBatch();
			var logger = processor.Logger;

			CombineAssertions(() =>
			{
				var anotherFactory = new BusinessObjectFactory();
				var processedInterchange = anotherFactory.Load<EDIInterchange>(interchange.PK);
				AssertEquals("EI_Status of processed EDIInterchange.", "ERR", processedInterchange.EI_Status);
				var errorReportLog = processedInterchange.Logs.MostRecentLogByEventTime(Events.ErrorReport);
				AssertContains("errorReportLog.SL_Reference", "Invalid message type. Expected: MessageAcknowledgement", errorReportLog.SL_Reference);
				AssertContains("Logs", "\tProcessing Interchange (Type:ACK, Number:1).\r\n\tInterchange #1: Status set to 'ERR' due to the following error: Invalid message type. Expected: MessageAcknowledgement", string.Join("\r\n", logger.UserLogStrings.Cast<string>()));
			});
		}

		public void TestProcessAllEDIInterchangeRegardlessOfBranch()
		{
			var ieData1 = InterchangeProcessorTestHelper.CreateCompanyAndBranch(Factory, Core.Constants.CountryCodes.Ireland);

			var interchange1 = AISInterchangeProcessorTestHelper.CreateStandardIE415VInterchange(Factory, "6debb28a-c9b0-44fb-9e85-0737b42aef48", "ACPTEST IM099044", "21IEDUB11A782454R2");
			interchange1.EI_GB = GlbBranch.CurrentBranch.PK;
			var interchange2 = AISInterchangeProcessorTestHelper.CreateStandardIM917Interchange(Factory, "07109E86-1C63-4DFA-B915-FA10EBC91633", "A19F4EB6-CB9E-4DD8-8A34-7C55CBB54C44");
			interchange2.EI_GB = ieData1.branch.PK;
			Factory.Save();
			var processor = new MessageRetrievingProcessor();
			processor.ExecuteBatch();
			CombineAssertions(() =>
			{
				var anotherFactory = new BusinessObjectFactory();
				var processedInterchange1 = anotherFactory.Load<EDIInterchange>(interchange1.PK);
				var processedInterchange2 = anotherFactory.Load<EDIInterchange>(interchange2.PK);
				AssertEquals("processedInterchange1.EI_Status", EDIInterchange.Status.Received, processedInterchange1.EI_Status);
				AssertEquals("processedInterchange2.EI_Status", EDIInterchange.Status.Received, processedInterchange2.EI_Status);
			});
		}

		public void TestProcessInterchangeUnrecognizedInterchangeType()
		{
			var interchange1 = InterchangeProcessorTestHelper.CreateIncomingInterchange(Factory, EDIInterchange.ApplicationCodes.IECustomsCommon, CommonInterchangeTypeList.Codes.MailboxAcknowledge, string.Empty);
			var interchange2 = InterchangeProcessorTestHelper.CreateIncomingInterchange(Factory, EDIInterchange.ApplicationCodes.IECustomsCommon, CommonInterchangeTypeList.Codes.MailboxRequest, string.Empty);
			var interchange3 = InterchangeProcessorTestHelper.CreateIncomingInterchange(Factory, EDIInterchange.ApplicationCodes.IECustomsCommon, CommonInterchangeTypeList.Codes.TransactionID, string.Empty);
			var interchange4 = InterchangeProcessorTestHelper.CreateIncomingInterchange(Factory, EDIInterchange.ApplicationCodes.IECustomsCommon, "XXX", string.Empty);
			var interchange5 = InterchangeProcessorTestHelper.CreateIncomingInterchange(Factory, EDIInterchange.ApplicationCodes.IECustomsCommon, CommonInterchangeTypeList.Codes.MessageAcknowledge, string.Empty);
			var interchange6 = InterchangeProcessorTestHelper.CreateIncomingInterchange(Factory, "iec", "mba", string.Empty);
			Factory.Save();
			var processor = new MessageRetrievingProcessor();
			processor.ExecuteBatch();
			var logger = processor.Logger;
			CombineAssertions(() =>
			{
				var anotherFactory = new BusinessObjectFactory();
				interchange1 = anotherFactory.Load<EDIInterchange>(interchange1.PK);
				interchange2 = anotherFactory.Load<EDIInterchange>(interchange2.PK);
				interchange3 = anotherFactory.Load<EDIInterchange>(interchange3.PK);
				interchange4 = anotherFactory.Load<EDIInterchange>(interchange4.PK);
				interchange5 = anotherFactory.Load<EDIInterchange>(interchange5.PK);
				interchange6 = anotherFactory.Load<EDIInterchange>(interchange6.PK);
				AssertNotEquals("interchange1.EI_Status - MBA", EDIInterchange.Status.Queued, interchange1.EI_Status);
				AssertNotEquals("interchange2.EI_Status - MBA", EDIInterchange.Status.Queued, interchange2.EI_Status);
				AssertEquals("interchange3.EI_Status - TID - should be ignored", EDIInterchange.Status.Queued, interchange3.EI_Status);
				AssertEquals("interchange4.EI_Status - XXX - should be ignored", EDIInterchange.Status.Queued, interchange4.EI_Status);
				AssertEquals("interchange5.EI_Status - ACK - should be ignored", EDIInterchange.Status.Queued, interchange5.EI_Status);
				AssertNotEquals("interchange6.EI_Status - iec - mba", EDIInterchange.Status.Queued, interchange6.EI_Status);

				var logs = string.Join("\r\n", logger.UserLogStrings.Cast<string>());
				AssertContains("Logs: Interchange 1 processed.", "Interchange '1' has been processed successfully.", logs);
				AssertContains("Logs: Interchange 6 processed.", "Interchange '6' has been processed successfully.", logs);

				AssertContains("Logs: Interchange 2 failed due to invalid body text.", "Processing Interchange (Type:MBR, Number:2).", logs);
				AssertContains("Logs: Interchange 2 failed due to invalid body text.",
					"Interchange #2: Status set to 'ERR' due to the following error: EI_BodyText does not contain a valid SOAP Envelope Body or the Body section is empty.",
					logs
				);
			});
		}

		public void TestProcessInterchangeUnrecognizedBodyText()
		{
			var interchange = AISInterchangeProcessorTestHelper.CreateStandardIE415VInterchange(Factory, "6debb28a-c9b0-44fb-9e85-0737b42aef48", "ACPTEST IM099044", "21IEDUB11A782454R2");
			interchange.EI_InterchangeNum = "INT001";
			interchange.EI_BodyText = "INVALID XML";
			Factory.Save();

			var processor = new MessageRetrievingProcessor();
			processor.ExecuteBatch();
			var logger = processor.Logger;

			CombineAssertions(() =>
			{
				var anotherFactory = new BusinessObjectFactory();
				var processedInterchange = anotherFactory.Load<EDIInterchange>(interchange.PK);
				AssertEquals("EI_Status of processed EDIInterchange.", EDIInterchange.Status.Error, processedInterchange.EI_Status);
				var errorReportLog = processedInterchange.Logs.MostRecentLogByEventTime(Events.ErrorReport);
				AssertContains("errorReportLog.SL_Reference", "EI_BodyText does not contain a valid SOAP Envelope Body or the Body section is empty.", errorReportLog.SL_Reference);
				AssertContains("Logs", "\tProcessing Interchange (Type:MBR, Number:INT001).\r\n\tInterchange #INT001: Status set to 'ERR' due to the following error: EI_BodyText does not contain a valid SOAP Envelope Body or the Body section is empty.", string.Join("\r\n", logger.UserLogStrings.Cast<string>()));
			});
		}

		public void TestNotProcessEMCSTransationIDInterchange()
		{
			var tidInterchange = Factory.New<EDIInterchange>();
			tidInterchange.EI_InterchangeType = CommonInterchangeTypeList.Codes.TransactionID;
			tidInterchange.EI_ApplicationCode = EDIInterchange.ApplicationCodes.IECustomsEMCS;
			tidInterchange.EI_ReceiveTransmit = EDIInterchange.Direction.Receive;
			tidInterchange.EI_TransportType = EDIInterchangeTransportTypeList.Codes.xT;
			tidInterchange.EI_SessionGUID = ZGuid.NewZGuid();
			tidInterchange.EI_From = "IECustomsTest";
			tidInterchange.EI_To = "IECustomsTest";
			tidInterchange.EI_Status = EDIInterchange.Status.Queued;
			Factory.Save();

			var processor = new MessageRetrievingProcessor();
			processor.ExecuteBatch();

			var anotherFactory = new BusinessObjectFactory();
			tidInterchange = anotherFactory.Load<EDIInterchange>(tidInterchange.PK);
			AssertEquals("Should not process EMCS TransationID Interchange", EDIInterchange.Status.Queued, tidInterchange.EI_Status);
		}

		public void TestProcessIE415VInterchange()
		{
			var interchange = AISInterchangeProcessorTestHelper.CreateAISVersion2_0IM415VInterchange(Factory, "6debb28a-c9b0-44fb-9e85-0737b42aef48", "A", "ACPTEST IM099044", "21IEDUB11A782454R2", new ZDateTime(2023, 08, 11));
			Factory.Save();

			var processor = new MessageRetrievingProcessor();
			processor.ExecuteBatch();

			CombineAssertions(() =>
			{
				var anotherFactory = new BusinessObjectFactory();
				var processedInterchange = anotherFactory.Load<EDIInterchange>(interchange.PK);
				AssertEquals("EI_Status of processed EDIInterchange.", "RCV", processedInterchange.EI_Status);
				var processedMessage = (EDIMessage)processedInterchange.ContainedMessages.First();
				AssertNotNull("Processed EDIMessage should exist.", processedMessage);
				AssertEquals("EM_Status", "QUE", processedMessage.EM_Status);
				AssertEquals("EM_ApplicationReference(TransactionID)", "6debb28a-c9b0-44fb-9e85-0737b42aef48", processedMessage.EM_ApplicationReference);
			});
		}

		public void TestProcessIM917Interchange()
		{
			var interchange = AISInterchangeProcessorTestHelper.CreateStandardIM917Interchange(Factory, "020ddfa8-b792-452a-bb65-51b36a83937c");
			Factory.Save();

			new MessageRetrievingProcessor().ExecuteBatch();

			CombineAssertions(() =>
			{
				var anotherFactory = new BusinessObjectFactory();
				var processedInterchange = anotherFactory.Load<EDIInterchange>(interchange.PK);
				AssertEquals("EI_Status of processed EDIInterchange.", "RCV", processedInterchange.EI_Status);
				var processedMessage = (EDIMessage)processedInterchange.ContainedMessages.First();
				AssertNotNull("Processed EDIMessage should exist.", processedMessage);
				AssertEquals("EM_Status", "QUE", processedMessage.EM_Status);
				AssertEquals("EM_ApplicationReference(TransactionID)", "020ddfa8-b792-452a-bb65-51b36a83937c", processedMessage.EM_ApplicationReference);
			});
		}
	}
}
