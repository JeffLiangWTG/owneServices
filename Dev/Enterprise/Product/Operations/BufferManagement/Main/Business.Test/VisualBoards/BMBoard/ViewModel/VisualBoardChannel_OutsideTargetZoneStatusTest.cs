using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.BufferManagement.Integration;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business.Testing;
using NUnit.Framework;

namespace Enterprise.BufferManagement.Business.Test
{
	public class VisualBoardChannel_OutsideTargetZoneStatusTest : BMSTestCaseWithFactory
	{
		#region Status: Outside Target Zone

		[RequiresSTA]
		[TestDate(2016, 9, 5)]
		public void TestStatus_WhenTaskOutsideTargetZoneAndNthPercentageInsideTargetZone_ThenShouldNotShowOutsideTargetZoneStatus()
		{
			var viewModel = AssertPreconditionAndGetSectionViewModel(config);

			AssertNPercentagePositionAndCCRTargetZone(
				"N-th percentage is inside target zone",
				viewModel,
				"CCR",
				expectedNPercentagePosition: 6,
				expectedMinTargetZonePosition: 5,
				expectedMaxTargetZonePosition: 8
			);

			AssertChannelStatus(
				"WHEN 1 task outside target zone but n-th % inside target zone THEN should not show 'outside target zone' status",
				viewModel,
				"Idle");
		}

		[RequiresSTA]
		[TestDate(2016, 9, 5)]
		public void TestStatus_WhenTaskOutsideTargetZoneAndNthPercentageInZone2Or3_ThenShouldNotShowOutsideTargetZoneStatus()
		{
			workflowInsideCCRTargetZone.FH_ReleaseDateTime = ZDateTime.UtcNow;

			var viewModel = AssertPreconditionAndGetSectionViewModel(config);

			AssertNPercentagePositionAndCCRTargetZone(
				"N-th percentage is outside target (lower in buffer penetration)",
				viewModel,
				"CCR",
				expectedNPercentagePosition: 13,
				expectedMinTargetZonePosition: 5,
				expectedMaxTargetZonePosition: 8
			);

			AssertChannelStatus(
				"WHEN n-th % outside target zone (lower in buffer penetration) THEN should not show 'outside target zone' status",
				viewModel,
				"Idle");
		}

		[RequiresSTA]
		[TestDate(2016, 9, 5)]
		public void TestStatus_WhenTaskOutsideTargetZoneAndNthPercentageInZone0Or1_ThenShouldShowOutsideTargetZoneStatus()
		{
			workflowInsideCCRTargetZone.FH_ReleaseDateTime = ZDateTime.UtcNow.AddDays(-12);

			var viewModel = AssertPreconditionAndGetSectionViewModel(config);

			AssertNPercentagePositionAndCCRTargetZone(
				"N-th percentage is outside target zone (higher in buffer penetration)",
				viewModel,
				"CCR",
				expectedNPercentagePosition: 1,
				expectedMinTargetZonePosition: 5,
				expectedMaxTargetZonePosition: 8
			);

			AssertChannelStatus(
				"WHEN n-th outside target zone (higher in buffer penetration) THEN should show 'outside target zone' status",
				viewModel,
				"Outside target zone, Idle");
		}

