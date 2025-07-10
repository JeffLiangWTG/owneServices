using System.Drawing;
using CargoWise.Windows.UI;
using NUnit.Framework;

namespace Enterprise.DocumentScanning.Thumbnails.Testing
{
	sealed class ThumbNailDrawerSizeMgrTest : TestCase
	{
		public void TestSize()
		{
			ThumbNailDrawerSizeMgr sizeMgr = new ThumbNailDrawerSizeMgr();

			Rectangle totalRect = new Rectangle(Point.Empty, sizeMgr.TotalBoxSizeForOneThumbnail);
			Rectangle subRect = new Rectangle(sizeMgr.ImagePanelOffset, sizeMgr.ImagePanelSize);
			Assert("area to draw thumbnail image is contained within the total box size for one thumbnail", totalRect.Contains(subRect));

			SizeManager.TotalBoxSizeForOneThumbnail = ControlDpiScalingHelper.NewScaledSize(50, 50);
			totalRect = new Rectangle(Point.Empty, sizeMgr.TotalBoxSizeForOneThumbnail);
			subRect = new Rectangle(sizeMgr.ImagePanelOffset, sizeMgr.ImagePanelSize);
			Assert("area to draw thumbnail image is contained within the total box size for one thumbnail", totalRect.Contains(subRect));
		}

		public void TestTotalBoxSize()
		{
			Assert("Default is set", SizeManager.TotalBoxSizeForOneThumbnail != Size.Empty);

			SizeManager.TotalBoxSizeForOneThumbnail = ControlDpiScalingHelper.NewScaledSize(100, 150);
			AssertSizeEquals(100, 150, SizeManager.TotalBoxSizeForOneThumbnail);

			SizeManager.TotalBoxSizeForOneThumbnail = ControlDpiScalingHelper.NewScaledSize(100, 100);
			AssertSizeEquals(100, 100, SizeManager.TotalBoxSizeForOneThumbnail);
		}

		public void TestImagePanelSize()
		{
			Assert("Default is not empty", SizeManager.ImagePanelSize != Size.Empty);

			SizeManager.TotalBoxSizeForOneThumbnail = ControlDpiScalingHelper.NewScaledSize(200, 200);
			AssertSizeEquals(180, 164, SizeManager.ImagePanelSize);

			SizeManager.TotalBoxSizeForOneThumbnail = ControlDpiScalingHelper.NewScaledSize(100, 150);
			AssertSizeEquals(80, 114, SizeManager.ImagePanelSize);
		}

		public void TestImagePanelOffset()
		{
			Assert("Default is not empty", SizeManager.ImagePanelOffset != Point.Empty);

			SizeManager.TotalBoxSizeForOneThumbnail = ControlDpiScalingHelper.NewScaledSize(200, 200);
			AssertPositionEquals(0, 22, SizeManager.ImagePanelOffset);

			SizeManager.TotalBoxSizeForOneThumbnail = ControlDpiScalingHelper.NewScaledSize(100, 150);
			AssertPositionEquals(0, 22, SizeManager.ImagePanelOffset);
		}

		public void TestDrawableAreaSize()
		{
			Assert("Default is not empty", SizeManager.DrawableAreaSize != Size.Empty);

			SizeManager.TotalBoxSizeForOneThumbnail = ControlDpiScalingHelper.NewScaledSize(200, 200);
			AssertSizeEquals(180, 186, SizeManager.DrawableAreaSize);

			SizeManager.TotalBoxSizeForOneThumbnail = ControlDpiScalingHelper.NewScaledSize(100, 150);
			AssertSizeEquals(80, 136, SizeManager.DrawableAreaSize);
		}

		void AssertSizeEquals(int unscaledX, int unscaledY, Size scaledActualSize)
		{
			AssertEquals(ControlDpiScalingHelper.NewScaledSize(unscaledX, unscaledY), scaledActualSize);
		}

