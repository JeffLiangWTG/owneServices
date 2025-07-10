using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.Data.Testing;
using CargoWise.DbUpgrader.Scripts.Definitions.Core.Triggers;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Core.Triggers.Testing
{
	[TestedType(typeof(TG_GlbCompanyCodeChangeUpdatesStmDataReport))]
	sealed class TG_GlbCompanyCodeChangeUpdatesStmDataReportTest : DbCreateScriptTest
	{
		#region Helpers

		internal static Guid InsertCompany(string code, string name, string currency, string country, bool isReciprocal = false, bool isGstRegistered = true)
		{
			var pk = Guid.NewGuid();
			var sql = @"INSERT INTO dbo.GlbCompany (GC_PK, GC_Code, GC_Name, GC_RX_NKLocalCurrency, GC_RN_NKCountryCode, GC_IsReciprocal, GC_IsGSTRegistered) VALUES (@PK, @Code, @Name, @Currency, @Country, @IsReciprocal, @IsGSTRegistered)";
			var cmd = Db.Connection.Command(sql);

			cmd.AddParameter("@PK", SqlDbType.UniqueIdentifier, pk);
			cmd.AddParameter("@Code", SqlDbType.Char, code);
			cmd.AddParameterBasedOnDbColumn("@Name", name, GlbCompanySchema.GC_Name);
			cmd.AddParameterBasedOnDbColumn("@Currency", currency, GlbCompanySchema.GC_RX_NKLocalCurrency);
			cmd.AddParameterBasedOnDbColumn("@Country", country, GlbCompanySchema.GC_RN_NKCountryCode);
			cmd.AddParameter("@IsReciprocal", SqlDbType.Bit, isReciprocal);
			cmd.AddParameter("@IsGSTRegistered", SqlDbType.Bit, isGstRegistered);
			cmd.ExecuteNonQuery();

			return pk;
		}

		internal static Guid InsertReport(string companyCode, string reportSuffix = "")
		{
			var pk = Guid.NewGuid();
			var sql = @"INSERT INTO dbo.StmData (SD_PK, SD_Name, SD_Type, SD_DepartmentGuid) VALUES (@PK, @Name, @Gid, @DepGuid)";
			var cmd = Db.Connection.Command(sql);

			cmd.AddParameter("@PK", SqlDbType.UniqueIdentifier, pk);
			var suffix = string.IsNullOrWhiteSpace(reportSuffix) ? string.Empty : reportSuffix;
			cmd.AddParameterBasedOnDbColumn("@Name", $"RPT_CFG${companyCode}${suffix}", StmDataSchema.SD_Name);
			cmd.AddParameterBasedOnDbColumn("@Gid", "GID", StmDataSchema.SD_Type);
			cmd.AddParameterBasedOnDbColumn("@DepGuid", Guid.NewGuid(), StmDataSchema.SD_DepartmentGuid);
			cmd.ExecuteNonQuery();

			return pk;
		}

		static void AssertReportHasCorrectCompanyCodeAndSuffix(string companyCode, Guid reportPK, string reportSuffix = "")
		{
			var queryReport = $"SELECT SD_Name FROM dbo.StmData WHERE SD_PK = '{reportPK}'";
			var queryResult = Db.Connection.ExecuteScalar(queryReport);
			AssertEquals($"RPT_CFG${companyCode}${reportSuffix}", queryResult.ToString());
		}

		#endregion

		#region SingleUpdates

		internal static void RunTriggerSingle(string oldCompanyCode, string newCompanyCode, string reportSuffix = "")
		{
			// create user with a company code
			var companyPK = InsertCompany(oldCompanyCode, "ABC Company", "Australia", "AUD");

			// create report for that user
			var reportPK = InsertReport(oldCompanyCode, reportSuffix);

			// update the company code
			var updateCompanyCode = $"UPDATE dbo.GlbCompany SET GC_Code = '{newCompanyCode}', GC_SystemLastEditUser = 'E', GC_SystemLastEditTimeUtc = GetDate() WHERE GC_PK = '{companyPK}'";
			var cmdUpdateCode = Db.Connection.Command(updateCompanyCode);
			cmdUpdateCode.ExecuteNonQuery();

			// report should have updated code
			AssertReportHasCorrectCompanyCodeAndSuffix(newCompanyCode, reportPK, reportSuffix);
		}

		[UseSnapshotProtection, ExpectNoExceptions]
		public void TestTriggerSingleCodeUpdate_Normal()
		{
			RunTriggerSingle("ABC", "XYZ");
		}

		[UseSnapshotProtection, ExpectNoExceptions]
		public void TestTriggerSingleCodeUpdate_FromLowerCase()
		{
			RunTriggerSingle("abc", "ABC");
		}

		[UseSnapshotProtection, ExpectNoExceptions]
		public void TestTriggerSingleCodeUpdate_ToLowerCase()
		{
			RunTriggerSingle("ABC", "abc");
		}

		[UseSnapshotProtection, ExpectNoExceptions]
		public void TestTriggerSingleCodeUpdate_Mixed()
		{
			RunTriggerSingle("aBC", "GHi");
		}

		#endregion

		#region MultipleUpdates

		[UseSnapshotProtection, ExpectNoExceptions]
		public void TestTriggerMultiCodeUpdate()
		{
			var companyCode1 = "ABC";
			var companyCode2 = "ABD";
			var companyCode3 = "DEF";

			// create 3 company codes
			InsertCompany(companyCode1, $"{companyCode1} Company", "Australia", "AUD");
			InsertCompany(companyCode2, $"{companyCode2} Company", "Australia", "AUD");
			InsertCompany(companyCode3, $"{companyCode3} Company", "Australia", "AUD");

			// create reports for these companies
			var reportPK1 = InsertReport(companyCode1);
			var reportPK2 = InsertReport(companyCode2);
			var reportPK3 = InsertReport(companyCode2);
			var reportPK4 = InsertReport(companyCode3);
			var reportPK5 = InsertReport(companyCode3);

			// selectively update the company codes by reversing them when they start with AB
			var updateCompanyCode = "UPDATE dbo.GlbCompany SET GC_Code = REVERSE(GC_Code), GC_SystemLastEditUser = 'E', GC_SystemLastEditTimeUtc = GetDate() WHERE GC_Code LIKE 'AB%'";
			var cmdUpdateCode = Db.Connection.Command(updateCompanyCode);
			cmdUpdateCode.ExecuteNonQuery();

			// reports should now have updated codes; ones with companyCode3 should be untouched
			AssertReportHasCorrectCompanyCodeAndSuffix(new string(companyCode1.Reverse().ToArray()), reportPK1);
			AssertReportHasCorrectCompanyCodeAndSuffix(new string(companyCode2.Reverse().ToArray()), reportPK2);
			AssertReportHasCorrectCompanyCodeAndSuffix(new string(companyCode2.Reverse().ToArray()), reportPK3);
			AssertReportHasCorrectCompanyCodeAndSuffix(companyCode3, reportPK4);
			AssertReportHasCorrectCompanyCodeAndSuffix(companyCode3, reportPK5);
		}

		#endregion

		#region FuzzyTesting

		// used for the test below
		static readonly Random rnd = new Random();

		[UseSnapshotProtection, ExpectNoExceptions]
		public void TestTriggerWithFuzzyInput()
		{
			RunFuzzyTest(100, 10);
		}

		internal static void RunFuzzyTest(int maxCompanyCodeCount, int maxReportsPerCompanyCode)
		{
			var codes = new HashSet<string>();

			// create users a bunch of company codes
			for (var i = 0; i < rnd.Next(1, maxCompanyCodeCount); ++i)
			{
				var code = new string(new[]
				{
					// This is not a typo, I am only setting the first letter to A or B to divide the set of codes into 2 sets
					(char)(rnd.Next(2)  + 'A'),
					(char)(rnd.Next(26) + 'A'),
					(char)(rnd.Next(26) + 'A')
				});

				InsertCompany(code, $"{code} Company", "Australia", "AUD");
				codes.Add(code);
			}

			// create a number of reports for each company code
			var oldReports = new Dictionary<Guid, string>();
			foreach (var code in codes)
			{
				for (var i = 0; i < rnd.Next(maxReportsPerCompanyCode); ++i)
				{
					var pk = InsertReport(code);
					oldReports.Add(pk, code);
				}
			}

			// selectively update the company codes by reversing them when they start with A
			var updateCompanyCodes = "UPDATE dbo.GlbCompany SET GC_Code = REVERSE(GC_CODE), GC_SystemLastEditUser = 'E', GC_SystemLastEditTimeUtc = GetDate() WHERE GC_CODE LIKE 'A%'";
			var cmdUpdateCode = Db.Connection.Command(updateCompanyCodes);
			cmdUpdateCode.ExecuteNonQuery();

			// in our local dictionary, reverse the strings that start with A and just copy the ones starting with B, as in the DB update above
			var newReports = new Dictionary<Guid, string>();
			oldReports.ForEach(
				s => newReports.Add(
					s.Key,
					s.Value.StartsWith("A") ? new string(s.Value.Reverse().ToArray()) : s.Value));

			// now, half our company codes should be **A, half B**, reports should also be all **A or B**.
			// we need to check the new company codes have correct and matching report codes and this is
			// just ensuring everything in the DB matches our expected values, which are in newReports
			foreach (var kvp in newReports)
			{
				AssertReportHasCorrectCompanyCodeAndSuffix(kvp.Value, kvp.Key);
			}
		}

		#endregion

		#region ExtendedReportName

		[UseSnapshotProtection, ExpectNoExceptions]
		public void TestTriggerSingleCodeUpdate_ExtendedName()
		{
			RunTriggerSingle("ABC", "BLA", "Moo");
		}
		#endregion
	}
}

