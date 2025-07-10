using System;
using System.Collections.Generic;
using System.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.Customs.FR;
using Enterprise.Build.Database.Script.TestFramework;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Customs.FR.Testing
{
	[TestedType(typeof(Report_FRDeclarations))]
	class Report_FRDeclarationsTest : CustomsReportDbCreateScriptTest
	{
		public void TestBroker()
		{
			TestDataCreator.CreateJobDeclaration(branchPK, companyPK, "B00001", "IMP", 1, brokerCode: "E");

			AssertFunctionReturnExpectedValue("BrokerDeclarant", "CargoWise Support");
		}

		public void TestEntryStatus()
		{
			var declarationPk = TestDataCreator.CreateJobDeclaration(branchPK, companyPK, "B00001", "IMP", 1);
			var entryInstructionPk = TestDataCreator.CreateCusEntryInstruction(declarationPk, "10", "", DateTime.Today, 1);
			var entryHeaderPk = TestDataCreator.CreateCusEntryHeader(true, "BLT", "OK", "100", "04321-B00181816", 301, 1, "", declarationPk, DateTime.Today, DateTime.Today, entryInstructionPk, "", DateTime.Today, DateTime.Today, DateTime.Today, 1);

			AssertFunctionReturnExpectedValue("EntryStatus", "BAE");

			UpdateEntryHeaderColumn("CH_EntryStatus", "XXX", SqlDbType.VarChar, entryHeaderPk);
			AssertFunctionReturnExpectedValue("EntryStatus", "XXX");

			var entryInstruction2Pk = TestDataCreator.CreateCusEntryInstruction(declarationPk, "20", "", DateTime.Today, 1);
			TestDataCreator.CreateCusEntryHeader(true, "BLT", "ERR", "140", "04321-B00181817", 401, 1, "", declarationPk, DateTime.Today, DateTime.Today, entryInstruction2Pk, "", DateTime.Today, DateTime.Today, DateTime.Today, 1);

			AssertFunctionReturnExpectedValue("EntryStatus", "MLT");
		}

		public void TestCustomsOffice()
		{
			var declarationPk = TestDataCreator.CreateJobDeclaration(branchPK, companyPK, "B00001", "IMP", 1);
			TestDataCreator.CreateCusCodeData("EUO", "CAU", "FR123456", declarationPk, parentTableCode: "JE");
			AssertFunctionReturnExpectedValue("CustomsOfficeCAU", "FR123456");

			TestDataCreator.CreateCusCodeData("EUO", "CAU", "FR00000", declarationPk, parentTableCode: "JE");
			AssertFunctionReturnExpectedValue("CustomsOfficeCAU", "FR123456");

			TestDataCreator.CreateCusCodeData("EUO", "ENT", "FR654321", declarationPk, parentTableCode: "JE");
			AssertFunctionReturnExpectedValue("CustomsOfficeENT", "FR654321");

			TestDataCreator.CreateCusCodeData("EUO", "ENT", "FR00000", declarationPk, parentTableCode: "JE");
			AssertFunctionReturnExpectedValue("CustomsOfficeENT", "FR654321");
		}

		public void TestDeltaMode()
		{
			var declarationPk = TestDataCreator.CreateJobDeclaration(branchPK, companyPK, "B00001", "IMP", 1);
			TestDataCreator.CreateGenAddOnColumn(declarationPk, "JE", "JE_DeltaMode", "STR", "G1");

			AssertFunctionReturnExpectedValue("DeltaMode", "G1");
		}

		public void TestDeclarationType()
		{
			var declarationPk = TestDataCreator.CreateJobDeclaration(branchPK, companyPK, "B00001", "IMP", 1);
			TestDataCreator.CreateCusEntryInstruction(declarationPk, "40", "", DateTime.Now, 1);

			AssertFunctionReturnExpectedValue("DeclarationType", "40");

			TestDataCreator.CreateCusEntryInstruction(declarationPk, "71P", "", DateTime.Now, 1);
			AssertFunctionReturnExpectedValue("DeclarationType", "40/71P");
		}

		public void TestJobType()
		{
			var declaration1Pk = TestDataCreator.CreateJobDeclaration(branchPK, companyPK, "B00001", "IMP", 1);
			AssertFunctionReturnExpectedValue("JobType", "B");

			var shipmentPk = TestDataCreator.CreateShipment("S0001");
			UpdateDeclarationColumn("JE_JS", shipmentPk, SqlDbType.UniqueIdentifier, declaration1Pk);
			AssertFunctionReturnExpectedValue("JobType", "S");
		}

		public void TestInvoiceCount()
		{
			void AssertInvoiceCount(int expectedInvoiceHeaderCount, int expectedInvoiceLineCount)
			{
				AssertFunctionReturnExpectedValue("InvoicesCount", expectedInvoiceHeaderCount);
				AssertFunctionReturnExpectedValue("InvoiceLinesCount", expectedInvoiceLineCount);
			}

			var declarationPk = TestDataCreator.CreateJobDeclaration(branchPK, companyPK, "B00001", "IMP", 1);
			AssertInvoiceCount(expectedInvoiceHeaderCount: 0, expectedInvoiceLineCount: 0);

			var invoice1Pk = TestDataCreator.CreateJobComInvoiceHeader(declarationPk, false, 1);
			AssertInvoiceCount(expectedInvoiceHeaderCount: 1, expectedInvoiceLineCount: 0);

			TestDataCreator.CreateJobComInvoiceLine(invoice1Pk, 1);
			TestDataCreator.CreateJobComInvoiceLine(invoice1Pk, 1);
			AssertInvoiceCount(expectedInvoiceHeaderCount: 1, expectedInvoiceLineCount: 2);

			var invoice2Pk = TestDataCreator.CreateJobComInvoiceHeader(declarationPk, false, 1);
			AssertInvoiceCount(expectedInvoiceHeaderCount: 2, expectedInvoiceLineCount: 2);

			TestDataCreator.CreateJobComInvoiceLine(invoice2Pk, 1);
			AssertInvoiceCount(expectedInvoiceHeaderCount: 2, expectedInvoiceLineCount: 3);

			TestDataCreator.CreateJobComInvoiceHeader(declarationPk, true, 1);
			AssertInvoiceCount(expectedInvoiceHeaderCount: 2, expectedInvoiceLineCount: 3);
		}

		public void TestSupplierEoriFallbacksToForeignIfNecessary()
		{
			var supplierPk = TestDataCreator.CreateOrganisation("SUPPL1", "SUPPLIER");
			var addressPk = TestDataCreator.CreateAddress(supplierPk, "ADD2", "There");

			var declarationPK = TestDataCreator.CreateJobDeclaration(branchPK, companyPK, "B00001", "IMP", 1, supplierPK: supplierPk);
			TestDataCreator.CreateDocAddress(addressPk, "", declarationPK, "JE", "SUD");

			TestDataCreator.CreateOrgCusCode(supplierPk, "EOR", "GBEORI_CODE", "GB");
			AssertFunctionReturnExpectedValue("SupplierEori", "GBEORI_CODE");

			TestDataCreator.CreateOrgCusCode(supplierPk, "EOR", "FREORI_CODE", "FR");
			AssertFunctionReturnExpectedValue("SupplierEori", "FREORI_CODE");
		}

		public void TestSupplierEoriSuffixFallbacksToEORISuffixWithoutAddressIfNecessary()
		{
			var supplierPk = TestDataCreator.CreateOrganisation("SUPPL1", "SUPPLIER");
			TestDataCreator.CreateOrgCusCode(supplierPk, "EOR", "EORI_CODE", "FR");
			TestDataCreator.CreateOrgCusCode(supplierPk, "EBS", "00001", "FR");

			var addressPK1 = TestDataCreator.CreateAddress(supplierPk, "ADD1", "There");
			TestDataCreator.CreateOrgCusCode(supplierPk, "EBS", "00002", "FR", addressPK1);

			var addressPK2 = TestDataCreator.CreateAddress(supplierPk, "ADD2", "There");

			var declarationPK = TestDataCreator.CreateJobDeclaration(branchPK, companyPK, "B00001", "IMP", 1, supplierPK: supplierPk);
			TestDataCreator.CreateDocAddress(addressPK2, "", declarationPK, "JE", "SUD");

			AssertFunctionReturnExpectedValue("SupplierEoriSuffix", "00001");
		}

		public void TestSupplierEoriSuffixFiltersOnCorrectAddressWhenAble()
		{
			var supplierPk = TestDataCreator.CreateOrganisation("SUPPL1", "SUPPLIER");
			TestDataCreator.CreateOrgCusCode(supplierPk, "EOR", "EORI_CODE", "FR");
			TestDataCreator.CreateOrgCusCode(supplierPk, "EBS", "00001", "FR");

			var addressPk1 = TestDataCreator.CreateAddress(supplierPk, "ADD1", "Here");
			TestDataCreator.CreateOrgCusCode(supplierPk, "EBS", "00002", "FR", addressPk1);

			var addressPk2 = TestDataCreator.CreateAddress(supplierPk, "ADD2", "There");
			TestDataCreator.CreateOrgCusCode(supplierPk, "EBS", "00003", "FR", addressPk2);

			var declarationPK = TestDataCreator.CreateJobDeclaration(branchPK, companyPK, "B00001", "IMP", 1, supplierPK: supplierPk);
			TestDataCreator.CreateDocAddress(addressPk1, "", declarationPK, "JE", "SUD");

			AssertFunctionReturnExpectedValue("SupplierEoriSuffix", "00002");
		}

		public void TestSupplierEoriSuffixEmptyWhenEORINotFrench()
		{
			var supplierPk = TestDataCreator.CreateOrganisation("SUPPL1", "SUPPLIER");
			TestDataCreator.CreateOrgCusCode(supplierPk, "EBS", "00001", "FR");
			var addressPk = TestDataCreator.CreateAddress(supplierPk, "ADD1", "Here");
			var declarationPk = TestDataCreator.CreateJobDeclaration(branchPK, companyPK, "B00001", "IMP", 1, supplierPK: supplierPk);
			TestDataCreator.CreateDocAddress(addressPk, "", declarationPk, "JE", "SUD");

			var eori = TestDataCreator.CreateOrgCusCode(supplierPk, "EOR", "EORI_CODE", "GB");
			AssertFunctionReturnExpectedValue("SupplierEoriSuffix", string.Empty);

			UpdateTableColumn("OrgCusCode", "OK_RN_NKCodeCountry", "FR", SqlDbType.VarChar, "OK_PK", eori);
			AssertFunctionReturnExpectedValue("SupplierEoriSuffix", "00001");
		}

		public void TestNoSupplierEoriSuffixEmptyWhenEORIIsOCCASIONNEL()
		{
			var supplierPk = TestDataCreator.CreateOrganisation("SUPPL1", "SUPPLIER");
			TestDataCreator.CreateOrgCusCode(supplierPk, "EBS", "00001", "FR");
			var addressPK1 = TestDataCreator.CreateAddress(supplierPk, "ADD1", "Here");
			var declarationPK = TestDataCreator.CreateJobDeclaration(branchPK, companyPK, "B00001", "IMP", 1, supplierPK: supplierPk);
			TestDataCreator.CreateDocAddress(addressPK1, "", declarationPK, "JE", "SUD");

			TestDataCreator.CreateOrgCusCode(supplierPk, "EOR", "OCCASIONNEL", "FR");
			AssertFunctionReturnExpectedValue("SupplierEori", "OCCASIONNEL");
			AssertFunctionReturnExpectedValue("SupplierEoriSuffix", string.Empty);
		}

		public void TestJobStatus()
		{
			var declaration1Pk = TestDataCreator.CreateJobDeclaration(branchPK, companyPK, "B00001", "IMP", 1);
			var department1Pk = TestDataCreator.CreateDepartment("DEP");
			var jobHeader1Pk = TestDataCreator.CreateJobHeader(branchPK, companyPK, declaration1Pk, department1Pk, "JE", "B00001", "B", "WRK");

			AssertFunctionReturnExpectedValue("JobStatus", "WRK");

			var shipment1Pk = TestDataCreator.CreateShipment("S0001");
			UpdateDeclarationColumn("JE_JS", shipment1Pk, SqlDbType.UniqueIdentifier, declaration1Pk);

			UpdateJobHeaderColumn("JH_ParentID", shipment1Pk, SqlDbType.UniqueIdentifier, jobHeader1Pk);
			UpdateJobHeaderColumn("JH_Status", "CLS", SqlDbType.VarChar, jobHeader1Pk);

			AssertFunctionReturnExpectedValue("JobStatus", "CLS");
		}

		public void TestShipmentPK()
		{
			var declaration1Pk = TestDataCreator.CreateJobDeclaration(branchPK, companyPK, "B00001", "IMP", 1);
			AssertFunctionReturnExpectedValue("ShipmentPK", DBNull.Value);

			var shipmentPk = TestDataCreator.CreateShipment("S0001");
			UpdateDeclarationColumn("JE_JS", shipmentPk, SqlDbType.UniqueIdentifier, declaration1Pk);
			AssertFunctionReturnExpectedValue("ShipmentPK", shipmentPk);
		}

		public void TestFilterByCompanyPk()
		{
			var otherBranchCurrentCompany = TestDataCreator.CreateBranch(companyPK, branchCode: "TB2", homePort: "FRMRS");

			var otherCompanyPK = TestDataCreator.CreateCompany(companyCode: "TC2", countryCode: "FR", currencyCode: "EUR");
			var branchOtherCompanyPK = TestDataCreator.CreateBranch(otherCompanyPK, branchCode: "TB3", homePort: "FRMRZ");

			TestDataCreator.CreateJobDeclaration(branchPK, companyPK, "B00001", "IMP", 1);
			TestDataCreator.CreateJobDeclaration(otherBranchCurrentCompany, companyPK, "B00002", "IMP", 2);
			TestDataCreator.CreateJobDeclaration(branchOtherCompanyPK, otherCompanyPK, "B00003", "EXP", 3);

			var filteredRows = GetFilteredRows(selectedColumn: "JobNumber", (ReportFrDeclarationParameters.CompanyPk, companyPK));
			AssertContainsExactElementsInAnyOrder("Expect 2 rows", new string[] { "B00001", "B00002" }, filteredRows);
		}

		public void TestFilterByBranchPk()
		{
			var otherBranchPK = TestDataCreator.CreateBranch(companyPK, branchCode: "TB2", homePort: "FRMRS");
			TestDataCreator.CreateJobDeclaration(branchPK, companyPK, "B00001", "IMP", 1);
			TestDataCreator.CreateJobDeclaration(otherBranchPK, companyPK, "B00002", "EXP", 2);

			var filteredRows = GetFilteredRows(selectedColumn: "JobNumber", (ReportFrDeclarationParameters.BranchPk, branchPK));
			AssertContainsExactElementsInAnyOrder("Expect 1 row", new string[] { "B00001" }, filteredRows);
		}

		public void TestFilterByMessageType()
		{
			TestDataCreator.CreateJobDeclaration(branchPK, companyPK, "B00001", "IMP", 1);
			TestDataCreator.CreateJobDeclaration(branchPK, companyPK, "B00002", "EXP", 2);
			TestDataCreator.CreateJobDeclaration(branchPK, companyPK, "B00003", "IMP", 3);

			var filteredRows = GetFilteredRows(selectedColumn: "JobNumber", (ReportFrDeclarationParameters.MessageType, "IMP"));
			AssertContainsExactElementsInAnyOrder("Expect 2 rows", new string[] { "B00001", "B00003" }, filteredRows);

			filteredRows = GetFilteredRows(selectedColumn: "JobNumber", (ReportFrDeclarationParameters.MessageType, "EXP"));
			AssertContainsExactElementsInAnyOrder("Expect 1 row", new string[] { "B00002" }, filteredRows);
		}

		public void TestFilterByTransportMode()
		{
			var declarationPk1 = TestDataCreator.CreateJobDeclaration(branchPK, companyPK, "B00001", "IMP", 1);
			UpdateDeclarationColumn("JE_TransportMode", "SEA", SqlDbType.VarChar, declarationPk1);

			var declarationPk2 = TestDataCreator.CreateJobDeclaration(branchPK, companyPK, "B00002", "IMP", 2);
			UpdateDeclarationColumn("JE_TransportMode", "AIR", SqlDbType.VarChar, declarationPk2);

			var filteredRows = GetFilteredRows(selectedColumn: "JobNumber", (ReportFrDeclarationParameters.TransportMode, "SEA"));
			AssertContainsExactElementsInAnyOrder("Expect 1 row", new string[] { "B00001" }, filteredRows);
		}

		public void TestFilterByContainerMode()
		{
			var declarationPK1 = TestDataCreator.CreateJobDeclaration(branchPK, companyPK, "B00001", "IMP", 1);
			UpdateDeclarationColumn("JE_ContainerMode", "NCT", SqlDbType.VarChar, declarationPK1);

			var declarationPK2 = TestDataCreator.CreateJobDeclaration(branchPK, companyPK, "B00002", "IMP", 2);
			UpdateDeclarationColumn("JE_ContainerMode", "CNT", SqlDbType.VarChar, declarationPK2);

			var declarationPK3 = TestDataCreator.CreateJobDeclaration(branchPK, companyPK, "B00003", "IMP", 3);
			UpdateDeclarationColumn("JE_ContainerMode", "CNT", SqlDbType.VarChar, declarationPK3);

			var filteredRows = GetFilteredRows(selectedColumn: "JobNumber", (ReportFrDeclarationParameters.ContainerMode, "CNT"));
			AssertContainsExactElementsInAnyOrder("Expect 2 rows", new string[] { "B00002", "B00003" }, filteredRows);
		}

		public void TestFilterByCreateDate()
		{
			var declarationPK1 = TestDataCreator.CreateJobDeclaration(branchPK, companyPK, "B00001", "IMP", 1);
			UpdateDeclarationColumn("JE_SystemCreateTimeUtc", new DateTime(2020, 11, 01), SqlDbType.SmallDateTime, declarationPK1);

			var declarationPK2 = TestDataCreator.CreateJobDeclaration(branchPK, companyPK, "B00002", "IMP", 2);
			UpdateDeclarationColumn("JE_SystemCreateTimeUtc", new DateTime(2020, 10, 16), SqlDbType.SmallDateTime, declarationPK2);

			var declarationPK3 = TestDataCreator.CreateJobDeclaration(branchPK, companyPK, "B00003", "IMP", 3);
			UpdateDeclarationColumn("JE_SystemCreateTimeUtc", new DateTime(2020, 11, 30, hour: 23, minute: 59, second: 23), SqlDbType.SmallDateTime, declarationPK3);

			var filteredRows = GetFilteredRows(selectedColumn: "JobNumber", (ReportFrDeclarationParameters.CreateDateFrom, new DateTime(2020, 11, 01)), (ReportFrDeclarationParameters.CreateDateTo, new DateTime(2020, 11, 30)));
			AssertContainsExactElementsInAnyOrder("Expect 2 rows", new string[] { "B00001", "B00003" }, filteredRows);
		}

		public void TestFilterByImporter()
		{
			var importer1Pk = TestDataCreator.CreateOrganisation("IMPOR", "Importer");

			var declarationPK1 = TestDataCreator.CreateJobDeclaration(branchPK, companyPK, "B00001", "IMP", 1);
			UpdateDeclarationColumn("JE_OH_Importer", importer1Pk, SqlDbType.UniqueIdentifier, declarationPK1);

			TestDataCreator.CreateJobDeclaration(branchPK, companyPK, "B00002", "IMP", 2);
			var declarationPK3 = TestDataCreator.CreateJobDeclaration(branchPK, companyPK, "B00003", "IMP", 3);
			UpdateDeclarationColumn("JE_OH_Importer", importer1Pk, SqlDbType.UniqueIdentifier, declarationPK3);

			var filteredRows = GetFilteredRows(selectedColumn: "JobNumber", (ReportFrDeclarationParameters.ImporterPk, importer1Pk));
			AssertContainsExactElementsInAnyOrder("Expect 2 rows", new string[] { "B00001", "B00003" }, filteredRows);
		}
		public void TestFilterBySupplier()
		{
			var supplierPk = TestDataCreator.CreateOrganisation("SUPPL", "Supplier");

			TestDataCreator.CreateJobDeclaration(branchPK, companyPK, "B00001", "IMP", 1, supplierPK: supplierPk);
			TestDataCreator.CreateJobDeclaration(branchPK, companyPK, "B00002", "IMP", 2);
			TestDataCreator.CreateJobDeclaration(branchPK, companyPK, "B00003", "IMP", 3, supplierPK: supplierPk);

			var filteredRows = GetFilteredRows(selectedColumn: "JobNumber", (ReportFrDeclarationParameters.SupplierPk, supplierPk));
			AssertContainsExactElementsInAnyOrder("Expect 2 rows", new string[] { "B00001", "B00003" }, filteredRows);
		}

		public void TestFilterByDepartureDate()
		{
			var declarationPK1 = TestDataCreator.CreateJobDeclaration(branchPK, companyPK, "B00001", "IMP", 1);
			UpdateDeclarationColumn("JE_ExportDate", new DateTime(2020, 11, 16), SqlDbType.SmallDateTime, declarationPK1);

			var declarationPK2 = TestDataCreator.CreateJobDeclaration(branchPK, companyPK, "B00002", "IMP", 2);
			UpdateDeclarationColumn("JE_ExportDate", new DateTime(2020, 12, 16), SqlDbType.SmallDateTime, declarationPK2);

			var declarationPK3 = TestDataCreator.CreateJobDeclaration(branchPK, companyPK, "B00003", "IMP", 3);
			UpdateDeclarationColumn("JE_ExportDate", new DateTime(2020, 11, 01), SqlDbType.SmallDateTime, declarationPK3);

			var filteredRows = GetFilteredRows(selectedColumn: "JobNumber", (ReportFrDeclarationParameters.DepartureDateFrom, new DateTime(2020, 11, 01)), (ReportFrDeclarationParameters.DepartureDateTo, new DateTime(2020, 11, 30)));
			AssertContainsExactElementsInAnyOrder("Expect 2 rows", new string[] { "B00001", "B00003" }, filteredRows);
		}

		public void TestArrivalDate()
		{
			var declarationPK1 = TestDataCreator.CreateJobDeclaration(branchPK, companyPK, "B00001", "IMP", 1);
			UpdateDeclarationColumn("JE_DateOfArrival", new DateTime(2020, 11, 16), SqlDbType.SmallDateTime, declarationPK1);

			var declarationPK2 = TestDataCreator.CreateJobDeclaration(branchPK, companyPK, "B00002", "IMP", 2);
			UpdateDeclarationColumn("JE_DateOfArrival", new DateTime(2020, 12, 16), SqlDbType.SmallDateTime, declarationPK2);

			var declarationPK3 = TestDataCreator.CreateJobDeclaration(branchPK, companyPK, "B00003", "IMP", 3);
			UpdateDeclarationColumn("JE_DateOfArrival", new DateTime(2020, 11, 01), SqlDbType.SmallDateTime, declarationPK3);

			var filteredRows = GetFilteredRows(selectedColumn: "JobNumber", (ReportFrDeclarationParameters.ArrivalDateFrom, new DateTime(2020, 11, 01)), (ReportFrDeclarationParameters.ArrivalDateTo, new DateTime(2020, 11, 30)));
			AssertContainsExactElementsInAnyOrder("Expect 2 rows", new string[] { "B00001", "B00003" }, filteredRows);
		}

		public void TestFilterByAssessmentDateFrom()
		{
			var declarationPK1 = TestDataCreator.CreateJobDeclaration(branchPK, companyPK, "B00001", "IMP", 1);
			TestDataCreator.CreateCusEntryInstruction(declarationPK1, "40", "", new DateTime(2020, 11, 04), 1);

			var declarationPK2 = TestDataCreator.CreateJobDeclaration(branchPK, companyPK, "B00002", "IMP", 2);
			TestDataCreator.CreateCusEntryInstruction(declarationPK2, "10P", "", new DateTime(2020, 10, 05), 2);

			var filteredRows = GetFilteredRows(selectedColumn: "JobNumber", (ReportFrDeclarationParameters.AssessmentDateFrom, new DateTime(2020, 11, 01)), (ReportFrDeclarationParameters.AssessmentDateTo, new DateTime(2020, 11, 30)));
			AssertContainsExactElementsInAnyOrder("Expect 1 row", new string[] { "B00001" }, filteredRows);
		}

		public void TestFilterByCpc()
		{
			var declarationPK1 = TestDataCreator.CreateJobDeclaration(branchPK, companyPK, "B00001", "IMP", 1);
			var entryInstruction1 = TestDataCreator.CreateCusEntryInstruction(declarationPK1, "40", "", new DateTime(2020, 11, 04), 1);
			TestDataCreator.CreateCusEntryHeader(true, "BLT", "OK", "BAE", "04321-B00181816", 301, 1, "", declarationPK1, DateTime.Today, DateTime.Today, entryInstruction1, "", DateTime.Today, DateTime.Today, DateTime.Today, 1);

			var declarationPK2 = TestDataCreator.CreateJobDeclaration(branchPK, companyPK, "B00002", "IMP", 2);
			var entryInstruction2 = TestDataCreator.CreateCusEntryInstruction(declarationPK2, "10P", "", new DateTime(2020, 10, 05), 2);
			TestDataCreator.CreateCusEntryHeader(true, "BLT", "OK", "BAE", "04321-B00181816", 301, 1, "", declarationPK2, DateTime.Today, DateTime.Today, entryInstruction2, "", DateTime.Today, DateTime.Today, DateTime.Today, 1);

			var filteredRows = GetFilteredRows(selectedColumn: "JobNumber", (ReportFrDeclarationParameters.CPC, "40"));
			AssertContainsExactElementsInAnyOrder("Expect 1 row", new string[] { "B00001" }, filteredRows);
		}

		public void TestFilterByEntryStatus()
		{
			var declarationPK1 = TestDataCreator.CreateJobDeclaration(branchPK, companyPK, "B00001", "IMP", 1);
			var entryInstructionPk = TestDataCreator.CreateCusEntryInstruction(declarationPK1, "40", "", new DateTime(2020, 11, 04), 1);
			TestDataCreator.CreateCusEntryHeader(true, "BLT", "OK", "100", "04321-B00181816", 301, 1, "", declarationPK1, DateTime.Today, DateTime.Today, entryInstructionPk, "", DateTime.Today, DateTime.Today, DateTime.Today, 1);

			TestDataCreator.CreateJobDeclaration(branchPK, companyPK, "B00002", "IMP", 2);

			var filteredRows = GetFilteredRows(selectedColumn: "JobNumber", (ReportFrDeclarationParameters.EntryStatus, "100"));
			AssertContainsExactElementsInAnyOrder("Expect 1 row", new string[] { "B00001" }, filteredRows);
		}

		public void TestFilterByMessageStatus()
		{
			var declarationPK1 = TestDataCreator.CreateJobDeclaration(branchPK, companyPK, "B00001", "IMP", 1);
			var entryInstructionPk = TestDataCreator.CreateCusEntryInstruction(declarationPK1, "40", "", new DateTime(2020, 11, 04), 1);
			TestDataCreator.CreateCusEntryHeader(true, "BLT", "SNT", "", "04321-B00181816", 301, 1, "", declarationPK1, DateTime.Today, DateTime.Today, entryInstructionPk, "", DateTime.Today, DateTime.Today, DateTime.Today, 1);

			TestDataCreator.CreateJobDeclaration(branchPK, companyPK, "B00002", "IMP", 2);

			var filteredRows = GetFilteredRows(selectedColumn: "JobNumber", (ReportFrDeclarationParameters.MessageStatus, "SNT"));
			AssertContainsExactElementsInAnyOrder("Expect 1 row", new string[] { "B00001" }, filteredRows);
		}

		public void TestFilterByProfile()
		{
			var declarationPK1 = TestDataCreator.CreateJobDeclaration(branchPK, companyPK, "B00001", "IMP", 1);
			UpdateDeclarationColumn("JE_CustomsProfile", "0000186", SqlDbType.VarChar, declarationPK1);

			TestDataCreator.CreateJobDeclaration(branchPK, companyPK, "B00002", "IMP", 2);

			var filteredRows = GetFilteredRows(selectedColumn: "JobNumber", (ReportFrDeclarationParameters.CustomsProfile, "0000186"));
			AssertContainsExactElementsInAnyOrder("Expect 1 row", new string[] { "B00001" }, filteredRows);
		}

		public void TestFilterByDeltaMode()
		{
			var declarationPK1 = TestDataCreator.CreateJobDeclaration(branchPK, companyPK, "B00001", "IMP", 1);
			TestDataCreator.CreateGenAddOnColumn(declarationPK1, "JE", "JE_DeltaMode", "STR", "G1");

			TestDataCreator.CreateJobDeclaration(branchPK, companyPK, "B00002", "IMP", 2);

			var filteredRows = GetFilteredRows(selectedColumn: "JobNumber", (ReportFrDeclarationParameters.DeltaMode, "G1"));
			AssertContainsExactElementsInAnyOrder("Expect 1 row", new string[] { "B00001" }, filteredRows);
		}

		public void TestFilterByPortOfDischarge()
		{
			var declarationPK1 = TestDataCreator.CreateJobDeclaration(branchPK, companyPK, "B00001", "IMP", 1);
			UpdateDeclarationColumn("JE_RL_NKPortOfArrival", "ITVCE", SqlDbType.VarChar, declarationPK1);

			TestDataCreator.CreateJobDeclaration(branchPK, companyPK, "B00002", "IMP", 2);

			var filteredRows = GetFilteredRows(selectedColumn: "JobNumber", (ReportFrDeclarationParameters.PortOfDischarge, "ITVCE"));
			AssertContainsExactElementsInAnyOrder("Expect 1 row", new string[] { "B00001" }, filteredRows);
		}

		public void TestFilterByDeltaReference()
		{
			var declarationPK1 = TestDataCreator.CreateJobDeclaration(branchPK, companyPK, "B00001", "IMP", 1);
			var entryInstructionPk = TestDataCreator.CreateCusEntryInstruction(declarationPK1, "", "", DateTime.Now, 1);
			var entryHeaderPk = TestDataCreator.CreateCusEntryHeader(true, "BLT", "OK", "BAE", "04321-B00181816", 301, 1, "", declarationPK1, DateTime.Today, DateTime.Today, entryInstructionPk, "", DateTime.Today, DateTime.Today, DateTime.Today, 1);
			TestDataCreator.CreateCusEntryNum(entryHeaderPk, "CusEntryHeader", "DELTA-REF", "IMP", "CUS", "FR");

			TestDataCreator.CreateJobDeclaration(branchPK, companyPK, "B00002", "IMP", 2);
			var filteredRows = GetFilteredRows(selectedColumn: "JobNumber", (ReportFrDeclarationParameters.DeltaReference, "DELTA-REF"));
			AssertContainsExactElementsInAnyOrder("Expect 1 row", new string[] { "B00001" }, filteredRows);
		}

		public void TestFilterByCustomsOffice()
		{
			var declarationPK1 = TestDataCreator.CreateJobDeclaration(branchPK, companyPK, "B00001", "IMP", 1);
			UpdateDeclarationColumn("JE_CustomsOffice", "FR123456", SqlDbType.VarChar, declarationPK1);

			TestDataCreator.CreateJobDeclaration(branchPK, companyPK, "B00002", "IMP", 2);

			var filteredRows = GetFilteredRows(selectedColumn: "JobNumber", (ReportFrDeclarationParameters.CustomsOffice, "FR123456"));
			AssertContainsExactElementsInAnyOrder("Expect 1 row", new string[] { "B00001" }, filteredRows);
		}

		public void TestFilterByPortOfLoading()
		{
			var declarationPK1 = TestDataCreator.CreateJobDeclaration(branchPK, companyPK, "B00001", "IMP", 1);
			UpdateDeclarationColumn("JE_RL_NKPortOfLoading", "FRMRS", SqlDbType.VarChar, declarationPK1);

			TestDataCreator.CreateJobDeclaration(branchPK, companyPK, "B00002", "IMP", 2);

			var filteredRows = GetFilteredRows(selectedColumn: "JobNumber", (ReportFrDeclarationParameters.PortOfLoading, "FRMRS"));
			AssertContainsExactElementsInAnyOrder("Expect 1 row", new string[] { "B00001" }, filteredRows);
		}

		protected override void SetUp()
		{
			base.SetUp();

			companyPK = TestDataCreator.CreateCompany(companyCode: "TC1", countryCode: "FR", currencyCode: "EUR");
			branchPK = TestDataCreator.CreateBranch(companyPK, branchCode: "TB1", homePort: "FRANCE");
		}

		protected override IEnumerable<(SqlDbType ParameterType, string ParameterName)> GetSqlParameters()
		{
			yield return (SqlDbType.UniqueIdentifier, ReportFrDeclarationParameters.CompanyPk);
			yield return (SqlDbType.UniqueIdentifier, ReportFrDeclarationParameters.BranchPk);
			yield return (SqlDbType.VarChar, ReportFrDeclarationParameters.MessageType);
			yield return (SqlDbType.VarChar, ReportFrDeclarationParameters.TransportMode);
			yield return (SqlDbType.VarChar, ReportFrDeclarationParameters.ContainerMode);
			yield return (SqlDbType.SmallDateTime, ReportFrDeclarationParameters.CreateDateFrom);
			yield return (SqlDbType.SmallDateTime, ReportFrDeclarationParameters.CreateDateTo);
			yield return (SqlDbType.UniqueIdentifier, ReportFrDeclarationParameters.ImporterPk);
			yield return (SqlDbType.UniqueIdentifier, ReportFrDeclarationParameters.SupplierPk);
			yield return (SqlDbType.SmallDateTime, ReportFrDeclarationParameters.DepartureDateFrom);
			yield return (SqlDbType.SmallDateTime, ReportFrDeclarationParameters.DepartureDateTo);
			yield return (SqlDbType.SmallDateTime, ReportFrDeclarationParameters.ArrivalDateFrom);
			yield return (SqlDbType.SmallDateTime, ReportFrDeclarationParameters.ArrivalDateTo);
			yield return (SqlDbType.SmallDateTime, ReportFrDeclarationParameters.AssessmentDateFrom);
			yield return (SqlDbType.SmallDateTime, ReportFrDeclarationParameters.AssessmentDateTo);
			yield return (SqlDbType.VarChar, ReportFrDeclarationParameters.CPC);
			yield return (SqlDbType.VarChar, ReportFrDeclarationParameters.EntryStatus);
			yield return (SqlDbType.VarChar, ReportFrDeclarationParameters.MessageStatus);
			yield return (SqlDbType.VarChar, ReportFrDeclarationParameters.CustomsProfile);
			yield return (SqlDbType.VarChar, ReportFrDeclarationParameters.DeltaMode);
			yield return (SqlDbType.VarChar, ReportFrDeclarationParameters.PortOfDischarge);
			yield return (SqlDbType.VarChar, ReportFrDeclarationParameters.DeltaReference);
			yield return (SqlDbType.VarChar, ReportFrDeclarationParameters.CustomsOffice);
			yield return (SqlDbType.VarChar, ReportFrDeclarationParameters.PortOfLoading);
		}

		protected override bool RequiresSchemaBinding => true;

		class ReportFrDeclarationParameters
		{
			public const string CompanyPk = "@CompanyPk";
			public const string BranchPk = "@BranchPk";
			public const string MessageType = "@MessageType";
			public const string TransportMode = "@TransportMode";
			public const string ContainerMode = "@ContainerMode";
			public const string CreateDateFrom = "@CreateDateFrom";
			public const string CreateDateTo = "@CreateDateTo";
			public const string ImporterPk = "@ImporterPk";
			public const string SupplierPk = "@SupplierPk";
			public const string DepartureDateFrom = "@DepartureDateFrom";
			public const string DepartureDateTo = "@DepartureDateTo";
			public const string ArrivalDateFrom = "@ArrivalDateFrom";
			public const string ArrivalDateTo = "@ArrivalDateTo";
			public const string AssessmentDateFrom = "@AssessmentDateFrom";
			public const string AssessmentDateTo = "@AssessmentDateTo ";
			public const string CPC = "@CPC";
			public const string EntryStatus = "@EntryStatus";
			public const string MessageStatus = "@MessageStatus";
			public const string CustomsProfile = "@CustomsProfile";
			public const string DeltaMode = "@DeltaMode";
			public const string PortOfDischarge = "@PortOfDischarge";
			public const string DeltaReference = "@DeltaReference";
			public const string CustomsOffice = "@CustomsOffice";
			public const string PortOfLoading = "@PortOfLoading";
		}

		Guid companyPK;
		Guid branchPK;
	}
}
