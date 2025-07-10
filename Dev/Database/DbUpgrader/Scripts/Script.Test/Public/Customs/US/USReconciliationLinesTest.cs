using System;
using System.Data;
using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.Customs.US;
using Enterprise.Build.Database.Script.TestFramework;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Customs.US
{
	[TestedType(typeof(USReconciliationLines))]
	class USReconciliationLinesTest : DbCreateScriptTest
	{
		Guid CreateCusEntryHeader(Guid declarationPK, string messageType, int clusterKey)
		{
			var entryHeaderPK = Guid.NewGuid();
			var entryHeaderSql = @"INSERT INTO dbo.CusEntryHeader(CH_PK, CH_DataModel, CH_JE, CH_MessageType, CH_ClusterKey, CH_SystemCreateTimeUtc, CH_SystemCreateUser, CH_SystemLastEditTimeUtc, CH_SystemLastEditUser)
VALUES(@entryHeaderPK, 'US', @declarationPK, @messageType, @clusterKey, getutcdate(), '~BP', getutcdate(), '~BP')";

			using (var command = Db.Connection.Command(entryHeaderSql))
			{
				command.AddParameter("@entryHeaderPK", SqlDbType.UniqueIdentifier, entryHeaderPK);
				command.AddParameter("@declarationPK", SqlDbType.UniqueIdentifier, declarationPK);
				command.AddParameter("@messageType", SqlDbType.VarChar, messageType);
				command.AddParameter("@clusterKey", SqlDbType.Int, clusterKey);

				command.ExecuteNonQuery();
			}
			return entryHeaderPK;
		}
		Guid CreateCusEntryHeader(Guid declarationPK, string messageType, string bgmReference, string primeEntry, int clusterKey)
		{
			var entryHeaderPK = Guid.NewGuid();
			var entryHeaderSql = @"INSERT INTO dbo.CusEntryHeader(CH_PK, CH_DataModel, CH_JE, CH_MessageType, CH_BGMReference, CH_CH_PrimeEntry, CH_ClusterKey, CH_SystemCreateTimeUtc, CH_SystemCreateUser, CH_SystemLastEditTimeUtc, CH_SystemLastEditUser)
VALUES(@entryHeaderPK, 'US', @declarationPK, @messageType, @bgmReference, @primeEntry, @clusterKey, getutcdate(), '~BP', getutcdate(), '~BP')";

			using (var command = Db.Connection.Command(entryHeaderSql))
			{
				command.AddParameter("@entryHeaderPK", SqlDbType.UniqueIdentifier, entryHeaderPK);
				command.AddParameter("@declarationPK", SqlDbType.UniqueIdentifier, declarationPK);
				command.AddParameter("@messageType", SqlDbType.VarChar, messageType);
				command.AddParameter("@bgmReference", SqlDbType.VarChar, bgmReference);
				command.AddParameter("@primeEntry", SqlDbType.VarChar, primeEntry);
				command.AddParameter("@clusterKey", SqlDbType.Int, clusterKey);

				command.ExecuteNonQuery();
			}
			return entryHeaderPK;
		}

		public void TestFromDeclaration()
		{
			var companyPK = TestDataCreator.CreateCompany("TC1", "US", "USD");
			var branchPK = TestDataCreator.CreateBranch(companyPK, "TB1", "USCHI");
			var declarationPK1 = TestDataCreator.CreateJobDeclaration(branchPK, companyPK, "RECONDE1", "REC", 1);

			var cusEntryHeaderCharges1PK = Guid.NewGuid();
			var cusEntryHeaderCharges2PK = Guid.NewGuid();
			var cusEntryHeader1PK = Guid.NewGuid();
			var invoiceLinePK1 = Guid.NewGuid();
			var cusCodeData1PK = Guid.NewGuid();
			var cusCodeData2PK = Guid.NewGuid();
			var cusCodeData3PK = Guid.NewGuid();
			var cusCodeData4PK = Guid.NewGuid();
			var cusCodeFee1PK = Guid.NewGuid();
			var cusCodeFee2PK = Guid.NewGuid();
			var cusCodeFee3PK = Guid.NewGuid();
			var cusCodeFee4PK = Guid.NewGuid();

			var clusterKey = 1;

			var cusEntryHeaderEnsSQL = @"INSERT INTO dbo.CusEntryHeader (CH_PK, CH_DataModel, CH_JE, CH_MessageType, CH_ClusterKey, CH_SystemCreateTimeUtc, CH_SystemCreateUser, CH_SystemLastEditTimeUtc, CH_SystemLastEditUser)
VALUES (@cusEntryHeader1PK, 'US', @declarationPK1, 'ENS', @clusterKey, getutcdate(), '~BP', getutcdate(), '~BP')";
			using (var command = Db.Connection.Command(cusEntryHeaderEnsSQL))
			{
				command.AddParameter("@cusEntryHeader1PK", SqlDbType.UniqueIdentifier, cusEntryHeader1PK);
				command.AddParameter("@declarationPK1", SqlDbType.UniqueIdentifier, declarationPK1);
				command.AddParameter("@clusterKey", SqlDbType.Int, clusterKey);
				command.ExecuteNonQuery();
			}

			var reconEntryPK = TestDataCreator.CreateCusEntryNum(declarationPK1, "JobDeclaration", "31000977", "RCI", "CUS", "US");
			var cusEntryRCIPK = CreateCusEntryHeader(declarationPK1, "RCI", "SV971024895", cusEntryHeader1PK.ToString(), clusterKey);
			var cusEntryRECPK = CreateCusEntryHeader(declarationPK1, "REC", clusterKey);

			var cusEntryHeaderSQL =
@"INSERT INTO dbo.CusEntryHeaderCharges (C1_PK, C1_CH, C1_ChargeAmount, C1_ChargeType, C1_ClusterKey, C1_SystemCreateTimeUtc, C1_SystemCreateUser, C1_SystemLastEditTimeUtc, C1_SystemLastEditUser) VALUES
(@cusEntryHeaderCharges1PK, @cusEntryRCIPK, 508.70, '499', @clusterKey, getutcdate(), '~BP', getutcdate(), '~BP'),
(@cusEntryHeaderCharges2PK, @cusEntryRCIPK, 30.00, '501', @clusterKey, getutcdate(), '~BP', getutcdate(), '~BP');

INSERT INTO dbo.CusCodeData (CY_PK, CY_Code, CY_Data, CY_ParentID, CY_ParentTableCode, CY_TYPE, CY_IsOverridden) VALUES (@cusCodeData1PK, '499', ' 508.70', @invoiceLinePK1, 'JI', 'REC', 0)
INSERT INTO dbo.CusCodeData (CY_PK, CY_Code, CY_Data, CY_ParentID, CY_ParentTableCode, CY_TYPE, CY_IsOverridden) VALUES (@cusCodeData2PK, '501', ' 3.66', @invoiceLinePK1, 'JI', 'REC', 1)
INSERT INTO dbo.CusCodeData (CY_PK, CY_Code, CY_Data, CY_ParentID, CY_ParentTableCode, CY_TYPE, CY_IsOverridden) VALUES (@cusCodeData3PK, '053', ' 8822.00', @cusEntryRCIPK, 'CH', 'REC', 0)
INSERT INTO dbo.CusCodeData (CY_PK, CY_Code, CY_Data, CY_ParentID, CY_ParentTableCode, CY_TYPE, CY_IsOverridden) VALUES (@cusCodeFee4PK, '053', ' 8822.00', @invoiceLinePK1, 'JI', 'REC', 0)
INSERT INTO dbo.CusCodeData (CY_PK, CY_Code, CY_Data, CY_ParentID, CY_ParentTableCode, CY_TYPE, CY_IsOverridden) VALUES (@cusCodeData4PK, '017', ' 77.77', @cusEntryRCIPK, 'CH', 'REC', 0)

INSERT INTO dbo.CusCodeData (CY_PK, CY_Code, CY_Data, CY_ParentID, CY_ParentTableCode, CY_TYPE, CY_IsOverridden) VALUES (@cusCodeFee1PK, '499', ' 38.100', @invoiceLinePK1, 'JI', 'FEE', 1)
INSERT INTO dbo.CusCodeData (CY_PK, CY_Code, CY_Data, CY_ParentID, CY_ParentTableCode, CY_TYPE, CY_IsOverridden) VALUES (@cusCodeFee2PK, '501', ' 30.66', @invoiceLinePK1, 'JI', 'FEE', 0)
INSERT INTO dbo.CusCodeData (CY_PK, CY_Code, CY_Data, CY_ParentID, CY_ParentTableCode, CY_TYPE, CY_IsOverridden) VALUES (@cusCodeFee3PK, '017', ' 77.77', @invoiceLinePK1, 'JI', 'REC', 0)
";

			using (var command = Db.Connection.Command(cusEntryHeaderSQL))
			{
				command.AddParameter("@cusEntryHeader1PK", SqlDbType.UniqueIdentifier, cusEntryHeader1PK);
				command.AddParameter("@declarationPK1", SqlDbType.UniqueIdentifier, declarationPK1);
				command.AddParameter("@cusEntryHeaderCharges1PK", SqlDbType.UniqueIdentifier, cusEntryHeaderCharges1PK);
				command.AddParameter("@cusEntryHeaderCharges2PK", SqlDbType.UniqueIdentifier, cusEntryHeaderCharges2PK);
				command.AddParameter("@cusCodeData1PK", SqlDbType.UniqueIdentifier, cusCodeData1PK);
				command.AddParameter("@cusCodeData2PK", SqlDbType.UniqueIdentifier, cusCodeData2PK);
				command.AddParameter("@cusCodeData3PK", SqlDbType.UniqueIdentifier, cusCodeData3PK);
				command.AddParameter("@cusCodeData4PK", SqlDbType.UniqueIdentifier, cusCodeData4PK);
				command.AddParameter("@invoiceLinePK1", SqlDbType.UniqueIdentifier, invoiceLinePK1);
				command.AddParameter("@cusEntryRCIPK", SqlDbType.UniqueIdentifier, cusEntryRCIPK);
				command.AddParameter("@cusCodeFee1PK", SqlDbType.UniqueIdentifier, cusCodeFee1PK);
				command.AddParameter("@cusCodeFee2PK", SqlDbType.UniqueIdentifier, cusCodeFee2PK);
				command.AddParameter("@cusCodeFee3PK", SqlDbType.UniqueIdentifier, cusCodeFee3PK);
				command.AddParameter("@cusCodeFee4PK", SqlDbType.UniqueIdentifier, cusCodeFee4PK);
				command.AddParameter("@clusterKey", SqlDbType.Int, clusterKey);
				command.ExecuteNonQuery();
			}

			var invoiceHeaderPK = Guid.NewGuid();
			var recondEntryPK = @"CH_ReconEntry=" + cusEntryRCIPK + @"*InvoiceType=IN";
			var invoiceHeaderSQL = @"
INSERT INTO dbo.JobComInvoiceHeader(JZ_PK, JZ_DataModel, JZ_AddInfo, JZ_JE, JZ_ClusterKey)
VALUES (@invoiceHeaderPK, 'US', @recondEntryPK, @declarationPK1, @clusterKey)
";
			using (var command = Db.Connection.Command(invoiceHeaderSQL))
			{
				command.AddParameter("@invoiceHeaderPK", SqlDbType.UniqueIdentifier, invoiceHeaderPK);
				command.AddParameter("@recondEntryPK", SqlDbType.VarChar, recondEntryPK);
				command.AddParameter("@declarationPK1", SqlDbType.UniqueIdentifier, declarationPK1);
				command.AddParameter("@clusterKey", SqlDbType.Int, clusterKey);

				command.ExecuteNonQuery();
			}

			var invoiceLineSql = @"
INSERT INTO dbo.JobComInvoiceLine(JI_PK, JI_DataModel, JI_JZ, JI_LineNo, JI_LinePrice, JI_AddInfo, JI_ClusterKey)
VALUES (@invoiceLinePK1, 'US', @invoiceHeaderPK1, 1, 1.00, '98GoodsValue=1*Duty=333333*HasMPF=Y*OverrideDuty=Y*R_Orig98Value=1*R_OrigCV=100000*R_OrigDuty=333333*R_OrigEntryLineNo=1*R_OrigFirstUQ=X*R_OrigHasMPF=Y*R_OrigOverrideDuty=Y*R_OrigTariff=8466931100*UC_NKCountryOfOrigin=CH',
	@clusterKey)
";
			using (var command = Db.Connection.Command(invoiceLineSql))
			{
				command.AddParameter("@invoiceLinePK1", SqlDbType.UniqueIdentifier, invoiceLinePK1);
				command.AddParameter("@invoiceHeaderPK1", SqlDbType.UniqueIdentifier, invoiceHeaderPK);
				command.AddParameter("@clusterKey", SqlDbType.Int, clusterKey);

				command.ExecuteNonQuery();
			}

			var reportSql = @"SELECT JE_DeclarationReference, OriginalTariff, OrigMPFAmount, ReconMPFAmount, OrigHMFAmount, ReconHMFAmount
, OriginalOtherFeeCode, OrigOtherFeeAmount, OverrideOriginMPF, OverrideReconMPF 
FROM USReconciliationLines(@companyPK)";
			using (var command = Db.Connection.Command(reportSql))
			{
				command.AddParameter("@companyPK", SqlDbType.UniqueIdentifier, companyPK);
				using (var reader = command.ExecuteReader())
				{
					reader.Read();
					CombineAssertions(() =>
					{
						AssertEquals("OriginalTariff", "8466931100", reader["OriginalTariff"].ToString());
						AssertEquals("OrigMPFAmount", "508.7000", reader["OrigMPFAmount"].ToString());
						AssertEquals("ReconMPFAmount", "38.1000", reader["ReconMPFAmount"].ToString());
						AssertEquals("OrigHMFAmount", "3.6600", reader["OrigHMFAmount"].ToString());
						AssertEquals("ReconHMFAmount", "30.6600", reader["ReconHMFAmount"].ToString());
						AssertEquals("OriginalOtherFeeCode", "053", reader["OriginalOtherFeeCode"].ToString());
						AssertEquals("OrigOtherFeeAmount", "8822.0000", reader["OrigOtherFeeAmount"].ToString());
						AssertEquals("OverrideOriginMPF", "N", reader["OverrideOriginMPF"].ToString());
						AssertEquals("OverrideReconMPF", "Y", reader["OverrideReconMPF"].ToString());
					});
				}
			}
		}
	}
}

