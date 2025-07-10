using System;
using CargoWise.Customs.IT.MessageContracts.Declaration.Export;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.IT.Business.Declaration;

namespace Enterprise.Customs.IT.Business.MessageSending.AidaXml.Export.Testing;

sealed class CustomsOfficeQualifierLocationOfGoodsWrapperTest : TestCaseWithFactory
{
	public void TestConstructor()
	{
		AssertExceptionThrown<ArgumentNullException>("When goodsLocation is null", () => new CustomsOfficeQualifierLocationOfGoodsWrapper(goodsLocation: null));
	}

	public void TestTypeOfLocation()
	{
		goodsLocation.CGL_Type = "V";

		var wrapper = GetNewWrapper();
		AssertEquals("TypeOfLocation", "V", wrapper.TypeOfLocation);
	}

	public void TestQualifier()
	{
		goodsLocation.CGL_Qualifier = "A";

		var wrapper = GetNewWrapper();
		AssertEquals("Qualifier", "A", wrapper.Qualifier);
	}

	public void TestAuthorisationNumber()
	{
		var wrapper = GetNewWrapper();
		AssertNull("AuthorisationNumber", wrapper.AuthorisationNumber);
	}

	public void TestCustomsOffice()
	{
		goodsLocation.CGL_CustomsOffice = "IT137100";

		var wrapper = GetNewWrapper();
		AssertEquals("CustomsOffice", "IT137100", wrapper.CustomsOffice);
	}

	public void TestAddress()
	{
		var wrapper = GetNewWrapper();
		AssertNull("Address", wrapper.Address);
	}

	public void TestContact()
	{
		var wrapper = GetNewWrapper();
		AssertNull("Contact", wrapper.Contact);
	}

	protected override void SetUp()
	{
		base.SetUp();

		var declaration = Factory.New<JobDeclaration>();
		goodsLocation = (CusGoodsLocation)declaration.GoodsLocation;
	}

	CusGoodsLocation goodsLocation;

	ILocationOfGoods GetNewWrapper() => new CustomsOfficeQualifierLocationOfGoodsWrapper(goodsLocation);
}
