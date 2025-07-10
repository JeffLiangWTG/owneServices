using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using Enterprise.Customs.BE.Business.CusTempStorage;
using Enterprise.Customs.BE.Business.Declaration;
using Enterprise.Customs.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.BE.Business.Testing;

[TestedType(typeof(CusGoodsLocation))]
sealed class CusGoodsLocationTest : EnterpriseBusinessObjectTestCase
{
	public void TestLookups()
	{
		AssertType<CusGoodsLocationLookups>(cusGoodsLocation.Lookups);
	}

	public void TestLookupsTS()
	{
		var temporaryStorageHeader = Factory.NewWithValidTestData<TemporaryStorageHeader>();
		var cusGoodsLocationTS = (CusGoodsLocation)temporaryStorageHeader.GoodsLocation;
		AssertType<PNTSCusGoodsLocationLookups>(cusGoodsLocationTS.Lookups);
	}

	public void TestValidation()
	{
		AssertType<CusGoodsLocationValidation>(cusGoodsLocation.Validation);
	}

	public void TestUnlocodeList()
	{
		AssertEquals("Unlocode: List", "Lookups.UnlocodeList", cusGoodsLocation.UnlocodeInfo.GetAttribute<ListAttribute>().ListDataSourceMember);
	}

	protected override BusinessObject GetNewBusinessObject()
	{
		var declaration = Factory.New<JobDeclaration>();
		var location = declaration.CustomsEntryInstructions.AddNew().GoodsLocation;
		location.CGL_LocationUse = "DEP";
		return location;
	}

	protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory) => GetNewBusinessObject();

	protected override BusinessObject GetBusinessObjectForFetchForLoad() => GetNewBusinessObject();
	protected override void SetUp()
	{
		base.SetUp();
		cusGoodsLocation = (CusGoodsLocation)GetNewBusinessObject();
	}

	CusGoodsLocation cusGoodsLocation;
}
