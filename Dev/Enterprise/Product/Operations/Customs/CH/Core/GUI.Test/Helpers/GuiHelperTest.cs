using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.CH.GUI.Testing;

public class GuiHelperTest : TestCase
{
	public void TestReorderTabPages()
	{
		using (var tabControl = new ZTabControl())
		using (var page1 = new ZTabPage())
		using (var page2 = new ZTabPage())
		using (var page3 = new ZTabPage())
		using (var page4 = new ZTabPage())
		{
			tabControl.TabPages.Add(page1);
			tabControl.TabPages.Add(page2);
			tabControl.TabPages.Add(page3);
			tabControl.TabPages.Add(page4);

			tabControl.ReorderTabPages(page1, page3, page2);

			CombineAssertions(() =>
			{
				AssertEquals(4, tabControl.TabPages.Count);
				AssertSame(page1, tabControl.TabPages[0]);
				AssertSame(page3, tabControl.TabPages[1]);
				AssertSame(page2, tabControl.TabPages[2]);
				AssertSame(page4, tabControl.TabPages[3]);
			});
		}
	}
}
