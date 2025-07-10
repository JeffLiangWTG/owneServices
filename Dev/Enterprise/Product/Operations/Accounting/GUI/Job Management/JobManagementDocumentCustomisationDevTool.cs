using System.Windows.Forms;
using CargoWise.Types;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.DocumentEngine.GUI;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Accounting.GUI
{
	internal class JobManagementDocumentCustomisationDevTool : DocumentCustomisationDevTool
	{
		public override string Name
		{
			get { return (NoResString)"Customize Job Profit Document"; }  // Developer Diagnostic Tool
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
				else
				{
					var docPrinter = new JobDocumentPrinter(factory);
					var docPrintItem = new JobDocumentPrintItem(docPrinter, job, factory);

					ShowCustomisationForm(docPrintItem);
				}
			}
		}
	}
}
