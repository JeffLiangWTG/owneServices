using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.CH.GUI;

public static class GuiHelper
{
	public static void ReorderTabPages(this ZTabControl tabControl, params ZTabPage[] orderedPages)
	{
		for (var i = 0; i < orderedPages.Length; i++)
		{
			tabControl.TabPages.Remove(orderedPages[i]);
			tabControl.TabPages.Insert(orderedPages[i], i);
		}
	}
}
