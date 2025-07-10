using System;

using Enterprise.Accounting.Module;
using Enterprise.Client.Rohlig.Bellin;
using Enterprise.Client.Rohlig.GUI;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Client.Rohlig.Module
{
	public class BellinModule : APTransactionModuleStrip
	{
		public BellinModule()
		{
		}

		protected override void AddInterfaceConnectorMenuItems()
		{
			base.AddInterfaceConnectorMenuItems();
			AddExportDataMenuItem(Constants.BellinCaption.Replace("Export", ""), new EventHandler(ExportBellinTransactions_Click));
		}

		internal
		void ExportBellinTransactions_Click(object sender, EventArgs args)
		{
			using (BellinExportForm form = new BellinExportForm(new BellinExportGUIWrapper(Factory)))
			{
				ShowDialog(form);
			}
		}

		protected virtual
 void ShowDialog(ZForm form)
		{
			ZFormModaliser.ShowDialogWithoutDispose(form);
		}
	}
}
