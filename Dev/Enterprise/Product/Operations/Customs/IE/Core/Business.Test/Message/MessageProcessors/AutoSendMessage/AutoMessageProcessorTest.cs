using Enterprise.Customs.Business;
using Enterprise.Customs.Common.EU;

namespace Enterprise.Customs.IE.Business.Testing
{
	abstract class AutoMessageProcessorTest : Customs.Business.Testing.AutoSendCustomsMessageProcessorTest
	{
		protected override void AssertEntryAndMessageResultForEndToEndTest(CusEntryHeader entry)
		{
			AssertEquals("Message should be generated", 1, entry.Messages.Count);
			AssertEquals(LogicalStatusList.Codes.Sent, entry.CH_Status);
			var originalMessage = entry.Messages[0];
			AssertEquals(ExpectedMessageType, originalMessage.EM_MessageType);
			AssertEquals(EDIMessage.Status.Queued, originalMessage.EM_Status);
			AssertEquals(EDIMessage.Direction.Transmit, originalMessage.EM_ReceiveTransmit);
		}

		protected abstract string ExpectedMessageType { get; }

		protected override CusEntryHeader GetEntryHeader(BaseJobDeclaration declaration) => declaration.ActiveEntryHeaders[0];

		protected override bool MessageCanBeAutoSentForLodgedEntries => false;

		protected override void TriggerNonEmptyMessageTypeValidation(BaseJobDeclaration declaration)
		{
			base.TriggerNonEmptyMessageTypeValidation(declaration);
			declaration.JE_ApplicationCode = "BLT";
		}

		protected override string ExpectedLastNotification => "There is no entry found to send";

		protected override void SetEntryClearedStatus(CusEntryHeader entry)
		{
		}
	}
}
