using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.DataRegistry.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	public class JobComInvoiceLine_Test : TestCaseWithFactory
	{
		public void TestFlatRateDescription()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			JobComInvoiceHeader invoiceHeader = declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			JobComInvoiceLine invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();

			CusEntryHeader entryHeader = declaration.CustomsEntryHeaders.AddNew();
			CusEntryLine entryLine = entryHeader.MergedLines.AddNew();
			entryLine.CL_FlatAmount = 10.12345m;
			entryLine.CL_FlatAmountUQ = "LA";

			invoiceLine.JI_CL = entryLine.PK;

			AssertEquals("Flat rate description", "10.12345/LA", invoiceLine.FlatRateDescription);
		}

		public void TestDoesTariffRateOrTreatmentCodeDeemGSTExemptionCached()
		{
			CMRTariffRatePeriodCharacteristic tariffRate = CMRTariffRatePeriodCharacteristic.New(Factory);
			tariffRate.TH_CharacteristicCode = ZShort.Parse(CharacteristicCodeList.Codes.NonTaxableImports);
			tariffRate.TH_TariffRatePeriodSnapshotPreferenceSchemeType = "GEN";
			tariffRate.TH_TariffRatePeriodSnapshotTariffClassificationNumber = "00000000";
			tariffRate.TH_TariffRatePeriodSnapshotRateNumber = "000";

			CMRTreatmentRatePeriodCharacteristic treatmentRate = CMRTreatmentRatePeriodCharacteristic.New(Factory);
			treatmentRate.TR_CharacteristicCode = ZShort.Parse(CharacteristicCodeList.Codes.NonTaxableImports);
			treatmentRate.TR_TreatmentRatePeriodSnapshotPreferenceSchemeType = "GEN";
			treatmentRate.TR_TreatmentRatePeriodSnapshotCode = "111";
			treatmentRate.TR_TreatmentRatePeriodSnapshotRateNumber = "222";

			JobDeclaration declaration = JobDeclaration.New(Factory);
			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			JobComInvoiceLine invoiceLine = invoice.JobComInvoiceLines.AddNew();

			invoiceLine.JI_Tariff = "0000.00.00 00";
			invoiceLine.AddInfo.ZA_PST = "GEN";
			invoiceLine.AddInfo.ZA_RNO = "000";
			AssertEquals("This combination deems GST exemption", true, invoiceLine.DoesTariffRateOrTreatmentCodeDeemGSTExemption);

			invoiceLine.AddInfo.ZA_RNO = "001";
			AssertEquals("This combination does not deem GST exemption", false, invoiceLine.DoesTariffRateOrTreatmentCodeDeemGSTExemption);

			invoiceLine.AddInfo.ZA_TreatmentCode_Hidden = "111";
			invoiceLine.AddInfo.ZA_TRN = "222";
			AssertEquals("This combination deems GST exemption", true, invoiceLine.DoesTariffRateOrTreatmentCodeDeemGSTExemption);

			invoiceLine.AddInfo.ZA_TRN = "220";
			AssertEquals("This combination does not deem GST exemption", false, invoiceLine.DoesTariffRateOrTreatmentCodeDeemGSTExemption);
		}

		public void TestJI_CustomsValue()
		{
			JobDeclaration declaration = JobDeclaration.New(Factory);
			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			JobComInvoiceLine invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoice.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;
			invoiceLine.AddInfo.ZA_ADJ = "400AUD";
			invoiceLine.JI_LinePrice = 100;
			AssertEquals(500m, invoiceLine.JI_CustomsValue);
			invoiceLine.AddInfo.ZA_ADJ = "400USD";
			AssertEquals(387.77m, invoiceLine.JI_CustomsValue);
		}

		public void TestJI_BondedWarehouseLineKey()
		{
			JobDeclaration declaration = JobDeclaration.New(Factory);
			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			JobComInvoiceLine invoiceLine = invoice.JobComInvoiceLines.AddNew();
			AssertEquals(ZGuid.Empty, invoiceLine.JI_BondedWarehouseLineKey);
			ZGuid key = ZGuid.NewZGuid();
			invoiceLine.JI_BondedWarehouseLineKey = key;
			AssertEquals(key, invoiceLine.JI_BondedWarehouseLineKey);
			AssertEquals(key, invoiceLine.AddInfo.ZA_BondedWarehouseLineKey_Hidden);
			ZGuid key2 = ZGuid.NewZGuid();
			invoiceLine.AddInfo.ZA_BondedWarehouseLineKey_Hidden = key2;
			AssertEquals(key2, invoiceLine.JI_BondedWarehouseLineKey);
			AssertEquals(key2, invoiceLine.AddInfo.ZA_BondedWarehouseLineKey_Hidden);
		}

		public void TestSettingPackToBondUpdatesWRQWhenNoUQ()
		{
			JobDeclaration declaration = JobDeclaration.New(Factory);
			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			JobComInvoiceLine invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_InvoiceQuantity = 4;
			invoiceLine.JI_InvoiceUQ = "KG";
			AssertEquals("ZA_WRU", "", invoiceLine.AddInfo.ZA_WRU);
			AssertEquals("ZA_WRQ", 0m, invoiceLine.AddInfo.ZA_WRQ);
			invoiceLine.JI_IsPackToBondForLine = true;
			AssertEquals("ZA_WRU", "KG", invoiceLine.AddInfo.ZA_WRU);
			AssertEquals("ZA_WRQ", 4m, invoiceLine.AddInfo.ZA_WRQ);
		}

		public void TestUseBondedWarehouseAutomation()
		{
			JobDeclaration declaration = JobDeclaration.New(Factory);
			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			JobComInvoiceLine invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.AddInfo.UseBondedWarehouseAutomation = true;
			AssertEquals(true, invoiceLine.UseBondedWarehouseAutomation);
			invoiceLine.AddInfo.UseBondedWarehouseAutomation = false;
			AssertEquals(false, invoiceLine.UseBondedWarehouseAutomation);
			invoiceLine.UseBondedWarehouseAutomation = true;
			AssertEquals(true, invoiceLine.UseBondedWarehouseAutomation);
			AssertEquals(true, invoiceLine.AddInfo.UseBondedWarehouseAutomation);
			invoiceLine.UseBondedWarehouseAutomation = false;
			AssertEquals(false, invoiceLine.UseBondedWarehouseAutomation);
			AssertEquals(false, invoiceLine.AddInfo.UseBondedWarehouseAutomation);
		}

		public void TestAUStateReadOnly()
		{
			var declaration = Factory.New<JobDeclaration>();
			var header = declaration.Invoices.AddNew();
			var line = header.JobComInvoiceLines.AddNew();
			Assert("AUState should not be greyed out.", !line.JI_AUState_ReadOnly);
		}

		public void TestIsPackToBondForWEA()
		{
			JobDeclaration declaration = JobDeclaration.New(Factory);
			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			JobComInvoiceLine invoiceLine = invoice.JobComInvoiceLines.AddNew();

			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			AssertEquals(false, invoiceLine.JI_IsPackToBondForLine);
			AssertEquals(false, invoiceLine.JI_IsPackToBondForLineInfo.ReadOnly);

			declaration.JE_MessageType = JobMessageTypeList.Codes.WarehousedByExternalAgent;
			AssertEquals(true, invoiceLine.JI_IsPackToBondForLine);
			AssertEquals(true, invoiceLine.JI_IsPackToBondForLineInfo.ReadOnly);

			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			AssertEquals(false, invoiceLine.JI_IsPackToBondForLine);
			AssertEquals(false, invoiceLine.JI_IsPackToBondForLineInfo.ReadOnly);
		}

		public void TestWarehouseAddressAndCCP()
		{
			CustomsDataRegistry.Instance.WHSUniversalXMLChangeDate.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, ZDateTime.Today.AddMonths(1).ToDateTime());
			var declaration = JobDeclaration.New(Factory);
			declaration.JE_SystemCreateTimeUtc = ZDateTime.Now;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_IsPackToBondForLine = true;

			var address1 = OrgHeader.New(Factory).MainAddress;
			address1.LocalControlledPremisesID = "ccp1";

			var address2 = OrgHeader.New(Factory).MainAddress;
			address2.LocalControlledPremisesID = "ccp2";

			invoiceLine.AddInfo.ZA_OA_WarehouseAddress_Hidden = address1.PK;
			AssertEquals("CCP1", invoiceLine.WarehouseCCP.ToUpper());
			AssertEquals(address1, invoiceLine.WarehouseAddress);

			declaration.WarehouseDocAddress.E2_OA_Address = address2.PK;
			AssertEquals("CCP1", invoiceLine.WarehouseCCP.ToUpper());
			AssertEquals(address1, invoiceLine.WarehouseAddress);

			invoiceLine.AddInfo.ZA_OA_WarehouseAddress_Hidden = ZGuid.Empty;
			AssertEquals("CCP2", invoiceLine.WarehouseCCP.ToUpper());
			AssertEquals(address2, invoiceLine.WarehouseAddress);

			declaration.WarehouseDocAddress.E2_OA_Address = ZGuid.Empty;
			AssertEquals("", invoiceLine.WarehouseCCP);
			AssertNull(invoiceLine.WarehouseAddress);

			invoiceLine.JI_IsPackToBondForLine = false;
			declaration.WarehouseDocAddress.E2_OA_Address = address2.PK;
			invoiceLine.AddInfo.ZA_OA_WarehouseAddress_Hidden = address1.PK;
			AssertEquals("", invoiceLine.WarehouseCCP);
			AssertNull(invoiceLine.WarehouseAddress);

			declaration.JE_MessageType = JobMessageTypeList.Codes.ExWarehouse;
			AssertEquals("CCP1", invoiceLine.WarehouseCCP.ToUpper());
			AssertEquals(address1, invoiceLine.WarehouseAddress);

			declaration.JE_SystemCreateTimeUtc = ZDateTime.Today.AddMonths(2);
			AssertEquals("CCP1", invoiceLine.WarehouseCCP.ToUpper());
			AssertEquals(address1, invoiceLine.WarehouseAddress);

			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			AssertEquals("", invoiceLine.WarehouseCCP.ToUpper());
			AssertNull(invoiceLine.WarehouseAddress);

			invoiceLine.JI_IsPackToBondForLine = true;
			AssertEquals("CCP2", invoiceLine.WarehouseCCP.ToUpper());
			AssertEquals(address2, invoiceLine.WarehouseAddress);
		}

		public void TestHasNoPreferenceRate()
		{
			JobDeclaration testDec = JobDeclaration.New(Factory);
			JobComInvoiceHeader invoice = testDec.Invoices.AddNew();

			CMRTariffRatePeriodSnapshot tariffRate = CMRTariffRatePeriodSnapshot.New(Factory);
			tariffRate.TT_TariffClassificationNumber = "00000000";
			tariffRate.TT_PreferenceSchemeType = "GEN";
			tariffRate.TT_StartDate = new ZDateTime(2005, 1, 1);

			JobComInvoiceLine invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "00000000 00";
			AssertEquals("Has no preferential rate", true, invoiceLine.HasNoPreferentialRate);
			AssertEquals("IsGeneralRate", true, invoiceLine.IsGeneralRate);

			CMRTariffRatePeriodSnapshot tariffRate2 = CMRTariffRatePeriodSnapshot.New(Factory);
			tariffRate2.TT_TariffClassificationNumber = "00000001";
			tariffRate2.TT_PreferenceSchemeType = "US";
			tariffRate2.TT_StartDate = new ZDateTime(2005, 1, 1);

			invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "00000001 00";
			AssertEquals("Has no preferential rate", false, invoiceLine.HasNoPreferentialRate);
			AssertEquals("IsGeneralRate", false, invoiceLine.IsGeneralRate);
		}

		public void TestPreferenceDetailsInheriting()
		{
			var tariffScheme1 = CMRTariffRatePeriodSnapshot.New(Factory);
			tariffScheme1.TT_PreferenceSchemeType = "GEN";
			tariffScheme1.TT_StartDate = new ZDateTime(2005, 1, 1);
			tariffScheme1.TT_TariffClassificationNumber = "00000000";
			var tariffScheme2 = CMRTariffRatePeriodSnapshot.New(Factory);
			tariffScheme2.TT_PreferenceSchemeType = "GEN";
			tariffScheme2.TT_StartDate = new ZDateTime(2005, 1, 1);
			tariffScheme2.TT_TariffClassificationNumber = "00000001";
			var tariffScheme3 = CMRTariffRatePeriodSnapshot.New(Factory);
			tariffScheme3.TT_PreferenceSchemeType = "DCT";
			tariffScheme3.TT_StartDate = new ZDateTime(2005, 1, 1);
			tariffScheme3.TT_TariffClassificationNumber = "00000001";

			var schemeCountry1 = Factory.New<CMRPreferenceSchemePeriodCountry>();
			schemeCountry1.PC_PreferenceSchemePeriodSnapshotSchemeType = "GEN";
			schemeCountry1.PC_CountryCode = "CN";
			var schemeCountry2 = Factory.New<CMRPreferenceSchemePeriodCountry>();
			schemeCountry2.PC_PreferenceSchemePeriodSnapshotSchemeType = "DCT";
			schemeCountry2.PC_CountryCode = "CN";
			Factory.Save();

			var declaration = Factory.New<JobDeclaration>();
			var invoiceHeader = declaration.Invoices.AddNew();
			invoiceHeader.AddInfo.ZA_POC = "CN";
			invoiceHeader.AddInfo.ZA_PST = "DCT";
			invoiceHeader.AddInfo.ZA_PRT = "P50";

			var invoiceLine = invoiceHeader.InvoiceLines.AddNew() as JobComInvoiceLine;
			invoiceLine.JI_Tariff = "00000000";
			AssertEquals("Getting all preference detailes from header but Tariff not supporting ZA_PST DCT, InheritingPreferenceDetailsFromHeader should be false", false, invoiceLine.InheritingPreferenceDetailsFromHeader);

			invoiceLine.JI_Tariff = "00000001";
			AssertEquals("Getting all preference detailes from header and tariff supporting DCT, InheritingPreferenceDetailsFromHeader should be true", true, invoiceLine.InheritingPreferenceDetailsFromHeader);

			invoiceLine.AddInfo.ZA_POC = "CN";
			AssertEquals("InheritingPreferenceDetailsFromHeader should return false: using its own Pref. Origin", false, invoiceLine.InheritingPreferenceDetailsFromHeader);
			invoiceLine.AddInfo.ZA_POC = "";

			invoiceLine.AddInfo.ZA_PST = "DCT";
			AssertEquals("InheritingPreferenceDetailsFromHeader should return false: using its own Pref. Scheme", false, invoiceLine.InheritingPreferenceDetailsFromHeader);
			invoiceLine.AddInfo.ZA_PST = "";

			invoiceLine.AddInfo.ZA_PRT = "P50";
			AssertEquals("InheritingPreferenceDetailsFromHeader should return false: using its own Pref. Rule", false, invoiceLine.InheritingPreferenceDetailsFromHeader);
			invoiceLine.AddInfo.ZA_PRT = "";
			AssertEquals("Getting all preference detailes from header and tariff supporting DCT, InheritingPreferenceDetailsFromHeader should be true", true, invoiceLine.InheritingPreferenceDetailsFromHeader);

			invoiceHeader.AddInfo.ZA_PST = "GEN";
			AssertEquals("Getting all preference detailes from header but Pref. Scheme is GST, InheritingPreferenceDetailsFromHeader should be false", false, invoiceLine.InheritingPreferenceDetailsFromHeader);
		}
	}
}
