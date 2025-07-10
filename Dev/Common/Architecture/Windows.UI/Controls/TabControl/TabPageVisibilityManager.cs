using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace CargoWise.Windows.UI
{
	/// <summary>
	/// Manages the visibility of a tab page. Correctly hides and shows the tab page when Visible is set.
	/// Also maintains a list of All Tab Pages - visible or not - for management purposes.
	/// </summary>
	public class TabPageVisibilityManager
	{
		public TabPageVisibilityManager(TabControl tabControl)
		{
			this.TabControl = tabControl;
			tabControl.Disposed += new EventHandler(TabControl_Disposed);
			allPagesList = new List<TabPage>();
			removedPagesList = new List<TabPage>();
		}

		public TabControl TabControl { get; private set; }

		#region Enabled

		public bool Enabled
		{
			get { return enabled; }
			set
			{
				enabled = value;
				if (Enabled)
				{
					TabControl.ControlAdded += new ControlEventHandler(TabControl_ControlAdded);
					TabControl.ControlRemoved += new ControlEventHandler(TabControl_ControlRemoved);
				}
			}
		}

		bool enabled;

		#endregion

		#region Control Add/Remove

		void TabControl_ControlAdded(object sender, ControlEventArgs e)
		{
			TabPage tabPage = e.Control as TabPage;
			if (tabPage != null)
			{
				int visibleIndex = TabControl.TabPages.IndexOf(tabPage);
				int newIndex = 0;
				if (visibleIndex > 0)
				{
					TabPage previousVisibleTab = TabControl.TabPages[visibleIndex - 1];
					int allPagesIndexPrevious = allPagesList.IndexOf(previousVisibleTab);
					newIndex = allPagesIndexPrevious + 1;
				}

				if (!allPagesList.Contains(tabPage))
				{
					allPagesList.Insert(newIndex, tabPage);
				}

				if (removedPagesList.Contains(tabPage))
				{
					removedPagesList.Remove(tabPage);
				}
			}
		}

		void TabControl_ControlRemoved(object sender, ControlEventArgs e)
		{
			if (RemoveFromAllPages)
			{
				var tabPage = (TabPage)e.Control;

				RemoveFromAllTabPages(tabPage);

				if (!removedPagesList.Contains(tabPage))
				{
					removedPagesList.Add(tabPage);
				}
			}
		}

		internal bool RemoveFromAllPages = true;

		public void RemoveFromAllTabPages(TabPage tabPage)
		{
			if (allPagesList.Contains(tabPage))
			{
				allPagesList.Remove(tabPage);
			}
		}

		#endregion

		#region All Tab Pages

		/// <summary>
		/// All Tab Pages that are/were on this TabControl - regardless of visibility.
		/// TabPages returns only visible tab pages.
		/// </summary>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays")]
		public TabPage[] AllTabPages
		{
			get { return allPagesList.ToArray(); }
		}

		readonly List<TabPage> allPagesList;
		readonly List<TabPage> removedPagesList;

		#endregion

		#region Tab Page Visibility

		/// <summary>
		/// Hide/Show the tab page.
		/// </summary>
		public void SetTabPageVisible(bool shouldBeVisible, TabPage tabPage)
		{
			if (!shouldBeVisible)
			{
				RemoveFromAllPages = false;
				try
				{
					if (tabPage.IsHandleCreated)
					{
						tabPage.BindingContext = tabPage.BindingContext; // Copy parent binding context to tab page to prevent creation of new BindingContext instance
					}
					TabControl.TabPages.Remove(tabPage);
				}
				finally
				{
					RemoveFromAllPages = true;
				}
			}
			else if (tabPage.Parent == null && !TabControl.TabPages.Contains(tabPage))
			{
				int allIndex = allPagesList.IndexOf(tabPage);
				int hiddenCount = AllTabPages.Count(page => allPagesList.IndexOf(page) < allIndex && !TabControl.TabPages.Contains(page));

				TabControl.TabPages.InsertPage(tabPage, allIndex - hiddenCount);
				if (tabPage.IsHandleCreated && TabControl.BindingContext != null)
				{
					tabPage.BindingContext = null; // Use parent binding context
				}
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters")]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic")]
		public bool GetTabPageVisible(TabPage tabPage)
		{
			return tabPage.Parent != null;
		}

		#endregion

		#region Dispose

		void TabControl_Disposed(object sender, EventArgs e)
		{
			foreach (TabPage page in AllTabPages)
			{
				if (!page.IsDisposed)
				{
					page.Dispose();
				}
			}

			foreach (TabPage page in removedPagesList)
			{
				if (!page.IsDisposed && page.Parent == null)
				{
					page.Dispose();
				}
			}
		}

		#endregion
	}
}
