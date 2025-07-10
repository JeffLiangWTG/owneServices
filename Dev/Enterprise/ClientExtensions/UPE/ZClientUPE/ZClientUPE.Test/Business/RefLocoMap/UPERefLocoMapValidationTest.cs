using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Client.UPE.Business.RefLocoMap.Testing
{
	class UPERefLocoMapValidationTest : BusinessObjectValidationTestCase
	{
		public void TestDuplicateSystemUsage_US()
		{
			unloco.RL_Code = "US~~~";

			var locoMap1 = Factory.New<UPERefLocoMap>();
			locoMap1.RY_RL_NKLocoPort = unloco.RL_Code;
			locoMap1.RY_RN = Core.Constants.CountryGuids.UnitedStates;
			locoMap1.RY_SystemUsage = "A";

			var locoMap2 = Factory.New<UPERefLocoMap>();
			locoMap2.RY_RL_NKLocoPort = unloco.RL_Code;
			locoMap2.RY_RN = Core.Constants.CountryGuids.UnitedStates;
			locoMap2.RY_SystemUsage = "A";
			AssertNoError("US can have multiple of the same schedule type for one port", locoMap2.RY_SystemUsageInfo, RefLocoMapValidation.DuplicateCountryAndSystemUsageCombinations);
			AssertHasWarning("Give US codes a warning only", locoMap2.RY_SystemUsageInfo, RefLocoMapValidation.DuplicateCountryAndSystemUsageCombinations);

			locoMap2.RY_SystemUsage = "B";
			AssertNoError(locoMap2.RY_SystemUsageInfo, RefLocoMapValidation.DuplicateCountryAndSystemUsageCombinations);
		}

		public void TestDuplicateSystemUsage_AU()
		{
			unloco.RL_Code = "AU~~~";

			var locoMap1 = Factory.New<UPERefLocoMap>();
			locoMap1.RY_RL_NKLocoPort = unloco.RL_Code;
			locoMap1.RY_RN = Core.Constants.CountryGuids.Australia;
			locoMap1.RY_SystemUsage = "A";

			var locoMap2 = Factory.New<UPERefLocoMap>();
			locoMap2.RY_RL_NKLocoPort = unloco.RL_Code;
			locoMap2.RY_RN = Core.Constants.CountryGuids.Australia;
			locoMap2.RY_SystemUsage = "A";
			AssertHasError(locoMap2.RY_SystemUsageInfo, RefLocoMapValidation.DuplicateCountryAndSystemUsageCombinations);
			AssertNoWarning(locoMap2.RY_SystemUsageInfo, RefLocoMapValidation.DuplicateCountryAndSystemUsageCombinations);

			locoMap2.RY_SystemUsage = "B";
			AssertNoError(locoMap2.RY_SystemUsageInfo, RefLocoMapValidation.DuplicateCountryAndSystemUsageCombinations);
			AssertNoWarning(locoMap2.RY_SystemUsageInfo, RefLocoMapValidation.DuplicateCountryAndSystemUsageCombinations);

			locoMap1.RY_SystemUsage = UPEOtherLocoMapSystemUsageList.Codes.Ups;
			locoMap2.RY_SystemUsage = UPEOtherLocoMapSystemUsageList.Codes.Ups;
			AssertNoErrors(locoMap2.RY_SystemUsageInfo);
			AssertHasWarning(locoMap2.RY_SystemUsageInfo, RefLocoMapValidation.DuplicateCountryAndSystemUsageCombinations);

			locoMap2.RY_SystemUsage = UPEAirSeaMailSystemUsageList.Codes.Air;
			AssertNoErrors(locoMap2.RY_SystemUsageInfo);
			AssertNoWarning(locoMap2.RY_SystemUsageInfo, RefLocoMapValidation.DuplicateCountryAndSystemUsageCombinations);
		}

		public void TestDuplicateSystemUsage_SG()
		{
			unloco.RL_Code = "SG~~~";

			var locoMap1 = Factory.New<UPERefLocoMap>();
			locoMap1.RY_RL_NKLocoPort = unloco.RL_Code;
			locoMap1.RY_RN = Core.Constants.CountryGuids.Singapore;
			locoMap1.RY_SystemUsage = SGLocoMapSystemUsageList.Codes.CustomsPortCodeList;

			var locoMap2 = Factory.New<UPERefLocoMap>();
			locoMap2.RY_RL_NKLocoPort = unloco.RL_Code;
			locoMap2.RY_RN = Core.Constants.CountryGuids.Singapore;
			locoMap2.RY_SystemUsage = SGLocoMapSystemUsageList.Codes.CustomsPortCodeList;
			AssertHasError(locoMap2.RY_SystemUsageInfo, RefLocoMapValidation.DuplicateCountryAndSystemUsageCombinations);
			AssertNoWarning(locoMap2.RY_SystemUsageInfo, RefLocoMapValidation.DuplicateCountryAndSystemUsageCombinations);

			locoMap1.RY_SystemUsage = UPEOtherLocoMapSystemUsageList.Codes.Ups;
			locoMap2.RY_SystemUsage = UPEOtherLocoMapSystemUsageList.Codes.Ups;
			AssertNoErrors(locoMap2.RY_SystemUsageInfo);
			AssertHasWarning(locoMap2.RY_SystemUsageInfo, RefLocoMapValidation.DuplicateCountryAndSystemUsageCombinations);

			locoMap2.RY_SystemUsage = SGLocoMapSystemUsageList.Codes.CustomsPortCodeList;
			AssertNoErrors(locoMap2.RY_SystemUsageInfo);
			AssertNoWarning(locoMap2.RY_SystemUsageInfo, RefLocoMapValidation.DuplicateCountryAndSystemUsageCombinations);
		}

		protected override void SetUp()
		{
			UPEDataRegistry.Instance.EnableUPECustomisations = true;
			unloco = Factory.New<RefUNLOCO>();
			base.SetUp();
		}
		RefUNLOCO unloco;
	}
}