		[RequiresSTA]
		[TestDate(2016, 9, 5)]
		public void TestStatus_WhenHasMultipleCCR_ThenStatusShouldBeIndependentEachOther()
		{
			var ccr2 = BMSTestHelper.CreateStaffInCurrentBranchDept(Factory, "CR2", "CCR2");
			ccr2.DesignateAsCCR(config.Buffer);
			config.Staffs.Add(ccr2);
			BMSTestHelper.CreatePrimaryChannelForSection(config.Section, ChannelTypeList.Codes.Resource, ccr2.PK, overrideChannels: true);

			var ccr2WorkflowOutsideCCRTargetZone = BMSTestHelper.CreateWorkflowAndTask(
				jobHeader,
				completionStatement: "CCR2 workflow (outside target - high)",
				currentComponent: config.Buffer,
				releaseDateTime: ZDateTime.UtcNow.AddDays(-12),
				staffCode: "CR2",
				lowEstMinutes: 4 * 8 * 60, // 4 days
				description: "Task for CCR2 workflow (outside target - high)");

			var ccr3 = BMSTestHelper.CreateStaffInCurrentBranchDept(Factory, "CR3", "CCR3");
			ccr3.DesignateAsCCR(config.Buffer);
			config.Staffs.Add(ccr3);
			BMSTestHelper.CreatePrimaryChannelForSection(config.Section, ChannelTypeList.Codes.Resource, ccr3.PK, overrideChannels: true);

			var ccr3WorkflowOutsideCCRTargetZone = BMSTestHelper.CreateWorkflowAndTask(
				jobHeader,
				completionStatement: "CCR3 workflow (outside target - lower)",
				currentComponent: config.Buffer,
				releaseDateTime: ZDateTime.UtcNow,
				staffCode: "CR3",
				lowEstMinutes: 4 * 8 * 60, // 4 days
				description: "Task for CCR3 workflow (outside target - lower)");

			Factory.Save();

			var viewModel = AssertPreconditionAndGetSectionViewModel(config);

			AssertNPercentagePositionAndCCRTargetZone(
				"CCR N-th percentage is inside target zone",
				viewModel,
				"CCR",
				expectedNPercentagePosition: 6,
				expectedMinTargetZonePosition: 5,
				expectedMaxTargetZonePosition: 8
			);

			AssertNPercentagePositionAndCCRTargetZone(
				"CCR2 N-th percentage is outside target (high)",
				viewModel,
				"CR2",
				expectedNPercentagePosition: 1,
				expectedMinTargetZonePosition: 5,
				expectedMaxTargetZonePosition: 8
			);

			AssertNPercentagePositionAndCCRTargetZone(
				"CCR3 N-th percentage is outside target (lower)",
				viewModel,
				"CR3",
				expectedNPercentagePosition: 13,
				expectedMinTargetZonePosition: 5,
				expectedMaxTargetZonePosition: 8
			);

			AssertTaskPositionAndZone(
				"CCR2 task is outside target (higher)",
				viewModel,
				taskPK: ccr2WorkflowOutsideCCRTargetZone.Tasks.Single().PK,
				expectedTaskPosition: 1,
				expectedTaskZone: 0);

			AssertTaskPositionAndZone(
				"CCR3 task is outside target (lower)",
				viewModel,
				taskPK: ccr3WorkflowOutsideCCRTargetZone.Tasks.Single().PK,
				expectedTaskPosition: 13,
				expectedTaskZone: 3);

			CombineAssertions("WHEN multiple CCRs, their status should be calculated independently", () =>
			{
				var ccrChannel = viewModel.ComponentGrid.Cells.FirstOrDefault(c => c.ContentType == CellContentType.ChannelHeading && c.Channel.EntityPK == config.CCR.PK).Channel;
				AssertEquals("CCR1 n-th % is in CCR target-zone", "Idle", ccrChannel.Status);

				var ccr2channel = viewModel.ComponentGrid.Cells.FirstOrDefault(c => c.ContentType == CellContentType.ChannelHeading && c.Channel.EntityPK == ccr2.PK).Channel;
				AssertEquals("CCR2 n-th % is outside CCR target-zone (higher in buffer penetration)", "Outside target zone, Idle", ccr2channel.Status);

				var ccr3channel = viewModel.ComponentGrid.Cells.FirstOrDefault(c => c.ContentType == CellContentType.ChannelHeading && c.Channel.EntityPK == ccr3.PK).Channel;
				AssertEquals("CCR3 n-th % is outside CCR target-zone (lower in buffer penetration)", "Idle", ccr3channel.Status);
			});
		}

		#endregion

		#region Implementation

		static void AssertChannelStatus(string message, BMBoardSectionViewModel viewModel, string expectedChannelStatus)
		{
			var channel = viewModel.ComponentGrid.Cells.Single(c =>
				c.Channel != null
				&& c.Channel.ChannelEntityCode == "CCR"
				&& c.ContentType == CellContentType.ChannelHeading
			).Channel;
			AssertEquals(message, expectedChannelStatus, channel.Status);
		}

		static void AssertTaskPositionAndZone(string message, BMBoardSectionViewModel viewModel, ZGuid taskPK, int expectedTaskPosition, int expectedTaskZone)
		{
			var cell = GetTaskCellContent(viewModel, taskPK);
			CombineAssertions(message, () =>
			{
				AssertEquals("task position", expectedTaskPosition, cell.SecondaryAxis);
				AssertEquals("task zone", expectedTaskZone, cell.Zone);
			});
		}

		static void AssertNPercentagePositionAndCCRTargetZone(string message, BMBoardSectionViewModel viewModel, string channelEntityCode, int expectedNPercentagePosition, int expectedMinTargetZonePosition, int expectedMaxTargetZonePosition)
		{
			var nPercentageCell = GetFadeCell(viewModel, channelEntityCode);

			var ccrTargetZone = GetCCRTargetZone(viewModel);
			var minCCRTargetZonePosition = ccrTargetZone.Select(a => a.Key.SecondaryAxis).Min();
			var maxCCRTargetZonePosition = ccrTargetZone.Select(a => a.Key.SecondaryAxis).Max();

			CombineAssertions(message, () =>
			{
				AssertEquals("n-th percentage position", expectedNPercentagePosition, nPercentageCell.SecondaryAxis);
				AssertEquals("CCR target zone position (min)", expectedMinTargetZonePosition, minCCRTargetZonePosition);
				AssertEquals("CCR target zone position (max)", expectedMaxTargetZonePosition, maxCCRTargetZonePosition);
			});
		}

