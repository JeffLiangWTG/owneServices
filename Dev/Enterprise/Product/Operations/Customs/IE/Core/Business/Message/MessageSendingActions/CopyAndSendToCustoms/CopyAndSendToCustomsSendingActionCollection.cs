using Enterprise.Customs.IE.Business.Declaration;

namespace Enterprise.Customs.IE.Business
{
	internal class CopyAndSendToCustomsSendingActionCollection : CusEntryHeaderMessageSendingActionCollection<CopyAndSendToCustomsSendingAction>
	{
		public CopyAndSendToCustomsSendingActionCollection(CopyAndSendToCustomsSendingActionParent sendingActionParent) : base(sendingActionParent)
		{
		}

		protected override CopyAndSendToCustomsSendingAction CreateElementCore(CusEntryHeader entryHeader)
		{
			var sendingAction = base.CreateElementCore(entryHeader);
			sendingAction.MessageType = ((CopyAndSendToCustomsSendingActionParent)Parent).MessageType;
			return sendingAction;
		}
	}
}
