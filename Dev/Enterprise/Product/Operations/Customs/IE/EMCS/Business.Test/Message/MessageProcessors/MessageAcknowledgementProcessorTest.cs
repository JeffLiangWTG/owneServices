using System;
using System.Linq;
using CargoWise.Application;
using CargoWise.Customs.IE.MessageDefinitions.Common.MessageAcknowledgement;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.EU.EMCS.Business;
using Enterprise.Customs.EU.EMCS.Registry;
using Enterprise.Customs.IE.Business;
using Enterprise.Customs.IE.Business.Testing;
using Enterprise.Customs.IE.EMCS.Messaging;
using Enterprise.Customs.IE.Messaging.Testing;
using Enterprise.Customs.Universal.Testing;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using Moq;
using ServiceManager.Integration.Abstractions;

namespace Enterprise.Customs.IE.EMCS.Business.Testing
{
	sealed class MessageAcknowledgementProcessorTest : TestCaseWithFactory
	{
		public void TestMessageAcknowledgement_Rejected_Error_EMCS_EmailConsignor()
		{
			var emailGroup = EmcsCustomsDataRegistry.Instance.EmcsSendConsignorAcknowledgements.GetFallBackValueAtAllLevels(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty);
			emailGroup.SendMode = Core.Constants.EmailTo.NominatedGroup;
			emailGroup.SendGroupPK = MessageProcessorNotificationTestHelper.SetupStaffGroup(Factory).PK;

			var incomingMessage = CreateIncomingMessageAcknowledgement();
			var staff = MessageProcessorNotificationTestHelper.SetupStaffData(Factory);
			var (declaration, outgoingMessage, processor) = CreateOriginalDataForEMCS(Factory);
			outgoingMessage.EM_SystemCreateUser = staff.GS_Code;
			declaration.JE_DeclarantType = EMCSEntryTypeList.Codes.Consignor;
			Factory.Save();

			using (EmcsCustomsDataRegistry.Instance.EmcsSendConsignorAcknowledgements.SetTemporaryValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, emailGroup))
			using (Factory.AddDisposableService())
			{
				processor.PreProcessMessage(incomingMessage);
				processor.ProcessMessage(incomingMessage);

				CombineAssertions("Result for processing REJECTED ERROR message.", () =>
				{
					AssertEquals("declaration.JE_MessageStatus", EDIMessage.Status.Failed, declaration.JE_MessageStatus);
					MessageProcessorNotificationTestHelper.AssertEmail(
						expectingSubject: $@"815 - Submit draft e-AD Response (Failure) for " + declaration.JE_DeclarationReference,
						expectingBodyTexts: new[] {
							"Submission has been rejected. Transaction ID Status: Error - Error",
							$@"<tr><td>Transaction ID</td><td>{TransactionId}</td></tr>",
							$@"<tr><td>Message Type</td><td>815 - Submit draft e-AD</td></tr>",
							$@"<tr><td>Message Number</td><td>{outgoingMessage.EM_MessageNum}</td></tr>",
							"Declaration message status has been set FAL."
						},
						expectingRecipients: expectingRecipientsFromRegistrySetting);
					AssertEquals("outgoingMessage.EM_Status", EDIMessage.Status.Sent, outgoingMessage.EM_Status);
				});
			}
		}

