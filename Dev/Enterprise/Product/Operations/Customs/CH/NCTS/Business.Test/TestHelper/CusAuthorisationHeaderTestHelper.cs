using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.CH.NCTS.Business.Testing;

public static class CusAuthorisationHeaderTestHelper
{
	public static CusAuthorisationHeader CreateAuthorisationHeader(BusinessObjectFactory factory, ZString type, ZGuid permitHolder, ZString number, ZDate startDate, ZDate endDate)
	{
		var authorisationHeader = factory.New<CusAuthorisationHeader>();
		authorisationHeader.CPH_Type = type;
		authorisationHeader.CPH_OH_PermitHolder = permitHolder;
		authorisationHeader.CPH_Number = number;
		authorisationHeader.CPH_StartDate = startDate;
		authorisationHeader.CPH_EndDate = endDate;
		return authorisationHeader;
	}

	public static CusAuthorisationHeader CreateAuthorisationHeader(BusinessObjectFactory factory, ZString type, ZGuid permitHolder, ZString number)
	{
		var startDate = ZDate.Today.AddDays(-10);
		var endDate = ZDate.Today.AddDays(10);
		var auth = CreateAuthorisationHeader(factory, type, permitHolder, number, startDate, endDate);
		return auth;
	}
}
