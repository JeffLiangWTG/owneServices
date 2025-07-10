using System.Collections.Generic;

namespace Enterprise.Client.EDI.ServiceTask.Test.BusinessIntelligence
{
	public class IncidentMainTest : ProductivityEtlExecutionTest
	{
		protected override IEnumerable<string> MainDbTableList => new[] { "IncidentMain" };

		public void TestStaffIncidentMain()
		{
			RunInitialLoad();

			var incident = CreateIncidentMain();
			incident.IM_Description = "Test Description";
			factory.Save();

			RunIncrementalLoad();

			AssertTableHasRow("Workflow.BAS__IncidentMain", "[Description] = 'Test Description'");
		}
	}
}
