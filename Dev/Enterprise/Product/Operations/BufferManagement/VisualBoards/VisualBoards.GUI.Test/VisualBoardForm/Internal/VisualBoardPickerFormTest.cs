using System.Linq;
using System.Windows.Forms;
using Enterprise.BufferManagement.Business.Test;
using Enterprise.VisualBoards.Business.Test;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.VisualBoards.GUI.Test
{
	class VisualBoardPickerFormTest : VisualBoardsTestCase
	{
		public void TestOpenButton_ShouldOpenNewBoard()
		{
			var system = BMSTestHelper.CreateSystem(Factory, "ORG");
			var board1 = BMSTestHelper.CreateBoard(system, "Bilgewater Cartel");
			var board2 = BMSTestHelper.CreateBoard(system, "Steamwheedle Cartel");

			Factory.Save();

			var viewModel = BoardPickerViewModelTest.GetViewModelForBoards(board1.PK, board1, board2);

			using (DisableAsyncBehaviour())
			using (var pickerForm = new VisualBoardPickerForm(viewModel))
			{
				pickerForm.Show();
				Application.DoEvents();

				AssertEquals(2, pickerForm.BusinessEntity.Boards.Count);

				pickerForm.BusinessEntity.BoardPK = board2.PK;

				((Button)pickerForm.Controls.Find("OpenButton", true)[0]).PerformClick();
				Application.DoEvents();

				var openForm = VisualBoardsFormTestHelper.GetOpenForms<VisualBoardForm>().SingleOrDefault();
				AssertNotNull(openForm);
				var openFormViewModelBoardName = openForm.BoardViewModel?.BoardName;
				var openFormViewModelBoardPK = openForm.BoardViewModel?.BoardPK;
				openForm.Dispose();

				AssertEquals($"Should have opened the Steamwheedle Cartel board, yet why is the {openFormViewModelBoardName} the one displayed, I wonder...", board2.PK, openFormViewModelBoardPK);
			}
		}
	}

	[TestedType(typeof(VisualBoardPickerForm))]
	class VisualBoardPickerFormBasherTest : ZFormBasherTest
	{
		protected override Form GetFormToBashCore()
		{
			var system = VisualBoardsTestHelper.CreateSystem(Factory);
			var board = system.Boards.AddNew();

			Factory.Save();

			var viewModel = BoardPickerViewModelTest.GetViewModelForBoards(board);

			return new VisualBoardPickerForm(viewModel);
		}
	}
}
