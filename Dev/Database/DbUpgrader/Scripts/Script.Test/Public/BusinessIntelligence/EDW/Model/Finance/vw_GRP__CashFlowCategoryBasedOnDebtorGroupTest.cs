using System.Data;
using System.Globalization;
using System.Linq;
using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.BusinessIntelligence.EDW.Model.Finance;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.BusinessIntelligence.EDW.Model.Finance.Testing
{
	[TestedType(typeof(vw_GRP__CashFlowCategoryBasedOnDebtorGroup))]
	internal class vw_GRP__CashFlowCategoryBasedOnDebtorGroupTest : BiCreateScriptTest
	{
		[ExpectNoExceptions]
		public void TestRun()
		{
			PrepareTestDataForIniLoad();
			ExecuteCusTableIniLoad();
			var result = SelectRows();
			AssertEquals("Rowcount", 1, result.Rows.Count);
			CombineAssertions(() =>
			{
				AssertEquals(1, result.Select("DebtorGroupPK = '2ee6da33-bbdf-4258-be43-cfce9bc57ab3' and CashFlowCategory = 'O03'").Length);
			});

			PrepareTestDataForIncLoad();
			ExecuteCusTableIncLoad();
			result = SelectRows();
			AssertEquals("Rowcount", 1, result.Rows.Count);
			CombineAssertions(() =>
			{
				AssertEquals(1, result.Select("DebtorGroupPK = '2ee6da33-bbdf-4258-be43-cfce9bc57ab3' and CashFlowCategory = 'O04'").Length);
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
						(NEWID(), 1, 'CashFlowCategoryBasedOnDebtorGroup',convert(varbinary(max), N'<ArrayOfCashFlowCategoryBasedOnDebtorGroup xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance""><CashFlowCategoryBasedOnDebtorGroup><OrgGroupPK>2ee6da33-bbdf-4258-be43-cfce9bc57ab3</OrgGroupPK><CashFlowCategory>O03</CashFlowCategory></CashFlowCategoryBasedOnDebtorGroup></ArrayOfCashFlowCategoryBasedOnDebtorGroup>'))", ScriptDbName
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
						(NEWID(), 1, 'CashFlowCategoryBasedOnDebtorGroup',convert(varbinary(max), N'<ArrayOfCashFlowCategoryBasedOnDebtorGroup xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance""><CashFlowCategoryBasedOnDebtorGroup><OrgGroupPK>2ee6da33-bbdf-4258-be43-cfce9bc57ab3</OrgGroupPK><CashFlowCategory>O04</CashFlowCategory></CashFlowCategoryBasedOnDebtorGroup></ArrayOfCashFlowCategoryBasedOnDebtorGroup>'))
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
				"SELECT InitialLoadQuery FROM [{0}].[biAdmin].[CustomTableConfiguration] WHERE  [ModelSchemaName] = 'Finance' AND [ModelTableName] = 'GRP__CashFlowCategoryBasedOnDebtorGroup'",
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
				"SELECT IncrementalLoadQuery FROM [{0}].[biAdmin].[CustomTableConfiguration] WHERE  [ModelSchemaName] = 'Finance' AND [ModelTableName] = 'GRP__CashFlowCategoryBasedOnDebtorGroup'",
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
			return DataUtils.GetDataTableFromQuery(TestConnection, $"SELECT * FROM [{ScriptDbName}].[Finance].GRP__CashFlowCategoryBasedOnDebtorGroup");
		}

		protected override DbConnection TestConnection => adminConnection ?? (adminConnection = Db.NewAdminConnection());
		AdminConnection adminConnection;
	}
}
