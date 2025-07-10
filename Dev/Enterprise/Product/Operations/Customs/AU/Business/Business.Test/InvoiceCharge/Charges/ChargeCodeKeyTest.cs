using Enterprise.Customs.Common;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class ChargeCodeKeyTest : TestCase
	{
		public void TestBuyingCommission()
		{
			AssertChargeCodeKey(new ChargeCodeChargeKey(AUChargeCodeList.Codes.BuyingCommission, true, true), ChargeCodeKey.DutiableBuyingCommission);
		}

		public void TestOtherCommission()
		{
			AssertChargeCodeKey(new ChargeCodeChargeKey(AUChargeCodeList.Codes.OtherCommission, true, true), ChargeCodeKey.DutiableOtherCommission);
		}

		public void TestCommission()
		{
			AssertChargeCodeKey(new ChargeCodeChargeKey(CustomsChargeTypeList.Codes.Commission, true, true), ChargeCodeKey.DutiableCommission);
			AssertChargeCodeKey(new ChargeCodeChargeKey(CustomsChargeTypeList.Codes.Commission, false, false), ChargeCodeKey.NonDutiableCommission);
		}

		public void TestExWorksAmount()
		{
			AssertChargeCodeKey(new ChargeCodeChargeKey(CustomsChargeTypeList.Codes.ExWorks, true, true), ChargeCodeKey.DutyGSTExWorksAmount);
			AssertChargeCodeKey(new ChargeCodeChargeKey(CustomsChargeTypeList.Codes.ExWorks, false, true), ChargeCodeKey.NonDutyGSTExWorksAmount);
		}

		public void TestLandingCharges()
		{
			AssertChargeCodeKey(new ChargeCodeChargeKey(CustomsChargeTypeList.Codes.LandingCharges, false, false), ChargeCodeKey.LandingCharges);
		}

		public void TestOtherCharges()
		{
			AssertChargeCodeKey(new ChargeCodeChargeKey(CustomsChargeTypeList.Codes.OtherCharges, true, true), ChargeCodeKey.DutyGSTOtherCharges);
			AssertChargeCodeKey(new ChargeCodeChargeKey(CustomsChargeTypeList.Codes.OtherCharges, false, true), ChargeCodeKey.NonDutyGSTOtherCharges);
			AssertChargeCodeKey(new ChargeCodeChargeKey(CustomsChargeTypeList.Codes.OtherCharges, false, false), ChargeCodeKey.NonDutyNonGSTOtherCharges);
		}

		public void TestAdditionCharge()
		{
			AssertChargeCodeKey(new ChargeCodeChargeKey(CustomsChargeTypeList.Codes.AdditionCharge, true, true), ChargeCodeKey.DutyGSTAdditionCharge);
		}

		public void TestDeductionCharge()
		{
			AssertChargeCodeKey(new ChargeCodeChargeKey(CustomsChargeTypeList.Codes.DeductionCharge, false, true), ChargeCodeKey.NonDutyGSTDeductionCharge);
			AssertChargeCodeKey(new ChargeCodeChargeKey(CustomsChargeTypeList.Codes.DeductionCharge, false, false), ChargeCodeKey.NonDutyNonGSTDeductionCharge);
		}

		public void TestForeignInlandFreight()
		{
			AssertChargeCodeKey(new ChargeCodeChargeKey(CustomsChargeTypeList.Codes.ForeignInlandFreight, true, true), ChargeCodeKey.DutyGSTForeignInlandFreight);
			AssertChargeCodeKey(new ChargeCodeChargeKey(CustomsChargeTypeList.Codes.ForeignInlandFreight, false, true), ChargeCodeKey.NonDutyGSTFIFT);
		}

		public void TestPackingCost()
		{
			AssertChargeCodeKey(new ChargeCodeChargeKey(CustomsChargeTypeList.Codes.PackingCost, true, true), ChargeCodeKey.PackingCost);
		}

		void AssertChargeCodeKey(ChargeCodeChargeKey expectedCharge, string keyForChargeCodeKey)
		{
			ChargeCodeChargeKey result = chargeCodeKey[keyForChargeCodeKey];
			AssertEquals("Charge type", expectedCharge.ChargeCode, result.ChargeCode);
			AssertEquals("Is Dutiable", expectedCharge.IsDutiable, result.IsDutiable);
			AssertEquals("Is GST", expectedCharge.IsVATible, result.IsVATible);
		}

		ChargeCodeKey chargeCodeKey;
		protected override void SetUp()
		{
			base.SetUp();
			chargeCodeKey = new ChargeCodeKey();
		}
	}
}
