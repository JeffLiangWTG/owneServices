using System;
using System.Collections.Generic;
using CargoWise.ComponentModel;
using Moq;
using NUnit.Framework;

namespace Enterprise.Scheduler.GraphEngine.Test
{
	public class NotifierGrengineTrackerTest : TestCase
	{
		public void TestMethodsWithNonEmptyList()
		{
			int notificationsCount = 0;
			var mockNotifications = new Mock<INotifications>();
			mockNotifications.Setup(o => o.Add(It.IsAny<INotification>())).Callback(() => notificationsCount++);
			var x = new NotifierGrengineTracker<int, string>();
			x.WithNotifications(mockNotifications.Object);
			var entityList = new List<string> { "Apple", "Banana", "Cherry" };
			x.Loaded(entityList);
			AssertEquals(1, notificationsCount);
			x.VetexAdded(entityList);
			AssertEquals(2, notificationsCount);
			x.VetexRemoved(entityList, 0);
			AssertEquals(3, notificationsCount);
		}

		public void TestMethodsWithEmptyList()
		{
			var mockNotifications = new Mock<INotifications>();
			mockNotifications.Setup(y => y.Add(It.IsAny<INotification>())).Throws(new Exception());
			var x = new NotifierGrengineTracker<int, string>();
			x.WithNotifications(mockNotifications.Object);
			AssertNoExceptionThrown(() => x.Loaded(new List<string>()));
			AssertNoExceptionThrown(() => x.VetexAdded(new List<string>()));
			AssertNoExceptionThrown(() => x.VetexRemoved(new List<string>(), 0));
		}
	}
}


