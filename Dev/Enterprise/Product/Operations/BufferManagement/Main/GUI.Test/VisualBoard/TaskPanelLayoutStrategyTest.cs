using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using CargoWise.Windows.UI;
using Enterprise.BufferManagement.Business;
using Enterprise.BufferManagement.Business.Test;
using Enterprise.MasterFiles.Business;
using Enterprise.VisualBoards.Business;
using Enterprise.VisualBoards.GUI;

namespace Enterprise.BufferManagement.GUI.Test
{
	public abstract class TaskPanelLayoutStrategyTest : BMSTestCaseWithFactory
	{
		public void TestPanelLayoutStrategy_ShouldAlwaysDisplayAtLeastOneCard()
		{
			ResizeTaskPanel(500, 30);
			var count1 = TaskCards.Length;
			AssertGreaterThan("Cards in the first line should be shown even if task panel is too narrow vertically", count1, 0);

			ResizeTaskPanel(30, 500);
			var count2 = TaskCards.Length;
			AssertGreaterThan("Cards in the first line should be shown even if task panel is too narrow horizontally", count2, 0);

			ResizeTaskPanel(30, 30);
			var count3 = TaskCards.Length;
			AssertGreaterThan("At least one card should be shown even if task panel is too small in both directions", count3, 0);

			CombineAssertions(() =>
			{
				AssertNotEquals("Just to check that we have different number for different cases (1 != 2)", count1, count2);
				AssertNotEquals("Just to check that we have different number for different cases (1 != 3)", count1, count3);
				AssertNotEquals("Just to check that we have different number for different cases (2 != 3)", count2, count3);
			});
		}

		public void TestPanelLayoutStrategy_ShouldRearrangeCardsCorrectly_WhenResizing()
		{
			numberOfShownCards = 0;
			numberOfShownCardsWasChanging = false;
			IterateResizingTaskPanelAndCheckingCardPlacement();

			Assert("Number of shown cards should change when resizing", numberOfShownCardsWasChanging);
		}

		#region Implementation

		protected abstract string PanelLayoutStyle { get; }

		const int maxTaskPanelWidth = 400;
		const int minTaskPanelWidth = 50;
		const int minTaskPanelHeight = 50;
		const int sizeChangeStep = 12;

		protected const int ScaleInaccuracyInPixels = 1;

		int numberOfShownCards;
		bool numberOfShownCardsWasChanging;

		protected TaskPanel TaskPanel;
		protected TaskCardControl[] TaskCards => taskCards ?? (taskCards = TaskPanel.Controls.OfType<TaskCardControl>().Where(c => c.Visible).ToArray());
		TaskCardControl[] taskCards;

		void IterateResizingTaskPanelAndCheckingCardPlacement()
		{
			int width;
			int height;
			for (width = minTaskPanelWidth, height = minTaskPanelHeight; width <= maxTaskPanelWidth || height <= minTaskPanelHeight; width += sizeChangeStep, height += sizeChangeStep)
			{
				ResizeAndCheckCardPlacement(width, height);
			}

			for (width = maxTaskPanelWidth, height = minTaskPanelHeight; width >= minTaskPanelWidth || height <= minTaskPanelHeight; width -= sizeChangeStep, height += sizeChangeStep)
			{
				ResizeAndCheckCardPlacement(width, height);
			}
		}

		void ResizeAndCheckCardPlacement(int width, int height)
		{
			ResizeTaskPanel(width, height);

			var cardsShown = TaskCards.Length;
			if (numberOfShownCards != cardsShown)
			{
				numberOfShownCards = cardsShown;
				numberOfShownCardsWasChanging = true;
			}

			AssertCardPlacementIsCorrect();
		}

		protected void ResizeTaskPanel(int width, int height)
		{
			TaskPanel.Size = ControlDpiScalingHelper.NewScaledSize(width, height);
			TaskPanel.SetupTasksForTest(Factory, Enumerable.Empty<IBoardFilter>());
			Application.DoEvents();
			taskCards = null; //to force TaskCards refresh
		}

		protected virtual void AssertCardPlacementIsCorrect()
		{
			AssertCardsRespectPaddingAndDoNotTouchPannelBorders_IfTheyAreNotInTheFirstLine();
		}

