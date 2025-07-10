using System.Data;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.IT.Business;

public sealed class CusAuthorizationUsage : EU.Business.CusAuthorizationUsage
{
	public CusAuthorizationUsage(BusinessObjectFactory factory, DataRow row) : base(factory, row)
	{
	}

	public bool IsForAuthorisationHeader(CusAuthorisationHeader authorisationHeader)
		=> authorisationHeader is not null
		&& AGC_Code == authorisationHeader.CPH_Type
		&& AGC_OH_Owner == authorisationHeader.CPH_OH_PermitHolder
		&& AGC_Number == authorisationHeader.CPH_Number;

	public new CusAuthorizationUsageValidation Validation => (CusAuthorizationUsageValidation)base.Validation;
	protected override EU.Business.CusAuthorizationUsageValidation GetNewValidation() => new CusAuthorizationUsageValidation(this);

	public new CusAuthorizationUsageLookups Lookups => (CusAuthorizationUsageLookups)base.Lookups;
	protected override EU.Business.CusAuthorizationUsageLookups GetNewLookups() => new CusAuthorizationUsageLookups(this);
}