		public void TestMessageAcknowledgement_Rejected_Error_EMCS_EmailConsignee()
		{
			var emailGroup = EmcsCustomsDataRegistry.Instance.EmcsSendConsigneeAcknowledgements.GetFallBackValueAtAllLevels(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty);
			emailGroup.SendMode = Core.Constants.EmailTo.NominatedGroup;
			emailGroup.SendGroupPK = MessageProcessorNotificationTestHelper.SetupStaffGroup(Factory).PK;

			var incomingMessage = CreateIncomingMessageAcknowledgement();
			var staff = MessageProcessorNotificationTestHelper.SetupStaffData(Factory);
			var (declaration, outgoingMessage, processor) = CreateOriginalDataForEMCS(Factory);
			outgoingMessage.EM_SystemCreateUser = staff.GS_Code;
			declaration.JE_DeclarantType = EMCSEntryTypeList.Codes.Consignee;
			Factory.Save();

			using (EmcsCustomsDataRegistry.Instance.EmcsSendConsigneeAcknowledgements.SetTemporaryValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, emailGroup))
			using (Factory.AddDisposableService())
			{
				processor.PreProcessMessage(incomingMessage);
				processor.ProcessMessage(incomingMessage);

				CombineAssertions("Result for processing REJECTED ERROR message.", () =>
				{
					AssertEquals("declaration.JE_MessageStatus", EDIMessage.Status.Failed, declaration.JE_MessageStatus);
					MessageProcessorNotificationTestHelper.AssertEmail(
						expectingSubject: $@"815 - Submit draft e-AD Response (Failure) for " + declaration.JE_DeclarationReference,
						expectingBodyTexts: new[] {
							"Submission has been rejected. Transaction ID Status: Error - Error",
							$@"<tr><td>Transaction ID</td><td>{TransactionId}</td></tr>",
							$@"<tr><td>Message Type</td><td>815 - Submit draft e-AD</td></tr>",
							$@"<tr><td>Message Number</td><td>{outgoingMessage.EM_MessageNum}</td></tr>",
							"Declaration message status has been set FAL."
						},
						expectingRecipients: expectingRecipientsFromRegistrySetting);
					AssertEquals("outgoingMessage.EM_Status", EDIMessage.Status.Sent, outgoingMessage.EM_Status);
				});
			}
		}

		public void TestMessageAcknowledgement_Rejected_ROSError_EMCS()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var eun = helper.CreateNewOrGetExistingDataGrouping(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, "European Union");
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Ireland, parent: eun);
			helper.CreateNewOrGetExistingCusCodeType(Constants.RefCusCodeListTypes.RevenueErrorType, "IEROS");
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Ireland, Constants.RefCusCodeListTypes.RevenueErrorType, "CODE", "DESCRIPTION", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);

			var staff = MessageProcessorNotificationTestHelper.SetupStaffData(Factory);
			var (declaration, outgoingMessage, processor) = CreateOriginalDataForEMCS(Factory);
			outgoingMessage.EM_SystemCreateUser = staff.GS_Code;
			var incomingMessage = InterchangeProcessorTestHelper.CreateMessageAcknowledgementServiceErrorMessage<EMCSInboundEDIMessage>(factory: Factory,
				applicationCode: EDIMessage.ApplicationCodes.IECustomsEMCS,
				messageType: "815",
				errorCode: "CODE"
			);
			incomingMessage.EM_LinkedObject = outgoingMessage.EM_LinkedObject;
			Factory.Save();

			using (Factory.AddDisposableService())
			{
				processor.PreProcessMessage(incomingMessage);
				processor.ProcessMessage(incomingMessage);

				CombineAssertions("Result for processing ROS ERROR message.", () =>
				{
					AssertEquals("declaration.JE_MessageStatus", EDIMessage.Status.Failed, declaration.JE_MessageStatus);
					MessageProcessorNotificationTestHelper.AssertEmail(
						expectingSubject: $@"815 - Submit draft e-AD Response (Failure) for " + declaration.JE_DeclarationReference,
						expectingBodyTexts: new[] {
							"Error submitting message. Error Code: CODE - DESCRIPTION",
							$@"<tr><td>Message Number</td><td>{outgoingMessage.EM_MessageNum}</td></tr>",
						},
						expectingRecipients: new string[] { staff.GS_EmailAddress });
					AssertEquals("outgoingMessage.EM_Status", EDIMessage.Status.Sent, outgoingMessage.EM_Status);
				});
			}
		}

		public void TestMessageAcknowledgement_Rejected_Invalid_EMCS()
		{
			var incomingMessage = InterchangeProcessorTestHelper.CreateMessageAcknowledgementMessage<EMCSInboundEDIMessage>(factory: Factory,
				applicationCode: EDIMessage.ApplicationCodes.IECustomsEMCS,
				messageType: "815",
				transactionId: TransactionId,
				messageStatus: MessageStatus.Rejected,
				transactionIdStatus: TransactionIdStatus.Invalid,
				errorCode: "IV1"
			);

			using (Factory.AddDisposableService())
			{
				var (ieCompany, ieBranch) = InterchangeProcessorTestHelper.CreateCompanyAndBranch(Factory, Core.Constants.CountryCodes.Ireland);
				var (declaration, outgoingMessage, processor) = CreateOriginalDataForEMCS(Factory);
				outgoingMessage.EM_GB = ieBranch.PK;

				processor.PreProcessMessage(incomingMessage);
				processor.ProcessMessage(incomingMessage);
				var newReSubmission = Factory.Load<EDIMessage>(
					new ZQuery(EDIMessageSchema.EM_ReceiveTransmit, EDIMessage.Direction.Transmit)
						.AddToFilter(EDIMessageSchema.EM_MessageNum, SQLComparisonOperator.StartsWith, outgoingMessage.EM_MessageNum + "_")
				).Single();

				CombineAssertions("Properties of re-submission.", () =>
				{
					AssertEquals("EM_ApplicationCode", EDIMessage.ApplicationCodes.IECustomsEMCS, newReSubmission.EM_ApplicationCode);
					AssertEquals("EM_GB", ieBranch.PK, newReSubmission.EM_GB);
					AssertEquals("EM_MessageType", "815", newReSubmission.EM_MessageType);
					AssertEquals("EM_ReceiveTransmit", EDIMessage.Direction.Transmit, newReSubmission.EM_ReceiveTransmit);
					AssertSame("EM_LinkedObject", declaration, newReSubmission.EM_LinkedObject);

					AssertEquals("EM_ApplicationReference should be set empty try to trigger another TransactionID allocation.", string.Empty, newReSubmission.EM_ApplicationReference);
					AssertEquals("EM_MessageNum should be original num + _ + resubmission count.", outgoingMessage.EM_MessageNum + "_1", newReSubmission.EM_MessageNum);
					AssertEquals("Should not create eMail for failed for the first time.", 0, Env.OutgoingCustomsMailManager.EmailsCreated.Count);
					AssertEquals("outgoingMessage.EM_Status", EDIMessage.Status.Sent, outgoingMessage.EM_Status);
				});
			}
		}

		public void TestMessageAcknowledgement_Rejected_Invalid_Existing3Resubmissions_EMCS()
		{
			var staff = MessageProcessorNotificationTestHelper.SetupStaffData(Factory);
			var (ieCompany, ieBranch) = InterchangeProcessorTestHelper.CreateCompanyAndBranch(Factory, Core.Constants.CountryCodes.Ireland);
			var (declaration, outgoingMessage, processor) = CreateOriginalDataForEMCS(Factory);
			outgoingMessage.EM_GB = ieBranch.PK;
			outgoingMessage.EM_SystemCreateUser = staff.GS_Code;
			var existingResubmission1 = (EDIMessage)outgoingMessage.Clone();
			existingResubmission1.EM_ApplicationReference = TransactionId + "_1";
			var existingResubmission2 = (EDIMessage)outgoingMessage.Clone();
			existingResubmission2.EM_ApplicationReference = TransactionId + "_2";
			var existingResubmission3 = (EDIMessage)outgoingMessage.Clone();
			existingResubmission3.EM_ApplicationReference = TransactionId + "_3";

			Factory.Save();

			var incomingMessage = InterchangeProcessorTestHelper.CreateMessageAcknowledgementMessage<EMCSInboundEDIMessage>(factory: Factory,
				applicationCode: EDIMessage.ApplicationCodes.IECustomsEMCS,
				messageType: "815",
				transactionId: TransactionId,
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

				CombineAssertions("Expectings: Not more re-submission created; Declaration message set REJ; Notification eMail created.", () =>
				{
					AssertEquals("No re-submission", 0, newReSubmissions.Length);
					AssertEquals("Declaration message status", EDIMessage.Status.Failed, declaration.JE_MessageStatus);
					MessageProcessorNotificationTestHelper.AssertEmail(
						expectingSubject: $@"815 - Submit draft e-AD Response (Failure) for " + declaration.JE_DeclarationReference,
						expectingBodyTexts: new[]
						{
						$@"Submission has been rejected. Transaction ID Status: Invalid - Invalid",
						$@"<tr><td>Transaction ID</td><td>{TransactionId}</td></tr>",
						$@"<tr><td>Message Type</td><td>815 - Submit draft e-AD</td></tr>",
						$@"<tr><td>Message Number</td><td>{outgoingMessage.EM_MessageNum}</td></tr>",
						"Declaration message status has been set FAL.",
						},
						expectingRecipients: new string[] { staff.GS_EmailAddress }
					);
					AssertEquals("outgoingMessage.EM_Status", EDIMessage.Status.Sent, outgoingMessage.EM_Status);
				});
			}
		}

		public void TestMessageAcknowledgement_Rejected_Expired_EMCS()
		{
			var (ieCompany, ieBranch) = InterchangeProcessorTestHelper.CreateCompanyAndBranch(Factory, Core.Constants.CountryCodes.Ireland);
			var (declaration, outgoingMessage, processor) = CreateOriginalDataForEMCS(Factory);
			outgoingMessage.EM_GB = ieBranch.PK;
			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var incomingMessage = InterchangeProcessorTestHelper.CreateMessageAcknowledgementMessage<EMCSInboundEDIMessage>(factory: newFactory,
				applicationCode: EDIMessage.ApplicationCodes.IECustomsEMCS,
				messageType: "815",
				transactionId: TransactionId,
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
					AssertEquals("EM_ApplicationCode", EDIMessage.ApplicationCodes.IECustomsEMCS, newReSubmission.EM_ApplicationCode);
					AssertEquals("EM_GB", ieBranch.PK, newReSubmission.EM_GB);
					AssertEquals("EM_MessageType", "815", newReSubmission.EM_MessageType);
					AssertEquals("EM_ReceiveTransmit", EDIMessage.Direction.Transmit, newReSubmission.EM_ReceiveTransmit);
					AssertEquals("EM_LinkUniqueID", declaration.PK, newReSubmission.EM_LinkUniqueID);

					AssertEquals("EM_ApplicationReference should be set empty try to trigger another TransactionID allocation.", string.Empty, newReSubmission.EM_ApplicationReference);
					AssertEquals("EM_MessageNum should be original num + _ + resubmission count.", outgoingMessage.EM_MessageNum + "_1", newReSubmission.EM_MessageNum);
					AssertEquals("Should not create eMail for failed for the first time.", 0, Env.OutgoingCustomsMailManager.EmailsCreated.Count);
					AssertEquals("outgoingMessage.EM_Status", EDIMessage.Status.Sent, outgoingMessage.EM_Status);
				});
			}
		}

		public void TestInterpretation()
		{
			var (declaration, outgoingMessage, processor) = CreateOriginalDataForEMCS(Factory);
			var incomingMessage = InterchangeProcessorTestHelper.CreateMessageAcknowledgementMessage<EMCSInboundEDIMessage>(factory: Factory,
				applicationCode: EDIMessage.ApplicationCodes.IECustomsEMCS,
				messageType: "815",
				transactionId: TransactionId,
				messageStatus: MessageStatus.Accepted,
				transactionIdStatus: TransactionIdStatus.Accepted
			);

			using (ObjectFactory.Substitute(Mock.Of<INudgingController>()))
			using (Factory.AddDisposableService())
			{
				processor.PreProcessMessage(incomingMessage);
				processor.ProcessMessage(incomingMessage);

				var expectedInterpretation = "Message Acknowledgement received.<br />\r\n<br />Message Status: ACCEPTED<br />\r\n<br />Transaction ID Status: ACCEPTED";
				AssertEquals("Interpretation should be set", expectedInterpretation, incomingMessage.EM_MessageInterpretation);
			}

			incomingMessage = InterchangeProcessorTestHelper.CreateMessageAcknowledgementMessage<EMCSInboundEDIMessage>(factory: Factory,
				applicationCode: EDIMessage.ApplicationCodes.IECustomsEMCS,
				messageType: "815",
				transactionId: TransactionId,
				messageStatus: MessageStatus.Rejected,
				transactionIdStatus: TransactionIdStatus.Invalid,
				errorCode: "IV4"
			);

			using (Factory.AddDisposableService())
			{
				processor.PreProcessMessage(incomingMessage);
				processor.ProcessMessage(incomingMessage);

				var expectedInterpretation = "Message Acknowledgement received.<br />\r\n<br />Message Status: REJECTED<br />\r\n<br />Transaction ID Status: INVALID";
				AssertEquals("Interpretation should be set", expectedInterpretation, incomingMessage.EM_MessageInterpretation);
			}
		}

		internal static (EMCSJobDeclaration declaration, EMCSOutboundEDIMessage outgoingMessage, MessageAcknowledgementProcessor processor) CreateOriginalDataForEMCS(BusinessObjectFactory factory, LoggingInformation logger = null)
		{
			var declaration = factory.New<EMCSJobDeclaration>();
			declaration.JE_DeclarationReference = "E012345678";
			var outgoingMessage = factory.New<EMCSOutboundEDIMessage>();
			outgoingMessage.EM_ApplicationCode = EDIMessage.ApplicationCodes.IECustomsEMCS;
			outgoingMessage.EM_MessageType = EMCSOutgoingMessageTypeList.Codes.SubmitDraftEAD;
			outgoingMessage.EM_ApplicationReference = TransactionId;
			outgoingMessage.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			outgoingMessage.EM_LinkedObject = declaration;
			outgoingMessage.EM_Status = EDIMessage.Status.Sent;
			return (declaration, outgoingMessage, new MessageAcknowledgementProcessor(logger ?? new LoggingInformation()));
		}

		EMCSInboundEDIMessage CreateIncomingMessageAcknowledgement() => InterchangeProcessorTestHelper.CreateMessageAcknowledgementMessage<EMCSInboundEDIMessage>(factory: Factory,
			applicationCode: EDIMessage.ApplicationCodes.IECustomsEMCS,
			messageType: "815",
			transactionId: TransactionId,
			messageStatus: MessageStatus.Rejected,
			transactionIdStatus: TransactionIdStatus.Error,
			errorCode: "ER1"
		);

		const string TransactionId = "TRS0000001";
		static readonly string[] expectingRecipientsFromRegistrySetting = new string[] { "staff3@where.com", "staff4@where.com", "staff5@where.com" };
	}
}
