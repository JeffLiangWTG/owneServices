using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.VisualBoards.Business.Test;
using NUnit.Framework;

namespace Enterprise.BufferManagement.Business.Test
{
	public abstract class ComponentGridBasherTest : VisualBoardsTestCase
	{
		protected virtual ComponentGridBasherTestCaseBuilder GetTestCaseBuilder()
		{
			return new ComponentGridBasherTestCaseBuilder();
		}

		#region Implementation

		protected IEnumerable<VisualBoardPositionForTest<int>> CreateExpectedSubComponentHeadingTests(int primaryAxis, int startingSecondaryAxis, int[] expectedZones)
		{
			return expectedZones.Select((zone, index) => new VisualBoardPositionForTest<int>(primaryAxis, startingSecondaryAxis + index, zone));
		}

		protected void AssertOverlappingSubComponentHeading(ZString subComponentName, int primaryAxis, IEnumerable<VisualBoardPositionForTest<int>> expectedSubComponentHeadingTests)
		{
			var expected = expectedSubComponentHeadingTests.ToList();
			expected.ForEach(c => c.ConvertToFlowDirectioned(Config.Section));

			IEnumerable<VisualBoardPositionForTest<int>> actualSubcomponentHeadings = null;

			if (!subComponentName.IsEmpty)
			{
				var subComponent = Config.Section.AllComponents.SelectMany(c => c.ChildComponents).First(c => c.FC_Name == subComponentName);
				actualSubcomponentHeadings = Config.SectionViewModel.ComponentGrid.Cells
					.Where(c => c.SubComponentZones.Keys.Contains(subComponent.PK) && c.ContentType == CellContentType.SubComponentZoneHeading && c.PrimaryAxis == primaryAxis)
					.Select(c => new VisualBoardPositionForTest<int>(c.PrimaryAxis, c.SecondaryAxis, value: c.SubComponentZones.Any() ? (int)c.Zone : -1));
			}
			else
			{
				actualSubcomponentHeadings = expected
					.Select(t => Config.SectionViewModel.ComponentGrid.Cells.FirstOrDefault(c => c.PrimaryAxis == t.PrimaryAxis && c.SecondaryAxis == t.SecondaryAxis))
					.Select(c => new VisualBoardPositionForTest<int>(c.PrimaryAxis, c.SecondaryAxis, value: c.SubComponentZones.Any() ? (int)c.Zone : -1));
			}

			var assertMessage = string.Format("Overlap subcomponents headings {0} with flow direction {1} : \r\nExpected:\r\n {2}\r\nActual:\r\n {3}",
				subComponentName,
				Config.Section.SectionConfiguration.FlowDirection,
				VisualBoardPositionForTest<int>.Log(Config.Section, expected),
				VisualBoardPositionForTest<int>.Log(Config.Section, actualSubcomponentHeadings));

			AssertContainsExactElementsInAnyOrder(assertMessage, new VisualBoardPositionTestComparer<int>(), expected, actualSubcomponentHeadings);
		}

		#endregion

		#region Test Channel Status: High Risk (Post CCR)

		[RequiresSTA]
		[TestDate(2014, 1, 31)]
		public void TestCCRStatus_WithIncrementWorkflowAging()
		{
			CreateStatusHighRiskPostCCRTest(ZDateTime.Today);

			var wipWorkflow = Config.Workflows.FirstOrDefault(w => w.FH_CompletionStatement == "wip-wf");
			foreach (var ccrTaskInPostCCRWorkflow in wipWorkflow.Tasks.Where(t => t.P9_GS_NKAssignedStaffMember == "CCR"))
			{
				ccrTaskInPostCCRWorkflow.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
			}

			for (var agingInDays = 0; agingInDays < 13; agingInDays++)
			{
				wipWorkflow.FH_ReleaseDateTime = ZDateTime.Today.AddDays(-agingInDays);
				Factory.Save();

				var viewModel = Config.ResetViewModel();

				var channel = Config.SectionViewModel.ComponentGrid.Cells.FirstOrDefault(c => c.ContentType == CellContentType.ChannelHeading && c.Channel.EntityPK == Config.CCR.PK).Channel;
				AssertCCRStatus_WithIncrementWorkflowAging(agingInDays, channel);
			}
		}

		protected virtual void AssertCCRStatus_WithIncrementWorkflowAging(int agingIndex, IVisualBoardChannel channel)
		{
			AssertEquals(string.Format("Aging={0}, post-ccr-task should not impact CCR status", agingIndex), "Idle", channel.Status);
		}

		[RequiresSTA]
		[TestDate(2014, 1, 31)]
		public void TestCCRStatus_Zone1PostCCRBuffer_WithClosedCCRTask()
		{
			CreateStatusHighRiskPostCCRTest(ZDateTime.Today.AddDays(-12));

			var wipWorkflow = Config.Workflows.FirstOrDefault(w => w.FH_CompletionStatement == "wip-wf");
			foreach (var ccrTaskInPostCCRWorkflow in wipWorkflow.Tasks.Where(t => t.P9_GS_NKAssignedStaffMember == "CCR"))
			{
				ccrTaskInPostCCRWorkflow.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
			}

			var channel = Config.SectionViewModel.ComponentGrid.Cells.FirstOrDefault(c => c.ContentType == CellContentType.ChannelHeading && c.Channel.EntityPK == Config.CCR.PK).Channel;
			AssertCCRStatus_Zone1PostCCRBuffer_WithClosedCCRTask(channel);
		}

