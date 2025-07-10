using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.BufferManagement.Business.Test
{
	class VisualBoardChannel_NotEnoughWorkStatusTest : BMSTestCaseWithFactory
	{
		[RequiresSTA]
		[TestDate(2016, 9, 5)]
		public void TestStatus_WhenPreconstraintIsNotAlignToConstraint_ThenCalculationShouldUseConstraintOffset()
		{
			AssertStatus(
				"Status should be 'Not enough work' because OPN CCR task duration (21 hours) which is less than 1/3 constraint offset (64 hours/3 = 21.3 hours)",
				config,
				expectedStatus: "Not enough work, Idle");
		}

		[RequiresSTA]
		[TestDate(2016, 9, 5)]
		public void TestStatus_NoConstraint_ThenShouldNotShowNotEnoughWork()
		{
			config.Constraint.Delete();
			Factory.Save();

			AssertStatus("WHEN no constraint THEN should not show status 'Not enough work'", config, expectedStatus: "Zone 3, Idle");
		}

		[RequiresSTA]
		[TestDate(2016, 9, 5)]
		public void TestStatus_ConstraintOffsetIsZero_ThenShouldNotShowNotEnoughWork()
		{
			config.Constraint.FC_OffsetInMinutes = 0;
			Factory.Save();

			AssertStatus("WHEN constraint offset = 0 THEN should not show status 'Not enough work'", config, expectedStatus: "Idle");
		}

		[RequiresSTA]
		[TestDate(2016, 9, 5)]
		public void TestStatus_MultipleConstraints_ThenShouldUseTheMinOffset()
		{
			AssertStatus(
				"The resource has 21 hours assigned, which is less than 1/3 of the offset of the smallest constraint sub-component (21.3 hours).",
				config,
				expectedStatus: "Not enough work, Idle");

			var constraint2 = BMSTestHelper.CreateConstraint(config.Buffer, "Constraint 2", offsetMinutes: 96 * 60);

			task.P9_EstDuration = new ZInt(25 * 60).GetDateTimeFromMinutes();

			Factory.Save();

			AssertStatus(
				"The resource has 25 hours assigned, which is more than 1/3 of the offset of the smallest constraint sub-component (21.3 hours).",
				config,
				expectedStatus: "Idle");
		}

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();

			WorkingDaysTestHelper.UpdateEveryDayTo9To5(Factory, Env.CurrentDepartment.PK);

			config = TestConfigsHelper.CreateComplexConstrainedSchematicTestConfig(Factory, makeResourcesPartOfReleaseGroup: false);

			config.PreConstraintBuffer.FC_BufferTimespanInMinutes = 3 * 8 * 60; // 3 days

			CombineAssertions("Precondition", () =>
			{
				AssertEquals("constraint offset: 8 days", 8d, config.Constraint.ComponentOffset.GetMinutesFromDateTimeSpan() / 60 / 8);
				AssertEquals("Pre constraint buffer timespan: 3 days", 3d, config.PreConstraintBuffer.BufferTimeSpanHours / 8);
			});

			var tasks = config.Workflows.SelectMany(w => w.Tasks);
			tasks.ForEach(task => task.P9_Status = ProcessTaskStatusCodeList.Codes.Cancelled);
			Factory.Save();

			workflow = BMSTestHelper.CreateWorkflowAndTask(
				Factory,
				completionStatement: "CCR workflow X",
				currentComponent: config.Buffer,
				staffCode: "CCR",
				lowEstMinutes: 21 * 60, // 21 hours
				description: "Task for CCR workflow X");

			task = workflow.Tasks.First();

			Factory.Save();
		}

		ComplexConstrainedSchematicTestConfig config;

		ProcessHeader workflow;

		ProcessTask task;

		void AssertStatus(string message, ComplexConstrainedSchematicTestConfig config, string expectedStatus)
		{
			var ccrOpenTasks = Factory.Load<ProcessTask>(new ZQuery(ProcessTasksSchema.P9_GS_NKAssignedStaffMember, "CCR")).Where(task => task.IsOpen);
			AssertEquals("should only have 1 CCR open task", "Task for CCR workflow X", ccrOpenTasks.First().P9_Description);

			var viewModel = BMSTestHelper.CreateViewModel(config.Section);
			var channel = viewModel.ComponentGrid.Cells.FirstOrDefault(c => c.ContentType == CellContentType.ChannelHeading).Channel;
			VisualBoardChannelTest.RefreshChannelHeading(channel, viewModel, config.Section, workflow.JobHeader);

			AssertEquals(message, expectedStatus, channel.Status);
		}

		#endregion
	}
}
