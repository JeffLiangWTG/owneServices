using System;
using CargoWise.Customs.IT.MessageContracts.NCTS.Departure;
using Enterprise.Customs.Common;
using Enterprise.Customs.IT.Business.MessageSending.AidaXml.Export;
using Enterprise.Customs.IT.Business.MessageSending.AidaXml.Shared;
using Enterprise.Customs.IT.NCTS.Business.Testing;

namespace Enterprise.Customs.IT.NCTS.Business.MessageSending.AidaXml.Testing;

sealed class LocationOfGoodsWrapperTest : LocationOfGoodsBase
{
	public override void TestConstructor()
	{
		AssertExceptionThrown<ArgumentNullException>(
			"When goodsLocation is null",
			() => new LocationOfGoodsWrapper(goodsLocation: null));
	}

	public override void TestTypeOfLocation()
	{
		goodsLocation.CGL_Qualifier = "V";
		goodsLocation.CGL_Type = "D";
		var wrapper = CreateWrapper();
		AssertEquals(nameof(ILocationOfGoods.TypeOfLocation), "D", wrapper.TypeOfLocation);

		goodsLocation.CGL_Qualifier = "Y";
		goodsLocation.CGL_Type = "B";
		wrapper = CreateWrapper();
		AssertEquals(nameof(ILocationOfGoods.TypeOfLocation), "B", wrapper.TypeOfLocation);

		goodsLocation.CGL_Qualifier = "Z";
		goodsLocation.CGL_Type = "A";
		wrapper = CreateWrapper();
		AssertEquals(nameof(ILocationOfGoods.TypeOfLocation), "A", wrapper.TypeOfLocation);
	}

	public override void TestQualifier()
	{
		goodsLocation.CGL_Qualifier = "V";
		var wrapper = CreateWrapper();
		AssertEquals(nameof(ILocationOfGoods.Qualifier), "V", wrapper.Qualifier);

		goodsLocation.CGL_Qualifier = "Y";
		wrapper = CreateWrapper();
		AssertEquals(nameof(ILocationOfGoods.Qualifier), "Y", wrapper.Qualifier);

		goodsLocation.CGL_Qualifier = "Z";
		wrapper = CreateWrapper();
		AssertEquals(nameof(ILocationOfGoods.Qualifier), "Z", wrapper.Qualifier);
	}

	public override void TestAuthorisationNumber()
	{
		goodsLocation.CGL_Qualifier = "V";
		goodsLocation.CGL_AdditionalIdentifier = "AdditionalIdentifier";
		goodsLocation.Address.AuthorisationNumber = "ALE1";
		var wrapper = CreateWrapper();
		AssertNullOrEmpty("when Qualifier is not Y", wrapper.AuthorisationNumber);

		goodsLocation.CGL_Qualifier = "Y";
		goodsLocation.CGL_Type = "B";
		goodsLocation.Address.AuthorisationNumber = "    ALE1    ";
		goodsLocation.CGL_AdditionalIdentifier = "AdditionalIdentifier";
		wrapper = CreateWrapper();
		AssertEquals("when Qualifier is Y, type is B", "ALE1.AdditionalIdentifier", wrapper.AuthorisationNumber);

		goodsLocation.CGL_Type = "D";
		wrapper = CreateWrapper();
		AssertEquals("when Qualifier is Y, type is not [B , C]", "ALE1", wrapper.AuthorisationNumber);

		goodsLocation.CGL_Type = "C";
		wrapper = CreateWrapper();
		AssertEquals("when Qualifier is Y, type is C", "ALE1.AdditionalIdentifier", wrapper.AuthorisationNumber);

		goodsLocation.CGL_Qualifier = "Z";
		goodsLocation.CGL_AdditionalIdentifier = "AdditionalIdentifier";
		goodsLocation.Address.AuthorisationNumber = "ALE1";
		wrapper = CreateWrapper();
		AssertNullOrEmpty("when Qualifier is not Y", wrapper.AuthorisationNumber);
	}

