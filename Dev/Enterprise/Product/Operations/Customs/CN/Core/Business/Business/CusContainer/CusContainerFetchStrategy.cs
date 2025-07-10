using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.CN.Business
{
	public class CusContainerFetchStrategy : Customs.Business.FetchStrategies.BaseCusContainerFetchStrategy
	{
		public CusContainerFetchStrategy(CusContainer container) : base(container)
		{
		}

		protected override void FetchForLoadCore()
		{
			base.FetchForLoadCore();
			Factory.AddFetchHint(RefContainerSchema.PK, BusinessObject.CO_RC);
			Factory.AddFetchHint(RefContainerCodeMapSchema.RCM_RC_Container, BusinessObject.CO_RC);
		}
	}
}
