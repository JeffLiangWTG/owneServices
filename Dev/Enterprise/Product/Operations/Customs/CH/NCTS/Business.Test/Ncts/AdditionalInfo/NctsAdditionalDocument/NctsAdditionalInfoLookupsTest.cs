using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.EU.NCTS.Business;
using static Enterprise.Customs.EU.NCTS.Business.NctsConstants;

namespace Enterprise.Customs.CH.NCTS.Business.Testing;

class NctsAdditionalInfoLookupsTest : BusinessObjectLookupsTestCase
{
	public void TestSubTypeList_Header()
	{
		var lookups = NctsHeaderAdditionalInfo.Lookups;
		CombineAssertions(() =>
		{
			AssertEquals("Codes for International Transit", "INF, REF, TRA", lookups.SubTypeList.CodesAsString);
			AssertSame("Cached", lookups.SubTypeList, lookups.SubTypeList);

			NctsHeader.MovementHeader.BM_InBondEntryType = NctsTypeOfDeclaration.Codes.NationalTransitSwitzerland;
			AssertEquals("Codes for National Transit", "INF, TRA", lookups.SubTypeList.CodesAsString);
			AssertSame("Cached", lookups.SubTypeList, lookups.SubTypeList);
		});
	}

	public void TestSubTypeList_GoodsItem()
	{
		var lookups = GoodsItemAdditionalInfo.Lookups;
		CombineAssertions(() =>
		{
			AssertEquals("Codes for International Transit", "INF, REF", lookups.SubTypeList.CodesAsString);
			AssertSame("Cached", lookups.SubTypeList, lookups.SubTypeList);

			NctsHeader.MovementHeader.BM_InBondEntryType = NctsTypeOfDeclaration.Codes.NationalTransitSwitzerland;
			AssertEquals("Codes for National Transit", "INF", lookups.SubTypeList.CodesAsString);
			AssertSame("Cached", lookups.SubTypeList, lookups.SubTypeList);
		});
	}

	NctsHeader CreateNctsHeader()
	{
		var nctsHeader = Factory.New<NctsHeader>();
		nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
		return nctsHeader;
	}

	NctsHeader NctsHeader => nctsHeader ?? (nctsHeader = CreateNctsHeader());
	NctsHeader nctsHeader;

	NctsAdditionalInfo NctsHeaderAdditionalInfo => nctsHeaderAdditionalInfo ?? (nctsHeaderAdditionalInfo = NctsHeader.AdditionalDocuments.AddNew());
	NctsAdditionalInfo nctsHeaderAdditionalInfo;

	NctsAdditionalInfo GoodsItemAdditionalInfo => goodsItemAdditionalInfo ?? (goodsItemAdditionalInfo = NctsHeader.Bills.AddNew().GoodsItems.AddNew().AdditionalInfos.AddNew());
	NctsAdditionalInfo goodsItemAdditionalInfo;
}
