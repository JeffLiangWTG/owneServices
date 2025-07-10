using System.Data;
using System.Globalization;
using System.Linq;
using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.BusinessIntelligence.EDW.Model.Finance;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.BusinessIntelligence.EDW.Model.Finance.Testing
{
	[TestedType(typeof(vw_GRP__CashFlowActivityConfiguration))]
	internal class vw_GRP__CashFlowActivityConfigurationTest : BiCreateScriptTest
	{
		[ExpectNoExceptions]
		public void TestRun()
		{
			PrepareTestDataForIniLoad();
			ExecuteCusTableIniLoad();
			var result = SelectRows();
			AssertEquals("Rowcount", 7, result.Rows.Count);
			CombineAssertions(() =>
			{
				AssertEquals(1, result.Select("Code = 'XXX' and CashFlowDescription = 'Undefined' and ActivityType = 'X' and ActivityDescription = 'Undefined Activities'").Length);
				AssertEquals(1, result.Select("Code = 'NON' and CashFlowDescription = 'Non Cash' and ActivityType = 'N' and ActivityDescription = 'Non Cash'").Length);
				AssertEquals(1, result.Select("Code = 'CSH' and CashFlowDescription = 'Cash or Cash Equivalent' and ActivityType = 'C' and ActivityDescription = 'Cash or Cash Equivalent'").Length);
				AssertEquals(1, result.Select("Code = 'EXX' and CashFlowDescription = 'Effects of Exchange Rate Change' and ActivityType = 'E' and ActivityDescription = 'Effects of Exchange Rate Change'").Length);
				AssertEquals(1, result.Select("Code = 'O01' and CashFlowDescription = 'Receipts From Customers' and ActivityType = 'O' and ActivityDescription = 'Operating Activities'").Length);
				AssertEquals(1, result.Select("Code = 'I01' and CashFlowDescription = 'Dividends Received' and ActivityType = 'I' and ActivityDescription = 'Investing Activities'").Length);
				AssertEquals(1, result.Select("Code = 'F01' and CashFlowDescription = 'Interests Paid' and ActivityType = 'F' and ActivityDescription = 'Financing Activities'").Length);
			});

			PrepareTestDataForIncLoad();
			ExecuteCusTableIncLoad();
			result = SelectRows();
			AssertEquals("Rowcount", 7, result.Rows.Count);
			CombineAssertions(() =>
			{
				AssertEquals(1, result.Select("Code = 'XXX' and CashFlowDescription = 'Undefined' and ActivityType = 'X' and ActivityDescription = 'Undefined Activities'").Length);
				AssertEquals(1, result.Select("Code = 'NON' and CashFlowDescription = 'Non Cash' and ActivityType = 'N' and ActivityDescription = 'Non Cash'").Length);
				AssertEquals(1, result.Select("Code = 'CSH' and CashFlowDescription = 'Cash or Cash Equivalent' and ActivityType = 'C' and ActivityDescription = 'Cash or Cash Equivalent'").Length);
				AssertEquals(1, result.Select("Code = 'EXX' and CashFlowDescription = 'Effects of Exchange Rate Change' and ActivityType = 'E' and ActivityDescription = 'Effects of Exchange Rate Change'").Length);
				AssertEquals(1, result.Select("Code = 'O01' and CashFlowDescription = 'Receipts From Customers' and ActivityType = 'O' and ActivityDescription = 'Operating Activities'").Length);
				AssertEquals(1, result.Select("Code = 'I01' and CashFlowDescription = 'Dividends Received' and ActivityType = 'I' and ActivityDescription = 'Investing Activities'").Length);
				AssertEquals(1, result.Select("Code = 'I02' and CashFlowDescription = 'Interests Paid' and ActivityType = 'I' and ActivityDescription = 'Financing Activities'").Length);
			});
		}

		protected override string ScriptDbName
		{
			get { return Db.EdwDatabaseName; }
		}

		void PrepareTestDataForIniLoad()
		{
			var sqlText = string.Format(CultureInfo.InvariantCulture, @"

				DELETE FROM [{0}].Finance.BAS__CashFlowActivityConfiguration

				INSERT [{0}].Finance.BAS__CashFlowActivityConfiguration
					([CashFlowActivityConfigurationID], [CashFlowActivityConfigurationKey], [Value])
					VALUES
						(NEWID(), 1, convert(varbinary(max), N'<ArrayOfCashFlowActivityConfiguration xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance""><CashFlowActivityConfiguration><Code>XXX</Code><Description>Undefined</Description><ActivityType>X</ActivityType><ActivityDescription>Undefined Activities</ActivityDescription></CashFlowActivityConfiguration><CashFlowActivityConfiguration><Code>NON</Code><Description>Non Cash</Description><ActivityType>N</ActivityType><ActivityDescription>Non Cash</ActivityDescription></CashFlowActivityConfiguration><CashFlowActivityConfiguration><Code>CSH</Code><Description>Cash or Cash Equivalent</Description><ActivityType>C</ActivityType><ActivityDescription>Cash or Cash Equivalent</ActivityDescription></CashFlowActivityConfiguration><CashFlowActivityConfiguration><Code>EXX</Code><Description>Effects of Exchange Rate Change</Description><ActivityType>E</ActivityType><ActivityDescription>Effects of Exchange Rate Change</ActivityDescription></CashFlowActivityConfiguration><CashFlowActivityConfiguration><Code>O01</Code><Description>Receipts From Customers</Description><ActivityType>O</ActivityType><ActivityDescription>Operating Activities</ActivityDescription></CashFlowActivityConfiguration><CashFlowActivityConfiguration><Code>I01</Code><Description>Dividends Received</Description><ActivityType>I</ActivityType><ActivityDescription>Investing Activities</ActivityDescription></CashFlowActivityConfiguration><CashFlowActivityConfiguration><Code>F01</Code><Description>Interests Paid</Description><ActivityType>F</ActivityType><ActivityDescription>Financing Activities</ActivityDescription></CashFlowActivityConfiguration></ArrayOfCashFlowActivityConfiguration>'))", ScriptDbName
			);
			TestConnection.ExecuteNonQuery(sqlText);
		}

		void PrepareTestDataForIncLoad()
		{
			var sqlText = string.Format(CultureInfo.InvariantCulture, @"

				DELETE FROM [{0}].Finance.BAS__CashFlowActivityConfiguration

				INSERT [{0}].Finance.BAS__CashFlowActivityConfiguration
					([CashFlowActivityConfigurationID], [CashFlowActivityConfigurationKey], [Value])
					VALUES
						(NEWID(), 1, convert(varbinary(max), N'<ArrayOfCashFlowActivityConfiguration xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance""><CashFlowActivityConfiguration><Code>XXX</Code><Description>Undefined</Description><ActivityType>X</ActivityType><ActivityDescription>Undefined Activities</ActivityDescription></CashFlowActivityConfiguration><CashFlowActivityConfiguration><Code>NON</Code><Description>Non Cash</Description><ActivityType>N</ActivityType><ActivityDescription>Non Cash</ActivityDescription></CashFlowActivityConfiguration><CashFlowActivityConfiguration><Code>CSH</Code><Description>Cash or Cash Equivalent</Description><ActivityType>C</ActivityType><ActivityDescription>Cash or Cash Equivalent</ActivityDescription></CashFlowActivityConfiguration><CashFlowActivityConfiguration><Code>EXX</Code><Description>Effects of Exchange Rate Change</Description><ActivityType>E</ActivityType><ActivityDescription>Effects of Exchange Rate Change</ActivityDescription></CashFlowActivityConfiguration><CashFlowActivityConfiguration><Code>O01</Code><Description>Receipts From Customers</Description><ActivityType>O</ActivityType><ActivityDescription>Operating Activities</ActivityDescription></CashFlowActivityConfiguration><CashFlowActivityConfiguration><Code>I01</Code><Description>Dividends Received</Description><ActivityType>I</ActivityType><ActivityDescription>Investing Activities</ActivityDescription></CashFlowActivityConfiguration><CashFlowActivityConfiguration><Code>I02</Code><Description>Interests Paid</Description><ActivityType>I</ActivityType><ActivityDescription>Financing Activities</ActivityDescription></CashFlowActivityConfiguration></ArrayOfCashFlowActivityConfiguration>'))

				INSERT INTO [{0}].[biadmin].[TransformedRow]
					(SchemaName, TableName, KeyValue)
				VALUES
					('Finance', 'BAS__CashFlowActivityConfiguration', 1)
			", ScriptDbName);
			TestConnection.ExecuteNonQuery(sqlText);
		}

		void ExecuteCusTableIniLoad()
		{
			var sqlText = string.Format(CultureInfo.InvariantCulture,
				"SELECT InitialLoadQuery FROM [{0}].[biAdmin].[CustomTableConfiguration] WHERE  [ModelSchemaName] = 'Finance' AND [ModelTableName] = 'GRP__CashFlowActivityConfiguration'",
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
				"SELECT IncrementalLoadQuery FROM [{0}].[biAdmin].[CustomTableConfiguration] WHERE  [ModelSchemaName] = 'Finance' AND [ModelTableName] = 'GRP__CashFlowActivityConfiguration'",
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
			return DataUtils.GetDataTableFromQuery(TestConnection, $"SELECT * FROM [{ScriptDbName}].[Finance].GRP__CashFlowActivityConfiguration");
		}

		protected override DbConnection TestConnection => adminConnection ?? (adminConnection = Db.NewAdminConnection());
		AdminConnection adminConnection;
	}
}
