using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Business.GlobalChargeCode
{
	public class GlobalChargeCodeMapFetchStrategy : EnterpriseBusinessObjectFetchStrategy
	{
		public GlobalChargeCodeMapFetchStrategy(GlobalChargeCodeMap globalChargeCode)
			: base(globalChargeCode)
		{
		}

		GlobalChargeCodeMap GlobalChargeCode
		{
			get { return BusinessObject as GlobalChargeCodeMap; }
		}

		protected override void FetchForLoadCore()
		{
			base.FetchForLoadCore();
			Factory.AddFetchHint(OrgHeaderSchema.PK, GlobalChargeCode.YG_OH);
		}
	}
}
