using System;

using Enterprise.Accounting.Module;
using Enterprise.Client.NZP.GUI;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Client.NZP.Module
{
	public class CMSModule : ARTransactionModuleStrip
	{
		public CMSModule()
		{
		}

		protected override void AddInterfaceConnectorMenuItems()
		{
			base.AddInterfaceConnectorMenuItems();
			AddExportDataMenuItem(Constants.CMSMenuItem, new EventHandler(CMSAccountingExport_Click));
		}

		void CMSAccountingExport_Click(object sender, EventArgs args)
		{
			ZFormModaliser.ShowDialogAndDispose(new CMSFlatFileXmlExportForm(new CMSExportGUIWrapper(Factory)));
		}
	}
}
