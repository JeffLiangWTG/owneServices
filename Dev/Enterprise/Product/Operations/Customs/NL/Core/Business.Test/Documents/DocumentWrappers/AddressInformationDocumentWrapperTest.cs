using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.NL.Business.Testing;

[TestedType(typeof(AddressInformationDocumentWrapper))]
sealed class AddressInformationDocumentWrapperTest : NonPersistentBusinessObjectTestCase
{
	AddressInformationDocumentWrapper agent;
	AddressInformationDocumentWrapper declarant;
	AddressInformationDocumentWrapper goodsLocation;
	AddressInformationDocumentWrapper goodsLocationAsJobDocAddress;

	protected override void SetUp()
	{
		base.SetUp();
		var entryHeader = DocumentWrapperTestHelper.GetEntryHeaderForTest(Factory);
		entryHeader.Declaration.GoodsLocation.CGL_Qualifier = "T";
		agent = new AddressInformationDocumentWrapper(entryHeader.Declaration.ControllingAgent);
		declarant = new AddressInformationDocumentWrapper(entryHeader.Declaration.Declarant.Header.CustomsAddress);
		goodsLocation = new AddressInformationDocumentWrapper(entryHeader.Declaration.GoodsLocation as CusGoodsLocation);
		goodsLocationAsJobDocAddress = new AddressInformationDocumentWrapper(entryHeader.Declaration.GoodsLocation.Address);
	}

	public void TestAddresses()
	{
		CombineAssertions(() =>
		{
			AssertEquals("Controlling Agent name", "Chris the Controlling Agent", agent.Name);
			AssertEquals("Agent Address", "CAstreet 12", agent.Address);
			AssertEquals("Agent City", "Rotterdam", agent.City);
			AssertEquals("Agent PostCode", "1079CK", agent.PostCode);
			AssertEquals("Agent CountryCode", "NL", agent.CountryCode);
			AssertEquals("Controlling Agent EORI", "123456789", agent.EORINumber);

			AssertEquals("Declarant name", "Delta the Declarant", declarant.Name);
			AssertEquals("Declarant Address", "Decstreet 12", declarant.Address);
			AssertEquals("Declarant City", "Brussel", declarant.City);
			AssertEquals("Declarant PostCode", "2010AB", declarant.PostCode);
			AssertEquals("Declarant CountryCode", "BE", declarant.CountryCode);
			AssertEquals("Declarant EORI", "987654321", declarant.EORINumber);

			AssertEquals("GoodsLocation name", "Janssen BV", goodsLocation.Name);
			AssertEquals("GoodsLocation Address", "25", goodsLocation.Address);
			AssertEquals("GoodsLocation City", "Amersfoort", goodsLocation.City);
			AssertEquals("GoodsLocation PostCode", "1062XD", goodsLocation.PostCode);
			AssertEquals("GoodsLocation CountryCode", "NL", goodsLocation.CountryCode);
			AssertEquals("GoodsLocation EORI", "NL194563729B01", goodsLocation.EORINumber);

			AssertEquals("GoodsLocation (as DocAddress) name", "Janssen BV", goodsLocationAsJobDocAddress.Name);
			AssertEquals("GoodsLocation (as DocAddress) Address", "Locationstreet 5 Department of Goods", goodsLocationAsJobDocAddress.Address);
			AssertEquals("GoodsLocation (as DocAddress) City", "Amersfoort", goodsLocationAsJobDocAddress.City);
			AssertEquals("GoodsLocation (as DocAddress) PostCode", "1062XD", goodsLocationAsJobDocAddress.PostCode);
			AssertEquals("GoodsLocation (as DocAddress) CountryCode", "NL", goodsLocationAsJobDocAddress.CountryCode);
			AssertEquals("GoodsLocation (as DocAddress) EORI", "NL194563729B01", goodsLocationAsJobDocAddress.EORINumber);
		});
	}

	protected override BusinessObject GetNewBusinessObject() => agent;
}
