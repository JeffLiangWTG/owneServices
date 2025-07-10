using System.Linq;
using Enterprise.Client.EDI.IncidentManager.Business;
using Enterprise.Client.EDI.Modules;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Client.EDI.IncidentManager.Module.Testing
{
	[TestedType(typeof(RelatedDiagnosticCriteriaOfInvestigationItemFilter))]
	public class RelatedDiagnosticCriteriaOfInvestigationItemFilterTest : ModuleFilterTestCase<RelatedDiagnosticCriteriaOfInvestigationItemFilter>
	{
		public void TestQuery()
		{
			var diagnostic1 = Factory.NewWithValidTestData<IncidentDiagnosticCriteria>();
			var diagnostic2 = Factory.NewWithValidTestData<IncidentDiagnosticCriteria>();
			var diagnostic3 = Factory.NewWithValidTestData<IncidentDiagnosticCriteria>();

			var investigationItem1 = Factory.NewWithValidTestData<InvestigationItem>();
			investigationItem1.INV_Description = "Test 1";
			var investigationItem2 = Factory.NewWithValidTestData<InvestigationItem>();
			investigationItem2.INV_Description = "Desc 2";
			var investigationItem3 = Factory.NewWithValidTestData<InvestigationItem>();
			investigationItem3.INV_Description = "Desc 3";

			var pivot1 = diagnostic1.InvestigationItemPivots.AddNew();
			pivot1.DIL_INV_InvestigationItem = investigationItem1.PK;
			var pivot2 = diagnostic2.InvestigationItemPivots.AddNew();
			pivot2.DIL_INV_InvestigationItem = investigationItem2.PK;
			var pivot3 = diagnostic3.InvestigationItemPivots.AddNew();
			pivot3.DIL_INV_InvestigationItem = investigationItem3.PK;

			Factory.Save();

			var filter = Filter;
			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.AnyMatch;

			var result = Factory.Load<InvestigationItem>(filter.Query);
			AssertEquals(3, result.Length);
			AssertContainsExactElementsInAnyOrder("Any match: " + filter.Query.LiteralTextADOFormatted, new[] { investigationItem1.INV_Description, investigationItem2.INV_Description, investigationItem3.INV_Description }, result.Select(x => x.INV_Description));
		}

		protected override FilterCategory ExpectedDefaultCategory => FilterCategories.Other;

		public override void TestIsExpensiveQuery()
		{
			AssertEquals(false, Filter.IsExpensiveQuery);
		}

		protected override RelatedDiagnosticCriteriaOfInvestigationItemFilter GetNewModuleFilter()
		{
			return new RelatedDiagnosticCriteriaOfInvestigationItemFilter("moo", ClientModuleRegistration.InvestigationItem, IncidentDiagnosticCriteriaSchema.PK, DiagnosticCriteriaInvestigationItemLinkSchema.DIL_IMD_DiagnosticCriteria, new InvestigationItemCollection(Factory), typeof(IncidentDiagnosticCriteria));
		}
	}
}
