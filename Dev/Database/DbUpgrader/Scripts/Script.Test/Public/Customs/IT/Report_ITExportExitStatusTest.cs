using System;
using System.Collections.Generic;
using System.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.Customs.IT;
using Enterprise.Build.Database.Script.TestFramework;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Customs.IT.Testing
{
	[TestedType(typeof(Report_ITExportExitStatus))]
	class Report_ITExportExitStatusTest : CustomsReportDbCreateScriptTest
	{
		public void TestExitStatusLookupJoinToRefCusCodeListMaintainsDataGrain()
		{
			SetUpSupplierAndDeclaration();
			SetUpEntryFields();

			UpdateEntryNumberColumn("CE_EntryStatus", "EXC", SqlDbType.VarChar, entryNumberIviPK);

			TestDataCreator.CreateRefDbDataGroupingCodeTypeAndListItems("EUN", "CSTEX", ("EXC", "EUN Exit Completed"));

			var filteredRows = GetFilteredRows("StatusDescription");
			AssertContainsExactElementsInAnyOrder("There's no CSTEX code where  DataGrouping=IT => StatusDescription should be empty", new object[] { DBNull.Value }, filteredRows);

			TestDataCreator.CreateRefDbDataGroupingCodeTypeAndListItems("IT", "CSTEX", ("EXC", "IT Exit Completed"));

			filteredRows = GetFilteredRows("StatusDescription");
			AssertContainsExactElementsInAnyOrder("Should get only the StatusDescription where DataGrouping=IT", new string[] { "IT Exit Completed" }, filteredRows);
		}

		public void TestExitOfficeLookupJoinToRefCusCodeListMaintainsDataGrain()
		{
			SetUpSupplierAndDeclaration();
			SetUpEntryFields();

			UpdateEntryNumberColumn("CE_EntryLineReference", "FR00001", SqlDbType.VarChar, entryNumberIviPK);

			TestDataCreator.CreateRefDbDataGroupingCodeTypeAndListItems("XX", "CUSOF", ("FR00001", "Inconsistent Office Name"));

			var filteredRows = GetFilteredRows("OfficeDescription");
			AssertContainsExactElementsInAnyOrder("There's no CUSOF code matching DataGrouping => OfficeDescription should be empty", new object[] { DBNull.Value }, filteredRows);

			TestDataCreator.CreateRefDbDataGroupingCodeTypeAndListItems("FR", "CUSOF", ("FR00001", "Matching Office Name"));

			filteredRows = GetFilteredRows("OfficeDescription");
			AssertContainsExactElementsInAnyOrder("Should get only the OfficeDescription which DataGrouping matches the code", new string[] { "Matching Office Name" }, filteredRows);
		}

		public void TestJobHeaderJoinMaintainsDataGrain()
		{
			SetUpSupplierAndDeclaration();
			SetUpEntryFields();

			var departmentPk = TestDataCreator.CreateDepartment("~D1");
			TestDataCreator.CreateJobHeader(branchPK, companyPK, declarationPK, departmentPk, "JE", "", "", "WRK");

			var anotherCompanyPK = TestDataCreator.CreateCompany(companyCode: "~C2", countryCode: "SM", currencyCode: "EUR");
			var anotherBranchPK = TestDataCreator.CreateBranch(anotherCompanyPK, branchCode: "~B2", homePort: "SMSRV");
			TestDataCreator.CreateJobHeader(anotherBranchPK, anotherCompanyPK, declarationPK, departmentPk, "JE", "", "", "WRK");

			var filteredRows = GetFilteredRows("JobNumber");
			AssertContainsExactElementsInAnyOrder("Should not duplicate rows as only the JobHeader matching the declaration Company is joined", new string[] { "B00001" }, filteredRows);
		}

		public void TestCancelledDeclarationNotLoaded()
		{
			SetUpSupplierAndDeclaration();
			SetUpEntryFields();

			var filteredRows = GetFilteredRows("JobNumber");
			AssertContainsExactElementsInAnyOrder("Should load declaration as it is not cancelled", new string[] { "B00001" }, filteredRows);

			UpdateDeclarationColumn("JE_IsCancelled", true, SqlDbType.Bit, declarationPK);

			filteredRows = GetFilteredRows("JobNumber");
			AssertContainsExactElementsInAnyOrder("Declaration is cancelled => should load no rows", Array.Empty<object>(), filteredRows);
		}

		#region Columns

		public void TestMRN()
		{
			SetUpSupplierAndDeclaration();
			SetUpEntryFields();

			UpdateEntryNumberColumn("CE_EntryNum ", "123", SqlDbType.VarChar, entryNumberMovementPK);
			AssertFunctionReturnExpectedValue("MRN", "123");
		}

		public void TestJobNumber()
		{
			SetUpSupplierAndDeclaration();
			SetUpEntryFields();

			UpdateDeclarationColumn("JE_DeclarationReference", "ABC", SqlDbType.VarChar, declarationPK);
			AssertFunctionReturnExpectedValue("JobNumber", "ABC");
		}

		public void TestEntryStyle()
		{
			SetUpSupplierAndDeclaration();
			SetUpEntryFields();

			UpdateDeclarationColumn("JE_MessageSubType", "FRM", SqlDbType.VarChar, declarationPK);
			AssertFunctionReturnExpectedValue("EntryStyle", "FRM");
		}

		public void TestCustomsRegistrationNumber()
		{
			SetUpSupplierAndDeclaration();
			SetUpEntryFields();

			UpdateEntryNumberColumn("CE_EntryNum ", "123", SqlDbType.VarChar, entryNumberRegistrationPK);

			AssertFunctionReturnExpectedValue("CustomsRegistrationNumber", "123");
		}

		public void TestReleaseDate()
		{
			SetUpSupplierAndDeclaration();
			SetUpEntryFields();

			UpdateEntryHeaderColumn("CH_EntryReleaseDate", new DateTime(2021, 01, 01), SqlDbType.SmallDateTime, entryHeaderPK);
			AssertFunctionReturnExpectedValue("ReleaseDate", new DateTime(2021, 01, 01));
		}

		public void TestDeclarationType()
		{
			SetUpSupplierAndDeclaration();

			var entryInstructionPK = TestDataCreator.CreateCusEntryInstruction(declarationPK, "10", "A", DateTime.Today, 1);

			var entryHeaderPK = TestDataCreator.CreateCusEntryHeader(true, "BLT", "OK", "100", "04321-B00181816", 301, 1, "", declarationPK, DateTime.Today, DateTime.Today, entryInstructionPK, "", DateTime.Today, DateTime.Today, DateTime.Today, 1);
			TestDataCreator.CreateCusEntryNum(entryHeaderPK, "CusEntryHeader", "MRN001", "MRN", "CUS", "IT");
			TestDataCreator.CreateCusEntryNum(entryHeaderPK, "CusEntryHeader", "REG001", "REG", "CUS", "IT");

			AssertFunctionReturnExpectedValue("DeclarationType", "10");
		}

		public void TestAuthorizationNumber()
		{
			SetUpSupplierAndDeclaration();
			SetUpEntryFields();

			UpdateDeclarationColumn("JE_AddInfo", "AuthorisationNumber=123", SqlDbType.VarChar, declarationPK);
			AssertFunctionReturnExpectedValue("AuthorizationNumber", "123");
		}

		public void TestSuppiler()
		{
			var companyPK = TestDataCreator.CreateCompany(companyCode: "TC1", countryCode: "IT", currencyCode: "EUR");
			var branchPK = TestDataCreator.CreateBranch(companyPK, branchCode: "TB1", homePort: "ITALY");

			var organisationSupplierPK = TestDataCreator.CreateOrganisation("SUPPL2", "SUPPLIER123");
			var declarationPK = TestDataCreator.CreateJobDeclaration(branchPK, companyPK, "B00002", "EXP", 2, supplierPK: organisationSupplierPK);

			var entryInstructionPK = TestDataCreator.CreateCusEntryInstruction(declarationPK, "10", "A", DateTime.Today, 2);
			var entryHeaderPK = TestDataCreator.CreateCusEntryHeader(true, "BLT", "OK", "100", "04321-B00181816", 301, 1, "", declarationPK, DateTime.Today, DateTime.Today, entryInstructionPK, "", DateTime.Today, DateTime.Today, DateTime.Today, 2);
			TestDataCreator.CreateCusEntryNum(entryHeaderPK, "CusEntryHeader", "MRN001", "MRN", "CUS", "IT");
			TestDataCreator.CreateCusEntryNum(entryHeaderPK, "CusEntryHeader", "REG001", "REG", "CUS", "IT");

			AssertFunctionReturnExpectedValue("SupplierFullName", "SUPPLIER123");
		}

		public void TestLocalClient()
		{
			SetUpSupplierAndDeclaration();
			SetUpEntryFields();

			var departmentPK = TestDataCreator.CreateDepartment("ITA");
			var jobHeaderPK = TestDataCreator.CreateJobHeader(branchPK, companyPK, declarationPK, departmentPK, "JE", "", "", "WRK");
			var localClientPK = TestDataCreator.CreateOrganisation("LCLCLIENTCODE", "LCLCLIENT123");
			var orgAddressPK = TestDataCreator.CreateAddress(localClientPK, "Delivery Address", "Address X");

			UpdateJobHeaderColumn("JH_OA_LocalChargesAddr", orgAddressPK, SqlDbType.UniqueIdentifier, jobHeaderPK);

			AssertFunctionReturnExpectedValue("LocalClientFullName", "LCLCLIENT123");
		}

		public void TestLocalClientWhenJobHeaderIsLinkedToShipment()
		{
			SetUpSupplierAndDeclaration();
			SetUpEntryFields();

			var shipmentPK = TestDataCreator.CreateJobShipment();
			UpdateDeclarationColumn("JE_JS", shipmentPK, SqlDbType.UniqueIdentifier, declarationPK);

			var departmentPK = TestDataCreator.CreateDepartment("ITA");
			var jobHeaderPK = TestDataCreator.CreateJobHeader(branchPK, companyPK, shipmentPK, departmentPK, "JS", "", "", "WRK");
			var localClientPK = TestDataCreator.CreateOrganisation("LCLCLIENTCODE", "LCLCLIENTSHP123");
			var orgAddressPK = TestDataCreator.CreateAddress(localClientPK, "Delivery Address", "Address X");
			UpdateJobHeaderColumn("JH_OA_LocalChargesAddr", orgAddressPK, SqlDbType.UniqueIdentifier, jobHeaderPK);

			AssertFunctionReturnExpectedValue("LocalClientFullName", "LCLCLIENTSHP123");
		}

		public void TestTransportID()
		{
			SetUpSupplierAndDeclaration();
			SetUpEntryFields();

			UpdateDeclarationColumn("JE_AddInfo", "Box18TransportID=RX2839A", SqlDbType.VarChar, declarationPK);
			AssertFunctionReturnExpectedValue("TransportID", "RX2839A");
		}

		public void TestDestination()
		{
			SetUpSupplierAndDeclaration();
			SetUpEntryFields();

			UpdateDeclarationColumn("JE_GoodsDestination", "XA", SqlDbType.VarChar, declarationPK);
			AssertFunctionReturnExpectedValue("Destination", "XA");
		}

		public void TestAgentsReference()
		{
			SetUpSupplierAndDeclaration();
			SetUpEntryFields();

			UpdateDeclarationColumn("JE_AgentsReference", "ABC", SqlDbType.VarChar, declarationPK);
			AssertFunctionReturnExpectedValue("AgentsReference", "ABC");
		}

		public void TestOffice()
		{
			SetUpSupplierAndDeclaration();
			SetUpEntryFields();

			UpdateEntryNumberColumn("CE_EntryLineReference", "IT00001", SqlDbType.VarChar, entryNumberIviPK);
			UpdateEntryNumberColumn("CE_EntryType", "IVI", SqlDbType.VarChar, entryNumberIviPK);
			UpdateEntryNumberColumn("CE_Category", "CUS", SqlDbType.VarChar, entryNumberIviPK);

			TestDataCreator.CreateRefDbDataGroupingCodeTypeAndListItems("IT", "CUSOF", ("IT00001", "Some Office Name"));

			CombineAssertions(() =>
			{
				AssertFunctionReturnExpectedValue("OfficeCode", "IT00001");
				AssertFunctionReturnExpectedValue("OfficeDescription", "Some Office Name");
			});
		}

		public void TestExitDate()
		{
			SetUpSupplierAndDeclaration();
			SetUpEntryFields();

			UpdateEntryNumberColumn("CE_IssueDate", new DateTime(2021, 01, 01), SqlDbType.SmallDateTime, entryNumberIviPK);

			AssertFunctionReturnExpectedValue("ExitDate", new DateTime(2021, 01, 01));
		}

		public void TestExitStatus()
		{
			SetUpSupplierAndDeclaration();
			SetUpEntryFields();

			UpdateEntryNumberColumn("CE_EntryStatus ", "EXC", SqlDbType.VarChar, entryNumberIviPK);

			AssertFunctionReturnExpectedValue("ExitStatus", "EXC");
		}

		public void TestStatusDescription()
		{
			SetUpSupplierAndDeclaration();
			SetUpEntryFields();

			UpdateEntryNumberColumn("CE_EntryStatus ", "EXC", SqlDbType.VarChar, entryNumberIviPK);
			UpdateEntryNumberColumn("CE_EntryType", "IVI", SqlDbType.VarChar, entryNumberIviPK);
			UpdateEntryNumberColumn("CE_Category", "CUS", SqlDbType.VarChar, entryNumberIviPK);

			TestDataCreator.CreateRefDbDataGroupingCodeTypeAndListItems("IT", "CSTEX", ("EXC", "Exit Completed"));

			AssertFunctionReturnExpectedValue("StatusDescription", "Exit Completed");
		}

		#endregion

		#region Filters

		public void TestFilterByAcceptanceDate()
		{
			SetUpSupplierAndDeclaration();
			SetUpEntryFields();

			UpdateEntryNumberColumn("CE_EntryNum ", "123", SqlDbType.VarChar, entryNumberMovementPK);

			var companyPK2 = TestDataCreator.CreateCompany(companyCode: "TC2", countryCode: "IT", currencyCode: "EUR");
			var branchPK2 = TestDataCreator.CreateBranch(companyPK2, branchCode: "TB2", homePort: "ITALY");

			var organisationSupplierPK2 = TestDataCreator.CreateOrganisation("SUPPL2", "SUPPLIER2");
			var declarationPK2 = TestDataCreator.CreateJobDeclaration(branchPK2, companyPK2, "B00002", "EXP", 2, supplierPK: organisationSupplierPK2);
			var entryInstructionPK2 = TestDataCreator.CreateCusEntryInstruction(declarationPK2, "10", "", new DateTime(2019, 02, 01), 2);
			var entryHeaderPK2 = TestDataCreator.CreateCusEntryHeader(true, "BLT", "OK", "100", "04321-B00181816", 200, 1, "", declarationPK2, DateTime.Today, DateTime.Today, entryInstructionPK2, "", DateTime.Today, DateTime.Today, DateTime.Today, 2);
			var entryNumberMovementPK2 = TestDataCreator.CreateCusEntryNum(entryHeaderPK2, "CusEntryHeader", "MRN002", "MRN", "CUS", "IT");
			TestDataCreator.CreateCusEntryNum(entryHeaderPK2, "CusEntryHeader", "REG002", "REG", "CUS", "IT");

			UpdateEntryNumberColumn("CE_EntryNum ", "456", SqlDbType.VarChar, entryNumberMovementPK2);

			var filteredRows = GetFilteredRows("MRN");
			AssertContainsExactElementsInAnyOrder("Empty filters", new string[] { "123", "456" }, filteredRows);

			filteredRows = GetFilteredRows("MRN", (Report_ITExportExitStatusParameter.AcceptanceDateFrom, new DateTime(2019, 01, 01)));
			AssertContainsExactElementsInAnyOrder("From filter", new string[] { "123", "456" }, filteredRows);

			filteredRows = GetFilteredRows("MRN", (Report_ITExportExitStatusParameter.AcceptanceDateFrom, DateTime.Today.AddDays(-1)), (Report_ITExportExitStatusParameter.AcceptanceDateTo, DateTime.Today));
			AssertContainsExactElementsInAnyOrder(new string[] { "123" }, filteredRows);

			filteredRows = GetFilteredRows("MRN", (Report_ITExportExitStatusParameter.AcceptanceDateFrom, DateTime.Today.AddDays(-2)), (Report_ITExportExitStatusParameter.AcceptanceDateTo, DateTime.Today.AddDays(-1)));
			AssertContainsExactElementsInAnyOrder(Array.Empty<string>(), filteredRows);

			filteredRows = GetFilteredRows("MRN", (Report_ITExportExitStatusParameter.AcceptanceDateFrom, new DateTime(2019, 01, 01)), (Report_ITExportExitStatusParameter.AcceptanceDateTo, new DateTime(2019, 03, 01)));
			AssertContainsExactElementsInAnyOrder(new string[] { "456" }, filteredRows);
		}

		public void TestFilterBySupplierCode()
		{
			SetUpSupplierAndDeclaration();
			SetUpEntryFields();

			UpdateEntryNumberColumn("CE_EntryNum ", "123", SqlDbType.VarChar, entryNumberMovementPK);

			var companyPK2 = TestDataCreator.CreateCompany(companyCode: "TC2", countryCode: "IT", currencyCode: "EUR");
			var branchPK2 = TestDataCreator.CreateBranch(companyPK2, branchCode: "TB2", homePort: "ITALY");
			var organisationSupplierPK2 = TestDataCreator.CreateOrganisation("SUPPL2", "SUPPLIER2");
			var declarationPK2 = TestDataCreator.CreateJobDeclaration(branchPK2, companyPK2, "B00002", "EXP", 2, supplierPK: organisationSupplierPK2);
			var entryInstructionPK2 = TestDataCreator.CreateCusEntryInstruction(declarationPK2, "10", "", new DateTime(2019, 02, 01), 2);
			var entryHeaderPK2 = TestDataCreator.CreateCusEntryHeader(true, "BLT", "OK", "100", "04321-B00181816", 200, 1, "", declarationPK2, DateTime.Today, DateTime.Today, entryInstructionPK2, "", DateTime.Today, DateTime.Today, DateTime.Today, 2);
			TestDataCreator.CreateCusEntryNum(entryHeaderPK2, "CusEntryHeader", "REG002", "REG", "CUS", "IT");

			var entryNumberMovementPK2 = TestDataCreator.CreateCusEntryNum(entryHeaderPK2, "CusEntryHeader", "MRN002", "MRN", "CUS", "IT");

			UpdateEntryNumberColumn("CE_EntryNum ", "456", SqlDbType.VarChar, entryNumberMovementPK2);

			var filteredRows = GetFilteredRows("MRN");
			AssertContainsExactElementsInAnyOrder("Empty filters", new string[] { "123", "456" }, filteredRows);

			filteredRows = GetFilteredRows("MRN", (Report_ITExportExitStatusParameter.SupplierOrganisationPK, organisationSupplierPK));
			AssertContainsExactElementsInAnyOrder("From filter", new string[] { "123" }, filteredRows);

			filteredRows = GetFilteredRows("MRN", (Report_ITExportExitStatusParameter.SupplierOrganisationPK, organisationSupplierPK2));
			AssertContainsExactElementsInAnyOrder("From filter", new string[] { "456" }, filteredRows);
		}

		public void TestFilterByLocalClientCode()
		{
			SetUpSupplierAndDeclaration();
			SetUpEntryFields();

			UpdateEntryNumberColumn("CE_EntryNum ", "123", SqlDbType.VarChar, entryNumberMovementPK);

			var departmentPK = TestDataCreator.CreateDepartment("ITA");
			var jobHeaderPK = TestDataCreator.CreateJobHeader(branchPK, companyPK, declarationPK, departmentPK, "JE", "B00001", "B", "WRK");
			var localClientPK = TestDataCreator.CreateOrganisation("LCLCLIENT123", "LCLCLIENT123");
			var orgAddressPK = TestDataCreator.CreateAddress(localClientPK, "Delivery Address", "Address X");

			UpdateJobHeaderColumn("JH_OA_LocalChargesAddr", orgAddressPK, SqlDbType.UniqueIdentifier, jobHeaderPK);

			var companyPK2 = TestDataCreator.CreateCompany(companyCode: "TC2", countryCode: "IT", currencyCode: "EUR");
			var branchPK2 = TestDataCreator.CreateBranch(companyPK2, branchCode: "TB2", homePort: "ITALY");
			var organisationSupplierPK2 = TestDataCreator.CreateOrganisation("SUPPL2", "SUPPLIER2");
			var declarationPK2 = TestDataCreator.CreateJobDeclaration(branchPK2, companyPK2, "B00002", "EXP", 2, supplierPK: organisationSupplierPK2);
			var jobHeaderPK2 = TestDataCreator.CreateJobHeader(branchPK2, companyPK2, declarationPK2, departmentPK, "JE", "B00002", "B", "WRK");
			var entryInstructionPK2 = TestDataCreator.CreateCusEntryInstruction(declarationPK2, "10", "", new DateTime(2019, 02, 01), 2);
			var entryHeaderPK2 = TestDataCreator.CreateCusEntryHeader(true, "BLT", "OK", "100", "04321-B00181816", 200, 2, "", declarationPK2, DateTime.Today, DateTime.Today, entryInstructionPK2, "", DateTime.Today, DateTime.Today, DateTime.Today, 2);
			var entryNumberMovementPK2 = TestDataCreator.CreateCusEntryNum(entryHeaderPK2, "CusEntryHeader", "MRN002", "MRN", "CUS", "IT");
			var localClientPK2 = TestDataCreator.CreateOrganisation("LCLCLIENT456", "LCLCLIENT456");
			var orgAddressPK2 = TestDataCreator.CreateAddress(localClientPK2, "Delivery Address", "Address X");
			TestDataCreator.CreateCusEntryNum(entryHeaderPK2, "CusEntryHeader", "REG002", "REG", "CUS", "IT");

			UpdateJobHeaderColumn("JH_OA_LocalChargesAddr", orgAddressPK2, SqlDbType.UniqueIdentifier, jobHeaderPK2);
			UpdateEntryNumberColumn("CE_EntryNum ", "456", SqlDbType.VarChar, entryNumberMovementPK2);

			var filteredRows = GetFilteredRows("MRN");
			AssertContainsExactElementsInAnyOrder("Empty filters", new string[] { "123", "456" }, filteredRows);

			filteredRows = GetFilteredRows("MRN", (Report_ITExportExitStatusParameter.LocalClientPK, localClientPK));
			AssertContainsExactElementsInAnyOrder("From filter", new string[] { "123" }, filteredRows);

			filteredRows = GetFilteredRows("MRN", (Report_ITExportExitStatusParameter.LocalClientPK, localClientPK2));
			AssertContainsExactElementsInAnyOrder("From filter", new string[] { "456" }, filteredRows);
		}

		public void TestFilterByDeclarationType()
		{
			SetUpSupplierAndDeclaration();
			SetUpEntryFields();

			UpdateEntryNumberColumn("CE_EntryNum ", "123", SqlDbType.VarChar, entryNumberMovementPK);

			var companyPK2 = TestDataCreator.CreateCompany(companyCode: "TC2", countryCode: "IT", currencyCode: "EUR");
			var branchPK2 = TestDataCreator.CreateBranch(companyPK2, branchCode: "TB2", homePort: "ITALY");
			var organisationSupplierPK2 = TestDataCreator.CreateOrganisation("SUPPL2", "SUPPLIER2");
			var declarationPK2 = TestDataCreator.CreateJobDeclaration(branchPK2, companyPK2, "B00002", "EXP", 2, supplierPK: organisationSupplierPK2);
			var entryInstructionPK2 = TestDataCreator.CreateCusEntryInstruction(declarationPK2, "B", "", new DateTime(2019, 02, 01), 2);
			var entryHeaderPK2 = TestDataCreator.CreateCusEntryHeader(true, "BLT", "OK", "100", "04321-B00181816", 200, 1, "", declarationPK2, DateTime.Today, DateTime.Today, entryInstructionPK2, "", DateTime.Today, DateTime.Today, DateTime.Today, 2);
			var entryNumberMovementPK2 = TestDataCreator.CreateCusEntryNum(entryHeaderPK2, "CusEntryHeader", "MRN002", "MRN", "CUS", "IT");
			TestDataCreator.CreateCusEntryNum(entryHeaderPK2, "CusEntryHeader", "REG002", "REG", "CUS", "IT");

			UpdateEntryNumberColumn("CE_EntryNum ", "456", SqlDbType.VarChar, entryNumberMovementPK2);

			var filteredRows = GetFilteredRows("MRN");
			AssertContainsExactElementsInAnyOrder("Empty filters", new string[] { "123", "456" }, filteredRows);

			filteredRows = GetFilteredRows("MRN", (Report_ITExportExitStatusParameter.DeclarationType, "A"));
			AssertContainsExactElementsInAnyOrder("From filter", new string[] { "123" }, filteredRows);

			filteredRows = GetFilteredRows("MRN", (Report_ITExportExitStatusParameter.DeclarationType, "B"));
			AssertContainsExactElementsInAnyOrder("From filter", new string[] { "456" }, filteredRows);
		}

		public void TestFilterByExitState()
		{
			SetUpSupplierAndDeclaration();
			SetUpEntryFields();

			UpdateEntryNumberColumn("CE_IssueDate", new DateTime(2021, 01, 01), SqlDbType.SmallDateTime, entryNumberIviPK);
			UpdateEntryNumberColumn("CE_EntryNum ", "123", SqlDbType.VarChar, entryNumberMovementPK);

			var companyPK2 = TestDataCreator.CreateCompany(companyCode: "TC2", countryCode: "IT", currencyCode: "EUR");
			var branchPK2 = TestDataCreator.CreateBranch(companyPK2, branchCode: "TB2", homePort: "ITALY");
			var organisationSupplierPK2 = TestDataCreator.CreateOrganisation("SUPPL2", "SUPPLIER2");
			var declarationPK2 = TestDataCreator.CreateJobDeclaration(branchPK2, companyPK2, "B00002", "EXP", 2, supplierPK: organisationSupplierPK2);
			var entryInstructionPK2 = TestDataCreator.CreateCusEntryInstruction(declarationPK2, "B", "", new DateTime(2019, 02, 01), 2);
			var entryHeaderPK2 = TestDataCreator.CreateCusEntryHeader(true, "BLT", "OK", "100", "04321-B00181816", 200, 1, "", declarationPK2, DateTime.Today, DateTime.Today, entryInstructionPK2, "", DateTime.Today, DateTime.Today, DateTime.Today, 2);
			var entryNumberMovementPK2 = TestDataCreator.CreateCusEntryNum(entryHeaderPK2, "CusEntryHeader", "MRN002", "MRN", "CUS", "IT");
			TestDataCreator.CreateCusEntryNum(entryHeaderPK2, "CusEntryHeader", "REG002", "REG", "CUS", "IT");

			UpdateEntryNumberColumn("CE_EntryNum ", "456", SqlDbType.VarChar, entryNumberMovementPK2);

			var filteredRows = GetFilteredRows("MRN");
			AssertContainsExactElementsInAnyOrder("Empty filters", new string[] { "123", "456" }, filteredRows);

			filteredRows = GetFilteredRows("MRN", (Report_ITExportExitStatusParameter.ExitState, "exit"));
			AssertContainsExactElementsInAnyOrder("From filter", new string[] { "123" }, filteredRows);

			filteredRows = GetFilteredRows("MRN", (Report_ITExportExitStatusParameter.ExitState, "no exit"));
			AssertContainsExactElementsInAnyOrder("From filter", new string[] { "456" }, filteredRows);

			filteredRows = GetFilteredRows("MRN", (Report_ITExportExitStatusParameter.ExitState, "all"));
			AssertContainsExactElementsInAnyOrder("From filter", new string[] { "123", "456" }, filteredRows);
		}

		public void TestFilterByExitDate()
		{
			SetUpSupplierAndDeclaration();
			SetUpEntryFields();

			UpdateEntryNumberColumn("CE_IssueDate", DateTime.Today, SqlDbType.SmallDateTime, entryNumberIviPK);
			UpdateEntryNumberColumn("CE_EntryNum ", "123", SqlDbType.VarChar, entryNumberMovementPK);

			var companyPK2 = TestDataCreator.CreateCompany(companyCode: "TC2", countryCode: "IT", currencyCode: "EUR");
			var branchPK2 = TestDataCreator.CreateBranch(companyPK2, branchCode: "TB2", homePort: "ITALY");
			var organisationSupplierPK2 = TestDataCreator.CreateOrganisation("SUPPL2", "SUPPLIER2");
			var declarationPK2 = TestDataCreator.CreateJobDeclaration(branchPK2, companyPK2, "B00002", "EXP", 2, supplierPK: organisationSupplierPK2);
			var entryInstructionPK2 = TestDataCreator.CreateCusEntryInstruction(declarationPK2, "B", "", new DateTime(2019, 02, 01), 2);
			var entryHeaderPK2 = TestDataCreator.CreateCusEntryHeader(true, "BLT", "OK", "100", "04321-B00181816", 200, 1, "", declarationPK2, DateTime.Today, DateTime.Today, entryInstructionPK2, "", DateTime.Today, DateTime.Today, DateTime.Today, 2);
			var entryNumberIviPK2 = TestDataCreator.CreateCusEntryNum(entryHeaderPK2, "CusEntryHeader", "MRN003", "IVI", "CUS", "IT");
			var entryNumberMovementPK2 = TestDataCreator.CreateCusEntryNum(entryHeaderPK2, "CusEntryHeader", "MRN002", "MRN", "CUS", "IT");
			TestDataCreator.CreateCusEntryNum(entryHeaderPK2, "CusEntryHeader", "REG002", "REG", "CUS", "IT");

			UpdateEntryNumberColumn("CE_EntryNum ", "456", SqlDbType.VarChar, entryNumberMovementPK2);
			UpdateEntryNumberColumn("CE_IssueDate", new DateTime(2019, 02, 01), SqlDbType.SmallDateTime, entryNumberIviPK2);

			var filteredRows = GetFilteredRows("MRN");
			AssertContainsExactElementsInAnyOrder("Empty filters", new string[] { "123", "456" }, filteredRows);

			filteredRows = GetFilteredRows("MRN", (Report_ITExportExitStatusParameter.ExitDateFrom, new DateTime(2019, 01, 01)));
			AssertContainsExactElementsInAnyOrder("From filter", new string[] { "123", "456" }, filteredRows);

			filteredRows = GetFilteredRows("MRN", (Report_ITExportExitStatusParameter.ExitDateFrom, DateTime.Today.AddDays(-1)), (Report_ITExportExitStatusParameter.ExitDateTo, DateTime.Today));
			AssertContainsExactElementsInAnyOrder(new string[] { "123" }, filteredRows);

			filteredRows = GetFilteredRows("MRN", (Report_ITExportExitStatusParameter.ExitDateFrom, DateTime.Today.AddDays(-2)), (Report_ITExportExitStatusParameter.ExitDateTo, DateTime.Today.AddDays(-1)));
			AssertContainsExactElementsInAnyOrder(Array.Empty<string>(), filteredRows);

			filteredRows = GetFilteredRows("MRN", (Report_ITExportExitStatusParameter.ExitDateFrom, new DateTime(2019, 01, 01)), (Report_ITExportExitStatusParameter.ExitDateTo, new DateTime(2019, 03, 01)));
			AssertContainsExactElementsInAnyOrder(new string[] { "456" }, filteredRows);
		}

		public void TestFilterByExportType()
		{
			SetUpSupplierAndDeclaration();
			SetUpEntryFields();

			UpdateDeclarationColumn("JE_MessageType", "EXP", SqlDbType.VarChar, declarationPK);
			UpdateEntryNumberColumn("CE_EntryNum ", "123", SqlDbType.VarChar, entryNumberMovementPK);

			var companyPK2 = TestDataCreator.CreateCompany(companyCode: "TC2", countryCode: "IT", currencyCode: "EUR");
			var branchPK2 = TestDataCreator.CreateBranch(companyPK2, branchCode: "TB2", homePort: "ITALY");
			var organisationSupplierPK2 = TestDataCreator.CreateOrganisation("SUPPL2", "SUPPLIER2");
			var declarationPK2 = TestDataCreator.CreateJobDeclaration(branchPK2, companyPK2, "B00002", "EXP", 2, supplierPK: organisationSupplierPK2);
			var entryInstructionPK2 = TestDataCreator.CreateCusEntryInstruction(declarationPK2, "B", "", new DateTime(2019, 02, 01), 2);
			var entryHeaderPK2 = TestDataCreator.CreateCusEntryHeader(true, "BLT", "OK", "100", "04321-B00181816", 200, 1, "", declarationPK2, DateTime.Today, DateTime.Today, entryInstructionPK2, "", DateTime.Today, DateTime.Today, DateTime.Today, 2);
			var entryNumberMovementPK2 = TestDataCreator.CreateCusEntryNum(entryHeaderPK2, "CusEntryHeader", "MRN002", "MRN", "CUS", "IT");
			TestDataCreator.CreateCusEntryNum(entryHeaderPK2, "CusEntryHeader", "REG002", "REG", "CUS", "IT");

			UpdateEntryNumberColumn("CE_EntryNum ", "456", SqlDbType.VarChar, entryNumberMovementPK2);
			UpdateDeclarationColumn("JE_MessageType", "COM", SqlDbType.VarChar, declarationPK2);

			var filteredRows = GetFilteredRows("MRN");
			AssertContainsExactElementsInAnyOrder("Only EXP is valid", ["123"], filteredRows);
		}

		#endregion

		#region Implementation

		void SetUpSupplierAndDeclaration()
		{
			companyPK = TestDataCreator.CreateCompany(companyCode: "TC1", countryCode: "IT", currencyCode: "EUR");
			branchPK = TestDataCreator.CreateBranch(companyPK, branchCode: "TB1", homePort: "ITALY");

			organisationSupplierPK = TestDataCreator.CreateOrganisation("SUPPL1", "SUPPLIER");
			declarationPK = TestDataCreator.CreateJobDeclaration(branchPK, companyPK, "B00001", "EXP", 1, supplierPK: organisationSupplierPK);
		}

		void SetUpEntryFields()
		{
			entryInstructionPK = TestDataCreator.CreateCusEntryInstruction(declarationPK, "A", "", DateTime.Today, 1);
			entryHeaderPK = TestDataCreator.CreateCusEntryHeader(true, "BLT", "OK", "100", "04321-B00181816", 301, 1, "", declarationPK, DateTime.Today, DateTime.Today, entryInstructionPK, "", DateTime.Today, DateTime.Today, DateTime.Today, 1);

			entryNumberMovementPK = TestDataCreator.CreateCusEntryNum(entryHeaderPK, "CusEntryHeader", "MRN001", "MRN", "CUS", "IT");
			entryNumberRegistrationPK = TestDataCreator.CreateCusEntryNum(entryHeaderPK, "CusEntryHeader", "REG001", "REG", "CUS", "IT");
			entryNumberIviPK = TestDataCreator.CreateCusEntryNum(entryHeaderPK, "CusEntryHeader", "MRN001", "IVI", "CUS", "IT");
		}

		Guid companyPK;
		Guid branchPK;
		Guid organisationSupplierPK;
		Guid declarationPK;
		Guid entryInstructionPK;
		Guid entryHeaderPK;
		Guid entryNumberMovementPK;
		Guid entryNumberRegistrationPK;
		Guid entryNumberIviPK;

		protected override IEnumerable<(SqlDbType ParameterType, string ParameterName)> GetSqlParameters()
		{
			yield return (SqlDbType.UniqueIdentifier, Report_ITExportExitStatusParameter.CompanyPK);
			yield return (SqlDbType.SmallDateTime, Report_ITExportExitStatusParameter.AcceptanceDateFrom);
			yield return (SqlDbType.SmallDateTime, Report_ITExportExitStatusParameter.AcceptanceDateTo);
			yield return (SqlDbType.UniqueIdentifier, Report_ITExportExitStatusParameter.SupplierOrganisationPK);
			yield return (SqlDbType.UniqueIdentifier, Report_ITExportExitStatusParameter.LocalClientPK);
			yield return (SqlDbType.VarChar, Report_ITExportExitStatusParameter.DeclarationType);
			yield return (SqlDbType.VarChar, Report_ITExportExitStatusParameter.ExitState);
			yield return (SqlDbType.SmallDateTime, Report_ITExportExitStatusParameter.ExitDateFrom);
			yield return (SqlDbType.SmallDateTime, Report_ITExportExitStatusParameter.ExitDateTo);
		}

		class Report_ITExportExitStatusParameter
		{
			public const string CompanyPK = "@CompanyPK";
			public const string AcceptanceDateFrom = "@AcceptanceDateFrom";
			public const string AcceptanceDateTo = "@AcceptanceDateTo";
			public const string SupplierOrganisationPK = "@SupplierOrganisationPK";
			public const string LocalClientPK = "@LocalClientPK";
			public const string DeclarationType = "@DeclarationType";
			public const string ExitState = "@ExitState";
			public const string ExitDateFrom = "@ExitDateFrom";
			public const string ExitDateTo = "@ExitDateTo";
		}

		#endregion

	}
}
