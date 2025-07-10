using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.IT.Business;

public interface ICustomsProfileListProviderSupportingData
{
	ZGuid CompanyPK { get; }

	IReadOnlyCollection<OrgHeader> GetEligibleOrganizations();
}
