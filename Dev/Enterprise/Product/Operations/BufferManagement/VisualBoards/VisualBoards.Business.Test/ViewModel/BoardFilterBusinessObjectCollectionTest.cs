using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.VisualBoards.Business.Test
{
	[TestedType(typeof(BoardFilterBusinessObjectCollection))]
	class BoardFilterBusinessObjectCollectionTest : NonPersistentBusinessObjectCollectionTestCase<BoardFilterBusinessObjectCollection>
	{
		protected override BoardFilterBusinessObjectCollection GetCollectionToTest()
		{
			return new BoardFilterBusinessObjectCollection(new DummyFilterable());
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new BoardFilterBusinessObject(new TestFilter(), new DummyFilterable());
		}

		public void TestFilterCollectionContainsAllChildFiltersRecursively()
		{
			var system = VisualBoardsTestHelper.CreateSystem(Factory, "ORG");
			var buffer = VisualBoardsTestHelper.CreateBuffer(system);
			var board = VisualBoardsTestHelper.CreateBoard(system, name: "DoodleBoard");
			var section = VisualBoardsTestHelper.CreateBoardSection(buffer, board);

			Factory.Save();

			var slideShowViewModel = VisualBoardsTestHelper.CreateSlideshowViewModel(board);
			var boardViewModel = slideShowViewModel.BoardViewModels[0];
			boardViewModel.Build(board);
			var sectionViewModel = boardViewModel.GetSections().First();

			((IFilterable)sectionViewModel).FilterManager.ApplyFilter(new TestFilter());
			((IFilterable)boardViewModel).FilterManager.ApplyFilter(new TestFilter());
			((IFilterable)slideShowViewModel).FilterManager.ApplyFilter(new TestFilter());

			var collection = new BoardFilterBusinessObjectCollection(slideShowViewModel);

			AssertEquals(3, collection.Count);
		}
	}
}
