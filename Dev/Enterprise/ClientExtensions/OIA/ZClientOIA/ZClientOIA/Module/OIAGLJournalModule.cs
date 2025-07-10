using System;

using CargoWise.EntityFramework;
using Enterprise.Accounting.Aggregator;
using Enterprise.Accounting.Module;
using Enterprise.Client.OIA.Business;
using Enterprise.Client.OIA.GUI;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Client.OIA.Module
{
	internal class OIAGLJournalModule : GLJournalModule
	{
		public OIAGLJournalModule()
		{
		}

		protected override void AddInterfaceConnectorMenuItems()
		{
			base.AddInterfaceConnectorMenuItems();
			AddExportDataMenuItem("GL Transactions in OIA CSV Format", new EventHandler(ExportOIAGLTransactionsEventHandler));
		}

		void ExportOIAGLTransactionsEventHandler(object sender, EventArgs args)
		{
			OIAGLTransactionBusinessObject bizo = new OIAGLTransactionBusinessObject(new BusinessObjectFactory());
			new AggregateController().PerformAggregationIfRequired();
			ZFormModaliser.ShowDialogAndDispose(new OIAGLTransactionsForm(bizo));
		}
	}
}
