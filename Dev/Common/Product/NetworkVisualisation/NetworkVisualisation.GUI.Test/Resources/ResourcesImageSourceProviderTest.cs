using System.Globalization;
using NUnit.Framework;

namespace CargoWise.NetworkVisualisation.GUI.Test.Resources
{
	public class ResourcesImageSourceProviderTest : TestCase
	{
		public void TestGetImageSource_ShouldReturnImageSource_ForSupportedImage()
		{
			var target = new ResourcesImageSourceProvider();

			var result = target.GetImageSource("asterisk");

			AssertNotNull(result);
		}

		public void TestGetImageSource_ShouldReturnTheSameImageSource_WhenSupportedImageGotTwice()
		{
			var target1 = new ResourcesImageSourceProvider();
			var target2 = new ResourcesImageSourceProvider();
			var imageId = "pause";

			var result1 = target1.GetImageSource(imageId);
			var result2 = target2.GetImageSource(imageId);

			AssertSame(result1, result2);
		}

		public void TestGetImageSource_ShouldReturnNull_ForNonSupportedImage()
		{
			var target = new ResourcesImageSourceProvider();

			var result = target.GetImageSource("NotSupportedImage");

			AssertNull(result);
		}

		public void TestGetImageSource_ShouldReturnNull_ForSupportedImageDifferentCase()
		{
			var target = new ResourcesImageSourceProvider();

			var result = target.GetImageSource("Asterisk");

			AssertNull(result);
		}

		public void TestGetImageSource_ShouldReturnImageSource_ForSupportedImageInOtherCulture()
		{
			Integration.Properties.Resources.Culture = new CultureInfo("de");
			var target = new ResourcesImageSourceProvider();

			var result = target.GetImageSource("asterisk");

			AssertNotNull(result);
		}
	}
}
