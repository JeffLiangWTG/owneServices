using CargoWise.EntityFramework.Testing;

namespace Enterprise.BufferManagement.Business.Test
{
	internal class BMBoardSlideshowPivotLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestBoards()
		{
			var system = Factory.New<BMSystem>();
			var board1 = system.Boards.AddNew();
			var board2 = system.Boards.AddNew();

			var system2 = Factory.New<BMSystem>();
			var board3 = system2.Boards.AddNew();

			var slideShow = Factory.New<BMBoardSlideshow>();
			var pivot = slideShow.BoardPivots.AddNew();

			AssertEquals(0, pivot.Lookups.Boards.Count);

			pivot.SystemPK = system.PK;
			AssertEquals(2, pivot.Lookups.Boards.Count);

			pivot.SystemPK = system2.PK;
			AssertEquals(1, pivot.Lookups.Boards.Count);
		}
	}
}
