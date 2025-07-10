using System;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.Accounting.Module;
using Enterprise.Client.DFD.GUI;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Client.DFD.Module
{
	public class DFDARTransactionModule : ARTransactionModuleStrip
	{
		protected override void AddInterfaceConnectorMenuItems()
		{
			base.AddInterfaceConnectorMenuItems();
			AddExportDataMenuItem("to XML (DFD)", new EventHandler(ARTransactionExport_Click));
		}

		void ARTransactionExport_Click(object sender, EventArgs e)
		{
			DFDFlatFileXmlExportGUIWrapper wrapper = new DFDFlatFileXmlExportGUIWrapper(new BusinessObjectFactory());
			ZFormModaliser.ShowDialogAndDispose(new DFDFlatFileXmlExportForm(wrapper));
		}

		internal MenuItem[] InternalGetNewActionMenuItems() {
			return GetNewActionMenuItems();
		}
	}
}
