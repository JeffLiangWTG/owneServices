using System.Text;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.Registry.Business.Testing
{
	[TestedType(typeof(ChargeCodesWithTypeRegistryDataType))]
	class ChargeCodesWithTypeRegistryDataTypeTest : NonPersistentBusinessObjectRegistryDataTypeTestCase<ChargeCodesWithTypeRegistryDataType>
	{
		public void TestDeserialize_ShouldAddDefaultPartyTypesIfMissing()
		{
			var dataType = new ChargeCodesWithTypeRegistryDataType();
			var partyTypes = new OrgProfitSharePartyLookups().PartyTypes;

			var partyTypePic = (CodeDescriptionPair)partyTypes["PIC"];
			var partyTypeDly = (CodeDescriptionPair)partyTypes["DLY"];
			Assert("Precondition: Party types should contain PIC and DLY", partyTypePic != null && partyTypeDly != null);

			var result = dataType.Deserialise(Encoding.Unicode.GetBytes(RegistryValueWithoutPicAndDly.Trim()));

			Assert(
				"PIC & DLY party types should be added to the collection",
				result[partyTypePic.MultilingualDescription] != null &&
				result[partyTypeDly.MultilingualDescription] != null);
		}

		const string RegistryValueWithoutPicAndDly = @"
<?xml version=""1.0"" encoding=""utf-16""?>
<ArrayOfChargeCodeWithType xmlns:xsd=""http://www.w3.org/2001/XMLSchema""
	xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"">
	<ChargeCodeWithType>
		<PartyType>Sending Agent</PartyType>
		<UseDefaultProfitShareChargeCode>Y</UseDefaultProfitShareChargeCode>
		<ChargeCode>00000000-0000-0000-0000-000000000000</ChargeCode>
	</ChargeCodeWithType>
	<ChargeCodeWithType>
		<PartyType>Receiving Agent</PartyType>
		<UseDefaultProfitShareChargeCode>Y</UseDefaultProfitShareChargeCode>
		<ChargeCode>00000000-0000-0000-0000-000000000000</ChargeCode>
	</ChargeCodeWithType>
	<ChargeCodeWithType>
		<PartyType>Controlling Agent</PartyType>
		<UseDefaultProfitShareChargeCode>Y</UseDefaultProfitShareChargeCode>
		<ChargeCode>00000000-0000-0000-0000-000000000000</ChargeCode>
	</ChargeCodeWithType>
	<ChargeCodeWithType>
		<PartyType>Head Office</PartyType>
		<UseDefaultProfitShareChargeCode>Y</UseDefaultProfitShareChargeCode>
		<ChargeCode>00000000-0000-0000-0000-000000000000</ChargeCode>
	</ChargeCodeWithType>
	<ChargeCodeWithType>
		<PartyType>G/W Agent (G/W Consol)</PartyType>
		<UseDefaultProfitShareChargeCode>Y</UseDefaultProfitShareChargeCode>
		<ChargeCode>00000000-0000-0000-0000-000000000000</ChargeCode>
	</ChargeCodeWithType>
	<ChargeCodeWithType>
		<PartyType>Lead G/W Agent</PartyType>
		<UseDefaultProfitShareChargeCode>N</UseDefaultProfitShareChargeCode>
		<ChargeCode>bbcbba48-f3a1-4915-ba78-fc604d8ca284</ChargeCode>
	</ChargeCodeWithType>
</ArrayOfChargeCodeWithType>";

		#region Implementation

		protected override ChargeCodesWithTypeRegistryDataType GetNewDataType() => new();

		protected override string ExpectedEditorName => "ChargeCodesWithTypeRegistryItemEditor";

		protected override ValidSampleAndBinaryValueInDB[] GetValidSamples()
		{
			ChargeCodeWithTypeCollection collection = ChargeCodeWithTypeCollection.GetDefaultCollection();
			byte[] byteArrayValue = Encoding.Unicode.GetBytes(ValidSampleXmlValue.Trim());
			return [new ValidSampleAndBinaryValueInDB(collection, byteArrayValue)];
		}

		const string ValidSampleXmlValue = @"
<?xml version=""1.0"" encoding=""utf-16""?>
<ArrayOfChargeCodeWithType xmlns:xsd=""http://www.w3.org/2001/XMLSchema""
	xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"">
	<ChargeCodeWithType>
		<PartyType>Sending Agent</PartyType>
		<UseDefaultProfitShareChargeCode>Y</UseDefaultProfitShareChargeCode>
		<ChargeCode>00000000-0000-0000-0000-000000000000</ChargeCode>
	</ChargeCodeWithType>
	<ChargeCodeWithType>
		<PartyType>Receiving Agent</PartyType>
		<UseDefaultProfitShareChargeCode>Y</UseDefaultProfitShareChargeCode>
		<ChargeCode>00000000-0000-0000-0000-000000000000</ChargeCode>
	</ChargeCodeWithType>
	<ChargeCodeWithType>
		<PartyType>Controlling Agent</PartyType>
		<UseDefaultProfitShareChargeCode>Y</UseDefaultProfitShareChargeCode>
		<ChargeCode>00000000-0000-0000-0000-000000000000</ChargeCode>
	</ChargeCodeWithType>
	<ChargeCodeWithType>
		<PartyType>Head Office</PartyType>
		<UseDefaultProfitShareChargeCode>Y</UseDefaultProfitShareChargeCode>
		<ChargeCode>00000000-0000-0000-0000-000000000000</ChargeCode>
	</ChargeCodeWithType>
	<ChargeCodeWithType>
		<PartyType>G/W Agent (G/W Consol)</PartyType>
		<UseDefaultProfitShareChargeCode>Y</UseDefaultProfitShareChargeCode>
		<ChargeCode>00000000-0000-0000-0000-000000000000</ChargeCode>
	</ChargeCodeWithType>
	<ChargeCodeWithType>
		<PartyType>Lead G/W Agent</PartyType>
		<UseDefaultProfitShareChargeCode>Y</UseDefaultProfitShareChargeCode>
		<ChargeCode>00000000-0000-0000-0000-000000000000</ChargeCode>
	</ChargeCodeWithType>
	<ChargeCodeWithType>
		<PartyType>Pickup Agent</PartyType>
		<UseDefaultProfitShareChargeCode>Y</UseDefaultProfitShareChargeCode>
		<ChargeCode>00000000-0000-0000-0000-000000000000</ChargeCode>
	</ChargeCodeWithType>
	<ChargeCodeWithType>
		<PartyType>Delivery Agent</PartyType>
		<UseDefaultProfitShareChargeCode>Y</UseDefaultProfitShareChargeCode>
		<ChargeCode>00000000-0000-0000-0000-000000000000</ChargeCode>
	</ChargeCodeWithType>
</ArrayOfChargeCodeWithType>";

		#endregion
	}
}
