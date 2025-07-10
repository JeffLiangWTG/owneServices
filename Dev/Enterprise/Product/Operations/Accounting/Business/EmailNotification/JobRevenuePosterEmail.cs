using CargoWise.Common;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Accounting.Registry.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Accounting.Business.EmailNotification
{
	class JobRevenuePosterEmail : AccountingEmailDef
	{
		public JobRevenuePosterEmail(Job job, string errors)
			: base()
		{
			Argument.NotNull(job, "Job");
			Argument.NotNullOrEmpty(errors, "Errors");

			this.errors = errors.Replace(System.Environment.NewLine, "\n").Replace("\n", System.Environment.NewLine);
			subject = GetSubjectCore(job);
			body = GetBodyCore(job);
		}

		readonly string errors;
		readonly string subject;
		readonly string body;

		protected override string GetBody()
		{
			return body;
		}

		string GetBodyCore(Job job)
		{
			return Res.GetString("83dfa6b7-5f49-4676-a3e1-229e77008ca9", "Revenue posting was run as a workflow action, and an attempt to post revenue for {0} failed because of the following errors:{1}{2}",
				job.JH_JobNum, System.Environment.NewLine, errors);
		}

		protected override string GetSubject()
		{
			return subject;
		}

		string GetSubjectCore(Job job)
		{
			return Res.GetString("80ccc55a-bfe1-4d9c-96d2-5de653347d0a", "Revenue posting errors for {0}", job.JH_JobNum);
		}

		protected override GuidRegistryItem Recipient
		{
			get { return AccountingConfigurationRegistry.Instance.JobPostingNotificationGroup; }
		}
	}
}


