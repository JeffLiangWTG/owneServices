using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Universal;

namespace Enterprise.Customs.KR.Business.Testing
{
	public class TestRefConditionSetupHelper
	{
		readonly BusinessObjectFactory factory;

		public TestRefConditionSetupHelper(BusinessObjectFactory factory)
		{
			this.factory = factory;
		}

		public RefCusTradeGroup GenerateTradeGroup(ZString group, ZDateTime startDate, ZDateTime endDate)
		{
			var refCusTradeGroup = factory.New<RefCusTradeGroup>();
			refCusTradeGroup.ZZA_StartDate = startDate;
			refCusTradeGroup.ZZA_EndDate = endDate;
			refCusTradeGroup.ZZA_TradeGroup = group;
			refCusTradeGroup.ZZA_ZZZ_NKDataGrouping = "KR";
			refCusTradeGroup.ZZA_Description = group;
			return refCusTradeGroup;
		}

		public void GenerateTradeGroupCountry(RefCusTradeGroup refCusTradeGroup, ZString country, ZDate startDate, ZDate endDate)
		{
			var refCusTradeGroupCountry = factory.New<RefCusTradeGroupCountry>();
			refCusTradeGroupCountry.ZZB_ZZA_TradeGroup = refCusTradeGroup.PK;
			refCusTradeGroupCountry.ZZB_RN_NKTradeGroupCountryCode = country;
			refCusTradeGroupCountry.ZZB_StartDate = startDate;
			refCusTradeGroupCountry.ZZB_EndDate = endDate;
			refCusTradeGroupCountry.ZZB_Description = country;
		}

		public RefCusConditionType GenerateSPARefCusConditionType()
		{
			var refCusConditionType = factory.New<RefCusConditionType>();
			refCusConditionType.ZX2_ConditionType = Messaging.Constants.ZZ.RefCusConditionType.SteelProduct;
			refCusConditionType.ZX2_ConditionClass = "CTRL";
			refCusConditionType.ZX2_Description = "Export Control of steel or its products";
			refCusConditionType.ZX2_ZZZ_NKDataGrouping = Core.Constants.CountryCodes.KoreaSouth;

			return refCusConditionType;
		}

		public RefCusConditionValueType GenerateSPARefCusConditionValueType()
		{
			var refCusConditionValueType = factory.New<RefCusConditionValueType>();
			refCusConditionValueType.ZX4_ValueType = Messaging.Constants.ZZ.RefCusConditionValueType.TradeGroup;
			refCusConditionValueType.ZX4_Description = "Trade Group";
			refCusConditionValueType.ZX4_IsFormula = false;
			refCusConditionValueType.ZX4_ZZZ_NKDataGrouping = Core.Constants.CountryCodes.KoreaSouth;

			return refCusConditionValueType;
		}

		public RefCusNomenclatureGroup GenerateNomenclature(ZString nomenclatureValue, ZString compositeKey, ZDateTime startDate, ZDateTime endDate)
		{
			var nomenclature = factory.New<RefCusNomenclatureGroup>();
			nomenclature.ZZ5_Value = nomenclatureValue;
			nomenclature.ZZ5_StartDate = startDate;
			nomenclature.ZZ5_EndDate = endDate;
			nomenclature.ZZ5_Description = "기타";
			nomenclature.ZZ5_CompositeKey = compositeKey;
			nomenclature.ZZ5_ZZZ_NKDataGrouping = Core.Constants.CountryCodes.KoreaSouth;

			return nomenclature;
		}

		public void SetCondition(RefCusNomenclatureGroup nomenclature, ZString destination, ZDateTime startDate, ZDateTime endDate)
		{
			var condition = factory.New<RefCusCondition>();
			condition.ZX1_ZZ5_Nomenclature = nomenclature.PK;
			condition.ZX1_ZX2_ConditionType = refCusConditionType.PK;
			condition.ZX1_StartDate = startDate;
			condition.ZX1_EndDate = endDate;
			condition.ZX1_IsExport = true;
			condition.ZX1_ZZZ_NKDataGrouping = Core.Constants.CountryCodes.KoreaSouth;

			var conditionValue = condition.ConditionValues.AddNew();
			conditionValue.ZX3_ZX4_ValueType = refCusConditionValueType.PK;
			conditionValue.ZX3_Value = destination;

			factory.Save();
		}

