using System;
using System.Data;
using System.Linq;
using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.Accounting;
using Enterprise.Build.Database.Script.TestFramework;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Accounting
{
	[TestedType(typeof(Report_ChargeCodeGLPostingOverrides))]
	class Report_ChargeCodeGLPostingOverridesTest : DbCreateScriptTest
	{
		public void TestReport_ChargeCodeGLPostingOverrides_ColumnFromAccChargeGLPostingOverride()
		{
			var companyPK = TestDataCreator.CreateCompany("TC1", "CA", "CAD");
			var chargeCUSDEF = TestDataCreator.CreateAccCharges(companyPK, "CUSDEF", "CUSDEF DESC", "CMT");
			var insertAccChargeGLPostingOverrideSql = @"
INSERT INTO dbo.AccChargeGLPostingOverride (Y1_PK, Y1_AC, Y1_ConsolidationAccountingCategoryClass, Y1_ConsolContainerMode, Y1_Direction, Y1_HousePaymentType, Y1_JobType, Y1_MasterPaymentType, Y1_TransportMode) 
VALUES (@Y1_PK, @Y1_AC, @Y1_ConsolidationAccountingCategoryClass, @Y1_ConsolContainerMode, @Y1_Direction, @Y1_HousePaymentType, @Y1_JobType, @Y1_MasterPaymentType, @Y1_TransportMode)
";
			using (var command = TestConnection.Command(insertAccChargeGLPostingOverrideSql))
			{
				command.AddParameter("@Y1_PK", SqlDbType.UniqueIdentifier, Guid.NewGuid());
				command.AddParameter("@Y1_AC", SqlDbType.UniqueIdentifier, chargeCUSDEF);
				command.AddParameter("@Y1_ConsolidationAccountingCategoryClass", SqlDbType.VarChar, "ALL");
				command.AddParameter("@Y1_ConsolContainerMode", SqlDbType.VarChar, "ALL");
				command.AddParameter("@Y1_Direction", SqlDbType.VarChar, "ALL");
				command.AddParameter("@Y1_HousePaymentType", SqlDbType.VarChar, "ALL");
				command.AddParameter("@Y1_JobType", SqlDbType.VarChar, "ALL");
				command.AddParameter("@Y1_MasterPaymentType", SqlDbType.VarChar, "ALL");
				command.AddParameter("@Y1_TransportMode", SqlDbType.VarChar, "ALL");
				command.ExecuteNonQuery();
			}

			var result = DataUtils.GetDataTableFromQuery(TestConnection, "SELECT * FROM Report_ChargeCodeGLPostingOverrides('TC1', '', '', NULL)");

			AssertEquals("Result should have 2 rows", 2, result.Rows.Count);

			var rowSelected = result.Rows.Cast<DataRow>().FirstOrDefault(x => (string)x["ChargeCodeType"] == "Override GL");
			AssertEquals("JobType", "ALL", rowSelected["JobType"]);
			AssertEquals("TransportMode", "ALL", rowSelected["TransportMode"]);
			AssertEquals("Direction", "ALL", rowSelected["Direction"]);
			AssertEquals("ConsolContainerMode", "ALL", rowSelected["ConsolContainerMode"]);
			AssertEquals("MasterPaymentTerm", "ALL", rowSelected["MasterPaymentTerm"]);
			AssertEquals("HousePaymentTerm", "ALL", rowSelected["HousePaymentTerm"]);
		}
	}
}

