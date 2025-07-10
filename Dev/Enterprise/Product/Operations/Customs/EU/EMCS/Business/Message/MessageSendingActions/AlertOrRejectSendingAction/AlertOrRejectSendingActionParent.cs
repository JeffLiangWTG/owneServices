using System;

namespace Enterprise.Customs.EU.EMCS.Business
{
	public sealed class AlertOrRejectSendingActionParent : EMCSMessageSendingActionParent<AlertOrRejectSendingAction>
	{
		public AlertOrRejectSendingActionParent(EMCSJobDeclaration declaration) : base(declaration)
		{
		}

		protected override Type SendingObjectCollectionType => typeof(AlertOrRejectSendingActionCollection);
	}
}
