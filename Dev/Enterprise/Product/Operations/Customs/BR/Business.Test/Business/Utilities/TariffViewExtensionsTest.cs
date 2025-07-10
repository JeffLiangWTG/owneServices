using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.BR.Registry;
using Enterprise.Customs.Business;
using Enterprise.Customs.Universal;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.BR.Business.Testing
{
	public class TariffViewExtensionsTest : TestCaseWithFactory
	{
		public void TestGetTariffNCMCharacteristics()
		{
			var helper = new Universal.Testing.UniversalReferenceTestDataHelper(Factory);
			var hsnTariffType = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.Brazil, "HSN");
			Factory.Save();

			var tariff = helper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.Brazil, hsnTariffType.PK, "56049000", ZDateTime.Today.AddDays(-10), ZDateTime.Today.AddDays(10));
			Factory.Save();

			helper.CreateRefCusTariffBRCharacteristic(tariff, Constants.Profile.Types.NVE, "A1", isExport: true, isImport: false);
			helper.CreateRefCusTariffBRCharacteristic(tariff, Constants.Profile.Types.NCM, "A2", isExport: true, isImport: false);
			helper.CreateRefCusTariffBRCharacteristic(tariff, Constants.Profile.Types.NCM, "A3", startDate: ZDate.Today.AddDays(-20), endDate: ZDate.Today.AddDays(-1), isExport: true, isImport: false);
			helper.CreateRefCusTariffBRCharacteristic(tariff, Constants.Profile.Types.NCM, "A4", isExport: true, isImport: false);
			helper.CreateRefCusTariffBRCharacteristic(tariff, Constants.Profile.Types.NCM, "A5", isExport: false, isImport: true);
			helper.CreateRefCusTariffBRCharacteristic(tariff, Constants.Profile.Types.NCM, "A6", isExport: true, isImport: true);

			helper.CreateRefCusTariffBRCharacteristic(tariff, Constants.Profile.Types.NCMTE, "T1", isExport: true, isImport: false);
			helper.CreateRefCusTariffBRCharacteristic(tariff, Constants.Profile.Types.NCMTE, "T2", startDate: ZDate.Today.AddDays(-20), endDate: ZDate.Today.AddDays(-1), isExport: true, isImport: false);
			helper.CreateRefCusTariffBRCharacteristic(tariff, Constants.Profile.Types.NCMTE, "T3", isExport: true, isImport: false);
			helper.CreateRefCusTariffBRCharacteristic(tariff, Constants.Profile.Types.NCMTE, "T4", isExport: true, isImport: false);
			helper.CreateRefCusTariffBRCharacteristic(tariff, Constants.Profile.Types.NCMTE, "T5", isExport: false, isImport: true);
			helper.CreateRefCusTariffBRCharacteristic(tariff, Constants.Profile.Types.NCMTE, "T6", isExport: true, isImport: true);

			Factory.Save();

			using (BRCustomsDataRegistry.Instance.ShouldLoadTariffAttributesOfTestEnvironment.SetTemporaryValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, false))
			{
				CombineAssertions("Export Testing = FALSE", () =>
				{
					var tariffCharacteristics = tariff.GetTariffNCMCharacteristics(ZDateTime.Today, isExport: true, isImport: false);
					AssertEquals("CharacteristicDirection.Export, should be", 3, tariffCharacteristics.Length);
					AssertContainsExactElementsInExactOrder(new[] { "A2", "A4", "A6" }, tariffCharacteristics.Select(x => x.Code));

					tariffCharacteristics = tariff.GetTariffNCMCharacteristics(ZDateTime.Today, isExport: false, isImport: true);
					AssertEquals("CharacteristicDirection.Import, should be", 2, tariffCharacteristics.Length);
					AssertContainsExactElementsInExactOrder(new[] { "A5", "A6" }, tariffCharacteristics.Select(x => x.Code));

					tariffCharacteristics = tariff.GetTariffNCMCharacteristics(ZDateTime.Today, isExport: true, isImport: true);
					AssertEquals("CharacteristicDirection.Both, should be", 4, tariffCharacteristics.Length);
					AssertContainsExactElementsInExactOrder(new[] { "A2", "A4", "A5", "A6" }, tariffCharacteristics.Select(x => x.Code));

					tariffCharacteristics = tariff.GetTariffNCMCharacteristics(ZDateTime.Today, isExport: false, isImport: false);
					AssertEquals("CharacteristicDirection: None, should be", 0, tariffCharacteristics.Length);
				});
			}

			using (BRCustomsDataRegistry.Instance.ShouldLoadTariffAttributesOfTestEnvironment.SetTemporaryValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, true))
			{
				CombineAssertions("Export Testing = TRUE", () =>
				{
					var tariffCharacteristics = tariff.GetTariffNCMCharacteristics(ZDateTime.Today, isExport: true, isImport: false);
					AssertEquals("CharacteristicDirection.Export, should be", 4, tariffCharacteristics.Length);
					AssertContainsExactElementsInExactOrder(new[] { "T1", "T3", "T4", "T6" }, tariffCharacteristics.Select(x => x.Code));

					tariffCharacteristics = tariff.GetTariffNCMCharacteristics(ZDateTime.Today, isExport: false, isImport: true);
					AssertEquals("CharacteristicDirection.Import, should be", 2, tariffCharacteristics.Length);
					AssertContainsExactElementsInExactOrder(new[] { "T5", "T6" }, tariffCharacteristics.Select(x => x.Code));

					tariffCharacteristics = tariff.GetTariffNCMCharacteristics(ZDateTime.Today, isExport: true, isImport: true);
					AssertEquals("CharacteristicDirection.Both, should be", 5, tariffCharacteristics.Length);
					AssertContainsExactElementsInExactOrder(new[] { "T1", "T3", "T4", "T5", "T6" }, tariffCharacteristics.Select(x => x.Code));

					tariffCharacteristics = tariff.GetTariffNCMCharacteristics(ZDateTime.Today, isExport: false, isImport: false);
					AssertEquals("CharacteristicDirection: None, should be", 0, tariffCharacteristics.Length);
				});
			}
		}

		public void TestGetTariffNVECharacteristics()
		{
			var helper = new Universal.Testing.UniversalReferenceTestDataHelper(Factory);
			var hsnTariffType = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.Brazil, "HSN");
			Factory.Save();

			var tariff = helper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.Brazil, hsnTariffType.PK, "56049000", ZDateTime.Today.AddDays(-10), ZDateTime.Today.AddDays(10));
			Factory.Save();

			helper.CreateRefCusTariffBRCharacteristic(tariff, Constants.Profile.Types.NCM, "A1");
			helper.CreateRefCusTariffBRCharacteristic(tariff, Constants.Profile.Types.NVE, "A2");
			helper.CreateRefCusTariffBRCharacteristic(tariff, Constants.Profile.Types.NVE, "A3", startDate: ZDate.Today.AddDays(-20), endDate: ZDate.Today.AddDays(-1));
			helper.CreateRefCusTariffBRCharacteristic(tariff, Constants.Profile.Types.NVE, "A4");
			helper.CreateRefCusTariffBRCharacteristic(tariff, Constants.Profile.Types.NVE, "A5");
			helper.CreateRefCusTariffBRCharacteristic(tariff, Constants.Profile.Types.NVE, "A6");
			helper.CreateRefCusTariffBRCharacteristic(tariff, Constants.Profile.Types.NCMTE, "T1");
			helper.CreateRefCusTariffBRCharacteristic(tariff, Constants.Profile.Types.NCMTE, "T2");

			Factory.Save();

			var tariffCharacteristics = tariff.GetTariffNVECharacteristics(ZDateTime.Today);
			AssertEquals("tariffCharacteristics count should be", 4, tariffCharacteristics.Length);
			AssertContainsExactElementsInExactOrder(new[] { "A2", "A4", "A5", "A6" }, tariffCharacteristics.Select(x => x.ZB1_Code));
		}

		public void TestGetTariffLPCCharacteristics()
		{
			var helper = new Universal.Testing.UniversalReferenceTestDataHelper(Factory);
			var lpcmTariffType = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.Brazil, "LPCM");
			Factory.Save();

			var tariff = helper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.Brazil, lpcmTariffType.PK, "56049000", ZDateTime.Today.AddDays(-10), ZDateTime.Today.AddDays(10));
			Factory.Save();

			helper.CreateRefCusTariffBRCharacteristic(tariff, Constants.Profile.Types.NCM, "A1");
			helper.CreateRefCusTariffBRCharacteristic(tariff, Constants.Profile.Types.LPC, "A2");
			helper.CreateRefCusTariffBRCharacteristic(tariff, Constants.Profile.Types.LPC, "A3", startDate: ZDate.Today.AddDays(-20), endDate: ZDate.Today.AddDays(-1));
			helper.CreateRefCusTariffBRCharacteristic(tariff, Constants.Profile.Types.LPC, "A4");
			helper.CreateRefCusTariffBRCharacteristic(tariff, Constants.Profile.Types.LPC, "A5");
			helper.CreateRefCusTariffBRCharacteristic(tariff, Constants.Profile.Types.LPC, "A6");

			helper.CreateRefCusTariffBRCharacteristic(tariff, Constants.Profile.Types.LPCT, "T1");
			helper.CreateRefCusTariffBRCharacteristic(tariff, Constants.Profile.Types.LPCT, "T2", startDate: ZDate.Today.AddDays(-20), endDate: ZDate.Today.AddDays(-1));
			helper.CreateRefCusTariffBRCharacteristic(tariff, Constants.Profile.Types.LPCT, "T3");
			helper.CreateRefCusTariffBRCharacteristic(tariff, Constants.Profile.Types.LPCT, "T4");

			Factory.Save();

			using (BRCustomsDataRegistry.Instance.ShouldLoadTariffAttributesOfTestEnvironment.SetTemporaryValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, false))
			{
				var tariffCharacteristics = tariff.GetTariffLPCCharacteristics(ZDateTime.Today).ToArray();
				AssertEquals("tariffCharacteristics count when Export Testing = FALSE && LPCO = TRUE, should be", 4, tariffCharacteristics.Length);
				AssertContainsExactElementsInExactOrder(new[] { "A2", "A4", "A5", "A6" }, tariffCharacteristics.Select(x => x.Code));
			}

			using (BRCustomsDataRegistry.Instance.ShouldLoadTariffAttributesOfTestEnvironment.SetTemporaryValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, true))
			{
				var tariffCharacteristics = tariff.GetTariffLPCCharacteristics(ZDateTime.Today).ToArray();
				AssertEquals("tariffCharacteristics count when Export Testing = TRUE && LPCO = TRUE, should be", 3, tariffCharacteristics.Length);
				AssertContainsExactElementsInExactOrder(new[] { "T1", "T3", "T4" }, tariffCharacteristics.Select(x => x.Code));
			}
		}

		public void TestGetTariffNCMQuestions()
		{
			ReferenceTestDataHelper.CreateNCMRefCusProfileQuestions(Factory);
			TariffView tariffView = null;

			using (BRCustomsDataRegistry.Instance.ShouldLoadTariffAttributesOfTestEnvironment.SetTemporaryValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, false))
			{
				AssertEquals(0, tariffView.GetTariffNCMQuestions("IMP", ZDateTime.Today, Constants.ProfileQuestion.AttributeValues.Product).Length);
				AssertEquals(0, tariffView.GetTariffNCMQuestions("EXP", ZDateTime.Today, Constants.ProfileQuestion.AttributeValues.Product).Length);

				tariffView = new TariffView.Loader(Factory).LoadMostRecentCachedTariff(Core.Constants.CountryCodes.Brazil, "87654321", ZDateTime.Today);
				AssertContainsExactElementsInAnyOrder(new[] { "ATT1" }, tariffView.GetTariffNCMQuestions("IMP", ZDateTime.Today, Constants.ProfileQuestion.AttributeValues.Product).Select(x => x.Code));
				AssertContainsExactElementsInAnyOrder(new[] { "ATT_3891", "ATT_3061", "ATT_1000" }, tariffView.GetTariffNCMQuestions("EXP", ZDateTime.Today, Constants.ProfileQuestion.AttributeValues.Product).Select(x => x.Code));
				AssertContainsExactElementsInAnyOrder(new[] { "ATT_4801", "ATT_4802", "ATT_4803", "ATT_4804" }, tariffView.GetTariffNCMQuestions("IMP", ZDateTime.Today, Constants.ProfileQuestion.AttributeValues.Duimp).Select(x => x.Code));

				tariffView.ZZ1_TariffCode = "12345678";
				AssertEquals(0, tariffView.GetTariffNCMQuestions("IMP", ZDateTime.Today, Constants.ProfileQuestion.AttributeValues.Product).Length);
				AssertEquals(1, tariffView.GetTariffNCMQuestions("EXP", ZDateTime.Today, Constants.ProfileQuestion.AttributeValues.Product).Length);
			}

			using (BRCustomsDataRegistry.Instance.ShouldLoadTariffAttributesOfTestEnvironment.SetTemporaryValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, true))
			{
				AssertEquals(1, tariffView.GetTariffNCMQuestions("IMP", ZDateTime.Today, Constants.ProfileQuestion.AttributeValues.Product).Length);
				AssertEquals(0, tariffView.GetTariffNCMQuestions("EXP", ZDateTime.Today, Constants.ProfileQuestion.AttributeValues.Product).Length);

				tariffView.ZZ1_TariffCode = "87654321";
				AssertContainsExactElementsInAnyOrder(new[] { "ATT_3887", "ATT_3890" }, tariffView.GetTariffNCMQuestions("EXP", ZDateTime.Today, Constants.ProfileQuestion.AttributeValues.Product).Select(x => x.Code));
				AssertContainsExactElementsInAnyOrder(new[] { "ATT_2557", "ATT_3892", "ATT_4807" }, tariffView.GetTariffNCMQuestions("IMP", ZDateTime.Today, Constants.ProfileQuestion.AttributeValues.Product).Select(x => x.Code));
				AssertContainsExactElementsInAnyOrder(new[] { "ATT_4805" }, tariffView.GetTariffNCMQuestions("IMP", ZDateTime.Today, Constants.ProfileQuestion.AttributeValues.Duimp).Select(x => x.Code));

				tariffView = null;
				AssertEquals(0, tariffView.GetTariffNCMQuestions("IMP", ZDateTime.Today, Constants.ProfileQuestion.AttributeValues.Product).Length);
				AssertEquals(0, tariffView.GetTariffNCMQuestions("EXP", ZDateTime.Today, Constants.ProfileQuestion.AttributeValues.Product).Length);
			}
		}

		public void TestGetTariffTTProfile()
		{
			ReferenceTestDataHelper.CreateTTRefCusProfileAndQuestions(Factory);
			TariffView tariffView = null;

			using (BRCustomsDataRegistry.Instance.ShouldLoadTariffAttributesOfTestEnvironment.SetTemporaryValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, false))
			{
				AssertEquals(0, tariffView.GetTariffTTProfiles("ZA", ZDateTime.Now.AddDays(-10)).Length);
				AssertEquals(0, tariffView.GetTariffTTProfiles("AO", ZDateTime.Now).Length);

				tariffView = new TariffView.Loader(Factory).LoadMostRecentCachedTariff(Core.Constants.CountryCodes.Brazil, "12345678", ZDateTime.Today);
				AssertEquals(0, tariffView.GetTariffTTProfiles(ZString.Empty, ZDateTime.Now).Length);
				AssertContainsExactElementsInAnyOrder(new[] { "ATT1", "ATT2", "ATT1", "ATT2", "ATT3", "ATT7", "ATT10", "ATT11", "ATT12", "ATT13", "" }, tariffView.GetTariffTTProfiles("ZA", ZDateTime.Now).Select(x => x.QuestionCode));
			}

			tariffView = null;

			using (BRCustomsDataRegistry.Instance.ShouldLoadTariffAttributesOfTestEnvironment.SetTemporaryValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, true))
			{
				AssertEquals(0, tariffView.GetTariffTTProfiles("ZA", ZDateTime.Now.AddDays(-10)).Length);
				AssertEquals(0, tariffView.GetTariffTTProfiles("AO", ZDateTime.Now).Length);

				tariffView = new TariffView.Loader(Factory).LoadMostRecentCachedTariff(Core.Constants.CountryCodes.Brazil, "12345678", ZDateTime.Today);
				AssertEquals(0, tariffView.GetTariffTTProfiles(ZString.Empty, ZDateTime.Now).Length);
				AssertContainsExactElementsInAnyOrder(new[] { "ATT8", "ATT9" }, tariffView.GetTariffTTProfiles("ZA", ZDateTime.Now).Select(x => x.QuestionCode));
			}
		}
	}
}
