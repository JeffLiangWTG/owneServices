using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.UniversalCopy.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.UniversalCopy.Module.Testing
{
	[TestedType(typeof(UniversalCopyScheduleController))]
	internal class UniversalCopyScheduleControllerBasherTest : ZControllerBasherTest
	{
		public void TestNoExceptionWithNullInitialItem()
		{
			var copyTemplate = Factory.New<UniversalCopyTemplate>();
			copyTemplate.S9_ModuleID = ModuleIDs.JobShipment + ZArchitecture.Business.StmModuleFilter.ModuleIdSuffix.UniversalCopyTemplate;
			var copyJob = Factory.New<StmUniversalCopy>();
			copyJob.SUC_S9_CopyTemplate = copyTemplate.PK;

			var controller = new UniversalCopyScheduleController();
			AssertNoExceptionThrown(() => controller.GetCheckPointForEdit(copyJob));
			AssertExceptionThrown(typeof(InvalidOperationException), "Copy item must be passed to UniversalCopyScheduleController", () => controller.GetCheckPointForEdit(null));

			controller = new UniversalCopyScheduleController(copyJob);
			AssertNoExceptionThrown(() => controller.GetCheckPointForEdit(null));
		}

		#region Implementation

		protected override ControllerID GetControllerID()
		{
			return ControllerIDs.UniversalCopySchedule;
		}

		protected override BusinessObject GetBusinessObjectThatIsInTheDatabase()
		{
			if (copy == null)
			{
				var copyTemplate = Factory.New<UniversalCopyTemplate>();
				copyTemplate.S9_ModuleID = DummyModuleIDs.Dummy.Name + ZArchitecture.Business.StmModuleFilter.ModuleIdSuffix.UniversalCopyTemplate;
				copy = Factory.New<StmUniversalCopy>();
				copy.SUC_CopyObjectTableCode = DummyBizoSchema.Constants.Prefix;
				copy.SUC_S9_CopyTemplate = copyTemplate.PK;
				copy.SUC_CopyObjectId = Factory.New<DummyBusinessObject>().PK;
				var sched = Factory.New<StmUniversalCopyScheduleTask>();
				sched.S5_ParentID = copy.PK;
				sched.S5_ParentTableCode = StmUniversalCopySchema.Constants.Prefix;
				Factory.Save();
			}
			return copy;
		}

		StmUniversalCopy copy;

		public override void TestNewForm()
		{
			Assert(true);
		}

		protected override void SetUp()
		{
			base.SetUp();
			Controller = new UniversalCopyScheduleController((StmUniversalCopy)GetBusinessObjectThatIsInTheDatabase());
		}

		#endregion
	}
}
