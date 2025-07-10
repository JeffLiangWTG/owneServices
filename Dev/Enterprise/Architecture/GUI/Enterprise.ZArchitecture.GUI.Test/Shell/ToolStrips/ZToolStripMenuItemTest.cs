using System.Reflection;
using CargoWiseOne.ResourceStrings;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.GUI.Testing
{
	sealed class ZToolStripMenuItemTest : TestCase
	{
		public void TestTextReturnsCaptionWithKeyAccelerator()
		{
			using (var menuItem = new ZToolStripMenuItem())
			{
				var resString = new ResourceStringData("key", "&Exit");
				menuItem.CaptionResourceString = resString;

				AssertEquals("&Exit", menuItem.Text);
			}
		}

		public void TestShouldSerializeCaptionResourceStringForDesigner()
		{
			var methodName = $"ShouldSerialize{nameof(ZToolStripMenuItem.CaptionResourceString)}";
			AssertNotNull($"{nameof(ZToolStripMenuItem)} should have the method '{methodName}' defined for visual studio designer.", typeof(ZToolStripMenuItem).GetMethod(methodName, BindingFlags.Instance | BindingFlags.NonPublic));
		}

		public void TestTextIsSameAsName()
		{
			string[] testNames = { "Godel", "Turing", "Kolmogorov" };
			foreach (var testName in testNames)
			{
				using (var menuItem = new ZToolStripMenuItem(testName))
				{
					AssertEquals("Menu item Text property is incorrect", testName, menuItem.Text);
					Assert("Menu item Text property is not same as Name property", menuItem.Text.Equals(menuItem.Name));
				}
			}
		}
	}
}
