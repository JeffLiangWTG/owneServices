using System.Linq;
using System.Windows.Forms;
using Enterprise.VisualBoards.Business;
using Enterprise.VisualBoards.Business.Test;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.VisualBoards.GUI.Test
{
	[TestedType(typeof(BoardFiltersForm))]
	class BoardFiltersFormTest : ZFormBasherTest
	{
		public void TestClearAllFilters_ShouldRemoveBoardAndSectionFilters()
		{
			var system = VisualBoardsTestHelper.CreateSystem(Factory);
			var group = VisualBoardsTestHelper.CreateGroup(Factory, "AAA");
			var bucket = VisualBoardsTestHelper.CreateBucket(system);
			var section = VisualBoardsTestHelper.CreateBoardSection(bucket);
			section.SectionConfiguration.ReleaseGroupPK = group.PK;

			Factory.Save();

			using (VisualBoardsTestCase.DisableAsyncBehaviour())
			using (var form = new VisualBoardForm(VisualBoardFormBasherTest.GetViewModel(section.Board)))
			{
				form.Show();
				Application.DoEvents();

				form.EnterBoardMeetingMode();
				var bucketControl = (Control)form.FindAll<IBoardSectionControl>().Single();
				bucketControl.ContextMenuStrip.Items[0].PerformClick();

				var boardViewModel = form.BoardViewModel;
				var sectionViewModel = boardViewModel.GetSections().Single();

				AssertEquals(1, form.SlideShowViewModel.FilterManager.AppliedFilters.Count());
				AssertEquals(1, boardViewModel.FilterManager.AppliedAndInheritedFilters.Count());
				AssertEquals(2, sectionViewModel.FilterManager.AppliedAndInheritedFilters.Count());

				using (var filterForm = new BoardFiltersForm(form.SlideShowViewModel))
				{
					filterForm.Show();

					AssertEquals(2, filterForm.FiltersGrid.List.Count);

					var deleteMenuItem = (ZMenuItem)filterForm.FiltersGrid.ContextMenu.MenuItems.Cast<MenuItem>().Single(m => m.Text == "&Delete");
					Assert("Delete menu item should not be readonly, because we might want to remove singe filter from the grid.", deleteMenuItem.Enabled);

					filterForm.Controls.Find("ClearAllFiltersButton", true).OfType<ZButton>().Single().PerformClick();

					AssertEquals(true, filterForm.IsDisposed);
					AssertEquals(0, form.SlideShowViewModel.FilterManager.AppliedFilters.Count());
					AssertEquals(0, boardViewModel.FilterManager.AppliedAndInheritedFilters.Count());
					AssertEquals(0, sectionViewModel.FilterManager.AppliedAndInheritedFilters.Count());
				}
			}
		}

		protected override Form GetFormToBashCore()
		{
			return new BoardFiltersForm(new DummyFilterable());
		}
	}
}
