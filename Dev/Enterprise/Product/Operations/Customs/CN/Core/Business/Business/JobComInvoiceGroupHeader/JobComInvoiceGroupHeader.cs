using System.Data;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.CN.Business
{
	public partial class JobComInvoiceGroupHeader : AutoJobComInvoiceGroupHeader, Integration.Customs.CN.IJobComInvoiceGroupHeader
	{
		public JobComInvoiceGroupHeader(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		protected override string GetStandaloneIncoTermAndChargeFactoryCountryContext() => base.GetStandaloneIncoTermAndChargeFactoryCountryContext() + this.GetIncoTermChargeFactoryCacheKey();
	}
}
