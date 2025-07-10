using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Linq;
using CargoWise.Common;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.BufferManagement.Business;

namespace Enterprise.BufferManagement.GUI
{
	abstract class TaskPanelLayoutStrategy
	{
		public void LayoutCards(ICardContent[] newCardContents, TaskPanel taskPanel, TaskControlFilterApplicator filterApplicator, ITaskCardRenderer cardRenderer)
		{
			taskPanel.CardHorizontalOffset = GetCardHorizontalOffset(taskPanel, newCardContents.Count(filterApplicator.IsVisible));

			CardsFallOffTheRight = false;
			CardsFallOffBottom = false;

			//some cards are allocated and visible
			//some cards are allocated, but not yet visible (saved for future visualisation when filters apply so that the cards become visible fast)
			//the other cards are to be allocated (not allocated until corresponding filters apply)
			//allocated cards are not disposed while they present on a board (until board refreshes)

			var taskCardControls = taskPanel.TaskCards;
			var distinctTaskCardControls = IEnumerableExtensions.DistinctBy(taskCardControls, c => c.CardContent.Identifier);

			ProcessNewCardContents(newCardContents, taskPanel, filterApplicator, cardRenderer, TaskCardPaddingInPixels, distinctTaskCardControls);
		}

		void ProcessNewCardContents(ICardContent[] newCardContents, TaskPanel taskPanel, TaskControlFilterApplicator filterApplicator, ITaskCardRenderer cardRenderer, Point cardLocation, IEnumerable<TaskCardControl> distinctTaskCardControls)
		{
			var someVisibleCardsLeft = true;
			var allocatedTaskCardControls = new List<TaskCardControl>();

			var existingTaskCardControlsFromTaskPanel = distinctTaskCardControls.ToDictionary(c => c.CardContent.Identifier);

			foreach (var newCardContent in newCardContents.Where(filterApplicator.IsVisible))
			{
				int? oldTaskCardChildIndex = null;

				if (someVisibleCardsLeft)
				{
					var isRedrawFilterApplicable = filterApplicator.IsRedrawFilterApplicable(newCardContent);

					ProcessUpdatedCard(
						existingTaskCardControlsFromTaskPanel,
						newCardContent,
						taskPanel,
						ref oldTaskCardChildIndex);

					var taskCardControl = GetOrCreateTaskCardToPosition(
						taskPanel.SharedMenuStrip,
						cardRenderer,
						taskPanel.Cell,
						taskPanel.SectionViewModel,
						existingTaskCardControlsFromTaskPanel,
						newCardContent,
						isRedrawFilterApplicable);

					filterApplicator.Apply(taskCardControl, newCardContent);

					allocatedTaskCardControls.Add(taskCardControl);

					if (existingTaskCardControlsFromTaskPanel.ContainsKey(newCardContent.Identifier))
					{
						existingTaskCardControlsFromTaskPanel.Remove(newCardContent.Identifier);
					}
					else
					{
						taskPanel.Controls.Add(taskCardControl);
						if (oldTaskCardChildIndex != null)
						{
							taskPanel.Controls.SetChildIndex(taskCardControl, oldTaskCardChildIndex.Value);
						}
					}

					UpdateCardVisibility(taskCardControl, taskPanel, allocatedTaskCardControls, ref cardLocation, ref someVisibleCardsLeft);
				}
				else if (existingTaskCardControlsFromTaskPanel.ContainsKey(newCardContent.Identifier))
				{
					existingTaskCardControlsFromTaskPanel[newCardContent.Identifier].Visible = false;
					existingTaskCardControlsFromTaskPanel.Remove(newCardContent.Identifier);
				}
			}

			foreach (var taskCardControl in existingTaskCardControlsFromTaskPanel.Values)
			{
				taskCardControl.BackgroundImage = null;
				taskCardControl.Dispose();
			}

			int index = 0;
			foreach (var allocatedTaskCardControl in allocatedTaskCardControls.Where(card => taskPanel.Controls.Contains(card)))
			{
				taskPanel.Controls.SetChildIndex(allocatedTaskCardControl, index);
				index++;
			}

#if WINZOR
			var zIndex = 0;
			foreach (var allocatedTaskCardControl in allocatedTaskCardControls.Where(card => taskPanel.Controls.Contains(card)).Reverse())
			{
				allocatedTaskCardControl.ZIndex = zIndex;
				zIndex++;
			}
#endif
		}

