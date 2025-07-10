namespace Enterprise.Customs.KR.Business
{
	public class AgreedRateMessageSendingObjectValidation : JobDeclarationMessageSendingObjectValidation
	{
		public AgreedRateMessageSendingObjectValidation(AgreedRateMessageSendingObject parent) : base(parent)
		{
		}
		protected new AgreedRateMessageSendingObject Parent => base.Parent as AgreedRateMessageSendingObject;
	}
}
