using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.BufferManagement.Business.Test
{
	[TestedType(typeof(BMBoardSlideshowCollection))]
	public class BMBoardSlideshowCollectionTest : ActiveBusinessObjectCollectionTestCase<BMBoardSlideshowCollection>
	{
		protected override BMBoardSlideshowCollection GetCollectionToTest()
		{
			return new BMBoardSlideshowCollection(Factory);
		}
	}
}
