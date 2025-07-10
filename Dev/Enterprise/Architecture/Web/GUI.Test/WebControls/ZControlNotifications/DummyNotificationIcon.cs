using System.Collections.Generic;
using System.IO;
using System.Web.UI;
using CargoWise.ComponentModel;

namespace Enterprise.ZArchitecture.Web.GUI.WebControls.Testing
{
	class DummyNotificationIcon : ZNotificationIcon
	{
		public DummyNotificationIcon(IEnumerable<INotification> notifications)
		: base(notifications)
		{
		}

		public void RenderControlForTest(StringWriter sw)
		{
			using (var writer = new HtmlTextWriter(sw))
			{
				this.RenderControl(writer);
			}
		}

		public override string AlternateText => "Test";

		protected override string GetResourceName() => "TestIcon.ico";

		protected override string GetImageURL() => "/myuri";

		protected override IEnumerable<INotification> GetNotificationMessages() => Notifications;
	}
}
