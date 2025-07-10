using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Accounting.Integration;
using Enterprise.DocumentEngine.GUI;
using Enterprise.Integration.Freight;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.GUI
{
	internal class JobManagementConsolDocumentCustomisationDevTool : DocumentCustomisationDevTool
	{
		public override string Name
		{
			get { return (NoResString)"Customize Consol Profit Document"; }  // Developer Diagnostic Tool
		}

		protected override void ShowCore(Form form)
		{
			ZForm zForm;

			if ((zForm = form as ZForm) == null)
			{
				Globals.Message.Show((NoResString)"Not a ZForm"); // Developer Diagnostic Tool
			}
			if (zForm.BusinessEntity == null)
			{
				Globals.Message.Show((NoResString)"Could not find the form BusinessEntity"); // Developer Diagnostic Tool
			}
			else
			{
				var factory = zForm.BusinessEntity.Factory;
				var profitLoss = zForm.BusinessEntity as JobProfitLoss;
				var jobPK = (profitLoss != null && profitLoss.JobPKs.Length > 0) ? profitLoss.JobPKs[0] : ZGuid.Empty;
				var job = factory.Load<Job>(jobPK);

				if (job == null)
				{
					Globals.Message.Show((NoResString)"Could not find the JobProfitLoss"); // Developer Diagnostic Tool
				}
				else if (job.PlugInData == null)
				{
					Globals.Message.Show((NoResString)"Could not find the Job Parent"); // Developer Diagnostic Tool
				}
				else
				{
					var consol = factory.LoadTop1<ICommonConsol>(new ZQuery(JobConsolSchema.JK_UniqueConsignRef, job.PlugInData.InvoicingSupporter.ConsolNumber)) as IJobCostingPlugIn;
					if (consol == null)
					{
						Globals.Message.Show((NoResString)"Could not find the Consol linked to the Job Parent"); // Developer Diagnostic Tool
					}
					else
					{
						var docPrinter = new ConsolJobDocumentPrinter(factory);
						var docPrintItem = new ConsolJobDocumentPrintItem(docPrinter, consol, factory);

						ShowCustomisationForm(docPrintItem);
					}
				}
			}
		}
	}
}
