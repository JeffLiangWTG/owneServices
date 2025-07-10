using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;

namespace NUnit.Framework
{
	public static class MenuAssertion
	{
		/// <summary>
		/// Asserts that a menu has the given menu path. If not then an <see cref="AssertionFailedError"/> is thrown.
		/// If so then the menu item is returned for further tests.
		/// </summary>
		public static MenuItem AssertHasMenu(Menu root, params string[] path)
		{
			return AssertHasMenu("", root, path);
		}

		/// <summary>
		/// Asserts that a menu has the given menu path. If not then an <see cref="AssertionFailedError"/> is thrown.
		/// If so then the menu item is returned for further tests.
		/// </summary>
		public static MenuItem AssertHasMenu(string message, Menu root, params string[] path)
		{
			return MenuAssertionWithHtml.HtmlAssertHasMenu(HtmlFormatter.Html(message), root, path);
		}

		public static MenuItem AssertHasMenu(IEnumerable<MenuItem> collection, params string[] path)
		{
			return AssertHasMenu("", collection, path);
		}

		public static MenuItem AssertHasMenu(string message, IEnumerable<MenuItem> collection, params string[] path)
		{
			var dummy = new ContextMenu(new List<MenuItem>(collection).ToArray());
			try
			{
				return AssertHasMenu(message, dummy, path);
			}
			finally
			{
				dummy.MenuItems.Clear();
				dummy.Dispose();
			}
		}

		class MenuAssertionWithHtml : AssertionWithHtml
		{
			/// <summary>
			/// Asserts that a menu has the given menu path. If not then an <see cref="AssertionFailedError"/> is thrown.
			/// If so then the menu item is returned for further tests.
			/// </summary>
			public static MenuItem HtmlAssertHasMenu(string message, Menu root, params string[] path)
			{
				AssertionCount++;
				return HtmlAssertHasMenu(message, root, 0, path);
			}

			static MenuItem HtmlAssertHasMenu(string message, Menu root, int index, params string[] path)
			{
				if (index == path.Length)
				{
					return root as MenuItem;
				}

				Menu child = FindMenu(root, path[index]);
				if (child != null)
				{
					return HtmlAssertHasMenu(message, child, index + 1, path);
				}

				var builder = new StringBuilder(message);
				builder.Append("<br/><br/>Cant Find:<br/>");
				builder.Append(Html(string.Join(" -> ", path, 0, index)));
				if (index > 0)
				{
					builder.Append(" -> ");
				}

				builder.Append(" <b>");
				builder.Append(Html(path[index]));
				builder.Append("</b> ");
				if (index + 1 < path.Length)
				{
					builder.Append(" -> ");
				}

				builder.Append(Html(string.Join(" -> ", path, index + 1, path.Length - index - 1)));

				if (root.MenuItems.Count == 0)
				{
					builder.Append("<br/><br/>No menu items exist at this level:<br/>");
				}
				else
				{
					builder.Append("<br/><br/>Items that do exist at this level:<br/>");
					foreach (MenuItem item in root.MenuItems)
					{
						builder.Append(Html(item.Text));
						builder.Append("<br/>");
					}
				}

				HtmlFail(builder.ToString());
				return null; // never reached, but needed to compile.
			}

			static MenuItem FindMenu(Menu parent, string childName)
			{
				foreach (MenuItem item in parent.MenuItems)
				{
					if (item.Text == childName)
					{
						return item;
					}
				}
				return null;
			}
		}
	}
}
