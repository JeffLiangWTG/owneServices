using CargoWise.Application;
using CargoWise.EntityFramework.Testing;
using ServiceManager.Integration.Abstractions;

namespace Enterprise.Scheduler.Business.Testing
{
	sealed class StmScheduleTaskTypeDeciderTest : TestCaseWithFactory
	{
		public void TestGetTypeForLoad()
		{
			StmScheduleTask reportScheduleTask = (StmScheduleTask)Factory.New<Enterprise.Integration.DocumentEngine.IReportScheduleTask>();
			AssertEquals("ReportScheduleTask", ObjectFactory.GetType<Enterprise.Integration.DocumentEngine.IReportScheduleTask>(), Factory.Load<StmScheduleTask>(reportScheduleTask.PK).GetType());

			StmScheduleTask serviceTaskSchedule = (StmScheduleTask)Factory.New<IServiceTaskSchedule>();
			AssertEquals("ServiceTaskSchedule", ObjectFactory.GetType<IServiceTaskSchedule>(), Factory.Load<StmScheduleTask>(serviceTaskSchedule.PK).GetType());

			DummyStmScheduleTask dummyScheduleTask = Factory.New<DummyStmScheduleTask>();
			AssertEquals("DummyStmScheduleTask", typeof(DummyStmScheduleTask), Factory.Load<StmScheduleTask>(dummyScheduleTask.PK).GetType());
		}
	}
}
