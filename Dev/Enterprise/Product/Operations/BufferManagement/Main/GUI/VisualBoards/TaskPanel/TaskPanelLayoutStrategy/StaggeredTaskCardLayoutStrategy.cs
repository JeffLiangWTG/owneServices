using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using CargoWise.Common;
using CargoWise.Windows.UI;
using Enterprise.ZArchitecture;

namespace Enterprise.BufferManagement.GUI
{
	class StaggeredTaskCardLayoutStrategy : TaskPanelLayoutStrategy
	{
		public override void HandleClick(TaskCardControl taskCardControl, TaskPanel taskPanel)
		{
			var cards = taskPanel.Controls.OfType<TaskCardControl>().Reverse().ToArray();
			{
				foreach (var card in cards)
				{
					card.CloseDetailedCardIfOpen();
				}

				foreach (var card in cards)
				{
					card.BringToFront();
					if (card == taskCardControl)
					{
						break;
					}
				}
			}

			if (taskCardControl.Parent != null)
			{
				var totalLabel = taskCardControl.Parent.Controls.OfType<ZLabel>().FirstOrDefault();
				if (totalLabel != null)
				{
					totalLabel.BringToFront();
				}
				taskPanel.SetStackAppearance();
			}
		}

		protected override int GetCardVerticalOffset(int cardHeight)
		{
			return ControlDpiScalingHelper.ScaleToCurrentDpiY(TaskPanel.TaskCardVerticalOffsetPixels);
		}

		protected override int GetCardHorizontalOffset(TaskPanel taskPanel, int numberOfTasksToAllocate)
		{
			var width = ControlDpiScalingHelper.ScaleToCurrentDpiX(taskPanel.SectionViewModel.GetSummaryCard(string.Empty).Width);
			var horizontalOffset = numberOfTasksToAllocate > 1
				? (taskPanel.Width - width - TaskCardPaddingInPixels.X * 2) / (numberOfTasksToAllocate - 1)
				: ControlDpiScalingHelper.ScaleToCurrentDpiX(TaskPanel.TaskCardMaxHorizontalOffsetPixels);

			horizontalOffset = Math.Max(horizontalOffset, ControlDpiScalingHelper.ScaleToCurrentDpiX(TaskPanel.TaskCardMaxHorizontalOffsetPixels)); // Ensures horizontal offet no less than the min default
			horizontalOffset = Math.Min(horizontalOffset, width); // Ensures horizontal offset no larger than the size of a card (so there is no space between cards)

			return horizontalOffset;
		}

		protected override bool CheckProposedCardLocationAndAdjustIfNecessary(TaskPanel taskPanel, List<TaskCardControl> allocatedControls, TaskCardControl taskCard, ref Point proposedLocation)
		{
			return IsTheFirstCard(proposedLocation) ||
				CheckFallingOffBottom(taskPanel, taskCard, ref proposedLocation) &&
				CheckFallingOffTheRight(taskPanel, allocatedControls, taskCard, ref proposedLocation) &&
				CheckFallingOffBottom(taskPanel, taskCard, ref proposedLocation);
		}

		bool CheckFallingOffBottom(TaskPanel taskPanel, TaskCardControl taskCard, ref Point proposedLocation)
		{
			if (DoesProposedLocationFallOffBottom(taskPanel, taskCard, proposedLocation))
			{
				if (CardsFallOffTheRight)
				{
					return false;
				}
				CardsFallOffBottom = true;
				proposedLocation = ControlDpiScalingHelper.NewScaledPoint(proposedLocation.X, TaskCardPaddingInPixels.Y, false);
			}
			return true;
		}

		bool CheckFallingOffTheRight(TaskPanel taskPanel, List<TaskCardControl> allocatedControls, TaskCardControl taskCard, ref Point proposedLocation)
		{
			if (DoesProposedLocationFallOffTheRight(taskPanel, taskCard, proposedLocation))
			{
				if (CardsFallOffBottom)
				{
					return false;
				}
				CardsFallOffTheRight = true;
				ControlDpiScalingHelper.SetX(ref proposedLocation, TaskCardPaddingInPixels.X, false);

				var lowestCard = allocatedControls.Except(new[] { taskCard }).MaxBySafe(t => t.Top + t.Height);

				if (lowestCard != null)
				{
					ControlDpiScalingHelper.SetY(ref proposedLocation, lowestCard.Top + lowestCard.Height + TaskCardPaddingInPixels.Y, false); //padding is to make a vertical gap between groups of cards
				}
			}
			return true;
		}
	}
}
