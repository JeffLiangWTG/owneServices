using System;

namespace Enterprise.Customs.EU.EMCS.Business
{
	public sealed class ExplanationOnDelaySendingActionParent : EMCSMessageSendingActionParent<ExplanationOnDelaySendingAction>
	{
		public ExplanationOnDelaySendingActionParent(EMCSJobDeclaration declaration) : base(declaration)
		{
		}

		protected override Type SendingObjectCollectionType => typeof(ExplanationOnDelaySendingActionCollection);
	}
}
