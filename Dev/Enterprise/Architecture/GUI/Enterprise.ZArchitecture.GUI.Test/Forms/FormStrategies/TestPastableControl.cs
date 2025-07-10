using System.Windows.Forms;
using CargoWise.Windows.UI;
using Enterprise.Core.Forms;

namespace Enterprise.ZArchitecture.GUI.Testing
{
	sealed class TestPastableControl : Control, IPastableControl
	{
		public TestPastableControl()
		{
			Controls.Add(new TextBox());
		}

		public bool WasPasted;

		public bool TryPaste()
		{
			return WasPasted = !this.GetReadOnly();
		}
	}
}
