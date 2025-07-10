#if DEBUG

using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.GUI.JobInvoicing
{
	public partial class JobInvoicingPrinter
	{
		public Business.ARAP.Invoicing.Printing.InvoicePrintTask NewTask_ForTestOnly(Business.ARAP.Invoicing.InvoicingBase arTransaction, Business.ARAP.Invoicing.Printing.InvoicePrintContext context)
		{
			return NewTask(arTransaction, context);
		}

		public IJobHeaderParent JobParent_ForTestOnly => jobParent;
	}
}

#endif
