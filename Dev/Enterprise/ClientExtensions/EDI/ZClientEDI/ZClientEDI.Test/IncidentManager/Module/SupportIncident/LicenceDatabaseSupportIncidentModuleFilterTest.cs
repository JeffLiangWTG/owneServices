using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Client.EDI.IncidentManager.Business;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using Enterprise.Client.EDI.Modules;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Client.EDI.IncidentManager.Module.Testing
{
	[TestedType(typeof(LicenceDatabaseSupportIncidentModuleFilter))]
	public class LicenceDatabaseSupportIncidentModuleFilterTest : ModuleFilterTestCase<LicenceDatabaseSupportIncidentModuleFilter>
	{
		public void TestExactAndNotEqualFilter()
		{
			var licenceDatabase1 = Factory.NewWithValidTestData<LicenceDatabase>();
			var incident1 = Factory.NewWithValidTestData<SupportIncident>();
			incident1.IM_LD = licenceDatabase1.PK;
			var licenceDatabase2 = Factory.NewWithValidTestData<LicenceDatabase>();
			var incident2 = Factory.NewWithValidTestData<SupportIncident>();
			incident2.IM_LD = licenceDatabase2.PK;
			Factory.Save();
			Filter.Property = licenceDatabase2.PK;
			Filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.Exact;
			var actualResult1 = Factory.Load<SupportIncident>(Filter.Query);
			AssertContainsExactElementsInAnyOrder(BusinessObjectEqualityComparer<BusinessObject>.PKOnlyComparer, new BusinessObject[] { incident2 }, actualResult1);
			Filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.NotEqual;
			var actualResult2 = Factory.Load<SupportIncident>(Filter.Query);
			AssertContainsExactElementsInAnyOrder(BusinessObjectEqualityComparer<BusinessObject>.PKOnlyComparer, new BusinessObject[] { incident1 }, actualResult2);
		}

		public void TestIsBlankAndIsNotBlankFilter()
		{
			var licenceDatabase1 = Factory.NewWithValidTestData<LicenceDatabase>();
			var incident1 = Factory.NewWithValidTestData<SupportIncident>();
			incident1.IM_LD = licenceDatabase1.PK;
			var incident2 = Factory.NewWithValidTestData<SupportIncident>();
			Factory.Save();
			Filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.IsBlank;
			var actualResult1 = Factory.Load<SupportIncident>(Filter.Query);
			AssertContainsExactElementsInAnyOrder(BusinessObjectEqualityComparer<BusinessObject>.PKOnlyComparer, new BusinessObject[] { incident2 }, actualResult1);
			Filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.IsNotBlank;
			var actualResult2 = Factory.Load<SupportIncident>(Filter.Query);
			AssertContainsExactElementsInAnyOrder(BusinessObjectEqualityComparer<BusinessObject>.PKOnlyComparer, new BusinessObject[] { incident1 }, actualResult2);
		}

		public void TestAllowedComparisonOperators()
		{
			AssertEquals(true, Filter.AllowedComparisonOperators.Contains(ModuleTextFilter.ComparisonConstants.Exact));
			AssertEquals(true, Filter.AllowedComparisonOperators.Contains(ModuleTextFilter.ComparisonConstants.NotEqual));
			AssertEquals(true, Filter.AllowedComparisonOperators.Contains(ModuleTextFilter.ComparisonConstants.IsBlank));
			AssertEquals(true, Filter.AllowedComparisonOperators.Contains(ModuleTextFilter.ComparisonConstants.IsNotBlank));
			AssertEquals(true, Filter.AllowedComparisonOperators.Contains(ModuleTextFilter.ComparisonConstants.AnyMatch));
			AssertEquals(true, Filter.AllowedComparisonOperators.Contains(ModuleTextFilter.ComparisonConstants.NoneMatch));
		}

		public override void TestQueryIsEmptyByDefault()
		{
			AssertEquals(true, Filter.Query.IsEmpty);
		}

		public override void TestIsExpensiveQuery()
		{
			AssertEquals(false, Filter.IsExpensiveQuery);
		}

		protected override LicenceDatabaseSupportIncidentModuleFilter GetNewModuleFilter()
		{
			return new LicenceDatabaseSupportIncidentModuleFilter("moo", ClientModuleRegistration.LicenceDatabase, IncidentMainSchema.IM_LD, Factory, typeof(SupportIncident));
		}

		protected override FilterCategory ExpectedDefaultCategory => FilterCategories.Other;
	}
}
