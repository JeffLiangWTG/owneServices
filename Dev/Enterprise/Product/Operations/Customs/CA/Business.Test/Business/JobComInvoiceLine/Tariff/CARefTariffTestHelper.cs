using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;

namespace Enterprise.Customs.CA.Business.Testing
{
	public static class CARefTariffTestHelper
	{
		public static ZString AnIncludedCodeForTesting(BusinessObjectFactory factory, ZString pgaTypeCode, string tariffCode = null, string programCode = "ALL")
		{
			return CreateOrGetExistingTariff4Testing(factory, pgaTypeCode, tariffCode, programCode).ZZ1_TariffCode;
		}

		public static TariffView CreateOrGetExistingTariff4Testing(BusinessObjectFactory factory, ZString pgaTypeCode, string tariffCode = null, string programCode = "ALL")
		{
			var refDataHelper = new UniversalReferenceTestDataHelper(factory);
			var tariffType = refDataHelper.CreateNewOrGetExistingTariffType("CA", "HSN");
			factory.Save();
			if (tariffCode == null)
			{
				tariffCode = "1234567890";
			}
			var tariff = refDataHelper.LoadOrCreateNewTariff("CA", tariffType.PK, tariffCode, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime, "Aggressive dog is dangerous");
			var conditionType = refDataHelper.CreateOrGetExistingRefCusConditionType("CA", "CTRL", "PGA");
			var condition = refDataHelper.CreateOrGetExistingRefCusCondition("CA", conditionType.PK, tariff.PK, null, false, false, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime, isTariff: true);
			var conditionValueType = refDataHelper.CreateOrGetExistingRefCusConditionValueType("CA", pgaTypeCode);
			refDataHelper.CreateOrGetExistingRefCusConditionValue(conditionValueType.PK, condition.PK, programCode);
			return tariff;
		}
	}
}
