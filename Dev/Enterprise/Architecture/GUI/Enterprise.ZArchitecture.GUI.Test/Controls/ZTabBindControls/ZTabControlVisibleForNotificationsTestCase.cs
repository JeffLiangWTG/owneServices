using System.Windows.Forms;

namespace Enterprise.ZArchitecture.GUI.Testing
{
	sealed class ZTabControlVisibleForNotificationsTestCase : SetVisibleOnControlUpdatesNotificationsTestCase
	{
		protected override Control GetNewControl()
		{
			return new ZTabControl();
		}
	}
}
