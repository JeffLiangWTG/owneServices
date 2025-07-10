using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.NL.Business.Testing;

[TestedType(typeof(EntryNumberCustomisationRegistryDataType))]
class EntryNumberCustomisationRegistryDataTypeTest : NonPersistentBusinessObjectRegistryDataTypeTestCase<EntryNumberCustomisationRegistryDataType>
{
	protected override string ExpectedEditorName => "BillOfLadingNumberCustomisationRegistryItemEditor";

	protected override EntryNumberCustomisationRegistryDataType GetNewDataType()
	{
		return new EntryNumberCustomisationRegistryDataType();
	}

	protected override ValidSampleAndBinaryValueInDB[] GetValidSamples()
	{
		var result1 = new EntryNumberCustomisation();
		result1.CheckDigitAlgorithm = "NON";
		var xmlText1 = @"<?xml version=""1.0"" encoding=""utf-16""?><EntryNumberCustomisation><ServiceLevel /><RemoveFountainPrefix>Y</RemoveFountainPrefix><CheckDigitAlgorithm>NON</CheckDigitAlgorithm><UseShipmentSequenceNumber>N</UseShipmentSequenceNumber><Elements><Element key=""SequenceNumber""><Order>50</Order><CheckDigit>Y</CheckDigit><Detail>8</Detail></Element><Element key=""YearAsDigit""><Order>1</Order><CheckDigit>Y</CheckDigit><Detail>2</Detail></Element></Elements></EntryNumberCustomisation>";

		var result2 = new EntryNumberCustomisation();
		result2.CheckDigitAlgorithm = "R31";
		var xmlText2 = @"<?xml version=""1.0"" encoding=""utf-16""?><EntryNumberCustomisation><ServiceLevel /><RemoveFountainPrefix>Y</RemoveFountainPrefix><CheckDigitAlgorithm>R31</CheckDigitAlgorithm><UseShipmentSequenceNumber>N</UseShipmentSequenceNumber><Elements><Element key=""SequenceNumber""><Order>50</Order><CheckDigit>Y</CheckDigit><Detail>8</Detail></Element><Element key=""YearAsDigit""><Order>1</Order><CheckDigit>Y</CheckDigit><Detail>2</Detail></Element></Elements></EntryNumberCustomisation>";

		return new ValidSampleAndBinaryValueInDB[]
		{
			new ValidSampleAndBinaryValueInDB(result1, xmlText1),
			new ValidSampleAndBinaryValueInDB(result2, xmlText2)
		};
	}
}
