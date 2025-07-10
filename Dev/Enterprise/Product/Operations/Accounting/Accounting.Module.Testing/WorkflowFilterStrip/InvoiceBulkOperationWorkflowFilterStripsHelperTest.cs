using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Module.Testing
{
	public class InvoiceBulkOperationWorkflowFilterStripsHelperTest : TestCaseWithFactory
	{
		public void TestExceptionWithBusinessObjectType()
		{
			Assert(!new InvoiceBulkOperationWorkflowFilterStripsHelper("JOB", typeof(AccTransactionHeader), Factory, true).IsApplicableToBizOTypeIsAssignableFrom());
			Assert(new InvoiceBulkOperationWorkflowFilterStripsHelper("JOB", typeof(AccTransactionLines), Factory, true).IsApplicableToBizOTypeIsAssignableFrom());
			Assert(new InvoiceBulkOperationWorkflowFilterStripsHelper("JOB", typeof(JobHeader), Factory, true).IsApplicableToBizOTypeIsAssignableFrom());
		}

		public void TestInvoiceBulkOperationWorkflowFilterStripsHelper()
		{
			var filters = new ModuleFilterCollection();
			var subQuery = new ZDBOnlySubQuery(typeof(JobHeader), AccTransactionLinesSchema.AL_JH);
			var helper = new InvoiceBulkOperationWorkflowFilterStripsHelper("JOB", typeof(AccTransactionLines), Factory, true);
			helper.AddRelatedParentJoiningQuery(subQuery);

			helper.AddFilterStrips(filters);

			AssertEquals(typeof(InvoiceBulkOperationWorkflowModuleDateFilter), filters["Milestone Date (Related)"].GetType());
			AssertEquals(typeof(InvoiceBulkOperationWorkflowModuleTextFilter), filters["Milestone Completed (Related)"].GetType());
			AssertEquals(typeof(InvoiceBulkOperationWorkflowModuleDateFilter), filters["Next Milestone (Related)"].GetType());
			AssertEquals(typeof(InvoiceBulkOperationWorkflowModuleDateFilter), filters["Last Completed Milestone (Related)"].GetType());

			Assert(((InvoiceBulkOperationWorkflowModuleDateFilter)filters["Milestone Date (Related)"]).MilestoneEventTypes.Count > 0);
			Assert(((InvoiceBulkOperationWorkflowModuleTextFilter)filters["Milestone Completed (Related)"]).MilestoneEventTypes.Count > 0);
			Assert(((InvoiceBulkOperationWorkflowModuleDateFilter)filters["Next Milestone (Related)"]).MilestoneEventTypes.Count > 0);
			Assert(((InvoiceBulkOperationWorkflowModuleDateFilter)filters["Last Completed Milestone (Related)"]).MilestoneEventTypes.Count > 0);
		}

		public void TestShouldAddRelatedMilestoneFilters()
		{
			var filters = new ModuleFilterCollection();
			var subQuery = new ZDBOnlySubQuery(typeof(JobHeader), AccTransactionLinesSchema.AL_JH);
			var helper = new InvoiceBulkOperationWorkflowFilterStripsHelper("JOB", typeof(AccTransactionLines), Factory, false);
			helper.AddRelatedParentJoiningQuery(subQuery);
			helper.AddFilterStrips(filters);

			AssertNull("should not add related filter", filters["Milestone Date (Related)"]);
			AssertNull("should not add related filter", filters["Milestone Completed (Related)"]);
			AssertNull("should not add related filter", filters["Next Milestone (Related)"]);
			AssertNull("should not add related filter", filters["Last Completed Milestone (Related)"]);

			filters = new ModuleFilterCollection();
			helper = new InvoiceBulkOperationWorkflowFilterStripsHelper("JOB", typeof(AccTransactionLines), Factory, true);
			helper.AddRelatedParentJoiningQuery(subQuery);
			helper.AddFilterStrips(filters);

			AssertNotNull("should add related filter", filters["Milestone Date (Related)"]);
			AssertNotNull("should add related filter", filters["Milestone Completed (Related)"]);
			AssertNotNull("should add related filter", filters["Next Milestone (Related)"]);
			AssertNotNull("should add related filter", filters["Last Completed Milestone (Related)"]);
		}
	}
}
