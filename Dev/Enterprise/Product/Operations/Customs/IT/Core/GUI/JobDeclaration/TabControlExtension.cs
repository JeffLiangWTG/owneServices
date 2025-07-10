using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.IT.GUI;

public static class TabControlExtension
{
	/// <summary>
	/// Order the tab pages following the expected order
	/// </summary>
	/// <param name="tabControl">Tab Control parent</param>
	/// <param name="expectedTabPagesInOrder">Expected order</param>
	public static void OrderTabPages(this ZTabControl tabControl, IReadOnlyDictionary<ZInt, ZTabPage> expectedTabPagesInOrder)
	{
		Argument.NotNull(tabControl, nameof(tabControl));
		Argument.NotNull(expectedTabPagesInOrder, nameof(expectedTabPagesInOrder));

		var allTabPages = tabControl.AllTabPages.Cast<ZTabPage>();
		var unknownTabPages = allTabPages.Except(expectedTabPagesInOrder.Values).ToArray();
		var tabPagesVisibilityDictionary = allTabPages.ToDictionary(x => x, x => x.TabVisible);

		tabControl.TabPages.Clear();
		foreach (var expectedTabPage in expectedTabPagesInOrder.Values)
		{
			AddIfContainedInAllTabPages(expectedTabPage);
		}
		tabControl.TabPages.AddRange(unknownTabPages);

		foreach (var tabPage in allTabPages)
		{
			tabPage.TabVisible = tabPagesVisibilityDictionary[tabPage];
		}

		void AddIfContainedInAllTabPages(ZTabPage expectedTabPage)
		{
			if (allTabPages.Contains(expectedTabPage))
			{
				tabControl.TabPages.Add(expectedTabPage);
			}
		}
	}
}
