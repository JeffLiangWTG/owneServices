using System;
using System.Windows.Forms;
using CargoWise.Common;

namespace Enterprise.LogShipping.Setup.GUI.ScreenControls
{
	public partial class SecondaryDatabaseControl : UserAreaControl
	{
		public SecondaryDatabaseControl()
		{
			InitializeComponent();
		}

		public override object Value
		{
			get
			{
				return new object[] { action, secondaryDatabasesComboBox.SelectedItem };
			}
		}

		#region Controls to Enable/Disable

		protected override Control[] ControlsToEnableDisable
		{
			get
			{
				return controlsToEnableDisable = controlsToEnableDisable ??
					new Control[] { setupRadioButton, removeRadioButton, changeRadioButton, secondaryDatabasesComboBox };
			}
		}

		Control[] controlsToEnableDisable;

		#endregion

		void setupRadioButton_CheckedChanged(object sender, EventArgs e)
		{
			Argument.NotNull(sender, nameof(sender));

			if (((RadioButton)sender).Checked)
			{
				secondaryDatabasesComboBox.Enabled = false;
				action = SetupAction.Setup;
			}
		}

		void changeRadioButton_CheckedChanged(object sender, EventArgs e)
		{
			Argument.NotNull(sender, nameof(sender));

			if (((RadioButton)sender).Checked)
			{
				secondaryDatabasesComboBox.Enabled = true;
				action = SetupAction.Change;
			}
		}

		void removeRadioButton_CheckedChanged(object sender, EventArgs e)
		{
			Argument.NotNull(sender, nameof(sender));

			if (((RadioButton)sender).Checked)
			{
				secondaryDatabasesComboBox.Enabled = true;
				action = SetupAction.Remove;
			}
		}

		void SecondaryDatabaseControl_VisibleChanged(object sender, EventArgs e)
		{
			LogShippingSetupForm parentForm = this.ParentForm as LogShippingSetupForm;
			if (this.Visible)
			{
				if (parentForm != null && parentForm.NextStep)
				{
					secondaryDatabasesComboBox.Items.Clear();
					secondaryDatabasesComboBox.Items.AddRange(parentForm.Navigator.ExistingLogShippingConfigurations);
				}
			}
		}

		SetupAction action = SetupAction.Setup;
	}
}
