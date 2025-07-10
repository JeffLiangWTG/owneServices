using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Client.EDI.IncidentManager.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Client.EDI.IncidentManager.Module.Testing
{
	[TestedType(typeof(OrganisationSupportIncidentModuleFilter))]
	class OrganisationSupportIncidentModuleFilterTest : ModuleFilterTestCase<OrganisationSupportIncidentModuleFilter>
	{
		public void TestExactAndNotEqualFilter()
		{
			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			var org2 = Factory.NewWithValidTestData<OrgHeader>();
			var incident1 = Factory.NewWithValidTestData<SupportIncident>();
			incident1.IM_OH_Client = org1.PK;
			var incident2 = Factory.NewWithValidTestData<SupportIncident>();
			incident2.IM_OH_Client = org2.PK;
			Factory.Save();
			Filter.Property = org2.PK;
			Filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.Exact;
			var actualResult1 = Factory.Load<SupportIncident>(Filter.Query);
			AssertContainsExactElementsInAnyOrder(BusinessObjectEqualityComparer<BusinessObject>.PKOnlyComparer, new BusinessObject[] { incident2 }, actualResult1);
			Filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.NotEqual;
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

		protected override OrganisationSupportIncidentModuleFilter GetNewModuleFilter()
		{
			var parent = new SupportIncidentFilterBusinessObject();
			return new OrganisationSupportIncidentModuleFilter(parent, "moo", ModuleIDs.Organisation, IncidentMainSchema.IM_OH_Client, Factory, typeof(SupportIncident));
		}

		protected override FilterCategory ExpectedDefaultCategory => FilterCategories.Organisations;
		protected override FilterCategory InitialTestCatergory => FilterCategories.Other;
	}
}
