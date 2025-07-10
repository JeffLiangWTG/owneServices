using System;
using Enterprise.Customs.IE.Business.Declaration;

namespace Enterprise.Customs.IE.Business
{
	public class DepositRefundApplicationMessageSendingActionParent : CusEntryHeaderMessageSendingActionParent<DepositRefundApplicationMessageSendingAction>
	{
		public DepositRefundApplicationMessageSendingActionParent(JobDeclaration declaration) : base(declaration) { }

		protected override Type SendingObjectCollectionType => typeof(DepositRefundApplicationMessageSendingActionCollection);
	}
}
