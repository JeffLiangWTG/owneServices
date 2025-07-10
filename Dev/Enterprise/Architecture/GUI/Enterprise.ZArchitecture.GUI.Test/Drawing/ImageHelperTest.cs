using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.GUI.Testing
{
	sealed class ImageHelperTest : TestCase
	{
		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestRotateImageByExifOrientationIfRequired()
		{
			var testImagesPath = Path.Combine(BaseSourcePath, "Enterprise", "Architecture", "GUI", "Enterprise.ZArchitecture.GUI.Test", "Testing", "TestRotateImageByExifOrientationIfRequired");
			var inputImageDirectory = new DirectoryInfo(testImagesPath);
			var expectedImage = Image.FromFile(Path.Combine(testImagesPath, "expected.jpg"));

			foreach (var file in inputImageDirectory.GetFiles("F*.jpg"))
			{
				var inputImage = Image.FromFile(file.FullName);
				Assert("EXIF orientation property should be present in the input image", inputImage.PropertyIdList.Contains(ImageHelper.ExifOrientationId));

				var actualImage = ImageHelper.GetRotatedImageByExifOrientationIfRequired(file.FullName);
				Assert("EXIF orientation property should have been removed", !actualImage.PropertyIdList.Contains(ImageHelper.ExifOrientationId));
				AssertImageEquals(string.Format(CultureInfo.InvariantCulture, "Actual image is different from expected image ({0})", file.Name), expectedImage, actualImage, 1);
			}
		}
	}
}
