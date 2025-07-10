using System;
using Enterprise.Customs.IE.Business.Declaration;

namespace Enterprise.Customs.IE.Business
{
	public class RefundApplicationMessageSendingActionParent : CusEntryHeaderMessageSendingActionParent<RefundApplicationMessageSendingAction>
	{
		public RefundApplicationMessageSendingActionParent(JobDeclaration declaration) : base(declaration) { }

		protected override Type SendingObjectCollectionType => typeof(RefundApplicationMessageSendingActionCollection);
	}
}
