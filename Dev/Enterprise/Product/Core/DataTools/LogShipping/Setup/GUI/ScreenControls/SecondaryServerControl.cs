#define SuppressResourceStringsCheckRegion

using System;
using System.Windows.Forms;

namespace Enterprise.LogShipping.Setup.GUI
{
	public partial class SecondaryServerControl : UserAreaControl
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Literal string is safe to use in this context.")]
		public SecondaryServerControl()
		{
			InitializeComponent();
			this.titleLabel.Text = string.Format("Select instance of [{0}] secondary server or leave it blank for the default one:", Environment.MachineName);
		}

		public override object Value
		{
			get
			{
				object value = null;
				if (instancesComboBox.SelectedItem != null)
				{
					value = instancesComboBox.SelectedItem;
				}
				else
				{
					value = Environment.MachineName;
					if (!string.IsNullOrEmpty(instancesComboBox.Text))
					{
						value += "\\" + instancesComboBox.Text;
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
				return controlsToEnableDisable = controlsToEnableDisable ?? new Control[] { instancesComboBox };
			}
		}

		Control[] controlsToEnableDisable;

		#endregion

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Literal string is safe to use in this context.")]
		void instancesComboBox_DropDown(object sender, EventArgs e)
		{
			if (!instancesLoaded)
			{
				ShowMessage("Getting SQL Server instances...");
				this.Cursor = Cursors.WaitCursor;
				try
				{
					SqlServerInfo[] instances = SqlServerInfo.GetLocalSqlServerInstances();
					instancesComboBox.Items.AddRange(instances);
					instancesComboBox.Enabled = true;
					ShowMessage("SQL Server instances loaded successfully.");
					instancesLoaded = true;
				}
				catch (SqlServerInfoException ex)
				{
					ShowMessage(String.Format(CannotGetInstancesMessageRaw, ex.Message));
					instancesComboBox.Enabled = true;
					ChangeStatus(TaskStatus.Failure);
				}
				this.Cursor = Cursors.Default;
			}
		}

		bool instancesLoaded;
	}
}
