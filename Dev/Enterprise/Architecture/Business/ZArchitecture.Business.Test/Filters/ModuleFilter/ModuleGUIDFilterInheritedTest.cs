using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.ZArchitecture.Modules.Testing;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.ZArchitecture.Business.Testing
{
	sealed class ModuleGUIDFilterInheritedTest : ModuleFilterWithListAndComparisonOperatorBaseTest<ZGuid>
	{
		protected override ModuleFiltersAndExpectedSQLForTest GetExactComparisonTest()
		{
			var guids = new[] { ZGuid.NewZGuid(), ZGuid.NewZGuid() }.OrderBy(g => g).ToArray();

			return new ModuleFiltersAndExpectedSQLForTest()
			{
				ModuleFilters = new[] {
					new ModuleGuidFilter("Filter", DummyModuleIDs.Dummy, DummyBizoSchema.Z0_Guid, new DummyBusinessObjectCollection(Factory)) { Property = guids[1], ComparisonOperator = ModuleGuidFilter.ComparisonConstants.Exact, OrCategory = FilterOrCategory.Red },
					new ModuleGuidFilter("Filter", DummyModuleIDs.Dummy, DummyBizoSchema.Z0_Guid, new DummyBusinessObjectCollection(Factory)) { Property = guids[0], ComparisonOperator = ModuleGuidFilter.ComparisonConstants.Exact, OrCategory = FilterOrCategory.Red }
				},
				ExpectedSQL = $"(Z0_Guid in (CONVERT('{guids[0]}', 'System.Guid'), CONVERT('{guids[1]}', 'System.Guid')))"
			};
		}

		protected override ModuleFiltersAndExpectedSQLForTest GetExactComparisonTest_InGroup()
		{
			var guids_group1 = new[] { ZGuid.NewZGuid(), ZGuid.NewZGuid() }.OrderBy(g => g).ToArray();
			var guid_group2 = ZGuid.NewZGuid();

			return new ModuleFiltersAndExpectedSQLForTest()
			{
				ModuleFilters = new[] {
					new ModuleGuidFilter("Filter", DummyModuleIDs.Dummy, DummyBizoSchema.Z0_Guid, new DummyBusinessObjectCollection(Factory)) { Property = guids_group1[1], ComparisonOperator = ModuleGuidFilter.ComparisonConstants.Exact, OrCategory = FilterOrCategory.Red, GroupName = "Group 1" },
					new ModuleGuidFilter("Filter", DummyModuleIDs.Dummy, DummyBizoSchema.Z0_Guid, new DummyBusinessObjectCollection(Factory)) { Property = guids_group1[0], ComparisonOperator = ModuleGuidFilter.ComparisonConstants.Exact, OrCategory = FilterOrCategory.Red, GroupName = "Group 1" },
					new ModuleGuidFilter("Filter", DummyModuleIDs.Dummy, DummyBizoSchema.Z0_Guid, new DummyBusinessObjectCollection(Factory)) { Property = guid_group2, ComparisonOperator = ModuleGuidFilter.ComparisonConstants.Exact, OrCategory = FilterOrCategory.Red, GroupName = "Group 2" }
				},
				ExpectedSQL = $"(Z0_Guid in (CONVERT('{guids_group1[0]}', 'System.Guid'), CONVERT('{guids_group1[1]}', 'System.Guid'))) and Z0_Guid = CONVERT('{guid_group2}', 'System.Guid')"
			};
		}
	}
}
