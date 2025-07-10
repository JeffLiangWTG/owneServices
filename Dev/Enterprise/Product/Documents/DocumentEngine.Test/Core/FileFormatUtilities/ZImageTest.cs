using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using Enterprise.DocumentEngine.Testing.UtilityClasses;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.FileFormatUtilities.Testing
{
	sealed class ZImageTest : TransactionedTestCase
	{
		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestFromStream()
		{
			using (var streamReader = new StreamReader(UnitTestingConstants.DifferentHorizontalAndVerticalResolutionFile))
			using (var image = ZImage.FromStream(streamReader.BaseStream))
			{
				AssertNotNull("Image shouldn't be null", image);
				Assert("Image should be created; width dimensions should be valid", image.Width > 0);
				Assert("Image should be created; height dimensions should be valid", image.Height > 0);
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestFromFile()
		{
			using (var image = ZImage.FromFile(UnitTestingConstants.DifferentHorizontalAndVerticalResolutionFile))
			{
				AssertNotNull("Image shouldn't be null", image);
				Assert("Image should be created; width dimensions should be valid", image.Width > 0);
				Assert("Image should be created; height dimensions should be valid", image.Height > 0);
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestIsA4LandscapePage()
		{
			using (var zImage = ZImage.FromFile(UnitTestingConstants.MultipageWithLandscapeFirstPageFile))
			{
				Assert("First page should be landscape", zImage.IsA4LandscapePage);

				zImage.CurrentPageIndex = 1;
				Assert("Second page shouldn't be landscape", !zImage.IsA4LandscapePage);

				zImage.CurrentPageIndex = 2;
				Assert("Third page should be landscape", zImage.IsA4LandscapePage);

				zImage.CurrentPageIndex = 3;
				Assert("Fourth page shouldn't be landscape", !zImage.IsA4LandscapePage);

				zImage.CurrentPageIndex = 4;
				Assert("Fifth page shouldn't be landscape", !zImage.IsA4LandscapePage);

				zImage.CurrentPageIndex = 5;
				Assert("Sixth page shouldnt' be landscape", !zImage.IsA4LandscapePage);
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestIsA4Page()
		{
			using (var zImage = ZImage.FromFile(UnitTestingConstants.MultipageWithLandscapeFirstPageFile))
			{
				Assert("First page should be a4", zImage.IsA4Page);

				zImage.CurrentPageIndex = 1;
				Assert("Second page should be a4", zImage.IsA4Page);

				zImage.CurrentPageIndex = 2;
				Assert("Third page should be a4", zImage.IsA4Page);

				zImage.CurrentPageIndex = 3;
				Assert("Fourth page should be a4", zImage.IsA4Page);

				zImage.CurrentPageIndex = 4;
				Assert("Fifth page shouldn't be a4", !zImage.IsA4Page);

				zImage.CurrentPageIndex = 5;
				Assert("Sixth page shouldnt' be a4", !zImage.IsA4Page);
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestPageCount()
		{
			using (var zImage = ZImage.FromFile(UnitTestingConstants.MultipageWithLandscapeFirstPageFile))
			{
				AssertEquals("PageCount", zImage.InternalImage_Exposed.GetFrameCount(FrameDimension.Page), zImage.PageCount);
				AssertEquals("PageCount", 6, zImage.PageCount);
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestCurrentPageIndex()
		{
			using (var zImage = ZImage.FromFile(UnitTestingConstants.MultipageWithLandscapeFirstPageFile))
			{
				AssertEquals("Page index should default to 0", 0, zImage.CurrentPageIndex);
				AssertEquals("Precondition: first page should be landscape", true, zImage.IsA4LandscapePage);

				zImage.CurrentPageIndex = 3;
				AssertEquals("Page index should change", 3, zImage.CurrentPageIndex);
				AssertEquals("selected page should have changed - no longer landscape", false, zImage.IsA4LandscapePage);

				zImage.CurrentPageIndex = 17;
				AssertEquals("Page index shouldn't change if it's beyond the page count", 3, zImage.CurrentPageIndex);

				zImage.CurrentPageIndex = -2;
				AssertEquals("Page index shouldn't change if it's beyond the page count", 3, zImage.CurrentPageIndex);
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestWidth()
		{
			using (var zImage = ZImage.FromFile(UnitTestingConstants.MultipageWithLandscapeFirstPageFile))
			{
				AssertEquals("Width should reflect width of internal image", zImage.InternalImage_Exposed.Width, zImage.Width);

				zImage.CurrentPageIndex = 1;
				AssertEquals("Width should reflect width of internal image even after page change", zImage.InternalImage_Exposed.Width, zImage.Width);
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestHeight()
		{
			using (var zImage = ZImage.FromFile(UnitTestingConstants.MultipageWithLandscapeFirstPageFile))
			{
				AssertEquals("Height should reflect height of internal image", zImage.InternalImage_Exposed.Height, zImage.Height);

				zImage.CurrentPageIndex = 1;
				AssertEquals("Height should reflect height of internal image", zImage.InternalImage_Exposed.Height, zImage.Height);
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestVerticalResolution()
		{
			using (var zImage = ZImage.FromFile(UnitTestingConstants.DifferentHorizontalAndVerticalResolutionFile))
			{
				AssertEquals("VerticalResolution should reflect that of internal image", zImage.InternalImage_Exposed.VerticalResolution, zImage.VerticalResolution);
				AssertEquals("VerticalResolution", 100f, zImage.VerticalResolution);
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestHorizontalResolution()
		{
			using (var zImage = ZImage.FromFile(UnitTestingConstants.DifferentHorizontalAndVerticalResolutionFile))
			{
				AssertEquals("HorizontalResolution should reflect that of internal image", zImage.InternalImage_Exposed.HorizontalResolution, zImage.HorizontalResolution);
				AssertEquals("HorizontalResolution", 200f, zImage.HorizontalResolution);
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestSize()
		{
			using (var zImage = ZImage.FromFile(UnitTestingConstants.DifferentHorizontalAndVerticalResolutionFile))
			{
				AssertEquals("Size should reflect that of internal image", zImage.InternalImage_Exposed.Size, zImage.Size);
				AssertEquals("Size", new Size(1728, 1152), zImage.Size);
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestPhysicalDimension()
		{
			using (var zImage = ZImage.FromFile(UnitTestingConstants.DifferentHorizontalAndVerticalResolutionFile))
			{
				AssertEquals("PhysicalDimension should reflect that of internal image", zImage.InternalImage_Exposed.PhysicalDimension, zImage.PhysicalDimension);
				AssertEquals("PhysicalDimension", new SizeF(1728, 1152), zImage.PhysicalDimension);
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestPixelFormat()
		{
			using (var zImage = ZImage.FromFile(UnitTestingConstants.DifferentHorizontalAndVerticalResolutionFile))
			{
				AssertEquals("PixelFormat should reflect that of internal image", zImage.InternalImage_Exposed.PixelFormat, zImage.PixelFormat);
				AssertEquals("PixelFormat", PixelFormat.Format1bppIndexed, zImage.PixelFormat);
			}

			using (var zImage = ZImage.FromFile(UnitTestingConstants.MultipageWithLandscapeFirstPageFile))
			{
				AssertEquals("PixelFormat should reflect that of internal image", zImage.InternalImage_Exposed.PixelFormat, zImage.PixelFormat);
				AssertEquals("PixelFormat", PixelFormat.Format1bppIndexed, zImage.PixelFormat);

				zImage.CurrentPageIndex = 4;
				AssertEquals("Pixel format", PixelFormat.Format8bppIndexed, zImage.PixelFormat);
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestRawFormat()
		{
			using (var zImage = ZImage.FromFile(UnitTestingConstants.DifferentHorizontalAndVerticalResolutionFile))
			{
				AssertEquals("RawFormat should reflect that of internal image", zImage.InternalImage_Exposed.RawFormat, zImage.RawFormat);
				AssertEquals("RawFormat", ImageFormat.Tiff, zImage.RawFormat);
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestRotateAndClonePage()
		{
			using (var zImage = ZImage.FromFile(UnitTestingConstants.MultipageWithLandscapeFirstPageFile))
			{
				AssertEquals("Precondition: first page is landscape", true, zImage.IsA4LandscapePage);
				using (var result = zImage.RotateAndClonePage(0, RotateFlipType.Rotate90FlipNone))
				{
					AssertEquals("result page should no longer be landscape", false, result.IsA4LandscapePage);
					AssertEquals("result page should be only the cloned page", 1, result.PageCount);
					AssertEquals("original should remain landscape", true, zImage.IsA4LandscapePage);

					zImage.CurrentPageIndex = 1;
					AssertEquals("the rest of the pages have remained the original orientation", false, zImage.IsA4LandscapePage);
					zImage.CurrentPageIndex = 2;
					AssertEquals("the rest of the pages have remained the original orientation", true, zImage.IsA4LandscapePage);
					zImage.CurrentPageIndex = 3;
					AssertEquals("the rest of the pages have remained the original orientation", false, zImage.IsA4LandscapePage);
					zImage.CurrentPageIndex = 4;
					Assert("the rest of the pages have remained the original orientation", zImage.Width > zImage.Height);
					zImage.CurrentPageIndex = 5;
					Assert("the rest of the pages have remained the original orientation", zImage.Height > zImage.Width);
				}
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestInternalImageNotNullFromStream()
		{
			using (var streamReader = new StreamReader(UnitTestingConstants.DifferentHorizontalAndVerticalResolutionFile))
			using (var image = ZImage.FromStream(streamReader.BaseStream))
			{
				AssertNotNull("Internal image should exist", image.InternalImage_Exposed);
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestInternalImageNotNullFromFile()
		{
			using (var image = ZImage.FromFile(UnitTestingConstants.DifferentHorizontalAndVerticalResolutionFile))
			{
				AssertNotNull("Internal image should exist", image.InternalImage_Exposed);
			}
		}

		[ExpectNoExceptions]
		public void TestInternalImageWithEmptyStringDoesNotThrowExceptions()
		{
			using (var image = ZImage.FromFile(string.Empty))
			{
			}
		}
	}
}
