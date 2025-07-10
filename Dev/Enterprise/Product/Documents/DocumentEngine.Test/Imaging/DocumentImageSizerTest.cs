using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.IO;
using CargoWise.BuildTools;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.Imaging.Testing
{
	sealed class DocumentImageSizerTest : TestCase
	{
		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestAdjustImageSizeFitCompletely()
		{
			ResizeOption resizeOptionApplied;
			using (var image = Image.FromFile(TestingConstants.TIF_Squares_100dpi))
			{
				var imageSizer = new DocumentImageSizer(image, ResizeOption.FitCompletely);
				var newSize = imageSizer.AdjustImageSize(image.Size);
				AssertEquals("No adjustments should be necessary, use the same size", image.Size, newSize);

				newSize = imageSizer.AdjustImageSize(new Size(1000, 800), out resizeOptionApplied);
				AssertEquals("Image should resize to the smallest axis (x)", new Size(800, 800), newSize);
				AssertEquals(ResizeOption.FitToHeight, resizeOptionApplied);

				newSize = imageSizer.AdjustImageSize(new Size(800, 1000), out resizeOptionApplied);
				AssertEquals("Image should resize to the smallest axis (y)", new Size(800, 800), newSize);
				AssertEquals(ResizeOption.FitToWidth, resizeOptionApplied);
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestAdjustImageSizeFitShorterSide()
		{
			using (var image = Image.FromFile(TestingConstants.TIF_Squares_100dpi_400x200))
			{
				var imageSizer = new DocumentImageSizer(image, ResizeOption.FitShorterSide);
				var newSize = imageSizer.AdjustImageSize(image.Size);
				AssertEquals("No adjustments should be necessary, use the same size", image.Size, newSize);

				newSize = imageSizer.AdjustImageSize(new Size(1000, 800));
				AssertEquals("Image should resize to fit y axis", new Size(1600, 800), newSize);

				newSize = imageSizer.AdjustImageSize(new Size(800, 1000));
				AssertEquals("Image should resize to fit y axis", new Size(2000, 1000), newSize);
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestAdjustImageSizeFitToWidth()
		{
			using (var image = Image.FromFile(TestingConstants.TIF_Squares_100dpi_400x200))
			{
				var imageSizer = new DocumentImageSizer(image, ResizeOption.FitToWidth);
				var newSize = imageSizer.AdjustImageSize(image.Size);
				AssertEquals("no adjustments should be necessary, use the same size", image.Size, newSize);

				newSize = imageSizer.AdjustImageSize(new Size(1000, 800));
				AssertEquals("image should resize to fit width", new Size(1000, 500), newSize);

				newSize = imageSizer.AdjustImageSize(new Size(800, 1000));
				AssertEquals("image should resize to fit width", new Size(800, 400), newSize);
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestAdjustImageSizeFitToHeight()
		{
			using (var image = Image.FromFile(TestingConstants.TIF_Squares_100dpi_400x200))
			{
				var imageSizer = new DocumentImageSizer(image, ResizeOption.FitToHeight);
				var newSize = imageSizer.AdjustImageSize(image.Size);
				AssertEquals("no adjustments should be necessary, use the same size", image.Size, newSize);

				newSize = imageSizer.AdjustImageSize(new Size(1000, 800));
				AssertEquals("image should resize to fit height", new Size(1600, 800), newSize);

				newSize = imageSizer.AdjustImageSize(new Size(800, 1000));
				AssertEquals("image should resize to fit height", new Size(2000, 1000), newSize);
			}
		}

		public void TestAdjustImageSizeFitCompletely_StaticOverload()
		{
			var newSize = DocumentImageSizer.AdjustImageSize(ResizeOption.FitCompletely, new Size(100, 100), 1);
			AssertEquals("No adjustments should be necessary", new Size(100, 100), newSize);

			newSize = DocumentImageSizer.AdjustImageSize(ResizeOption.FitCompletely, new Size(100, 100), 0.5);
			AssertEquals("The image should resize to smallest axis", new Size(50, 100), newSize);

			newSize = DocumentImageSizer.AdjustImageSize(ResizeOption.FitCompletely, new Size(100, 100), 1.25);
			AssertEquals("image should resize to smallest axis", new Size(100, 80), newSize);

			newSize = DocumentImageSizer.AdjustImageSize(ResizeOption.FitCompletely, new Size(200, 100), 0.5);
			AssertEquals("The image should resize to smallest axis", new Size(50, 100), newSize);

			newSize = DocumentImageSizer.AdjustImageSize(ResizeOption.FitCompletely, new Size(200, 100), 1.25);
			AssertEquals("image should resize to smallest axis", new Size(125, 100), newSize);
		}

		public void TestAdjustImageSizeFitShorterSide_StaticOverload()
		{
			var newSize = DocumentImageSizer.AdjustImageSize(ResizeOption.FitShorterSide, new Size(100, 100), 1);
			AssertEquals("No adjustments should be necessary", new Size(100, 100), newSize);

			newSize = DocumentImageSizer.AdjustImageSize(ResizeOption.FitShorterSide, new Size(100, 100), 0.5);
			AssertEquals("image should adjust to smallest axis", new Size(100, 200), newSize);

			newSize = DocumentImageSizer.AdjustImageSize(ResizeOption.FitShorterSide, new Size(200, 100), 0.5);
			AssertEquals("The image should resize to smallest axis", new Size(200, 400), newSize);

			newSize = DocumentImageSizer.AdjustImageSize(ResizeOption.FitShorterSide, new Size(100, 200), 0.5);
			AssertEquals("image should resize to smallest axis", new Size(100, 200), newSize);
		}

		public void TestAdjustImageSizeFitToWidth_StaticOverload()
		{
			var newSize = DocumentImageSizer.AdjustImageSize(ResizeOption.FitToWidth, new Size(100, 100), 1);
			AssertEquals("No adjustments should be necessary", new Size(100, 100), newSize);

			newSize = DocumentImageSizer.AdjustImageSize(ResizeOption.FitToWidth, new Size(100, 100), 0.5);
			AssertEquals("The image should resize to width", new Size(100, 200), newSize);

			newSize = DocumentImageSizer.AdjustImageSize(ResizeOption.FitToWidth, new Size(200, 100), 0.5);
			AssertEquals("The image should resize to width", new Size(200, 400), newSize);

			newSize = DocumentImageSizer.AdjustImageSize(ResizeOption.FitToWidth, new Size(100, 200), 0.5);
			AssertEquals("The image should resize to width", new Size(100, 200), newSize);
		}

		public void TestAdjustImageSizeFitToHeight_StaticOverload()
		{
			var newSize = DocumentImageSizer.AdjustImageSize(ResizeOption.FitToHeight, new Size(100, 100), 1);
			AssertEquals("No adjustments should be necessary", new Size(100, 100), newSize);

			newSize = DocumentImageSizer.AdjustImageSize(ResizeOption.FitToHeight, new Size(100, 100), 0.5);
			AssertEquals("The image should resize to height", new Size(50, 100), newSize);

			newSize = DocumentImageSizer.AdjustImageSize(ResizeOption.FitToHeight, new Size(200, 100), 0.5);
			AssertEquals("The image should resize to height", new Size(50, 100), newSize);

			newSize = DocumentImageSizer.AdjustImageSize(ResizeOption.FitToHeight, new Size(100, 200), 0.5);
			AssertEquals("The image should resize to height", new Size(100, 200), newSize);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestCreateThumbnail_FitShorterSideDefault()
		{
			using (var original = Image.FromFile(TestingConstants.TIF_Resolution200x100))
			{
				var imageSizer = new DocumentImageSizer(original);

				using (var clonedSection = imageSizer.CreateThumbnail(new Size(100, 100)))
				{
					AssertEquals("Width", 100, clonedSection.Width);
					AssertEquals("Height", 133, clonedSection.Height);
					AssertEquals("Resolution should be the same on both axes", clonedSection.HorizontalResolution, clonedSection.VerticalResolution);
				}
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestCreateThumbnail_FitCompletely()
		{
			using (var original = Image.FromFile(TestingConstants.TIF_Resolution200x100))
			{
				var imageSizer = new DocumentImageSizer(original, ResizeOption.FitCompletely);

				using (var clonedSection = imageSizer.CreateThumbnail(new Size(100, 100)))
				{
					AssertEquals("Width", 75, clonedSection.Width);
					AssertEquals("Height", 100, clonedSection.Height);
					AssertEquals("Resolution should be the same on both axes", clonedSection.HorizontalResolution, clonedSection.VerticalResolution);
				}
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestAdjustSizeForResolutionThatNeedsAdjusting()
		{
			using (var image = Image.FromFile(TestingConstants.TIF_Resolution200x100))
			{
				var imageSizer = new DocumentImageSizer(image);
				var adjustedSize = imageSizer.AdjustSizeForResolution();
				AssertEquals("The width should be the same on the adjusted size", image.Width, adjustedSize.Width);
				AssertEquals("the adjusted height should be double the original", image.Height * 2, adjustedSize.Height);
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestAdjustSizeForResolutionThatDoesNotNeedAdjusting()
		{
			using (var image = Image.FromFile(TestingConstants.TIF_Squares_100dpi))
			{
				var imageSizer = new DocumentImageSizer(image);
				AssertEquals("Size from the image should be the same as the adjusted size", image.Size, imageSizer.AdjustSizeForResolution());
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestScaleSizeForResolution()
		{
			using (var original = Image.FromFile(TestingConstants.TIF_Resolution200x100))
			{
				var imageSizer = new DocumentImageSizer(original, ResizeOption.FitShorterSide);
				var scaledRectangle = imageSizer.ScaleIntersectionRectangleForResolution(new RectangleF(10, 10, 100, 100));
				AssertEquals("should return the same width", 100, (int)scaledRectangle.Width);
				AssertEquals("should return half the size for height", 50, (int)scaledRectangle.Height);

				AssertEquals("should return the same X coordinate for height", 10, (int)scaledRectangle.X);
				AssertEquals("should return half the Y coordinate for width", 5, (int)scaledRectangle.Y);
			}
		}

		static class TestingConstants
		{
			[SuppressMessage("CargoWiseOne", "CW1051:Do not use BaseSourcePath", Justification = "Baseline")]
			static readonly string TestDirectory = Path.Combine(BuildConstants.LocalEnterprisePath, @"Enterprise\Product\Documents\DocumentScanning\DocumentScanning.Business.Test\TestDocs");

			public static string TIF_Resolution200x100 = Path.Combine(TestDirectory, "Resolution200x100.tif");
			public static string TIF_Small = Path.Combine(TestDirectory, "small.tif");
			public static string TIF_Squares_100dpi = Path.Combine(TestDirectory, "Squares_100dpi.tif");
			public static string TIF_Squares_100dpi_400x200 = Path.Combine(TestDirectory, "Squares_100dpi_400x200.tif");
			public static string TIF_Squares_200dpi = Path.Combine(TestDirectory, "Squares_200dpi.tif");
			public static string TIF_Test = Path.Combine(TestDirectory, "Test.tif");
			public static string TIF_TwoBarcodes = Path.Combine(TestDirectory, "TwoBarcodes.tif");

			public static string PDF_Sample = Path.Combine(TestDirectory, "Sample.PDF");
		}
	}
}
