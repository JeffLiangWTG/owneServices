using System;
using System.Data;
using System.Globalization;
using System.Linq;
using CargoWise.Data;
using CargoWise.Data.Testing;
using CargoWise.DbUpgrader.Scripts.Definitions.BusinessIntelligence.EDW.ReportFunctions.Accounting;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Testing.Public.BusinessIntelligence.EDW.ReportFunctions.Accounting.Testing
{
	[TestedType(typeof(GetJobParentsByControllingCustomerAndAgent_RootMNGNames))]
	class GetJobParentsByControllingCustomerAndAgent_RootMNGNamesEDWTest : BiCreateScriptTest
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
				Execute(edwConnection);
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

				EndTest(edwConnection);
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
				@"SELECT * FROM [{0}].[dbo].[GetRootMNGNamesEDW](@ultimateMNGPKs, @MNGListIsEmpty)",
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

		string GetIniLoadSQLText(AdminConnection edwConnection)
		{
			var sqlText = string.Format(CultureInfo.InvariantCulture,
					"SELECT [InitialLoadQuery] FROM [{0}].[biAdmin].[CustomTableConfiguration] WHERE  [ModelSchemaName] = 'Finance' AND [ModelTableName] = 'GRP__RootMNGNames'",
					ScriptDbName
			);

			var resultTable = DataUtils.GetDataTableFromQuery(edwConnection, sqlText);
			var record = resultTable.Select().Single();
			var iniLoadSQLText = record.ItemArray[0].ToString();
			return iniLoadSQLText;
		}

		void Execute(AdminConnection edwConnection)
		{
			var iniLoadSQLText = GetIniLoadSQLText(edwConnection);
			var sqlText1 = "USE " + ScriptDbName + " " + iniLoadSQLText;

			edwConnection.ExecuteNonQuery(sqlText1);
		}

		void EndTest(AdminConnection edwConnection)
		{
			var sqlText = "USE " + Db.DatabaseName;
			edwConnection.ExecuteNonQuery(sqlText);
		}

		public void TestSampleCallEDW()
		{
			var sqlCmd = $@"
			INSERT INTO {Db.EdwDatabaseName}.Organization.BAS__Organization(OrganizationID, OrganizationKey, Code, FullName)
VALUES('32EC689D-09BE-40C1-A4B9-C6A37E2ECB6C', 1, 'O1', 'Org 1'),
('257123E1-589D-4926-A8FF-5A186317F57F', 2, 'O2', 'Org 2'),
('CB7B4B03-5072-4D4D-B045-67E248D6C773', 3, 'O3', 'Org 3')

INSERT INTO {Db.EdwDatabaseName}.Organization.BAS__OrganizationAddress(OrganizationAddressID,OrganizationAddressKey, OrganizationKey, Code, Address1)
VALUES ('6F1755E3-D182-4844-9399-E8585E8D3BE8', 1, 1, 'ADD1', '25 Ocean St, Alexandria, NSW, Australia'),
 ('0DFAE11D-2EBE-4C8C-8DAA-B0EA68C2E126', 2, 2, 'ADD2', '58 Frederick St, Mascot, NSW, Australia'),
 ('030D36AF-940C-4166-8631-E07742040DE3', 3, 3, 'ADD3', '55 Oxford St, Newtown, NSW, Australia')

INSERT INTO {Db.EdwDatabaseName}.InternationalLogistics.BAS__Shipment (ShipmentID,ShipmentKey, JobNumber)
VALUES ('86FAD7FC-C51D-4AB2-AD06-4F8DBDB312B4', 1, 'S00001'),
 ('BE507CEF-415D-47F5-8377-310A706A1FDD', 2, 'S00002'),
 ('A0B7FE8D-63CA-4E98-89CB-33F7E9FE65C0', 3, 'S00003')

INSERT INTO {Db.EdwDatabaseName}.InternationalLogistics.BAS__DocAddress(DocAddressID, DocAddressKey, OrganizationAddressKey, CompanyName, ParentID, ParentTableCode, AddressType, AddressSequence)
VALUES('0E25825D-4962-4E9C-A4DC-B086378B994C', 1, 1, 'AU COMPANY', '86FAD7FC-C51D-4AB2-AD06-4F8DBDB312B4', 'JS', 'SCP', 0),
('1B20A621-9C4E-4B2D-BED7-7F1DC7E1E163', 2, 2, 'AU COMPANY', '86FAD7FC-C51D-4AB2-AD06-4F8DBDB312B4', 'JS', 'CAG', 0),
('EE656E31-F4A0-46DE-B36B-390B50917A7E', 3, 3, 'AU COMPANY', 'BE507CEF-415D-47F5-8377-310A706A1FDD', 'JS', 'CAG', 0),
('EE656E31-F4A0-46DE-B36B-390B50917A7E', 4, 2, 'AU COMPANY', 'A0B7FE8D-63CA-4E98-89CB-33F7E9FE65C0', 'JS', 'SCP', 0)

DECLARE @CompanyPk AS UNIQUEIDENTIFIER = 'D381CB3B-281E-4BA4-B7B8-A0B045FDA68D'
DECLARE @BranchPk AS UNIQUEIDENTIFIER = '3C282841-E41C-4CBD-9A98-15ED3C775433'
DECLARE @JEPk AS UNIQUEIDENTIFIER = 'e691ccaf-ae24-4471-b7c8-7bc812e54634'
DECLARE @ControllingAgentJE AS UNIQUEIDENTIFIER = '1cef7c3e-e891-4979-9d1d-37c17c0c5a73'
DECLARE @ControllingCustomerJE AS UNIQUEIDENTIFIER = 'da361802-2425-4b03-9bf1-08e73c5eb6ff'

INSERT INTO {Db.EdwDatabaseName}.Organization.BAS__Company (CompanyKey, CompanyID, CountryCode) VALUES (1, @CompanyPk, 'AU')
INSERT INTO {Db.EdwDatabaseName}.Organization.BAS__Branch (BranchKey, BranchID, CompanyKey) VALUES (1, @BranchPk, 1)
INSERT INTO {Db.EdwDatabaseName}.Organization.BAS__Organization (OrganizationKey, OrganizationID, Code) VALUES (4, @ControllingAgentJE, 'TESTORGADL2')
INSERT INTO {Db.EdwDatabaseName}.Organization.BAS__Organization (OrganizationKey, OrganizationID, Code) VALUES (5, @ControllingCustomerJE, 'TESTORGADL3')

INSERT INTO {Db.EdwDatabaseName}.Customs.BAS__Declaration (DeclarationID, DeclarationKey, BranchKey, JE_OH_ControllingAgent, JE_OH_ControllingCustomer)
VALUES (@JEPk, 1, 1, @ControllingAgentJE, @ControllingCustomerJE)";

			TestConnection.ExecuteNonQuery(sqlCmd);
			var sql = $@"select * from {Db.EdwDatabaseName}.dbo.GetJobParentsByControllingCustomerAndAgent()";
			var result = DataUtils.GetDataTableFromQuery(TestConnection, sql);
			AssertEquals("Result should have four rows", 4, result.Rows.Count);
		}
	}
}

