using System.Linq;
using System.Windows.Forms;
using Enterprise.VisualBoards.Business;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.VisualBoards.GUI.Test
{
	[TestedType(typeof(SearchFilter))]
	class SearchFilterTest : BoardFilterTestCase<SearchFilter>
	{
		public override void TestFilterName()
		{
			AssertEquals("Search for 'Poodle'", new SearchFilter("Poodle").FilterName);
		}

		public override void TestAllowMultiple()
		{
			AssertEquals(false, new SearchFilter("Poodle").AllowMultiple);
		}

		public override void TestEquals()
		{
			AssertEquals(new SearchFilter("blah"), new SearchFilter("boo"));
		}

		protected override SearchFilter GetFilter()
		{
			return new SearchFilter("Blah");
		}

		public override void TestWhenRemovedThroughFilterManager_ShouldRestoreVisualState()
		{
			var system = CreateSystem("ORG");
			var bucket = CreateBucket(system);
			var section = CreateBoardSection(bucket);

			Factory.Save();

			using (DisableAsyncBehaviour())
			using (var form = new VisualBoardForm(VisualBoardFormBasherTest.GetViewModel(section.Board)))
			{
				form.Show();
				Application.DoEvents();

				var searchControl = form.FindAll<ZSearchBox>().FirstOrDefault();
				AssertEquals(string.Empty, searchControl.SearchTerm);

				searchControl.SearchTerm = "Something";
				searchControl.OnSearchPerformed(false);

				AssertEquals("Something", searchControl.SearchTerm);

				form.SlideShowViewModel.FilterManager.Clear();
				AssertEquals(string.Empty, searchControl.SearchTerm);
			}
		}
	}
}
