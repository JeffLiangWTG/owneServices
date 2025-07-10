using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.Universal.Testing;
using static Enterprise.Core.Constants;
using static Enterprise.Core.Constants.Customs.Universal;

namespace Enterprise.Customs.ES.NCTS.Business.Testing;

class NctsArrivalMovementHeaderLookupsTest : BusinessObjectLookupsTestCase
{
	public void TestLocationsList()
	{
		var helper = new UniversalReferenceTestDataHelper(Factory);

		helper.CreateNewOrGetExistingCusCodeType(RefCusCodeListTypes.Codes.GoodsOfLocationType, "Locations");
		var grouping = helper.CreateNewOrGetExistingDataGrouping("EUN");
		helper.CreateNewOrGetExistingDataGrouping(CountryCodes.Spain, parent: grouping);
		helper.CreateCusCodeList("ES", RefCusCodeListTypes.Codes.GoodsOfLocationType, "01", "Test 1", ZDateTime.BrettsBirthday, ZDateTime.Now.AddMonths(2));
		helper.CreateCusCodeList("ES", RefCusCodeListTypes.Codes.GoodsOfLocationType, "02", "Test 2", ZDateTime.BrettsBirthday, ZDateTime.Now.AddMonths(2));
		helper.CreateCusCodeList("ES", RefCusCodeListTypes.Codes.GoodsOfLocationType, "03", "Test 3", ZDateTime.BrettsBirthday, ZDateTime.Now.AddMonths(2));
		helper.CreateCusCodeList("ES", RefCusCodeListTypes.Codes.ImportAddDocAdditionalInformation, "04", "Test 4", ZDateTime.BrettsBirthday, ZDateTime.Now.AddMonths(2));

		Factory.Save();

		var list = lookups.LocationsList;
		var listCached = lookups.LocationsList;
		CombineAssertions(() =>
		{
			AssertEquals("01, 02, 03", list.CodesAsString);
			AssertEquals(list, listCached);
		});
	}

	public void TestNctsMovementHeaderTransactionStatusList()
	{
		var list = lookups.NctsMovementHeaderTransactionStatusList;
		CombineAssertions(() =>
		{
			AssertEquals("Codes", "007, 013, 014, 015, 043, 044, 141, 170, AAC, ACK, ARR, AWO, CAC, CNT, CRF, DAC, DAJ, DAR, DCC, DCI, DGN, DIS, DMA, DNR, DPJ, DRI, DRJ, DRL, DTJ, FIN, FRC, MAM, MAS, MDS, MNS, MPN, MRI, MRR, MUS, NCK, NRL, PRC, R4A, RYP, STU, URJ, DOT, TNN, TSA", list.CodesAsString);
			AssertSame("Cached", list, lookups.NctsMovementHeaderTransactionStatusList);
		});
	}

	public void TestDeclarationTypeList()
	{
		var list = lookups.DeclarationTypeList;
		CombineAssertions(() =>
		{
			AssertEquals("Values", "DAT, DUA", list.CodesAsString);
			AssertSame("Cached", list, lookups.DeclarationTypeList);
		});
	}

	public void TestNctsTransitStatusList_Phase4()
	{
		nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
		CombineAssertions(() =>
		{
			var list = lookups.NctsTransitStatusList;
			AssertType<NctsTransitStatusList>(list);
			AssertSame("Cached", list, lookups.NctsTransitStatusList);
			AssertEquals("Count is correct", 20, list.Count);
			AssertEquals("Extra Values in ES contains DGP", true, list.ContainsCode("DGP"));
			AssertEquals("Extra Values in ES contains INV", true, list.ContainsCode("INV"));
			AssertEquals("Extra Values in ES contains PDA", true, list.ContainsCode("PDA"));
		});
	}

	public void TestNctsTransitStatusList_Phase5()
	{
		nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
		CombineAssertions(() =>
		{
			var list = lookups.NctsTransitStatusList;
			AssertType<ESNCTS5ArrivalCustomsStatusList>(list);
			AssertSame("Cached", list, lookups.NctsTransitStatusList);
			AssertEquals("Count is correct", 13, list.Count);
			AssertEquals("Extra Values in ES contains C01", true, list.ContainsCode("C01"));
		});
	}

	protected override void SetUp()
	{
		base.SetUp();
		nctsHeader = Factory.New<NctsHeader>();
		nctsHeader.SetMovementType(NctsMovementType.Codes.Arrival);
		lookups = new NctsArrivalMovementHeaderLookups(nctsHeader.ArrivalMovementHeader);
	}
	NctsHeader nctsHeader;
	NctsArrivalMovementHeaderLookups lookups;
}
