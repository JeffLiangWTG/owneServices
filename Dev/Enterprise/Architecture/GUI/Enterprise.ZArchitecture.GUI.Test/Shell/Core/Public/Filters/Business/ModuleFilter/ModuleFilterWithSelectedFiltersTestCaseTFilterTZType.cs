using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Modules.Testing;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.ZArchitecture.GUI.Testing
{
	public abstract class ModuleFilterWithSelectedFiltersTestCase<TFilter, TZType> : ModuleFilterTestCase<TFilter>
			where TZType : IZType
			where TFilter : ModuleFilterWithSelectedFilters<TZType>
	{
		#region Filters Match

		public void TestFiltersMatch()
		{
			var dummy1 = Factory.NewWithValidTestData<DummyBusinessObject>();
			var dummy2 = Factory.NewWithValidTestData<DummyBusinessObject>();
			var dependent1 = Factory.NewWithValidTestData<DummyDependantBusinessObject>();
			var dependent2 = Factory.NewWithValidTestData<DummyDependantBusinessObject>();
			var dependent3 = Factory.NewWithValidTestData<DummyDependantBusinessObject>();

			dummy1.Z0_Description = "Walla Walla";
			dummy2.Z0_Description = "Keyokuk";
			dummy1.Z0_Code = "AAA";
			dummy2.Z0_Code = "BBB";

			var filterBizo = new DummyDependentFilterBusinessObject();
			var filter = (TFilter)filterBizo[FilterDescriptionForFiltersMatchTest];

			if (filter.FilterColumn == DummyDependentBizoSchema.ZD1_Z0)
			{
				dependent1.ZD1_Z0 = dummy1.PK;
				dependent2.ZD1_Z0 = dummy2.PK;
			}
			else if (filter.FilterColumn == DummyDependentBizoSchema.ZD1_Code)
			{
				dependent1.ZD1_Code = dummy1.Z0_Code;
				dependent2.ZD1_Code = dummy2.Z0_Code;
			}

			Factory.Save();

			var result = Factory.Load<DummyDependantBusinessObject>(filterBizo.Filter);
			AssertEquals("Empty filter, should return all business objects, and yet...", 3, result.Length);

			filter.IsActive = true;
			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.FiltersMatch;

			var query = filterBizo.Filter;
			AssertNoExceptionThrown("The query should be valid, and yet..." + query.LiteralTextADOFormatted, () => result = Factory.Load<DummyDependantBusinessObject>(query));
			AssertEquals("A 'filters match' filter has been added but is blank, so it should return dependents with associated dummy business objects only, and yet..." + query.LiteralTextADOFormatted, 2, result.Length);

			filter.SelectedFilters.AddTextFilterStrip("Z0_Description", "Walla Walla");

			var subResult = Factory.Load<DummyBusinessObject>(filter.SelectedFilters.Filter);
			AssertEquals("Only dummy1 associate to dependent1 should match because there was only one sub-filter match, and yet... Query: " + filter.SelectedFilters.Filter.LiteralTextADOFormatted, 1, subResult.Length);
			AssertEquals("Only dummy1 associate to dependent1 should match because there was only one sub-filter match, and yet...", dummy1.PK, subResult.Single().PK);
		}

		protected abstract string FilterDescriptionForFiltersMatchTest { get; }

		#endregion
	}
}
