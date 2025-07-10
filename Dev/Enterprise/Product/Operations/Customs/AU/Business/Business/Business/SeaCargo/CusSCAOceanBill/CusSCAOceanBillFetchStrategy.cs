using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class CusSCAOceanBillFetchStrategy : EnterpriseBusinessObjectFetchStrategy
	{
		public CusSCAOceanBillFetchStrategy(CusSCAOceanBill oceanBill)
			: base(oceanBill)
		{
			this.oceanBill = oceanBill;
		}

		readonly CusSCAOceanBill oceanBill;

		protected override void FetchForFactorySaveCore()
		{
			base.FetchForFactorySaveCore();
			Factory.AddFetchHint(CusSCAHouseSchema.CA_CB, oceanBill.PK);
		}
	}
}
