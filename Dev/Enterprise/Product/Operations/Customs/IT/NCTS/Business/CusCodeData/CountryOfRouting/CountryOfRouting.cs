using System.Data;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.IT.NCTS.Business;

public class CountryOfRouting : EU.NCTS.Business.CountryOfRouting
{
	public CountryOfRouting(BusinessObjectFactory factory, DataRow row)
		: base(factory, row)
	{
	}

	protected override CusCodeDataValidation GetNewValidation() => new CountryOfRoutingValidation(this);
}
