using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.EU.Business.Testing;

namespace Enterprise.Customs.GB.Business.Declaration.Testing
{
	class MasterUCRHelperTest : TestCaseWithFactory
	{
		public void TestGetMatchedCode()
		{
			var masterUCR = "HABC12512345678";
			var matches = MasterUCRHelper.PreparePatternMatch(masterUCR);
			AssertEquals(1, matches.Count);

			masterUCR = "HABC1251234567899999999";
			matches = MasterUCRHelper.PreparePatternMatch(masterUCR);
			AssertEquals(1, matches.Count);
			AssertEquals("H", MasterUCRHelper.GetMatchedCode(matches, "ACP"));
			AssertEquals("ABC", MasterUCRHelper.GetMatchedCode(matches, "SHED"));
			AssertEquals("12512345678", MasterUCRHelper.GetMatchedCode(matches, "MAWB"));
			AssertEquals("99999999", MasterUCRHelper.GetMatchedCode(matches, "HAWB"));
		}

		public void TestFindMawbFromPatternMatch()
		{
			var mawb = Factory.New<Ccsuk.AirCargoInventory.BusinessObjects.CusMAWB>();
			mawb.CM_MAWB = "125-12345678";
			mawb.CargoTerminalOperatorAirport = "LON";
			mawb.CargoTerminalOperator = "ABC";
			mawb.CM_SystemCreateTimeUtc = ZDateTime.Now.AddDays(-1);

			var masterUCR = "TABC12512345678";
			var matches = MasterUCRHelper.PreparePatternMatch(masterUCR);
			var result = MasterUCRHelper.FindMawbFromPatternMatch(matches, Factory);
			AssertNull(result);

			masterUCR = "HDEF12512345678";
			matches = MasterUCRHelper.PreparePatternMatch(masterUCR);
			result = MasterUCRHelper.FindMawbFromPatternMatch(matches, Factory);
			AssertNull(result);

			masterUCR = "HABC12512345678";
			matches = MasterUCRHelper.PreparePatternMatch(masterUCR);
			result = MasterUCRHelper.FindMawbFromPatternMatch(matches, Factory);
			AssertNull(result);

			mawb.CargoTerminalOperatorAirport = "LHR";
			var mawb2 = Factory.New<Ccsuk.AirCargoInventory.BusinessObjects.CusMAWB>();
			mawb2.CM_MAWB = "125-12345678";
			mawb2.CargoTerminalOperatorAirport = "LHR";
			mawb2.CargoTerminalOperator = "ABC";
			mawb2.CM_SystemCreateTimeUtc = ZDateTime.Now.AddDays(-2);

			masterUCR = "HABC12512345678";
			matches = MasterUCRHelper.PreparePatternMatch(masterUCR);
			result = MasterUCRHelper.FindMawbFromPatternMatch(matches, Factory);
			AssertEquals(mawb2.PK, result.PK);
		}

		public void TestFindMawbFromPatternMatch_OnlySelectIsActive()
		{
			var inactiveMawb = Factory.New<Ccsuk.AirCargoInventory.BusinessObjects.CusMAWB>();
			inactiveMawb.CM_MAWB = "125-12345678";
			inactiveMawb.CargoTerminalOperatorAirport = "LHR";
			inactiveMawb.CargoTerminalOperator = "ABC";
			inactiveMawb.CM_SystemCreateTimeUtc = ZDateTime.Now.AddDays(-2);
			inactiveMawb.CM_IsActive = false;

			var activeMawb = Factory.New<Ccsuk.AirCargoInventory.BusinessObjects.CusMAWB>();
			activeMawb.CM_MAWB = "125-12345678";
			activeMawb.CargoTerminalOperatorAirport = "LHR";
			activeMawb.CargoTerminalOperator = "ABC";
			activeMawb.CM_SystemCreateTimeUtc = ZDateTime.Now.AddDays(-2);

			var masterUCR = "HABC12512345678";
			var matches = MasterUCRHelper.PreparePatternMatch(masterUCR);
			var result = MasterUCRHelper.FindMawbFromPatternMatch(matches, Factory);
			AssertEquals("only select active mawbs", activeMawb.PK, result.PK);
		}

		public void TestFindHawbFromPattern()
		{
			var mawb = Factory.New<Ccsuk.AirCargoInventory.BusinessObjects.CusMAWB>();
			mawb.CM_MAWB = "125-12345678";
			mawb.CargoTerminalOperatorAirport = "LHR";
			mawb.CargoTerminalOperator = "ABC";

			var hawb = mawb.ChildBills.AddNew();
			hawb.CS_HAWB = "99999999";
			hawb.CS_SystemCreateTimeUtc = ZDateTime.Now.AddDays(-1);
			var hawb2 = mawb.ChildBills.AddNew();
			hawb2.CS_HAWB = "99999999";
			hawb2.CS_SystemCreateTimeUtc = ZDateTime.Now.AddDays(-2);

			var result = MasterUCRHelper.FindHawbFromPattern("00000000", mawb.PK, Factory);
			AssertNull(result);

			result = MasterUCRHelper.FindHawbFromPattern("99999999", mawb.PK, Factory);
			AssertEquals(hawb2.PK, result.PK);
		}

		public void TestFindHawbFromPattern_OnlySelectIsActive()
		{
			var mawb = Factory.New<Ccsuk.AirCargoInventory.BusinessObjects.CusMAWB>();
			mawb.CM_MAWB = "125-12345678";
			mawb.CargoTerminalOperatorAirport = "LHR";
			mawb.CargoTerminalOperator = "ABC";

			var inactiveHawb = mawb.ChildBills.AddNew();
			inactiveHawb.CS_HAWB = "99999999";
			inactiveHawb.CS_SystemCreateTimeUtc = ZDateTime.Now.AddDays(-2);
			inactiveHawb.CS_IsActive = false;

			var activeHawb = mawb.ChildBills.AddNew();
			activeHawb.CS_HAWB = "99999999";
			activeHawb.CS_SystemCreateTimeUtc = ZDateTime.Now.AddDays(-2);
			activeHawb.CS_IsActive = true;

			var result = MasterUCRHelper.FindHawbFromPattern("99999999", mawb.PK, Factory);
			AssertEquals("only select active hawbs", activeHawb.PK, result.PK);
		}

		protected override void SetUp()
		{
			base.SetUp();

			ShedTest.CreateShed(Factory, "GB", "LHRABC", "BRITISH AIRWAYS at Heathrow", acpCode: "H");
			Factory.Save();
		}
	}
}
