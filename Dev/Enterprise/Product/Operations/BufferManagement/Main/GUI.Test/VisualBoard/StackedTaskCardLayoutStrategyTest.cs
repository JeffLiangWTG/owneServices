using System;
using System.Collections.Generic;
using System.Linq;
using Enterprise.BufferManagement.Business;

namespace Enterprise.BufferManagement.GUI.Test
{
	public class StackedTaskCardLayoutStrategyTest : TaskPanelLayoutStrategyTest
	{
		protected override string PanelLayoutStyle => PanelLayoutTypeList.Codes.Stacked;

		protected override void AssertCardPlacementIsCorrect()
		{
			base.AssertCardPlacementIsCorrect();
			AssertCardOffsetsAreCorrect();
		}

		void AssertCardOffsetsAreCorrect()
		{
			AssertHorizontalOffsetsIncreaseEvenly();
			AssertVerticalGapsBetweenCardsAreEqual();
		}

		protected void AssertVerticalGapsBetweenCardsAreEqual()
		{
			var gaps = GetVerticalGapsBetweenCards();
			if (gaps.Any())
			{
				Assert("All vertical gaps between consequent cards for stacked layout should be the same except jumps back to the top line", Math.Abs(gaps.Max() - gaps.Min()) < ScaleInaccuracyInPixels);
			}
			else
			{
				Assert("There is only one card - nothing to check", true);
			}
		}

		IEnumerable<int> GetVerticalGapsBetweenCards()
		{
			var lastBottom = 0;
			foreach (var card in TaskCards)
			{
				if (lastBottom != 0 && card.Top >= lastBottom) //we don't consider jumps back to the top line
				{
					yield return card.Top - lastBottom;
				}

				lastBottom = card.Top + card.Height;
			}
		}
	}
}