		public void TestDrawableAreaOffset()
		{
			Assert("Default is not empty", SizeManager.DrawableAreaOffset != Point.Empty);

			SizeManager.TotalBoxSizeForOneThumbnail = ControlDpiScalingHelper.NewScaledSize(200, 200);
			AssertPositionEquals(10, 8, SizeManager.DrawableAreaOffset);

			SizeManager.TotalBoxSizeForOneThumbnail = ControlDpiScalingHelper.NewScaledSize(100, 150);
			AssertPositionEquals(10, 8, SizeManager.DrawableAreaOffset);
		}

		void AssertPositionEquals(int unscaledX, int unscaledY, Point scaledActualPoint)
		{
			AssertEquals(ControlDpiScalingHelper.NewScaledPoint(unscaledX, unscaledY), scaledActualPoint);
		}

		public void TestLabelSize()
		{
			Assert("Default is not empty", SizeManager.LabelSize != Size.Empty);

			SizeManager.TotalBoxSizeForOneThumbnail = ControlDpiScalingHelper.NewScaledSize(200, 200);
			AssertSizeEquals(180, 22, SizeManager.LabelSize);

			SizeManager.TotalBoxSizeForOneThumbnail = ControlDpiScalingHelper.NewScaledSize(100, 150);
			AssertSizeEquals(80, 22, SizeManager.LabelSize);
		}

		public void TestLabelOffset()
		{
			AssertEquals("Default is empty", Point.Empty, SizeManager.LabelOffset);

			SizeManager.TotalBoxSizeForOneThumbnail = ControlDpiScalingHelper.NewScaledSize(200, 200);
			AssertPositionEquals(0, 0, SizeManager.LabelOffset);

			SizeManager.TotalBoxSizeForOneThumbnail = ControlDpiScalingHelper.NewScaledSize(100, 150);
			AssertPositionEquals(0, 0, SizeManager.LabelOffset);
		}

		public void TestDropPanelSize()
		{
			Assert("Default is not empty", SizeManager.DropPanelSize != Size.Empty);

			SizeManager.TotalBoxSizeForOneThumbnail = ControlDpiScalingHelper.NewScaledSize(200, 200);
			AssertSizeEquals(20, 186, SizeManager.DropPanelSize);

			SizeManager.TotalBoxSizeForOneThumbnail = ControlDpiScalingHelper.NewScaledSize(100, 150);
			AssertSizeEquals(20, 136, SizeManager.DropPanelSize);
		}

		public void TestDropPanelLastInRowSize()
		{
			Assert("Default is not empty", SizeManager.DropPanelLastInRowSize != Size.Empty);

			SizeManager.TotalBoxSizeForOneThumbnail = ControlDpiScalingHelper.NewScaledSize(200, 200);
			AssertSizeEquals(10, 186, SizeManager.DropPanelLastInRowSize);

			SizeManager.TotalBoxSizeForOneThumbnail = ControlDpiScalingHelper.NewScaledSize(100, 150);
			AssertSizeEquals(10, 136, SizeManager.DropPanelLastInRowSize);
		}

		public void TestDropPanelFirstInRowSize()
		{
			Assert("Default is not empty", SizeManager.DropPanelFirstInRowSize != Size.Empty);

			SizeManager.TotalBoxSizeForOneThumbnail = ControlDpiScalingHelper.NewScaledSize(200, 200);
			AssertSizeEquals(10, 186, SizeManager.DropPanelFirstInRowSize);

			SizeManager.TotalBoxSizeForOneThumbnail = ControlDpiScalingHelper.NewScaledSize(100, 150);
			AssertSizeEquals(10, 136, SizeManager.DropPanelFirstInRowSize);
		}

		public void TestMinimumTotalBoxSizeForOneThumbnail()
		{
			AssertSizeEquals(80, 106, SizeManager.MinimumTotalBoxSizeForOneThumbnail);
		}

		protected override void SetUp()
		{
			base.SetUp();
			SizeManager = new ThumbNailDrawerSizeMgr();
		}

		ThumbNailDrawerSizeMgr SizeManager;
	}
}
