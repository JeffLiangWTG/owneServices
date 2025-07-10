using System;
using System.Collections.Generic;
using System.Windows.Forms;
using CargoWise.Windows.UI;
using Enterprise.DocumentEngine.GUI.DocumentMenu;
using Enterprise.DocumentEngine.Testing;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.DocumentEngine.GUI.Testing
{
	sealed class DocEngineDynamicMenuItemProviderTest : DocEngineDynamicMenuTest<DocEngineDynamicMenuItemProvider, DynamicMenuItem, ZDocumentMenuItem>
	{
		protected override List<ReportRunInfoForTesting> GetReportInfos(ZDocumentMenuItem documentsMenu)
		{
			return documentsMenu.HelperForTesting.LastRunReportInfos;
		}

		protected override void PeformMenuClick(object menuItem)
		{
			((MenuItem)menuItem).PerformClick();
		}

		protected override void AddEmptyEventArgs(DynamicMenuItem item)
		{
			item.OnPopup(EventArgs.Empty);
		}

		protected override string GetText(object menuItem)
		{
			return ((MenuItem)menuItem).Text;
		}
	}
}
