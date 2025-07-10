#define SuppressResourceStringsCheckRegion

using System;
using System.Windows.Forms;

namespace Enterprise.LogShipping.Setup.GUI
{
	public partial class PrimaryServerControl : UserAreaControl
	{
		public PrimaryServerControl()
		{
			InitializeComponent();
		}

		public override object Value
		{
			get
			{
				object value = null;
				if (instanceComboBox.SelectedItem != null)
				{
					value = instanceComboBox.SelectedItem;
				}
				else
				{
					value = serverNameTextBox.Text;
					if (!string.IsNullOrEmpty(instanceComboBox.Text))
					{
						value += "\\" + instanceComboBox.Text;
					}
				}
				return value;
			}
		}

		#region Controls to Enable/Disable

		protected override Control[] ControlsToEnableDisable
		{
			get
			{
				return controlsToEnableDisable = controlsToEnableDisable ??
					new Control[] { serverNameTextBox, instanceComboBox, refreshButton };
			}
		}

		Control[] controlsToEnableDisable;

		#endregion

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Literal string is safe to use in this context.")]
		void refreshButton_Click(object sender, EventArgs e)
		{
			this.Enabled = false;
			instanceComboBox.Enabled = false;
			ChangeStatus(TaskStatus.Setup);
			instanceComboBox.Items.Clear();
			if (!string.IsNullOrEmpty(serverNameTextBox.Text))
			{
				ShowMessage("Getting SQL Server instances...");
				this.Cursor = Cursors.WaitCursor;
				try
				{
					SqlServerInfo[] instances = SqlServerInfo.GetSqlServersInstances(serverNameTextBox.Text);
					instanceComboBox.Items.AddRange(instances);
					instanceComboBox.Enabled = true;
					ShowMessage("SQL Server instances loaded successfully.");
				}
				catch (SqlServerInfoException ex)
				{
					ShowMessage(String.Format(CannotGetInstancesMessageRaw, ex.Message));
					instanceComboBox.Enabled = true;
					ChangeStatus(TaskStatus.Failure);
				}
				this.Cursor = Cursors.Default;
			}
			else
			{
				ShowMessage("Error: Server name was not entered.");
				ChangeStatus(TaskStatus.Failure);
			}
			this.Enabled = true;
		}

		void serverNameTextBox_TextChanged(object sender, EventArgs e)
		{
			instanceComboBox.Items.Clear();
			instanceComboBox.Text = string.Empty;
		}
	}
}