		protected virtual void AssertCCRStatus_Zone1PostCCRBuffer_WithClosedCCRTask(IVisualBoardChannel channel)
		{
			AssertEquals("post-ccr-task should not impact CCR status", "Idle", channel.Status);
		}

		[RequiresSTA]
		[TestDate(2014, 1, 1)]
		public void TestCCRStatus_Zone1PostCCRBuffer_WithLowerSequenceNumberClosedCCRTask_AndHigherSequenceNumberOpenCCRTask()
		{
			CreateStatusHighRiskPostCCRTest(ZDateTime.Today.AddDays(-12));

			var wipWorkflow = Config.Workflows.FirstOrDefault(w => w.FH_CompletionStatement == "wip-wf");
			foreach (var ccrTaskInPostCCRWorkflow in wipWorkflow.Tasks.Where(t => t.P9_GS_NKAssignedStaffMember == "CCR"))
			{
				ccrTaskInPostCCRWorkflow.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
			}

			BMSTestHelper.CreateTask(wipWorkflow, "CCR", description: "wip-wf: ccr-task", lowEstMinutes: 100, sequence: 100);

			// Avoid triggering high risk (pre-ccr)
			var wipWorkflow2 = BMSTestHelper.CreateWorkflowAndTask(
				Factory,
				completionStatement: "wip-wf2",
				description: "wip-wf2: ccr-task",
				currentComponent: Config.Buffer,
				releaseDateTime: ZDateTime.Today,
				staffCode: "CCR",
				lowEstMinutes: 3 * 8 * 60, sequence: 2);
			wipWorkflow2.Parent.WorkflowItems[0].P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
			BMSTestHelper.CreateTask(wipWorkflow2, "NC1", description: "wip-wf2: non-ccr-task", lowEstMinutes: 1 * 8 * 60, sequence: 1);
			Config.Workflows.Add(wipWorkflow2);

			var channel = Config.SectionViewModel.ComponentGrid.Cells.FirstOrDefault(c => c.ContentType == CellContentType.ChannelHeading && c.Channel.EntityPK == Config.CCR.PK).Channel;
			AssertCCRStatus_Zone1PostCCRBuffer_WithLowerSequenceNumberClosedCCRTask_AndHigherSequenceNumberOpenCCRTask(channel);
		}

		protected virtual void AssertCCRStatus_Zone1PostCCRBuffer_WithLowerSequenceNumberClosedCCRTask_AndHigherSequenceNumberOpenCCRTask(IVisualBoardChannel channel)
		{
			AssertEquals("post-ccr-task shoud not impact CCR status", "Idle", channel.Status);
		}

		[RequiresSTA]
		[TestDate(2014, 1, 1)]
		public void TestCCRStatus_Zone1PostCCRBuffer_WithClosedCCRTask_ButPreCCRWorkflow()
		{
			CreateStatusHighRiskPostCCRTest(ZDateTime.Today.AddDays(-12));

			var wipWorkflow = Config.Workflows.FirstOrDefault(w => w.FH_CompletionStatement == "wip-wf");
			foreach (var ccrTaskInPostCCRWorkflow in wipWorkflow.Tasks.Where(t => t.P9_GS_NKAssignedStaffMember == "CCR"))
			{
				ccrTaskInPostCCRWorkflow.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
				ccrTaskInPostCCRWorkflow.P9_Sequence += 10;
			}

			// Avoid triggering high risk (pre-ccr)
			var wipWorkflow2 = BMSTestHelper.CreateWorkflowAndTask(
				Factory,
				completionStatement: "wip-wf2",
				description: "wip-wf2: ccr-task",
				currentComponent: Config.Buffer,
				releaseDateTime: ZDateTime.Today,
				staffCode: "CCR",
				lowEstMinutes: 3 * 8 * 60, sequence: 2);
			wipWorkflow2.Parent.WorkflowItems[0].P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
			BMSTestHelper.CreateTask(wipWorkflow2, "NC1", description: "wip-wf2: non-ccr-task", lowEstMinutes: 1 * 8 * 60, sequence: 1);
			Config.Workflows.Add(wipWorkflow2);

			var channel = Config.SectionViewModel.ComponentGrid.Cells.FirstOrDefault(c => c.ContentType == CellContentType.ChannelHeading && c.Channel.EntityPK == Config.CCR.PK).Channel;
			AssertCCRStatus_Zone1PostCCRBuffer_WithClosedCCRTask_ButPreCCRWorkflow(channel);
		}

		protected virtual void AssertCCRStatus_Zone1PostCCRBuffer_WithClosedCCRTask_ButPreCCRWorkflow(IVisualBoardChannel channel)
		{
			AssertEquals("post-ccr-task shoud not impact CCR status", "Idle", channel.Status);
		}

		[RequiresSTA]
		[TestDate(2014, 1, 1)]
		public void TestCCRStatus_Zone1PostCCRBuffer_WithOpenCCRTask()
		{
			CreateStatusHighRiskPostCCRTest(ZDateTime.Today.AddDays(-12));

			var channel = Config.SectionViewModel.ComponentGrid.Cells.FirstOrDefault(c => c.ContentType == CellContentType.ChannelHeading && c.Channel.EntityPK == Config.CCR.PK).Channel;
			AssertCCRStatus_Zone1PostCCRBuffer_WithOpenCCRTask(channel);
		}

