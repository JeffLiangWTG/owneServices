namespace Enterprise.Customs.IE.Business
{
	public class RefundApplicationMessageSendingActionCollection : CusEntryHeaderMessageSendingActionCollection<RefundApplicationMessageSendingAction>
	{
		public RefundApplicationMessageSendingActionCollection(RefundApplicationMessageSendingActionParent sendingActionParent) : base(sendingActionParent) { }

		protected new RefundApplicationMessageSendingActionParent Parent => (RefundApplicationMessageSendingActionParent)base.Parent;
	}
}
