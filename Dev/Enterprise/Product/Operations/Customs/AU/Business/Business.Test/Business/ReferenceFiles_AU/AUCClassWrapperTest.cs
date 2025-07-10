using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class AUCClassWrapperTest : TestCaseWithFactory
	{
		public void TestLoad()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var impTariffType = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.Australia, Enterprise.Customs.Universal.Constants.TariffTypes.Import);
			var tariff = helper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.Australia, impTariffType.PK, "2203009117", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, description: "Tariff Code Description", taxOrFeeCode: "GST");
			helper.CreateTariffUOM(tariff, Enterprise.Customs.Universal.Constants.UnitOfMeasureTypes.StatisticalUOMType, "LA");
			helper.CreateTariffUOM(tariff, Enterprise.Customs.Universal.Constants.UnitOfMeasureTypes.AdditionalUOMType, "L");

			var impTestDataTariffType = helper.CreateNewOrGetExistingTariffType(AUConstants.RefDataGroupCodes.AustraliaTest, Enterprise.Customs.Universal.Constants.TariffTypes.Import);
			var testTariff = helper.LoadOrCreateNewTariff(AUConstants.RefDataGroupCodes.AustraliaTest, impTestDataTariffType.PK, "2203009118", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, description: "Tariff Code Test Description", taxOrFeeCode: "GST");
			helper.CreateTariffUOM(testTariff, Enterprise.Customs.Universal.Constants.UnitOfMeasureTypes.StatisticalUOMType, "TestLA");
			helper.CreateTariffUOM(testTariff, Enterprise.Customs.Universal.Constants.UnitOfMeasureTypes.AdditionalUOMType, "TestL");

			helper.CreateNomenclatureGroupType("AU", "Australia");
			helper.CreateNomenclatureGroup("AU", "2203009", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, description: "Other:", nomenclatureGroupType: "AU", compositeKey: "04.22..03.00.9");
			helper.CreateNomenclatureGroup("AU", "2203009117", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, description: "Nomenclature Code Description", nomenclatureGroupType: "AU", compositeKey: "04.22..03.00.9.1.17");
			Factory.Save();

			using (AUCustomsDataRegistry.Instance.UseCustomsReferenceData.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, true))
			{
				using (AUCustomsDataRegistry.Instance.UseCMRTariffTestData.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, true))
				{
					var tesDataWrapper = AUCClassWrapper.Load(Factory, "2203.00.91 18", ZDateTime.Today);
					AssertNotNull(tesDataWrapper);
					CombineAssertions(() =>
					{
						AssertEquals("ZZ1_TariffCode", "2203009118", tesDataWrapper.ZZ1_TariffCode);
						AssertEquals("ZZ1_TariffCodeForDisplay", "2203.00.91 18", tesDataWrapper.ZZ1_TariffCodeForDisplay);
						AssertEquals("ZZ1_ZZ8_UQ1", "TestLA", tesDataWrapper.ZZ1_ZZ8_UQ1);
						AssertEquals("ZZ1_ZZ8_UQ2", "TestL", tesDataWrapper.ZZ1_ZZ8_UQ2);
						AssertStartsWith("ZZ1_Description", "TARIFF CODE TEST DESCRIPTION", tesDataWrapper.ZZ1_Description);
						AssertEquals("HasChildren", false, tesDataWrapper.HasChildren);
					});
				}

				using (AUCustomsDataRegistry.Instance.UseCMRTariffTestData.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, false))
				{
					var wrapper = AUCClassWrapper.Load(Factory, "2203.00.91 17", ZDateTime.Today);
					AssertNotNull(wrapper);
					CombineAssertions(() =>
					{
						AssertEquals("ZZ1_TariffCode", "2203009117", wrapper.ZZ1_TariffCode);
						AssertEquals("ZZ1_TariffCodeForDisplay", "2203.00.91 17", wrapper.ZZ1_TariffCodeForDisplay);
						AssertEquals("ZZ1_ZZ8_UQ1", "LA", wrapper.ZZ1_ZZ8_UQ1);
						AssertEquals("ZZ1_ZZ8_UQ2", "L", wrapper.ZZ1_ZZ8_UQ2);
						AssertStartsWith("ZZ1_Description", "TARIFF CODE DESCRIPTION", wrapper.ZZ1_Description);
						AssertEquals("HasChildren", false, wrapper.HasChildren);
					});
				}
			}
		}

		public void TestLoad_AUCClass()
		{
			var beerTariff = Factory.LoadFromNaturalKey<AUCClass>(AUCClassSchema.UJ_Code, "2203.00.31 15");
			AssertNotNull($"PreCondition : AUCClass tariff {"2203.00.31 15"} exists", beerTariff);
			AssertEquals("PreCondition: UQ1", "LA", beerTariff.UJ_UQ1);
			AssertEquals("PreCondition: UQ2", "L", beerTariff.UJ_UQ2);

			using (AUCustomsDataRegistry.Instance.UseCustomsReferenceData.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, false))
			{
				var wrapper = AUCClassWrapper.Load(Factory, "2203.00.31 15", ZDateTime.Today);
				AssertNotNull(wrapper);
				CombineAssertions(() =>
				{
					AssertEquals("ZZ1_TariffCode", "2203.00.31 15", wrapper.ZZ1_TariffCode);
					AssertEquals("ZZ1_TariffCodeForDisplay", "2203.00.31 15", wrapper.ZZ1_TariffCodeForDisplay);
					AssertEquals("ZZ1_ZZ8_UQ1", "LA", wrapper.ZZ1_ZZ8_UQ1);
					AssertEquals("ZZ1_ZZ8_UQ1", "L", wrapper.ZZ1_ZZ8_UQ2);
					AssertStartsWith("ZZ1_Description", "HAVING AN ALCOHOLIC STRENGTH", wrapper.ZZ1_Description);
					AssertEquals("HasChildren", false, wrapper.HasChildren);
				});
			}
		}

		public void TestLoadPartialCode()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNomenclatureGroupType("AU", "Australia");
			helper.CreateNomenclatureGroup("AU", "04", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, description: "Nom1 Description:", nomenclatureGroupType: "AU", compositeKey: "04");
			helper.CreateNomenclatureGroup("AU", "22", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, description: "Nom2 Description:", nomenclatureGroupType: "AU", compositeKey: "04.22");
			helper.CreateNomenclatureGroup("AU", "2203", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, description: "Nom3 Description:", nomenclatureGroupType: "AU", compositeKey: "04.22..03");
			helper.CreateNomenclatureGroup("AU", "2203009", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, description: "Nom4 Description:", nomenclatureGroupType: "AU", compositeKey: "04.22..03.00.9");
			helper.CreateNomenclatureGroup("AU", "2203009117", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, description: "Nomenclature Code Description", nomenclatureGroupType: "AU", compositeKey: "04.22..03.00.9.1.17");

			var impTariffType = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.Australia, Enterprise.Customs.Universal.Constants.TariffTypes.Import);
			var testTariff = helper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.Australia, impTariffType.PK, "2203009117", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, description: "Tariff Code Description", compositeKey: "04.22..03.00.9.1.17", taxOrFeeCode: "GST");
			helper.CreateTariffUOM(testTariff, Enterprise.Customs.Universal.Constants.UnitOfMeasureTypes.StatisticalUOMType, "LA");
			helper.CreateTariffUOM(testTariff, Enterprise.Customs.Universal.Constants.UnitOfMeasureTypes.AdditionalUOMType, "L");
			Factory.Save();

			using (AUCustomsDataRegistry.Instance.UseCustomsReferenceData.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, true))
			{
				var wrapper = AUCClassWrapper.LoadPartialCode(Factory, "2203.00.9", ZDateTime.Today);
				AssertNull("TariffView does not yet support partial codes even when nomenclature exists.", wrapper);
			}
		}

		public void TestLoadPartialCode_AUCClass()
		{
			var beerTariff = Factory.LoadFromNaturalKey<AUCClass>(AUCClassSchema.UJ_Code, "2203.00.3");
			AssertNotNull($"PreCondition : AUCClass tariff {"2203.00.3"} exists", beerTariff);
			AssertEquals("PreCondition: UQ1", "", beerTariff.UJ_UQ1);
			AssertEquals("PreCondition: UQ2", "", beerTariff.UJ_UQ2);

			using (AUCustomsDataRegistry.Instance.UseCustomsReferenceData.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, false))
			{
				var wrapper = AUCClassWrapper.LoadPartialCode(Factory, "2203.00.3", ZDateTime.Today);
				AssertNotNull(wrapper);
				CombineAssertions(() =>
				{
					AssertEquals("ZZ1_TariffCode", "2203.00.3", wrapper.ZZ1_TariffCode);
					AssertEquals("ZZ1_TariffCodeForDisplay", "2203.00.3", wrapper.ZZ1_TariffCodeForDisplay);
					AssertEquals("ZZ1_ZZ8_UQ1", "", wrapper.ZZ1_ZZ8_UQ1);
					AssertEquals("ZZ1_ZZ8_UQ1", "", wrapper.ZZ1_ZZ8_UQ2);
					AssertStartsWith("ZZ1_Description", "OTHER GOODS, AS FOLLOWS:", wrapper.ZZ1_Description);
					AssertEquals("HasChildren", true, wrapper.HasChildren);
				});
			}
		}

		public void TestGetTariffFetchHint()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var impTariffType = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.Australia, Enterprise.Customs.Universal.Constants.TariffTypes.Import);
			var testTariff = helper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.Australia, impTariffType.PK, "2203009117", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, description: "Tariff Code Description", compositeKey: "04.22..03.00.9.1.17", taxOrFeeCode: "GST");
			Factory.Save();

			var fetchHint = AUCClassWrapper.GetTariffFetchHint(Factory, "2203.00.91 17");
			AssertEquals("Is TariffView", TariffViewSchema.Constants.TableName, fetchHint.TableName);
			var fetchQuery = fetchHint.GetQuery();
			var querySql = fetchQuery.LiteralTextADO;
			AssertStartsWith("Filter SQL", "ZZ1_ZZZ_NKDataGrouping = 'AU' and ", querySql);
			AssertEndsWith("Filter SQL", " and ZZ1_CRT_NKTariffVersion = '' and ZZ1_TariffCode = '2203009117' and ZZ1_ZZI_NKTariffType = 'IMP'", querySql);

			var tariffs = Factory.Load<TariffView>(fetchQuery);
			AssertEquals("2203009117", tariffs[0].ZZ1_TariffCode);
			AssertEquals("Tariff Code Description", tariffs[0].ZZ1_Description);
			AssertEquals(1, tariffs.Length);
		}
	}
}
