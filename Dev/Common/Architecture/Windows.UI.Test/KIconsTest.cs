using System;
using System.Resources;
using System.Threading;
using System.Windows.Forms;
using NUnit.Framework;

namespace CargoWise.Windows.UI.Testing
{
	sealed class KIconsTest : TestCase
	{
		public enum TestIconTypes
		{
			Icon,
			Image
		}

		public class TestIcons : KIcons
		{
			protected override ResourceManager[] NewResourceManagers()
			{
				return new ResourceManager[]
				{
					GetResourceManager("CargoWise.Windows.UI.Testing.Icons+Test.TestIcons", typeof(TestIcons).Assembly)
				};
			}

			public new ImageList ImageListCore
			{ get { return base.ImageListCore; } }

			public static readonly TestIcons Instance = new TestIcons();
		}

		public void TestIsIcon()
		{
			AssertEquals(true, TestIcons.Instance.IsIconCore(TestIconTypes.Icon));
			AssertEquals(false, TestIcons.Instance.IsIconCore(TestIconTypes.Image));
		}

		public void TestGetIcon()
		{
			AssertNotNull(TestIcons.Instance.GetIconCore(TestIconTypes.Icon));
		}

		public void TestIsImage()
		{
			AssertEquals(false, TestIcons.Instance.IsImageCore(TestIconTypes.Icon));
			AssertEquals(true, TestIcons.Instance.IsImageCore(TestIconTypes.Image));
		}

		public void TestGetImage()
		{
			AssertNotNull(TestIcons.Instance.GetImageCore(TestIconTypes.Image));
		}

		public void TestGetImageListIndexForImage()
		{
			int firstIndex = TestIcons.Instance.GetImageListIndexCore(TestIconTypes.Image);
			int firstImageCount = TestIcons.Instance.ImageListCore.Images.Count;
			int secondIndex = TestIcons.Instance.GetImageListIndexCore(TestIconTypes.Image);
			int secondImageCount = TestIcons.Instance.ImageListCore.Images.Count;

			Assert("ImageList index invalid", firstIndex >= 0);
			AssertEquals("Image wasn't cached", firstIndex, secondIndex);
			AssertEquals("Image wasn't cached", firstImageCount, secondImageCount);
		}

		public void TestGetImageListIndexForIcon()
		{
			int firstIndex = TestIcons.Instance.GetImageListIndexCore(TestIconTypes.Icon);
			int firstImageCount = TestIcons.Instance.ImageListCore.Images.Count;
			int secondIndex = TestIcons.Instance.GetImageListIndexCore(TestIconTypes.Icon);
			int secondImageCount = TestIcons.Instance.ImageListCore.Images.Count;

			Assert("ImageList index invalid", firstIndex >= 0);
			AssertEquals("Icon wasn't cached", firstIndex, secondIndex);
			AssertEquals("Icon wasn't cached", firstImageCount, secondImageCount);
		}

		public void TestGetImageList_CachedPerThread()
		{
			ImageListOnOtherThread = null;
			Thread t = new Thread(new ThreadStart(TestGetImageList_CachedPerThread_ThreadStart));
			t.Start();
			t.Join();
			AssertNotNull("ImageList on other thread not null for test", ImageListOnOtherThread);
			Assert("ImageList should be per thread", ImageListOnOtherThread != TestIcons.Instance.ImageListCore);

			if (exceptionInThread != null)
			{
				throw exceptionInThread;
			}
		}

		ImageList ImageListOnOtherThread;
		void TestGetImageList_CachedPerThread_ThreadStart()
		{
			try
			{
				TestIcons.Instance.GetImageListIndexCore(TestIconTypes.Icon);
				ImageListOnOtherThread = TestIcons.Instance.ImageListCore;
			}
			catch (Exception ex)
			{
				exceptionInThread = ex;
			}
		}
		Exception exceptionInThread;
	}
}
