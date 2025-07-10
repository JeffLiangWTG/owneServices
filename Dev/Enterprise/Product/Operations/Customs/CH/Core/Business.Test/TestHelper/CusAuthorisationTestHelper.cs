using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.CH.Business.Testing;

public class CusAuthorisationTestHelper
{
	public CusAuthorisationTestHelper(BusinessObjectFactory factory, OrgHeader defaultPermitHolder = null)
	{
		Factory = factory;
		DefaultPermitHolder = defaultPermitHolder;
	}
	BusinessObjectFactory Factory { get; }
	OrgHeader DefaultPermitHolder { get; }

	public void CreateCusAuthorisationHeader(string number, string type, OrgHeader permitHolder = null)
	{
		var authorization1 = Factory.New<CusAuthorisationHeader>();
		authorization1.CPH_Type = type;
		authorization1.CPH_OH_PermitHolder = permitHolder?.PK ?? DefaultPermitHolder?.PK ?? ZGuid.Empty;
		authorization1.CPH_Number = number;
		authorization1.CPH_StartDate = ZDate.Today.AddDays(-10);
		Factory.Save();
	}
}
