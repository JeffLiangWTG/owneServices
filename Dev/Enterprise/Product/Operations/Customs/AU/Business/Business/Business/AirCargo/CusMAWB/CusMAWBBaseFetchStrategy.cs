using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.AU.Declaration.Business
{
	class CusMAWBBaseFetchStrategy : EnterpriseBusinessObjectFetchStrategy
	{
		public CusMAWBBaseFetchStrategy(CusMAWBBase cusMawb) : base(cusMawb)
		{
			this.cusMawb = cusMawb;
		}
		readonly CusMAWBBase cusMawb;

		protected override void FetchForValidateCore()
		{
			foreach (var bill in cusMawb.ChildBills)
			{
				bill.FetchStrategy.FetchForValidate();
			}
			base.FetchForValidateCore();
		}
	}
}
