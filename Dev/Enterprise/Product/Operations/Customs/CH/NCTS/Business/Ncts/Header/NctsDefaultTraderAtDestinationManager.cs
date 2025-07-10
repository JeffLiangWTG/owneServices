using CargoWise.Common;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.CH.NCTS.Business;

public class NctsDefaultTraderAtDestinationManager : INctsDefaultTraderAtDestinationManager
{
	public NctsDefaultTraderAtDestinationManager(NctsHeader nctsHeader)
	{
		this.nctsHeader = Argument.NotNull(nctsHeader, nameof(nctsHeader));
	}
	readonly NctsHeader nctsHeader;

	public void ApplyDefaultingIfEnabled()
	{
		var orgHeader = GlbBranch.CurrentBranch.OrgProxy ?? GlbCompany.CurrentCompany.OrgProxy;
		if (orgHeader != null)
		{
			using (nctsHeader.DestinationTrader.GetValidationSuspender())
			{
				nctsHeader.DestinationTrader.OrganisationPK = orgHeader.PK;
			}
		}
	}

	public bool IsEnabled() => true;
}
