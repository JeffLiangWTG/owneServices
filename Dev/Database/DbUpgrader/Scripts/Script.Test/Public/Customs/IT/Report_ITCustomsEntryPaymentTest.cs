using System;
using System.Collections.Generic;
using System.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.Customs.IT;
using Enterprise.Build.Database.Script.TestFramework;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Customs.IT.Testing
{
	[TestedType(typeof(Report_ITCustomsEntryPayment))]
	class Report_ITCustomsEntryPayment_FiltersTest : Report_ITCustomsEntryPaymentBaseTest
	{
		public void TestFilterByCompanyPK()
		{
			var company2PK = TestDataCreator.CreateCompany(companyCode: "IT2", countryCode: "IT", currencyCode: "EUR");
			var branch2PK = TestDataCreator.CreateBranch(company2PK, branchCode: "IT2", homePort: "ITALY");
			var declaration2PK = TestDataCreator.CreateJobDeclaration(branch2PK, company2PK, "B00002", "IMP", 2);
			var jobHeader2PK = TestDataCreator.CreateJobHeader(branch2PK, company2PK, declaration2PK, departmentPK, "JE", "", "", "WRK");
			var entryHeader2PK = TestDataCreator.CreateCusEntryHeader(true, "BLT", "ACO", "REG", "04321-B00002", 301, 1, "", declaration2PK, DateTime.Today, DateTime.Today, Guid.Empty, "", DateTime.Today, DateTime.Today, DateTime.Today, 2);
			TestDataCreator.CreateCusEntryPayInfo("", 0m, "", DateTime.Today, "", "", "", entryHeader2PK, DateTime.Today, 2);
			TestDataCreator.CreateJobCharge(branch2PK, company2PK, departmentPK, jobHeader2PK, accChargeCode1PK, "5678", DateTime.Today);

			var filteredRows = GetFilteredRows(selectedColumn: "DeclarationPK");
			AssertContainsExactElementsInAnyOrder(new Guid[] { declarationPK, declarationPK, declaration2PK, declaration2PK }, filteredRows);

			filteredRows = GetFilteredRows(selectedColumn: "DeclarationPK", (Report_ITCustomsEntryPaymentParameters.CompanyPK, companyPK));
			AssertContainsExactElementsInAnyOrder(new Guid[] { declarationPK, declarationPK }, filteredRows);

			filteredRows = GetFilteredRows(selectedColumn: "DeclarationPK", (Report_ITCustomsEntryPaymentParameters.CompanyPK, company2PK));
			AssertContainsExactElementsInAnyOrder(new Guid[] { declaration2PK, declaration2PK }, filteredRows);
		}

		public void TestFilterByCustomsReleaseDateRange()
		{
			UpdateEntryHeaderColumn("CH_EntryReleaseDate", new DateTime(2020, 01, 01), SqlDbType.SmallDateTime, entryHeaderPK);

			var declaration2PK = TestDataCreator.CreateJobDeclaration(branchPK, companyPK, "B00002", "IMP", 2);
			var jobHeader2PK = TestDataCreator.CreateJobHeader(branchPK, companyPK, declaration2PK, departmentPK, "JE", "1", "", "WRK");
			var entryHeader2PK = TestDataCreator.CreateCusEntryHeader(true, "BLT", "ACO", "REG", "04321-B00002", 301, 1, "", declaration2PK, DateTime.Today, new DateTime(2020, 01, 10), Guid.Empty, "", DateTime.Today, DateTime.Today, DateTime.Today, 2);
			TestDataCreator.CreateCusEntryPayInfo("81", 200m, "", DateTime.Today, "", "F", "", entryHeader2PK, DateTime.Today, 2);
			var entryHeader3PK = TestDataCreator.CreateCusEntryHeader(true, "BLT", "ACO", "REG", "04321-B00002", 301, 1, "", declaration2PK, DateTime.Today, new DateTime(2020, 01, 05), Guid.Empty, "", DateTime.Today, DateTime.Today, DateTime.Today, 2);
			TestDataCreator.CreateCusEntryPayInfo("79", 99.2m, "", DateTime.Today, "", "G", "", entryHeader3PK, DateTime.Today, 2);
			TestDataCreator.CreateJobCharge(branchPK, companyPK, departmentPK, jobHeader2PK, accChargeCode1PK, "1234", DateTime.Today);

			var filteredRows = GetFilteredRows(selectedColumn: "DeclarationPK");
			AssertContainsExactElementsInAnyOrder(new Guid[] { declarationPK, declarationPK, declaration2PK, declaration2PK, declaration2PK }, filteredRows);

			filteredRows = GetFilteredRows(selectedColumn: "DeclarationPK", (Report_ITCustomsEntryPaymentParameters.CustomsReleaseDateFrom, new DateTime(2020, 01, 01)));
			AssertContainsExactElementsInAnyOrder(new Guid[] { declarationPK, declarationPK, declaration2PK, declaration2PK, declaration2PK }, filteredRows);

			filteredRows = GetFilteredRows(selectedColumn: "DeclarationPK", (Report_ITCustomsEntryPaymentParameters.CustomsReleaseDateFrom, new DateTime(2020, 01, 01)), (Report_ITCustomsEntryPaymentParameters.CustomsReleaseDateTo, new DateTime(2020, 01, 05)));
			AssertContainsExactElementsInAnyOrder(new Guid[] { declarationPK, declarationPK, declaration2PK, declaration2PK, declaration2PK }, filteredRows);

			filteredRows = GetFilteredRows(selectedColumn: "DeclarationPK", (Report_ITCustomsEntryPaymentParameters.CustomsReleaseDateFrom, new DateTime(2020, 01, 05)), (Report_ITCustomsEntryPaymentParameters.CustomsReleaseDateTo, new DateTime(2020, 01, 06)));
			AssertContainsExactElementsInAnyOrder(new Guid[] { declaration2PK, declaration2PK, declaration2PK }, filteredRows);

			filteredRows = GetFilteredRows(selectedColumn: "DeclarationPK", (Report_ITCustomsEntryPaymentParameters.CustomsReleaseDateFrom, new DateTime(2020, 01, 05)));
			AssertContainsExactElementsInAnyOrder(new Guid[] { declaration2PK, declaration2PK, declaration2PK }, filteredRows);

			filteredRows = GetFilteredRows(selectedColumn: "DeclarationPK", (Report_ITCustomsEntryPaymentParameters.CustomsReleaseDateTo, new DateTime(2020, 01, 04)));
			AssertContainsExactElementsInAnyOrder(new Guid[] { declarationPK, declarationPK }, filteredRows);
		}

		public void TestFilterByPaymentDateRange()
		{
			UpdateCusEntryPayInfoColumn("C9_PaymentDate", new DateTime(2020, 01, 01), SqlDbType.SmallDateTime, entryPayInfoPK);

			var declaration2PK = TestDataCreator.CreateJobDeclaration(branchPK, companyPK, "B00002", "IMP", 2);
			var jobHeader2PK = TestDataCreator.CreateJobHeader(branchPK, companyPK, declaration2PK, departmentPK, "JE", "1", "", "WRK");
			var entryHeader2PK = TestDataCreator.CreateCusEntryHeader(true, "BLT", "ACO", "REG", "04321-B00002", 301, 1, "", declaration2PK, DateTime.Today, new DateTime(2020, 01, 10), Guid.Empty, "", DateTime.Today, DateTime.Today, DateTime.Today, 2);
			TestDataCreator.CreateCusEntryPayInfo("81", 200m, "", new DateTime(2020, 01, 10), "", "F", "", entryHeader2PK, DateTime.Today, 2);
			var entryHeader3PK = TestDataCreator.CreateCusEntryHeader(true, "BLT", "ACO", "REG", "04321-B00002", 301, 1, "", declaration2PK, DateTime.Today, new DateTime(2020, 01, 05), Guid.Empty, "", DateTime.Today, DateTime.Today, DateTime.Today, 2);
			TestDataCreator.CreateCusEntryPayInfo("79", 99.2m, "", new DateTime(2020, 01, 05), "", "G", "", entryHeader3PK, DateTime.Today, 2);
			TestDataCreator.CreateJobCharge(branchPK, companyPK, departmentPK, jobHeader2PK, accChargeCode1PK, "1234", DateTime.Today);

			var filteredRows = GetFilteredRows(selectedColumn: "DeclarationPK");
			AssertContainsExactElementsInAnyOrder(new Guid[] { declarationPK, declarationPK, declaration2PK, declaration2PK, declaration2PK }, filteredRows);

			filteredRows = GetFilteredRows(selectedColumn: "DeclarationPK", (Report_ITCustomsEntryPaymentParameters.PaymentDateFrom, new DateTime(2020, 01, 01)));
			AssertContainsExactElementsInAnyOrder(new Guid[] { declarationPK, declarationPK, declaration2PK, declaration2PK, declaration2PK }, filteredRows);

			filteredRows = GetFilteredRows(selectedColumn: "DeclarationPK", (Report_ITCustomsEntryPaymentParameters.PaymentDateFrom, new DateTime(2020, 01, 01)), (Report_ITCustomsEntryPaymentParameters.PaymentDateTo, new DateTime(2020, 01, 05)));
			AssertContainsExactElementsInAnyOrder(new Guid[] { declarationPK, declarationPK, declaration2PK, declaration2PK, declaration2PK }, filteredRows);

			filteredRows = GetFilteredRows(selectedColumn: "DeclarationPK", (Report_ITCustomsEntryPaymentParameters.PaymentDateFrom, new DateTime(2020, 01, 05)), (Report_ITCustomsEntryPaymentParameters.PaymentDateTo, new DateTime(2020, 01, 06)));
			AssertContainsExactElementsInAnyOrder(new Guid[] { declaration2PK, declaration2PK, declaration2PK }, filteredRows);

			filteredRows = GetFilteredRows(selectedColumn: "DeclarationPK", (Report_ITCustomsEntryPaymentParameters.PaymentDateFrom, new DateTime(2020, 01, 05)));
			AssertContainsExactElementsInAnyOrder(new Guid[] { declaration2PK, declaration2PK, declaration2PK }, filteredRows);

			filteredRows = GetFilteredRows(selectedColumn: "DeclarationPK", (Report_ITCustomsEntryPaymentParameters.PaymentDateTo, new DateTime(2020, 01, 04)));
			AssertContainsExactElementsInAnyOrder(new Guid[] { declarationPK, declarationPK }, filteredRows);
		}

		public void TestFilterByDefermentAccountNumber()
		{
			UpdateDeclarationColumn("JE_DefermentAccountNumber", "1234", SqlDbType.VarChar, declarationPK);

			var declaration2PK = TestDataCreator.CreateJobDeclaration(branchPK, companyPK, "B00002", "IMP", 2);
			UpdateDeclarationColumn("JE_DefermentAccountNumber", "1234", SqlDbType.VarChar, declaration2PK);
			var jobHeader2PK = TestDataCreator.CreateJobHeader(branchPK, companyPK, declaration2PK, departmentPK, "JE", "2", "2", "WRK");
			var entryHeader2PK = TestDataCreator.CreateCusEntryHeader(true, "BLT", "ACO", "REG", "04321-B00002", 301, 1, "", declaration2PK, DateTime.Today, DateTime.Today, Guid.Empty, "", DateTime.Today, DateTime.Today, DateTime.Today, 2);
			TestDataCreator.CreateCusEntryPayInfo("", 0m, "", DateTime.Today, "", "", "", entryHeader2PK, DateTime.Today, 2);
			TestDataCreator.CreateJobCharge(branchPK, companyPK, departmentPK, jobHeader2PK, accChargeCode1PK, "1234", DateTime.Today);

			var filteredRows = GetFilteredRows(selectedColumn: "DeclarationPK");
			AssertContainsExactElementsInAnyOrder(new Guid[] { declarationPK, declarationPK, declaration2PK, declaration2PK }, filteredRows);

			filteredRows = GetFilteredRows(selectedColumn: "DeclarationPK", (Report_ITCustomsEntryPaymentParameters.DefermentAccountNumber, "1234"));
			AssertContainsExactElementsInAnyOrder(new Guid[] { declarationPK, declarationPK, declaration2PK, declaration2PK }, filteredRows);

			UpdateDeclarationColumn("JE_DefermentAccountNumber", "5678", SqlDbType.VarChar, declaration2PK);
			filteredRows = GetFilteredRows(selectedColumn: "DeclarationPK", (Report_ITCustomsEntryPaymentParameters.DefermentAccountNumber, "5678"));
			AssertContainsExactElementsInAnyOrder(new Guid[] { declaration2PK, declaration2PK }, filteredRows);
		}

		public void TestFilterByCustomsOffice()
		{
			UpdateDeclarationColumn("JE_CustomsOffice", "1234", SqlDbType.VarChar, declarationPK);

			var declaration2PK = TestDataCreator.CreateJobDeclaration(branchPK, companyPK, "B00002", "IMP", 2);
			UpdateDeclarationColumn("JE_CustomsOffice", "1234", SqlDbType.VarChar, declaration2PK);
			var jobHeader2PK = TestDataCreator.CreateJobHeader(branchPK, companyPK, declaration2PK, departmentPK, "JE", "2", "2", "WRK");
			var entryHeader2PK = TestDataCreator.CreateCusEntryHeader(true, "BLT", "ACO", "REG", "04321-B00002", 301, 1, "", declaration2PK, DateTime.Today, DateTime.Today, Guid.Empty, "", DateTime.Today, DateTime.Today, DateTime.Today, 2);
			TestDataCreator.CreateCusEntryPayInfo("", 0m, "", DateTime.Today, "", "", "", entryHeader2PK, DateTime.Today, 2);
			TestDataCreator.CreateJobCharge(branchPK, companyPK, departmentPK, jobHeader2PK, accChargeCode1PK, "1234", DateTime.Today);

			var filteredRows = GetFilteredRows(selectedColumn: "DeclarationPK");
			AssertContainsExactElementsInAnyOrder(new Guid[] { declarationPK, declarationPK, declaration2PK, declaration2PK }, filteredRows);

			filteredRows = GetFilteredRows(selectedColumn: "DeclarationPK", (Report_ITCustomsEntryPaymentParameters.CustomsOffice, "1234"));
			AssertContainsExactElementsInAnyOrder(new Guid[] { declarationPK, declarationPK, declaration2PK, declaration2PK }, filteredRows);

			UpdateDeclarationColumn("JE_CustomsOffice", "5678", SqlDbType.VarChar, declaration2PK);
			filteredRows = GetFilteredRows(selectedColumn: "DeclarationPK", (Report_ITCustomsEntryPaymentParameters.CustomsOffice, "5678"));
			AssertContainsExactElementsInAnyOrder(new Guid[] { declaration2PK, declaration2PK }, filteredRows);
		}

		public void TestFilterByMethodOfPayment()
		{
			var declaration2PK = TestDataCreator.CreateJobDeclaration(branchPK, companyPK, "B00002", "IMP", 2);
			var jobHeader2PK = TestDataCreator.CreateJobHeader(branchPK, companyPK, declaration2PK, departmentPK, "JE", "1", "", "WRK");
			var entryHeader2PK = TestDataCreator.CreateCusEntryHeader(true, "BLT", "ACO", "REG", "04321-B00002", 301, 1, "", declaration2PK, DateTime.Today, new DateTime(2020, 01, 10), Guid.Empty, "", DateTime.Today, DateTime.Today, DateTime.Today, 2);
			TestDataCreator.CreateCusEntryPayInfo("81", 200m, "", DateTime.Today, "", "F", "", entryHeader2PK, DateTime.Today, 2);
			var entryHeader3PK = TestDataCreator.CreateCusEntryHeader(true, "BLT", "ACO", "REG", "04321-B00002", 301, 1, "", declaration2PK, DateTime.Today, new DateTime(2020, 01, 05), Guid.Empty, "", DateTime.Today, DateTime.Today, DateTime.Today, 2);
			TestDataCreator.CreateCusEntryPayInfo("79", 99.2m, "", DateTime.Today, "", "G", "", entryHeader3PK, DateTime.Today, 2);
			TestDataCreator.CreateJobCharge(branchPK, companyPK, departmentPK, jobHeader2PK, accChargeCode1PK, "1234", DateTime.Today);

			var filteredRows = GetFilteredRows(selectedColumn: "DeclarationPK", (Report_ITCustomsEntryPaymentParameters.MethodOfPaymentList, ""));
			AssertContainsExactElementsInAnyOrder(new Guid[] { declarationPK, declarationPK, declaration2PK, declaration2PK, declaration2PK }, filteredRows);

			filteredRows = GetFilteredRows(selectedColumn: "DeclarationPK", (Report_ITCustomsEntryPaymentParameters.MethodOfPaymentList, "A"));
			AssertContainsExactElementsInAnyOrder(new Guid[] { declarationPK, declarationPK }, filteredRows);

			filteredRows = GetFilteredRows(selectedColumn: "DeclarationPK", (Report_ITCustomsEntryPaymentParameters.MethodOfPaymentList, "F,G"));
			AssertContainsExactElementsInAnyOrder(new Guid[] { declaration2PK, declaration2PK, declaration2PK }, filteredRows);

			filteredRows = GetFilteredRows(selectedColumn: "DeclarationPK", (Report_ITCustomsEntryPaymentParameters.MethodOfPaymentList, "F"));
			AssertContainsExactElementsInAnyOrder(new Guid[] { declaration2PK, declaration2PK, declaration2PK }, filteredRows);

			filteredRows = GetFilteredRows(selectedColumn: "DeclarationPK", (Report_ITCustomsEntryPaymentParameters.MethodOfPaymentList, "G"));
			AssertContainsExactElementsInAnyOrder(new Guid[] { declaration2PK, declaration2PK, declaration2PK }, filteredRows);
		}

		public void TestFilterByImporter()
		{
			var importerPK = TestDataCreator.CreateOrganisation("IMPCODE", "IMPNAME");
			UpdateDeclarationColumn("JE_OH_Importer", importerPK, SqlDbType.UniqueIdentifier, declarationPK);

			var declaration2PK = TestDataCreator.CreateJobDeclaration(branchPK, companyPK, "B00002", "IMP", 2);
			UpdateDeclarationColumn("JE_OH_Importer", importerPK, SqlDbType.UniqueIdentifier, declaration2PK);
			var jobHeader2PK = TestDataCreator.CreateJobHeader(branchPK, companyPK, declaration2PK, departmentPK, "JE", "2", "2", "WRK");
			var entryHeader2PK = TestDataCreator.CreateCusEntryHeader(true, "BLT", "ACO", "REG", "04321-B00002", 301, 1, "", declaration2PK, DateTime.Today, DateTime.Today, Guid.Empty, "", DateTime.Today, DateTime.Today, DateTime.Today, 2);
			TestDataCreator.CreateCusEntryPayInfo("", 0m, "", DateTime.Today, "", "", "", entryHeader2PK, DateTime.Today, 2);
			TestDataCreator.CreateJobCharge(branchPK, companyPK, departmentPK, jobHeader2PK, accChargeCode1PK, "1234", DateTime.Today);

			var filteredRows = GetFilteredRows(selectedColumn: "DeclarationPK");
			AssertContainsExactElementsInAnyOrder(new Guid[] { declarationPK, declarationPK, declaration2PK, declaration2PK }, filteredRows);

			filteredRows = GetFilteredRows(selectedColumn: "DeclarationPK", (Report_ITCustomsEntryPaymentParameters.ImporterPK, importerPK));
			AssertContainsExactElementsInAnyOrder(new Guid[] { declarationPK, declarationPK, declaration2PK, declaration2PK }, filteredRows);

			var importer2PK = TestDataCreator.CreateOrganisation("IMPCODE2", "IMPNAME2");
			UpdateDeclarationColumn("JE_OH_Importer", importer2PK, SqlDbType.UniqueIdentifier, declaration2PK);
			filteredRows = GetFilteredRows(selectedColumn: "DeclarationPK", (Report_ITCustomsEntryPaymentParameters.ImporterPK, importer2PK));
			AssertContainsExactElementsInAnyOrder(new Guid[] { declaration2PK, declaration2PK }, filteredRows);
		}

		public void TestFilterByLocalClient()
		{
			var localClientPK = TestDataCreator.CreateOrganisation("LOCCLICODE", "LOCCLINAME");
			var localClientAddressPK = TestDataCreator.CreateAddress(localClientPK, "XXX", "ADDR");
			UpdateJobHeaderColumn("JH_OA_LocalChargesAddr", localClientAddressPK, SqlDbType.UniqueIdentifier, jobHeaderPK);

			var declaration2PK = TestDataCreator.CreateJobDeclaration(branchPK, companyPK, "B00002", "IMP", 2);
			var jobHeader2PK = TestDataCreator.CreateJobHeader(branchPK, companyPK, declaration2PK, departmentPK, "JE", "2", "2", "WRK");
			UpdateJobHeaderColumn("JH_OA_LocalChargesAddr", localClientAddressPK, SqlDbType.UniqueIdentifier, jobHeader2PK);
			var entryHeader2PK = TestDataCreator.CreateCusEntryHeader(true, "BLT", "ACO", "REG", "04321-B00002", 301, 1, "", declaration2PK, DateTime.Today, DateTime.Today, Guid.Empty, "", DateTime.Today, DateTime.Today, DateTime.Today, 2);
			TestDataCreator.CreateCusEntryPayInfo("", 0m, "", DateTime.Today, "", "", "", entryHeader2PK, DateTime.Today, 2);
			TestDataCreator.CreateJobCharge(branchPK, companyPK, departmentPK, jobHeader2PK, accChargeCode1PK, "1234", DateTime.Today);

			var filteredRows = GetFilteredRows(selectedColumn: "DeclarationPK");
			AssertContainsExactElementsInAnyOrder(new Guid[] { declarationPK, declarationPK, declaration2PK, declaration2PK }, filteredRows);

			filteredRows = GetFilteredRows(selectedColumn: "DeclarationPK", (Report_ITCustomsEntryPaymentParameters.LocalClientPK, localClientPK));
			AssertContainsExactElementsInAnyOrder(new Guid[] { declarationPK, declarationPK, declaration2PK, declaration2PK }, filteredRows);

			var localClient2PK = TestDataCreator.CreateOrganisation("LOCCLICODE2", "LOCCLINAME2");
			var localClientAddress2PK = TestDataCreator.CreateAddress(localClient2PK, "XXX", "ADDR");
			UpdateJobHeaderColumn("JH_OA_LocalChargesAddr", localClientAddress2PK, SqlDbType.UniqueIdentifier, jobHeader2PK);
			filteredRows = GetFilteredRows(selectedColumn: "DeclarationPK", (Report_ITCustomsEntryPaymentParameters.LocalClientPK, localClient2PK));
			AssertContainsExactElementsInAnyOrder(new Guid[] { declaration2PK, declaration2PK }, filteredRows);
		}

		public void TestJobHeaderJoinMaintainsDataGrain()
		{
			UpdateJobChargeColumn("JR_SellTaxDate", new DateTime(2019, 3, 19), SqlDbType.SmallDateTime, jobChargePK);

			var anotherCompanyPK = TestDataCreator.CreateCompany(companyCode: "~C2", countryCode: "SM", currencyCode: "EUR");
			var anotherBranchPK = TestDataCreator.CreateBranch(anotherCompanyPK, branchCode: "~B2", homePort: "SMSRV");
			TestDataCreator.CreateJobHeader(anotherBranchPK, anotherCompanyPK, declarationPK, departmentPK, "JE", "", "", "WRK");

			var filteredRows = GetRowsWithConcatenatedJobA93AndInvoiceNumbers();
			AssertContainsExactElementsInAnyOrder(
				"Should not duplicate rows as only the JobHeader matching the declaration Company is joined",
				new string[] { "B00001|78|#", "B00001|#|2019-03-19" },
				filteredRows);
		}

		public void TestCancelledDeclarationNotLoaded()
		{
			UpdateJobChargeColumn("JR_SellTaxDate", new DateTime(2020, 12, 25), SqlDbType.SmallDateTime, jobChargePK);

			var filteredRows = GetRowsWithConcatenatedJobA93AndInvoiceNumbers();
			AssertContainsExactElementsInAnyOrder("Should load declaration as it is not cancelled", new string[] { "B00001|78|#", "B00001|#|2020-12-25" }, filteredRows);

			UpdateDeclarationColumn("JE_IsCancelled", true, SqlDbType.Bit, declarationPK);

			filteredRows = GetRowsWithConcatenatedJobA93AndInvoiceNumbers();
			AssertContainsExactElementsInAnyOrder("Declaration is cancelled => should load no rows", Array.Empty<object>(), filteredRows);
		}

		object[] GetRowsWithConcatenatedJobA93AndInvoiceNumbers() => GetFilteredRows("JobNumber + '|' + isnull(A93Number,'#') + '|' + isnull(convert(varchar(10), InvoiceDate, 23),'#')");

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();

			companyPK = TestDataCreator.CreateCompany(companyCode: "ITA", countryCode: "IT", currencyCode: "EUR");
			branchPK = TestDataCreator.CreateBranch(companyPK, branchCode: "ITA", homePort: "ITALY");
			departmentPK = TestDataCreator.CreateDepartment("ITA");
			declarationPK = TestDataCreator.CreateJobDeclaration(branchPK, companyPK, "B00001", "IMP", 1);
			jobHeaderPK = TestDataCreator.CreateJobHeader(branchPK, companyPK, declarationPK, departmentPK, "JE", "", "", "WRK");
			entryHeaderPK = TestDataCreator.CreateCusEntryHeader(true, "BLT", "ACO", "REG", "04321-B00181816", 301, 1, "", declarationPK, DateTime.Today, DateTime.Today, Guid.Empty, "", DateTime.Today, DateTime.Today, DateTime.Today, 1);
			entryPayInfoPK = TestDataCreator.CreateCusEntryPayInfo("78", 0m, "", DateTime.Today, "", "A", "", entryHeaderPK, DateTime.Today, 1);

			accChargeCode1PK = TestDataCreator.CreateAccCharges(companyPK, "DIRITTI", "DIRITTI DOGANALI", "DIR", "CDS");
			jobChargePK = TestDataCreator.CreateJobCharge(branchPK, companyPK, departmentPK, jobHeaderPK, accChargeCode1PK, "5678", DateTime.Today);
		}

		Guid companyPK;
		Guid branchPK;
		Guid departmentPK;
		Guid declarationPK;
		Guid jobHeaderPK;
		Guid entryHeaderPK;
		Guid entryPayInfoPK;
		Guid accChargeCode1PK;
		Guid jobChargePK;

		#endregion
	}

	[TestedType(typeof(Report_ITCustomsEntryPayment))]
	class Report_ITCustomsEntryPayment_JobChargePartTest : Report_ITCustomsEntryPaymentBaseTest
	{
		#region Columns

		public void TestCustomsOffice()
		{
			UpdateDeclarationColumn("JE_CustomsOffice", "137100", SqlDbType.VarChar, declarationPK);
			AssertFunctionReturnExpectedValue("CustomsOffice", "137100");
		}

		public void TestJobNumber()
		{
			UpdateDeclarationColumn("JE_DeclarationReference", "XXYYZZ", SqlDbType.VarChar, declarationPK);
			AssertFunctionReturnExpectedValue("JobNumber", "XXYYZZ");
		}

		public void TestImporterCode()
		{
			AssertFunctionReturnExpectedValue("ImporterCode", DBNull.Value);

			var importerPK = TestDataCreator.CreateOrganisation("IMPCODE", "IMPNAME");
			UpdateDeclarationColumn("JE_OH_Importer", importerPK, SqlDbType.UniqueIdentifier, declarationPK);
			AssertFunctionReturnExpectedValue("ImporterCode", "IMPCODE");
		}

		public void TestImporterPK()
		{
			AssertFunctionReturnExpectedValue("ImporterPK", DBNull.Value);

			var importerPK = TestDataCreator.CreateOrganisation("IMPCODE", "IMPNAME");
			UpdateDeclarationColumn("JE_OH_Importer", importerPK, SqlDbType.UniqueIdentifier, declarationPK);
			AssertFunctionReturnExpectedValue("ImporterPK", importerPK);
		}

		public void TestLocalClientCode()
		{
			AssertFunctionReturnExpectedValue("LocalClientCode", DBNull.Value);

			var localClientPK = TestDataCreator.CreateOrganisation("LOCCLICODE", "LOCCLINAME");
			var localClientAddressPK = TestDataCreator.CreateAddress(localClientPK, "XXX", "ADDR");
			UpdateJobHeaderColumn("JH_OA_LocalChargesAddr", localClientAddressPK, SqlDbType.UniqueIdentifier, jobHeaderPK);
			AssertFunctionReturnExpectedValue("LocalClientCode", "LOCCLICODE");
		}

		public void TestLocalClientPK()
		{
			AssertFunctionReturnExpectedValue("LocalClientPK", DBNull.Value);

			var localClientPK = TestDataCreator.CreateOrganisation("LOCCLICODE", "LOCCLINAME");
			var localClientAddressPK = TestDataCreator.CreateAddress(localClientPK, "XXX", "ADDR");
			UpdateJobHeaderColumn("JH_OA_LocalChargesAddr", localClientAddressPK, SqlDbType.UniqueIdentifier, jobHeaderPK);
			AssertFunctionReturnExpectedValue("LocalClientPK", localClientPK);
		}

		public void TestA93MethodOfPayment()
		{
			AssertFunctionReturnExpectedValue("A93MethodOfPayment", DBNull.Value);
		}

		public void TestA93Amount()
		{
			AssertFunctionReturnExpectedValue("A93Amount", DBNull.Value);
		}

		public void TestA93Number()
		{
			AssertFunctionReturnExpectedValue("A93Number", DBNull.Value);
		}

		public void TestA93PaymentDate()
		{
			AssertFunctionReturnExpectedValue("A93PaymentDate", DBNull.Value);
		}

		public void TestDefermentAccountNumber()
		{
			AssertFunctionReturnExpectedValue("DefermentAccountNumber", DBNull.Value);
		}

		public void TestCustomsRegistrationNumber()
		{
			AssertFunctionReturnExpectedValue("CustomsRegistrationNumber", DBNull.Value);
		}

		public void TestEntryReleaseDate()
		{
			AssertFunctionReturnExpectedValue("EntryReleaseDate", DBNull.Value);
		}

		public void TestInvoiceNumber()
		{
			UpdateJobChargeColumn("JR_APInvoiceNum", "12345678", SqlDbType.VarChar, jobChargePK);
			AssertFunctionReturnExpectedValue("InvoiceNumber", "");

			var accTransHeaderPK = TestDataCreator.CreateAccTransactionHeader(branchPK, companyPK, departmentPK, jobHeaderPK, "AR", "INV", "B055", transactionNumber: "654321");
			var accTransLinePK = TestDataCreator.CreateAccTransactionLine(branchPK, companyPK, jobHeaderPK, accTransHeaderPK, taxPK: null, departmentPK, 1, "ACC LINE", "REV");
			UpdateJobChargeColumn("JR_AL_ARLine", accTransLinePK, SqlDbType.UniqueIdentifier, jobChargePK);
			AssertFunctionReturnExpectedValue("InvoiceNumber", "654321");

			// Check non-CDS charge is ignored
			var nonCdsAccChargeCodePK = TestDataCreator.CreateAccCharges(companyPK, "XXX", "XXX", "XXX", "XXX");
			TestDataCreator.CreateJobCharge(branchPK, companyPK, departmentPK, jobHeaderPK, nonCdsAccChargeCodePK, "", DateTime.Today);
			AssertFunctionReturnExpectedValue("InvoiceNumber", "654321");
		}

		public void TestInvoiceDate()
		{
			var today = DateTime.Today;
			var twoDaysAgo = today.AddDays(-2);

			UpdateJobChargeColumn("JR_APInvoiceDate", today, SqlDbType.SmallDateTime, jobChargePK);
			UpdateJobChargeColumn("JR_SellTaxDate", twoDaysAgo, SqlDbType.SmallDateTime, jobChargePK);
			AssertFunctionReturnExpectedValue("InvoiceDate", twoDaysAgo);

			// Check non-CDS charge is ignored
			var oneMonthFromToday = today.AddMonths(1);
			var nonCdsAccChargeCodePK = TestDataCreator.CreateAccCharges(companyPK, "XXX", "XXX", "XXX", "XXX");
			TestDataCreator.CreateJobCharge(branchPK, companyPK, departmentPK, jobHeaderPK, nonCdsAccChargeCodePK, "1234", oneMonthFromToday);
			AssertFunctionReturnExpectedValue("InvoiceDate", twoDaysAgo);
		}

		public void TestDebtorCode()
		{
			AssertFunctionReturnExpectedValue("DebtorCode", DBNull.Value);

			var debtorPK = TestDataCreator.CreateOrganisation("DEBCODE", "DEBNAME");
			UpdateJobChargeColumn("JR_OH_SellAccount", debtorPK, SqlDbType.UniqueIdentifier, jobChargePK);
			AssertFunctionReturnExpectedValue("DebtorCode", "DEBCODE");
		}

		public void TestChargedAmount()
		{
			UpdateJobChargeColumn("JR_LocalSellAmt", 123m, SqlDbType.Decimal, jobChargePK);
			AssertFunctionReturnExpectedValue("ChargedAmount", 123m);
		}

		#endregion

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();

			companyPK = TestDataCreator.CreateCompany(companyCode: "ITA", countryCode: "IT", currencyCode: "EUR");
			branchPK = TestDataCreator.CreateBranch(companyPK, branchCode: "ITA", homePort: "ITALY");
			departmentPK = TestDataCreator.CreateDepartment("ITA");

			declarationPK = TestDataCreator.CreateJobDeclaration(branchPK, companyPK, "B00001", "IMP", 1);
			jobHeaderPK = TestDataCreator.CreateJobHeader(branchPK, companyPK, declarationPK, departmentPK, "JE", "", "", "WRK");

			var accChargeCode1PK = TestDataCreator.CreateAccCharges(companyPK, "DIRITTI", "DIRITTI DOGANALI", "DIR", "CDS");
			jobChargePK = TestDataCreator.CreateJobCharge(branchPK, companyPK, departmentPK, jobHeaderPK, accChargeCode1PK, "5678", DateTime.Today);
		}

		Guid branchPK;
		Guid companyPK;
		Guid departmentPK;
		Guid declarationPK;
		Guid jobHeaderPK;
		Guid jobChargePK;

		#endregion
	}

	[TestedType(typeof(Report_ITCustomsEntryPayment))]
	class Report_ITCustomsEntryPayment_EntryPayInfoPartTest : Report_ITCustomsEntryPaymentBaseTest
	{
		#region Columns

		public void TestCustomsOffice()
		{
			UpdateDeclarationColumn("JE_CustomsOffice", "137100", SqlDbType.VarChar, declarationPK);
			AssertFunctionReturnExpectedValue("CustomsOffice", "137100");
		}

		public void TestJobNumber()
		{
			UpdateDeclarationColumn("JE_DeclarationReference", "XXYYZZ", SqlDbType.VarChar, declarationPK);
			AssertFunctionReturnExpectedValue("JobNumber", "XXYYZZ");
		}

		public void TestImporterCode()
		{
			AssertFunctionReturnExpectedValue("ImporterCode", DBNull.Value);

			var importerPK = TestDataCreator.CreateOrganisation("IMPCODE", "IMPNAME");
			UpdateDeclarationColumn("JE_OH_Importer", importerPK, SqlDbType.UniqueIdentifier, declarationPK);
			AssertFunctionReturnExpectedValue("ImporterCode", "IMPCODE");
		}

		public void TestImporterPK()
		{
			AssertFunctionReturnExpectedValue("ImporterPK", DBNull.Value);

			var importerPK = TestDataCreator.CreateOrganisation("IMPCODE", "IMPNAME");
			UpdateDeclarationColumn("JE_OH_Importer", importerPK, SqlDbType.UniqueIdentifier, declarationPK);
			AssertFunctionReturnExpectedValue("ImporterPK", importerPK);
		}

		public void TestLocalClientCode()
		{
			AssertFunctionReturnExpectedValue("LocalClientCode", DBNull.Value);

			var localClientPK = TestDataCreator.CreateOrganisation("LOCCLICODE", "LOCCLINAME");
			var localClientAddressPK = TestDataCreator.CreateAddress(localClientPK, "XXX", "ADDR");
			UpdateJobHeaderColumn("JH_OA_LocalChargesAddr", localClientAddressPK, SqlDbType.UniqueIdentifier, jobHeaderPK);
			AssertFunctionReturnExpectedValue("LocalClientCode", "LOCCLICODE");
		}

		public void TestLocalClientPK()
		{
			AssertFunctionReturnExpectedValue("LocalClientPK", DBNull.Value);

			var localClientPK = TestDataCreator.CreateOrganisation("LOCCLICODE", "LOCCLINAME");
			var localClientAddressPK = TestDataCreator.CreateAddress(localClientPK, "XXX", "ADDR");
			UpdateJobHeaderColumn("JH_OA_LocalChargesAddr", localClientAddressPK, SqlDbType.UniqueIdentifier, jobHeaderPK);
			AssertFunctionReturnExpectedValue("LocalClientPK", localClientPK);
		}

		public void TestLocalClientWhenJobHeaderIsLinkedToShipment()
		{
			AssertFunctionReturnExpectedValue("LocalClientCode", DBNull.Value);

			var shipmentPK = TestDataCreator.CreateJobShipment();
			UpdateJobHeaderColumn("JH_ParentTableCode", "JS", SqlDbType.VarChar, jobHeaderPK);
			UpdateJobHeaderColumn("JH_ParentID", shipmentPK, SqlDbType.UniqueIdentifier, jobHeaderPK);
			UpdateDeclarationColumn("JE_JS", shipmentPK, SqlDbType.UniqueIdentifier, declarationPK);

			var localClientPK = TestDataCreator.CreateOrganisation("LCLCLISHPCOD", "LCLCLISHPNAME");
			var orgAddressPK = TestDataCreator.CreateAddress(localClientPK, "AddressCode", "AddressDesc");
			UpdateJobHeaderColumn("JH_OA_LocalChargesAddr", orgAddressPK, SqlDbType.UniqueIdentifier, jobHeaderPK);

			AssertFunctionReturnExpectedValue("LocalClientCode", "LCLCLISHPCOD");
		}

		public void TestA93MethodOfPayment()
		{
			AssertFunctionReturnExpectedValue("A93MethodOfPayment", "");

			UpdateCusEntryPayInfoColumn("C9_PaymentParty", "F", SqlDbType.VarChar, entryPayInfoPK);
			AssertFunctionReturnExpectedValue("A93MethodOfPayment", "F");
		}

		public void TestA93Amount()
		{
			UpdateCusEntryPayInfoColumn("C9_PaymentAmount", 199m, SqlDbType.VarChar, entryPayInfoPK);
			AssertFunctionReturnExpectedValue("A93Amount", 199m);
		}

		public void TestA93Number()
		{
			AssertFunctionReturnExpectedValue("A93Number", "");

			UpdateCusEntryPayInfoColumn("C9_IncomingPayResponseNo", "000081", SqlDbType.VarChar, entryPayInfoPK);
			AssertFunctionReturnExpectedValue("A93Number", "000081");
		}

		public void TestA93PaymentDate()
		{
			UpdateCusEntryPayInfoColumn("C9_PaymentDate", new DateTime(2020, 01, 01), SqlDbType.SmallDateTime, entryPayInfoPK);
			AssertFunctionReturnExpectedValue("A93PaymentDate", new DateTime(2020, 01, 01));
		}

		public void TestDefermentAccountNumber()
		{
			UpdateDeclarationColumn("JE_DefermentAccountNumber", "123456", SqlDbType.VarChar, declarationPK);
			AssertFunctionReturnExpectedValue("DefermentAccountNumber", "123456");
		}

		public void TestCustomsRegistrationNumber()
		{
			AssertFunctionReturnExpectedValue("CustomsRegistrationNumber", DBNull.Value);

			TestDataCreator.CreateCusEntryNum(entryHeaderPK, "CusEntryHeader", "4 T-1000P", "REG", "CUS", "IT", DateTime.Today, "");
			AssertFunctionReturnExpectedValue("CustomsRegistrationNumber", "4 T-1000P");

			TestDataCreator.CreateCusEntryNum(entryHeaderPK, "CusEntryHeader", "4 T-1002S", "REG", "CUS", "IT", DateTime.Today.AddDays(1), "");
			AssertFunctionReturnExpectedValue("CustomsRegistrationNumber", "4 T-1002S");
		}

		public void TestEntryReleaseDate()
		{
			UpdateCusEntryHeaderColumn("CH_EntryReleaseDate", new DateTime(2020, 01, 01), SqlDbType.SmallDateTime, entryHeaderPK);
			AssertFunctionReturnExpectedValue("EntryReleaseDate", new DateTime(2020, 01, 01));
		}

		public void TestInvoiceNumber()
		{
			AssertFunctionReturnExpectedValue("InvoiceNumber", DBNull.Value);
		}

		public void TestInvoiceDate()
		{
			AssertFunctionReturnExpectedValue("InvoiceDate", DBNull.Value);
		}

		public void TestDebtorCode()
		{
			AssertFunctionReturnExpectedValue("DebtorCode", DBNull.Value);
		}

		public void TestChargedAmount()
		{
			AssertFunctionReturnExpectedValue("ChargedAmount", DBNull.Value);
		}

		#endregion

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();

			companyPK = TestDataCreator.CreateCompany(companyCode: "ITA", countryCode: "IT", currencyCode: "EUR");
			branchPK = TestDataCreator.CreateBranch(companyPK, branchCode: "ITA", homePort: "ITALY");
			departmentPK = TestDataCreator.CreateDepartment("ITA");
			declarationPK = TestDataCreator.CreateJobDeclaration(branchPK, companyPK, "B00001", "IMP", 1);
			jobHeaderPK = TestDataCreator.CreateJobHeader(branchPK, companyPK, declarationPK, departmentPK, "JE", "", "", "WRK");
			entryHeaderPK = TestDataCreator.CreateCusEntryHeader(true, "BLT", "ACO", "REG", "04321-B00181816", 301, 1, "", declarationPK, DateTime.Today, DateTime.Today, Guid.Empty, "", DateTime.Today, DateTime.Today, DateTime.Today, 1);
			entryPayInfoPK = TestDataCreator.CreateCusEntryPayInfo("", 0m, "", DateTime.Today, "", "", "", entryHeaderPK, DateTime.Today, 1);
		}
		Guid companyPK;
		Guid branchPK;
		Guid departmentPK;
		Guid declarationPK;
		Guid jobHeaderPK;
		Guid entryHeaderPK;
		Guid entryPayInfoPK;

		#endregion
	}

	#region Report_ITCustomsEntryPaymentBaseTest

	abstract class Report_ITCustomsEntryPaymentBaseTest : CustomsReportDbCreateScriptTest
	{
		protected void UpdateCusEntryHeaderColumn(string columnName, object columnValue, SqlDbType columnType, object primaryKeyValue)
		{
			UpdateTableColumn("CusEntryHeader", columnName, columnValue, columnType, "CH_PK", primaryKeyValue);
		}

		protected void UpdateCusEntryPayInfoColumn(string columnName, object columnValue, SqlDbType columnType, object primaryKeyValue)
		{
			UpdateTableColumn("CusEntryPayInfo", columnName, columnValue, columnType, "C9_PK", primaryKeyValue);
		}

		protected void UpdateJobChargeColumn(string columnName, object columnValue, SqlDbType columnType, object primaryKeyValue)
		{
			UpdateTableColumn("JobCharge", columnName, columnValue, columnType, "JR_PK", primaryKeyValue);
		}

		protected override IEnumerable<(SqlDbType ParameterType, string ParameterName)> GetSqlParameters()
		{
			yield return (SqlDbType.UniqueIdentifier, Report_ITCustomsEntryPaymentParameters.CompanyPK);
			yield return (SqlDbType.SmallDateTime, Report_ITCustomsEntryPaymentParameters.CustomsReleaseDateFrom);
			yield return (SqlDbType.SmallDateTime, Report_ITCustomsEntryPaymentParameters.CustomsReleaseDateTo);
			yield return (SqlDbType.SmallDateTime, Report_ITCustomsEntryPaymentParameters.PaymentDateFrom);
			yield return (SqlDbType.SmallDateTime, Report_ITCustomsEntryPaymentParameters.PaymentDateTo);
			yield return (SqlDbType.VarChar, Report_ITCustomsEntryPaymentParameters.DefermentAccountNumber);
			yield return (SqlDbType.VarChar, Report_ITCustomsEntryPaymentParameters.CustomsOffice);
			yield return (SqlDbType.VarChar, Report_ITCustomsEntryPaymentParameters.MethodOfPaymentList);
			yield return (SqlDbType.UniqueIdentifier, Report_ITCustomsEntryPaymentParameters.ImporterPK);
			yield return (SqlDbType.UniqueIdentifier, Report_ITCustomsEntryPaymentParameters.LocalClientPK);
		}

		public class Report_ITCustomsEntryPaymentParameters
		{
			public const string CompanyPK = "@CompanyPK";
			public const string CustomsReleaseDateFrom = "@CustomsReleaseDateFrom";
			public const string CustomsReleaseDateTo = "@CustomsReleaseDateTo";
			public const string PaymentDateFrom = "@PaymentDateFrom";
			public const string PaymentDateTo = "@PaymentDateTo";
			public const string DefermentAccountNumber = "@DefermentAccountNumber";
			public const string CustomsOffice = "@CustomsOffice";
			public const string MethodOfPaymentList = "@MethodOfPaymentList";
			public const string ImporterPK = "@ImporterPK";
			public const string LocalClientPK = "@LocalClientPK";
		}
	}

	#endregion
}
