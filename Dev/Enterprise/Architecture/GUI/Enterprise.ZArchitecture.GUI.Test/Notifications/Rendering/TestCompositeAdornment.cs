using System.Windows.Forms;

namespace Enterprise.ZArchitecture.GUI.Notifications.Testing
{
	sealed class TestCompositeAdornment : CompositeAdornment
	{
		readonly Control control;

		public TestCompositeAdornment(Control control)
		{
			this.control = control;
		}

		public override Control Control
		{
			get { return control; }
		}
	}
}
