using System.Linq;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.MasterFiles.Integration;
using Enterprise.Messaging.Integration;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.UniversalDataBuss.Management
{
	class RecipientBranchLocator : IRecipientBranchLocator
	{
		/// <summary>
		/// 1. Company has no active branches, reject
		/// 2. Company has 1 active branch, return this branch
		/// 3. TryGetBranchFromEventBranch if method is overridden
		/// 4. Current branch belongs to company, return this branch
		/// 5. Return first active branch
		/// </summary>

		public ZGuid GetBranchPK(IEDIMessage message, ITopLevelDataObject topLevelDataObject, IGlbCompany glbCompany, IXmlSessionTracker logger) => GetBranchPKCore(message, topLevelDataObject, glbCompany, logger); 
		protected virtual ZGuid GetBranchPKCore(IEDIMessage message, ITopLevelDataObject topLevelDataObject, IGlbCompany glbCompany, IXmlSessionTracker logger)
		{
			var activeBranches = glbCompany.GetActiveBranches().ToArray();
			if (activeBranches.Length == 0)
			{
				logger.LogBoth(LogType.Error, UniversalXmlUserContextLogging.CompanyHasNoActiveBranches(glbCompany.GC_Code));
				return ZGuid.Empty;
			}

			if (activeBranches.Length == 1)
			{
				var onlyActiveBranch = activeBranches.First();
				logger.LogVerboseOnly(LogType.Information, UniversalXmlUserContextLogging.TargetingOnlyActiveBranch(glbCompany.GC_Code, onlyActiveBranch.GB_Code));
				return onlyActiveBranch.PK;
			}

			if (TryGetBranchFromEventBranch(logger, topLevelDataObject.DataContext, glbCompany, activeBranches, out ZGuid branchPK))
			{
				return branchPK;
			}

			var branchCode = string.Empty;
			if (Env.CurrentCompanyPK != glbCompany.PK || !Env.CurrentBranch.IsActive)
			{
				branchCode = glbCompany.FirstActiveBranchCode;
				logger.LogVerboseOnly(LogType.Information, UniversalXmlUserContextLogging.TargetingFirstActiveBranch(glbCompany.GC_Code, branchCode));
			}
			else
			{
				branchCode = Env.CurrentBranch.Code;
			}

			var branch = activeBranches.FirstOrDefault(checkBranch => checkBranch.GB_Code.EqualsIgnoringCase(branchCode));

			if (branch == null)
			{
				var errorMessage = $"Company has no active branch.\n\nCompany:\nCode: {glbCompany.GC_Code}\nActive Branches:\n";
				foreach (var activeBranch in activeBranches)
				{
					errorMessage += $"{activeBranch.GB_Code}\n";
				}
				errorMessage += $"\nEnv.CurrentCompany:\nCode: {Env.CurrentCompany.Code}\n";
				errorMessage += $"\nEnv.CurrentBranch:\nCode: {Env.CurrentBranch.Code}\nIsActive: {Env.CurrentBranch.IsActive}";

				ErrorReporter.ReportOnce(errorMessage);
				logger.LogBoth(LogType.Error, UniversalXmlUserContextLogging.CompanyHasNoActiveBranches(glbCompany.GC_Code));
				return ZGuid.Empty;
			}

			return branch.PK;
		}

		public ZBool UseEventBranchToDecideImportCompany => UseEventBranchToDecideImportCompanyCore;
		public ZBool RequiresCodesMappaedToTarget => RequiresCodesMappaedToTargetCore;

		protected virtual ZBool UseEventBranchToDecideImportCompanyCore => false;
		protected virtual ZBool RequiresCodesMappaedToTargetCore => true;

		protected virtual ZBool TryGetBranchFromEventBranch(IXmlSessionTracker logger, IDataContextDataObject dataContext, IGlbCompany company, IGlbBranch[] companyBranches, out ZGuid branchPK)
		{
			branchPK = ZGuid.Empty;
			return false;
		}
	}
}
