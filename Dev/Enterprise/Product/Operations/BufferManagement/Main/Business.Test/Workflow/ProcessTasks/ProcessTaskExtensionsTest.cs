using System;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using NUnit.Framework;

namespace Enterprise.BufferManagement.Business.Test
{
	class ProcessTaskExtensionsTest : BMSTestCaseWithFactory
	{
		[TestDate(2021, 9, 20, 4, 30, 00)]
		public void TestWorkingTimeSinceBecomingStartable_WithDifferentComponentContext()
		{
			var branch = Factory.NewWithValidTestData<GlbBranch>();
			branch.GB_Code = "BR2";
			branch.GB_RL_NKHomePort = "NLRTM";

			var department = Factory.NewWithValidTestData<GlbDepartment>();
			WorkingDaysTestHelper.UpdateWeekDaysTo9To5(Factory, department.PK);

			var system = BMSTestHelper.CreateSystem(Factory, "DUM");
			var buffer = BMSTestHelper.CreateBuffer(system);
			buffer.FC_GB_AgingBranch = branch.PK;
			buffer.FC_GE_AgingDepartment = department.PK;

			var job = Factory.New<DummyWithWorkflow>();
			var jobHeader = ProcessJobHeaderProvider.GetForParent(job, Factory, addDefaultProcessHeaderIfNone: false);
			var workflow = BMSTestHelper.CreateWorkflow(jobHeader, "workflow");
			workflow.FH_FC_CurrentComponent = buffer.PK;

			var task1 = BMSTestHelper.CreateTask(workflow, taskType: "UDF", sequence: 1);
			var task2 = BMSTestHelper.CreateTask(workflow, taskType: "UDF", sequence: 2);

			Factory.Save();
			TestDateAttribute.AddHours(30);

			// task1: SRT=Y
			// task2: none
			CombineAssertions(() =>
			{
				// Netherlands/Rotterdam - difference with UTC +2:00, so
				// Day1: 9:00 - 17:00
				// Day2: 9:00 - 12:30
				AssertEquals("task1.WorkingTimeSinceBecomingStartable", new TimeSpan(11, 30, 0), task1.WorkingTimeSinceBecomingStartable);
				AssertEquals("task2.WorkingTimeSinceBecomingStartable", TimeSpan.Zero, task2.WorkingTimeSinceBecomingStartable);
			});
		}
	}
}
