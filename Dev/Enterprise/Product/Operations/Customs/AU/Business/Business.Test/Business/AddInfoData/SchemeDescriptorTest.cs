using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class SchemeDescriptorTest : TestCaseWithFactory
	{
		public void TestGetDescriptionIncludingDutyRate()
		{
			var testTariffRate = CMRTariffRatePeriodSnapshot.New(Factory);
			testTariffRate.TT_TariffClassificationNumber = "00000000";
			testTariffRate.TT_PreferenceSchemeType = "GEN";
			testTariffRate.TT_CalculationType = "CALC";
			testTariffRate.TT_CustomsValueRate = 5m;
			testTariffRate.TT_QuantityRate = 1.25m;
			testTariffRate.TT_QuantityUnit = "KG";
			AssertEquals("Description including Duty rate", "General(" + testTariffRate.GetDutyRateDescription() + ")", SchemeDescriptor.GetDescriptionIncludingDutyRate(new CMRSchemeList(), testTariffRate));
		}

		[NUnit.Framework.ExpectNoExceptions()]
		public void TestNullDescription()
		{
			var testTariffRate = CMRTariffRatePeriodSnapshot.New(Factory);
			testTariffRate.TT_TariffClassificationNumber = "00000000";
			testTariffRate.TT_PreferenceSchemeType = "";
			AssertNull("PreCondition: there is no description for an empty code", new CMRSchemeList().GetDescriptionFromCode(""));
			SchemeDescriptor.GetDescriptionIncludingDutyRate(new CMRSchemeList(), testTariffRate);
		}
	}
}
