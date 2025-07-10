using System.Windows.Forms;
using Microsoft.Win32;

namespace CargoWise.Windows.UI.Testing
{
	sealed class KDropDownForTest : ToolStripDropDown
	{
		public KDropDownForTest() : base()
		{
			SystemEvents.UserPreferenceChanged += new UserPreferenceChangedEventHandler(this.OnUserPreferenceChanged);
		}

		void OnUserPreferenceChanged(object sender, UserPreferenceChangedEventArgs e) { }
	}
}
