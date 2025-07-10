using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.EU.NCTS.Messaging;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.EU.NCTS.Business.Testing
{
	class SecurityTraderCountryGroupHelperTest : TestCaseWithFactory
	{
		public void TestSecurityTraderCountryGroup()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingDataGrouping("EUN");
			var tradeGroup = helper.LoadOrCreateTradeGroup("EUN", "EUC", new ZDateTime(2019, 1, 1), new ZDateTime(2040, 12, 31));
			helper.AddCountry(tradeGroup, Core.Constants.CountryCodes.France, new ZDate(2019, 1, 1), new ZDate(2040, 12, 31));
			helper.AddCountry(tradeGroup, Core.Constants.CountryCodes.UnitedKingdom, new ZDate(2019, 1, 1), new ZDate(2020, 12, 31));

			var tradeGroupCUAM = helper.CreateTradeGroup("EUN", "EUCTP", new ZDateTime(2019, 1, 1), new ZDateTime(2040, 12, 31));
			helper.AddCountry(tradeGroupCUAM, Core.Constants.CountryCodes.UnitedKingdom, new ZDate(2019, 1, 1), new ZDate(2040, 12, 31));

			var tradeGroupEUSEC = helper.CreateTradeGroup("EUN", "EUSEC", new ZDateTime(2019, 1, 1), new ZDateTime(2040, 12, 31));
			helper.AddCountry(tradeGroupEUSEC, Core.Constants.CountryCodes.Germany, new ZDate(2019, 1, 1), new ZDate(2040, 12, 31));

			var belfast = new RefUNLOCO.Loader(Factory).Load("GBBEL");
			if (belfast.CountryStates == null || string.Compare(belfast.CountryStates.RW_RegionName, "NORTHERN IRELAND", true) != 0)
			{
				var ni = Factory.New<RefCountryStates>();
				belfast.RL_RW = ni.PK;
				ni.RW_RegionName = "nORtHeRn IRelAnd";
			}

			var jobDocAddress = Factory.New<JobDocAddress>();
			var orgAddress = Factory.New<OrgAddress>();
			jobDocAddress.E2_OA_Address = orgAddress.PK;

			var orgHeader = Factory.New<OrgHeader>();
			orgAddress.OA_RN_NKCountryCode = Core.Constants.CountryCodes.France;
			orgAddress.OA_OH = orgHeader.PK;

			AssertEquals("When EU country, should return EuForSafetyAndSecurity", SecurityTraderCountryGroup.EuForSafetyAndSecurity, SecurityTraderCountryGroupHelper.GetSecurityTraderCountryGroup(jobDocAddress));

			orgAddress.OA_RN_NKCountryCode = Core.Constants.CountryCodes.UnitedKingdom;
			orgAddress.OA_RL_NKRelatedPortCode = ZString.Empty;
			orgAddress.Header.OH_RL_NKClosestPort = "GBBEL";
			AssertEquals("When closest header port is NI, should return NorthernIreland", SecurityTraderCountryGroup.NorthernIreland, SecurityTraderCountryGroupHelper.GetSecurityTraderCountryGroup(jobDocAddress));

			orgAddress.OA_RL_NKRelatedPortCode = "GBBEL";
			orgAddress.Header.OH_RL_NKClosestPort = ZString.Empty;
			AssertEquals("When related port code is NI, should return NorthernIreland", SecurityTraderCountryGroup.NorthernIreland, SecurityTraderCountryGroupHelper.GetSecurityTraderCountryGroup(jobDocAddress));

			orgAddress.OA_RN_NKCountryCode = Core.Constants.CountryCodes.France;
			AssertEquals("When EU country, should return EuForSafetyAndSecurity", SecurityTraderCountryGroup.EuForSafetyAndSecurity, SecurityTraderCountryGroupHelper.GetSecurityTraderCountryGroup(jobDocAddress));

			orgAddress.OA_RN_NKCountryCode = Core.Constants.CountryCodes.Germany;
			AssertEquals("When EUSEC country, should return EuForSafetyAndSecurity", SecurityTraderCountryGroup.EuForSafetyAndSecurity, SecurityTraderCountryGroupHelper.GetSecurityTraderCountryGroup(jobDocAddress));

			orgAddress.OA_RN_NKCountryCode = Core.Constants.CountryCodes.UnitedKingdom;
			orgAddress.OA_RL_NKRelatedPortCode = "GBLHR";
			orgAddress.Header.OH_RL_NKClosestPort = "GBLHR";
			AssertEquals("When not EU but is an NCTS contracting country, should return NotEU", SecurityTraderCountryGroup.NotEU, SecurityTraderCountryGroupHelper.GetSecurityTraderCountryGroup(jobDocAddress));

			orgAddress.Header.OH_RL_NKClosestPort = ZString.Empty;
			orgAddress.OA_RL_NKRelatedPortCode = ZString.Empty;
			AssertEquals("When not EU but is an NCTS contracting country, should return NotEU", SecurityTraderCountryGroup.NotEU, SecurityTraderCountryGroupHelper.GetSecurityTraderCountryGroup(jobDocAddress));

			orgAddress.OA_RN_NKCountryCode = Core.Constants.CountryCodes.UnitedStates;
			orgAddress.OA_RL_NKRelatedPortCode = "USMIA";
			orgAddress.Header.OH_RL_NKClosestPort = "USMIA";
			AssertEquals("When not EU and not an NCTS contracting country, should return None", SecurityTraderCountryGroup.None, SecurityTraderCountryGroupHelper.GetSecurityTraderCountryGroup(jobDocAddress));

			jobDocAddress = null;
			AssertEquals("When no job doc address has been set, should return None", SecurityTraderCountryGroup.None, SecurityTraderCountryGroupHelper.GetSecurityTraderCountryGroup(jobDocAddress));
		}
	}
}
