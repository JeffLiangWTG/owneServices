using System;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Security.ActiveDirectory.GUI
{
	public sealed partial class ADPasswordSettingsForm : ZChildForm
	{
		public ADPasswordSettingsForm()
		{
			InitializeComponent();

			manager = new ADPasswordSettingsManager();
			manager.LoadFromAD();
			SetDataBinding(manager.BizO, "");

			adPSOName.Visible = EnvProxy.Instance.CurrentUser.IsSupportUser;
		}

		readonly ADPasswordSettingsManager manager;

		public override void SetDataBinding(object dataSource, string dataMember)
		{
			base.SetDataBinding(dataSource, dataMember);

			if (BusinessEntity != null)
			{
				BusinessEntity.HasChangesChanged += BusinessEntity_HasChangesChanged;
				UpdateButtonStatus();
			}
		}

		void BusinessEntity_HasChangesChanged(object sender, HasChangesChangedEventArgs e)
		{
			UpdateButtonStatus();
		}

		void UpdateButtonStatus()
		{
			if (BusinessEntity != null)
			{
				saveButton.Enabled = BusinessEntity.HasChanges;
			}
		}

		public override string FormVerb => string.Empty;

		void SaveButton_Click(object sender, EventArgs e)
		{
			manager.BizO.RunPreSaveValidation();
			if (manager.BizO.HasErrors)
			{
				Globals.Message.ShowError(Res.GetString("A8322863-B7B8-488F-AA77-25C4F33B90D7", "Please correct the error(s) before continue."));
			}
			else if (Globals.Message.Show(
				Res.GetString("975408E7-CB0A-45B7-ADE6-6F4C6018A9BD", "These settings will be applied to Active Directory and take effect immediately. Do you want to proceed?"),
				Res.GetString("A99DFB1E-18A3-4EFC-9BBE-75427075E3D2", "Save all changes"),
				MessageBoxButtons.YesNo,
				DialogResult.No) == DialogResult.Yes)
			{
				if (manager.SaveToAD())
				{
					DialogResult = DialogResult.OK;
				}
			}
		}

		void CancelButton_Click(object sender, EventArgs e)
		{
			if (manager.BizO.HasChanges)
			{
				if (Globals.Message.Show(
									Res.GetString("06EA1688-12F9-468B-AFF2-2128623BF53B", "The settings have been modified. Are you sure you want to cancel?"),
									Res.GetString("A9E907E5-B298-4113-B759-A65CD74051AF", "Cancel"),
									MessageBoxButtons.YesNo,
									MessageBoxIcon.Warning,
									DialogResult.No) == DialogResult.Yes)
				{
					DialogResult = DialogResult.Cancel;
				}
			}
			else
			{
				DialogResult = DialogResult.Cancel;
			}
		}

		void OverrideDomainPasswordPolicy_CheckedChanged(object sender, EventArgs e)
		{
			if (!overrideDomainPasswordPolicy.Checked)
			{
				Globals.Message.ShowWarning(Res.GetString("7568D409-9F87-4BF2-AE05-4A2292FB3566", "Overridden password settings will be discarded and reverted back domain password policy."));
			}
		}
	}
}
