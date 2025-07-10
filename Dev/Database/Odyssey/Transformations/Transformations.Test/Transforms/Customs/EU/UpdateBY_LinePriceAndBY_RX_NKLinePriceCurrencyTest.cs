using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;
using CargoWise.Data;
using Enterprise.Build.Database.Script.TestFramework;
using Enterprise.DbUpgrader.Shared;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.DbUpgrader.Transformation.DataModification.Testing;
using Enterprise.DbUpgrader.Transformations.Transforms.Customs.EU;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Transformations.Test.Transforms.Customs.EU
{
	[TestedType(typeof(UpdateBY_LinePriceAndBY_RX_NKLinePriceCurrency))]
	sealed class UpdateBY_LinePriceAndBY_RX_NKLinePriceCurrencyTest : DataTransformationTestCase
	{
		protected override DataTransformation GetNewTestTransformationInstance()
		{
			return new UpdateBY_LinePriceAndBY_RX_NKLinePriceCurrency();
		}

		public void TestCusInBondCargoDescLinePriceAndCurrencyTrigger()
		{
			//Arrange
			var transformation = new UpdateBY_LinePriceAndBY_RX_NKLinePriceCurrency();
			transformation.Run(TransformationSection.OnlinePreUpgrade, CancellationToken.None);
			Assert("Trigger created in OnlinePreUpgrade", DbObjectCreator.TriggerExists(Db.Connection, CusInBondCargoDescSchema.Constants.TableName, UpdateBY_LinePriceAndBY_RX_NKLinePriceCurrency.CusInBondCargoDescLinePriceAndCurrencyTriggerName));

			// Act
			PrepareTestData();

			//Assert
			AssertTransformationResults();

			transformation.Run(TransformationSection.OfflinePreUpgrade, CancellationToken.None);
			AssertEquals("Trigger cleaned up in OfflinePreUpgrade", false, DbObjectCreator.TriggerExists(Db.Connection, CusInBondCargoDescSchema.Constants.TableName, UpdateBY_LinePriceAndBY_RX_NKLinePriceCurrency.CusInBondCargoDescLinePriceAndCurrencyTriggerName));
		}

		public void TestGlbCompanyLinePriceCurrencyTrigger()
		{
			//Arrange
			var transformation = new UpdateBY_LinePriceAndBY_RX_NKLinePriceCurrency();
			transformation.Run(TransformationSection.OnlinePreUpgrade, CancellationToken.None);
			Assert("Trigger created in OnlinePreUpgrade", DbObjectCreator.TriggerExists(Db.Connection, GlbCompanySchema.Constants.TableName, UpdateBY_LinePriceAndBY_RX_NKLinePriceCurrency.GlbCompanyLinePriceCurrencyTriggerName));

			PrepareTestData();

			// Act
			ChangeCompanyCountry("BBE", "CH");
			ChangeCompanyCountry("CCH", "BE");
			ChangeCompanyCountry("DDE", "GB");
			ChangeCompanyCountry("EES", "NO");
			ChangeCompanyCountry("FFR", "PL");
			ChangeCompanyCountry("IIE", "TR");

			//Assert
			var results = ReadData();
			AssertTransformationResult("BE", countryInBondCargoDescBE, results, 1.1m, "CHF", true);
			AssertTransformationResult("BENC4", countryInBondCargoDescBENC4, results, 0m, string.Empty, false);
			AssertTransformationResult("BEArrival", countryInBondCargoDescBEArrival, results, 0m, string.Empty, false);
			AssertTransformationResult("CH", countryInBondCargoDescCH, results, 2.2m, "EUR", true);
			AssertTransformationResult("DE", countryInBondCargoDescDE, results, 3.3m, "GBP", true);
			AssertTransformationResult("ES", countryInBondCargoDescES, results, 4.4m, "NOK", true);
			AssertTransformationResult("FR", countryInBondCargoDescFR, results, 5.5m, "PLN", true);
			AssertTransformationResult("IE", countryInBondCargoDescIE, results, 7.7m, "TRY", true);

			transformation.Run(TransformationSection.OfflinePreUpgrade, CancellationToken.None);
			AssertEquals("Trigger cleaned up in OfflinePreUpgrade", false, DbObjectCreator.TriggerExists(Db.Connection, GlbCompanySchema.Constants.TableName, UpdateBY_LinePriceAndBY_RX_NKLinePriceCurrency.GlbCompanyLinePriceCurrencyTriggerName));
		}

		protected override void PrepareTestData()
		{
			countryInBondCargoDescBE = CreateCountryInBondCargoDesc("BBE", "BE", "EUR", "BRS", "jobBE", "NC5", "D", 1.1m);
			countryInBondCargoDescCH = CreateCountryInBondCargoDesc("CCH", "CH", "CHF", "ZRH", "jobCH", "NC5", "D", 2.2m);
			countryInBondCargoDescDE = CreateCountryInBondCargoDesc("DDE", "DE", "EUR", "BER", "jobDE", "NC5", "D", 3.3m);
			countryInBondCargoDescES = CreateCountryInBondCargoDesc("EES", "ES", "EUR", "BAR", "jobES", "NC5", "D", 4.4m);
			countryInBondCargoDescFR = CreateCountryInBondCargoDesc("FFR", "FR", "EUR", "PRS", "jobFR", "NC5", "D", 5.5m);
			countryInBondCargoDescGB = CreateCountryInBondCargoDesc("GGB", "GB", "GBP", "LND", "jobGB", "NC5", "D", 6.6m);
			countryInBondCargoDescIE = CreateCountryInBondCargoDesc("IIE", "IE", "EUR", "DBL", "jobIE", "NC5", "D", 7.7m);
			countryInBondCargoDescIT = CreateCountryInBondCargoDesc("IIT", "IT", "EUR", "MLN", "jobIT", "NC5", "D", 8.8m);
			countryInBondCargoDescNL = CreateCountryInBondCargoDesc("NNL", "NL", "EUR", "AMD", "jobNL", "NC5", "D", 9.9m);
			countryInBondCargoDescNO = CreateCountryInBondCargoDesc("NNO", "NO", "NOK", "OSL", "jobNO", "NC5", "D", 10.1m);
			countryInBondCargoDescPL = CreateCountryInBondCargoDesc("PPL", "PL", "PLN", "WRS", "jobPL", "NC5", "D", 11.11m);
			countryInBondCargoDescTR = CreateCountryInBondCargoDesc("TTR", "TR", "TRL", "STB", "jobTR", "NC5", "D", 12.12m);
			countryInBondCargoDescAT = CreateCountryInBondCargoDesc("AAT", "AT", "EUR", "VNN", "jobAT", "NC5", "D", 13.13m);
			var beBranchPk = GetBranch("BRS");
			countryInBondCargoDescBENC4 = CreateCountryInBondCargoDesc("jobBENC4", beBranchPk, "NC4", "D", 14.14m);
			countryInBondCargoDescBEArrival = CreateCountryInBondCargoDesc("jobBEArrival", beBranchPk, "NC5", "A", 15.15m);
		}

		protected override void AssertTransformationResults()
		{
			var results = ReadData();

			AssertContainsExactElementsInAnyOrder(new[] { countryInBondCargoDescBE,
				countryInBondCargoDescCH,
				countryInBondCargoDescDE,
				countryInBondCargoDescES,
				countryInBondCargoDescFR,
				countryInBondCargoDescGB,
				countryInBondCargoDescIE,
				countryInBondCargoDescIT,
				countryInBondCargoDescNL,
				countryInBondCargoDescNO,
				countryInBondCargoDescPL,
				countryInBondCargoDescTR,
				countryInBondCargoDescAT,
				countryInBondCargoDescBENC4,
				countryInBondCargoDescBEArrival,
			}, results.Select(e => e.Key));

			AssertTransformationResult("BE", countryInBondCargoDescBE, results, 1.1m, "EUR", true);
			AssertTransformationResult("CH", countryInBondCargoDescCH, results, 2.2m, "CHF", true);
			AssertTransformationResult("DE", countryInBondCargoDescDE, results, 3.3m, "EUR", true);
			AssertTransformationResult("ES", countryInBondCargoDescES, results, 4.4m, "EUR", true);
			AssertTransformationResult("FR", countryInBondCargoDescFR, results, 5.5m, "EUR", true);
			AssertTransformationResult("GB", countryInBondCargoDescGB, results, 6.6m, "GBP", true);
			AssertTransformationResult("IE", countryInBondCargoDescIE, results, 7.7m, "EUR", true);
			AssertTransformationResult("IT", countryInBondCargoDescIT, results, 8.8m, "EUR", true);
			AssertTransformationResult("NL", countryInBondCargoDescNL, results, 9.9m, "EUR", true);
			AssertTransformationResult("NO", countryInBondCargoDescNO, results, 10.1m, "NOK", true);
			AssertTransformationResult("PL", countryInBondCargoDescPL, results, 11.11m, "PLN", true);
			AssertTransformationResult("TR", countryInBondCargoDescTR, results, 12.12m, "TRY", true);
			AssertTransformationResult("AT", countryInBondCargoDescAT, results, 0m, string.Empty, false);
			AssertTransformationResult("BENC4", countryInBondCargoDescBENC4, results, 0m, string.Empty, false);
			AssertTransformationResult("BEArrival", countryInBondCargoDescBEArrival, results, 0m, string.Empty, false);
		}

		void AssertTransformationResult(string message, Guid cusInBondCargoDescPk, IReadOnlyDictionary<Guid, (decimal linePrice, string linePriceCurrency, DateTime lastUpdated, string lastUpdatedBy)> results, decimal expectedLinePrice, string expectedLinePriceCurrency, bool expectedAuditUpdated)
		{
			Assert("CusInBondDesc exists", results.TryGetValue(cusInBondCargoDescPk, out var result));
			CombineAssertions(message, () =>
			{
				AssertEquals("LinePrice", expectedLinePrice, result.linePrice);
				AssertEquals("LinePriceCurrency", expectedLinePriceCurrency, result.linePriceCurrency);
				AssertEquals("LastUpdatedBy", expectedAuditUpdated ? "E" : "~BP", result.lastUpdatedBy);
				if (expectedAuditUpdated)
				{
					AssertNotEquals("Last Updated Date changed", LastUpdateDateTime, result.lastUpdated);
				}
				else
				{
					AssertEquals("Last Updated Date not changed ", LastUpdateDateTime, result.lastUpdated);
				}
			});
		}

		Dictionary<Guid, (decimal linePrice, string linePriceCurrency, DateTime lastUpdated, string lastUpdatedBy)> ReadData()
		{
			var results = new Dictionary<Guid, (decimal linePrice, string linePriceCurrency, DateTime lastUpdated, string lastUpdatedBy)>();

			TestConnection.ExecuteReader("SELECT * FROM dbo.CusInBondCargoDesc",
				reader => results.Add((Guid)reader["BY_PK"], ((decimal)reader["BY_LinePrice"], (string)reader["BY_RX_NKLinePriceCurrency"], (DateTime)reader["BY_SystemLastEditTimeUtc"], (string)reader["BY_SystemLastEditUser"])));
			return results;
		}

		static void ChangeCompanyCountry(string companyCode, string countryCode)
		{
			using var command = Db.Connection.Command($"UPDATE dbo.GlbCompany SET GC_RN_NKCountryCode = '{countryCode}', GC_SystemLastEditTimeUtc = GETDATE(), GC_SystemLastEditUser = '~BP' WHERE GC_Code = @companyCode");
			command.AddParameter("@companyCode", SqlDbType.VarChar, GlbCompanySchema.GC_Code.MaxLength, companyCode);
			command.ExecuteNonQuery();
		}

		static Guid CreateCountryInBondCargoDesc(string companyCode, string countryCode, string currencyCode, string branchCode, string jobNumber, string applicationCode, string headerType, decimal monetaryValue)
		{
			var companyPk = TestDataCreator.CreateCompany(companyCode, countryCode, currencyCode);
			var branchPk = TestDataCreator.CreateBranch(companyPk, branchCode, "Port1", countryCode);
			var headerPK = TestDataCreator.CreateCusInbondHeader(jobNumber, branchPk, applicationCode, headerType);
			var cusInbondBillPk = TestDataCreator.CreateCusInbondBill(headerPK);
			var cusInBondCargoDescPk = TestDataCreator.CreateCusInBondCargoDesc(cusInbondBillPk, "B0", "desc123", "NEW", 150, 200);
			UpdateCusInBondCargoDesc(cusInBondCargoDescPk, monetaryValue);

			return cusInBondCargoDescPk;
		}

		static Guid CreateCountryInBondCargoDesc(string jobNumber, Guid branchPk, string applicationCode, string headerType, decimal monetaryValue)
		{
			var headerPK = TestDataCreator.CreateCusInbondHeader(jobNumber, branchPk, applicationCode, headerType);
			var cusInbondBillPk = TestDataCreator.CreateCusInbondBill(headerPK);
			var cusInBondCargoDescPk = TestDataCreator.CreateCusInBondCargoDesc(cusInbondBillPk, "B0", "desc123", "NEW", 150, 200);
			UpdateCusInBondCargoDesc(cusInBondCargoDescPk, monetaryValue);

			return cusInBondCargoDescPk;
		}

		static void UpdateCusInBondCargoDesc(Guid cusInBondCargoDescPK, decimal monetaryValue)
		{
			var sql = "UPDATE dbo.CusInBondCargoDesc SET BY_MonetaryValue = @monetaryValue, BY_SystemLastEditTimeUtc = @systemLastEditTimeUtc, BY_SystemLastEditUser = '~BP' WHERE BY_PK=@cusInBondCargoDescPK";
			using var command = Db.Connection.Command(sql);
			command.AddParameter("@cusInBondCargoDescPK", SqlDbType.UniqueIdentifier, cusInBondCargoDescPK);
			command.AddParameter("@monetaryValue", SqlDbType.Decimal, monetaryValue);
			command.AddParameter("@systemLastEditTimeUtc", SqlDbType.DateTime, LastUpdateDateTime);
			command.ExecuteNonQuery();
		}

		Guid GetBranch(string branchCode)
		{
			using var command = Db.Connection.Command("SELECT GB_PK FROM dbo.GlbBranch WHERE GB_Code = @branchCode");
			command.AddParameter("@branchCode", SqlDbType.VarChar, GlbBranchSchema.GB_Code.MaxLength, branchCode);
			return (Guid)command.ExecuteScalar();
		}

		static readonly DateTime LastUpdateDateTime = new (2024, 03, 20, 12, 30, 00);
		Guid countryInBondCargoDescBE;
		Guid countryInBondCargoDescCH;
		Guid countryInBondCargoDescDE;
		Guid countryInBondCargoDescES;
		Guid countryInBondCargoDescFR;
		Guid countryInBondCargoDescGB;
		Guid countryInBondCargoDescIE;
		Guid countryInBondCargoDescIT;
		Guid countryInBondCargoDescNL;
		Guid countryInBondCargoDescNO;
		Guid countryInBondCargoDescPL;
		Guid countryInBondCargoDescTR;
		Guid countryInBondCargoDescAT;
		Guid countryInBondCargoDescBENC4;
		Guid countryInBondCargoDescBEArrival;
	}
}
