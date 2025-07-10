using System;

namespace Enterprise.Customs.EU.EMCS.Business
{
	public sealed class ReasonForShortageSendingActionParent : EMCSMessageSendingActionParent<ReasonForShortageSendingAction>
	{
		public ReasonForShortageSendingActionParent(EMCSJobDeclaration declaration) : base(declaration)
		{
		}

		protected override Type SendingObjectCollectionType => typeof(ReasonForShortageSendingActionCollection);
	}
}
