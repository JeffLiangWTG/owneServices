using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using NUnit.Framework;

namespace Enterprise.BufferManagement.Business.Test
{
	[TestDate(2015, 4, 11)]
	public class ChannelCapacityConstrainedModeTest : BMSTestCaseWithFactory
	{
		public void TestChannelCapacity_InvolvingCCRs_ForConstrainedModeReleaseGroup()
		{
			var capacity1 = CreateChannelCapacity(constrainedModeReleaseGroup.PK, ccr);
			var capacity2 = CreateChannelCapacity(constrainedModeReleaseGroup.PK, nonCCRInConstrainedReleaseGroup);
			var capacity3 = CreateChannelCapacity(constrainedModeReleaseGroup.PK, nonCCRInNonConstrainedReleaseGroup);

			CapacityCalculatorTestHelper.AssertCapacity("Constrained Mode board section", capacity1, "Kathryn Janeway",
@"Total Capacity:
    buffer: 64 hours
    Other Buffer: 12 hours (24 hours for work involving a CCR)",
@"Available Capacity:
    buffer: 62.5 hours
    Other Buffer: 12 hours (24 hours for work involving a CCR)", ExpectedZoneBreakdown,
@"Calculated at: 11-Apr-2015 10:00:00 (buffer)
Calculated at: 11-Apr-2015 10:00:00 (Other Buffer)");

			CapacityCalculatorTestHelper.AssertCapacity("Constrained Mode board section", capacity2, "Benjamin Sisko",
@"Total Capacity:
    buffer: 48 hours (96 hours for work involving a CCR)
    Other Buffer: 12 hours (24 hours for work involving a CCR)",
@"Available Capacity:
    buffer: 46.5 hours (94.5 hours for work involving a CCR)
    Other Buffer: 12 hours (24 hours for work involving a CCR)", ExpectedZoneBreakdown,
@"Calculated at: 11-Apr-2015 10:00:00 (buffer)
Calculated at: 11-Apr-2015 10:00:00 (Other Buffer)");

			CapacityCalculatorTestHelper.AssertCapacity("Constrained Mode board section", capacity3, "Jean-Luc Picard",
@"Total Capacity:
    buffer: 48 hours (96 hours for work involving a CCR)
    Other Buffer: 12 hours (24 hours for work involving a CCR)",
@"Available Capacity:
    buffer: 46.5 hours (94.5 hours for work involving a CCR)
    Other Buffer: 12 hours (24 hours for work involving a CCR)", ExpectedZoneBreakdown,
@"Calculated at: 11-Apr-2015 10:00:00 (buffer)
Calculated at: 11-Apr-2015 10:00:00 (Other Buffer)");
		}

		public void TestChannelCapacity_InvolvingCCRs_ForNonConstrainedModeReleaseGroup()
		{
			var capacity1 = CreateChannelCapacity(nonConstrainedModeReleaseGroup.PK, ccr);
			var capacity2 = CreateChannelCapacity(nonConstrainedModeReleaseGroup.PK, nonCCRInConstrainedReleaseGroup);
			var capacity3 = CreateChannelCapacity(nonConstrainedModeReleaseGroup.PK, nonCCRInNonConstrainedReleaseGroup);

			CapacityCalculatorTestHelper.AssertCapacity("Non-Constrained Mode board section", capacity1, "Kathryn Janeway",
@"Total Capacity:
    buffer: 64 hours
    Other Buffer: 12 hours",
@"Available Capacity:
    buffer: 62.5 hours
    Other Buffer: 12 hours", ExpectedZoneBreakdown,
@"Calculated at: 11-Apr-2015 10:00:00 (buffer)
Calculated at: 11-Apr-2015 10:00:00 (Other Buffer)");

			CapacityCalculatorTestHelper.AssertCapacity("Non-Constrained Mode board section", capacity2, "Benjamin Sisko",
@"Total Capacity:
    buffer: 48 hours
    Other Buffer: 12 hours",
@"Available Capacity:
    buffer: 46.5 hours
    Other Buffer: 12 hours", ExpectedZoneBreakdown,
@"Calculated at: 11-Apr-2015 10:00:00 (buffer)
Calculated at: 11-Apr-2015 10:00:00 (Other Buffer)");

			CapacityCalculatorTestHelper.AssertCapacity("Non-Constrained Mode board section", capacity3, "Jean-Luc Picard",
@"Total Capacity:
    buffer: 48 hours
    Other Buffer: 12 hours",
@"Available Capacity:
    buffer: 46.5 hours
    Other Buffer: 12 hours", ExpectedZoneBreakdown,
@"Calculated at: 11-Apr-2015 10:00:00 (buffer)
Calculated at: 11-Apr-2015 10:00:00 (Other Buffer)");
		}

		#region Implementation

		protected virtual string ExpectedZoneBreakdown =>
			@"Allocated Capacity in buffer: 1.5 hours
    Zone 0: 0 hours
    Zone 1: 0 hours
    Zone 2: 0 hours
    Zone 3: 1.5 hours

Allocated Capacity in Other Buffer: 0 hours
    Zone 0: 0 hours
    Zone 1: 0 hours
    Zone 2: 0 hours
    Zone 3: 0 hours";

		ChannelCapacity CreateChannelCapacity(ZGuid sectionReleaseGroupPK, GlbStaff resource)
		{
			var section = BMSTestHelper.CreateBoardSection(config.Buffer);
			section.SectionConfiguration.ReleaseGroupPK = sectionReleaseGroupPK;
			BMSTestHelper.CreateAdditionalComponent(section, otherBuffer);

			var viewModel = BMSTestHelper.CreateViewModel(section);
			var channel = viewModel.CreateChannelForTest(resource);
			var cell = new CellContent(0, 0, CellContentType.ChannelHeading) { Channel = channel };

			return new ChannelCapacity(channel, cell, section, viewModel, viewModel.ComponentGrid.CardAllocationMap);
		}

		protected override void SetUp()
		{
			base.SetUp();

			WorkingDaysTestHelper.UpdateWeekDaysTo9To5(Factory, Env.CurrentDepartment.PK);

			config = TestConfigsHelper.CreateConstrainedSchematicTestConfig(Factory);
			otherBuffer = BMSTestHelper.CreateBuffer(config.System, "Other Buffer", timespanMinutes: 24 * 60);

			constrainedModeReleaseGroup = config.ReleaseGroup;
			nonConstrainedModeReleaseGroup = Factory.NewWithValidTestData<GlbGroup>();

			ccr = BMSTestHelper.CreateStaffInCurrentBranchDept(Factory, "JWY", "Kathryn Janeway");
			nonCCRInConstrainedReleaseGroup = BMSTestHelper.CreateStaffInCurrentBranchDept(Factory, "SKO", "Benjamin Sisko");
			nonCCRInNonConstrainedReleaseGroup = BMSTestHelper.CreateStaffInCurrentBranchDept(Factory, "PIC", "Jean-Luc Picard");

			ccr.DesignateAsCCR(config.Buffer);

			constrainedModeReleaseGroup.Staff.AddRange(ccr, nonCCRInConstrainedReleaseGroup);
			nonConstrainedModeReleaseGroup.Staff.Add(nonCCRInNonConstrainedReleaseGroup);

			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false);
			var workflow = BMSTestHelper.CreateWorkflow(jobHeader, "workflow", config.Buffer);

			BMSTestHelper.CreateTask(workflow, ccr.GS_Code, 60);
			BMSTestHelper.CreateTask(workflow, nonCCRInConstrainedReleaseGroup.GS_Code, 60);
			BMSTestHelper.CreateTask(workflow, nonCCRInNonConstrainedReleaseGroup.GS_Code, 60);

			Factory.Save();
		}

		protected ConstrainedSchematicTestConfig config;
		BMComponent otherBuffer;

		GlbGroup constrainedModeReleaseGroup, nonConstrainedModeReleaseGroup;
		GlbStaff ccr, nonCCRInConstrainedReleaseGroup, nonCCRInNonConstrainedReleaseGroup;

		#endregion
	}
}
