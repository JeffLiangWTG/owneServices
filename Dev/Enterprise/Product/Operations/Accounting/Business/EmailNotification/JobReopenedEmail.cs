using System;
using CargoWise.Types;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Accounting.Registry.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Accounting.Business.EmailNotification
{
	public class JobReopenedEmail : AccountingEmailDef
	{
		public JobReopenedEmail(Job job, ZGuid reopenedUserPK)
		{
			subject = GetSubjectCore(job);
			body = GetBodyCore(job.Factory.Load<GlbStaff>(reopenedUserPK), job);
		}

		public JobReopenedEmail(Job job, ZGuid reopenedUserPK, ZGuid branchPk) : this(job, reopenedUserPK)
		{
			this.branchPk = branchPk;
		}

		readonly string body;
		readonly string subject;
		readonly ZGuid branchPk;

		string GetBodyCore(GlbStaff reopenedUser, params Job[] jobs)
		{
			string grantedBy = string.Empty;
			if (reopenedUser != null)
			{
				grantedBy = "\r\n" + Res.GetString("660e9cf1-f8e3-4fd6-8054-d8d98186ee92", "Access granted by {0} ({1})", reopenedUser.GS_LoginName, reopenedUser.GS_FullName);
			}
			var body = new ZStringBuilder(Res.GetString("783b1e4c-07ad-49b6-9f92-2e56ed96ad08", "The following {0} reopened by {1} ({2}) : {3}{4}{5}",
									jobs.Length == 1 ? Res.GetString("ee568af1-f439-44b3-9b46-7353ae97c102", "job was") : Res.GetString("2f46ff17-13df-47ab-8fe1-3bf8787c87c0", "jobs were"),
									GlbStaff.CurrentUser.GS_LoginName,
									GlbStaff.CurrentUser.GS_FullName,
									System.Environment.NewLine,
									GetFormattedJobInfos(jobs),
									grantedBy));

			return body.ToString();
		}

		string GetSubjectCore(params Job[] jobs)
		{
			return Res.GetString("728a3cff-364f-4341-b268-a78a0793f8b9", "{0} reopened by {1} ({2})",
				jobs.Length == 1 ? Res.GetString("89b15410-6285-4451-9917-be888a940eb0", "Job number") + " " + jobs[0].JH_JobNum + " " + Res.GetString("676d8c4f-31fe-4085-86e3-89cf52aa4dc8", "was") : Res.GetString("2e5798e9-7c31-4369-842d-06f9e1956f73", "Multiple jobs were"),
				GlbStaff.CurrentUser.GS_LoginName,
				GlbStaff.CurrentUser.GS_FullName);
		}

		#region Overrides
		protected override Guid GetRecipient()
		{
			return Recipient.GetFallBackValueAtAllLevels(Guid.Empty, branchPk.ToGuid(), Guid.Empty);
		}

		protected override GuidRegistryItem Recipient
		{
			get { return AccountingConfigurationRegistry.Instance.JobReopenNotifyGroup; }
			}

		protected sealed override string GetBody()
		{
			return body;
		}

		protected sealed override string GetSubject()
		{
			return subject;
		}

		#endregion

		#region Helper functions

		string GetFormattedJobInfos(Job[] jobs)
		{
			var result = new ZStringBuilder();
			foreach (var job in jobs)
			{
				result.AppendLine(GetJobInfo(job));
			}

			return result.ToString();
		}

		string GetJobInfo(Job job)
		{
			var jobClosedMessage = "";
			if (job.IsClosed)
			{
				jobClosedMessage = " " + Res.GetString("c68ad30e-1bd1-4261-b33a-70831f06b24a", "However this was closed again after some changes.");
			}
			var jobCompany = job.Company;
			var companyCode = jobCompany != null ? jobCompany.GC_Code : ZString.Empty;

			return Res.GetString("9edddb06-2e76-4b0d-9fc0-0cd9eb2a47bb", "Number: '{0}', Company: '{1}'.{2}", job.JH_JobNum, companyCode, jobClosedMessage);
		}

		#endregion
	}
}

