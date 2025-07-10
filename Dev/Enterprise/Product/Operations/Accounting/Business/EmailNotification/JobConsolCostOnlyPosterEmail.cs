using CargoWise.Common;
using Enterprise.Accounting.Integration;
using Enterprise.Accounting.Registry.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Accounting.Business.EmailNotification
{
	class JobConsolCostOnlyPosterEmail : AccountingEmailDef
	{
		public JobConsolCostOnlyPosterEmail(IJobCostingPlugIn consol, string errors)
			: base()
		{
			Argument.NotNull(consol, nameof(consol));
			Argument.NotNullOrEmpty(errors, nameof(errors));

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
			return Res.GetString("5c4e46ca-445b-44c2-945f-66713b56d764", "The workflow trigger action 'Post Consol Cost Only' ran for Consol {0}, but it failed due to the following errors:{1}{2}",
				consol.JK_UniqueConsignRef, System.Environment.NewLine, errors);
		}

		protected override string GetSubject()
		{
			return subject;
		}

		string GetSubjectCore(IJobCostingPlugIn consol)
		{
			return Res.GetString("6451ed27-121c-477a-b62e-db0d1d9fe7ad", "Consol cost only posting errors for Consol {0}", consol.JK_UniqueConsignRef);
		}

		protected override GuidRegistryItem Recipient
		{
			get { return AccountingConfigurationRegistry.Instance.JobPostingNotificationGroup; }
		}
	}
}