	public override void TestAdditionalIdentifier()
	{
		goodsLocation.CGL_AdditionalIdentifier = "AdditionalIdentifier";
		var wrapper = CreateWrapper();
		AssertNullOrEmpty(wrapper.AdditionalIdentifier);
	}

	public override void TestCustomsOffice()
	{
		goodsLocation.CGL_Qualifier = "V";
		goodsLocation.CGL_CustomsOffice = "IT123456";
		var wrapper = CreateWrapper();
		AssertEquals("When Qualifier is V", "IT123456", wrapper.CustomsOffice);

		goodsLocation.CGL_Qualifier = "Y";
		goodsLocation.CGL_CustomsOffice = "IT123456";
		wrapper = CreateWrapper();
		AssertNullOrEmpty("When Qualifier is not V", wrapper.CustomsOffice);
	}

	public override void TestAddress()
	{
		CombineAssertions(() =>
		{
			goodsLocation.Address.E2_RN_NKCountryCode = "IT";
			goodsLocation.CGL_Qualifier = "V";
			var wrapper = CreateWrapper();
			AssertNull("When Qualifier is V", wrapper.Address);

			goodsLocation.CGL_Qualifier = "Y";
			wrapper = CreateWrapper();
			AssertNull("When Qualifier is Y", wrapper.Address);

			goodsLocation.CGL_Qualifier = "Z";
			wrapper = CreateWrapper();
			var locationAddress = wrapper.Address;
			AssertNotNull("When CGL_Qualifier is Z", locationAddress);
			AssertType<AddressWrapper>(locationAddress);
			AssertSame("Cached ", wrapper.Address, locationAddress);
		});
	}

	public override void TestContact()
	{
		var goodsLocationAddress = goodsLocation.Address;
		goodsLocationAddress.E2_Contact = "name";
		goodsLocationAddress.E2_Phone = "97253585858";
		goodsLocationAddress.E2_Email = "d@d.com";

		goodsLocation.CGL_Qualifier = "V";
		var wrapper = CreateWrapper();
		AssertNull("When Qualifier is not [Y,Z], always null", wrapper.Contact);

		goodsLocation.CGL_Qualifier = "Y";
		goodsLocationAddress = goodsLocation.Address;
		goodsLocationAddress.E2_Contact = "name";
		goodsLocationAddress.E2_Phone = "97253585858";
		goodsLocationAddress.E2_Email = "d@d.com";
		wrapper = CreateWrapper();
		var contact = wrapper.Contact;
		AssertNotNull("When Qualifier is [Y,Z], all 3 fields are filled", wrapper.Contact);
		AssertSame(contact, wrapper.Contact);
		AssertType<ContactWrapper>(contact);

		goodsLocationAddress = goodsLocation.Address;
		goodsLocationAddress.E2_Contact = "";
		goodsLocationAddress.E2_Phone = "";
		goodsLocationAddress.E2_Email = "";
		wrapper = CreateWrapper();
		AssertNull("When Qualifier is [Y,Z], no fields is filled", wrapper.Contact);

		goodsLocation.CGL_Qualifier = "Z";
		goodsLocationAddress = goodsLocation.Address;
		goodsLocationAddress.E2_Email = "F@g.com";
		wrapper = CreateWrapper();
		AssertNotNull("When Qualifier is [Y,Z],at least one of the 3 fields is filled", wrapper.Contact);
	}

	protected override void SetUp()
	{
		base.SetUp();
		var nctsHeader = Factory.NewDepartureNctsHeader();
		nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
		goodsLocation = nctsHeader.MovementHeader.GoodsLocation;
	}

	protected override ILocationOfGoods CreateWrapper() => new LocationOfGoodsWrapper(goodsLocation);

	CusGoodsLocation goodsLocation;
}
