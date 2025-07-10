using System;
using CargoWise.Common;
using Enterprise.Customs.ES.ExitControl.Business;
using Enterprise.Customs.EU.ExitControl.GUI;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.ES.ExitControl.GUI
{
	public class ViewOnCustomsWebsiteMenuItemCreator
	{
		public ViewOnCustomsWebsiteMenuItemCreator(IReportsGridUserControlProvider provider)
		{
			this.provider = Argument.NotNull(provider, nameof(provider));
		}
		readonly IReportsGridUserControlProvider provider;

		public ZMenuItem Create() => new ZMenuItem(ResString.GetMultilingualString("37B33DAB-B743-40C8-A906-BDABE9EB69B1", "View on Customs Website"), ViewOnCustomsWebsite_Click);

		void ViewOnCustomsWebsite_Click(object sender, EventArgs e)
		{
			var reportsGrid = provider.UserControl.ReportsGrid;

			var selectedElements = reportsGrid.SelectedElements;
			if (selectedElements.Length == 0)
			{
				Globals.Message.Show(CommonPromptMessages.SelectARowMessage);
			}
			else
			{
				foreach (CusExitReport report in selectedElements)
				{
					var linkWebPage = report.GetUrlToLaunch();
					if (!string.IsNullOrEmpty(linkWebPage))
					{
						WebUrlLauncher.Launch(linkWebPage);
					}
				}
			}
		}
	}
}
