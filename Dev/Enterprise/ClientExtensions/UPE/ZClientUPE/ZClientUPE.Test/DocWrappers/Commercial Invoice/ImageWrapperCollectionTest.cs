using System.Drawing;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Client.UPE.Business.Testing
{
	[TestedType(typeof(ImageWrapperCollection))]
	public class ImageWrapperCollectionTest : NonPersistentBusinessObjectCollectionTestCase<ImageWrapperCollection>
	{
		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			var image = new Bitmap(10, 11);
			return new ImageWrapper(image);
		}

		protected override ImageWrapperCollection GetCollectionToTest()
		{
			return new ImageWrapperCollection(Factory);
		}
	}
}
