using System.Linq;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Business.ARAP.Invoicing
{
	class APLineRelatedJobOpener
	{
		public static void ReopenClosedJobwithSuspendedValidation(InvoicingBase invoicingBase)
		{
			if (invoicingBase != null
				&& invoicingBase.IsBeingCreatedPostedAllocatedApprovedOrIncomplete
				&& JobReopenSecurityCheckHelper.CanReopenJob_NonInteractiveSecurityCheck(
					invoicingBase.Factory, invoicingBase.RelatedJobsForReversing.Where(x => x != null && x.JH_Status == JobHeaderStatus.Closed.Code)))
			{
				invoicingBase.Factory.SuspendValidation();
				try
				{
					invoicingBase.ReOpenClosedJob();
				}
				finally
				{
					invoicingBase.Factory.ResumeValidation();
				}
			}
		}
	}
}