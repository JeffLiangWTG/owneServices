namespace Enterprise.ZArchitecture.Web.GUI.WebControls.Testing
{
	sealed class ZErrorMessageProviderTest : ZWebNotificationProviderTest
	{
		protected override ZWebNotificationProvider GetNewNotificationProvider()
		{
			return new ZMessageErrorProvider(TestControl, TestNotifications);
		}
	}
}
