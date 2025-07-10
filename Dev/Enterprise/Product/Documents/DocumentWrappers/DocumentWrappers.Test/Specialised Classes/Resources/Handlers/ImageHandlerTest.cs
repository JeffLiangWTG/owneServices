using System.Drawing;
using System.IO;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.DocumentWrappers.Resources.Handlers.Testing
{
	sealed class ImageHandlerTest : TestCaseWithFactory
	{
		public void TestGetImageWithResourcePath()
		{
			ImageHandlerTestClass imageHandler = new ImageHandlerTestClass();

			AssertEquals("Precondition: Image should be null.", null, imageHandler.GetImageWithResourcePath(ZString.Empty));

			var basePath = "Enterprise.DocumentWrappers.Testing.Specialised_Classes.Resources.Raw.";
			var testImage = basePath + "TestImage.gif";
			using (Stream fStream = GetType().Assembly.GetManifestResourceStream(testImage))
			{
				Image expectedImage = Image.FromStream(fStream);
				Image imageFromHandler = imageHandler.GetImageWithResourcePath(testImage);
				AssertNotNull(expectedImage);
				AssertNotNull(imageFromHandler);
				Assert("The two images should have the same width", expectedImage.Width == imageFromHandler.Width);
				Assert("The two images should have the same height", expectedImage.Height == imageFromHandler.Height);
				Assert("The two images should have the same size", expectedImage.Size == imageFromHandler.Size);
			}
		}

		#region Implementation

		public class ImageHandlerTestClass : ImageHandler
		{
			public new Image GetImageWithResourcePath(ZString path)
			{
				return base.GetImageWithResourcePath(path);
			}
		}

		#endregion
	}
}
