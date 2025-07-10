using CargoWise.Data;
using Enterprise.Accounting.Utility.Testing;

namespace Enterprise.Accounting.Business.Testing.ScriptTests.DbCreateScriptTests.CashFlow
{
	class CashFlowCategoryBasedOnDebtorGroupTest : ScriptTest
	{
		public void TestCashFlowCategoryBasedOnDebtorGroupDefaultValue()
		{
			var result = DataUtils.GetDataTableFromQuery(TestConnection, $"SELECT * FROM [{ScriptDbName}].{FunctionOrTableDbName}");
			AssertEquals("Result should have 0 rows", 0, result.Rows.Count);
		}

		public void TestCashFlowCategoryBasedOnDebtorGroupOverrideValue()
		{
			PrepareData();
			var result = DataUtils.GetDataTableFromQuery(TestConnection, $"SELECT * FROM [{ScriptDbName}].{FunctionOrTableDbName}");
			AssertEquals("Result should have rows", 2, result.Rows.Count);
			result = DataUtils.GetDataTableFromQuery(TestConnection, string.Format($"SELECT * FROM [{ScriptDbName}].{FunctionOrTableDbName} where DebtorGroupPK = '878D7ACA-FFC3-49FC-9710-969CA0C0F2AC'"));
			AssertEquals("Result should contain Org 1", 1, result.Rows.Count);
			result = DataUtils.GetDataTableFromQuery(TestConnection, string.Format($"SELECT * FROM [{ScriptDbName}].{FunctionOrTableDbName} where DebtorGroupPK = 'E2502D59-69F0-4E8C-A2EC-40E40E006B3F'"));
			AssertEquals("Result should contain Org 2", 1, result.Rows.Count);

			void PrepareData()
			{
				var insertRegistryValues = string.Format(@"DECLARE @registryRawValue nvarchar(MAX)
Set @registryRawValue = '<ArrayOfCashFlowCategoryBasedOnDebtorGroup xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance""><CashFlowCategoryBasedOnDebtorGroup><OrgGroupPK>{0}</OrgGroupPK><CashFlowCategory>O04</CashFlowCategory></CashFlowCategoryBasedOnDebtorGroup><CashFlowCategoryBasedOnDebtorGroup><OrgGroupPK>{1}</OrgGroupPK><CashFlowCategory>O05</CashFlowCategory></CashFlowCategoryBasedOnDebtorGroup></ArrayOfCashFlowCategoryBasedOnDebtorGroup>'
INSERT INTO dbo.StmData
(SD_PK, SD_Name, SD_Type, SD_BinaryValue, SD_IsLogged, SD_IsCancelled)
Values
(newID(), 'CashFlowCategoryBasedOnDebtorGroup', 'BIN', CONVERT(varbinary(MAX), @registryRawValue), 1, 0)", "878D7ACA-FFC3-49FC-9710-969CA0C0F2AC", "E2502D59-69F0-4E8C-A2EC-40E40E006B3F");

				TestConnection.ExecuteNonQuery(insertRegistryValues);
			}
		}

		string ScriptDbName => Db.DatabaseName;

		string FunctionOrTableDbName => "[dbo].CashFlowCategoryBasedOnDebtorGroup()";
	}
}

