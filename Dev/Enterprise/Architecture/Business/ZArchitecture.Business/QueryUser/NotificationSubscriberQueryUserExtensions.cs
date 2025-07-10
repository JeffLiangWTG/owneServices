using CargoWise.ComponentModel;

namespace Enterprise.ZArchitecture
{
	public static class NotificationSubscriberQueryUserExtensions
	{
		public static void QueryUser(this INotifications notifications, IQueryUserEventArgs e)
		{
			INotificationSubscriberQueryUser queryUser = notifications as INotificationSubscriberQueryUser;
			if (queryUser != null)
			{
				queryUser.QueryUser(e);
			}
		}
	}
}
