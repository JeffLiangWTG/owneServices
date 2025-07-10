using CargoWise.Types;
using Enterprise.BufferManagement.Business;
using Enterprise.BufferManagement.Business.Test;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.BufferManagement.Module.Test
{
	[TestedType(typeof(ComponentChangeLogsFilter))]
	class ComponentChangeLogsFilterTest : ModuleFilterTestCase<ComponentChangeLogsFilter>
	{
		public void TestFilter()
		{
			var config = TestConfigsHelper.CreateSchematicTestConfig(Factory);
			var bucket2 = BMSTestHelper.CreateBucket(config.System, "Ru Paul's Best Friend Race");

			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false);
			var workflow1 = BMSTestHelper.CreateWorkflow(jobHeader, "Bob the Drag Queen");
			var workflow2 = BMSTestHelper.CreateWorkflow(jobHeader, "Violet Chachki");
			var workflow3 = BMSTestHelper.CreateWorkflow(jobHeader, "Bianca del Rio");

			workflow2.FH_FC_CurrentComponent = bucket2.PK;
			workflow3.FH_FC_CurrentComponent = bucket2.PK;

			Factory.Save();

			Filter.SelectedFilters.AddGuidFilterStrip(ViewComponentChangeLog.ModuleFilterConstants.ToComponent, bucket2.PK);

			Filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.AnyMatch;
			AssertContainsExactElementsInAnyOrder(new[] { workflow2, workflow3 }, Factory.Load<ProcessHeader>(Filter.Query));

			Filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.NoneMatch;
			AssertContainsExactElementsInAnyOrder(new[] { workflow1, jobHeader }, Factory.Load<ProcessHeader>(Filter.Query));
		}

		public void TestFilter_WhenContainsTransferType_ShouldGenerateWarning()
		{
			Filter.SelectedFilters.AddTextFilterStrip(ViewComponentChangeLog.ModuleFilterConstants.DeferralReason, "XXX");
			Filter.Validation.ValidateAll();

			AssertNoWarning(Filter.SelectedFiltersDescriptionInfo, "Using 'Transfer Type' in Component Change Logs has been deprecated for performance reasons. Consider using 'Last Transfer Type' for workflows instead.");

			Filter.SelectedFilters.AddTextFilterStrip(ViewComponentChangeLog.ModuleFilterConstants.TransferType, "DFR");
			Filter.Validation.ValidateAll();

			AssertHasWarning(Filter.SelectedFiltersDescriptionInfo, "Using 'Transfer Type' in Component Change Logs has been deprecated for performance reasons. Consider using 'Last Transfer Type' for workflows instead.");
		}

		protected override FilterCategory ExpectedDefaultCategory => FilterCategories.Other;
		protected override ZString ExpectedDescription => "Component Change Logs";

		public override void TestIsExpensiveQuery()
		{
			AssertEquals(false, Filter.IsExpensiveQuery);
		}

		protected override ComponentChangeLogsFilter GetNewModuleFilter()
		{
			return new ComponentChangeLogsFilter(Factory);
		}
	}
}
