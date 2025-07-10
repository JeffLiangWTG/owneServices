using System.Drawing;
using System.Windows.Forms;
using CargoWise.Windows.UI;
using Enterprise.DocumentScanning.Business.Test;
using Enterprise.DocumentScanning.Thumbnails;

namespace Enterprise.DocumentScanning.GUI.Testing
{
	sealed class ThumbNailDrawerTest : TestCaseWithDocumentFactory
	{
		public void TestGetTotalBoxSizeByNumberOfImagesPerRow()
		{
			Size optimumSize = Drawer.GetTotalBoxSizeByNumberOfImagesPerRow(3);
			AssertScaledEquals("Width should be within range of 150-170 - depends on screen resolution", optimumSize, 150, 170, true);
			AssertScaledEquals("Height should be within range of 200-220 - depends on screen resolution", optimumSize, 200, 220, false);

			ParentPanel.Size = ControlDpiScalingHelper.NewScaledSize(250, 250);
			Drawer = new ThumbNailDrawer(ParentPanel);
			optimumSize = Drawer.GetTotalBoxSizeByNumberOfImagesPerRow(5);
			AssertScaledEquals("Width should be within range of 35-55 - depends on screen resolution", optimumSize, 35, 55, true);
			AssertScaledEquals("Height should be within range of 50-70 - depends on screen resolution", optimumSize, 50, 70, false);
		}

		void AssertScaledEquals(string description, Size optimumSize, int minValue, int maxValue, bool isXAxis)
		{
			if (isXAxis)
			{
				Assert(description, optimumSize.Width > ControlDpiScalingHelper.ScaleToCurrentDpiX(minValue) && optimumSize.Width < ControlDpiScalingHelper.ScaleToCurrentDpiX(maxValue));
			}
			else
			{
				Assert(description, optimumSize.Height > ControlDpiScalingHelper.ScaleToCurrentDpiY(minValue) && optimumSize.Height < ControlDpiScalingHelper.ScaleToCurrentDpiY(maxValue));
			}
		}

		public void TestCalculateOptimalNumberOfPagesPerRow()
		{
			// using the values from the TestGetMaximumTotalBoxSizeToAutoFit....
			// see that test for reasoning
			Size optimum;
			int numberPerRow = Drawer.CalculateOptimalNumberOfPagesPerRow(3, out optimum);
			AssertEquals("works out square", 2, numberPerRow);

			numberPerRow = Drawer.CalculateOptimalNumberOfPagesPerRow(10, out optimum);
			AssertEquals("greater numbers per row preferred over greater numbers of rows", 4, numberPerRow);

			numberPerRow = Drawer.CalculateOptimalNumberOfPagesPerRow(15, out optimum);
			AssertEquals("works out square", 4, numberPerRow);

			numberPerRow = Drawer.CalculateOptimalNumberOfPagesPerRow(0, out optimum);
			AssertEquals("if it can't work out what it's supposed to do it will default to using the minimum size for one thumbnail " +
				"and seeing how many it can fit in the width (in this case 500-ish pixels)... so we won't get a value of 0 per row", 5, numberPerRow);
		}

		public void TestGetMaximumTotalBoxSizeToAutoFit()
		{
			// No point putting assertions for the exact sizes returned, because it will be
			// different on different resolutions/themes.

			Size usableSize = Drawer.ParentPanelUsableSize;
			Size totalBoxSize1PerRow = Drawer.GetMaximumTotalBoxSizeToAutoFit(1, 3);
			Size totalBoxSize2PerRow = Drawer.GetMaximumTotalBoxSizeToAutoFit(2, 3);
			// 2 per row will supply larger images than 1 per row

			Assert("box from 1 per row is smaller than box from 2 per row", totalBoxSize1PerRow.Width < totalBoxSize2PerRow.Width);
			Assert("box from 1 per row is smaller than box from 2 per row", totalBoxSize1PerRow.Height < totalBoxSize2PerRow.Height);

			Size totalBoxSize3PerRow = Drawer.GetMaximumTotalBoxSizeToAutoFit(3, 3);
			// autofit will fit all images in the space of the available panel (500x500)
			// 3 per row will be larger images than 1 per row.
			// when the images are 3 across, the width of the panel is the limiting factor
			// when the images are 1 across the height of the panel is the limiting factor
			// the height divided by 3 is smaller than the width divided by 3, because the width
			// of the images is smaller than the height.

			Assert("box from 3 per row is larger than box from 1 per row", totalBoxSize3PerRow.Width > totalBoxSize1PerRow.Width);
			Assert("box from 3 per row is smaller than box from 2 per row", totalBoxSize3PerRow.Width < totalBoxSize2PerRow.Width);
		}