		static IEnumerable<KeyValuePair<CellContent, List<ICardContent>>> GetCCRTargetZone(BMBoardSectionViewModel viewModel)
		{
			return viewModel.ComponentGrid.CardAllocationMap.CardsByCell_ForTest
				.Where(pair => pair.Key.CCRHeaderZone == 2);
		}

		static CellContent GetTaskCellContent(BMBoardSectionViewModel viewModel, ZGuid taskPK)
		{
			return viewModel.ComponentGrid.CardAllocationMap.CardsByCell_ForTest
				.First(pair => pair.Value.Any(content => content.TaskIdentifier == taskPK))
				.Key;
		}

		static CellContent GetFadeCell(BMBoardSectionViewModel viewModel, ZString channelEntityCode)
		{
			return viewModel.ComponentGrid.Cells
				.Single(c => c.Channel != null && c.Channel.ChannelEntityCode == channelEntityCode && c.BackgroundFadeColor != null);
		}

		BMBoardSectionViewModel AssertPreconditionAndGetSectionViewModel(ComplexConstrainedSchematicTestConfig config)
		{
			var viewModel = BMSTestHelper.CreateViewModel(config.Section);
			var channel = viewModel.ComponentGrid.Cells.Single(c =>
				c.Channel != null
				&& c.Channel.ChannelEntityCode == "CCR"
				&& c.ContentType == CellContentType.ChannelHeading
			).Channel;

			VisualBoardChannelTest.RefreshChannelHeading(channel, viewModel, config.Section, jobHeader);

			var cardsByCell = viewModel.ComponentGrid.CardAllocationMap.CardsByCell_ForTest;

			var ccrTargetZone = cardsByCell.Where(pair => pair.Key.CCRHeaderZone == 2);
			var minCCRTargetZonePosition = ccrTargetZone.Select(a => a.Key.SecondaryAxis).Min();
			var maxCCRTargetZonePosition = ccrTargetZone.Select(a => a.Key.SecondaryAxis).Max();

			CombineAssertions("CCR target zone position", () =>
			{
				AssertEquals("min CCR target zone", 5, minCCRTargetZonePosition);
				AssertEquals("max CCR target zone", 8, maxCCRTargetZonePosition);
			});

			AssertTaskPositionAndZone(
				"CCR task outside CCR-Target-Zone (higher in buffer penetration)",
				viewModel,
				taskPK: workflowOutsideCCRTargetZone.Tasks.Single().PK,
				expectedTaskPosition: 1,
				expectedTaskZone: 0);

			return viewModel;
		}

		protected override void SetUp()
		{
			base.SetUp();

			WorkingDaysTestHelper.UpdateEveryDayTo9To5(Factory, Env.CurrentDepartment.PK);

			config = TestConfigsHelper.CreateComplexConstrainedSchematicTestConfig(Factory, makeResourcesPartOfReleaseGroup: false, createWorkflowsAndTasks: false);

			AssertEquals("Precondition: board flow direction", FlowDirectionList.Codes.Up, config.Section.SectionConfiguration.FlowDirection);

			workflowInsideCCRTargetZone = BMSTestHelper.CreateWorkflowAndTask(
				Factory,
				completionStatement: "CCR workflow (inside target)",
				currentComponent: config.Buffer,
				releaseDateTime: ZDateTime.UtcNow.AddDays(-7),
				staffCode: "CCR",
				lowEstMinutes: 4 * 8 * 60, // 4 days - to avoid 'Not Enough Work'
				description: "Task for CCR workflow (inside target)");

			jobHeader = workflowInsideCCRTargetZone.JobHeader;

			workflowOutsideCCRTargetZone = BMSTestHelper.CreateWorkflowAndTask(
				jobHeader,
				completionStatement: "CCR workflow (outside target - high)",
				currentComponent: config.Buffer,
				releaseDateTime: ZDateTime.UtcNow.AddDays(-12),
				staffCode: "CCR",
				lowEstMinutes: 15, // 15 min
				description: "Task for CCR workflow (outside target - high)");

			Factory.Save();
		}

		ComplexConstrainedSchematicTestConfig config;
		ProcessHeader workflowInsideCCRTargetZone;
		ProcessHeader workflowOutsideCCRTargetZone;
		ProcessJobHeader jobHeader;

		#endregion
	}
}
