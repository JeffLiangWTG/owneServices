using System;
using CargoWise.DbUpgrader.Scripts.Definitions.MasterFiles.RefExchangeRate;
using Enterprise.Build.Database.Script.TestFramework;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.MasterFiles.RefExchangeRate
{
	[TestedType(typeof(trgRefExchangeRate_Ins))]
	class trgRefExchangeRate_Ins_Test : DbCreateScriptTest
	{
		public void TestInsert()
		{
			var refExchangeRateZZCount_Before = (int)TestConnection.ExecuteScalar(@"SELECT COUNT(*) FROM RefDatabase_RefExchangeRateZZ");
			var refExchangeRateCount_Before = (int)TestConnection.ExecuteScalar(@"SELECT COUNT(*) FROM dbo.ZZRefExchangeRate");

			var companyPK = TestDataCreator.CreateCompany("ZA1", "ZA", "ZAR");
			var clientPK = TestDataCreator.CreateOrganisation("OH1", "CLIENT1");
			var pk = Guid.NewGuid();
			using (var command = TestConnection.Command(@"INSERT INTO dbo.RefExchangeRate(RE_PK, RE_GC, RE_OH_Client, RE_RX_NKExCurrency, RE_ExRateType, RE_StartDate, RE_ExpiryDate, RE_SellRate, RE_IsSystem, RE_AsPublished) VALUES
				(@PK, @CompanyPk, @Client, @Currency, @RateType, @StartDate, @ExpiryDate, @Rate, @IsSystem, @AsPublished)"))
			{
				command.AddParameterBasedOnDbColumn("@PK", pk, RefExchangeRateSchema.PK);
				command.AddParameterBasedOnDbColumn("@CompanyPk", companyPK, RefExchangeRateSchema.RE_GC);
				command.AddParameterBasedOnDbColumn("@Client", clientPK, RefExchangeRateSchema.RE_OH_Client);
				command.AddParameterBasedOnDbColumn("@Currency", "CAD", RefExchangeRateSchema.RE_RX_NKExCurrency);
				command.AddParameterBasedOnDbColumn("@RateType", "SEL", RefExchangeRateSchema.RE_ExRateType);
				command.AddParameterBasedOnDbColumn("@StartDate", new DateTime(2016, 1, 1), RefExchangeRateSchema.RE_StartDate);
				command.AddParameterBasedOnDbColumn("@ExpiryDate", new DateTime(2016, 12, 1), RefExchangeRateSchema.RE_ExpiryDate);
				command.AddParameterBasedOnDbColumn("@Rate", 0.5m, RefExchangeRateSchema.RE_SellRate);
				command.AddParameterBasedOnDbColumn("@IsSystem", true, RefExchangeRateSchema.RE_IsSystem);
				command.AddParameterBasedOnDbColumn("@AsPublished", string.Empty, RefExchangeRateSchema.RE_AsPublished);
				command.ExecuteNonQuery();
			}

			var refExchangeRateZZCount_After = (int)TestConnection.ExecuteScalar(@"SELECT COUNT(*) FROM RefDatabase_RefExchangeRateZZ");
			var refExchangeRateCount_After = (int)TestConnection.ExecuteScalar(@"SELECT COUNT(*) FROM dbo.ZZRefExchangeRate");

			AssertEquals("RefDatabase_RefExchangeRateZZ should NOT be updated", refExchangeRateZZCount_Before, refExchangeRateZZCount_After);
			AssertEquals("One record should be inserted into ZZRefExchangeRate", refExchangeRateCount_Before + 1, refExchangeRateCount_After);

			using (var command = TestConnection.Command(@"SELECT * FROM dbo.ZZRefExchangeRate WHERE RE_PK=@PK"))
			{
				command.AddParameterBasedOnDbColumn("@PK", pk, ZZRefExchangeRateSchema.PK);
				using (var reader = command.ExecuteReader())
				{
					reader.Read();
					AssertEquals("RE_GC", companyPK, (Guid)reader["RE_GC"]);
					AssertEquals("RE_OH_Client", clientPK, (Guid)reader["RE_OH_Client"]);
					AssertEquals("RE_RX_NKExCurrency", "CAD", (string)reader["RE_RX_NKExCurrency"]);
					AssertEquals("RE_ExRateType", "SEL", (string)reader["RE_ExRateType"]);
					AssertEquals("RE_StartDate", new DateTime(2016, 1, 1), (DateTime)reader["RE_StartDate"]);
					AssertEquals("RE_ExpiryDate", new DateTime(2016, 12, 1), (DateTime)reader["RE_ExpiryDate"]);
					AssertEquals("RE_SellRate", 0.5m, (decimal)reader["RE_SellRate"]);
					AssertEquals("RE_AsPublished", string.Empty, (string)reader["RE_AsPublished"]);
				}
			}
		}
	}
}

