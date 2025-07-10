using System;
using System.Data;
using CargoWise.Data;
using CargoWise.Types;
using Enterprise.Environment;

namespace Enterprise.Customs.ES.Business.Testing
{
	public static class ProcessorTestHelper
	{
		public static ZGuid CreateCusExitReport(int clusterKey = 1, string referenceNum = "E00000001", string mrn = "AAA", string brokerCode = "", string certName = "")
		{
			var pkHeader = Guid.NewGuid();
			var sqlHeader = @"
INSERT INTO dbo.CusExitHeader (CXH_PK, CXH_ApplicationCode, CXH_AutoVersion, CXH_ClusterKey, CXH_IsValid, CXH_JobReference, CXH_SystemCreateTimeUtc, CXH_SystemCreateUser, CXH_SystemLastEditTimeUtc, CXH_SystemLastEditUser, CXH_GB_Branch, CXH_GC_Company, CXH_GS_NKCustomsAgent, CXH_CustomsProfile)
VALUES
(@CXH_PK, 'XIT', 1, @CXH_ClusterKey, 1, @CXH_JobReference, GETUTCDATE(), '~BP', GETUTCDATE(), '~BP', @CXH_GB_Branch, @CXH_GC_Company, @CXH_GS_NKCustomsAgent, @CXH_CustomsProfile)
";
			using (DbCommand command = Db.Connection.Command(sqlHeader))
			{
				command.AddParameter("@CXH_PK", SqlDbType.UniqueIdentifier, pkHeader);
				command.AddParameter("@CXH_GB_Branch", SqlDbType.UniqueIdentifier, Env.CurrentBranchPK);
				command.AddParameter("@CXH_GC_Company", SqlDbType.UniqueIdentifier, Env.CurrentCompanyPK);
				command.AddParameter("@CXH_GS_NKCustomsAgent", SqlDbType.VarChar, brokerCode);
				command.AddParameter("@CXH_ClusterKey", SqlDbType.Int, clusterKey);
				command.AddParameter("@CXH_JobReference", SqlDbType.VarChar, referenceNum);
				command.AddParameter("@CXH_CustomsProfile", SqlDbType.VarChar, certName);
				command.ExecuteNonQuery();
			}

			var pkConsignment = Guid.NewGuid();
			var sqlConsginment = @"
INSERT INTO dbo.CusExitConsignment (CXC_PK, CXC_AutoVersion, CXC_ClusterKey, CXC_IsValid, CXC_CXH_Header, CXC_MovementReference, CXC_SystemCreateTimeUtc, CXC_SystemCreateUser, CXC_SystemLastEditTimeUtc, CXC_SystemLastEditUser)
	VALUES (@CXC_PK, 1, @CXC_ClusterKey, 1, @CXC_CXH_Header, @CXC_MovementReference, GETUTCDATE(), '~BP', GETUTCDATE(), '~BP')
";
			using (DbCommand command = Db.Connection.Command(sqlConsginment))
			{
				command.AddParameter("@CXC_PK", SqlDbType.UniqueIdentifier, pkConsignment);
				command.AddParameter("@CXC_CXH_Header", SqlDbType.UniqueIdentifier, pkHeader);
				command.AddParameter("@CXC_MovementReference", SqlDbType.VarChar, mrn);
				command.AddParameter("@CXC_ClusterKey", SqlDbType.Int, clusterKey);
				command.ExecuteNonQuery();
			}

			var pkReport = Guid.NewGuid();
			var sqlReport = @"
INSERT INTO dbo.CusExitReport (CER_PK, CER_ClusterKey, CER_IsValid, CER_CXH_Header, CER_Type, CER_OfficeOfExit, CER_SystemCreateTimeUtc, CER_SystemCreateUser, CER_SystemLastEditTimeUtc, CER_SystemLastEditUser, CER_Behavior, CER_CXC_Consignment)
	VALUES (@CER_PK, @CER_ClusterKey, 1, @CER_CXH_Header, 'PRE', 'IEDUB100', GETUTCDATE(), '~BP', GETUTCDATE(), '~BP', 'DIS', @CER_CXC_Consignment)
";
			using (DbCommand command = Db.Connection.Command(sqlReport))
			{
				command.AddParameter("@CER_PK", SqlDbType.UniqueIdentifier, pkReport);
				command.AddParameter("@CER_CXH_Header", SqlDbType.UniqueIdentifier, pkHeader);
				command.AddParameter("@CER_CXC_Consignment", SqlDbType.UniqueIdentifier, pkConsignment);
				command.AddParameter("@CER_ClusterKey", SqlDbType.Int, clusterKey);
				command.ExecuteNonQuery();
			}
			return (ZGuid)pkReport;
		}

