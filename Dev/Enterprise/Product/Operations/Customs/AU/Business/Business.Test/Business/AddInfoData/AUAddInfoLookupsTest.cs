using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	class AUAddInfoLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestEntryKeys()
		{
			JobDeclaration declaration = JobDeclaration.New(Factory);
			JobComInvoiceLine line = declaration.FilteredInvoiceLines.AddNew();
			line.SetDeclarationForTesting(declaration);
			AssertNotNull(line.AddInfo.Lookups.EntryKeys);
		}

		public void TestZA_PRFList() => CombineAssertions(() =>
		{
			addInfo.ZA_ORG = Core.Constants.CountryCodes.Germany;
			AssertEquals("AggregatedZA_ORG other than 'US' or 'TH'", string.Empty, lookups.ZA_PRFList.CodesAsString);

			addInfo.ZA_ORG = Core.Constants.CountryCodes.UnitedStates;
			AssertEquals("AggregatedZA_ORG = 'US'", "U, X", lookups.ZA_PRFList.CodesAsString);

			addInfo.ZA_ORG = Core.Constants.CountryCodes.Thailand;
			AssertEquals("AggregatedZA_ORG = 'TH'", "H, I, T, X", lookups.ZA_PRFList.CodesAsString);

			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Export;
			AssertEquals("Export declaration", string.Empty, lookups.ZA_PRFList.CodesAsString);
		});

		public void TestBondedWarehouseAddresses()
		{
			AssertEquals("Empty", 0, lookups.BondedWarehouseAddresses.Count);

			addInfo.WarehouseOrgFK = ZGuid.NewZGuid();
			AssertEquals("Empty", 0, lookups.BondedWarehouseAddresses.Count);

			OrgHeader org = OrgHeader.New(Factory);
			OrgAddress address1 = org.Addresses[0];
			addInfo.WarehouseOrgFK = org.PK;
			AssertEquals("Addresses", 1, lookups.BondedWarehouseAddresses.Count);
			AssertEquals("Address1", address1, lookups.BondedWarehouseAddresses[0]);

			OrgHeader org2 = OrgHeader.New(Factory);
			OrgAddress addressA = org2.Addresses[0];
			OrgAddress addressB = org2.Addresses.AddNew();
			addInfo.WarehouseOrgFK = org2.PK;
			AssertEquals("Addresses", 2, lookups.BondedWarehouseAddresses.Count);
			AssertEquals("AddressA", true, lookups.BondedWarehouseAddresses.Contains(addressA));
			AssertEquals("AddressB", true, lookups.BondedWarehouseAddresses.Contains(addressB));
		}

		public void TestBondedWarehouses()
		{
			AssertEquals("BondedWarehouses", typeof(BondedWarehouseCollection), lookups.BondedWarehouses.GetType());
		}

		public void TestOrgAddressList()
		{
			AssertEquals("AddInfo Org List", typeof(RefCountryCollection), lookups.ZA_ORG_List.GetType());
		}

		public void TestZA_InstrumentType_List()
		{
			var classification = Factory.New<Classification>();
			CodeDescriptionPairList result = classification.AddInfo.Lookups.ZA_InstrumentType_List;
			AssertEquals("Result should have both CMR and Edifice list", 7, result.Count);

			JobDeclaration testDec = JobDeclaration.New(Factory);
			testDec.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			testDec.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages;

			JobComInvoiceHeader invoice = testDec.Invoices.AddNew();
			JobComInvoiceLine invoiceLine = invoice.JobComInvoiceLines.AddNew();

			result = invoiceLine.AddInfo.Lookups.ZA_InstrumentType_List;
			AssertEquals("It should have only CMR list", 7, result.Count);
			AssertEquals("It should not have TC1", false, result.ContainsCode(CustomsInstrumentTypeList.Codes.TariffConcession));

			testDec.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceLegacyMessages;
			invoiceLine = invoice.JobComInvoiceLines.AddNew();
			result = invoiceLine.AddInfo.Lookups.ZA_InstrumentType_List;
			AssertEquals("It should have only Edifice list", 4, result.Count);
			AssertEquals("It should not have TC", false, result.ContainsCode(CMRInstrumentTypeList.Codes.TariffConcessionOrder));
		}

		public void TestCMRInstrumentNumberList()
		{
			JobDeclaration testDec = JobDeclaration.New(Factory);
			testDec.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			testDec.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages;

			JobComInvoiceHeader invoice = testDec.Invoices.AddNew();
			JobComInvoiceLine invoiceLine = invoice.JobComInvoiceLines.AddNew();

			invoiceLine.AddInfo.ZA_InstrumentType_Hidden = CMRInstrumentTypeList.Codes.TariffConcessionOrder;
			CMRInstrumentCollection result = invoiceLine.AddInfo.Lookups.CMRInstrumentNumberList;
			AssertEquals("Instrument Number should have Instrument type defaulted", true, result.FilterBusinessObjectDefaults.ContainsDefaultFor("Instrument Type" + FilterBusinessObjectDefault.FilterPropertyDelimiter + "Property"));
		}

		public void TestZA_TRN_WithPreferenceAtHeader()
		{
			AddGENRateNumberAndExpiredRateNumberForTreatmentRateNumber();

			JobComInvoiceHeader invoiceHeader = declaration.Invoices.AddNew();
			invoiceHeader.AddInfo.ZA_EFD = "030305";
			invoiceHeader.AddInfo.ZA_PST = "GEN";
			JobComInvoiceLine line = invoiceHeader.JobComInvoiceLines.AddNew();
			line.JI_Tariff = "4901.10.00 01";
			line.AddInfo.ZA_TreatmentCode_Hidden = "987";

			lookups = line.AddInfo.Lookups;
			CodeDescriptionPairList rateNumberList = lookups.ZA_TRN_List;
			AssertNotNull("RateNumberList", rateNumberList);
			AssertEquals("Has 1 in list", 1, rateNumberList.Count);
			AssertEquals("Code", true, rateNumberList.ContainsCode("002"));
			AssertEquals("Expired Code", false, rateNumberList.ContainsCode("003"));
		}

		public void TestZA_TRN_ListAllRateCodes()
		{
			AddGENRateNumberAndExpiredRateNumberForTreatmentRateNumber();

			CMRTreatmentRatePeriodSnapshot rateNumber001 = CMRTreatmentRatePeriodSnapshot.New(Factory);
			rateNumber001.TP_StartDate = new ZDateTime(2005, 03, 03);
			rateNumber001.TP_RateNumber = "001";
			rateNumber001.TP_PreferenceSchemeType = "GEN";
			rateNumber001.TP_CalculationType = "FREE";
			rateNumber001.TP_Code = "987";

			CMRTreatmentRatePeriodSnapshot rateNumber003 = CMRTreatmentRatePeriodSnapshot.New(Factory);
			rateNumber003.TP_StartDate = new ZDateTime(2005, 03, 03);
			rateNumber003.TP_RateNumber = "003";
			rateNumber003.TP_PreferenceSchemeType = "GEN";
			rateNumber003.TP_CalculationType = "FREE";
			rateNumber003.TP_Code = "987";

			CMRTreatmentRatePeriodSnapshot rateNumber044 = CMRTreatmentRatePeriodSnapshot.New(Factory);
			rateNumber044.TP_StartDate = new ZDateTime(2005, 03, 03);
			rateNumber044.TP_RateNumber = "044";
			rateNumber044.TP_PreferenceSchemeType = "GEN";
			rateNumber044.TP_CalculationType = "FREE";
			rateNumber044.TP_Code = "987";

			JobComInvoiceHeader invoiceHeader = declaration.Invoices.AddNew();
			invoiceHeader.AddInfo.ZA_EFD = "030305";
			JobComInvoiceLine line = invoiceHeader.JobComInvoiceLines.AddNew();
			line.JI_Tariff = "4901.10.00 01";
			line.AddInfo.ZA_TreatmentCode_Hidden = "987";
			line.AddInfo.ZA_PST = "GEN";

			lookups = line.AddInfo.Lookups;

			CodeDescriptionPairList rateNumberList = lookups.ZA_TRN_List;
			AssertNotNull("RateNumberList", rateNumberList);
			AssertEquals("Has 4 in list", 4, rateNumberList.Count);
			AssertEquals("001", true, rateNumberList.ContainsCode("001"));
			AssertEquals("044", true, rateNumberList.ContainsCode("044"));
			AssertEquals("003", true, rateNumberList.ContainsCode("003"));
			AssertEquals("002", true, rateNumberList.ContainsCode("002"));
		}

		public void TestZA_TRN_ListWithPreferenceGENAndBasicTariff()
		{
			AddGENRateNumberAndExpiredRateNumberForTreatmentRateNumber();

			JobComInvoiceHeader invoiceHeader = declaration.Invoices.AddNew();
			invoiceHeader.AddInfo.ZA_EFD = "030305";
			JobComInvoiceLine line = invoiceHeader.JobComInvoiceLines.AddNew();
			line.JI_Tariff = "4901.10.00 01";
			line.AddInfo.ZA_TreatmentCode_Hidden = "987";
			line.AddInfo.ZA_PST = "GEN";

			lookups = line.AddInfo.Lookups;

			CodeDescriptionPairList rateNumberList = lookups.ZA_TRN_List;
			AssertNotNull("RateNumberList", rateNumberList);
			AssertEquals("Has 1 in list", 1, rateNumberList.Count);
			AssertEquals("Code", true, rateNumberList.ContainsCode("002"));
			AssertEquals("Expired Code", false, rateNumberList.ContainsCode("003"));
		}

		public void TestZA_TRN_ListWithNoInvoiceLines()
		{
			AddGENRateNumberAndExpiredRateNumberForTreatmentRateNumber();

			JobComInvoiceHeader invoiceHeader = declaration.Invoices.AddNew();
			invoiceHeader.AddInfo.ZA_EFD = "030305";
			CodeDescriptionPairList rateNumberList = invoiceHeader.AddInfo.Lookups.ZA_TRN_List;
			AssertEquals("RateNumberList", 0, rateNumberList.Count);
		}

		public void TestTreamentRateNumberDescriptionForDutyFree()
		{
			AddGENRateNumberAndExpiredRateNumberForTreatmentRateNumber();

			JobComInvoiceHeader invoiceHeader = declaration.Invoices.AddNew();
			invoiceHeader.AddInfo.ZA_EFD = "030305";
			JobComInvoiceLine line = invoiceHeader.JobComInvoiceLines.AddNew();
			line.AddInfo.ZA_PST = "GEN";
			line.JI_Tariff = "4901.10.00";
			line.AddInfo.ZA_TreatmentCode_Hidden = "987";
			lookups = line.AddInfo.Lookups;

			CodeDescriptionPairList rateNumberList = lookups.ZA_TRN_List;
			AssertEquals("Description", "Duty (GEN, 002):FREE", rateNumberList.GetDescriptionFromCode("002"));
		}

		public void TestTreamentRateNumberDescriptionForPercentage()
		{
			CMRTreatmentRatePeriodSnapshot sGRateNumber = CMRTreatmentRatePeriodSnapshot.New(Factory);
			sGRateNumber.TP_StartDate = new ZDateTime(2005, 03, 03);
			sGRateNumber.TP_RateNumber = "002";
			sGRateNumber.TP_PreferenceSchemeType = "SG";
			sGRateNumber.TP_CalculationType = "CALC";
			sGRateNumber.TP_CustomsValueRate = 5.12345M;
			sGRateNumber.TP_Code = "987";

			JobComInvoiceHeader invoiceHeader = declaration.Invoices.AddNew();
			invoiceHeader.AddInfo.ZA_EFD = "030305";
			JobComInvoiceLine line = invoiceHeader.JobComInvoiceLines.AddNew();
			line.AddInfo.ZA_PST = "GEN";
			line.JI_Tariff = "4901.10.00";
			line.AddInfo.ZA_TreatmentCode_Hidden = "987";
			lookups = line.AddInfo.Lookups;

			line.AddInfo.ZA_PST = "SG";
			CodeDescriptionPairList rateNumberList = lookups.ZA_TRN_List;
			AssertEquals("Description", "Duty (SG, 002):5.12345%", rateNumberList.GetDescriptionFromCode("002"));
		}

		public void TestTreamentRateNumberDescriptionForRate()
		{
			CMRTreatmentRatePeriodSnapshot sGRateNumber = CMRTreatmentRatePeriodSnapshot.New(Factory);
			sGRateNumber.TP_StartDate = new ZDateTime(2005, 03, 03);
			sGRateNumber.TP_RateNumber = "002";
			sGRateNumber.TP_PreferenceSchemeType = "SG";
			sGRateNumber.TP_CalculationType = "CALC";
			sGRateNumber.TP_QuantityRate = 87.89654M;
			sGRateNumber.TP_QuantityUnit = "L";
			sGRateNumber.TP_Code = "987";

			JobComInvoiceHeader invoiceHeader = declaration.Invoices.AddNew();
			invoiceHeader.AddInfo.ZA_EFD = "030305";
			JobComInvoiceLine line = invoiceHeader.JobComInvoiceLines.AddNew();
			line.AddInfo.ZA_PST = "GEN";
			line.JI_Tariff = "4901.10.00";
			line.AddInfo.ZA_TreatmentCode_Hidden = "987";
			lookups = line.AddInfo.Lookups;

			line.AddInfo.ZA_PST = "SG";
			CodeDescriptionPairList rateNumberList = lookups.ZA_TRN_List;
			AssertEquals("Description", "Duty (SG, 002):$87.89654/L", rateNumberList.GetDescriptionFromCode("002"));
		}

		public void TestTreamentRateNumberDescriptionForSecondPercentage()
		{
			CMRTreatmentRatePeriodSnapshot sGRateNumber = CMRTreatmentRatePeriodSnapshot.New(Factory);
			sGRateNumber.TP_StartDate = new ZDateTime(2005, 03, 03);
			sGRateNumber.TP_RateNumber = "002";
			sGRateNumber.TP_PreferenceSchemeType = "SG";
			sGRateNumber.TP_CalculationType = "CALC";
			sGRateNumber.TP_OtherDutyFactorRate = 7.89456M;
			sGRateNumber.TP_Code = "987";

			JobComInvoiceHeader invoiceHeader = declaration.Invoices.AddNew();
			invoiceHeader.AddInfo.ZA_EFD = "030305";
			JobComInvoiceLine line = invoiceHeader.JobComInvoiceLines.AddNew();
			line.AddInfo.ZA_PST = "GEN";
			line.JI_Tariff = "4901.10.00";
			line.AddInfo.ZA_TreatmentCode_Hidden = "987";
			lookups = line.AddInfo.Lookups;

			line.AddInfo.ZA_PST = "SG";
			CodeDescriptionPairList rateNumberList = lookups.ZA_TRN_List;
			AssertEquals("Description", "Duty (SG, 002):7.89456%", rateNumberList.GetDescriptionFromCode("002"));
		}

		public void TestTreamentRateNumberDescriptionForSecondRate()
		{
			CMRTreatmentRatePeriodSnapshot sGRateNumber = CMRTreatmentRatePeriodSnapshot.New(Factory);
			sGRateNumber.TP_StartDate = new ZDateTime(2005, 03, 03);
			sGRateNumber.TP_RateNumber = "002";
			sGRateNumber.TP_PreferenceSchemeType = "SG";
			sGRateNumber.TP_CalculationType = "CALC";
			sGRateNumber.TP_SecondQuantityRate = 56.56987M;
			sGRateNumber.TP_SecondQuantityUnit = "LA";
			sGRateNumber.TP_Code = "987";

			JobComInvoiceHeader invoiceHeader = declaration.Invoices.AddNew();
			invoiceHeader.AddInfo.ZA_EFD = "030305";
			JobComInvoiceLine line = invoiceHeader.JobComInvoiceLines.AddNew();
			line.AddInfo.ZA_PST = "GEN";
			line.JI_Tariff = "4901.10.00";
			line.AddInfo.ZA_TreatmentCode_Hidden = "987";
			lookups = line.AddInfo.Lookups;

			line.AddInfo.ZA_PST = "SG";
			CodeDescriptionPairList rateNumberList = lookups.ZA_TRN_List;
			AssertEquals("Description", "Duty (SG, 002):$56.56987/LA", rateNumberList.GetDescriptionFromCode("002"));
		}

		public void TestTreamentRateNumberFullDescription()
		{
			CMRTreatmentRatePeriodSnapshot sGRateNumber = CMRTreatmentRatePeriodSnapshot.New(Factory);
			sGRateNumber.TP_StartDate = new ZDateTime(2005, 03, 03);
			sGRateNumber.TP_RateNumber = "002";
			sGRateNumber.TP_PreferenceSchemeType = "SG";
			sGRateNumber.TP_CalculationType = "CALC";
			sGRateNumber.TP_CustomsValueRate = 5.12345M;
			sGRateNumber.TP_QuantityRate = 87.89654M;
			sGRateNumber.TP_QuantityUnit = "L";
			sGRateNumber.TP_OtherDutyFactorRate = 7.89456M;
			sGRateNumber.TP_SecondQuantityRate = 56.56987M;
			sGRateNumber.TP_SecondQuantityUnit = "LA";
			sGRateNumber.TP_Code = "987";

			JobComInvoiceHeader invoiceHeader = declaration.Invoices.AddNew();
			invoiceHeader.AddInfo.ZA_EFD = "030305";
			JobComInvoiceLine line = invoiceHeader.JobComInvoiceLines.AddNew();
			line.AddInfo.ZA_PST = "GEN";
			line.AddInfo.ZA_TreatmentCode_Hidden = "987";
			line.JI_Tariff = "4901.10.00";
			lookups = line.AddInfo.Lookups;

			line.AddInfo.ZA_PST = "SG";
			CodeDescriptionPairList rateNumberList = lookups.ZA_TRN_List;
			AssertEquals("Description", "Duty (SG, 002):5.12345% + $87.89654/L + $56.56987/LA + 7.89456%", rateNumberList.GetDescriptionFromCode("002"));
		}

		public void TestZA_RNO_WithPreferenceAtHeader()
		{
			AddGENRateNumberAndExpiredRateNumber();

			JobComInvoiceHeader invoiceHeader = declaration.Invoices.AddNew();
			invoiceHeader.AddInfo.ZA_EFD = "030305";
			invoiceHeader.AddInfo.ZA_PST = "GEN";
			JobComInvoiceLine line = invoiceHeader.JobComInvoiceLines.AddNew();
			line.JI_Tariff = "4901.10.00 01";

			lookups = line.AddInfo.Lookups;
			CodeDescriptionPairList rateNumberList = lookups.ZA_RNO_List;
			AssertNotNull("RateNumberList", rateNumberList);
			AssertEquals("Has 1 in list", 1, rateNumberList.Count);
			AssertEquals("001", true, rateNumberList.ContainsCode("001"));
		}

		public void TestZA_RNO_ListHasAllGENRateCodes()
		{
			AddGENRateNumberAndExpiredRateNumber();

			CMRTariffRatePeriodSnapshot rateNumber002 = CMRTariffRatePeriodSnapshot.New(Factory);
			rateNumber002.TT_StartDate = new ZDateTime(2005, 03, 03);
			rateNumber002.TT_RateNumber = "002";
			rateNumber002.TT_PreferenceSchemeType = "GEN";
			rateNumber002.TT_CalculationType = "FREE";
			rateNumber002.TT_TariffClassificationNumber = "49011000";

			CMRTariffRatePeriodSnapshot rateNumber003 = CMRTariffRatePeriodSnapshot.New(Factory);
			rateNumber003.TT_StartDate = new ZDateTime(2005, 03, 03);
			rateNumber003.TT_RateNumber = "003";
			rateNumber003.TT_PreferenceSchemeType = "GEN";
			rateNumber003.TT_CalculationType = "FREE";
			rateNumber003.TT_TariffClassificationNumber = "49011000";

			CMRTariffRatePeriodSnapshot rateNumber044 = CMRTariffRatePeriodSnapshot.New(Factory);
			rateNumber044.TT_StartDate = new ZDateTime(2005, 03, 03);
			rateNumber044.TT_RateNumber = "044";
			rateNumber044.TT_PreferenceSchemeType = "GEN";
			rateNumber044.TT_CalculationType = "FREE";
			rateNumber044.TT_TariffClassificationNumber = "49011000";

			JobComInvoiceHeader invoiceHeader = declaration.Invoices.AddNew();
			invoiceHeader.AddInfo.ZA_EFD = "030305";
			JobComInvoiceLine line = invoiceHeader.JobComInvoiceLines.AddNew();
			line.JI_Tariff = "4901.10.00 01";
			line.AddInfo.ZA_PST = "GEN";

			lookups = line.AddInfo.Lookups;

			CodeDescriptionPairList rateNumberList = lookups.ZA_RNO_List;
			AssertNotNull("RateNumberList", rateNumberList);
			AssertEquals("Has 4 in list", 4, rateNumberList.Count);
			AssertEquals("001", true, rateNumberList.ContainsCode("001"));
			AssertEquals("044", true, rateNumberList.ContainsCode("044"));
			AssertEquals("003", true, rateNumberList.ContainsCode("003"));
			AssertEquals("002", true, rateNumberList.ContainsCode("002"));
		}

		public void TestZA_RNO_ListWithPreferenceGENAndBasicTariff()
		{
			AddGENRateNumberAndExpiredRateNumber();

			JobComInvoiceHeader invoiceHeader = declaration.Invoices.AddNew();
			invoiceHeader.AddInfo.ZA_EFD = "030305";
			JobComInvoiceLine line = invoiceHeader.JobComInvoiceLines.AddNew();
			line.JI_Tariff = "4901.10.00 01";
			line.AddInfo.ZA_PST = "GEN";

			lookups = line.AddInfo.Lookups;

			CodeDescriptionPairList rateNumberList = lookups.ZA_RNO_List;
			AssertNotNull("RateNumberList", rateNumberList);
			AssertEquals("Has at least one in list", true, rateNumberList.Count > 0);
			AssertEquals("Code", true, rateNumberList.ContainsCode("001"));
			AssertEquals("Expired Code", false, rateNumberList.ContainsCode("044"));
		}

		public void TestZA_RNO_ListChangesOnPreferenceChange()
		{
			AddGENRateNumberAndExpiredRateNumber();

			CMRPreferenceSchemePeriodCountry schemeCountryUS = CMRPreferenceSchemePeriodCountry.New(Factory);
			schemeCountryUS.PC_PreferenceSchemePeriodSnapshotSchemeType = "US";
			schemeCountryUS.PC_CountryCode = "US";

			JobComInvoiceHeader invoiceHeader = declaration.Invoices.AddNew();
			invoiceHeader.AddInfo.ZA_EFD = "030305";
			JobComInvoiceLine line = invoiceHeader.JobComInvoiceLines.AddNew();
			line.JI_Tariff = "4901.10.00 01";
			line.JI_CountryOfOrigin = "US";

			lookups = line.AddInfo.Lookups;

			AssertEquals("Is general rate now", true, line.IsGeneralRate);
			AssertEquals("Rate number defaulted", "001", line.AddInfo.ZA_RNO);

			line.AddInfo.ZA_PST = "GEN";
			AssertEquals("Rate Number defaulted", "001", line.AddInfo.ZA_RNO);
		}

		public void TestZA_RNO_ListChangesOnTariffChange()
		{
			AddGENRateNumberAndExpiredRateNumber();

			JobComInvoiceHeader invoiceHeader = declaration.Invoices.AddNew();
			invoiceHeader.AddInfo.ZA_EFD = "030305";
			JobComInvoiceLine line = invoiceHeader.JobComInvoiceLines.AddNew();
			line.AddInfo.ZA_PST = "GEN";
			lookups = line.AddInfo.Lookups;

			CodeDescriptionPairList rateNumberList = lookups.ZA_RNO_List;
			AssertNotNull("RateNumberList", rateNumberList);
			AssertEquals("Has at least one in list", false, rateNumberList.Count > 0);

			line.JI_Tariff = "4901.10.00 01";
			AssertEquals("Rate Number defaulted", "001", line.AddInfo.ZA_RNO);
		}

		public void TestZA_RNO_ListWithWeirdTariff()
		{
			AddGENRateNumberAndExpiredRateNumber();

			JobComInvoiceHeader invoiceHeader = declaration.Invoices.AddNew();
			invoiceHeader.AddInfo.ZA_EFD = "030305";
			JobComInvoiceLine line = invoiceHeader.JobComInvoiceLines.AddNew();
			line.AddInfo.ZA_PST = "GEN";
			line.JI_Tariff = "4 90 1.1 0.00";
			lookups = line.AddInfo.Lookups;

			CodeDescriptionPairList rateNumberList = lookups.ZA_RNO_List;
			AssertNotNull("RateNumberList", rateNumberList);
			AssertEquals("Has at least one in list", true, rateNumberList.Count > 0);
			AssertEquals("Code", true, rateNumberList.ContainsCode("001"));
			AssertEquals("Expired Code", false, rateNumberList.ContainsCode("044"));

			line.JI_Tariff = "4 90 1.1";
			rateNumberList = lookups.ZA_RNO_List;
			AssertNotNull("RateNumberList", rateNumberList);
			AssertEquals("Has at least one in list", false, rateNumberList.Count > 0);
		}

		public void TestZA_RNO_ListWithNoInvoiceLines()
		{
			AddGENRateNumberAndExpiredRateNumber();

			JobComInvoiceHeader invoiceHeader = declaration.Invoices.AddNew();
			invoiceHeader.AddInfo.ZA_EFD = "030305";
			CodeDescriptionPairList rateNumberList = invoiceHeader.AddInfo.Lookups.ZA_RNO_List;
			AssertEquals("RateNumberList", 0, rateNumberList.Count);
		}

		public void TestRateNumberDescriptionForDutyFree()
		{
			AddGENRateNumberAndExpiredRateNumber();

			JobComInvoiceHeader invoiceHeader = declaration.Invoices.AddNew();
			invoiceHeader.AddInfo.ZA_EFD = "030305";
			JobComInvoiceLine line = invoiceHeader.JobComInvoiceLines.AddNew();
			line.AddInfo.ZA_PST = "GEN";
			line.JI_Tariff = "4901.10.00";
			lookups = line.AddInfo.Lookups;

			CodeDescriptionPairList rateNumberList = lookups.ZA_RNO_List;
			AssertEquals("Description", "Duty: FREE", rateNumberList.GetDescriptionFromCode("001"));
		}

		public void TestRateNumberDescriptionForPercentage()
		{
			CMRTariffRatePeriodSnapshot sGRateNumber = CMRTariffRatePeriodSnapshot.New(Factory);
			sGRateNumber.TT_StartDate = new ZDateTime(2005, 03, 03);
			sGRateNumber.TT_RateNumber = "001";
			sGRateNumber.TT_PreferenceSchemeType = "SG";
			sGRateNumber.TT_CalculationType = "CALC";
			sGRateNumber.TT_CustomsValueRate = 5.12345M;
			sGRateNumber.TT_TariffClassificationNumber = "49011000";

			JobComInvoiceHeader invoiceHeader = declaration.Invoices.AddNew();
			invoiceHeader.AddInfo.ZA_EFD = "030305";
			JobComInvoiceLine line = invoiceHeader.JobComInvoiceLines.AddNew();
			line.AddInfo.ZA_PST = "GEN";
			line.JI_Tariff = "4901.10.00";
			lookups = line.AddInfo.Lookups;

			line.AddInfo.ZA_PST = "SG";
			CodeDescriptionPairList rateNumberList = lookups.ZA_RNO_List;
			AssertEquals("Description", "Duty: 5.12345%", rateNumberList.GetDescriptionFromCode("001"));
		}

		public void TestRateNumberDescriptionForRate()
		{
			CMRTariffRatePeriodSnapshot sGRateNumber = CMRTariffRatePeriodSnapshot.New(Factory);
			sGRateNumber.TT_StartDate = new ZDateTime(2005, 03, 03);
			sGRateNumber.TT_RateNumber = "001";
			sGRateNumber.TT_PreferenceSchemeType = "SG";
			sGRateNumber.TT_CalculationType = "CALC";
			sGRateNumber.TT_QuantityRate = 87.89654M;
			sGRateNumber.TT_QuantityUnit = "L";
			sGRateNumber.TT_TariffClassificationNumber = "49011000";

			JobComInvoiceHeader invoiceHeader = declaration.Invoices.AddNew();
			invoiceHeader.AddInfo.ZA_EFD = "030305";
			JobComInvoiceLine line = invoiceHeader.JobComInvoiceLines.AddNew();
			line.AddInfo.ZA_PST = "GEN";
			line.JI_Tariff = "4901.10.00";
			lookups = line.AddInfo.Lookups;

			line.AddInfo.ZA_PST = "SG";
			CodeDescriptionPairList rateNumberList = lookups.ZA_RNO_List;
			AssertEquals("Description", "Duty: $87.89654/L", rateNumberList.GetDescriptionFromCode("001"));
		}

		public void TestRateNumberDescriptionForSecondPercentage()
		{
			CMRTariffRatePeriodSnapshot sGRateNumber = CMRTariffRatePeriodSnapshot.New(Factory);
			sGRateNumber.TT_StartDate = new ZDateTime(2005, 03, 03);
			sGRateNumber.TT_RateNumber = "001";
			sGRateNumber.TT_PreferenceSchemeType = "SG";
			sGRateNumber.TT_CalculationType = "CALC";
			sGRateNumber.TT_OtherDutyFactorRate = 7.89456M;
			sGRateNumber.TT_TariffClassificationNumber = "49011000";

			JobComInvoiceHeader invoiceHeader = declaration.Invoices.AddNew();
			invoiceHeader.AddInfo.ZA_EFD = "030305";
			JobComInvoiceLine line = invoiceHeader.JobComInvoiceLines.AddNew();
			line.AddInfo.ZA_PST = "GEN";
			line.JI_Tariff = "4901.10.00";
			lookups = line.AddInfo.Lookups;

			line.AddInfo.ZA_PST = "SG";
			CodeDescriptionPairList rateNumberList = lookups.ZA_RNO_List;
			AssertEquals("Description", "Duty: 7.89456%", rateNumberList.GetDescriptionFromCode("001"));
		}

		public void TestRateNumberDescriptionForSecondRate()
		{
			CMRTariffRatePeriodSnapshot sGRateNumber = CMRTariffRatePeriodSnapshot.New(Factory);
			sGRateNumber.TT_StartDate = new ZDateTime(2005, 03, 03);
			sGRateNumber.TT_RateNumber = "001";
			sGRateNumber.TT_PreferenceSchemeType = "SG";
			sGRateNumber.TT_CalculationType = "CALC";
			sGRateNumber.TT_SecondQuantityRate = 56.56987M;
			sGRateNumber.TT_SecondQuantityUnit = "LA";
			sGRateNumber.TT_TariffClassificationNumber = "49011000";

			JobComInvoiceHeader invoiceHeader = declaration.Invoices.AddNew();
			invoiceHeader.AddInfo.ZA_EFD = "030305";
			JobComInvoiceLine line = invoiceHeader.JobComInvoiceLines.AddNew();
			line.AddInfo.ZA_PST = "GEN";
			line.JI_Tariff = "4901.10.00";
			lookups = line.AddInfo.Lookups;

			line.AddInfo.ZA_PST = "SG";
			CodeDescriptionPairList rateNumberList = lookups.ZA_RNO_List;
			AssertEquals("Description", "Duty: $56.56987/LA", rateNumberList.GetDescriptionFromCode("001"));
		}

		public void TestRateNumberFullDescription()
		{
			CMRTariffRatePeriodSnapshot sGRateNumber = CMRTariffRatePeriodSnapshot.New(Factory);
			sGRateNumber.TT_StartDate = new ZDateTime(2005, 03, 03);
			sGRateNumber.TT_RateNumber = "001";
			sGRateNumber.TT_PreferenceSchemeType = "SG";
			sGRateNumber.TT_CalculationType = "CALC";
			sGRateNumber.TT_CustomsValueRate = 5.12345M;
			sGRateNumber.TT_QuantityRate = 87.89654M;
			sGRateNumber.TT_QuantityUnit = "L";
			sGRateNumber.TT_OtherDutyFactorRate = 7.89456M;
			sGRateNumber.TT_SecondQuantityRate = 56.56987M;
			sGRateNumber.TT_SecondQuantityUnit = "LA";
			sGRateNumber.TT_TariffClassificationNumber = "49011000";

			JobComInvoiceHeader invoiceHeader = declaration.Invoices.AddNew();
			invoiceHeader.AddInfo.ZA_EFD = "030305";
			JobComInvoiceLine line = invoiceHeader.JobComInvoiceLines.AddNew();
			line.AddInfo.ZA_PST = "GEN";
			line.JI_Tariff = "4901.10.00";
			lookups = line.AddInfo.Lookups;

			line.AddInfo.ZA_PST = "SG";
			CodeDescriptionPairList rateNumberList = lookups.ZA_RNO_List;
			AssertEquals("Description", "Duty: 5.12345% + $87.89654/L + $56.56987/LA + 7.89456%", rateNumberList.GetDescriptionFromCode("001"));
		}

		public void TestHeaderValuationBasisList()
		{
			AssertEquals("HeaderValuationBasisList", 9, lookups.HeaderValuationBasisListForEDIFICE.Count);
		}

		public void TestZA_ValuationBasis_Hidden_List()
		{
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceLegacyMessages;

			JobComInvoiceHeader invoiceHeader = declaration.Invoices.AddNew();
			JobComInvoiceLine invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			AssertEquals("ZA_ValuationBasis_Hidden_List", 5, invoiceLine.AddInfo.Lookups.ZA_ValuationBasis_Hidden_List.Count);
		}

		public void TestValuationBasisListForCMR()
		{
			declaration = JobDeclaration.New(Factory);
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages;
			JobComInvoiceHeader invoiceHeader = declaration.Invoices.AddNew();
			JobComInvoiceLine invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			AssertEquals("ValuationBasisListForCMR", 6, invoiceLine.AddInfo.Lookups.ValuationBasisListForCMR.Count);
		}

		public void TestTreatmentCodeListForAClassification()
		{
			var classification = Factory.New<Classification>();

			CMRTreatmentRatePeriodSnapshot treatmentSnapshot1 = CMRTreatmentRatePeriodSnapshot.New(Factory);
			treatmentSnapshot1.TP_Code = "CCC";
			treatmentSnapshot1.TP_RateNumber = "001";
			treatmentSnapshot1.TP_PreferenceSchemeType = "GEN";
			treatmentSnapshot1.TP_StartDate = new ZDateTime(2005, 1, 1);
			treatmentSnapshot1.TP_CalculationType = "CALC";
			treatmentSnapshot1.TP_CustomsValueRate = 5m;

			CMRTreatmentRatePeriodSnapshot treatmentSnapshot2 = CMRTreatmentRatePeriodSnapshot.New(Factory);
			treatmentSnapshot2.TP_Code = "AAA";
			treatmentSnapshot2.TP_RateNumber = "001";
			treatmentSnapshot2.TP_PreferenceSchemeType = "GEN";
			treatmentSnapshot2.TP_StartDate = new ZDateTime(2005, 1, 1);
			treatmentSnapshot2.TP_CalculationType = "INCALC";

			CodeDescriptionPairList treatmentList = classification.AddInfo.Lookups.TreatmentCodeList;
			AssertNotNull("TreatmentList", treatmentList);
			AssertEquals("TreatmentList count", true, treatmentList.Count > 0);
			AssertEquals("Empty Desc", "Duty (GEN, 001):5%", treatmentList.GetDescriptionFromCode("CCC"));
		}

		public void TestTreatmentCodeLIstExceptionForEdifice()
		{
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceLegacyMessages;
			declaration.JE_DateOfFirstArrival = ZDateTime.Empty;
			JobComInvoiceHeader invoiceHeader = declaration.Invoices.AddNew();
			JobComInvoiceLine invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			AssertNotNull("List is not null", invoiceLine.AddInfo.Lookups.TreatmentCodeList);
		}

		public void TestTreatmentCodeList()
		{
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages;
			declaration.JE_DateOfFirstArrival = new ZDateTime(2005, 1, 1);

			CMRTreatmentRatePeriodSnapshot treatmentSnapshot1 = CMRTreatmentRatePeriodSnapshot.New(Factory);
			treatmentSnapshot1.TP_Code = "CCC";
			treatmentSnapshot1.TP_RateNumber = "001";
			treatmentSnapshot1.TP_PreferenceSchemeType = "GEN";
			treatmentSnapshot1.TP_StartDate = new ZDateTime(2005, 1, 1);
			treatmentSnapshot1.TP_CalculationType = "CALC";
			treatmentSnapshot1.TP_CustomsValueRate = 5m;

			CMRTreatmentRatePeriodSnapshot treatmentSnapshot2 = CMRTreatmentRatePeriodSnapshot.New(Factory);
			treatmentSnapshot2.TP_Code = "AAA";
			treatmentSnapshot2.TP_RateNumber = "001";
			treatmentSnapshot2.TP_PreferenceSchemeType = "GEN";
			treatmentSnapshot2.TP_StartDate = new ZDateTime(2005, 1, 1);
			treatmentSnapshot2.TP_CalculationType = "INCALC";

			CodeDescriptionPairList treatmentList = lookups.TreatmentCodeList;
			AssertNotNull("TreatmentList", treatmentList);
			Assert("TreatmentList count", treatmentList.Count > 0);
			AssertEquals("Empty Desc", "Duty (GEN, 001):5%", treatmentList.GetDescriptionFromCode("CCC"));

			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Export;
			treatmentList = lookups.TreatmentCodeList;
			AssertNotNull("TreatmentList", treatmentList);
			Assert("TreatmentList count", treatmentList.Count > 0);
			AssertEquals("Empty Desc", "Duty (GEN, 001):5%", treatmentList.GetDescriptionFromCode("CCC"));
		}

		public void TestTreatmentCodeListIsSortedForCMR()
		{
			TestCaseHelper.ClearTable(CMRTreatmentRatePeriodSnapshot.Schema.TableName);
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages;
			declaration.JE_DateOfFirstArrival = new ZDateTime(2005, 1, 1);
			string tableName = CMRTreatmentSnapshotSchema.Constants.TableName;
			TestCaseHelper.ClearTable(tableName);
			CMRTreatmentRatePeriodSnapshot treatmentSnapshot1 = CMRTreatmentRatePeriodSnapshot.New(Factory);
			treatmentSnapshot1.TP_Code = "_CC";
			treatmentSnapshot1.TP_RateNumber = "001";
			treatmentSnapshot1.TP_PreferenceSchemeType = "GEN";
			treatmentSnapshot1.TP_StartDate = new ZDateTime(2005, 1, 1);
			treatmentSnapshot1.TP_CalculationType = "CALC";
			treatmentSnapshot1.TP_CustomsValueRate = 5m;

			CMRTreatmentRatePeriodSnapshot treatmentSnapshot2 = CMRTreatmentRatePeriodSnapshot.New(Factory);
			treatmentSnapshot2.TP_Code = "_AA";
			treatmentSnapshot2.TP_RateNumber = "001";
			treatmentSnapshot2.TP_PreferenceSchemeType = "GEN";
			treatmentSnapshot2.TP_StartDate = new ZDateTime(2005, 1, 1);
			treatmentSnapshot2.TP_CalculationType = "INCALC";

			CMRTreatmentRatePeriodSnapshot treatmentSnapshot3 = CMRTreatmentRatePeriodSnapshot.New(Factory);
			treatmentSnapshot3.TP_Code = "_BB";
			treatmentSnapshot3.TP_RateNumber = "001";
			treatmentSnapshot3.TP_PreferenceSchemeType = "GEN";
			treatmentSnapshot3.TP_StartDate = new ZDateTime(2005, 1, 1);
			treatmentSnapshot3.TP_CalculationType = "INCALC";

			CodeDescriptionPairList treatmentList = lookups.TreatmentCodeList;
			AssertNotNull("TreatmentList", treatmentList);
			AssertEquals("TreatmentList count", true, treatmentList.Count >= 3);
			AssertEquals("The list is not sorted", "_AA", treatmentList[0].Code);
			AssertEquals("The list is not sorted", "_BB", treatmentList[1].Code);
			AssertEquals("The list is not sorted", "_CC", treatmentList[2].Code);
		}

		public void TestTreatmentCodeSelectedBasedOnSchemeIfEntered()
		{
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages;
			declaration.JE_DateOfFirstArrival = new ZDateTime(2005, 1, 1);

			CMRTreatmentRatePeriodSnapshot treatmentSnapshot1 = CMRTreatmentRatePeriodSnapshot.New(Factory);
			treatmentSnapshot1.TP_Code = "CCC";
			treatmentSnapshot1.TP_RateNumber = "001";
			treatmentSnapshot1.TP_PreferenceSchemeType = "GEN";
			treatmentSnapshot1.TP_StartDate = new ZDateTime(2005, 1, 1);
			treatmentSnapshot1.TP_CalculationType = "CALC";
			treatmentSnapshot1.TP_CustomsValueRate = 5m;

			CMRTreatmentRatePeriodSnapshot treatmentSnapshot2 = CMRTreatmentRatePeriodSnapshot.New(Factory);
			treatmentSnapshot2.TP_Code = "AAA";
			treatmentSnapshot2.TP_RateNumber = "001";
			treatmentSnapshot2.TP_PreferenceSchemeType = "US";
			treatmentSnapshot2.TP_StartDate = new ZDateTime(2005, 1, 1);
			treatmentSnapshot2.TP_CalculationType = "INCALC";

			CMRTreatmentRatePeriodSnapshot treatmentSnapshot3 = CMRTreatmentRatePeriodSnapshot.New(Factory);
			treatmentSnapshot3.TP_Code = "ZZZ";
			treatmentSnapshot3.TP_RateNumber = "001";
			treatmentSnapshot3.TP_PreferenceSchemeType = "GEN";
			treatmentSnapshot3.TP_StartDate = new ZDateTime(2005, 1, 1);
			treatmentSnapshot3.TP_CalculationType = "INFO";

			string tableName = CMRTreatmentSnapshotSchema.Constants.TableName;
			TestCaseHelper.ClearTable(tableName);

			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			JobComInvoiceLine invoiceLine = invoice.JobComInvoiceLines.AddNew();

			invoiceLine.AddInfo.ZA_PST = "GEN";
			Assert("Treatment code list should not have AAA which relates to US only", !invoiceLine.AddInfo.Lookups.TreatmentCodeList.ContainsCode("AAA"));
			Assert("Treatment code list should have CCC which relates to GEN", invoiceLine.AddInfo.Lookups.TreatmentCodeList.ContainsCode("CCC"));
			Assert("Treatment code list should have ZZZ which relates to GEN", invoiceLine.AddInfo.Lookups.TreatmentCodeList.ContainsCode("ZZZ"));

			invoiceLine.AddInfo.ZA_PST = "US";
			Assert("Treatment code list should have AAA which relates to US", invoiceLine.AddInfo.Lookups.TreatmentCodeList.ContainsCode("AAA"));
			Assert("Treatment code list should not have CCC which relates to GEN only", !invoiceLine.AddInfo.Lookups.TreatmentCodeList.ContainsCode("CCC"));
			Assert("Treatment code list should have ZZZ which relates to GEN but is INFO", invoiceLine.AddInfo.Lookups.TreatmentCodeList.ContainsCode("ZZZ"));
		}

		public void TestZA_LCTE_List()
		{
			AssertEquals("CMRLCTEList", typeof(CMRCodeListsCollection), lookups.CMRLCTEList.GetType());
		}

		public void TestCMRLCTEList()
		{
			AssertListDoesntContainWrongCode(lookups.CMRLCTEList, CMRCodeLists.CodeTypes.LCTX);
		}

		public void TestZA_WETE_List()
		{
			AssertEquals("CMRWETEList", typeof(CMRCodeListsCollection), lookups.CMRWETEList.GetType());
		}

		public void TestCMRWETEList()
		{
			AssertListDoesntContainWrongCode(lookups.CMRWETEList, CMRCodeLists.CodeTypes.WETX);
		}

		public void TestZA_GSTE_List()
		{
			AssertEquals("CMRGSTEList", typeof(CMRCodeListsCollection), lookups.CMRGSTEList.GetType());
		}

		public void TestCMRGSTEList()
		{
			AssertListDoesntContainWrongCode(lookups.CMRGSTEList, CMRCodeLists.CodeTypes.GSTX);
		}

		public void TestZA_DXT_List()
		{
			CodeDescriptionPairList result = lookups.ZA_DXT_List;
			AssertNotNull("Result is not null", result);
			AssertEquals("List has three elements", 3, result.Count);
			AssertEquals("Of type CodeDescriptionPairListDumpingExemptionType", typeof(CodeDescriptionPairListDumpingExemptionType), result.GetType());
		}

		public void TestZA_POC_List()
		{
			IBusinessObjectCollection result = lookups.ZA_POC_List;
			AssertNotNull("Result is not null", result);
			AssertEquals("Of type RefCountry", typeof(RefCountryCollection), result.GetType());
		}

		public void TestZA_PRT_List()
		{
			CMRPreferenceRulePeriodSnapshot preference = CMRPreferenceRulePeriodSnapshot.New(Factory);
			preference.PU_StartDate = new ZDateTime(2005, 03, 03);
			preference.PU_RuleType = "Type";
			preference.PU_Description = "Description";

			CMRPreferenceRulePeriodSnapshot expiredPreference = CMRPreferenceRulePeriodSnapshot.New(Factory);
			expiredPreference.PU_RuleType = "Exp";
			expiredPreference.PU_Description = "Expired Description";
			expiredPreference.PU_EndDate = new ZDateTime(2004, 01, 01);

			JobComInvoiceHeader invoiceHeader = declaration.Invoices.AddNew();
			invoiceHeader.AddInfo.ZA_EFD = "030305";

			lookups = invoiceHeader.AddInfo.Lookups;

			CodeDescriptionPairList preferenceList = lookups.ZA_PRT_List;
			AssertNotNull("PreferenceList", preferenceList);
			AssertEquals("Has at least one in list", true, preferenceList.Count > 0);
			AssertEquals("Code", "Type", preferenceList.GetCodeFromDescription("Description"));
			AssertEquals("Description", "Description", preferenceList.GetDescriptionFromCode("Type"));

			AssertEquals("Expired", null, preferenceList.GetDescriptionFromCode("Expired Description"));
		}

		public void TestZA_PRT_ListIsSorted()
		{
			CMRPreferenceRulePeriodSnapshot preference1 = CMRPreferenceRulePeriodSnapshot.New(Factory);
			preference1.PU_StartDate = new ZDateTime(2005, 03, 03);
			preference1.PU_RuleType = "DDD";
			preference1.PU_Description = "DDD Description";

			CMRPreferenceRulePeriodSnapshot preference2 = CMRPreferenceRulePeriodSnapshot.New(Factory);
			preference2.PU_StartDate = new ZDateTime(2005, 03, 03);
			preference2.PU_RuleType = "CCC";
			preference2.PU_Description = "CCC Description";

			CMRPreferenceRulePeriodSnapshot preference3 = CMRPreferenceRulePeriodSnapshot.New(Factory);
			preference3.PU_StartDate = new ZDateTime(2005, 03, 03);
			preference3.PU_RuleType = "MMM";
			preference3.PU_Description = "MMM Description";

			JobComInvoiceLine line = declaration.Invoices.AddNew().JobComInvoiceLines.AddNew();
			line.AddInfo.ZA_PST = ZString.Empty;
			CodeDescriptionPairList list = line.AddInfo.Lookups.ZA_PRT_List;
			list.Sort();
			Assert("List is empty", list.Count > 0);
			for (short i = 0; i < list.Count; i++)
			{
				AssertEquals(line.AddInfo.Lookups.ZA_PRT_List[i].Code, list[i].Code);
			}
		}

		public void TestRulesWithScheme_NoDuplicatedCodes()
		{
			var rule1 = CMRPreferenceSchemeRule.New(Factory);
			rule1.PR_RuleType = "PE";
			rule1.PR_PreferenceSchemePeriodSnapshotSchemeType = "TEST";
			var rule2 = CMRPreferenceSchemeRule.New(Factory);
			rule2.PR_RuleType = "PSR";
			rule2.PR_PreferenceSchemePeriodSnapshotSchemeType = "TEST";
			var rule3 = CMRPreferenceSchemeRule.New(Factory);
			rule3.PR_RuleType = "PE";
			rule3.PR_PreferenceSchemePeriodSnapshotSchemeType = "TEST";

			var invoiceHeader = declaration.Invoices.AddNew();
			invoiceHeader.AddInfo.ZA_PST = "TEST";

			var list = invoiceHeader.AddInfo.Lookups.ZA_PRT_List;
			AssertEquals("Should not contain duplicated codes", 2, list.Count);
			Assert(list.ContainsCode("PE"));
			Assert(list.ContainsCode("PSR"));
		}

		public void TestRulesWithScheme()
		{
			CMRPreferenceRulePeriodSnapshot rule1 = CMRPreferenceRulePeriodSnapshot.New(Factory);
			rule1.PU_StartDate = new ZDateTime(2005, 03, 03);
			rule1.PU_RuleType = "AA";
			rule1.PU_Description = "AAAAA";

			CMRPreferenceSchemeRule ruleScheme1 = CMRPreferenceSchemeRule.New(Factory);
			ruleScheme1.PR_PreferenceSchemePeriodSnapshotSchemeType = "A1";
			ruleScheme1.PR_RuleType = "AA";

			CMRPreferenceRulePeriodSnapshot rule2 = CMRPreferenceRulePeriodSnapshot.New(Factory);
			rule2.PU_StartDate = new ZDateTime(2005, 03, 03);
			rule2.PU_RuleType = "BB";
			rule2.PU_Description = "BBBBB";

			CMRPreferenceSchemeRule ruleScheme2 = CMRPreferenceSchemeRule.New(Factory);
			ruleScheme2.PR_PreferenceSchemePeriodSnapshotSchemeType = "B1";
			ruleScheme2.PR_RuleType = "BB";

			JobComInvoiceHeader invoiceHeader = declaration.Invoices.AddNew();
			invoiceHeader.AddInfo.ZA_EFD = "030305";
			invoiceHeader.AddInfo.ZA_PST = "A1";

			JobComInvoiceLine invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			CodeDescriptionPairList ruleList = invoiceLine.AddInfo.Lookups.ZA_PRT_List;
			AssertEquals("AA should be in the list", true, ruleList.ContainsCode("AA"));
			AssertEquals("AA' description should be in the list", "AAAAA", ruleList.GetDescriptionFromCode("AA"));
			AssertEquals("BB should not be in the list", false, ruleList.ContainsCode("BB"));

			invoiceLine.AddInfo.ZA_PST = "B1";
			ruleList = invoiceLine.AddInfo.Lookups.ZA_PRT_List;
			AssertEquals("AA should not be in the list", false, ruleList.ContainsCode("AA"));
			AssertEquals("BB should be in the list", true, ruleList.ContainsCode("BB"));
			AssertEquals("BB' description be in the list", "BBBBB", ruleList.GetDescriptionFromCode("BB"));
		}

		public void TestRulesWithGeneralRate()
		{
			JobComInvoiceHeader invoiceHeader = declaration.Invoices.AddNew();
			invoiceHeader.AddInfo.ZA_EFD = "030305";
			invoiceHeader.AddInfo.ZA_PST = "GEN";

			AssertEquals("Is general rate", true, invoiceHeader.AddInfo.IsGeneralRate);
			AssertEquals("Empty list of rules", 0, invoiceHeader.AddInfo.Lookups.ZA_PRT_List.Count);
		}

		public void TestPreferenceSchemeCodeList()
		{
			CMRPreferenceSchemePeriodSnapshot preference = CMRPreferenceSchemePeriodSnapshot.New(Factory);
			preference.PF_StartDate = new ZDateTime(2005, 03, 03);
			preference.PF_SchemeType = "Type";
			preference.PF_Description = "Description";

			CMRPreferenceSchemePeriodSnapshot expiredreference = CMRPreferenceSchemePeriodSnapshot.New(Factory);
			expiredreference.PF_SchemeType = "Exp";
			expiredreference.PF_Description = "Expired Description";
			expiredreference.PF_EndDate = new ZDateTime(2004, 01, 01);

			JobComInvoiceHeader invoiceHeader = declaration.Invoices.AddNew();
			invoiceHeader.AddInfo.ZA_EFD = "030305";

			CMRPreferenceSchemePeriodSnapshot preferenceScheme1 = CMRPreferenceSchemePeriodSnapshot.New(Factory);
			preferenceScheme1.PF_SchemeType = "AA";
			preferenceScheme1.PF_Description = "AAAAA";
			preferenceScheme1.PF_StartDate = new ZDateTime(2005, 03, 03);

			CMRPreferenceSchemePeriodSnapshot preferenceScheme2 = CMRPreferenceSchemePeriodSnapshot.New(Factory);
			preferenceScheme2.PF_SchemeType = "BB";
			preferenceScheme2.PF_Description = "BBBBB";
			preferenceScheme2.PF_StartDate = new ZDateTime(2005, 03, 03);

			AssertEquals("POC is empty", "", invoiceHeader.AddInfo.ZA_POC);
			CodeDescriptionPairList preferenceList = invoiceHeader.AddInfo.Lookups.ZA_PST_List;

			AssertNotNull("PreferenceList", preferenceList);
			AssertEquals("Has at least one in list", true, preferenceList.Count > 0);
			AssertEquals("Code", "Type", preferenceList.GetCodeFromDescription("Description"));
			AssertEquals("Description", "Description", preferenceList.GetDescriptionFromCode("Type"));
			AssertEquals("List has an item for AA", true, preferenceList.ContainsCode("AA"));
			AssertEquals("List has an item for BB", true, preferenceList.ContainsCode("BB"));

			AssertEquals("Expired", null, preferenceList.GetDescriptionFromCode("Expired Description"));
		}

		public void TestPreferenceSchemeWithTariff()
		{
			SetUpTariffSchemes();

			JobDeclaration testDec = JobDeclaration.New(Factory);
			JobComInvoiceHeader invoice = testDec.Invoices.AddNew();
			JobComInvoiceLine invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "0000.00.00 00";
			AssertEquals("Tariff number", "00000000", invoiceLine.TariffNumber);

			AssertEquals("No origin therefore Tariff is the only factor", "", invoiceLine.AddInfo.AggregatedPOCFallBackToORG);

			CodeDescriptionPairList result = invoiceLine.AddInfo.Lookups.ZA_PST_List;
			AssertEquals("The list should have AA", true, result.ContainsCode("AA"));
			AssertEquals("The list should not have BB as it is not relevant to the tariff selected", false, result.ContainsCode("BB"));
			AssertEquals("The list should have GEN", true, result.ContainsCode("GEN"));
			AssertEquals("The list should have GEN", "General(Duty: )", result.GetDescriptionFromCode("GEN"));
			AssertEquals("The list should have a description for AA", "(Duty: )", result.GetDescriptionFromCode("AA"));
		}

		public void TestPreferenceSchemeWithTariffAndOriginCombination()
		{
			SetUpTariffSchemes();

			CMRPreferenceSchemePeriodCountry schemeCountry1 = CMRPreferenceSchemePeriodCountry.New(Factory);
			schemeCountry1.PC_PreferenceSchemePeriodSnapshotSchemeType = "AA";
			schemeCountry1.PC_CountryCode = "XX";

			CMRPreferenceSchemePeriodCountry schemeCountry2 = CMRPreferenceSchemePeriodCountry.New(Factory);
			schemeCountry2.PC_PreferenceSchemePeriodSnapshotSchemeType = "BB";
			schemeCountry2.PC_CountryCode = "YY";

			CMRPreferenceSchemePeriodCountry schemeCountry3 = CMRPreferenceSchemePeriodCountry.New(Factory);
			schemeCountry3.PC_PreferenceSchemePeriodSnapshotSchemeType = "CC";
			schemeCountry3.PC_CountryCode = "ZZ";

			JobDeclaration testDec = JobDeclaration.New(Factory);
			JobComInvoiceHeader invoice = testDec.Invoices.AddNew();
			JobComInvoiceLine invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "0000.00.00 00";
			invoiceLine.AddInfo.ZA_ORG = "ZZ";

			CodeDescriptionPairList result = invoiceLine.AddInfo.Lookups.ZA_PST_List;
			AssertEquals("The list should not have AA as it is not relevant for the origin", false, result.ContainsCode("AA"));
			AssertEquals("The list should not have BB as it is not relevant to the tariff selected", false, result.ContainsCode("BB"));
			AssertEquals("The list should have CC", true, result.ContainsCode("CC"));
			AssertEquals("The list should have a description for CC", "(Duty: )", result.GetDescriptionFromCode("CC"));
			AssertEquals("The list should have GEN", true, result.ContainsCode("GEN"));
			AssertEquals("The list should have GEN", "General(Duty: )", result.GetDescriptionFromCode("GEN"));

			invoiceLine.AddInfo.ZA_ORG = "YY";
			result = invoiceLine.AddInfo.Lookups.ZA_PST_List;
			AssertEquals("The list should not have AA as it is not relevant for the origin", false, result.ContainsCode("AA"));
			AssertEquals("The list should not have BB as this is not relevant for tariff", false, result.ContainsCode("BB"));
			AssertEquals("The list should not have CC as this is not relevant to Origin entered", false, result.ContainsCode("CC"));
			AssertEquals("The list should have GEN", true, result.ContainsCode("GEN"));
			AssertEquals("The list should have GEN", "General(Duty: )", result.GetDescriptionFromCode("GEN"));

			invoiceLine.JI_Tariff = "00000001 00";
			result = invoiceLine.AddInfo.Lookups.ZA_PST_List;
			AssertEquals("The list should not have AA as it is not relevant for the origin", false, result.ContainsCode("AA"));
			AssertEquals("The list should have BB", true, result.ContainsCode("BB"));
			AssertEquals("The list should have BB", "(Duty: )", result.GetDescriptionFromCode("BB"));
			AssertEquals("The list should not have CC as this is not relevant to Origin entered", false, result.ContainsCode("CC"));
			AssertEquals("The list should have GEN", true, result.ContainsCode("GEN"));
			AssertEquals("The list should have GEN", "General(Duty: )", result.GetDescriptionFromCode("GEN"));
		}

		public void TestPreferenceSchemeWithTreatmentAndOriginCombination()
		{
			SetUpTariffSchemes();

			CMRPreferenceSchemePeriodCountry schemeCountry1 = CMRPreferenceSchemePeriodCountry.New(Factory);
			schemeCountry1.PC_PreferenceSchemePeriodSnapshotSchemeType = "AA";
			schemeCountry1.PC_CountryCode = "XX";

			CMRPreferenceSchemePeriodCountry schemeCountry2 = CMRPreferenceSchemePeriodCountry.New(Factory);
			schemeCountry2.PC_PreferenceSchemePeriodSnapshotSchemeType = "BB";
			schemeCountry2.PC_CountryCode = "YY";

			CMRPreferenceSchemePeriodCountry schemeCountry3 = CMRPreferenceSchemePeriodCountry.New(Factory);
			schemeCountry3.PC_PreferenceSchemePeriodSnapshotSchemeType = "CC";
			schemeCountry3.PC_CountryCode = "ZZ";

			CMRTreatmentRatePeriodSnapshot treatmentRateNumber1 = CMRTreatmentRatePeriodSnapshot.New(Factory);
			treatmentRateNumber1.TP_StartDate = new ZDateTime(2005, 03, 03);
			treatmentRateNumber1.TP_PreferenceSchemeType = "GEN";
			treatmentRateNumber1.TP_Code = "99A";

			CMRTreatmentRatePeriodSnapshot treatmentRateNumber2 = CMRTreatmentRatePeriodSnapshot.New(Factory);
			treatmentRateNumber2.TP_StartDate = new ZDateTime(2005, 03, 03);
			treatmentRateNumber2.TP_PreferenceSchemeType = "AA";
			treatmentRateNumber2.TP_Code = "99A";

			CMRTreatmentRatePeriodSnapshot treatmentRateNumber3 = CMRTreatmentRatePeriodSnapshot.New(Factory);
			treatmentRateNumber3.TP_StartDate = new ZDateTime(2005, 03, 03);
			treatmentRateNumber3.TP_PreferenceSchemeType = "BB";
			treatmentRateNumber3.TP_Code = "99A";

			CMRTreatmentRatePeriodSnapshot treatmentRateNumber4 = CMRTreatmentRatePeriodSnapshot.New(Factory);
			treatmentRateNumber4.TP_StartDate = new ZDateTime(2005, 03, 03);
			treatmentRateNumber4.TP_PreferenceSchemeType = "GEN";
			treatmentRateNumber4.TP_Code = "44A";
			treatmentRateNumber4.TP_CalculationType = "INFO";

			JobDeclaration testDec = JobDeclaration.New(Factory);
			testDec.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			testDec.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages;
			JobComInvoiceHeader invoice = testDec.Invoices.AddNew();
			JobComInvoiceLine invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "0000.00.00 00";
			invoiceLine.AddInfo.ZA_TreatmentCode_Hidden = "99A";
			invoiceLine.AddInfo.ZA_ORG = "YY";

			CodeDescriptionPairList result = invoiceLine.AddInfo.Lookups.ZA_PST_List;
			AssertEquals("2 items in list", 2, result.Count);
			Assert("The list should not have AA as it is not relevant for the origin", !result.ContainsCode("AA"));
			Assert("The list should have BB", result.ContainsCode("BB"));
			Assert("The list should have GEN", result.ContainsCode("GEN"));

			treatmentRateNumber3.TP_Code = "99B";
			Factory.Save();
			BusinessObjectFactory factory2 = new BusinessObjectFactory();
			JobComInvoiceLine invoiceLineInFactory2 = factory2.Load<JobComInvoiceLine>(invoiceLine.PK);
			result = invoiceLineInFactory2.AddInfo.Lookups.ZA_PST_List;
			AssertEquals("1 item in list", 1, result.Count);
			Assert("The list should not have AA as it is not relevant for the origin", !result.ContainsCode("AA"));
			Assert("The list should not have BB as it is not relevant for the treatment", !result.ContainsCode("AA"));
			Assert("The list should have GEN", result.ContainsCode("GEN"));

			invoiceLine.AddInfo.ZA_TreatmentCode_Hidden = "44A";
			invoiceLine.AddInfo.ZA_ORG = "ZZ";
			result = invoiceLine.AddInfo.Lookups.ZA_PST_List;
			AssertEquals("2 items in list", 2, result.Count);
			Assert("The list should have CC from the tariff query", result.ContainsCode("CC"));
			Assert("The list should have GEN from the tariff query", result.ContainsCode("GEN"));
		}

		public void TestPreferenceSchemeWithOrigin()
		{
			CMRPreferenceSchemePeriodCountry schemeCountry1 = CMRPreferenceSchemePeriodCountry.New(Factory);
			schemeCountry1.PC_PreferenceSchemePeriodSnapshotSchemeType = "CC";
			schemeCountry1.PC_CountryCode = "XX";

			CMRPreferenceSchemePeriodCountry schemeCountry2 = CMRPreferenceSchemePeriodCountry.New(Factory);
			schemeCountry2.PC_PreferenceSchemePeriodSnapshotSchemeType = "DD";
			schemeCountry2.PC_CountryCode = "YY";

			CMRPreferenceSchemePeriodSnapshot preferenceScheme1 = CMRPreferenceSchemePeriodSnapshot.New(Factory);
			preferenceScheme1.PF_SchemeType = "CC";
			preferenceScheme1.PF_Description = "CCCCC";

			CMRPreferenceSchemePeriodSnapshot preferenceScheme2 = CMRPreferenceSchemePeriodSnapshot.New(Factory);
			preferenceScheme2.PF_SchemeType = "DD";
			preferenceScheme2.PF_Description = "DDDDD";

			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			invoice.AddInfo.ZA_POC = "XX";

			CodeDescriptionPairList result = invoice.AddInfo.Lookups.ZA_PST_List;
			AssertEquals("The list should have CC", true, result.ContainsCode("CC"));
			AssertEquals("The list should not have DD as it is not relevant to Origin", false, result.ContainsCode("DD"));
			AssertEquals("The list should have GEN", true, result.ContainsCode("GEN"));
			AssertEquals("The list should have a description for CC", "CCCCC", result.GetDescriptionFromCode("CC"));

			invoice.AddInfo.ZA_POC = "";
			invoice.AddInfo.ZA_ORG = "YY";
			result = invoice.AddInfo.Lookups.ZA_PST_List;
			AssertEquals("The list should not have CC", false, result.ContainsCode("CC"));
			AssertEquals("The list should have DD", true, result.ContainsCode("DD"));
			AssertEquals("The list should have GEN", true, result.ContainsCode("GEN"));
			AssertEquals("The list should have a description for DD", "DDDDD", result.GetDescriptionFromCode("DD"));
		}

		public void TestPreferenceSchemeWithOrigin_NoDuplicatedCodes()
		{
			var scheme1 = CMRPreferenceSchemePeriodCountry.New(Factory);
			scheme1.PC_PreferenceSchemePeriodSnapshotSchemeType = "AANZ";
			scheme1.PC_CountryCode = "AA";
			var scheme2 = CMRPreferenceSchemePeriodCountry.New(Factory);
			scheme2.PC_PreferenceSchemePeriodSnapshotSchemeType = "PACR";
			scheme2.PC_CountryCode = "AA";
			var scheme3 = CMRPreferenceSchemePeriodCountry.New(Factory);
			scheme3.PC_PreferenceSchemePeriodSnapshotSchemeType = "AANZ";
			scheme3.PC_CountryCode = "AA";

			var invoiceHeader = declaration.Invoices.AddNew();
			invoiceHeader.AddInfo.ZA_ORG = "AA";

			var list = invoiceHeader.AddInfo.Lookups.ZA_PST_List;
			AssertEquals("Should not contain duplicated codes", 3, list.Count);
			Assert(list.ContainsCode("GEN"));
			Assert(list.ContainsCode("AANZ"));
			Assert(list.ContainsCode("PACR"));
		}

		public void TestZA_HART_List()
		{
			AssertEquals("CMRGSTEList", typeof(CMRCodeListsCollection), lookups.ZA_HART_List.GetType());
		}

		public void TestLoadingOfZA_HART_List()
		{
			SetUpCMRCodeList("HEADARSTYP");

			CMRCodeListsCollection hARTList = lookups.ZA_HART_List;
			AssertNotNull("Header Amber Reason Type", hARTList);
			AssertEquals("Header Amber Reason Type should not load any objects in lookup", 0, hARTList.Count);
		}

		public void TestLoadingDrawbackLists()
		{
			CodeDescriptionPairList list = lookups.DrawbackAmberCodeList;
			AssertEquals("Drawback Amber Code List", 4, list.Count);
			AssertEquals("Calculation", "C", list.GetCodeFromDescription("Calculation"));
			list = lookups.ZA_DAM_List;
			AssertEquals("Drawback Assessment Method Code List", 4, list.Count);
			AssertEquals("Shipment by Shipment", JobDeclaration.DrawbackAssessmentMethods.ActualShipment, list.GetCodeFromDescription("Shipment by Shipment. The claim amount is calculated from the Import documents which directly relates to the Export consignment."));
		}

		public void TestGlobalEntryLineKeys()
		{
			JobDeclaration declaration = JobDeclaration.New(Factory);
			OrgHeader org = OrgHeader.New(Factory);
			declaration.JE_OH_Importer = org.PK;
			AssertNotNull(declaration.AddInfo.Lookups.GlobalEntryLineKeys);
			AssertEquals(org, ((GlobalCusEntryLineCollection)declaration.AddInfo.Lookups.GlobalEntryLineKeys).Importer);

			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			JobComInvoiceLine invoiceLine = invoice.JobComInvoiceLines.AddNew();
			AUOrgSupplierPart product = Factory.New<AUOrgSupplierPart>();
			product.OP_PartNum = "TestPartNum";
			OrgPartRelation relation = product.RelatedOrganisations.AddNew();
			relation.OU_Relationship = "OWN";
			relation.OU_OH = org.PK;
			invoiceLine.JI_PartNo = product.OP_PartNum;
			AssertNotNull(invoiceLine.AddInfo.Lookups.GlobalEntryLineKeys);
			AssertEquals(org, ((GlobalCusEntryLineCollection)invoiceLine.AddInfo.Lookups.GlobalEntryLineKeys).Importer);
			AssertEquals(product, ((GlobalCusEntryLineCollection)invoiceLine.AddInfo.Lookups.GlobalEntryLineKeys).LinePart);
		}

		public void TestGlobalDrawbackEntryLineKeys()
		{
			JobDeclaration declaration = JobDeclaration.New(Factory);
			OrgHeader org = OrgHeader.New(Factory);
			declaration.JE_OH_Importer = org.PK;
			AssertNotNull(declaration.AddInfo.Lookups.GlobalDrawbackEntryLineKeys);
			AssertEquals(org, ((GlobalCusEntryLineCollection)declaration.AddInfo.Lookups.GlobalDrawbackEntryLineKeys).Importer);

			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			JobComInvoiceLine invoiceLine = invoice.JobComInvoiceLines.AddNew();
			AUOrgSupplierPart product = Factory.New<AUOrgSupplierPart>();
			product.OP_PartNum = "TestPartNum";
			OrgPartRelation relation = product.RelatedOrganisations.AddNew();
			relation.OU_Relationship = "OWN";
			relation.OU_OH = org.PK;
			invoiceLine.JI_PartNo = product.OP_PartNum;
			AssertNotNull(invoiceLine.AddInfo.Lookups.GlobalDrawbackEntryLineKeys);
			AssertEquals(org, ((GlobalCusEntryLineCollection)invoiceLine.AddInfo.Lookups.GlobalDrawbackEntryLineKeys).Importer);
			AssertEquals(product, ((GlobalCusEntryLineCollection)invoiceLine.AddInfo.Lookups.GlobalDrawbackEntryLineKeys).LinePart);
		}

		public void TestZA_RRC_List()
		{
			AssertNotNull("Refund Reasons List", lookups.ZA_RRC_List);
		}

		public void TestRefundReasonSortingCMRRefundReason()
		{
			TestCaseHelper.ClearTable(CMRRefundReason.Schema.TableName);

			var refundReason1 = CMRRefundReason.New(Factory);
			refundReason1.CR_RefundReasonStartDate = new ZDateTime(2005, 03, 03);
			refundReason1.CR_RefundReasonType = "Type1";
			refundReason1.CR_RefundReasonDescription = "Description";

			var refundReason2 = CMRRefundReason.New(Factory);
			refundReason2.CR_RefundReasonStartDate = new ZDateTime(2005, 03, 03);
			refundReason2.CR_RefundReasonType = "Type2";
			refundReason2.CR_RefundReasonDescription = "Another Description";

			var expiredRefundReason = CMRRefundReason.New(Factory);
			expiredRefundReason.CR_RefundReasonType = "Expired";
			expiredRefundReason.CR_RefundReasonDescription = "Expired Description";
			expiredRefundReason.CR_RefundReasonEndDate = new ZDateTime(2004, 01, 01);

			JobComInvoiceHeader invoiceHeader = declaration.Invoices.AddNew();
			invoiceHeader.AddInfo.ZA_EFD = "030305";

			using (AUCustomsDataRegistry.Instance.UseRefDatabaseData.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				var refundReasonList = invoiceHeader.AddInfo.Lookups.ZA_RRC_List;
				AssertEquals("Type2 - Another Description\r\nType1 - Description", refundReasonList.ElementsAsString);
			}
		}

		public void TestRefundReasonSortingRefCusCode()
		{
			TestCaseHelper.ClearTable(RefCusCodeList.Schema.TableName);

			var helper = new UniversalReferenceTestDataHelper(Factory);

			var refCusCode1 = CreateNewRefCusCodeList(helper, "TST1", "RefCodeDesc1");
			var refCusCode2 = CreateNewRefCusCodeList(helper, "AST2", "RefCodeDesc2");
			var expiredReason = CreateNewRefCusCodeList(helper, "TST2", "RefCusCodeExpiredDesc", new DateTime(2001, 01, 01));
			Factory.Save();

			var invoiceHeader = declaration.Invoices.AddNew();
			invoiceHeader.AddInfo.ZA_EFD = "030305";

			using (AUCustomsDataRegistry.Instance.UseRefDatabaseData.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var refundReasonList = invoiceHeader.AddInfo.Lookups.ZA_RRC_List;
				AssertEquals("TST1 - RefCodeDesc1\r\nAST2 - RefCodeDesc2", refundReasonList.ElementsAsString);
			}
		}

		public void TestZA_AQISServicePaymentCurrency_List()
		{
			IBusinessObjectCollection result = lookups.ZA_AQISServicePaymentCurrency_List;
			AssertNotNull("Service Payment Currency List", result);
			AssertEquals("Of type RefCurrency", typeof(RefCurrencyCollection), result.GetType());
		}

		public void TestZA_REL_List()
		{
			CodeDescriptionPairList list = lookups.ZA_REL_List;
			AssertEquals("Line Related Transaction Code List", 3, list.Count);
			AssertEquals("Yes", CMRRelatedTransaction.Yes.Code, list.GetCodeFromDescription(CMRRelatedTransaction.Yes.Description));
			AssertEquals("No", CMRRelatedTransaction.No.Code, list.GetCodeFromDescription(CMRRelatedTransaction.No.Description));
			AssertEquals("Default", CMRRelatedTransaction.Default.Code, list.GetCodeFromDescription(CMRRelatedTransaction.Default.Description));
		}

		public void TestZA_HeaderREL_List()
		{
			CodeDescriptionPairList list = lookups.ZA_HeaderREL_List;
			AssertEquals("Header Related Transaction Code List", 2, list.Count);
			AssertEquals("Yes", CMRRelatedTransaction.Yes.Code, list.GetCodeFromDescription(CMRRelatedTransaction.Yes.Description));
			AssertEquals("No", CMRRelatedTransaction.No.Code, list.GetCodeFromDescription(CMRRelatedTransaction.No.Description));
		}

		public void TestSettlementPeriodTypeList() => CombineAssertions(() =>
		{
			var list = lookups.SettlementPeriodTypeList;
			AssertEquals("List", "SW, SM, SQ, WEK", list.CodesAsString);
			AssertSame("Cached", list, lookups.SettlementPeriodTypeList);
		});

		protected override void SetUp()
		{
			base.SetUp();
			declaration = JobDeclaration.New(Factory);
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages;

			lookups = declaration.AddInfo.Lookups;
			addInfo = declaration.AddInfo;
			TestCaseHelper.ClearTable(CMRTariffRatePeriodSnapshot.Schema.TableName);
		}

		JobDeclaration declaration;
		AUAddInfoLookups lookups;
		AUAddInfo addInfo;

		void SetUpCMRCodeList(ZString codeType)
		{
			CMRCodeLists code = CMRCodeLists.New(Factory);
			code.CI_Code = "TEST";
			code.CI_CodeType = codeType;
			code.CI_Startdate = new ZDateTime(2005, 03, 03);
			code.CI_Description = "Description";

			CMRCodeLists expiredCode = CMRCodeLists.New(Factory);
			expiredCode.CI_Code = "Expired";
			expiredCode.CI_CodeType = codeType;
			expiredCode.CI_Description = "Expired Description";
			expiredCode.CI_EndDate = new ZDateTime(2004, 01, 01);

			JobComInvoiceHeader invoiceHeader = declaration.Invoices.AddNew();
			invoiceHeader.AddInfo.ZA_EFD = "030305";

			lookups = invoiceHeader.AddInfo.Lookups;
		}

		void AssertListDoesntContainWrongCode(CMRCodeListsCollection codeListsCollection, ZString code)
		{
			ZBool result = true;
			codeListsCollection.Load();
			foreach (CMRCodeLists codeLists in codeListsCollection)
			{
				if (codeLists.CI_CodeType != code)
				{
					result = false;
					break;
				}
			}
			AssertEquals("CMRInstrumentTypeList contains wrong codes", true, result);
		}

		void SetUpTariffSchemes()
		{
			CMRTariffRatePeriodSnapshot tariffScheme1 = CMRTariffRatePeriodSnapshot.New(Factory);
			tariffScheme1.TT_PreferenceSchemeType = "AA";
			tariffScheme1.TT_StartDate = new ZDateTime(2005, 1, 1);
			tariffScheme1.TT_TariffClassificationNumber = "00000000";

			CMRTariffRatePeriodSnapshot tariffScheme2 = CMRTariffRatePeriodSnapshot.New(Factory);
			tariffScheme2.TT_PreferenceSchemeType = "BB";
			tariffScheme2.TT_StartDate = new ZDateTime(2005, 1, 1);
			tariffScheme2.TT_TariffClassificationNumber = "00000001";

			CMRTariffRatePeriodSnapshot tariffScheme3 = CMRTariffRatePeriodSnapshot.New(Factory);
			tariffScheme3.TT_PreferenceSchemeType = "CC";
			tariffScheme3.TT_StartDate = new ZDateTime(2005, 1, 1);
			tariffScheme3.TT_TariffClassificationNumber = "00000000";

			CMRTariffRatePeriodSnapshot tariffScheme4 = CMRTariffRatePeriodSnapshot.New(Factory);
			tariffScheme4.TT_PreferenceSchemeType = "GEN";
			tariffScheme4.TT_StartDate = new ZDateTime(2005, 1, 1);
			tariffScheme4.TT_TariffClassificationNumber = "00000000";

			CMRTariffRatePeriodSnapshot tariffScheme5 = CMRTariffRatePeriodSnapshot.New(Factory);
			tariffScheme5.TT_PreferenceSchemeType = "GEN";
			tariffScheme5.TT_StartDate = new ZDateTime(2005, 1, 1);
			tariffScheme5.TT_TariffClassificationNumber = "00000001";

			CMRPreferenceSchemePeriodSnapshot preferenceScheme1 = CMRPreferenceSchemePeriodSnapshot.New(Factory);
			preferenceScheme1.PF_SchemeType = "AA";
			preferenceScheme1.PF_Description = "AAAAA";

			CMRPreferenceSchemePeriodSnapshot preferenceScheme2 = CMRPreferenceSchemePeriodSnapshot.New(Factory);
			preferenceScheme2.PF_SchemeType = "BB";
			preferenceScheme2.PF_Description = "BBBBB";

			CMRPreferenceSchemePeriodSnapshot preferenceScheme3 = CMRPreferenceSchemePeriodSnapshot.New(Factory);
			preferenceScheme3.PF_SchemeType = "CC";
			preferenceScheme3.PF_Description = "CCCCC";
		}

		void AddGENRateNumberAndExpiredRateNumberForTreatmentRateNumber()
		{
			CMRTreatmentRatePeriodSnapshot gENRateNumber = CMRTreatmentRatePeriodSnapshot.New(Factory);
			gENRateNumber.TP_StartDate = new ZDateTime(2005, 03, 03);
			gENRateNumber.TP_RateNumber = "002";
			gENRateNumber.TP_PreferenceSchemeType = "GEN";
			gENRateNumber.TP_CalculationType = "FREE";
			gENRateNumber.TP_Code = "987";

			CMRTreatmentRatePeriodSnapshot expiredRateNumber = CMRTreatmentRatePeriodSnapshot.New(Factory);
			expiredRateNumber.TP_EndDate = new ZDateTime(2004, 01, 01);
			expiredRateNumber.TP_RateNumber = "003";
			expiredRateNumber.TP_PreferenceSchemeType = "GEN";
			expiredRateNumber.TP_Code = "987";
		}

		void AddGENRateNumberAndExpiredRateNumber()
		{
			CMRTariffRatePeriodSnapshot gENRateNumber = CMRTariffRatePeriodSnapshot.New(Factory);
			gENRateNumber.TT_StartDate = new ZDateTime(2005, 03, 03);
			gENRateNumber.TT_RateNumber = "001";
			gENRateNumber.TT_PreferenceSchemeType = "GEN";
			gENRateNumber.TT_CalculationType = "FREE";
			gENRateNumber.TT_TariffClassificationNumber = "49011000";

			CMRTariffRatePeriodSnapshot expiredRateNumber = CMRTariffRatePeriodSnapshot.New(Factory);
			expiredRateNumber.TT_EndDate = new ZDateTime(2004, 01, 01);
			expiredRateNumber.TT_RateNumber = "044";
			expiredRateNumber.TT_PreferenceSchemeType = "GEN";
			expiredRateNumber.TT_TariffClassificationNumber = "49011000";
			Factory.Save();
		}

		RefCusCodeList CreateNewRefCusCodeList(UniversalReferenceTestDataHelper helper, string code, string description, DateTime? expiryDate = null)
		{
			return helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Australia, AUConstants.RefCusCodeTypeCodes.CMRRR, code, description, new DateTime(2000, 01, 01), expiryDate ?? DateTime.Today.AddDays(1));
		}
	}
}
