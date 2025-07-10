using System;
using System.Collections.Generic;
using System.Data;
using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.Customs.US;
using Enterprise.Build.Database.Script.TestFramework;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Customs.US
{
	[TestedType(typeof(USExportDeclarations))]
	class USExportDeclarationsTest : DbCreateScriptTest
	{
		public void TestMainSupplierIDNumber()
		{
			var supplierOrgPK = TestDataCreator.CreateOrganisation("Supplier", "Test Supplier", "USPHL");
			var supplierAddressPK = TestDataCreator.CreateAddress(supplierOrgPK, "Supplier Address", "Address 1");
			TestDataCreator.CreateOrgAddressCapability(supplierAddressPK, "OFC", true);
			TestDataCreator.CreateOrgCusCode(supplierOrgPK, supplierAddressPK, "DUN", "758516744", "US");

			var companyPK = TestDataCreator.CreateCompany("DUS", "US", "USD");
			var branchPK = TestDataCreator.CreateBranch(companyPK, "PHL", "USPHL");
			var declarationPK1 = TestDataCreator.CreateJobDeclaration(branchPK, companyPK, "B0000001", "EXP", 1, null, supplierOrgPK, dataModel: "US");

			var script = $@"SELECT * FROM USExportDeclarations('{companyPK}','','','','','',null,null)";
			using (var command = TestConnection.Command(script))
			using (var reader = command.ExecuteReader())
			{
				reader.Read();
				AssertEquals("B0000001", reader["JE_DeclarationReference"].ToString());
				AssertEquals("DUN: 758516744", reader["MainSupplierIDNumber"].ToString());
			}

			var additionalInfoHelper = new AdditionalInfoHelper(new[]
			{
				new AdditionalInfoConfig("NKCountryOfExport", "UC_NKCountryOfExport", "CA"),
				new AdditionalInfoConfig("StateOfOrigin", "StateOfOrigin", "AR")
			});
			UpdateDeclarationAdditionalInfo(declarationPK1, additionalInfoHelper.AdditionalInfoText);

			script = $@"SELECT * FROM USExportDeclarations('{companyPK}','','','','','',null,null)";
			using (var command = TestConnection.Command(script))
			{
				using (var reader = command.ExecuteReader())
				{
					reader.Read();
					CombineAssertions(() =>
					{
						AssertEquals("B0000001", reader["JE_DeclarationReference"].ToString());
						AssertEquals("DUN: 758516744", reader["MainSupplierIDNumber"].ToString());
						AssertEquals("CA", reader["US_UC_NKCountryOfExport"].ToString());
						AssertEquals("AR", reader["US_StateOfOrigin"].ToString());
					});
				}
			}
		}

		public void TestOrgCusCode()
		{
			var companyPK = TestDataCreator.CreateCompany("TC1", "US", "USD");
			var branchPK = TestDataCreator.CreateBranch(companyPK, "TB1", "USCHI");

			var importerOrgPK = TestDataCreator.CreateOrganisation("ORG1", "Organization1");
			TestDataCreator.CreateOrgCusCode(importerOrgPK, "EIN", "EIN_CODE 1", "US");
			TestDataCreator.CreateOrgCusCode(importerOrgPK, "DUN", "DUN_CODE 1", "US");
			var supplierOrgPK = TestDataCreator.CreateOrganisation("ORG2", "Organization2");
			TestDataCreator.CreateOrgCusCode(supplierOrgPK, "EIN", "EIN_CODE 2", "US");
			TestDataCreator.CreateOrgCusCode(supplierOrgPK, "DUN", "DUN_CODE 2", "US");
			var forwarderOrgPK = TestDataCreator.CreateOrganisation("ORG3", "Organization3");
			TestDataCreator.CreateOrgCusCode(forwarderOrgPK, "EIN", "EIN_CODE 3", "US");
			TestDataCreator.CreateOrgCusCode(forwarderOrgPK, "DUN", "DUN_CODE 3", "US");
			var fPPIOrgPK = TestDataCreator.CreateOrganisation("ORG4", "Organization4");
			TestDataCreator.CreateOrgCusCode(fPPIOrgPK, "EIN", "EIN_CODE 4", "US");
			TestDataCreator.CreateOrgCusCode(fPPIOrgPK, "DUN", "DUN_CODE 4", "US");
			var mainDeclaration = TestDataCreator.CreateJobDeclaration(branchPK, companyPK, "B000010", "EXP", 1, supplierPK: supplierOrgPK, importerPK: importerOrgPK, forwarderPK: forwarderOrgPK, dataModel: "US");
			var usDeclaration = TestDataCreator.CreateJobUSDeclaration(mainDeclaration, fPPIOrgPK, 1);
			var reportSql = $"SELECT ImporterIDNumber, MainSupplierIDNumber, ForwarderIDNumber, FPPIIDNumber FROM USExportDeclarations('{companyPK}', '', null, null, null, null, null, null)";

			TestConnection.ExecuteReader(
				reportSql,
				(reader) =>
				{
					AssertEquals("EIN: EIN_CODE 1", (string)reader["ImporterIDNumber"]);
					AssertEquals("EIN: EIN_CODE 2", (string)reader["MainSupplierIDNumber"]);
					AssertEquals("EIN: EIN_CODE 3", (string)reader["ForwarderIDNumber"]);
					AssertEquals("EIN: EIN_CODE 4", (string)reader["FPPIIDNumber"]);
				}
			);
		}

		public void TestUSExportDeclarations()
		{
			var insertSQL = $@"
			DECLARE @BranchPK UNIQUEIDENTIFIER, @Declaration1PK UNIQUEIDENTIFIER, @Declaration2PK UNIQUEIDENTIFIER, @Declaration3PK UNIQUEIDENTIFIER
			SET @BranchPK = NEWID()
			SET @Declaration1PK = NEWID()
			SET @Declaration2PK = NEWID()
			SET @Declaration3PK = NEWID()

			INSERT INTO dbo.GlbCompany(GC_PK, GC_RN_NKCountryCode, GC_RX_NKLocalCurrency, GC_Code, GC_Name) VALUES('{GCPK}', 'US', 'USD', 'USC', 'US company')
			INSERT INTO dbo.GlbBranch(GB_PK, GB_GC, GB_Code, GB_RN_NKCountryCode) VALUES(@BranchPK, '{GCPK}', 'USB', 'US')

			INSERT INTO dbo.JobDeclaration(JE_PK, JE_DataModel, JE_GB, JE_GC, JE_MessageType, JE_TransportMode, JE_IsCancelled, JE_SystemCreateTimeUtc, JE_DeclarationReference, JE_ClusterKey) VALUES
			(@Declaration1PK, 'US', @BranchPK, '{GCPK}', 'EXP', 'SEA', 0, '2019-05-01', 'J0000001', 1),
			(@Declaration2PK, 'US', @BranchPK, '{GCPK}', 'EXP', 'SEA', 0, '2019-05-01', 'J0000002', 2),
			(@Declaration3PK, 'US', @BranchPK, '{GCPK}', 'EXP', 'SEA', 0, '2019-05-01', 'J0000003', 3)

			INSERT INTO dbo.CusEntryHeader (CH_PK, CH_DataModel, CH_JE, CH_MessageType, CH_Status, CH_ClusterKey, CH_SystemCreateTimeUtc, CH_SystemCreateUser, CH_SystemLastEditTimeUtc, CH_SystemLastEditUser) VALUES 
			(NEWID(), 'US', @Declaration1PK, 'ITN', 'DSC', 1, getutcdate(), '~BP', getutcdate(), '~BP'),
			(NEWID(), 'US', @Declaration2PK, 'ITN', 'OSC', 2, getutcdate(), '~BP', getutcdate(), '~BP'),
			(NEWID(), 'US', @Declaration3PK, 'ITN', 'DSC', 3, getutcdate(), '~BP', getutcdate(), '~BP'),
			(NEWID(), 'US', @Declaration3PK, 'ITN', 'OSC', 3, getutcdate(), '~BP', getutcdate(), '~BP')";

			using (var command = TestConnection.Command(insertSQL))
			{
				command.ExecuteScalar();
			}

			AssertCH_Status("DSC");
			AssertCH_Status("OSC");
			AssertCH_Status("MES");
		}

		public void TestExportDate()
		{
			var supplierOrgPK = TestDataCreator.CreateOrganisation("Supplier", "Test Supplier", "USPHL");
			var supplierAddressPK = TestDataCreator.CreateAddress(supplierOrgPK, "Supplier Address", "Address 1");
			TestDataCreator.CreateOrgAddressCapability(supplierAddressPK, "OFC", true);
			TestDataCreator.CreateOrgCusCode(supplierOrgPK, supplierAddressPK, "DUN", "758516744", "US");

			var companyPK = TestDataCreator.CreateCompany("TC1", "US", "USD");
			var branchPK = TestDataCreator.CreateBranch(companyPK, "TB1", "USCHI");

			var now = DateTime.Now;

			var declarationPk1 = TestDataCreator.CreateJobDeclaration(branchPK, companyPK, "Test001", "EXP", 1, dataModel: "US", addInfo: $"DateOfExport={now.AddMinutes(1):yyyy-MM-dd HH:mm:ss}");
			var declarationPk2 = TestDataCreator.CreateJobDeclaration(branchPK, companyPK, "Test002", "EXP", 2, dataModel: "US", addInfo: $"DateOfExport={now.AddMinutes(-3):yyyy-MM-dd HH:mm:ss}");
			var declarationPk3 = TestDataCreator.CreateJobDeclaration(branchPK, companyPK, "Test003", "EXP", 3, dataModel: "US", addInfo: $"DateOfExport={now.AddMinutes(3):yyyy-MM-dd HH:mm:ss}");

			var reportSql = @"SELECT JE_DeclarationReference FROM USExportDeclarations(@companyPK, null, @exportDateFrom, @exportDateTo, null, null, null, null)";
			using (var command = Db.Connection.Command(reportSql))
			{
				var retList = new List<string>();
				command.AddParameter("@companyPK", SqlDbType.UniqueIdentifier, companyPK);
				command.AddParameter("@exportDateFrom", SqlDbType.SmallDateTime, now);
				command.AddParameter("@exportDateTo", SqlDbType.SmallDateTime, now.AddMinutes(2));

				using (var reader = command.ExecuteReader())
				{
					while (reader.Read())
					{
						retList.Add((string)reader["JE_DeclarationReference"]);
					}
				}
				AssertEquals(1, retList.Count);
				AssertEquals("Test001", retList[0]);
			}
		}

		void AssertCH_Status(string status)
		{
			var script = $@"SELECT count(*) FROM USExportDeclarations('{GCPK}','','','','2019-04-01','2019-06-01',NULL,NULL) WHERE EXPStatus = '{status}'";
			using (var command = TestConnection.Command(script))
			{
				AssertEquals(1, (int)command.ExecuteScalar());
			}
		}

		void UpdateDeclarationAdditionalInfo(Guid declarationPK, string additionalInfo)
		{
			var updateSQL = @"
UPDATE dbo.JobDeclaration
SET
	JE_AddInfo = @additionalInfo,
	JE_SystemLastEditTimeUtc = GETUTCDATE(),
	JE_SystemLastEditUser = '~BP'
WHERE
	JE_PK = @declarationPK";

			using (var command = Db.Connection.Command(updateSQL))
			{
				command.AddParameter("@declarationPK", SqlDbType.UniqueIdentifier, declarationPK);
				command.AddParameter("@additionalInfo", SqlDbType.Text, additionalInfo);
				command.ExecuteNonQuery();
			}
		}

		readonly Guid GCPK = Guid.NewGuid();
	}
}
