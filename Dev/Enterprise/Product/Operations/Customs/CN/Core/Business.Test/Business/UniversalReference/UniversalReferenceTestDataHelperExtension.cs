using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.CN.Business.Testing
{
	public static class UniversalReferenceTestDataHelperExtension
	{
		public static TariffView LoadCustomsTariff(this UniversalReferenceTestDataHelper helper, BusinessObjectFactory factory, string tariffCode)
		{
			var query = new ZQuery(TariffViewSchema.ZZ1_ZZZ_NKDataGrouping, Core.Constants.CountryCodes.China);
			query.AddToFilter(TariffViewSchema.ZZ1_TariffCode, tariffCode);

			var subQuery = new ZDBOnlyQuery(typeof(TariffView));
			var typeQuery = new ZDBOnlySubQuery(typeof(RefCusTariffType), RefCusTariffTypeSchema.PK);
			typeQuery.AddToFilter(RefCusTariffTypeSchema.ZZI_TariffType, Universal.Constants.TariffTypes.HarmonizedSystem);
			subQuery.AddSubQuery(TariffViewSchema.ZZ1_ZZI_TariffType, typeQuery, JoinCondition.And);
			query.AddToFilter(subQuery);

			return factory.LoadTop1<TariffView>(query);
		}

		public static TariffView CreateCustomsTariff(this UniversalReferenceTestDataHelper helper, string tariffCode, params string[] additionalElements)
		{
			var cusTariffType = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.China, Universal.Constants.TariffTypes.HarmonizedSystem);
			cusTariffType.Factory.Save();

			var tariff = helper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.China, cusTariffType.PK, tariffCode, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			tariff.Factory.Save();

			if (additionalElements != null)
			{
				for (int i = 0; i < additionalElements.Length; i++)
				{
					var attributeName = Constants.UniversalReferenceConstants.CusTariffAttributeName.AdditionalInfomation + (i + 1).ToString().PadLeft(2, '0');
					if (!tariff.HasAttribute(attributeName))
					{
						helper.CreateTariffAttribute(attributeName, additionalElements[i], tariff);
					}
				}
			}
			return tariff;
		}

		public static TariffView CreateCIQTariff(this UniversalReferenceTestDataHelper helper, string ciqTariffCode)
		{
			var cusTariffType = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.China, Universal.Constants.TariffTypes.HarmonizedSystem);
			var ciqTariffType = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.China, Constants.UniversalReferenceConstants.CusTariffTypes.ChinaCIQTariff);
			ciqTariffType.Factory.Save();

			var tariffCode = ciqTariffCode.Substring(0, 10);
			var ciqTariff = helper.CreateCustomsTariff(tariffCode);
			helper.CreateTariffRelationship(helper.LoadOrCreateNewTariff("CN", ciqTariffType.PK, ciqTariffCode, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue).PK, cusTariffType.PK, tariffCode);

			return ciqTariff;
		}

		public static void CreateSingleValueConditionForTariff(this UniversalReferenceTestDataHelper helper, ZGuid tariffPK, ZBool isImport, ZBool isExport, ZGuid conditionTypePK, ZString conditionComment, ZGuid conditionValueTypePK, ZString conditionValueValue, ZGuid? preferencePK = null, bool trueMeansStop = false)
		{
			var cond = helper.CreateOrGetExistingRefCusCondition(Core.Constants.CountryCodes.China, conditionTypePK, tariffPK, conditionComment, isImport, isExport, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			if (preferencePK.HasValue)
			{
				cond.ZX1_ZZS_Preference = preferencePK.Value;
			}

			cond.ZX1_ConditionValueTrueMeansStop = trueMeansStop;
			cond.Factory.Save();
			if (!trueMeansStop)
			{
				helper.CreateOrGetExistingRefCusConditionValue(conditionValueTypePK, cond.PK, conditionValueValue);
			}
		}

		public static RefCusCodeList CreateAdditionalElement(this UniversalReferenceTestDataHelper helper, string code, string name)
		{
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CNAdditionalElements, "China Customs Tariff Additional Elements");
			return helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.China, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CNAdditionalElements, code, name, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
		}
	}
}
