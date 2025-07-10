using CargoWise.Common;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Accounting.Registry.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Accounting.Business.EmailNotification
{
	class SisterCompanyChargePosterEmail : AccountingEmailDef
	{
		public SisterCompanyChargePosterEmail(Job job, string errors)
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
			return Res.GetString("f288acda-8bdb-43b1-b302-9587a6d1c4e4", "Group Company Charge posting was run as a workflow action, and an attempt to post charge(s) for {0} failed because of the following errors:{1}{2}",
				job.JH_JobNum, System.Environment.NewLine, errors);
		}

		protected override string GetSubject()
		{
			return subject;
		}

		string GetSubjectCore(Job job)
		{
			return Res.GetString("4191ec1f-badc-40e1-8b24-bafe984d8e29", "Group Company Charge posting errors for {0}", job.JH_JobNum);
		}

		protected override GuidRegistryItem Recipient
		{
			get { return AccountingConfigurationRegistry.Instance.JobPostingNotificationGroup; }
		}
	}
}

