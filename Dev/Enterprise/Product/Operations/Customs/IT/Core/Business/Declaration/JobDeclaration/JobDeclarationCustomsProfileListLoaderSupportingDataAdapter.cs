using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.IT.Business.Declaration;

public class JobDeclarationCustomsProfileListLoaderSupportingDataAdapter : ICustomsProfileListProviderSupportingData
{
	public JobDeclarationCustomsProfileListLoaderSupportingDataAdapter(JobDeclaration declaration)
	{
		this.declaration = Argument.NotNull(declaration, nameof(declaration));
	}

	readonly JobDeclaration declaration;

	ZGuid ICustomsProfileListProviderSupportingData.CompanyPK => !declaration.IsUCC6 ? declaration.CompanyPK : GlbCompany.CurrentCompany.PK;

	IReadOnlyCollection<OrgHeader> ICustomsProfileListProviderSupportingData.GetEligibleOrganizations()
	{
		return GetEligibleOrganizations()
			.WhereNotNull()
			.ToList()
			.AsReadOnly();
	}

	IEnumerable<OrgHeader> GetEligibleOrganizations()
	{
		yield return declaration.DeclarantAddress?.Header;
		yield return declaration.RepresentativeOrgAddress?.Header;
	}
}
