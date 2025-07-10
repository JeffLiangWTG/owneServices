using System.Linq;
using System.Windows.Forms;
using Enterprise.VisualBoards.Business;
using Enterprise.VisualBoards.Business.Test;
using NUnit.Framework;

namespace Enterprise.VisualBoards.GUI.Test
{
	[TestedType(typeof(BoardMeetingModeFilter))]
	class BoardMeetingModeFilterTest : BoardFilterTestCase<BoardMeetingModeFilter>
	{
		public override void TestFilterName()
		{
			AssertEquals("Board Meeting", new BoardMeetingModeFilter().FilterName);
		}

		public override void TestAllowMultiple()
		{
			AssertEquals(false, new BoardMeetingModeFilter().AllowMultiple);
		}

		public override void TestEquals()
		{
			AssertEquals("Any instance should equal another", new BoardMeetingModeFilter(), new BoardMeetingModeFilter());
		}

		protected override BoardMeetingModeFilter GetFilter()
		{
			return new BoardMeetingModeFilter();
		}

		public override void TestWhenRemovedThroughFilterManager_ShouldRestoreVisualState()
		{
			var system = VisualBoardsTestHelper.CreateSystem(Factory, "ORG");
			var bucket = VisualBoardsTestHelper.CreateBucket(system);
			var section = VisualBoardsTestHelper.CreateBoardSection(bucket);

			Factory.Save();

			using (DisableAsyncBehaviour())
			using (var form = new VisualBoardForm(VisualBoardFormBasherTest.GetViewModel(section.Board)))
			{
				form.Show();
				Application.DoEvents();

				form.EnterBoardMeetingMode();
				var boardMeetingModeForm = Application.OpenForms.OfType<BoardMeetingModeForm>().SingleOrDefault();

				AssertNotNull("Board meeting mode form should be shown", boardMeetingModeForm);

				form.SlideShowViewModel.FilterManager.Clear();

				boardMeetingModeForm = Application.OpenForms.OfType<BoardMeetingModeForm>().SingleOrDefault();
				AssertNull("Board meeting mode form not should be shown", boardMeetingModeForm);
			}
		}
	}
}
