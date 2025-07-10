using System;
using System.Drawing;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Core.Testing
{
	sealed class ColorListTest : TestCase
	{
		public void TestColorList()
		{
			var list = new ColorList();
			Assert(list.ContainsCode("Black"));
			Assert(list.ContainsCode("Blanched Almond"));
			Assert(list.Contains(new CodeDescriptionPair("Blanched Almond", "BlanchedAlmond")));
		}

		public void TestColorFromName()
		{
			AssertColorEquals(Color.BlanchedAlmond, ColorList.ColorFromName("Blanched Almond"));
			AssertColorEquals(Color.Black, ColorList.ColorFromName("Black"));
		}

		public void TestNameFromColor()
		{
			AssertEquals("Black", ColorList.NameFromColor(Color.Black));
			AssertEquals("Blanched Almond", ColorList.NameFromColor(Color.BlanchedAlmond));
			AssertEquals("Dark Olive Green", ColorList.NameFromColor(Color.DarkOliveGreen));
		}

		public void TestColorList_ShouldNotContainUnknownColors()
		{
			foreach (CodeDescriptionPair pair in new ColorList())
			{
				var colorName = pair.Description;
				var foundMatch = Enum.TryParse<KnownColor>(colorName, out _);

				AssertEquals($"Color {colorName} is in the ColorList but is not in the KnownColor enum. SAD!", true, foundMatch);
			}
		}
	}
}
