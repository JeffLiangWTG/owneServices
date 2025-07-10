using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.Accounting;
using Enterprise.Build.Database.Script.Public.Accounting;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Testing.Public.BusinessIntelligence.EDW.ReportFunctions.Accounting
{
	[TestedType(typeof(GetJobParentsByControllingCustomerAndAgent))]
	class GetJobParentsByControllingCustomerAndAgentEdwHashTest : GetJobParentsByControllingCustomerAndAgentTest
	{
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

