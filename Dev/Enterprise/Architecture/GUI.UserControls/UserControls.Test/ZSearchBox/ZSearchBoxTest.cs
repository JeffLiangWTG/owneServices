using System.Windows.Forms;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.GUI.Testing
{
	sealed class ZSearchBoxTest : TransactionedTestCase
	{
		public void TestSearchLabel()
		{
			using (var control = new ZSearchBox())
			{
				AssertEquals("Search...", control.InnerTextBox.PlaceHolderText);
				AssertEquals(string.Empty, control.InnerTextBox.Text);

				control.InnerTextBox.Focus();
				AssertEquals(string.Empty, control.InnerTextBox.Text);

				control.SearchTerm = "Blah";
				AssertEquals("Blah", control.SearchTerm);

				control.SearchTerm = string.Empty;
				AssertEquals(string.Empty, control.InnerTextBox.Text);
				AssertEquals("Search...", control.InnerTextBox.PlaceHolderText);
			}
		}

		public void TestSearch_ShouldFireOnEnterOnly()
		{
			using (var control = new ZSearchBox())
			{
				var wasSearched = false;

				control.SearchPerformed += (s, e) =>
				{
					AssertEquals(false, wasSearched);
					wasSearched = true;

					AssertEquals("Blah", e.SearchTerm);
					AssertEquals(false, e.Cleared);
				};

				control.SearchTerm = "Blah";
				control.OnKeyDown(Keys.E);
				control.OnKeyDown(Keys.Enter);
			}
		}

		public void TestClearSearch_StartingEmpty()
		{
			using (var control = new ZSearchBox())
			{
				control.SearchPerformed += (s, e) =>
				{
					AssertEquals(string.Empty, e.SearchTerm);
					AssertEquals(true, e.Cleared);
				};

				control.OnKeyDown(Keys.Enter);

				AssertEquals(string.Empty, control.SearchTerm);
			}
		}

		public void TestClearSearch_StartingWithSearchTerm()
		{
			using (var control = new ZSearchBox())
			{
				control.SearchPerformed += (s, e) =>
				{
					AssertEquals(string.Empty, e.SearchTerm);
					AssertEquals(true, e.Cleared);
				};

				control.SearchTerm = "Blah";
				control.OnSearchCleared();
				AssertEquals(string.Empty, control.SearchTerm);
			}
		}
	}
}
