using Enterprise.BufferManagement.Business;
using Enterprise.BufferManagement.Business.Test;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI.Testing;

namespace Enterprise.BufferManagement.GUI.Test
{
	class FilterValidatorTest : BMSTestCaseWithFactory
	{
		public void TestFilter_WhenGetCurrentTasksHasNewLine_ShouldNotBeValid()
		{
			var customSQLFilter = $@"P9_PK IN 
			(
				SELECT P9_PK FROM dbo.GetCurrentTasks
(
)
)";

			AssertFilter(customSQLFilter, expectError: true);
		}

		public void TestFilter_WhenGetCurrentTasksHasNewLineAndTab_ShouldNotBeValid()
		{
			var customSQLFilter = $@"P9_PK IN 
			(
				SELECT P9_PK FROM dbo.GetCurrentTasks
				(
				)
			)";

			AssertFilter(customSQLFilter, expectError: true);
		}

		public void TestFilter_WhenGetCurrentTasksHasTab_ShouldNotBeValid()
		{
			var customSQLFilter = $@"P9_PK IN 
			(
				SELECT P9_PK FROM dbo.GetCurrentTasks	()
			)";

			AssertFilter(customSQLFilter, expectError: true);
		}

		public void TestFilter_WhenGetCurrentTasksHasSpace_ShouldNotBeValid()
		{
			var customSQLFilter = $@"P9_PK IN 
			(
				SELECT P9_PK FROM dbo.GetCurrentTasks  ()
			)";

			AssertFilter(customSQLFilter, expectError: true);
		}

		public void TestFilter_WhenNotGetCurrentTasks_ShouldBeValid()
		{
			TestConnection.ExecuteNonQuery(@"CREATE FUNCTION dbo.GetCurrentTasksABC() RETURNS TABLE AS RETURN SELECT * FROM dbo.GetCurrentTasks()"); // just so the custom sql is valid
			var customSQLFilter = $@"P9_PK IN 
			(
				SELECT P9_PK FROM dbo.GetCurrentTasksABC()
			)";

			AssertFilter(customSQLFilter, expectError: false);
		}

		public void TestFilter_WhenGetCurrentTasksIgnoringIterationsHasNewLine_ShouldNotBeValid()
		{
			var customSQLFilter = $@"P9_PK IN 
			(
				SELECT P9_PK FROM dbo.GetCurrentTasksIgnoringIterations
(
)
)";

			AssertFilter(customSQLFilter, expectError: true);
		}

		public void TestFilter_WhenGetCurrentTasksIgnoringIterationsHasNewLineAndTab_ShouldNotBeValid()
		{
			var customSQLFilter = $@"P9_PK IN 
			(
				SELECT P9_PK FROM dbo.GetCurrentTasksIgnoringIterations
				(
				)
			)";

			AssertFilter(customSQLFilter, expectError: true);
		}

		public void TestFilter_WhenGetCurrentTasksIgnoringIterationsHasTab_ShouldNotBeValid()
		{
			var customSQLFilter = $@"P9_PK IN 
			(
				SELECT P9_PK FROM dbo.GetCurrentTasksIgnoringIterations	()
			)";

			AssertFilter(customSQLFilter, expectError: true);
		}

		public void TestFilter_WhenGetCurrentTasksIgnoringIterationsHasSpace_ShouldNotBeValid()
		{
			var customSQLFilter = $@"P9_PK IN 
			(
				SELECT P9_PK FROM dbo.GetCurrentTasksIgnoringIterations  ()
			)";

			AssertFilter(customSQLFilter, expectError: true);
		}

		public void TestFilter_WhenNotGetCurrentTasksIgnoringIterations_ShouldBeValid()
		{
			TestConnection.ExecuteNonQuery(@"CREATE FUNCTION dbo.GetCurrentTasksIgnoringIterationsABC() RETURNS TABLE AS RETURN SELECT * FROM dbo.GetCurrentTasksIgnoringIterations()"); // just so the custom sql is valid
			var customSQLFilter = $@"P9_PK IN 
			(
				SELECT P9_PK FROM dbo.GetCurrentTasksIgnoringIterationsABC()
			)";

			AssertFilter(customSQLFilter, expectError: false);
		}

		#region Implementation

		void AssertFilter(string customSQLFilter, bool expectError)
		{
			var taskFilter = BufferSection.TaskFilter;

			FilterStripsTestHelper.AddFilterStrips(taskFilter, new FilterStripsTestHelper.FilterStripDefinition
			{
				FilterStripName = "Custom SQL Filter",
				FilterStripValueSetter = (filter) => ((ModuleSQLFilter)filter).Property1 = customSQLFilter
			});

			FilterValidator.ValidateFilter(taskFilter);

			if (expectError)
			{
				var expectedMessage = "The 'Startable Task Only' filter cannot be chosen on board section filters. Use the configuration option labeled 'Enable Show Startable Items filter by default' instead.";
				AssertHasRowError("WHEN Section-Configuration has CurrentTaskOnly filter THEN should show error", taskFilter, expectedMessage);
			}
			else
			{
				AssertNoErrors("should have no error", taskFilter);
			}
		}

		protected override void SetUp()
		{
			base.SetUp();

			Config = TestConfigsHelper.CreateVisualBoardTestConfig(Factory);
			BufferSection = Config.BufferSection;
		}

		VisualBoardTestConfig Config;
		BMBoardSection BufferSection;

		#endregion
	}
}
