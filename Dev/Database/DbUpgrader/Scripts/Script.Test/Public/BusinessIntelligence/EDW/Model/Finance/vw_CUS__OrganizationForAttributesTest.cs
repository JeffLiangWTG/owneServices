using System.Data;
using System.Globalization;
using System.Linq;
using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.BusinessIntelligence.EDW.Model.Finance;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.BusinessIntelligence.EDW.Model.Finance.Testing
{
	[TestedType(typeof(vw_CUS__OrganizationForAttributes))]
	internal class vw_CUS__OrganizationForAttributesTest : BiCreateScriptTest
	{
		public void TestColumns()
		{
			var columns = new[]
			{
				"OrganizationForAttributesKey",
				"OrganizationKey",
				"OrganizationCode",
				"OrganizationAddressKey",
				"EconomicGrouping",
				"CountryKey",
				"CountryCode"
			};

			var result = SelectRows();
			foreach (string column in columns)
			{
				Assert($"{column} should be contained", result.Columns.Contains(column));
			}
			AssertEquals(columns.Length, result.Columns.Count);
		}

		public void TestViewData()
		{
			var result = SelectRows("vw_CUS__OrganizationForAttributes");
			AssertInitialData(result);
		}

		public void TestIniLoad()
		{
			var result = SelectRows();
			AssertInitialData(result);
		}

		public void TestIncLoadForInsertNewOrganization()
		{
			var sqlText = string.Format(CultureInfo.InvariantCulture, $@"

				INSERT [{ScriptDbName}].[Geography].[BAS__Country]
					([CountryKey], [CountryID], [Code], [EconomicGrouping])
					VALUES
						(2, NEWID(), 'CN', 'AWM')

				INSERT [{ScriptDbName}].[Organization].[BAS__Organization]
					([OrganizationKey], [OrganizationID], [Code])
					VALUES
						(2, NEWID(), 'KRAFOOCHI')

				INSERT [{ScriptDbName}].[Organization].[BAS__OrganizationAddress]
					([OrganizationAddressKey], [OrganizationAddressID], [OrganizationKey], [CountryCode], [IsActive], [SystemCreateTimeUtc])
					VALUES
						(2, NEWID(), 2, 'CN', 1, '2024-10-17')

				INSERT [{ScriptDbName}].[Organization].[BAS__OrganizationAddressCapability]
					([OrganizationAddressCapabilityKey], [OrganizationAddressCapabilityID], [OrganizationAddressKey], [IsMainAddress], [AddressType])
					VALUES
						(2, NEWID(), 2, 1, 'OFC')

				INSERT INTO [{ScriptDbName}].[biadmin].[TransformedRow] (SchemaName, TableName, KeyValue)
				VALUES
				('Geography', 'BAS__Country', 2),
				('Organization', 'BAS__Organization', 2),
				('Organization', 'BAS__OrganizationAddress', 2),
				('Organization', 'BAS__OrganizationAddressCapability', 2)
				"
			);

			TestConnection.ExecuteNonQuery(sqlText);
			ExecuteCusTableLoad("IncrementalLoadQuery");
			var result = SelectRows();

			AssertEquals("Rowcount", 2, result.Rows.Count);
			AssertEquals(1, result.Select("OrganizationCode = 'KRAFOOCHI' and CountryCode = 'CN' and OrganizationAddressKey = 2 and OrganizationKey = 2 and EconomicGrouping = 'AWM' and CountryKey = 2").Length);
		}

		public void TestIncLoadForUpdateCountry()
		{
			var sqlText = string.Format(CultureInfo.InvariantCulture, $@"

				UPDATE [{ScriptDbName}].[Geography].[BAS__Country] SET EconomicGrouping = 'TET' WHERE CountryKey = 1

				INSERT INTO [{ScriptDbName}].[biadmin].[TransformedRow] (SchemaName, TableName, KeyValue)
				VALUES
				('Geography', 'BAS__Country', 1)
				"
			);

			TestConnection.ExecuteNonQuery(sqlText);
			ExecuteCusTableLoad("IncrementalLoadQuery");
			var result = SelectRows();

			AssertEquals("Rowcount", 1, result.Rows.Count);
			AssertEquals(1, result.Select("OrganizationCode = 'ANEINTSIN' and CountryCode = 'AU' and OrganizationAddressKey = 1 and OrganizationKey = 1 and EconomicGrouping = 'TET' and CountryKey = 1").Length);
			AssertTransformedRow();
		}

		public void TestIncLoadForUpdateOrganization()
		{
			var sqlText = string.Format(CultureInfo.InvariantCulture, $@"

				UPDATE [{ScriptDbName}].[Organization].[BAS__Organization] SET Code = 'CCCCCCCCC' WHERE OrganizationKey = 1

				INSERT INTO [{ScriptDbName}].[biadmin].[TransformedRow] (SchemaName, TableName, KeyValue)
				VALUES
				('Organization', 'BAS__Organization', 1)
				"
			);

			TestConnection.ExecuteNonQuery(sqlText);
			ExecuteCusTableLoad("IncrementalLoadQuery");
			var result = SelectRows();

			AssertEquals("Rowcount", 1, result.Rows.Count);
			AssertEquals(1, result.Select("OrganizationCode = 'CCCCCCCCC' and CountryCode = 'AU' and OrganizationAddressKey = 1 and OrganizationKey = 1 and EconomicGrouping = 'BLN' and CountryKey = 1").Length);
			AssertTransformedRow();
		}

		public void TestIncLoadForUpdateOrganizationAddress()
		{
			var sqlText = string.Format(CultureInfo.InvariantCulture, $@"

				UPDATE [{ScriptDbName}].[Organization].[BAS__OrganizationAddress] SET Code = 'CCCCCCCCC' WHERE OrganizationAddressKey = 1

				INSERT INTO [{ScriptDbName}].[biadmin].[TransformedRow] (SchemaName, TableName, KeyValue)
				VALUES
				('Organization', 'BAS__OrganizationAddress', 1)
				"
			);

			TestConnection.ExecuteNonQuery(sqlText);
			ExecuteCusTableLoad("IncrementalLoadQuery");
			var result = SelectRows();

			AssertEquals("Rowcount", 1, result.Rows.Count);
			AssertEquals(1, result.Select("OrganizationCode = 'ANEINTSIN' and CountryCode = 'AU' and OrganizationAddressKey = 1 and OrganizationKey = 1 and EconomicGrouping = 'BLN' and CountryKey = 1").Length);
			AssertTransformedRow();
		}

		public void TestIncLoadForUpdateOrganizationAddressCapability()
		{
			var sqlText = string.Format(CultureInfo.InvariantCulture, $@"

				UPDATE [{ScriptDbName}].[Organization].[BAS__OrganizationAddressCapability] SET IsMainAddress = 0 WHERE OrganizationAddressCapabilityKey = 1

				INSERT [{ScriptDbName}].[Organization].[BAS__OrganizationAddress]
				([OrganizationAddressKey], [OrganizationAddressID], [OrganizationKey], [CountryCode], [IsActive], [SystemCreateTimeUtc])
				VALUES
				(2, NEWID(), 1, 'AU', 1, '2024-10-17')

				INSERT [{ScriptDbName}].[Organization].[BAS__OrganizationAddressCapability]
				([OrganizationAddressCapabilityKey], [OrganizationAddressCapabilityID], [OrganizationAddressKey], [IsMainAddress], [AddressType])
				VALUES
				(2, NEWID(), 2, 1, 'OFC')

				INSERT INTO [{ScriptDbName}].[biadmin].[TransformedRow] (SchemaName, TableName, KeyValue, RefValue1)
				VALUES
				('Organization', 'BAS__OrganizationAddressCapability', 1, 1),
				('Organization', 'BAS__OrganizationAddressCapability', 2, 2),
				('Organization', 'BAS__OrganizationAddress', 2, null)
				"
			);

			TestConnection.ExecuteNonQuery(sqlText);
			ExecuteCusTableLoad("IncrementalLoadQuery");
			var result = SelectRows();

			AssertEquals("Rowcount", 1, result.Rows.Count);
			AssertEquals(1, result.Select("OrganizationCode = 'ANEINTSIN' and CountryCode = 'AU' and OrganizationAddressKey = 2 and OrganizationKey = 1 and EconomicGrouping = 'BLN' and CountryKey = 1").Length);
			AssertTransformedRow();
		}

		void AssertTransformedRow()
		{
			var transformedRow = DataUtils.GetDataTableFromQuery(TestConnection, $"SELECT * FROM [{ScriptDbName}].[biadmin].[TransformedRow] WHERE SchemaName = 'Finance' AND TableName = 'CUS__OrganizationForAttributes'");
			AssertEquals("transformedRowcount", 2, transformedRow.Rows.Count);
			AssertEquals(1, transformedRow.Select("KeyValue = 1 and RefValue1 = 1 and RefValue2 = 1 and RefValue3 = 1").Length);
			AssertEquals(1, transformedRow.Select("KeyValue = 2").Length);
		}

		void AssertInitialData(DataTable result)
		{
			AssertEquals("Rowcount", 1, result.Rows.Count);
			AssertEquals(1, result.Select("OrganizationCode = 'ANEINTSIN' and CountryCode = 'AU' and OrganizationAddressKey = 1 and OrganizationKey = 1 and EconomicGrouping = 'BLN' and CountryKey = 1").Length);
		}

		protected override string ScriptDbName
		{
			get { return Db.EdwDatabaseName; }
		}

		DataTable SelectRows(string tableViewName = "CUS__OrganizationForAttributes")
		{
			return DataUtils.GetDataTableFromQuery(TestConnection, $"SELECT * FROM [{ScriptDbName}].[{ScriptToTest.SchemaName}].[{tableViewName}]");
		}

		protected override void SetUp()
		{
			base.SetUp();
			PrepareData();
			ExecuteCusTableLoad("InitialLoadQuery");
		}

		void ExecuteCusTableLoad(string sqlName)
		{
			var sqlText = string.Format(CultureInfo.InvariantCulture,
				"SELECT {0} FROM [{1}].[biAdmin].[CustomTableConfiguration] WHERE  [ModelSchemaName] = 'Finance' AND [ModelTableName] = 'CUS__OrganizationForAttributes'",
				sqlName, ScriptDbName
			);
			var resultTable = DataUtils.GetDataTableFromQuery(TestConnection, sqlText);
			var record = resultTable.Select().Single();
			var incLoadSQLText = record.ItemArray[0].ToString();
			var sqlTextDL = "USE " + ScriptDbName + " " + incLoadSQLText;
			TestConnection.ExecuteNonQuery(sqlTextDL);
		}

		void PrepareData()
		{
			var sqlText = string.Format(CultureInfo.InvariantCulture, $@"
				DELETE FROM [{ScriptDbName}].[Organization].[BAS__OrganizationAddressCapability];
				DELETE FROM [{ScriptDbName}].[Organization].[BAS__OrganizationAddress];
				DELETE FROM [{ScriptDbName}].[Geography].[BAS__Country];
				DELETE FROM [{ScriptDbName}].[Organization].[BAS__Organization];

				INSERT [{ScriptDbName}].[Geography].[BAS__Country]
					([CountryKey], [CountryID], [Code], [EconomicGrouping])
					VALUES
						(1, NEWID(), 'AU', 'BLN')

				INSERT [{ScriptDbName}].[Organization].[BAS__Organization]
					([OrganizationKey], [OrganizationID], [Code])
					VALUES
						(1, NEWID(), 'ANEINTSIN')

				INSERT [{ScriptDbName}].[Organization].[BAS__OrganizationAddress]
					([OrganizationAddressKey], [OrganizationAddressID], [OrganizationKey], [CountryCode], [IsActive], [SystemCreateTimeUtc])
					VALUES
						(1, NEWID(), 1, 'AU', 1, '2024-10-16')

				INSERT [{ScriptDbName}].[Organization].[BAS__OrganizationAddressCapability]
					([OrganizationAddressCapabilityKey], [OrganizationAddressCapabilityID], [OrganizationAddressKey], [IsMainAddress], [AddressType])
					VALUES
						(1, NEWID(), 1, 1, 'OFC')
				"
			);

			TestConnection.ExecuteNonQuery(sqlText);
		}
	}
}
