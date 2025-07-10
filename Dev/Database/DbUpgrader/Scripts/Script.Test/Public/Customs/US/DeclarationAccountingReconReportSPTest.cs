using System;
using System.Data;
using System.Text;
using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.Customs.US;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Customs.US.Testing
{
	[TestedType(typeof(DeclarationAccountingReconReportSP))]
	class USEntriesAccountingReconReportSPTest : DbCreateScriptTest
	{
		readonly Guid accChargeCodePK1 = Guid.NewGuid();
		readonly Guid accChargeCodePK2 = Guid.NewGuid();
		readonly Guid companyPK1 = Guid.NewGuid();
		readonly Guid companyPK2 = Guid.NewGuid();

		public void TestOnlyDisplayDataForCurrentCompany()
		{
			PrepareTestData();
			var currentCompany = companyPK1;
			var uSCustomsDisbursementChargeCodes = accChargeCodePK1.ToString() + accChargeCodePK2.ToString();
			DataTable result = DataUtils.GetDataTableFromQuery(TestConnection, string.Format("EXEC DeclarationAccountingReconReportSP '{0}', NULL, '', NULL, '', '', '', '', '', '', '', 'LWD', '{1}'", currentCompany, uSCustomsDisbursementChargeCodes));

			AssertEquals("Result should have rows", 1, result.Rows.Count);
		}

		public void TestQueryAllWithNullDateTime()
		{
			PrepareTestData();
			var currentCompany = companyPK1;
			var uSCustomsDisbursementChargeCodes = accChargeCodePK1.ToString() + accChargeCodePK2.ToString();
			DataTable result = DataUtils.GetDataTableFromQuery(TestConnection, string.Format("EXEC DeclarationAccountingReconReportSP '{0}', NULL, '', NULL, NULL, NULL, NULL, NULL, NULL, NULL, '', 'LWD', '{1}'", currentCompany, uSCustomsDisbursementChargeCodes));

			AssertEquals("Result should have rows", 1, result.Rows.Count);
		}

		public void PrepareTestData()
		{
			Guid branchPK1 = Guid.NewGuid();
			Guid branchPK2 = Guid.NewGuid();
			Guid departmentPK1 = Guid.NewGuid();
			Guid departmentPK2 = Guid.NewGuid();
			Guid declarationPK1 = Guid.NewGuid();
			Guid declarationPK2 = Guid.NewGuid();
			Guid cusEntryHeaderPK1 = Guid.NewGuid();
			Guid cusEntryHeaderPK2 = Guid.NewGuid();
			Guid cusEntryNumberPK1 = Guid.NewGuid();
			Guid cusEntryNumberPK2 = Guid.NewGuid();
			Guid genAddOnColumnPK1 = Guid.NewGuid();
			Guid genAddOnColumnPK2 = Guid.NewGuid();
			Guid cusStatementHeaderPK1 = Guid.NewGuid();
			Guid cusStatementHeaderPK2 = Guid.NewGuid();
			Guid cusStatementLinePK1 = Guid.NewGuid();
			Guid cusStatementLinePK2 = Guid.NewGuid();
			Guid cusEntryPayInfoPK1 = Guid.NewGuid();
			Guid cusEntryPayInfoPK2 = Guid.NewGuid();
			Guid jobHeaderPK1 = Guid.NewGuid();
			Guid jobHeaderPK2 = Guid.NewGuid();
			Guid accTransactionHeaderPK1 = Guid.NewGuid();
			Guid accTransactionHeaderPK2 = Guid.NewGuid();
			Guid accTransactionLinesPK1 = Guid.NewGuid();
			Guid accTransactionLinesPK2 = Guid.NewGuid();
			Guid jobChargePK1 = Guid.NewGuid();
			Guid jobChargePK2 = Guid.NewGuid();

			var sqlTextBuilder = new StringBuilder($@"
INSERT INTO dbo.GlbCompany(GC_PK, GC_RN_NKCountryCode, GC_RX_NKLocalCurrency, GC_Code, GC_Name) VALUES('{companyPK1}', 'US', 'USD', 'DUS', 'US company1')
INSERT INTO dbo.GlbCompany(GC_PK, GC_RN_NKCountryCode, GC_RX_NKLocalCurrency, GC_Code, GC_Name) VALUES('{companyPK2}', 'US', 'USD', 'PHX', 'US company2')

INSERT INTO dbo.GlbBranch(GB_PK, GB_GC, GB_Code) VALUES('{branchPK1}', '{companyPK1}', 'BR1')
INSERT INTO dbo.GlbBranch(GB_PK, GB_GC, GB_Code) VALUES('{branchPK2}', '{companyPK2}', 'BR2')

INSERT INTO dbo.GlbDepartment (GE_PK, GE_Code) VALUES ('{departmentPK1}', 'GE1')
INSERT INTO dbo.GlbDepartment (GE_PK, GE_Code) VALUES ('{departmentPK2}', 'GE2')

INSERT INTO dbo.JobDeclaration (JE_PK, JE_DataModel, JE_DeclarationReference, JE_GB, JE_GC, JE_ClusterKey, JE_AddInfo, JE_PaymentMethod) VALUES ('{declarationPK1}', 'US', 'B0001', '{branchPK1}', '{companyPK1}', 1, 'PaymentType=3', 'BRK')
INSERT INTO dbo.JobDeclaration (JE_PK, JE_DataModel, JE_DeclarationReference, JE_GB, JE_GC, JE_ClusterKey, JE_AddInfo, JE_PaymentMethod) VALUES ('{declarationPK2}', 'US', 'B0002', '{branchPK2}', '{companyPK2}', 2, 'PaymentType=3', 'BRK')

INSERT INTO dbo.CusEntryHeader (CH_PK, CH_DataModel, CH_JE, CH_MessageType, CH_Status, CH_ClusterKey, CH_SystemCreateTimeUtc, CH_SystemCreateUser, CH_SystemLastEditTimeUtc, CH_SystemLastEditUser) VALUES
('{cusEntryHeaderPK1}', 'US', '{declarationPK1}', 'ENS', 'DSC', 1, getutcdate(), '~BP', getutcdate(), '~BP'),
('{cusEntryHeaderPK2}', 'US', '{declarationPK2}', 'ENS', 'DSC', 2, getutcdate(), '~BP', getutcdate(), '~BP');

INSERT INTO dbo.CusEntryNum (CE_PK, CE_ParentID, CE_ParentTable, CE_EntryType, CE_Category, CE_EntryNum, CE_SystemCreateTimeUtc, CE_SystemCreateUser, CE_SystemLastEditTimeUtc, CE_SystemLastEditUser) VALUES
('{cusEntryNumberPK1}', '{declarationPK1}', 'CusEntryHeader', 'ENS', 'CUS', '111', getutcdate(), '~BP', getutcdate(), '~BP'),
('{cusEntryNumberPK2}', '{declarationPK2}', 'CusEntryHeader', 'ENS', 'CUS', '111', getutcdate(), '~BP', getutcdate(), '~BP')

INSERT INTO dbo.GenAddOnColumn (XA_PK, XA_Name, XA_Type, XA_Data, XA_ParentTableCode, XA_ParentID) VALUES ('{genAddOnColumnPK1}', 'US_EntryFilerCode', 'STR', 'XJ5', 'JE', '{declarationPK1}')
INSERT INTO dbo.GenAddOnColumn (XA_PK, XA_Name, XA_Type, XA_Data, XA_ParentTableCode, XA_ParentID) VALUES ('{genAddOnColumnPK2}', 'US_EntryFilerCode', 'STR', 'XJ5', 'JE', '{declarationPK2}')

INSERT INTO dbo.CusStatementHeader (B2_PK, B2_GC, B2_B2_PeriodicStatement, B2_IsMonthlyStatement, B2_EntryFilerCode) VALUES ('{cusStatementHeaderPK1}', '{companyPK1}', NULL, 1, 'SV9')
INSERT INTO dbo.CusStatementHeader (B2_PK, B2_GC, B2_B2_PeriodicStatement, B2_IsMonthlyStatement, B2_EntryFilerCode) VALUES ('{cusStatementHeaderPK2}', '{companyPK2}', NULL, 1, 'SV7')

INSERT INTO dbo.CusStatementLine (B3_PK, B3_B2, B3_EntryNum, B3_EntryFilerCode, B3_CustomsFeesTotal) VALUES ('{cusStatementLinePK1}', '{cusStatementHeaderPK1}', 'EntryNum1', 'SV9', '20')
INSERT INTO dbo.CusStatementLine (B3_PK, B3_B2, B3_EntryNum, B3_EntryFilerCode, B3_CustomsFeesTotal) VALUES ('{cusStatementLinePK2}', '{cusStatementHeaderPK2}', 'EntryNum2', 'SV7', '20')

INSERT INTO dbo.CusEntryPayInfo(C9_PK, C9_CH, C9_PaymentDate, C9_PaymentAmount, C9_TransactionType, C9_PaymentParty, C9_ClusterKey, C9_SystemCreateTimeUtc, C9_SystemCreateUser, C9_SystemLastEditTimeUtc, C9_SystemLastEditUser)
values('{cusEntryPayInfoPK1}', '{cusEntryHeaderPK1}', '2016-12-01', 55, 'OTH', 'C', 1, getutcdate(), '~BP', getutcdate(), '~BP'), ('{cusEntryPayInfoPK2}', '{cusEntryHeaderPK2}', '2016-12-01', 55, 'OTH', 'C', 2, getutcdate(), '~BP', getutcdate(), '~BP')

INSERT INTO dbo.JobHeader (JH_PK, JH_ParentID, JH_ParentTableCode, JH_JobNum, JH_GC, JH_GB, JH_GE, JH_Status, JH_OA_LocalChargesAddr, JH_OA_AgentCollectAddr)
VALUES ('{jobHeaderPK1}', NEWID(), 'JS', '000000001', '{companyPK1}', '{branchPK1}', '{departmentPK1}', 'WRK', NULL, NULL)
INSERT INTO dbo.JobHeader (JH_PK, JH_ParentID, JH_ParentTableCode, JH_JobNum, JH_GC, JH_GB, JH_GE, JH_Status, JH_OA_LocalChargesAddr, JH_OA_AgentCollectAddr)
VALUES ('{jobHeaderPK2}', NEWID(), 'JS', '000000002', '{companyPK2}', '{branchPK2}', '{departmentPK2}', 'WRK', NULL, NULL)

INSERT INTO dbo.AccTransactionHeader (AH_PK, AH_Ledger, AH_TransactionType, AH_GC, AH_GB, AH_GE, AH_InvoiceDate, AH_PostDate) VALUES ('{accTransactionHeaderPK1}', 'AP', 'INV', '{companyPK1}', '{branchPK1}', '{departmentPK1}', '2017-2-2', '2017-2-2')
INSERT INTO dbo.AccTransactionHeader (AH_PK, AH_Ledger, AH_TransactionType, AH_GC, AH_GB, AH_GE, AH_InvoiceDate, AH_PostDate) VALUES ('{accTransactionHeaderPK2}', 'AP', 'INV', '{companyPK2}', '{branchPK2}', '{departmentPK2}', '2017-2-2', '2017-2-2')

INSERT INTO dbo.Acctransactionlines (AL_PK, AL_AH, AL_GB, AL_GE, AL_ReverseDate, AL_AG, AL_GC, AL_LineType)
VALUES ('{accTransactionLinesPK1}', '{accTransactionHeaderPK1}', '{branchPK1}', '{departmentPK1}' , '2015-04-24 14:04:00', NULL, '{companyPK1}', 'CST')
INSERT INTO dbo.Acctransactionlines (AL_PK, AL_AH, AL_GB, AL_GE, AL_ReverseDate, AL_AG, AL_GC, AL_LineType)
VALUES ('{accTransactionLinesPK2}', '{accTransactionHeaderPK2}', '{branchPK2}', '{departmentPK2}' , '2015-04-24 14:04:00', NULL, '{companyPK2}', 'CST')

INSERT INTO dbo.AccChargeCode (AC_PK, AC_Code, AC_ChargeGroup, AC_GC) VALUES ('{accChargeCodePK1}', 'XXX', 'FRT', '{companyPK1}')
INSERT INTO dbo.AccChargeCode (AC_PK, AC_Code, AC_ChargeGroup, AC_GC) VALUES ('{accChargeCodePK2}', 'XXX', 'FRT', '{companyPK2}')

INSERT INTO dbo.JobCharge (JR_PK, JR_JH, JR_AL_APLine, JR_AC, JR_GB, JR_GC, JR_GE, JR_LocalCostAmt, JR_SystemLastEditTimeUtc, JR_SystemLastEditUser) VALUES ('{jobChargePK1}', '{jobHeaderPK1}', '{accTransactionLinesPK1}', '{accChargeCodePK1}', '{branchPK1}', '{companyPK1}', '{departmentPK1}', '50', GETUTCDATE(), '~BP')
INSERT INTO dbo.JobCharge (JR_PK, JR_JH, JR_AL_APLine, JR_AC, JR_GB, JR_GC, JR_GE, JR_LocalCostAmt, JR_SystemLastEditTimeUtc, JR_SystemLastEditUser) VALUES ('{jobChargePK2}', '{jobHeaderPK2}', '{accTransactionLinesPK2}', '{accChargeCodePK2}', '{branchPK2}', '{companyPK2}', '{departmentPK2}', '50', GETUTCDATE(), '~BP')

");
			using (var command = Db.Connection.Command(sqlTextBuilder.ToString()))
			{
				command.ExecuteNonQuery();
			}
		}
	}
}

