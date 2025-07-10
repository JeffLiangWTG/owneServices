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
	[TestedType(typeof(NctsDefaultTraderAtDestinationRegistryItemUserControl))]
	class NctsDefaultTraderAtDestinationRegistryItemUserControlTest : RegistryZUserControlTestCase
	{
		public void TestLeaveBlankCheckBox()
		{
			using (var control = new NctsDefaultTraderAtDestinationRegistryItemUserControl())
			{
				var leaveBlankCheckBox = control.FindSingle<ZCheckBox>("LeaveBlankCheckBox");
				AssertEquals("Visible", true, leaveBlankCheckBox.Visible);
				AssertEquals("BindTo", "LeaveBlank", leaveBlankCheckBox.BindTo);
			}
		}

		public void TestTraderAtDestinationGuidFindBox()
		{
			using (var control = new NctsDefaultTraderAtDestinationRegistryItemUserControl())
			{
				var principalCodeFindBox = control.FindSingle<ZCodeFindBox>("TraderAtDestinationGuidFindBox");
				AssertEquals("Visible", true, principalCodeFindBox.Visible);
				AssertEquals("BindTo", "TraderAtDestination", principalCodeFindBox.BindTo);
			}
		}

		protected override IBusiness GetNewBusinessEntity() => new NctsDefaultTraderAtDestination(new FallbackLevel(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty), Factory);

		protected override bool IsControlOrBusinessEntityReadOnly(RegistryZUserControl control, IBusiness businessEntity) => control.ReadOnly;
	}
}
