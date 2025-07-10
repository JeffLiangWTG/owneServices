using System.Windows.Forms;

namespace Enterprise.ZArchitecture.GUI.Testing
{
	sealed class ZPanelVisibleForNotificationsTestCase : SetVisibleOnControlUpdatesNotificationsTestCase
	{
		protected override Control GetNewControl()
		{
			return new ZPanel();
		}
	}
}
