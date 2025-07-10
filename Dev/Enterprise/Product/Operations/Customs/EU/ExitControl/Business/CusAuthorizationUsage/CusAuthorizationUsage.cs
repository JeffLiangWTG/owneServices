using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;

namespace Enterprise.Customs.EU.ExitControl.Business;

[DependentBusinessObject(typeof(CusExitReport), nameof(CusExitReport.CusAuthorizationUsages))]
public class CusAuthorizationUsage(BusinessObjectFactory factory, DataRow row) : EU.Business.CusAuthorizationUsage(factory, row)
{
	[ResourceStringData("Enterprise.Customs.EU.ExitControl.Business|AGC_Code", Caption = "Type")]
	public override ZString AGC_Code { get => base.AGC_Code; set => base.AGC_Code = value; }

	protected override EU.Business.CusAuthorizationUsageLookups GetNewLookups() => new CusAuthorizationUsageLookups(this);
}