		protected virtual void AssertCCRStatus_Zone1PostCCRBuffer_WithOpenCCRTask(IVisualBoardChannel channel)
		{
			AssertEquals("post-ccr-task should not impact CCR status", "Idle", channel.Status);
		}

		[RequiresSTA]
		[TestDate(2014, 1, 1)]
		public void TestCCRStatus_Zone2PostCCRBuffer()
		{
			CreateStatusHighRiskPostCCRTest(ZDateTime.Today.AddDays(-11));

			var channel = Config.SectionViewModel.ComponentGrid.Cells.FirstOrDefault(c => c.ContentType == CellContentType.ChannelHeading && c.Channel.EntityPK == Config.CCR.PK).Channel;
			AssertCCRStatus_Zone2PostCCRBuffer(channel);
		}

		protected virtual void AssertCCRStatus_Zone2PostCCRBuffer(IVisualBoardChannel channel)
		{
			AssertEquals("post-ccr-task should not impact CCR status", "Idle", channel.Status);
		}

		void CreateStatusHighRiskPostCCRTest(ZDateTime releaseDateTime)
		{
			var workflow = BMSTestHelper.CreateWorkflowAndTask(
				Factory,
				completionStatement: "wip-wf",
				description: "wip-wf: ccr-task",
				currentComponent: Config.Buffer,
				releaseDateTime: releaseDateTime,
				staffCode: "CCR",
				lowEstMinutes: 60, sequence: 1);
			workflow.Parent.WorkflowItems[0].P9_Status = ProcessTaskStatusCodeList.Codes.Closed;

			BMSTestHelper.CreateTask(workflow, "NC1", description: "wip-wf: post-ccr-task", lowEstMinutes: 100, sequence: 2);
			Config.Workflows.Add(workflow);

			var ccrWorkflow = BMSTestHelper.CreateWorkflowAndTask(
				Factory,
				completionStatement: "ccr-wf",
				description: "ccr-wf: ccr-task",
				currentComponent: Config.Buffer,
				releaseDateTime: ZDateTime.Now,
				staffCode: "CCR",
				lowEstMinutes: 3 * 8 * 60,
				sequence: 1);
			Config.Workflows.Add(ccrWorkflow);

			Factory.Save();
		}

		#endregion

		#region Test Channel Status

		[RequiresSTA]
		[TestDate(2014, 1, 1)]
		public void TestChannelStatus_OutsideTargetZone()
		{
			CCRWorkflow.FH_ReleaseDateTime = ZDateTime.UtcNow.AddDays(-4);
			CCRWorkflow.Parent.WorkflowItems[0].P9_EstDuration = new ZInt(3 * 8 * 60).GetDateTimeFromMinutes();
			var viewModel = Config.SectionViewModel;

			Factory.Save();

			var channel = Config.SectionViewModel.ComponentGrid.Cells.FirstOrDefault(c => c.ContentType == CellContentType.ChannelHeading).Channel;

			viewModel.Cache.Clear();
			AssertChannelStatus_OutsideTargetZone_InitialCondition(channel);

			var preCCRWorkflow = BMSTestHelper.CreateWorkflowAndTask(
				Factory,
				completionStatement: "pre-ccr-workflow",
				currentComponent: Config.Buffer,
				releaseDateTime: ZDateTime.UtcNow.AddDays(-9),
				staffCode: Config.CCR.GS_Code,
				lowEstMinutes: 8 * 60,
				sequence: 2,
				description: "pre-ccr-workflow - ccr-task");
			Config.Workflows.Add(preCCRWorkflow);
			Factory.Save();

			viewModel = Config.ResetViewModel();
			channel = Config.SectionViewModel.ComponentGrid.Cells.FirstOrDefault(c => c.ContentType == CellContentType.ChannelHeading).Channel;

			AssertChannelStatus_OutsideTargetZone_AddedNewCCRWorkflowAndTaskWith9DaysAging(channel);

			var preCCRTask = CreateTask(
				workflow: preCCRWorkflow,
				staffCode: Config.NonCCR1.GS_Code,
				lowEstMinutes: 60,
				sequence: 1,
				description: "pre-ccr-worflow - Non-CCR-1 task");
			Factory.Save();
			Config.Section.Factory.ClearCachedValue<BufferPenetrationCalculator.BufferPenetrationCache>();
			viewModel = Config.ResetViewModel();
			channel = Config.SectionViewModel.ComponentGrid.Cells.FirstOrDefault(c => c.ContentType == CellContentType.ChannelHeading).Channel;

			AssertChannelStatus_OutsideTargetZone_MadeCCRTaskNonCurrent(channel);
		}

		protected virtual void AssertChannelStatus_OutsideTargetZone_InitialCondition(IVisualBoardChannel channel)
		{
			AssertEquals("Initial condition: Not high risk because CCR queue is not too short", "Idle", channel.Status);
		}

		protected virtual void AssertChannelStatus_OutsideTargetZone_AddedNewCCRWorkflowAndTaskWith9DaysAging(IVisualBoardChannel channel)
		{
			AssertEquals("'Outside target zone' because n-th percentage is outside target zone (higher in buffer penetration)", "Outside target zone, Idle", channel.Status);
		}

		protected virtual void AssertChannelStatus_OutsideTargetZone_MadeCCRTaskNonCurrent(IVisualBoardChannel channel)
		{
			AssertEquals("'Outside target zone' because regardless task current status, CCR n-th percentage is outside target zone (higher in buffer penetration)", "Outside target zone, Idle", channel.Status);
		}

		#endregion