		public void TestEraseAllThumbnailControls()
		{
			ParentPanel.Controls.Add(new Label());
			ParentPanel.Controls.Add(new Label());
			AssertEquals("Controls count before Erase", 2, ParentPanel.Controls.Count);
			Drawer.EraseAllThumbnailControls();
			AssertEquals("Controls count after Erase", 0, ParentPanel.Controls.Count);
		}

		public void TestGetNewOrder()
		{
			BeginTestNewOrderSet(4, new int[] { 2 }, 0, new int[] { 0, 2, 1, 3 });
			BeginTestNewOrderSet(4, new int[] { 0 }, 2, new int[] { 1, 2, 0, 3 });
			BeginTestNewOrderSet(4, new int[] { 2 }, -1, new int[] { 2, 0, 1, 3 });
		}

		void BeginTestNewOrderSet(int totalPages, int[] selectedPages, int destIndex, int[] expectedNewOrder)
		{
			int[] newOrder = Drawer.GetNewOrder(totalPages, selectedPages, destIndex);
			AssertEquals("Returned count", totalPages, newOrder.Length);

			for (int i = 0; i < totalPages; i++)
			{
				AssertEquals("item " + i, expectedNewOrder[i], newOrder[i]);
			}
		}

		public void TestGetThumbnailStartPoint()
		{
			bool isLastInRow = true;

			Point position = Drawer.GetThumbnailStartPoint(1, 0, 1, out isLastInRow);
			AssertPositionEquals(8, 0, position);
			Assert("Last thumbnail in the row", isLastInRow);

			position = Drawer.GetThumbnailStartPoint(1, 1, 3, out isLastInRow);
			AssertPositionEquals(8, 120, position);
			Assert("last thumbnail in the row", isLastInRow);

			position = Drawer.GetThumbnailStartPoint(1, 2, 3, out isLastInRow);
			AssertPositionEquals(8, 240, position);
			Assert("last thumbnail in the row", isLastInRow);

			position = Drawer.GetThumbnailStartPoint(2, 3, 5, out isLastInRow);
			AssertPositionEquals(108, 120, position);
			Assert("last thumbnail in the row", isLastInRow);

			position = Drawer.GetThumbnailStartPoint(2, 2, 5, out isLastInRow);
			AssertPositionEquals(8, 120, position);
			Assert("not last thumbnail in the row", !isLastInRow);

			position = Drawer.GetThumbnailStartPoint(2, 0, 5, out isLastInRow);
			AssertPositionEquals(8, 0, position);
			Assert("not last thumbnail in the row", !isLastInRow);

			position = Drawer.GetThumbnailStartPoint(2, 4, 5, out isLastInRow);
			AssertPositionEquals(8, 240, position);
			Assert("last thumbnail in the row because its the last page", isLastInRow);

			position = Drawer.GetThumbnailStartPoint(4, 3, 5, out isLastInRow);
			AssertPositionEquals(308, 0, position);
			Assert("last thumbnail in the row", isLastInRow);
		}

		void AssertPositionEquals(int unscaledX, int unscaledY, Point scaledActualPoint)
		{
			AssertEquals(ControlDpiScalingHelper.NewScaledPoint(unscaledX, unscaledY), scaledActualPoint);
		}

		public void TestColorLabel()
		{
			Label testLabel = new Label();
			AssertEquals("Before anything - backcolour is control", SystemColors.Control, testLabel.BackColor);
			AssertEquals("Before anything - forecolour is control", SystemColors.ControlText, testLabel.ForeColor);

			Drawer.ColorLabel(testLabel, true, true);
			AssertEquals("After selected, backcolour should be highlighted now", SystemColors.Highlight, testLabel.BackColor);
			AssertEquals("After selected, forecolour should be highlighted now", SystemColors.HighlightText, testLabel.ForeColor);

			Drawer.ColorLabel(testLabel, false, true);
			AssertEquals("After deselected, backcolour back to control color", SystemColors.Control, testLabel.BackColor);
			AssertEquals("After deselected, forecolour back to control color", SystemColors.ControlText, testLabel.ForeColor);
		}

		protected override void TearDown()
		{
			base.TearDown();
			DeleteTempFiles();
		}

		protected override void SetUp()
		{
			base.SetUp();
			ParentPanel = new Panel();
			ParentPanel.Size = ControlDpiScalingHelper.NewScaledSize(500, 500);
			Drawer = new ThumbNailDrawer(ParentPanel);
		}

		ThumbNailDrawer Drawer;
		Panel ParentPanel;
	}
}
