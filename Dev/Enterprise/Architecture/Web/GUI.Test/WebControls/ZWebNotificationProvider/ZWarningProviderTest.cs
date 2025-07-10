namespace Enterprise.ZArchitecture.Web.GUI.WebControls.Testing
{
	sealed class ZWarningProviderTest : ZWebNotificationProviderTest
	{
		protected override ZWebNotificationProvider GetNewNotificationProvider()
		{
			return new ZWarningProvider(TestControl, TestNotifications);
		}
	}
}
