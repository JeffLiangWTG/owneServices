using System;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.IT.Business.Declaration;

namespace Enterprise.Customs.IT.Business.MessageSending.AidaXml.Export.Testing;

sealed class ContactWrapperTest : TestCaseWithFactory
{
	public void TestNewOrNull_ThrowsWhenAddressIsNull()
	{
		AssertExceptionThrown<ArgumentNullException>("When docAddress is null", () => ContactWrapper.NewOrNull(docAddress: null));
	}

	public void TestNewOrNull_ReturnsNullWhenAllInvolvedFieldsAreEmpty()
	{
		goodsLocationAddress.E2_Contact = "";
		goodsLocationAddress.E2_Phone = "";
		goodsLocationAddress.E2_Email = "";
		AssertNull("Contact Wrapper", ContactWrapper.NewOrNull(goodsLocationAddress));
	}

	public void TestNewOrNull_ReturnsWrapperWhenAnyInvolvedFieldIsFilled_Contact()
	{
		goodsLocationAddress.E2_Contact = "Contact Name";

		var contactWrapper = ContactWrapper.NewOrNull(goodsLocationAddress);
		AssertNotNull("Contact Wrapper", contactWrapper);
		AssertEquals("Name", "Contact Name", contactWrapper.Name);
	}

	public void TestNewOrNull_ReturnsWrapperWhenAnyInvolvedFieldIsFilled_Phone()
	{
		goodsLocationAddress.E2_Phone = "123456";

		var contactWrapper = ContactWrapper.NewOrNull(goodsLocationAddress);
		AssertNotNull("Contact Wrapper", contactWrapper);
		AssertEquals("PhoneNumber", "123456", contactWrapper.PhoneNumber);
	}

	public void TestNewOrNull_ReturnsWrapperWhenAnyInvolvedFieldIsFilled_Email()
	{
		goodsLocationAddress.E2_Email = "a@a.a";

		var contactWrapper = ContactWrapper.NewOrNull(goodsLocationAddress);
		AssertNotNull("Contact Wrapper", contactWrapper);
		AssertEquals("Email", "a@a.a", contactWrapper.EmailAddress);
	}

	protected override void SetUp()
	{
		base.SetUp();
		var declaration = Factory.New<JobDeclaration>();
		var goodsLocation = (CusGoodsLocation)declaration.GoodsLocation;
		goodsLocationAddress = goodsLocation.Address;
	}

	CusGoodsLocationAddress goodsLocationAddress;
}
