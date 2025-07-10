using System.Collections.Generic;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.Accounting.Business.ConsolCosting;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Accounting.GUI.JobInvoicing.ConsolCosting;
using Enterprise.Accounting.Integration;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.GUI.JobInvoicing
{
	public class ApportionmentForCommonWorkSheetPlugin : ApportionmentPlugin
	{
		public ApportionmentForCommonWorkSheetPlugin(IBusiness hostEntity) : base(hostEntity)
		{ }

		protected override ConsolInvoicingPostManagerGUIWrapper GetPostManagerGUIWrapper(JobInvoicingPostingOption postingOption, BusinessObjectFactory plugInFactory, IEnumerable<Job> jobs, IJobCostingPlugIn consol, Form parentForm, ApportionmentListing consolCostListing)
		{
			return new CommonWorkSheetPostManagerGUIWrapper(postingOption, plugInFactory, jobs, consol, parentForm, consolCostListing);
		}
	}
}
