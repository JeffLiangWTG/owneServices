using CargoWise.Common;
using CargoWise.Customs.BR.MessageContracts.Subscription.Outgoing;

namespace Enterprise.Customs.BR.Business.Subscription
{
	public class NotificationSubscriptionProvider : INotificationSubscription
	{
		public NotificationSubscriptionProvider(SubscriptionMessageSendingObject sendingObject)
		{
			this.sendingObject = Argument.NotNull(sendingObject, nameof(sendingObject));
		}
		readonly SubscriptionMessageSendingObject sendingObject;

		public int ID => sendingObject.MessageType != SubscriptionMessageTypesList.Codes.ORI
									&& int.TryParse(sendingObject.Subscription.GP_MailBoxID, out var id) ? id : 0;

		public string Event => IsCanceledMessage ? null : EventIdList.GetCustomsEventValue(sendingObject.Subscription.GP_UserID);

		public string Endpoint => IsCanceledMessage ? null : BREDIInterchange.EndpointAddressPlaceHolder;

		bool IsCanceledMessage => sendingObject.MessageType == SubscriptionMessageTypesList.Codes.CAN;
	}
}
