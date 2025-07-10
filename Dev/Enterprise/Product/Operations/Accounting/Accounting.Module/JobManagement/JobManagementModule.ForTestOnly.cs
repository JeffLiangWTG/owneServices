#if DEBUG

using System;
using System.Collections.Generic;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Accounting.Module
{
	public partial class JobManagementModule
	{
		public void CloseJobs_ForTestOnly(BusinessObject[] jobsToCloseFromModuleFactory)
		{
			CloseJobs(jobsToCloseFromModuleFactory);
		}

		public void ViewOperationsDetails_ForTestOnly(object sender, EventArgs e)
		{
			ViewOperationsDetails(sender, e);
		}

		public void MarkJobHeaderAsInactiveEventHandler_ForTestOnly(object sender, EventArgs e)
		{
			MarkJobHeaderAsInactiveEventHandler(sender, e);
		}

		public void ReOpenJobEventHandler_ForTestOnly(object sender, EventArgs e)
		{
			ReOpenJobEventHandler(sender, e);
		}

		public void BulkJobCloseEventHandler_ForTestOnly(object sender, EventArgs e)
		{
			BulkJobCloseEventHandler(sender, e);
		}

		public MenuItem[] GetNewActionMenuItems_ForTestOnly()
		{
			return GetNewActionMenuItems();
		}

		public IZForm CurrentEditForm_ForTestOnly => CurrentEditForm;

		public void CloseJobsCore_ForTestOnly(IEnumerable<Job> jobsToClose) => CloseJobsCore(jobsToClose);

		public void SetFactoryForSaveCloseJobs(BusinessObjectFactory factory)
		{
			testFactory = factory;
		} 
	}
}

#endif
