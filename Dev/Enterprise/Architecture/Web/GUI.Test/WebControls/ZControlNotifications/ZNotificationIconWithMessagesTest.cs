using System.IO;
using System.Web.UI;
using Enterprise.ZArchitecture.Web.GUI.Testing;

namespace Enterprise.ZArchitecture.Web.GUI.WebControls.Testing
{
	sealed class ZNotificationIconWithMessagesTest : WebControlTest
	{
		public void TestTooltipWhenHasNotificationMessages()
		{
			var sw = new StringWriter();
			NotificationIcon.RenderControlForTest(sw);

			AssertEquals("Title attribute is added to img", @"<img src=""/myuri"" alt=""Test"" title=""some error message"" />", sw.ToString());
		}

		protected override Control GetNewControl() => new DummyNotificationIcon(new[] { new DummyNotificationWithMessage() });

		DummyNotificationIcon NotificationIcon => Control as DummyNotificationIcon;
	}
}
