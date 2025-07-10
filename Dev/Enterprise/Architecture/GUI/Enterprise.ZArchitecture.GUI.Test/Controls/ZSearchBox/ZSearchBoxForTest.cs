using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using CargoWise.Common;
using Enterprise.ZArchitecture.GUI.Drawing;
using Enterprise.ZArchitecture.GUI.Forms;
using Enterprise.ZArchitecture.GUI.SearchBox;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.GUI.Testing
{
	[GuiTest]
	sealed class ZSearchBoxForTest : TestCase
	{
		#region Functionality

		public void TestCreateAutoButtonTheme()
		{
			using (var sb = new ZSearchBox())
			{
				// arrange
				sb.ForeColor = Color.Azure;
				sb.BackColor = Color.Bisque;

				// act
				var theme = sb.CreateAutoButtonTheme();
				var expectedTheme = new ButtonTheme(
					Color.Azure, Color.Bisque,
					Color.LightGray, Color.FromArgb(255, 205, 145),
					Color.White, Color.FromArgb(255, 181, 94),
					0);

				// assert
				ButtonThemeTestHelpers.AssertButtonThemesEqual(expectedTheme, theme);
			}
		}

		public void TestShowResults()
		{
			using (var sb = new ZSearchBox())
			using (var form = new ZMainForm())
			{
				// arrange
				form.Show();
				sb.Parent = form;

				// act
				sb.ShowResults();

				// assert
				Assert("Parent form should contain SearchResultsDisplay", form.Controls.Contains(sb.SearchResultsDisplay));
				Assert("SearchResultsDisplay should be visible", sb.SearchResultsDisplay.Visible);
				AssertEquals("SearchResultsDisplay should be at the front", 0, form.Controls.GetChildIndex(sb.SearchResultsDisplay));
				Assert("Search text box should have focus", sb.tbSearch.Focused);
			}
		}

		public void TestHideResults()
		{
			using (var sb = new ZSearchBox())
			using (var form = new ZMainForm())
			{
				// arrange
				form.Show();
				sb.Parent = form;
				sb.ShowResults();
				Assert("SearchResultsDisplay should be visible", sb.SearchResultsDisplay.Visible);

				// act
				sb.HideResults();

				// assert
				Assert("SearchResultsDisplay should not be visible", !sb.SearchResultsDisplay.Visible);
			}
		}

		public void TestResetSearchBox()
		{
			using (var sb = new ZSearchBox())
			{
				// arrange
				sb.tbSearch.Text = "hello world";
				sb.BackColor = Color.FromArgb(1, 2, 3);
				sb.tbSearch.Focus();

				// act
				sb.ResetSearchBox();

				// assert
				AssertEquals("Search text box wasn't cleared", string.Empty, sb.tbSearch.Text);
				AssertColorEquals("BackColor of search box was not reset to the default", sb.DefaultBackColor, sb.BackColor);
				AssertNull("Focus should not be on the search box", sb.ActiveControl);
				Assert("Focus should not be on the search box", !sb.tbSearch.Focused);
			}
		}

		public void TestResizeAndRepositionSearchResults()
		{
			using (var form = new ZMainForm())
			{
				// arrange
				form.Location = new Point(123, 456);
				form.Show();
				var sb = form.SearchBox;
				sb.Width = 135;
				sb.ShowResults();
				Assert("SearchResultsDisplay should be visible", sb.SearchResultsDisplay.Visible);

				// act
				sb.ResizeAndRepositionSearchResults();

				// assert
				AssertEquals("SearchResultsDisplay.Width was wrong", 270, sb.SearchResultsDisplay.Width);
				AssertEquals("SearchResultsDisplay.Height was wrong", 384, sb.SearchResultsDisplay.Height);
				AssertEquals("SearchResultsDisplay.Left was wrong", 454, sb.SearchResultsDisplay.Left);
				AssertEquals("SearchResultsDisplay.Top was wrong", 28, sb.SearchResultsDisplay.Top);
			}
		}

		public void TestResizeAndRepositionComponents()
		{
			using (var sb = new ZSearchBox())
			{
				// arrange
				sb.Height = 64;
				sb.Width = 256;

				// act
				sb.ResizeAndRepositionComponents();

				// assert
				AssertEquals("Search button dimensions are not equal (won't be a square box)", sb.btnSearch.Width, sb.btnSearch.Height);
				AssertEquals("Search box didn't resize appropriately", sb.Width - (sb.Height - sb.Padding.Vertical), sb.pnSearchBox.Width); // pnSearchBox.Width is based on the button width which is based on the button height and padding

				AssertEquals("Search text box Top didn't match it's containing panel Top", 3, sb.tbSearch.Top);
				AssertEquals("Search text box Left didn't match it's containing panel Left", 3, sb.tbSearch.Left);
				AssertEquals("Search text box Width didn't match it's containing panel Width", 194, sb.tbSearch.Width);
			}
		}

		public void TestPerformSearchHandlerIsNull()
		{
			using (var sb = new ZSearchBox())
			{
				// arrange
				sb.tbSearch.Text = "something_something";

				// act
				WaitForSearch(sb);

				// assert
				AssertEquals("ErrorReporter last message was wrong",
					"The OnSearch handler was null; no search results will ever be returned. The parent control needs to set this; check the control hierarchy: ZSearchBox; ",
					ErrorReporter.LastMessageReported);

				ErrorReporter.Clear();
			}
		}

		public void TestPerformSearch()
		{
			using (var form = new ZMainForm())
			using (var sb = new ZSearchBox())
			{
				// arrange
				form.Controls.Add(sb);
				sb.AutoSearch = false;
				sb.Search += (s) => new List<IDisplayItem>
				{
					DisplayItemFactory.CreateHeadingItem("hello"),
					DisplayItemFactory.CreateSearchItemWithEmptyAction("world", "world2")
				};
				sb.tbSearch.Text = "hello world";
				form.Show();

				// act
				WaitForSearch(sb);

				// assert
				var itemsInSearchListBox = sb.SearchResultsDisplay.SearchBoxResults.ItemsAsDisplayItems;
				AssertEquals("Incorrect item count in the results list box", 2, itemsInSearchListBox.Count());

				// first item
				var firstItem = itemsInSearchListBox.First();
				AssertEquals("Search result data was incorrect", "hello", firstItem.Data.First());
				AssertEquals("lastSearch wasn't set to the current search term", sb.lastSearch, sb.tbSearch.Text);

				// second item
				var secondItem = itemsInSearchListBox.Last();
				AssertEquals("Search result data was incorrect", "world", secondItem.Data.First());

				AssertEquals("lastSearch wasn't set to the current search term", sb.lastSearch, sb.tbSearch.Text);
			}
		}

		public void TestPerformSearchChangesColour()
		{
			using (var form = new ZMainForm())
			using (var sb = new ZSearchBox())
			{
				// arrange
				form.Controls.Add(sb);
				sb.AutoSearch = false;
				sb.Search += (s) => new List<IDisplayItem>
				{
					DisplayItemFactory.CreateHeadingItem("hello"),
					DisplayItemFactory.CreateSearchItemWithEmptyAction("world", "world2")
				};
				sb.tbSearch.Text = "hello world";
				form.Show();

				var originalBackColour = sb.tbSearch.BackColor;
				var backColourWasChangedForSearch = false;
				sb.tbSearch.BackColorChanged += (a, b) => backColourWasChangedForSearch = true;
				// act
				WaitForSearch(sb);

				// assert
				Assert("tbSearch.BackColor wasn't set to the search colour", backColourWasChangedForSearch);
				AssertEquals("tbSearch.BackColor wasn't reset", originalBackColour, sb.tbSearch.BackColor);

				var itemsInSearchListBox = sb.SearchResultsDisplay.SearchBoxResults.ItemsAsDisplayItems;
				AssertEquals("Incorrect item count in the results list box", 2, itemsInSearchListBox.Count());

				// first item
				var firstItem = itemsInSearchListBox.First();
				AssertEquals("Search result data was incorrect", "hello", firstItem.Data.First());
				AssertEquals("lastSearch wasn't set to the current search term", sb.lastSearch, sb.tbSearch.Text);

				// second item
				var secondItem = itemsInSearchListBox.Last();
				AssertEquals("Search result data was incorrect", "world", secondItem.Data.First());

				AssertEquals("lastSearch wasn't set to the current search term", sb.lastSearch, sb.tbSearch.Text);
			}
		}

		internal static void WaitForSearch(ZSearchBox sb, bool callSearch = true)
		{
			if (callSearch)
			{
				sb.PerformSearch();
			}

			var count = 0;
			var max = 50;

			if (sb.searchTask == null)
			{
				return;
			}

			while (!sb.searchTask.IsCompleted && count++ < max)
			{
				Thread.Sleep(100);
				Application.DoEvents();
			}
		}

		public void TestPerformSearchNoResults()
		{
			using (var form = new ZMainForm())
			using (var sb = new ZSearchBox())
			{
				// arrange
				form.Controls.Add(sb);
				sb.AutoSearch = false;
				sb.Search += (s) => { return new List<IDisplayItem> { }; };
				sb.tbSearch.Text = "something_with_no_results";
				form.Show();

				// act
				WaitForSearch(sb);

				// assert
				var itemsInSearchListBox = sb.SearchResultsDisplay.SearchBoxResults.ItemsAsDisplayItems;
				AssertEquals("Incorrect item count in the results list box", 1, itemsInSearchListBox.Count());
				var item = itemsInSearchListBox.First();
				AssertEquals("Search result data was incorrect", "No results found", item.Data.Aggregate((a, b) => a + " : " + b));
			}
		}
		public void TestPerformSearchEmptyString()
		{
			using (var sb = new ZSearchBox())
			{
				// arrange
				sb.AutoSearch = false;
				sb.Search += (s) => { return new List<IDisplayItem> { }; };
				sb.tbSearch.Text = "";

				// act
				WaitForSearch(sb);

				// assert
				var itemsInSearchListBox = sb.SearchResultsDisplay.SearchBoxResults.ItemsAsDisplayItems;
				AssertEquals("Incorrect item count in the results list box", 1, itemsInSearchListBox.Count());
				var item = itemsInSearchListBox.First();
				AssertEquals("Search result data was incorrect", "Empty search term", item.Data.Aggregate((a, b) => a + " : " + b));
			}
		}

		public void TestPerformSearchExceptionThrown()
		{
			using (var form = new ZMainForm())
			using (var sb = new ZSearchBox())
			{
				// arrange
				form.Controls.Add(sb);
				sb.AutoSearch = false;
				var ex = new InvalidOperationException("you were a bad boy");
				sb.Search = (e) => { throw ex; };
				sb.tbSearch.Text = "something_to_search_for";
				form.Show();

				// act
				AssertNoExceptionThrown(() => WaitForSearch(sb));

				// assert
				AssertEquals("ErrorReporter last message was wrong", "Unhandled exception caught when calling Search", ErrorReporter.LastMessageReported);
				AssertEquals("ErrorReporter last exception was wrong", ex, ErrorReporter.LastExceptionReported);
				ErrorReporter.Clear();
			}
		}

		#endregion
	}
}
