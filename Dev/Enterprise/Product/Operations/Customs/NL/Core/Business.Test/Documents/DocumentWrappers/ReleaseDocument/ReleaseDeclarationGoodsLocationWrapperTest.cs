using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.NL.Business.Testing;

[TestedType(typeof(ReleaseDeclarationGoodsLocationWrapper))]
sealed class ReleaseDeclarationGoodsLocationWrapperTest : NonPersistentBusinessObjectTestCase
{
	protected override BusinessObject GetNewBusinessObject() => new ReleaseDeclarationGoodsLocationWrapper(Factory.New<CusGoodsLocation>());

	public void TestConstructor() => AssertExceptionThrown<ArgumentNullException>(() => new ReleaseDeclarationGoodsLocationWrapper(null));

	public void TestTypeCode() => AssertEquals("Z", wrapper.TypeCode);

	public void TestAddress() => AssertType<AddressInformationDocumentWrapper>(wrapper.Address);

	public void TestIdentificationTypeCode() => AssertEquals("A", wrapper.IdentificationTypeCode);

	protected override void SetUp()
	{
		base.SetUp();
		var goodsLocation = Factory.New<CusGoodsLocation>();
		goodsLocation.CGL_Qualifier = "Z";
		goodsLocation.CGL_Type = "A";
		goodsLocation.Address.Address1 = "Street 5";
		goodsLocation.Address.Postcode = "1234AB";
		goodsLocation.Address.E2_RN_NKCountryCode = "NL";

		wrapper = new ReleaseDeclarationGoodsLocationWrapper(goodsLocation);
	}
	ReleaseDeclarationGoodsLocationWrapper wrapper;
}
