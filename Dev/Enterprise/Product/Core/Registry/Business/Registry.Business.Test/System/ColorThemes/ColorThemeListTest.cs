using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	sealed class ColorThemeListTest : TestCase
	{
		public void TestColorThemeList()
		{
			ColorThemeList list = new ColorThemeList();
			Assert(list.Count > 0);

			ColorTheme theme = list.GetThemeFromName("Classic");
			AssertEquals("Classic", theme.Name);
		}
	}
}