		void UpdateCardVisibility(TaskCardControl taskCardControl, TaskPanel taskPanel, List<TaskCardControl> allocatedTaskCardControls, [DpiState(DpiState.ScaledVariant)] ref Point cardLocation, ref bool someVisibleCardsLeft)
		{
			//visible after filtering
			if (taskCardControl.Visible)
			{
				if (CheckProposedCardLocationAndAdjustIfNecessary(taskPanel, allocatedTaskCardControls, taskCardControl, ref cardLocation))
				{
					taskCardControl.Location = cardLocation;
					cardLocation = ControlDpiScalingHelper.NewScaledPoint(
						cardLocation.X + taskPanel.CardHorizontalOffset,
						CalculateVerticalPositionBasedOnLastCard(cardLocation.Y, taskCardControl),
						false);
				}
				else
				{
					taskCardControl.Visible = false;
					someVisibleCardsLeft = false;
				}
			}
		}

		void ProcessUpdatedCard(Dictionary<ZGuid, TaskCardControl> existingTaskCardControlsFromTaskPanel, ICardContent newCardContent, TaskPanel taskPanel, ref int? oldTaskCardChildIndex)
		{
#if DEBUG
			taskPanel.PreProcessUpdateCardAction_ForTest?.Invoke();
#endif

			if (existingTaskCardControlsFromTaskPanel.TryGetValue(newCardContent.Identifier, out TaskCardControl oldTaskCardControl))
			{
				var oldLastEditTime = oldTaskCardControl.CardContent.LastEditTime;
				var newLastEditTime = newCardContent.LastEditTime;

				if (oldLastEditTime != newLastEditTime)
				{
					oldTaskCardChildIndex = taskPanel.Controls.GetChildIndex(oldTaskCardControl, throwException: false);

					if (oldTaskCardChildIndex != -1)
					{
						taskPanel.Controls.Remove(oldTaskCardControl);
						oldTaskCardControl.Dispose();
						existingTaskCardControlsFromTaskPanel.Remove(newCardContent.Identifier);
					}
				}
			}
		}

		[SuppressMessage("Microsoft.Design", "CA1031: Do not catch general exception types", Justification = "valid for handling disposables in factory methods")]
		static TaskCardControl GetOrCreateTaskCardToPosition(KContextMenuStrip sharedMenuStrip, ITaskCardRenderer cardRenderer, CellContent cell, BMBoardSectionViewModel sectionViewModel, Dictionary<ZGuid, TaskCardControl> existingControls, ICardContent cardContent, bool isRedrawFilterApplicable)
		{
			TaskCardControl taskCard = null;
			try
			{
				if (!existingControls.TryGetValue(cardContent.Identifier, out taskCard))
				{
					taskCard = cardRenderer.ConstructTaskCardControl(sharedMenuStrip, cardContent, sectionViewModel, cell, canUseBitmapCache: !isRedrawFilterApplicable);
				}
			}
			catch
			{
				try
				{
					if (taskCard != null)
					{
						taskCard.Dispose();
					}
				}
				catch { }
				throw;
			}

			taskCard.Visible = true;
			return taskCard;
		}

		protected bool DoesProposedLocationFallOffBottom(TaskPanel panel, TaskCardControl taskCard, Point proposedLocation) => panel.Height - TaskCardPaddingInPixels.Y < proposedLocation.Y + taskCard.Height;

		protected bool DoesProposedLocationFallOffTheRight(TaskPanel panel, TaskCardControl taskCard, Point proposedLocation) => panel.Width - TaskCardPaddingInPixels.X < proposedLocation.X + taskCard.Width;

		int CalculateVerticalPositionBasedOnLastCard(int initialVerticalPosition, TaskCardControl taskCard) => initialVerticalPosition + GetCardVerticalOffset(taskCard != null ? taskCard.Height : 0);

		protected bool IsTheFirstCard(Point location) => IsTheFirstColumn(location) && IsTheFirstRow(location);

		protected bool IsTheFirstColumn(Point location) => location.X == TaskCardPaddingInPixels.X;

		protected bool IsTheFirstRow(Point location) => location.Y == TaskCardPaddingInPixels.Y;

		protected Point TaskCardPaddingInPixels => ControlDpiScalingHelper.NewScaledPoint(TaskPanel.TaskCardPadding, TaskPanel.TaskCardPadding, true);

		public abstract void HandleClick(TaskCardControl taskCardControl, TaskPanel taskPanel);

		protected abstract int GetCardVerticalOffset(int cardHeight);

		protected abstract int GetCardHorizontalOffset(TaskPanel taskPanel, int numberOfTasksToAllocate);

		protected bool CardsFallOffTheRight;
		protected bool CardsFallOffBottom;

		protected abstract bool CheckProposedCardLocationAndAdjustIfNecessary(TaskPanel taskPanel, List<TaskCardControl> allocatedControls, TaskCardControl taskCard, ref Point proposedLocation);
	}
}
