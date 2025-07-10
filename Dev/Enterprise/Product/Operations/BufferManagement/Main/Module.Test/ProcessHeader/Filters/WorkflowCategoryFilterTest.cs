using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.BufferManagement.Business;
using Enterprise.BufferManagement.Business.Test;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using NUnit.Framework;

namespace Enterprise.BufferManagement.Module.Test
{
	[TestedType(typeof(WorkflowCategoryFilter))]
	class WorkflowCategoryFilterTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			return new WorkflowCategoryFilter(new GetList(() => System.Array.Empty<object>()));
		}

		public void TestWorkflowCategoryFilter()
		{
			BMSTestHelper.AddWorkflowCategoryToRegistry("ORG", "AAA", "It hurts");
			BMSTestHelper.AddWorkflowCategoryToRegistry("ORG", "KKK", "Bad boyz");
			BMSTestHelper.AddWorkflowCategoryToRegistry("INQ", "SSS", "Tiny snek");
			BMSTestHelper.AddWorkflowCategoryToRegistry("INQ", "XXX", "Not for kids");

			var processHeader1 = BMSTestHelper.CreateWorkflowAndParents<OrgHeader>(Factory, "Hitech Software");
			processHeader1.FH_Category = "AAA";
			var jobHeader1 = processHeader1.JobHeader;
			jobHeader1.FH_Category = "AAA";

			var processHeader2 = BMSTestHelper.CreateWorkflowAndParents<OrgHeader>(Factory, "Lowtech Software");
			processHeader2.FH_Category = "KKK";
			var jobHeader2 = processHeader2.JobHeader;
			jobHeader2.FH_Category = "AAA";

			var processHeader3 = BMSTestHelper.CreateWorkflowAndParents<SalesEnquiry>(Factory, "DumbTech Local");
			processHeader3.FH_Category = "SSS";
			var jobHeader3 = processHeader3.JobHeader;
			jobHeader3.FH_Category = "XXX";

			var processHeader4 = BMSTestHelper.CreateWorkflowAndParents<SalesEnquiry>(Factory, "SapiArt Worldwide");
			processHeader4.FH_Category = "XXX";
			var jobHeader4 = processHeader4.JobHeader;
			jobHeader4.FH_Category = "SSS";

			Factory.Save();

			var filterBizo = new ProcessHeaderFilterBusinessObject();
			var filter = (WorkflowCategoryFilter)filterBizo[ProcessHeader.ModuleFilterConstants.WorkflowCategory];

			filter.IsActive = true;
			filter.WorkflowType = "ORG";
			filter.WorkflowCategory = "AAA";

			AssertContainsExactElementsInAnyOrder(Factory.Load<ProcessHeader>(filterBizo.Filter), new ProcessHeader[] { jobHeader1, jobHeader2, processHeader1 });

			filter.WorkflowCategory = "KKK";

			AssertContainsExactElementsInAnyOrder(Factory.Load<ProcessHeader>(filterBizo.Filter), new ProcessHeader[] { processHeader2 });

			filter.WorkflowType = "INQ";
			filter.WorkflowCategory = "SSS";

			AssertContainsExactElementsInAnyOrder(Factory.Load<ProcessHeader>(filterBizo.Filter), new ProcessHeader[] { processHeader3, jobHeader4 });

			filter.WorkflowCategory = "XXX";

			AssertContainsExactElementsInAnyOrder(Factory.Load<ProcessHeader>(filterBizo.Filter), new ProcessHeader[] { processHeader4, jobHeader3 });
		}

		public void TestWorkflowCategoryFilter_Validation()
		{
			BMSTestHelper.AddWorkflowCategoryToRegistry("ORG", "AAA", "It hurts");

			var filterBizo = new ProcessHeaderFilterBusinessObject();
			var filter = (WorkflowCategoryFilter)filterBizo[ProcessHeader.ModuleFilterConstants.WorkflowCategory];
			filter.IsActive = true;
			filter.Validation.ValidateAll();
			AssertHasError(filter.WorkflowTypeInfo, "Please enter a value.");
			AssertHasError(filter.WorkflowCategoryInfo, "Please enter a value.");

			filter.WorkflowType = "ZZZ";
			filter.Validation.ValidateAll();
			AssertHasError(filter.WorkflowTypeInfo, "Enter a valid selection.");
			AssertHasError(filter.WorkflowCategoryInfo, "Please enter a value.");

			filter.WorkflowType = "ORG";
			filter.Validation.ValidateAll();
			AssertNoErrors(filter.WorkflowTypeInfo);
			AssertHasError(filter.WorkflowCategoryInfo, "Please enter a value.");

			filter.WorkflowCategory = "XXX";
			filter.Validation.ValidateAll();
			AssertNoErrors(filter.WorkflowTypeInfo);
			AssertHasError(filter.WorkflowCategoryInfo, "Enter a valid selection.");

			filter.WorkflowCategory = "AAA";
			filter.Validation.ValidateAll();
			AssertNoErrors(filter.WorkflowTypeInfo);
			AssertNoErrors(filter.WorkflowCategoryInfo);
		}
	}
}
