using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.BufferManagement.Business;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.VisualBoards.Business.Test
{
	[TestedType(typeof(BoardPickerViewModel))]
	public class BoardPickerViewModelTest : NonPersistentBusinessObjectTestCase
	{
		public void TestBoards()
		{
			var system = Factory.New<BMSystem>();
			var board1 = system.Boards.AddNew();
			var board2 = system.Boards.AddNew();

			board1.MB_Name = "Board 1";
			board2.MB_Name = "Board 2";

			var viewModel = GetViewModelForBoards(board1.PK, board1);
			AssertContainsExactElementsInAnyOrder(new[] { "Board 1" }, viewModel.Boards.GetAllCodes());
			AssertEquals(board1, viewModel.GetBoard(Factory));

			viewModel = GetViewModelForBoards(board2.PK, board1, board2);
			AssertContainsExactElementsInAnyOrder(new[] { "Board 1", "Board 2" }, viewModel.Boards.GetAllCodes());
			AssertEquals(board2, viewModel.GetBoard(Factory));
		}

		public void TestBoards_ShouldNotHitDb()
		{
			var system = Factory.New<BMSystem>();
			system.FS_Name = "System";
			var board = system.Boards.AddNew();
			board.MB_Name = "Board";
			Factory.Save();
			Factory.ResetDatabaseLoadCount();

			using (AssertDbHitsForAllFactories("Creating the view model and showing the list of boards shouldn't need the database at all. SAD!", new Dictionary<string, int>()))
			{
				var viewModel = GetViewModelForBoards(board);
				AssertContainsExactElementsInAnyOrder(new[] { "Board" }, viewModel.Boards.GetAllCodes());
			}
		}

		public void TestBoards_ShouldContainAllBoardsProvided_RegardlessOfSystem()
		{
			var system1 = Factory.New<BMSystem>();
			var system2 = Factory.New<BMSystem>();

			var board1 = system1.Boards.AddNew();
			var board2 = system2.Boards.AddNew();

			board1.MB_Name = "Board 1";
			board2.MB_Name = "Board 2";

			var viewModel = GetViewModelForBoards(board1, board2);

			AssertContainsExactElementsInAnyOrder("Boards from all systems should be included in the view model. SAD!", new[] { "Board 1", "Board 2" }, viewModel.Boards.GetAllCodes());
		}

		#region Implementation

		protected override BusinessObject GetNewBusinessObject()
		{
			var system = VisualBoardsTestHelper.CreateSystem(Factory);
			var board = system.Boards.AddNew();

			return GetViewModelForBoards(board.PK, board);
		}

		#region Helper Methods

		public static BoardPickerViewModel GetViewModelForBoards(params BMBoard[] boards)
		{
			return GetViewModelForBoards(ZGuid.Empty, boards);
		}

		public static BoardPickerViewModel GetViewModelForBoards(ZGuid selectedBoardPk, params BMBoard[] boards)
		{
			var sortedBoards = boards.OrderBy(x => x.MB_Name);
			var elementsForList = sortedBoards.Select(x => BoardPickerViewModel.GetElementForBoardList(x.PK.ToGuid(), x.MB_Name)).ToArray();
			var boardList = new CodeDescriptionPairList();
			boardList.AddRange(elementsForList);

			var viewModel = new BoardPickerViewModel(boardList);

			if (selectedBoardPk.IsValid)
			{
				viewModel.BoardPK = selectedBoardPk;
			}

			return viewModel;
		}

		#endregion

		#endregion
	}
}
