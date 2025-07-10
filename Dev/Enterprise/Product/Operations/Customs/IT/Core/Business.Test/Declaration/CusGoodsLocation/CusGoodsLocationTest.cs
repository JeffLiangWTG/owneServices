using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IT.Business.Declaration.Testing;

[TestedType(typeof(CusGoodsLocation))]
sealed class CusGoodsLocationTest : EnterpriseBusinessObjectTestCase
{
	public void TestAddressType()
	{
		AssertType<CusGoodsLocationAddress>("AddressType", goodsLocation.Address);
	}

	public void TestLookup()
	{
		AssertType<CusGoodsLocationLookups>("Lookups", goodsLocation.Lookups);
	}

	public void TestIsPlaceCodeAvailable()
	{
		goodsLocation.CGL_Qualifier = "Z";
		AssertEquals("When Qualifier is not Y and Type is not B or C", false, goodsLocation.IsPlaceCodeAvailable);

		goodsLocation.CGL_Qualifier = "Y";
		goodsLocation.CGL_Type = "B";
		AssertEquals("When Qualifier is Y and Type is B or C", true, goodsLocation.IsPlaceCodeAvailable);
	}

	public void TestAdditionalIdentifierCaption()
	{
		var additionalIdentifierCaption = DataBoundResourceStrings.GetDataForProperty(goodsLocation.CGL_AdditionalIdentifierInfo).Caption;
		AssertEquals("AdditionalIdentifier Caption", "Place Code", additionalIdentifierCaption);
	}

	public void TestGoodsLocationAddressOverride()
	{
		var address = goodsLocation.Address;
		AssertEquals("When CGL_Qualifier is empty, E2_AddressOverride", true, address.E2_AddressOverride);

		goodsLocation.CGL_Qualifier = "Y";
		AssertEquals("When CGL_Qualifier = Y, E2_AddressOverride", true, address.E2_AddressOverride);

		goodsLocation.CGL_Qualifier = "Z";
		AssertEquals("When CGL_Qualifier = Z, E2_AddressOverride", false, address.E2_AddressOverride);
	}

	public void TestClearAddressFields()
	{
		var orgAddress = Factory.New<OrgAddress>();
		var address = goodsLocation.Address;
		address.E2_OA_Address = orgAddress.PK;

		goodsLocation.CGL_Qualifier = "Z";
		AssertEquals("When CGL_Qualifier = Z, E2_AddressOverride", false, address.E2_AddressOverride);
		AssertEquals("OrganisationPK", orgAddress.OA_OH, address.OrganisationPK);

		CombineAssertions("Qalifier change calls ClearAddressFields", () =>
		{
			goodsLocation.CGL_Qualifier = "Y";
			AssertEquals("When CGL_Qualifier = Y, E2_AddressOverride", true, address.E2_AddressOverride);
			AssertEquals("OrganisationPK", ZGuid.Empty, address.OrganisationPK);
		});
	}

	protected override void SetUp()
	{
		base.SetUp();

		declaration = Factory.New<JobDeclaration>();
		goodsLocation = (CusGoodsLocation)declaration.GoodsLocation;
		goodsLocation.CGL_LocationUse = "DEP";
	}

	JobDeclaration declaration;
	CusGoodsLocation goodsLocation;

	protected override BusinessObject GetNewBusinessObject() => goodsLocation;

	protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory) => goodsLocation;

	protected override BusinessObject GetBusinessObjectForFetchForLoad() => goodsLocation;
}
