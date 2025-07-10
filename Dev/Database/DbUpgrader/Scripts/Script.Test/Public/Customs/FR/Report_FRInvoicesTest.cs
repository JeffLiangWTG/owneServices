using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.DbUpgrader.Scripts.Definitions.Customs.FR;
using Enterprise.Build.Database.Script.TestFramework;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Customs.FR.Testing
{
	[TestedType(typeof(Report_FRInvoices))]
	class Report_FRInvoicesTest : CustomsReportDbCreateScriptTest
	{
		protected override bool RequiresSchemaBinding => false;

		#region Columns

		public void TestInvoicePK()
		{
			AssertFunctionReturnExpectedValue("InvoicePK", invoicePK);
		}

		public void TestDeclarationPK()
		{
			AssertFunctionReturnExpectedValue("DeclarationPK", declarationPK);
		}

		public void TestInvoiceNumber()
		{
			UpdateInvoiceColumn("JZ_InvoiceNumber", "123456", SqlDbType.VarChar, invoicePK);
			AssertFunctionReturnExpectedValue("InvoiceNumber", "123456");
		}

		public void TestInvoiceDate()
		{
			UpdateInvoiceColumn("JZ_InvoiceDate", new DateTime(1995, 03, 25), SqlDbType.SmallDateTime, invoicePK);
			AssertFunctionReturnExpectedValue("InvoiceDate", new DateTime(1995, 03, 25));
		}

		public void TestInvoiceIncoterm()
		{
			UpdateInvoiceColumn("JZ_IncoTerm", "FOB", SqlDbType.VarChar, invoicePK);
			AssertFunctionReturnExpectedValue("IncoTerm", "FOB");
		}

		public void TestInvoiceIncotermPlace()
		{
			UpdateInvoiceColumn("JZ_IncoTermPlace", "1", SqlDbType.VarChar, invoicePK);
			AssertFunctionReturnExpectedValue("IncoTermPlace", "1");
		}

		public void TestDeclarationID()
		{
			AssertFunctionReturnExpectedValue("DeclarationID", "B00001");
		}

		public void TestEntryID()
		{
			UpdateDeclarationColumn("JE_MessageType", "IMP", SqlDbType.VarChar, declarationPK);
			var entryInstructionPK1 = TestDataCreator.CreateCusEntryInstruction(declarationPK, "A", "A", new DateTime(2020, 01, 01), 1);
			UpdateInvoiceLineColumn("JI_CEI", entryInstructionPK1, SqlDbType.UniqueIdentifier, invoiceLinePK);
			var entryHeaderPK1 = TestDataCreator.CreateCusEntryHeader(true, "IMP", "", "", "", 0f, 0, "", declarationPK, new DateTime(2020, 01, 01), new DateTime(2020, 01, 01), entryInstructionPK1, "", new DateTime(2020, 01, 01), new DateTime(2020, 01, 01), new DateTime(2020, 01, 01), 1);
			TestDataCreator.CreateCusEntryNum(entryHeaderPK1, "CusEntryHeader", "1A", "IMP", "", "FR");
			var entryLinePK1 = TestDataCreator.CreateCusEntryLine(entryHeaderPK1, 1);
			UpdateInvoiceLineColumn("JI_CL", entryLinePK1, SqlDbType.UniqueIdentifier, invoiceLinePK);
			AssertFunctionReturnExpectedValues("EntryID", ["1A"]);

			var invoiceLinePK2 = TestDataCreator.CreateJobComInvoiceLine(invoicePK, 1);
			var entryInstructionPK2 = TestDataCreator.CreateCusEntryInstruction(declarationPK, "A", "A", new DateTime(2020, 01, 01), 1);
			UpdateInvoiceLineColumn("JI_CEI", entryInstructionPK2, SqlDbType.UniqueIdentifier, invoiceLinePK2);
			var entryHeaderPK2 = TestDataCreator.CreateCusEntryHeader(true, "IMP", "", "", "", 0f, 0, "", declarationPK, new DateTime(2020, 01, 02), new DateTime(2020, 01, 02), entryInstructionPK2, "", new DateTime(2020, 01, 02), new DateTime(2020, 01, 02), new DateTime(2020, 01, 02), 1);
			TestDataCreator.CreateCusEntryNum(entryHeaderPK2, "CusEntryHeader", "2B", "IMP", "", "FR");
			var entryLinePK2 = TestDataCreator.CreateCusEntryLine(entryHeaderPK2, 1);
			UpdateInvoiceLineColumn("JI_CL", entryLinePK2, SqlDbType.UniqueIdentifier, invoiceLinePK2);

			AssertFunctionReturnExpectedValues("EntryID", ["1A", "2B"]);

			var invoiceLinePK3 = TestDataCreator.CreateJobComInvoiceLine(invoicePK, 1);
			var entryInstructionPK3 = TestDataCreator.CreateCusEntryInstruction(declarationPK, "A", "A", new DateTime(2020, 01, 01), 1);
			UpdateInvoiceLineColumn("JI_CEI", entryInstructionPK3, SqlDbType.UniqueIdentifier, invoiceLinePK3);
			var entryHeaderPK3 = TestDataCreator.CreateCusEntryHeader(true, "IMP", "", "", "", 0f, 0, "", declarationPK, new DateTime(2020, 01, 03), new DateTime(2020, 01, 03), entryInstructionPK3, "", new DateTime(2020, 01, 03), new DateTime(2020, 01, 03), new DateTime(2020, 01, 03), 1);
			TestDataCreator.CreateCusEntryNum(entryHeaderPK3, "CusEntryHeader", "3C", "IMP", "", "FR");
			var entryLinePK3 = TestDataCreator.CreateCusEntryLine(entryHeaderPK3, 1);
			UpdateInvoiceLineColumn("JI_CL", entryLinePK3, SqlDbType.UniqueIdentifier, invoiceLinePK3);
			AssertFunctionReturnExpectedValues("EntryID", ["1A", "2B"]);

			void AssertFunctionReturnExpectedValues(string columnName, string[] expectedValues)
			{
				var rows = GetFilteredRows(columnName);
				AssertEquals($"Expect 1 row", 1, rows.Length);
				var row = rows.Single();
				AssertContainsExactElementsInAnyOrder(columnName, expectedValues, row.ToString().Split('/'));
			}
		}

		public void TestDeclarationType()
		{
			var entryInstructionPK1 = TestDataCreator.CreateCusEntryInstruction(declarationPK, "A", "A", new DateTime(2020, 01, 01), 1);
			UpdateInvoiceLineColumn("JI_CEI", entryInstructionPK1, SqlDbType.UniqueIdentifier, invoiceLinePK);
			var invoiceLinePK2 = TestDataCreator.CreateJobComInvoiceLine(invoicePK, 1);
			var entryInstructionPK2 = TestDataCreator.CreateCusEntryInstruction(declarationPK, "B", "B", new DateTime(2020, 01, 01), 1);
			UpdateInvoiceLineColumn("JI_CEI", entryInstructionPK2, SqlDbType.UniqueIdentifier, invoiceLinePK2);
			AssertFunctionReturnExpectedValue("DeclarationType", "A/B");

			var invoiceLinePK3 = TestDataCreator.CreateJobComInvoiceLine(invoicePK, 1);
			var entryInstructionPK3 = TestDataCreator.CreateCusEntryInstruction(declarationPK, "C", "C", new DateTime(2020, 01, 01), 1);
			UpdateInvoiceLineColumn("JI_CEI", entryInstructionPK3, SqlDbType.UniqueIdentifier, invoiceLinePK3);
			AssertFunctionReturnExpectedValue("DeclarationType", "A/B");
		}

		public void TestEntryStatusDateTime()
		{
			AssertFunctionReturnExpectedValue("EntryStatusDateTime", DBNull.Value);

			UpdateDeclarationColumn("JE_MessageType", "IMP", SqlDbType.VarChar, declarationPK);
			var entryInstructionPK1 = TestDataCreator.CreateCusEntryInstruction(declarationPK, "A", "A", new DateTime(2020, 01, 01), 1);
			UpdateInvoiceLineColumn("JI_CEI", entryInstructionPK1, SqlDbType.UniqueIdentifier, invoiceLinePK);
			var entryHeaderPK1 = TestDataCreator.CreateCusEntryHeader(true, "IMP", "", "100", "", 0f, 0, "", declarationPK, new DateTime(2020, 01, 01), new DateTime(2020, 01, 01), entryInstructionPK1, "", new DateTime(2020, 01, 01), new DateTime(2020, 01, 01), new DateTime(2020, 01, 01), 1);
			TestDataCreator.CreateCusEntryNum(entryHeaderPK1, "CusEntryHeader", "1A", "IMP", "", "FR", new DateTime(2020, 01, 01));
			var entryLinePK1 = TestDataCreator.CreateCusEntryLine(entryHeaderPK1, 1);
			UpdateInvoiceLineColumn("JI_CL", entryLinePK1, SqlDbType.UniqueIdentifier, invoiceLinePK);
			AssertFunctionReturnExpectedValue("EntryStatusDateTime", new DateTime(2020, 01, 01));

			var invoiceLinePK2 = TestDataCreator.CreateJobComInvoiceLine(invoicePK, 1);
			var entryInstructionPK2 = TestDataCreator.CreateCusEntryInstruction(declarationPK, "A", "A", new DateTime(2020, 01, 01), 1);
			UpdateInvoiceLineColumn("JI_CEI", entryInstructionPK2, SqlDbType.UniqueIdentifier, invoiceLinePK2);
			var entryHeaderPK2 = TestDataCreator.CreateCusEntryHeader(true, "IMP", "", "100", "", 0f, 0, "", declarationPK, new DateTime(2020, 01, 02), new DateTime(2020, 01, 02), entryInstructionPK2, "", new DateTime(2020, 01, 02), new DateTime(2020, 01, 02), new DateTime(2020, 01, 02), 1);
			var entryNumber = TestDataCreator.CreateCusEntryNum(entryHeaderPK2, "CusEntryHeader", "2B", "IMP", "", "FR", new DateTime(2020, 01, 01));
			var entryLinePK2 = TestDataCreator.CreateCusEntryLine(entryHeaderPK2, 1);
			UpdateInvoiceLineColumn("JI_CL", entryLinePK2, SqlDbType.UniqueIdentifier, invoiceLinePK2);
			AssertFunctionReturnExpectedValue("EntryStatusDateTime", new DateTime(2020, 01, 01));

			UpdateEntryNumberColumn("CE_IssueDate", new DateTime(2020, 01, 02), SqlDbType.SmallDateTime, entryNumber);
			AssertFunctionReturnExpectedValue("EntryStatusDateTime", DBNull.Value);
		}

		public void TestEntryStatus()
		{
			AssertFunctionReturnExpectedValue("EntryStatus", DBNull.Value);

			UpdateDeclarationColumn("JE_MessageType", "IMP", SqlDbType.VarChar, declarationPK);
			var entryInstructionPK1 = TestDataCreator.CreateCusEntryInstruction(declarationPK, "A", "A", new DateTime(2020, 01, 01), 1);
			UpdateInvoiceLineColumn("JI_CEI", entryInstructionPK1, SqlDbType.UniqueIdentifier, invoiceLinePK);
			var entryHeaderPK1 = TestDataCreator.CreateCusEntryHeader(true, "IMP", "", "100", "", 0f, 0, "", declarationPK, new DateTime(2020, 01, 01), new DateTime(2020, 01, 01), entryInstructionPK1, "", new DateTime(2020, 01, 01), new DateTime(2020, 01, 01), new DateTime(2020, 01, 01), 1);
			TestDataCreator.CreateCusEntryNum(entryHeaderPK1, "CusEntryHeader", "1A", "IMP", "", "FR", new DateTime(2020, 01, 01));
			var entryLinePK1 = TestDataCreator.CreateCusEntryLine(entryHeaderPK1, 1);
			UpdateInvoiceLineColumn("JI_CL", entryLinePK1, SqlDbType.UniqueIdentifier, invoiceLinePK);
			AssertFunctionReturnExpectedValue("EntryStatus", "BAE");

			var invoiceLinePK2 = TestDataCreator.CreateJobComInvoiceLine(invoicePK, 1);
			var entryInstructionPK2 = TestDataCreator.CreateCusEntryInstruction(declarationPK, "A", "A", new DateTime(2020, 01, 01), 1);
			UpdateInvoiceLineColumn("JI_CEI", entryInstructionPK2, SqlDbType.UniqueIdentifier, invoiceLinePK2);
			var entryHeaderPK2 = TestDataCreator.CreateCusEntryHeader(true, "IMP", "", "100", "", 0f, 0, "", declarationPK, new DateTime(2020, 01, 02), new DateTime(2020, 01, 02), entryInstructionPK2, "", new DateTime(2020, 01, 02), new DateTime(2020, 01, 02), new DateTime(2020, 01, 02), 1);
			TestDataCreator.CreateCusEntryNum(entryHeaderPK2, "CusEntryHeader", "2B", "IMP", "", "FR", new DateTime(2020, 01, 02));
			var entryLinePK2 = TestDataCreator.CreateCusEntryLine(entryHeaderPK2, 1);
			UpdateInvoiceLineColumn("JI_CL", entryLinePK2, SqlDbType.UniqueIdentifier, invoiceLinePK2);
			AssertFunctionReturnExpectedValue("EntryStatus", "BAE");

			UpdateEntryHeaderColumn("CH_EntryStatus", "200", SqlDbType.VarChar, entryHeaderPK2);
			AssertFunctionReturnExpectedValue("EntryStatus", "MLP");
		}

		public void TestInvoiceGrossWeight()
		{
			AssertFunctionReturnExpectedValue("InvoiceGrossWeight", 0m);

			UpdateInvoiceColumn("JZ_Weight", 12.123m, SqlDbType.Decimal, invoicePK);
			AssertFunctionReturnExpectedValue("InvoiceGrossWeight", 12.123m);
		}

		public void TestInvoiceGrossWeightUQ()
		{
			UpdateInvoiceColumn("JZ_WeightUQ", "KG", SqlDbType.VarChar, invoicePK);
			AssertFunctionReturnExpectedValue("InvoiceGrossWeightUQ", "KG");
		}

		public void TestTotalInvoiceValue()
		{
			AssertFunctionReturnExpectedValue("TotalInvoiceValue", 0m);

			UpdateInvoiceColumn("JZ_InvoiceAmount", 12.123m, SqlDbType.Decimal, invoicePK);
			AssertFunctionReturnExpectedValue("TotalInvoiceValue", 12.123m);
		}

		public void TestInvoiceCurrency()
		{
			UpdateInvoiceColumn("JZ_RX_NKInvoice_Currency", "EUR", SqlDbType.VarChar, invoicePK);
			AssertFunctionReturnExpectedValue("InvoiceCurrency", "EUR");
		}

		public void TestInvoiceCurrencyExchangeRate()
		{
			AssertFunctionReturnExpectedValue("InvoiceCurrencyExRate", 0m);

			UpdateInvoiceColumn("JZ_InvoiceCurrExRate", 0.99m, SqlDbType.Decimal, invoicePK);
			AssertFunctionReturnExpectedValue("InvoiceCurrencyExRate", 0.99m);
		}

		public void TestNumberOfInvoiceLines()
		{
			AssertFunctionReturnExpectedValue("NumberOfInvoiceLines", 1);

			TestDataCreator.CreateJobComInvoiceLine(invoicePK, 1);
			AssertFunctionReturnExpectedValue("NumberOfInvoiceLines", 2);
		}

		public void TestTotalInvoiceDuty()
		{
			AssertFunctionReturnExpectedValue("TotalInvoiceDuty", 0m);

			TestDataCreator.CreateRefDatabaseRefDataGrouping("FR", "France");
			var dutyRateTypePK = TestDataCreator.CreateRefCusRateType("DTY", "Duty Type", 1, "FR", "");
			TestDataCreator.CreateRefCusRateCode("A00", dutyRateTypePK, "Customs Duty");
			var dutyRateTypeNotPayablePK = TestDataCreator.CreateRefCusRateType("NOP", "Duty Type Not Payable", 0, "FR", "");
			TestDataCreator.CreateRefCusRateCode("A99", dutyRateTypeNotPayablePK, "Customs Duty Not Payable");

			UpdateDeclarationColumn("JE_ApplicationCode", "BLT", SqlDbType.VarChar, declarationPK);
			var entryInstructionPK = TestDataCreator.CreateCusEntryInstruction(declarationPK, "A", "A", new DateTime(2020, 01, 01), 1);
			var entryHeaderPK = TestDataCreator.CreateCusEntryHeader(true, "", "", "", "", 0f, 1, "", declarationPK, new DateTime(2020, 01, 01), new DateTime(2020, 01, 01), entryInstructionPK, "", new DateTime(2020, 01, 01), new DateTime(2020, 01, 01), new DateTime(2020, 01, 01), 1);
			var entryLinePK = TestDataCreator.CreateCusEntryLine(entryHeaderPK, 1);
			UpdateInvoiceLineColumn("JI_CL", entryLinePK, SqlDbType.UniqueIdentifier, invoiceLinePK);
			TestDataCreator.CreateCusEntryLineFee(entryLinePK, "A00", 100.12f, 1, "CUS");
			TestDataCreator.CreateCusEntryLineFee(entryLinePK, "A00", 999f, 1, "CUS");
			TestDataCreator.CreateCusEntryLineFee(entryLinePK, "A99", 500f, 1, "CUS");
			AssertFunctionReturnExpectedValue("TotalInvoiceDuty", 1099.12m);
		}

		public void TestTotalInvoiceVat()
		{
			AssertFunctionReturnExpectedValue("TotalInvoiceVAT", 0m);

			TestDataCreator.CreateRefDatabaseRefDataGrouping("FR", "France");
			var vatRateTypePK = TestDataCreator.CreateRefCusRateType("VAT", "VAT Type", 1, "FR", "");
			TestDataCreator.CreateRefCusRateCode("B00", vatRateTypePK, "VAT");
			var vatRateTypeNotPayablePK = TestDataCreator.CreateRefCusRateType("NOP", "VAT Type Not Payable", 0, "FR", "");
			TestDataCreator.CreateRefCusRateCode("B99", vatRateTypeNotPayablePK, "VAT Not Payable");

			UpdateDeclarationColumn("JE_ApplicationCode", "BLT", SqlDbType.VarChar, declarationPK);
			var entryInstructionPK = TestDataCreator.CreateCusEntryInstruction(declarationPK, "A", "A", new DateTime(2020, 01, 01), 1);
			var entryHeaderPK = TestDataCreator.CreateCusEntryHeader(true, "", "", "", "", 0f, 1, "", declarationPK, new DateTime(2020, 01, 01), new DateTime(2020, 01, 01), entryInstructionPK, "", new DateTime(2020, 01, 01), new DateTime(2020, 01, 01), new DateTime(2020, 01, 01), 1);
			var entryLinePK = TestDataCreator.CreateCusEntryLine(entryHeaderPK, 1);
			UpdateInvoiceLineColumn("JI_CL", entryLinePK, SqlDbType.UniqueIdentifier, invoiceLinePK);
			UpdateInvoiceLineColumn("JI_ZZF_NKTaxType", "B00", SqlDbType.VarChar, invoiceLinePK);

			TestDataCreator.CreateCusEntryLineFee(entryLinePK, "B00", 100.12f, 1, "CUS");
			TestDataCreator.CreateCusEntryLineFee(entryLinePK, "B00", 999f, 1, "CUS");
			TestDataCreator.CreateCusEntryLineFee(entryLinePK, "B99", 500f, 1, "CUS");
			AssertFunctionReturnExpectedValue("TotalInvoiceVAT", 1099.12m);
		}

		public void TestSupplierPkAndCodeAndName()
		{
			var invSupplierPK = TestDataCreator.CreateOrganisation("INVSUPCODE", "INVSUPNAME");
			UpdateInvoiceColumn("JZ_OH_Supplier", invSupplierPK, SqlDbType.UniqueIdentifier, invoicePK);

			var decSupplierPK = TestDataCreator.CreateOrganisation("DECSUPCODE", "DECSUPNAME");
			UpdateDeclarationColumn("JE_OH_Supplier", decSupplierPK, SqlDbType.UniqueIdentifier, declarationPK);

			AssertFunctionReturnExpectedValue("SupplierPK", invSupplierPK);
			AssertFunctionReturnExpectedValue("SupplierCode", "INVSUPCODE");
			AssertFunctionReturnExpectedValue("SupplierName", "INVSUPNAME");

			UpdateInvoiceColumn("JZ_OH_Supplier", DBNull.Value, SqlDbType.UniqueIdentifier, invoicePK);
			AssertFunctionReturnExpectedValue("SupplierPK", decSupplierPK);
			AssertFunctionReturnExpectedValue("SupplierCode", "DECSUPCODE");
			AssertFunctionReturnExpectedValue("SupplierName", "DECSUPNAME");
		}

		public void TestInvoiceSupplierAddress()
		{
			var invSupplierPK = TestDataCreator.CreateOrganisation("INVSUPCODE", "INVSUPNAME");
			UpdateInvoiceColumn("JZ_OH_Supplier", invSupplierPK, SqlDbType.UniqueIdentifier, invoicePK);

			var addressPK1 = TestDataCreator.CreateAddress(invSupplierPK, "ADD1", "There");
			UpdateInvoiceColumn("JZ_OA_SupplierAddress", addressPK1, SqlDbType.UniqueIdentifier, invoicePK);

			AssertFunctionReturnExpectedValue("SupplierAddress", addressPK1);

			UpdateInvoiceColumn("JZ_OA_SupplierAddress", DBNull.Value, SqlDbType.UniqueIdentifier, invoicePK);
			AssertFunctionReturnExpectedValue("SupplierAddress", DBNull.Value);
		}

		public void TestImporterCodeAndName()
		{
			var importerPK = TestDataCreator.CreateOrganisation("IMPCODE", "IMPNAME");
			UpdateDeclarationColumn("JE_OH_Importer", importerPK, SqlDbType.UniqueIdentifier, declarationPK);
			AssertFunctionReturnExpectedValue("ImporterCode", "IMPCODE");
			AssertFunctionReturnExpectedValue("ImporterName", "IMPNAME");
		}

		public void TestTransportMode()
		{
			UpdateDeclarationColumn("JE_TransportMode", "ROA", SqlDbType.VarChar, declarationPK);
			AssertFunctionReturnExpectedValue("TransportMode", "ROA");
		}

		public void TestDateOfDischarge()
		{
			UpdateDeclarationColumn("JE_DateOfArrival", new DateTime(2020, 01, 01), SqlDbType.SmallDateTime, declarationPK);
			AssertFunctionReturnExpectedValue("DateOfDischarge", new DateTime(2020, 01, 01));
		}

		public void TestPortOfDischarge()
		{
			UpdateDeclarationColumn("JE_RL_NKPortOfArrival", "ITVCE", SqlDbType.VarChar, declarationPK);
			AssertFunctionReturnExpectedValue("PortOfDischarge", "ITVCE");
		}

		public void TestDeltaMode()
		{
			TestDataCreator.CreateGenAddOnColumn(declarationPK, "JE", "JE_DeltaMode", "STR", "G1");
			AssertFunctionReturnExpectedValue("DeltaMode", "G1");
		}

		public void TestCustomsProfile()
		{
			UpdateDeclarationColumn("JE_CustomsProfile", "RFI", SqlDbType.VarChar, declarationPK);
			AssertFunctionReturnExpectedValue("CustomsProfile", "RFI");
		}

		public void TestBranchPK()
		{
			AssertFunctionReturnExpectedValue("BranchPK", branchPK);
		}

		#endregion

		#region Filters

		public void TestFilterByCompany()
		{
			UpdateInvoiceColumn("JZ_InvoiceNumber", "111", SqlDbType.VarChar, invoicePK);

			var companyPK2 = TestDataCreator.CreateCompany(companyCode: "TC2", countryCode: "FR", currencyCode: "EUR");
			var branchPK2 = TestDataCreator.CreateBranch(companyPK2, branchCode: "TB2", homePort: "XXYYZZ");
			var declarationPK2 = TestDataCreator.CreateJobDeclaration(branchPK2, companyPK2, "B00002", "IMP", 2);
			var invoicePK2 = TestDataCreator.CreateJobComInvoiceHeader(declarationPK2, false, 2);
			UpdateInvoiceColumn("JZ_InvoiceNumber", "222", SqlDbType.VarChar, invoicePK2);

			var filteredRows = GetFilteredRows("InvoiceNumber");
			AssertContainsExactElementsInAnyOrder(new string[] { "111", "222" }, filteredRows);

			filteredRows = GetFilteredRows("InvoiceNumber", (Report_FRInvoicesParameter.CompanyPK, companyPK));
			AssertContainsExactElementsInAnyOrder(new string[] { "111" }, filteredRows);

			filteredRows = GetFilteredRows("InvoiceNumber", (Report_FRInvoicesParameter.CompanyPK, companyPK2));
			AssertContainsExactElementsInAnyOrder(new string[] { "222" }, filteredRows);
		}

		public void TestFilterByDeclarationBranch()
		{
			UpdateInvoiceColumn("JZ_InvoiceNumber", "111", SqlDbType.VarChar, invoicePK);

			var companyPK2 = TestDataCreator.CreateCompany(companyCode: "TC2", countryCode: "FR", currencyCode: "EUR");
			var branchPK2 = TestDataCreator.CreateBranch(companyPK2, branchCode: "TB2", homePort: "XXYYZZ");
			var declarationPK2 = TestDataCreator.CreateJobDeclaration(branchPK2, companyPK2, "B00002", "IMP", 2);
			var invoicePK2 = TestDataCreator.CreateJobComInvoiceHeader(declarationPK2, false, 2);
			UpdateInvoiceColumn("JZ_InvoiceNumber", "222", SqlDbType.VarChar, invoicePK2);

			var filteredRows = GetFilteredRows("InvoiceNumber");
			AssertContainsExactElementsInAnyOrder(new string[] { "111", "222" }, filteredRows);

			filteredRows = GetFilteredRows("InvoiceNumber", (Report_FRInvoicesParameter.BranchPK, branchPK));
			AssertContainsExactElementsInAnyOrder(new string[] { "111" }, filteredRows);

			filteredRows = GetFilteredRows("InvoiceNumber", (Report_FRInvoicesParameter.BranchPK, branchPK2));
			AssertContainsExactElementsInAnyOrder(new string[] { "222" }, filteredRows);
		}

		public void TestFilterByImporter()
		{
			UpdateInvoiceColumn("JZ_InvoiceNumber", "111", SqlDbType.VarChar, invoicePK);
			var importerPK = TestDataCreator.CreateOrganisation("IMPCODE1", "IMPNAME1");
			UpdateDeclarationColumn("JE_OH_Importer", importerPK, SqlDbType.UniqueIdentifier, declarationPK);

			var importerPK2 = TestDataCreator.CreateOrganisation("IMPCODE2", "IMPNAME2");
			var declarationPK2 = TestDataCreator.CreateJobDeclaration(branchPK, companyPK, "B00002", "IMP", 2);
			UpdateDeclarationColumn("JE_OH_Importer", importerPK2, SqlDbType.UniqueIdentifier, declarationPK2);
			var invoicePK2 = TestDataCreator.CreateJobComInvoiceHeader(declarationPK2, false, 2);
			UpdateInvoiceColumn("JZ_InvoiceNumber", "222", SqlDbType.VarChar, invoicePK2);

			var filteredRows = GetFilteredRows("InvoiceNumber");
			AssertContainsExactElementsInAnyOrder(new string[] { "111", "222" }, filteredRows);

			filteredRows = GetFilteredRows("InvoiceNumber", (Report_FRInvoicesParameter.ImporterPK, importerPK));
			AssertContainsExactElementsInAnyOrder(new string[] { "111" }, filteredRows);

			filteredRows = GetFilteredRows("InvoiceNumber", (Report_FRInvoicesParameter.ImporterPK, importerPK2));
			AssertContainsExactElementsInAnyOrder(new string[] { "222" }, filteredRows);
		}

		public void TestFilterBySupplier()
		{
			UpdateInvoiceColumn("JZ_InvoiceNumber", "111", SqlDbType.VarChar, invoicePK);
			var invoiceSupplierPK = TestDataCreator.CreateOrganisation("INVSUPCODE", "INVSUPNAME");
			UpdateInvoiceColumn("JZ_OH_Supplier", invoiceSupplierPK, SqlDbType.UniqueIdentifier, invoicePK);

			var declarationSupplierPK = TestDataCreator.CreateOrganisation("DECSUPCODE", "DECSUPNAME");
			var declarationPK2 = TestDataCreator.CreateJobDeclaration(branchPK, companyPK, "B00002", "IMP", 2);
			UpdateDeclarationColumn("JE_OH_Supplier", declarationSupplierPK, SqlDbType.UniqueIdentifier, declarationPK2);
			var invoicePK2 = TestDataCreator.CreateJobComInvoiceHeader(declarationPK2, false, 2);
			UpdateInvoiceColumn("JZ_InvoiceNumber", "222", SqlDbType.VarChar, invoicePK2);

			var filteredRows = GetFilteredRows("InvoiceNumber");
			AssertContainsExactElementsInAnyOrder(new string[] { "111", "222" }, filteredRows);

			filteredRows = GetFilteredRows("InvoiceNumber", (Report_FRInvoicesParameter.SupplierPK, invoiceSupplierPK));
			AssertContainsExactElementsInAnyOrder(new string[] { "111" }, filteredRows);

			filteredRows = GetFilteredRows("InvoiceNumber", (Report_FRInvoicesParameter.SupplierPK, declarationSupplierPK));
			AssertContainsExactElementsInAnyOrder(new string[] { "222" }, filteredRows);
		}

		public void TestFilterByInvoiceNumber()
		{
			UpdateInvoiceColumn("JZ_InvoiceNumber", "111", SqlDbType.VarChar, invoicePK);

			var invoicePK2 = TestDataCreator.CreateJobComInvoiceHeader(declarationPK, false, 1);
			UpdateInvoiceColumn("JZ_InvoiceNumber", "222", SqlDbType.VarChar, invoicePK2);

			var filteredRows = GetFilteredRows("InvoiceNumber");
			AssertContainsExactElementsInAnyOrder(new string[] { "111", "222" }, filteredRows);

			filteredRows = GetFilteredRows("InvoiceNumber", (Report_FRInvoicesParameter.InvoiceNumber, "111"));
			AssertContainsExactElementsInAnyOrder(new string[] { "111" }, filteredRows);

			filteredRows = GetFilteredRows("InvoiceNumber", (Report_FRInvoicesParameter.InvoiceNumber, "222"));
			AssertContainsExactElementsInAnyOrder(new string[] { "222" }, filteredRows);
		}

		public void TestFilterByJobNumber()
		{
			UpdateInvoiceColumn("JZ_InvoiceNumber", "111", SqlDbType.VarChar, invoicePK);
			UpdateDeclarationColumn("JE_DeclarationReference", "B00001", SqlDbType.VarChar, declarationPK);

			var declarationPK2 = TestDataCreator.CreateJobDeclaration(branchPK, companyPK, "B00002", "IMP", 2);
			var invoicePK2 = TestDataCreator.CreateJobComInvoiceHeader(declarationPK2, false, 2);
			UpdateInvoiceColumn("JZ_InvoiceNumber", "222", SqlDbType.VarChar, invoicePK2);

			var filteredRows = GetFilteredRows("InvoiceNumber");
			AssertContainsExactElementsInAnyOrder(new string[] { "111", "222" }, filteredRows);

			filteredRows = GetFilteredRows("InvoiceNumber", (Report_FRInvoicesParameter.JobNumber, "B00001"));
			AssertContainsExactElementsInAnyOrder(new string[] { "111" }, filteredRows);

			filteredRows = GetFilteredRows("InvoiceNumber", (Report_FRInvoicesParameter.JobNumber, "B00002"));
			AssertContainsExactElementsInAnyOrder(new string[] { "222" }, filteredRows);
		}

		public void TestFilterByTransportMode()
		{
			UpdateInvoiceColumn("JZ_InvoiceNumber", "111", SqlDbType.VarChar, invoicePK);
			UpdateDeclarationColumn("JE_TransportMode", "ROA", SqlDbType.VarChar, declarationPK);

			var declarationPK2 = TestDataCreator.CreateJobDeclaration(branchPK, companyPK, "B00002", "IMP", 2);
			UpdateDeclarationColumn("JE_TransportMode", "SEA", SqlDbType.VarChar, declarationPK2);
			var invoicePK2 = TestDataCreator.CreateJobComInvoiceHeader(declarationPK2, false, 2);
			UpdateInvoiceColumn("JZ_InvoiceNumber", "222", SqlDbType.VarChar, invoicePK2);

			var filteredRows = GetFilteredRows("InvoiceNumber");
			AssertContainsExactElementsInAnyOrder(new string[] { "111", "222" }, filteredRows);

			filteredRows = GetFilteredRows("InvoiceNumber", (Report_FRInvoicesParameter.TransportMode, "ROA"));
			AssertContainsExactElementsInAnyOrder(new string[] { "111" }, filteredRows);

			filteredRows = GetFilteredRows("InvoiceNumber", (Report_FRInvoicesParameter.TransportMode, "SEA"));
			AssertContainsExactElementsInAnyOrder(new string[] { "222" }, filteredRows);
		}

		public void TestFilterByMessageType()
		{
			UpdateInvoiceColumn("JZ_InvoiceNumber", "111", SqlDbType.VarChar, invoicePK);

			var declarationPK2 = TestDataCreator.CreateJobDeclaration(branchPK, companyPK, "B00002", "EXP", 2);
			var invoicePK2 = TestDataCreator.CreateJobComInvoiceHeader(declarationPK2, false, 2);
			UpdateInvoiceColumn("JZ_InvoiceNumber", "222", SqlDbType.VarChar, invoicePK2);

			var filteredRows = GetFilteredRows("InvoiceNumber");
			AssertContainsExactElementsInAnyOrder(new string[] { "111", "222" }, filteredRows);

			filteredRows = GetFilteredRows("InvoiceNumber", (Report_FRInvoicesParameter.MessageType, "IMP"));
			AssertContainsExactElementsInAnyOrder(new string[] { "111" }, filteredRows);

			filteredRows = GetFilteredRows("InvoiceNumber", (Report_FRInvoicesParameter.MessageType, "EXP"));
			AssertContainsExactElementsInAnyOrder(new string[] { "222" }, filteredRows);
		}

		public void TestFilterByJobCreateTime()
		{
			UpdateInvoiceColumn("JZ_InvoiceNumber", "111", SqlDbType.VarChar, invoicePK);
			UpdateDeclarationColumn("JE_SystemCreateTimeUtc", new DateTime(2020, 01, 01), SqlDbType.SmallDateTime, declarationPK);

			var declarationPK2 = TestDataCreator.CreateJobDeclaration(branchPK, companyPK, "B00002", "EXP", 2);
			UpdateDeclarationColumn("JE_SystemCreateTimeUtc", new DateTime(2020, 01, 10), SqlDbType.SmallDateTime, declarationPK2);
			var invoicePK2 = TestDataCreator.CreateJobComInvoiceHeader(declarationPK2, false, 2);
			UpdateInvoiceColumn("JZ_InvoiceNumber", "222", SqlDbType.VarChar, invoicePK2);

			var filteredRows = GetFilteredRows("InvoiceNumber");
			AssertContainsExactElementsInAnyOrder("Empty filters", new string[] { "111", "222" }, filteredRows);

			filteredRows = GetFilteredRows("InvoiceNumber", (Report_FRInvoicesParameter.CreateDateFrom, new DateTime(2020, 01, 01)));
			AssertContainsExactElementsInAnyOrder("From filter", new string[] { "111", "222" }, filteredRows);

			filteredRows = GetFilteredRows("InvoiceNumber", (Report_FRInvoicesParameter.CreateDateFrom, new DateTime(2020, 01, 01)), (Report_FRInvoicesParameter.CreateDateTo, new DateTime(2020, 01, 05)));
			AssertContainsExactElementsInAnyOrder(new string[] { "111" }, filteredRows);

			filteredRows = GetFilteredRows("InvoiceNumber", (Report_FRInvoicesParameter.CreateDateFrom, new DateTime(2020, 01, 05)), (Report_FRInvoicesParameter.CreateDateTo, new DateTime(2020, 01, 06)));
			AssertContainsExactElementsInAnyOrder(Array.Empty<string>(), filteredRows);

			filteredRows = GetFilteredRows("InvoiceNumber", (Report_FRInvoicesParameter.CreateDateFrom, new DateTime(2020, 01, 05)));
			AssertContainsExactElementsInAnyOrder(new string[] { "222" }, filteredRows);
		}

		public void TestFilterByJobArrivalDate()
		{
			UpdateInvoiceColumn("JZ_InvoiceNumber", "111", SqlDbType.VarChar, invoicePK);
			UpdateDeclarationColumn("JE_DateOfArrival", new DateTime(2020, 01, 01), SqlDbType.SmallDateTime, declarationPK);

			var declarationPK2 = TestDataCreator.CreateJobDeclaration(branchPK, companyPK, "B00002", "EXP", 2);
			UpdateDeclarationColumn("JE_DateOfArrival", new DateTime(2020, 01, 10), SqlDbType.SmallDateTime, declarationPK2);
			var invoicePK2 = TestDataCreator.CreateJobComInvoiceHeader(declarationPK2, false, 2);
			UpdateInvoiceColumn("JZ_InvoiceNumber", "222", SqlDbType.VarChar, invoicePK2);

			var filteredRows = GetFilteredRows("InvoiceNumber");
			AssertContainsExactElementsInAnyOrder("Empty filters", new string[] { "111", "222" }, filteredRows);

			filteredRows = GetFilteredRows("InvoiceNumber", (Report_FRInvoicesParameter.ArrivalDateFrom, new DateTime(2020, 01, 01)));
			AssertContainsExactElementsInAnyOrder("From filter", new string[] { "111", "222" }, filteredRows);

			filteredRows = GetFilteredRows("InvoiceNumber", (Report_FRInvoicesParameter.ArrivalDateFrom, new DateTime(2020, 01, 01)), (Report_FRInvoicesParameter.ArrivalDateTo, new DateTime(2020, 01, 05)));
			AssertContainsExactElementsInAnyOrder(new string[] { "111" }, filteredRows);

			filteredRows = GetFilteredRows("InvoiceNumber", (Report_FRInvoicesParameter.ArrivalDateFrom, new DateTime(2020, 01, 05)), (Report_FRInvoicesParameter.ArrivalDateTo, new DateTime(2020, 01, 06)));
			AssertContainsExactElementsInAnyOrder(Array.Empty<string>(), filteredRows);

			filteredRows = GetFilteredRows("InvoiceNumber", (Report_FRInvoicesParameter.ArrivalDateFrom, new DateTime(2020, 01, 05)));
			AssertContainsExactElementsInAnyOrder(new string[] { "222" }, filteredRows);
		}

		public void TestFilterByPortOfDischarge()
		{
			UpdateInvoiceColumn("JZ_InvoiceNumber", "111", SqlDbType.VarChar, invoicePK);
			UpdateDeclarationColumn("JE_RL_NKPortOfArrival", "XX", SqlDbType.VarChar, declarationPK);

			var declarationPK2 = TestDataCreator.CreateJobDeclaration(branchPK, companyPK, "B00002", "EXP", 2);
			UpdateDeclarationColumn("JE_RL_NKPortOfArrival", "YY", SqlDbType.VarChar, declarationPK2);
			var invoicePK2 = TestDataCreator.CreateJobComInvoiceHeader(declarationPK2, false, 2);
			UpdateInvoiceColumn("JZ_InvoiceNumber", "222", SqlDbType.VarChar, invoicePK2);

			var filteredRows = GetFilteredRows("InvoiceNumber");
			AssertContainsExactElementsInAnyOrder(new string[] { "111", "222" }, filteredRows);

			filteredRows = GetFilteredRows("InvoiceNumber", (Report_FRInvoicesParameter.PortOfDischarge, "XX"));
			AssertContainsExactElementsInAnyOrder(new string[] { "111" }, filteredRows);

			filteredRows = GetFilteredRows("InvoiceNumber", (Report_FRInvoicesParameter.PortOfDischarge, "YY"));
			AssertContainsExactElementsInAnyOrder(new string[] { "222" }, filteredRows);
		}

		public void TestFilterByCustomsOffice()
		{
			UpdateInvoiceColumn("JZ_InvoiceNumber", "111", SqlDbType.VarChar, invoicePK);
			UpdateDeclarationColumn("JE_CustomsOffice", "XX", SqlDbType.VarChar, declarationPK);

			var declarationPK2 = TestDataCreator.CreateJobDeclaration(branchPK, companyPK, "B00002", "IMP", 2);
			var invoicePK2 = TestDataCreator.CreateJobComInvoiceHeader(declarationPK2, false, 2);
			UpdateInvoiceColumn("JZ_InvoiceNumber", "222", SqlDbType.VarChar, invoicePK2);
			UpdateDeclarationColumn("JE_CustomsOffice", "YY", SqlDbType.VarChar, declarationPK2);

			var filteredRows = GetFilteredRows("InvoiceNumber");
			AssertContainsExactElementsInAnyOrder(new string[] { "111", "222" }, filteredRows);

			filteredRows = GetFilteredRows("InvoiceNumber", (Report_FRInvoicesParameter.CustomsOffice, "XX"));
			AssertContainsExactElementsInAnyOrder(new string[] { "111" }, filteredRows);

			filteredRows = GetFilteredRows("InvoiceNumber", (Report_FRInvoicesParameter.CustomsOffice, "YY"));
			AssertContainsExactElementsInAnyOrder(new string[] { "222" }, filteredRows);
		}

		public void TestFilterByDeltaReference()
		{
			UpdateInvoiceColumn("JZ_InvoiceNumber", "111", SqlDbType.VarChar, invoicePK);
			UpdateDeclarationColumn("JE_MessageType", "IMP", SqlDbType.VarChar, declarationPK);
			var entryInstructionPK1 = TestDataCreator.CreateCusEntryInstruction(declarationPK, "A", "A", new DateTime(2020, 01, 01), 1);
			UpdateInvoiceLineColumn("JI_CEI", entryInstructionPK1, SqlDbType.UniqueIdentifier, invoiceLinePK);
			var entryHeaderPK1 = TestDataCreator.CreateCusEntryHeader(true, "IMP", "", "", "", 0f, 0, "", declarationPK, new DateTime(2020, 01, 01), new DateTime(2020, 01, 01), entryInstructionPK1, "", new DateTime(2020, 01, 01), new DateTime(2020, 01, 01), new DateTime(2020, 01, 01), 1);
			TestDataCreator.CreateCusEntryNum(entryHeaderPK1, "CusEntryHeader", "1A", "IMP", "", "FR");
			var entryLinePK1 = TestDataCreator.CreateCusEntryLine(entryHeaderPK1, 1);
			UpdateInvoiceLineColumn("JI_CL", entryLinePK1, SqlDbType.UniqueIdentifier, invoiceLinePK);

			var invoiceLinePK2 = TestDataCreator.CreateJobComInvoiceLine(invoicePK, 1);
			var entryInstructionPK2 = TestDataCreator.CreateCusEntryInstruction(declarationPK, "A", "A", new DateTime(2020, 01, 01), 1);
			UpdateInvoiceLineColumn("JI_CEI", entryInstructionPK2, SqlDbType.UniqueIdentifier, invoiceLinePK2);
			var entryHeaderPK2 = TestDataCreator.CreateCusEntryHeader(true, "IMP", "", "", "", 0f, 0, "", declarationPK, new DateTime(2020, 01, 02), new DateTime(2020, 01, 02), entryInstructionPK2, "", new DateTime(2020, 01, 02), new DateTime(2020, 01, 02), new DateTime(2020, 01, 02), 1);
			TestDataCreator.CreateCusEntryNum(entryHeaderPK2, "CusEntryHeader", "2B", "IMP", "", "FR");
			var entryLinePK2 = TestDataCreator.CreateCusEntryLine(entryHeaderPK2, 1);
			UpdateInvoiceLineColumn("JI_CL", entryLinePK2, SqlDbType.UniqueIdentifier, invoiceLinePK2);

			var invoiceLinePK3 = TestDataCreator.CreateJobComInvoiceLine(invoicePK, 1);
			var entryInstructionPK3 = TestDataCreator.CreateCusEntryInstruction(declarationPK, "A", "A", new DateTime(2020, 01, 01), 1);
			UpdateInvoiceLineColumn("JI_CEI", entryInstructionPK3, SqlDbType.UniqueIdentifier, invoiceLinePK3);
			var entryHeaderPK3 = TestDataCreator.CreateCusEntryHeader(true, "IMP", "", "", "", 0f, 0, "", declarationPK, new DateTime(2020, 01, 03), new DateTime(2020, 01, 03), entryInstructionPK3, "", new DateTime(2020, 01, 03), new DateTime(2020, 01, 03), new DateTime(2020, 01, 03), 1);
			TestDataCreator.CreateCusEntryNum(entryHeaderPK3, "CusEntryHeader", "3C", "IMP", "", "FR");
			var entryLinePK3 = TestDataCreator.CreateCusEntryLine(entryHeaderPK3, 1);
			UpdateInvoiceLineColumn("JI_CL", entryLinePK3, SqlDbType.UniqueIdentifier, invoiceLinePK3);

			var invoicePK2 = TestDataCreator.CreateJobComInvoiceHeader(declarationPK, false, 1);
			UpdateInvoiceColumn("JZ_InvoiceNumber", "222", SqlDbType.VarChar, invoicePK2);
			var invoiceLinePK4 = TestDataCreator.CreateJobComInvoiceLine(invoicePK2, 1);
			var entryInstructionPK4 = TestDataCreator.CreateCusEntryInstruction(declarationPK, "A", "A", new DateTime(2020, 01, 01), 1);
			UpdateInvoiceLineColumn("JI_CEI", entryInstructionPK4, SqlDbType.UniqueIdentifier, invoiceLinePK4);
			var entryHeaderPK4 = TestDataCreator.CreateCusEntryHeader(true, "IMP", "", "", "", 0f, 0, "", declarationPK, new DateTime(2020, 01, 04), new DateTime(2020, 01, 04), entryInstructionPK4, "", new DateTime(2020, 01, 04), new DateTime(2020, 01, 04), new DateTime(2020, 01, 04), 1);
			TestDataCreator.CreateCusEntryNum(entryHeaderPK4, "CusEntryHeader", "4D", "IMP", "", "FR");
			var entryLinePK4 = TestDataCreator.CreateCusEntryLine(entryHeaderPK4, 1);
			UpdateInvoiceLineColumn("JI_CL", entryLinePK4, SqlDbType.UniqueIdentifier, invoiceLinePK4);

			var filteredRows = GetFilteredRows("InvoiceNumber");
			AssertContainsExactElementsInAnyOrder(new string[] { "111", "222" }, filteredRows);

			filteredRows = GetFilteredRows("InvoiceNumber", (Report_FRInvoicesParameter.DeltaReference, "1A"));
			AssertContainsExactElementsInAnyOrder(new string[] { "111" }, filteredRows);

			filteredRows = GetFilteredRows("InvoiceNumber", (Report_FRInvoicesParameter.DeltaReference, "2B"));
			AssertContainsExactElementsInAnyOrder(new string[] { "111" }, filteredRows);

			filteredRows = GetFilteredRows("InvoiceNumber", (Report_FRInvoicesParameter.DeltaReference, "3C"));
			AssertContainsExactElementsInAnyOrder(new string[] { "111" }, filteredRows);

			filteredRows = GetFilteredRows("InvoiceNumber", (Report_FRInvoicesParameter.DeltaReference, "4D"));
			AssertContainsExactElementsInAnyOrder(new string[] { "222" }, filteredRows);
		}

		public void TestFilterByEntryStatus()
		{
			UpdateInvoiceColumn("JZ_InvoiceNumber", "111", SqlDbType.VarChar, invoicePK);
			var entryInstructionPK1 = TestDataCreator.CreateCusEntryInstruction(declarationPK, "A", "A", new DateTime(2020, 01, 01), 1);
			var entryHeaderPK1 = TestDataCreator.CreateCusEntryHeader(true, "IMP", "", "100", "", 0f, 0, "", declarationPK, new DateTime(2020, 01, 01), new DateTime(2020, 01, 01), entryInstructionPK1, "", new DateTime(2020, 01, 01), new DateTime(2020, 01, 01), new DateTime(2020, 01, 01), 1);
			TestDataCreator.CreateCusEntryNum(entryHeaderPK1, "CusEntryHeader", "1A", "IMP", "", "FR", new DateTime(2020, 01, 01));
			var entryLinePK1 = TestDataCreator.CreateCusEntryLine(entryHeaderPK1, 1);
			UpdateInvoiceLineColumn("JI_CL", entryLinePK1, SqlDbType.UniqueIdentifier, invoiceLinePK);

			var invoiceLinePK2 = TestDataCreator.CreateJobComInvoiceLine(invoicePK, 1);
			var entryInstructionPK2 = TestDataCreator.CreateCusEntryInstruction(declarationPK, "A", "A", new DateTime(2020, 01, 01), 1);
			var entryHeaderPK2 = TestDataCreator.CreateCusEntryHeader(true, "IMP", "", "200", "", 0f, 0, "", declarationPK, new DateTime(2020, 01, 01), new DateTime(2020, 01, 01), entryInstructionPK2, "", new DateTime(2020, 01, 01), new DateTime(2020, 01, 01), new DateTime(2020, 01, 01), 1);
			TestDataCreator.CreateCusEntryNum(entryHeaderPK2, "CusEntryHeader", "1A", "IMP", "", "FR", new DateTime(2020, 01, 01));
			var entryLinePK2 = TestDataCreator.CreateCusEntryLine(entryHeaderPK2, 1);
			UpdateInvoiceLineColumn("JI_CL", entryLinePK2, SqlDbType.UniqueIdentifier, invoiceLinePK2);

			var invoicePK2 = TestDataCreator.CreateJobComInvoiceHeader(declarationPK, false, 1);
			UpdateInvoiceColumn("JZ_InvoiceNumber", "222", SqlDbType.VarChar, invoicePK2);
			var invoiceLinePK3 = TestDataCreator.CreateJobComInvoiceLine(invoicePK2, 1);
			var entryInstructionPK3 = TestDataCreator.CreateCusEntryInstruction(declarationPK, "A", "A", new DateTime(2020, 01, 01), 1);
			var entryHeaderPK3 = TestDataCreator.CreateCusEntryHeader(true, "IMP", "", "200", "", 0f, 0, "", declarationPK, new DateTime(2020, 01, 01), new DateTime(2020, 01, 01), entryInstructionPK3, "", new DateTime(2020, 01, 01), new DateTime(2020, 01, 01), new DateTime(2020, 01, 01), 1);
			TestDataCreator.CreateCusEntryNum(entryHeaderPK3, "CusEntryHeader", "1A", "IMP", "", "FR", new DateTime(2020, 01, 01));
			var entryLinePK3 = TestDataCreator.CreateCusEntryLine(entryHeaderPK3, 1);
			UpdateInvoiceLineColumn("JI_CL", entryLinePK3, SqlDbType.UniqueIdentifier, invoiceLinePK3);

			var filteredRows = GetFilteredRows("InvoiceNumber");
			AssertContainsExactElementsInAnyOrder(new string[] { "111", "222" }, filteredRows);

			filteredRows = GetFilteredRows("InvoiceNumber", (Report_FRInvoicesParameter.EntryStatus, "100"));
			AssertContainsExactElementsInAnyOrder(new string[] { "111" }, filteredRows);

			filteredRows = GetFilteredRows("InvoiceNumber", (Report_FRInvoicesParameter.EntryStatus, "200"));
			AssertContainsExactElementsInAnyOrder(new string[] { "111", "222" }, filteredRows);
		}

		#endregion

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();

			companyPK = TestDataCreator.CreateCompany(companyCode: "TC1", countryCode: "FR", currencyCode: "EUR");
			branchPK = TestDataCreator.CreateBranch(companyPK, branchCode: "TB1", homePort: "FRANCE");
			declarationPK = TestDataCreator.CreateJobDeclaration(branchPK, companyPK, "B00001", "IMP", 1);
			invoicePK = TestDataCreator.CreateJobComInvoiceHeader(declarationPK, false, 1);
			invoiceLinePK = TestDataCreator.CreateJobComInvoiceLine(invoicePK, 1);
		}
		Guid companyPK;
		Guid branchPK;
		Guid declarationPK;
		Guid invoicePK;
		Guid invoiceLinePK;

		protected override IEnumerable<(SqlDbType ParameterType, string ParameterName)> GetSqlParameters()
		{
			yield return (SqlDbType.UniqueIdentifier, Report_FRInvoicesParameter.CompanyPK);
			yield return (SqlDbType.UniqueIdentifier, Report_FRInvoicesParameter.BranchPK);
			yield return (SqlDbType.UniqueIdentifier, Report_FRInvoicesParameter.ImporterPK);
			yield return (SqlDbType.UniqueIdentifier, Report_FRInvoicesParameter.SupplierPK);
			yield return (SqlDbType.VarChar, Report_FRInvoicesParameter.InvoiceNumber);
			yield return (SqlDbType.VarChar, Report_FRInvoicesParameter.JobNumber);
			yield return (SqlDbType.VarChar, Report_FRInvoicesParameter.TransportMode);
			yield return (SqlDbType.VarChar, Report_FRInvoicesParameter.MessageType);
			yield return (SqlDbType.SmallDateTime, Report_FRInvoicesParameter.CreateDateFrom);
			yield return (SqlDbType.SmallDateTime, Report_FRInvoicesParameter.CreateDateTo);
			yield return (SqlDbType.SmallDateTime, Report_FRInvoicesParameter.ArrivalDateFrom);
			yield return (SqlDbType.SmallDateTime, Report_FRInvoicesParameter.ArrivalDateTo);
			yield return (SqlDbType.VarChar, Report_FRInvoicesParameter.PortOfDischarge);
			yield return (SqlDbType.VarChar, Report_FRInvoicesParameter.CustomsOffice);
			yield return (SqlDbType.VarChar, Report_FRInvoicesParameter.DeltaReference);
			yield return (SqlDbType.VarChar, Report_FRInvoicesParameter.EntryStatus);
		}

		class Report_FRInvoicesParameter
		{
			public const string CompanyPK = "@CompanyPK";
			public const string BranchPK = "@BranchPK";
			public const string ImporterPK = "@ImporterPK";
			public const string SupplierPK = "@SupplierPK";
			public const string InvoiceNumber = "@InvoiceNumber ";
			public const string JobNumber = "@JobNumber";
			public const string TransportMode = "@TransportMode";
			public const string MessageType = "@MessageType";
			public const string CreateDateFrom = "@CreateDateFrom";
			public const string CreateDateTo = "@CreateDateTo";
			public const string ArrivalDateFrom = "@ArrivalDateFrom";
			public const string ArrivalDateTo = "@ArrivalDateTo";
			public const string PortOfDischarge = "@PortOfDischarge";
			public const string CustomsOffice = "@CustomsOffice";
			public const string DeltaReference = "@DeltaReference";
			public const string EntryStatus = "@EntryStatus";
		}

		#endregion
	}
}
