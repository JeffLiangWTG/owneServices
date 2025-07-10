using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.CA.Business.Testing
{
	[TestedType(typeof(CARefTariffDataLoader))]
	sealed class CARefTariffDataLoaderTest : TestCaseWithFactory
	{
		public void TestDoesTariffHasPGA()
		{
			var refDataHelper = new UniversalReferenceTestDataHelper(Factory);
			var tariffType = refDataHelper.CreateNewOrGetExistingTariffType("CA", "HSN");
			Factory.Save();
			var tariff = refDataHelper.CreateTariff("CA", tariffType.PK, "1231231230", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime, "Aggressive dog is dangerous");
			var conditionType = refDataHelper.CreateOrGetExistingRefCusConditionType("CA", "CTRL", "PGA1");
			var condition = refDataHelper.CreateOrGetExistingRefCusCondition("CA", conditionType.PK, tariff.PK, null, false, false, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime, isTariff: true);
			var conditionValueType = refDataHelper.CreateOrGetExistingRefCusConditionValueType("CA", "AA");
			refDataHelper.CreateOrGetExistingRefCusConditionValue(conditionValueType.PK, condition.PK, "11");
			Assert("No PGA there", !CARefTariffDataLoader.DoesTariffHasPGA(Factory, "1231231230", ZDateTime.Today));

			CARefTariffTestHelper.CreateOrGetExistingTariff4Testing(Factory, "ECCC", "4564564560");
			Assert("We got PGA", CARefTariffDataLoader.DoesTariffHasPGA(Factory, "4564564560", ZDateTime.Today));
		}

		public void TestDoesTariffHasPGAType()
		{
			CARefTariffTestHelper.CreateOrGetExistingTariff4Testing(Factory, "ECCC", "4564564560");
			Assert("No PGA type there", !CARefTariffDataLoader.DoesTariffHasPGAType(Factory, "4564564560", "HC", ZDateTime.Today));
			Assert("We got PGA type", CARefTariffDataLoader.DoesTariffHasPGAType(Factory, "4564564560", "ECCC", ZDateTime.Today));
		}

		public void TestDoesTariffHasPGAProgram()
		{
			CARefTariffTestHelper.CreateOrGetExistingTariff4Testing(Factory, "ECCC", "1112223330", "WEN");
			Assert("No PGA program there", !CARefTariffDataLoader.DoesTariffPGATypeHasProgram(Factory, "1112223330", "ECCC", "VEE", ZDateTime.Today));
			Assert("We got PGA program", CARefTariffDataLoader.DoesTariffPGATypeHasProgram(Factory, "1112223330", "ECCC", "WEN", ZDateTime.Today));
		}

		public void TestLoadAllProgramsOfPGAType()
		{
			var refDataHelper = new UniversalReferenceTestDataHelper(Factory);
			var tariffType = refDataHelper.CreateNewOrGetExistingTariffType("CA", "HSN");
			Factory.Save();
			var tariff = refDataHelper.CreateTariff("CA", tariffType.PK, "2223334440", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime, "Aggressive dog is dangerous");
			var conditionType = refDataHelper.CreateOrGetExistingRefCusConditionType("CA", "CTRL", "PGA");
			var condition = refDataHelper.CreateOrGetExistingRefCusCondition("CA", conditionType.PK, tariff.PK, null, false, false, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime, isTariff: true);
			var conditionValueType = refDataHelper.CreateOrGetExistingRefCusConditionValueType("CA", "ECCC");
			refDataHelper.CreateOrGetExistingRefCusConditionValue(conditionValueType.PK, condition.PK, "ALL");
			refDataHelper.CreateOrGetExistingRefCusConditionValue(conditionValueType.PK, condition.PK, "CTO");

			var programs = CARefTariffDataLoader.LoadAllProgramsOfPGAType(Factory, "2223334440", "HC", ZDateTime.Today);
			Assert("0 types", !programs.Any());
			programs = CARefTariffDataLoader.LoadAllProgramsOfPGAType(Factory, "2223334440", "ECCC", ZDateTime.Today);
			Assert("2 types", programs.Count == 2);
			Assert(programs.Contains("ALL") && programs.Contains("CTO"));
		}
	}
}
