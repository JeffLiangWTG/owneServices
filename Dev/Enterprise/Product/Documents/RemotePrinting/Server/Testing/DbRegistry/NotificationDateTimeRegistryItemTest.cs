using System;
using CargoWise.EntityFramework.Testing.DataAccess;

namespace Enterprise.RemotePrinting.Server.Testing
{
	public class NotificationDateTimeRegistryItemTest : TransactionCoordinatorTestCase
	{
		public void TestDefaultValue()
		{
			var value = new ClientUpdateErrorNotifier.NotificationDateTimeRegistryItem("M1").GetLastNotificationDateTimeUtc(TestConnection);
			AssertEquals(DateTime.MinValue, value);
		}

		public void TestSetAndGet()
		{
			new ClientUpdateErrorNotifier.NotificationDateTimeRegistryItem("M1").SetLastNotificationDateTimeUtc(TestConnection);
			var value = new ClientUpdateErrorNotifier.NotificationDateTimeRegistryItem("M1").GetLastNotificationDateTimeUtc(TestConnection);
			Assert("Stored value should be close to now", (DateTime.UtcNow - value).TotalSeconds < 10);

			var otherValue = new ClientUpdateErrorNotifier.NotificationDateTimeRegistryItem("M2").GetLastNotificationDateTimeUtc(TestConnection);
			AssertEquals("One registry should not affect other", DateTime.MinValue, otherValue);
		}
	}
}
