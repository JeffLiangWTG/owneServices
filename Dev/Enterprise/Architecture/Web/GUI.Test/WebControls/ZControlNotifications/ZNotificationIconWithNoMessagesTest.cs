using System.IO;
using System.Web.UI;
using Enterprise.ZArchitecture.Web.GUI.Testing;

namespace Enterprise.ZArchitecture.Web.GUI.WebControls.Testing
{
	sealed class ZNotificationIconWithNoMessagesTest : WebControlTest
	{
		public void TestNoTooltipWhenNoNotificationMessages()
		{
			var sw = new StringWriter();
			NotificationIcon.RenderControlForTest(sw);

			AssertEquals("Title attribute is NOT added to img", @"<img src=""/myuri"" alt=""Test"" />", sw.ToString());
		}

		protected override Control GetNewControl() => new DummyNotificationIcon(new[] { new DummyNotification() });

		DummyNotificationIcon NotificationIcon => Control as DummyNotificationIcon;
	}
}
