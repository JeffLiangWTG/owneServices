using System;
using System.Drawing;
using System.Windows.Forms;

namespace Enterprise.AlwaysOn.Setup.GUI
{
	public partial class ChangeReplicaSettingControl : UserControl, IParentFormHook // Cannot use ODesignableForm because this is an external tool
	{
		public ChangeReplicaSettingControl()
		{
			InitializeComponent();
		}

		ReplicaControl replicaControl;

		IAlwaysOnReplica AvailabilityReplica
		{
			get
			{
				return replicaControl.AvailabilityReplica;
			}
		}

		#region IParentFormHook members

		public void HookUiControlForm(IUiControlForm uiControlForm)
		{
			this.uiControlForm = uiControlForm;
		}
		IUiControlForm uiControlForm;

		#endregion // IParentFormHook members

		#region Initialisation

		public void Reset(ReplicaControl replicaControl)
		{
			this.replicaControl = replicaControl;
			SetControlLocation();

			if (uiControlForm == null || AvailabilityReplica == null)
			{
				throw new InvalidOperationException("uiControlForm or AvailabilityReplica should not be null");
			}

			ReinitialiseReplicaOptionComboBoxes();
		}

		void SetControlLocation()
		{
			var location = Point.Add(replicaControl.OptionsControlLocation, new Size(-2, -5));
			Control control = replicaControl;

			while (control != null && !ReferenceEquals(control, Parent))
			{
				location = Point.Add(location, new Size(control.Location.X, control.Location.Y));
				control = control.Parent;
			}

			Location = location;
		}

		/// <summary>
		/// Only supports changing options for secondary replicas.
		/// Hence AllowConnection.READ_WRITE is not allowed here.
		/// </summary>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "const strings")]
		void ReinitialiseReplicaOptionComboBoxes()
		{
			commitModeComboBox.SelectedIndex = (int)AvailabilityReplica.Options.CommitMode;
			failoverModeComboBox.SelectedIndex = (int)AvailabilityReplica.Options.Failover;

			allowConnectionsComboBox.SelectedIndex = (int)AvailabilityReplica.Options.SecondaryAllowConnection - 1;
			uiControlForm.ShowMessage("Please select new replica settings and click on Apply to confirm.");
		}

		#endregion

		#region Apply Changes

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "const string")]
		void ConfirmIfHasChanges()
		{
			var newReplicaOptions = ReplicaOptions.NewFromTextOptions(
				commitModeComboBox.SelectedItem.ToString(),
				failoverModeComboBox.SelectedItem.ToString(),
				allowConnectionsComboBox.SelectedItem.ToString(),
				AvailabilityReplica.Options.SecondaryReadOnlyRoutingUrl);

			if (newReplicaOptions.CompareTo(AvailabilityReplica.Options) != 0)
			{
				if (ValidateReplicaOptions(newReplicaOptions))
				{
					if (replicaControl == null || AvailabilityReplica == null)
					{
						throw new InvalidOperationException("replicaControl or AvailabilityReplica should not be null");
					}

					uiControlForm.ShowMessage($"Changing replica [{AvailabilityReplica.ServerInfo.ServerAlias}] settings...");
					AvailabilityReplica.ParentGroup.ChangeReplicaSettings(AvailabilityReplica, newReplicaOptions);
					uiControlForm.AppendMessage("\r\n\r\nSuccessfully changed replica settings.");
					uiControlForm.OnActionConfirmed(ContextEnum.ChangeReplicaSettings, replicaControl);
				}
			}
			else
			{
				uiControlForm.OnActionCancelled();
			}

			replicaControl?.AvailabilityReplica?.RefreshHealthState();
		}

		bool ValidateReplicaOptions(IReplicaOptions replicaOptions)
		{
			if (!replicaOptions.Validate(out var validationError))
			{
				uiControlForm.ShowError(validationError);
				return false;
			}

			return true;
		}

		#endregion

		#region Events

		void confirmButton_Click(object sender, EventArgs e)
		{
			using var scope = Program.SqlContextManager.NewExecutionScope();

			ConfirmIfHasChanges();
		}

		void cancelButton_Click(object sender, EventArgs e)
		{
			uiControlForm.OnActionCancelled();
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "string")]
		void allowConnectionsComboBox_SelectedIndexChanged(object sender, EventArgs e)
		{
#pragma warning disable CW1046 // Do Not Specify Tooltips Manually Rule
			if (!string.Equals(allowConnectionsComboBox.Text, "All", StringComparison.OrdinalIgnoreCase))
			{
				allowConnectionsComboBox.Enabled = true;
				errorProvider.SetError(allowConnectionsComboBox, allowAllConnectionsToSecondaryReplica);
				toolTipService.SetToolTip(allowConnectionsComboBox, allowAllConnectionsToSecondaryReplica);
			}
			else
			{
				allowConnectionsComboBox.Enabled = false;
				errorProvider.SetError(allowConnectionsComboBox, string.Empty);
				toolTipService.SetToolTip(allowConnectionsComboBox, null);
			}
#pragma warning restore CW1046 // Do Not Specify Tooltips Manually Rule

		}
		#endregion

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "const strings")]
		const string allowAllConnectionsToSecondaryReplica = "Connections to secondary replica must be set to 'All'";
	}
}
