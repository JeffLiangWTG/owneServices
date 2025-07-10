using System;
using CargoWise.ComponentModel;

namespace Enterprise.ZArchitecture.Business.Testing
{
	sealed class NotificationSubscriberForQueryUserTest : INotifications, INotificationSubscriberQueryUser
	{
		#region INotifications Members

		public void Add(INotification notification)
		{
			throw new NotSupportedException();
		}

		public IQueryUserEventArgs LastQueryUserEventArgs;
		public void QueryUser(IQueryUserEventArgs e)
		{
			LastQueryUserEventArgs = e;
		}

		#endregion
	}
}
