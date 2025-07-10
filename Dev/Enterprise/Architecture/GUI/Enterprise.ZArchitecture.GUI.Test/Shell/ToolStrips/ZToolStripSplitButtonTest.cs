using System.Reflection;
using CargoWiseOne.ResourceStrings;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.GUI.Testing
{
	sealed class ZToolStripSplitButtonTest : TestCase
	{
		public void TestTextReturnsCaptionWithKeyAccelerator()
		{
			using (var menuItem = new ZToolStripSplitButton())
			{
				var resString = new ResourceStringData("key", "&Exit");
				menuItem.CaptionResourceString = resString;

				AssertEquals("&Exit", menuItem.Text);
			}
		}

		public void TestShouldSerializeCaptionResourceStringForDesigner()
		{
			var methodName = $"ShouldSerialize{nameof(ZToolStripSplitButton.CaptionResourceString)}";
			AssertNotNull($"{nameof(ZToolStripSplitButton)} should have the method '{methodName}' defined for visual studio designer.", typeof(ZToolStripSplitButton).GetMethod(methodName, BindingFlags.Instance | BindingFlags.NonPublic));
		}
	}
}
