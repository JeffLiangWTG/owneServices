using System.Collections.Generic;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.Accounting.Business.ConsolCosting;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Accounting.Integration;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.GUI.JobInvoicing
{
	public class CommonWorkSheetPostManagerGUIWrapper : ConsolInvoicingPostManagerGUIWrapper
	{
		public CommonWorkSheetPostManagerGUIWrapper(JobInvoicingPostingOption postingOption, BusinessObjectFactory plugInFactory, IEnumerable<Job> jobs, IJobCostingPlugIn consol, Form parentForm, ApportionmentListing consolCostListing)
			: base(postingOption, plugInFactory, jobs, consol, parentForm, consolCostListing)
		{
		}

		protected override JobInvoicingSecurityHelper GetSecurityHelper(IJobInvoicingPlugIn jobInvoicingPlugIn)
		{
			return new JobInvoicingSecurityHelper(jobInvoicingPlugIn == null ? Env.Security.None : jobInvoicingPlugIn.InvoicingSupporter.JobInvoicingSecurity);
		}
	}
}
