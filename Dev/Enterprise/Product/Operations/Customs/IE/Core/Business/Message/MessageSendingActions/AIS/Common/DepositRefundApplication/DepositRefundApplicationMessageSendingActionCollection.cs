namespace Enterprise.Customs.IE.Business
{
	public class DepositRefundApplicationMessageSendingActionCollection : CusEntryHeaderMessageSendingActionCollection<DepositRefundApplicationMessageSendingAction>
	{
		public DepositRefundApplicationMessageSendingActionCollection(DepositRefundApplicationMessageSendingActionParent sendingActionParent) : base(sendingActionParent) { }

		protected new DepositRefundApplicationMessageSendingActionParent Parent => (DepositRefundApplicationMessageSendingActionParent)base.Parent;
	}
}
