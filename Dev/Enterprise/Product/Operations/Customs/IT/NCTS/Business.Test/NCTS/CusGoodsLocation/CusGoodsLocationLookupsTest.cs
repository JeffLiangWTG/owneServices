using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Common;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.IT.NCTS.Business.Testing;

sealed class CusGoodsLocationLookupsTest : BusinessObjectLookupsTestCase
{
	public void TestQualifierList()
	{
		AssertEquals("QualifierList", "V, Y, Z", goodsLocationLookups.QualifierList.CodesAsString);
	}

	public void TestAdditionalIdentifierList_Phase4()
	{
		nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
		AssertEquals(0, goodsLocationLookups.AdditionalIdentifierList.Count);
	}

	public void TestOrganisationList()
	{
		AssertType("Type", typeof(OrganisationsFindBoxCollection), goodsLocationLookups.OrganisationList);
	}

	protected override void SetUp()
	{
		base.SetUp();

		nctsHeader = Factory.New<NctsHeader>();
		nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
		nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
		goodsLocation = nctsHeader.MovementHeader.GoodsLocation;
		goodsLocationLookups = goodsLocation.Lookups;
	}

	NctsHeader nctsHeader;
	CusGoodsLocation goodsLocation;
	CusGoodsLocationLookups goodsLocationLookups;
}
