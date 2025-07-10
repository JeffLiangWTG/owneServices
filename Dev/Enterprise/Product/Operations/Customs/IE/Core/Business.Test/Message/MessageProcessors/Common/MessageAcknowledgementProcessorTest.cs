using System;
using System.Data;
using System.Linq;
using CargoWise.Application;
using CargoWise.Customs.IE.MessageDefinitions.Common.MessageAcknowledgement;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.EU.Registry;
using Enterprise.Customs.IE.Business.Declaration;
using Enterprise.Customs.IE.Messaging;
using Enterprise.Customs.IE.Messaging.Testing;
using Enterprise.Customs.Universal.Testing;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Schema;
using Moq;
using ServiceManager.Integration.Abstractions;

namespace Enterprise.Customs.IE.Business.Testing
{
	class MessageAcknowledgementProcessorTest : TestCaseWithFactory
	{
		public void TestMessageFriendlyName()
		{
			AssertEquals("MessageFriendlyName", "Message Acknowledgement", new MessageAcknowledgementProcessor(new LoggingInformation()).MessageFriendlyName);
		}

		public void TestPreProcess()
		{
			var (ieCompany, ieBranch) = InterchangeProcessorTestHelper.CreateCompanyAndBranch(Factory, Core.Constants.CountryCodes.Ireland);
			var incomingMessage = InterchangeProcessorTestHelper.CreateMessageAcknowledgementMessage<AESInboundEDIMessage>(Factory, EDIMessage.ApplicationCodes.IECustomsExport, AESOutgoingMessageTypeList.Codes.ArrivalAtExit, transactionId);
			using (Factory.AddDisposableService())
			{
				var (entryHeader, outgoingMessage, processor) = CreateOriginalData(Factory);
				outgoingMessage.EM_GB = ieBranch.PK;
				processor.PreProcessMessage(incomingMessage);

				CombineAssertions(() =>
				{
					AssertEquals("EM_GB should have been set.", ieBranch.PK, incomingMessage.EM_GB);
					AssertEquals("Status should be PreProcessedOK.", EDIMessage.Status.PreProcessedOK, incomingMessage.EM_Status);
					AssertSame("Incoming message should have been linked to CusEntryHeader.", entryHeader, incomingMessage.EM_LinkedObject);
				});
			}
		}

		public void TestMessageAcknowlegement_Accepted()
		{
			var incomingMessage = InterchangeProcessorTestHelper.CreateMessageAcknowledgementMessage<AESInboundEDIMessage>(Factory, EDIMessage.ApplicationCodes.IECustomsExport, AESOutgoingMessageTypeList.Codes.ArrivalAtExit, transactionId);

			using (ObjectFactory.Substitute(Mock.Of<INudgingController>()))
			using (Factory.AddDisposableService())
			{
				var (entryHeader, outgoingMessage, processor) = CreateOriginalData(Factory);
				AssertNoExceptionThrown("Should not throw any execption for ACCEPTED message.", () =>
				{
					processor.PreProcessMessage(incomingMessage);
					processor.ProcessMessage(incomingMessage);
				});
				AssertEquals("outgoingMessage.EM_Status", EDIMessage.Status.Acknowledged, outgoingMessage.EM_Status);
			}
		}

