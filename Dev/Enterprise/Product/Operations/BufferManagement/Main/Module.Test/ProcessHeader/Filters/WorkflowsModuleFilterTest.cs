using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BufferManagement.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.BufferManagement.Module.Testing
{
	[TestedType(typeof(WorkflowsModuleFilter))]
	class WorkflowModuleFilterTest : ModuleFilterTestCase<WorkflowsModuleFilter>
	{
		public void TestFilter()
		{
			var enquiryWithAllSpire = Factory.NewWithValidTestData<SalesEnquiry>();
			var enquiryWithNoSpire = Factory.NewWithValidTestData<SalesEnquiry>();
			var enquiryWithSomeSpire = Factory.NewWithValidTestData<SalesEnquiry>();
			var enquiryWithNoWorkflows = Factory.NewWithValidTestData<SalesEnquiry>();

			enquiryWithAllSpire.O1_ContactName = "enquiryWithAllSpire";
			enquiryWithNoSpire.O1_ContactName = "enquiryWithNoSpire";
			enquiryWithSomeSpire.O1_ContactName = "enquiryWithSomeSpire";
			enquiryWithNoWorkflows.O1_ContactName = "enquiryWithNoWorkflows";

			var parentTableCode = BusinessObjectFactory.GetTableCodeFromType(typeof(SalesEnquiry));

			var spireWorkflowJobHeader = ProcessJobHeader.GetForParent(enquiryWithAllSpire, Factory, false);
			var spireWorkflow1 = CreateWorkflowForFilterTest(enquiryWithAllSpire, parentTableCode, "SPIRE!", spireWorkflowJobHeader.PK);
			var spireWorkflow2 = CreateWorkflowForFilterTest(enquiryWithAllSpire, parentTableCode, "SPIRE!", spireWorkflowJobHeader.PK);

			var blankWorkflowJobHeader = ProcessJobHeader.GetForParent(enquiryWithNoSpire, Factory, false);
			var blankWorkflow1 = CreateWorkflowForFilterTest(enquiryWithNoSpire, parentTableCode, ZString.Empty, blankWorkflowJobHeader.PK);
			var blankWorkflow2 = CreateWorkflowForFilterTest(enquiryWithNoSpire, parentTableCode, ZString.Empty, blankWorkflowJobHeader.PK);

			var jobHeader = ProcessJobHeader.GetForParent(enquiryWithSomeSpire, Factory, false);
			var spireWorkflow3 = CreateWorkflowForFilterTest(enquiryWithSomeSpire, parentTableCode, "SPIRE!", jobHeader.PK);
			var blankWorkflow3 = CreateWorkflowForFilterTest(enquiryWithSomeSpire, parentTableCode, ZString.Empty, jobHeader.PK);

			Factory.Save();

			Filter.SelectedFilters.AddTextFilterStrip("Completion Statement", "SPIRE!");

			var subFilterResult = Factory.Load<ProcessHeader>(Filter.SelectedFilters.Filter);
			AssertContainsExactElementsInAnyOrder(new[] { spireWorkflow1.FH_CompletionStatement, spireWorkflow2.FH_CompletionStatement, spireWorkflow3.FH_CompletionStatement }, subFilterResult.Select(x => x.FH_CompletionStatement));

			Filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.AnyMatch;
			var result = Factory.Load<SalesEnquiry>(Filter.Query);
			AssertContainsExactElementsInAnyOrder("Any match: " + Filter.Query.LiteralTextSqlFormatted, new[] { enquiryWithAllSpire.O1_ContactName, enquiryWithSomeSpire.O1_ContactName }, result.Select(x => x.O1_ContactName));
			AssertContains("FH_ParentTableCode = 'O1'", Filter.Query.LiteralTextSqlFormatted);

			Filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.NoneMatch;
			result = Factory.Load<SalesEnquiry>(Filter.Query);
			AssertContainsExactElementsInAnyOrder("None match: " + Filter.Query.LiteralTextSqlFormatted, new[] { enquiryWithNoSpire.O1_ContactName, enquiryWithNoWorkflows.O1_ContactName }, result.Select(x => x.O1_ContactName));

			Filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.AllMatch;
			result = Factory.Load<SalesEnquiry>(Filter.Query);

			AssertContainsExactElementsInAnyOrder("All match: " + Filter.Query.LiteralTextSqlFormatted, new[] { enquiryWithNoWorkflows.O1_ContactName }, result.Select(x => x.O1_ContactName));
		}

		ProcessHeader CreateWorkflowForFilterTest(IWorkflowProvider parent, string parentTableCode, string completionStatement, ZGuid parentJobHeaderPK)
		{
			var workflow = Factory.NewWithValidTestData<ProcessHeader>();
			workflow.FH_ParentId = parent.PK;
			workflow.FH_ParentTableCode = parentTableCode;
			workflow.FH_CompletionStatement = completionStatement;
			workflow.FH_FH_ParentHeader = parentJobHeaderPK;

			return workflow;
		}

		public override void TestIsExpensiveQuery()
		{
			AssertEquals(false, Filter.IsExpensiveQuery);
		}

		protected override WorkflowsModuleFilter GetNewModuleFilter()
		{
			return new WorkflowsModuleFilter("moo", OrgColdCallRegisterSchema.PK, ProcessHeaderSchema.FH_ParentId, new ProcessHeaderCollection(Factory), typeof(SalesEnquiry));
		}

		protected override FilterCategory ExpectedDefaultCategory => FilterCategories.Other;
	}
}
