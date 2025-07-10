using System.Linq;
using Enterprise.Client.EDI.IncidentManager.Business;
using Enterprise.Client.EDI.Modules;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Client.EDI.IncidentManager.Module.Testing
{
	[TestedType(typeof(IncidentDiagnosticCriteriaModuleFilter))]
	public class IncidentDiagnosticCriteriaModuleFilterTest : ModuleFilterTestCase<IncidentDiagnosticCriteriaModuleFilter>
	{
		public void TestQuery()
		{
			var triage1 = Factory.NewWithValidTestData<IncidentTriage>();
			var triage2 = Factory.NewWithValidTestData<IncidentTriage>();
			var triage3 = Factory.NewWithValidTestData<IncidentTriage>();

			var diagnostic1 = Factory.NewWithValidTestData<IncidentDiagnosticCriteria>();
			diagnostic1.IMD_Description = "Test 1";
			var diagnostic2 = Factory.NewWithValidTestData<IncidentDiagnosticCriteria>();
			diagnostic2.IMD_Description = "Desc 2";
			var diagnostic3 = Factory.NewWithValidTestData<IncidentDiagnosticCriteria>();

			var pivot1 = triage1.DiagnosticCriteriaPivots.AddNew();
			pivot1.IMO_IMD_DiagnosticCriteria = diagnostic1.PK;
			var pivot2 = triage2.DiagnosticCriteriaPivots.AddNew();
			pivot2.IMO_IMD_DiagnosticCriteria = diagnostic2.PK;
			var pivot3 = triage3.DiagnosticCriteriaPivots.AddNew();
			pivot3.IMO_IMD_DiagnosticCriteria = diagnostic3.PK;

			Factory.Save();

			var filter = Filter;
			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.AnyMatch;

			var result = Factory.Load<IncidentTriage>(filter.Query);
			AssertEquals(3, result.Length);
			AssertContainsExactElementsInAnyOrder("Any match: " + filter.Query.LiteralTextADOFormatted, new[] { triage1.IMT_TriageNumber, triage2.IMT_TriageNumber, triage3.IMT_TriageNumber }, result.Select(x => x.IMT_TriageNumber));
		}

		protected override FilterCategory ExpectedDefaultCategory => FilterCategories.Other;

		public override void TestIsExpensiveQuery()
		{
			AssertEquals(false, Filter.IsExpensiveQuery);
		}

		protected override IncidentDiagnosticCriteriaModuleFilter GetNewModuleFilter()
		{
			return new IncidentDiagnosticCriteriaModuleFilter("moo", ClientModuleRegistration.IncidentDiagnosticCriteria, IncidentTriageSchema.PK, IncidentTriageDiagnosticCriteriaPivotSchema.IMO_IMT_Triage, new IncidentDiagnosticCriteriaCollection(Factory), typeof(IncidentTriage));
		}
	}
}
