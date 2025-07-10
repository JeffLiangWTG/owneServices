using CargoWise.Types;

namespace Enterprise.Customs.AU.Declaration.Business;

public class MiscOrganisationWrapper : IOrgHeaderWrapper
{
	public MiscOrganisationWrapper(JobDeclaration declaration)
	{
		this.declaration = declaration;
	}
	readonly JobDeclaration declaration;

	#region IOrgHeaderWrapper Members

	ZString IOrgHeaderWrapper.GoodsOwnerPartyID
	{
		get { return declaration.ZA_GoodsOwnerPartyIDHidden; }
	}

	bool IOrgHeaderWrapper.IsCompanyOrg
	{
		get { return false; }
	}

	#endregion
}
