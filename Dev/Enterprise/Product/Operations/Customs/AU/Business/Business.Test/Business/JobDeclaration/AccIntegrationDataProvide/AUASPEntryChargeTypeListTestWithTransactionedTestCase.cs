using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Registry.Business;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class AUASPEntryChargeTypeListTestWithTransactionedTestCase : TestCaseWithFactory
	{
		public void TestGetSpecialChargeCodePks()
		{
			var chargeTypeList = new AUASPEntryChargeTypeListForTest();
			var chargeCodeWithDate = new ChargeCodeWithDate();
			var chargeCode = Guid.NewGuid();
			chargeCodeWithDate.ChargeCode = chargeCode;

			RatingDataRegistry.Instance.CustomsQuarantineChargeCode.SetValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, chargeCodeWithDate);
			var list = chargeTypeList.GetSpecialChargeCodePks(Env.CurrentCompanyPK);
			AssertContainsExactElementsInAnyOrder(new ZGuid[] { chargeCode }, list);
		}

		public void TestGetChargeTypeSpecificRegistrySetting()
		{
			var chargeTypeList = new AUASPEntryChargeTypeListForTest();
			var chargeCodeWithDate = new ChargeCodeWithDate();
			var chargeCode = Guid.NewGuid();
			chargeCodeWithDate.ChargeCode = chargeCode;

			RatingDataRegistry.Instance.CustomsQuarantineChargeCode.SetValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, chargeCodeWithDate);
			var result = chargeTypeList.GetChargeTypeSpecificRegistrySetting(CusEntryChargeTypeList.Codes.AQISServicePaymentAmount);
			AssertEquals(CusEntryChargeTypeList.Codes.AQISServicePaymentAmount, result.ChargeType);
			AssertEquals(chargeCode, result.AC_ChargeCode);

			result = chargeTypeList.GetChargeTypeSpecificRegistrySetting(CusEntryChargeTypeList.Codes.AQISContainerCharges);
			AssertNull(result);
		}

		public void TestGetChargeCodePKForASP()
		{
			var chargeCodeWithDate = new ChargeCodeWithDate();
			var chargeCode = Guid.NewGuid();
			chargeCodeWithDate.ChargeCode = chargeCode;
			AssertEquals(Guid.Empty, AUASPEntryChargeTypeListForTest.GetChargeCodePKForASP(Env.CurrentCompanyPK));

			RatingDataRegistry.Instance.CustomsQuarantineChargeCode.SetValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, chargeCodeWithDate);
			AssertEquals(chargeCode, AUASPEntryChargeTypeListForTest.GetChargeCodePKForASP(Env.CurrentCompanyPK));
		}
	}
}
