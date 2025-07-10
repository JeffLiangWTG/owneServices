using System;
using Enterprise.MasterFiles.Business;
using WTG.ProductionRules.Business.Common;
using WTG.ProductionRules.Business.JobBillingDefaulting;
using WTG.ProductionRules.Core;

namespace Enterprise.Accounting.RulesEngine.Facts
{
	public class JobFact : InputFactWithUserDefinedProperties, IJobFact
	{
		public JobFact(JobHeader job, IEnvironmentFact environmentFact, IOrganisationWithMainAddressFact localClientFact, IStaffFact salesRepFact)
		{
			Argument.NotNull(job, nameof(job));
			Argument.NotNull(environmentFact, nameof(environmentFact));

			PK = job.PK.IsValid ? job.PK.ToGuid() : Guid.Empty;
			LocalClient = new FactLeftJoin<IOrganisationWithMainAddressFact>(localClientFact);
			SalesRep = new FactLeftJoin<IStaffFact>(salesRepFact);

			CurrentBranch = environmentFact.CurrentBranch;
			CurrentDepartment = environmentFact.CurrentDepartment;
			CurrentCompanyCountry = environmentFact.CurrentCompanyCountry;
		}

		public Guid PK { get; }

		public FactLeftJoin<IStaffFact> SalesRep { get; }

		public FactLeftJoin<IOrganisationWithMainAddressFact> LocalClient { get; }

		public FactJoin<IBranchFact> CurrentBranch { get; }

		public FactJoin<IDepartmentFact> CurrentDepartment { get; }

		public string CurrentCompanyCountry { get; }
	}
}
