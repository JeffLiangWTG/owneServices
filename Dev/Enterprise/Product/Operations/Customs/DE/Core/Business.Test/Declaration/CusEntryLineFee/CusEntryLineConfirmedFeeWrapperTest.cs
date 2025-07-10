using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.DE.Business.Declaration.Testing
{
	[TestedType(typeof(CusEntryLineConfirmedFeeWrapper))]
	sealed class CusEntryLineConfirmedFeeWrapperTest : NonPersistentBusinessObjectTestCase
	{
		public void TestCreate()
		{
			var cusEntryLineFeeWrapper = new CusEntryLineConfirmedFeeWrapper(cusEntryLineFee, "MethodOfCalculationOverwritten", "MethodOfCalculationDE", "QuickViewCardText");
			CombineAssertions(() =>
			{
				AssertEquals("PK", cusEntryLineFee.PK, cusEntryLineFeeWrapper.PK);
				AssertEquals("CF_ChargeType", "A00", cusEntryLineFeeWrapper.CF_ChargeType);
				AssertEquals("ChargeTypeDescription", cusEntryLineFee.ChargeTypeDescription, cusEntryLineFeeWrapper.ChargeTypeDescription);
				AssertEquals("CF_RateOverrideReasonCode", "ADD", cusEntryLineFeeWrapper.CF_RateOverrideReasonCode);
				AssertEquals("CF_BaseValue", new ZDecimal(10.1), cusEntryLineFeeWrapper.CF_BaseValue);
				AssertEquals("CF_MethodOfCalculation", "MethodOfCalculationOverwritten", cusEntryLineFeeWrapper.CF_MethodOfCalculation);
				AssertEquals("CF_Rate", new ZDecimal(20.2), cusEntryLineFeeWrapper.CF_Rate);
				AssertEquals("CF_ChargeAmount", new ZDecimal(30.3), cusEntryLineFeeWrapper.CF_ChargeAmount);
				AssertEquals("CF_MethodOfPayment", "ABC", cusEntryLineFeeWrapper.CF_MethodOfPayment);
				AssertEquals("NationalFeeTypeCode", "U167", cusEntryLineFeeWrapper.NationalFeeTypeCode);
				AssertEquals("QuickViewCard", "QuickViewCardText", cusEntryLineFeeWrapper.QuickViewCard);
				AssertEquals("MethodOfCalculationDE", "MethodOfCalculationDE", cusEntryLineFeeWrapper.MethodOfCalculationDE);
				AssertEquals("HumanReadableName", "MethodOfCalculationOverwritten:", cusEntryLineFeeWrapper.HumanReadableName);
			});
		}

		protected override BusinessObject GetNewBusinessObject() => new CusEntryLineConfirmedFeeWrapper(cusEntryLineFee, string.Empty, string.Empty, string.Empty);

		protected override void SetUp()
		{
			base.SetUp();
			cusEntryLineFee = Factory.New<EU.Business.Declaration.CusEntryLineFee>();
			cusEntryLineFee.CF_ChargeType = "A00";
			cusEntryLineFee.CF_RateOverrideReasonCode = "ADD";
			cusEntryLineFee.CF_BaseValue = 10.1;
			cusEntryLineFee.CF_MethodOfCalculation = "01";
			cusEntryLineFee.CF_Rate = 20.2;
			cusEntryLineFee.CF_ChargeAmount = 30.3;
			cusEntryLineFee.CF_MethodOfPayment = "ABC";
			cusEntryLineFee.NationalFeeTypeCode = "U167";
		}
		EU.Business.Declaration.CusEntryLineFee cusEntryLineFee;
	}
}
