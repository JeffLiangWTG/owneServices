using System.Linq;
using Enterprise.DbUpgrader.Data;

namespace Enterprise.Builder.DataUpgradeSetup
{
	public class AllTasksSetupController : DataUpgradeSetupController
	{
		protected AllTasksSetupController(UpgradeTask[] tasks)
			: base(tasks)
		{
		}

		public static AllTasksSetupController New()
		{
			DataUpgrader allTasksDataUpgrader = new DbUpgrader.Data.Test.DataUpgraderTest.DataUpgraderForAllTasks();
			UpgradeTask[] allTasks = allTasksDataUpgrader.UpgradeTaskList.OfType<UpgradeTask>().ToArray();

			AllTasksSetupController newControler = new AllTasksSetupController(allTasks);
			return newControler;
		}
	}
}
