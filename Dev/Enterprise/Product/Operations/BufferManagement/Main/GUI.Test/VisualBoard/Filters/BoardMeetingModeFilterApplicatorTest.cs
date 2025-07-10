using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Types;
using Enterprise.BufferManagement.Business;
using Enterprise.BufferManagement.Business.Test;
using Enterprise.MasterFiles.Business;
using Enterprise.VisualBoards.Business;
using Enterprise.VisualBoards.GUI;
using Enterprise.VisualBoards.GUI.Test;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.BufferManagement.GUI.Test
{
	#region Board Filter test case

	[TestedType(typeof(BoardMeetingModeFilterApplicator))]
	class BoardMeetingModeFilterApplicatorTest : TaskVisibilityFilterApplicatorTestCase<BoardMeetingModeFilterApplicator>
	{
		public override void TestApply_DbHits()
		{
			var config = TestConfigsHelper.CreateSchematicTestConfig(Factory);
			var jobHeader = CreateJobHeader<OrgHeader>(false);
			CreateTask(CreateWorkflow(jobHeader, "workflow1"), "", 60);
			CreateTask(CreateWorkflow(jobHeader, "workflow2"), "", 60);
			CreateTask(CreateWorkflow(jobHeader, "workflow3"), "", 60);
			CreateTask(CreateWorkflow(jobHeader, "workflow4"), "", 60);

			var cells = new[]
			{
				 new CellContent(0, 0, CellContentType.Cards),
				 new CellContent(0, 0, CellContentType.Cards),
				 new CellContent(0, 0, CellContentType.Cards),
				 new CellContent(0, 0, CellContentType.Cards),
			};

			var viewModel = BMSTestHelper.CreateSectionAndViewModel(config.System).Item2;

			Factory.Save();
			var newFactory = Factory.CreateNewFactory();
			var loadedJobHeader = newFactory.Load<ProcessJobHeader>(jobHeader.PK);

			var applicator = GetFilter();
			foreach (var pair in cells.Zip(loadedJobHeader.ProcessHeaders, (c, w) => new { Card = new WorkflowCardContent(w, w.GetTasksWithoutAccessingWorkflowParent().First(), viewModel), Cell = c }))
			{
				applicator.IsApplicable(pair.Card, pair.Cell, null);
			}

			var moreDbHitsAllowed = new Dictionary<string, int>
			{
				{ BMComponentSchema.Constants.TableName, 1 },
				{ OrgHeaderSchema.Constants.TableName, 1 },
				{ ProcessHeaderSchema.Constants.TableName, 2 },
				{ ProcessTasksSchema.Constants.TableName, 1 },
			};

			AssertDbHits(moreDbHitsAllowed, newFactory);
		}

		public void TestIsBoardMeetingApplicable()
		{
			var cell = new CellContent(0, 0, CellContentType.Cards);
			AssertEquals(false, BoardMeetingModeFilterApplicator.ShouldCellBeOnBoardMeetingAgenda(cell));

			cell.Zone = 0;
			AssertEquals(true, BoardMeetingModeFilterApplicator.ShouldCellBeOnBoardMeetingAgenda(cell));

			cell.Zone = 3;
			AssertEquals(false, BoardMeetingModeFilterApplicator.ShouldCellBeOnBoardMeetingAgenda(cell));

			cell.Zone = 1;
			AssertEquals(true, BoardMeetingModeFilterApplicator.ShouldCellBeOnBoardMeetingAgenda(cell));

			cell.Zone = 2;
			AssertEquals(false, BoardMeetingModeFilterApplicator.ShouldCellBeOnBoardMeetingAgenda(cell));

			cell.IsLastAgeIndexForZone = true;
			AssertEquals(true, BoardMeetingModeFilterApplicator.ShouldCellBeOnBoardMeetingAgenda(cell));
		}

		public override void TestIsApplicable()
		{
			var config = TestConfigsHelper.CreateSchematicTestConfig(Factory);
			var jobHeader = CreateJobHeader<OrgHeader>(addDefaultProcessHeaderIfNone: false);
			var workflow = CreateWorkflow(jobHeader, "workflow", currentComponent: config.Buffer);
			var task = BMSTestHelper.CreateTask(workflow);
			var viewModel = BMSTestHelper.CreateSectionAndViewModel(config.System).Item2;

			var cell = new CellContent(0, 0, CellContentType.Cards) { Zone = 0 };
			Assert(BoardMeetingModeFilterApplicator.ShouldCellBeOnBoardMeetingAgenda(cell));

			var applicator = GetFilter();
			AssertEquals("Cell is in zone 0 - it should be visible", true, applicator.IsApplicable(task, cell, viewModel));

			cell.Zone = 3;
			AssertEquals("Cell is in zone 3 - it should NOT be visible", false, applicator.IsApplicable(task, cell, viewModel));
		}

		public void TestIsApplicable_QualityIterationWorkflowInZone0_WithCCR()
		{
			var config = TestConfigsHelper.CreateConstrainedSchematicTestConfig(Factory);
			var section = BMSTestHelper.CreateBoardSection(config.Buffer);

			var jobHeader = CreateJobHeader<OrgHeader>(addDefaultProcessHeaderIfNone: false);
			var workflow = CreateWorkflow(jobHeader, "workflow", currentComponent: config.Buffer);
			var qcbWorkflow = CreateWorkflow(jobHeader, "workflow (Quality Iteration)", currentComponent: config.Buffer);

			workflow.FH_ReleaseDateTime = ZDateTime.UtcNow - TimeSpan.FromDays(20);
			qcbWorkflow.GetOrCreateLinkToParent(workflow);
			qcbWorkflow.SynchroniseBufferPenetration = true;

			var preCcrTask = BMSTestHelper.CreateTask(qcbWorkflow, staffCode: config.NonCCR1.GS_Code, sequence: 1);
			var ccrTask = BMSTestHelper.CreateTask(qcbWorkflow, staffCode: config.CCR.GS_Code, sequence: 2);

			var cell = new CellContent(0, 0, CellContentType.Cards) { Zone = 0 };
			Assert(BoardMeetingModeFilterApplicator.ShouldCellBeOnBoardMeetingAgenda(cell));

			Factory.Save();
			var viewModel = BMSTestHelper.CreateViewModel(section);

			var applicator = GetFilter();
			AssertEquals("Cell is in zone 0 - it should be visible", true, applicator.IsApplicable(preCcrTask, cell, viewModel));
			AssertEquals("Should not be visible as it is not current for CCR resource", false, applicator.IsApplicable(ccrTask, cell, viewModel));
		}

		public void TestIsApplicable_QualityIterationWorkflowInZone0_WithoutCCR()
		{
			var config = TestConfigsHelper.CreateSchematicTestConfig(Factory);
			var jobHeader = CreateJobHeader<OrgHeader>(addDefaultProcessHeaderIfNone: false);
			var workflow = CreateWorkflow(jobHeader, "workflow", currentComponent: config.Buffer);
			var qcbWorkflow = CreateWorkflow(jobHeader, "workflow (Quality Iteration)", currentComponent: config.Buffer);

			workflow.FH_ReleaseDateTime = ZDateTime.UtcNow - TimeSpan.FromDays(20);
			qcbWorkflow.GetOrCreateLinkToParent(workflow);
			qcbWorkflow.SynchroniseBufferPenetration = true;

			var task1 = BMSTestHelper.CreateTask(qcbWorkflow);
			var task2 = BMSTestHelper.CreateTask(qcbWorkflow);
			var viewModel = BMSTestHelper.CreateSectionAndViewModel(config.System).Item2;

			var cell = new CellContent(0, 0, CellContentType.Cards) { Zone = 0 };
			Assert(BoardMeetingModeFilterApplicator.ShouldCellBeOnBoardMeetingAgenda(cell));

			var applicator = GetFilter();
			AssertEquals("Cell is in zone 0 - it should be visible", true, applicator.IsApplicable(task1, cell, viewModel));
			AssertEquals("Cell is in zone 0 - it should be visible", true, applicator.IsApplicable(task2, cell, viewModel));
		}

		public void TestIsApplicable_WhenNotInBuffer()
		{
			var config = TestConfigsHelper.CreateSchematicTestConfig(Factory);
			var jobHeader = CreateJobHeader<OrgHeader>(addDefaultProcessHeaderIfNone: false);
			var workflow1 = CreateWorkflow(jobHeader, "workflow1", currentComponent: config.Bucket);
			var workflow2 = CreateWorkflow(jobHeader, "workflow2", currentComponent: config.Buffer);
			var task1 = BMSTestHelper.CreateTask(workflow1);
			var task2 = BMSTestHelper.CreateTask(workflow2);
			var viewModel = BMSTestHelper.CreateSectionAndViewModel(config.System).Item2;

			var applicator = GetFilter();
			var cell = new CellContent(0, 0, CellContentType.Cards) { Zone = 0 };

			AssertEquals("Workflow in a bucket is not relevant, but shouldn't be hidden since nothing in a bucket should specifically be emphasised during board meetings", true, applicator.IsApplicable(task1, cell, viewModel));
			AssertEquals("Workflow in a buffer should be relevant", true, applicator.IsApplicable(task2, cell, viewModel));
		}

		protected override IEnumerable<KeyValuePair<string, int>> GetExpectedDbHits()
		{
			yield return new KeyValuePair<string, int>(ProcessTasksSchema.Constants.TableName, 2);
			yield return new KeyValuePair<string, int>(BMComponentResourceLinkSchema.Constants.TableName, 0);
			yield return new KeyValuePair<string, int>(BMComponentSchema.Constants.TableName, 0);
			yield return new KeyValuePair<string, int>(ProcessHeaderSchema.Constants.TableName, 1);
		}

		protected override BoardMeetingModeFilterApplicator GetFilter()
		{
			return new BoardMeetingModeFilterApplicator(new BoardMeetingModeFilter());
		}
	}

	#endregion

	#region Filter Applicator test case

	[TestedType(typeof(BoardMeetingModeFilterApplicator))]
	class BoardMeetingModeFilterApplicatorBoardFilterTest : BoardFilterTestCase<BoardMeetingModeFilterApplicator>
	{
		public override void TestFilterName()
		{
			AssertEquals("Board Meeting", GetFilter().FilterName);
		}

		public override void TestAllowMultiple()
		{
			AssertEquals(false, GetFilter().AllowMultiple);
		}

		public override void TestEquals()
		{
			Assert("This test is not relevant for the applicator - it's never added to a FilterManager", true);
		}

		protected override BoardMeetingModeFilterApplicator GetFilter()
		{
			return new BoardMeetingModeFilterApplicator(new BoardMeetingModeFilter());
		}

		public override void TestWhenRemovedThroughFilterManager_ShouldRestoreVisualState()
		{
			var config = TestConfigsHelper.CreateSchematicTestConfig(Factory);
			var section = BMSTestHelper.CreateBoardSection(config.Buffer);

			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false);
			var workflow = BMSTestHelper.CreateWorkflow(jobHeader, "workflow", config.Buffer);
			var task = BMSTestHelper.CreateTask(workflow);

			Factory.Save();

			using (BMSTestCaseWithFactory.DisableAsyncBehaviour())
			using (var form = new VisualBoardForm(VisualBoardFormBasherTest.GetViewModel(section.Board)))
			{
				form.Show();
				Application.DoEvents();

				var taskCard = form.FindAll<TaskCardControl>().Single();
				AssertEquals(true, taskCard.Visible);

				form.EnterBoardMeetingMode();
				taskCard = form.FindAll<TaskCardControl>().SingleOrDefault();
				AssertNull(taskCard);

				form.SlideShowViewModel.FilterManager.Clear();
				taskCard = form.FindAll<TaskCardControl>().Single();
				AssertEquals(true, taskCard.Visible);
			}
		}
	}

	#endregion

	#region BoardMeetingModeFilterTest

	[TestDate(2015, 5, 15, 12, 30, 0)]
	public class BoardMeetingModeFilterTest : BMSTestCaseWithFactory
	{
		public void TestRiskFilterVisibility_NoPreConstraint()
		{
			task1.P9_GS_NKAssignedStaffMember = resource1.GS_Code;
			task2.P9_GS_NKAssignedStaffMember = resource2.GS_Code;
			task3.P9_GS_NKAssignedStaffMember = resource1.GS_Code;
			resource2.DesignateAsCCR(config.Buffer);
			RefreshCache();

			config.PreConstraintBuffer.Delete();
			Factory.Save();

			SetValuesAndAssertVisibility(workflowPosition: preCCR, bufferPosition: preConstraint, zone: 3, shouldBeVisibile: true);
			SetValuesAndAssertVisibility(workflowPosition: preCCR, bufferPosition: preConstraint, zone: 2, shouldBeVisibile: true);
			SetValuesAndAssertVisibility(workflowPosition: preCCR, bufferPosition: preConstraint, zone: 1, shouldBeVisibile: true);
			SetValuesAndAssertVisibility(workflowPosition: preCCR, bufferPosition: preConstraint, zone: 0, shouldBeVisibile: true);

			SetValuesAndAssertVisibility(workflowPosition: postCCR, bufferPosition: preConstraint, zone: 3, shouldBeVisibile: false);
			SetValuesAndAssertVisibility(workflowPosition: postCCR, bufferPosition: preConstraint, zone: 2, shouldBeVisibile: false);
			SetValuesAndAssertVisibility(workflowPosition: postCCR, bufferPosition: preConstraint, zone: 1, shouldBeVisibile: false);
			SetValuesAndAssertVisibility(workflowPosition: postCCR, bufferPosition: preConstraint, zone: 0, shouldBeVisibile: false);

			SetValuesAndAssertVisibility(workflowPosition: preCCR, bufferPosition: postConstraint, zone: 3, shouldBeVisibile: true);
			SetValuesAndAssertVisibility(workflowPosition: preCCR, bufferPosition: postConstraint, zone: 2, shouldBeVisibile: true);
			SetValuesAndAssertVisibility(workflowPosition: preCCR, bufferPosition: postConstraint, zone: 1, shouldBeVisibile: true);
			SetValuesAndAssertVisibility(workflowPosition: preCCR, bufferPosition: postConstraint, zone: 0, shouldBeVisibile: true);

			SetValuesAndAssertVisibility(workflowPosition: postCCR, bufferPosition: postConstraint, zone: 3, shouldBeVisibile: false);
			SetValuesAndAssertVisibility(workflowPosition: postCCR, bufferPosition: postConstraint, zone: 2, shouldBeVisibile: false);
			SetValuesAndAssertVisibility(workflowPosition: postCCR, bufferPosition: postConstraint, zone: 1, shouldBeVisibile: true);
			SetValuesAndAssertVisibility(workflowPosition: postCCR, bufferPosition: postConstraint, zone: 0, shouldBeVisibile: true);
		}

		public void TestRiskFilterVisibility_NoPostConstraint()
		{
			task1.P9_GS_NKAssignedStaffMember = resource1.GS_Code;
			task2.P9_GS_NKAssignedStaffMember = resource2.GS_Code;
			task3.P9_GS_NKAssignedStaffMember = resource1.GS_Code;
			resource2.DesignateAsCCR(config.Buffer);
			RefreshCache();

			config.PostConstraintBuffer.Delete();
			Factory.Save();

			SetValuesAndAssertVisibility(workflowPosition: preCCR, bufferPosition: preConstraint, zone: 3, shouldBeVisibile: false);
			SetValuesAndAssertVisibility(workflowPosition: preCCR, bufferPosition: preConstraint, zone: 2, shouldBeVisibile: false);
			SetValuesAndAssertVisibility(workflowPosition: preCCR, bufferPosition: preConstraint, zone: 1, shouldBeVisibile: true);
			SetValuesAndAssertVisibility(workflowPosition: preCCR, bufferPosition: preConstraint, zone: 0, shouldBeVisibile: true);

			SetValuesAndAssertVisibility(workflowPosition: postCCR, bufferPosition: preConstraint, zone: 3, shouldBeVisibile: true);
			SetValuesAndAssertVisibility(workflowPosition: postCCR, bufferPosition: preConstraint, zone: 2, shouldBeVisibile: true);
			SetValuesAndAssertVisibility(workflowPosition: postCCR, bufferPosition: preConstraint, zone: 1, shouldBeVisibile: true);
			SetValuesAndAssertVisibility(workflowPosition: postCCR, bufferPosition: preConstraint, zone: 0, shouldBeVisibile: true);

			SetValuesAndAssertVisibility(workflowPosition: preCCR, bufferPosition: postConstraint, zone: 3, shouldBeVisibile: true);
			SetValuesAndAssertVisibility(workflowPosition: preCCR, bufferPosition: postConstraint, zone: 2, shouldBeVisibile: true);
			SetValuesAndAssertVisibility(workflowPosition: preCCR, bufferPosition: postConstraint, zone: 1, shouldBeVisibile: true);
			SetValuesAndAssertVisibility(workflowPosition: preCCR, bufferPosition: postConstraint, zone: 0, shouldBeVisibile: true);

			SetValuesAndAssertVisibility(workflowPosition: postCCR, bufferPosition: postConstraint, zone: 3, shouldBeVisibile: true);
			SetValuesAndAssertVisibility(workflowPosition: postCCR, bufferPosition: postConstraint, zone: 2, shouldBeVisibile: true);
			SetValuesAndAssertVisibility(workflowPosition: postCCR, bufferPosition: postConstraint, zone: 1, shouldBeVisibile: true);
			SetValuesAndAssertVisibility(workflowPosition: postCCR, bufferPosition: postConstraint, zone: 0, shouldBeVisibile: true);
		}

		public void TestRiskFilterVisibility_NoConstraint()
		{
			task1.P9_GS_NKAssignedStaffMember = resource1.GS_Code;
			task2.P9_GS_NKAssignedStaffMember = resource2.GS_Code;
			task3.P9_GS_NKAssignedStaffMember = resource1.GS_Code;
			resource2.DesignateAsCCR(config.Buffer);
			RefreshCache();

			config.Constraint.Delete();
			Factory.Save();

			SetValuesAndAssertVisibility(workflowPosition: preCCR, bufferPosition: preConstraint, zone: 3, shouldBeVisibile: true);
			SetValuesAndAssertVisibility(workflowPosition: preCCR, bufferPosition: preConstraint, zone: 2, shouldBeVisibile: true);
			SetValuesAndAssertVisibility(workflowPosition: preCCR, bufferPosition: preConstraint, zone: 1, shouldBeVisibile: true);
			SetValuesAndAssertVisibility(workflowPosition: preCCR, bufferPosition: preConstraint, zone: 0, shouldBeVisibile: true);

			SetValuesAndAssertVisibility(workflowPosition: postCCR, bufferPosition: preConstraint, zone: 3, shouldBeVisibile: true);
			SetValuesAndAssertVisibility(workflowPosition: postCCR, bufferPosition: preConstraint, zone: 2, shouldBeVisibile: true);
			SetValuesAndAssertVisibility(workflowPosition: postCCR, bufferPosition: preConstraint, zone: 1, shouldBeVisibile: true);
			SetValuesAndAssertVisibility(workflowPosition: postCCR, bufferPosition: preConstraint, zone: 0, shouldBeVisibile: true);

			SetValuesAndAssertVisibility(workflowPosition: preCCR, bufferPosition: postConstraint, zone: 3, shouldBeVisibile: true);
			SetValuesAndAssertVisibility(workflowPosition: preCCR, bufferPosition: postConstraint, zone: 2, shouldBeVisibile: true);
			SetValuesAndAssertVisibility(workflowPosition: preCCR, bufferPosition: postConstraint, zone: 1, shouldBeVisibile: true);
			SetValuesAndAssertVisibility(workflowPosition: preCCR, bufferPosition: postConstraint, zone: 0, shouldBeVisibile: true);

			SetValuesAndAssertVisibility(workflowPosition: postCCR, bufferPosition: postConstraint, zone: 3, shouldBeVisibile: true);
			SetValuesAndAssertVisibility(workflowPosition: postCCR, bufferPosition: postConstraint, zone: 2, shouldBeVisibile: true);
			SetValuesAndAssertVisibility(workflowPosition: postCCR, bufferPosition: postConstraint, zone: 1, shouldBeVisibile: true);
			SetValuesAndAssertVisibility(workflowPosition: postCCR, bufferPosition: postConstraint, zone: 0, shouldBeVisibile: true);
		}

		public void TestRiskFilterVisibility_ConstrainedMode_NonCCRChannels_PreConstraint()
		{
			task1.P9_GS_NKAssignedStaffMember = resource1.GS_Code;
			task2.P9_GS_NKAssignedStaffMember = resource2.GS_Code;
			task3.P9_GS_NKAssignedStaffMember = resource1.GS_Code;
			resource2.DesignateAsCCR(config.Buffer);
			RefreshCache();

			SetValuesAndAssertVisibility(workflowPosition: preCCR, bufferPosition: preConstraint, zone: 3, shouldBeVisibile: false);
			SetValuesAndAssertVisibility(workflowPosition: preCCR, bufferPosition: preConstraint, zone: 2, shouldBeVisibile: false);
			SetValuesAndAssertVisibility(workflowPosition: preCCR, bufferPosition: preConstraint, zone: 1, shouldBeVisibile: true);
			SetValuesAndAssertVisibility(workflowPosition: preCCR, bufferPosition: preConstraint, zone: 0, shouldBeVisibile: true);

			SetValuesAndAssertVisibility(workflowPosition: postCCR, bufferPosition: preConstraint, zone: 3, shouldBeVisibile: false);
			SetValuesAndAssertVisibility(workflowPosition: postCCR, bufferPosition: preConstraint, zone: 2, shouldBeVisibile: false);
			SetValuesAndAssertVisibility(workflowPosition: postCCR, bufferPosition: preConstraint, zone: 1, shouldBeVisibile: false);
			SetValuesAndAssertVisibility(workflowPosition: postCCR, bufferPosition: preConstraint, zone: 0, shouldBeVisibile: false);
		}

		public void TestRiskFilterVisibility_ConstrainedMode_NonCCRChannels_PostConstraint()
		{
			task1.P9_GS_NKAssignedStaffMember = resource1.GS_Code;
			task2.P9_GS_NKAssignedStaffMember = resource2.GS_Code;
			task3.P9_GS_NKAssignedStaffMember = resource1.GS_Code;
			resource2.DesignateAsCCR(config.Buffer);
			RefreshCache();

			SetValuesAndAssertVisibility(workflowPosition: preCCR, bufferPosition: postConstraint, zone: 3, shouldBeVisibile: true);
			SetValuesAndAssertVisibility(workflowPosition: preCCR, bufferPosition: postConstraint, zone: 2, shouldBeVisibile: true);
			SetValuesAndAssertVisibility(workflowPosition: preCCR, bufferPosition: postConstraint, zone: 1, shouldBeVisibile: true);
			SetValuesAndAssertVisibility(workflowPosition: preCCR, bufferPosition: postConstraint, zone: 0, shouldBeVisibile: true);

			SetValuesAndAssertVisibility(workflowPosition: postCCR, bufferPosition: postConstraint, zone: 3, shouldBeVisibile: false);
			SetValuesAndAssertVisibility(workflowPosition: postCCR, bufferPosition: postConstraint, zone: 2, shouldBeVisibile: false);
			SetValuesAndAssertVisibility(workflowPosition: postCCR, bufferPosition: postConstraint, zone: 1, shouldBeVisibile: true);
			SetValuesAndAssertVisibility(workflowPosition: postCCR, bufferPosition: postConstraint, zone: 0, shouldBeVisibile: true);
		}

		public void TestRiskFilterVisibility_CCR()
		{
			task1.P9_GS_NKAssignedStaffMember = resource1.GS_Code;
			task2.P9_GS_NKAssignedStaffMember = resource2.GS_Code;
			task3.P9_GS_NKAssignedStaffMember = resource1.GS_Code;
			resource2.DesignateAsCCR(config.Buffer);

			task1.P9_Status = ProcessTaskStatusCodeList.Codes.Working;
			RefreshCache();
			AssertVisibility(false, task1);
			AssertVisibility(false, task2);
			AssertVisibility(false, task3);

			task1.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
			RefreshCache();
			AssertVisibility(false, task1);
			AssertVisibility(true, task2);
			AssertVisibility(false, task3);

			task1.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
			task2.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
			RefreshCache();
			AssertVisibility(false, task1);
			AssertVisibility(false, task2);
			AssertVisibility(false, task3);
		}

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();

			config = TestConfigsHelper.CreateConstrainedSchematicTestConfig(Factory);
			var tuple = BMSTestHelper.CreateSectionAndViewModel(config.Buffer);

			section = tuple.Item1;
			sectionViewModel = tuple.Item2;
			config.PreConstraintBuffer.FC_BufferTimespanInMinutes = 50;
			config.PostConstraintBuffer.FC_DisplaySequence = 1;

			config.Constraint.FC_OffsetInMinutes = 50;
			config.Constraint.FC_DisplaySequence = 2;

			config.PostConstraintBuffer.FC_OffsetInMinutes = 50;
			config.PostConstraintBuffer.FC_BufferTimespanInMinutes = 50;

			ConstrainedModeHelper.SwitchToConstrainedMode(config.ReleaseGroup, config.Buffer.PK);

			resource1 = BMSTestHelper.CreateStaffInCurrentBranchDept(Factory);
			resource1.Groups.Add(config.ReleaseGroup);
			resource2 = BMSTestHelper.CreateStaffInCurrentBranchDept(Factory);
			resource2.Groups.Add(config.ReleaseGroup);

			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory);
			workflow = BMSTestHelper.CreateWorkflow(jobHeader, "Z Workflow", config.Buffer);
			task1 = BMSTestHelper.CreateTask(workflow, "", 0, sequence: 1);
			task2 = BMSTestHelper.CreateTask(workflow, "", 0, sequence: 2);
			task3 = BMSTestHelper.CreateTask(workflow, "", 0, sequence: 3);

			Factory.Save();
		}

		ConstrainedSchematicTestConfig config;
		GlbStaff resource1;
		GlbStaff resource2;
		ProcessHeader workflow;
		ProcessTask task1;
		ProcessTask task2;
		ProcessTask task3;
		BMBoardSection section;
		BMBoardSectionViewModel sectionViewModel;

		void SetValuesAndAssertVisibility(ConstraintStatus workflowPosition, ConstraintStatus bufferPosition, int zone, bool shouldBeVisibile)
		{
			SetValues(workflowPosition, bufferPosition, zone);
			AssertVisibility(shouldBeVisibile);
		}

		void SetValues(ConstraintStatus workflowPosition, ConstraintStatus bufferPosition, int zone)
		{
			switch (workflowPosition)
			{
				case ConstraintStatus.PreConstraint:
					task1.P9_Status = ProcessTaskStatusCodeList.Codes.Assigned;
					task2.P9_Status = ProcessTaskStatusCodeList.Codes.Assigned;
					task3.P9_Status = ProcessTaskStatusCodeList.Codes.Assigned;
					break;
				case ConstraintStatus.ReadyForConstraint:
					task1.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
					task2.P9_Status = ProcessTaskStatusCodeList.Codes.Assigned;
					task3.P9_Status = ProcessTaskStatusCodeList.Codes.Assigned;
					break;
				case ConstraintStatus.PostConstraint:
					task1.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
					task2.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
					task3.P9_Status = ProcessTaskStatusCodeList.Codes.Assigned;
					break;
				default:
					throw new NotSupportedException();
			}

			switch (bufferPosition)
			{
				case ConstraintStatus.PreConstraint:
					switch (zone)
					{
						case 0:
							workflow.FH_ReleaseDateTime = ZDateTime.UtcNow - TimeSpan.FromMinutes(50);
							break;
						case 1:
							workflow.FH_ReleaseDateTime = ZDateTime.UtcNow - TimeSpan.FromMinutes(40);
							break;
						case 2:
							workflow.FH_ReleaseDateTime = ZDateTime.UtcNow - TimeSpan.FromMinutes(20);
							break;
						case 3:
							break;
						default:
							throw new NotSupportedException();
					}
					break;
				case ConstraintStatus.PostConstraint:
					switch (zone)
					{
						case 0:
							workflow.FH_ReleaseDateTime = ZDateTime.UtcNow - TimeSpan.FromMinutes(100);
							break;
						case 1:
							workflow.FH_ReleaseDateTime = ZDateTime.UtcNow - TimeSpan.FromMinutes(90);
							break;
						case 2:
							workflow.FH_ReleaseDateTime = ZDateTime.UtcNow - TimeSpan.FromMinutes(70);
							break;
						case 3:
							workflow.FH_ReleaseDateTime = ZDateTime.UtcNow - TimeSpan.FromMinutes(60);
							break;
						default:
							throw new NotSupportedException();
					}
					break;
				default:
					throw new NotSupportedException();
			}
			Factory.Save();
		}

		void AssertVisibility(bool expectedVisibility, ProcessTask task = null)
		{
			var taskToCheck = task ?? workflow.CurrentTasks.Single();
			var cardContent = new TaskCardContent(taskToCheck, sectionViewModel);
			AssertEquals(expectedVisibility, BoardMeetingModeFilterApplicator.ShouldTaskBeOnBoardMeetingAgenda(Factory, cardContent, new CellContent(0, 0, CellContentType.Cards), sectionViewModel));
		}

		const ConstraintStatus preCCR = ConstraintStatus.PreConstraint;
		const ConstraintStatus postCCR = ConstraintStatus.PostConstraint;

		const ConstraintStatus preConstraint = ConstraintStatus.PreConstraint;
		const ConstraintStatus postConstraint = ConstraintStatus.PostConstraint;

		void RefreshCache()
		{
			sectionViewModel = BMSTestHelper.CreateViewModel(section);
			Factory.Save();
			BMBoardSectionViewModel.CreateAndPopulatePropertyCache(TaskChannelMap.ForTest(section, sectionViewModel, workflow), sectionViewModel);
		}

		#endregion
	}

	#endregion
}
