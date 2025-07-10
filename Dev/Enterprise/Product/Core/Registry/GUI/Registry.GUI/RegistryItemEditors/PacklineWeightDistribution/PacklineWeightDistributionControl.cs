using System;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Registry.GUI
{
	[SuppressBindingMemberBashingTest]
	public partial class PacklineWeightDistributionControl
	{
		public PacklineWeightDistributionControl()
		{
			InitializeComponent();
		}

		protected override void OnAfterFirstBinding(EventArgs e)
		{
			base.OnAfterFirstBinding(e);

			if (DataSource is PacklineWeightDistributionConfiguration configuration)
			{
				if (configuration.EnablePacklineWeightDistribution)
				{
					YesRadioButton.Checked = true;
					EnableActualWeightDistributionCheckBox.ReadOnly = false;
					EnableVolumetricWeightDistributionCheckBox.ReadOnly = false;
				}
				else
				{
					NoRadioButton.Checked = true;
					EnableActualWeightDistributionCheckBox.Checked = false;
					EnableActualWeightDistributionCheckBox.ReadOnly = true;
					EnableVolumetricWeightDistributionCheckBox.Checked = false;
					EnableVolumetricWeightDistributionCheckBox.ReadOnly = true;
				}
			}
		}

		protected override void SetControlOrBusinessEntityReadOnly(bool readOnly)
		{
			base.SetControlOrBusinessEntityReadOnly(readOnly);
			YesRadioButton.Enabled = !readOnly;
			NoRadioButton.Enabled = !readOnly;
		}

		protected void NoRadioButton_Checked(object sender, EventArgs e)
		{
			EnableActualWeightDistributionCheckBox.Checked = false;
			EnableActualWeightDistributionCheckBox.ReadOnly = true;
			EnableVolumetricWeightDistributionCheckBox.Checked = false;
			EnableVolumetricWeightDistributionCheckBox.ReadOnly = true;
		}

		protected void YesRadioButton_Checked(object sender, EventArgs e)
		{
			EnableActualWeightDistributionCheckBox.ReadOnly = false;
			EnableActualWeightDistributionCheckBox.Checked = true;
			EnableVolumetricWeightDistributionCheckBox.ReadOnly = false;
			EnableVolumetricWeightDistributionCheckBox.Checked = true;
		}

		protected void CheckBox_CheckedChanged(object sender, EventArgs e)
		{
			if (!EnableActualWeightDistributionCheckBox.Checked && !EnableVolumetricWeightDistributionCheckBox.Checked)
			{
				NoRadioButton.Checked = true;
				YesRadioButton.Checked = false;
				NoRadioButton_Checked(sender, e);
			}
		}
	}
}
