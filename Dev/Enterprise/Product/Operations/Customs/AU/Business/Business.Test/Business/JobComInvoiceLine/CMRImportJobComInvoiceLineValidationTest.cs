using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common.AU;
using Enterprise.Customs.Universal.Testing;
using Enterprise.Environment;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	public abstract class CMRImportJobComInvoiceLineValidationTest : SharedImportJobComInvoiceLineValidationTest
	{
		[TestDate(2005, 1, 1)]
		public void TestCheckJI_CustomsUnitOfQty()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var impTariffType = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.Australia, Enterprise.Customs.Universal.Constants.TariffTypes.Import);
			var tariff1 = helper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.Australia, impTariffType.PK, "2203006920", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, description: "Tariff Code Description", taxOrFeeCode: "GST");
			helper.CreateTariffUOM(tariff1, Enterprise.Customs.Universal.Constants.UnitOfMeasureTypes.StatisticalUOMType, "LA");
			helper.CreateTariffUOM(tariff1, Enterprise.Customs.Universal.Constants.UnitOfMeasureTypes.AdditionalUOMType, "L");
			var tariff2 = helper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.Australia, impTariffType.PK, "1111111111", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, description: "Tariff Code Description", taxOrFeeCode: "GST");
			helper.CreateTariffUOM(tariff2, Enterprise.Customs.Universal.Constants.UnitOfMeasureTypes.StatisticalUOMType, "ERR");
			Factory.Save();

			using (AUCustomsDataRegistry.Instance.UseCustomsReferenceData.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, true))
			{
				var declaration = JobDeclaration.New(Factory);
				declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
				declaration.JE_DateOfFirstArrival = EffectiveDutyDate;

				var invoice = declaration.Invoices.AddNew();
				var invoiceLine = declaration.InvoiceLines.AddNew();
				invoiceLine.JI_Tariff = "2203.00.69 20";

				AssertEquals("UQ is defaulted", "LA", invoiceLine.JI_CustomsUnitQty);
				AssertEquals("Second UQ is defaulted", "L", invoiceLine.JI_SecondQuantityUQ);

				invoiceLine.JI_CustomsUnitQty = "CC";
				AssertHasWarning(invoiceLine.JI_CustomsUnitQtyInfo, string.Format("The tariff requires a different unit of quantity, {0}", "LA"));

				invoiceLine.JI_Tariff = "1111.11.11 11";
				invoiceLine.JI_CustomsUnitQty = "ZZ";
				AssertNoWarnings(invoiceLine.JI_CustomsUnitQtyInfo);
			}
		}

		[TestDate(2005, 1, 1)]
		public void TestCheckJI_CustomsUnitOfQty_AUCClass()
		{
			CMRStatisticalClassificationPeriodSnapshot statClassification = CMRStatisticalClassificationPeriodSnapshot.New(Factory);
			statClassification.SC_TariffClassificationNumber = "00000000";
			statClassification.SC_StatisticalClassificationCode = "00";
			statClassification.SC_StartDate = EffectiveDutyDate;
			statClassification.SC_EndDate = EffectiveDutyDate;
			statClassification.SC_QuantityUnit = "AA";
			statClassification.SC_SecondQuantityUnit = "BB";

			using (AUCustomsDataRegistry.Instance.UseCustomsReferenceData.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, false))
			{
				JobDeclaration declaration = JobDeclaration.New(Factory);
				declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
				declaration.JE_DateOfFirstArrival = EffectiveDutyDate;

				JobComInvoiceHeader invoice = declaration.Invoices.AddNew();
				JobComInvoiceLine invoiceLine = declaration.InvoiceLines.AddNew();
				invoiceLine.JI_Tariff = "00000000 00";

				AssertEquals("UQ is defaulted", "AA", invoiceLine.JI_CustomsUnitQty);
				AssertEquals("Second UQ is defaulted", "BB", invoiceLine.JI_SecondQuantityUQ);

				invoiceLine.JI_CustomsUnitQty = "CC";
				AssertHasWarning(invoiceLine.JI_CustomsUnitQtyInfo, string.Format("The tariff requires a different unit of quantity, {0}", "AA"));
			}
		}

		public void TestDefaultWarningOfPreferenceIfOriginDiffers()
		{
			JobDeclaration testDec = JobDeclaration.New(Factory);
			testDec.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			testDec.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages;
			testDec.JE_DateOfFirstArrival = new ZDateTime(2005, 1, 1);
			JobComInvoiceHeader invoice = testDec.Invoices.AddNew();
			invoice.AddInfo.ZA_POC = "US";
			invoice.AddInfo.ZA_PST = "US";
			invoice.AddInfo.ZA_PRT = "WO";

			CMRTariffRatePeriodSnapshot tariffRate = CMRTariffRatePeriodSnapshot.New(Factory);
			tariffRate.TT_TariffClassificationNumber = "00000000";
			tariffRate.TT_StartDate = new ZDateTime(2005, 1, 1);
			tariffRate.TT_PreferenceSchemeType = "CA";

			CMRTariffRatePeriodSnapshot tariffRate2 = CMRTariffRatePeriodSnapshot.New(Factory);
			tariffRate2.TT_TariffClassificationNumber = "00000000";
			tariffRate2.TT_StartDate = new ZDateTime(2005, 1, 1);
			tariffRate2.TT_PreferenceSchemeType = "GEN";

			CMRTariffRatePeriodSnapshot tariffRate3 = CMRTariffRatePeriodSnapshot.New(Factory);
			tariffRate3.TT_TariffClassificationNumber = "00000000";
			tariffRate3.TT_StartDate = new ZDateTime(2005, 1, 1);
			tariffRate3.TT_PreferenceSchemeType = "US";

			CMRPreferenceSchemePeriodCountry schemeCountryUS = CMRPreferenceSchemePeriodCountry.New(Factory);
			schemeCountryUS.PC_PreferenceSchemePeriodSnapshotSchemeType = "US";
			schemeCountryUS.PC_CountryCode = "US";

			CMRPreferenceSchemePeriodCountry schemeCountryCA = CMRPreferenceSchemePeriodCountry.New(Factory);
			schemeCountryCA.PC_PreferenceSchemePeriodSnapshotSchemeType = "CA";
			schemeCountryCA.PC_CountryCode = "CA";

			JobComInvoiceLine invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "00000000 00";
			invoiceLine.JI_CountryOfOrigin = "CA";

			AssertEquals("Is General rate", false, invoiceLine.IsGeneralRate);
			AssertHasWarning(invoiceLine.JI_CountryOfOriginInfo, CMRImportJobComInvoiceLineValidation.DefaultWarningForPreference);

			invoice.AddInfo.ZA_POC = "CA";
			invoiceLine.JI_CountryOfOrigin = "CA";
			AssertNoWarning(invoiceLine.JI_CountryOfOriginInfo, CMRImportJobComInvoiceLineValidation.DefaultWarningForPreference);
		}

		[TestDate(2005, 1, 3)]
		public void TestStatClassificationUQs()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var impTariffType = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.Australia, Enterprise.Customs.Universal.Constants.TariffTypes.Import);
			var tariff1 = helper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.Australia, impTariffType.PK, "0000000000", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, description: "Tariff Code Description", taxOrFeeCode: "GST");
			helper.CreateTariffUOM(tariff1, Enterprise.Customs.Universal.Constants.UnitOfMeasureTypes.StatisticalUOMType, "AA");
			helper.CreateTariffUOM(tariff1, Enterprise.Customs.Universal.Constants.UnitOfMeasureTypes.AdditionalUOMType, "BB");
			Factory.Save();

			using (AUCustomsDataRegistry.Instance.UseCustomsReferenceData.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, true))
			{
				TestDec.JE_DateOfFirstArrival = EffectiveDutyDate;
				TestDec.JE_EntrySubmittedDate = EffectiveDutyDate;
				var entry = TestDec.CustomsEntryHeaders.AddNew();
				entry.CH_Status = CustomsEntryStatus.ClearFormalLodge.Code;

				var statClassification = CMRStatisticalClassificationPeriodSnapshot.New(Factory);
				statClassification.SC_TariffClassificationNumber = "00000000";
				statClassification.SC_StatisticalClassificationCode = "00";
				statClassification.SC_StartDate = EffectiveDutyDate;
				statClassification.SC_EndDate = EffectiveDutyDate;
				statClassification.SC_QuantityUnit = "AA";
				statClassification.SC_SecondQuantityUnit = "BB";

				invoiceLine.JI_Tariff = "0000.00.00 00";
				AssertEquals("Effective duty date", EffectiveDutyDate, invoiceLine.EffectiveDutyDate);

				var validation = (CMRImportJobComInvoiceLineValidation)invoiceLine.Validation;
				AssertEquals("NeedsCustomsUQ", true, validation.NeedsCustomsUQ);
				AssertEquals("CustomsUQ", "AA", validation.CustomsUQ);
				AssertEquals("SecondUQ", "BB", validation.SecondUQ);
			}
		}

		[TestDate(2005, 1, 3)]
		public void TestStatClassificationUQs_AUCClass()
		{
			TestDec.JE_DateOfFirstArrival = EffectiveDutyDate;
			TestDec.JE_EntrySubmittedDate = EffectiveDutyDate;
			var entry = TestDec.CustomsEntryHeaders.AddNew();
			entry.CH_Status = CustomsEntryStatus.ClearFormalLodge.Code;

			var statClassification = CMRStatisticalClassificationPeriodSnapshot.New(Factory);
			statClassification.SC_TariffClassificationNumber = "00000000";
			statClassification.SC_StatisticalClassificationCode = "00";
			statClassification.SC_StartDate = EffectiveDutyDate;
			statClassification.SC_EndDate = EffectiveDutyDate;
			statClassification.SC_QuantityUnit = "AA";
			statClassification.SC_SecondQuantityUnit = "BB";

			var statClassification2 = CMRStatisticalClassificationPeriodSnapshot.New(Factory);
			statClassification2.SC_TariffClassificationNumber = "00000000";
			statClassification2.SC_StatisticalClassificationCode = "00";
			statClassification2.SC_StartDate = EffectiveDutyDate.AddDays(1);
			statClassification2.SC_QuantityUnit = "";
			statClassification2.SC_SecondQuantityUnit = "";

			invoiceLine.JI_Tariff = "0000.00.00 00";
			AssertEquals("Effective duty date", EffectiveDutyDate, invoiceLine.EffectiveDutyDate);

			var validation = (CMRImportJobComInvoiceLineValidation)invoiceLine.Validation;
			AssertEquals("NeedsCustomsUQ", true, validation.NeedsCustomsUQ);
			AssertEquals("CustomsUQ", "AA", validation.CustomsUQ);
			AssertEquals("SecondUQ", "BB", validation.SecondUQ);

			TestDec.JE_DateOfFirstArrival = EffectiveDutyDate.AddDays(1);
			TestDec.JE_EntrySubmittedDate = EffectiveDutyDate.AddDays(1);

			AssertEquals("Effective duty date", EffectiveDutyDate.AddDays(1), invoiceLine.EffectiveDutyDate);

			AssertEquals("NeedsCustomsUQ", false, validation.NeedsCustomsUQ);
			AssertEquals("CustomsUQ", "", validation.CustomsUQ);
			AssertEquals("SecondUQ", "", validation.SecondUQ);
		}

		public void TestCheckJI_Tariff()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var impTariffType = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.Australia, Enterprise.Customs.Universal.Constants.TariffTypes.Import);
			var tariff1 = helper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.Australia, impTariffType.PK, "2203006920", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, description: "Tariff Code Description", taxOrFeeCode: "GST");
			helper.CreateTariffUOM(tariff1, Enterprise.Customs.Universal.Constants.UnitOfMeasureTypes.StatisticalUOMType, "LA");
			helper.CreateTariffUOM(tariff1, Enterprise.Customs.Universal.Constants.UnitOfMeasureTypes.AdditionalUOMType, "L");
			Factory.Save();

			using (AUCustomsDataRegistry.Instance.UseCustomsReferenceData.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, true))
			{
				var declaration = Factory.NewWithValidTestData<JobDeclaration>();
				declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
				declaration.JE_DateOfFirstArrival = EffectiveDutyDate;

				var invoice = declaration.Invoices.AddNew();
				var invoiceLine = invoice.InvoiceLines.AddNew();
				invoiceLine.JI_Tariff = "00000000 00";

				AssertEquals("Is SOFA declaration", false, declaration.IsSOFADeclaration);
				AssertNoWarning(invoiceLine.JI_TariffInfo, CMRImportJobComInvoiceLineValidation.TariffMayNotQualifyForSOFA);

				declaration.AddInfo.ZA_SOFAIndicator_Hidden = true;
				invoiceLine.JI_Tariff = "2203.00.69 20";
				AssertHasWarning(invoiceLine.JI_TariffInfo, CMRImportJobComInvoiceLineValidation.TariffMayNotQualifyForSOFA);
				AssertNoMessageErrorContaining(invoiceLine.JI_TariffInfo, "Invalid Tariff/Stat code combination for ");

				invoiceLine.JI_Tariff = "9999.30.11 05";
				AssertHasWarning(invoiceLine.JI_TariffInfo, CMRImportJobComInvoiceLineValidation.TariffMayNotQualifyForSOFA);
				AssertHasMessageErrorContaining(invoiceLine.JI_TariffInfo, "Invalid Tariff/Stat code combination for ");
			}
		}

		public void TestCheckJI_Tariff_AUCClass_WithoutStatClassification()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var tariffType = helper.CreateNewOrGetExistingTariffType("AU", Enterprise.Customs.Universal.Constants.TariffTypes.Import);
			tariffType.ZZI_ZZ9_NKNomenclatureGroupType = "AU";
			helper.LoadOrCreateNewTariff("AU", tariffType.PK, "1234567899", ZDateTime.Today.AddDays(-1), ZDateTime.Today.AddDays(1), "test description 1");

			var newFactory = new BusinessObjectFactory();
			var auTariff = newFactory.New<AUCClass>();
			auTariff.UJ_Code = "1234.56.78 90";
			newFactory.Save();

			using (AUCustomsDataRegistry.Instance.UseCustomsReferenceData.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, false))
			{
				var declaration = Factory.NewWithValidTestData<JobDeclaration>();
				declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
				declaration.JE_DateOfFirstArrival = EffectiveDutyDate;

				var invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();
				invoiceLine.JI_Tariff = "1234.56.78 90";
				AssertHasMessageErrorContaining(invoiceLine.JI_TariffInfo, "Invalid Tariff/Stat code combination for ");

				using (AUCustomsDataRegistry.Instance.UseCustomsReferenceData.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, true))
				{
					invoiceLine.JI_Tariff = "1234.56.78 99";
					AssertNoMessageErrorContaining(invoiceLine.JI_TariffInfo, "Invalid Tariff/Stat code combination for ");
				}
			}
		}

		public void TestCheckJI_Tariff_AUCClass()
		{
			using (AUCustomsDataRegistry.Instance.UseCustomsReferenceData.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, false))
			{
				var statClassification = CMRStatisticalClassificationPeriodSnapshot.New(Factory);
				statClassification.SC_TariffClassificationNumber = "00000000";
				statClassification.SC_StatisticalClassificationCode = "00";
				statClassification.SC_StartDate = EffectiveDutyDate;
				statClassification.SC_EndDate = EffectiveDutyDate;
				statClassification.SC_QuantityUnit = "AA";
				statClassification.SC_SecondQuantityUnit = "BB";

				var declaration = Factory.NewWithValidTestData<JobDeclaration>();
				declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
				declaration.JE_DateOfFirstArrival = EffectiveDutyDate;

				var invoice = declaration.Invoices.AddNew();
				var invoiceLine = invoice.InvoiceLines.AddNew();
				invoiceLine.JI_Tariff = "00000000 00";

				AssertEquals("Is SOFA declaration", false, declaration.IsSOFADeclaration);
				AssertNoWarning(invoiceLine.JI_TariffInfo, CMRImportJobComInvoiceLineValidation.TariffMayNotQualifyForSOFA);

				declaration.AddInfo.ZA_SOFAIndicator_Hidden = true;
				invoiceLine.JI_Tariff = "2203.00.69 20";
				AssertHasWarning(invoiceLine.JI_TariffInfo, CMRImportJobComInvoiceLineValidation.TariffMayNotQualifyForSOFA);
				AssertNoMessageErrorContaining(invoiceLine.JI_TariffInfo, "Invalid Tariff/Stat code combination for ");

				invoiceLine.JI_Tariff = "9999.30.11 05";
				AssertHasWarning(invoiceLine.JI_TariffInfo, CMRImportJobComInvoiceLineValidation.TariffMayNotQualifyForSOFA);
				AssertHasMessageErrorContaining(invoiceLine.JI_TariffInfo, "Invalid Tariff/Stat code combination for ");
			}
		}

		protected JobDeclaration TestDec
		{
			get
			{
				if (fTestDec == null)
				{
					fTestDec = JobDeclaration.New(Factory);
					fTestDec.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
					fTestDec.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages;
					fTestDec.JE_DateOfFirstArrival = EffectiveDutyDate;
				}
				return fTestDec;
			}
		}
		JobDeclaration fTestDec;

		protected ZDateTime EffectiveDutyDate
		{
			get
			{
				if (fEffectiveDutyDate.IsEmpty)
				{
					fEffectiveDutyDate = new ZDateTime(2005, 1, 1);
				}
				return fEffectiveDutyDate;
			}
		}
		ZDateTime fEffectiveDutyDate;

		protected JobComInvoiceHeader Invoice
		{
			get
			{
				if (fInvoice == null)
				{
					fInvoice = TestDec.Invoices.AddNew();
				}
				return fInvoice;
			}
		}
		JobComInvoiceHeader fInvoice;

		protected new JobComInvoiceLine invoiceLine
		{
			get
			{
				if (fInvoiceLine == null)
				{
					fInvoiceLine = Invoice.JobComInvoiceLines.AddNew();
				}
				return fInvoiceLine;
			}
		}
		JobComInvoiceLine fInvoiceLine;
	}
}
