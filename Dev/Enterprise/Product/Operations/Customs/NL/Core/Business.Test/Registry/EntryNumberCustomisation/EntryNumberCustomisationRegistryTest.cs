using System.Linq;
using CargoWise.Types;
using Enterprise.Registry.Business;
using Enterprise.Registry.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.NL.Business.Testing;

[TestedType(typeof(NLCustomsRegistry))]
class EntryNumberCustomisationRegistryTest : RegistryItemSetTestCaseWithFactory<NLCustomsRegistry>
{
	public void TestDefaultValues()
	{
		var dataType = (BillCustomisationRegistryDataType)ItemSet.EntryNumberCustomisation.DataType;
		AssertEquals("GeneratedNumberName", "Entry Number Customization", dataType.GeneratedNumberName);

		AssertEquals("MaxLength", NLCustomsRegistry.EntryReferenceNumberMaxLength, dataType.MaxLength);
		AssertEquals("NON", dataType.DefaultValue.CheckDigitAlgorithm);
		AssertEquals("2", dataType.DefaultValue.UnFilteredElements[BillOfLadingNumberCustomisationElement.Keys.YearAsDigit].Detail);
		AssertEquals(new ZByte(1), dataType.DefaultValue.UnFilteredElements[BillOfLadingNumberCustomisationElement.Keys.YearAsDigit].Order);
		Assert(dataType.DefaultValue.UnFilteredElements[BillOfLadingNumberCustomisationElement.Keys.YearAsDigit].Include);
		AssertEquals("8", dataType.DefaultValue.UnFilteredElements[BillOfLadingNumberCustomisationElement.Keys.SequenceNumber].Detail);
		AssertEquals(new ZByte(50), dataType.DefaultValue.UnFilteredElements[BillOfLadingNumberCustomisationElement.Keys.SequenceNumber].Order);
		Assert(dataType.DefaultValue.UnFilteredElements[BillOfLadingNumberCustomisationElement.Keys.SequenceNumber].Include);
		AssertEquals(2, (dataType.DefaultValue.UnFilteredElements).OfType<BillOfLadingNumberCustomisationElement>().Count(x => x.Include));
	}
}
