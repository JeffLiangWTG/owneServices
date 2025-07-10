using CargoWise.Common;
using Enterprise.Accounting.Integration;
using Enterprise.Accounting.Registry.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Accounting.Business.EmailNotification
{
	class OverseasAgentChargesPosterEmail : AccountingEmailDef
	{
		public OverseasAgentChargesPosterEmail(IJobCostingPlugIn consol, string errors)
			: base()
		{
			Argument.NotNull(consol, "Consol");
			Argument.NotNullOrEmpty(errors, "Errors");

			this.errors = errors.Replace(System.Environment.NewLine, "\n").Replace("\n", System.Environment.NewLine);
			subject = GetSubjectCore(consol);
			body = GetBodyCore(consol);
		}

		readonly string errors;
		readonly string subject;
		readonly string body;

		protected override string GetBody()
		{
			return body;
		}

		string GetBodyCore(IJobCostingPlugIn consol)
		{
			return Res.GetString("BA87B64E-79A0-4ED5-AD7C-C8C57F8BC3E3", "The workflow trigger action 'Post Overseas Agent Charges' ran for Consol {0}, but it failed due to the following errors:{1}{2}",
				consol.JK_UniqueConsignRef, System.Environment.NewLine, errors);
		}

		protected override string GetSubject()
		{
			return subject;
		}

		string GetSubjectCore(IJobCostingPlugIn consol)
		{
			return Res.GetString("9EA0BEF5-0B39-466E-A68E-10A55AA5ED6B", "Overseas Agent Charges posting errors for Consol {0}", consol.JK_UniqueConsignRef);
		}

		protected override GuidRegistryItem Recipient
		{
			get { return AccountingConfigurationRegistry.Instance.JobPostingNotificationGroup; }
		}
	}
}