		#region CCR Heading Cell Zone

		[RequiresSTA]
		[TestDate(2014, 1, 1)]
		public void TestCCRHeadingZones()
		{
			var expectedCCRHeadingZonesForVariousConstraintOffsets = new[] {
				// <- Pre-constraint | Constraint | Post-constraint ->
				new[] { /* constraint */ 2, 2, 2, 2, 1, 1, 1, 1, 1, 1, 1, 1, 0 }, // constraint with offset 0 day
				new[] { 3, /* constraint */ 2, 2, 2, 2, 1, 1, 1, 1, 1, 1, 1, 0 }, // constraint with offset 1 day
				new[] { 3, 2, /* constraint */ 2, 2, 2, 1, 1, 1, 1, 1, 1, 1, 0 }, // constraint with offset 2 day
				new[] { 3, 3, 2, /* constraint */ 2, 2, 2, 1, 1, 1, 1, 1, 1, 0 }, // constraint with offset 3 day
				new[] { 3, 3, 3, 2, /* constraint */ 2, 2, 2, 1, 1, 1, 1, 1, 0 }, // constraint with offset 4 day
				new[] { 3, 3, 3, 2, 2, /* constraint */ 2, 2, 1, 1, 1, 1, 1, 0 }, // constraint with offset 5 day
				new[] { 3, 3, 3, 3, 2, 2, /* constraint */ 2, 2, 1, 1, 1, 1, 0 }, // constraint with offset 6 day
				new[] { 3, 3, 3, 3, 3, 2, 2, /* constraint */ 2, 2, 1, 1, 1, 0 }, // constraint with offset 7 day
				new[] { 3, 3, 3, 3, 3, 2, 2, 2, /* constraint */ 2, 1, 1, 1, 0 }, // constraint with offset 8 day 
				new[] { 3, 3, 3, 3, 3, 3, 2, 2, 2, /* constraint */ 2, 1, 1, 0 }, // constraint with offset 9 day 
				new[] { 3, 3, 3, 3, 3, 3, 3, 2, 2, 2, /* constraint */ 2, 1, 0 }, // constraint with offset 10 day 
				new[] { 3, 3, 3, 3, 3, 3, 3, 2, 2, 2, 2, /* constraint */ 1, 0 }, // constraint with offset 11 day 
				new[] { 3, 3, 3, 3, 3, 3, 3, 3, 2, 2, 2, 2, /* constraint */ 0 }, // constraint with offset 12 day 
			};

			var timePerCell = (ZInt)ComponentGridHelper.GetTimePerCellForBuffer(Config.Section.SectionConfiguration, Config.Buffer).GetMinutesFromDateTimeSpan();

			for (var index = 0; index < expectedCCRHeadingZonesForVariousConstraintOffsets.Length; index++)
			{
				var constraintOffsetInDay = index;
				Config.Constraint.FC_OffsetInMinutes = constraintOffsetInDay * timePerCell;

				var viewModel = Config.ResetViewModel();
				var ccrHeaderCells = GetCCRHeadingCells(Config.SectionViewModel.ComponentGrid.Cells);

				var expectedCCRHeadingZones = expectedCCRHeadingZonesForVariousConstraintOffsets[index];
				expectedCCRHeadingZones = ModifyCCRHeadingCellZoneTest(expectedCCRHeadingZonesForVariousConstraintOffsets, index, expectedCCRHeadingZones);

				var message = string.Format("CCR Heading cell zone 2 should be in the constraint line, and zone 3 or 1 for cells not in the CCR Target. Constraint line = {0} day ({1} minutes - flow = {2})",
					constraintOffsetInDay,
					Config.Constraint.FC_OffsetInMinutes,
					Config.Section.SectionConfiguration.FlowDirection);

				AssertCCRHeadingCellZone(message, ccrHeaderCells, expectedCCRHeadingZones);
			}
		}

		void AssertCCRHeadingCellZone(string message, IEnumerable<CellContent> ccrHeadingCells, int[] expectedZones)
		{
			var actualCCRHeadingCellZones = ccrHeadingCells.Where(c => c.SecondaryAxis > 0).OrderBy(c => c.SecondaryAxis).Select(c => c.Zone.Value).ToArray();

			var flowDirectionedExpectedZones = Config.Section.GetFlowDirectionedArray(expectedZones);

			message = string.Format("{0}. Expected {1} but found {2}", message, string.Join(",", flowDirectionedExpectedZones), string.Join(",", actualCCRHeadingCellZones));
			AssertArrayEqualsByElements(message, flowDirectionedExpectedZones, actualCCRHeadingCellZones);
		}

		protected virtual int[] ModifyCCRHeadingCellZoneTest(int[][] tests, int testIndex, int[] test)
		{
			return test;
		}

		IEnumerable<CellContent> GetCCRHeadingCells(IEnumerable<CellContent> cells, ZInt? primaryAxis = null)
		{
			return cells.Where(c =>
				(primaryAxis == null || primaryAxis == c.PrimaryAxis)
				&& c.ContentType == CellContentType.CCRHeading)
			.OrderBy(c => c.SecondaryAxis);
		}

		#endregion

		#region SubComponentZoneHeading Background FadeColor

		[RequiresSTA]
		[TestDate(2013, 10, 4)]
		public void TestSubComponentZoneHeadingFade_PreCCRAging()
		{
			var preCCRWorkflow = BMSTestHelper.CreateWorkflowForTest(Factory, Config, ConstraintStatus.PreConstraint);

			var testCases = GetTestCaseBuilder().GetTestsForSubComponentZoneHeadingFade_PreCCRAging(preCCRWorkflow);
			TestSubComponentZoneHeadingFade(testCases, new[] { preCCRWorkflow });
		}

