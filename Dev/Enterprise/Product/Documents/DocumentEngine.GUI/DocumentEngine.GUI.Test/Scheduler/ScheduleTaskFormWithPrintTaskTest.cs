using CargoWise.EntityFramework.Testing;
using Enterprise.DocumentEngine.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.DocumentEngine.GUI.Scheduler.Testing
{
	sealed class ScheduleTaskFormWithPrintTaskTest : TestCaseWithFactory
	{
		public void TestSchedule()
		{
			Enterprise.DocumentEngine.Testing.PrintTaskTest.MockPrintTask printTask = new Enterprise.DocumentEngine.Testing.PrintTaskTest.MockPrintTask();
			printTask.Schedule();
			AssertNull("LastScheduleController", printTask.LastScheduleController);

			DocumentPack documentPack = new MockDocumentPack();
			printTask.Add(documentPack);
			printTask.Schedule();
			AssertNull("LastScheduleController", printTask.LastScheduleController);

			StmMenuItem menuItem = Factory.LoadTop1<StmMenuItem>(new DocumentZQuery());
			printTask.Remove(documentPack);
			printTask.Add(new DocumentPack(menuItem));
			printTask.Schedule();
			AssertNotNull("LastScheduleController", printTask.LastScheduleController);

			ZController lastScheduleController = (ZController)printTask.LastScheduleController;
			using (ScheduleTaskForm lastShownForm = (ScheduleTaskForm)lastScheduleController.LastShownForm)
			{
				AssertEquals("LastScheduleController.LastShownForm.BusinessEntity.S5_ParentID", menuItem.PK, lastShownForm.BusinessEntity.S5_ParentID);
			}
		}
	}
}
