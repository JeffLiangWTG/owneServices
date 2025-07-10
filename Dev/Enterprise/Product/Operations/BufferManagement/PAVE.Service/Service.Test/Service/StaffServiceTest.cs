using System;
using CargoWise.EntityFramework.Testing;
using Enterprise.BufferManagement.Business.Test;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using NUnit.Framework;

namespace Enterprise.BufferManagement.Service.Test
{
	internal class StaffServiceTest : TestCaseWithFactory
	{
		#region Setup and Helpers
		StaffService staffService;

		protected override void SetUp()
		{
			base.SetUp();
			BMSTestHelper.EnableBMSInRegistry();
			staffService = new StaffService();
		}

		#endregion

		#region GetCapacity

		public void TestGetCapacity_ShouldReturnNull_WhenNoStaffOrComponent()
		{
			var capacity = staffService.GetStaffCapacity(Guid.Empty, Guid.Empty);
			AssertNull(capacity);

			capacity = staffService.GetStaffCapacity(Guid.NewGuid(), Guid.NewGuid());
			AssertNull(capacity);

			var staff = Factory.NewWithValidTestData<GlbStaff>();
			Factory.Save();

			capacity = staffService.GetStaffCapacity(Guid.NewGuid(), staff.PK.ToGuid());
			AssertNull(capacity);

			var config = TestConfigsHelper.CreateSchematicTestConfig(Factory);
			Factory.Save();

			capacity = staffService.GetStaffCapacity(config.Buffer.PK.ToGuid(), Guid.NewGuid());
			AssertNull(capacity);
		}

		[TestDate(2025, 6, 17, 9, 0, 0)] //Tuesday
		public void TestGetCapacity_ShouldReturnCapacity()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_Code = "ST1";
			staff.GS_GE_HomeDepartment = Env.CurrentDepartment.PK;
			WorkingDaysTestHelper.UpdateWeekDaysTo9To5(Factory, staff.GS_GE_HomeDepartment);

			var config = TestConfigsHelper.CreateSchematicTestConfig(Factory);
			config.Buffer.FC_BufferTimespanInMinutes = 8 * 60;
			config.Buffer.FC_BufferLoadLimitPercent = 100;

			var jobHeader = BMSTestHelper.CreateJobHeader<DummyWithWorkflow>(Factory, addDefaultProcessHeaderIfNone: false);
			var workflow = BMSTestHelper.CreateWorkflow(jobHeader, "Workflow", currentComponent: config.Buffer);
			var task = BMSTestHelper.CreateTask(workflow, description: "Task", staffCode: staff.GS_Code, lowEstMinutes: 4 * 60, estVariationFactor: 1);

			Factory.Save();

			var capacity = staffService.GetStaffCapacity(config.Buffer.PK.ToGuid(), staff.PK.ToGuid());

			AssertNotNull(capacity);
			AssertEquals(50, capacity.UtilisedPercent); //50%, 4 hours utilised out of 8 hours full capacity
			Assert(!capacity.IsOverloaded);
		}

		#endregion
	}
}