		[RequiresSTA]
		[TestDate(2013, 10, 4)]
		public void TestSubComponentZoneHeadingFade_PostCCRAging()
		{
			var postCCRWorkflow = BMSTestHelper.CreateWorkflowForTest(Factory, Config, ConstraintStatus.PostConstraint);

			var testCases = GetTestCaseBuilder().GetTestsForSubComponentZoneHeadingFade_PostCCRAging(postCCRWorkflow);
			TestSubComponentZoneHeadingFade(testCases, new[] { postCCRWorkflow });
		}

		[RequiresSTA]
		[TestDate(2013, 10, 4)]
		public void TestSubComponentZoneHeadingFade_FadeIndependently()
		{
			var preCCRWorkflow = BMSTestHelper.CreateWorkflowForTest(Factory, Config, ConstraintStatus.PreConstraint);
			var postCCRWorkflow = BMSTestHelper.CreateWorkflowForTest(Factory, Config, ConstraintStatus.PostConstraint);

			var testCases = GetTestCaseBuilder().GetTestsForSubComponentZoneHeadingFade_FadeIndependently(preCCRWorkflow, postCCRWorkflow);
			TestSubComponentZoneHeadingFade(testCases, new[] { preCCRWorkflow, postCCRWorkflow });
		}

		[RequiresSTA]
		[TestDate(2013, 10, 4)]
		public void TestSubComponentZoneHeadingFade_NonPrePostCCRAging()
		{
			WorkingDaysTestHelper.UpdateEveryDayTo9To5(Factory, Env.CurrentDepartment.PK);

			CombineAssertions("Given non pre/post ccr-workflow that age by 1 day increment, pre/post-ccr-subcomponent should not fade", () =>
			{
				for (var agingInDay = 0; agingInDay < 13; agingInDay++)
				{
					var ccrWorkflow = BMSTestHelper.CreateWorkflowAndTask(Factory, completionStatement: "ccr-wf", description: "ccr-wf: ccr-task", currentComponent: Config.Buffer, releaseDateTime: ZDateTime.UtcToday.AddDays(-agingInDay), staffCode: Config.CCR.GS_Code, lowEstMinutes: 4 * 8 * 60, sequence: 1);

					var preCCRWorkflowNotPenetratingSubComponent = BMSTestHelper.CreateWorkflowAndTask(Factory, completionStatement: "pre-ccr-wf", description: "pre-ccr-wf: ccr-task", currentComponent: Config.Bucket, releaseDateTime: ZDateTime.UtcToday.AddDays(-agingInDay), staffCode: Config.CCR.GS_Code, lowEstMinutes: 4 * 8 * 60, sequence: 2);
					BMSTestHelper.CreateTask(preCCRWorkflowNotPenetratingSubComponent, Config.NonCCR1.GS_Code, description: "pre-ccr-wf: pre-ccr-task", lowEstMinutes: 4 * 8 * 60, sequence: 1);

					var postCCRWorkflowNotPenetratingSubComponent = BMSTestHelper.CreateWorkflowAndTask(Factory, completionStatement: "post-ccr-wf", description: "post-ccr-wf: ccr-task", currentComponent: Config.Bucket, releaseDateTime: ZDateTime.UtcToday.AddDays(-agingInDay), staffCode: Config.CCR.GS_Code, lowEstMinutes: 4 * 8 * 60, sequence: 1);
					BMSTestHelper.CreateTask(postCCRWorkflowNotPenetratingSubComponent, Config.NonCCR1.GS_Code, description: "post-ccr-wf: post-ccr-task", lowEstMinutes: 4 * 8 * 60, sequence: 2);

					Config.Workflows.Add(ccrWorkflow);
					Config.Workflows.Add(preCCRWorkflowNotPenetratingSubComponent);
					Config.Workflows.Add(postCCRWorkflowNotPenetratingSubComponent);

					var viewModel = Config.SectionViewModel;

					var subComponentHeadingCells = Config.SectionViewModel.ComponentGrid.Cells.Where(c => c.ContentType == CellContentType.SubComponentZoneHeading);

					foreach (var subComponentHeadingCellsGrouped in subComponentHeadingCells.GroupBy(c => c.PrimaryAxis))
					{
						var primaryAxis = subComponentHeadingCellsGrouped.Key;

						var expected = subComponentHeadingCellsGrouped.Select(c => new VisualBoardPositionForTest<Color?>(c.PrimaryAxis, c.SecondaryAxis, Color.Empty));
						var actual = subComponentHeadingCellsGrouped.Select(c => new VisualBoardPositionForTest<Color?>(c.PrimaryAxis, c.SecondaryAxis, c.BackgroundFadeColor == null ? Color.Empty : c.BackgroundFadeColor.Value));
						BMSTestHelper.AssertColors(string.Format("given non pre/post ccr-workflow that age by {0} day, pre/post-ccr heading should not fade with BackgroundFadeColor", agingInDay),
							Config.Section,
							expected,
							actual
						);

						expected = subComponentHeadingCellsGrouped.Select(c => new VisualBoardPositionForTest<Color?>(c.PrimaryAxis, c.SecondaryAxis, Config.Section.GetZoneColor(c.Zone)));
						actual = subComponentHeadingCellsGrouped.Select(c => new VisualBoardPositionForTest<Color?>(c.PrimaryAxis, c.SecondaryAxis, c.BackColor.Value));
						BMSTestHelper.AssertColors(string.Format("given non pre/post ccr-workflow that age by {0} day, pre/post-ccr heading should not fade with backColor", agingInDay),
							Config.Section,
							expected,
							actual
						);
					}
				}
			});
		}

