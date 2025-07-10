using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Business.ARAP.Invoicing
{
	public class UnapprovedTransactionImportProcessorCreator : IOtherCompanyAPInvoiceImportCreator
	{
		public IProcessor CreateOtherCompanyAPInvoiceImport(IWorkflowProvider workflowProvider, ZGuid companyPK)
		{
			if (companyPK.IsEmpty)
			{
				return null;
			}

			var plugIn = workflowProvider as IJobInvoicingPlugIn;
			if (plugIn == null)
			{
				return null;
			}

			return new UnapprovedTransactionImporter(companyPK, plugIn);
		}
	}
}
