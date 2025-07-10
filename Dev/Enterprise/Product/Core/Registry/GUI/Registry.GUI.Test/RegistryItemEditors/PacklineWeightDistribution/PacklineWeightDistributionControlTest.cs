using CargoWise.EntityFramework;
using Enterprise.Registry.Business;
using NUnit.Framework;

namespace Enterprise.Registry.GUI.Testing.RegistryItemEditors.PacklineWeightDistribution
{
	[TestedType(typeof(PacklineWeightDistributionControl))]
	sealed class PacklineWeightDistributionControlTest : RegistryBusinessObjectTemplateZUserControlTest
	{
		protected override IBusiness GetNewBusinessEntity()
		{
			return new PacklineWeightDistributionConfiguration();
		}

		public void TestCheckBoxChanged()
		{
			using (var testForm = new RegistryFormTest.DummyRegistryForm())
			using (var control = new PacklineWeightDistributionControl())
			{
				testForm.Controls.Add(control);
				testForm.Show();
				var collection = GetNewBusinessEntity();
				control.SetDataBinding(collection, null);

				AssertEquals("No Radio Button is selected.", expected: true, control.NoRadioButton.Checked);
				AssertEquals("Yes Radio Button is not selected.", expected: false, control.YesRadioButton.Checked);
				AssertEquals("Enable Actual Weight Distribution CheckBox is not checked.", expected: false, control.EnableActualWeightDistributionCheckBox.Checked);
				AssertEquals("Enable Actual Weight Distribution CheckBox is read only (i.e. not clickable).", expected: true, control.EnableActualWeightDistributionCheckBox.ReadOnly);
				AssertEquals("Enable Volumetric Weight Distribution CheckBox is not checked.", expected: false, control.EnableVolumetricWeightDistributionCheckBox.Checked);
				AssertEquals("Enable Volumetric Weight Distribution CheckBox is read only (i.e. not clickable).", expected: true, control.EnableVolumetricWeightDistributionCheckBox.ReadOnly);

				control.YesRadioButton.PerformClick();
				AssertEquals("No Radio Button is not selected.", expected: false, control.NoRadioButton.Checked);
				AssertEquals("Yes Radio Button is selected.", expected: true, control.YesRadioButton.Checked);
				AssertEquals("Enable Actual Weight Distribution CheckBox is checked.", expected: true, control.EnableActualWeightDistributionCheckBox.Checked);
				AssertEquals("Enable Actual Weight Distribution CheckBox is not read only (i.e. clickable).", expected: false, control.EnableActualWeightDistributionCheckBox.ReadOnly);
				AssertEquals("Enable Volumetric Weight Distribution CheckBox is checked.", expected: true, control.EnableVolumetricWeightDistributionCheckBox.Checked);
				AssertEquals("Enable Volumetric Weight Distribution CheckBox is not read only (i.e. clickable).", expected: false, control.EnableVolumetricWeightDistributionCheckBox.ReadOnly);

				control.EnableActualWeightDistributionCheckBox.Checked = false;
				AssertEquals("Enable Actual Weight Distribution CheckBox is not checked.", expected: false, control.EnableActualWeightDistributionCheckBox.Checked);

				control.EnableVolumetricWeightDistributionCheckBox.Checked = false;
				AssertEquals("No Radio Button is selected.", expected: true, control.NoRadioButton.Checked);
				AssertEquals("Yes Radio Button is not selected.", expected: false, control.YesRadioButton.Checked);
				AssertEquals("Enable Actual Weight Distribution CheckBox is not checked.", expected: false, control.EnableActualWeightDistributionCheckBox.Checked);
				AssertEquals("Enable Actual Weight Distribution CheckBox is read only (i.e. not clickable).", expected: true, control.EnableActualWeightDistributionCheckBox.ReadOnly);
				AssertEquals("Enable Volumetric Weight Distribution CheckBox is not checked.", expected: false, control.EnableVolumetricWeightDistributionCheckBox.Checked);
				AssertEquals("Enable Volumetric Weight Distribution CheckBox is read only (i.e. not clickable).", expected: true, control.EnableVolumetricWeightDistributionCheckBox.ReadOnly);
			}
		}
	}
}
