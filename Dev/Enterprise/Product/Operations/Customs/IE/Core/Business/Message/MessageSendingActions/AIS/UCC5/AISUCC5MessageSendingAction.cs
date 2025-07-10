using System;
using Enterprise.Customs.IE.Business.Declaration;

namespace Enterprise.Customs.IE.Business
{
	public class AISUCC5MessageSendingAction : CusEntryHeaderMessageSendingAction
	{
		public AISUCC5MessageSendingAction(CusEntryHeader cusEntryHeader) : base(cusEntryHeader) { }

		protected override Type SenderType => typeof(AISUCC5MessageSender);

		protected override CusEntryHeaderMessageSendingActionLookups GetNewLookups()
		{
			return new AISUCC5MessageSendingActionLookups(this);
		}

		public new AISUCC5MessageSendingActionValidation Validation => (AISUCC5MessageSendingActionValidation)base.Validation;

		protected override CusEntryHeaderMessageSendingActionValidation GetNewValidation()
		{
			return new AISUCC5MessageSendingActionValidation(this);
		}
	}
}
