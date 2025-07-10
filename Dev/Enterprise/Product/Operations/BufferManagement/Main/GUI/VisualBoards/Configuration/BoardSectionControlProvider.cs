using System.Collections.Generic;
using System.Windows.Forms;
using Enterprise.VisualBoards.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.BufferManagement.GUI
{
	static class BoardSectionControlProvider
	{
		internal static ZUserControl GetConfigControl(string sectionType)
		{
			var descriptor = SectionDescriptorProvider.Get(sectionType);
			var configControl = descriptor != null ? descriptor.GetSectionConfigurationControl() as ZUserControl : null;

			if (configControl != null)
			{
				configControl.Dock = DockStyle.Fill;
			}

			return configControl;
		}

		internal static IEnumerable<ZBindingTabPage> GetAdditionalTabPages(string sectionType)
		{
			var descriptor = SectionDescriptorProvider.Get(sectionType);
			if (descriptor != null)
			{
				foreach (var tabSpec in descriptor.GetAdditionalTabs())
				{
					var tabPage = tabSpec.GetTabPage();
					if (tabPage != null)
					{
						yield return tabPage;
					}
				}
			}
		}
	}
}
