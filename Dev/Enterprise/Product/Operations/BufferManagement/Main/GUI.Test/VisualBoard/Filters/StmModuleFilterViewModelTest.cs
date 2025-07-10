using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.BufferManagement.Business;
using Enterprise.BufferManagement.Business.Test;
using Enterprise.ZArchitecture.Business;
using NUnit.Framework;

namespace Enterprise.BufferManagement.GUI.Test
{
	[TestedType(typeof(StmModuleFilterViewModel))]
	class StmModuleFilterViewModelTest : NonPersistentBusinessObjectTestCase
	{
		#region Current Task Only Filter

		#region Task Filter

		public void TestTaskFilters_WhenHasCurrentTaskOnlyFilter_ShouldNotBeValid()
		{
			AssertNoErrors("Precondition: BufferSection.HasErrors", BufferSection);

			CurrentTaskOnlyFilterTestHelper.AddFilterToTaskFilter(BufferSection.TaskFilter);

			AssertTaskFilters_CurrentTaskOnlyFilter(BufferSection);
		}

		public void TestTaskFilters_WhenHasNestedCurrentTaskOnlyFilter_ShouldNotBeValid()
		{
			AssertNoErrors("Precondition: BufferSection.HasErrors", BufferSection);

			CurrentTaskOnlyFilterTestHelper.AddNestedFilterToTaskFilter(BufferSection.TaskFilter);

			AssertTaskFilters_CurrentTaskOnlyFilter(BufferSection);
		}

		#endregion

		#region Workflow Filter

		public void TestWorkflowFilters_WhenHasCurrentTaskOnlyFilter_ShouldNotBeValid()
		{
			AssertNoErrors("Precondition: BufferSection.HasErrors", BufferSection);

			CurrentTaskOnlyFilterTestHelper.AddFilterToWorkflowFilter(BufferSection.WorkflowFilter);

			AssertWorkflowFilters_CurrentTaskOnlyFilter(BufferSection);
		}

		public void TestWorkflowFilters_WhenHasUserDefinedCurrentTaskOnlyFilter_ShouldNotBeValid()
		{
			AssertNoErrors("Precondition: BufferSection.HasErrors", BufferSection);

			CurrentTaskOnlyFilterTestHelper.AddUserDefinedFilterToWorkflowFilter(BufferSection.WorkflowFilter);

			AssertWorkflowFilters_CurrentTaskOnlyFilter(BufferSection);
		}

		#endregion

		#endregion

		#region Implementation

		static void AssertTaskFilters_CurrentTaskOnlyFilter(BMBoardSection section)
		{
			var filterStripViewModel = new StmModuleFilterViewModel(section.WorkflowFilter, section.TaskFilter, section.SectionName);
			filterStripViewModel.Validation.ValidateAll();

			AssertFilter(filterStripViewModel.TaskFilter);
		}

		static void AssertWorkflowFilters_CurrentTaskOnlyFilter(BMBoardSection section)
		{
			var filterStripViewModel = new StmModuleFilterViewModel(section.WorkflowFilter, section.TaskFilter, section.SectionName);
			filterStripViewModel.Validation.ValidateAll();

			AssertFilter(filterStripViewModel.WorkflowFilter);
		}

		static void AssertFilter(StmModuleFilter filter)
		{
			var expectedMessage = "The 'Startable Task Only' filter cannot be chosen on board section filters. Use the configuration option labeled 'Enable Show Startable Items filter by default' instead.";
			AssertHasRowError("WHEN Section-Configuration has CurrentTaskOnly filter THEN should show error", filter, expectedMessage);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return new StmModuleFilterViewModel(Factory.NewWithValidTestData<StmModuleFilter>(), Factory.NewWithValidTestData<StmModuleFilter>(), "Test");
		}

		protected override void SetUp()
		{
			BMSTestHelper.EnableBMSInRegistry();

			Config = TestConfigsHelper.CreateVisualBoardTestConfig(Factory);
			BufferSection = Config.BufferSection;
		}

		VisualBoardTestConfig Config;
		BMBoardSection BufferSection;

		#endregion
	}
}
