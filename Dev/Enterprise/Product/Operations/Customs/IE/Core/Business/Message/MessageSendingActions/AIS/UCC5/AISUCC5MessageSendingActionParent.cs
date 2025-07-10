using System;
using Enterprise.Customs.IE.Business.Declaration;

namespace Enterprise.Customs.IE.Business
{
	public class AISUCC5MessageSendingActionParent : CusEntryHeaderMessageSendingActionParent<AISUCC5MessageSendingAction>
	{
		public AISUCC5MessageSendingActionParent(JobDeclaration declaration) : base(declaration) { }

		protected override Type SendingObjectCollectionType => typeof(AISUCC5MessageSendingActionCollection);
	}
}
