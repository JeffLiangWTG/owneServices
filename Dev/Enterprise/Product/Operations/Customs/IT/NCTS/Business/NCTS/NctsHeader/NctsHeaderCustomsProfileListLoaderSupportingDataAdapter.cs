using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.IT.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.IT.NCTS.Business;

public class NctsHeaderCustomsProfileListLoaderSupportingDataAdapter : ICustomsProfileListProviderSupportingData
{
	public NctsHeaderCustomsProfileListLoaderSupportingDataAdapter(NctsHeader nctsHeader)
	{
		this.nctsHeader = Argument.NotNull(nctsHeader, nameof(nctsHeader));
	}

	readonly NctsHeader nctsHeader;

	ZGuid ICustomsProfileListProviderSupportingData.CompanyPK => nctsHeader.Company?.PK ?? ZGuid.Empty;

	IReadOnlyCollection<OrgHeader> ICustomsProfileListProviderSupportingData.GetEligibleOrganizations()
	{
		return GetEligibleOrganizations()
			.WhereNotNull()
			.ToList()
			.AsReadOnly();
	}

	IEnumerable<OrgHeader> GetEligibleOrganizations()
	{
		if (nctsHeader.IsPhase5)
		{
			yield return nctsHeader.Principal.Organisation;
			yield return nctsHeader.Consignor.Organisation;
			yield return nctsHeader.Consignee.Organisation;
		}
		else
		{
			yield return nctsHeader.DeclarantAddress?.Header;
		}
		yield return nctsHeader.MovementHeader?.Representative.Organisation;
	}
}