		#endregion

		#region Test CCR Channel

		[RequiresSTA]
		[TestDate(2013, 10, 4)]
		public void TestCCRChannelBackgroundColour_WithNonCCRReleaseGroup()
		{
			var section = Config.Section;

			Config.Buffer.ReleaseGroupLinks[0].FO_IsConstrainedMode = ZBool.False;

			// Keep thing simple, so we don't have to check 80% fade
			foreach (var task in Config.Workflows.SelectMany(w => w.Tasks))
			{
				task.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
			}

			Factory.Save();
			var viewModel = Config.ResetViewModel();
			var grid = viewModel.ComponentGrid;

			var ccrChannelPrimaryAxis = 2;
			var expectedZonesAsNumber = new int?[] { 3, 3, 3, 3, 2, 2, 2, 2, 1, 1, 1, 1, 0 };
			var expectedZones = new List<VisualBoardPositionForTest<int?>>();
			var index = 1;
			expectedZonesAsNumber.ForEach(z => expectedZones.Add(new VisualBoardPositionForTest<int?>(ccrChannelPrimaryAxis, index++, z)));

			var cCRChannelCardCells = grid.Cells.Where(c => c.PrimaryAxis == ccrChannelPrimaryAxis && c.ContentType == CellContentType.Cards);

			CombineAssertions("\r\nGIVEN ReleaseGroup is NON-CCR\r\nWHEN Channel is CCR\r\nTHEN the Channel should be treated like NON-CCR", () =>
			{
				var actualCCRHeaderZones = cCRChannelCardCells.Select(c => c.CCRHeaderZone);
				actualCCRHeaderZones.ForEach(z => AssertNull("CCR Channel's CCR Header Zones should be null", z));

				var actualSubComponentZones = cCRChannelCardCells.Select(c => c.SubComponentZones);
				actualSubComponentZones.ForEach(z => AssertEquals("CCR Channel's SubComponent Zones should not exist", false, z.Any()));

				var actualZones = cCRChannelCardCells.Select(c => new VisualBoardPositionForTest<int?>(c.PrimaryAxis, c.SecondaryAxis, c.Zone));
				actualZones.ForEach(z => z.ConvertToFlowDirectioned(section));
				expectedZones.ForEach(z => z.ConvertToFlowDirectioned(section));
				VisualBoardPositionForTest<int?>.AssertCollection("CCR Channel's Zone should be equal to primary-component-zone", expectedZones, actualZones);

				var actualBGColors = cCRChannelCardCells.Select(c => new VisualBoardPositionForTest<Color?>(c.PrimaryAxis, c.SecondaryAxis, c.BackColor));
				actualBGColors.ForEach(z => z.ConvertToFlowDirectioned(section));
				var expectedBGColorsForTest = expectedZones.Select(c => new VisualBoardPositionForTest<Color?>(c.PrimaryAxis, c.SecondaryAxis, section.GetZoneColor(c.Value)));
				expectedBGColorsForTest.ForEach(z => z.ConvertToFlowDirectioned(section));
				VisualBoardPositionForTest<Color?>.AssertCollection("CCR Channel's background color should be derived from primary-component-zone", expectedBGColorsForTest, actualBGColors);
			});
		}

		#endregion

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();

			WorkingDaysTestHelper.UpdateEveryDayTo9To5(Factory, Env.CurrentDepartment.PK);
			Config = TestConfigsHelper.CreateComplexConstrainedSchematicTestConfig(Factory, makeResourcesPartOfReleaseGroup: false); // Because these tests are highly reliant on the incorrect behaviour that CCR/non-CCR resources are not actually part of the constrained mode release group.

			CCRWorkflow = Config.Workflows[0];
			NonCCR1Workflow = Config.Workflows[1];
			NonCCR2Workflow = Config.Workflows[2];
			Factory.Save();
		}

		protected ComplexConstrainedSchematicTestConfig Config;
		protected ProcessHeader CCRWorkflow;
		protected ProcessHeader NonCCR1Workflow;
		protected ProcessHeader NonCCR2Workflow;

		#region SubComponentZoneHeading Background FadeColor

		IEnumerable<VisualBoardPositionForTest<Color?>> GetExpectedSubCmptHeadingColors(
			BMBoardSection section,
			IEnumerable<CellContent> cells,
			SubCmptHeadingFadeTest fadePositions,
			CellContentType contentType,
			bool defaultColorIsNull,
			Func<BMBoardSection, IEnumerable<CellContent>, SubCmptHeadingFadeTest, CellContentType, IEnumerable<VisualBoardPositionForTest<Color?>>> getColors)
		{
			var subComponentHeadingCells = Config.SectionViewModel.ComponentGrid.Cells.Where(c => c.ContentType == CellContentType.SubComponentZoneHeading);

			var expectedSubComponentHeadingColors = getColors(Config.Section, subComponentHeadingCells, fadePositions, contentType);

			var result = new List<VisualBoardPositionForTest<Color?>>();

			foreach (var cell in cells)
			{
				var subComponentHeadingCell = expectedSubComponentHeadingColors.FirstOrDefault(c => c.PrimaryAxis == cell.PrimaryAxis && c.SecondaryAxis == cell.SecondaryAxis);
				if (subComponentHeadingCell != null)
				{
					result.Add(subComponentHeadingCell);
					continue;
				}

				result.Add(new VisualBoardPositionForTest<Color?>(cell.PrimaryAxis, cell.SecondaryAxis, defaultColorIsNull ? null : section.GetZoneColor(cell.Zone)));
			}
			return result;
		}

