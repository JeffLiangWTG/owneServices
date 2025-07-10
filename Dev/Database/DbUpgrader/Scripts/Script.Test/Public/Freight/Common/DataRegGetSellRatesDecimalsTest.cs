using System;
using System.Collections.Generic;
using System.Text;
using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.Freight.Common;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Freight.Common.Testing
{
	[TestedType(typeof(DataRegGetSellRatesDecimals))]
	class DataRegGetSellRatesDecimalsTest : DbCreateScriptTest
	{
		public void TestDataRegGetSellRatesDecimal_RegistryHasAllCode()
		{
			var companyPK = Guid.NewGuid();
			CreateSellRatesDecimalsRegistry
			(
				companyPK,
				new[]
				{
					new SellRatesDecimal { Code = "ALL", Decimals = 3 },
					new SellRatesDecimal { Code = "AIR", Decimals = 2 }
				}
			);
			AssertSellRatesDecimalsRegistry("AIR", companyPK, expectedDecimals: 2, message: "Match company and category");
			AssertSellRatesDecimalsRegistry("SEA", companyPK, expectedDecimals: 3, message: "Match company, not match category THEN fallback to ALL");
			AssertSellRatesDecimalsRegistry("AIR", Guid.NewGuid(), expectedDecimals: 4, message: "Not match company, match category THEN fallback to default value 4");
			AssertSellRatesDecimalsRegistry("SEA", Guid.NewGuid(), expectedDecimals: 4, message: "Not match company, not match category THEN fallback to default value 4");
		}

		public void TestDataRegGetSellRatesDecimals_RegistryHasNoAllCode()
		{
			var companyPK = Guid.NewGuid();
			CreateSellRatesDecimalsRegistry
			(
				companyPK,
				new[]
				{
					new SellRatesDecimal { Code = "AIR", Decimals = 2 }
				}
			);
			AssertSellRatesDecimalsRegistry("AIR", companyPK, expectedDecimals: 2, message: "Match company and category");
			AssertSellRatesDecimalsRegistry("SEA", companyPK, expectedDecimals: 4, message: "Match company, not match category THEN fallback default value 4");
			AssertSellRatesDecimalsRegistry("AIR", Guid.NewGuid(), expectedDecimals: 4, message: "Not match company, match category THEN fallback to default value 4");
			AssertSellRatesDecimalsRegistry("SEA", Guid.NewGuid(), expectedDecimals: 4, message: "Not match company, not match category THEN fallback to default value 4");
		}

		public void TestDataRegGetSellRatesDecimals_RegistryIsEmpty()
		{
			var companyPK = Guid.NewGuid();
			CreateSellRatesDecimalsRegistry
			(
				companyPK,
				Array.Empty<SellRatesDecimal>()
			);
			AssertSellRatesDecimalsRegistry("SEA", companyPK, expectedDecimals: 4, message: "Matching registry doesn't exist THEN it should fallback to 4");
		}

		void AssertSellRatesDecimalsRegistry(string rateCategory, Guid companyPK, int expectedDecimals, string message)
		{
			var reportSql = string.Format("SELECT Decimals FROM DataRegGetSellRatesDecimals('{0}', '{1}', {2}, {3})", rateCategory, companyPK.ToString(), "NULL", "NULL");
			var data = DataUtils.GetDataTableFromQuery(TestConnection, reportSql);
			AssertEquals(message, expectedDecimals, data.Rows[0][0]);
		}

		void CreateSellRatesDecimalsRegistry(Guid companyPK, IEnumerable<SellRatesDecimal> sellRatesDecimals)
		{
			var sellRatesDecimalsText = new StringBuilder();
			foreach (var sellRatesDecimal in sellRatesDecimals)
			{
				sellRatesDecimalsText.Append($@"
					<SellRatesDecimals>
						<Code>{sellRatesDecimal.Code}</Code>
						<Decimals>{sellRatesDecimal.Decimals}</Decimals>
					</SellRatesDecimals>");
			}

			TestConnection.ExecuteNonQuery
			(
				$@"INSERT dbo.StmData
					([SD_PK], [SD_Owner], [SD_Name], [SD_BinaryValue])
					VALUES
					(
						NEWID(),
						'{companyPK}',
						'SellRatesDecimals',
						convert
						(
							varbinary(max),
							N'<ArrayOfSellRatesDecimals>
								{sellRatesDecimalsText}
							</ArrayOfSellRatesDecimals>'
						)
					);"
			);
		}

		class SellRatesDecimal
		{
			public string Code { get; set; }
			public int Decimals { get; set; }
		}
	}
}
