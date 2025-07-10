using System;
using System.Data;
using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.Accounting.Utilities;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Accounting.Utilities
{
	[TestedType(typeof(GetTaxRate))]
	class GetTaxRateTest : DbCreateScriptTest
	{
		public void TestGetTaxRateTest()
		{
			var resultTable = DataUtils.GetDataTableFromQuery(TestConnection, "SELECT TaxRate FROM GetTaxRate(NULL, 4, 2)");
			AssertEquals(null, resultTable.Rows[0].Field<decimal?>("TaxRate"));

			var pK = Guid.NewGuid();
			TestConnection.ExecuteNonQuery($@"
INSERT INTO dbo.AccTaxRate (AT_PK, AT_Code, AT_IsActive, AT_Description, AT_Type, AT_ExtraTaxRateType, AT_RN_NKCountry, AT_A9_DefaultVatClass, AT_PostingGroupId, AT_ReferenceExtraRateType, AT_ReferenceRateType)
VALUES ('{pK}', 'GST', 1, '', 'RAT', '', '', NULL, 1, '', '')");

			resultTable = DataUtils.GetDataTableFromQuery(TestConnection, $"SELECT TaxRate FROM GetTaxRate('{pK}', 4, 2)");
			AssertEquals("Rows.Count", 1, resultTable.Rows.Count);
			var row = resultTable.Rows[0];
			AssertEquals("Calculated Rate", 2M, row.Field<decimal>("TaxRate"));

			resultTable = DataUtils.GetDataTableFromQuery(TestConnection, $"SELECT TaxRate FROM GetTaxRate('{pK}', 0, 2)");
			row = resultTable.Rows[0];
			AssertEquals("Calculated Rate", 0M, row.Field<decimal>("TaxRate"));

			resultTable = DataUtils.GetDataTableFromQuery(TestConnection, $"SELECT TaxRate FROM GetTaxRate('{pK}', 3125, 654)");
			row = resultTable.Rows[0];
			AssertEquals("Calculated Rate should have 20 digits after decimal", 4.778287461773700306M, row.Field<decimal>("TaxRate"));

			resultTable = DataUtils.GetDataTableFromQuery(TestConnection, $"SELECT TaxRate FROM GetTaxRate('{pK}', 125, 10)");
			row = resultTable.Rows[0];
			AssertEquals("Calculated Rate ", 12.5M, row.Field<decimal>("TaxRate"));
		}
	}
}

