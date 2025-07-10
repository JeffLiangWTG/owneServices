using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.MX.Business.Testing
{
	class JobDeclarationLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestMessageTypeList()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			var list = declaration.Lookups.MessageTypeList;

			AssertEquals("MessageTypeList should be", 2, list.Count);
			AssertContainsExactElementsInAnyOrder(new[] { "EXP", "IMP" }, list.GetAllCodes());
		}

		public void TestGoodsOrigin()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			var list = declaration.Lookups.GoodsOrigin as CodeDescriptionPairList;

			AssertEquals("GoodsOrigin should be", 10, list.Count);
			AssertContainsExactElementsInAnyOrder(new[]	{ "1", "2", "3", "5", "6", "7", "8", "9", "10", "11" }, list.GetAllCodes());
			AssertSame(list, Factory.GetCachedValue<GoodsRegionList>());
		}

		public void TestGoodsDestination()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			var list = declaration.Lookups.GoodsDestination as CodeDescriptionPairList;

			AssertEquals("GoodsDestination should be", 10, list.Count);
			AssertContainsExactElementsInAnyOrder(new[] { "1", "2", "3", "5", "6", "7", "8", "9", "10", "11" }, list.GetAllCodes());
			AssertSame(list, Factory.GetCachedValue<GoodsRegionList>());
		}

		public void TestMessageSubTypeList()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();

			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			var list = declaration.Lookups.MessageSubTypeList;

			AssertEquals("MessageSubTypeList for EXP should be", 53, list.Count);
			AssertSame(list, MXDeclarationTypeList.GetMessageSubTypeListForExport(Factory));
			AssertContainsExactElementsInExactOrder(new[] { "A1","A4", "AD", "AJ", "BA", "BB", "BC", "BD", "BF", "BM", "BO", "BP", "BR", "CT", "D1", "F4", "F8", "F9", "G1", "G6",
																"G7", "G9", "GC", "H1", "H8", "I1", "J3", "J4", "K1", "K2", "K3", "L1", "M3", "M4", "M5", "R1", "RT", "S2", "T1",
																"T3", "T6", "T7", "T9", "V1", "V2", "V3", "V4", "V5", "V6", "V7", "V8", "V9", "VD" }, list.GetAllCodes());

			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			list = declaration.Lookups.MessageSubTypeList;

			AssertEquals("MessageSubTypeList for IMP should be", 68, list.Count);
			AssertSame(list, MXDeclarationTypeList.GetMessageSubTypeListForImport(Factory));
			AssertContainsExactElementsInExactOrder(new[] { "A1","A3","A4", "A5", "AD", "AF", "AJ", "BA", "BB", "BC", "BD", "BE", "BH","BI", "BO", "BP", "BR", "C1", "C3", "D1", "E1", "E2", "E3",
																"E4", "F2", "F3", "F4", "F5", "F8", "F9", "G1", "G2", "G6", "G7", "G8", "G9", "GC", "H1", "H8", "I1", "IN", "J4", "K1", "K2", "L1", "M1", "M2",
																"M3", "M4", "P1", "R1", "S2", "T1", "T3", "T6", "T7", "T9", "V1", "V2", "V3", "V5", "V6", "V7", "V8", "V9", "VD", "VF", "VU" }, list.GetAllCodes());
		}

		public void TestCustomsRegimeList()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var list = declaration.Lookups.CustomRegimeList;

			AssertEquals("CustomRegimeList should be", 7, list.Count);
			AssertSame(list, CustomsRegimeList.GetCustomRegimeListForImport(Factory));
			AssertContainsExactElementsInExactOrder(new[] { "IMD", "ITR", "ITE", "DFI", "RFE", "TRA", "RFS" }, list.GetAllCodes());

			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			list = declaration.Lookups.CustomRegimeList;

			AssertEquals("CustomRegimeList should be", 7, list.Count);
			AssertSame(list, CustomsRegimeList.GetCustomRegimeListForExport(Factory));
			AssertContainsExactElementsInExactOrder(new[] { "EXD", "ETR", "ETE", "DFI", "RFE", "TRA", "RFS" }, list.GetAllCodes());
		}

		public void TestFacilityList()
		{
			ReferenceTestDataHelper.CreateCustomsFacilitiesCodes(Factory);
			var list = MXRefCusCodeListTypes.GetCustomsFacilities(Factory);
			list.Load();

			var lookups = new JobDeclarationLookups(Factory.New<JobDeclaration>());
			AssertSame(list, lookups.EntryAreaList);
			AssertSame(list, lookups.ClearanceList);
		}
	}
}
