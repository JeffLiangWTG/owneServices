using System.Drawing;
using System.IO;
using CargoWise.BrandManager;
using Enterprise.Faxing.Engine;
using Enterprise.Faxing.Integration;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Faxing.Testing
{
	sealed class TiffPageRotatorTest : TestCase
	{
		[DatCapabilityRequirement("SOURCE_CODE")]
		[TestTargetPlatform(TargetPlatform.x86)]
		public void TestRotateLandscapeToPortrait()
		{
			using (var tf = TempFile.NewFromFile(Path.Combine(testFilesPath, "rotation.tif")))
			{
				var sourceInfo = ImageInfo.GetImageInfo(tf.Filename);

				var rotator = new TiffPageRotator();
				Assert(rotator.HasLandscapePages(tf.Filename));
				rotator.RotateLandscapeToPortrait(tf.Filename);
				Assert(!rotator.HasLandscapePages(tf.Filename));

				var resultInfo = ImageInfo.GetImageInfo(tf.Filename);
				AssertEquals(sourceInfo.HorizontalResolution, resultInfo.HorizontalResolution);
				AssertEquals(sourceInfo.VerticalResolution, resultInfo.VerticalResolution);
				AssertEquals(sourceInfo.Compression, resultInfo.Compression);
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		[TestTargetPlatform(TargetPlatform.x86)]
		public void TestRotateLandscapeToPortrait2()
		{
			using (var tf = TempFile.NewFromFile(Path.Combine(testFilesPath, "Magellan.tif")))
			{
				var sourceInfo = ImageInfo.GetImageInfo(tf.Filename);

				var rotator = new TiffPageRotator();
				Assert(rotator.HasLandscapePages(tf.Filename));
				rotator.RotateLandscapeToPortrait(tf.Filename);
				Assert(!rotator.HasLandscapePages(tf.Filename));

				var resultInfo = ImageInfo.GetImageInfo(tf.Filename);
				AssertEquals(sourceInfo.HorizontalResolution, resultInfo.HorizontalResolution);
				AssertEquals(sourceInfo.VerticalResolution, resultInfo.VerticalResolution);
				AssertEquals(sourceInfo.Compression, resultInfo.Compression);
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestExceptionRaised()
		{
			using (var tf = TempFile.NewFromFile(Path.Combine(testFilesPath, "Magellan.tif")))
			{
				var sourceInfo = ImageInfo.GetImageInfo(tf.Filename);
				var rotator = new TiffPageRotator();
				try
				{
					rotator.HasLandscapePages(tf.Filename);
				}
				catch (FaxRouterException32Bit e)
				{
					AssertEquals($"{BrandingFactory.Instance.ProductName} 64-bit does not support Fax Routing please switch to {BrandingFactory.Instance.ProductName} 32-bit", e.Message);
				}
				try
				{
					rotator.RotateLandscapeToPortrait(tf.Filename);
				}
				catch (FaxRouterException32Bit e)
				{
					AssertEquals($"{BrandingFactory.Instance.ProductName} 64-bit does not support Fax Routing please switch to {BrandingFactory.Instance.ProductName} 32-bit", e.Message);
				}
			}
		}

		readonly string testFilesPath = Path.Combine(BaseSourcePath, "Enterprise", "Product", "Documents", "Faxing", "Faxing.Test", "TestFiles");

		sealed class ImageInfo
		{
			internal static ImageInfo GetImageInfo(string filename)
			{
				using (var img = Image.FromFile(filename))
				{
					var result = new ImageInfo()
					{
						VerticalResolution = img.VerticalResolution,
						HorizontalResolution = img.HorizontalResolution,
						Compression = img.GetPropertyItem(0x103).Value[0]
					};

					return result;
				}
			}

			ImageInfo() { }

			public float HorizontalResolution { get; private set; }
			public float VerticalResolution { get; private set; }
			public int Compression { get; private set; }
		}
	}
}
