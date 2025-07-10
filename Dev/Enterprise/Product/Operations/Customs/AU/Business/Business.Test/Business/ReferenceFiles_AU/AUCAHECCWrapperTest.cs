using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Schema;
using TariffTypes = Enterprise.Customs.Universal.Constants.TariffTypes;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class AUCAHECCWrapperTest : TestCaseWithFactory
	{
		public void TestLoad()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var expTariffType = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.Australia, TariffTypes.Export);
			var tariff = helper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.Australia, expTariffType.PK, "34060002", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, description: "Test Tariff", taxOrFeeCode: "GST");
			helper.CreateTariffUOM(tariff, Enterprise.Customs.Universal.Constants.UnitOfMeasureTypes.StatisticalUOMType, "T");

			var expTestDataTariffType = helper.CreateNewOrGetExistingTariffType(AUConstants.RefDataGroupCodes.AustraliaTest, TariffTypes.Export);
			var testDataTariff = helper.LoadOrCreateNewTariff(AUConstants.RefDataGroupCodes.AustraliaTest, expTestDataTariffType.PK, "34060001", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, description: "Test Data Tariff", taxOrFeeCode: "GST");
			helper.CreateTariffUOM(testDataTariff, Enterprise.Customs.Universal.Constants.UnitOfMeasureTypes.StatisticalUOMType, "TestT");

			using (AUCustomsDataRegistry.Instance.EnableCWRefForAHECC.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				using (AUCustomsDataRegistry.Instance.UseCMRTariffTestData.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, true))
				{
					var testDataWrapper = AUCAHECCWrapper.Load(Factory, "3406.00.01", ZDateTime.Today);
					AssertNotNull(testDataWrapper);
					CombineAssertions(() =>
					{
						AssertEquals("ZZ1_TariffCode", "34060001", testDataWrapper.ZZ1_TariffCode);
						AssertEquals("ZZ1_TariffCodeForDisplay", "3406.00.01", testDataWrapper.ZZ1_TariffCodeForDisplay);
						AssertEquals("ZZ1_ZZ8_UQ1", "TestT", testDataWrapper.ZZ1_ZZ8_UQ1);
						AssertEquals("FullTariffDescription", "Test Data Tariff", testDataWrapper.ZZ1_Description);
						AssertEquals("HasChildren", false, testDataWrapper.HasChildren);
					});
				}

				using (AUCustomsDataRegistry.Instance.UseCMRTariffTestData.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, false))
				{
					var wrapper = AUCAHECCWrapper.Load(Factory, "3406.00.02", ZDateTime.Today);
					AssertNotNull(wrapper);
					CombineAssertions(() =>
					{
						AssertEquals("ZZ1_TariffCode", "34060002", wrapper.ZZ1_TariffCode);
						AssertEquals("ZZ1_TariffCodeForDisplay", "3406.00.02", wrapper.ZZ1_TariffCodeForDisplay);
						AssertEquals("ZZ1_ZZ8_UQ1", "T", wrapper.ZZ1_ZZ8_UQ1);
						AssertEquals("FullTariffDescription", "Test Tariff", wrapper.ZZ1_Description);
						AssertEquals("HasChildren", false, wrapper.HasChildren);
					});
				}

				var categoryWrapper = AUCAHECCWrapper.Load(Factory, "3406.00", ZDateTime.Today);
				AssertNull(categoryWrapper);
			}
		}

		public void TestLoad_AHECC()
		{
			var ahecc1 = Factory.New<AUCAHECC>();
			ahecc1.UA_AHECC = "3406.00";
			ahecc1.UA_ShortDescription = "X";
			var ahecc2 = Factory.New<AUCAHECC>();
			ahecc2.UA_AHECC = "3406.00.00";
			ahecc2.UA_LongDescription = "Test Tariff";
			ahecc2.UA_UQ = "T";

			using (AUCustomsDataRegistry.Instance.EnableCWRefForAHECC.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				var wrapper = AUCAHECCWrapper.Load(Factory, "3406.00.00", ZDateTime.Today);
				AssertNotNull(wrapper);
				CombineAssertions(() =>
				{
					AssertEquals("ZZ1_TariffCode", "3406.00.00", wrapper.ZZ1_TariffCode);
					AssertEquals("ZZ1_TariffCodeForDisplay", "3406.00.00", wrapper.ZZ1_TariffCodeForDisplay);
					AssertEquals("ZZ1_ZZ8_UQ1", "T", wrapper.ZZ1_ZZ8_UQ1);
					AssertEquals("FullTariffDescription", "Test Tariff", wrapper.ZZ1_Description);
					AssertEquals("HasChildren", false, wrapper.HasChildren);
				});

				var categoryWrapper = AUCAHECCWrapper.Load(Factory, "3406.00", ZDateTime.Today);
				AssertNotNull(categoryWrapper);
				CombineAssertions(() =>
				{
					AssertEquals("ZZ1_TariffCode", "3406.00", categoryWrapper.ZZ1_TariffCode);
					AssertEquals("ZZ1_TariffCodeForDisplay", "3406.00", categoryWrapper.ZZ1_TariffCodeForDisplay);
					AssertEquals("ZZ1_ZZ8_UQ1", "", categoryWrapper.ZZ1_ZZ8_UQ1);
					AssertEquals("FullTariffDescription", "X", categoryWrapper.ZZ1_Description);
					AssertEquals("HasChildren", true, categoryWrapper.HasChildren);
				});
			}
		}

		public void TestGetTariffFetchHint()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var expTariffType = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.Australia, TariffTypes.Export);
			helper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.Australia, expTariffType.PK, "01032000", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, taxOrFeeCode: "GST"
				, description: "PURE-BRED BREEDING ANIMALS");

			var fetchHint = AUCAHECCWrapper.GetTariffFetchHint(Factory, "0103.20.00");
			AssertEquals("Is TariffView", TariffViewSchema.Constants.TableName, fetchHint.TableName);
			var fetchQuery = fetchHint.GetQuery();
			var querySql = fetchQuery.LiteralTextADO;
			AssertStartsWith("Filter SQL", "ZZ1_ZZZ_NKDataGrouping = 'AU' and ", querySql);
			AssertEndsWith("Filter SQL", " and ZZ1_CRT_NKTariffVersion = '' and ZZ1_TariffCode = '01032000' and ZZ1_ZZI_NKTariffType = 'EXP'", querySql);

			var tariffs = Factory.Load<TariffView>(fetchQuery);
			AssertEquals("01032000", tariffs[0].ZZ1_TariffCode);
			AssertEquals("PURE-BRED BREEDING ANIMALS", tariffs[0].ZZ1_Description);
			AssertEquals(1, tariffs.Length);
		}
	}
}
