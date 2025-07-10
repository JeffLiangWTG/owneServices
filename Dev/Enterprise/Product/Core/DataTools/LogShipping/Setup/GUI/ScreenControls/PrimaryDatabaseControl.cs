using System;
using System.Windows.Forms;

namespace Enterprise.LogShipping.Setup.GUI
{
	public partial class PrimaryDatabaseControl : UserAreaControl
	{
		public PrimaryDatabaseControl()
		{
			InitializeComponent();
		}

		public override object Value
		{
			get { return databasesComboBox.Text; }
		}

		#region Controls to Enable/Disable

		protected override Control[] ControlsToEnableDisable
		{
			get
			{
				return controlsToEnableDisable = controlsToEnableDisable ?? new Control[] { databasesComboBox };
			}
		}

		Control[] controlsToEnableDisable;

		#endregion

		void databasesComboBox_VisibleChanged(object sender, EventArgs e)
		{
			LogShippingSetupForm parentForm = this.ParentForm as LogShippingSetupForm;
			if (this.Visible && parentForm != null && parentForm.NextStep)
			{
				databasesComboBox.Items.Clear();
				databasesComboBox.Items.AddRange(parentForm.Navigator.PrimaryServerEnterpriseDatabases);
			}
		}
	}
}
