using CargoWise.Common;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Accounting.Registry.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Accounting.Business.EmailNotification
{
	class JobOverseasAgentChargesPosterEmail : AccountingEmailDef
	{
		public JobOverseasAgentChargesPosterEmail(Job job, string errors)
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
			return Res.GetString("3565bac2-c380-4898-a3ca-f2c94cbd573b", "Overseas Agent Charges posting was run as a workflow action, and an attempt to post overseas agent charge(s) for {0} failed because of the following errors:{1}{2}",
				job.JH_JobNum, System.Environment.NewLine, errors);
		}

		protected override string GetSubject()
		{
			return subject;
		}

		string GetSubjectCore(Job job)
		{
			return Res.GetString("e88e56ac-6333-4645-be96-cd13dc77a794", "Overseas Agent Charge posting errors for {0}", job.JH_JobNum);
		}

		protected override GuidRegistryItem Recipient
		{
			get { return AccountingConfigurationRegistry.Instance.JobPostingNotificationGroup; }
		}
	}
}

