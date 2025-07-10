using CargoWise.Common;
using Enterprise.Accounting.Integration;
using Enterprise.Accounting.Registry.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Accounting.Business.EmailNotification
{
	class ConsolCostPosterEmail : AccountingEmailDef
	{
		public ConsolCostPosterEmail(IJobCostingPlugIn consol, string errors)
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
			return Res.GetString("096ed3cf-cc3d-4f47-a0e8-c6aa1e32e68c", "The workflow trigger action 'Post All Costs' ran for Consol {0}, but it failed due to the following errors:{1}{2}",
				consol.JK_UniqueConsignRef, System.Environment.NewLine, errors);
		}

		protected override string GetSubject()
		{
			return subject;
		}

		string GetSubjectCore(IJobCostingPlugIn consol)
		{
			return Res.GetString("d2104aba-5d92-417f-ac99-e3ce00757239", "All Cost posting errors for Consol {0}", consol.JK_UniqueConsignRef);
		}

		protected override GuidRegistryItem Recipient
		{
			get { return AccountingConfigurationRegistry.Instance.JobPostingNotificationGroup; }
		}
	}
}

