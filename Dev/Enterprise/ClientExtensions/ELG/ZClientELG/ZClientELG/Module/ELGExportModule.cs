using System;
using Enterprise.Accounting.GUI.XmlExport;
using Enterprise.Accounting.Module;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Client.ELG
{
	public class ELGExportModuleStrip : ARTransactionModuleStrip
	{
		public ELGExportModuleStrip()
			: base()
		{
		}

		protected override void AddInterfaceConnectorMenuItems()
		{
			base.AddInterfaceConnectorMenuItems();
			AddExportDataMenuItem(MenuItem, new EventHandler(ELGSagTransactionsExport_Click));
		}

		void ELGSagTransactionsExport_Click(object sender, EventArgs args)
		{
			ZFormModaliser.ShowDialogAndDispose(new FlatFileXmlExportForm(new ELGExportGUIWrapper(Factory)));
		}

		const string MenuItem = "Transactions in Sage Format";
	}
}
