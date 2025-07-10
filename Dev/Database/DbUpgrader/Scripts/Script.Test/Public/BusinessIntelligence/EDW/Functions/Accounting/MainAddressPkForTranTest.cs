using System;
using System.Data;
using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.BusinessIntelligence.EDW.Functions.Accounting.ComplianceReport;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.BusinessIntelligence.EDW.Functions.Accounting.Testing
{
	[TestedType(typeof(MainAddressPkForTran))]
	class MainAddressPkForTranTest : BiCreateScriptTest
	{
		[TestDate(2021, 9, 20)]
		public void TestMainAddressPkForTranWithoutCallRankingAddresses_APM()
		{
			PrepareTestDataForTranWithoutCallRankingAddresses();

			var result = RunScript("AR", arInvoiceKey, addressAPMKey, "AU", companyKey, branchKey);
			AssertRecordExistAndExpectedKey(result, addressAPMKey);
		}

		[TestDate(2021, 9, 20)]
		public void TestMainAddressPkForTranWithoutCallRankingAddresses_ARM()
		{
			PrepareTestDataForTranWithoutCallRankingAddresses();
			UpdateJobWithAddressKey();

			var result = RunScript("AR", arInvoiceKey, null, "AU", companyKey, branchKey);
			AssertRecordExistAndExpectedKey(result, addressARMKey);
		}

		[TestDate(2021, 9, 20)]
		public void TestMainAddressPkForTranWithoutCallRankingAddresses_PST()
		{
			PrepareTestDataForTranWithoutCallRankingAddresses();
			UpdateJobWithAddressKey();
			UpdateJobWithLocalAgentAddressKey();

			var result = RunScript("AR", arInvoiceKey, null, "AU", companyKey, branchKey);
			AssertRecordExistAndExpectedKey(result, addressPSTKey);
		}

		[TestDate(2021, 9, 20)]
		public void TestMainAddressPkForTranWithCallRankingAddresses_AR()
		{
			PrepareTestDataForTranWithCallRankingAddresses();

			var result = RunScript("AR", arInvoiceKey, null, "AU", companyKey, branchKey);
			AssertRecordExistAndExpectedKey(result, addressARMKey);
		}

		[TestDate(2021, 9, 20)]
		public void TestMainAddressPkForTranWithCallRankingAddresses_AP()
		{
			PrepareTestDataForTranWithCallRankingAddresses();

			var result = RunScript("AP", apInvoiceKey, null, "AU", companyKey, branchKey);
			AssertRecordExistAndExpectedKey(result, addressAPMKey);
		}

		[TestDate(2021, 9, 20)]
		public void TestMainAddressPkForTranWithCallRankingAddresses_GL()
		{
			PrepareTestDataForTranWithCallRankingAddresses();

			var result = RunScript("GL", glInvoiceKey, null, "AU", companyKey, branchKey);
			AssertRecordExistAndExpectedKey(result, addressPSTKey);
		}

		[TestDate(2021, 9, 20)]
		public void TestMainAddressPkForTranWithCallRankingAddresses_NonMainPostalAddress()
		{
			PrepareTestDataForTranWithCallRankingAddresses();
			UpdatePSTOrgAddressCapabilityAsNonMain();

			var result = RunScript("GL", glInvoiceKey, null, "AU", companyKey, branchKey);
			AssertRecordExistAndExpectedKey(result, addressOFCKey);
		}

		void AssertRecordExistAndExpectedKey(DataTable result, long expectedKey)
		{
			AssertEquals("The result should be 1 row", 1, result.Rows.Count);
			AssertEquals($"The main address pk should be {expectedKey}", expectedKey, result.Rows[0][0]);
		}

		protected override string ScriptDbName => Db.EdwDatabaseName;

		#region Implementation

		DataTable RunScript(string ledger, long transactionPK, long? invoiceAddressPK, string countryCode, long companyPK, long branchPK)
		{
			var sql = $@"SELECT * FROM [{ScriptDbName}].[dbo].MainAddressPkForTran('{ledger}', '{transactionPK}', {(invoiceAddressPK == null ? "NULL" : $"'{invoiceAddressPK}'")}, '{countryCode}', '{companyPK}', '{branchPK}')";
			return DataUtils.GetDataTableFromQuery(TestConnection, sql);
		}

		void PrepareBaseTestData()
		{
			companyKey = TestHelper.InsertCompanyAndGetCompanyKey(companyPK, "AUD", "AU");
			DeleteAllBranches();
			TestHelper.CreateBASBranch(branchKey, branchPK, companyKey: (int)companyKey);
			orgHeaderKey = TestHelper.InsertOrganization("1stCarDiv", orgPK);

			addressARMKey = TestHelper.InsertAddress("SYD", orgHeaderKey, "YY", portCode: "AUSYD");
			TestHelper.InsertOrganizationAddressCapability(addressARMKey, "ARM", isMainAddress: 1);

			addressAPMKey = TestHelper.InsertAddress("SYD", orgHeaderKey, "YY", portCode: "AUSYD");
			TestHelper.InsertOrganizationAddressCapability(addressAPMKey, "APM", isMainAddress: 1);

			addressPSTKey = TestHelper.InsertAddress("SYD", orgHeaderKey, "YY", portCode: "AUSYD");
			TestHelper.InsertOrganizationAddressCapability(addressPSTKey, "PST", isMainAddress: 1);

			addressOFCKey = TestHelper.InsertAddress("SYD", orgHeaderKey, "YY", portCode: "AUSYD");
			TestHelper.InsertOrganizationAddressCapability(addressOFCKey, "OFC", isMainAddress: 1);

			jobKey = TestHelper.CreateJobHeader("s00001234", companyPK);
		}

		void PrepareTestDataForTranWithoutCallRankingAddresses()
		{
			PrepareBaseTestData();

			arInvoiceKey = TestHelper.CreateGLTransaction("GL", orgHeaderKey, "UYU", "INV", 0, jobKey);
		}

		void PrepareTestDataForTranWithCallRankingAddresses()
		{
			PrepareBaseTestData();
			UpdateOrgIsGlbAccount();

			arInvoiceKey = TestHelper.CreateGLTransaction("AR", orgHeaderKey, "UYU", "INV", 0, jobKey);
			apInvoiceKey = TestHelper.CreateGLTransaction("AP", orgHeaderKey, "UYU", "INV", 0, jobKey);

			glInvoiceKey = TestHelper.CreateGLTransaction("GL", orgHeaderKey, "UYU", "INV", 0, jobKey);
		}

		void DeleteAllBranches()
		{
			var sql = $@"Delete from [{ScriptDbName}].[Organization].[BAS__Branch]";
			TestConnection.ExecuteNonQuery(sql);
		}

		void UpdateOrgIsGlbAccount()
		{
			var sql = $@"
				UPDATE [{ScriptDbName}].[Organization].[BAS__Organization]
				SET [IsGlobalAccount] = 1
				WHERE [OrganizationKey] = {orgHeaderKey}";
			TestConnection.ExecuteNonQuery(sql);
		}

		void UpdateJobWithAddressKey()
		{
			var sql = $@"
				UPDATE [{ScriptDbName}].[Finance].[BAS__JobHeader]
				SET [AddressKey] = {addressARMKey}
				WHERE [JobHeaderKey] = {jobKey}";
			TestConnection.ExecuteNonQuery(sql);
		}

		void UpdateJobWithLocalAgentAddressKey()
		{
			var sql = $@"
				UPDATE [{ScriptDbName}].[Finance].[BAS__JobHeader]
				SET [LocalAgentAddressKey] = {addressPSTKey}
				WHERE [JobHeaderKey] = {jobKey}";
			TestConnection.ExecuteNonQuery(sql);
		}

		void UpdatePSTOrgAddressCapabilityAsNonMain()
		{
			var sql = $@"
				UPDATE [{ScriptDbName}].[Organization].[BAS__OrganizationAddressCapability]
				SET [IsMainAddress] = 0
				WHERE [OrganizationAddressKey] = {addressPSTKey}";
			TestConnection.ExecuteNonQuery(sql);
		}

		long arInvoiceKey, apInvoiceKey, glInvoiceKey;
		long addressARMKey, addressAPMKey, addressPSTKey, addressOFCKey;
		long companyKey;
		readonly int branchKey = 1;
		int orgHeaderKey;
		long jobKey;

		AccountingFunctionTestingHelper TestHelper => fTestHelper ?? (fTestHelper = new AccountingFunctionTestingHelper(TestConnection, ScriptDbName));
		AccountingFunctionTestingHelper fTestHelper;

		readonly Guid companyPK = Guid.Parse("58350488-82BA-4BB9-A871-BE96DB5F6913");
		readonly Guid branchPK = Guid.Parse("098A5D4D-F889-445A-BF65-6253A9989442");
		readonly Guid orgPK = Guid.Parse("8FA53B87-091E-402B-9C8C-7E207F22CAA1");
		#endregion
	}
}
