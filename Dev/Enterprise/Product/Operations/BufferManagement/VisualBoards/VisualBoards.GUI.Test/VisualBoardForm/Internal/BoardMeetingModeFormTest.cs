using System.Windows.Forms;
using Enterprise.BufferManagement.Business;
using Enterprise.BufferManagement.Business.Test;
using Enterprise.VisualBoards.Business;
using Enterprise.VisualBoards.Business.Test;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.VisualBoards.GUI.Test
{
	[TestedType(typeof(BoardMeetingModeForm))]
	class BoardMeetingModeFormTest : ZFormBasherTest
	{
		public void TestCloseForm_ShouldEndBoardMeetingMode()
		{
			var system = Factory.NewWithValidTestData<BMSystem>();
			var board = system.Boards.AddNew();
			var bucket = BMSTestHelper.CreateBucket(system, "Bucket");
			var section = BMSTestHelper.CreateBoardSection(bucket, board);

			Factory.Save();

			var viewModel = VisualBoardsTestHelper.CreateSlideshowViewModel(board);

			using (VisualBoardsTestCase.DisableAsyncBehaviour())
			using (var visualBoardForm = new VisualBoardForm(viewModel))
			using (var form = new BoardMeetingModeForm(visualBoardForm))
			{
				form.Show();
				visualBoardForm.EnterBoardMeetingMode();

				AssertEquals(true, viewModel.CurrentBoardViewModel.FilterManager.IsApplied(typeof(BoardMeetingModeFilter)));

				form.Close();
				AssertEquals(false, viewModel.CurrentBoardViewModel.FilterManager.IsApplied(typeof(BoardMeetingModeFilter)));
			}
		}

		protected override Form GetFormToBashCore()
		{
			return new BoardMeetingModeForm();
		}
	}
}
