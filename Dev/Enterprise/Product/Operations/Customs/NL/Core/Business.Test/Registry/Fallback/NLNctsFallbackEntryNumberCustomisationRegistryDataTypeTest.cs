using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.NL.Business.Testing;

[TestedType(typeof(NLNctsFallbackEntryNumberCustomisationRegistryDataType))]
sealed class NLNctsFallbackEntryNumberCustomisationRegistryDataTypeTest : NonPersistentBusinessObjectRegistryDataTypeTestCase<NLNctsFallbackEntryNumberCustomisationRegistryDataType>
{
	protected override string ExpectedEditorName => "BillOfLadingNumberCustomisationRegistryItemEditor";

	protected override NLNctsFallbackEntryNumberCustomisationRegistryDataType GetNewDataType()
	{
		return new NLNctsFallbackEntryNumberCustomisationRegistryDataType();
	}

	protected override ValidSampleAndBinaryValueInDB[] GetValidSamples()
	{
		var result1 = new NLNctsFallbackEntryNumberCustomisation();
		result1.CheckDigitAlgorithm = "NON";
		var xmlText1 = @"<?xml version=""1.0"" encoding=""utf-16""?><NLNctsFallbackEntryNumberCustomisation><ServiceLevel /><RemoveFountainPrefix>Y</RemoveFountainPrefix><AutoAllocateMasterBillNumbersToConsols>N</AutoAllocateMasterBillNumbersToConsols><CheckDigitAlgorithm>NON</CheckDigitAlgorithm><UseShipmentSequenceNumber>N</UseShipmentSequenceNumber><Elements><Element key=""MonthAs2Digits""><Order>2</Order><CheckDigit>N</CheckDigit><Detail>2</Detail></Element><Element key=""SequenceNumber""><Order>3</Order><CheckDigit>N</CheckDigit><Detail>6</Detail></Element><Element key=""YearAsDigit""><Order>1</Order><CheckDigit>Y</CheckDigit><Detail>2</Detail></Element></Elements></NLNctsFallbackEntryNumberCustomisation>";

		var result2 = new NLNctsFallbackEntryNumberCustomisation();
		result2.CheckDigitAlgorithm = "R31";
		var xmlText2 = @"<?xml version=""1.0"" encoding=""utf-16""?><NLNctsFallbackEntryNumberCustomisation><ServiceLevel /><RemoveFountainPrefix>Y</RemoveFountainPrefix><AutoAllocateMasterBillNumbersToConsols>N</AutoAllocateMasterBillNumbersToConsols><CheckDigitAlgorithm>R31</CheckDigitAlgorithm><UseShipmentSequenceNumber>N</UseShipmentSequenceNumber><Elements><Element key=""MonthAs2Digits""><Order>2</Order><CheckDigit>N</CheckDigit><Detail>2</Detail></Element><Element key=""SequenceNumber""><Order>3</Order><CheckDigit>N</CheckDigit><Detail>6</Detail></Element><Element key=""YearAsDigit""><Order>1</Order><CheckDigit>Y</CheckDigit><Detail>2</Detail></Element></Elements></NLNctsFallbackEntryNumberCustomisation>";

		return new ValidSampleAndBinaryValueInDB[]
		{
			new ValidSampleAndBinaryValueInDB(result1, xmlText1),
			new ValidSampleAndBinaryValueInDB(result2, xmlText2)
		};
	}
}
