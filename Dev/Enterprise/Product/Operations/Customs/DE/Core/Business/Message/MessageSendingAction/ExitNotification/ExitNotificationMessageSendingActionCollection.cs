using CargoWise.EntityFramework;

namespace Enterprise.Customs.DE.Business
{
	public sealed class ExitNotificationMessageSendingActionCollection : MessageSendingActionCollection
	{
		public ExitNotificationMessageSendingActionCollection(CusExitControlHeader exitHeader) : base(exitHeader.CusExitDetails, x => ((CusExitDetail)x).CED_MovementReferenceNumber, exitHeader.Factory)
		{
		}

		public new ExitNotificationMessageSendingAction AddNew() => (ExitNotificationMessageSendingAction)base.AddNew();

		public new ExitNotificationMessageSendingAction this[int index] => (ExitNotificationMessageSendingAction)base[index];

		public ExitNotificationMessageSendingAction AddNew(CusExitDetail exitDetail)
		{
			var result = (ExitNotificationMessageSendingAction)GetSendingAction(exitDetail);
			Add(result);
			return result;
		}

		protected override MessageSendingAction GetSendingAction(BusinessObject messagingObject) => new ExitNotificationMessageSendingAction((CusExitDetail)messagingObject);
	}
}
