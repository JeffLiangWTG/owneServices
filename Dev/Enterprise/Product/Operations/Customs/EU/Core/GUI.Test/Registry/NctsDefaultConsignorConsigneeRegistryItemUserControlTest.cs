using System;
using CargoWise.EntityFramework;
using Enterprise.Customs.EU.Registry;
using Enterprise.Environment;
using Enterprise.Registry.GUI;
using Enterprise.Registry.GUI.Testing;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.EU.GUI.Registry.Testing
{
	[TestedType(typeof(NctsDefaultConsignorConsigneeRegistryItemUserControl))]
	class NctsDefaultConsignorConsigneeRegistryItemUserControlTest : RegistryZUserControlTestCase
	{
		public void TestLeaveBlankCheckBox()
		{
			using (var control = new NctsDefaultConsignorConsigneeRegistryItemUserControl())
			{
				var leaveBlankCheckBox = control.FindSingle<ZCheckBox>("LeaveBlankCheckBox");
				CombineAssertions("LeaveBlankCheckBox: ", () =>
				{
					AssertEquals("Visible", true, leaveBlankCheckBox.Visible);
					AssertEquals("BindTo", "LeaveBlank", leaveBlankCheckBox.BindTo);
				});
			}
		}

		public void TestValueFromCheckBox()
		{
			using (var control = new NctsDefaultConsignorConsigneeRegistryItemUserControl())
			{
				var valueFromCheckBoxFindBox = control.FindSingle<ZCheckBox>("ValueFromCheckBox");
				CombineAssertions("ValueFromCheckBox: ", () =>
				{
					AssertEquals("Visible", true, valueFromCheckBoxFindBox.Visible);
					AssertEquals("BindTo", "ValueFrom", valueFromCheckBoxFindBox.BindTo);
				});
			}
		}

		public void TestConsignorCheckBox()
		{
			using (var control = new NctsDefaultConsignorConsigneeRegistryItemUserControl())
			{
				var consignorCheckBox = control.FindSingle<ZCheckBox>("ConsignorCheckBox");
				CombineAssertions("ConsignorCheckBox: ", () =>
				{
					var valueFromCheckBoxFindBox = control.FindSingle<ZCheckBox>("ValueFromCheckBox");
					AssertEquals("BindTo", "Consignor", consignorCheckBox.BindTo);
					valueFromCheckBoxFindBox.Checked = false;
					AssertEquals("Visible", false, consignorCheckBox.Visible);
					valueFromCheckBoxFindBox.Checked = true;
					AssertEquals("Visible", true, consignorCheckBox.Visible);
				});
			}
		}

		public void TestConsigneeCheckBox()
		{
			using (var control = new NctsDefaultConsignorConsigneeRegistryItemUserControl())
			{
				var consigneeCheckBox = control.FindSingle<ZCheckBox>("ConsigneeCheckBox");
				CombineAssertions("ConsigneeCheckBox: ", () =>
				{
					var valueFromCheckBoxFindBox = control.FindSingle<ZCheckBox>("ValueFromCheckBox");
					AssertEquals("BindTo", "Consignee", consigneeCheckBox.BindTo);
					valueFromCheckBoxFindBox.Checked = false;
					AssertEquals("Visible", false, consigneeCheckBox.Visible);
					valueFromCheckBoxFindBox.Checked = true;
					AssertEquals("Visible", true, consigneeCheckBox.Visible);
				});
			}
		}

		protected override IBusiness GetNewBusinessEntity() => new NctsDefaultConsignorConsignee(new FallbackLevel(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty), Factory);

		protected override bool IsControlOrBusinessEntityReadOnly(RegistryZUserControl control, IBusiness businessEntity) => control.ReadOnly;
	}
}
