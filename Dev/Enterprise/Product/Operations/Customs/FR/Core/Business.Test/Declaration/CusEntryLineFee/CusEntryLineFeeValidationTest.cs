using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.Business.Testing;

namespace Enterprise.Customs.FR.Business.Declaration.Testing
{
	public class CusEntryLineFeeValidationTest : EU.Business.Declaration.Testing.EUUniversalCusEntryLineFeeValidationTest<JobDeclaration, CusEntryLine, CusEntryLineFee>
	{
		public void TestCheckCF_ChargeAmount()
		{
			(var declaration, var entryLine, var entryLineFee) = SetEntryLineFeeData();
			entryLineFee.CF_RateOverrideReasonCode = "ADD";
			entryLineFee.CF_ChargeAmount = ZDecimal.Zero;
			entryLineFee.Validation.ValidateCF_ChargeAmount();
			AssertHasMessageError(entryLineFee.CF_ChargeAmountInfo, "Charge Amount cannot be zero.");

			entryLineFee.CF_ChargeAmount = 1m;
			entryLineFee.Validation.ValidateCF_ChargeAmount();
			AssertNoMessageError(entryLineFee.CF_ChargeAmountInfo, "Charge Amount cannot be zero.");

			entryLineFee.CF_RateOverrideReasonCode = "PRE";
			entryLineFee.Validation.ValidateCF_ChargeAmount();
			AssertNoMessageError(entryLineFee.CF_ChargeAmountInfo, "Charge Amount cannot be zero.");

			entryLineFee.CF_ChargeAmount = ZDecimal.Zero;
			entryLineFee.Validation.ValidateCF_ChargeAmount();
			AssertHasMessageError(entryLineFee.CF_ChargeAmountInfo, "Charge Amount cannot be zero.");
		}

		public void TestDecimalPlacesAllowedForBaseValuePrecision()
		{
			var expectedMessageError = "Base Amount allows only 2 decimal places.";
			var (declaration, entryLine, entryLineFee) = SetEntryLineFeeData();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;

			CombineAssertions("For UCC6 declarations, when CF_MethodOfCalculation equals %, Base Amount should allow 6 decimal places, else only 2 decimal places are allowed.", () =>
			{
				using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, true))
				{
					entryLineFee.CF_MethodOfCalculation = ZString.Empty;
					entryLineFee.CF_BaseValue = 5.123456;
					AssertNoMessageError(entryLineFee.CF_BaseValueInfo, expectedMessageError);

					entryLineFee.CF_BaseValue = 10.23;
					AssertNoMessageError(entryLineFee.CF_BaseValueInfo, expectedMessageError);

					entryLineFee.CF_MethodOfCalculation = Customs.Business.UniversalReferenceConstants.MethodOfCalculation.Percentage;
					entryLineFee.Validation.ValidateCF_BaseValue();
					AssertNoMessageError(entryLineFee.CF_BaseValueInfo, expectedMessageError);

					entryLineFee.CF_BaseValue = 5.123456;
					AssertHasMessageError(entryLineFee.CF_BaseValueInfo, expectedMessageError);
				}
			});

			CombineAssertions("For non UCC6 declarations, Base amount should allow only 2 decimal places.", () =>
			{
				using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, false))
				{
					entryLineFee.CF_MethodOfCalculation = ZString.Empty;
					entryLineFee.CF_BaseValue = 5.1234567;
					AssertHasMessageErrorContaining(entryLineFee.CF_BaseValueInfo, expectedMessageError);

					entryLineFee.CF_BaseValue = 10.23;
					AssertNoMessageError(entryLineFee.CF_BaseValueInfo, expectedMessageError);

					entryLineFee.CF_MethodOfCalculation = Customs.Business.UniversalReferenceConstants.MethodOfCalculation.Percentage;
					entryLineFee.Validation.ValidateCF_BaseValue();
					AssertNoMessageError(entryLineFee.CF_BaseValueInfo, expectedMessageError);

					entryLineFee.CF_BaseValue = 5.123456;
					AssertHasMessageError(entryLineFee.CF_BaseValueInfo, expectedMessageError);
				}
			});
		}
	}
}