		public static ZGuid CreateNctsHeader(string headerType = "D", string referenceNum = "NCD00000001", string brokerCode = "", string certName = "", string phase = "", string mrn = "")
		{
			var pkHeader = Guid.NewGuid();
			var sqlHeader = @"
INSERT INTO dbo.CusInBondHeader (BH_PK, BH_IsValid, BH_HeaderType, BH_JobReference, BH_SystemCreateTimeUtc, BH_SystemCreateUser, BH_SystemLastEditTimeUtc, BH_SystemLastEditUser, BH_GB ,BH_ApplicationCode, BH_CustomsProfile)
VALUES
(@BH_PK, 1, @BH_HeaderType, @BH_JobReference, GETUTCDATE(), '~BP', GETUTCDATE(), '~BP', @BH_GB, 'NCT', @BH_CustomsProfile)
";

			using (DbCommand command = Db.Connection.Command(sqlHeader))
			{
				command.AddParameter("@BH_PK", SqlDbType.UniqueIdentifier, pkHeader);
				command.AddParameter("@BH_GB", SqlDbType.UniqueIdentifier, Env.CurrentBranchPK);
				command.AddParameter("@BH_HeaderType", SqlDbType.VarChar, headerType);
				command.AddParameter("@BH_JobReference", SqlDbType.VarChar, referenceNum);
				command.AddParameter("@BH_CustomsProfile", SqlDbType.VarChar, certName);
				command.ExecuteNonQuery();
			}

			var pkMovement = Guid.NewGuid();
			var sqlConsginment = @"
INSERT INTO dbo.CusInBondMoveHeader (BM_PK, BM_SubApplicationCode, BM_BH, BM_SystemCreateTimeUtc, BM_SystemCreateUser, BM_SystemLastEditTimeUtc, BM_SystemLastEditUser, BM_GS_NKCusAgent, BM_Phase)
	VALUES (@BM_PK, @BM_SubApplicationCode, @BM_BH, GETUTCDATE(), '~BP', GETUTCDATE(), '~BP', @BM_GS_NKCusAgent, @BM_Phase)
";
			using (DbCommand command = Db.Connection.Command(sqlConsginment))
			{
				command.AddParameter("@BM_PK", SqlDbType.UniqueIdentifier, pkMovement);
				command.AddParameter("@BM_BH", SqlDbType.UniqueIdentifier, pkHeader);
				command.AddParameter("@BM_SubApplicationCode", SqlDbType.VarChar, headerType);
				command.AddParameter("@BM_GS_NKCusAgent", SqlDbType.VarChar, brokerCode);
				command.AddParameter("@BM_Phase", SqlDbType.VarChar, phase);
				command.ExecuteNonQuery();
			}

			if (!string.IsNullOrWhiteSpace(mrn))
			{
				var sqlMRN = @"
INSERT INTO dbo.CusEntryNum (CE_PK, CE_ParentID, CE_ParentTable, CE_EntryNum, CE_EntryType, CE_Category, CE_RN_NKCountryCode, CE_IsValid, CE_SystemCreateTimeUtc, CE_SystemCreateUser, CE_SystemLastEditTimeUtc, CE_SystemLastEditUser)
	VALUES (NEWID(), @CE_ParentID, 'CusInBondHeader', @CE_EntryNum, 'MRN', 'OTH', 'ES', 1, GETUTCDATE(), '~BP', GETUTCDATE(), '~BP')
";
				using (DbCommand command = Db.Connection.Command(sqlMRN))
				{
					command.AddParameter("@CE_ParentID", SqlDbType.UniqueIdentifier, pkHeader);
					command.AddParameter("@CE_EntryNum", SqlDbType.VarChar, mrn);
					command.ExecuteNonQuery();
				}
			}

			return (ZGuid)pkHeader;
		}

