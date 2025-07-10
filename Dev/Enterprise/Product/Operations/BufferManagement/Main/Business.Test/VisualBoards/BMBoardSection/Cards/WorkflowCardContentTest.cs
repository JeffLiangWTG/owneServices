using System;
using System.Drawing;
using System.Linq;
using CargoWise.Types;
using Enterprise.BufferManagement.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.VisualBoards.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.BufferManagement.Business.Test
{
	class WorkflowCardContentTest : BMSTestCaseWithFactory
	{
		public void TestCreateWorkflowCardContent_OneTask_ShouldThrowArgumentException()
		{
			var system = BMSTestHelper.CreateSystem(Factory);
			var bucket = CreateBucket(system);
			var section = CreateBoardSection(bucket);
			section.SectionConfiguration.CardType = CardTypeList.Codes.JobLevelWorkflow;

			var jobHeader = CreateJobHeader<OrgHeader>(addDefaultProcessHeaderIfNone: false);
			var workflow = jobHeader.ProcessHeaders.AddNew();
			workflow.FH_FC_CurrentComponent = bucket.PK;

			var task1 = BMSTestHelper.CreateTask(workflow, GlbStaff.CurrentUser.GS_Code, 60);
			var viewModel = BMSTestHelper.CreateViewModel(section, BMSTestHelper.CreateBoardViewModel(section.Board));

			AssertEquals(workflow.Parent.PK, task1.P9_ParentID);

			task1.P9_FH_ProcessHeader = ZGuid.Empty;
			Factory.Save();

			AssertExceptionThrown<ArgumentNullException>(() => new WorkflowCardContent(jobHeader, workflow.GetTasksWithoutAccessingWorkflowParent().FirstOrDefault(), viewModel));
		}

		public void TestCreateWorkflowCardContent_TwoTasks_ShouldNotThrowException()
		{
			var system = BMSTestHelper.CreateSystem(Factory);
			var bucket = CreateBucket(system);
			var section = CreateBoardSection(bucket);
			section.SectionConfiguration.CardType = CardTypeList.Codes.JobLevelWorkflow;

			var jobHeader = CreateJobHeader<OrgHeader>(addDefaultProcessHeaderIfNone: false);
			var workflow = jobHeader.ProcessHeaders.AddNew();
			workflow.FH_FC_CurrentComponent = bucket.PK;

			var task1 = BMSTestHelper.CreateTask(workflow, GlbStaff.CurrentUser.GS_Code, 60);
			var task2 = BMSTestHelper.CreateTask(workflow, GlbStaff.CurrentUser.GS_Code, 60);
			var viewModel = BMSTestHelper.CreateViewModel(section, BMSTestHelper.CreateBoardViewModel(section.Board));

			AssertEquals(workflow.Parent.PK, task1.P9_ParentID);
			AssertEquals(workflow.Parent.PK, task2.P9_ParentID);

			task1.P9_FH_ProcessHeader = ZGuid.Empty;
			Factory.Save();

			AssertNoExceptionThrown(() => new WorkflowCardContent(jobHeader, task1, viewModel));
		}

		public void TestIsCurrent()
		{
			var jobHeader = CreateJobHeader<OrgHeader>(addDefaultProcessHeaderIfNone: false);
			var workflow1 = jobHeader.ProcessHeaders.AddNew();
			var workflow2 = jobHeader.ProcessHeaders.AddNew();

			workflow1.GetOrCreateDependencyLink(workflow2);

			var task1 = CreateTask(workflow1, GlbStaff.CurrentUser.GS_Code, 60);
			var task2 = CreateTask(workflow2, GlbStaff.CurrentUser.GS_Code, 60);

			var viewModel = BMSTestHelper.CreateDummyViewModel(Factory);
			var content1 = new WorkflowCardContent(workflow1, task1, viewModel);
			var content2 = new WorkflowCardContent(workflow2, task2, viewModel);

			AssertEquals(true, content1.IsCurrent);
			AssertEquals(false, content2.IsCurrent);

			task1.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;

			AssertEquals(true, content1.IsCurrent);
			AssertEquals(false, content2.IsCurrent);

			viewModel.Cache.Clear();

			AssertEquals(true, content1.IsCurrent);
			AssertEquals(true, content2.IsCurrent);

			viewModel.Cache.Clear();
			task1.P9_Status = ProcessTaskStatusCodeList.Codes.Cancelled;

			AssertEquals(true, content1.IsCurrent);
			AssertEquals(true, content2.IsCurrent);

			viewModel.Cache.Clear();
			task1.P9_Status = ProcessTaskStatusCodeList.Codes.Working;

			AssertEquals(true, content1.IsCurrent);
			AssertEquals(false, content2.IsCurrent);
		}

		public void TestGetTagColors_ShouldInheritFromWorkflowAndJobHeader()
		{
			var config = TestConfigsHelper.CreateSchematicTestConfig(Factory);

			var tagDef = BMSTestHelper.CreateTagDefinition(Factory, "GEN");
			var blueTag = BMSTestHelper.CreateTagMagnitude(tagDef, "BLU", color: Color.Blue);
			var greenTag = BMSTestHelper.CreateTagMagnitude(tagDef, "GRN", color: Color.Green);

			var jobHeader = CreateJobHeader<OrgHeader>();
			var workflow = jobHeader.ProcessHeaders[0];
			var task = CreateTask(workflow, GlbStaff.CurrentUser.GS_Code, 60);

			var section = BMSTestHelper.CreateBoardSection(config.Buffer);
			section.SectionConfiguration.CardType = CardTypeList.Codes.Workflow;

			Factory.Save();

			var viewModel = BMSTestHelper.CreateViewModel(section, workflow);
			var cardContent = new WorkflowCardContent(workflow, task, viewModel);

			AssertContainsExactElementsInAnyOrder(Array.Empty<Color>(), BMSTestHelper.GetTagColors(cardContent, viewModel));

			BMSTestHelper.AddTagAndClearCache(workflow, greenTag, viewModel, section, workflow);
			AssertContainsExactElementsInAnyOrder(new Color[] { Color.Green }, BMSTestHelper.GetTagColors(cardContent, viewModel));

			BMSTestHelper.AddTagAndClearCache(jobHeader, blueTag, viewModel, section, workflow);
			AssertContainsExactElementsInAnyOrder(new Color[] { Color.Green, Color.Blue }, BMSTestHelper.GetTagColors(cardContent, viewModel));
		}

		public void TestGetTagColors_ForJobWorkflow()
		{
			var config = TestConfigsHelper.CreateSchematicTestConfig(Factory);

			var tagDef = BMSTestHelper.CreateTagDefinition(Factory, "GEN");
			var blueTag = BMSTestHelper.CreateTagMagnitude(tagDef, "BLU", color: Color.Blue);
			var greenTag = BMSTestHelper.CreateTagMagnitude(tagDef, "GRN", color: Color.Green);

			var jobHeader = CreateJobHeader<OrgHeader>();
			var workflow1 = jobHeader.ProcessHeaders[0];
			var task = CreateTask(workflow1, GlbStaff.CurrentUser.GS_Code, 60);

			var workflow2 = jobHeader.ProcessHeaders.AddNew();
			var task2 = CreateTask(workflow2, GlbStaff.CurrentUser.GS_Code, 120);

			var section = CreateBoardSection(config.Buffer);
			section.SectionConfiguration.CardType = CardTypeList.Codes.JobLevelWorkflow;

			Factory.Save();

			var viewModel = BMSTestHelper.CreateViewModel(section, workflow1);

			var cardContent = new WorkflowCardContent(jobHeader, task, viewModel);

			BMSTestHelper.AddTagAndClearCache(jobHeader, greenTag, viewModel, section, workflow1, workflow2);
			AssertContainsExactElementsInAnyOrder(new Color[] { Color.Green }, BMSTestHelper.GetTagColors(cardContent, viewModel));

			BMSTestHelper.AddTagAndClearCache(jobHeader, blueTag, viewModel, section, workflow1, workflow2);
			AssertContainsExactElementsInAnyOrder(new Color[] { Color.Green, Color.Blue }, BMSTestHelper.GetTagColors(cardContent, viewModel));
		}

		public void TestTagColors()
		{
			var config = TestConfigsHelper.CreateSchematicTestConfig(Factory);
			var workflow = BMSTestHelper.CreateWorkflowAndParents<OrgHeader>(Factory, "workflow", config.Buffer);
			var task = BMSTestHelper.CreateTask(workflow, string.Empty, 30);

			var tagDefinition = Factory.New<TagDefinition>();
			tagDefinition.TGD_Code = "ZZZ";
			var magnitude1 = BMSTestHelper.CreateTagMagnitude(tagDefinition, "B11");
			magnitude1.Color = ColorList.NameFromColor(Color.Blue);
			var magnitude2 = BMSTestHelper.CreateTagMagnitude(tagDefinition, "B12");

			workflow.AddTag(magnitude1);
			workflow.AddTag(magnitude2);

			var section = CreateBoardSection(config.Buffer);
			section.SectionConfiguration.CardType = CardTypeList.Codes.Workflow;

			Factory.Save();

			var viewModel = BMSTestHelper.CreateViewModel(section, workflow);

			AssertArrayEqualsByElements(new[] { Color.Blue }, BMSTestHelper.GetTagColors(new WorkflowCardContent((ProcessHeader)task.ProcessHeader, task, viewModel), viewModel));
		}

		public void TestCardContent()
		{
			var config = TestConfigsHelper.CreateSchematicTestConfig(Factory);
			var workflow = BMSTestHelper.CreateWorkflowAndParents<OrgHeader>(Factory, "workflow", config.Buffer);
			var task = CreateTask(workflow, string.Empty, 0);

			var cardContent = new WorkflowCardContent(workflow, task, BMSTestHelper.CreateDummyViewModel(Factory));
			AssertEquals(ZString.Empty, cardContent.NoteText);
		}

		public void TestCardContent_NotTaskToCreateNewCardContent()
		{
			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory);
			var workflow = BMSTestHelper.CreateWorkflow(jobHeader, "Imagine unicorns had fun");

			AssertExceptionThrown<ArgumentNullException>(() => new WorkflowCardContent(workflow, null, BMSTestHelper.CreateDummyViewModel(Factory)));
		}

		#region Border

		public void TestBorder_TagOverride_TagsWithNoOverride()
		{
			var config = TestConfigsHelper.CreateSchematicTestConfig(Factory);

			var definition = BMSTestHelper.CreateTagDefinition(Factory, "MIP");
			var magnitude = BMSTestHelper.CreateTagMagnitude(definition, "MEP", "The mep magnitude.", color: Color.Blue, borderStyle: VisualBoardButtonBorderStyle.Solid);

			var workflow = BMSTestHelper.CreateWorkflowAndParents<OrgHeader>(Factory, "workflow", config.Buffer);
			workflow.AddTag(magnitude);
			var task = workflow.Parent.WorkflowItems.AddNew();

			var section = CreateBoardSection(config.Buffer);
			section.SectionConfiguration.CardType = CardTypeList.Codes.Workflow;

			Factory.Save();

			var viewModel = BMSTestHelper.CreateViewModel(section, workflow);
			var taskCard = new WorkflowCardContent(workflow, task, viewModel);

			AssertColorEquals(Color.Black, taskCard.BorderColor);
			AssertEquals(new SizedButtonBorderStyle(VisualBoardButtonBorderStyle.Solid, false), taskCard.BorderStyle);
		}

		public void TestBorder_TagOverride_Workflow_OverrideBorderColor()
		{
			var config = TestConfigsHelper.CreateSchematicTestConfig(Factory);

			var definition = BMSTestHelper.CreateTagDefinition(Factory, "DAY");
			var magnitude = BMSTestHelper.CreateTagMagnitude(definition, "MON", "Monday", color: Color.Blue);
			magnitude.ApplyColorToBorder = true;

			var workflow = BMSTestHelper.CreateWorkflowAndParents<OrgHeader>(Factory, "workflow", config.Buffer);
			workflow.AddTag(magnitude);
			var task = workflow.Parent.WorkflowItems.AddNew();

			var section = CreateBoardSection(config.Buffer);
			section.SectionConfiguration.CardType = CardTypeList.Codes.Workflow;

			Factory.Save();

			var viewModel = BMSTestHelper.CreateViewModel(section, workflow);
			var taskCard = new WorkflowCardContent(workflow, task, viewModel);

			AssertColorEquals(Color.Blue, taskCard.BorderColor);
			AssertEquals(new SizedButtonBorderStyle(VisualBoardButtonBorderStyle.Outset, false), taskCard.BorderStyle);
		}

		public void TestBorder_TagOverride_OverrideBorderColor_Priority()
		{
			var config = TestConfigsHelper.CreateSchematicTestConfig(Factory);

			var definition = BMSTestHelper.CreateTagDefinition(Factory, "DAY");
			var magnitude1 = BMSTestHelper.CreateTagMagnitude(definition, "MON", "Monday", color: Color.Blue, borderStyle: VisualBoardButtonBorderStyle.Solid);
			magnitude1.ApplyColorToBorder = true;
			magnitude1.VisualStylePriority = 6;
			var magnitude2 = BMSTestHelper.CreateTagMagnitude(definition, "TUE", "Tuesday", color: Color.Wheat);
			magnitude2.ApplyColorToBorder = true;
			magnitude2.VisualStylePriority = 200;
			var magnitude3 = BMSTestHelper.CreateTagMagnitude(definition, "WED", "Wednesday", color: Color.Red);
			magnitude3.ApplyColorToBorder = true;
			magnitude3.VisualStylePriority = 201;

			var workflow = BMSTestHelper.CreateWorkflowAndParents<OrgHeader>(Factory, "workflow", config.Buffer);
			workflow.AddTag(magnitude1);
			workflow.AddTag(magnitude2);
			workflow.AddTag(magnitude3);
			var task = workflow.Parent.WorkflowItems.AddNew();

			var section = CreateBoardSection(config.Buffer);
			section.SectionConfiguration.CardType = CardTypeList.Codes.Workflow;

			Factory.Save();

			var viewModel = BMSTestHelper.CreateViewModel(section, workflow);
			var taskCard = new WorkflowCardContent(workflow, task, viewModel);

			AssertColorEquals(Color.Red, taskCard.BorderColor);
			AssertEquals(new SizedButtonBorderStyle(VisualBoardButtonBorderStyle.Solid, false), taskCard.BorderStyle);
		}

		public void TestBorder_TagOverride_OverrideBorderColor_PrioritiesIdentical()
		{
			var config = TestConfigsHelper.CreateSchematicTestConfig(Factory);

			var definition = BMSTestHelper.CreateTagDefinition(Factory, "DAY");
			var magnitude1 = BMSTestHelper.CreateTagMagnitude(definition, "MON", "Monday", color: Color.Blue, borderStyle: VisualBoardButtonBorderStyle.Solid);
			magnitude1.ApplyColorToBorder = true;
			magnitude1.VisualStylePriority = 1;
			var magnitude2 = BMSTestHelper.CreateTagMagnitude(definition, "TUE", "Tuesday", color: Color.Wheat);
			magnitude2.ApplyColorToBorder = true;
			magnitude2.VisualStylePriority = 1;
			var magnitude3 = BMSTestHelper.CreateTagMagnitude(definition, "WED", "Wednesday", color: Color.Red);
			magnitude3.ApplyColorToBorder = true;
			magnitude3.VisualStylePriority = 1;

			var workflow = BMSTestHelper.CreateWorkflowAndParents<OrgHeader>(Factory, "workflow", config.Buffer);
			workflow.AddTag(magnitude1);
			workflow.AddTag(magnitude2);
			workflow.AddTag(magnitude3);
			var task = workflow.Parent.WorkflowItems.AddNew();

			var section = CreateBoardSection(config.Buffer);
			section.SectionConfiguration.CardType = CardTypeList.Codes.Workflow;

			Factory.Save();

			var viewModel = BMSTestHelper.CreateViewModel(section, workflow);
			var taskCard = new WorkflowCardContent(workflow, task, viewModel);

			AssertColorEquals("Tag color should be Monday, because it is earlier in the alphabet", Color.Blue, taskCard.BorderColor);
			AssertEquals(new SizedButtonBorderStyle(VisualBoardButtonBorderStyle.Solid, false), taskCard.BorderStyle);
		}

		#endregion
	}
}
