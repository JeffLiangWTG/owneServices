using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Accounting.CriticalValidation;
using Enterprise.Security;
using static Enterprise.MasterFiles.Business.Accounting.CriticalValidation.CriticalValidationInfoCollectorService;

namespace Enterprise.Accounting.Business.JobInvoicing
{
	public static class JobReopenSecurityCheckHelper
	{
		#region Interactive

		public static bool CanReopenJob_InteractiveSecurityCheck<T>(T provider, IEnumerable<Job> jobs) where T : IFactoryProvider, ISecurityOverrideProviderSource
		{
			var hasContext = provider.Factory.HasContext(BusinessContext.AllowReopenJobWhenImporting)
				|| provider.Factory.HasContext(BusinessContext.CASS)
				|| provider.Factory.HasContext(BusinessContext.AllowReopenJobWhenAutoPostingARCreditNote);

			if (hasContext)
			{
				return true;
			}
			else
			{
				var hasreopenJobPastAllowedReOpenPeriod = true;
				var hasReOpenJobAccess = false;

				if (jobs != null && jobs.Any(x => x != null && x.IsPastAllowedRestrictionDate()))
				{
					hasreopenJobPastAllowedReOpenPeriod = provider.Provider.SecurityCertificates[Env.Security.ReopenJobPastAllowedReOpenPeriod]?.IsAllowed ?? false;
				}

				if (hasreopenJobPastAllowedReOpenPeriod)
				{
					hasReOpenJobAccess = provider.Provider.SecurityCertificates[Env.Security.ReopenJob]?.IsAllowed ?? false;
				}
				var jobPK = (jobs != null && jobs.Any()) ? jobs.FirstOrDefault().PK : Guid.NewGuid();

				CriticalValidationInfoCollectorService.GetOrCreateService(provider.Factory).AddInfoWhenAllowed(jobPK, CriticalValidationInfoCollectorServiceKeyType.LoginFormCancelledJobReopenJobChargeCreatedOnClosedJobStackTrace, () =>
				{
					return FormattableString.Invariant(
							$@"ReopenJobPastAllowedReOpenPeriod: {hasreopenJobPastAllowedReOpenPeriod}
ReopenJob: {hasReOpenJobAccess}
StackTrace: {System.Environment.NewLine + System.Environment.StackTrace}");
				}, collectionFrequency: CollectionFrequency.CollectAlways_ForEveryUserAndAction_THIS_CAN_IMPACT_PERFORMANCE);

				return hasReOpenJobAccess && hasreopenJobPastAllowedReOpenPeriod;
			}
		}

		public static bool CanReopenJob_InteractiveSecurityCheck<T>(T provider, Job job) where T : IFactoryProvider, ISecurityOverrideProviderSource
		{
			return CanReopenJob_InteractiveSecurityCheck(provider, new Job[] { job });
		}

		#endregion

		#region NonInteractive

		public static bool CanReopenJob_NonInteractiveSecurityCheck(BusinessObjectFactory factory, IEnumerable<Job> jobs)
		{
			return CanReopenJobWithSecurity_NonInteractiveSecurityCheck(factory, jobs).CheckResult;
		}

		public static bool CanReopenJob_NonInteractiveSecurityCheck(BusinessObjectFactory factory, Job job)
		{
			return CanReopenJobWithSecurity_NonInteractiveSecurityCheck(factory, new Job[] { job }).CheckResult;
		}

		public static (bool CheckResult, SecurityCheckpoint SecurityCheckpoint) CanReopenJobWithSecurity_NonInteractiveSecurityCheck(BusinessObjectFactory factory, Job job)
		{
			return CanReopenJobWithSecurity_NonInteractiveSecurityCheck(factory, new Job[] { job });
		}

		static (bool CheckResult, SecurityCheckpoint SecurityCheckpoint) CanReopenJobWithSecurity_NonInteractiveSecurityCheck(BusinessObjectFactory factory, IEnumerable<Job> jobs)
		{
			var hasContext = factory.HasContext(BusinessContext.AllowReopenJobWhenImporting)
				|| factory.HasContext(BusinessContext.CASS)
				|| factory.HasContext(BusinessContext.AllowReopenJobWhenAutoPostingARCreditNote);

			if (hasContext)
			{
				return (true, null);
			}
			else
			{
				if (jobs != null && jobs.Any(x => x != null && x.IsPastAllowedRestrictionDate()))
				{
					return (Env.Security.ReopenJobPastAllowedReOpenPeriod.IsAllowed, Env.Security.ReopenJobPastAllowedReOpenPeriod);
				}

				return (Env.Security.ReopenJob.IsAllowed, Env.Security.ReopenJob);
			}
		}

		#endregion
	}
}
