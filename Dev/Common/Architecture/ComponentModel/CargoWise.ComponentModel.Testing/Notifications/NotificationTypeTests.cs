#if DEBUG
using System.Collections.Generic;
using NUnit.Framework;

namespace CargoWise.ComponentModel.Testing
{
	public class NotificationTypeTests : TestCase
	{
		public void TestGetFatalNotifications()
		{
			NotificationCollection notifications = new NotificationCollection();
			notifications.AddError("error");
			notifications.AddWarning("warning");

			List<INotification> list = new List<INotification>(notifications.GetFatalNotifications());
			AssertEquals("Only fatal notifications included", 1, list.Count);
			AssertEquals("Only fatal notifications included", "error", list[0].Message);
		}
	}
}
#endif
