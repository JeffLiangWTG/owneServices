using System;
using System.Collections.Generic;
using System.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.Customs.IT;
using Enterprise.Build.Database.Script.TestFramework;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Customs.IT.Testing
{
	[TestedType(typeof(Report_ITEntryPayInfo))]
	class Report_ITEntryPayInfo_CusEntryPayInfoTest : Report_ITEntryPayInfoBaseTest
	{
		#region Columns

		public void TestA93Number()
		{
			AssertFunctionReturnExpectedValue("A93Number", "");

			UpdateCusEntryPayInfoColumn("C9_IncomingPayResponseNo", "000081", SqlDbType.VarChar, entryPayInfoPK);
			AssertFunctionReturnExpectedValue("A93Number", "000081");
		}

		public void TestCustomsOffice()
		{
			AssertFunctionReturnExpectedValue("CustomsOffice", "");

			UpdateDeclarationColumn("JE_CustomsOffice", "IT137100", SqlDbType.VarChar, declarationPK);
			AssertFunctionReturnExpectedValue("CustomsOffice", "IT137100");
		}

		public void TestJobNumber()
		{
			AssertFunctionReturnExpectedValue("JobNumber", "");

			UpdateDeclarationColumn("JE_DeclarationReference", "B000001", SqlDbType.VarChar, declarationPK);
			AssertFunctionReturnExpectedValue("JobNumber", "B000001");
		}

		public void TestImporterFields()
		{
			AssertFunctionReturnExpectedValue("ImporterCode", DBNull.Value);
			AssertFunctionReturnExpectedValue("ImporterFullName", DBNull.Value);

			var importerPK = TestDataCreator.CreateOrganisation("IMPCODE", new string('0', 25));
			UpdateDeclarationColumn("JE_OH_Importer", importerPK, SqlDbType.UniqueIdentifier, declarationPK);
			AssertFunctionReturnExpectedValue("ImporterCode", "IMPCODE");
			AssertFunctionReturnExpectedValue("ImporterFullName", new string('0', 20));
		}

		public void TestLocalClientFieldsWhenJobHeaderAppliesToDeclaration()
		{
			AssertFunctionReturnExpectedValue("LocalClientCode", DBNull.Value);
			AssertFunctionReturnExpectedValue("LocalClientFullName", DBNull.Value);

			var localClientPK = TestDataCreator.CreateOrganisation("LOCCLICODE", new string('0', 25));
			var localClientAddressPK = TestDataCreator.CreateAddress(localClientPK, "XXX", "ADDR");
			UpdateJobHeaderColumn("JH_OA_LocalChargesAddr", localClientAddressPK, SqlDbType.UniqueIdentifier, jobHeaderPK);
			AssertFunctionReturnExpectedValue("LocalClientCode", "LOCCLICODE");
			AssertFunctionReturnExpectedValue("LocalClientFullName", new string('0', 20));
		}

		public void TestLocalClientFieldsWhenJobHeaderAppliesToShipment()
		{
			var shipmentPK = TestDataCreator.CreateJobShipment();
			UpdateDeclarationColumn("JE_JS", shipmentPK, SqlDbType.UniqueIdentifier, declarationPK);
			UpdateJobHeaderColumn("JH_ParentID", shipmentPK, SqlDbType.UniqueIdentifier, jobHeaderPK);
			UpdateJobHeaderColumn("JH_ParentTableCode", "JS", SqlDbType.VarChar, jobHeaderPK);

			AssertFunctionReturnExpectedValue("LocalClientCode", DBNull.Value);
			AssertFunctionReturnExpectedValue("LocalClientFullName", DBNull.Value);

			var localClientPK = TestDataCreator.CreateOrganisation("LOCCLICODE", new string('0', 25));
			var localClientAddressPK = TestDataCreator.CreateAddress(localClientPK, "XXX", "ADDR");
			UpdateJobHeaderColumn("JH_OA_LocalChargesAddr", localClientAddressPK, SqlDbType.UniqueIdentifier, jobHeaderPK);
			AssertFunctionReturnExpectedValue("LocalClientCode", "LOCCLICODE");
			AssertFunctionReturnExpectedValue("LocalClientFullName", new string('0', 20));
		}

		public void TestA93MethodOfPayment()
		{
			AssertFunctionReturnExpectedValue("A93MethodOfPayment", "");

			UpdateCusEntryPayInfoColumn("C9_PaymentParty", "F", SqlDbType.VarChar, entryPayInfoPK);
			AssertFunctionReturnExpectedValue("A93MethodOfPayment", "F");
		}

		public void TestA93Amount()
		{
			AssertFunctionReturnExpectedValue("A93Amount", 0m);

			UpdateCusEntryPayInfoColumn("C9_PaymentAmount", 199m, SqlDbType.Decimal, entryPayInfoPK);
			AssertFunctionReturnExpectedValue("A93Amount", 199m);
		}

		public void TestA93PaymentDate()
		{
			AssertFunctionReturnExpectedValue("A93PaymentDate", DBNull.Value);

			UpdateCusEntryPayInfoColumn("C9_PaymentDate", new DateTime(2020, 01, 01), SqlDbType.SmallDateTime, entryPayInfoPK);
			AssertFunctionReturnExpectedValue("A93PaymentDate", new DateTime(2020, 01, 01));
		}

		public void TestCustomsRegistrationNumber()
		{
			AssertFunctionReturnExpectedValue("CustomsRegistrationNumber", DBNull.Value);

			TestDataCreator.CreateCusEntryNum(entryHeaderPK, "CusEntryHeader", "22ITQXT080002689T3", "MRN", "CUS", "IT", DateTime.Today, "");
			TestDataCreator.CreateCusEntryNum(entryHeaderPK, "CusEntryHeader", "4 T-1000P", "REG", "CUS", "IT", DateTime.Today, "");
			TestDataCreator.CreateCusEntryNum(entryHeaderPK, "CusEntryHeader", "1 - 1234A", "REG", "CUS", "AU", DateTime.Today.AddDays(1), "");
			AssertFunctionReturnExpectedValue("CustomsRegistrationNumber", "4 T-1000P");

			TestDataCreator.CreateCusEntryNum(entryHeaderPK, "CusEntryHeader", "4 T-1002S", "REG", "CUS", "IT", DateTime.Today.AddDays(1), "");
			AssertFunctionReturnExpectedValue("CustomsRegistrationNumber", "4 T-1002S");
		}

		public void TestCustomsRegistrationDate()
		{
			AssertFunctionReturnExpectedValue("CustomsRegistrationDate", DBNull.Value);

			TestDataCreator.CreateCusEntryNum(entryHeaderPK, "CusEntryHeader", "4 T-1000P", "REG", "CUS", "IT", new DateTime(2020, 01, 01), "");
			AssertFunctionReturnExpectedValue("CustomsRegistrationDate", new DateTime(2020, 01, 01));
		}

		#endregion

		#region Filters

		public void TestFilterByIsCancelled()
		{
			UpdateDeclarationColumn("JE_DeclarationReference", "B000001", SqlDbType.VarChar, declarationPK);
			UpdateDeclarationColumn("JE_IsCancelled", false, SqlDbType.Bit, declarationPK);

			var declaration2PK = TestDataCreator.CreateJobDeclaration(branchPK, companyPK, "B000002", "IMP", 2);
			UpdateDeclarationColumn("JE_IsCancelled", true, SqlDbType.Bit, declaration2PK);
			TestDataCreator.CreateJobHeader(branchPK, companyPK, declaration2PK, departmentPK, "JE", "2", "2", "WRK");
			var entryHeader2PK = TestDataCreator.CreateCusEntryHeader(true, "", "", "", "", 0f, 0, "", declaration2PK, DateTime.Today, DateTime.Today, Guid.Empty, "", DateTime.Today, DateTime.Today, DateTime.Today, 2);
			TestDataCreator.CreateCusEntryPayInfo("", 0m, "", DateTime.Today, "", "", "", entryHeader2PK, DateTime.Today, 2);

			var filteredRows = GetFilteredRows(selectedColumn: "JobNumber");
			AssertContainsExactElementsInAnyOrder(new string[] { "B000001" }, filteredRows);
		}

		public void TestFilterByCompanyPK()
		{
			UpdateDeclarationColumn("JE_DeclarationReference", "B000001", SqlDbType.VarChar, declarationPK);

			var company2PK = TestDataCreator.CreateCompany(companyCode: "IT2", countryCode: "IT", currencyCode: "EUR");
			var branch2PK = TestDataCreator.CreateBranch(company2PK, branchCode: "IT2", homePort: "ITALY");
			var declaration2PK = TestDataCreator.CreateJobDeclaration(branch2PK, company2PK, "B000002", "IMP", 2);
			TestDataCreator.CreateJobHeader(branch2PK, company2PK, declaration2PK, departmentPK, "JE", "", "", "WRK");
			var entryHeader2PK = TestDataCreator.CreateCusEntryHeader(true, "", "", "", "", 0f, 0, "", declaration2PK, DateTime.Today, null, Guid.Empty, "", DateTime.Today, DateTime.Today, DateTime.Today, 2);
			TestDataCreator.CreateCusEntryPayInfo("", 0m, "", null, "", "", "", entryHeader2PK, DateTime.Today, 2);

			var filteredRows = GetFilteredRows(selectedColumn: "JobNumber");
			AssertContainsExactElementsInAnyOrder(new string[] { "B000001" }, filteredRows);
		}

		public void TestFilterByCustomsRegistrationDateRange()
		{
			UpdateDeclarationColumn("JE_DeclarationReference", "B000001", SqlDbType.VarChar, declarationPK);
			TestDataCreator.CreateCusEntryNum(entryHeaderPK, "CusEntryHeader", "4 T-1000P", "REG", "CUS", "IT", new DateTime(2020, 01, 01), "");

			var declaration2PK = TestDataCreator.CreateJobDeclaration(branchPK, companyPK, "B000002", "IMP", 2);
			TestDataCreator.CreateJobHeader(branchPK, companyPK, declaration2PK, departmentPK, "JE", "1", "", "WRK");
			var entryHeader2PK = TestDataCreator.CreateCusEntryHeader(true, "", "", "", "", 0f, 0, "", declaration2PK, DateTime.Today, new DateTime(2020, 01, 10), Guid.Empty, "", DateTime.Today, DateTime.Today, DateTime.Today, 2);
			TestDataCreator.CreateCusEntryPayInfo("81", 200m, "", DateTime.Today, "", "F", "", entryHeader2PK, DateTime.Today, 2);
			TestDataCreator.CreateCusEntryNum(entryHeader2PK, "CusEntryHeader", "8-9999X", "REG", "CUS", "IT", new DateTime(2020, 01, 10), "");

			var declaration3PK = TestDataCreator.CreateJobDeclaration(branchPK, companyPK, "B000003", "IMP", 3);
			TestDataCreator.CreateJobHeader(branchPK, companyPK, declaration3PK, departmentPK, "JE", "2", "", "WRK");
			var entryHeader3PK = TestDataCreator.CreateCusEntryHeader(true, "", "", "", "", 0f, 0, "", declaration3PK, DateTime.Today, new DateTime(2020, 01, 10), Guid.Empty, "", DateTime.Today, DateTime.Today, DateTime.Today, 3);
			TestDataCreator.CreateCusEntryPayInfo("99", 200m, "", DateTime.Today, "", "F", "", entryHeader3PK, DateTime.Today, 3);

			var filteredRows = GetFilteredRows(selectedColumn: "JobNumber");
			AssertContainsExactElementsInAnyOrder(new string[] { "B000001", "B000002", "B000003" }, filteredRows);

			filteredRows = GetFilteredRows(selectedColumn: "JobNumber", (Report_ITEntryPayInfoParameters.CustomsRegistrationDateFrom, new DateTime(2020, 01, 01)));
			AssertContainsExactElementsInAnyOrder(new string[] { "B000001", "B000002" }, filteredRows);

			filteredRows = GetFilteredRows(selectedColumn: "JobNumber", (Report_ITEntryPayInfoParameters.CustomsRegistrationDateFrom, new DateTime(2020, 01, 01)), (Report_ITEntryPayInfoParameters.CustomsRegistrationDateTo, new DateTime(2020, 01, 05)));
			AssertContainsExactElementsInAnyOrder(new string[] { "B000001" }, filteredRows);

			filteredRows = GetFilteredRows(selectedColumn: "JobNumber", (Report_ITEntryPayInfoParameters.CustomsRegistrationDateFrom, new DateTime(2020, 01, 05)), (Report_ITEntryPayInfoParameters.CustomsRegistrationDateTo, new DateTime(2020, 01, 06)));
			AssertContainsExactElementsInAnyOrder(Array.Empty<string>(), filteredRows);

			filteredRows = GetFilteredRows(selectedColumn: "JobNumber", (Report_ITEntryPayInfoParameters.CustomsRegistrationDateFrom, new DateTime(2020, 01, 05)));
			AssertContainsExactElementsInAnyOrder(new string[] { "B000002" }, filteredRows);

			filteredRows = GetFilteredRows(selectedColumn: "JobNumber", (Report_ITEntryPayInfoParameters.CustomsRegistrationDateTo, new DateTime(2020, 01, 04)));
			AssertContainsExactElementsInAnyOrder(new string[] { "B000001" }, filteredRows);
		}

		public void TestFilterByPaymentDateRange()
		{
			UpdateDeclarationColumn("JE_DeclarationReference", "B000001", SqlDbType.VarChar, declarationPK);
			UpdateCusEntryPayInfoColumn("C9_PaymentDate", new DateTime(2020, 01, 01), SqlDbType.SmallDateTime, entryPayInfoPK);

			var declaration2PK = TestDataCreator.CreateJobDeclaration(branchPK, companyPK, "B000002", "IMP", 2);
			TestDataCreator.CreateJobHeader(branchPK, companyPK, declaration2PK, departmentPK, "JE", "1", "", "WRK");
			var entryHeader2PK = TestDataCreator.CreateCusEntryHeader(true, "", "", "", "", 0f, 0, "", declaration2PK, DateTime.Today, DateTime.Today, Guid.Empty, "", DateTime.Today, DateTime.Today, DateTime.Today, 2);
			TestDataCreator.CreateCusEntryPayInfo("81", 200m, "", new DateTime(2020, 01, 10), "", "F", "", entryHeader2PK, DateTime.Today, 2);

			var filteredRows = GetFilteredRows(selectedColumn: "JobNumber");
			AssertContainsExactElementsInAnyOrder(new string[] { "B000001", "B000002" }, filteredRows);

			filteredRows = GetFilteredRows(selectedColumn: "JobNumber", (Report_ITEntryPayInfoParameters.PaymentDateFrom, new DateTime(2020, 01, 01)));
			AssertContainsExactElementsInAnyOrder(new string[] { "B000001", "B000002" }, filteredRows);

			filteredRows = GetFilteredRows(selectedColumn: "JobNumber", (Report_ITEntryPayInfoParameters.PaymentDateFrom, new DateTime(2020, 01, 01)), (Report_ITEntryPayInfoParameters.PaymentDateTo, new DateTime(2020, 01, 05)));
			AssertContainsExactElementsInAnyOrder(new string[] { "B000001" }, filteredRows);

			filteredRows = GetFilteredRows(selectedColumn: "JobNumber", (Report_ITEntryPayInfoParameters.PaymentDateFrom, new DateTime(2020, 01, 05)), (Report_ITEntryPayInfoParameters.PaymentDateTo, new DateTime(2020, 01, 06)));
			AssertContainsExactElementsInAnyOrder(Array.Empty<string>(), filteredRows);

			filteredRows = GetFilteredRows(selectedColumn: "JobNumber", (Report_ITEntryPayInfoParameters.PaymentDateFrom, new DateTime(2020, 01, 05)));
			AssertContainsExactElementsInAnyOrder(new string[] { "B000002" }, filteredRows);

			filteredRows = GetFilteredRows(selectedColumn: "JobNumber", (Report_ITEntryPayInfoParameters.PaymentDateTo, new DateTime(2020, 01, 04)));
			AssertContainsExactElementsInAnyOrder(new string[] { "B000001" }, filteredRows);
		}

		public void TestFilterByDefermentAccountNumber()
		{
			UpdateDeclarationColumn("JE_DeclarationReference", "B000001", SqlDbType.VarChar, declarationPK);
			UpdateDeclarationColumn("JE_DefermentAccountNumber", "1234", SqlDbType.VarChar, declarationPK);

			var declaration2PK = TestDataCreator.CreateJobDeclaration(branchPK, companyPK, "B000002", "IMP", 2);
			UpdateDeclarationColumn("JE_DefermentAccountNumber", "1234", SqlDbType.VarChar, declaration2PK);
			TestDataCreator.CreateJobHeader(branchPK, companyPK, declaration2PK, departmentPK, "JE", "2", "2", "WRK");
			var entryHeader2PK = TestDataCreator.CreateCusEntryHeader(true, "", "", "", "", 0f, 0, "", declaration2PK, DateTime.Today, DateTime.Today, Guid.Empty, "", DateTime.Today, DateTime.Today, DateTime.Today, 2);
			TestDataCreator.CreateCusEntryPayInfo("", 0m, "", DateTime.Today, "", "", "", entryHeader2PK, DateTime.Today, 2);

			var filteredRows = GetFilteredRows(selectedColumn: "JobNumber");
			AssertContainsExactElementsInAnyOrder(new string[] { "B000001", "B000002" }, filteredRows);

			filteredRows = GetFilteredRows(selectedColumn: "JobNumber", (Report_ITEntryPayInfoParameters.DefermentAccountNumber, "1234"));
			AssertContainsExactElementsInAnyOrder(new string[] { "B000001", "B000002" }, filteredRows);

			UpdateDeclarationColumn("JE_DefermentAccountNumber", "5678", SqlDbType.VarChar, declaration2PK);
			filteredRows = GetFilteredRows(selectedColumn: "JobNumber", (Report_ITEntryPayInfoParameters.DefermentAccountNumber, "5678"));
			AssertContainsExactElementsInAnyOrder(new string[] { "B000002" }, filteredRows);
		}

		public void TestFilterByCustomsOffice()
		{
			UpdateDeclarationColumn("JE_DeclarationReference", "B000001", SqlDbType.VarChar, declarationPK);
			UpdateDeclarationColumn("JE_CustomsOffice", "1234", SqlDbType.VarChar, declarationPK);

			var declaration2PK = TestDataCreator.CreateJobDeclaration(branchPK, companyPK, "B000002", "IMP", 2);
			UpdateDeclarationColumn("JE_CustomsOffice", "1234", SqlDbType.VarChar, declaration2PK);
			TestDataCreator.CreateJobHeader(branchPK, companyPK, declaration2PK, departmentPK, "JE", "2", "2", "WRK");
			var entryHeader2PK = TestDataCreator.CreateCusEntryHeader(true, "", "", "", "", 0f, 0, "", declaration2PK, DateTime.Today, DateTime.Today, Guid.Empty, "", DateTime.Today, DateTime.Today, DateTime.Today, 2);
			TestDataCreator.CreateCusEntryPayInfo("", 0m, "", DateTime.Today, "", "", "", entryHeader2PK, DateTime.Today, 2);

			var filteredRows = GetFilteredRows(selectedColumn: "JobNumber");
			AssertContainsExactElementsInAnyOrder(new string[] { "B000001", "B000002" }, filteredRows);

			filteredRows = GetFilteredRows(selectedColumn: "JobNumber", (Report_ITEntryPayInfoParameters.CustomsOffice, "1234"));
			AssertContainsExactElementsInAnyOrder(new string[] { "B000001", "B000002" }, filteredRows);

			UpdateDeclarationColumn("JE_CustomsOffice", "5678", SqlDbType.VarChar, declaration2PK);
			filteredRows = GetFilteredRows(selectedColumn: "JobNumber", (Report_ITEntryPayInfoParameters.CustomsOffice, "5678"));
			AssertContainsExactElementsInAnyOrder(new string[] { "B000002" }, filteredRows);
		}

		#endregion

		protected override void SetUp()
		{
			base.SetUp();

			declarationPK = TestDataCreator.CreateJobDeclaration(branchPK, companyPK, "", "IMP", 1);
			jobHeaderPK = TestDataCreator.CreateJobHeader(branchPK, companyPK, declarationPK, departmentPK, "JE", "", "", "WRK");
			entryHeaderPK = TestDataCreator.CreateCusEntryHeader(true, "BLT", "ACO", "REG", "04321-B00181816", 301, 1, "", declarationPK, DateTime.Today, null, Guid.Empty, "", DateTime.Today, DateTime.Today, DateTime.Today, 1);
			entryPayInfoPK = TestDataCreator.CreateCusEntryPayInfo("", 0m, "", null, "", "", "", entryHeaderPK, DateTime.Today, 1);
		}

		Guid declarationPK;
		Guid jobHeaderPK;
		Guid entryHeaderPK;
		Guid entryPayInfoPK;

		void UpdateCusEntryPayInfoColumn(string columnName, object columnValue, SqlDbType columnType, object primaryKeyValue)
		{
			UpdateTableColumn("CusEntryPayInfo", columnName, columnValue, columnType, "C9_PK", primaryKeyValue);
		}
	}

	[TestedType(typeof(Report_ITEntryPayInfo))]
	class Report_ITEntryPayInfo_CusInBondPayInfoTest : Report_ITEntryPayInfoBaseTest
	{
		#region Columns

		public void TestA93Number()
		{
			AssertFunctionReturnExpectedValue("A93Number", "");

			UpdateCusInBondPayInfoColumn("BPI_IncomingPayResponseNo", "000081", SqlDbType.VarChar, inBondPayInfoPK);
			AssertFunctionReturnExpectedValue("A93Number", "000081");
		}

		public void TestCustomsOffice()
		{
			AssertFunctionReturnExpectedValue("CustomsOffice", DBNull.Value);

			TestDataCreator.CreateCusCodeData("EUO", "DEP", "IT137100", nctsHeaderPK, "BH");
			AssertFunctionReturnExpectedValue("CustomsOffice", "IT137100");

			TestDataCreator.CreateCusCodeData("EUO", "DEP", "IT999999", nctsHeaderPK, "BH");
			AssertFunctionReturnExpectedValue("CustomsOffice", "IT999999");
		}

		public void TestCustomsOfficeOnMovementHeader()
		{
			AssertFunctionReturnExpectedValue("CustomsOffice", DBNull.Value);

			TestDataCreator.CreateCusCodeData("EUO", "DEP", "IT137100", nctsDepartureMovementHeaderPK, "BM");
			AssertFunctionReturnExpectedValue("CustomsOffice", "IT137100");

			TestDataCreator.CreateCusCodeData("EUO", "DEP", "IT999999", nctsDepartureMovementHeaderPK, "BM");
			AssertFunctionReturnExpectedValue("CustomsOffice", "IT999999");
		}

		public void TestJobNumber()
		{
			AssertFunctionReturnExpectedValue("JobNumber", "");

			UpdateCusInBondHeaderColumn("BH_JobReference", "NCT0000001", SqlDbType.VarChar, nctsHeaderPK);
			AssertFunctionReturnExpectedValue("JobNumber", "NCT0000001");
		}

		public void TestImporterFields()
		{
			AssertFunctionReturnExpectedValue("ImporterCode", DBNull.Value);
			AssertFunctionReturnExpectedValue("ImporterFullName", DBNull.Value);
		}

		public void TestLocalClientFieldsWhenJobHeaderAppliesToHeader()
		{
			AssertFunctionReturnExpectedValue("LocalClientCode", DBNull.Value);
			AssertFunctionReturnExpectedValue("LocalClientFullName", DBNull.Value);

			var localClientPK = TestDataCreator.CreateOrganisation("LOCCLICODE", new string('0', 25));
			var localClientAddressPK = TestDataCreator.CreateAddress(localClientPK, "XXX", "ADDR");
			UpdateJobHeaderColumn("JH_OA_LocalChargesAddr", localClientAddressPK, SqlDbType.UniqueIdentifier, jobHeaderPK);
			AssertFunctionReturnExpectedValue("LocalClientCode", "LOCCLICODE");
			AssertFunctionReturnExpectedValue("LocalClientFullName", new string('0', 20));
		}

		public void TestLocalClientFieldsWhenJobHeaderAppliesToShipment()
		{
			var shipmentPK = TestDataCreator.CreateJobShipment();
			UpdateCusInBondHeaderColumn("BH_ParentID", shipmentPK, SqlDbType.UniqueIdentifier, nctsHeaderPK);
			UpdateJobHeaderColumn("JH_ParentID", shipmentPK, SqlDbType.UniqueIdentifier, jobHeaderPK);
			UpdateJobHeaderColumn("JH_ParentTableCode", "JS", SqlDbType.VarChar, jobHeaderPK);

			AssertFunctionReturnExpectedValue("LocalClientCode", DBNull.Value);
			AssertFunctionReturnExpectedValue("LocalClientFullName", DBNull.Value);

			var localClientPK = TestDataCreator.CreateOrganisation("LOCCLICODE", new string('0', 25));
			var localClientAddressPK = TestDataCreator.CreateAddress(localClientPK, "XXX", "ADDR");
			UpdateJobHeaderColumn("JH_OA_LocalChargesAddr", localClientAddressPK, SqlDbType.UniqueIdentifier, jobHeaderPK);
			AssertFunctionReturnExpectedValue("LocalClientCode", "LOCCLICODE");
			AssertFunctionReturnExpectedValue("LocalClientFullName", new string('0', 20));
		}

		public void TestA93MethodOfPayment()
		{
			AssertFunctionReturnExpectedValue("A93MethodOfPayment", "");

			UpdateCusInBondPayInfoColumn("BPI_MethodOfPayment", "F", SqlDbType.VarChar, inBondPayInfoPK);
			AssertFunctionReturnExpectedValue("A93MethodOfPayment", "F");
		}

		public void TestA93Amount()
		{
			AssertFunctionReturnExpectedValue("A93Amount", 0m);

			UpdateCusInBondPayInfoColumn("BPI_PaymentAmount", 199m, SqlDbType.Decimal, inBondPayInfoPK);
			AssertFunctionReturnExpectedValue("A93Amount", 199m);
		}

		public void TestA93PaymentDate()
		{
			AssertFunctionReturnExpectedValue("A93PaymentDate", DBNull.Value);

			UpdateCusInBondPayInfoColumn("BPI_PaymentDate", new DateTime(2020, 01, 01), SqlDbType.SmallDateTime, inBondPayInfoPK);
			AssertFunctionReturnExpectedValue("A93PaymentDate", new DateTime(2020, 01, 01));
		}

		public void TestCustomsRegistrationNumber()
		{
			AssertFunctionReturnExpectedValue("CustomsRegistrationNumber", DBNull.Value);

			TestDataCreator.CreateCusEntryNum(nctsHeaderPK, "CusInBondHeader", "22ITQXT080002689T3", "MRN", "CUS", "IT", DateTime.Today, "");
			TestDataCreator.CreateCusEntryNum(nctsHeaderPK, "CusInBondHeader", "4 T-1000P", "REG", "CUS", "IT", DateTime.Today, "");
			TestDataCreator.CreateCusEntryNum(nctsHeaderPK, "CusInBondHeader", "1 - 1234A", "REG", "CUS", "AU", DateTime.Today.AddDays(1), "");
			AssertFunctionReturnExpectedValue("CustomsRegistrationNumber", "4 T-1000P");

			TestDataCreator.CreateCusEntryNum(nctsHeaderPK, "CusInBondHeader", "4 T-1002S", "REG", "CUS", "IT", DateTime.Today.AddDays(1), "");
			AssertFunctionReturnExpectedValue("CustomsRegistrationNumber", "4 T-1002S");
		}

		public void TestCustomsRegistrationDate()
		{
			AssertFunctionReturnExpectedValue("CustomsRegistrationDate", DBNull.Value);

			TestDataCreator.CreateCusEntryNum(nctsHeaderPK, "CusInBondHeader", "4 T-1000P", "REG", "CUS", "IT", new DateTime(2020, 01, 01), "");
			AssertFunctionReturnExpectedValue("CustomsRegistrationDate", new DateTime(2020, 01, 01));
		}

		#endregion

		#region Filters

		public void TestFilterByIsCancelled()
		{
			UpdateCusInBondHeaderColumn("BH_JobReference", "NCT0000001", SqlDbType.VarChar, nctsHeaderPK);
			UpdateCusInBondHeaderColumn("BH_IsActive", true, SqlDbType.Bit, nctsHeaderPK);

			var nctsHeader2PK = TestDataCreator.CreateCusInbondHeader("NCT0000002", branchPK, "NCT");
			UpdateCusInBondHeaderColumn("BH_IsActive", false, SqlDbType.Bit, nctsHeader2PK);
			var nctsDepartureMovementHeader2PK = TestDataCreator.CreateCusInBondMoveHeader(nctsHeader2PK);
			TestDataCreator.CreateJobHeader(branchPK, companyPK, nctsHeader2PK, departmentPK, "BH", "2", "2", "WRK");
			TestDataCreator.CreateCusInBondPayInfo(nctsDepartureMovementHeader2PK, "", "", 0m, null, "", "");

			var filteredRows = GetFilteredRows(selectedColumn: "JobNumber");
			AssertContainsExactElementsInAnyOrder(new string[] { "NCT0000001" }, filteredRows);
		}

		public void TestFilterByCompanyPK()
		{
			UpdateCusInBondHeaderColumn("BH_JobReference", "NCT0000001", SqlDbType.VarChar, nctsHeaderPK);

			var company2PK = TestDataCreator.CreateCompany(companyCode: "IT2", countryCode: "IT", currencyCode: "EUR");
			var branch2PK = TestDataCreator.CreateBranch(company2PK, branchCode: "IT2", homePort: "ITALY");
			var nctsHeader2PK = TestDataCreator.CreateCusInbondHeader("NCT0000002", branch2PK, "NCT");
			var nctsDepartureMovementHeader2PK = TestDataCreator.CreateCusInBondMoveHeader(nctsHeader2PK);
			TestDataCreator.CreateJobHeader(branch2PK, company2PK, nctsHeader2PK, departmentPK, "BH", "", "", "WRK");
			TestDataCreator.CreateCusInBondPayInfo(nctsDepartureMovementHeader2PK, "", "", 0m, null, "", "");

			var filteredRows = GetFilteredRows(selectedColumn: "JobNumber");
			AssertContainsExactElementsInAnyOrder(new string[] { "NCT0000001" }, filteredRows);
		}

		public void TestFilterByCustomsRegistrationDateRange()
		{
			UpdateCusInBondHeaderColumn("BH_JobReference", "NCT0000001", SqlDbType.VarChar, nctsHeaderPK);
			TestDataCreator.CreateCusEntryNum(nctsHeaderPK, "CusInBondHeader", "4 T-1000P", "REG", "CUS", "IT", new DateTime(2020, 01, 01), "");

			var nctsHeader2PK = TestDataCreator.CreateCusInbondHeader("NCT0000002", branchPK, "NCT");
			var nctsDepartureMovementHeader2PK = TestDataCreator.CreateCusInBondMoveHeader(nctsHeader2PK);
			TestDataCreator.CreateJobHeader(branchPK, companyPK, nctsHeader2PK, departmentPK, "BH", "1", "", "WRK");
			TestDataCreator.CreateCusInBondPayInfo(nctsDepartureMovementHeader2PK, "", "", 0m, null, "", "");
			TestDataCreator.CreateCusEntryNum(nctsHeader2PK, "CusInBondHeader", "8-9999X", "REG", "CUS", "IT", new DateTime(2020, 01, 10), "");

			var filteredRows = GetFilteredRows(selectedColumn: "JobNumber");
			AssertContainsExactElementsInAnyOrder(new string[] { "NCT0000001", "NCT0000002" }, filteredRows);

			filteredRows = GetFilteredRows(selectedColumn: "JobNumber", (Report_ITEntryPayInfoParameters.CustomsRegistrationDateFrom, new DateTime(2020, 01, 01)));
			AssertContainsExactElementsInAnyOrder(new string[] { "NCT0000001", "NCT0000002" }, filteredRows);

			filteredRows = GetFilteredRows(selectedColumn: "JobNumber", (Report_ITEntryPayInfoParameters.CustomsRegistrationDateFrom, new DateTime(2020, 01, 01)), (Report_ITEntryPayInfoParameters.CustomsRegistrationDateTo, new DateTime(2020, 01, 05)));
			AssertContainsExactElementsInAnyOrder(new string[] { "NCT0000001" }, filteredRows);

			filteredRows = GetFilteredRows(selectedColumn: "JobNumber", (Report_ITEntryPayInfoParameters.CustomsRegistrationDateFrom, new DateTime(2020, 01, 05)), (Report_ITEntryPayInfoParameters.CustomsRegistrationDateTo, new DateTime(2020, 01, 06)));
			AssertContainsExactElementsInAnyOrder(Array.Empty<string>(), filteredRows);

			filteredRows = GetFilteredRows(selectedColumn: "JobNumber", (Report_ITEntryPayInfoParameters.CustomsRegistrationDateFrom, new DateTime(2020, 01, 05)));
			AssertContainsExactElementsInAnyOrder(new string[] { "NCT0000002" }, filteredRows);

			filteredRows = GetFilteredRows(selectedColumn: "JobNumber", (Report_ITEntryPayInfoParameters.CustomsRegistrationDateTo, new DateTime(2020, 01, 04)));
			AssertContainsExactElementsInAnyOrder(new string[] { "NCT0000001" }, filteredRows);
		}

		public void TestFilterByPaymentDateRange()
		{
			UpdateCusInBondHeaderColumn("BH_JobReference", "NCT0000001", SqlDbType.VarChar, nctsHeaderPK);
			UpdateCusInBondPayInfoColumn("BPI_PaymentDate", new DateTime(2020, 01, 01), SqlDbType.SmallDateTime, inBondPayInfoPK);

			var nctsHeader2PK = TestDataCreator.CreateCusInbondHeader("NCT0000002", branchPK, "NCT");
			var nctsDepartureMovementHeader2PK = TestDataCreator.CreateCusInBondMoveHeader(nctsHeader2PK);
			TestDataCreator.CreateJobHeader(branchPK, companyPK, nctsHeader2PK, departmentPK, "BH", "1", "", "WRK");
			TestDataCreator.CreateCusInBondPayInfo(nctsDepartureMovementHeader2PK, "", "", 0m, new DateTime(2020, 01, 10), "", "");

			var filteredRows = GetFilteredRows(selectedColumn: "JobNumber");
			AssertContainsExactElementsInAnyOrder(new string[] { "NCT0000001", "NCT0000002" }, filteredRows);

			filteredRows = GetFilteredRows(selectedColumn: "JobNumber", (Report_ITEntryPayInfoParameters.PaymentDateFrom, new DateTime(2020, 01, 01)));
			AssertContainsExactElementsInAnyOrder(new string[] { "NCT0000001", "NCT0000002" }, filteredRows);

			filteredRows = GetFilteredRows(selectedColumn: "JobNumber", (Report_ITEntryPayInfoParameters.PaymentDateFrom, new DateTime(2020, 01, 01)), (Report_ITEntryPayInfoParameters.PaymentDateTo, new DateTime(2020, 01, 05)));
			AssertContainsExactElementsInAnyOrder(new string[] { "NCT0000001" }, filteredRows);

			filteredRows = GetFilteredRows(selectedColumn: "JobNumber", (Report_ITEntryPayInfoParameters.PaymentDateFrom, new DateTime(2020, 01, 05)), (Report_ITEntryPayInfoParameters.PaymentDateTo, new DateTime(2020, 01, 06)));
			AssertContainsExactElementsInAnyOrder(Array.Empty<string>(), filteredRows);

			filteredRows = GetFilteredRows(selectedColumn: "JobNumber", (Report_ITEntryPayInfoParameters.PaymentDateFrom, new DateTime(2020, 01, 05)));
			AssertContainsExactElementsInAnyOrder(new string[] { "NCT0000002" }, filteredRows);

			filteredRows = GetFilteredRows(selectedColumn: "JobNumber", (Report_ITEntryPayInfoParameters.PaymentDateTo, new DateTime(2020, 01, 04)));
			AssertContainsExactElementsInAnyOrder(new string[] { "NCT0000001" }, filteredRows);
		}

		public void TestFilterByDefermentAccountNumber()
		{
			UpdateCusInBondHeaderColumn("BH_JobReference", "NCT0000001", SqlDbType.VarChar, nctsHeaderPK);
			TestDataCreator.CreateGenAddOnColumn(nctsDepartureMovementHeaderPK, "BM", "DefermentAccountNumber", "STR", "1234");

			var nctsHeader2PK = TestDataCreator.CreateCusInbondHeader("NCT0000002", branchPK, "NCT");
			var nctsDepartureMovementHeader2PK = TestDataCreator.CreateCusInBondMoveHeader(nctsHeader2PK);
			TestDataCreator.CreateJobHeader(branchPK, companyPK, nctsHeader2PK, departmentPK, "BH", "2", "2", "WRK");
			TestDataCreator.CreateCusInBondPayInfo(nctsDepartureMovementHeader2PK, "", "", 0m, null, "", "");
			TestDataCreator.CreateOrUpdateGenAddOnColumn(nctsDepartureMovementHeader2PK, "BM", "STR", "DefermentAccountNumber", "1234");

			var filteredRows = GetFilteredRows(selectedColumn: "JobNumber");
			AssertContainsExactElementsInAnyOrder(new string[] { "NCT0000001", "NCT0000002" }, filteredRows);

			filteredRows = GetFilteredRows(selectedColumn: "JobNumber", (Report_ITEntryPayInfoParameters.DefermentAccountNumber, "1234"));
			AssertContainsExactElementsInAnyOrder(new string[] { "NCT0000001", "NCT0000002" }, filteredRows);

			TestDataCreator.CreateOrUpdateGenAddOnColumn(nctsDepartureMovementHeader2PK, "BM", "STR", "DefermentAccountNumber", "5678");
			filteredRows = GetFilteredRows(selectedColumn: "JobNumber", (Report_ITEntryPayInfoParameters.DefermentAccountNumber, "5678"));
			AssertContainsExactElementsInAnyOrder(new string[] { "NCT0000002" }, filteredRows);
		}

		public void TestFilterByCustomsOffice()
		{
			UpdateCusInBondHeaderColumn("BH_JobReference", "NCT0000001", SqlDbType.VarChar, nctsHeaderPK);
			TestDataCreator.CreateCusCodeData("EUO", "DEP", "1234", nctsHeaderPK, "BH");

			var nctsHeader2PK = TestDataCreator.CreateCusInbondHeader("NCT0000002", branchPK, "NCT");
			var nctsDepartureMovementHeader2PK = TestDataCreator.CreateCusInBondMoveHeader(nctsHeader2PK);
			TestDataCreator.CreateCusCodeData("EUO", "DEP", "5678", nctsHeader2PK, "BH");
			TestDataCreator.CreateJobHeader(branchPK, companyPK, nctsHeader2PK, departmentPK, "BH", "2", "2", "WRK");
			TestDataCreator.CreateCusInBondPayInfo(nctsDepartureMovementHeader2PK, "", "", 0m, null, "", "");

			var filteredRows = GetFilteredRows(selectedColumn: "JobNumber");
			AssertContainsExactElementsInAnyOrder(new string[] { "NCT0000001", "NCT0000002" }, filteredRows);

			filteredRows = GetFilteredRows(selectedColumn: "JobNumber", (Report_ITEntryPayInfoParameters.CustomsOffice, "1234"));
			AssertContainsExactElementsInAnyOrder(new string[] { "NCT0000001" }, filteredRows);

			filteredRows = GetFilteredRows(selectedColumn: "JobNumber", (Report_ITEntryPayInfoParameters.CustomsOffice, "5678"));
			AssertContainsExactElementsInAnyOrder(new string[] { "NCT0000002" }, filteredRows);
		}

		public void TestFilterByCustomsOfficeOnMovementHeader()
		{
			UpdateCusInBondHeaderColumn("BH_JobReference", "NCT0000001", SqlDbType.VarChar, nctsHeaderPK);
			TestDataCreator.CreateCusCodeData("EUO", "DEP", "1234", nctsDepartureMovementHeaderPK, "BM");

			var nctsHeader2PK = TestDataCreator.CreateCusInbondHeader("NCT0000002", branchPK, "NCT");
			var nctsDepartureMovementHeader2PK = TestDataCreator.CreateCusInBondMoveHeader(nctsHeader2PK);
			TestDataCreator.CreateCusCodeData("EUO", "DEP", "5678", nctsDepartureMovementHeader2PK, "BM");
			TestDataCreator.CreateJobHeader(branchPK, companyPK, nctsHeader2PK, departmentPK, "BH", "2", "2", "WRK");
			TestDataCreator.CreateCusInBondPayInfo(nctsDepartureMovementHeader2PK, "", "", 0m, null, "", "");

			var filteredRows = GetFilteredRows(selectedColumn: "JobNumber");
			AssertContainsExactElementsInAnyOrder(new string[] { "NCT0000001", "NCT0000002" }, filteredRows);

			filteredRows = GetFilteredRows(selectedColumn: "JobNumber", (Report_ITEntryPayInfoParameters.CustomsOffice, "1234"));
			AssertContainsExactElementsInAnyOrder(new string[] { "NCT0000001" }, filteredRows);

			filteredRows = GetFilteredRows(selectedColumn: "JobNumber", (Report_ITEntryPayInfoParameters.CustomsOffice, "5678"));
			AssertContainsExactElementsInAnyOrder(new string[] { "NCT0000002" }, filteredRows);
		}

		#endregion

		protected override void SetUp()
		{
			base.SetUp();

			nctsHeaderPK = TestDataCreator.CreateCusInbondHeader("", branchPK, "NCT");
			nctsDepartureMovementHeaderPK = TestDataCreator.CreateCusInBondMoveHeader(nctsHeaderPK);
			jobHeaderPK = TestDataCreator.CreateJobHeader(branchPK, companyPK, nctsHeaderPK, departmentPK, "BH", "", "", "WRK");
			inBondPayInfoPK = TestDataCreator.CreateCusInBondPayInfo(nctsDepartureMovementHeaderPK, "", "", 0m, null, "", "");
		}

		Guid nctsHeaderPK;
		Guid nctsDepartureMovementHeaderPK;
		Guid jobHeaderPK;
		Guid inBondPayInfoPK;

		void UpdateCusInBondPayInfoColumn(string columnName, object columnValue, SqlDbType columnType, object primaryKeyValue)
		{
			UpdateTableColumn("CusInBondPayInfo", columnName, columnValue, columnType, "BPI_PK", primaryKeyValue);
		}

		void UpdateCusInBondHeaderColumn(string columnName, object columnValue, SqlDbType columnType, object primaryKeyValue)
		{
			UpdateTableColumn("CusInBondHeader", columnName, columnValue, columnType, "BH_PK", primaryKeyValue);
		}
	}

	abstract class Report_ITEntryPayInfoBaseTest : CustomsReportDbCreateScriptTest
	{
		protected override IEnumerable<(SqlDbType ParameterType, string ParameterName)> GetSqlParameters()
		{
			yield return (SqlDbType.UniqueIdentifier, Report_ITEntryPayInfoParameters.CompanyPK);
			yield return (SqlDbType.SmallDateTime, Report_ITEntryPayInfoParameters.CustomsRegistrationDateFrom);
			yield return (SqlDbType.SmallDateTime, Report_ITEntryPayInfoParameters.CustomsRegistrationDateTo);
			yield return (SqlDbType.SmallDateTime, Report_ITEntryPayInfoParameters.PaymentDateFrom);
			yield return (SqlDbType.SmallDateTime, Report_ITEntryPayInfoParameters.PaymentDateTo);
			yield return (SqlDbType.VarChar, Report_ITEntryPayInfoParameters.DefermentAccountNumber);
			yield return (SqlDbType.VarChar, Report_ITEntryPayInfoParameters.CustomsOffice);
		}

		protected override void SetUp()
		{
			base.SetUp();
			companyPK = TestDataCreator.CreateCompany(companyCode: "ITA", countryCode: "IT", currencyCode: "EUR");
			branchPK = TestDataCreator.CreateBranch(companyPK, branchCode: "ITA", homePort: "ITALY", countryCode: "IT");
			departmentPK = TestDataCreator.CreateDepartment("ITA");
		}

		protected Guid companyPK;
		protected Guid branchPK;
		protected Guid departmentPK;

		protected override object[] GetFilteredRows(string selectedColumn, params (string ParamName, object ParamValue)[] filters)
		{
			var filterList = new List<(string ParamName, object ParamValue)>(filters);
			filterList.Insert(0, (Report_ITEntryPayInfoParameters.CompanyPK, companyPK));
			return base.GetFilteredRows(selectedColumn, filterList.ToArray());
		}

		protected class Report_ITEntryPayInfoParameters
		{
			public const string CompanyPK = "@CompanyPK";
			public const string CustomsRegistrationDateFrom = "@CustomsRegistrationDateFrom";
			public const string CustomsRegistrationDateTo = "@CustomsRegistrationDateTo";
			public const string PaymentDateFrom = "@PaymentDateFrom";
			public const string PaymentDateTo = "@PaymentDateTo";
			public const string DefermentAccountNumber = "@DefermentAccountNumber";
			public const string CustomsOffice = "@CustomsOffice";
		}
	}
}
