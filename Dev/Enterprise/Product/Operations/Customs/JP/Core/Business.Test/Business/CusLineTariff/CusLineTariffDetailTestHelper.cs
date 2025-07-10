using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;

namespace Enterprise.Customs.JP.Business.Testing
{
	public class CusLineTariffDetailTestHelper
	{
		public TariffView PrepareTestData(BusinessObjectFactory factory)
		{
			var helper = new UniversalReferenceTestDataHelper(factory);
			var startDate = ZDateTime.Today.AddDays(-1);
			var endDate = ZDateTime.Today.AddDays(1);
			var rateType = helper.CreateNewOrGetExistingRateType("JP", "DTY", "Duty");
			var rateCode = helper.LoadOrCreateNewCusRateCode(factory, "DTY", rateType.PK);
			var tarriffTypeL = helper.CreateNewOrGetExistingTariffType("JP", "L");
			factory.Save();

			var tariff = helper.LoadOrCreateNewTariff("JP", tarriffTypeL.PK, "L121040", startDate, endDate);
			helper.CreateRate(tariff, rateCode.PK, startDate, endDate, "0.15 * VFD", null, "15%", "JP");
			helper.CreateNewOrGetExistingCusCodeList("JP", "CTEI", "E01", startDate, endDate);
			factory.Save();

			return tariff;
		}

		public void PrepareTestDataForTariffTypeList(BusinessObjectFactory factory, ZString typeCode, ZString tariffCode, ZString code)
		{
			var helper = new UniversalReferenceTestDataHelper(factory);
			var startDate = ZDateTime.Today.AddDays(-1);
			var endDate = ZDateTime.Today.AddDays(1);
			var rateType = helper.CreateNewOrGetExistingRateType("JP", "DTY", "Duty");
			var rateCode = helper.LoadOrCreateNewCusRateCode(factory, "DTY", rateType.PK);
			var tarriffType = helper.CreateNewOrGetExistingTariffType("JP", typeCode);
			factory.Save();

			var tariff = helper.LoadOrCreateNewTariff("JP", tarriffType.PK, tariffCode, startDate, endDate);
			helper.CreateRate(tariff, rateCode.PK, startDate, endDate, "0.15 * VFD", null, "15%", "JP");
			helper.CreateNewOrGetExistingCusCodeList("JP", "CTEI", code, startDate, endDate);
			factory.Save();
		}
	}
}
