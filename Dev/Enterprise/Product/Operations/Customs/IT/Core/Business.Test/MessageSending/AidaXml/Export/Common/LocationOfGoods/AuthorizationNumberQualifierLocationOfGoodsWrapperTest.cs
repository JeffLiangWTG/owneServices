using System;
using CargoWise.Customs.IT.MessageContracts.Declaration.Export;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.IT.Business.Declaration;

namespace Enterprise.Customs.IT.Business.MessageSending.AidaXml.Export.Testing;

sealed class AuthorizationNumberQualifierLocationOfGoodsWrapperTest : TestCaseWithFactory
{
	public void TestConstructor()
	{
		AssertExceptionThrown<ArgumentNullException>("When goodsLocation is null", () => new AuthorizationNumberQualifierLocationOfGoodsWrapper(goodsLocation: null));
	}

	public void TestTypeOfLocation()
	{
		goodsLocation.CGL_Type = "Y";

		var wrapper = GetNewWrapper();
		AssertEquals("TypeOfLocation", "Y", wrapper.TypeOfLocation);
	}

	public void TestQualifier()
	{
		goodsLocation.CGL_Qualifier = "A";

		var wrapper = GetNewWrapper();
		AssertEquals("Qualifier", "A", wrapper.Qualifier);
	}

	public void TestAuthorisationNumber()
	{
		CombineAssertions(() =>
		{
			goodsLocation.Address.AuthorisationNumber = "AUTHNUM";
			goodsLocation.CGL_AdditionalIdentifier = "ADDID";

			goodsLocation.CGL_Type = "B";
			var wrapper = GetNewWrapper();
			AssertEquals("AuthorisationNumber", "AUTHNUM.ADDID", wrapper.AuthorisationNumber);

			goodsLocation.CGL_Type = "C";
			wrapper = GetNewWrapper();
			AssertEquals("AuthorisationNumber", "AUTHNUM.ADDID", wrapper.AuthorisationNumber);

			goodsLocation.CGL_Type = "X";
			wrapper = GetNewWrapper();
			AssertEquals("AuthorisationNumber", "AUTHNUM", wrapper.AuthorisationNumber);
		});
	}

	public void TestCustomsOffice()
	{
		var wrapper = GetNewWrapper();
		AssertNull("CustomsOffice", wrapper.CustomsOffice);
	}

	public void TestAddress()
	{
		var wrapper = GetNewWrapper();
		AssertNull("Address", wrapper.Address);
	}

	public void TestContact()
	{
		goodsLocation.Address.E2_Contact = "";
		goodsLocation.Address.E2_Phone = "";
		goodsLocation.Address.E2_Email = "";

		var wrapper = GetNewWrapper();
		AssertNull("Contact", wrapper.Contact);

		goodsLocation.Address.E2_Contact = "RFI";
		wrapper = GetNewWrapper();
		AssertType<ContactWrapper>("Contact", wrapper.Contact);
	}

	protected override void SetUp()
	{
		base.SetUp();

		var declaration = Factory.New<JobDeclaration>();
		goodsLocation = (CusGoodsLocation)declaration.GoodsLocation;
	}

	CusGoodsLocation goodsLocation;

	ILocationOfGoods GetNewWrapper() => new AuthorizationNumberQualifierLocationOfGoodsWrapper(goodsLocation);
}
