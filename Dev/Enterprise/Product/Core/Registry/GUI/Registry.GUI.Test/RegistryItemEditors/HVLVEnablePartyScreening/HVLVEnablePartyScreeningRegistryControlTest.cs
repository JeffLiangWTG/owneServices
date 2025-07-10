using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Windows.UI;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Registry.GUI
{
	[TestedType(typeof(HVLVEnablePartyScreeningRegistryControl))]
	sealed class HVLVEnablePartyScreeningRegistryControlTest : Testing.RegistryZUserControlTestCase
	{
		public void TestControlBindings()
		{
			using (var form = new ZForm())
			using (var control = GetNewControl())
			{
				form.Controls.Add(control);
				form.Show();

				var enableHVLVPartyScreeningCheckBox = form.Controls.Find("enableHVLVPartyScreeningCheckBox", true).First() as ZCheckBox;
				var enableNewDPSResultFormCheckBox = form.Controls.Find("enableNewDPSResultFormCheckBox", true).First() as ZCheckBox;

				CombineAssertions(() =>
				{
					AssertEquals("EnableHVLVPartyScreening Binded", "EnableHVLVPartyScreening", control.BindingSource.GetBindingMember(enableHVLVPartyScreeningCheckBox));
					AssertEquals("EnableNewDPSResultForm Binded", "EnableNewDPSResultForm", control.BindingSource.GetBindingMember(enableNewDPSResultFormCheckBox));
				});
			}
		}

		[RequiresSTA]
		public void TestEnableNewDPSResultForm_ShouldBeBasedOnEnableHVLVPartyScreeningCheckBox()
		{
			using var form = new ZForm();
			using var control = GetNewControl();

			form.Controls.Add(control);
			form.Show();

			var enableHVLVPartyScreeningCheckBox = form.Controls.Find("enableHVLVPartyScreeningCheckBox", true).First() as ZCheckBox;
			var enableNewDPSResultFormCheckBox = form.Controls.Find("enableNewDPSResultFormCheckBox", true).First() as ZCheckBox;

			control.SetReadOnly(true);
			AssertEquals("enableHVLVPartyScreeningCheckBox is readonly when control is set to read only", true, enableHVLVPartyScreeningCheckBox.ReadOnly);
			AssertEquals("enableHVLVPartyScreeningCheckBox is unchecked", false, enableHVLVPartyScreeningCheckBox.Checked);
			AssertEquals("enableNewDPSResultFormCheckBox is readonly when control is set to read only", true, enableNewDPSResultFormCheckBox.ReadOnly);
			AssertEquals("enableNewDPSResultFormCheckBox is checked when control is set to read only", true, enableNewDPSResultFormCheckBox.Checked);

			control.SetReadOnly(false);
			AssertEquals("enableHVLVPartyScreeningCheckBox is editable", false, enableHVLVPartyScreeningCheckBox.ReadOnly);
			AssertEquals("enableNewDPSResultFormCheckBox is readonly when HVLVPartyScreeningCheckBox is unchecked", true, enableNewDPSResultFormCheckBox.ReadOnly);

			enableHVLVPartyScreeningCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
			AssertEquals("enableHVLVPartyScreeningCheckBox is checked", true, enableHVLVPartyScreeningCheckBox.Checked);
			AssertEquals("enableNewDPSResultFormCheckBox is editable when HVLVPartyScreeningCheckBox is checked", false, enableNewDPSResultFormCheckBox.ReadOnly);

			enableNewDPSResultFormCheckBox.CheckState = System.Windows.Forms.CheckState.Unchecked;
			AssertEquals("enableNewDPSResultFormCheckBox is unchecked", false, enableNewDPSResultFormCheckBox.Checked);

			enableHVLVPartyScreeningCheckBox.CheckState = System.Windows.Forms.CheckState.Unchecked;
			AssertEquals("enableNewDPSResultFormCheckBox is checked", true, enableNewDPSResultFormCheckBox.Checked);
			AssertEquals("enableNewDPSResultFormCheckBox is readonly when HVLVPartyScreeningCheckBox is unchecked", true, enableNewDPSResultFormCheckBox.ReadOnly);
		}

		#region Implementation

		protected override IBusiness GetNewBusinessEntity()
		{
			return new HVLVEnablePartyScreening();
		}

		protected override bool IsControlOrBusinessEntityReadOnly(RegistryZUserControl control, IBusiness businessEntity)
		{
			return ((HVLVEnablePartyScreeningRegistryControl)control).ReadOnly;
		}

		#endregion
	}
}
