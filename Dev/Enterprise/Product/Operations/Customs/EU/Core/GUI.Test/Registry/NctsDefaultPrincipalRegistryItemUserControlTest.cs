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
	[TestedType(typeof(NctsDefaultPrincipalRegistryItemUserControl))]
	class NctsDefaultPrincipalRegistryItemUserControlTest : RegistryZUserControlTestCase
	{
		public void TestLeaveBlankCheckBox()
		{
			using (var control = new NctsDefaultPrincipalRegistryItemUserControl())
			{
				var leaveBlankCheckBox = control.FindSingle<ZCheckBox>("LeaveBlankCheckBox");
				AssertEquals("Visible", true, leaveBlankCheckBox.Visible);
				AssertEquals("BindTo", "LeaveBlank", leaveBlankCheckBox.BindTo);
			}
		}

		public void TestPrincipalGuidFindBox()
		{
			using (var control = new NctsDefaultPrincipalRegistryItemUserControl())
			{
				var principalCodeFindBox = control.FindSingle<ZCodeFindBox>("PrincipalGuidFindBox");
				AssertEquals("Visible", true, principalCodeFindBox.Visible);
				AssertEquals("BindTo", "Principal", principalCodeFindBox.BindTo);
			}
		}

		protected override IBusiness GetNewBusinessEntity() => new NctsDefaultPrincipal(new FallbackLevel(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty), Factory);

		protected override bool IsControlOrBusinessEntityReadOnly(RegistryZUserControl control, IBusiness businessEntity) => control.ReadOnly;
	}
}
