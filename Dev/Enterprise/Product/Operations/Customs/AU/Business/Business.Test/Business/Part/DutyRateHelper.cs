using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	static class DutyRateHelper
	{
		public static void CreateTestTariffRatePeriodSnapshot(BusinessObjectFactory factory)
		{
			CreateTestTariffRatePeriodSnapshot(factory, AUAddInfo.GeneralPreferenceRate, 5, "CALC");
		}

		public static void CreateTestTariffRatePeriodSnapshot(BusinessObjectFactory factory, string preference, ZDecimal customsValueRate, string calculationType)
		{
			var tariffRatePeriodSnapshot = factory.New<CMRTariffRatePeriodSnapshot>();
			tariffRatePeriodSnapshot.TT_PreferenceSchemeType = preference;
			tariffRatePeriodSnapshot.TT_TariffClassificationNumber = "42010000";
			tariffRatePeriodSnapshot.TT_RateNumber = "001";
			tariffRatePeriodSnapshot.TT_CalculationType = calculationType;
			tariffRatePeriodSnapshot.TT_CustomsValueRate = customsValueRate;
			tariffRatePeriodSnapshot.TT_StartDate = ZDateTime.Now.AddDays(-1);
			tariffRatePeriodSnapshot.TT_EndDate = ZDateTime.Now.AddDays(1);
		}
	}
}
