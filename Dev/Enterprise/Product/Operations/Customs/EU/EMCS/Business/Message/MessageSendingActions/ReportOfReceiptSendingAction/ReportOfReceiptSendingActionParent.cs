using System;

namespace Enterprise.Customs.EU.EMCS.Business
{
	public class ReportOfReceiptSendingActionParent : EMCSMessageSendingActionParent<ReportOfReceiptSendingAction>
	{
		public ReportOfReceiptSendingActionParent(EMCSJobDeclaration declaration) : base(declaration)
		{
		}

		protected override Type SendingObjectCollectionType => typeof(ReportOfReceiptSendingActionCollection);
	}
}
