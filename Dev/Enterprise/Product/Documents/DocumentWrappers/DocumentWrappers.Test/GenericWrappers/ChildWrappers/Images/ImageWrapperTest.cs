using System.Drawing;
using CargoWise.Types;
using Enterprise.DocumentWrappers.GenericWrappers.Base;
using Enterprise.DocumentWrappers.GenericWrappers.Base.Testing;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.GenericWrappers.Testing
{
	[TestedType(typeof(ImageWrapper))]
	sealed class ImageWrapperTest : GenericWrapperTest
	{
		public override void TestWrapperMappingsEmpty()
		{
			ImageWrapper wrapper = (ImageWrapper)GetNewDocumentWrapper();
			AssertEquals(Image, wrapper.Image);
		}

		#region Implementation

		protected override ZString ExpectedDefaultFormatting
		{
			get
			{
				return @"
Registry : (No Default Field Value Available on Registry)
";
			}
		}

		protected override GenericWrapper GetSetupWrapperForDefaultFormatting()
		{
			return new ImageWrapper(Image, Factory);
		}

		protected override string ExpectedFieldMap
		{
			get
			{
				return @"
Image                                           (Default Field: Image)
======================================================================
Name                                    Type
----------------------------------------------------------------------
";
			}
		}

		protected override DocBaseWrapper GetNewDocumentWrapper()
		{
			return new ImageWrapper(Image, Factory);
		}

		protected override void TearDown()
		{
			base.TearDown();

			if (image != null)
			{
				image.Dispose();
			}
		}

		Image Image
		{
			get { return image ?? (image = new Bitmap(20, 20)); }
		}
		Image image;

		#endregion
	}
}
