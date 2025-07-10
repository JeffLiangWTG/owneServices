using CargoWise.EntityFramework;
using Enterprise.ArchiveManager.Business.Schedule;
using Enterprise.ArchiveManager.Module.Schedule;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.ArchiveManager.Module.Test.Schedule
{
	[TestedType(typeof(ArchiveScheduleController))]
	class ArchiveScheduleControllerBasherTest : ZControllerBasherTest
	{
		protected override ControllerID GetControllerID()
			=> ControllerIDs.ArchiveSchedule;

		protected override BusinessObject GetBusinessObjectThatIsInTheDatabase()
			=> task;

		protected override void SetUp()
		{
			base.SetUp();
			task = Factory.New<ArchiveScheduleTask>();
			task.S5_ScheduleType = "FWD";
			Factory.Save();
		}

		ArchiveScheduleTask task;
	}
}
