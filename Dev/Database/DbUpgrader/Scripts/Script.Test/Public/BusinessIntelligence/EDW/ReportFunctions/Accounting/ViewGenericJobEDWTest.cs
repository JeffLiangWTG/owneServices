using System;
using System.Data;
using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.BusinessIntelligence.EDW.ReportFunctions.Accounting;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.BusinessIntelligence.EDW.ReportFunctions.Accounting.Testing
{
	[TestedType(typeof(ViewGenericJob))]
	class ViewGenericJobEDWTest : BiCreateScriptTest
	{
		/// <summary>
		/// Basic auto-generated test.
		/// Please replace it with more elaborated tests.
		/// </summary>

		[ExpectNoExceptions]
		public void TestSampleCall()
		{
			CreateJobConsol(Guid.NewGuid(), "C00001234", 1, 1);
			CreateJobShipment(Guid.NewGuid(), "S00001111", 0, 11);
			string sql = string.Format(
				@"SELECT * FROM [{0}].[dbo].[ViewGenericJob]",
				ScriptDbName);
			DataTable result = DataUtils.GetDataTableFromQuery(TestConnection, sql);
			AssertEquals("Result should have rows", 1, result.Rows.Count);
		}

		public void TestJobShipment()
		{
			CreateJobShipment(Guid.NewGuid(), "S00001234", 1, 12);
			string sql = string.Format(
				@"SELECT * FROM [{0}].[dbo].[ViewGenericJob]",
				ScriptDbName);
			DataTable result = DataUtils.GetDataTableFromQuery(TestConnection, sql);
			AssertEquals("Result should have rows", 1, result.Rows.Count);
			AssertEquals("Shipment Job", "S00001234", result.Rows[0]["VJ_JobNumber"].ToString());
			AssertEquals("Shipment Job Company PK", string.Empty, result.Rows[0]["VJ_CompanyPK"].ToString());
		}

		public void TestJobSundryChargesActuallyReturnsJobNumber()
		{
			CreateJobSundryCharges(Guid.NewGuid(), "SC00000001", 21);
			CreateJobSundryCharges(Guid.NewGuid(), "SC00000002", 22);
			string sql = string.Format(
				@"SELECT * FROM [{0}].[dbo].[ViewGenericJob] order by vj_jobnumber",
				ScriptDbName);
			DataTable result = DataUtils.GetDataTableFromQuery(TestConnection, sql);
			AssertEquals("Result should have 2 rows", 2, result.Rows.Count);
			AssertEquals("First row should have correct job number", "SC00000001", result.Rows[0]["VJ_JobNumber"]);
			AssertEquals("Second row should have correct job number", "SC00000002", result.Rows[1]["VJ_JobNumber"]);
		}

		public void TestViewGenericJobIncludeAllJobs()
		{
			CreateCompany("TC1", "AU", "AUD");
			CreateBranch(101, "TB1", 1);
			CreateJobDeclaration(1001, "B0002", 0, 32);
			CreateJobDeclaration(1001, "B0001", 1, 31);
			CreateJobShipment(Guid.NewGuid(), "S00001234", 1, 14);
			CreateJobShipment(Guid.NewGuid(), "S00001235", 0, 15);
			string sql = string.Format(
				@"SELECT * FROM [{0}].[dbo].[ViewGenericJob] ORDER BY VJ_JobNumber, VJ_IsInactive",
				ScriptDbName);
			DataTable result = DataUtils.GetDataTableFromQuery(TestConnection, sql);
			AssertEquals("Result should have two rows", 4, result.Rows.Count);
			AssertEquals("First row should be the declaration", "B0001", result.Rows[0]["VJ_JobNumber"]);
			AssertEquals("Second row should be the declaration", "B0002", result.Rows[1]["VJ_JobNumber"]);
			AssertEquals("Third row should be the shipment", "S00001234", result.Rows[2]["VJ_JobNumber"]);
			AssertEquals("Fourth row should be the shipment", "S00001235", result.Rows[3]["VJ_JobNumber"]);
		}

		void CreateJobShipment(Guid shipmentID, string jobNumber, int isCancelled, int shipmentKey)
		{
			string sql = string.Format(
				@"INSERT INTO [{0}].[InternationalLogistics].[BAS__Shipment] (ShipmentID, JobNumber, IsCancelled, ShipmentKey) VALUES ('{1}', '{2}', {3}, {4})",
				ScriptDbName, shipmentID, jobNumber, isCancelled, shipmentKey);

			TestConnection.ExecuteNonQuery(sql);
		}

		void CreateJobConsol(Guid? consolidationID, string jobNumber, int isCancelled, int consolidationKey)
		{
			string sql = string.Format(
				@"INSERT INTO [{0}].[InternationalLogistics].[BAS__Consolidation] (ConsolidationID, JobNumber, IsCancelled, ConsolidationKey) VALUES ('{1}', '{2}', {3}, {4})",
				ScriptDbName, consolidationID, jobNumber, isCancelled, consolidationKey);

			TestConnection.ExecuteNonQuery(sql);
		}

		public void CreateCompany(string companyCode, string countryCode, string currencyCode)
		{
			var companyPK = Guid.NewGuid();

			string sql = string.Format(
				@"INSERT INTO [{0}].[Organization].[BAS__Company] (CompanyCode, CountryCode, LocalCurrency, CompanyID, CompanyKey) VALUES ('{1}', '{2}', '{3}', '{4}', {5})",
				ScriptDbName, companyCode, countryCode, currencyCode, companyPK, 101);

			TestConnection.ExecuteNonQuery(sql);
		}

		void CreateBranch(int companyKey, string branchCode, int homePortKey)
		{
			var branchPK = Guid.NewGuid();

			string sql = string.Format(
				@"INSERT INTO [{0}].[Organization].[BAS__Branch] (BranchID, CompanyKey, BranchCode, HomePortKey, BranchKey) VALUES ('{1}', {2}, '{3}', {4}, {5})",
				ScriptDbName, branchPK, companyKey, branchCode, homePortKey, 1001);

			TestConnection.ExecuteNonQuery(sql);
		}

		Guid CreateJobDeclaration(int branchKey, string declarationReference, int isInactive, int declarationKey)
		{
			var declarationPK = Guid.NewGuid();
			string sql = string.Format(
				@"INSERT INTO [{0}].[Customs].[BAS__Declaration] (DeclarationID, DeclarationKey, BranchKey, JobNumber, IsInactive) VALUES ('{1}', {2}, {3} ,'{4}' ,{5})",
				ScriptDbName, declarationPK, declarationKey, branchKey, declarationReference, isInactive);

			TestConnection.ExecuteNonQuery(sql);
			return declarationPK;
		}

		void CreateJobSundryCharges(Guid? jobSundryChargesID, string jobNumber, int jobSundryChargesKey)
		{
			string sql = string.Format(
				@"INSERT INTO [{0}].[Finance].[BAS__JobSundryCharges] (JobSundryChargesID, JobNumber, JobSundryChargesKey) VALUES ('{1}', '{2}', {3})",
				ScriptDbName, jobSundryChargesID, jobNumber, jobSundryChargesKey);

			TestConnection.ExecuteNonQuery(sql);
		}

		protected override string ScriptDbName
		{
			get { return Db.EdwDatabaseName; }
		}
	}
}

