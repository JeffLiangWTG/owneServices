using System;
using System.Collections;
using System.Globalization;
using System.Reflection;
using System.Windows.Forms;

namespace CargoWise.Windows.UI
{
	/// <summary>
	/// Extension methods for the CargoWise.Windows.UI.KTabControl.ControlCollection class.
	/// </summary>
	public static class TabControlControlCollectionExtensions
	{
		#region Insert Tab Page

		/// <summary>
		/// Inserts a tab at the specified index.
		/// This corrects a bug in the .NET implementation which sometimes does not insert the tab page.
		/// </summary>
		public static void InsertPage(this TabControl.TabPageCollection pages, TabPage page, int index)
		{
			TabControl tabControl = GetTabControl(pages);

			tabControl.SuspendLayout();

			int selectedIndex = tabControl.SelectedIndex;
			int oldLength = tabControl.Controls.Count;

			try
			{
				Stack pagesStack = new Stack();

				for (int i = tabControl.TabPages.Count - 1; i >= Math.Max(index, 0); i--)
				{
					pagesStack.Push(tabControl.TabPages[i]);
					RemoveInternal(pages[i], i);
				}

				tabControl.Controls.Add(page);

				while (pagesStack.Count > 0)
				{
					tabControl.Controls.Add((TabPage)pagesStack.Pop());
				}
			}
			finally
			{
				if (index <= selectedIndex && selectedIndex > 0 && tabControl.Controls.Count > oldLength)
				{
					tabControl.SelectTab(selectedIndex + 1);
				}
				tabControl.ResumeLayout();
			}
		}

		/// <summary>
		/// When calling TabPages.Remove(), the call to base.Remove() causes issues where the tab positions are then re-ordered.
		/// This method works around this problem.
		/// </summary>
		/// <param name="page"></param>
		/// <param name="index"></param>
		static void RemoveInternal(TabPage page, int index)
		{
			TabControl tabControl = (TabControl)page.Parent;
			if (tabControl != null && index != -1 && index < tabControl.TabPages.Count)
			{
				typeof(TabControl).GetMethod("RemoveTabPage", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(tabControl, new object[] { index });
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2001:AvoidCallingProblematicMethods", MessageId = "System.Type.InvokeMember")]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "field name, not a resource string")]
		static TabControl GetTabControl(TabControl.TabPageCollection pages)
		{
#if NETFRAMEWORK || WINZOR
			const string fieldName = "owner";
#else
			const string fieldName = "_owner";
#endif
			return (TabControl)typeof(TabControl.TabPageCollection).InvokeMember(fieldName, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.GetField, null, pages, null, CultureInfo.InvariantCulture);
		}

#endregion
	}
}
