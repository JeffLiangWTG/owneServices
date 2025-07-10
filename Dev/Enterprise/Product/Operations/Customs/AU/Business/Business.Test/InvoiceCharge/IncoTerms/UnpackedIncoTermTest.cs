using Enterprise.Customs.Common;
using Enterprise.Customs.Common.Testing;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	abstract class UnpackedIncoTermTest : IncoTermTest
	{
		protected override string CountryContext() => JobDeclaration.AUEdifice;

		protected override bool ExpectedValueForCanThisIncoTermHaveThisCharge(ICustomsChargeCode charge)
			=> charge.IsIncoTermNeutral && charge.Code != AUChargeCodeList.Codes.PackingCost;

		protected override bool ExpectedValueForThisChargeMandatory(ICustomsChargeCode chargeCode)
			=> chargeCode.Code == AUChargeCodeList.Codes.PackingCost;

		protected override bool ExpectedValueForThisChargeRecommeded(ICustomsChargeCode chargeCode)
			=> base.ExpectedValueForThisChargeRecommeded(chargeCode) || ExpectedValueForThisChargeMandatory(chargeCode);
	}
}
