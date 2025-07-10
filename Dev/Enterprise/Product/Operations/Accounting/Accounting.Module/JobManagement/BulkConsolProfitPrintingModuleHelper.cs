using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Accounting.GUI.JobInvoicing;
using Enterprise.Accounting.Integration;
using Enterprise.Environment;
using Enterprise.Security;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Accounting.Module
{
#if DEBUG
	internal
#endif
	class BulkConsolProfitPrintingModuleHelper : BulkJobProfitPrintingModuleHelper
	{
		#region Implementation

		protected override JobProfitDocumentPrintingForm CreatePrintingForm(JobDocumentPrinter docPrinter)
		{
			return new ConsolJobProfitDocumentPrintingForm((ConsolJobDocumentPrinter)docPrinter);
		}

		protected override JobDocumentPrinter CreateJobDocumentPrinter(BusinessObjectFactory factory)
		{
			return new ConsolJobDocumentPrinter(factory);
		}

		protected override List<JobDocumentPrintItem> GetPrintItems(BusinessObjectFactory factory, JobDocumentPrinter docPrinter, BusinessObject[] selectedElements)
		{
			List<JobDocumentPrintItem> jobPrinters = new List<JobDocumentPrintItem>();
			foreach (BusinessObject element in selectedElements)
			{
				IJobCostingPlugIn consol = element as IJobCostingPlugIn;
				if (consol != null)
				{
					jobPrinters.Add(new ConsolJobDocumentPrintItem((ConsolJobDocumentPrinter)docPrinter, consol, factory));
				}
				else
				{
					Globals.Message.ShowError(Res.GetString("194c0240-601e-4a4e-85e1-d54a158d7d21", "There was an error preparing the Job Profit Document"));
				}
			}

			return jobPrinters;
		}

		protected override ZString GetJobNumber(BusinessObject bizObj)
		{
			ZString result = ZString.Empty;
			IJobCostingPlugIn consol = bizObj as IJobCostingPlugIn;
			if (consol != null)
			{
				result = consol.JK_UniqueConsignRef;
			}

			return result;
		}

		protected override SecurityCheckpoint[] GetSecurityCheckPointsCore(BusinessObject[] selectedElements)
		{
			return new SecurityCheckpoint[] { Env.Security.ConsolBulkPrintJobProfitDoc };
		}

		#endregion
	}
}
