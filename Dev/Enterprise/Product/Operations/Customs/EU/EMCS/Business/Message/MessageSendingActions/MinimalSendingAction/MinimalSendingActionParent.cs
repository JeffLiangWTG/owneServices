using System;

namespace Enterprise.Customs.EU.EMCS.Business
{
	public sealed class MinimalSendingActionParent : EMCSMessageSendingActionParent<EMCSMessageSendingAction>
	{
		public MinimalSendingActionParent(EMCSJobDeclaration declaration) : base(declaration) { }

		protected override Type SendingObjectCollectionType => typeof(MinimalSendingActionCollection);
	}
}
