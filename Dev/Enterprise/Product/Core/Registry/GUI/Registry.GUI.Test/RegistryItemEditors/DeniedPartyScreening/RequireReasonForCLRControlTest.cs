using CargoWise.EntityFramework;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Registry.GUI.Testing
{
	[TestedType(typeof(RequireReasonForCLRControl))]
	sealed class RequireReasonForCLRControlTest : RegistryZUserControlTestCase
	{
		protected override IBusiness GetNewBusinessEntity() => new RequireReasonForCLRWrapper();

		protected override bool IsControlOrBusinessEntityReadOnly(RegistryZUserControl control, IBusiness businessEntity) => ((RequireReasonForCLRControl)control).ReadOnly;

		public void TestControlsSetToReadOnly()
		{
			using (var form = new ZForm())
			using (var requireReasonForCLRControl = new RequireReasonForCLRControlForTest())
			{
				form.Controls.Add(requireReasonForCLRControl);
				form.Show();
				AssertEquals("Precondition: ", false, requireReasonForCLRControl.RequireReasonForCLRGrid.ReadOnly);

				requireReasonForCLRControl.SetControlOrBusinessEntityReadOnly(true);
				AssertEquals(false, requireReasonForCLRControl.Enabled);

				requireReasonForCLRControl.SetControlOrBusinessEntityReadOnly(false);
				AssertEquals(true, requireReasonForCLRControl.Enabled);
			}
		}

		public void TestSetDataBinding()
		{
			using (var form = new ZForm())
			using (var requireReasonForCLRControl = new RequireReasonForCLRControlForTest())
			{
				form.Controls.Add(requireReasonForCLRControl);
				form.Show();

				requireReasonForCLRControl.NoRadioButton.Checked = false;
				requireReasonForCLRControl.SetDataBinding(null, "");
				AssertEquals(false, requireReasonForCLRControl.RequireReasonForCLRGrid.ReadOnly);

				requireReasonForCLRControl.NoRadioButton.Checked = true;
				requireReasonForCLRControl.SetDataBinding(null, "");
				AssertEquals(true, requireReasonForCLRControl.RequireReasonForCLRGrid.ReadOnly);
			}
		}

		public void TestYesRadioButton_CheckedChanged()
		{
			using (var form = new ZForm())
			using (var requireReasonForCLRControl = new RequireReasonForCLRControlForTest())
			{
				form.Controls.Add(requireReasonForCLRControl);
				form.Show();

				requireReasonForCLRControl.YesRadioButton.Checked = true;
				AssertEquals(false, requireReasonForCLRControl.RequireReasonForCLRGrid.ReadOnly);

				requireReasonForCLRControl.YesRadioButton.Checked = false;
				AssertEquals(true, requireReasonForCLRControl.RequireReasonForCLRGrid.ReadOnly);
			}
		}
	}
}
