using System.Data;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.IN.Business;

public partial class JobComInvoiceGroupHeader : AutoJobComInvoiceGroupHeader
{
	public JobComInvoiceGroupHeader(BusinessObjectFactory factory, DataRow row)
		: base(factory, row)
	{
	}

	protected override string GetStandaloneIncoTermAndChargeFactoryCountryContext()
	{
		return base.GetStandaloneIncoTermAndChargeFactoryCountryContext() + this.GetCustomsChargeTypeListCacheKey();
	}
}