		protected void AssertCardsRespectPaddingAndDoNotTouchPannelBorders_IfTheyAreNotInTheFirstLine()
		{
			var offenders = TaskCards.Where(c =>
				c.Left < TaskCardPaddingInPixels.X - ScaleInaccuracyInPixels ||
				c.Left + c.Width > TaskPanel.Width - TaskCardPaddingInPixels.X + ScaleInaccuracyInPixels && c.Left > TaskCardPaddingInPixels.X + ScaleInaccuracyInPixels ||
				c.Top < TaskCardPaddingInPixels.Y - ScaleInaccuracyInPixels ||
				c.Top + c.Height > TaskPanel.Height - TaskCardPaddingInPixels.Y + ScaleInaccuracyInPixels && c.Top > TaskCardPaddingInPixels.Y + ScaleInaccuracyInPixels);

			if (offenders.Any())
			{
				var str = new StringBuilder();
				str.AppendLine($"All task cards should appreciate task card padding, and yet {offenders.Count()} offending card(s) found:");
				str.AppendLine();

				foreach (var card in offenders)
				{
					str.AppendLine($"Card: left border = {card.Left}, right border = {card.Left + card.Width}, top border = {card.Top}, bottom border = {card.Top + card.Height}");
				}
				str.AppendLine();
				str.AppendLine($"Allowed region is: left border = {TaskCardPaddingInPixels.X}, right border = {TaskPanel.Width - TaskCardPaddingInPixels.X}, top border = {TaskCardPaddingInPixels.Y}, bottom border = {TaskPanel.Height - TaskCardPaddingInPixels.Y}");

				Assert(str.ToString(), false);
			}
		}

		protected Point TaskCardPaddingInPixels => ControlDpiScalingHelper.NewScaledPoint(TaskPanel.TaskCardPadding, TaskPanel.TaskCardPadding, true);

		protected void AssertHorizontalOffsetsIncreaseEvenly()
		{
			var offsets = TaskCards.Select(c => c.Left).Distinct().OrderBy(x => x).ToArray();
			AssertOffsetsIncreaseEvenly(offsets);
		}

		protected void AssertVerticalOffsetsIncreaseEvenly()
		{
			var offsets = TaskCards.Select(c => c.Top).Distinct().OrderBy(y => y).ToArray();
			AssertOffsetsIncreaseEvenly(offsets, maxDeltaUnderConsideration: TaskCards.Min(c => c.Height));
		}

		void AssertOffsetsIncreaseEvenly(int[] offsets, int maxDeltaUnderConsideration = 1000)
		{
			var minOffset = offsets.Min();

			var expectedDelta = 0;
			var lastOffset = 0;
			foreach (var offset in offsets)
			{
				var relative = offset - minOffset;

				if (expectedDelta != 0)
				{
					var delta = relative - lastOffset;
					if (delta <= maxDeltaUnderConsideration)
					{
						AssertCloseEnough("All offsets should increase evenly", expectedDelta, delta, ScaleInaccuracyInPixels);
					}
					else
					{
						Assert("maxDeltaUnderConsideration is for staggered layout where we don't want to consider huge vertical jumps between cards", true);
					}
				}
				else
				{
					expectedDelta = relative;
				}

				lastOffset = relative;
			}
		}

		IDisposable asyncBehaviour;
		VisualBoardForm form;
		protected ProcessHeader workflow;
		protected BMComponent bucket;
		protected BMBoardSection section;
		protected BMBoardSectionViewModel viewModel;

		protected override void SetUp()
		{
			base.SetUp();

			var system = CreateSystem("ORG");
			bucket = CreateBucket(system);
			section = CreateBoardSection(bucket);
			section.SectionConfiguration.PanelLayoutStyle = PanelLayoutStyle;

			var jobHeader = CreateJobHeader<OrgHeader>();
			workflow = jobHeader.ProcessHeaders.AddNew();

			var tasks = new List<ProcessTask>();
			const int numberOfTasks = 15;

			for (int i = 0; i < numberOfTasks; i++)
			{
				var taskDesc = $"Task {i}";
				var task = CreateTask(workflow, GlbStaff.CurrentUser.GS_Code, 0, description: taskDesc);
				if (i % 3 == 0 || i % 5 == 0)
				{
					task.P9_CardNote = taskDesc;
				}
				tasks.Add(task);
			}
			Factory.Save();

			viewModel = BMSTestHelper.CreateViewModel(section, workflow);

			asyncBehaviour = BMSTestCaseWithFactory.DisableAsyncBehaviour();
			form = new VisualBoardForm(VisualBoardFormTest.GetViewModel(section.Board));
			TaskPanel = BMSGUITestCase.CreateTaskPanel(new CellContent(0, 0, CellContentType.Cards) { Label = "Day 1", Channel = viewModel.CreateChannelForTest(GlbStaff.CurrentUser) }, viewModel);
			form.Controls.Add(TaskPanel);
			form.Show();

			foreach (var task in tasks)
			{
				TaskPanel.AddTask(task);
			}

			BMBoardSectionViewModel.CreateAndPopulatePropertyCache(
				TaskChannelMap.ForTest(section, viewModel, workflow),
				viewModel);

			TaskPanel.BringToFront();
		}

		protected override void TearDown()
		{
			TaskPanel.Dispose();
			form.Dispose();
			asyncBehaviour.Dispose();
			base.TearDown();
		}

		#endregion
	}
}
