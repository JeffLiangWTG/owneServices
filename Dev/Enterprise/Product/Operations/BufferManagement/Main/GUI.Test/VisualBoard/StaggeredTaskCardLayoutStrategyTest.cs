using System.Linq;
using Enterprise.BufferManagement.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.BufferManagement.GUI.Test
{
	public class StaggeredTaskCardLayoutStrategyTest : TaskPanelLayoutStrategyTest
	{
		protected override string PanelLayoutStyle => PanelLayoutTypeList.Codes.Staggered;

		protected override void AssertCardPlacementIsCorrect()
		{
			base.AssertCardPlacementIsCorrect();
			AssertCardOffsetsAreCorrect();
		}

		void AssertCardOffsetsAreCorrect()
		{
			AssertHorizontalOffsetsIncreaseEvenly();
			AssertVerticalOffsetsIncreaseEvenly();
		}

		public void TestPanelLayoutStrategy_ShouldNotAddDuplicateCards()
		{
			var duplicateTask = CreateTask(workflow, GlbStaff.CurrentUser.GS_Code, 0, description: "Dup");
			duplicateTask.P9_CardNote = "Dup";
			Factory.Save();

			var count0 = TaskCards.Length;

			TaskPanel.AddTask(duplicateTask, unique: false);
			TaskPanel.AddTask(duplicateTask, unique: false);
			TaskPanel.AddTask(duplicateTask, unique: false);
			TaskPanel.AddTask(duplicateTask, unique: false);
			TaskPanel.AddTask(duplicateTask, unique: false);

			BMBoardSectionViewModel.CreateAndPopulatePropertyCache(
				TaskChannelMap.ForTest(section, viewModel, workflow),
				viewModel);

			ResizeTaskPanel(500, 30);
			var count1 = TaskCards.Length;
			Assert("No duplicate cards added", count1 > count0);

			TaskPanel.Cell.OnCardsRefreshed(fullRefresh: false);
			var count2 = TaskCards.Length;
			Assert("No new cards added", count1 == count2);
		}

		public void TestPanelLayoutStrategy_WhenFiltering_ShouldArrangeCardsToUsePanelSpaceAndNotOverlap()
		{
			SetupTasks();
			Assert("Precondition: The task cards should be overlapping.", TaskPanel.TaskCards.First().Location.X + TaskPanel.TaskCards.First().Width > TaskPanel.TaskCards.ElementAt(1).Location.X);

			BMBoardSectionViewModel.CreateAndPopulatePropertyCache(
				TaskChannelMap.ForTest(section, viewModel, workflow, workflow1, workflow2),
				viewModel);

			TaskPanel.SetupTasksForTest(Factory, TaskPanel.TaskCards.Select(taskCard => taskCard.CardContent).ToList(), new[] { new HideCurrentTasksFilter() });

			var amountOfVisibleCards = TaskPanel.TaskCards.Count();
			var visibleCardsHorizontalOffset = TaskPanel.TaskCards.ElementAt(1).Location.X - TaskPanel.TaskCards.ElementAt(0).Location.X;
			var visibleVerticalOffset = TaskPanel.TaskCards.ElementAt(1).Location.Y - TaskPanel.TaskCards.ElementAt(0).Location.Y;
			var taskCardsOverflow = TaskPanel.Width - TaskPanel.Padding.Right - TaskPanel.Padding.Left >= TaskPanel.TaskCards.First().Width * amountOfVisibleCards;
			var spaceBetweenTaskCards = (visibleCardsHorizontalOffset == TaskPanel.TaskCards.First().Width) || (visibleVerticalOffset == TaskPanel.TaskCards.First().Height);

			Assert("The task cards should not be overlapping.", taskCardsOverflow && spaceBetweenTaskCards);
		}

		public void TestPanelLayoutStrategy_WhenFiltering_ShouldArrangeCardsToUsePanelSpaceAndStillOverlap()
		{
			SetupTasks();
			var jobHeader = CreateJobHeader<OrgHeader>(false);
			var workflow3 = CreateWorkflow(jobHeader, "Workflow", bucket);

			var task7 = CreateTask(workflow, GlbStaff.CurrentUser.GS_Code, 20);
			var task8 = CreateTask(workflow, GlbStaff.CurrentUser.GS_Code, 20);
			var task9 = CreateTask(workflow3, GlbStaff.CurrentUser.GS_Code, 20);
			var task10 = CreateTask(workflow3, GlbStaff.CurrentUser.GS_Code, 20);

			TaskPanel.AddTask(task7);
			TaskPanel.AddTask(task8);
			TaskPanel.AddTask(task9);
			TaskPanel.AddTask(task10);

			Factory.Save();

			BMBoardSectionViewModel.CreateAndPopulatePropertyCache(
				TaskChannelMap.ForTest(section, viewModel, workflow, workflow1, workflow2, workflow3),
				viewModel);

			TaskPanel.SetupTasksForTest(Factory, TaskPanel.TaskCards.Select(taskCard => taskCard.CardContent).ToList(), new[] { new HideCurrentTasksFilter() });

			var amountOfVisibleCards = TaskPanel.TaskCards.Count();
			var lastTaskCard = TaskPanel.TaskCards.Max(taskCard => taskCard.Location.X);
			var visibleCardsHorizontalOffset = TaskPanel.TaskCards.ElementAt(1).Location.X - TaskPanel.TaskCards.ElementAt(0).Location.X;
			var visibleVerticalOffset = TaskPanel.TaskCards.ElementAt(1).Location.Y - TaskPanel.TaskCards.ElementAt(0).Location.Y;

			Assert("The task cards should be overlapping.", TaskPanel.TaskCards.First().Location.X + TaskPanel.TaskCards.First().Width > TaskPanel.TaskCards.ElementAt(1).Location.X);
			Assert("The task cards should overflow.", TaskPanel.Width < TaskPanel.TaskCards.First().Width * amountOfVisibleCards);
			AssertCloseEnough("The last task card should be at the edge of task panel", (TaskPanel.Width - TaskCardPaddingInPixels.X), lastTaskCard + TaskPanel.TaskCards.First().Width, 2);
		}

		void SetupTasks()
		{
			var jobHeader = CreateJobHeader<OrgHeader>(false);
			workflow1 = CreateWorkflow(jobHeader, "Workflow", bucket);
			workflow2 = CreateWorkflow(jobHeader, "Workflow", bucket);

			var task1 = CreateTask(workflow1, GlbStaff.CurrentUser.GS_Code, 20);
			var task2 = CreateTask(workflow1, GlbStaff.CurrentUser.GS_Code, 20);
			var task3 = CreateTask(workflow1, GlbStaff.CurrentUser.GS_Code, 20);
			var task4 = CreateTask(workflow2, GlbStaff.CurrentUser.GS_Code, 20);
			var task5 = CreateTask(workflow2, GlbStaff.CurrentUser.GS_Code, 20);
			var task6 = CreateTask(workflow2, GlbStaff.CurrentUser.GS_Code, 20);

			Factory.Save();

			ResizeTaskPanel(400, 150);
			TaskPanel.AddTask(task1);
			TaskPanel.AddTask(task2);
			TaskPanel.AddTask(task3);
			TaskPanel.AddTask(task4);
			TaskPanel.AddTask(task5);
			TaskPanel.AddTask(task6);
		}

		ProcessHeader workflow1, workflow2;
	}
}
