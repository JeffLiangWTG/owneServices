using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.ZArchitecture.Business.Testing
{
	sealed class RelatedModuleFiltersHelperTest : TestCaseWithFactory
	{
		public void TestCreateFilter()
		{
			var dummy = Factory.New<DummyWithRelatedFilters>();
			var filter = RelatedModuleFiltersHelper.CreateFilter(dummy, "Your words", ModuleIDs.ProcessTasks);

			AssertEquals(ModuleIDs.ProcessTasks.Name, filter.S9_ModuleID);
			AssertEquals(StmModuleFilterTypes.Codes.FilterRule, filter.S9_FilterType);
			AssertEquals("Your words", filter.S9_FilterName);
			AssertEquals(ZGuid.Empty, filter.S9_RelatedEntityID);
			AssertEquals(dummy.PK, filter.S9_ParentID);
			AssertEquals(dummy.TablePrefix, filter.S9_ParentTableCode);
		}

		public void TestGetOrCreateFilter()
		{
			var dummy = Factory.New<DummyWithRelatedFilters>();
			var filter = RelatedModuleFiltersHelper.GetOrCreateFilter(dummy, "", null, ModuleIDs.BMTagRule);

			AssertEquals(ModuleIDs.BMTagRule.Name, filter.S9_ModuleID);
			AssertEquals(StmModuleFilterTypes.Codes.FilterRule, filter.S9_FilterType);
			AssertEquals(string.Empty, filter.S9_FilterName);
			AssertEquals(ZGuid.Empty, filter.S9_RelatedEntityID);
			AssertEquals(dummy.PK, filter.S9_ParentID);
			AssertEquals(dummy.TablePrefix, filter.S9_ParentTableCode);
		}

		public void TestLoadFilters()
		{
			var dummy1 = Factory.New<DummyWithRelatedFilters>();
			var dummy2 = Factory.New<DummyWithRelatedFilters>();
			var dummy3 = Factory.New<DummyWithRelatedFilters>();

			RelatedModuleFiltersHelper.GetOrCreateFilter(dummy1, "Filter 1", null, ModuleIDs.ProcessTasks);
			RelatedModuleFiltersHelper.GetOrCreateFilter(dummy2, "Filter 2", null, ModuleIDs.ProcessTasks);
			RelatedModuleFiltersHelper.GetOrCreateFilter(dummy3, "Filter 3", null, ModuleIDs.ProcessTasks);

			var results = RelatedModuleFiltersHelper.LoadFilters(new[] { dummy1.PK, dummy2.PK }, DummyBizoSchema.Constants.Prefix, Factory);
			AssertContainsExactElementsInAnyOrder(new[] { "Filter 1", "Filter 2" }, results.Select(x => x.S9_FilterName));
		}

		public void TestNewFilterBusinessObject_HasModuleType()
		{
			var filterBizo = RelatedModuleFiltersHelper.GetNewFilterBusinessObject(DummyModuleIDs.Dummy);
			AssertNotNull("ModuleType property should have a value when a filter business object is created using GetNewFilterBusinessObject function.", filterBizo.ModuleType);
		}

		public void TestGetLoadQuery_ShouldIncludeFilterTypeInQuery()
		{
			var dummy = Factory.New<DummyWithRelatedFilters>();
			var query = RelatedModuleFiltersHelper.GetLoadQuery(dummy, null, false);

			AssertContains("S9_FilterType = 'FRU'", query.LiteralTextSqlFormatted);
		}
	}
}
