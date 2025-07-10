using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;
using static Enterprise.Core.Constants.Customs.Universal.RefDataGrouping;
using static Enterprise.Customs.Universal.Constants;

namespace Enterprise.ComplianceRisk.Business.Test
{
	public static class ComplianceRiskTariffTestDataHelper
	{
		public static TariffView CreateTariffWithConditions(BusinessObjectFactory factory, string tariffCode, string tariffConditions)
		{
			var dataGroupingCode = Codes.WorldCustomsOrganisationWCO;
			var refDataHelper = new UniversalReferenceTestDataHelper(factory);
			var cusTariffType = refDataHelper.CreateNewOrGetExistingTariffType(dataGroupingCode, TariffTypes.HarmonizedSystem);
			factory.Save();

			var tariff = refDataHelper.CreateTariff(dataGroupingCode, cusTariffType.PK, tariffCode, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			var conditionType = refDataHelper.CreateOrGetExistingRefCusConditionType(dataGroupingCode, "RISK", "TEST");
			var conditionValueType = refDataHelper.CreateOrGetExistingRefCusConditionValueType(dataGroupingCode, "CVT");
			var refCusCondition = refDataHelper.CreateOrGetExistingRefCusCondition(dataGroupingCode, conditionType.PK, tariff.PK, $"comment of tariff {tariff.ZZ1_TariffCode}", isImport: true, isExport: true, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);

			refDataHelper.CreateOrGetExistingRefCusConditionValue(conditionValueType.PK, refCusCondition.PK, tariffConditions);
			factory.Save();

			return tariff;
		}

		public static TariffView CreateNewOrLoadTariff(BusinessObjectFactory factory, string tariffCode)
		{
			var dataGroupingCode = Codes.WorldCustomsOrganisationWCO;
			var refDataHelper = new UniversalReferenceTestDataHelper(factory);
			var tariffType = refDataHelper.CreateNewOrGetExistingTariffType(dataGroupingCode, TariffTypes.HarmonizedSystem);
			factory.Save();

			var tariff = refDataHelper.CreateTariff(dataGroupingCode, tariffType.PK, tariffCode, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			factory.Save();

			return tariff;
		}
	}
}
