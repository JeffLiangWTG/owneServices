using System.Windows.Forms;
using NUnit.Framework;

namespace CargoWise.Windows.UI.Testing
{
	sealed class TabPageVisibilityManagerTest : TestCase
	{
		#region TestVisible

		public void TestVisible()
		{
			using (TabControl = new TabControlWithVisibilitySupport())
			{
				TabPageWithVisibilitySupport page1 = AddTabPage(TabControl, "1");
				TabPageWithVisibilitySupport page2 = AddTabPage(TabControl, "2");
				TabPageWithVisibilitySupport page3 = AddTabPage(TabControl, "3");
				TabPageWithVisibilitySupport page4 = AddTabPage(TabControl, "4");
				TabPageWithVisibilitySupport page5 = AddTabPage(TabControl, "5");

				page4.TabVisible = false;
				AssertTabPages(4, 5, "1", "2", "3", "5");

				page4.TabVisible = true;
				AssertTabPages(5, 5, "1", "2", "3", "4", "5");

				page1.TabVisible = false;
				page3.TabVisible = false;
				AssertTabPages(3, 5, "2", "4", "5");

				page1.TabVisible = true;
				AssertTabPages(4, 5, "1", "2", "4", "5");

				page3.TabVisible = true;
				AssertTabPages(5, 5, "1", "2", "3", "4", "5");

				page4.TabVisible = false;
				page2.TabVisible = false;
				AssertTabPages(3, 5, "1", "3", "5");

				page2.TabVisible = true;
				AssertTabPages(4, 5, "1", "2", "3", "5");

				page4.TabVisible = true;
				AssertTabPages(5, 5, "1", "2", "3", "4", "5");

				page3.TabVisible = false;
				page4.TabVisible = false;
				page5.TabVisible = false;
				AssertTabPages(2, 5, "1", "2");

				page5.TabVisible = true;
				page4.TabVisible = true;
				page3.TabVisible = true;
				AssertTabPages(5, 5, "1", "2", "3", "4", "5");

				page3.TabVisible = true;
				AssertTabPages(5, 5, "1", "2", "3", "4", "5");

				TabControl.TabPages.Remove(page1);
				TabControl.TabPages.Remove(page4);
				AssertTabPages(3, 3, "2", "3", "5");

				TabControl.TabPages.Add(page1);
				TabControl.TabPages.Add(page4);
				AssertTabPages(5, 5, "2", "3", "5", "1", "4");
			}
		}

		#endregion

		#region TestBindingContext

		public void TestBindingContext()
		{
			using (var form = new KForm())
			using (var tabControl = new TabControlWithVisibilitySupport())
			using (var page1 = AddTabPage(tabControl, "1"))
			using (var page2 = AddTabPage(tabControl, "2"))
			{
				form.Controls.Add(tabControl);
				form.Show();

				AssertNotNull("Precondition: form should have a binding context", form.BindingContext);

				tabControl.SelectedIndex = 1;
				tabControl.SelectedIndex = 0;

				AssertEquals(2, tabControl.TabPages.Count);
				AssertSame(form.BindingContext, page1.BindingContext);
				AssertSame(form.BindingContext, page2.BindingContext);

				page2.TabVisible = false;

				AssertEquals(1, tabControl.TabPages.Count);
				AssertSame(form.BindingContext, page1.BindingContext);
				AssertSame("Should still use same binding context", form.BindingContext, page2.BindingContext);

				page2.TabVisible = true;

				AssertEquals(2, tabControl.TabPages.Count);
				AssertSame(form.BindingContext, page1.BindingContext);
				AssertSame(form.BindingContext, page2.BindingContext);
			}
		}

		#endregion

		#region RemoveFromAllTabPages

