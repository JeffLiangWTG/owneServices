namespace Enterprise.Customs.ES.ExitControl.Business
{
	public class ExitControlMessageSendingObject : EU.ExitControl.Business.ExitControlMessageSendingObject
	{
		public ExitControlMessageSendingObject(CusExitReport messagingObject) : base(messagingObject)
		{
		}
		public new CusExitReport MessagingObject => (CusExitReport)base.MessagingObject;

		protected override bool ShouldSend_ReadOnly => MessagingObject.IsSentOrNotEmpty;
	}
}
