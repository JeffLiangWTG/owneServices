using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.ComponentModel;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.BufferManagement.Business.Test
{
	class BMComponentSectionConfigurationLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestReleaseGroupModuleID_WhenNoSystemIsSet()
		{
			var board = Factory.New<BMBoard>();
			var section = board.Sections.AddNew();
			var config = new BMComponentSectionConfiguration(section);

			AssertEquals(ModuleIDs.GlbGroup, ZMetaData.GetModuleId(config.Lookups.SystemReleaseGroups));
		}
	}
}
