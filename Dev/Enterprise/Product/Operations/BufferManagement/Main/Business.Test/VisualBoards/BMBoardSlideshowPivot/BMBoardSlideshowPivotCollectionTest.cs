using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.BufferManagement.Business.Test
{
	[TestedType(typeof(BMBoardSlideshowPivotCollection))]
	public class BMBoardSlideshowPivotCollectionTest : ActiveBusinessObjectCollectionTestCase<BMBoardSlideshowPivotCollection>
	{
		public void TestDefaultsForNewChild()
		{
			var system = Factory.New<BMSystem>();
			var board = system.Boards.AddNew();

			var system2 = Factory.New<BMSystem>();
			var board2 = system2.Boards.AddNew();

			var slideShow = Factory.New<BMBoardSlideshow>();
			var pivot1 = slideShow.BoardPivots.AddNew();
			AssertEquals((short)1, pivot1.MC_Sequence);
			pivot1.SystemPK = system.PK;

			var pivot2 = slideShow.BoardPivots.AddNew();
			AssertEquals((short)2, pivot2.MC_Sequence);
			AssertEquals(system.PK, pivot2.SystemPK);

			pivot2.MC_Sequence = 33;
			pivot2.SystemPK = system2.PK;

			var pivot3 = slideShow.BoardPivots.AddNew();
			AssertEquals((short)34, pivot3.MC_Sequence);
			AssertEquals(system2.PK, pivot3.SystemPK);
		}

		protected override BMBoardSlideshowPivotCollection GetCollectionToTest()
		{
			var slideShow = Factory.New<BMBoardSlideshow>();
			return new BMBoardSlideshowPivotCollection(slideShow);
		}
	}
}
