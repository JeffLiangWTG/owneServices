using System.Windows.Forms;

namespace Enterprise.ZArchitecture.GUI.Testing
{
	sealed class ZGroupBoxVisibleForNotificationsTestCase : SetVisibleOnControlUpdatesNotificationsTestCase
	{
		protected override Control GetNewControl()
		{
			return new ZGroupBox();
		}
	}
}
