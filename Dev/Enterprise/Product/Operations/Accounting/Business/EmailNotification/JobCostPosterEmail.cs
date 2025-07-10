using CargoWise.Common;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Accounting.Registry.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Accounting.Business.EmailNotification
{
	class JobCostPosterEmail : AccountingEmailDef
	{
		public JobCostPosterEmail(Job job, string errors)
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
			return Res.GetString("f2c08d3d-b21d-4e68-b77d-0e2e9d655e2e", "Cost posting was run as a workflow action, and an attempt to post cost(s) for {0} failed because of the following errors:{1}{2}",
				job.JH_JobNum, System.Environment.NewLine, errors);
		}

		protected override string GetSubject()
		{
			return subject;
		}

		string GetSubjectCore(Job job)
		{
			return Res.GetString("C55E62DA-0F0A-4F88-BADD-9C0D7757CB41", "Cost posting errors for {0}", job.JH_JobNum);
		}

		protected override GuidRegistryItem Recipient
		{
			get { return AccountingConfigurationRegistry.Instance.JobPostingNotificationGroup; }
		}
	}
}

