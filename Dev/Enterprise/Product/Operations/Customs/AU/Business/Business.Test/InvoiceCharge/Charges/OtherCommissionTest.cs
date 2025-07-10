using Enterprise.Customs.Common;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class OtherCommissionTest : Common.Testing.CommissionTest
	{
		protected override string ExpectedCode => AUChargeCodeList.Codes.OtherCommission;

		protected override string ExpectedDescription => AUChargeCodeList.Descriptions.OtherCommission;

		protected override ICustomsChargeCode GetChargeCodeToTest() => EdificeIncoTermAndCustomsChargeFactory.EdificeOtherCommission;

		protected override string GetCountryContext() => JobDeclaration.AUEdifice;
	}
}
