using System.Data;
using CargoWise.EntityFramework;
using Enterprise.Customs.BE.Business.Declaration;

namespace Enterprise.Customs.BE.Business;

[DependentBusinessObject(typeof(JobComInvoiceLine), "CusAuthorizationUsages")]
public class CusAuthorizationUsage : EU.Business.CusAuthorizationUsage
{
	public CusAuthorizationUsage(BusinessObjectFactory factory, DataRow row) : base(factory, row)
	{
	}

	protected override EU.Business.CusAuthorizationUsageLookups GetNewLookups() => new CusAuthorizationUsageLookups(this);
}
