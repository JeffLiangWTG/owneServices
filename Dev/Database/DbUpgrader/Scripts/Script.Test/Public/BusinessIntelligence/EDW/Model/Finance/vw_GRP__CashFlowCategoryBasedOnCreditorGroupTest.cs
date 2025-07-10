using System.Data;
using System.Globalization;
using System.Linq;
using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.BusinessIntelligence.EDW.Model.Finance;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.BusinessIntelligence.EDW.Model.Finance.Testing
{
	[TestedType(typeof(vw_GRP__CashFlowCategoryBasedOnCreditorGroup))]
	internal class vw_GRP__CashFlowCategoryBasedOnCreditorGroupTest : BiCreateScriptTest
	{
		[ExpectNoExceptions]
		public void TestRun()
		{
			PrepareTestDataForIniLoad();
			ExecuteCusTableIniLoad();
			var result = SelectRows();
			AssertEquals("Rowcount", 5, result.Rows.Count);
			CombineAssertions(() =>
			{
				AssertEquals(1, result.Select("CreditorGroupPK = '4d9eb221-e8e4-42e7-bd8d-b45215aed868' and CashFlowCategory = 'I01'").Length);
				AssertEquals(1, result.Select("CreditorGroupPK = '7ea13d4f-e5fd-453d-b234-cf4644216de7' and CashFlowCategory = 'O02'").Length);
				AssertEquals(1, result.Select("CreditorGroupPK = 'dfe48c5a-d0d3-4065-b352-466fff4b3959' and CashFlowCategory = 'O05'").Length);
				AssertEquals(1, result.Select("CreditorGroupPK = 'f9d4ed6b-cfc8-4b8f-8d20-966fd10e072e' and CashFlowCategory = 'I02'").Length);
				AssertEquals(1, result.Select("CreditorGroupPK = 'fed6dc50-788c-4513-a486-c3f1a2dec229' and CashFlowCategory = 'O05'").Length);
			});

			PrepareTestDataForIncLoad();
			ExecuteCusTableIncLoad();
			result = SelectRows();
			AssertEquals("Rowcount", 5, result.Rows.Count);
			CombineAssertions(() =>
			{
				AssertEquals(1, result.Select("CreditorGroupPK = '4d9eb221-e8e4-42e7-bd8d-b45215aed868' and CashFlowCategory = 'I01'").Length);
				AssertEquals(1, result.Select("CreditorGroupPK = '7ea13d4f-e5fd-453d-b234-cf4644216de7' and CashFlowCategory = 'O02'").Length);
				AssertEquals(1, result.Select("CreditorGroupPK = 'dfe48c5a-d0d3-4065-b352-466fff4b3959' and CashFlowCategory = 'O05'").Length);
				AssertEquals(1, result.Select("CreditorGroupPK = 'f9d4ed6b-cfc8-4b8f-8d20-966fd10e072e' and CashFlowCategory = 'I02'").Length);
				AssertEquals(1, result.Select("CreditorGroupPK = 'fed6dc50-788c-4513-a486-c3f1a2dec229' and CashFlowCategory = 'O04'").Length);
			});
		}

		protected override string ScriptDbName
		{
			get { return Db.EdwDatabaseName; }
		}

		void PrepareTestDataForIniLoad()
		{
			var sqlText = string.Format(CultureInfo.InvariantCulture, @"

				DELETE FROM [{0}].Finance.BAS__CashFlowCategoryBasedOnGroup

				INSERT [{0}].[Finance].[BAS__CashFlowCategoryBasedOnGroup]
					([CashFlowCategoryBasedOnGroupID], [CashFlowCategoryBasedOnGroupKey], [Name], [Value])
					VALUES
						(NEWID(), 1, 'CashFlowCategoryBasedOnCreditorGroup', convert(varbinary(max), N'<ArrayOfCashFlowCategoryBasedOnCreditorGroup xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance""><CashFlowCategoryBasedOnCreditorGroup><OrgGroupPK>4d9eb221-e8e4-42e7-bd8d-b45215aed868</OrgGroupPK><CashFlowCategory>I01</CashFlowCategory></CashFlowCategoryBasedOnCreditorGroup><CashFlowCategoryBasedOnCreditorGroup><OrgGroupPK>7ea13d4f-e5fd-453d-b234-cf4644216de7</OrgGroupPK><CashFlowCategory>O02</CashFlowCategory></CashFlowCategoryBasedOnCreditorGroup><CashFlowCategoryBasedOnCreditorGroup><OrgGroupPK>dfe48c5a-d0d3-4065-b352-466fff4b3959</OrgGroupPK><CashFlowCategory>O05</CashFlowCategory></CashFlowCategoryBasedOnCreditorGroup><CashFlowCategoryBasedOnCreditorGroup><OrgGroupPK>f9d4ed6b-cfc8-4b8f-8d20-966fd10e072e</OrgGroupPK><CashFlowCategory>I02</CashFlowCategory></CashFlowCategoryBasedOnCreditorGroup><CashFlowCategoryBasedOnCreditorGroup><OrgGroupPK>fed6dc50-788c-4513-a486-c3f1a2dec229</OrgGroupPK><CashFlowCategory>O05</CashFlowCategory></CashFlowCategoryBasedOnCreditorGroup></ArrayOfCashFlowCategoryBasedOnCreditorGroup>'))", ScriptDbName
			);
			TestConnection.ExecuteNonQuery(sqlText);
		}

		void PrepareTestDataForIncLoad()
		{
			var sqlText = string.Format(CultureInfo.InvariantCulture, @"
				DELETE FROM [{0}].Finance.BAS__CashFlowCategoryBasedOnGroup

				INSERT [{0}].[Finance].[BAS__CashFlowCategoryBasedOnGroup]
					([CashFlowCategoryBasedOnGroupID], [CashFlowCategoryBasedOnGroupKey], [Name], [Value])
					VALUES
						(NEWID(), 1, 'CashFlowCategoryBasedOnCreditorGroup', convert(varbinary(max), N'<ArrayOfCashFlowCategoryBasedOnCreditorGroup xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance""><CashFlowCategoryBasedOnCreditorGroup><OrgGroupPK>4d9eb221-e8e4-42e7-bd8d-b45215aed868</OrgGroupPK><CashFlowCategory>I01</CashFlowCategory></CashFlowCategoryBasedOnCreditorGroup><CashFlowCategoryBasedOnCreditorGroup><OrgGroupPK>7ea13d4f-e5fd-453d-b234-cf4644216de7</OrgGroupPK><CashFlowCategory>O02</CashFlowCategory></CashFlowCategoryBasedOnCreditorGroup><CashFlowCategoryBasedOnCreditorGroup><OrgGroupPK>dfe48c5a-d0d3-4065-b352-466fff4b3959</OrgGroupPK><CashFlowCategory>O05</CashFlowCategory></CashFlowCategoryBasedOnCreditorGroup><CashFlowCategoryBasedOnCreditorGroup><OrgGroupPK>f9d4ed6b-cfc8-4b8f-8d20-966fd10e072e</OrgGroupPK><CashFlowCategory>I02</CashFlowCategory></CashFlowCategoryBasedOnCreditorGroup><CashFlowCategoryBasedOnCreditorGroup><OrgGroupPK>fed6dc50-788c-4513-a486-c3f1a2dec229</OrgGroupPK><CashFlowCategory>O04</CashFlowCategory></CashFlowCategoryBasedOnCreditorGroup></ArrayOfCashFlowCategoryBasedOnCreditorGroup>'))
				INSERT INTO [{0}].biadmin.TransformedRow(SchemaName, TableName, KeyValue, PKValue)
				VALUES
				('Finance', 'BAS__CashFlowCategoryBasedOnGroup', 1, newID())
				", ScriptDbName
			);
			TestConnection.ExecuteNonQuery(sqlText);
		}

		void ExecuteCusTableIniLoad()
		{
			var sqlText = string.Format(CultureInfo.InvariantCulture,
				"SELECT InitialLoadQuery FROM [{0}].[biAdmin].[CustomTableConfiguration] WHERE  [ModelSchemaName] = 'Finance' AND [ModelTableName] = 'GRP__CashFlowCategoryBasedOnCreditorGroup'",
				ScriptDbName
			);
			var resultTable = DataUtils.GetDataTableFromQuery(TestConnection, sqlText);
			var record = resultTable.Select().Single();
			var incLoadSQLText = record.ItemArray[0].ToString();
			var sqlTextDL = "USE " + ScriptDbName + " " + incLoadSQLText;
			TestConnection.ExecuteNonQuery(sqlTextDL);
		}
		void ExecuteCusTableIncLoad()
		{
			var sqlText = string.Format(CultureInfo.InvariantCulture,
				"SELECT IncrementalLoadQuery FROM [{0}].[biAdmin].[CustomTableConfiguration] WHERE  [ModelSchemaName] = 'Finance' AND [ModelTableName] = 'GRP__CashFlowCategoryBasedOnCreditorGroup'",
				ScriptDbName
			);
			var resultTable = DataUtils.GetDataTableFromQuery(TestConnection, sqlText);
			var record = resultTable.Select().Single();
			var incLoadSQLText = record.ItemArray[0].ToString();
			var sqlTextDL = "USE " + ScriptDbName + " " + incLoadSQLText;
			TestConnection.ExecuteNonQuery(sqlTextDL);
		}

		DataTable SelectRows()
		{
			return DataUtils.GetDataTableFromQuery(TestConnection, $"SELECT * FROM [{ScriptDbName}].[Finance].GRP__CashFlowCategoryBasedOnCreditorGroup");
		}

		protected override DbConnection TestConnection => adminConnection ?? (adminConnection = Db.NewAdminConnection());
		AdminConnection adminConnection;
	}
}