		IEnumerable<VisualBoardPositionForTest<Color?>> GetExpectedBackgroundFadeColors_ForFading(BMBoardSection section, IEnumerable<CellContent> cells, SubCmptHeadingFadeTest subCmptHeadingFadeTest, CellContentType contentType)
		{
			foreach (var cell in cells.OrderBy(c => c.SecondaryAxis))
			{
				var backgroundFadeColor = new VisualBoardPositionForTest<Color?>(cell.PrimaryAxis, cell.SecondaryAxis, null);
				if (subCmptHeadingFadeTest != null)
				{
					foreach (var startFadingPosition in subCmptHeadingFadeTest.ExpectedFadePositions)
					{
						if (cell.PrimaryAxis == startFadingPosition.PrimaryAxis && cell.SecondaryAxis == startFadingPosition.SecondaryAxis)
						{
							backgroundFadeColor.Value = section.GetZoneColor(cell.Zone).FadeTowardsWhite();
							break;
						}
					}

					yield return backgroundFadeColor;
				}
			}
		}

		IEnumerable<VisualBoardPositionForTest<Color?>> GetExpectedBackColors_ForFading(BMBoardSection section, IEnumerable<CellContent> cells, SubCmptHeadingFadeTest subCmptHeadingFadeTest, CellContentType contentType)
		{
			foreach (var cell in cells.OrderBy(c => c.SecondaryAxis))
			{
				var backFadeColor = new VisualBoardPositionForTest<Color?>(cell.PrimaryAxis, cell.SecondaryAxis, section.GetZoneColor(cell.Zone));
				if (subCmptHeadingFadeTest != null)
				{
					foreach (var startFadingPosition in subCmptHeadingFadeTest.ExpectedFadePositions)
					{
						var isFade = section.SectionConfiguration.FlowsInSameDirectionAsAxis()
							? cell.SecondaryAxis > startFadingPosition.SecondaryAxis
							: cell.SecondaryAxis < startFadingPosition.SecondaryAxis;

						var cellAtPosition = cells.First(c => c.PrimaryAxis == startFadingPosition.PrimaryAxis && c.SecondaryAxis == startFadingPosition.SecondaryAxis);
						var canFade = section.SectionConfiguration.ShowChildComponentZones ?
							cell.SubComponentHeadingPK == cellAtPosition.SubComponentHeadingPK
								&& cell.Zone != null
								&& cell.PrimaryAxis == cellAtPosition.PrimaryAxis :
							cell.PrimaryAxis == cellAtPosition.PrimaryAxis;

						if (isFade && canFade)
						{
							backFadeColor.Value = section.GetZoneColor(cell.Zone).FadeTowardsWhite();
							break;
						}
					}

					yield return backFadeColor;
				}
			}
		}

		string GetAssertMessageForSubComponentZoneHeadingFadeTest(ComplexConstrainedSchematicTestConfig config, SubCmptHeadingFadeTest test, string colorType, int totalTabs = 0, bool showGridAsHTML = false)
		{
			var tabs = new string(' ', totalTabs * 4);
			var message = new ZStringBuilder();

			if (showGridAsHTML)
			{
				message.Append(string.Format("\r\n{0}Given visual board:\r\n{1}",
					tabs,
					BMSTestHelper.GetGridAsHTML(config.SectionViewModel)
				));
			}

			foreach (var workflowAging in test.WorkflowAgings)
			{
				var workflow = Factory.Load<ProcessHeader>(workflowAging.WorkflowPK);
				message.Append(string.Format("\r\n{0}When: {1}-Workflow({2}) with {3}-day age",
					tabs,
					workflow.ConstraintStatus,
					workflow.PK,
					workflowAging.AgingInDays
				));
			}

			if (test.ExpectedFadePositions.Count < 1)
			{
				message.Append(string.Format("\r\n{0}Then: SubComponent-Headings should not fade", tabs));
			}

			foreach (var expectedFadePosition in test.ExpectedFadePositions)
			{
				message.Append(string.Format("\r\n{0}Then: SubComponent-Headings should fade at {1}, {2} with {3}",
					tabs,
					expectedFadePosition != null ? expectedFadePosition.PrimaryAxis.ToString() : "null",
					expectedFadePosition != null ? expectedFadePosition.SecondaryAxis.ToString() : "null",
					colorType
				));
			}

			return message.ToString().TrimEnd('\r', '\n');
		}

