using Enterprise.Customs.Common;
using Enterprise.Customs.Common.BR;

namespace Enterprise.Customs.BR.Business.Testing
{
	class ExportIncoTermAndCustomsChargeFactoryTest : IncoTermAndCustomsChargeFactoryTest
	{
		public void TestDistributeBy()
		{
			var chargesOverseas = incoTermAndChargeFactory.GetCharge(CustomsChargeCodeProvider.OverseasInsurance.Code);
			AssertEquals("OverseasInsurance should be", ChargeDistributeByList.Codes.FOB, chargesOverseas.DistributeBy);

			var chargesFreight = incoTermAndChargeFactory.GetCharge(CustomsChargeCodeProvider.OverseasFreight.Code);
			AssertEquals("OverseasFreight should be", ChargeDistributeByList.Codes.NetWeight, chargesFreight.DistributeBy);
		}

		protected override string GetCountryContext() => Core.Constants.CountryCodes.Brazil + BRJobMessageTypeList.Codes.Export;
	}
}
