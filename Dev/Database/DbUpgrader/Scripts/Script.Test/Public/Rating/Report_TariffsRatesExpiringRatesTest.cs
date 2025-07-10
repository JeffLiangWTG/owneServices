using System;
using System.Data;
using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.Rating;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Rating
{
	[TestedType(typeof(Report_TariffsRatesExpiringRates))]
	class Report_TariffsRatesExpiringRatesTest : DbCreateScriptTest
	{
		protected override void SetUp()
		{
			base.SetUp();

			orgHeaderPK = CreateOrgHeader();
			companyPK = CreateCompany();
			ratingHeaderPK = CreateRatingHeader(companyPK);
			globalRatingHeaderPK = CreateRatingHeader(companyPK: Guid.Empty);
		}

		Guid orgHeaderPK;
		Guid companyPK;
		Guid ratingHeaderPK;
		Guid globalRatingHeaderPK;

		public void TestCountForTypes()
		{
			foreach (var ratingHeaderPK in new[] { ratingHeaderPK, globalRatingHeaderPK })
			{
				CreateRateEntry("AIR", "LSE", ratingHeaderPK);
				CreateRateEntry("FCL", "SEA", ratingHeaderPK);
				CreateRateEntry("LCL", "LCL", ratingHeaderPK);
				CreateRateEntry("ORG", "FCL", ratingHeaderPK);
				CreateRateEntry("DST", "ROA", ratingHeaderPK);
				CreateRateEntry("SCO", "SEA", ratingHeaderPK);
				CreateRateEntry("SNC", "LCL", ratingHeaderPK);
				CreateRateEntry("SED", "SEA", ratingHeaderPK);
				CreateRateEntry("SID", "SEA", ratingHeaderPK);
				CreateRateEntry("SOR", "LCL", ratingHeaderPK);
				CreateRateEntry("SDE", "FCL", ratingHeaderPK);
				CreateRateEntry("PAC", "FTL", ratingHeaderPK);
				CreateRateEntry("UNP", "RAI", ratingHeaderPK);
				CreateRateEntry("CST", "ROA", ratingHeaderPK);
				CreateRateEntry("WHS", "ALL", ratingHeaderPK);
				CreateRateEntry("TRN", "LRO", ratingHeaderPK);
				CreateRateEntry("TBC", "FRO", ratingHeaderPK);
			}

			AssertResultLinesCount("FOR", "ALL", expectedCount: 3, includeGlobal: 'N'); // AIR, FCL, LCL
			AssertResultLinesCount("ORG", "ALL", expectedCount: 1, includeGlobal: 'N');
			AssertResultLinesCount("DST", "ALL", expectedCount: 1, includeGlobal: 'N');
			AssertResultLinesCount("LRA", "ALL", expectedCount: 4, includeGlobal: 'N'); // SCO, SNC, SED, SID
			AssertResultLinesCount("SOR", "ALL", expectedCount: 1, includeGlobal: 'N');
			AssertResultLinesCount("SDE", "ALL", expectedCount: 1, includeGlobal: 'N');
			AssertResultLinesCount("CFS", "ALL", expectedCount: 3, includeGlobal: 'N'); // PAC, UNP, CST
			AssertResultLinesCount("WHS", "ALL", expectedCount: 1, includeGlobal: 'N');
			AssertResultLinesCount("TRN", "ALL", expectedCount: 2, includeGlobal: 'N'); // TRN, TBC
			AssertResultLinesCount("ALL", "ALL", expectedCount: 17, includeGlobal: 'N');

			AssertResultLinesCount("FOR", "ALL", expectedCount: 6, includeGlobal: 'Y'); // AIR, FCL, LCL
			AssertResultLinesCount("ORG", "ALL", expectedCount: 2, includeGlobal: 'Y');
			AssertResultLinesCount("DST", "ALL", expectedCount: 2, includeGlobal: 'Y');
			AssertResultLinesCount("LRA", "ALL", expectedCount: 8, includeGlobal: 'Y'); // SCO, SNC, SED, SID
			AssertResultLinesCount("SOR", "ALL", expectedCount: 2, includeGlobal: 'Y');
			AssertResultLinesCount("SDE", "ALL", expectedCount: 2, includeGlobal: 'Y');
			AssertResultLinesCount("CFS", "ALL", expectedCount: 6, includeGlobal: 'Y'); // PAC, UNP, CST
			AssertResultLinesCount("WHS", "ALL", expectedCount: 2, includeGlobal: 'Y');
			AssertResultLinesCount("TRN", "ALL", expectedCount: 4, includeGlobal: 'Y'); // TRN, TBC
			AssertResultLinesCount("ALL", "ALL", expectedCount: 34, includeGlobal: 'Y');
		}

		public void TestCountForModes()
		{
			foreach (var ratingHeaderPK in new[] { ratingHeaderPK, globalRatingHeaderPK })
			{
				CreateRateEntry("AIR", "LSE", ratingHeaderPK);
				CreateRateEntry("AIR", "ULD", ratingHeaderPK);
				CreateRateEntry("FCL", "SEA", ratingHeaderPK);
				CreateRateEntry("DST", "FCL", ratingHeaderPK);
				CreateRateEntry("ORG", "LCL", ratingHeaderPK);
				CreateRateEntry("FCL", "ROA", ratingHeaderPK);
				CreateRateEntry("DST", "FRO", ratingHeaderPK);
				CreateRateEntry("LCL", "LRO", ratingHeaderPK);
				CreateRateEntry("UNP", "FTL", ratingHeaderPK);
				CreateRateEntry("CST", "RAI", ratingHeaderPK);
				CreateRateEntry("DST", "FRA", ratingHeaderPK);
				CreateRateEntry("DST", "FWL", ratingHeaderPK);
			}

			AssertResultLinesCount("ALL", "AIR", expectedCount: 2, includeGlobal: 'N');
			AssertResultLinesCount("ALL", "LSE", expectedCount: 1, includeGlobal: 'N');
			AssertResultLinesCount("ALL", "ULD", expectedCount: 1, includeGlobal: 'N');
			AssertResultLinesCount("ALL", "SEA", expectedCount: 3, includeGlobal: 'N');
			AssertResultLinesCount("ALL", "ROA", expectedCount: 4, includeGlobal: 'N');
			AssertResultLinesCount("ALL", "RAI", expectedCount: 3, includeGlobal: 'N');
			AssertResultLinesCount("ALL", "ALL", expectedCount: 12, includeGlobal: 'N');

			AssertResultLinesCount("ALL", "AIR", expectedCount: 4, includeGlobal: 'Y');
			AssertResultLinesCount("ALL", "LSE", expectedCount: 2, includeGlobal: 'Y');
			AssertResultLinesCount("ALL", "ULD", expectedCount: 2, includeGlobal: 'Y');
			AssertResultLinesCount("ALL", "SEA", expectedCount: 6, includeGlobal: 'Y');
			AssertResultLinesCount("ALL", "ROA", expectedCount: 8, includeGlobal: 'Y');
			AssertResultLinesCount("ALL", "RAI", expectedCount: 6, includeGlobal: 'Y');
			AssertResultLinesCount("ALL", "ALL", expectedCount: 24, includeGlobal: 'Y');
		}

		public void TestReportResultsWithinSpecifiedPeriod()
		{
			CreateRateEntry("PAC", "FTL", ratingHeaderPK, endDays: 1);
			CreateRateEntry("DST", "FRA", ratingHeaderPK, endDays: 29);
			CreateRateEntry("SNC", "LCL", ratingHeaderPK, endDays: 31);

			AssertEquals
			(
				expected: string.Format("Client Rate Type: DST Mode: FRA Start Date: {0} End Date: {1}\r\nClient Rate Type: PAC Mode: FTL Start Date: {0} End Date: {2}", DateTime.Today, DateTime.Today.AddDays(29), DateTime.Today.AddDays(1)),
				actual: GetActualResultString(type: "ALL", mode: "ALL", expiringDays: 30)
			);
		}

		void AssertResultLinesCount(string type, string mode, int expectedCount, char includeGlobal, string message = default)
		{
			var cmd = TestConnection.Command("dbo.Report_TariffsRatesExpiringRates");
			cmd.CommandType = CommandType.StoredProcedure;
			cmd.AddParameter("@CompanyPK", SqlDbType.UniqueIdentifier, companyPK);
			cmd.AddParameter("@ExpiringDays", SqlDbType.Decimal, 30.0);
			cmd.AddParameter("@Rate", SqlDbType.VarChar, "Client Rates, Company Tariff, Costings");
			cmd.AddParameter("@Type", SqlDbType.Char, type);
			cmd.AddParameter("@Mode", SqlDbType.Char, mode);
			cmd.AddParameter("@IncludeGlobal", SqlDbType.Char, includeGlobal);

			var resultCount = 0;
			using (var reader = cmd.ExecuteReader())
			{
				while (reader.Read())
				{
					resultCount++;
				}
			}

			AssertEquals(message, expectedCount, resultCount);
		}

		string GetActualResultString(string type, string mode, int expiringDays)
		{
			var cmd = TestConnection.Command("dbo.Report_TariffsRatesExpiringRates");
			cmd.CommandType = CommandType.StoredProcedure;
			cmd.AddParameter("@CompanyPK", SqlDbType.UniqueIdentifier, companyPK);
			cmd.AddParameter("@ExpiringDays", SqlDbType.Decimal, expiringDays);
			cmd.AddParameter("@Rate", SqlDbType.VarChar, "Client Rates, Company Tariff, Costings");
			cmd.AddParameter("@Type", SqlDbType.Char, type);
			cmd.AddParameter("@Mode", SqlDbType.Char, mode);

			var result = "";
			using (var reader = cmd.ExecuteReader())
			{
				while (reader.Read())
				{
					result += reader["Name"];
					result += " Type: " + reader["Type"];
					result += " Mode: " + reader["Mode"];
					result += " Start Date: " + reader["StartDate"];
					result += " End Date: " + reader["EndDate"];

					result += System.Environment.NewLine;
				}
			}

			return result.TrimEnd();
		}

		#region Setup Methods

		Guid CreateOrgHeader()
		{
			var pk = Guid.NewGuid();
			var insertSql = "insert into dbo.OrgHeader (OH_PK, OH_Code) values (@pk, 'RELPA1')";

			using (var command = Db.Connection.Command(insertSql))
			{
				command.AddParameter("@pk", SqlDbType.UniqueIdentifier, pk);
				command.ExecuteNonQuery();
			}

			return pk;
		}

		Guid CreateRatingHeader(Guid companyPK)
		{
			var hasCompanyPK = companyPK != Guid.Empty;

			var pk = Guid.NewGuid();
			var insertSql = $@"INSERT INTO dbo.RatingHeader(TH_PK, TH_OH, TH_RateType {(hasCompanyPK ? ", TH_GC" : "")}, TH_SystemLastEditTimeUtc, TH_SystemLastEditUser, TH_SystemCreateTimeUtc, TH_SystemCreateUser)
	VALUES (@pk, @headerPK, 'SAL' {(hasCompanyPK ? ", @companyPK" : "")}, GetUtcDate(), '~BP', GetUtcDate(), '~BP')";

			using (var command = Db.Connection.Command(insertSql))
			{
				command.AddParameter("@pk", SqlDbType.UniqueIdentifier, pk);
				command.AddParameter("@headerPK", SqlDbType.UniqueIdentifier, orgHeaderPK);
				if (hasCompanyPK)
				{
					command.AddParameter("@companyPK", SqlDbType.UniqueIdentifier, companyPK);
				}
				command.ExecuteNonQuery();
			}

			return pk;
		}

		Guid CreateCompany()
		{
			var pk = Guid.NewGuid();
			var insertSql = "INSERT INTO dbo.GlbCompany(GC_PK, GC_Code, GC_Name, GC_RN_NKCountryCode, GC_RX_NKLocalCurrency) VALUES (@pk, 'DAN', 'AU company', 'AU', 'AUD')";

			using (var command = Db.Connection.Command(insertSql))
			{
				command.AddParameter("@pk", SqlDbType.UniqueIdentifier, pk);
				command.ExecuteNonQuery();
			}

			return pk;
		}

		void CreateRateEntry(string type, string mode, Guid ratingHeaderPK, int endDays = 6)
		{
			var sql = @"INSERT INTO dbo.RateEntry(TI_PK, TI_TH, TI_GC_Publisher, TI_RateCategory, TI_Mode, TI_RateStartDate, TI_RateEndDate, TI_SystemLastEditTimeUtc, TI_SystemLastEditUser, TI_SystemCreateTimeUtc, TI_SystemCreateUser)
	VALUES (@pk, @header, @companyPK, @type, @mode, @startDate, @endDate, GetUtcDate(), '~BP', GetUtcDate(), '~BP')";

			using (var command = TestConnection.Command(sql))
			{
				command.AddParameter("@pk", SqlDbType.UniqueIdentifier, Guid.NewGuid());
				command.AddParameter("@header", SqlDbType.UniqueIdentifier, ratingHeaderPK);
				command.AddParameter("@companyPK", SqlDbType.UniqueIdentifier, companyPK);
				command.AddParameter("@type", SqlDbType.NVarChar, type);
				command.AddParameter("@mode", SqlDbType.NVarChar, mode);
				command.AddParameter("@startDate", SqlDbType.Date, DateTime.Today);
				command.AddParameter("@endDate", SqlDbType.Date, DateTime.Today.AddDays(endDays));
				command.ExecuteNonQuery();
			}
		}
		#endregion
	}
}

