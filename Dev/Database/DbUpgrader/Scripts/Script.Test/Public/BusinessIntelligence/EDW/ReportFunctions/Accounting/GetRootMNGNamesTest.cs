using System;
using System.Data;
using System.Linq;
using CargoWise.Data;
using CargoWise.Data.Testing;
using CargoWise.DbUpgrader.Scripts.Definitions.BusinessIntelligence.EDW.ReportFunctions.Accounting;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.BusinessIntelligence.EDW.ReportFunctions.Accounting.Testing
{
	[TestedType(typeof(GetRootMNGNames))]
	class GetRootMNGNamesTest : BiCreateScriptTest
	{
		/// <summary>
		/// Basic auto-generated test.
		/// Please replace it with more elaborated tests.
		/// </summary>

		[ExpectNoExceptions]
		[UseSnapshotProtection]
		public void TestSampleCall()
		{
			using (var edwConnection = Db.NewAdminConnection(ScriptDbName))
			{
				edwConnection.BeginTransaction();
				PrepareTestData(edwConnection);
				var result = GetResultSet(edwConnection, Array.Empty<Guid>());
				AssertEquals("Result should have one row", 3, result.Rows.Count);

				result = GetResultSet(edwConnection, new Guid[] { org1 });
				AssertEquals("Result should have two row", 2, result.Rows.Count);
				AssertContainsExactElementsInAnyOrder(new[] { org1_1, org1_1_1 }, result.Rows.Cast<DataRow>().Select(x => x["OrgPK"]));
				AssertContainsExactElementsInAnyOrder(new[] { "MNG 1", "MNG 1" }, result.Rows.Cast<DataRow>().Select(x => x["MNGName"]));
				AssertContainsExactElementsInAnyOrder(new[] { "MNG 1.1", "MNG 1" }, result.Rows.Cast<DataRow>().Select(x => x["RelatedMNGName"]));

				result = GetResultSet(edwConnection, new Guid[] { org2 });
				AssertEquals("Result should have one row", 1, result.Rows.Count);
				AssertContainsExactElementsInAnyOrder(new[] { org2_1 }, result.Rows.Cast<DataRow>().Select(x => x["OrgPK"]));
				AssertContainsExactElementsInAnyOrder(new[] { "MNG 2" }, result.Rows.Cast<DataRow>().Select(x => x["MNGName"]));
				AssertContainsExactElementsInAnyOrder(new[] { "MNG 2" }, result.Rows.Cast<DataRow>().Select(x => x["RelatedMNGName"]));

				result = GetResultSet(edwConnection, new Guid[] { org3 });
				AssertEquals("Result should have no row", 0, result.Rows.Count);
			}
		}

		void PrepareTestData(AdminConnection edwConnection)
		{
			org1 = CreateOrganisation(edwConnection, "O1", "MNG 1", 1);
			org2 = CreateOrganisation(edwConnection, "O2", "MNG 2", 2);
			org3 = CreateOrganisation(edwConnection, "O3", "Org 3", 3);

			org1_1 = CreateOrganisation(edwConnection, "O11", "MNG 1.1", 4);
			org1_2 = CreateOrganisation(edwConnection, "O12", "MNG 1.2", 5);
			org2_1 = CreateOrganisation(edwConnection, "O21", "MNG 2.1", 6);

			org1_1_1 = CreateOrganisation(edwConnection, "O111", "Child 1.1.1", 7);
			org1_1_2 = CreateOrganisation(edwConnection, "O112", "Child 1.1.2", 8);

			CreateOrgRelatedParty(edwConnection, org1_1_1, org1_1, "MNG", 11, null);
			CreateOrgRelatedParty(edwConnection, org1_1_2, org1_1, "MNG", 22, 30);
			CreateOrgRelatedParty(edwConnection, org1_1, org1, "MNG", 33, null);
			CreateOrgRelatedParty(edwConnection, org1_2, org1, "MNG", 44, 30);

			CreateOrgRelatedParty(edwConnection, org2_1, org2, "MNG", 55, null);
		}

		Guid CreateOrganisation(AdminConnection edwConnection, string code, string name, int orgKey)
		{
			var orgID = Guid.NewGuid();

			string sql = string.Format(
				@"INSERT INTO [{0}].[Organization].[BAS__Organization] (Code, FullName, OrganizationKey, OrganizationID) VALUES ('{1}', '{2}', {3}, '{4}')",
				ScriptDbName, code, name, orgKey, orgID);

			edwConnection.ExecuteNonQuery(sql);
			return orgID;
		}

		void CreateOrgRelatedParty(AdminConnection edwConnection, Guid parent, Guid relatedParty, string partyType, int organizationRelatedPartyKey, int? companyKey)
		{
			string sql = string.Format(
				@"INSERT INTO [{0}].[Organization].[BAS__OrganizationRelatedParty] (OrganizationRelatedPartyID, ParentOrganizationID, RelatedParty, PartyType, CompanyKey, OrganizationRelatedPartyKey) VALUES (NEWID(), '{1}', '{2}', '{3}', {4}, {5})",
				ScriptDbName, parent, relatedParty, partyType, companyKey == null ? "NULL" : " " + companyKey, organizationRelatedPartyKey);

			edwConnection.ExecuteNonQuery(sql);
		}

		DataTable GetResultSet(AdminConnection edwConnection, Guid[] values)
		{
			string sql = string.Format(
				@"SELECT * FROM [{0}].[dbo].[GetRootMNGNames](@ultimateMNGPKs, @MNGListIsEmpty)",
				ScriptDbName);
			var command = edwConnection.Command(sql);
			AddTVP_uniqueidentifierAndIsEmptyParameters(command, "@ultimateMNGPKs", "@MNGListIsEmpty", values);
			return DataUtils.GetDataTableFromCommand(command);
		}

		void AddTVP_uniqueidentifierAndIsEmptyParameters(DbCommand command, string paramName, string isEmptyParamName, Guid[] values)
		{
			var table = new DataTable();
			table.Columns.Add("Value", typeof(Guid)); // Part of SQL code

			if (values != null)
			{
				foreach (var value in values)
				{
					table.Rows.Add(value);
				}
			}

			command.AddTableValuedParameter(paramName, "dbo.TVP_uniqueidentifier", table);
			command.AddParameter(isEmptyParamName, SqlDbType.Bit, table.Rows.Count == 0);
		}

		Guid org1;
		Guid org1_1;
		Guid org1_2;
		Guid org1_1_1;
		Guid org1_1_2;

		Guid org2;
		Guid org2_1;
		Guid org3;

		protected override string ScriptDbName
		{
			get { return Db.EdwDatabaseName; }
		}
	}
}
