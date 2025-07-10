using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.ASYCUDA.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.ASYCUDA.Module.Testing
{
	[TestedType(typeof(CountryRelatedFilter))]
	sealed class CountryRelatedFilterTest : ModuleFilterTestCase<CountryRelatedFilter>
	{
		public void TestIsEmpty()
		{
			var filter = GetNewModuleFilter();
			Assert(filter.IsEmpty);
			filter.Property1 = Core.Constants.CountryCodes.Singapore;
			Assert(filter.IsEmpty);
			filter.Property2 = MessageStatusCodeList.Codes.Sent;
			Assert(!filter.IsEmpty);
			filter.Property1 = string.Empty;
			Assert(!filter.IsEmpty);
		}

		public void TestProperty2List()
		{
			using (GlbCompany.TemporaryLoginInNewCompanyForCountry(Core.Constants.CountryCodes.Singapore))
			{
				var filterBusinessObject = new DummyFilterStripBusinessObject();
				var sgFilter = GetNewModuleFilter();
				sgFilter.IsActive = true;
				filterBusinessObject.AddModuleFilterForTest(sgFilter);
				AssertCollectionContains("sgFilter is active", sgFilter, filterBusinessObject.ActiveModuleFilters);
				AssertSame("sgFilter is attached to bizo", filterBusinessObject, sgFilter.FilterBusinessObject);
				AssertEquals(Core.Constants.CountryCodes.Singapore, sgFilter.Property1);
				AssertStartsWith("Has Logged in country (SG) Status List without a country filter", "ACP, AWA, CAN, ERR,", (sgFilter.Property2List as CodeDescriptionPairList).CodesAsString);
				var countryFilter1 = new ModuleNkFilter(AsycudaFilterStrip.FilterConstants.Country, AsycudaManifestHeaderSchema.AMA_RN_NKCountry, ModuleIDs.RefCountry, new RefCountryCollection(Factory));
				countryFilter1.Property = ZString.Empty;
				countryFilter1.IsActive = true;
				filterBusinessObject.AddModuleFilterForTest(countryFilter1);
				AssertCollectionContains("countryFilter1 is active", countryFilter1, filterBusinessObject.ActiveModuleFilters);
				AssertSame("countryFilter1 is attached to bizo", filterBusinessObject, countryFilter1.FilterBusinessObject);
				AssertEquals(Core.Constants.CountryCodes.Singapore, sgFilter.Property1);
				AssertStartsWith("Has SG Status List when country filter is empty", "ACP, AWA, CAN, ERR,", (sgFilter.Property2List as CodeDescriptionPairList).CodesAsString);
				countryFilter1.Property = Core.Constants.CountryCodes.SouthAfrica;
				AssertEquals(Core.Constants.CountryCodes.SouthAfrica, sgFilter.Property1);
				AssertStartsWith("Has ZA Status List when country filter is ZA", "ACK, AWA, ERR, ", (sgFilter.Property2List as CodeDescriptionPairList).CodesAsString);
				var countryFilter2 = (ModuleNkFilter)filterBusinessObject.ModuleFilters.AddNewDuplicateFilter(AsycudaFilterStrip.FilterConstants.Country, countryFilter1);
				countryFilter2.Property = ZString.Empty;
				countryFilter2.IsActive = true;
				AssertCollectionContains("countryFilter2 is active", countryFilter2, filterBusinessObject.ActiveModuleFilters);
				AssertSame("countryFilter2 is attached to bizo", filterBusinessObject, countryFilter2.FilterBusinessObject);
				AssertEquals(Core.Constants.CountryCodes.SouthAfrica, sgFilter.Property1);
				AssertStartsWith("Has ZA Status List when country filter 1 is ZA and the other country filter is empty", "ACK, AWA, ERR, ", (sgFilter.Property2List as CodeDescriptionPairList).CodesAsString);
				countryFilter2.Property = Core.Constants.CountryCodes.UnitedStates;
				AssertEquals(ZString.Empty, sgFilter.Property1);
				AssertNullOrEmptyOrWhitespace("List is Empty when conflicting country filters exist", (sgFilter.Property2List as CodeDescriptionPairList).CodesAsString);
			}
		}

		public override void TestIsExpensiveQuery()
		{
			AssertEquals(false, Filter.IsExpensiveQuery);
		}

		protected override CountryRelatedFilter GetNewModuleFilter() => new CountryRelatedFilter("moo", (val1, val2) => new ZQuery(), Factory, FieldType.TextDropEdit, 100, CountryRelatedFilterHelper.ListGetters.MessageStatusGetter);

		protected override FilterCategory ExpectedDefaultCategory => FilterCategories.TextSearch;
	}
}
