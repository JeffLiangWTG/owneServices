using System;
using Enterprise.Customs.IE.Business.Declaration;
using Enterprise.Customs.IE.Business.Message.MessageSendingActions.CopyAndSendToCustoms;

namespace Enterprise.Customs.IE.Business
{
	public class CopyAndSendToCustomsSendingAction : CusEntryHeaderMessageSendingAction
	{
		public CopyAndSendToCustomsSendingAction(CusEntryHeader entryHeader) : base(entryHeader)
		{
		}
		protected override CusEntryHeaderMessageSendingActionValidation GetNewValidation() => new CopyAndSendToCustomsSendingActionValidation(this);

		protected override Type SenderType => typeof(CopyAndSendToCustomsSender);

		protected override CusEntryHeaderMessageSendingActionLookups GetNewLookups() => null;
	}
}
