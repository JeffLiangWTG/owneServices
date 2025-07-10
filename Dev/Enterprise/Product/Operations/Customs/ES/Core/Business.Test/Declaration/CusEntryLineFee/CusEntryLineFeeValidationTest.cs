using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.Business.Testing;
using static Enterprise.Customs.ES.Business.ESConstants;

namespace Enterprise.Customs.ES.Business.Declaration.Testing;

public class CusEntryLineFeeValidationTest : EU.Business.Declaration.Testing.EUUniversalCusEntryLineFeeValidationTest<JobDeclaration, CusEntryLine, CusEntryLineFee>
{
	public void TestBaseValueDecimalPlaces_Export()
	{
		var expectedMessageError = "Base Amount allows only 2 decimal places.";
		using (ConfigurationTestHelper.TemporarilySetUseUniversalFeeCalculationConfiguration(Factory, true))
		{
			var (declaration, _, entryLineFee) = SetEntryLineFeeData();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			entryLineFee.CF_BaseValue = 5.1234567;
			AssertHasMessageErrorContaining("Decimal places message error for wrong base value with UseUniversalFeeCalculation true", entryLineFee.CF_BaseValueInfo, expectedMessageError);
		}
	}

	public void TestBaseValueDecimalPlaces_Import_MethodOfCalculationPercentage()
	{
		var expectedMessageError = "Base Amount allows only 2 decimal places.";
		using (ConfigurationTestHelper.TemporarilySetUseUniversalFeeCalculationConfiguration(Factory, true))
		{
			var (declaration, _, entryLineFee) = SetEntryLineFeeData();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			entryLineFee.CF_MethodOfCalculation = Customs.Business.UniversalReferenceConstants.MethodOfCalculation.Percentage;
			entryLineFee.CF_BaseValue = 5.1234567;
			AssertHasMessageErrorContaining("Decimal places message error for wrong base value with UseUniversalFeeCalculation true", entryLineFee.CF_BaseValueInfo, expectedMessageError);
		}
	}

	public void TestBaseValueDecimalPlaces_Import_MethodOfCalculationNotPercentage()
	{
		var expectedCommonMessageError = "Base Amount allows only";
		var expectedSpecificMessageError = "Base Amount allows only 3 decimal places.";
		using (ConfigurationTestHelper.TemporarilySetUseUniversalFeeCalculationConfiguration(Factory, true))
		{
			CombineAssertions(() =>
			{
				var (declaration, entryLine, entryLineFee) = SetEntryLineFeeData();
				declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
				entryLineFee.CF_MethodOfCalculation = ZString.Empty;
				entryLine.Header.ZG_UCC6Version = UCC6VersionCodes.UCC6;
				entryLineFee.CF_BaseValue = 5.1234567;
				AssertNoMessageErrorContaining("No decimal places message error for wrong base value when UCC6 and Method of Calculation is empty with UseUniversalFeeCalculation true", entryLineFee.CF_BaseValueInfo, expectedCommonMessageError);

				entryLine.Header.ZG_UCC6Version = UCC6VersionCodes.NoUCC6;
				entryLineFee.CF_BaseValue = 5.9876543;
				AssertHasMessageErrorContaining("Decimal places message error for wrong base value when not UCC6 and Method of Calculation is empty with UseUniversalFeeCalculation true", entryLineFee.CF_BaseValueInfo, expectedCommonMessageError);
				AssertHasMessageErrorContaining("Decimal places message error for wrong base value when not UCC6 and Method of Calculation is empty with UseUniversalFeeCalculation true, with specific value", entryLineFee.CF_BaseValueInfo, expectedSpecificMessageError);

				entryLineFee.CF_MethodOfCalculation = "AAA";
				entryLine.Header.ZG_UCC6Version = UCC6VersionCodes.UCC6;
				entryLineFee.CF_BaseValue = 5.1234567;
				AssertNoMessageErrorContaining("No decimal places message error for wrong base value when UCC6 and Method of Calculation is not empty and not % with UseUniversalFeeCalculation true", entryLineFee.CF_BaseValueInfo, expectedCommonMessageError);

				entryLine.Header.ZG_UCC6Version = UCC6VersionCodes.NoUCC6;
				entryLineFee.CF_BaseValue = 5.9876543;
				AssertHasMessageErrorContaining("Decimal places message error for wrong base value when not UCC6 and Method of Calculation is not empty and not % with UseUniversalFeeCalculation true", entryLineFee.CF_BaseValueInfo, expectedCommonMessageError);
				AssertHasMessageErrorContaining("Decimal places message error for wrong base value when not UCC6 and Method of Calculation is not empty and not % with UseUniversalFeeCalculation true, with specific value", entryLineFee.CF_BaseValueInfo, expectedSpecificMessageError);
			});
		}
	}
}
