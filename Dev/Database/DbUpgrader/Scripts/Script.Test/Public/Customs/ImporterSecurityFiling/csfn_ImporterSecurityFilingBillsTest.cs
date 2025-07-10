using System;
using System.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.Customs.ImporterSecurityFiling;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Customs.ImporterSecurityFiling
{
	[TestedType(typeof(csfn_ImporterSecurityFilingBills))]
	class csfn_ImporterSecurityFilingBillsTest : DbCreateScriptTest
	{
		public void Testcsfn_ImporterSecurityFilingBills()
		{
			var inertSQL = @"
			INSERT INTO dbo.GlbCompany(GC_PK, GC_RN_NKCountryCode, GC_RX_NKLocalCurrency, GC_Code, GC_Name) VALUES(@CompanyPK, 'US', 'USD', 'USC', 'US company')
			INSERT INTO dbo.GlbBranch(GB_PK, GB_GC, GB_Code, GB_RN_NKCountryCode) VALUES(@BranchPK, @CompanyPK, 'USB', 'US')

			INSERT INTO dbo.JobDeclaration(JE_PK, JE_DataModel, JE_GB, JE_GC, JE_MessageType, JE_TransportMode, JE_IsCancelled, JE_SystemCreateTimeUtc, JE_DeclarationReference, JE_ClusterKey) VALUES
			(@Declaration1PK, 'US', @BranchPK, @CompanyPK, 'IMP', 'SEA', 0, '2019-05-01', 'J0000001', 1),
			(@Declaration2PK, 'US', @BranchPK, @CompanyPK, 'IMP', 'ROA', 0, '2019-05-01', 'J0000002', 2),
			(@Declaration3PK, 'US', @BranchPK, @CompanyPK, 'EXP', 'SEA', 0, '2019-05-01', 'J0000003', 3),
			(@Declaration4PK, 'US', @BranchPK, @CompanyPK, 'IMP', 'SEA', 1, '2019-05-01', 'J0000004', 4),
			(@Declaration5PK, 'US', @BranchPK, @CompanyPK, 'IMP', 'SEA', 0, '2019-05-01', 'J0000005', 5),
			(@Declaration6PK, 'US', @BranchPK, @CompanyPK, 'IMP', 'SEA', 0, '2019-05-01', 'J0000006', 6),
			(@Declaration7PK, 'US', @BranchPK, @CompanyPK, 'IMP', 'SEA', 0, '2019-05-01', 'J0000007', 7)

			INSERT INTO dbo.CusDecHouseBill(CU_PK, CU_AddInfo, CU_JE, CU_BillNum, CU_BillType, CU_CU_ParentBill, CU_ClusterKey) VALUES
			(NEWID(), 'UI_NKBillIssuerSCAC=APLU', @Declaration1PK, 'MB89148257', 'MB', null, 1),
			(NEWID(), 'UI_NKBillIssuerSCAC=APLU', @Declaration2PK, 'MB89148257', 'MB', null, 2),
			(NEWID(), 'UI_NKBillIssuerSCAC=APLU', @Declaration3PK, 'MB89148257', 'MB', null, 3),
			(NEWID(), 'UI_NKBillIssuerSCAC=APLU', @Declaration4PK, 'MB89148257', 'MB', null, 4),
			(NEWID(), 'UI_NKBillIssuerSCAC=APLU', @Declaration5PK, 'MB891482571', 'MB', null, 5),
			(NEWID(), 'UI_NKBillIssuerSCAC=APLU', @Declaration6PK, 'MB89148257', 'HB', null, 6),
			(@CusDecHouseBill1PK, 'UI_NKBillIssuerSCAC=APLU', @Declaration7PK, 'MB891482572', 'MB', null, 7),
			(NEWID(), 'UI_NKBillIssuerSCAC=APLU', @Declaration7PK, 'MB89148257', 'MB', @CusDecHouseBill1PK, 7)

			INSERT INTO dbo.CusISFHeader(BF_PK, BF_ActionReasonCode, BF_EntryType, BF_GB, BF_ShipmentType, BF_TransportMode, BF_SystemCreateTimeUTC, BF_JobReference) VALUES
			(@ISFHeaderPK, 'CT', '1', @BranchPK, '01', '11', '2019-05-10', 'ISF000001')

			INSERT INTO dbo.CusISFBill(BB_PK, BB_BF, BB_BillNum, BB_BillType) VALUES
			(NEWID(), @ISFHeaderPK, 'APLUMB89148257', 'OB')";

			using (var command = TestConnection.Command(inertSQL))
			{
				command.AddParameter("@CompanyPK", SqlDbType.UniqueIdentifier, Guid.NewGuid());
				command.AddParameter("@BranchPK", SqlDbType.UniqueIdentifier, Guid.NewGuid());
				command.AddParameter("@Declaration1PK", SqlDbType.UniqueIdentifier, Guid.NewGuid());
				command.AddParameter("@Declaration2PK", SqlDbType.UniqueIdentifier, Guid.NewGuid());
				command.AddParameter("@Declaration3PK", SqlDbType.UniqueIdentifier, Guid.NewGuid());
				command.AddParameter("@Declaration4PK", SqlDbType.UniqueIdentifier, Guid.NewGuid());
				command.AddParameter("@Declaration5PK", SqlDbType.UniqueIdentifier, Guid.NewGuid());
				command.AddParameter("@Declaration6PK", SqlDbType.UniqueIdentifier, Guid.NewGuid());
				command.AddParameter("@Declaration7PK", SqlDbType.UniqueIdentifier, Guid.NewGuid());
				command.AddParameter("@ISFHeaderPK", SqlDbType.UniqueIdentifier, Guid.NewGuid());
				command.AddParameter("@CusDecHouseBill1PK", SqlDbType.UniqueIdentifier, Guid.NewGuid());
				command.ExecuteScalar();
			}

			var reportSql = @"SELECT * FROM csfn_ImporterSecurityFilingBills('1', '01', 'CT', NULL, NULL, 'ALL') WHERE BF_JobReference = 'ISF000001'";
			using (var command = TestConnection.Command(reportSql))
			{
				using (var reader = command.ExecuteReader())
				{
					reader.Read();
					AssertEquals("Job Ref.", "ISF000001", reader["BF_JobReference"].ToString());
					AssertEquals("Declaration Job Number", "J0000001, J0000004, J0000007", reader["DeclarationJobNumber"].ToString());
				}
			}
		}
	}
}
