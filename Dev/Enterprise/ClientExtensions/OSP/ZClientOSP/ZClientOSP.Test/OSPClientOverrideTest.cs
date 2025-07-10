using CargoWise.EntityFramework.Testing;
using Enterprise.Client.OSP.Module;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Client.OSP.Testing
{
	class OSPClientOverrideTest : TestCaseWithFactory
	{
		public void TestModuleOverrides()
		{
			ModuleOverrides moduleOverrides = ClientOverride.Instance.ModuleOverrides;
			AssertNotNull("ModuleOverrides should not be null", moduleOverrides);
			AssertNotNull("ModulesOverries should contain JobConsol", moduleOverrides[ModuleIDs.JobConsol, GlbCompany.CurrentCompany.GC_RN_NKCountryCode]);
			AssertEquals("ModulesOverrie should be OSPConsolModuleOverride", typeof(OSPConsolModuleOverride).FullName, moduleOverrides[ModuleIDs.JobConsol, GlbCompany.CurrentCompany.GC_RN_NKCountryCode].TypePath.Split(',')[0]);
		}
	}
}
