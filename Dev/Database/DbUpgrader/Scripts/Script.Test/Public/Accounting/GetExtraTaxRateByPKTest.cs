using System;
using System.Data;
using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.Accounting;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Accounting
{
	[TestedType(typeof(GetExtraTaxRateByPK))]
	class GetExtraTaxRateByPKTest : DbCreateScriptTest
	{
		public void TestSimpleRun()
		{
			var extraTaxRateType1 = "EDU";
			var extraTaxRateType2 = "REF";

			var result = DataUtils.GetDataTableFromQuery(
							TestConnection, $"SELECT * FROM GetExtraTaxRateByPK(NULL, '{extraTaxRateType1}',100, 20, 50, 5)");
			AssertEquals(null, result.Rows[0].Field<decimal?>("ExtraTaxRate"));

			var pK = Guid.NewGuid();
			TestConnection.ExecuteNonQuery($@"
INSERT INTO dbo.AccTaxRate (AT_PK, AT_Code, AT_IsActive, AT_Description, AT_Type, AT_ExtraTaxRateType, AT_RN_NKCountry, AT_A9_DefaultVatClass, AT_PostingGroupId, AT_ReferenceExtraRateType, AT_ReferenceRateType) 
VALUES ('{pK}', 'GST', 1, '', 'RAT', '', '', NULL, 1, '', '')");

			result = DataUtils.GetDataTableFromQuery(
							TestConnection, $"SELECT * FROM GetExtraTaxRateByPK('{pK}','{extraTaxRateType1}',100, 20, 50, 5)");
			AssertEquals("50 / 5 = 10", 10M, result.Rows[0].Field<decimal>("ExtraTaxRate"));

			result = DataUtils.GetDataTableFromQuery(
						TestConnection, $"SELECT * FROM GetExtraTaxRateByPK('{pK}','{extraTaxRateType2}',100, 20, 50, 5)");
			AssertEquals("100 / 20 * 50 / 5 = 50", 50M, result.Rows[0].Field<decimal>("ExtraTaxRate"));

			result = DataUtils.GetDataTableFromQuery(
						TestConnection, $"SELECT * FROM GetExtraTaxRateByPK('{pK}','{extraTaxRateType2}',1786, 100, 50, 5)");
			AssertEquals("1786 / 100 * 50 / 5 = 178.6", 178.6M, result.Rows[0].Field<decimal>("ExtraTaxRate"));

			result = DataUtils.GetDataTableFromQuery(
			TestConnection, $"SELECT * FROM GetExtraTaxRateByPK('{pK}','{extraTaxRateType2}',100, 20, 72, 6)");
			AssertEquals("100 / 20 * 72 / 6 = 60", 60M, result.Rows[0].Field<decimal>("ExtraTaxRate"));
		}
	}
}

