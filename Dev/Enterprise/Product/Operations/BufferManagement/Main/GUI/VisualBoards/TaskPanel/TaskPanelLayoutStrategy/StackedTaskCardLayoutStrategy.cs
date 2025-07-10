using System.Collections.Generic;
using System.Drawing;
using CargoWise.Windows.UI;

namespace Enterprise.BufferManagement.GUI
{
	class StackedTaskCardLayoutStrategy : TaskPanelLayoutStrategy
	{
		public override void HandleClick(TaskCardControl taskCardControl, TaskPanel taskPanel)
		{
			// Do nothing.
		}

		protected override int GetCardHorizontalOffset(TaskPanel taskPanel, int numberOfTasksToBeAllocated)
		{
			return 0;
		}

		protected override int GetCardVerticalOffset(int cardHeight)
		{
			return cardHeight + ControlDpiScalingHelper.ScaleToCurrentDpiY(CardPadding);
		}

		const int CardPadding = 4;

		protected override bool CheckProposedCardLocationAndAdjustIfNecessary(TaskPanel taskPanel, List<TaskCardControl> allocatedControls, TaskCardControl taskCard, ref Point proposedLocation)
		{
			return IsTheFirstCard(proposedLocation) ||
				CheckFallingOffBottom(taskPanel, taskCard, ref proposedLocation) &&
				CheckFallingOffTheRight(taskPanel, taskCard, ref proposedLocation);
		}

		bool CheckFallingOffBottom(TaskPanel taskPanel, TaskCardControl taskCard, ref Point proposedLocation)
		{
			if (DoesProposedLocationFallOffBottom(taskPanel, taskCard, proposedLocation))
			{
				CardsFallOffBottom = true;
				var x = proposedLocation.X
						+ ControlDpiScalingHelper.ScaleToCurrentDpiX(CardPadding)
						+ (taskCard != null ? taskCard.Width : 0);

				proposedLocation = ControlDpiScalingHelper.NewScaledPoint(x, TaskCardPaddingInPixels.Y, isInStandardDpi: false);
			}
			return true;
		}

		bool CheckFallingOffTheRight(TaskPanel taskPanel, TaskCardControl taskCard, ref Point proposedLocation)
		{
			return
				!DoesProposedLocationFallOffTheRight(taskPanel, taskCard, proposedLocation) ||
				IsTheFirstColumn(proposedLocation);
		}
	}
}