		protected void TestSubComponentZoneHeadingFade(IEnumerable<SubCmptHeadingFadeTest> tests, IEnumerable<ProcessHeader> workflows)
		{
			WorkingDaysTestHelper.UpdateEveryDayTo9To5(Factory, Env.CurrentDepartment.PK);

			var className = this.GetType().Name;
			StackTrace stackTrace = new StackTrace();
			StackFrame stackFrame = stackTrace.GetFrame(1);
			var methodName = stackFrame.GetMethod().Name;

			CombineAssertions("Pre-CCR-Subcomponent-Heading fade should be independent from Post-CCR-Subcomponent-Heading fade", () =>
			{
				foreach (var test in tests)
				{
					foreach (var workflowAging in test.WorkflowAgings)
					{
						var workflow = workflows.FirstOrDefault(w => w.PK == workflowAging.WorkflowPK);
						workflow.FH_ReleaseDateTime = workflowAging.AgingInDays != null ? ZDateTime.UtcToday.AddDays(-workflowAging.AgingInDays.Value) : ZDateTime.UtcToday;
					}

					Factory.Save();
					Config.ResetViewModel();
					var viewModel = Config.SectionViewModel;

					foreach (var position in test.ExpectedFadePositions)
					{
						position.ConvertToFlowDirectioned(Config.Section);
					}

					AssertConstraint(className, methodName, test, CellContentType.SubComponentZoneHeading);
				}
			});
		}

		void AssertConstraint(string className, string methodName, SubCmptHeadingFadeTest test, CellContentType contentType)
		{
			var constraintHeadingCells = Config.SectionViewModel.ComponentGrid.Cells.Where(c => c.ContentType == contentType);
			var fadeCells = Config.SectionViewModel.ComponentGrid.Cells.Where(c => c.BackgroundFadeColor != null);

			var actualBackgroundFadeColors = constraintHeadingCells.Select(c => new VisualBoardPositionForTest<Color?>(c.PrimaryAxis, c.SecondaryAxis, c.BackgroundFadeColor)).ToList();
			var expectedBackgroundFadeColors = GetExpectedSubCmptHeadingColors(Config.Section, constraintHeadingCells, test, contentType, true, GetExpectedBackgroundFadeColors_ForFading);
			var assertMessage = string.Format("{0}.{1}: {2}", className, methodName, GetAssertMessageForSubComponentZoneHeadingFadeTest(Config, test, "BackgroundFadeColor"));
			BMSTestHelper.AssertColors(
				assertMessage,
				Config.Section,
				expectedBackgroundFadeColors,
				actualBackgroundFadeColors
			);

			var actualBackColors = constraintHeadingCells.Select(c => new VisualBoardPositionForTest<Color?>(c.PrimaryAxis, c.SecondaryAxis, c.BackColor)).ToList();
			var expectedBackColors = GetExpectedSubCmptHeadingColors(Config.Section, constraintHeadingCells, test, contentType, false, GetExpectedBackColors_ForFading);
			assertMessage = string.Format("{0}.{1}: {2}", className, methodName, GetAssertMessageForSubComponentZoneHeadingFadeTest(Config, test, "BackColor", totalTabs: 1));
			BMSTestHelper.AssertColors(
				assertMessage,
				Config.Section,
				expectedBackColors,
				actualBackColors,
				totalTabs: 1
			);
		}

		#endregion

		#endregion
	}

	#region Flow-Direction Tests

	/// <summary>
	/// https://wisetechglobal.sharepoint.com/Development/Development%20Wiki/Testing%20Visual%20Board.aspx#ComponentGridBasherFlowUpTest
	/// </summary>
	public class ComponentGridBasherFlowUpTest : ComponentGridBasherTest
	{
		protected override void SetUp()
		{
			base.SetUp();

			Config.Section.SectionConfiguration.FlowDirection = FlowDirectionList.Codes.Up;
		}
	}

	/// <summary>
	/// https://wisetechglobal.sharepoint.com/Development/Development%20Wiki/Testing%20Visual%20Board.aspx#ComponentGridBasherFlowRightTest
	/// </summary>
	public class ComponentGridBasherFlowRightTest : ComponentGridBasherTest
	{
		protected override void SetUp()
		{
			base.SetUp();

			Config.Section.SectionConfiguration.FlowDirection = FlowDirectionList.Codes.Right;
		}
	}

	/// <summary>
	/// https://wisetechglobal.sharepoint.com/Development/Development%20Wiki/Testing%20Visual%20Board.aspx#ComponentGridBasherFlowDownTest
	/// </summary>
	public class ComponentGridBasherFlowDownTest : ComponentGridBasherTest
	{
		protected override void SetUp()
		{
			base.SetUp();

			Config.Section.SectionConfiguration.FlowDirection = FlowDirectionList.Codes.Down;
		}
	}

	/// <summary>
	/// https://wisetechglobal.sharepoint.com/Development/Development%20Wiki/Testing%20Visual%20Board.aspx#ComponentGridBasherFlowLeftTest
	/// </summary>
	public class ComponentGridBasherFlowLeftTest : ComponentGridBasherTest
	{
		protected override void SetUp()
		{
			base.SetUp();

			Config.Section.SectionConfiguration.FlowDirection = FlowDirectionList.Codes.Left;
		}
	}

	#endregion

	#region Constraint Tests

	public class ComponentGridBasherMultipleConstraintLines : ComponentGridBasherTest
	{
		protected override void SetUp()
		{
			base.SetUp();

			var constraint2 = BMSTestHelper.CreateConstraint(Config.Buffer, "Constraint-2", offsetMinutes: ConstraintSecondaryAxis * 8 * 60);
			constraint2.FC_DisplaySequence = 3;
		}

		public int ConstraintSecondaryAxis = 10;

		protected override int[] ModifyCCRHeadingCellZoneTest(int[][] tests, int testIndex, int[] test)
		{
			return ConstraintSecondaryAxis < testIndex ? test = tests[ConstraintSecondaryAxis] : test;
		}
	}

	#endregion
}
