using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.CH.Business.Testing;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.CH.NCTS.Business.Testing;

class NctsDepartureMovementHeaderLookupsTest : BusinessObjectLookupsTestCase
{
	public void TestDeclarationTypeList()
	{
		new RefDataTestHelper(Factory).CreateNctsDeclarationTypeList();

		var declarationTypeList = Lookups.DeclarationTypeList;

		CombineAssertions(() =>
		{
#if NETFRAMEWORK
			AssertEquals("CodesAsString", "T, T1, T2, T2F, T-CH", declarationTypeList.CodesAsString);
#elif NET
			AssertEquals("CodesAsString", "T, T-CH, T1, T2, T2F", declarationTypeList.CodesAsString);
#endif
			AssertSame("Cached", declarationTypeList, declarationTypeList);
		});
	}

	public void TestNctsSpecificCircumstanceIndicatorList()
	{
		new RefDataTestHelper(Factory).CreateGlobalCodeN0296List();

		var list = Lookups.NctsSpecificCircumstanceIndicatorList;

		CombineAssertions(() =>
		{
			AssertEquals("Valid code", true, list.ContainsCode("A20"));
			AssertEquals("Invalid code", false, list.ContainsCode("AAA"));
		});
	}

	public void TestBorderModeOfTransportList()
	{
		CombineAssertions(() =>
		{
			AssertEquals("Codes", "1, 2, 3, 4, 7, 8, 9", Lookups.BorderModeOfTransportList.CodesAsString);
			AssertSame("Cached", Lookups.BorderModeOfTransportList, Lookups.BorderModeOfTransportList);
		});
	}

	public void TestTransportAtBorderTypeOfIdList()
	{
		CombineAssertions(() =>
		{
			AssertEquals("Codes", "10, 21, 30, 40, 41, 80, 81, 99", Lookups.TransportAtBorderTypeOfIdList.CodesAsString);
			AssertSame("Cached", Lookups.TransportAtBorderTypeOfIdList, Lookups.TransportAtBorderTypeOfIdList);
		});
	}

	public void TestModeOfTransportList()
	{
		LookupsTestHelper.AssertListCodesAndIsCached(() => Lookups.ModeOfTransportList, "2, 3, 4, 7, 8, 9");
	}

	public void TestTransportAtDepartureTypeOfIdList()
	{
		AssertSame(NctsLookupsHelper.TransportTypeOfIdList(Factory), Lookups.TransportAtDepartureTypeOfIdList);
	}

	public void TestExportEntryHeaderCollection()
	{
		AssertType<ExportEntryHeaderCollection>(Lookups.ExportEntryHeaderCollection);
	}

	public void TestNctsTransitStatusList()
	{
		LookupsTestHelper.AssertListCodesAndIsCached(() => Lookups.NctsTransitStatusList, new EU.NCTS.Business.CodeDescriptionPairLists.NCTS5DepartureCustomsStatusList().CodesAsString + ", ACT, CO4, EXP");
	}

	public void TestOfficeCodeList()
	{
		const string Transit = OfficeCodes_NCTS.Codes.NCTSOfficeOfTransit;

		var officeNo = 0;
		foreach (var type in new OfficeCodes_NCTS().GetAllCodes().Append(Transit))
		{
			var customsOffice = NctsHeader.MovementHeader.CustomsOfficesForDeparture.AddNew();
			customsOffice.CY_Code = type;
			customsOffice.CY_Data = $"{type}#{++officeNo}";
		}

		var lookup = Lookups.OfficeCodeList;
		AssertContainsExactElementsInAnyOrder(new[] { Transit, Transit }, lookup.GetAllCodes().Select(x => x.Substring(0, 3)));
	}

	NctsHeader NctsHeader => nctsHeader ?? (nctsHeader = CreateNctsHeader());
	NctsHeader nctsHeader;
	NctsHeader CreateNctsHeader()
	{
		var nctsHeader = Factory.New<NctsHeader>();
		nctsHeader.BH_HeaderType = NctsMovementType.Codes.Departure;
		return nctsHeader;
	}

	NctsDepartureMovementHeaderLookups Lookups => NctsHeader.MovementHeader.Lookups;
}
