using System.Drawing;
using Enterprise.DocumentWrappers.GenericWrappers.Base;
using Enterprise.DocumentWrappers.GenericWrappers.Base.Testing;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.GenericWrappers.Testing
{
	[TestedType(typeof(ImageWrapperCollection))]
	sealed class ImageWrapperCollectionTest : GenericWrapperCollectionTest<ImageWrapperCollection>
	{
		public void TestNewEmpty()
		{
			ImageWrapperCollection collection = new ImageWrapperCollection(Factory);
			AssertEquals(0, collection.Count);
		}

		public void TestNewPopulated()
		{
			using (Image image1 = new Bitmap(20, 20))
			using (Image image2 = new Bitmap(20, 20))
			using (Image image3 = new Bitmap(20, 20))
			{
				ImageWrapperCollection collection = new ImageWrapperCollection(new Image[]
				{
					image1,
					image2,
					null,
					image3,
				}, Factory);

				AssertEquals(3, collection.Count);
				AssertEquals(image1, collection[0].Image);
				AssertEquals(image2, collection[1].Image);
				AssertEquals(image3, collection[2].Image);
			}
		}

		#region Implementation

		protected override GenericWrapper GetNewWrapperToAddToTheCollection()
		{
			return new ImageWrapper(new Bitmap(10, 10), Factory);
		}

		protected override ImageWrapperCollection GetNewDocumentWrapperCollection()
		{
			return new ImageWrapperCollection(Factory);
		}

		#endregion
	}
}