		public void TestMessageAcknowlegement_Rejected_Error()
		{
			var staff = MessageProcessorNotificationTestHelper.SetupStaffData(Factory);
			var emailGroup = EUCustomsDataRegistry.Instance.SendExportAcknowledgements.GetFallBackValueAtAllLevels(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty);
			emailGroup.SendMode = Core.Constants.EmailTo.NominatedGroup;
			emailGroup.SendGroupPK = MessageProcessorNotificationTestHelper.SetupStaffGroup(Factory).PK;

			var (entryHeader, outgoingMessage, processor) = CreateOriginalData(Factory);
			outgoingMessage.EM_SystemCreateUser = staff.GS_Code;

			var incomingMessage = InterchangeProcessorTestHelper.CreateMessageAcknowledgementMessage<AESInboundEDIMessage>(factory: Factory,
				applicationCode: EDIMessage.ApplicationCodes.IECustomsExport,
				messageType: AESOutgoingMessageTypeList.Codes.ArrivalAtExit,
				transactionId: transactionId,
				messageStatus: MessageStatus.Rejected,
				transactionIdStatus: TransactionIdStatus.Error,
				errorCode: "ER1"
			);
			Factory.Save();

			using (EUCustomsDataRegistry.Instance.SendExportAcknowledgements.SetTemporaryValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, emailGroup))
			using (Factory.AddDisposableService())
			{
				processor.PreProcessMessage(incomingMessage);
				processor.ProcessMessage(incomingMessage);

				CombineAssertions("Result for processing REJECTED ERROR message.", () =>
				{
					AssertEquals("CusEntryHeader.CH_Status", "FAL", entryHeader.CH_Status);
					var messageTypeDesc = outgoingMessage.MessageTypeWithDescription;
					MessageProcessorNotificationTestHelper.AssertEmail(
						expectingSubject: $@"{messageTypeDesc} Response (Failure) for " + entryHeader.Declaration.JE_DeclarationReference,
						expectingBodyTexts: new[] {
							"Submission has been rejected. Transaction ID Status: Error - Error",
							$@"<tr><td>Transaction ID</td><td>{transactionId}</td></tr>",
							$@"<tr><td>Message Type</td><td>{messageTypeDesc}</td></tr>",
							$@"<tr><td>Message Number</td><td>{outgoingMessage.EM_MessageNum}</td></tr>",
							"Entry status has been set ERR, Movement Reference Number: ."
						},
						expectingRecipients: new string[] { "staff3@where.com", "staff4@where.com", "staff5@where.com" });
					AssertEquals("outgoingMessage.EM_Status", EDIMessage.Status.Sent, outgoingMessage.EM_Status);
				});
			}
		}

