using System;
using System.Collections.Generic;
using System.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.Customs.FR;
using Enterprise.Build.Database.Script.Public.MasterFiles.RefExchangeRate;
using Enterprise.Build.Database.Script.TestFramework;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Customs.FR.Testing
{
	[TestedType(typeof(Report_FRVAT))]
	class Report_FRVATTest : CustomsReportDbCreateScriptTest
	{
		#region Columns

		public void TestJobNumber()
		{
			AssertFunctionReturnExpectedValue("JobNumber", "B0001");

			UpdateDeclarationColumn("JE_DeclarationReference", "B0002", SqlDbType.VarChar, declarationPK);
			AssertFunctionReturnExpectedValue("JobNumber", "B0002");
		}

		public void TestDUCR()
		{
			AssertFunctionReturnExpectedValue("DUCR", "");

			UpdateDeclarationColumn("JE_UCR", "DUCR0001", SqlDbType.VarChar, declarationPK);
			AssertFunctionReturnExpectedValue("DUCR", "DUCR0001");
		}

		public void TestEntryNumber()
		{
			AssertFunctionReturnExpectedValue("EntryNumber", "EB0001");

			UpdateEntryNumberColumn("CE_EntryNum", "E0002", SqlDbType.VarChar, entryNumPK);
			AssertFunctionReturnExpectedValue("EntryNumber", "E0002");
		}

		public void TestEntryReference()
		{
			AssertFunctionReturnExpectedValue("EntryReference", "");

			UpdateEntryHeaderColumn("CH_BGMReference", "B0001/1", SqlDbType.VarChar, entryHeaderPK);
			AssertFunctionReturnExpectedValue("EntryReference", "B0001/1");
		}

		public void TestBAEDate()
		{
			UpdateTableColumn("CusEntryHeader", "CH_EntryReleaseDate", new DateTime(2022, 04, 01), SqlDbType.SmallDateTime, "CH_PK", entryHeaderPK);
			AssertFunctionReturnExpectedValue("BAEDate", new DateTime(2022, 04, 01));
		}

		public void TestCPC()
		{
			AssertFunctionReturnExpectedValue("CPC", "   (   /    )");

			UpdateDeclarationColumn("JE_MessageSubType", "IM", SqlDbType.Char, declarationPK);
			UpdateInvoiceLineColumn("JI_Procedure", "7041000", SqlDbType.VarChar, invoiceLinePK);
			AssertFunctionReturnExpectedValue("CPC", "   (IM /7041)");

			var entryInstructionPK = SetupEntryInstruction(declarationPK, DateTime.Now, clusterKey);
			LinkEntryInstructionToInvoiceLine(entryInstructionPK, invoiceLinePK);
			UpdateTableColumn("CusEntryInstruction", "CEI_Style", "70P", SqlDbType.VarChar, "CEI_PK", entryInstructionPK);
			UpdateTableColumn("CusEntryInstruction", "CEI_SubStyle", "A", SqlDbType.VarChar, "CEI_PK", entryInstructionPK);
			AssertFunctionReturnExpectedValue("CPC", "70P(IMA/7041)");
		}

		public void TestCountriesOfSupply()
		{
			AssertFunctionReturnExpectedValue("CountriesOfSupply", "US");

			var invoiceLine2PK = SetupInvoiceLine(invoiceHeaderPK, clusterKey, "CN");
			var entryLine2PK = SetupEntryLine(entryHeaderPK, clusterKey);
			var entryLineFee2PK = SetupEntryLineFee(entryLine2PK, "CUS", "B00", "6", 140, 22, clusterKey);
			LinkInvoiceLineToEntryLine(invoiceLine2PK, entryLine2PK);
			AssertFunctionReturnExpectedValue("CountriesOfSupply", "CN/US"); // order by

			var invoiceLine3PK = SetupInvoiceLine(invoiceHeaderPK, clusterKey, "CN");
			var entryLine3PK = SetupEntryLine(entryHeaderPK, clusterKey);
			var entryLineFee3PK = SetupEntryLineFee(entryLine3PK, "CUS", "B00", "6", 140, 22, clusterKey);
			LinkInvoiceLineToEntryLine(invoiceLine3PK, entryLine3PK);
			AssertFunctionReturnExpectedValue("CountriesOfSupply", "CN/US"); // distinct
		}

		public void TestInvoiceNumbers()
		{
			AssertFunctionReturnExpectedValue("InvoiceNumbers", DBNull.Value);

			SetupSupportingDocument("JZ", invoiceHeaderPK, "SUP", "N380", "INV0009");
			AssertFunctionReturnExpectedValue("InvoiceNumbers", "INV0009");

			SetupSupportingDocument("JZ", invoiceHeaderPK, "SUP", "N380", "INV0001");
			AssertFunctionReturnExpectedValue("InvoiceNumbers", "INV0001/INV0009"); // order by

			SetupSupportingDocument("JZ", invoiceHeaderPK, "SUP", "N380", "INV0009");
			AssertFunctionReturnExpectedValue("InvoiceNumbers", "INV0001/INV0009"); // distinct
		}

		public void TestTotalInvoiceAmount()
		{
			AssertFunctionReturnExpectedValue("TotalInvoiceAmount", 0m);

			UpdateEntryLineColumn("CL_InvoiceAmount", 20m, SqlDbType.Decimal, entryLinePK);
			UpdateEntryLineColumn("CL_RX_NKInvoiceAmountCurrency", "EUR", SqlDbType.VarChar, entryLinePK);
			AssertFunctionReturnExpectedValue("TotalInvoiceAmount", 20m);

			var invoiceLine1BPK = SetupInvoiceLine(invoiceHeaderPK, clusterKey, "AU");
			LinkInvoiceLineToEntryLine(invoiceLine1BPK, entryLinePK);
			AssertFunctionReturnExpectedValue("TotalInvoiceAmount", 20m);

			var invoiceLine2PK = SetupInvoiceLine(invoiceHeaderPK, clusterKey, "AU");
			var entryLine2PK = SetupEntryLine(entryHeaderPK, clusterKey);
			SetupEntryLineFee(entryLine2PK, "CUS", "B00", "6", 40, 4, clusterKey);
			LinkInvoiceLineToEntryLine(invoiceLine2PK, entryLine2PK);
			UpdateEntryLineColumn("CL_InvoiceAmount", 30m, SqlDbType.Decimal, entryLine2PK);
			UpdateEntryLineColumn("CL_RX_NKInvoiceAmountCurrency", "EUR", SqlDbType.VarChar, entryLine2PK);
			AssertFunctionReturnExpectedValue("TotalInvoiceAmount", 50m);
		}

		public void TestTotalInvoiceAmount_OneLineHasTax6_TheOtherLineHasNoTax6()
		{
			AssertFunctionReturnExpectedValue("TotalInvoiceAmount", 0m);

			UpdateEntryLineColumn("CL_InvoiceAmount", 20m, SqlDbType.Decimal, entryLinePK);
			UpdateEntryLineColumn("CL_RX_NKInvoiceAmountCurrency", "EUR", SqlDbType.VarChar, entryLinePK);
			AssertFunctionReturnExpectedValue("TotalInvoiceAmount", 20m);

			var invoiceLine2PK = SetupInvoiceLine(invoiceHeaderPK, clusterKey, "AU");
			var entryLine2PK = SetupEntryLine(entryHeaderPK, clusterKey);
			LinkInvoiceLineToEntryLine(invoiceLine2PK, entryLine2PK);
			UpdateEntryLineColumn("CL_InvoiceAmount", 30m, SqlDbType.Decimal, entryLine2PK);
			UpdateEntryLineColumn("CL_RX_NKInvoiceAmountCurrency", "EUR", SqlDbType.VarChar, entryLine2PK);
			AssertFunctionReturnExpectedValue("TotalInvoiceAmount", 20m);
		}

		public void TestCurrency()
		{
			AssertFunctionReturnExpectedValue("Currency", "EUR");

			UpdateEntryLineColumn("CL_RX_NKInvoiceAmountCurrency", "USD", SqlDbType.VarChar, entryLinePK);
			AssertFunctionReturnExpectedValue("Currency", "USD");
		}

		public void TestRate()
		{
			AssertFunctionReturnExpectedValue("Rate", 1m);

			UpdateEntryLineColumn("CL_RX_NKInvoiceAmountCurrency", "USD", SqlDbType.VarChar, entryLinePK);
			UpdateInvoiceColumn("JZ_ValuationDateOverride", "2023-10-20", SqlDbType.SmallDateTime, invoiceHeaderPK);
			SetupExchangeRate("USD", 1.12m);
			AssertFunctionReturnExpectedValue("Rate", 1m);

			UpdateInvoiceColumn("JZ_ValuationDateOverride", "2023-10-19 1:00:00", SqlDbType.SmallDateTime, invoiceHeaderPK);
			AssertFunctionReturnExpectedValue("Rate", 1.12m);
		}

		public void TestImporter()
		{
			AssertFunctionReturnExpectedValue("ImporterCode", "IMP1");

			UpdateTableColumn("OrgHeader", "OH_Code", "IMP4", SqlDbType.VarChar, "OH_PK", importerPK);
			UpdateTableColumn("OrgHeader", "OH_FullName", "Another Importer", SqlDbType.VarChar, "OH_PK", importerPK);
			AssertFunctionReturnExpectedValue("ImporterCode", "IMP4");
			AssertFunctionReturnExpectedValue("ImporterName", "Another Importer");
		}

		public void TestSupplierName()
		{
			AssertFunctionReturnExpectedValue("SupplierName", "TEST SUP1");

			UpdateTableColumn("OrgHeader", "OH_FullName", "Another Supplier", SqlDbType.VarChar, "OH_PK", supplierPK);
			AssertFunctionReturnExpectedValue("SupplierName", "Another Supplier");
		}

		public void TestEORISuffixFallbacksToEORIWithoutAddressIfNecessary()
		{
			TestDataCreator.CreateOrgCusCode(importerPK, "EOR", "123456789", "FR");
			TestDataCreator.CreateOrgCusCode(importerPK, "EBS", "11111", "FR");

			var addressPK1 = TestDataCreator.CreateAddress(importerPK, "AD1", "Here");
			TestDataCreator.CreateOrgCusCode(importerPK, "EBS", "22222", "FR", addressPK1);

			var addressPK2 = TestDataCreator.CreateAddress(importerPK, "AD2", "There");
			TestDataCreator.CreateDocAddress(addressPK2, "", declarationPK, "JE", "IMD");

			AssertFunctionReturnExpectedValue("EORISuffix", "11111");
		}

		public void TestEORISuffixFiltersOnCorrectAddressWhenAble()
		{
			TestDataCreator.CreateOrgCusCode(importerPK, "EOR", "123456789", "FR");
			TestDataCreator.CreateOrgCusCode(importerPK, "EBS", "11111", "FR");

			var addressPK1 = TestDataCreator.CreateAddress(importerPK, "AD1", "Here");
			TestDataCreator.CreateOrgCusCode(importerPK, "EBS", "22222", "FR", addressPK1);

			var addressPK2 = TestDataCreator.CreateAddress(importerPK, "AD2", "There");
			TestDataCreator.CreateOrgCusCode(importerPK, "EBS", "33333", "FR", addressPK2);

			TestDataCreator.CreateDocAddress(addressPK1, "", declarationPK, "JE", "IMD");

			AssertFunctionReturnExpectedValue("EORISuffix", "22222");
		}

		public void TestEoriSuffixEmptyWhenEORIIsFROccasionnel()
		{
			var addressPK = TestDataCreator.CreateAddress(importerPK, "AD1", "There");
			TestDataCreator.CreateDocAddress(addressPK, "", declarationPK, "JE", "IMD");

			var eori = TestDataCreator.CreateOrgCusCode(importerPK, "EOR", "12345", "FR");
			TestDataCreator.CreateOrgCusCode(importerPK, "EBS", "00001", "FR");
			AssertFunctionReturnExpectedValue("EORISuffix ", "00001");

			UpdateTableColumn("OrgCusCode", "OK_CustomsRegNo", "OCCASIONNEL", SqlDbType.VarChar, "OK_PK", eori);
			AssertFunctionReturnExpectedValue("EORISuffix ", string.Empty);
		}

		public void TestEORI_OCCASIONNEL_InFR()
		{
			AssertFunctionReturnExpectedValue("EORI", DBNull.Value);

			TestDataCreator.CreateOrgCusCode(importerPK, "EOR", "OCCASIONNEL", "FR");
			var addressPK = TestDataCreator.CreateAddress(importerPK, "AD1", "There");
			TestDataCreator.CreateDocAddress(addressPK, "", declarationPK, "JE", "IMD");
			AssertFunctionReturnExpectedValue("EORI", "OCCASIONNEL");
		}

		public void TestEORI_OCCASIONNEL_NotInFR()
		{
			AssertFunctionReturnExpectedValue("EORI", DBNull.Value);

			TestDataCreator.CreateOrgCusCode(importerPK, "EOR", "OCCASIONNEL", "DE");
			var addressPK = TestDataCreator.CreateAddress(importerPK, "AD1", "There");
			TestDataCreator.CreateDocAddress(addressPK, "", declarationPK, "JE", "IMD");
			AssertFunctionReturnExpectedValue("EORI", "DEOCCASIONNEL");
		}

		public void TestEORI_ContainsCountryCode()
		{
			AssertFunctionReturnExpectedValue("EORI", DBNull.Value);

			TestDataCreator.CreateOrgCusCode(importerPK, "EOR", "FR002300", "FR");
			var addressPK = TestDataCreator.CreateAddress(importerPK, "AD1", "There");
			TestDataCreator.CreateDocAddress(addressPK, "", declarationPK, "JE", "IMD");
			AssertFunctionReturnExpectedValue("EORI", "FR002300");
		}

		public void TestEORI_ContainsWrongCountryCode()
		{
			AssertFunctionReturnExpectedValue("EORI", DBNull.Value);

			TestDataCreator.CreateOrgCusCode(importerPK, "EOR", "GB002300", "FR");
			var addressPK = TestDataCreator.CreateAddress(importerPK, "AD1", "There");
			TestDataCreator.CreateDocAddress(addressPK, "", declarationPK, "JE", "IMD");
			AssertFunctionReturnExpectedValue("EORI", "FRGB002300");
		}

		public void TestEORI_IssuedByOtherCountry()
		{
			AssertFunctionReturnExpectedValue("EORI", DBNull.Value);

			TestDataCreator.CreateOrgCusCode(importerPK, "EOR", "002300", "DE");
			var addressPK = TestDataCreator.CreateAddress(importerPK, "AD1", "There");
			TestDataCreator.CreateDocAddress(addressPK, "", declarationPK, "JE", "IMD");
			AssertFunctionReturnExpectedValue("EORI", "DE002300");
		}

		public void TestEORI_IssuedByGB_AndStartsWithXI()
		{
			AssertFunctionReturnExpectedValue("EORI", DBNull.Value);

			TestDataCreator.CreateOrgCusCode(importerPK, "EOR", "XI007700", "GB");
			var addressPK = TestDataCreator.CreateAddress(importerPK, "AD1", "There");
			TestDataCreator.CreateDocAddress(addressPK, "", declarationPK, "JE", "IMD");
			AssertFunctionReturnExpectedValue("EORI", "XI007700");
		}

		public void TestEORI_FallbacksToForeignIfNecessary()
		{
			AssertFunctionReturnExpectedValue("EORI", DBNull.Value);

			TestDataCreator.CreateOrgCusCode(importerPK, "EOR", "EORI_GB", "GB");
			var addressPK = TestDataCreator.CreateAddress(importerPK, "AD1", "There");
			TestDataCreator.CreateDocAddress(addressPK, "", declarationPK, "JE", "IMD");
			AssertFunctionReturnExpectedValue("EORI", "GBEORI_GB");

			TestDataCreator.CreateOrgCusCode(importerPK, "EOR", "EORI_FR", "FR");
			AssertFunctionReturnExpectedValue("EORI", "FREORI_FR");
		}

		public void TestVATNumber()
		{
			AssertFunctionReturnExpectedValue("VATNumber", DBNull.Value);

			var vatCodePK = TestDataCreator.CreateOrgCusCode(importerPK, "TVA", "004400", "FR");
			AssertFunctionReturnExpectedValue("VATNumber", "004400");
		}

		public void TestVATNumber_IssuedByOtherCountry()
		{
			AssertFunctionReturnExpectedValue("VATNumber", DBNull.Value);

			var vatCodePK = TestDataCreator.CreateOrgCusCode(importerPK, "TVA", "004400", "DE");
			AssertFunctionReturnExpectedValue("VATNumber", DBNull.Value);
		}

		public void TestVATAmount()
		{
			AssertFunctionReturnExpectedValue("TotalVATBaseAmount", 10m);
			AssertFunctionReturnExpectedValue("TotalVATAmount", 1m);

			UpdateEntryLineFee("CF_MethodOfPayment", "3", SqlDbType.VarChar, entryLineFeePK);
			var rows = GetFilteredRows("TotalVATBaseAmount");
			AssertEquals(0, rows.Length);  // wrong method of payment

			SetupEntryLineFee(entryLinePK, "CW1", "B00", "6", 10, 1, clusterKey);
			rows = GetFilteredRows("TotalVATBaseAmount");
			AssertEquals(0, rows.Length);  // wrong source

			SetupEntryLineFee(entryLinePK, "CUS", "A00", "6", 20, 2, clusterKey);
			rows = GetFilteredRows("TotalVATBaseAmount");
			AssertEquals(0, rows.Length); // wrong charge type

			SetupEntryLineFee(entryLinePK, "CUS", "B00", "2", 30, 3, clusterKey);
			rows = GetFilteredRows("TotalVATBaseAmount");
			AssertEquals(0, rows.Length); // wrong method of payment

			SetupEntryLineFee(entryLinePK, "CUS", "B00", "6", 40, 4, clusterKey);
			AssertFunctionReturnExpectedValue("TotalVATBaseAmount", 40m);
			AssertFunctionReturnExpectedValue("TotalVATAmount", 4m);

			var invoiceLine2PK = SetupInvoiceLine(invoiceHeaderPK, clusterKey, "CN");
			var entryLine2PK = SetupEntryLine(entryHeaderPK, clusterKey);
			var entryLineFee2PK = SetupEntryLineFee(entryLine2PK, "CUS", "B00", "6", 50, 5, clusterKey);
			LinkInvoiceLineToEntryLine(invoiceLine2PK, entryLine2PK);
			AssertFunctionReturnExpectedValue("TotalVATBaseAmount", 90m);
			AssertFunctionReturnExpectedValue("TotalVATAmount", 9m);

			var invoiceLine2BPK = SetupInvoiceLine(invoiceHeaderPK, clusterKey, "AU");
			LinkInvoiceLineToEntryLine(invoiceLine2BPK, entryLine2PK);
			AssertFunctionReturnExpectedValue("TotalVATBaseAmount", 90m);
			AssertFunctionReturnExpectedValue("TotalVATAmount", 9m);
		}

		public void TestVATAmount_WithTwoCurrencies()
		{
			UpdateInvoiceColumn("JZ_RX_NKInvoice_Currency", "JPY", SqlDbType.VarChar, invoiceHeaderPK);
			UpdateEntryLineColumn("CL_InvoiceAmount", 120, SqlDbType.Decimal, entryLinePK);
			UpdateEntryLineColumn("CL_RX_NKInvoiceAmountCurrency", "JPY", SqlDbType.VarChar, entryLinePK);

			var invoiceHeader2PK = SetupInvoiceHeader(declarationPK, clusterKey);
			var invoiceLine2PK = SetupInvoiceLine(invoiceHeader2PK, clusterKey, "TN");
			var baeEvent2PK = SetupStmALog(entryHeaderPK, "CES", "100", DateTime.Now);
			var entryLine2PK = SetupEntryLine(entryHeaderPK, clusterKey);
			var entryLineFee2PK = SetupEntryLineFee(entryLine2PK, "CUS", "B00", "6", 140, 22, clusterKey);
			LinkInvoiceLineToEntryLine(invoiceLine2PK, entryLine2PK);
			UpdateInvoiceColumn("JZ_RX_NKInvoice_Currency", "CNY", SqlDbType.VarChar, invoiceHeader2PK);
			UpdateEntryLineColumn("CL_InvoiceAmount", 160, SqlDbType.Decimal, entryLine2PK);
			UpdateEntryLineColumn("CL_RX_NKInvoiceAmountCurrency", "CNY", SqlDbType.VarChar, entryLine2PK);

			var rows = GetFilteredRows("TotalInvoiceAmount");
			AssertEquals(2, rows.Length);
			AssertContainsExactElementsInAnyOrder(new decimal[] { 120, 160 }, rows);

			rows = GetFilteredRows("TotalVATBaseAmount");
			AssertEquals(2, rows.Length);
			AssertContainsExactElementsInAnyOrder(new decimal[] { 10, 140 }, rows);

			rows = GetFilteredRows("TotalVATAmount");
			AssertEquals(2, rows.Length);
			AssertContainsExactElementsInAnyOrder(new decimal[] { 1, 22 }, rows);
		}

		public void TestVATProcedure()
		{
			AssertFunctionReturnExpectedValue("VATProcedureCode", "");
			AssertFunctionReturnExpectedValue("VATProcedureDescription", "");

			UpdateDeclarationColumn("JE_AddInfo", "VATDeferType=" + "2", SqlDbType.VarChar, declarationPK);
			AssertFunctionReturnExpectedValue("VATProcedureCode", "2");
			AssertFunctionReturnExpectedValue("VATProcedureDescription", "AI2");

			UpdateDeclarationColumn("JE_AddInfo", "VATDeferType=" + "L", SqlDbType.VarChar, declarationPK);
			AssertFunctionReturnExpectedValue("VATProcedureCode", "L");
			AssertFunctionReturnExpectedValue("VATProcedureDescription", "ATVAI");

			UpdateDeclarationColumn("JE_AddInfo", "VATDeferType=" + "S", SqlDbType.VarChar, declarationPK);
			AssertFunctionReturnExpectedValue("VATProcedureCode", "S");
			AssertFunctionReturnExpectedValue("VATProcedureDescription", "Standard");
		}

		public void TestOrderRefs()
		{
			AssertFunctionReturnExpectedValue("OrderRefs", DBNull.Value);
			var jobDocAndCartagePk = TestDataCreator.CreateJobDocsAndCartage(declarationPK, "JE");
			TestDataCreator.CreateJobOrderItem(jobDocAndCartagePk, "xxx");
			TestDataCreator.CreateJobOrderItem(jobDocAndCartagePk, "yyy");
			AssertFunctionReturnExpectedValue("OrderRefs", "xxx,yyy");
		}

		#endregion

		#region Filters

		public void TestCompany()
		{
			var company2PK = SetupCompany("FR2");
			var branch2PK = SetupBranch(companyPK, "PA2", "FRCLIO");
			var filteredRows = GetFilteredRows("JobNumber", (Report_FRVATParameter.CompanyPK, company2PK));
			AssertEquals(0, filteredRows.Length);

			filteredRows = GetFilteredRows("JobNumber", (Report_FRVATParameter.CompanyPK, companyPK));
			AssertContainsExactElementsInAnyOrder(new [] { "B0001" }, filteredRows);

			filteredRows = GetFilteredRows("JobNumber", (Report_FRVATParameter.CompanyPK, null));
			AssertContainsExactElementsInAnyOrder(new[] { "B0001" }, filteredRows);
		}

		public void TestImporterPK()
		{
			var importer2PK = SetupOrganisation("IMP2", "IMP2", "Bla");
			var filteredRows = GetFilteredRows("JobNumber", (Report_FRVATParameter.ImporterPK, importer2PK));
			AssertEquals(0, filteredRows.Length);

			filteredRows = GetFilteredRows("JobNumber", (Report_FRVATParameter.ImporterPK, importerPK));
			AssertContainsExactElementsInAnyOrder(new[] { "B0001" }, filteredRows);
		}

		public void TestSupplierPK()
		{
			var supplierPK = SetupOrganisation("SUP2", "SUP2", "Bla");
			var filteredRows = GetFilteredRows("JobNumber", (Report_FRVATParameter.SupplierPK, supplierPK));
			AssertEquals(0, filteredRows.Length);

			UpdateDeclarationColumn("JE_OH_Supplier", supplierPK, SqlDbType.UniqueIdentifier, declarationPK);
			filteredRows = GetFilteredRows("JobNumber", (Report_FRVATParameter.SupplierPK, supplierPK));
			AssertContainsExactElementsInAnyOrder(new[] { "B0001" }, filteredRows);
		}

		public void TestVATProcedureCode()
		{
			var filteredRows = GetFilteredRows("JobNumber", (Report_FRVATParameter.VATProcedureCode, ""));
			AssertContainsExactElementsInAnyOrder(new[] { "B0001" }, filteredRows);

			UpdateDeclarationColumn("JE_AddInfo", "VATDeferType=" + "2", SqlDbType.VarChar, declarationPK);
			filteredRows = GetFilteredRows("JobNumber", (Report_FRVATParameter.VATProcedureCode, ""));
			AssertContainsExactElementsInAnyOrder("empty filter match all declarations", new[] { "B0001" }, filteredRows);

			filteredRows = GetFilteredRows("JobNumber", (Report_FRVATParameter.VATProcedureCode, "L"));
			AssertEquals("VAT procedure not matched", 0, filteredRows.Length);

			filteredRows = GetFilteredRows("JobNumber", (Report_FRVATParameter.VATProcedureCode, "2"));
			AssertContainsExactElementsInAnyOrder("VAT procedure matched", new[] { "B0001" }, filteredRows);
		}

		public void TestFranceAndOverseasDepartmentsAndTerritories()
		{
			UpdateTableColumn("GlbCompany", "GC_RN_NKCountryCode", "FR", SqlDbType.VarChar, "GC_PK", companyPK);
			var filteredRows = GetFilteredRows("JobNumber");
			AssertContainsExactElementsInAnyOrder(new[] { "B0001" }, filteredRows);

			UpdateTableColumn("GlbCompany", "GC_RN_NKCountryCode", "YT", SqlDbType.VarChar, "GC_PK", companyPK);
			filteredRows = GetFilteredRows("JobNumber");
			AssertContainsExactElementsInAnyOrder(new[] { "B0001" }, filteredRows);

			UpdateTableColumn("GlbCompany", "GC_RN_NKCountryCode", "CN", SqlDbType.VarChar, "GC_PK", companyPK);
			filteredRows = GetFilteredRows("JobNumber");
			AssertEquals(0, filteredRows.Length);
		}

		public void TestVATNumberSupportingDocument()
		{
			var filteredRows = GetFilteredRows("JobNumber");
			AssertContainsExactElementsInAnyOrder(new[] { "B0001" }, filteredRows);

			UpdateTableColumn("CusSupportingInfo", "CSI_Code", "G008", SqlDbType.VarChar, "CSI_PK", vatNumberDocPK);
			filteredRows = GetFilteredRows("JobNumber");
			AssertEquals(0, filteredRows.Length);
		}

		public void TestExcludeCancelledEntries()
		{
			var filteredRows = GetFilteredRows("JobNumber");
			AssertContainsExactElementsInAnyOrder(new[] { "B0001" }, filteredRows);

			UpdateEntryHeaderColumn("CH_EntryStatus", "150", SqlDbType.VarChar, entryHeaderPK);
			filteredRows = GetFilteredRows("JobNumber");
			AssertEquals(0, filteredRows.Length);
		}

		public void TestCL_CustomsPostedStatus()
		{
			var filteredRows = GetFilteredRows("JobNumber");
			AssertContainsExactElementsInAnyOrder(new[] { "B0001" }, filteredRows);

			UpdateEntryLineColumn("CL_CustomsPostedStatus", "DLT", SqlDbType.VarChar, entryLinePK);
			filteredRows = GetFilteredRows("JobNumber");
			AssertEquals(0, filteredRows.Length);

			UpdateEntryLineColumn("CL_CustomsPostedStatus", "DPD", SqlDbType.VarChar, entryLinePK);
			filteredRows = GetFilteredRows("JobNumber");
			AssertEquals(0, filteredRows.Length);
		}

		#endregion

		#region Implementation

		Guid SetupCompany(string companyCode)
		{
			return TestDataCreator.CreateCompany(companyCode, "FR", "EUR");
		}

		Guid SetupBranch(Guid companyPK, string branchCode, string homePort)
		{
			return TestDataCreator.CreateBranch(companyPK, branchCode, homePort);
		}

		Guid SetupOrganisation(string code, string addressCode, string address )
		{
			var orgPk = TestDataCreator.CreateOrganisation(code, "TEST " + code);
			TestDataCreator.CreateAddress(orgPk, addressCode, address);
			return orgPk;
		}

		Guid SetupDeclaration(Guid branchPK, Guid companyPK, string declarationReference, int clusterKey, string messageType = "IMP")
		{
			return TestDataCreator.CreateJobDeclaration(branchPK, companyPK, declarationReference, messageType, clusterKey);
		}

		Guid SetupInvoiceHeader(Guid declarationPK, int clusterKey)
		{
			return TestDataCreator.CreateJobComInvoiceHeader(declarationPK, clusterKey);
		}

		Guid SetupSupportingDocument(string parentTableCode, Guid parentPK, string type, string code, string reference)
		{
			return TestDataCreator.CreateCusSupportingInfo(type, code, "", reference, parentPK, parentTableCode, "FR");
		}

		Guid SetupEntryInstruction(Guid declarationPK, DateTime assessmentDate, int clusterKey)
		{
			return TestDataCreator.CreateCusEntryInstruction(declarationPK, "", "", assessmentDate, clusterKey);
		}

		Guid SetupEntryHeader(Guid declarationPK, int clusterKey, DateTime? entryReleaseDate)
		{
			return TestDataCreator.CreateCusEntryHeader(declarationPK, clusterKey, entryReleaseDate: entryReleaseDate);
		}

		Guid SetupEntryLine(Guid entryHeaderPK, int clusterKey)
		{
			var entryLinePK = TestDataCreator.CreateCusEntryLine(entryHeaderPK, clusterKey);
			UpdateEntryLineColumn("CL_CustomsPostedStatus", "ACT", SqlDbType.VarChar, entryLinePK);
			return entryLinePK;
		}

		Guid SetupInvoiceLine(Guid invoiceHeaderPK, int clusterKey, string countryOfSupply = "")
		{
			var invoiceLinePK = TestDataCreator.CreateJobComInvoiceLine(invoiceHeaderPK, clusterKey);
			if (!string.IsNullOrEmpty(countryOfSupply))
			{
				UpdateInvoiceLineColumn("JI_AddInfo", "CountryOfSupply=" + countryOfSupply, SqlDbType.VarChar, invoiceLinePK);
			}
			return invoiceLinePK;
		}

		Guid SetupExchangeRate(string currency, decimal rate)
		{
			var pk = Guid.NewGuid();
			RefExchangeRateTest.InsertRefExchangeRateZZ(TestConnection, pk, "FR", currency, "CUS", string.Empty, new DateTime(1900, 01, 01), new DateTime(2023, 10, 19), rate);
			return pk;
		}

		Guid SetupEntryLineFee(Guid entryLinePK, string source, string chargeType, string methodOfPayment, decimal baseAmount, float amount, int clusterKey)
		{
			var entryLineFeePK = TestDataCreator.CreateCusEntryLineFee(entryLinePK, chargeType, amount, clusterKey, source);
			UpdateEntryLineFee("CF_BaseValue", baseAmount, SqlDbType.Decimal, entryLineFeePK);
			UpdateEntryLineFee("CF_MethodOfPayment", methodOfPayment, SqlDbType.VarChar, entryLineFeePK);
			return entryLineFeePK;
		}

		Guid SetupStmALog(Guid entryHeaderPK, string @event, string reference, DateTime eventTime)
		{
			return TestDataCreator.CreateStmALog("CusEntryHeader", entryHeaderPK, reference, eventTime, @event);
		}

		Guid SetupEntryNum(Guid entryHeaderPK, string entryNum, string entryType = "IMP", string country = "FR")
		{
			return TestDataCreator.CreateCusEntryNum(entryHeaderPK, "CusEntryHeader", entryNum, entryType, "CUS", country);
		}

		void LinkInvoiceLineToEntryLine(Guid invoiceLinePK, Guid entryLinePK)
		{
			UpdateInvoiceLineColumn("JI_CL", entryLinePK, SqlDbType.UniqueIdentifier, invoiceLinePK);
		}

		void LinkEntryInstructionToInvoiceLine(Guid entryInstructionPK, Guid invoiceLinePK)
		{
			UpdateInvoiceLineColumn("JI_CEI", entryInstructionPK, SqlDbType.UniqueIdentifier, invoiceLinePK);
		}

		#endregion

		protected override void SetUp()
		{
			base.SetUp();

			companyPK = SetupCompany("FR1");
			branchPK = SetupBranch(companyPK, "PAR", "FRLEH");
			importerPK = SetupOrganisation("IMP1", "ADD1", "Here");
			supplierPK = SetupOrganisation("SUP1", "ADD2", "There");
			clusterKey = 1;
			(declarationPK, vatNumberDocPK, invoiceHeaderPK, invoiceLinePK, entryHeaderPK, entryNumPK, baeEventPK, entryLinePK, entryLineFeePK) = CreateDeclaration(clusterKey, "B0001");
		}

		(Guid, Guid, Guid, Guid, Guid, Guid, Guid, Guid, Guid) CreateDeclaration(int clusterKey, string jobReference)
		{
			var declarationPK = SetupDeclaration(branchPK, companyPK, jobReference, clusterKey);
			UpdateDeclarationColumn("JE_OH_Importer", importerPK, SqlDbType.UniqueIdentifier, declarationPK);
			UpdateDeclarationColumn("JE_OH_Supplier", supplierPK, SqlDbType.UniqueIdentifier, declarationPK);
			var vatNumberDocPK = SetupSupportingDocument("JE", declarationPK, "SUP", "1008", "VATNumber");
			var invoiceHeaderPK = SetupInvoiceHeader(declarationPK, clusterKey);
			var invoiceLinePK = SetupInvoiceLine(invoiceHeaderPK, clusterKey, "US");
			var entryHeaderPK = SetupEntryHeader(declarationPK, clusterKey, DateTime.Now);
			var entryNumPK = SetupEntryNum(entryHeaderPK, "E" + jobReference);
			var entryLinePK = SetupEntryLine(entryHeaderPK, clusterKey);
			var entryLineFeePK = SetupEntryLineFee(entryLinePK, "CUS", "B00", "6", 10, 1, clusterKey);
			LinkInvoiceLineToEntryLine(invoiceLinePK, entryLinePK);
			return (declarationPK, vatNumberDocPK, invoiceHeaderPK, invoiceLinePK, entryHeaderPK, entryNumPK, baeEventPK, entryLinePK, entryLineFeePK);
		}

		Guid companyPK;
		Guid branchPK;
		Guid importerPK;
		Guid supplierPK;
		int clusterKey;
		Guid declarationPK;
		Guid vatNumberDocPK;
		Guid invoiceHeaderPK;
		Guid invoiceLinePK;
		Guid entryHeaderPK;
		Guid entryNumPK;
		Guid baeEventPK;
		Guid entryLinePK;
		Guid entryLineFeePK;

		protected override IEnumerable<(SqlDbType ParameterType, string ParameterName)> GetSqlParameters()
		{
			yield return (SqlDbType.UniqueIdentifier, Report_FRVATParameter.CompanyPK);
			yield return (SqlDbType.UniqueIdentifier, Report_FRVATParameter.ImporterPK);
			yield return (SqlDbType.UniqueIdentifier, Report_FRVATParameter.SupplierPK);
			yield return (SqlDbType.VarChar, Report_FRVATParameter.VATProcedureCode);
			yield return (SqlDbType.SmallDateTime, Report_FRVATParameter.ValidationDateFrom);
			yield return (SqlDbType.SmallDateTime, Report_FRVATParameter.ValidationDateTo);
		}

		protected override bool RequiresSchemaBinding => false;

		class Report_FRVATParameter
		{
			public const string CompanyPK = "@CompanyPK";
			public const string ImporterPK = "@ImporterPK";
			public const string SupplierPK = "@SupplierPK";
			public const string VATProcedureCode = "@VATProcedureCode";
			public const string ValidationDateFrom = "@ValidationDateFrom";
			public const string ValidationDateTo = "@ValidationDateTo";
		}
	}
}