		public static ZGuid CreateAsycudaManifestHeader(int clusterKey = 1, string jobReference = "H7D0000001", string brokerCode = "", string certName = "")
		{
			var pkHeader = Guid.NewGuid();
			var sqlHeader = @"
INSERT INTO dbo.AsycudaManifestHeader (AMA_PK, AMA_ClusterKey, AMA_IsValid, AMA_ManifestType, AMA_RN_NKCountry, AMA_JobReference, AMA_GS_NKCustomsAgent, AMA_SystemCreateTimeUtc, AMA_SystemCreateUser, AMA_SystemLastEditTimeUtc, AMA_SystemLastEditUser, AMA_GB, AMA_ApplicationCode, AMA_CustomsProfile)
	VALUES (@AMA_PK, @AMA_ClusterKey, 1, 'EH7', 'ES', @AMA_JobReference, @AMA_GS_NKCustomsAgent, GETUTCDATE(), '~BP', GETUTCDATE(), '~BP', @AMA_GB, 'LVC', @AMA_CustomsProfile)
";

			using (var command = Db.Connection.Command(sqlHeader))
			{
				command.AddParameter("@AMA_PK", SqlDbType.UniqueIdentifier, pkHeader);
				command.AddParameter("@AMA_ClusterKey", SqlDbType.Int, clusterKey);
				command.AddParameter("@AMA_JobReference", SqlDbType.VarChar, jobReference);
				command.AddParameter("@AMA_GB", SqlDbType.UniqueIdentifier, Env.CurrentBranchPK);
				command.AddParameter("@AMA_GS_NKCustomsAgent", SqlDbType.VarChar, brokerCode);
				command.AddParameter("@AMA_CustomsProfile", SqlDbType.VarChar, certName);
				command.ExecuteNonQuery();
			}

			return (ZGuid)pkHeader;
		}

		public static ZGuid CreateAsycudaBill(int clusterKey = 1, string referenceNum = "ABL000001", string brokerCode = "", string certName = "")
		{
			var pkHeader = Guid.NewGuid();
			var sqlHeader = @"
INSERT INTO dbo.AsycudaManifestHeader (AMA_PK, AMA_ClusterKey, AMA_IsValid, AMA_ManifestType, AMA_RN_NKCountry, AMA_JobReference, AMA_GS_NKCustomsAgent, AMA_SystemCreateTimeUtc, AMA_SystemCreateUser, AMA_SystemLastEditTimeUtc, AMA_SystemLastEditUser, AMA_GB, AMA_ApplicationCode, AMA_CustomsProfile)
	VALUES (@AMA_PK, @AMA_ClusterKey, 1, 'EH7', 'ES', 'H7D0000001', @AMA_GS_NKCustomsAgent, GETUTCDATE(), '~BP', GETUTCDATE(), '~BP', @AMA_GB, 'LVC', @AMA_CustomsProfile)
";

			using (var command = Db.Connection.Command(sqlHeader))
			{
				command.AddParameter("@AMA_PK", SqlDbType.UniqueIdentifier, pkHeader);
				command.AddParameter("@AMA_ClusterKey", SqlDbType.Int, clusterKey);
				command.AddParameter("@AMA_GB", SqlDbType.UniqueIdentifier, Env.CurrentBranchPK);
				command.AddParameter("@AMA_GS_NKCustomsAgent", SqlDbType.VarChar, brokerCode);
				command.AddParameter("@AMA_CustomsProfile", SqlDbType.VarChar, certName);
				command.ExecuteNonQuery();
			}

			var pkBill = Guid.NewGuid();
			var sqlConsginment = @"
INSERT INTO dbo.AsycudaBill (ABL_PK, ABL_ClusterKey, ABL_AMA, ABL_BillNumber, ABL_SystemCreateTimeUtc, ABL_SystemCreateUser, ABL_SystemLastEditTimeUtc, ABL_SystemLastEditUser)
	VALUES (@ABL_PK, @ABL_ClusterKey, @ABL_AMA, @ABL_BillNumber, GETUTCDATE(), '~BP', GETUTCDATE(), '~BP')
";
			using (var command = Db.Connection.Command(sqlConsginment))
			{
				command.AddParameter("@ABL_PK", SqlDbType.UniqueIdentifier, pkBill);
				command.AddParameter("@ABL_AMA", SqlDbType.UniqueIdentifier, pkHeader);
				command.AddParameter("@ABL_BillNumber", SqlDbType.VarChar, referenceNum);
				command.AddParameter("@ABL_ClusterKey", SqlDbType.Int, clusterKey);
				command.ExecuteNonQuery();
			}

			return (ZGuid)pkBill;
		}
	}
}