		public void Setup()
		{
			refCusConditionType = GenerateSPARefCusConditionType();
			refCusConditionValueType = GenerateSPARefCusConditionValueType();
		}

		public static void AddRefCusCondition(TariffView tariff, string regulationCodeValue, string documentNameValue, RefCusConditionType conditionType, RefCusConditionValueType regulationNumber, RefCusConditionValueType documentName, bool isExport, bool isImport)
		{
			var condition = tariff.Conditions.AddNew();
			condition.ZX1_ZZ1_Tariff = tariff.PK;
			condition.ZX1_StartDate = ZDateTime.Today.AddDays(-2);
			condition.ZX1_EndDate = ZDateTime.Today.AddDays(1);
			condition.ZX1_ZZZ_NKDataGrouping = "KR";
			condition.ZX1_ZX2_ConditionType = conditionType.PK;
			condition.ZX1_IsExport = isExport;
			condition.ZX1_IsImport = isImport;

			if (!string.IsNullOrEmpty(regulationCodeValue))
			{
				var conditionValue = condition.ConditionValues.AddNew();
				conditionValue.ZX3_Value = regulationCodeValue;
				conditionValue.ZX3_ZX4_ValueType = regulationNumber.PK;
			}
			if (!string.IsNullOrEmpty(documentNameValue))
			{
				var conditionValue = condition.ConditionValues.AddNew();
				conditionValue.ZX3_Value = documentNameValue;
				conditionValue.ZX3_ZX4_ValueType = documentName.PK;
			}
		}

		public static void AddRefCusCondition(TariffView tariff, RefCusConditionType conditionType, bool isExport, bool isImport, RefCusConditionValueType conditionValueType, string conditionValueString)
		{
			var condition = tariff.Conditions.AddNew();
			condition.ZX1_ZZ1_Tariff = tariff.PK;
			condition.ZX1_StartDate = ZDateTime.Today.AddDays(-2);
			condition.ZX1_EndDate = ZDateTime.Today.AddDays(1);
			condition.ZX1_ZZZ_NKDataGrouping = "KR";
			condition.ZX1_ZX2_ConditionType = conditionType.PK;
			condition.ZX1_IsExport = isExport;
			condition.ZX1_IsImport = isImport;

			var conditionValue = condition.ConditionValues.AddNew();
			conditionValue.ZX3_Value = conditionValueString;
			conditionValue.ZX3_ZX4_ValueType = conditionValueType.PK;
		}

		public static void AddRefCusCondition(TariffView tariff, string regulationCodeValue, string documentNameValue, RefCusConditionType conditionType, RefCusConditionValueType regulationNumber, RefCusConditionValueType documentName, bool isExport, bool isImport, ZDateTime startDate, ZDateTime endDate)
		{
			var condition = tariff.Conditions.AddNew();
			condition.ZX1_ZZ1_Tariff = tariff.PK;
			condition.ZX1_StartDate = startDate;
			condition.ZX1_EndDate = endDate;
			condition.ZX1_ZZZ_NKDataGrouping = "KR";
			condition.ZX1_ZX2_ConditionType = conditionType.PK;
			condition.ZX1_IsExport = isExport;
			condition.ZX1_IsImport = isImport;

			if (!string.IsNullOrEmpty(regulationCodeValue))
			{
				var conditionValue = condition.ConditionValues.AddNew();
				conditionValue.ZX3_Value = regulationCodeValue;
				conditionValue.ZX3_ZX4_ValueType = regulationNumber.PK;
			}
			if (!string.IsNullOrEmpty(documentNameValue))
			{
				var conditionValue = condition.ConditionValues.AddNew();
				conditionValue.ZX3_Value = documentNameValue;
				conditionValue.ZX3_ZX4_ValueType = documentName.PK;
			}
		}

		RefCusConditionType refCusConditionType;
		RefCusConditionValueType refCusConditionValueType;
	}
}
