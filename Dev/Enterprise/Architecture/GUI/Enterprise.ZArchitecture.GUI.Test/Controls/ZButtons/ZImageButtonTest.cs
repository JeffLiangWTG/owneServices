using System.Drawing;

namespace Enterprise.ZArchitecture.GUI.Testing
{
	sealed class ZImageButtonTest : ZControlBaseTestCase<ZImageButton>
	{
		public void TestBackgroundImage()
		{
			var downBackgroundImage = new Bitmap(1, 1);
			var hotBackgroundImage = new Bitmap(2, 1);
			var normalBackgroundImage = new Bitmap(2, 2);

			using (var button = new ZImageButtonForTest())
			{
				button.DownBackgroundImage = downBackgroundImage;
				button.HotBackgroundImage = hotBackgroundImage;
				button.NormalBackgroundImage = normalBackgroundImage;

				AssertBackgroundImage(button, false, false, normalBackgroundImage);
				AssertBackgroundImage(button, false, true, hotBackgroundImage);
				AssertBackgroundImage(button, true, false, downBackgroundImage);
				AssertBackgroundImage(button, true, true, downBackgroundImage);

				button.DownBackgroundImage = null;
				AssertBackgroundImage(button, false, false, normalBackgroundImage);
				AssertBackgroundImage(button, false, true, hotBackgroundImage);
				AssertBackgroundImage(button, true, false, hotBackgroundImage);
				AssertBackgroundImage(button, true, true, hotBackgroundImage);

				button.HotBackgroundImage = null;
				AssertBackgroundImage(button, false, false, normalBackgroundImage);
				AssertBackgroundImage(button, false, true, normalBackgroundImage);
				AssertBackgroundImage(button, true, false, normalBackgroundImage);
				AssertBackgroundImage(button, true, true, normalBackgroundImage);
			}
		}

		public void TestNormalBackgroundImage_ShouldRefreshBackgroundImageOnChange()
		{
			var image1 = new Bitmap(1, 1);
			var image2 = new Bitmap(2, 1);

			using (var button = new ZImageButtonForTest())
			{
				button.NormalBackgroundImage = image1;
				AssertEquals(string.Format("Actual image width: {0}", button.BackgroundImage.Width), image1, button.BackgroundImage);

				button.NormalBackgroundImage = image2;
				AssertEquals(string.Format("Actual image width: {0}", button.BackgroundImage.Width), image2, button.BackgroundImage);
			}
		}

		public void TestDownBackgroundImage_ShouldRefreshBackgroundImageOnChange()
		{
			var image1 = new Bitmap(1, 1);
			var image2 = new Bitmap(2, 1);

			using (var button = new ZImageButtonForTest())
			{
				button.IsDownOverride = true;

				button.DownBackgroundImage = image1;
				AssertEquals(string.Format("Actual image width: {0}", button.BackgroundImage.Width), image1, button.BackgroundImage);

				button.DownBackgroundImage = image2;
				AssertEquals(string.Format("Actual image width: {0}", button.BackgroundImage.Width), image2, button.BackgroundImage);
			}
		}

		public void TestHotBackgroundImage_ShouldRefreshBackgroundImageOnChange()
		{
			var image1 = new Bitmap(1, 1);
			var image2 = new Bitmap(2, 1);

			using (var button = new ZImageButtonForTest())
			{
				button.IsHotOverride = true;

				button.HotBackgroundImage = image1;
				AssertEquals(string.Format("Actual image width: {0}", button.BackgroundImage.Width), image1, button.BackgroundImage);

				button.HotBackgroundImage = image2;
				AssertEquals(string.Format("Actual image width: {0}", button.BackgroundImage.Width), image2, button.BackgroundImage);
			}
		}

		void AssertBackgroundImage(ZImageButtonForTest button, bool isDown, bool isHot, Image expectedBackgroundImage)
		{
			button.IsDownOverride = isDown;
			button.IsHotOverride = isHot;
			button.RefreshBackgroundImage_Exposed();

			AssertEquals(expectedBackgroundImage, button.BackgroundImage);
		}

		#region Implementation

		class ZImageButtonForTest : ZImageButton
		{
			public bool IsDownOverride;
			public bool IsHotOverride;

			protected override bool IsDown
			{
				get { return IsDownOverride; }
			}

			protected override bool IsHot
			{
				get { return IsHotOverride; }
			}

			public void RefreshBackgroundImage_Exposed()
			{
				RefreshBackgroundImage();
			}
		}

		#endregion
	}
}
