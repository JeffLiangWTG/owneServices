using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Accounting.Integration;
using Enterprise.Environment;
using Enterprise.Security;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Accounting.GUI.JobInvoicing
{
	public partial class ConsolJobProfitLossControl : JobProfitLossControl
	{
		public ConsolJobProfitLossControl()
		{
			InitializeComponent();

			ProfitLossGrid.ColumnLayoutContext = ProfitLossSummaryGridContext.Consol;
			ProfitLossSummaryGrid.ColumnLayoutContext = ProfitLossSummaryGridContext.Consol;
			GlobalJobCostingTabPage.TabVisible = false;
		}

		#region Dispose

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (components != null)
				{
					components.Dispose();
				}
			}
			base.Dispose(disposing);
		}

		#endregion

		#region Overrides

		protected override void PrintJobProfitDocument()
		{
			if (!SecurityHelper.CheckIsAllowedForSecurityCheckPoint(SecurityCore.PrintJob))
			{
				SecurityHelper.ShowError(SecurityCore.PrintJob);
			}
			else
			{
				IJobCostingPlugIn consol = CurrentDataItem as IJobCostingPlugIn;

				if (consol != null)
				{
					if (!consol.CostSupporter.HasChanges)
					{
						BusinessObjectFactory printingFactory = new BusinessObjectFactory();
						ConsolJobDocumentPrinter docPrinter = new ConsolJobDocumentPrinter(printingFactory);
						if (ZFormModaliser.ShowDialogAndDispose(new ConsolJobProfitDocumentPrintingForm(docPrinter)) == DialogResult.OK)
						{
							docPrinter.IsProfitLossDoc = true;
							ConsolJobDocumentPrintItem docPrintItem = new ConsolJobDocumentPrintItem(docPrinter, consol, printingFactory);
							docPrinter.PrintJobProfitDocuments(printingFactory, docPrintItem);
						}
					}
					else
					{
						Globals.Message.ShowError(Res.GetString("08f52fbf-499a-4de7-b548-1c6c2866bbe6", "Please save this Consol before printing the Job Profit Document"));
					}
				}
				else
				{
					Globals.Message.ShowError(Res.GetString("846679f7-4173-4421-aac8-6507ef012603", "There was an error preparing the Job Profit Document"));
				}
			}
		}

		public override SecurityCheckpoint PluginSecurity
		{
			// Consol Level Security is not yet module specific. 
			get { return Env.Security.MaintainConsol; }
		}

		#endregion
	}
}

