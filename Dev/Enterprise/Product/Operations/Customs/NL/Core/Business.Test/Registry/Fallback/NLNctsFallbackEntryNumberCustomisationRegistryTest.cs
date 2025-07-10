using CargoWise.Types;
using Enterprise.Registry.Business;
using Enterprise.Registry.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.NL.Business.Testing;

[TestedType(typeof(NLCustomsRegistry))]
sealed class NLNctsFallbackEntryNumberCustomisationRegistryTest : RegistryItemSetTestCaseWithFactory<NLCustomsRegistry>
{
	public void TestDefaultValues()
	{
		var dataType = (BillCustomisationRegistryDataType)ItemSet.NLNctsFallbackEntryNumberCustomisation.DataType;
		AssertEquals("GeneratedNumberName", "DVA Departure emergency procedure sequence numbers", dataType.GeneratedNumberName);

		AssertEquals("MaxLength", 10, dataType.MaxLength);
		AssertEquals("NON", dataType.DefaultValue.CheckDigitAlgorithm);

		CombineAssertions(() =>
		{
			AssertEquals(new ZByte(1), dataType.DefaultValue.UnFilteredElements[BillOfLadingNumberCustomisationElement.Keys.YearAsDigit].Order);
			Assert(dataType.DefaultValue.UnFilteredElements[BillOfLadingNumberCustomisationElement.Keys.YearAsDigit].Include);
			AssertEquals("2", dataType.DefaultValue.UnFilteredElements[BillOfLadingNumberCustomisationElement.Keys.YearAsDigit].Detail);
			Assert(dataType.DefaultValue.UnFilteredElements[BillOfLadingNumberCustomisationElement.Keys.YearAsDigit].ReadOnly);

			AssertEquals(new ZByte(3), dataType.DefaultValue.UnFilteredElements[BillOfLadingNumberCustomisationElement.Keys.SequenceNumber].Order);
			Assert(dataType.DefaultValue.UnFilteredElements[BillOfLadingNumberCustomisationElement.Keys.SequenceNumber].Include);
			AssertEquals("6", dataType.DefaultValue.UnFilteredElements[BillOfLadingNumberCustomisationElement.Keys.SequenceNumber].Detail);
			Assert(!dataType.DefaultValue.UnFilteredElements[BillOfLadingNumberCustomisationElement.Keys.SequenceNumber].CheckDigit);

			AssertEquals(new ZByte(2), dataType.DefaultValue.UnFilteredElements[BillOfLadingNumberCustomisationElement.Keys.MonthAs2Digits].Order);
			Assert(dataType.DefaultValue.UnFilteredElements[BillOfLadingNumberCustomisationElement.Keys.MonthAs2Digits].Include);
			AssertEquals("2", dataType.DefaultValue.UnFilteredElements[BillOfLadingNumberCustomisationElement.Keys.MonthAs2Digits].Detail);
			Assert(!dataType.DefaultValue.UnFilteredElements[BillOfLadingNumberCustomisationElement.Keys.MonthAs2Digits].CheckDigit);

			Assert(!dataType.DefaultValue.UnFilteredElements[BillOfLadingNumberCustomisationElement.Keys.TransportMode].CheckDigit);

			Assert(!dataType.DefaultValue.UnFilteredElements[BillOfLadingNumberCustomisationElement.Keys.Direction].CheckDigit);
		});
	}
}
