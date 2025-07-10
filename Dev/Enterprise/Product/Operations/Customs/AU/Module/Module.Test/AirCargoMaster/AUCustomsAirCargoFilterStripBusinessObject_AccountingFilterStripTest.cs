using Enterprise.Accounting.Integration.Testing;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.AU.Module.AirCargo.Testing
{
	sealed class AUCustomsAirCargoFilterStripBusinessObject_AccountingFilterStripTest : AccountingFilterStripTest<CusMAWB>
	{
		protected override CusMAWB GetNewBusinessObjectForFilterCollection()
		{
			var mawb = Factory.NewWithValidTestData<CusMAWB>();
			mawb.CM_MAWB = mawbNumber.ToString();
			mawbNumber++;
			return mawb;
		}

		protected override ModuleIdentifier FilterStripModuleID => ModuleIDs.Customs.AU.AirCargo;

		protected override bool ShouldUseBillingFilters => false;

		protected override void SetUp()
		{
			mawbNumber = 0;
			base.SetUp();
		}

		int mawbNumber;
	}
}
