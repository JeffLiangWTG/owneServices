using CargoWise.Data;
using Enterprise.Accounting.Utility.Testing;

namespace Enterprise.Accounting.Business.Testing.ScriptTests.DbCreateScriptTests.CashFlow
{
	class CashFlowCategoryBasedOnCreditorGroupTest : ScriptTest
	{
		public void TestCashFlowCategoryBasedOnCreditorGroupOverrideValue()
		{
			PrepareData();
			var result = DataUtils.GetDataTableFromQuery(TestConnection, $"SELECT * FROM [{ScriptDbName}].{FunctionOrTableDbName}");
			AssertEquals("Result should have rows", 2, result.Rows.Count);
			result = DataUtils.GetDataTableFromQuery(TestConnection, string.Format($"SELECT * FROM [{ScriptDbName}].{FunctionOrTableDbName} where CreditorGroupPK = '878D7ACA-FFC3-49FC-9710-969CA0C0F2AC'"));
			AssertEquals("Result should contain Org 1", 1, result.Rows.Count);
			result = DataUtils.GetDataTableFromQuery(TestConnection, string.Format($"SELECT * FROM [{ScriptDbName}].{FunctionOrTableDbName} where CreditorGroupPK = 'E2502D59-69F0-4E8C-A2EC-40E40E006B3F'"));
			AssertEquals("Result should contain Org 2", 1, result.Rows.Count);
		}

		void PrepareData()
		{
			var insertRegistryValues = string.Format(@"DECLARE @registryRawValue nvarchar(MAX)
Set @registryRawValue = '<ArrayOfCashFlowCategoryBasedOnCreditorGroup xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance""><CashFlowCategoryBasedOnCreditorGroup><OrgGroupPK>{0}</OrgGroupPK><CashFlowCategory>O04</CashFlowCategory></CashFlowCategoryBasedOnCreditorGroup><CashFlowCategoryBasedOnCreditorGroup><OrgGroupPK>{1}</OrgGroupPK><CashFlowCategory>O05</CashFlowCategory></CashFlowCategoryBasedOnCreditorGroup></ArrayOfCashFlowCategoryBasedOnCreditorGroup>'
INSERT INTO dbo.StmData
(SD_PK, SD_Name, SD_Type, SD_BinaryValue, SD_IsLogged, SD_IsCancelled)
Values
(newID(), 'CashFlowCategoryBasedOnCreditorGroup', 'BIN', CONVERT(varbinary(MAX), @registryRawValue), 1, 0)", "878D7ACA-FFC3-49FC-9710-969CA0C0F2AC", "E2502D59-69F0-4E8C-A2EC-40E40E006B3F");
			TestConnection.ExecuteNonQuery(insertRegistryValues);
		}

		protected string ScriptDbName => Db.DatabaseName;

		protected string FunctionOrTableDbName => "[dbo].CashFlowCategoryBasedOnCreditorGroup()";
	}
}

