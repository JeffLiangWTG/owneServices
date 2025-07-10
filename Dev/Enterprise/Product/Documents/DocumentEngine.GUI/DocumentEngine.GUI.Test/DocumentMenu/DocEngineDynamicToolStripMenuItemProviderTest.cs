using System;
using System.Collections.Generic;
using System.Windows.Forms;
using CargoWise.Windows.UI;
using Enterprise.DocumentEngine.GUI.DocumentMenu;
using Enterprise.DocumentEngine.Testing;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.DocumentEngine.GUI.Testing
{
	sealed class DocEngineDynamicToolStripMenuItemProviderTest : DocEngineDynamicMenuTest<DocEngineDynamicToolStripMenuItemProvider, DynamicToolStripMenuItem, ZDocumentToolStripMenuItem>
	{
		protected override List<ReportRunInfoForTesting> GetReportInfos(ZDocumentToolStripMenuItem documentsMenu)
		{
			return documentsMenu.HelperForTesting.LastRunReportInfos;
		}

		protected override void PeformMenuClick(object menuItem)
		{
			((ToolStripMenuItem)menuItem).PerformClick();
		}

		protected override void AddEmptyEventArgs(DynamicToolStripMenuItem item)
		{
			item.OnOpening(EventArgs.Empty);
		}

		protected override string GetText(object menuItem)
		{
			return ((ToolStripMenuItem)menuItem).Text;
		}
	}
}
