using System.Linq;
using System.Windows.Forms;
using Enterprise.VisualBoards.GUI.Test;
using NUnit.Framework;

namespace Enterprise.BufferManagement.GUI.Test
{
	public class BMSFormTestHelper : VisualBoardsFormTestHelper
	{
		public static MenuItem GetChildMenuItem(MenuItem menuItem, string textToFind)
		{
			return menuItem.MenuItems.Cast<MenuItem>().SingleOrDefault(x => x.Text == textToFind);
		}

		public static void AssertMenuItems(MenuItem parentItem, params string[] expectedMenuItemNames)
		{
			var actualMenuItemNames = parentItem.MenuItems.Cast<MenuItem>().Select(x => x.Text);
			Assertion.AssertSequencesEqual(expectedMenuItemNames, actualMenuItemNames);
		}
	}
}
