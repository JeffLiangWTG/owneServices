using System;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Registry.GUI;

namespace Enterprise.Accounting.Registry.GUI
{
	public partial class BranchLevelPostingConfigurationControl : RegistryZUserControl
	{
		public BranchLevelPostingConfigurationControl()
		{
			InitializeComponent();
		}

		protected override void SetControlOrBusinessEntityReadOnly(bool readOnly)
		{
			base.SetControlOrBusinessEntityReadOnly(readOnly);

			BranchGroupSettingsGrid.ReadOnly = readOnly;
			enableBranchLevelPosting.ReadOnly = readOnly;
			disableBranchLevelPosting.ReadOnly = readOnly;
		}

		BranchLevelPostingConfiguration BizObj => BoundBusinessObject as BranchLevelPostingConfiguration;

		protected override void OnAfterFirstBinding(EventArgs e)
		{
			base.OnAfterFirstBinding(e);

			if (BizObj != null)
			{
				if (BizObj.EnableBranchLevelPosting)
				{
					enableBranchLevelPosting.Checked = true;
				}
				else
				{
					disableBranchLevelPosting.Checked = true;
				}
			}
		}

		void enableBranchLevelPosting_CheckedChanged(object sender, EventArgs e)
		{
			BranchGroupSettingsGrid.ReadOnly = false;
			if (BizObj != null)
			{
				BizObj.EnableBranchLevelPosting = true;
			}
		}

		void disableBranchLevelPosting_CheckedChanged(object sender, EventArgs e)
		{
			BranchGroupSettingsGrid.ReadOnly = true;
			if (BizObj != null)
			{
				BizObj.EnableBranchLevelPosting = false;
			}
		}
	}
}
