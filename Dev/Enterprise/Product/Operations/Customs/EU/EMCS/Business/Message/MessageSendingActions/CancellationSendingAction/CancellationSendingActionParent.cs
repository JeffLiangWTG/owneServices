using System;

namespace Enterprise.Customs.EU.EMCS.Business
{
	public sealed class CancellationSendingActionParent : EMCSMessageSendingActionParent<CancellationSendingAction>
	{
		public CancellationSendingActionParent(EMCSJobDeclaration declaration) : base(declaration)
		{
		}

		protected override Type SendingObjectCollectionType => typeof(CancellationSendingActionCollection);
	}
}
