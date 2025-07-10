using System.Linq;
using System.Threading;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.CA.Registry;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.CA.ServiceTasks
{
	public abstract class ServiceProviderImplUnderDefaultBranch : Customs.ServiceTasks.CustomsServiceTask
	{
		protected ServiceProviderImplUnderDefaultBranch() { }

		protected sealed override void RunTaskCore(CancellationToken token)
		{
			using (DisposableEnvironment.ForBranch(GetDefaultBranchForServiceTasks(new BusinessObjectFactory()).PK.ToGuid(), true))
			{
				RunTaskMain(token);
			}
		}

		protected GlbBranch GetDefaultBranchForServiceTasks(BusinessObjectFactory factory)
		{
			ZGuid defaultBranchPK = CACustomsDataRegistry.Instance.DefaultBranchForServiceTasks.Value;

			GlbBranch taskBranch = null;
			if (!defaultBranchPK.IsEmpty)
			{
				taskBranch = factory.Load<GlbBranch>(defaultBranchPK);
			}

			if (taskBranch == null || !taskBranch.GB_IsActive)
			{
				taskBranch = GetAnyActiveBranchPreferCanadianCompany(factory);
			}

#if DEBUG
			LastTaskBranchPKForTesting = taskBranch.PK;
#endif

			return taskBranch;
		}

		GlbBranch GetAnyActiveBranchPreferCanadianCompany(BusinessObjectFactory factory)
		{
			var activeCompany = GlbCompany.GetActiveCompanies(Core.Constants.CountryCodes.Canada, factory).FirstOrDefault()
				?? GlbCompany.GetActiveCompanies(countryCode: null, factory: factory).FirstOrDefault();

			return activeCompany?.FirstActiveBranch;
		}

		protected abstract void RunTaskMain(CancellationToken token);

#if DEBUG
		internal ZGuid LastTaskBranchPKForTesting { get; set; }
#endif

	}
}
