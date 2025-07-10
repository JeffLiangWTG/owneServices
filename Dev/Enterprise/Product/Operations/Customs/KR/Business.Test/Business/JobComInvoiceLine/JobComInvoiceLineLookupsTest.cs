using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.KR.Messaging;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using static Enterprise.Customs.KR.Messaging.Constants;
using Constants = Enterprise.Customs.Universal.Constants;
using KRConstants = Enterprise.Customs.KR.Messaging.Constants;
using TariffTypes = Enterprise.Customs.KR.Messaging.Constants.ZZ.TariffTypes;

namespace Enterprise.Customs.KR.Business.Testing
{
	sealed class JobComInvoiceLineLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestInvoiceLine()
		{
			var parent = Factory.New<JobComInvoiceLine>();
			AssertEquals(parent.Lookups.InvoiceLine, parent);
		}

		public void TestSecondaryPreferences()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var tariffTypePK = UniversalReferenceTestDataHelper.CreateNewOrGetExistingRefCusTariffType(Factory, Core.Constants.CountryCodes.KoreaSouth, TariffTypes.DutyReductionExemption).PK;
			var tariff = helper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.KoreaSouth, tariffTypePK, "A088000101", ZDate.Today.AddDays(-2), ZDate.Today.AddDays(2));
			Factory.Save();

			var invoiceLine = Factory.New<JobComInvoiceLine>();
			var secondaryPreferences = invoiceLine.Lookups.SecondaryPreferences;
			AssertSame("Cached", TariffViewCollection.GetCachedCollection(Factory, Core.Constants.CountryCodes.KoreaSouth, TariffTypes.DutyReductionExemption, ZDateTime.Today), secondaryPreferences);
			AssertEquals("Tariff Matches", true, tariff.MatchesFilter(secondaryPreferences.CompleteFilter));
		}

		public void TestPrimaryPreferenceList()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var preferenceA = helper.CreatePreferenceForCountry("A", "기본세율", Core.Constants.CountryCodes.KoreaSouth);
			helper.CreatePreferenceForCountry("C2", "WTO협정세율(선택2)", Core.Constants.CountryCodes.KoreaSouth);
			helper.CreatePreferenceForCountry("FAU1", "한ㆍ호주 FTA협정세율(선택1)", Core.Constants.CountryCodes.KoreaSouth);
			var tariffType = helper.CreateTariffType(Core.Constants.CountryCodes.KoreaSouth, Universal.Constants.TariffTypes.HarmonizedSystem);
			Factory.Save();
			var tariff = helper.CreateTariff(Core.Constants.CountryCodes.KoreaSouth, tariffType.PK, "0712319000", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			var rateType = helper.CreateCusRateType(Core.Constants.CountryCodes.KoreaSouth, Constants.RateTypes.Duty);
			var rateCode = helper.CreateCusRateCode(Factory, KRConstants.ZZ.RateCodes.DutyAdValorem, rateType.PK);
			var rate = helper.CreateRate(tariff, rateCode.PK, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime, "VFD * 0.08", preferenceA.PK, "", Core.Constants.CountryCodes.KoreaSouth);
			var tradeGroup = helper.CreateTradeGroup(Core.Constants.CountryCodes.KoreaSouth, Core.Constants.CountryCodes.Australia, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.AddCountry(tradeGroup, Core.Constants.CountryCodes.Australia);
			helper.CreateCusApplicability(rate, tradeGroup, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			Factory.Save();

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Common.KR.KRJobMessageTypeList.Codes.Import;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			var primaryPreferences = (CodeDescriptionPairList)invoiceLine.Lookups.PrimaryPreferenceList;
			CombineAssertions(() =>
			{
				AssertEquals(3, primaryPreferences.Count);
				AssertEquals("A, C2, FAU1", primaryPreferences.CodesAsString);
			});

			invoiceLine.JI_Tariff = tariff.ZZ1_TariffCode;
			primaryPreferences = (CodeDescriptionPairList)invoiceLine.Lookups.PrimaryPreferenceList;
			CombineAssertions(() =>
			{
				AssertEquals(3, primaryPreferences.Count);
				AssertEquals("A, C2, FAU1", primaryPreferences.CodesAsString);
			});

			invoiceLine.JI_CountryOfOrigin = Core.Constants.CountryCodes.Australia;
			primaryPreferences = (CodeDescriptionPairList)invoiceLine.Lookups.PrimaryPreferenceList;
			CombineAssertions(() =>
			{
				AssertEquals(1, primaryPreferences.Count);
				AssertEquals("A", primaryPreferences.CodesAsString);
			});
		}

		public void TestFTAList()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(KRConstants.ZZ.NKCodeType.EXFTA, "Export FTA Code");
			helper.CreateNewOrGetExistingCusCodeType("EXTPF", "Export Preference Code");
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.KoreaSouth, "South Korea");

			SetCusCodeListAndAttribute(helper, "301", "비특혜", "EXTPF", KRConstants.ZZ.CodeListAttributeNames.TradePreferenceTradeGroup, KRConstants.ZZ.TradeGroups.All);
			SetCusCodeListAndAttribute(helper, "998", "해당없음", "EXTPF", KRConstants.ZZ.CodeListAttributeNames.TradePreferenceTradeGroup, KRConstants.ZZ.TradeGroups.All);
			SetCusCodeListAndAttribute(helper, "999", "기타", "EXTPF", KRConstants.ZZ.CodeListAttributeNames.TradePreferenceTradeGroup, KRConstants.ZZ.TradeGroups.All);

			var tradeGroup1 = helper.CreateTradeGroup(Core.Constants.CountryCodes.KoreaSouth, Core.Constants.CountryCodes.Chile, ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(1));
			helper.AddCountry(tradeGroup1, Core.Constants.CountryCodes.Chile);
			SetCusCodeListAndAttribute(helper, "101", "한-칠레", KRConstants.ZZ.NKCodeType.EXFTA, KRConstants.ZZ.CodeListAttributeNames.FTATradeGroup, Core.Constants.CountryCodes.Chile);

			var tradeGroup2 = helper.CreateTradeGroup(Core.Constants.CountryCodes.KoreaSouth, Core.Constants.CountryCodes.Singapore, ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(1));
			helper.AddCountry(tradeGroup2, Core.Constants.CountryCodes.Singapore);
			SetCusCodeListAndAttribute(helper, "102", "한-싱가포르", KRConstants.ZZ.NKCodeType.EXFTA, KRConstants.ZZ.CodeListAttributeNames.FTATradeGroup, Core.Constants.CountryCodes.Singapore);

			var tradeGroup3 = helper.CreateTradeGroup(Core.Constants.CountryCodes.KoreaSouth, "ASEAN", ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(1));
			helper.AddCountry(tradeGroup3, Core.Constants.CountryCodes.Singapore);
			SetCusCodeListAndAttribute(helper, "104", "한-아세안", KRConstants.ZZ.NKCodeType.EXFTA, KRConstants.ZZ.CodeListAttributeNames.FTATradeGroup, Core.Constants.CountryCodes.Singapore);

			var tradeGroup4 = helper.CreateTradeGroup(Core.Constants.CountryCodes.KoreaSouth, Core.Constants.CountryCodes.KoreaNorth, ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(1));
			helper.AddCountry(tradeGroup4, Core.Constants.CountryCodes.KoreaNorth);
			SetCusCodeListAndAttribute(helper, "201", "남북교역", "EXTPF", KRConstants.ZZ.CodeListAttributeNames.TradePreferenceTradeGroup, Core.Constants.CountryCodes.KoreaNorth);
			Factory.Save();

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Common.KR.KRJobMessageTypeList.Codes.Export;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.CertificateOfOriginIssueStatus = Messaging.CertificateOfOriginIssuedCodeList.Codes.Y;
			invoiceLine.JI_CountryOfOrigin = Core.Constants.CountryCodes.KoreaSouth;
			var ftaList1 = (CodeDescriptionPairList)invoiceLine.Lookups.PrimaryPreferenceList;
			AssertEquals(3, ftaList1.Count);
			AssertEquals("101", ftaList1[0].Code);
			AssertEquals("102", ftaList1[1].Code);
			AssertEquals("104", ftaList1[2].Code);

			declaration.JE_GoodsDestination = Core.Constants.CountryCodes.Chile;
			Factory.Save();
			var ftaList2 = (CodeDescriptionPairList)invoiceLine.Lookups.PrimaryPreferenceList;
			AssertEquals(1, ftaList2.Count);
			AssertEquals("101", ftaList2[0].Code);

			declaration.JE_GoodsDestination = Core.Constants.CountryCodes.Singapore;
			Factory.Save();
			var ftaList3 = (CodeDescriptionPairList)invoiceLine.Lookups.PrimaryPreferenceList;
			AssertEquals(2, ftaList3.Count);
			AssertEquals("102", ftaList3[0].Code);
			AssertEquals("104", ftaList3[1].Code);

			declaration.JE_GoodsDestination = Core.Constants.CountryCodes.KoreaNorth;
			Factory.Save();
			var ftaList4 = (CodeDescriptionPairList)invoiceLine.Lookups.PrimaryPreferenceList;
			AssertEquals(0, ftaList4.Count);

			declaration.JE_GoodsDestination = Core.Constants.CountryCodes.UnitedStates;
			Factory.Save();
			invoiceLine.CertificateOfOriginIssueStatus = Messaging.CertificateOfOriginIssuedCodeList.Codes.N;
			var ftaList6 = (CodeDescriptionPairList)invoiceLine.Lookups.PrimaryPreferenceList;
			AssertEquals(0, ftaList6.Count);

			invoiceLine.CertificateOfOriginIssueStatus = Messaging.CertificateOfOriginIssuedCodeList.Codes.Y;
			invoiceLine.JI_CountryOfOrigin = Core.Constants.CountryCodes.UnitedStates;
			var ftaList7 = (CodeDescriptionPairList)invoiceLine.Lookups.PrimaryPreferenceList;
			AssertEquals(0, ftaList7.Count);
		}

		public void TestFTAEffectiveAssessmentDate()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(KRConstants.ZZ.NKCodeType.EXFTA, "Export FTA Code");
			helper.CreateNewOrGetExistingCusCodeType("EXTPF", "Export Preference Code");
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.KoreaSouth, "South Korea");

			var tradeGroup = helper.CreateTradeGroup(Core.Constants.CountryCodes.KoreaSouth, Core.Constants.CountryCodes.Myanmar, new ZDateTime(2022, 01, 01), new ZDateTime(2022, 12, 31));
			helper.AddCountry(tradeGroup, Core.Constants.CountryCodes.Myanmar);
			SetCusCodeListAndAttribute(helper, "119", "RCEP", KRConstants.ZZ.NKCodeType.EXFTA, KRConstants.ZZ.CodeListAttributeNames.FTATradeGroup, Core.Constants.CountryCodes.Myanmar, new ZDateTime(2022, 01, 01), new ZDateTime(2022, 12, 31));
			Factory.Save();

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Common.KR.KRJobMessageTypeList.Codes.Export;
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.CertificateOfOriginIssueStatus = Messaging.CertificateOfOriginIssuedCodeList.Codes.Y;
			invoiceLine.JI_CountryOfOrigin = Core.Constants.CountryCodes.KoreaSouth;
			KRCustomsRegistry.Instance.UNIPASSDeclarantID.SetTemporaryValue(declaration.RegistryCompanyPK, Guid.Empty, Guid.Empty, "12345");
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			declaration.JE_GoodsDestination = Core.Constants.CountryCodes.Myanmar;
			invoiceLine.CusEntryLine.Header.EntryNumber = "1163921400096X";
			invoiceLine.CusEntryLine.Header.CusEntryNumber.CE_IssueDate = new ZDateTime(2021, 1, 1);
			Factory.Save();
			var ftaList = (CodeDescriptionPairList)invoiceLine.Lookups.PrimaryPreferenceList;
			AssertEquals(0, ftaList.Count);

			invoiceLine.CusEntryLine.Header.CusEntryNumber.CE_IssueDate = new ZDateTime(2022, 1, 1);
			Factory.Save();
			ftaList = (CodeDescriptionPairList)invoiceLine.Lookups.PrimaryPreferenceList;
			AssertEquals(1, ftaList.Count);
			AssertEquals("119", ftaList[0].Code);

			invoiceLine.CusEntryLine.Header.CusEntryNumber.CE_IssueDate = new ZDateTime(2023, 1, 1);
			Factory.Save();
			ftaList = (CodeDescriptionPairList)invoiceLine.Lookups.PrimaryPreferenceList;
			AssertEquals(0, ftaList.Count);
		}

		public void TestInvoiceUQList()
		{
			var invoiceLine = Factory.New<JobComInvoiceLine>();
			var invoiceUQList = invoiceLine.Lookups.InvoiceUQList;

			CombineAssertions(() =>
			{
				AssertEquals(94, invoiceUQList.Count);
				AssertEquals("BAG, BO, BOX, CAN, CM, CMK, CR, CT, CX, DZ, DRM, EA, FT, GR, GRM, GLI, GLL, GN, GRO, KG, KMT, KTN, L, LTR, LBR, PN, M, M2, SM, M3, MGM, ML, MLT, MMT, NO, ONZ, PC, PKG, PH, PK, 2U, RL, TNE, TB, U, YD, SH, 1B, 50, AM, AV, BAR, BC, BLL, BN, BR, BTL, BU, CMQ, CP, CQ, CTN, CUR, D63, GG, GT, HC, JPS, JR, KL, KT, KWT, LK, M5, MCU, MIU, MR, MTQ, MW, NIU, PP, PT, QT, RM, SET, SF, SR, SY, TH, TQ, TU, U2, UN, VI", invoiceUQList.CodesAsString);
			});

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Common.KR.KRJobMessageTypeList.Codes.Import;
			var invoice = declaration.Invoices.AddNew();
			invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceUQList = invoiceLine.Lookups.InvoiceUQList;
			CombineAssertions(() =>
			{
				AssertEquals(94, invoiceUQList.Count);
				AssertEquals("BAG, BO, BOX, CAN, CM, CMK, CR, CT, CX, DZ, DRM, EA, FT, GR, GRM, GLI, GLL, GN, GRO, KG, KMT, KTN, L, LTR, LBR, PN, M, M2, SM, M3, MGM, ML, MLT, MMT, NO, ONZ, PC, PKG, PH, PK, 2U, RL, TNE, TB, U, YD, SH, 1B, 50, AM, AV, BAR, BC, BLL, BN, BR, BTL, BU, CMQ, CP, CQ, CTN, CUR, D63, GG, GT, HC, JPS, JR, KL, KT, KWT, LK, M5, MCU, MIU, MR, MTQ, MW, NIU, PP, PT, QT, RM, SET, SF, SR, SY, TH, TQ, TU, U2, UN, VI", invoiceUQList.CodesAsString);
			});

			declaration.JE_MessageType = ElectronicDocumentTypeList.Codes._008;
			invoiceUQList = invoiceLine.Lookups.InvoiceUQList;

			CombineAssertions(() =>
			{
				AssertEquals(18, invoiceUQList.Count);
				AssertEquals("001, 002, 003, 004, 005, 006, 007, 008, 009, 010, 011, 012, 013, 014, 015, 016, 017, 900", invoiceUQList.CodesAsString);
			});
		}

		public void TestDutyRateSelectionList()
		{
			var dataGrouping = Core.Constants.CountryCodes.KoreaSouth;
			var referenceDataHelper = new UniversalReferenceTestDataHelper(Factory);
			var tariffType = referenceDataHelper.CreateTariffType(dataGrouping, Universal.Constants.TariffTypes.HarmonizedSystem);
			Factory.Save();

			var tariff1 = referenceDataHelper.CreateTariff(dataGrouping, tariffType.PK, "3706101000", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, "기타");
			referenceDataHelper.CreateTariffUOM(tariff1, "CU1", "KG");
			var preferenceC2 = referenceDataHelper.CreatePreferenceForCountry("C2", "WTO협정세율(선택2)", Core.Constants.CountryCodes.KoreaSouth);
			var rateType = referenceDataHelper.CreateCusRateType(dataGrouping, Universal.Constants.RateTypes.Duty);
			var rateCodeDTA = referenceDataHelper.CreateCusRateCode(Factory, ZZ.RateCodes.DutyAdValorem, rateType.PK);
			var rateAdValorem = referenceDataHelper.CreateRate(tariff1, rateCodeDTA.PK, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, "VFD * 0.065", dataGrouping: dataGrouping, preferencePk: preferenceC2.PK);
			var rateCodeDTS = referenceDataHelper.CreateCusRateCode(Factory, ZZ.RateCodes.DutySpecific, rateType.PK);
			var rateSpecific = referenceDataHelper.CreateRate(tariff1, rateCodeDTS.PK, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, "[KG] * 1560", dataGrouping: dataGrouping, preferencePk: preferenceC2.PK);
			var tradeGroup = referenceDataHelper.CreateTradeGroup("AU", "AU", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			referenceDataHelper.AddCountry(tradeGroup, Core.Constants.CountryCodes.Australia);
			referenceDataHelper.CreateCusApplicability(rateAdValorem, tradeGroup, ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTime.Date);
			referenceDataHelper.CreateCusApplicability(rateSpecific, tradeGroup, ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTime.Date);
			referenceDataHelper.CreateCusApplicability(rateAdValorem, null, ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTime.Date, additionalCode: ZZ.ApplicabilityAdditionalCodes.Max);
			referenceDataHelper.CreateCusApplicability(rateSpecific, null, ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTime.Date, additionalCode: ZZ.ApplicabilityAdditionalCodes.Max);
			referenceDataHelper.CreateCusApplicability(rateAdValorem, null, ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTime.Date, additionalCode: ZZ.ApplicabilityAdditionalCodes.Min);
			referenceDataHelper.CreateCusApplicability(rateSpecific, null, ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTime.Date, additionalCode: ZZ.ApplicabilityAdditionalCodes.Min);

			var tariff2 = referenceDataHelper.CreateTariff(dataGrouping, tariffType.PK, "3706101001", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, "기타");
			referenceDataHelper.CreateTariffUOM(tariff2, "CU1", "KG");
			rateAdValorem = referenceDataHelper.CreateRate(tariff2, rateCodeDTA.PK, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, "VFD * 0.01", dataGrouping: dataGrouping, preferencePk: preferenceC2.PK);
			rateSpecific = referenceDataHelper.CreateRate(tariff2, rateCodeDTS.PK, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, "[KG] * 2000", dataGrouping: dataGrouping, preferencePk: preferenceC2.PK);
			referenceDataHelper.CreateCusApplicability(rateAdValorem, tradeGroup, ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTime.Date);
			referenceDataHelper.CreateCusApplicability(rateSpecific, tradeGroup, ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTime.Date);
			referenceDataHelper.CreateCusApplicability(rateAdValorem, null, ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTime.Date, additionalCode: ZZ.ApplicabilityAdditionalCodes.Max);
			referenceDataHelper.CreateCusApplicability(rateSpecific, null, ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTime.Date, additionalCode: ZZ.ApplicabilityAdditionalCodes.Max);

			var tariff3 = referenceDataHelper.CreateTariff(dataGrouping, tariffType.PK, "3706101002", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, "기타");
			referenceDataHelper.CreateTariffUOM(tariff3, "CU1", "KG");
			Factory.Save();

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Common.KR.KRJobMessageTypeList.Codes.Import;
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.Tariff;
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
			var invoice = declaration.Invoices.AddNew();

			var invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "3706101000";
			invoiceLine.JI_PrimaryPreference = "C2";
			invoiceLine.JI_CountryOfOrigin = "AU";
			AssertContains("MIN", invoiceLine.Lookups.DutyRateSelectionList.CodesAsString);
			AssertContains("MAX", invoiceLine.Lookups.DutyRateSelectionList.CodesAsString);

			invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "3706101001";
			invoiceLine.JI_PrimaryPreference = "C2";
			invoiceLine.JI_CountryOfOrigin = "AU";
			AssertEquals("MAX", invoiceLine.Lookups.DutyRateSelectionList.CodesAsString);

			invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "3706101002";
			AssertEquals("", invoiceLine.Lookups.DutyRateSelectionList.CodesAsString);
		}

		void SetCusCodeListAndAttribute(UniversalReferenceTestDataHelper helper, ZString code, ZString description, ZString codeType, ZString attributeType, ZString country, ZDateTime startDate, ZDateTime endDate)
		{
			var codeListAndAttributeNames = helper.CreateCusCodeListWithAttributeNames(Core.Constants.CountryCodes.KoreaSouth,
									codeType, code, description, startDate, endDate,
									new[] { new KeyValuePair<string, string>(attributeType, country) });
			helper.CreateCusCodeListAttribute(codeListAndAttributeNames.PK, attributeType, country);
		}

		void SetCusCodeListAndAttribute(UniversalReferenceTestDataHelper helper, ZString code, ZString description, ZString codeType, ZString attributeType, ZString country)
		{
			SetCusCodeListAndAttribute(helper, code, description, codeType, attributeType, country, ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(1));
		}

		public void TestAllListAboutCertificateOfOrigin()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoice = declaration.Invoices.AddNew();

			var issuedCodeList = invoice.Lookups.CertificateOfOriginIssuedCodeList;
			var determinationRuleCodeList = invoice.Lookups.CountryOfOriginDeterminationRuleCodeList;
			var splitCodeList = invoice.Lookups.CertificateOfOriginSplitCodeList;

			CombineAssertions(() =>
			{
				AssertEquals(2, issuedCodeList.Count);
				AssertEquals("N, Y", issuedCodeList.CodesAsString);
				AssertEquals(CertificateOfOriginIssuedCodeList.Descriptions.N, issuedCodeList[0].Description);

				AssertEquals(12, determinationRuleCodeList.Count);
				AssertEquals("2, 4, 6, 8, A, B, C, D, E, F, G, H", determinationRuleCodeList.CodesAsString);

				AssertEquals(2, splitCodeList.Count);
				AssertEquals("N, Y", splitCodeList.CodesAsString);
				AssertEquals(CertificateOfOriginSplitCodeList.Descriptions.N, splitCodeList[0].Description);
			});
		}

		public void TestTaxOrFeeCodeList()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();

			var taxOrFeeCodeList = invoiceLine.Lookups.TaxOrFeeCodeList;

			AssertEquals(3, taxOrFeeCodeList.Count);
			AssertEquals("VTA, VTB, VTC", taxOrFeeCodeList.CodesAsString);
		}

		public void TestReductionRateRegulationList()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();

			var reductionRateRegulationList = invoiceLine.Lookups.ReductionRateRegulationList;

			AssertEquals(4, reductionRateRegulationList.Count);
			AssertEquals("01, 04, 05, 06", reductionRateRegulationList.CodesAsString);
			AssertEquals(ImportReductionRateRegulationList.Descriptions._01, reductionRateRegulationList[0].Description);
		}

		public void TestAdditionalDutyTypeCodeList()
		{
			var invoiceLine = CreateInvoiceLine();
			var additionalDutyTypeCodeList = invoiceLine.Lookups.AdditionalDutyTypeCodeList;
			CombineAssertions(() =>
			{
				AssertEquals(3, additionalDutyTypeCodeList.Count);
				AssertEquals("I, K, M", additionalDutyTypeCodeList.CodesAsString);
			});
		}

		public void TestTaxExemptionCodes()
		{
			SetTaxReductionExemptionData();

			var invoiceLine = CreateInvoiceLine();
			var taxExemptionCodes = invoiceLine.Lookups.TaxExemptionCodes;
			taxExemptionCodes.Load();
			CombineAssertions(() =>
			{
				AssertEquals(3, taxExemptionCodes.Count);
				Assert("The code with the attribute SCT exists", taxExemptionCodes.Cast<TariffView>().Any(x => x.ZZ1_TariffCode == "L190009"));
				Assert("The code with the attribute LQT exists", taxExemptionCodes.Cast<TariffView>().Any(x => x.ZZ1_TariffCode == "D310201"));
				Assert("The code with the attribute TRT exists", taxExemptionCodes.Cast<TariffView>().Any(x => x.ZZ1_TariffCode == "T120104"));
				AssertDefaultFilters(taxExemptionCodes);
			});
		}

		public void TestVATReductionCodes()
		{
			SetTaxReductionExemptionData();

			var invoiceLine = CreateInvoiceLine();
			var vatReductionCodes = invoiceLine.Lookups.VATReductionCodes;
			vatReductionCodes.Load();
			CombineAssertions(() =>
			{
				AssertEquals(1, vatReductionCodes.Count);
				Assert("The code with the attribute VAT exists", vatReductionCodes.Cast<TariffView>().Any(x => x.ZZ1_TariffCode == "E042001"));
				AssertDefaultFilters(vatReductionCodes);
			});
		}

		void AssertDefaultFilters(TariffViewCollection codes)
		{
			Assert("Default filter 'Tariff Type[Country/Region or Grouping]'", codes.FilterBusinessObjectDefaults.ContainsDefaultFor(Constants.RefCusTariffFilters.TariffType + FilterBusinessObjectDefault.FilterPropertyDelimiter + "Property1"));
			AssertEquals(Core.Constants.CountryCodes.KoreaSouth, codes.FilterBusinessObjectDefaults["Tariff Type:Property1"].Value);
			Assert("Default filter 'Tariff Type[Tariff Type]", codes.FilterBusinessObjectDefaults.ContainsDefaultFor(Constants.RefCusTariffFilters.TariffType + FilterBusinessObjectDefault.FilterPropertyDelimiter + "Property2"));
			AssertEquals(Messaging.Constants.ZZ.TariffTypes.DomesticTaxReductionExemption, codes.FilterBusinessObjectDefaults["Tariff Type:Property2"].Value);
			Assert("Default filter 'Tariff Code", codes.FilterBusinessObjectDefaults.ContainsDefaultFor(Constants.RefCusTariffFilters.TariffCode + FilterBusinessObjectDefault.FilterPropertyDelimiter + "Property"));
			AssertEquals(ZString.Empty, codes.FilterBusinessObjectDefaults["Tariff Code:Property"].Value);
			Assert("Default filter 'Effective Date", codes.FilterBusinessObjectDefaults.ContainsDefaultFor(Constants.RefCusTariffFilters.EffectiveDate + FilterBusinessObjectDefault.FilterPropertyDelimiter + "Property1"));
			AssertEquals(ZDate.Today, codes.FilterBusinessObjectDefaults["Effective Date:Property1"].Value);
		}

		public void TestDomesticTaxCodes()
		{
			SetupRefCusTariffs();
			var invoiceLine = CreateInvoiceLine();
			var domesticTaxCodes = invoiceLine.Lookups.DomesticTaxCodes;
			domesticTaxCodes.Load();
			CombineAssertions(() =>
			{
				AssertEquals(2, domesticTaxCodes.Count);
				Assert(domesticTaxCodes.Cast<TariffView>().Any(x => x.ZZ1_TariffCode == "411000"));
				Assert(domesticTaxCodes.Cast<TariffView>().Any(x => x.ZZ1_TariffCode == "941400"));
			});
		}

		public void TestInstallmentCodes()
		{
			SetupRefCusCodes();
			var invoiceLine = CreateInvoiceLine();
			var installmentCodes = invoiceLine.Lookups.InstallmentCodes;
			installmentCodes.Load();
			CombineAssertions(() =>
			{
				AssertEquals(2, installmentCodes.Count);
				Assert(installmentCodes.Cast<ZZRefCusCodeListCombined>().Any(x => x.ZZD_Code == "A088000101"));
				Assert(installmentCodes.Cast<ZZRefCusCodeListCombined>().Any(x => x.ZZD_Code == "A0900001020H"));
			});
		}

		void SetupRefCusCodes()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateCusCodeType(Messaging.Constants.ZZ.NKCodeType.InstallmentCode, "Instalment Code");
			helper.CreateCusCodeList(Core.Constants.CountryCodes.KoreaSouth, Messaging.Constants.ZZ.NKCodeType.InstallmentCode, "A088000101", ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTimeValue.Date);
			helper.CreateCusCodeList(Core.Constants.CountryCodes.KoreaSouth, Messaging.Constants.ZZ.NKCodeType.InstallmentCode, "A0900001020H", ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTimeValue.Date);
			Factory.Save();
		}

		void SetTaxReductionExemptionData()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var tariffTypeDTE = helper.CreateTariffType(Core.Constants.CountryCodes.KoreaSouth, Messaging.Constants.ZZ.TariffTypes.DomesticTaxReductionExemption);
			Factory.Save();
			var tariff1 = helper.CreateTariff(Core.Constants.CountryCodes.KoreaSouth, tariffTypeDTE.PK, "L190009", ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTimeValue.Date);
			var tariff2 = helper.CreateTariff(Core.Constants.CountryCodes.KoreaSouth, tariffTypeDTE.PK, "D310201", ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTimeValue.Date);
			var tariff3 = helper.CreateTariff(Core.Constants.CountryCodes.KoreaSouth, tariffTypeDTE.PK, "E042001", ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTimeValue.Date);
			var tariff4 = helper.CreateTariff(Core.Constants.CountryCodes.KoreaSouth, tariffTypeDTE.PK, "T120104", ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTimeValue.Date);

			helper.CreateTariffAttribute(Messaging.Constants.ZZ.TariffAttributeNames.TaxCode, Messaging.Constants.ZZ.TariffAttributes.SpecialConsumptionTax, tariff1);
			helper.CreateTariffAttribute(Messaging.Constants.ZZ.TariffAttributeNames.TaxCode, Messaging.Constants.ZZ.TariffAttributes.LiquorTax, tariff2);
			helper.CreateTariffAttribute(Messaging.Constants.ZZ.TariffAttributeNames.TaxCode, Messaging.Constants.ZZ.TariffAttributes.ValueAddedTax, tariff3);
			helper.CreateTariffAttribute(Messaging.Constants.ZZ.TariffAttributeNames.TaxCode, Messaging.Constants.ZZ.TariffAttributes.TransportationTax, tariff4);
			Factory.Save();
		}

		void SetupRefCusTariffs()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var tariffTypeDMT = helper.CreateTariffType(Core.Constants.CountryCodes.KoreaSouth, Messaging.Constants.ZZ.TariffTypes.DomesticTaxRate);
			var tariffTypeDRE = helper.CreateTariffType(Core.Constants.CountryCodes.KoreaSouth, Messaging.Constants.ZZ.TariffTypes.DutyReductionExemption);
			Factory.Save();
			helper.CreateTariff(Core.Constants.CountryCodes.KoreaSouth, tariffTypeDMT.PK, "411000", ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTimeValue.Date);
			helper.CreateTariff(Core.Constants.CountryCodes.KoreaSouth, tariffTypeDMT.PK, "941400", ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTimeValue.Date);
			helper.CreateTariff(Core.Constants.CountryCodes.KoreaSouth, tariffTypeDRE.PK, "A088000101", ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTimeValue.Date);
			helper.CreateTariff(Core.Constants.CountryCodes.KoreaSouth, tariffTypeDRE.PK, "A0900001020H", ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTimeValue.Date);
			Factory.Save();
		}

		public void TestOtherGovernmentAndAssociatedAgencyList()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateCusCodeType(Messaging.Constants.ZZ.NKCodeType.OtherGovernmentAndAssociatedAgency, "Other Government and Associated Agency");
			helper.CreateCusCodeList(Core.Constants.CountryCodes.KoreaSouth, Messaging.Constants.ZZ.NKCodeType.OtherGovernmentAndAssociatedAgency, "001", "과학기술부", ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTimeValue.Date);
			helper.CreateCusCodeList(Core.Constants.CountryCodes.KoreaSouth, Messaging.Constants.ZZ.NKCodeType.OtherGovernmentAndAssociatedAgency, "002", "국방부", ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTimeValue.Date);
			Factory.Save();

			var invoiceLine = CreateInvoiceLine();
			var otherGovernmentAndAssociatedAgencyList = invoiceLine.Lookups.OtherGovernmentAndAssociatedAgencyList;
			otherGovernmentAndAssociatedAgencyList.Load();
			CombineAssertions(() =>
			{
				AssertEquals(2, otherGovernmentAndAssociatedAgencyList.Count);
				Assert(otherGovernmentAndAssociatedAgencyList.Cast<ZZRefCusCodeListCombined>().Any(x => x.ZZD_Code == "001"));
				Assert(otherGovernmentAndAssociatedAgencyList.Cast<ZZRefCusCodeListCombined>().Any(x => x.ZZD_Code == "002"));
			});
		}

		JobComInvoiceLine CreateInvoiceLine()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			return invoice.InvoiceLines.AddNew();
		}
	}
}
