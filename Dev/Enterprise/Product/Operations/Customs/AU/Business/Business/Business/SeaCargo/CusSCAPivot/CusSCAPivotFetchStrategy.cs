using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class CusSCAPivotFetchStrategy : EnterpriseBusinessObjectFetchStrategy
	{
		public CusSCAPivotFetchStrategy(CusSCAPivot pivot)
			: base(pivot)
		{ }

		new CusSCAPivot BusinessObject
		{
			get { return (CusSCAPivot)base.BusinessObject; }
		}

		protected override void FetchForFactorySaveCore()
		{
			base.FetchForFactorySaveCore();
			if (BusinessObject.ShouldAttachAnyUnattachedHouseCARSTS)
			{
				Factory.AddFetchHint(typeof(CMRCARSTMessage), BusinessObject.GetUnattachedCARSTSQuery());
			}
		}
	}
}
