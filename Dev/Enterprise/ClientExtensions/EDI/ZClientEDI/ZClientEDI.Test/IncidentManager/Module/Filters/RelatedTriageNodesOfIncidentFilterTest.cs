using System.Linq;
using Enterprise.Client.EDI.IncidentManager.Business;
using Enterprise.Client.EDI.Modules;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Client.EDI.IncidentManager.Module.Testing
{
	[TestedType(typeof(RelatedTriageNodesOfIncidentFilter))]
	public class RelatedTriageNodesOfIncidentFilterTest : ModuleFilterTestCase<RelatedTriageNodesOfIncidentFilter>
	{
		public void TestRelatedTriageNodesFilterQuery_AnyMatch()
		{
			var incident1 = Factory.NewWithValidTestData<SupportIncident>();
			var incident2 = Factory.NewWithValidTestData<SupportIncident>();
			var incident3 = Factory.NewWithValidTestData<SupportIncident>();

			var triage1 = Factory.NewWithValidTestData<IncidentTriage>();
			triage1.IMT_TriageNumber = "TRI0TES0001";
			triage1.IMT_SupportDescription = "triage 1";
			var triage2 = Factory.NewWithValidTestData<IncidentTriage>();
			triage2.IMT_SupportDescription = "triage 2";
			var triage3 = Factory.NewWithValidTestData<IncidentTriage>();
			triage3.IMT_SupportDescription = "triage 3";

			incident1.IM_IMT_Triage = triage1.PK;
			incident2.IM_IMT_Triage = triage2.PK;

			Factory.Save();

			// Empty filter
			var filter = Filter;
			filter.IsActive = true;
			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.AnyMatch;

			var result = Factory.Load<SupportIncident>(filter.Query);
			AssertEquals(2, result.Length);
			AssertContainsExactElementsInAnyOrder("Any Match - empty filter: " + filter.Query.LiteralTextADOFormatted, new[] { incident1.Number, incident2.Number }, result.Select(x => x.Number));

			// Filter with data
			var triageNumberFilter = filter.SelectedFilters.AddFilterStrip<ModuleTextFilter>("Triage Number");
			triageNumberFilter.IsActive = true;
			triageNumberFilter.Property = "TRI0TES0001";

			result = Factory.Load<SupportIncident>(filter.Query);
			AssertEquals(1, result.Length);
			AssertContainsExactElementsInAnyOrder("Any Match - filter with data: " + filter.Query.LiteralTextADOFormatted, new[] { incident1.Number }, result.Select(x => x.Number));
		}

		public void TestRelatedTriageNodesFilterQuery_NoneMatch()
		{
			var incident1 = Factory.NewWithValidTestData<SupportIncident>();
			var incident2 = Factory.NewWithValidTestData<SupportIncident>();
			var incident3 = Factory.NewWithValidTestData<SupportIncident>();
			var incident4 = Factory.NewWithValidTestData<SupportIncident>();

			var triage1 = Factory.NewWithValidTestData<IncidentTriage>();
			triage1.IMT_TriageNumber = "TRI0TES0001";
			triage1.IMT_SupportDescription = "triage 1";
			var triage2 = Factory.NewWithValidTestData<IncidentTriage>();
			triage2.IMT_SupportDescription = "triage 2";
			var triage3 = Factory.NewWithValidTestData<IncidentTriage>();
			triage3.IMT_SupportDescription = "triage 3";

			incident1.IM_IMT_Triage = triage1.PK;
			incident2.IM_IMT_Triage = triage2.PK;

			Factory.Save();

			// Empty filter
			var filter = Filter;
			filter.IsActive = true;
			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.NoneMatch;

			var result = Factory.Load<SupportIncident>(filter.Query);
			AssertEquals(2, result.Length);
			AssertContainsExactElementsInAnyOrder("None Match - empty filter: " + filter.Query.LiteralTextADOFormatted, new[] { incident3.Number, incident4.Number }, result.Select(x => x.Number));

			// Filter with data
			var triageNumberFilter = filter.SelectedFilters.AddFilterStrip<ModuleTextFilter>("Triage Number");
			triageNumberFilter.IsActive = true;
			triageNumberFilter.Property = "TRI0TES0001";

			result = Factory.Load<SupportIncident>(filter.Query);
			AssertEquals(3, result.Length);
			AssertContainsExactElementsInAnyOrder("None Match - filter with data: " + filter.Query.LiteralTextADOFormatted, new[] { incident2.Number, incident3.Number, incident4.Number }, result.Select(x => x.Number));
		}

		public void TestRelatedTriageNodesFilterQuery_AllMatch()
		{
			var incident1 = Factory.NewWithValidTestData<SupportIncident>();
			var incident2 = Factory.NewWithValidTestData<SupportIncident>();
			var incident3 = Factory.NewWithValidTestData<SupportIncident>();
			var incident4 = Factory.NewWithValidTestData<SupportIncident>();

			var triage1 = Factory.NewWithValidTestData<IncidentTriage>();
			triage1.IMT_TriageNumber = "TRI0TES0001";
			triage1.IMT_SupportDescription = "triage 1";
			var triage2 = Factory.NewWithValidTestData<IncidentTriage>();
			triage2.IMT_SupportDescription = "triage 2";
			var triage3 = Factory.NewWithValidTestData<IncidentTriage>();
			triage3.IMT_TriageNumber = "TRI0TES0003";

			incident1.IM_IMT_Triage = triage1.PK;
			incident2.IM_IMT_Triage = triage2.PK;

			Factory.Save();

			// Empty filter
			var filter = Filter;
			filter.IsActive = true;
			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.AllMatch;

			var result = Factory.Load<SupportIncident>(filter.Query);
			AssertEquals(4, result.Length);
			AssertContainsExactElementsInAnyOrder("All Match - empty filter: " + filter.Query.LiteralTextADOFormatted, new[] { incident1.Number, incident2.Number, incident3.Number, incident4.Number }, result.Select(x => x.Number));

			// Filter with data
			var triageNumberFilter = filter.SelectedFilters.AddFilterStrip<ModuleTextFilter>("Triage Number");
			triageNumberFilter.IsActive = true;
			triageNumberFilter.Property = "TRI0TES0001";

			result = Factory.Load<SupportIncident>(filter.Query);
			AssertEquals(1, result.Length);
			AssertContainsExactElementsInAnyOrder("All Match - filter with data: " + filter.Query.LiteralTextADOFormatted, new[] { incident1.Number }, result.Select(x => x.Number));
		}

		protected override FilterCategory ExpectedDefaultCategory => FilterCategories.Other;

		public override void TestIsExpensiveQuery()
		{
			AssertEquals(false, Filter.IsExpensiveQuery);
		}

		protected override RelatedTriageNodesOfIncidentFilter GetNewModuleFilter()
		{
			return new RelatedTriageNodesOfIncidentFilter("moo", ClientModuleRegistration.IncidentTriage, IncidentMainSchema.PK, IncidentMainSchema.IM_IMT_Triage, new IncidentTriageCollection(Factory), typeof(SupportIncident));
		}
	}
}
