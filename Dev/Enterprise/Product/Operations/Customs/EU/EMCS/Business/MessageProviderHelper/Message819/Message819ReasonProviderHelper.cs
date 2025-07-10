namespace Enterprise.Customs.EU.EMCS.Business
{
	public class Message819ReasonProviderHelper
	{
		public Message819ReasonProviderHelper(IAlertOrRejectReason alertOrRejectReason)
		{
			this.alertOrRejectReason = alertOrRejectReason;
		}
		readonly IAlertOrRejectReason alertOrRejectReason;

		public string ReasonCode => alertOrRejectReason.Reason;
	}
}
