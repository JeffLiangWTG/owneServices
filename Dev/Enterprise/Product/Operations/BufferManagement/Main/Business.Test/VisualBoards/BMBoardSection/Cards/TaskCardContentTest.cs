using System.Drawing;
using System.Reflection;
using Enterprise.BufferManagement.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.VisualBoards.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.BufferManagement.Business.Test
{
	class TaskCardContentTest : BMSTestCaseWithFactory
	{
		public void TestTaskCardContent_PropertyGetters()
		{
			var config = TestConfigsHelper.CreateSchematicTestConfig(Factory);
			var workflow = CreateJobHeader<OrgHeader>().ProcessHeaders[0];
			var task = CreateTask(workflow, string.Empty, 10);
			var section = BMSTestHelper.CreateBoardSection(config.Buffer);

			Factory.Save();

			var viewModel = BMSTestHelper.CreateViewModel(section, workflow);
			var content = new TaskCardContent(task, viewModel);

			CombineAssertions(() =>
			{
				foreach (var property in content.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.FlattenHierarchy))
				{
					AssertNoExceptionThrown("Should not throw exception when calling property " + property.Name, () => property.GetValue(content));
				}
			});
		}

		public void TestGetTagColors_ShouldInheritFromWorkflowAndJobHeader()
		{
			var config = TestConfigsHelper.CreateSchematicTestConfig(Factory);

			var tagDef = BMSTestHelper.CreateTagDefinition(Factory, "GEN");
			var blueTag = BMSTestHelper.CreateTagMagnitude(tagDef, "BLU", color: Color.Blue);
			var greenTag = BMSTestHelper.CreateTagMagnitude(tagDef, "GRN", color: Color.Green);
			var orangeTag = BMSTestHelper.CreateTagMagnitude(tagDef, "ORG", color: Color.Orange);

			var jobHeader = CreateJobHeader<OrgHeader>();
			var workflow = jobHeader.ProcessHeaders[0];
			var task = CreateTask(workflow, GlbStaff.CurrentUser.GS_Code, 60);

			var section = BMSTestHelper.CreateBoardSection(config.Buffer);

			Factory.Save();

			var viewModel = BMSTestHelper.CreateViewModel(section, workflow);
			var cardContent = new TaskCardContent(task, viewModel);

			AssertContainsExactElementsInAnyOrder(System.Array.Empty<Color>(), BMSTestHelper.GetTagColors(cardContent, viewModel));

			BMSTestHelper.AddTagAndClearCache(task, orangeTag, viewModel, section, workflow);
			AssertContainsExactElementsInAnyOrder(new Color[] { Color.Orange }, BMSTestHelper.GetTagColors(cardContent, viewModel));

			BMSTestHelper.AddTagAndClearCache(workflow, greenTag, viewModel, section, workflow);
			AssertContainsExactElementsInAnyOrder(new Color[] { Color.Orange, Color.Green }, BMSTestHelper.GetTagColors(cardContent, viewModel));

			BMSTestHelper.AddTagAndClearCache(jobHeader, blueTag, viewModel, section, workflow);
			AssertContainsExactElementsInAnyOrder(new Color[] { Color.Orange, Color.Green, Color.Blue }, BMSTestHelper.GetTagColors(cardContent, viewModel));
		}

		public void TestTagMagnitudes()
		{
			var config = TestConfigsHelper.CreateSchematicTestConfig(Factory);

			var workflow = BMSTestHelper.CreateWorkflowAndParents<OrgHeader>(Factory, "workflow", config.Buffer);
			var task = BMSTestHelper.CreateTask(workflow, string.Empty, 30);

			var tagDefinition = Factory.New<TagDefinition>();
			tagDefinition.TGD_Code = "ZZZ";
			var magnitude1 = BMSTestHelper.CreateTagMagnitude(tagDefinition, "B11");
			magnitude1.Color = ColorList.NameFromColor(Color.Blue);
			var magnitude2 = BMSTestHelper.CreateTagMagnitude(tagDefinition, "B12");

			task.AddTag(magnitude1);
			task.AddTag(magnitude2);

			var section = BMSTestHelper.CreateBoardSection(config.Buffer);

			Factory.Save();

			var viewModel = BMSTestHelper.CreateViewModel(section, workflow);

			AssertArrayEqualsByElements(new[] { Color.Blue }, BMSTestHelper.GetTagColors(new TaskCardContent(task, viewModel), viewModel));
		}

		public void TestTagMagnitudes_InheritedFromWorkflow()
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

			var section = BMSTestHelper.CreateBoardSection(config.Buffer);

			Factory.Save();

			var viewModel = BMSTestHelper.CreateViewModel(section, workflow);

			AssertArrayEqualsByElements(new[] { Color.Blue }, BMSTestHelper.GetTagColors(new TaskCardContent(task, viewModel), viewModel));
		}

		public void TestTagMagnitudes_AlphabeticalOrderByDefinition()
		{
			var config = TestConfigsHelper.CreateSchematicTestConfig(Factory);

			var workflow = BMSTestHelper.CreateWorkflowAndParents<OrgHeader>(Factory, "workflow", config.Buffer);
			var task = BMSTestHelper.CreateTask(workflow, string.Empty, 30);

			var tagDefinition = Factory.New<TagDefinition>();
			tagDefinition.TGD_Code = "ZZZ";
			var magnitude1 = BMSTestHelper.CreateTagMagnitude(tagDefinition, "B11");
			magnitude1.Color = ColorList.NameFromColor(Color.Chartreuse);
			var magnitude2 = BMSTestHelper.CreateTagMagnitude(tagDefinition, "B12");
			magnitude2.Color = ColorList.NameFromColor(Color.Chocolate);

			var tagDefinition2 = Factory.New<TagDefinition>();
			tagDefinition2.TGD_Code = "ZAA";
			var magnitude3 = BMSTestHelper.CreateTagMagnitude(tagDefinition2, "C11");
			magnitude3.Color = ColorList.NameFromColor(Color.Gray);
			var magnitude4 = BMSTestHelper.CreateTagMagnitude(tagDefinition2, "C12");
			magnitude4.Color = ColorList.NameFromColor(Color.Red);

			var tagDefinition3 = Factory.New<TagDefinition>();
			tagDefinition3.TGD_Code = "ZBB";
			var magnitude5 = BMSTestHelper.CreateTagMagnitude(tagDefinition3, "D11");
			magnitude5.Color = ColorList.NameFromColor(Color.Green);
			var magnitude6 = BMSTestHelper.CreateTagMagnitude(tagDefinition3, "D12");
			magnitude6.Color = ColorList.NameFromColor(Color.CadetBlue);

			workflow.AddTag(magnitude1);
			workflow.AddTag(magnitude2);
			task.AddTag(magnitude3);
			task.AddTag(magnitude4);
			task.AddTag(magnitude5);
			task.AddTag(magnitude6);

			var section = BMSTestHelper.CreateBoardSection(config.Buffer);

			Factory.Save();

			var viewModel = BMSTestHelper.CreateViewModel(section, workflow);

			AssertArrayEqualsByElements("Colors are sorted by Visualisation Priority then by Tag Magnitude", new[] { Color.Chartreuse, Color.Chocolate, Color.Gray, Color.Red, Color.Green, Color.CadetBlue }, BMSTestHelper.GetTagColors(new TaskCardContent(task, viewModel), viewModel));
		}

		public void TestCardContent()
		{
			var job = Factory.New<OrgHeader>();
			var workflow = ProcessJobHeader.GetForParent(job, Factory).ProcessHeaders.AddNew();
			var task = job.WorkflowItems.AddNew();
			task.P9_CardNote = "Dis is urgent";

			var viewModel = BMSTestHelper.CreateDummyViewModel(Factory);
			var cardContent = new TaskCardContent(task, viewModel);
			AssertEquals("Dis is urgent", cardContent.NoteText);
		}

		public void TestIsCurrent_MultipleTasksWithSameSequence()
		{
			var system = CreateSystem("ORG");
			var workflow = ProcessJobHeader.GetForParent(Factory.New<OrgHeader>(), Factory);
			var task1 = workflow.Parent.WorkflowItems.AddNew();
			var task2 = workflow.Parent.WorkflowItems.AddNew();
			var task3 = workflow.Parent.WorkflowItems.AddNew();
			var task4 = workflow.Parent.WorkflowItems.AddNew();

			task1.P9_Status = task2.P9_Status = task3.P9_Status = ProcessTaskStatusCodeList.Codes.Assigned;
			task3.P9_Status = ProcessTaskStatusCodeList.Codes.Cancelled;

			task1.P9_Sequence = task2.P9_Sequence = task3.P9_Sequence = 1;
			task4.P9_Sequence = 2;

			var viewModel = BMSTestHelper.CreateDummyViewModel(Factory);

			var task1Card = new TaskCardContent(task1, viewModel);
			var task2Card = new TaskCardContent(task2, viewModel);
			var task3Card = new TaskCardContent(task3, viewModel);
			var task4Card = new TaskCardContent(task4, viewModel);

			CombineAssertions(() =>
			{
				AssertEquals("Task1 should be current", true, task1Card.IsCurrent);
				AssertEquals("Task2 should be current", true, task2Card.IsCurrent);
				AssertEquals("Task3 should NOT be current - it is closed", false, task3Card.IsCurrent);
				AssertEquals("Task4 should NOT be current - it has a higher sequence number", false, task4Card.IsCurrent);
			});
		}

		#region Border

		public void TestBorder_Default()
		{
			var config = TestConfigsHelper.CreateSchematicTestConfig(Factory);

			var workflow = BMSTestHelper.CreateWorkflowAndParents<OrgHeader>(Factory, "workflow", config.Buffer);
			var task = workflow.Parent.WorkflowItems.AddNew();

			var section = BMSTestHelper.CreateBoardSection(config.Buffer);

			Factory.Save();

			var viewModel = BMSTestHelper.CreateViewModel(section, workflow);
			var taskCard = new TaskCardContent(task, viewModel);

			AssertColorEquals(Color.Black, taskCard.BorderColor);
			AssertEquals(new SizedButtonBorderStyle(VisualBoardButtonBorderStyle.Outset, false), taskCard.BorderStyle);
		}

		public void TestBorder_TagOverride_TagsWithNoOverride()
		{
			var config = TestConfigsHelper.CreateSchematicTestConfig(Factory);

			var definition = BMSTestHelper.CreateTagDefinition(Factory, "MIP");
			var magnitude = BMSTestHelper.CreateTagMagnitude(definition, "MEP", "The mep magnitude.", color: Color.Blue, borderStyle: VisualBoardButtonBorderStyle.Solid);

			var workflow = BMSTestHelper.CreateWorkflowAndParents<OrgHeader>(Factory, "workflow", config.Buffer);
			workflow.AddTag(magnitude);
			var task = workflow.Parent.WorkflowItems.AddNew();

			var section = BMSTestHelper.CreateBoardSection(config.Buffer);

			Factory.Save();

			var viewModel = BMSTestHelper.CreateViewModel(section, workflow);
			var taskCard = new TaskCardContent(task, viewModel);

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

			var section = BMSTestHelper.CreateBoardSection(config.Buffer);

			Factory.Save();

			var viewModel = BMSTestHelper.CreateViewModel(section, workflow);
			var taskCard = new TaskCardContent(task, viewModel);

			AssertColorEquals(Color.Blue, taskCard.BorderColor);
			AssertEquals(new SizedButtonBorderStyle(VisualBoardButtonBorderStyle.Outset, false), taskCard.BorderStyle);
		}

		public void TestBorder_TagOverride_Task_OverrideBorderColor()
		{
			var config = TestConfigsHelper.CreateSchematicTestConfig(Factory);

			var definition = BMSTestHelper.CreateTagDefinition(Factory, "DAY");
			var magnitude = BMSTestHelper.CreateTagMagnitude(definition, "MON", "Monday", color: Color.Blue);
			magnitude.ApplyColorToBorder = true;

			var workflow = BMSTestHelper.CreateWorkflowAndParents<OrgHeader>(Factory, "workflow", config.Buffer);
			var task = workflow.Parent.WorkflowItems.AddNew();
			task.AddTag(magnitude);

			var section = BMSTestHelper.CreateBoardSection(config.Buffer);

			Factory.Save();

			var viewModel = BMSTestHelper.CreateViewModel(section, workflow);
			var taskCard = new TaskCardContent(task, viewModel);

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

			var section = BMSTestHelper.CreateBoardSection(config.Buffer);

			Factory.Save();

			var viewModel = BMSTestHelper.CreateViewModel(section, workflow);
			var taskCard = new TaskCardContent(task, viewModel);

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

			var section = BMSTestHelper.CreateBoardSection(config.Buffer);

			Factory.Save();

			var viewModel = BMSTestHelper.CreateViewModel(section, workflow);
			var taskCard = new TaskCardContent(task, viewModel);

			AssertColorEquals("Tag color should be Monday, because it is earlier in the alphabet", Color.Blue, taskCard.BorderColor);
			AssertEquals(new SizedButtonBorderStyle(VisualBoardButtonBorderStyle.Solid, false), taskCard.BorderStyle);
		}

		#endregion
	}
}
