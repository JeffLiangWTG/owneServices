using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Accounting.GUI.JobInvoicing
{
	public class InvoicingPostManagerGUIWrapper : PostManagerGUIWrapper
	{
		public InvoicingPostManagerGUIWrapper(JobInvoicingPostingOption postingOption, BusinessObjectFactory plugInFactory, Job job, Form parentForm, Action refreshJobCharges = null)
			: base(job?.Parent, postingOption, plugInFactory, parentForm, refreshJobCharges)
		{
			Jobs = LoadJobsInNewFactory(job);
		}

		protected override BasePostManager GetNewPostManager()
		{
			return new InvoicingPostManager(Jobs.Any() ? Jobs.First() : null, APInvoiceApprovalGUIProvider);
		}

		protected override void NothingPostedHandlerCore(string message)
		{
			if (Jobs.Any() && !Jobs.First().IsWorkOnHold)
			{
				base.NothingPostedHandlerCore(message);
			}
		}

		protected override string GetNothingPostedMessage()
		{
			return (OverriddenNothingPostedMessage != null && !OverriddenNothingPostedMessage.IsEmpty) ? OverriddenNothingPostedMessage : base.GetNothingPostedMessage();
		}

		public MultilingualString OverriddenNothingPostedMessage
		{
			get;
			set;
		}

		protected override string GetJobOnHoldMessage(IEnumerable<Job> jobs)
		{
			return Res.GetString("262d0031-7d26-45eb-b738-65b39a0bf158", "You cannot post because this job is on hold.\r\nTo post, change the job status from '{0}'.", JobHeaderStatus.WorkOnHold.Code);
		}

		public string ValidateJobAndParentSaved()
		{
			return GetPostManagerValidation(Jobs, PostingOption, OriginalJobs).ValidateJobAndParentSavedOnly();
		}

#if DEBUG

		public bool TransactionFactoryRefreshEnabled_ForTestOnly
		{
			get { return TransactionFactory.RefreshEnabled; }
		}

#endif
	}
}
