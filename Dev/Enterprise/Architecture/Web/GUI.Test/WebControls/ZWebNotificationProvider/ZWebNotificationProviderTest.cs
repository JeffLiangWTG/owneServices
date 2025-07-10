using System.Web.UI;
using CargoWise.ComponentModel;
using Enterprise.ZArchitecture.Web.GUI.Testing;

namespace Enterprise.ZArchitecture.Web.GUI.WebControls.Testing
{
	public abstract class ZWebNotificationProviderTest : WebControlTest
	{
		protected ZWebNotificationProvider TestProvider
		{
			get
			{
				if (fTestProvider == null)
				{
					fTestProvider = GetNewNotificationProvider();
				}
				return fTestProvider;
			}
		}
		ZWebNotificationProvider fTestProvider;

		protected Control TestControl
		{
			get
			{
				if (fTestControl == null)
				{
					fTestControl = CreateNewControl();
				}
				return fTestControl;
			}
		}
		Control fTestControl;

		protected Control CreateNewControl()
		{
			ZTextBox result = new ZTextBox();
			Page.Controls.Add(result);
			return result;
		}

		protected NotificationCollection TestNotifications
		{
			get
			{
				if (fNotifications == null)
				{
					fNotifications = GetNewNotifications();
				}
				return fNotifications;
			}
		}
		NotificationCollection fNotifications;

		protected NotificationCollection GetNewNotifications()
		{
			return new NotificationCollection();
		}

		protected abstract ZWebNotificationProvider GetNewNotificationProvider();

		protected override Control GetNewControl()
		{
			return GetNewNotificationProvider();
		}
	}
}
