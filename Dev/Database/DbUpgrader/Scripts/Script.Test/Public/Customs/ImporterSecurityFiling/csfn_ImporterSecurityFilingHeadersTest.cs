using System;
using System.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.Customs.ImporterSecurityFiling;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Customs.ImporterSecurityFiling
{
	[TestedType(typeof(csfn_ImporterSecurityFilingHeaders))]
	class csfn_ImporterSecurityFilingHeadersTest : DbCreateScriptTest
	{
		public void Testcsfn_ImporterSecurityFilingHeaders()
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

			var reportSql = @"SELECT * FROM csfn_ImporterSecurityFilingHeaders('1', '01', 'CT', NULL, 'ALL', 1) WHERE BF_JobReference = 'ISF000001'";
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

		readonly Guid BFPK1 = Guid.NewGuid();
		readonly Guid BFPK2 = Guid.NewGuid();
		readonly Guid BFPK3 = Guid.NewGuid();
		readonly Guid BFPK4 = Guid.NewGuid();
		readonly Guid BFPK5 = Guid.NewGuid();

		public void Testcsfn_ImporterSecurityFilingHeaders_numberOfDays()
		{
			var inertSQL = $@"
			DECLARE @BranchPK UniqueIdentifier, @CompanyPK UniqueIdentifier
			SET @BranchPK = newid()
			SET @CompanyPK = newid()
			INSERT INTO dbo.GlbCompany(GC_PK, GC_RN_NKCountryCode, GC_RX_NKLocalCurrency, GC_Code, GC_Name) VALUES(@CompanyPK, 'US', 'USD', 'USC', 'US company')
			INSERT INTO dbo.GlbBranch(GB_PK, GB_GC, GB_Code, GB_RN_NKCountryCode) VALUES(@BranchPK, @CompanyPK, 'USB', 'US')

			INSERT INTO dbo.CusISFHeader(BF_PK, BF_ActionReasonCode, BF_EntryType, BF_GB, BF_ShipmentType, BF_TransportMode, BF_SystemCreateTimeUTC, BF_JobReference, BF_FirstAcceptedDate) VALUES
			('{BFPK1}', 'CT', '1', @BranchPK, '01', '11', '2020-01-01', 'ISF000001', '2020-04-9')
			INSERT INTO dbo.JobConsolTransport (JW_PK, JW_ParentType,JW_RL_NKDiscPort, JW_ParentGUID, JW_ETD, JW_ETA)
			VALUES (newID(), 'ISF', 'USCHI', '{BFPK1}', '2020-04-10', '2020-04-10')

			INSERT INTO dbo.CusISFHeader(BF_PK, BF_ActionReasonCode, BF_EntryType, BF_GB, BF_ShipmentType, BF_TransportMode, BF_SystemCreateTimeUTC, BF_JobReference, BF_FirstAcceptedDate) VALUES
			('{BFPK2}', 'CT', '1', @BranchPK, '01', '11', '2020-01-01', 'ISF000002', '2020-04-10')
			INSERT INTO dbo.JobConsolTransport (JW_PK, JW_ParentType,JW_RL_NKDiscPort, JW_ParentGUID, JW_ETD, JW_ETA)
			VALUES (newID(), 'ISF', 'USCHI', '{BFPK2}', '2020-04-10', '2020-04-10')

			INSERT INTO dbo.CusISFHeader(BF_PK, BF_ActionReasonCode, BF_EntryType, BF_GB, BF_ShipmentType, BF_TransportMode, BF_SystemCreateTimeUTC, BF_JobReference, BF_FirstAcceptedDate) VALUES
			('{BFPK3}', 'CT', '1', @BranchPK, '01', '11', '2020-01-01', 'ISF000003', '2020-04-15')
			INSERT INTO dbo.JobConsolTransport (JW_PK, JW_ParentType,JW_RL_NKDiscPort, JW_ParentGUID, JW_ETD, JW_ETA)
			VALUES (newID(), 'ISF', 'USCHI', '{BFPK3}', '2020-04-10', '2020-04-10')

			INSERT INTO dbo.CusISFHeader(BF_PK, BF_ActionReasonCode, BF_EntryType, BF_GB, BF_ShipmentType, BF_TransportMode, BF_SystemCreateTimeUTC, BF_JobReference, BF_FirstAcceptedDate) VALUES
			('{BFPK4}', 'CT', '1', @BranchPK, '01', '11', '2020-01-01', 'ISF000004', '2020-04-20')
			INSERT INTO dbo.JobConsolTransport (JW_PK, JW_ParentType,JW_RL_NKDiscPort, JW_ParentGUID, JW_ETD, JW_ETA)
			VALUES (newID(), 'ISF', 'USCHI', '{BFPK4}', '2020-04-10', '2020-04-10')

			INSERT INTO dbo.CusISFHeader(BF_PK, BF_ActionReasonCode, BF_EntryType, BF_GB, BF_ShipmentType, BF_TransportMode, BF_SystemCreateTimeUTC, BF_JobReference, BF_FirstAcceptedDate) VALUES
			('{BFPK5}', 'CT', '1', @BranchPK, '01', '11', '2020-01-01', 'ISF000005', '2020-04-21')
			INSERT INTO dbo.JobConsolTransport (JW_PK, JW_ParentType,JW_RL_NKDiscPort, JW_ParentGUID, JW_ETD, JW_ETA)
			VALUES (newID(), 'ISF', 'USCHI', '{BFPK5}', '2020-04-10', '2020-04-10')
			";

			using (var command = TestConnection.Command(inertSQL))
			{
				command.ExecuteScalar();
			}

			Assert_numberOfDays(Guid.Empty, "ALL", 5);
			Assert_numberOfDays(BFPK1, "PTD", 1);
			Assert_numberOfDays(BFPK2, "0", 1);
			Assert_numberOfDays(BFPK3, "5", 1);
			Assert_numberOfDays(BFPK4, "10", 1);
			Assert_numberOfDays(BFPK5, ">10", 1);
		}

		void Assert_numberOfDays(Guid bFPK, string days, int count)
		{
			var reportSql = $@"SELECT * FROM csfn_ImporterSecurityFilingHeaders('1', '01', 'CT', NULL, '{days}', 1)";
			using (var command = TestConnection.Command(reportSql))
			{
				var i = 0;
				using (var reader = command.ExecuteReader())
				{
					while (reader.Read())
					{
						if (bFPK != Guid.Empty)
						{
							AssertEquals(bFPK.ToString(), reader["BF_PK"].ToString());
						}
						i++;
					}
				}
				AssertEquals(count, i);
			}
		}
	}
}
