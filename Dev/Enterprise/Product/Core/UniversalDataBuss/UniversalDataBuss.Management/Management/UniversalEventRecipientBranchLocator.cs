using System.Linq;
using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.MasterFiles.Integration;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.UniversalDataBuss.Management
{
	class UniversalEventRecipientBranchLocator : RecipientBranchLocator
	{
		protected override ZBool TryGetBranchFromEventBranch(IXmlSessionTracker logger, IDataContextDataObject dataContext, IGlbCompany company, IGlbBranch[] companyBranches, out ZGuid branchPK)
		{
			if (dataContext != null && !dataContext.EventBranchCode.IsEmpty)
			{
				var eventBranch = companyBranches.FirstOrDefault(b => b.GB_Code.EqualsIgnoringCase(dataContext.EventBranchCode));
				if (eventBranch != null)
				{
					branchPK = eventBranch.PK;
					return true;
				}

				logger.LogBoth(LogType.Warning, UniversalXmlUserContextLogging.EventBranchIsNotActiveOnDataContextCompany(company.GC_Code, dataContext.EventBranchCode));
			}

			branchPK = ZGuid.Empty;
			return false;
		}

		protected override ZBool UseEventBranchToDecideImportCompanyCore => true;
	}
}
