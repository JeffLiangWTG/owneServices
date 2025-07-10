using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.BufferManagement.Business;

namespace Enterprise.VisualBoards.Business.Test
{
	class BoardPickerViewModelValidationTest : BusinessObjectValidationTestCase
	{
		public void TestValidateBoardPK()
		{
			const string boardPKMissingError = "Please enter a value.";
			const string invalidBoardPKError = "Enter a valid selection.";

			var system = Factory.New<BMSystem>();
			var board = system.Boards.AddNew();
			var viewModel = GetViewModel(board);

			viewModel.BoardPK = ZGuid.Empty;
			AssertHasError(viewModel.BoardPKInfo, boardPKMissingError);

			viewModel.BoardPK = ZGuid.Invalid;
			AssertHasError(viewModel.BoardPKInfo, invalidBoardPKError);

			viewModel.BoardPK = board.PK;
			AssertNoError(viewModel.BoardPKInfo, boardPKMissingError);
			AssertNoError(viewModel.BoardPKInfo, invalidBoardPKError);
		}

		BoardPickerViewModel GetViewModel(BMBoard board)
		{
			return BoardPickerViewModelTest.GetViewModelForBoards(board.PK, board);
		}
	}
}
