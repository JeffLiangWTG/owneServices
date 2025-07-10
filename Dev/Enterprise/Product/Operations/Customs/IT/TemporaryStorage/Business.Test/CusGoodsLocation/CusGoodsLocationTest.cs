using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IT.TemporaryStorage.Business.Testing;

[TestedType(typeof(CusGoodsLocation))]
sealed class CusGoodsLocationTest : EnterpriseBusinessObjectTestCase
{
	public void TestLookups()
	{
		AssertType<CusGoodsLocationLookups>(Location.Lookups);
	}

	public void TestAddress()
	{
		AssertType<CusGoodsLocationAddress>(Location.Address);
	}

	public void TestValidation()
	{
		AssertType<CusGoodsLocationValidation>(Location.Validation);
	}

	public void TestAdditionalIdentifier_MaxLength()
	{
		AssertEquals(6, Location.AdditionalIdentifierInfo.MaxLength);
	}

	public void TestAdditionalIdentifier_Attributes()
	{
		AssertEntity<CusGoodsLocation>()
			.HasProperty(x => x.AdditionalIdentifier)
			.WithCaption("Place ID")
			.WithList("Lookups.AdditionalIdentifierList");
	}

	public void TestSetDefaultsForNew()
	{
		var header = Factory.New<TemporaryStorageHeader>();
		var location = CusGoodsLocation.New<CusGoodsLocation>(header, CusGoodsLocationUseList.Codes.TemporaryStorage);
		AssertEquals("CGL_Qualifier", "Y", location.CGL_Qualifier);
		AssertEquals("CGL_Type", "C", location.CGL_Type);
	}

	public void TestAdditionalIdentifierCaptions()
	{
		var expectedCaption = "Place ID";

		CombineAssertions("Caption is set for both property and wrapped property", () =>
		{
			AssertEntity<CusGoodsLocation>()
				.HasProperty(x => x.CGL_AdditionalIdentifier)
				.WithCaption(expectedCaption);

			AssertEntity<CusGoodsLocation>()
				.HasProperty(x => x.AdditionalIdentifier)
				.WithCaption(expectedCaption);
		});
	}

	protected override BusinessObject GetNewBusinessObject() => GetNewBusinessObject(Factory);

	protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory) => GetNewBusinessObject(factory);

	protected override BusinessObject GetBusinessObjectForFetchForLoad() => GetNewBusinessObject();

	CusGoodsLocation GetNewBusinessObject(BusinessObjectFactory factory)
	{
		var location = factory.New<CusGoodsLocation>();
		location.Parent = factory.New<TemporaryStorageHeader>();
		location.CGL_LocationUse = "DEP";
		return location;
	}

	CusGoodsLocation Location => fLocation ??= GetNewBusinessObject(Factory);
	CusGoodsLocation fLocation;
}
