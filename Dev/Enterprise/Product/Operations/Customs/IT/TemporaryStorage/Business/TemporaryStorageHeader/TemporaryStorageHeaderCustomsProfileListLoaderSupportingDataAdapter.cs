using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.IT.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.IT.TemporaryStorage.Business;

sealed class TemporaryStorageHeaderCustomsProfileListLoaderSupportingDataAdapter : ICustomsProfileListProviderSupportingData
{
	public TemporaryStorageHeaderCustomsProfileListLoaderSupportingDataAdapter(TemporaryStorageHeader temporaryStorageHeader)
	{
		this.temporaryStorageHeader = Argument.NotNull(temporaryStorageHeader, nameof(temporaryStorageHeader));
	}

	readonly TemporaryStorageHeader temporaryStorageHeader;

	ZGuid ICustomsProfileListProviderSupportingData.CompanyPK => GlbCompany.CurrentCompany.PK;

	IReadOnlyCollection<OrgHeader> ICustomsProfileListProviderSupportingData.GetEligibleOrganizations()
	{
		return GetEligibleOrganizations()
			.WhereNotNull()
			.ToList()
			.AsReadOnly();
	}

	IEnumerable<OrgHeader> GetEligibleOrganizations()
	{
		yield return temporaryStorageHeader.Declarant?.Header;
		yield return temporaryStorageHeader.Representative?.Header;
	}
}