		public void TestMessageAcknowlegement_Rejected_ROSError()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var eun = helper.CreateNewOrGetExistingDataGrouping(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, "European Union");
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Ireland, parent: eun);
			helper.CreateNewOrGetExistingCusCodeType(Constants.RefCusCodeListTypes.RevenueErrorType, "IEROS");
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Ireland, Constants.RefCusCodeListTypes.RevenueErrorType, "CODE", "DESCRIPTION", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);

			var sessionGuid = ZGuid.BrettsGuid;
			var staff = MessageProcessorNotificationTestHelper.SetupStaffData(Factory);
			var (entryHeader, outgoingMessage, processor) = CreateOriginalData(Factory);
			outgoingMessage.EM_SystemCreateUser = staff.GS_Code;

			var incomingMessage = InterchangeProcessorTestHelper.CreateMessageAcknowledgementServiceErrorMessage<AESInboundEDIMessage>(factory: Factory,
				applicationCode: EDIMessage.ApplicationCodes.IECustomsExport,
				messageType: AESOutgoingMessageTypeList.Codes.ExportOriginal,
				errorCode: "CODE"
			);

			incomingMessage.EM_LinkedObject = outgoingMessage.EM_LinkedObject;
			outgoingMessage.EM_Status = EDIMessageStatusList.Codes.Sent;

			Factory.Save();

			using (Factory.AddDisposableService())
			{
				processor.PreProcessMessage(incomingMessage);
				processor.ProcessMessage(incomingMessage);

				CombineAssertions("Result for processing ROS ERROR message.", () =>
				{
					AssertEquals("CusEntryHeader.CH_Status", "FAL", entryHeader.CH_Status);
					var messageTypeDesc = outgoingMessage.MessageTypeWithDescription;
					var expectedInterpretation = $@"Error submitting message. Error Code: CODE - DESCRIPTION<br />
<br /><table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table""><tr><td>Transaction ID</td><td>TRS0000001</td></tr><tr><td>Message Type</td><td>507 - CC507C: Arrival at Exit</td></tr><tr><td>Message Number</td><td>IEE00000000000001</td></tr></table><br />Entry status has been set ERR<br />";
					AssertXMLEquals("Interpretation", expectedInterpretation, incomingMessage.EM_MessageInterpretation);
					MessageProcessorNotificationTestHelper.AssertEmail(
						expectingSubject: $@"{messageTypeDesc} Response (Failure) for " + entryHeader.Declaration.JE_DeclarationReference,
						expectingBodyTexts: new[] {
							"Error submitting message. Error Code: CODE - DESCRIPTION",
							$@"<tr><td>Transaction ID</td><td>{transactionId}</td></tr>",
							$@"<tr><td>Message Type</td><td>{messageTypeDesc}</td></tr>",
							$@"<tr><td>Message Number</td><td>{outgoingMessage.EM_MessageNum}</td></tr>",
							"Entry status has been set ERR"
						},
						expectingRecipients: new string[] { staff.GS_EmailAddress });
					AssertEquals("outgoingMessage.EM_Status", EDIMessage.Status.Sent, outgoingMessage.EM_Status);
					AssertEquals("incomingMessage.EM_Status", EDIMessage.Status.Received, incomingMessage.EM_Status);
				});
			}
		}

		public void TestMessageAcknowlegement_Rejected_Invalid_Resubmissions()
		{
			var incomingMessage = InterchangeProcessorTestHelper.CreateMessageAcknowledgementMessage<AESInboundEDIMessage>(factory: Factory,
				applicationCode: EDIMessage.ApplicationCodes.IECustomsExport,
				messageType: AESOutgoingMessageTypeList.Codes.ArrivalAtExit,
				transactionId: transactionId,
				messageStatus: MessageStatus.Rejected,
				transactionIdStatus: TransactionIdStatus.Invalid,
				errorCode: "IV1"
			);

			using (Factory.AddDisposableService())
			{
				var (ieCompany, ieBranch) = InterchangeProcessorTestHelper.CreateCompanyAndBranch(Factory, Core.Constants.CountryCodes.Ireland);
				var (entryHeader, outgoingMessage, processor) = CreateOriginalData(Factory);
				outgoingMessage.EM_GB = ieBranch.PK;

				processor.PreProcessMessage(incomingMessage);
				processor.ProcessMessage(incomingMessage);
				var newReSubmission = Factory.Load<EDIMessage>(
					new ZQuery(EDIMessageSchema.EM_ReceiveTransmit, EDIMessage.Direction.Transmit)
						.AddToFilter(EDIMessageSchema.EM_MessageNum, SQLComparisonOperator.StartsWith, outgoingMessage.EM_MessageNum + "_")
				).Single();

				CombineAssertions("Properties of re-submission.", () =>
				{
					AssertEquals("EM_ApplicationCode", EDIMessage.ApplicationCodes.IECustomsExport, newReSubmission.EM_ApplicationCode);
					AssertEquals("EM_GB", ieBranch.PK, newReSubmission.EM_GB);
					AssertEquals("EM_MessageType", AESOutgoingMessageTypeList.Codes.ArrivalAtExit, newReSubmission.EM_MessageType);
					AssertEquals("EM_ReceiveTransmit", EDIMessage.Direction.Transmit, newReSubmission.EM_ReceiveTransmit);
					AssertSame("EM_LinkedObject", entryHeader, newReSubmission.EM_LinkedObject);

					AssertEquals("EM_ApplicationReference should be set empty try to trigger another TransactionID allocation.", string.Empty, newReSubmission.EM_ApplicationReference);
					AssertEquals("EM_MessageNum should be original num + _ + resubmission count.", outgoingMessage.EM_MessageNum + "_1", newReSubmission.EM_MessageNum);
					AssertEquals("Should not create eMail for failed for the first time.", 0, Env.OutgoingCustomsMailManager.EmailsCreated.Count);
					AssertEquals("outgoingMessage.EM_Status", EDIMessage.Status.Sent, outgoingMessage.EM_Status);
				});
			}
		}

		public void TestMessageAcknowlegement_Rejected_Invalid_Existing1Resubmission()
		{
			var (ieCompany, ieBranch) = InterchangeProcessorTestHelper.CreateCompanyAndBranch(Factory, Core.Constants.CountryCodes.Ireland);
			var (entryHeader, outgoingMessage, processor) = CreateOriginalData(Factory);
			outgoingMessage.EM_GB = ieBranch.PK;
			var existingResubmission = (EDIMessage)outgoingMessage.Clone();
			existingResubmission.EM_ApplicationReference = transactionId + "_1";
			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var incomingMessage = InterchangeProcessorTestHelper.CreateMessageAcknowledgementMessage<AESInboundEDIMessage>(factory: newFactory,
				applicationCode: EDIMessage.ApplicationCodes.IECustomsExport,
				messageType: AESOutgoingMessageTypeList.Codes.ArrivalAtExit,
				transactionId: transactionId,
				messageStatus: MessageStatus.Rejected,
				transactionIdStatus: TransactionIdStatus.Invalid,
				errorCode: "IV2"
			);

			using (Factory.AddDisposableService())
			{
				processor.PreProcessMessage(incomingMessage);
				processor.ProcessMessage(incomingMessage);

				var newReSubmission = newFactory.Load<EDIMessage>(
					new ZQuery(EDIMessageSchema.EM_ReceiveTransmit, EDIMessage.Direction.Transmit)
						.AddToFilter(EDIMessageSchema.EM_MessageNum, SQLComparisonOperator.StartsWith, outgoingMessage.EM_MessageNum + "_")
						.AddToFilter(EDIMessageSchema.PK, SQLComparisonOperator.NotEqual, existingResubmission.PK)
				).Single();

				CombineAssertions("Properties of 2nd re-submission.", () =>
				{
					AssertEquals("EM_ApplicationCode", EDIMessage.ApplicationCodes.IECustomsExport, newReSubmission.EM_ApplicationCode);
					AssertEquals("EM_GB", ieBranch.PK, newReSubmission.EM_GB);
					AssertEquals("EM_MessageType", AESOutgoingMessageTypeList.Codes.ArrivalAtExit, newReSubmission.EM_MessageType);
					AssertEquals("EM_ReceiveTransmit", EDIMessage.Direction.Transmit, newReSubmission.EM_ReceiveTransmit);
					AssertEquals("EM_LinkUniqueID", entryHeader.PK, newReSubmission.EM_LinkUniqueID);

					AssertEquals("EM_ApplicationReference should be set empty try to trigger another TransactionID allocation.", string.Empty, newReSubmission.EM_ApplicationReference);
					AssertEquals("EM_MessageNum should be original num + _ + resubmission count.", outgoingMessage.EM_MessageNum + "_2", newReSubmission.EM_MessageNum);
					AssertEquals("Should not create eMail for failed for the first time.", 0, Env.OutgoingCustomsMailManager.EmailsCreated.Count);
					AssertEquals("outgoingMessage.EM_Status", EDIMessage.Status.Sent, outgoingMessage.EM_Status);
				});
			}
		}

		public void TestMessageAcknowlegement_Rejected_Invalid_Existing2Resubmissions()
		{
			var (ieCompany, ieBranch) = InterchangeProcessorTestHelper.CreateCompanyAndBranch(Factory, Core.Constants.CountryCodes.Ireland);
			var (entryHeader, outgoingMessage, processor) = CreateOriginalData(Factory);
			outgoingMessage.EM_GB = ieBranch.PK;
			var existingResubmission1 = (EDIMessage)outgoingMessage.Clone();
			existingResubmission1.EM_ApplicationReference = transactionId + "_1";
			var existingResubmission2 = (EDIMessage)outgoingMessage.Clone();
			existingResubmission2.EM_ApplicationReference = transactionId + "_2";
			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var incomingMessage = InterchangeProcessorTestHelper.CreateMessageAcknowledgementMessage<AESInboundEDIMessage>(factory: newFactory,
				applicationCode: EDIMessage.ApplicationCodes.IECustomsExport,
				messageType: AESOutgoingMessageTypeList.Codes.ArrivalAtExit,
				transactionId: transactionId,
				messageStatus: MessageStatus.Rejected,
				transactionIdStatus: TransactionIdStatus.Invalid,
				errorCode: "IV2"
			);

			using (Factory.AddDisposableService())
			{
				processor.PreProcessMessage(incomingMessage);
				processor.ProcessMessage(incomingMessage);

				var newReSubmission = newFactory.Load<EDIMessage>(
					new ZQuery(EDIMessageSchema.EM_ReceiveTransmit, EDIMessage.Direction.Transmit)
						.AddToFilter(EDIMessageSchema.EM_MessageNum, SQLComparisonOperator.StartsWith, outgoingMessage.EM_MessageNum + "_")
						.AddToFilter(EDIMessageSchema.PK, SQLComparisonOperator.NotEqual, new[] { existingResubmission1.PK, existingResubmission2.PK })
				).Single();

				CombineAssertions("Properties of 3rd re-submission.", () =>
				{
					AssertEquals("EM_ApplicationCode", EDIMessage.ApplicationCodes.IECustomsExport, newReSubmission.EM_ApplicationCode);
					AssertEquals("EM_GB", ieBranch.PK, newReSubmission.EM_GB);
					AssertEquals("EM_MessageType", AESOutgoingMessageTypeList.Codes.ArrivalAtExit, newReSubmission.EM_MessageType);
					AssertEquals("EM_ReceiveTransmit", EDIMessage.Direction.Transmit, newReSubmission.EM_ReceiveTransmit);
					AssertEquals("EM_LinkUniqueID", entryHeader.PK, newReSubmission.EM_LinkUniqueID);

					AssertEquals("EM_ApplicationReference should be set empty try to trigger another TransactionID allocation.", string.Empty, newReSubmission.EM_ApplicationReference);
					AssertEquals("EM_MessageNum should be original num + _ + resubmission count.", outgoingMessage.EM_MessageNum + "_3", newReSubmission.EM_MessageNum);
					AssertEquals("Should not create eMail for failed for the first time.", false, Env.OutgoingCustomsMailManager.EmailsCreated.Any());
					AssertEquals("outgoingMessage.EM_Status", EDIMessage.Status.Sent, outgoingMessage.EM_Status);
				});
			}
		}

		public void TestMessageAcknowlegement_Rejected_Invalid_Existing3Resubmissions()
		{
			var staff = MessageProcessorNotificationTestHelper.SetupStaffData(Factory);
			var (ieCompany, ieBranch) = InterchangeProcessorTestHelper.CreateCompanyAndBranch(Factory, Core.Constants.CountryCodes.Ireland);
			var (entryHeader, outgoingMessage, processor) = CreateOriginalData(Factory);
			outgoingMessage.EM_GB = ieBranch.PK;
			outgoingMessage.EM_SystemCreateUser = staff.GS_Code;
			var existingResubmission1 = (EDIMessage)outgoingMessage.Clone();
			existingResubmission1.EM_ApplicationReference = transactionId + "_1";
			var existingResubmission2 = (EDIMessage)outgoingMessage.Clone();
			existingResubmission2.EM_ApplicationReference = transactionId + "_2";
			var existingResubmission3 = (EDIMessage)outgoingMessage.Clone();
			existingResubmission3.EM_ApplicationReference = transactionId + "_3";

			Factory.Save();

			var incomingMessage = InterchangeProcessorTestHelper.CreateMessageAcknowledgementMessage<AESInboundEDIMessage>(factory: Factory,
				applicationCode: EDIMessage.ApplicationCodes.IECustomsExport,
				messageType: AESOutgoingMessageTypeList.Codes.ArrivalAtExit,
				transactionId: transactionId,
				messageStatus: MessageStatus.Rejected,
				transactionIdStatus: TransactionIdStatus.Invalid,
				errorCode: "IV4"
			);

			using (Factory.AddDisposableService())
			{
				processor.PreProcessMessage(incomingMessage);
				processor.ProcessMessage(incomingMessage);

				var newReSubmissions = Factory.Load<EDIMessage>(
					new ZQuery(EDIMessageSchema.EM_ReceiveTransmit, EDIMessage.Direction.Transmit)
						.AddToFilter(EDIMessageSchema.EM_MessageNum, SQLComparisonOperator.StartsWith, outgoingMessage.EM_MessageNum + "_")
						.AddToFilter(EDIMessageSchema.PK, SQLComparisonOperator.NotEqual, new[] { existingResubmission1.PK, existingResubmission2.PK, existingResubmission3.PK })
				);

				CombineAssertions("Expectings: Not more re-submission created; EntryHeader set ERR; Notification eMail created.", () =>
				{
					AssertEquals("No re-submission", 0, newReSubmissions.Length);
					AssertEquals("CusEntryHeader status", "FAL", entryHeader.CH_Status);
					var messageTypeDesc = outgoingMessage.MessageTypeWithDescription;
					MessageProcessorNotificationTestHelper.AssertEmail(
						expectingSubject: $@"{messageTypeDesc} Response (Failure) for " + entryHeader.Declaration.JE_DeclarationReference,
						expectingBodyTexts: new[]
						{
						$@"Submission has been rejected. Transaction ID Status: Invalid - Invalid",
						$@"<tr><td>Transaction ID</td><td>{transactionId}</td></tr>",
						$@"<tr><td>Message Type</td><td>{messageTypeDesc}</td></tr>",
						$@"<tr><td>Message Number</td><td>{outgoingMessage.EM_MessageNum}</td></tr>",
						"Entry status has been set ERR, Movement Reference Number: .",
						},
						expectingRecipients: new string[] { staff.GS_EmailAddress }
					);
					AssertEquals("outgoingMessage.EM_Status", EDIMessage.Status.Sent, outgoingMessage.EM_Status);
				});
			}
		}

		public void TestMessageAcknowlegement_Rejected_Expired()
		{
			var (ieCompany, ieBranch) = InterchangeProcessorTestHelper.CreateCompanyAndBranch(Factory, Core.Constants.CountryCodes.Ireland);
			var (entryHeader, outgoingMessage, processor) = CreateOriginalData(Factory);
			outgoingMessage.EM_GB = ieBranch.PK;
			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var incomingMessage = InterchangeProcessorTestHelper.CreateMessageAcknowledgementMessage<AESInboundEDIMessage>(factory: newFactory,
				applicationCode: EDIMessage.ApplicationCodes.IECustomsExport,
				messageType: AESOutgoingMessageTypeList.Codes.ArrivalAtExit,
				transactionId: transactionId,
				messageStatus: MessageStatus.Rejected,
				transactionIdStatus: TransactionIdStatus.Expired,
				errorCode: "IV1"
			);

			using (Factory.AddDisposableService())
			{
				processor.PreProcessMessage(incomingMessage);
				processor.ProcessMessage(incomingMessage);

				var newReSubmission = newFactory.Load<EDIMessage>(
					new ZQuery(EDIMessageSchema.EM_ReceiveTransmit, EDIMessage.Direction.Transmit)
					.AddToFilter(EDIMessageSchema.PK, SQLComparisonOperator.NotEqual, outgoingMessage.PK)
				).Single();

				CombineAssertions("Properties of re-submission.", () =>
				{
					AssertEquals("EM_ApplicationCode", EDIMessage.ApplicationCodes.IECustomsExport, newReSubmission.EM_ApplicationCode);
					AssertEquals("EM_GB", ieBranch.PK, newReSubmission.EM_GB);
					AssertEquals("EM_MessageType", AESOutgoingMessageTypeList.Codes.ArrivalAtExit, newReSubmission.EM_MessageType);
					AssertEquals("EM_ReceiveTransmit", EDIMessage.Direction.Transmit, newReSubmission.EM_ReceiveTransmit);
					AssertEquals("EM_LinkUniqueID", entryHeader.PK, newReSubmission.EM_LinkUniqueID);

					AssertEquals("EM_ApplicationReference should be set empty try to trigger another TransactionID allocation.", string.Empty, newReSubmission.EM_ApplicationReference);
					AssertEquals("EM_MessageNum should be original num + _ + resubmission count.", outgoingMessage.EM_MessageNum + "_1", newReSubmission.EM_MessageNum);
					AssertEquals("Should not create eMail for failed for the first time.", 0, Env.OutgoingCustomsMailManager.EmailsCreated.Count);
					AssertEquals("outgoingMessage.EM_Status", EDIMessage.Status.Sent, outgoingMessage.EM_Status);
				});
			}
		}

		public void TestIE583MessageAcknowlegement_Accepted_AdditionalAction()
		{
			var incomingMessage = InterchangeProcessorTestHelper.CreateMessageAcknowledgementMessage<AESInboundEDIMessage>(Factory, EDIMessage.ApplicationCodes.IECustomsExport, AESOutgoingMessageTypeList.Codes.InformationOnNonExitedExport, transactionId);
			incomingMessage.EM_MessageSubType = CommonInterchangeTypeList.Codes.MessageAcknowledge;
			using (ObjectFactory.Substitute(Mock.Of<INudgingController>()))
			using (Factory.AddDisposableService())
			{
				var (exitReport, outgoingMessage) = CreateOriginalDataWithAdditionalProcessing(Factory);
				var processor = new MessageAcknowledgementProcessor(null);
				AssertNoExceptionThrown("Should not throw any execption for ACCEPTED message.", () =>
				{
					processor.PreProcessMessage(incomingMessage);
					processor.ProcessMessage(incomingMessage);
				});
				exitReport.Factory.Save();
				AssertEquals("outgoingMessage.EM_Status", EDIMessage.Status.Acknowledged, outgoingMessage.EM_Status);
				exitReport.Reload();
				AssertEquals("original exit report.CER_MessageStatus", "ACC", exitReport.CER_MessageStatus);
			}
		}

		public void TestIE590MessageAcknowlegement_Accepted_AdditionalAction()
		{
			var incomingMessage = InterchangeProcessorTestHelper.CreateMessageAcknowledgementMessage<AESInboundEDIMessage>(Factory, EDIMessage.ApplicationCodes.IECustomsExport, AESOutgoingMessageTypeList.Codes.ExitNotification, transactionId);
			incomingMessage.EM_MessageSubType = CommonInterchangeTypeList.Codes.MessageAcknowledge;
			using (ObjectFactory.Substitute(Mock.Of<INudgingController>()))
			using (Factory.AddDisposableService())
			{
				var (exitReport, outgoingMessage) = CreateOriginalDataWithAdditionalProcessing(Factory);
				var processor = new MessageAcknowledgementProcessor(null);
				AssertNoExceptionThrown("Should not throw any execption for ACCEPTED message.", () =>
				{
					processor.PreProcessMessage(incomingMessage);
					processor.ProcessMessage(incomingMessage);
				});
				exitReport.Factory.Save();
				AssertEquals("outgoingMessage.EM_Status", EDIMessage.Status.Acknowledged, outgoingMessage.EM_Status);
				exitReport.Reload();
				AssertEquals("original exit report.CER_MessageStatus", "ACC", exitReport.CER_MessageStatus);
			}
		}

		const string transactionId = "TRS0000001";

		internal static (CusEntryHeader entryHeader, AESOutboundEDIMessage outgoingMessage, MessageAcknowledgementProcessor processor) CreateOriginalData(BusinessObjectFactory factory, LoggingInformation logger = null)
		{
			var declaration = factory.New<JobDeclaration>();
			declaration.JE_DeclarationReference = "B012345678";
			var entryHeader = (CusEntryHeader)declaration.ActiveEntryHeaders.AddNew();
			var outgoingMessage = factory.New<AESOutboundEDIMessage>();
			outgoingMessage.EM_ApplicationCode = EDIMessage.ApplicationCodes.IECustomsExport;
			outgoingMessage.EM_MessageType = AESOutgoingMessageTypeList.Codes.ArrivalAtExit;
			outgoingMessage.EM_ApplicationReference = transactionId;
			outgoingMessage.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			outgoingMessage.EM_LinkedObject = entryHeader;
			outgoingMessage.EM_Status = EDIMessage.Status.Sent;
			return (entryHeader, outgoingMessage, new MessageAcknowledgementProcessor(logger ?? new LoggingInformation()));
		}

		internal static (ExitControl.Business.CusExitReport exitReport, AESOutboundEDIMessage outgoingMessage) CreateOriginalDataWithAdditionalProcessing(BusinessObjectFactory factory)
		{
			var header = factory.New<ExitControl.Business.CusExitHeader>();
			var exitReport = header.CusExitReports.AddNew();
			exitReport.CER_MessageStatus = "SNT";
			var consignment = header.CusExitConsignments.AddNew();
			exitReport.CER_CXC_Consignment = consignment.PK;

			var outgoingMessage = factory.New<AESOutboundEDIMessage>();
			outgoingMessage.EM_ApplicationCode = EDIMessage.ApplicationCodes.IECustomsExport;
			outgoingMessage.EM_MessageType = AESOutgoingMessageTypeList.Codes.ExitNotification;
			outgoingMessage.EM_ApplicationReference = transactionId;
			outgoingMessage.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			outgoingMessage.EM_LinkedObject = exitReport;
			outgoingMessage.EM_Status = EDIMessage.Status.Acknowledged;
			factory.Save();
			return (exitReport, outgoingMessage);
		}

		internal static (Integration.Customs.IEEMCS.IEMCSJobDeclaration declaration, EMCSOutboundMessageForTest outgoingMessage, MessageAcknowledgementProcessor processor) CreateOriginalDataForEMCS(BusinessObjectFactory factory, LoggingInformation logger = null)
		{
			var declaration = factory.New<Integration.Customs.IEEMCS.IEMCSJobDeclaration>();
			declaration.JE_DeclarationReference = "E012345678";
			var outgoingMessage = factory.New<EMCSOutboundMessageForTest>();
			outgoingMessage.EM_ApplicationCode = EDIMessage.ApplicationCodes.IECustomsEMCS;
			outgoingMessage.EM_MessageType = "815";
			outgoingMessage.EM_ApplicationReference = transactionId;
			outgoingMessage.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			outgoingMessage.EM_LinkedObject = (BusinessObject)declaration;
			outgoingMessage.EM_Status = EDIMessage.Status.Sent;
			return (declaration, outgoingMessage, new MessageAcknowledgementProcessor(logger ?? new LoggingInformation()));
		}

		internal sealed class EMCSOutboundMessageForTest : OutboundEDIMessage
		{
			public EMCSOutboundMessageForTest(BusinessObjectFactory factory, DataRow row) : base(factory, row)
			{
			}

			protected override string GetMessageReferenceNumber() => EM_MessageNum;
		}
	}
}
