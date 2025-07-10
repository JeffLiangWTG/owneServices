using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.CN.Business.Testing
{
	class CNScheduleResolverTest : TestCaseWithFactory
	{
		public void TestMatchingUNLOCO()
		{
			var loco = Factory.LoadTop1<RefUNLOCO>(new ZQuery());
			var noMatch = loco.RefLocoMaps.AddNew();
			noMatch.RY_RN = Core.Constants.CountryGuids.China;
			noMatch.RY_SystemUsage = "OTH";
			AssertNotEquals(loco.RL_Code, CNScheduleResolver.GetMatchedUNLOCO("LOCAL", Factory));
			var match1 = loco.RefLocoMaps.AddNew();
			match1.RY_RN = Core.Constants.CountryGuids.China;
			match1.RY_LocalPortCode = "LOCAL";
			match1.RY_SystemUsage = CNLocoMapSystemUsageList.Codes.CustomsPortCodeList;
			AssertEquals(loco.RL_Code, CNScheduleResolver.GetMatchedUNLOCO("LOCAL", Factory));
			var match2 = loco.RefLocoMaps.AddNew();
			match2.RY_RN = Core.Constants.CountryGuids.China;
			match2.RY_LocalPortCode = "LOCAL";
			match2.RY_SystemUsage = CNLocoMapSystemUsageList.Codes.CustomsPortCodeList;
			AssertEquals(loco.RL_Code, CNScheduleResolver.GetMatchedUNLOCO("LOCAL", Factory));
			var loco1 = Factory.New<RefUNLOCO>();
			loco1.RL_Code = "TST1";
			var match3 = loco1.RefLocoMaps.AddNew();
			match3.RY_RN = Core.Constants.CountryGuids.China;
			match3.RY_LocalPortCode = "3154";
			match3.RY_SystemUsage = CNLocoMapSystemUsageList.Codes.CustomsPortCodeList;
			match3.RY_IsSystem = true;
			var loco2 = Factory.New<RefUNLOCO>();
			loco2.RL_Code = "TST2";
			var match4 = loco2.RefLocoMaps.AddNew();
			match4.RY_RN = Core.Constants.CountryGuids.China;
			match4.RY_LocalPortCode = "3154";
			match4.RY_SystemUsage = CNLocoMapSystemUsageList.Codes.CustomsPortCodeList;
			match4.RY_IsSystem = false;
			AssertEquals(loco2.RL_Code, CNScheduleResolver.GetMatchedUNLOCO("3154", Factory));
			match3.RY_IsSystem = false;
			match4.RY_IsSystem = true;
			AssertEquals(loco1.RL_Code, CNScheduleResolver.GetMatchedUNLOCO("3154", Factory));
		}

		public void TestMatchingLocoMap()
		{
			var loco = Factory.New<RefUNLOCO>();
			loco.RL_Code = "ULCO";
			var match = loco.RefLocoMaps.AddNew();
			match.RY_RN = Core.Constants.CountryGuids.China;
			match.RY_LocalPortCode = "LOCAL";
			match.RY_SystemUsage = CNLocoMapSystemUsageList.Codes.CustomsPortCodeList;
			var noMatch = loco.RefLocoMaps.AddNew();
			noMatch.RY_RN = Core.Constants.CountryGuids.China;
			noMatch.RY_SystemUsage = "STH";
			var locoMap = CNScheduleResolver.GetMatchedLocoMap("ULCO", Factory);
			AssertEquals(match, locoMap);
			var match3 = loco.RefLocoMaps.AddNew();
			match3.RY_RN = Core.Constants.CountryGuids.China;
			match3.RY_LocalPortCode = "3154";
			match3.RY_SystemUsage = CNLocoMapSystemUsageList.Codes.CustomsPortCodeList;
			match3.RY_IsSystem = true;
			var match4 = loco.RefLocoMaps.AddNew();
			match4.RY_RN = Core.Constants.CountryGuids.China;
			match4.RY_LocalPortCode = "3155";
			match4.RY_SystemUsage = CNLocoMapSystemUsageList.Codes.CustomsPortCodeList;
			match4.RY_IsSystem = false;
			AssertEquals(match4, CNScheduleResolver.GetMatchedLocoMap("ULCO", Factory));
			match3.RY_IsSystem = false;
			match4.RY_IsSystem = true;
			AssertEquals(match3, CNScheduleResolver.GetMatchedLocoMap("ULCO", Factory));
		}
	}
}
