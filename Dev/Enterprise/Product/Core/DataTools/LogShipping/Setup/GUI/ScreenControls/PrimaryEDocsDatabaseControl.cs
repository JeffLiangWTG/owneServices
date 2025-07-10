using System.Windows.Forms;

namespace Enterprise.LogShipping.Setup.GUI
{
	public partial class PrimaryEDocsDatabaseControl : UserAreaControl
	{
		public PrimaryEDocsDatabaseControl()
		{
			InitializeComponent();
			eDocsDatabasesCheckBox.Checked = true;
		}

		public override object Value
		{
			get { return eDocsDatabasesCheckBox.Checked; }
		}

		#region Controls to Enable/Disable

		Control[] controlsToEnableDisable;
		protected override Control[] ControlsToEnableDisable
		{
			get
			{
				return controlsToEnableDisable = controlsToEnableDisable ?? new Control[] { eDocsDatabasesCheckBox };
			}
		}

		#endregion
	}
}
