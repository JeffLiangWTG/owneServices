using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.BusinessIntelligence.EDW.Model.Organization;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.BusinessIntelligence.EDW.Model.Organization.Testing
{
	[TestedType(typeof(vw_GRP__OrgRelatedParty))]
	class vw_GRP__OrgRelatedPartyTest : BiCreateScriptTest
	{
		[ExpectNoExceptions]
		public void TestRun()
		{
			TestColumnsAreAsExpected(ScriptToTest.Name, GetColumns());

			PrepareTestData();
			Execute();

			var resultTable = SelectRows();
			AssertEquals("Row Count", 1, resultTable.Rows.Count);

			AssertRowValues(resultTable, "Test 1 (Filter TRUE): ", new Guid("ADC2FB93-135D-4B38-AD2E-1B6274F197CF"), new Guid("1FD4E7C9-42CE-4381-AE2C-4600EC4C27BA"), new Guid("1FD4E7C9-42CE-4381-AE2C-4600EC4C27BA"));
		}

		HashSet<string> GetColumns()
		{
			return new HashSet<string>
			{
				"CompanyID",
				"ParentOrganizationID",
				"RelatedPartyID"
			};
		}

		void PrepareTestData()
		{
			var sqlText = string.Format(CultureInfo.InvariantCulture, @"
				INSERT [{0}].[Organization].[BAS__OrganizationRelatedParty]
					(CompanyID, ParentOrganizationID, RelatedParty, PartyType, FreightDirection, Location, OrganizationRelatedPartyID, OrganizationRelatedPartyKey)
				VALUES
					('ADC2FB93-135D-4B38-AD2E-1B6274F197CF', '1FD4E7C9-42CE-4381-AE2C-4600EC4C27BA', '1FD4E7C9-42CE-4381-AE2C-4600EC4C27BA', 'ARS', 'AR', '', NEWID(), 1),
					('7E71843A-A5C6-4159-A1DD-23C875E2452D', 'DD1587E6-BF49-45C6-98C4-08BB69FC51FF', 'DD1587E6-BF49-45C6-98C4-08BB69FC51FF', 'RRT', 'PIC', 'ABC', NEWID(), 2);
				", ScriptDbName
			);
			TestConnection.ExecuteNonQuery(sqlText);
		}

		DataTable SelectRows()
		{
			var sqlText = string.Format(CultureInfo.InvariantCulture,
				"SELECT * FROM [{0}].[Organization].[vw_GRP__OrgRelatedParty]",
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

		void Execute()
		{
			var sqlText = string.Format(CultureInfo.InvariantCulture,
				"SELECT * FROM [{0}].[{1}].[{2}]",
				ScriptDbName,
				ScriptToTest.SchemaName,
				ScriptToTest.Name
			);
			TestConnection.ExecuteNonQuery(sqlText);
		}
	}
}
