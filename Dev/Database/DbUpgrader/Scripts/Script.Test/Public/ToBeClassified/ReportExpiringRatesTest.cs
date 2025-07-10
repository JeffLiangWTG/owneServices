using System;
using System.Collections.Generic;
using System.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.ToBeClassified;
using Enterprise.Build.Database.Script.TestFramework;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.ToBeClassified
{
	[TestedType(typeof(ReportExpiringRates))]
	class ReportExpiringRatesTest : DbCreateScriptTest
	{
		[TestDate(2016, 05, 05)]
		public void TestExpiringRatesByMode()
		{
			var parent = CreateRatingHeader();
			CreateRateEntry(parent, "AIR", "LSE");
			CreateRateEntry(parent, "FCL", "SEA");
			CreateRateEntry(parent, "LCL", "LCL");
			CreateRateEntry(parent, "ORG", "SEA");
			CreateRateEntry(parent, "ORG", "LCL");
			CreateRateEntry(parent, "ORG", "FCL");

			var results = GetResultsForMode("ALL");
			AssertContainsExactElementsInAnyOrder(new[] { "AIR-LSE", "FCL-SEA", "LCL-LCL", "ORG-SEA", "ORG-LCL", "ORG-FCL" }, results);

			results = GetResultsForMode("SEA");
			AssertContainsExactElementsInAnyOrder(new[] { "FCL-SEA", "LCL-LCL", "ORG-SEA", "ORG-LCL", "ORG-FCL" }, results);

			results = GetResultsForMode("FCL");
			AssertContainsExactElementsInAnyOrder(new[] { "FCL-SEA", "ORG-SEA", "ORG-FCL" }, results);

			results = GetResultsForMode("LCL");
			AssertContainsExactElementsInAnyOrder(new[] { "LCL-LCL", "ORG-SEA", "ORG-LCL" }, results);
		}

		#region Implementation

		#region Creating Objects

		Guid CreateRatingHeader()
		{
			var pk = Guid.NewGuid();
			var insertSql = "INSERT INTO dbo.RatingHeader(TH_PK, TH_RateType, TH_GC, TH_SystemLastEditTimeUtc, TH_SystemLastEditUser, TH_SystemCreateTimeUtc, TH_SystemCreateUser) VALUES (@pk, 'COS', @companyPK, GetUtcDate(), '~BP', GetUtcDate(), '~BP')";

			using (var command = TestConnection.Command(insertSql))
			{
				command.AddParameter("@pk", SqlDbType.UniqueIdentifier, pk);
				command.AddParameter("@companyPK", SqlDbType.UniqueIdentifier, TestDbHelper.DefaultCompanyPK);
				command.ExecuteNonQuery();
			}

			return pk;
		}

		void CreateRateEntry(Guid parent, string category, string mode)
		{
			var sql = @"INSERT INTO dbo.RateEntry (TI_PK, TI_TH, TI_GC_Publisher, TI_RateCategory, TI_Mode, TI_RateStartDate, TI_RateEndDate, TI_SystemLastEditTimeUtc, TI_SystemLastEditUser, TI_SystemCreateTimeUtc, TI_SystemCreateUser)
VALUES (@pk, @header, @companyPK, @type, @mode, @start, @end, GetUtcDate(), '~BP', GetUtcDate(), '~BP')";

			using (var command = TestConnection.Command(sql))
			{
				command.AddParameter("@pk", SqlDbType.UniqueIdentifier, Guid.NewGuid());
				command.AddParameter("@header", SqlDbType.UniqueIdentifier, parent);
				command.AddParameter("@companyPK", SqlDbType.UniqueIdentifier, TestDbHelper.DefaultCompanyPK);
				command.AddParameter("@type", SqlDbType.NVarChar, category);
				command.AddParameter("@mode", SqlDbType.NVarChar, mode);
				command.AddParameter("@start", SqlDbType.Date, DateTime.Today);
				command.AddParameter("@end", SqlDbType.Date, DateTime.Today.AddDays(10));
				command.ExecuteNonQuery();
			}
		}

		#endregion

		List<string> GetResultsForMode(string mode)
		{
			var results = new List<string>();
			var reportValues = new
			{
				CompanyPK = TestDbHelper.DefaultCompanyPK,
				ExpiringDays = 30,
				Rate = "Client Rates, Company Tariff, Costings",
				Type = "Freight, Origin, Destination",
				Mode = mode
			};

			using (var reader = Helper.RunSP("ReportExpiringRates", reportValues))
			{
				while (reader.Read())
				{
					results.Add(string.Format("{0}-{1}", reader.GetString(2), reader.GetString(3)));
				}
			}

			return results;
		}

		TestDbHelper Helper
		{
			get { return helper ?? (helper = new TestDbHelper(TestConnection)); }
		}

		TestDbHelper helper;
		#endregion
	}
}

