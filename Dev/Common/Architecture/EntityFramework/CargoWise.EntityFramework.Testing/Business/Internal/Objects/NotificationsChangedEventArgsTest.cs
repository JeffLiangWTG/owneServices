namespace CargoWise.EntityFramework.Testing
{
	sealed class NotificationsChangedEventArgsTest : TestCaseWithDummy
	{
		public void TestConstruction()
		{
			AssertEquals(Dummy, new NotificationsChangedEventArgs(Dummy).SourceOfNotificationChange);
		}
	}
}
