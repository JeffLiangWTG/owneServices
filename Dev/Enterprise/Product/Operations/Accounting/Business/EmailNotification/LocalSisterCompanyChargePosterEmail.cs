using CargoWise.Common;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Accounting.Registry.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Accounting.Business.EmailNotification
{
	class LocalSisterCompanyChargePosterEmail : AccountingEmailDef
	{
		public LocalSisterCompanyChargePosterEmail(Job job, string errors)
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
			return Res.GetString("ae6cdbc9-fea2-4b93-8590-88f046c14583", "Local Group Company Charge posting was run as a workflow action, and an attempt to post charge(s) for {0} failed because of the following errors:{1}{2}",
				job.JH_JobNum, System.Environment.NewLine, errors);
		}

		protected override string GetSubject()
		{
			return subject;
		}

		string GetSubjectCore(Job job)
		{
			return Res.GetString("eadd935b-e9a4-4172-a8a5-74f6ea306c46", "Local Group Company Charge posting errors for {0}", job.JH_JobNum);
		}

		protected override GuidRegistryItem Recipient
		{
			get { return AccountingConfigurationRegistry.Instance.JobPostingNotificationGroup; }
		}
	}
}

