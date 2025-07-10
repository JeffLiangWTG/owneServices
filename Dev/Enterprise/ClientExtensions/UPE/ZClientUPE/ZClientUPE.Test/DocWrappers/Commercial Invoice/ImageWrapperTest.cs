using System.Drawing;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Client.UPE.Business.Testing
{
	[TestedType(typeof(ImageWrapper))]
	public class ImageWrapperTest : NonPersistentBusinessObjectTestCase
	{
		public void TestProperties()
		{
			AssertNotNull(wrapper);
			AssertEquals("it should be the same image", image.Size.Height, wrapper.Image.Size.Height);
			AssertEquals("it should be the same image", image.Size.Width, wrapper.Image.Size.Width);
		}

		#region Implementation
		ImageWrapper wrapper;
		Bitmap image;
		protected override void SetUp()
		{
			image = new Bitmap(10, 10);
			wrapper = new ImageWrapper(image);
			base.SetUp();
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return wrapper;
		}
		#endregion
	}
}
