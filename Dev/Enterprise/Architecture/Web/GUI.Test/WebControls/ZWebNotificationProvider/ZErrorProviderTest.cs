namespace Enterprise.ZArchitecture.Web.GUI.WebControls.Testing
{
	sealed class ZErrorProviderTest : ZWebNotificationProviderTest
	{
		protected override ZWebNotificationProvider GetNewNotificationProvider()
		{
			return new ZErrorProvider(TestControl, TestNotifications);
		}
	}
}
