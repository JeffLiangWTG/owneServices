using Enterprise.Customs.Common;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class BuyingCommissionTest : Common.Testing.CommissionTest
	{
		protected override string ExpectedCode => AUChargeCodeList.Codes.BuyingCommission;

		protected override string ExpectedDescription => AUChargeCodeList.Descriptions.BuyingCommission;

		protected override ICustomsChargeCode GetChargeCodeToTest() => EdificeIncoTermAndCustomsChargeFactory.EdificeBuyingCommission;

		protected override string GetCountryContext() => JobDeclaration.AUEdifice;
	}
}
