using System;
using System.Data;
using System.Globalization;
using System.Linq;
using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.BusinessIntelligence.EDW.Model.Organization;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.BusinessIntelligence.EDW.Model.Organization.Testing
{
	[TestedType(typeof(vw_GRP__OrgRelatedParty))]
	class usp_IncLoad_GRP__OrgRelatedPartyTest : BiCreateScriptTest
	{
		[ExpectNoExceptions]
		public void TestRun()
		{
			PrepareTestData();
			Execute();

			var resultTable = SelectRows();
			AssertEquals("Rowcount", 1, resultTable.Rows.Count);

			AssertRowValues(resultTable, "Test 1 (Filter TRUE): ", new Guid("ADC2FB93-135D-4B38-AD2E-1B6274F197CF"), new Guid("1FD4E7C9-42CE-4381-AE2C-4600EC4C27BA"), new Guid("1FD4E7C9-42CE-4381-AE2C-4600EC4C27BA"));
		}

		void PrepareTestData()
		{
			var sqlText = string.Format(CultureInfo.InvariantCulture, @"
				INSERT [{0}].[Organization].[BAS__OrganizationRelatedParty]
					(CompanyID, ParentOrganizationID, RelatedParty, PartyType, FreightDirection, Location, OrganizationRelatedPartyID, OrganizationRelatedPartyKey)
				VALUES
					('ADC2FB93-135D-4B38-AD2E-1B6274F197CF', '1FD4E7C9-42CE-4381-AE2C-4600EC4C27BA', '1FD4E7C9-42CE-4381-AE2C-4600EC4C27BA', 'ARS', 'AR', '', NEWID(), 1),
					('7E71843A-A5C6-4159-A1DD-23C875E2452D', 'DD1587E6-BF49-45C6-98C4-08BB69FC51FF', 'DD1587E6-BF49-45C6-98C4-08BB69FC51FF', 'RRT', 'PIC', 'ABC', NEWID(), 2);

				INSERT INTO [{0}].[biadmin].[TransformedRow]
					(SchemaName, TableName, RefValue1, RefValue2)
					SELECT 'Organization', 'BAS__OrganizationRelatedParty', CompanyID, ParentOrganizationID
					FROM [{0}].[Organization].[BAS__OrganizationRelatedParty]
				", ScriptDbName
			);
			TestConnection.ExecuteNonQuery(sqlText);
		}

		DataTable SelectRows()
		{
			var sqlText = string.Format(CultureInfo.InvariantCulture,
					"SELECT * FROM [{0}].[Organization].[GRP__OrgRelatedParty]",
					ScriptDbName
			);

			return DataUtils.GetDataTableFromQuery(TestConnection, sqlText);
		}

		void AssertRowValues(DataTable resultTable, string testName, Guid? companyID, Guid? parentOrganizationID, Guid? relatedPartyID)
		{
			var selectQuery = string.Format("CompanyID {0} AND ParentOrganizationID {1} AND RelatedPartyID {2}",
				companyID == null ? "IS NULL" : "= '" + companyID + "'",
				parentOrganizationID == null ? "IS NULL" : "= '" + parentOrganizationID + "'",
				relatedPartyID == null ? "IS NULL" : "= '" + relatedPartyID + "'"
			);

			var rows = resultTable.Select(selectQuery);

			AssertEquals(testName, 1, rows.Length);
		}

		protected override string ScriptDbName
		{
			get { return Db.EdwDatabaseName; }
		}

		string GetIncLoadSQLText()
		{
			var sqlText = string.Format(CultureInfo.InvariantCulture,
					"SELECT [IncrementalLoadQuery] FROM [{0}].[biadmin].[CustomTableConfiguration] WHERE [ModelSchemaName] = 'Organization' AND [ModelTableName] = 'GRP__OrgRelatedParty'",
					ScriptDbName
			);

			var resultTable = DataUtils.GetDataTableFromQuery(TestConnection, sqlText);
			var record = resultTable.Select().Single();
			var incLoadSQLText = record.ItemArray[0].ToString();
			return incLoadSQLText;
		}

		void Execute()
		{
			var incLoadSQLText = GetIncLoadSQLText();
			var sqlText1 = "USE " + ScriptDbName + " " + incLoadSQLText;
			var sqlText2 = "USE " + Db.DatabaseName;

			TestConnection.ExecuteNonQuery(sqlText1);
			TestConnection.ExecuteNonQuery(sqlText2);
		}
	}
}
