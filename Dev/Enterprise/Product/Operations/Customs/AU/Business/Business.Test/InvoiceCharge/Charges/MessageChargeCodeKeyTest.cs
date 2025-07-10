using Enterprise.Customs.Common;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class MessageChargeCodeKeyTest : TestCase
	{
		public void TestBuyingCommission()
		{
			AssertMessgeCodeKey(new MessageChargeKey(AUChargeCodeList.Codes.BuyingCommission, true, true, false), MessageChargeCodeKey.DutiableBuyingCommissionExcluded);
			AssertMessgeCodeKey(new MessageChargeKey(AUChargeCodeList.Codes.BuyingCommission, false, false, true), MessageChargeCodeKey.NonDutiableBuyingCommissionIncluded);
		}

		public void TestOtherCommission()
		{
			AssertMessgeCodeKey(new MessageChargeKey(AUChargeCodeList.Codes.OtherCommission, true, true, false), MessageChargeCodeKey.DutiableOtherCommissionExcluded);
			AssertMessgeCodeKey(new MessageChargeKey(AUChargeCodeList.Codes.OtherCommission, false, false, true), MessageChargeCodeKey.NonDutiableOtherCommissionIncluded);
		}

		public void TestCommission()
		{
			AssertMessgeCodeKey(new MessageChargeKey(CustomsChargeTypeList.Codes.Commission, true, true, false), MessageChargeCodeKey.DutiableCommissionExcluded);
			AssertMessgeCodeKey(new MessageChargeKey(CustomsChargeTypeList.Codes.Commission, false, false, true), MessageChargeCodeKey.NonDutiableCommissionIncluded);
		}

		public void TestExWorksAmount()
		{
			AssertMessgeCodeKey(new MessageChargeKey(CustomsChargeTypeList.Codes.ExWorks, true, true, false), MessageChargeCodeKey.DutyGSTExWorksAmountExcluded);
			AssertMessgeCodeKey(new MessageChargeKey(CustomsChargeTypeList.Codes.ExWorks, false, true, true), MessageChargeCodeKey.NonDutyGSTExWorksAmountIncluded);
		}

		public void TestLandingCharges()
		{
			AssertMessgeCodeKey(new MessageChargeKey(CustomsChargeTypeList.Codes.LandingCharges, false, false, true), MessageChargeCodeKey.LandingChargesIncluded);
		}

		public void TestOtherCharges()
		{
			AssertMessgeCodeKey(new MessageChargeKey(CustomsChargeTypeList.Codes.OtherCharges, true, true, false), MessageChargeCodeKey.DutyGSTOtherChargesExcluded);
			AssertMessgeCodeKey(new MessageChargeKey(CustomsChargeTypeList.Codes.OtherCharges, false, true, true), MessageChargeCodeKey.NonDutyGSTOtherChargesIncluded);
			AssertMessgeCodeKey(new MessageChargeKey(CustomsChargeTypeList.Codes.OtherCharges, false, false, true), MessageChargeCodeKey.NonDutyNonGSTOtherChargesIncluded);
		}

		public void TestAdditionCharge()
		{
			AssertMessgeCodeKey(new MessageChargeKey(CustomsChargeTypeList.Codes.AdditionCharge, true, true, false), MessageChargeCodeKey.DutyGSTAdditionChargeExcluded);
		}

		public void TestDeductionCharge()
		{
			AssertMessgeCodeKey(new MessageChargeKey(CustomsChargeTypeList.Codes.DeductionCharge, false, true, true), MessageChargeCodeKey.NonDutyGSTDeductionChargeIncluded);
			AssertMessgeCodeKey(new MessageChargeKey(CustomsChargeTypeList.Codes.DeductionCharge, false, false, true), MessageChargeCodeKey.NonDutyNonGSTDeductionChargeIncluded);
		}

		public void TestForeignInlandFreight()
		{
			AssertMessgeCodeKey(new MessageChargeKey(CustomsChargeTypeList.Codes.ForeignInlandFreight, true, true, false), MessageChargeCodeKey.DutyGSTForeignInlandFreightExcluded);
			AssertMessgeCodeKey(new MessageChargeKey(CustomsChargeTypeList.Codes.ForeignInlandFreight, false, true, true), MessageChargeCodeKey.NonDutyGSTForeignInlandFreightIncluded);
		}

		public void TestPackingCost()
		{
			AssertMessgeCodeKey(new MessageChargeKey(CustomsChargeTypeList.Codes.PackingCost, true, true, false), MessageChargeCodeKey.PackingCostExcluded);
		}

		public void TestDiscount()
		{
			AssertMessgeCodeKey(new MessageChargeKey(CustomsChargeTypeList.Codes.Discount, false, false, false), MessageChargeCodeKey.NonDutyNonGSTDiscountExcluded);
		}

		void AssertMessgeCodeKey(MessageChargeKey expectedCharge, string keyForMessageChargeCodeKey)
		{
			MessageChargeKey result = messageChargeKey[keyForMessageChargeCodeKey];
			AssertEquals("Charge type", expectedCharge.ChargeCode, result.ChargeCode);
			AssertEquals("Is Dutiable", expectedCharge.IsDutiable, result.IsDutiable);
			AssertEquals("Is GST", expectedCharge.IsVATible, result.IsVATible);
			AssertEquals("Is Included in iTOT", expectedCharge.IsIncludedInITOT, result.IsIncludedInITOT);
		}

		MessageChargeCodeKey messageChargeKey;
		protected override void SetUp()
		{
			base.SetUp();
			messageChargeKey = new MessageChargeCodeKey();
		}
	}
}
