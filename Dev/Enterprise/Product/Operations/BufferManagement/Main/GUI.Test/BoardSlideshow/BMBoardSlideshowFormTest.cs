using System.Linq;
using System.Windows.Forms;
using Enterprise.BufferManagement.Business;
using Enterprise.BufferManagement.Business.Test;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.BufferManagement.GUI.Test
{
	[TestedType(typeof(BMBoardSlideshowForm))]
	public class BMBoardSlideshowFormTest : ZFormBasherTest
	{
		protected override Form GetFormToBashCore()
		{
			return new BMBoardSlideshowForm(Factory.New<BMBoardSlideshow>());
		}

		public void TestEditButton_ShouldShowBoardEditForm()
		{
			var system = BMSTestHelper.CreateSystem(Factory);
			var board1 = BMSTestHelper.CreateBoard(system, "Azeroth Invasion Plan");
			var board2 = BMSTestHelper.CreateBoard(system, "One Night in Karazhan");
			var board3 = BMSTestHelper.CreateBoard(system, "Annihilation of Draenor");

			var slideshow = BMSTestHelper.CreateSlideshow(Factory, board1, board2, board3);

			Factory.Save();

			using (var form = new BMBoardSlideshowForm(slideshow))
			{
				form.Show();

				ZFormModaliser.ShowDialogsInTest = true;

				var button = form.FindAll<ZButton>().Single(x => x.Name == "EditBoardButton");
				AssertEquals("Edit Selected Board", button.Text);

				button.PerformClick();

				var boardForm = (ZForm)ZFormModaliser.LastFormShownForTest;
				AssertNotNull(boardForm);
				AssertEquals(board1.PK, ((BMBoard)boardForm.BusinessEntity).PK);
			}
		}

		public void TestRunSlideShowButton_ShouldBeUseable_EvenInViewMode()
		{
			var system = BMSTestHelper.CreateSystem(Factory);
			var board1 = BMSTestHelper.CreateBoard(system, "Blizzard");
			var board2 = BMSTestHelper.CreateBoard(system, "China");
			var board3 = BMSTestHelper.CreateBoard(system, "Hong Kong");

			var slideshow = BMSTestHelper.CreateSlideshow(Factory, board1, board2, board3);

			Factory.Save();

			using (var form = new BMBoardSlideshowForm(slideshow))
			{
				form.DisplayMode = ZArchitecture.Core.ODisplayMode.ReadOnly;
				form.Show();
				Application.DoEvents();

				var editButton = form.FindAll<ZButton>().Single(x => x.Name == "EditBoardButton");
				AssertEquals("Edit Selected Board", editButton.Text);
				AssertEquals("A normal button is readonly", true, editButton.ReadOnly);

				var runButton = form.FindAll<ZButton>().Single(x => x.Name == "RunSlideShowButton");
				AssertEquals("Run Slide Show", runButton.Text);
				AssertEquals("This button should not be readonly, even though the form is", false, runButton.ReadOnly);
			}
		}
	}
}
