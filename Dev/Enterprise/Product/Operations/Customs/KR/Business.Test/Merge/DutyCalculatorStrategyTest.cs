using System;
using System.Linq;
using System.Reflection;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.KR.Messaging;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.KR.Business.Testing
{
	[TestedType(typeof(DutyCalculatorStrategy))]
	sealed class DutyCalculatorStrategyTest : Customs.Business.Testing.DutyCalculatorStrategyAbstractTest<DutyCalculatorStrategy>
	{
		public void TestShouldCalculateDutiesByDefault_NonImport()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Export;
			var strategy = new DutyCalculatorStrategy(declaration);
			AssertEquals(false, (bool)strategy.GetType().GetProperty("ShouldCalculateDuties", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(strategy));
		}

		public void TestCalculateDutiesWithTariffRate()
		{
			var dataGrouping = Core.Constants.CountryCodes.KoreaSouth;
			var referenceDataHelper = new UniversalReferenceTestDataHelper(Factory);
			var tariffType = referenceDataHelper.CreateTariffType(dataGrouping, Universal.Constants.TariffTypes.HarmonizedSystem);
			var preferenceC = referenceDataHelper.CreatePreferenceForCountry("C", "WTO협정세율", Core.Constants.CountryCodes.KoreaSouth);
			Factory.Save();
			var tariff1 = referenceDataHelper.CreateTariff(dataGrouping, tariffType.PK, "0101211000", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, "농가 사육용");
			var tariff2 = referenceDataHelper.CreateTariff(dataGrouping, tariffType.PK, "0101291000", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, "경주말");
			var rateType = referenceDataHelper.CreateCusRateType(dataGrouping, Universal.Constants.RateTypes.Duty);
			var rateCode = referenceDataHelper.CreateCusRateCode(Factory, Constants.ZZ.RateCodes.DutyAdValorem, rateType.PK);
			var rateForTariff1 = referenceDataHelper.CreateRate(tariff1, rateCode.PK, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, "0", preferencePk: preferenceC.PK, dataGrouping: dataGrouping, rateFormulaDeriveFrom: "0");
			var rateForTariff2 = referenceDataHelper.CreateRate(tariff2, rateCode.PK, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, "VFD * 0.08", preferencePk: preferenceC.PK, dataGrouping: dataGrouping, rateFormulaDeriveFrom: "8");
			var tradeGroup = referenceDataHelper.CreateTradeGroup("AU", "AU", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			referenceDataHelper.AddCountry(tradeGroup, Core.Constants.CountryCodes.Australia);
			referenceDataHelper.CreateCusApplicability(rateForTariff1, tradeGroup, ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTime.Date);
			referenceDataHelper.CreateCusApplicability(rateForTariff2, tradeGroup, ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTime.Date);
			Factory.Save();

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.Tariff;
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;

			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.KoreaRepublicOf;
			invoice.JZ_InvoiceAmount = 5326522m;

			var invoiceLine1 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine1.JI_Tariff = "0101211000";
			invoiceLine1.JI_LinePrice = 1000000m;
			invoiceLine1.JI_PrimaryPreference = "C";
			invoiceLine1.JI_CountryOfOrigin = Core.Constants.CountryCodes.Australia;

			var invoiceLine2 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine2.JI_Tariff = "0101291000";
			invoiceLine2.JI_LinePrice = 4326522m;
			invoiceLine2.JI_PrimaryPreference = "C";
			invoiceLine2.JI_CountryOfOrigin = Core.Constants.CountryCodes.Australia;

			var invoice2 = declaration.Invoices.AddNew();
			invoice2.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.KoreaRepublicOf;
			invoice2.JZ_InvoiceAmount = 22222m;

			var invoiceLine3 = invoice2.JobComInvoiceLines.AddNew();
			invoiceLine3.JI_Tariff = "0101291000";
			invoiceLine3.JI_LinePrice = 22222m;
			invoiceLine3.JI_PrimaryPreference = "C";
			invoiceLine3.JI_CountryOfOrigin = Core.Constants.CountryCodes.Australia;

			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer(true));
			AssertEquals("One entry header has been generated", 1, declaration.ActiveEntryHeaders.Count);

			var entry = declaration.ActiveEntryHeaders[0];
			AssertEquals("Two entry lines exist", 2, entry.MergedLines.Count);

			AssertFeeAmounts("Ad-Valorem Rate", Universal.Constants.RateTypes.Duty, entry, "0101211000", 0m, 0m);
			AssertFeeAmounts("Ad-Valorem Rate", Universal.Constants.RateTypes.Duty, entry, "0101291000", 347899m, 8m);
			AssertEquals("DTY 347890 [Truncated from 347899(0 + 347899)]", 347890m, entry.Charges.GetAmount(Universal.Constants.RateTypes.Duty));
		}

		public void TestCalculateDutiesWithSpecificRate()
		{
			var dataGrouping = Core.Constants.CountryCodes.KoreaSouth;
			var referenceDataHelper = new UniversalReferenceTestDataHelper(Factory);
			var tariffType = referenceDataHelper.CreateTariffType(dataGrouping, Universal.Constants.TariffTypes.HarmonizedSystem);
			var preferenceC = referenceDataHelper.CreatePreferenceForCountry("C", "WTO협정세율", Core.Constants.CountryCodes.KoreaSouth);
			Factory.Save();
			var tariff1 = referenceDataHelper.CreateTariff(dataGrouping, tariffType.PK, "3822003067", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, "제3706.90.5010호의 것");
			referenceDataHelper.CreateTariffUOM(tariff1, "CU1", "KG");
			var tariff2 = referenceDataHelper.CreateTariff(dataGrouping, tariffType.PK, "8523292231", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, "비디오 녹화된 것");
			referenceDataHelper.CreateTariffUOM(tariff2, "CU1", "U");
			referenceDataHelper.CreateTariffUOM(tariff2, "CU2", "KG");
			var tariff3 = referenceDataHelper.CreateTariff(dataGrouping, tariffType.PK, "3706101000", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, "사운드트랙만 있는 것");
			referenceDataHelper.CreateTariffUOM(tariff3, "CU1", "M");
			referenceDataHelper.CreateTariffUOM(tariff3, "CU2", "KG");
			var rateType = referenceDataHelper.CreateCusRateType(dataGrouping, Universal.Constants.RateTypes.Duty);
			var rateCode = referenceDataHelper.CreateCusRateCode(Factory, Constants.ZZ.RateCodes.DutySpecific, rateType.PK);
			var rateForTariff1 = referenceDataHelper.CreateRate(tariff1, rateCode.PK, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, "[KG] * 60", preferencePk: preferenceC.PK, dataGrouping: dataGrouping, rateFormulaDeriveFrom: "60");
			var rateForTariff2 = referenceDataHelper.CreateRate(tariff2, rateCode.PK, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, "[U] * 20", preferencePk: preferenceC.PK, dataGrouping: dataGrouping, rateFormulaDeriveFrom: "20");
			var rateForTariff3 = referenceDataHelper.CreateRate(tariff3, rateCode.PK, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, "[M] * 240", preferencePk: preferenceC.PK, dataGrouping: dataGrouping, rateFormulaDeriveFrom: "240");
			var tradeGroup = referenceDataHelper.CreateTradeGroup("AU", "AU", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			referenceDataHelper.AddCountry(tradeGroup, Core.Constants.CountryCodes.Australia);
			referenceDataHelper.CreateCusApplicability(rateForTariff1, tradeGroup, ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTime.Date);
			referenceDataHelper.CreateCusApplicability(rateForTariff2, tradeGroup, ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTime.Date);
			referenceDataHelper.CreateCusApplicability(rateForTariff3, tradeGroup, ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTime.Date);
			Factory.Save();

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.Tariff;
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;

			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.KoreaRepublicOf;

			var invoiceLine1 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine1.JI_Tariff = "3822003067";
			invoiceLine1.JI_CustomsUnitQty = "U";
			invoiceLine1.JI_CustomsQuantity = 10000;
			invoiceLine1.JI_CustomsSecondUnitQty = "KG";
			invoiceLine1.JI_CustomsSecondQuantity = 50000;
			invoiceLine1.JI_PrimaryPreference = "C";
			invoiceLine1.JI_CountryOfOrigin = Core.Constants.CountryCodes.Australia;

			var invoiceLine2 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine2.JI_Tariff = "8523292231";
			invoiceLine2.JI_CustomsThirdUnitQty = "U";
			invoiceLine2.JI_CustomsThirdQuantity = 500;
			invoiceLine2.JI_PrimaryPreference = "C";
			invoiceLine2.JI_CountryOfOrigin = Core.Constants.CountryCodes.Australia;

			var invoiceLine3 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine3.JI_Tariff = "3706101000";
			invoiceLine3.JI_CustomsUnitQty = "KG";
			invoiceLine3.JI_CustomsQuantity = 500;
			invoiceLine3.JI_PrimaryPreference = "C";
			invoiceLine3.JI_CountryOfOrigin = Core.Constants.CountryCodes.Australia;

			var invoice2 = declaration.Invoices.AddNew();
			invoice2.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.KoreaRepublicOf;

			var invoiceLine4 = invoice2.JobComInvoiceLines.AddNew();
			invoiceLine4.JI_Tariff = "3822003067";
			invoiceLine4.JI_CustomsUnitQty = "KG";
			invoiceLine4.JI_CustomsQuantity = 10000;
			invoiceLine4.JI_PrimaryPreference = "C";
			invoiceLine4.JI_CountryOfOrigin = Core.Constants.CountryCodes.Australia;

			var invoiceLine5 = invoice2.JobComInvoiceLines.AddNew();
			invoiceLine5.JI_Tariff = "8523292231";
			invoiceLine5.JI_CustomsUnitQty = "M2";
			invoiceLine5.JI_CustomsQuantity = 10000;
			invoiceLine5.JI_CustomsSecondUnitQty = "U";
			invoiceLine5.JI_CustomsSecondQuantity = 1000;
			invoiceLine5.JI_PrimaryPreference = "C";
			invoiceLine5.JI_CountryOfOrigin = Core.Constants.CountryCodes.Australia;

			var invoiceLine6 = invoice2.JobComInvoiceLines.AddNew();
			invoiceLine6.JI_Tariff = "3706101000";
			invoiceLine6.JI_CustomsUnitQty = "U";
			invoiceLine6.JI_CustomsQuantity = 500;
			invoiceLine6.JI_PrimaryPreference = "C";
			invoiceLine6.JI_CountryOfOrigin = Core.Constants.CountryCodes.Australia;

			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer(true));
			AssertEquals("One entry header has been generated", 1, declaration.ActiveEntryHeaders.Count);

			var entry = declaration.ActiveEntryHeaders[0];
			AssertEquals("Three entry lines exist", 3, entry.MergedLines.Count);

			AssertFeeAmounts("Specific Rate", Universal.Constants.RateTypes.Duty, entry, "3822003067", 3600000m, 60m);
			AssertFeeAmounts("Specific Rate", Universal.Constants.RateTypes.Duty, entry, "8523292231", 30000m, 20m);
			AssertFeeAmounts("Specific Rate", Universal.Constants.RateTypes.Duty, entry, "3706101000", 0m, 0m);
		}

		public void TestCalculateDutiesWithBothRates_MaxRate()
		{
			var dataGrouping = Core.Constants.CountryCodes.KoreaSouth;
			var referenceDataHelper = new UniversalReferenceTestDataHelper(Factory);
			var tariffType = referenceDataHelper.CreateTariffType(dataGrouping, Universal.Constants.TariffTypes.HarmonizedSystem);
			Factory.Save();
			var tariff = referenceDataHelper.CreateTariff(dataGrouping, tariffType.PK, "0712319000", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, "기타");
			referenceDataHelper.CreateTariffUOM(tariff, "CU1", "KG");
			var preferenceA = referenceDataHelper.CreatePreferenceForCountry("A", "기본세율", Core.Constants.CountryCodes.KoreaSouth);
			var rateType = referenceDataHelper.CreateCusRateType(dataGrouping, Universal.Constants.RateTypes.Duty);
			var rateCodeDTA = referenceDataHelper.CreateCusRateCode(Factory, Constants.ZZ.RateCodes.DutyAdValorem, rateType.PK);
			var rateAdvalorem = referenceDataHelper.CreateRate(tariff, rateCodeDTA.PK, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, "VFD * 0.3", dataGrouping: dataGrouping, preferencePk: preferenceA.PK, rateFormulaDeriveFrom: "30");
			var rateCodeDTS = referenceDataHelper.CreateCusRateCode(Factory, Constants.ZZ.RateCodes.DutySpecific, rateType.PK);
			var rateSpecific = referenceDataHelper.CreateRate(tariff, rateCodeDTS.PK, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, "[KG] * 1218", dataGrouping: dataGrouping, preferencePk: preferenceA.PK, rateFormulaDeriveFrom: "1218");
			var tradeGroup = referenceDataHelper.CreateTradeGroup("AU", "AU", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			referenceDataHelper.AddCountry(tradeGroup, Core.Constants.CountryCodes.Australia);
			referenceDataHelper.CreateCusApplicability(rateAdvalorem, tradeGroup, ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTime.Date, additionalCode: Constants.ZZ.ApplicabilityAdditionalCodes.Max);
			referenceDataHelper.CreateCusApplicability(rateSpecific, tradeGroup, ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTime.Date, additionalCode: Constants.ZZ.ApplicabilityAdditionalCodes.Max);
			Factory.Save();

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.Tariff;
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;

			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.KoreaRepublicOf;

			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "0712319000";
			invoiceLine.JI_CustomsUnitQty = "KG";
			invoiceLine.JI_CustomsQuantity = 1000;
			invoiceLine.JI_LinePrice = 10000000m;
			invoiceLine.JI_PrimaryPreference = "A";
			invoiceLine.JI_CountryOfOrigin = Core.Constants.CountryCodes.Australia;
			invoiceLine.JI_DutyRateSelection = "MAX";

			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer(true));
			AssertEquals("One entry header has been generated", 1, declaration.ActiveEntryHeaders.Count);
			AssertFeeAmounts("Max Rate has been selected.", Universal.Constants.RateTypes.Duty, declaration.ActiveEntryHeaders[0], "0712319000", 3000000m, 30m);

			invoiceLine.JI_PrimaryPreference = "C2";
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer(true));
			AssertEquals("One entry header has been generated", 1, declaration.ActiveEntryHeaders.Count);
			AssertFeeAmounts("Any rates matching the criteria does not exist.", Universal.Constants.RateTypes.Duty, declaration.ActiveEntryHeaders[0], "0712319000", 0m, 0m);

			invoiceLine.JI_PrimaryPreference = "A";
			invoiceLine.JI_DutyRateSelection = "MIN";
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer(true));
			AssertEquals("One entry header has been generated", 1, declaration.ActiveEntryHeaders.Count);
			AssertFeeAmounts("Any rates matching the criteria does not exist.", Universal.Constants.RateTypes.Duty, declaration.ActiveEntryHeaders[0], "0712319000", 0m, 0m);
		}

		public void TestCalculateDutiesWithBothRates_UserSelection()
		{
			var dataGrouping = Core.Constants.CountryCodes.KoreaSouth;
			var referenceDataHelper = new UniversalReferenceTestDataHelper(Factory);
			var tariffType = referenceDataHelper.CreateTariffType(dataGrouping, Universal.Constants.TariffTypes.HarmonizedSystem);
			Factory.Save();
			var tariff = referenceDataHelper.CreateTariff(dataGrouping, tariffType.PK, "3706101000", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, "기타");
			referenceDataHelper.CreateTariffUOM(tariff, "CU1", "KG");
			var preferenceC2 = referenceDataHelper.CreatePreferenceForCountry("C2", "WTO협정세율(선택2)", Core.Constants.CountryCodes.KoreaSouth);
			var rateType = referenceDataHelper.CreateCusRateType(dataGrouping, Universal.Constants.RateTypes.Duty);
			var rateCodeDTA = referenceDataHelper.CreateCusRateCode(Factory, Constants.ZZ.RateCodes.DutyAdValorem, rateType.PK);
			var rateAdvalorem = referenceDataHelper.CreateRate(tariff, rateCodeDTA.PK, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, "VFD * 0.065", dataGrouping: dataGrouping, preferencePk: preferenceC2.PK, rateFormulaDeriveFrom: "6.5");
			var rateCodeDTS = referenceDataHelper.CreateCusRateCode(Factory, Constants.ZZ.RateCodes.DutySpecific, rateType.PK);
			var rateSpecific = referenceDataHelper.CreateRate(tariff, rateCodeDTS.PK, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, "[KG] * 1560", dataGrouping: dataGrouping, preferencePk: preferenceC2.PK, rateFormulaDeriveFrom: "1560");
			var tradeGroup = referenceDataHelper.CreateTradeGroup("AU", "AU", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			referenceDataHelper.AddCountry(tradeGroup, Core.Constants.CountryCodes.Australia);
			referenceDataHelper.CreateCusApplicability(rateAdvalorem, tradeGroup, ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTime.Date, additionalCode: Constants.ZZ.ApplicabilityAdditionalCodes.Max);
			referenceDataHelper.CreateCusApplicability(rateAdvalorem, tradeGroup, ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTime.Date, additionalCode: Constants.ZZ.ApplicabilityAdditionalCodes.Min);
			referenceDataHelper.CreateCusApplicability(rateSpecific, tradeGroup, ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTime.Date, additionalCode: Constants.ZZ.ApplicabilityAdditionalCodes.Max);
			referenceDataHelper.CreateCusApplicability(rateSpecific, tradeGroup, ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTime.Date, additionalCode: Constants.ZZ.ApplicabilityAdditionalCodes.Min);
			Factory.Save();

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.Tariff;
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;

			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.KoreaRepublicOf;

			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "3706101000";
			invoiceLine.JI_CustomsUnitQty = "KG";
			invoiceLine.JI_CustomsQuantity = 1000;
			invoiceLine.JI_LinePrice = 10000000m;
			invoiceLine.JI_PrimaryPreference = "C2";
			invoiceLine.JI_CountryOfOrigin = Core.Constants.CountryCodes.Australia;

			invoiceLine.JI_DutyRateSelection = "MAX";
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer(true));
			AssertEquals("One entry header has been generated", 1, declaration.ActiveEntryHeaders.Count);
			AssertFeeAmounts("Max Rate has been selected.", Universal.Constants.RateTypes.Duty, declaration.ActiveEntryHeaders[0], "3706101000", 1560000m, 1560m);

			invoiceLine.JI_DutyRateSelection = "MIN";
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer(true));
			AssertEquals("One entry header has been generated", 1, declaration.ActiveEntryHeaders.Count);
			AssertFeeAmounts("Min Rate has been selected.", Universal.Constants.RateTypes.Duty, declaration.ActiveEntryHeaders[0], "3706101000", 650000m, 6.5m);

			invoiceLine.JI_DutyRateSelection = "";
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer(true));
			AssertEquals("One entry header has been generated", 1, declaration.ActiveEntryHeaders.Count);
			AssertFeeAmounts("User did not select any rate so the duty is not calculated.", Universal.Constants.RateTypes.Duty, declaration.ActiveEntryHeaders[0], "3706101000", 0m, 0m);

			invoiceLine.JI_PrimaryPreference = "A";
			invoiceLine.JI_DutyRateSelection = "MAX";
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer(true));
			AssertEquals("One entry header has been generated", 1, declaration.ActiveEntryHeaders.Count);
			AssertFeeAmounts("Any rates matching the criteria does not exist.", Universal.Constants.RateTypes.Duty, declaration.ActiveEntryHeaders[0], "3706101000", 0m, 0m);
		}

		public void TestCalculateDutiesWithBasePrice()
		{
			var dataGrouping = Core.Constants.CountryCodes.KoreaSouth;
			var referenceDataHelper = new UniversalReferenceTestDataHelper(Factory);
			var tariffType = referenceDataHelper.CreateTariffType(dataGrouping, Universal.Constants.TariffTypes.HarmonizedSystem);
			var preferenceC = referenceDataHelper.CreatePreferenceForCountry("C", "WTO협정세율", Core.Constants.CountryCodes.KoreaSouth);
			Factory.Save();
			var tariff = referenceDataHelper.CreateTariff(dataGrouping, tariffType.PK, "1006100000", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, "벼");
			referenceDataHelper.CreateTariffUOM(tariff, "CU1", "KG");
			var rateType = referenceDataHelper.CreateCusRateType(dataGrouping, Universal.Constants.RateTypes.Duty);
			var rateCode = referenceDataHelper.CreateCusRateCode(Factory, Constants.ZZ.RateCodes.DutyAdValorem, rateType.PK);
			var rateAdvalorem = referenceDataHelper.CreateRate(tariff, rateCode.PK, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, "VFD * 5.13 + IF(([KG] * 145 - VFD)/([KG] * 145) * 100 > 75, [KG] * 145 * 0.52 - VFD * 0.9, IF(([KG] * 145 - VFD)/([KG] * 145) * 100 > 60, [KG] * 145 * 0.47 - VFD * 0.7, IF(([KG] * 145 - VFD)/([KG] * 145) * 100 > 40, [KG] * 145 * 0.39 - VFD * 0.5, IF(([KG] * 145 - VFD)/([KG] * 145) * 100 > 10, [KG] * 145 * 0.27 - VFD * 0.3, 0))))", preferencePk: preferenceC.PK, dataGrouping: dataGrouping, rateFormulaDeriveFrom: "513");
			var tradeGroup = referenceDataHelper.CreateTradeGroup("AU", "AU", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			referenceDataHelper.AddCountry(tradeGroup, Core.Constants.CountryCodes.Australia);
			referenceDataHelper.CreateCusApplicability(rateAdvalorem, tradeGroup, ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTime.Date);
			Factory.Save();

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.Tariff;
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;

			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.KoreaRepublicOf;

			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "1006100000";
			invoiceLine.JI_CustomsUnitQty = "KG";
			invoiceLine.JI_CustomsQuantity = 50000;
			invoiceLine.JI_LinePrice = 1000000m;
			invoiceLine.JI_PrimaryPreference = "C";
			invoiceLine.JI_CountryOfOrigin = Core.Constants.CountryCodes.Australia;

			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer(true));
			AssertFeeAmounts("Base Price - F1", Universal.Constants.RateTypes.Duty, declaration.ActiveEntryHeaders[0], "1006100000", 8000000m, 513m);

			invoiceLine.JI_LinePrice = 2500000m;
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer(true));
			AssertFeeAmounts("Base Price - F2", Universal.Constants.RateTypes.Duty, declaration.ActiveEntryHeaders[0], "1006100000", 14482500m, 513m);

			invoiceLine.JI_LinePrice = 3500000m;
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer(true));
			AssertFeeAmounts("Base Price - F3", Universal.Constants.RateTypes.Duty, declaration.ActiveEntryHeaders[0], "1006100000", 19032500m, 513m);

			invoiceLine.JI_LinePrice = 5300000m;
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer(true));
			AssertFeeAmounts("Base Price - F4", Universal.Constants.RateTypes.Duty, declaration.ActiveEntryHeaders[0], "1006100000", 27556500m, 513m);
		}

		public void TestDutyReductionOrExemption()
		{
			SetupRefDBDataForDutyReductionAndExemption();

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.Tariff;
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;

			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.KoreaRepublicOf;

			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "0101291000";
			invoiceLine.JI_LinePrice = 10000000m;
			invoiceLine.JI_PrimaryPreference = "C";
			invoiceLine.JI_CountryOfOrigin = Core.Constants.CountryCodes.Australia;

			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer(true));
			AssertFeeAmounts("Ad-Valorem Rate", Universal.Constants.RateTypes.Duty, declaration.ActiveEntryHeaders[0], "0101291000", 800000m, 8m);

			invoiceLine.JI_SecondaryPreference = "A095000101";
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer(true));
			AssertFeeAmounts("Ad-Valorem Rate after duty reduction", Universal.Constants.RateTypes.Duty, declaration.ActiveEntryHeaders[0], "0101291000", 560000m, 8m);

			invoiceLine.JI_SecondaryPreference = "A088000101";
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer(true));
			AssertFeeAmounts("Ad-Valorem Rate after duty exemption", Universal.Constants.RateTypes.Duty, declaration.ActiveEntryHeaders[0], "0101291000", 0m, 8m);
		}

		public void TestDomesticTaxCalculationWithAdValoremRate()
		{
			SetupRefDBDataForDomesticTax();

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.Tariff;
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;

			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.KoreaRepublicOf;

			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "2206001010";
			invoiceLine.JI_SecondaryPreference = "A095000102";
			invoiceLine.JI_DomesticTaxCode = "941220-A";
			invoiceLine.JI_PrimaryPreference = "C";
			invoiceLine.JI_LinePrice = 10000000m;
			invoiceLine.JI_CountryOfOrigin = Core.Constants.CountryCodes.Australia;

			var invoiceLine2 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine2.JI_Tariff = "9003191000";
			invoiceLine2.JI_DomesticTaxCode = "412000-A";
			invoiceLine2.JI_LinePrice = 10000000m;
			invoiceLine2.JI_PrimaryPreference = "C";
			invoiceLine2.JI_CustomsUnitQty = "U";
			invoiceLine2.JI_CustomsQuantity = 2;
			invoiceLine2.JI_CountryOfOrigin = Core.Constants.CountryCodes.Australia;

			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer(true));
			AssertFeeAmounts("Ad-Valorem Rate: LQT", ChargeTypeList.Codes.LiquorTax, declaration.ActiveEntryHeaders[0], "2206001010", 3630000m, 30m);
			AssertEquals(3630000m, declaration.ActiveEntryHeaders[0].Charges.GetAmount(ChargeTypeList.Codes.LiquorTax));

			AssertFeeAmounts("Ad-Valorem Rate with tax-free unit price: SCT", ChargeTypeList.Codes.SpecialConsumptionTax, declaration.ActiveEntryHeaders[0], "9003191000", 1600000, 20m);
			AssertEquals(1600000m, declaration.ActiveEntryHeaders[0].Charges.GetAmount(ChargeTypeList.Codes.SpecialConsumptionTax));

			invoiceLine2.JI_CustomsQuantity = 4;
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer(true));
			AssertFeeAmounts("Ad-Valorem Rate with tax-free unit price: SCT", ChargeTypeList.Codes.SpecialConsumptionTax, declaration.ActiveEntryHeaders[0], "9003191000", 0m, 0m);
			AssertEquals(0m, declaration.ActiveEntryHeaders[0].Charges.GetAmount(ChargeTypeList.Codes.SpecialConsumptionTax));
		}

		public void TestDomesticTaxCalculationWithSpecificRate()
		{
			var dataGrouping = Core.Constants.CountryCodes.KoreaSouth;
			var referenceDataHelper = new UniversalReferenceTestDataHelper(Factory);
			var tariffType = referenceDataHelper.CreateTariffType(dataGrouping, Universal.Constants.TariffTypes.HarmonizedSystem);
			Factory.Save();
			var tariff = referenceDataHelper.CreateTariff(dataGrouping, tariffType.PK, "2711110000", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, "천연가스");
			referenceDataHelper.CreateTariffUOM(tariff, "CU1", "KG");

			var tariffTypeDMT = referenceDataHelper.CreateTariffType(dataGrouping, Constants.ZZ.TariffTypes.DomesticTaxRate);
			Factory.Save();
			var tariffDomesticTax0 = referenceDataHelper.CreateTariff(dataGrouping, tariffTypeDMT.PK, "524000-0", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, "석유가스");
			referenceDataHelper.CreateTariffAttribute(Constants.ZZ.TariffAttributes.TaxClassification1, Constants.ZZ.TariffAttributes.TransportationTax, tariffDomesticTax0);
			var tariffDomesticTax1 = referenceDataHelper.CreateTariff(dataGrouping, tariffTypeDMT.PK, "524000-A", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, "석유가스");
			referenceDataHelper.CreateTariffAttribute(Constants.ZZ.TariffAttributes.TaxClassification1, Constants.ZZ.TariffAttributes.TransportationTax, tariffDomesticTax1);
			var tariffDomesticTax2 = referenceDataHelper.CreateTariff(dataGrouping, tariffTypeDMT.PK, "524000-B", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, "석유가스");
			referenceDataHelper.CreateTariffAttribute(Constants.ZZ.TariffAttributes.TaxClassification1, Constants.ZZ.TariffAttributes.TransportationTax, tariffDomesticTax2);
			var rateTypeDMT = referenceDataHelper.CreateCusRateType(dataGrouping, "DMT");
			rateTypeDMT.ZZR_CustomsValueFormula = "CV + DTY - InstallationCost";
			var rateCodeDMT = referenceDataHelper.CreateCusRateCode(Factory, "TRT", rateTypeDMT.PK);
			var tradeGroup = referenceDataHelper.CreateTradeGroup("AU", "AU", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			referenceDataHelper.AddCountry(tradeGroup, Core.Constants.CountryCodes.Australia);
			referenceDataHelper.CreateRate(tariffDomesticTax1, rateCodeDMT.PK, new ZDateTime(2024, 1, 1), new ZDateTime(2024, 1, 31), "[KG] * 20", dataGrouping: dataGrouping, rateFormulaDeriveFrom: "20");
			referenceDataHelper.CreateRate(tariffDomesticTax2, rateCodeDMT.PK, new ZDateTime(2024, 1, 1), new ZDateTime(2024, 1, 31), "[KG] * 14", dataGrouping: dataGrouping, rateFormulaDeriveFrom: "14");
			Factory.Save();

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.Tariff;
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;

			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.KoreaRepublicOf;

			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "2710121000";
			invoiceLine.JI_DomesticTaxCode = "524000-A";
			invoiceLine.JI_CustomsUnitQty = "KG";
			invoiceLine.JI_CustomsQuantity = 10000;
			invoiceLine.JI_CountryOfOrigin = Core.Constants.CountryCodes.Australia;
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer(true));
			Factory.Save();

			AssertFeeAmounts("Domestic tax is 0 as a valid RateApplicability does not exist for the assessment date.", ChargeTypeList.Codes.TransportationTax, declaration.ActiveEntryHeaders[0], "2710121000", 0m, 0m);
			AssertEquals(0m, declaration.ActiveEntryHeaders[0].Charges.GetAmount(ChargeTypeList.Codes.TransportationTax));

			declaration.CustomsEntryHeaders[0].CusEntryNumber.CE_IssueDate = new ZDateTime(2024, 1, 1);
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer(true));
			AssertFeeAmounts("Specific Rate: TRT", ChargeTypeList.Codes.TransportationTax, declaration.ActiveEntryHeaders[0], "2710121000", 200000m, 20m);
			AssertEquals(200000m, declaration.ActiveEntryHeaders[0].Charges.GetAmount(ChargeTypeList.Codes.TransportationTax));

			invoiceLine.JI_DomesticTaxCode = "524000-B";
			invoiceLine.JI_CustomsUnitQty = "KG";
			invoiceLine.JI_CustomsQuantity = 10000;
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer(true));
			AssertFeeAmounts("Specific Rate: TRT", ChargeTypeList.Codes.TransportationTax, declaration.ActiveEntryHeaders[0], "2710121000", 140000m, 14m);
			AssertEquals(140000m, declaration.ActiveEntryHeaders[0].Charges.GetAmount(ChargeTypeList.Codes.TransportationTax));

			invoiceLine.JI_DomesticTaxCode = "524000-0";
			invoiceLine.JI_CustomsUnitQty = "KG";
			invoiceLine.JI_CustomsQuantity = 10000;
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer(true));
			AssertFeeAmounts("Domestic tax is 0. There is no tax rate for 524000-0.", ChargeTypeList.Codes.TransportationTax, declaration.ActiveEntryHeaders[0], "2710121000", 0m, 0m);
			AssertEquals(0m, declaration.ActiveEntryHeaders[0].Charges.GetAmount(ChargeTypeList.Codes.TransportationTax));
		}

		public void TestDomesticTaxCalculationWith940000()
		{
			var dataGrouping = Core.Constants.CountryCodes.KoreaSouth;
			var referenceDataHelper = new UniversalReferenceTestDataHelper(Factory);
			var tariffType = referenceDataHelper.CreateTariffType(dataGrouping, Universal.Constants.TariffTypes.HarmonizedSystem);
			Factory.Save();
			var tariff = referenceDataHelper.CreateTariff(dataGrouping, tariffType.PK, "2208301000", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, "위스키");
			referenceDataHelper.CreateTariffUOM(tariff, "CU1", "L");
			referenceDataHelper.CreateTariffUOM(tariff, "CU2", "KG");

			var tariffTypeDMT = referenceDataHelper.CreateTariffType(dataGrouping, Constants.ZZ.TariffTypes.DomesticTaxRate);
			Factory.Save();
			var tariffDomesticTax = referenceDataHelper.CreateTariff(dataGrouping, tariffTypeDMT.PK, "940000-A", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, "주정");
			referenceDataHelper.CreateTariffAttribute(Constants.ZZ.TariffAttributes.TaxClassification1, Constants.ZZ.TariffAttributes.LiquorTax, tariffDomesticTax);
			var rateTypeDMT = referenceDataHelper.CreateCusRateType(dataGrouping, "DMT");
			rateTypeDMT.ZZR_CustomsValueFormula = "CV + DTY - InstallationCost";
			var rateCodeDMT = referenceDataHelper.CreateCusRateCode(Factory, "LQT", rateTypeDMT.PK);
			var tradeGroup = referenceDataHelper.CreateTradeGroup("AU", "AU", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			referenceDataHelper.AddCountry(tradeGroup, Core.Constants.CountryCodes.Australia);
			var taxRateLQT = referenceDataHelper.CreateRate(tariffDomesticTax, rateCodeDMT.PK, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, "[KG] * (57 + IF([AC] > 95, 0.6 * ([AC] - 95), 0))", dataGrouping: dataGrouping, rateFormulaDeriveFrom: "57");
			referenceDataHelper.CreateRateUOM(taxRateLQT.PK, "KG");
			referenceDataHelper.CreateRateUOM(taxRateLQT.PK, "AC");
			Factory.Save();

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.Tariff;
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;

			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.KoreaRepublicOf;

			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "2208301000";
			invoiceLine.JI_DomesticTaxCode = "940000-A";
			invoiceLine.JI_CustomsUnitQty = "KG";
			invoiceLine.JI_CustomsQuantity = 10000m;
			invoiceLine.JI_CustomsSecondUnitQty = "AC";
			invoiceLine.JI_CountryOfOrigin = Core.Constants.CountryCodes.Australia;

			invoiceLine.JI_CustomsSecondQuantity = 96m;
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer(true));
			AssertFeeAmounts("LQT for 940000 [AC:96]", ChargeTypeList.Codes.LiquorTax, declaration.ActiveEntryHeaders[0], "2208301000", 576000m, 57m);
			AssertEquals(576000m, declaration.ActiveEntryHeaders[0].Charges.GetAmount(ChargeTypeList.Codes.LiquorTax));

			invoiceLine.JI_CustomsSecondQuantity = 95m;
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer(true));
			AssertFeeAmounts("LQT for 940000 [AC:95]", ChargeTypeList.Codes.LiquorTax, declaration.ActiveEntryHeaders[0], "2208301000", 570000m, 57m);
			AssertEquals(570000m, declaration.ActiveEntryHeaders[0].Charges.GetAmount(ChargeTypeList.Codes.LiquorTax));
		}

		public void TestEducationTax()
		{
			SetupRefDBDataForDomesticTax();

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.Tariff;
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;

			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.KoreaRepublicOf;

			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "2206001010";
			invoiceLine.JI_SecondaryPreference = "A095000102";
			invoiceLine.JI_DomesticTaxCode = "941220-A";
			invoiceLine.JI_PrimaryPreference = "C";
			invoiceLine.JI_LinePrice = 113552519m;
			invoiceLine.JI_CustomsUnitQty = "U";
			invoiceLine.JI_CustomsQuantity = 2;
			invoiceLine.JI_CountryOfOrigin = Core.Constants.CountryCodes.Australia;

			var invoiceLine2 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine2.JI_Tariff = "9003191000";
			invoiceLine2.JI_DomesticTaxCode = "412000-A";
			invoiceLine2.JI_LinePrice = 224323158m;
			invoiceLine2.JI_PrimaryPreference = "C";
			invoiceLine2.JI_CustomsUnitQty = "U";
			invoiceLine2.JI_CustomsQuantity = 2;
			invoiceLine2.JI_CountryOfOrigin = Core.Constants.CountryCodes.Australia;

			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer(true));
			AssertFeeAmounts("[Pre-Condition] Duty 30%: 113552519 * 0.3 = 34065755 [Truncated from 34065755.7] - DRE 30%: 34065755 * 0.3 = 10219726 [Truncated from 10219726.5] = 23846029",
							ChargeTypeList.Codes.Duty, declaration.ActiveEntryHeaders[0], "2206001010", 23846029m);
			AssertFeeAmounts("[Pre-Condition] Domestic Tax 30% = (113552519 + 23846029) * 0.3 = 41219564 [Truncated from 41219564.4]",
							ChargeTypeList.Codes.LiquorTax, declaration.ActiveEntryHeaders[0], "2206001010", 41219564m);
			AssertFeeAmounts("EDT for 941220: 10% of DMT = 41219564 * 0.1 = 4121956 [Truncated from 4121956.4]", ChargeTypeList.Codes.EducationTax, declaration.ActiveEntryHeaders[0], "2206001010", 4121956m);

			AssertFeeAmounts("[Pre-Condition] Duty 80%: 224323158 * 0.8 = 179458526 [Truncated from 179458526.4]", ChargeTypeList.Codes.Duty, declaration.ActiveEntryHeaders[0], "9003191000", 179458526m);
			AssertFeeAmounts("[Pre-Condition] Domestic Tax: (VFD - 5000000*[U]) * 0.2 = (224323158 + 179458526 - (5000000 * 2)) * 0.2 = 78756336 [Truncated from 78756336.8]",
							ChargeTypeList.Codes.SpecialConsumptionTax, declaration.ActiveEntryHeaders[0], "9003191000", 78756336m);
			AssertFeeAmounts("EDT for 412000: 30% of DMT = 78756336 * 0.3 = 23626900 [Truncated from 23626900.8]", ChargeTypeList.Codes.EducationTax, declaration.ActiveEntryHeaders[0], "9003191000", 23626900m);
			AssertEquals("DTY 203304550 [Truncated from 203304555]", 203304550m, declaration.ActiveEntryHeaders[0].Charges.GetAmount(ChargeTypeList.Codes.Duty));
			AssertEquals("LQT 41219560 [Truncated from 41219564]", 41219560m, declaration.ActiveEntryHeaders[0].Charges.GetAmount(ChargeTypeList.Codes.LiquorTax));
			AssertEquals("EDT 27748850 [Truncated from 27748856]", 27748850m, declaration.ActiveEntryHeaders[0].Charges.GetAmount(ChargeTypeList.Codes.EducationTax));
			AssertEquals("SCT 78756330 [Truncated from 78756336]", 78756330m, declaration.ActiveEntryHeaders[0].Charges.GetAmount(ChargeTypeList.Codes.SpecialConsumptionTax));

			invoiceLine.JI_DomesticTaxExemptionCode = "E109801";
			invoiceLine2.JI_DomesticTaxExemptionCode = "E109801";
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer(true));
			AssertFeeAmounts("[Pre-Condition] DomesticTax (DTE [U] * 4,000,000 reduced) = DMT:41219564 - (2 * 4000000) = 33219564",
							ChargeTypeList.Codes.LiquorTax, declaration.ActiveEntryHeaders[0], "2206001010", 33219564m);
			AssertFeeAmounts("EDT for 941220: 10% of DMT = 33219564 * 0.1 = 3321956 [Truncated from 3321956.4]", ChargeTypeList.Codes.EducationTax, declaration.ActiveEntryHeaders[0], "2206001010", 3321956m);

			AssertFeeAmounts("[Pre-Condition] DomesticTax (DTE [U] * 4,000,000 reduced) = DMT:78756336 - (2 * 4000000) = 70756336",
							ChargeTypeList.Codes.SpecialConsumptionTax, declaration.ActiveEntryHeaders[0], "9003191000", 70756336m);
			AssertFeeAmounts("EDT for 412000: 30% of DMT = 70756336 * 0.3 = 21226900 [Truncated from 21226900.8]", ChargeTypeList.Codes.EducationTax, declaration.ActiveEntryHeaders[0], "9003191000", 21226900m);

			AssertEquals("LQT 33219560 [Truncated from 33219564]", 33219560m, declaration.ActiveEntryHeaders[0].Charges.GetAmount(ChargeTypeList.Codes.LiquorTax));
			AssertEquals("EDT 24548850 [Truncated from 24548856]", 24548850m, declaration.ActiveEntryHeaders[0].Charges.GetAmount(ChargeTypeList.Codes.EducationTax));
			AssertEquals("SCT 70756330 [Truncated from 70756336]", 70756330m, declaration.ActiveEntryHeaders[0].Charges.GetAmount(ChargeTypeList.Codes.SpecialConsumptionTax));
		}

		public void TestAgricultureTaxA()
		{
			SetupTaxOrFeeData();
			SetupRefDBDataForDutyReductionAndExemption();

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.Tariff;
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;

			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.KoreaRepublicOf;

			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "0101291000";
			invoiceLine.JI_LinePrice = 12345678m;
			invoiceLine.JI_PrimaryPreference = "C";
			invoiceLine.JI_CountryOfOrigin = Core.Constants.CountryCodes.Australia;

			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer(true));
			AssertFeeAmounts("[Pre-Condition] Duty 8%: 12345678 * 0.08 = 987654 [Truncated from 987654.24]", ChargeTypeList.Codes.Duty, declaration.ActiveEntryHeaders[0], "0101291000", 987654m);
			AssertFeeAmounts("AgricultureTax A is 0 as Duty Reduction Amount is 0", ChargeTypeList.Codes.AgricultureTax, declaration.ActiveEntryHeaders[0], "0101291000", 0m);
			AssertEquals("DTY 987650 [Truncated from 987654]", 987650m, declaration.ActiveEntryHeaders[0].Charges.GetAmount(ChargeTypeList.Codes.Duty));
			AssertEquals(0m, declaration.ActiveEntryHeaders[0].Charges.GetAmount(ChargeTypeList.Codes.AgricultureTax));

			invoiceLine.JI_SecondaryPreference = "A095000101";
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer(true));
			AssertFeeAmounts("[Pre-Condition] Duty 8%: 12345678 * 0.08 = 987654 - DRE 30%: 987654 * 0.3 = 296296 [Truncated from 296296.2]",
							ChargeTypeList.Codes.Duty, declaration.ActiveEntryHeaders[0], "0101291000", 691358m);
			AssertFeeAmounts("AgricultureTax A (20% of Duty Reduction Amount) = 296296 * 0.2 = 59259 [Truncated from 59259.2]",
							ChargeTypeList.Codes.AgricultureTax, declaration.ActiveEntryHeaders[0], "0101291000", 59259m);
			AssertEquals("DTY 691350 [Truncated from 691358]", 691350m, declaration.ActiveEntryHeaders[0].Charges.GetAmount(ChargeTypeList.Codes.Duty));
			AssertEquals("AGT 59250 [Truncated from 59259]", 59250m, declaration.ActiveEntryHeaders[0].Charges.GetAmount(ChargeTypeList.Codes.AgricultureTax));

			invoiceLine.JI_SecondaryPreference = "A088000101";
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer(true));
			AssertFeeAmounts("[Pre-Condition] Duty 8% 12345678 * 0.08 = 987654 - DRE 100% Exempted: 987654 = 0", ChargeTypeList.Codes.Duty, declaration.ActiveEntryHeaders[0], "0101291000", 0m);
			AssertFeeAmounts("AgricultureTax A (20% of Duty Exemption Amount = 987654 * 0.2 = 197530 [Truncated from 197530.8]",
							ChargeTypeList.Codes.AgricultureTax, declaration.ActiveEntryHeaders[0], "0101291000", 197530m);
			AssertEquals(0m, declaration.ActiveEntryHeaders[0].Charges.GetAmount(ChargeTypeList.Codes.Duty));
			AssertEquals("AGT 197530 [Truncated from 197530m]", 197530m, declaration.ActiveEntryHeaders[0].Charges.GetAmount(ChargeTypeList.Codes.AgricultureTax));
		}

		public void TestAgricultureTaxBandBoth()
		{
			SetupTaxOrFeeData();
			SetupRefDBDataForDomesticTax();

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.Tariff;
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;

			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.KoreaRepublicOf;

			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "9003191000";
			invoiceLine.JI_DomesticTaxCode = "412000-A";
			invoiceLine.JI_LinePrice = 123456789m;
			invoiceLine.JI_PrimaryPreference = "C";
			invoiceLine.JI_CustomsUnitQty = "U";
			invoiceLine.JI_CustomsQuantity = 2;
			invoiceLine.JI_CountryOfOrigin = Core.Constants.CountryCodes.Australia;

			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer(true));
			AssertFeeAmounts("[Pre-Condition] Duty 80%: 123456789 * 0.8 = 98765431 [Truncated from 98765431.2]", ChargeTypeList.Codes.Duty, declaration.ActiveEntryHeaders[0], "9003191000", 98765431m);
			AssertFeeAmounts("[Pre-Condition] DomesticTax: (VFD - 5000000 * [U]) * 0.2 = (123456789 + 98765431 - (5000000 * 2)) * 0.2 = 42444444",
							ChargeTypeList.Codes.SpecialConsumptionTax, declaration.ActiveEntryHeaders[0], "9003191000", 42444444);
			AssertFeeAmounts("AgricultureTax B (10% of Domestic Tax Amount) = 42444444 * 0.1 = 4244444 [Truncated from 4244444.4",
							ChargeTypeList.Codes.AgricultureTax, declaration.ActiveEntryHeaders[0], "9003191000", 4244444);
			AssertEquals("DTY 98765430 [Truncated from 98765431]", 98765430m, declaration.ActiveEntryHeaders[0].Charges.GetAmount(ChargeTypeList.Codes.Duty));
			AssertEquals("SCT 42444440 [Truncated from 42444444]", 42444440m, declaration.ActiveEntryHeaders[0].Charges.GetAmount(ChargeTypeList.Codes.SpecialConsumptionTax));
			AssertEquals("AGT 4244440 [Truncated from 4244444]", 4244440m, declaration.ActiveEntryHeaders[0].Charges.GetAmount(ChargeTypeList.Codes.AgricultureTax));

			invoiceLine.JI_SecondaryPreference = "A095000102";
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer(true));
			AssertFeeAmounts("[Pre-Condition] Duty 80%: 123456789 * 0.8 = 98765431 [Truncated from 98765431.2] - DRE 30%: 98765431 * 0.3 = 29629629 [Truncated from 29629629.3]",
							ChargeTypeList.Codes.Duty, declaration.ActiveEntryHeaders[0], "9003191000", 69135802m);
			AssertFeeAmounts("[Pre-Condition] DomesticTax: (VFD - 5000000 * [U]) * 0.2 = (123456789 + 69135802 - (5000000 * 2)) * 0.2 = 36518518 [Truncated from 36518518.2]",
						ChargeTypeList.Codes.SpecialConsumptionTax, declaration.ActiveEntryHeaders[0], "9003191000", 36518518m);
			AssertFeeAmounts("Both AgricultureTax A and B: AGT A (20% of Duty Reduction Amount: 29629629 * 0.2 = 5925925 [Truncated from 5925925.8]) + " +
							"AGT B (10% of Domestic Tax Amount: 36518518 * 0.1 = 3651851 [Truncated from 3651851.8])", ChargeTypeList.Codes.AgricultureTax, declaration.ActiveEntryHeaders[0], "9003191000", 9577776m);

			AssertEquals("DTY 69135800 [Truncated from 69135802]", 69135800m, declaration.ActiveEntryHeaders[0].Charges.GetAmount(ChargeTypeList.Codes.Duty));
			AssertEquals("SCT 36518510 [Truncated from 36518518]", 36518510m, declaration.ActiveEntryHeaders[0].Charges.GetAmount(ChargeTypeList.Codes.SpecialConsumptionTax));
			AssertEquals("AGT 9577770 [Truncated from 9577776]", 9577770m, declaration.ActiveEntryHeaders[0].Charges.GetAmount(ChargeTypeList.Codes.AgricultureTax));

			invoiceLine.JI_DomesticTaxExemptionCode = "E109801";
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer(true));
			AssertFeeAmounts("[Pre-Condition] Duty 80%: 123456789 * 0.8 = 98765431 [Truncated from 98765431.2] - DRE 30%: 98765431 * 0.3 = 29629629 [Truncated from 29629629.3]",
							ChargeTypeList.Codes.Duty, declaration.ActiveEntryHeaders[0], "9003191000", 69135802m);
			AssertFeeAmounts("[Pre-Condition] DMT (VFD - 5000000*[U]) * 0.2: ((123456789 + 69135802 - (5000000 * 2)) * 0.2 = 36518518 [Truncated from 36518518.2] - DTE ([U] * 4000000): = 2 * 4000000 = 8000000",
							ChargeTypeList.Codes.SpecialConsumptionTax, declaration.ActiveEntryHeaders[0], "9003191000", 28518518m);
			AssertFeeAmounts("Both AgricultureTax A and B: AGT A (20% of Duty Reduction Amount: 29629629 * 0.2 = 5925925 [Truncated from 5925925.8]) + " +
							"AGT B (10% of Domestic Tax Amount: 28518518 * 0.1 = 2851851 [Truncated from 2851851.8])", ChargeTypeList.Codes.AgricultureTax, declaration.ActiveEntryHeaders[0], "9003191000", 8777776m);

			AssertEquals("DTY 69135800 [Truncated from 69135802]", 69135800m, declaration.ActiveEntryHeaders[0].Charges.GetAmount(ChargeTypeList.Codes.Duty));
			AssertEquals("SCT 28518510 [Truncated from 28518518]", 28518510m, declaration.ActiveEntryHeaders[0].Charges.GetAmount(ChargeTypeList.Codes.SpecialConsumptionTax));
			AssertEquals("AGT 8777770 [Truncated from 8777776]", 8777770m, declaration.ActiveEntryHeaders[0].Charges.GetAmount(ChargeTypeList.Codes.AgricultureTax));
		}

		public void TestDomesticTaxExemption()
		{
			SetupRefDBDataForDomesticTaxReductionOrExemption();

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.Tariff;
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;

			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.KoreaRepublicOf;

			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "8704211010";
			invoiceLine.JI_DomesticTaxCode = "511100-A";
			invoiceLine.JI_PrimaryPreference = "C";
			invoiceLine.JI_LinePrice = 113552511m;
			invoiceLine.JI_CountryOfOrigin = Core.Constants.CountryCodes.Australia;

			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer(true));
			AssertFeeAmounts("[Pre-Condition] Duty: 10% 113552511 * 0.1 = 11355251 (Truncated from 11355251.1)", ChargeTypeList.Codes.Duty, declaration.ActiveEntryHeaders[0], "8704211010", 11355251m, 10m);
			AssertFeeAmounts("[Pre-Condition] Domestic tax: 5% = (113552511 + 11355251) * 0.05 = 6245388 (Truncated from 6245388.1)",
							ChargeTypeList.Codes.SpecialConsumptionTax, declaration.ActiveEntryHeaders[0], "8704211010", 6245388m, 5m);
			AssertEquals("DTY 11355250 [Truncated from 11355251]", 11355250m, declaration.ActiveEntryHeaders[0].Charges.GetAmount(ChargeTypeList.Codes.Duty));
			AssertEquals("SCT 6245380 [Truncated from 6245388]", 6245380m, declaration.ActiveEntryHeaders[0].Charges.GetAmount(ChargeTypeList.Codes.SpecialConsumptionTax));

			invoiceLine.JI_DomesticTaxExemptionCode = "E106211";
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer(true));
			AssertFeeAmounts("Domestic tax after exemption", ChargeTypeList.Codes.SpecialConsumptionTax, declaration.ActiveEntryHeaders[0], "8704211010", 0m, 0m);
			AssertEquals(0m, declaration.ActiveEntryHeaders[0].Charges.GetAmount(ChargeTypeList.Codes.SpecialConsumptionTax));
		}

		public void TestDomesticTaxReductionForSpecificCars()
		{
			SetupRefDBDataForDomesticTaxReductionOrExemption();

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.Tariff;
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;

			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.KoreaRepublicOf;

			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "8704211010";
			invoiceLine.JI_DomesticTaxCode = "511100-A";
			invoiceLine.JI_PrimaryPreference = "C";
			invoiceLine.JI_LinePrice = 113552511m;
			invoiceLine.JI_CountryOfOrigin = Core.Constants.CountryCodes.Australia;
			invoiceLine.JI_CustomsQuantity = 1;
			invoiceLine.JI_CustomsUnitQty = "U";
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer(true));
			AssertFeeAmounts("[Pre-Condition] Duty: 10% = 113552511 * 0.1 = 11355251 (Truncated from 11355251.1)", ChargeTypeList.Codes.Duty, declaration.ActiveEntryHeaders[0], "8704211010", 11355251m, 10m);
			AssertFeeAmounts("[Pre-Condition] Domestic tax: 5% = (113552511 + 11355251) * 0.05 = 6245388 (Truncated from 6245388.1)",
							ChargeTypeList.Codes.SpecialConsumptionTax, declaration.ActiveEntryHeaders[0], "8704211010", 6245388m, 5m);
			AssertEquals("DTY 11355250 [Truncated from 11355251]", 11355250m, declaration.ActiveEntryHeaders[0].Charges.GetAmount(ChargeTypeList.Codes.Duty));
			AssertEquals("SCT 6245380 [Truncated from 6245388]", 6245380m, declaration.ActiveEntryHeaders[0].Charges.GetAmount(ChargeTypeList.Codes.SpecialConsumptionTax));

			invoiceLine.JI_DomesticTaxExemptionCode = "E109801";
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer(true));
			AssertFeeAmounts("Domestic Tax after reduction: [U] * 4000000 = 6245388 - (1 * 4000000) = 2245388", ChargeTypeList.Codes.SpecialConsumptionTax, declaration.ActiveEntryHeaders[0], "8704211010", 2245388m, 5m);
			AssertEquals("SCT 2245380 [Truncated from 2245388]", 2245380m, declaration.ActiveEntryHeaders[0].Charges.GetAmount(ChargeTypeList.Codes.SpecialConsumptionTax));

			invoiceLine.JI_CustomsQuantity = 2;
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer(true));
			AssertFeeAmounts("Domestic Tax after reduction is 0 as DTE ([U] * 4000000) = 2  * 4000000 = 8000000 is greater than the initial DMT 6245388",
							ChargeTypeList.Codes.SpecialConsumptionTax, declaration.ActiveEntryHeaders[0], "8704211010", 0m, 0m);
			AssertEquals(0m, declaration.ActiveEntryHeaders[0].Charges.GetAmount(ChargeTypeList.Codes.SpecialConsumptionTax));
		}

		public void TestDomesticTaxReductionWithInstallationCost()
		{
			SetupRefDBDataForDomesticTaxReductionOrExemption();

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.Tariff;
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;

			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.KoreaRepublicOf;

			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "8704211010";
			invoiceLine.JI_DomesticTaxCode = "511100-A";
			invoiceLine.JI_PrimaryPreference = "C";
			invoiceLine.JI_LinePrice = 261461569m;
			invoiceLine.JI_CountryOfOrigin = Core.Constants.CountryCodes.Australia;
			invoiceLine.JI_CustomsQuantity = 2;
			invoiceLine.JI_CustomsUnitQty = "U";
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer(true));
			AssertFeeAmounts("[Pre-Condition] Duty: 10% = 261461569 * 0.1 = 26146156 [Truncated from 26146156.9]", ChargeTypeList.Codes.Duty, declaration.ActiveEntryHeaders[0], "8704211010", 26146156m, 10m);
			AssertFeeAmounts("[Pre-Condition] Domestic tax: 5% = (261461569 + 26146156) * 0.05 = 14380386 [Truncated from 14380386.25]",
							ChargeTypeList.Codes.SpecialConsumptionTax, declaration.ActiveEntryHeaders[0], "8704211010", 14380386m, 5m);
			AssertEquals("DTY 26146150 [Truncated from 26146156]", 26146150m, declaration.ActiveEntryHeaders[0].Charges.GetAmount(ChargeTypeList.Codes.Duty));
			AssertEquals("SCT 14380380 [Truncated from 14380386]", 14380380m, declaration.ActiveEntryHeaders[0].Charges.GetAmount(ChargeTypeList.Codes.SpecialConsumptionTax));

			invoiceLine.JI_DomesticTaxExemptionCode = "L180131";
			invoiceLine.JI_InstallationCost = 2000000m;
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer(true));
			AssertFeeAmounts("Domestic Tax after reduction: DMT 5% (261461569 + 26146156 - 2000000) = 285607725 * 0.05 = 14280386 [Truncated from 14280386.25] - DTE ([U] * 5000000) = 2 * 5000000 = 10000000",
							ChargeTypeList.Codes.SpecialConsumptionTax, declaration.ActiveEntryHeaders[0], "8704211010", 4280386m, 5m);
			AssertEquals("SCT 4280380 [Truncated from 4280386]", 4280380m, declaration.ActiveEntryHeaders[0].Charges.GetAmount(ChargeTypeList.Codes.SpecialConsumptionTax));

			invoiceLine.JI_CustomsQuantity = 3;
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer(true));
			AssertFeeAmounts("Domestic Tax after reduction is 0 as DTE ([U] * 5000000) = 3  * 5000000 = 15000000 is greater than the initial DMT 14380386",
							ChargeTypeList.Codes.SpecialConsumptionTax, declaration.ActiveEntryHeaders[0], "8704211010", 0m, 0m);
			AssertEquals(0m, declaration.ActiveEntryHeaders[0].Charges.GetAmount(ChargeTypeList.Codes.SpecialConsumptionTax));
		}

		public void TestVATCalculation()
		{
			SetupTaxOrFeeData();
			SetupRefDBDataForDomesticTax();

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.Tariff;
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;

			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.KoreaRepublicOf;

			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "9003191000";
			invoiceLine.JI_DomesticTaxCode = "412000-A";
			invoiceLine.JI_LinePrice = 113552511m;
			invoiceLine.JI_PrimaryPreference = "C";
			invoiceLine.JI_DomesticTaxExemptionCode = "E109801";
			invoiceLine.JI_CustomsUnitQty = "U";
			invoiceLine.JI_CustomsQuantity = 2;
			invoiceLine.JI_CountryOfOrigin = Core.Constants.CountryCodes.Australia;
			invoiceLine.JI_SecondaryPreference = "A095000102";
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer(true));

			AssertFeeAmounts("[Pre-Condition] DTY 80%: 113552511 * 0.8 = 90842008 [Truncated from 90842008.8] - DRE 30%: 90842008 * 0.3 = 27252602 [Truncated from 27252602.4]",
							ChargeTypeList.Codes.Duty, declaration.ActiveEntryHeaders[0], "9003191000", 63589406m);

			AssertFeeAmounts("[Pre-Condition] DMT (VFD - 5000000*[U]) * 0.2: ((113552511 + 63589406 - (5000000 * 2)) * 0.2 = 33428383 [Truncated from 33428383.4] - DTE ([U] * 4000000): = 2 * 4000000 = 8000000",
							ChargeTypeList.Codes.SpecialConsumptionTax, declaration.ActiveEntryHeaders[0], "9003191000", 25428383m);

			AssertFeeAmounts("[Pre-Condition] AgricultureTax A and B (20% of Duty Reduction Amount: 27252602 * 0.2 = 5450520 [Truncated from 5450520.4] + " +
							"10% of Domestic Tax Amount: 25428383 * 0.1 = 2542838 [Truncated from 2542838.3])", ChargeTypeList.Codes.AgricultureTax, declaration.ActiveEntryHeaders[0], "9003191000", 7993358m);

			AssertFeeAmounts("[Pre-Condition] EducationTax (30% of DMT) = 25428383 * 0.3 = 7628514 [Truncated from 7628514.9]",
							ChargeTypeList.Codes.EducationTax, declaration.ActiveEntryHeaders[0], "9003191000", 7628514m);
			AssertEquals("DTY 63589400 [Truncated from 63589406]", 63589400m, declaration.ActiveEntryHeaders[0].Charges.GetAmount(ChargeTypeList.Codes.Duty));
			AssertEquals("SCT 25428380 [Truncated from 25428383]", 25428380m, declaration.ActiveEntryHeaders[0].Charges.GetAmount(ChargeTypeList.Codes.SpecialConsumptionTax));
			AssertEquals("AGT 7993350 [Truncated from 7993358]", 7993350m, declaration.ActiveEntryHeaders[0].Charges.GetAmount(ChargeTypeList.Codes.AgricultureTax));
			AssertEquals("EDT 7628510 [Truncated from 7628514]", 7628510m, declaration.ActiveEntryHeaders[0].Charges.GetAmount(ChargeTypeList.Codes.EducationTax));

			invoiceLine.JI_ZZF_NKTaxType = Constants.ZZ.RefCusTaxOrFeeCodes.VATRateA;
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer(true));
			AssertEquals("VTA ValueForVAT when product code is not 'A': CV+DTY(After deducting DRE)+DMT(After deducting DTE)+AGT+EDT = CV:113552511 + DTY:63589406(DRE deducted) + DMT:25428383(DTE deducted) + AGT:7993358 + EDT:7628514",
						218192172m, declaration.ActiveEntryHeaders[0].AllEntryLines[0].CL_ValueForVAT);
			AssertEquals("VTA ValueExemptForVAT: 0", 0m, declaration.ActiveEntryHeaders[0].AllEntryLines[0].CL_ValueExemptForVAT);
			AssertFeeAmounts("VAT A (Full RAte): ValueForVAT * 0.1 = 218192172 * 0.1 = 21819217 [Truncated from 21819217.2]", ChargeTypeList.Codes.VAT, declaration.ActiveEntryHeaders[0], "9003191000", 21819217m);
			AssertEquals("VAT 21819210 [Truncated from 21819217]", 21819210m, declaration.ActiveEntryHeaders[0].Charges.GetAmount(ChargeTypeList.Codes.VAT));

			invoiceLine.JI_ProductTypeCode = ProductOrMaterialCodeList.Codes.A;
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer(true));
			AssertEquals("VTA ValueForVAT when product code is 'A': DMT(After deducting DTE)+AGT+EDT = DMT:25428383(DTE deducted) + AGT:7993358 + EDT:7628514",
						41050255m, declaration.ActiveEntryHeaders[0].AllEntryLines[0].CL_ValueForVAT);
			AssertEquals("VTA ValueExemptForVAT: 0", 0m, declaration.ActiveEntryHeaders[0].AllEntryLines[0].CL_ValueExemptForVAT);
			AssertFeeAmounts("VAT A (Full RAte): ValueForVAT * 0.1 = 41050255 * 0.1 = 4105025 [Truncated from 4105025.5]", ChargeTypeList.Codes.VAT, declaration.ActiveEntryHeaders[0], "9003191000", 4105025m);
			AssertEquals("VAT 4105020 [Truncated from 4105025]", 4105020m, declaration.ActiveEntryHeaders[0].Charges.GetAmount(ChargeTypeList.Codes.VAT));

			invoiceLine.JI_ZZF_NKTaxType = Constants.ZZ.RefCusTaxOrFeeCodes.VATRateB;
			invoiceLine.JI_ProductTypeCode = ProductOrMaterialCodeList.Codes.B;
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer(true));
			AssertEquals("VTB ValueForVAT: 0", 0m, declaration.ActiveEntryHeaders[0].AllEntryLines[0].CL_ValueForVAT);
			AssertEquals("VTB ValueExemptForVAT when product code is not 'A': CV+DTY(After deducting DRE)+DMT(After deducting DTE)+AGT+EDT = CV:113552511 + DTY:63589406(DRE deducted) + DMT:25428383(DTE deducted) + AGT:7993358 + EDT:7628514",
						218192172m, declaration.ActiveEntryHeaders[0].AllEntryLines[0].CL_ValueExemptForVAT);
			AssertFeeAmounts("VAT B (Exemption): 0", ChargeTypeList.Codes.VAT, declaration.ActiveEntryHeaders[0], "9003191000", 0m);
			AssertEquals(0m, declaration.ActiveEntryHeaders[0].Charges.GetAmount(ChargeTypeList.Codes.VAT));

			invoiceLine.JI_ProductTypeCode = ProductOrMaterialCodeList.Codes.A;
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer(true));
			AssertEquals("VTB ValueForVAT: 0", 0m, declaration.ActiveEntryHeaders[0].AllEntryLines[0].CL_ValueForVAT);
			AssertEquals("VTB ValueExemptForVAT when product code is 'A': DMT(After deducting DTE)+AGT+EDT = DMT:25428383(DTE deducted) + AGT:7993358 + EDT:7628514",
						41050255m, declaration.ActiveEntryHeaders[0].AllEntryLines[0].CL_ValueExemptForVAT);
			AssertFeeAmounts("VAT B (Exemption): 0", ChargeTypeList.Codes.VAT, declaration.ActiveEntryHeaders[0], "9003191000", 0m);
			AssertEquals(0m, declaration.ActiveEntryHeaders[0].Charges.GetAmount(ChargeTypeList.Codes.VAT));

			invoiceLine.JI_ZZF_NKTaxType = Constants.ZZ.RefCusTaxOrFeeCodes.VATRateC;
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer(true));
			AssertEquals("VTC ValueExemptForVAT: (CV+DTY(After deducting DRE)+DMT(After deducting DTE)+AGT+EDT+DRE) * DRE Rate " +
						"= (CV:113552511 + DTY:63589406(DRE deducted) + DMT:25428383(DTE deducted) + AGT:7993358 + EDT:7628514 + DRE: 27252602) * 0.3 = 73633432 [Truncated from 73633432.2]",
						73633432m, declaration.ActiveEntryHeaders[0].AllEntryLines[0].CL_ValueExemptForVAT);
			AssertEquals("VTC ValueForVAT: (CV+DTY(After deducting DRE)+DMT(After deducting DTE)+AGT+EDT+DRE) - ValueExemptForVAT " +
						"= (CV:113552511 + DTY:63589406(DRE deducted) + DMT:25428383(DTE deducted) + AGT:7993358 + EDT:7628514 + DRE: 27252602) - 65457651 = 171811342",
						171811342m, declaration.ActiveEntryHeaders[0].AllEntryLines[0].CL_ValueForVAT);
			AssertFeeAmounts("VAT C (Reduced RAte): ValueForVAT * 0.1 = 171811342 * 0.1 = 17181134 [Truncated from 17181134.2]", ChargeTypeList.Codes.VAT, declaration.ActiveEntryHeaders[0], "9003191000", 17181134m);
			AssertEquals("VAT 17181130 [Truncated from 17181134]", 17181130m, declaration.ActiveEntryHeaders[0].Charges.GetAmount(ChargeTypeList.Codes.VAT));
		}

		void AssertFeeAmounts(ZString rateDetail, ZString feeType, CusEntryHeader entry, ZString tariff, ZDecimal feeAmount, ZDecimal? rate = null)
		{
			var entryLine = entry.MergedLines.Cast<CusEntryLine>().FirstOrDefault(x => x.CL_AdValoremTariff == tariff);
			AssertEquals(rateDetail, feeAmount, entryLine.Fees.GetAmount(feeType));
			if (rate != null)
			{
				AssertEquals(rate, entryLine.Fees.GetElementWithThisCode(feeType)?.CF_Rate ?? 0);
			}
		}

		void SetupRefDBDataForDutyReductionAndExemption()
		{
			var dataGrouping = Core.Constants.CountryCodes.KoreaSouth;
			var referenceDataHelper = new UniversalReferenceTestDataHelper(Factory);
			var tariffType = referenceDataHelper.CreateTariffType(dataGrouping, Universal.Constants.TariffTypes.HarmonizedSystem);
			var preferenceC = referenceDataHelper.CreatePreferenceForCountry("C", "WTO협정세율", Core.Constants.CountryCodes.KoreaSouth);
			Factory.Save();
			var tariff = referenceDataHelper.CreateTariff(dataGrouping, tariffType.PK, "0101291000", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, "경주말");
			referenceDataHelper.CreateTariffUOM(tariff, "CU1", "U");
			referenceDataHelper.CreateTariffUOM(tariff, "CU2", "KG");
			var rateType = referenceDataHelper.CreateCusRateType(dataGrouping, Universal.Constants.RateTypes.Duty);
			var rateCode = referenceDataHelper.CreateCusRateCode(Factory, Constants.ZZ.RateCodes.DutyAdValorem, rateType.PK);
			var rateAdvalorem = referenceDataHelper.CreateRate(tariff, rateCode.PK, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, "VFD * 0.08", preferencePk: preferenceC.PK, dataGrouping: dataGrouping, rateFormulaDeriveFrom: "8");
			var tradeGroup = referenceDataHelper.CreateTradeGroup("AU", "AU", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			referenceDataHelper.AddCountry(tradeGroup, Core.Constants.CountryCodes.Australia);
			referenceDataHelper.CreateCusApplicability(rateAdvalorem, tradeGroup, ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTime.Date);

			var tariffTypeDRE = referenceDataHelper.CreateTariffType(dataGrouping, Constants.ZZ.TariffTypes.DutyReductionExemption);
			Factory.Save();
			var tariffDutyReduction = referenceDataHelper.CreateTariff(dataGrouping, tariffTypeDRE.PK, "A095000101", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, "관세법 제95조제1항제1호 해당물품");
			referenceDataHelper.CreateTariffAttribute(Constants.ZZ.TariffAttributes.AgricultureTaxAApplies, "Y", tariffDutyReduction);
			var tariffDutyExemption = referenceDataHelper.CreateTariff(dataGrouping, tariffTypeDRE.PK, "A088000101", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, "관세법 제88조제1항제1호 해당물품");
			referenceDataHelper.CreateTariffAttribute(Constants.ZZ.TariffAttributes.IsDutyExempt, "Y", tariffDutyExemption);
			referenceDataHelper.CreateTariffAttribute(Constants.ZZ.TariffAttributes.AgricultureTaxAApplies, "Y", tariffDutyExemption);
			var rateTypeDRE = referenceDataHelper.CreateCusRateType(dataGrouping, "DRE");
			rateTypeDRE.ZZR_CustomsValueFormula = "DTY";
			var rateCodeDRE = referenceDataHelper.CreateCusRateCode(Factory, "DRE", rateTypeDRE.PK);
			referenceDataHelper.CreateRate(tariffDutyReduction, rateCodeDRE.PK, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, "VFD * 0.3", dataGrouping: dataGrouping, rateFormulaDeriveFrom: "30");
			referenceDataHelper.CreateRate(tariffDutyExemption, rateCodeDRE.PK, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, "VFD", dataGrouping: dataGrouping, rateFormulaDeriveFrom: "100");
			Factory.Save();
		}

		void SetupRefDBDataForDomesticTax()
		{
			var dataGrouping = Core.Constants.CountryCodes.KoreaSouth;
			var referenceDataHelper = new UniversalReferenceTestDataHelper(Factory);
			var tariffType = referenceDataHelper.CreateTariffType(dataGrouping, Universal.Constants.TariffTypes.HarmonizedSystem);
			var tariffTypeDRE = referenceDataHelper.CreateTariffType(dataGrouping, Constants.ZZ.TariffTypes.DutyReductionExemption);
			var tariffTypeDMT = referenceDataHelper.CreateTariffType(dataGrouping, Constants.ZZ.TariffTypes.DomesticTaxRate);
			var tariffTypeDTE = referenceDataHelper.CreateTariffType(dataGrouping, Constants.ZZ.TariffTypes.DomesticTaxReductionExemption);
			var preferenceC = referenceDataHelper.CreatePreferenceForCountry("C", "WTO협정세율", Core.Constants.CountryCodes.KoreaSouth);
			Factory.Save();
			var tariff = referenceDataHelper.CreateTariff(dataGrouping, tariffType.PK, "2206001010", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, "사과주");
			referenceDataHelper.CreateTariffUOM(tariff, "CU1", "L");
			referenceDataHelper.CreateTariffUOM(tariff, "CU2", "KG");
			var tariff2 = referenceDataHelper.CreateTariff(dataGrouping, tariffType.PK, "9003191000", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, "귀금속을 사용한 것");
			referenceDataHelper.CreateTariffUOM(tariff2, "CU1", "U");
			referenceDataHelper.CreateTariffUOM(tariff2, "CU2", "KG");

			var rateType = referenceDataHelper.CreateCusRateType(dataGrouping, Universal.Constants.RateTypes.Duty);
			var rateCode = referenceDataHelper.CreateCusRateCode(Factory, Constants.ZZ.RateCodes.DutyAdValorem, rateType.PK);
			var dutyRateAdValorem = referenceDataHelper.CreateRate(tariff, rateCode.PK, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, "VFD * 0.3", preferencePk: preferenceC.PK, dataGrouping: dataGrouping, rateFormulaDeriveFrom: "30");
			var dutyRateAdValorem2 = referenceDataHelper.CreateRate(tariff2, rateCode.PK, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, "VFD * 0.8", preferencePk: preferenceC.PK, dataGrouping: dataGrouping, rateFormulaDeriveFrom: "80");
			var tradeGroup = referenceDataHelper.CreateTradeGroup("AU", "AU", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			referenceDataHelper.AddCountry(tradeGroup, Core.Constants.CountryCodes.Australia);
			referenceDataHelper.CreateCusApplicability(dutyRateAdValorem, tradeGroup, ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTime.Date);
			referenceDataHelper.CreateCusApplicability(dutyRateAdValorem2, tradeGroup, ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTime.Date);

			var tariffDutyReduction = referenceDataHelper.CreateTariff(dataGrouping, tariffTypeDRE.PK, "A095000102", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, "관세법 제95조제1항제2호 해당물품");
			referenceDataHelper.CreateTariffAttribute(Constants.ZZ.TariffAttributes.AgricultureTaxAApplies, "Y", tariffDutyReduction);
			var rateTypeDRE = referenceDataHelper.CreateCusRateType(dataGrouping, "DRE");
			rateTypeDRE.ZZR_CustomsValueFormula = "DTY";
			var rateCodeDRE = referenceDataHelper.CreateCusRateCode(Factory, "DRE", rateTypeDRE.PK);
			referenceDataHelper.CreateRate(tariffDutyReduction, rateCodeDRE.PK, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, "VFD * 0.3", dataGrouping: dataGrouping, rateFormulaDeriveFrom: "30");

			var tariffDomesticTax = referenceDataHelper.CreateTariff(dataGrouping, tariffTypeDMT.PK, "941220-A", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, "과실주");
			referenceDataHelper.CreateTariffAttribute(Constants.ZZ.TariffAttributes.TaxClassification1, Constants.ZZ.TariffAttributes.LiquorTax, tariffDomesticTax);
			var tariffDomesticTax2 = referenceDataHelper.CreateTariff(dataGrouping, tariffTypeDMT.PK, "412000-A", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, "귀금속제품");
			referenceDataHelper.CreateTariffAttribute(Constants.ZZ.TariffAttributes.TaxClassification1, Constants.ZZ.TariffAttributes.SpecialConsumptionTax, tariffDomesticTax2);
			referenceDataHelper.CreateTariffAttribute(Constants.ZZ.TariffAttributes.AgricultureTaxBApplies, "Y", tariffDomesticTax2);
			var rateTypeDMT = referenceDataHelper.CreateCusRateType(dataGrouping, "DMT");
			rateTypeDMT.ZZR_CustomsValueFormula = "CV + DTY - InstallationCost";
			var rateCodeLQT = referenceDataHelper.CreateCusRateCode(Factory, "LQT", rateTypeDMT.PK);
			referenceDataHelper.CreateRate(tariffDomesticTax, rateCodeLQT.PK, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, "VFD * 0.3", dataGrouping: dataGrouping, rateFormulaDeriveFrom: "30");
			var rateCodeSCT = referenceDataHelper.CreateCusRateCode(Factory, "SCT", rateTypeDMT.PK);
			referenceDataHelper.CreateRate(tariffDomesticTax2, rateCodeSCT.PK, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, "(VFD - 5000000*[U]) * 0.2", dataGrouping: dataGrouping, rateFormulaDeriveFrom: "20");
			var rateTypeEDT = referenceDataHelper.CreateCusRateType(dataGrouping, "EDT");
			rateTypeEDT.ZZR_CustomsValueFormula = "DMT";
			var rateCodeEDT = referenceDataHelper.CreateCusRateCode(Factory, "EDT", rateTypeEDT.PK);
			referenceDataHelper.CreateRate(tariffDomesticTax, rateCodeEDT.PK, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, "VFD * 0.1", dataGrouping: dataGrouping);
			referenceDataHelper.CreateRate(tariffDomesticTax2, rateCodeEDT.PK, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, "VFD * 0.3", dataGrouping: dataGrouping);

			var tariffDomesticTaxReduction = referenceDataHelper.CreateTariff(dataGrouping, tariffTypeDTE.PK, "E109801", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, "조세특례제한법 제109조 제8항 제1호 물품");
			referenceDataHelper.CreateTariffUOM(tariffDomesticTaxReduction, "CU1", "U");
			var rateTypeDTE = referenceDataHelper.CreateCusRateType(dataGrouping, "DTE");
			rateTypeDTE.ZZR_CustomsValueFormula = "DMT";
			var rateCodeDTE = referenceDataHelper.CreateCusRateCode(Factory, "DTE", rateTypeDTE.PK);
			referenceDataHelper.CreateRate(tariffDomesticTaxReduction, rateCodeDTE.PK, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, "[U] * 4000000", dataGrouping: dataGrouping);
			Factory.Save();
		}

		void SetupRefDBDataForDomesticTaxReductionOrExemption()
		{
			var dataGrouping = Core.Constants.CountryCodes.KoreaSouth;
			var referenceDataHelper = new UniversalReferenceTestDataHelper(Factory);
			var tariffType = referenceDataHelper.CreateTariffType(dataGrouping, Universal.Constants.TariffTypes.HarmonizedSystem);
			var tariffTypeDMT = referenceDataHelper.CreateTariffType(dataGrouping, Constants.ZZ.TariffTypes.DomesticTaxRate);
			var tariffTypeDTE = referenceDataHelper.CreateTariffType(dataGrouping, Constants.ZZ.TariffTypes.DomesticTaxReductionExemption);
			var preferenceC = referenceDataHelper.CreatePreferenceForCountry("C", "WTO협정세율", Core.Constants.CountryCodes.KoreaSouth);
			Factory.Save();

			var tariff = referenceDataHelper.CreateTariff(dataGrouping, tariffType.PK, "8704211010", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, "신차");
			var rateType = referenceDataHelper.CreateCusRateType(dataGrouping, Universal.Constants.RateTypes.Duty);
			var rateCode = referenceDataHelper.CreateCusRateCode(Factory, Constants.ZZ.RateCodes.DutyAdValorem, rateType.PK);
			var dutyRateAdValorem = referenceDataHelper.CreateRate(tariff, rateCode.PK, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, "VFD * 0.1", preferencePk: preferenceC.PK, dataGrouping: dataGrouping, rateFormulaDeriveFrom: "10");
			var tradeGroup = referenceDataHelper.CreateTradeGroup("AU", "AU", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			referenceDataHelper.AddCountry(tradeGroup, Core.Constants.CountryCodes.Australia);
			referenceDataHelper.CreateCusApplicability(dutyRateAdValorem, tradeGroup, ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTime.Date);

			var tariffDomesticTax = referenceDataHelper.CreateTariff(dataGrouping, tariffTypeDMT.PK, "511100-A", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, "자동차 내국세");
			referenceDataHelper.CreateTariffAttribute(Constants.ZZ.TariffAttributes.TaxClassification1, Constants.ZZ.TariffAttributes.SpecialConsumptionTax, tariffDomesticTax);
			var rateTypeDMT = referenceDataHelper.CreateCusRateType(dataGrouping, "DMT");
			rateTypeDMT.ZZR_CustomsValueFormula = "CV + DTY - InstallationCost";
			var rateCodeSCT = referenceDataHelper.CreateCusRateCode(Factory, "SCT", rateTypeDMT.PK);
			var taxRateSCT = referenceDataHelper.CreateRate(tariffDomesticTax, rateCodeSCT.PK, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, "VFD * 0.05", dataGrouping: dataGrouping, rateFormulaDeriveFrom: "5");

			var tariffDomesticTaxExemption = referenceDataHelper.CreateTariff(dataGrouping, tariffTypeDTE.PK, "E106211", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, "조세특례제한법 제106조의2제1항제1호 물품");
			var tariffDomesticTaxReduction = referenceDataHelper.CreateTariff(dataGrouping, tariffTypeDTE.PK, "E109801", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, "조세특례제한법 제109조 제8항 제1호 물품");
			var tariffDomesticTaxInstallationCost = referenceDataHelper.CreateTariff(dataGrouping, tariffTypeDTE.PK, "L180131", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, "개별소비세법 18조 1항 3호가목의 물품");
			referenceDataHelper.CreateTariffUOM(tariffDomesticTaxReduction, "CU1", "U");
			referenceDataHelper.CreateTariffUOM(tariffDomesticTaxInstallationCost, "CU1", "U");
			var rateTypeDTE = referenceDataHelper.CreateCusRateType(dataGrouping, "DTE");
			rateTypeDTE.ZZR_CustomsValueFormula = "DMT";
			var rateCodeDTE = referenceDataHelper.CreateCusRateCode(Factory, "DTE", rateTypeDTE.PK);
			referenceDataHelper.CreateRate(tariffDomesticTaxExemption, rateCodeDTE.PK, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, "VFD", dataGrouping: dataGrouping);
			referenceDataHelper.CreateRate(tariffDomesticTaxReduction, rateCodeDTE.PK, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, "[U] * 4000000", dataGrouping: dataGrouping);
			referenceDataHelper.CreateRate(tariffDomesticTaxInstallationCost, rateCodeDTE.PK, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, "[U] * 5000000", dataGrouping: dataGrouping);
			Factory.Save();
		}

		void SetupTaxOrFeeData()
		{
			var dataGrouping = Core.Constants.CountryCodes.KoreaSouth;
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateRefCusTaxOrFeeType("FLA", "Flat");
			Factory.Save();
			helper.CreateTaxOrFee("AGA", 0.2m, dataGrouping, 0m, 0m, "FLA", description: "Agriculture Tax A");
			helper.CreateTaxOrFee("AGB", 0.1m, dataGrouping, 0m, 0m, "FLA", description: "Agriculture Tax B");
			helper.CreateTaxOrFee("VTA", 0.1m, dataGrouping, 0m, 0m, "FLA", description: "VAT Rate A");
			helper.CreateTaxOrFee("VTB", 0m, dataGrouping, 0m, 0m, "FLA", description: "VAT Rate B");
			helper.CreateTaxOrFee("VTC", 0.1m, dataGrouping, 0m, 0m, "FLA", description: "VAT Rate C");
			Factory.Save();
		}

		protected override void SetUp()
		{
			base.SetUp();
			KRCustomsRegistry.Instance.UNIPASSDeclarantID.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, "6N002");
		}
		protected override BaseJobDeclaration GetDeclaration()
		{
			var declaration = base.GetDeclaration();
			var message = declaration.Messages.AddNew();
			message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			return declaration;
		}

		protected override DutyCalculatorStrategy GetDutyCalculatorStrategy() => new DutyCalculatorStrategy((JobDeclaration)Declaration);

		protected override bool ExpectedShouldTruncate => true;

		protected override bool ExpectedCanBeNegative => true;

		protected override bool ExpectedShouldCalculateDuties => true;

		protected override ZString ExpectedEntryLineFeeType => Enterprise.Customs.KR.Messaging.ChargeTypeList.Codes.Duty;
	}
}