		public void TestRemoveFromAllTabPages()
		{
			using (TabControl = new TabControlWithVisibilitySupport())
			{
				TabPageWithVisibilitySupport tabPage1 = AddTabPage(TabControl, "1");
				TabPageWithVisibilitySupport tabPage2 = AddTabPage(TabControl, "2");
				AssertCollectionContains(tabPage1, TabControl.AllTabPages);
				AssertCollectionContains(tabPage2, TabControl.AllTabPages);

				TabControl.Controls.Remove(tabPage1);
				AssertCollectionNotContains(tabPage1, TabControl.AllTabPages);

				TabControl.TabPageVisibilityManager.RemoveFromAllPages = false;
				TabControl.Controls.Remove(tabPage2);
				AssertCollectionContains(tabPage2, TabControl.AllTabPages);
			}
		}

		public void TestDisposeTabPage()
		{
			var page1 = new TabPageWithVisibilitySupport("1");
			var page2 = new TabPageWithVisibilitySupport("2");
			var page3 = new TabPageWithVisibilitySupport("3");

			using (var form = new KForm())
			using (var tabControl = new TabControlWithVisibilitySupport())
			{
				form.Controls.Add(tabControl);
				tabControl.Controls.Add(page1);
				tabControl.Controls.Add(page2);
				tabControl.Controls.Add(page3);
				form.Show();

				page2.TabVisible = false;
				tabControl.Controls.Remove(page3);

				AssertCollectionContains(page1, tabControl.TabPages);
				AssertCollectionNotContains(page2, tabControl.TabPages);
				AssertCollectionContains(page2, tabControl.AllTabPages);
				AssertCollectionNotContains(page3, tabControl.TabPages);
				AssertCollectionNotContains(page3, tabControl.AllTabPages);
			}

			Assert("Tab in TabPages should be disposed", page1.IsDisposed);
			Assert("Tab in AllTabPages should be disposed", page2.IsDisposed);
			Assert("Tab in removedTabPages should be disposed", page3.IsDisposed);
		}

		#endregion

		#region Implementation

		TabControlWithVisibilitySupport TabControl;

		void AssertTabPages(int expectedTabPageCount, int expectedAllTabPageCount, params string[] expectedTabPageCaptions)
		{
			AssertEquals(expectedTabPageCount, TabControl.TabPages.Count);
			AssertEquals(expectedAllTabPageCount, TabControl.TabPageVisibilityManager.AllTabPages.Length);
			for (int i = 0; i < expectedTabPageCaptions.Length; i++)
			{
				AssertEquals(expectedTabPageCaptions[i], TabControl.TabPages[i].Text);
			}
		}

		TabPageWithVisibilitySupport AddTabPage(TabControl tabControl, string text)
		{
			TabPageWithVisibilitySupport page = new TabPageWithVisibilitySupport(text);
			tabControl.TabPages.Add(page);

			return page;
		}

		class TabControlWithVisibilitySupport : TabControl
		{
			public TabControlWithVisibilitySupport()
			{
				TabPageVisibilityManager = new TabPageVisibilityManager(this);
				TabPageVisibilityManager.Enabled = true;
			}

			public readonly TabPageVisibilityManager TabPageVisibilityManager;

			public TabPage[] AllTabPages
			{
				get { return TabPageVisibilityManager.AllTabPages; }
			}
		}

		class TabPageWithVisibilitySupport : TabPage
		{
			public TabPageWithVisibilitySupport(string text)
			{
				this.Text = text;
			}

			public bool TabVisible
			{
				get { return VisibilityManager != null && VisibilityManager.GetTabPageVisible(this); }
				set
				{
					if (TabVisible != value && VisibilityManager != null)
					{
						VisibilityManager.SetTabPageVisible(value, this);
					}
				}
			}

			TabPageVisibilityManager VisibilityManager
			{
				get
				{
					if (fVisibilityManager == null && ParentTabControl != null)
					{
						fVisibilityManager = ParentTabControl.TabPageVisibilityManager;
					}
					return fVisibilityManager;
				}
			}

			TabPageVisibilityManager fVisibilityManager;

			TabControlWithVisibilitySupport ParentTabControl
			{
				get { return (TabControlWithVisibilitySupport)Parent; }
			}
		}

		#endregion
	}
}
