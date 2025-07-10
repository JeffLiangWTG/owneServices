using System.Collections.Generic;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture;

namespace Enterprise.ServiceManager.Tasks.XMLAutomation.Testing
{
	sealed class BatchExportDirectorTest : TestCaseWithFactory
	{
		public void TestTasksListCount()
		{
			NotificationBuffer buffer = new NotificationBuffer();
			AccountingExportTasksCreator accTasksCreator = new AccountingExportTasksCreator(buffer, Factory);

			BatchExportDirector director = new BatchExportDirector(null);
			int numOfAccTasks = ((IList<XMLTask>)accTasksCreator.XMLTasks).Count;

			AssertEquals("No of tasks to be performed.", numOfAccTasks, director.ExportTasks.Count);

			AssertEquals("All tasks should have unique UniqueIdentifiers.", numOfAccTasks, director.ExportTasks.Select(x => x.UniqueIdentifier).Distinct().Count());
		}
	}
}
