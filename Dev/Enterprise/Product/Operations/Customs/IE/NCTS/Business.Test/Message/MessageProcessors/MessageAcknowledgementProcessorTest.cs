using System;
using CargoWise.Customs.IE.MessageDefinitions.Common.MessageAcknowledgement;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.BatchProcessor;
using Enterprise.Customs.EU.Registry;
using Enterprise.Customs.IE.Business;
using Enterprise.Customs.IE.Business.Testing;
using Enterprise.Customs.IE.Messaging.Testing;
using Enterprise.Customs.IE.NCTS.Messaging;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.IE.NCTS.Business.Testing
{
	sealed class MessageAcknowledgementProcessorTest : TestCaseWithFactory
	{
		public void TestMessageAcknowlegement_Rejected_Error()
		{
			var emailGroup = EUCustomsDataRegistry.Instance.SendNctsAcknowledgements.GetFallBackValueAtAllLevels(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty);
			emailGroup.SendMode = Core.Constants.EmailTo.NominatedGroup;
			emailGroup.SendGroupPK = MessageProcessorNotificationTestHelper.SetupStaffGroup(Factory).PK;

			var (nctsHeader, movementHeader, outgoingMessage, processor) = CreateOriginalData();

			var incomingMessage = InterchangeProcessorTestHelper.CreateMessageAcknowledgementMessage<NCTSInboundEDIMessage>(factory: Factory,
				applicationCode: EDIMessage.ApplicationCodes.IECustomsNCTS,
				messageType: NCTSOutgoingDepartureMessageTypeList.Codes.DeclarationData,
				transactionId: TransactionId,
				messageStatus: MessageStatus.Rejected,
				transactionIdStatus: TransactionIdStatus.Error,
				errorCode: "ER1"
			);
			Factory.Save();

			using (EUCustomsDataRegistry.Instance.SendNctsAcknowledgements.SetTemporaryValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, emailGroup))
			using (Factory.AddDisposableService())
			{
				processor.PreProcessMessage(incomingMessage);
				processor.ProcessMessage(incomingMessage);

				CombineAssertions("Result for processing REJECTED ERROR message.", () =>
				{
					var messageTypeDesc = outgoingMessage.MessageTypeWithDescription;
					MessageProcessorNotificationTestHelper.AssertEmail(
						expectingSubject: $"{messageTypeDesc} Response (Failure) for {nctsHeader.BH_JobReference}",
						expectingBodyTexts: new[] {
							"Submission has been rejected. Transaction ID Status: Error - Error",
							$"<tr><td>Transaction ID</td><td>{TransactionId}</td></tr>",
							$"<tr><td>Message Type</td><td>{messageTypeDesc}</td></tr>",
							$"<tr><td>Message Number</td><td>{outgoingMessage.EM_MessageNum}</td></tr>",
							"Entry status has been set ERR, Movement Reference Number: ."
						},
						expectingRecipients: expectingRecipients);
				});
			}
		}

		(NctsHeader declaration, NctsDepartureMovementHeader messageAttachee, NCTSOutboundEDIMessage outgoingMessage, MessageAcknowledgementProcessor processor) CreateOriginalData()
		{
			var staff = MessageProcessorNotificationTestHelper.SetupStaffData(Factory);
			var header = Factory.New<NctsHeader>();
			header.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Departure);
			header.BH_JobReference = "B00001000";
			header.BH_GB = GlbBranch.CurrentBranch.PK;
			var movementHeader = header.MovementHeader;

			var outgoingMessage = Factory.New<NCTSOutboundEDIMessage>();
			outgoingMessage.EM_ApplicationReference = TransactionId;
			outgoingMessage.EM_MessageType = NCTSOutgoingDepartureMessageTypeList.Codes.DeclarationData;
			outgoingMessage.EM_Status = EDIMessage.Status.Sent;
			outgoingMessage.EM_GB = GlbBranch.CurrentBranch.PK;
			outgoingMessage.EM_SystemCreateUser = staff.GS_Code;
			movementHeader.Messages.Add(outgoingMessage);
			return (header, movementHeader, outgoingMessage, new MessageAcknowledgementProcessor(new LoggingInformation()));
		}

		const string TransactionId = "IEN0000001";
		internal static readonly string[] expectingRecipients = new string[] { "staff3@where.com", "staff4@where.com", "staff5@where.com" };
	}
}
