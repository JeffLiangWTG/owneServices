using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.CA.Business.Testing
{
	sealed class CACustomsCodesResolverTest : TestCaseWithFactory
	{
		public void TestMatchingUserDefinedFirstThenSystemDefinedForOffice()
		{
			#region Prepare Test Data

			CreateUNLoco("ABAAA");
			CreateLocoMap("0001", "ABAAA", CALocoMapSystemUsageList.Codes.Air, true);
			CreateLocoMap("0002", "ABAAA", CALocoMapSystemUsageList.Codes.Air, false);
			CreateLocoMap("0003", "ABDDD", CALocoMapSystemUsageList.Codes.Sea, false);

			CreateUNLoco("ABBBB");
			CreateLocoMap("0004", "ABBBB", CALocoMapSystemUsageList.Codes.Air, true);
			CreateLocoMap("0005", "ABDDD", CALocoMapSystemUsageList.Codes.Sea, false);
			CreateLocoMap("0006", "ABBBB", CALocoMapSystemUsageList.Codes.All, true);

			CreateUNLoco("ABCCC");
			CreateLocoMap("0007", "ABCCC", CALocoMapSystemUsageList.Codes.Air, true);
			CreateLocoMap("0008", "ABCCC", CALocoMapSystemUsageList.Codes.Air, false);
			CreateLocoMap("0009", "ABCCC", CALocoMapSystemUsageList.Codes.Air, false);
			CreateLocoMap("0010", "ABCCC", CALocoMapSystemUsageList.Codes.All, true);

			CreateUNLoco("ABDDD");
			CreateLocoMap("0011", "ABDDD", CALocoMapSystemUsageList.Codes.Sea, false);
			CreateLocoMap("0012", "ABDDD", CALocoMapSystemUsageList.Codes.All, false);
			CreateLocoMap("0013", "ABDDD", CALocoMapSystemUsageList.Codes.Oth, true);

			CreateUNLoco("ABEEE");
			CreateLocoMap("0014", "ABEEE", CALocoMapSystemUsageList.Codes.Sea, true);
			CreateLocoMap("0015", "ABEEE", CALocoMapSystemUsageList.Codes.Oth, true);

			Factory.Save();

			#endregion

			AssertEquals("AIR user definied code should be matched", "0002", CACustomsCodesResolver.MatchingCustomsCode(CACustomsCodeType.Office, "ABAAA", "AIR", Factory));
			AssertEquals("AIR system definied code should be matched", "0004", CACustomsCodesResolver.MatchingCustomsCode(CACustomsCodeType.Office, "ABBBB", "AIR", Factory));
			AssertEquals("No code matched as multiple AIR user definied results found", ZString.Empty, CACustomsCodesResolver.MatchingCustomsCode(CACustomsCodeType.Office, "ABCCC", "AIR", Factory));
			AssertEquals("ALL system definied code should be matched", "0012", CACustomsCodesResolver.MatchingCustomsCode(CACustomsCodeType.Office, "ABDDD", "AIR", Factory));
			AssertEquals("OTH system definied code should be matched", "0015", CACustomsCodesResolver.MatchingCustomsCode(CACustomsCodeType.Office, "ABEEE", "AIR", Factory));
		}

		public void TestMatchingUNLOCO()
		{
			var loco = Factory.LoadTop1<RefUNLOCO>(new ZQuery());
			var match = loco.RefLocoMaps.AddNew();
			match.RY_RN = Core.Constants.CountryGuids.Canada;
			match.RY_LocalPortCode = "LOCAL";
			match.RY_SystemUsage = CALocoMapSystemUsageList.Codes.Oth;

			var noMatch = loco.RefLocoMaps.AddNew();
			noMatch.RY_RN = Core.Constants.CountryGuids.Canada;
			noMatch.RY_SystemUsage = CALocoMapSystemUsageList.Codes.Sub;

			AssertEquals(loco.RL_Code, CACustomsCodesResolver.MatchingUNLOCO("LOCAL", Factory));

			var anotherMatch = loco.RefLocoMaps.AddNew();
			anotherMatch.RY_RN = Core.Constants.CountryGuids.Canada;
			match.RY_SystemUsage = CALocoMapSystemUsageList.Codes.Air;
			AssertEquals(loco.RL_Code, CACustomsCodesResolver.MatchingUNLOCO("LOCAL", Factory));
		}

		public void TestNoMatch()
		{
			var loco = Factory.New<RefUNLOCO>();
			loco.RL_Code = "TEST";
			AssertEquals(0, CACustomsCodesResolver.GetMatchesForCodeType(CACustomsCodeType.Office, "TEST", ZString.Empty, Factory).Count);
			AssertEquals(ZString.Empty, CACustomsCodesResolver.MatchingCustomsCode(CACustomsCodeType.Office, "LEON", ZString.Empty, Factory));
		}

		public void TestSingleMatch()
		{
			var loco = Factory.New<RefUNLOCO>();
			loco.RL_Code = "TEST";
			var match = loco.RefLocoMaps.AddNew();
			match.RY_RN = Core.Constants.CountryGuids.Canada;
			match.RY_LocalPortCode = "LOCAL";
			match.RY_SystemUsage = CALocoMapSystemUsageList.Codes.Sea;
			var noMatch = loco.RefLocoMaps.AddNew();
			noMatch.RY_RN = Core.Constants.CountryGuids.Canada;
			noMatch.RY_SystemUsage = CALocoMapSystemUsageList.Codes.Air;

			var matches = CACustomsCodesResolver.GetMatchesForCodeType(CACustomsCodeType.Office, "TEST", TransportTypeList.Codes.Sea, Factory);
			AssertEquals(1, matches.Count);
			AssertEquals(match, matches[0]);
			AssertEquals("LOCAL", CACustomsCodesResolver.MatchingCustomsCode(CACustomsCodeType.Office, "TEST", TransportTypeList.Codes.Sea, Factory));
		}

		public void TestMultipleMatches()
		{
			var loco = Factory.New<RefUNLOCO>();
			loco.RL_Code = "TEST";
			var match = loco.RefLocoMaps.AddNew();
			match.RY_RN = Core.Constants.CountryGuids.Canada;
			match.RY_SystemUsage = CALocoMapSystemUsageList.Codes.All;
			match.RY_LocalPortCode = "1111";
			RefLocoMap match2 = loco.RefLocoMaps.AddNew();
			match2.RY_RN = Core.Constants.CountryGuids.Canada;
			match2.RY_SystemUsage = CALocoMapSystemUsageList.Codes.All;
			match2.RY_LocalPortCode = "2222";

			var matches = CACustomsCodesResolver.GetMatchesForCodeType(CACustomsCodeType.Office, "TEST", ZString.Empty, Factory);
			AssertEquals(2, matches.Count);
			AssertEquals(ZString.Empty, CACustomsCodesResolver.MatchingCustomsCode(CACustomsCodeType.Office, "BRETT", ZString.Empty, Factory));
		}

		public void TestSingleSubLocationMatch()
		{
			var loco = Factory.New<RefUNLOCO>();
			loco.RL_Code = "TEST";
			var match = loco.RefLocoMaps.AddNew();
			match.RY_RN = Core.Constants.CountryGuids.Canada;
			match.RY_LocalPortCode = "LOCAL";
			match.RY_SystemUsage = CALocoMapSystemUsageList.Codes.Sub;
			var noMatch = loco.RefLocoMaps.AddNew();
			noMatch.RY_RN = Core.Constants.CountryGuids.Canada;
			noMatch.RY_SystemUsage = CALocoMapSystemUsageList.Codes.Air;

			var matches = CACustomsCodesResolver.GetMatchesForCodeType(CACustomsCodeType.SubLocation, "TEST", ZString.Empty, Factory);
			AssertEquals(1, matches.Count);
			AssertEquals(match, matches[0]);
			AssertEquals("LOCAL", CACustomsCodesResolver.MatchingCustomsCode(CACustomsCodeType.SubLocation, "TEST", ZString.Empty, Factory));
		}

		void CreateUNLoco(string locoCode)
		{
			var testUSLoco = Factory.NewWithValidTestData<RefUNLOCO>();
			testUSLoco.RL_Code = locoCode;
			testUSLoco.RL_PortName = "TEST Port - " + locoCode;
			testUSLoco.RL_IsSystem = true;
			testUSLoco.RL_HasAirport = true;
			testUSLoco.RL_HasSeaport = true;
			testUSLoco.RL_RN_NKCountryCode = Core.Constants.CountryCodes.Canada;
		}

		void CreateLocoMap(string localPort, string unLoco, string usage, bool isSystem)
		{
			var locoMap = Factory.NewWithValidTestData<RefLocoMap>();
			locoMap.RY_LocalPortCode = localPort;
			locoMap.RY_RL_NKLocoPort = unLoco;
			locoMap.RY_SystemUsage = usage;
			locoMap.RY_RN = Core.Constants.CountryGuids.Canada;
			locoMap.RY_IsSystem = isSystem;
		}
	}
}
