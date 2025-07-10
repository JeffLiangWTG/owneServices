using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Business.GlobalChargeCode
{
	public class GlobalChargeCodeMapPivotFetchStrategy : EnterpriseBusinessObjectFetchStrategy
	{
		public GlobalChargeCodeMapPivotFetchStrategy(GlobalChargeCodeMapPivot globalChargeCodePivot)
			: base(globalChargeCodePivot)
		{
		}

		GlobalChargeCodeMapPivot GlobalChargeCodePivot
		{
			get { return BusinessObject as GlobalChargeCodeMapPivot; }
		}

		protected override void FetchForLoadCore()
		{
			base.FetchForLoadCore();
			Factory.AddFetchHint(AccGlobalChargeCodeMapSchema.PK, GlobalChargeCodePivot.YP_YG);
			Factory.AddFetchHint(AccChargeCodeSchema.PK, GlobalChargeCodePivot.YP_AC);
		}
	}
}
