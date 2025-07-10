using CargoWise.EntityFramework.Testing;
using NUnit.Framework;
using WTG.NUnit;

namespace Enterprise.Customs.CA.Services.Testing
{
	sealed class ValidateTransactionExtensionsTest : TestCaseWithFactory
	{
		[ExpectNoExceptions]
		public void TestSerialize()
		{
			#region Expect XML

			string expectXml = @"<?xml version=""1.0"" encoding=""utf-8""?>
<ValidateTransaction>
  <CommodityGroup commodityGroupId=""A1"">
    <Commodity commodityId=""GGL023--0001"">
      <HSNumber>020500</HSNumber>
      <RequirementId>21247</RequirementId>
      <RequirementVersion>14</RequirementVersion>
      <AirsCode>512700</AirsCode>
      <OriginCountry>US</OriginCountry>
      <OriginState>TN</OriginState>
      <Enduse>69</Enduse>
      <Miscellaneous>34</Miscellaneous>
      <Registration registrationId=""14"" />
      <Registration registrationId=""28"" />
      <Registration registrationId=""40"" />
      <Registration registrationId=""41"" />
    </Commodity>
  </CommodityGroup>
  <CommodityGroup commodityGroupId=""A2"">
    <Commodity commodityId=""GHL030--0001"">
      <HSNumber>960000</HSNumber>
    </Commodity>
    <Commodity commodityId=""GHL030--0002"">
      <HSNumber>920000</HSNumber>
    </Commodity>
  </CommodityGroup>
</ValidateTransaction>";

			#endregion

			var data = new ValidateTransaction();
			var commodityGroup = new ValidateTransactionCommodityGroup() { commodityGroupId = "A1" };
			var commodity = new ValidateTransactionCommodityGroupCommodity()
			{
				commodityId = "GGL023--0001",
				HSNumber = "020500",
				RequirementId = "21247",
				RequirementVersion = "14",
				AirsCode = "512700",
				OriginCountry = "US",
				OriginState = "TN",
				Enduse = "69",
				Miscellaneous = "34"
			};
			commodity.Registration.AddNew().registrationId = "14";
			commodity.Registration.AddNew().registrationId = "28";
			commodity.Registration.AddNew().registrationId = "40";
			commodity.Registration.AddNew().registrationId = "41";
			commodityGroup.Commodity.Add(commodity);
			data.CommodityGroup.Add(commodityGroup);

			commodityGroup = new ValidateTransactionCommodityGroup() { commodityGroupId = "A2" };
			commodity = new ValidateTransactionCommodityGroupCommodity()
			{
				commodityId = "GHL030--0001",
				HSNumber = "960000"
			};
			commodityGroup.Commodity.Add(commodity);
			commodity = new ValidateTransactionCommodityGroupCommodity()
			{
				commodityId = "GHL030--0002",
				HSNumber = "920000"
			};
			commodityGroup.Commodity.Add(commodity);
			data.CommodityGroup.Add(commodityGroup);

			var xml = new string(System.Text.Encoding.UTF8.GetChars(data.Serialize()));
			NUnit.Framework.Assert.That(xml, NUnit.Framework.Is.EqualTo(expectXml));
		}

		[ExpectNoExceptions]
		public void TestDeserialize()
		{
			string xml = @"<ValidateTransactionResult errorCount=""1"">
	<CommodityGroup commodityGroupId=""A1"" errorCount=""1"">
		<Commodity commodityId=""GGL023--0001"">
			<Error>MISSING OR INVALID REGISTRATION NUMBER</Error>
		</Commodity>
	</CommodityGroup>
	<CommodityGroup commodityGroupId=""A2"" errorCount=""0"" />
</ValidateTransactionResult>";

			var data = System.Text.Encoding.UTF8.GetBytes(xml);
			var result = data.Deserialize<ValidateTransactionResult>();

			NUnit.Framework.Assert.That(result.errorCount, NUnit.Framework.Is.EqualTo("1").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(result.CommodityGroup[0].commodityGroupId, NUnit.Framework.Is.EqualTo("A1").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(result.CommodityGroup[0].errorCount, NUnit.Framework.Is.EqualTo("1").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(result.CommodityGroup[0].Commodity[0].commodityId, NUnit.Framework.Is.EqualTo("GGL023--0001").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(result.CommodityGroup[0].Commodity[0].Error, NUnit.Framework.Is.EqualTo("MISSING OR INVALID REGISTRATION NUMBER").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(result.CommodityGroup[1].commodityGroupId, NUnit.Framework.Is.EqualTo("A2").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(result.CommodityGroup[1].errorCount, NUnit.Framework.Is.EqualTo("0").Using(CustomComparers.TypeComparison));
		}
	}
}
