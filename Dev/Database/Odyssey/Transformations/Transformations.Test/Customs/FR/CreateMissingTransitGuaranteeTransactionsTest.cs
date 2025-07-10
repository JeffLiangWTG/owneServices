using System;
using System.Data;
using CargoWise.Data;
using Enterprise.Build.Database.Script.TestFramework;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.DbUpgrader.Transformation.DataModification.Testing;
using Enterprise.DbUpgrader.Transformations.Transforms.Customs.FR;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Transformations.Test.Customs.FR
{
	[TestedType(typeof(CreateMissingTransitGuaranteeTransactions))]
	public class CreateMissingTransitGuaranteeTransactionsTest : DataTransformationTestCase
	{
		Guid entryHeaderPK = Guid.NewGuid();
		Guid jobDeclarationPK = Guid.NewGuid();
		Guid cusInBondHeaderPK = Guid.NewGuid();
		Guid cusInBondHeaderPK2 = Guid.NewGuid();
		Guid cusInBondHeaderPK3 = Guid.NewGuid();
		Guid cusInBondMoveHeaderPK = Guid.NewGuid();
		Guid cusInBondMoveHeaderPK2 = Guid.NewGuid();
		Guid cusInBondMoveHeaderPK3 = Guid.NewGuid();
		Guid cusBondDetailPK1 = Guid.NewGuid();
		Guid cusBondDetailPK2 = Guid.NewGuid();
		Guid cusBondDetailPK3 = Guid.NewGuid();
		Guid cusBondDetailPK4 = Guid.NewGuid();
		Guid cusPermitHeaderPK = Guid.NewGuid();
		Guid cusPermitHeaderPK2 = Guid.NewGuid();
		Guid cusPermitHeaderPK21 = Guid.NewGuid();
		Guid cusPermitHeaderPK22 = Guid.NewGuid();
		Guid cusPermitHeaderPK3 = Guid.NewGuid();
		Guid cusPermitHeaderPK4 = Guid.NewGuid();
		Guid guaranteeHolderPK = Guid.NewGuid();
		Guid addressPK = Guid.NewGuid();
		decimal bondAmountForMissingTransaction = 222m;
		decimal bondAmountForMissingTransaction2 = 345m;
		string ediMessageNum = "111";
		string paperlessInbondNum = "123456";
		string paperlessInbondNum2 = "234567";
		string paperlessInbondNum3 = "345678";
		DateTime transactionDate = new DateTime(2025, 01, 01, 17, 44, 00);
		DateTime transactionDate2 = new DateTime(2025, 01, 02, 17, 44, 00);
		string CPH_Number1 = "G1";
		string CPH_Number2 = "G2";
		string CPH_Number3 = "G3";

		protected override DataTransformation GetNewTestTransformationInstance() => new CreateMissingTransitGuaranteeTransactions();

		public override string[] expectedIndex => new string[]
		{
			"NONCLUSTERED INDEX [_WTG__Create missing transit guarantee transactions._1] ON [dbo].[EDIMessage] ([EM_LinkTable], [EM_ApplicationCode], [EM_ReceiveTransmit], [EM_Status], [EM_MessageType], [EM_MessageSubType]) INCLUDE ([EM_LinkUniqueID], [EM_MessageNum]) WHERE ([EM_LinkTable]='CusInBondHeader' AND [EM_ApplicationCode]='FRC' AND [EM_ReceiveTransmit]='RCV' AND [EM_Status]='PRS' AND [EM_MessageType]='TP5' AND [EM_MessageSubType]='029') WITH (ALLOW_PAGE_LOCKS = OFF, ONLINE = ON)",
			"NONCLUSTERED INDEX [_WTG__Create missing transit guarantee transactions._2] ON [dbo].[CusPermitHeader] ([CPH_PK], [CPH_StartDate]) INCLUDE ([CPH_Number], [CPH_OH_PermitHolder], [CPH_RN_NKCountryCode], [CPH_SubType]) WHERE ([CPH_ApplicationCode]='GUA' AND [CPH_RN_NKCountryCode]='FR' AND [CPH_Type]='COD') WITH (ALLOW_PAGE_LOCKS = OFF, ONLINE = ON)",
			"NONCLUSTERED INDEX [_WTG__Create missing transit guarantee transactions._3] ON [dbo].[CusBondDetail] ([PW_BondNumber], [PW_BondType], [PW_ParentID]) INCLUDE ([PW_ApplicationCode], [PW_BondAmount]) WITH (ALLOW_PAGE_LOCKS = OFF, ONLINE = ON)",
			"NONCLUSTERED INDEX [_WTG__Create missing transit guarantee transactions._4] ON [dbo].[CusInBondHeader] ([BH_PK]) INCLUDE ([BH_ApplicationCode], [BH_GB]) WITH (ALLOW_PAGE_LOCKS = OFF, ONLINE = ON)",
			"NONCLUSTERED INDEX [_WTG__Create missing transit guarantee transactions._5] ON [dbo].[CusInBondMoveHeader] ([BM_BH], [BM_CustomsStatus]) INCLUDE ([BM_PaperlessInbondNum]) WITH (ALLOW_PAGE_LOCKS = OFF, ONLINE = ON)"
		};

		protected override void AssertTransformationResults()
		{
			CombineAssertions(@"Created missing transit guarantee transactions should match the following conditions:
                CPL_CPH_PermitHeader should be same as the PK of the permitheader with missing transaction. 
                CPL_AppId should equal EM_MessageNum of EDIMessage granting the REL status. 
                Bond amount should be negative of the bond amount of the guarantee with missing transaction. 
                Comment should equal BM_PaperlessInbondNum of the CusInBondMoveHeader with status REL. 
                Transaction date should equal CE_IssueDate of the CusEntryNum with EntryType MRN. 
                Transaction type should be TRA. 
                Reference should equal BM_PaperlessInbondNum of the CusInBondMoveHeader. 
                Transaction status should be CON.",
			() =>
			{
				AssertEquals(@cusPermitHeaderPK21, TestConnection.ExecuteScalar($"SELECT CPL_CPH_PermitHeader FROM CusPermitLineTransaction WHERE CPL_CPH_PermitHeader = '{@cusPermitHeaderPK21}'"));
				AssertEquals(ediMessageNum, TestConnection.ExecuteScalar($"SELECT CPL_AppId FROM CusPermitLineTransaction WHERE CPL_CPH_PermitHeader = '{@cusPermitHeaderPK21}'"));
				AssertEquals(-bondAmountForMissingTransaction, TestConnection.ExecuteScalar($"SELECT CPL_TranValue FROM CusPermitLineTransaction WHERE CPL_CPH_PermitHeader = '{@cusPermitHeaderPK21}'"));
				AssertEquals("NCTS Departure " + paperlessInbondNum2, TestConnection.ExecuteScalar($"SELECT CPL_Comment FROM CusPermitLineTransaction WHERE CPL_CPH_PermitHeader = '{@cusPermitHeaderPK21}'"));
				AssertEquals(transactionDate2, TestConnection.ExecuteScalar($"SELECT CPL_TransactionDate FROM CusPermitLineTransaction WHERE CPL_CPH_PermitHeader = '{@cusPermitHeaderPK21}'"));
				AssertEquals("TRA", TestConnection.ExecuteScalar($"SELECT CPL_TransactionType FROM CusPermitLineTransaction WHERE CPL_CPH_PermitHeader = '{@cusPermitHeaderPK21}'"));
				AssertEquals(paperlessInbondNum2, TestConnection.ExecuteScalar($"SELECT CPL_Reference FROM CusPermitLineTransaction WHERE CPL_CPH_PermitHeader = '{@cusPermitHeaderPK21}'"));
				AssertEquals("CON", TestConnection.ExecuteScalar($"SELECT CPL_TransactionStatus FROM CusPermitLineTransaction WHERE CPL_CPH_PermitHeader = '{@cusPermitHeaderPK21}'"));
			});
		}

		protected override void PrepareTestData()
		{
			var companyPK = TestDataCreator.CreateCompany("DFR", "FR", "EUR");
			var branchPK = TestDataCreator.CreateBranch(companyPK, "TB1", "FRANCE", "FR");
			var departmentPK = TestDataCreator.CreateDepartment("DEP1");

			var sqlText = @"
INSERT INTO OrgHeader
	(OH_PK, OH_IsValid, OH_Code, OH_FullName, OH_RL_NKClosestPort, OH_Language, OH_SystemCreateTimeUtc, OH_SystemCreateUser, OH_SystemLastEditTimeUtc, OH_SystemLastEditUser)
VALUES
	(@guaranteeHolderPK, 1, 'HOLDER', 'Guarantee Holder Name', 'FRPAR', 'FR', '2025-01-01', 'AFU', GETUTCDATE(), 'AFU');

INSERT INTO OrgAddress 
	(OA_PK, OA_OH, OA_City, OA_Address1,OA_SystemCreateTimeUtc, OA_SystemCreateUser, OA_SystemLastEditTimeUtc, OA_SystemLastEditUser)
VALUES 
	(@addressPk, @guaranteeHolderPK, 'PAR', 'Address1', '2025-01-01', 'AFU', GETUTCDATE(), 'AFU')

INSERT INTO CusInBondHeader
	(BH_PK, BH_GB, BH_JobReference, BH_ApplicationCode, BH_HeaderType, BH_SystemCreateTimeUtc, BH_IsActive, BH_SystemLastEditTimeUtc, BH_SystemLastEditUser, BH_SystemCreateUser)
VALUES
	(@cusInBondHeaderPK, @branchPK, 'BH01', 'NC5', 'D', '2025-01-01', 1, GETUTCDATE(), 'AFU', 'AFU'),
	(@cusInBondHeaderPK2, @branchPK, 'BH02', 'NC5', 'D', '2025-01-02', 1, GETUTCDATE(), 'AFU', 'AFU'),
	(@cusInBondHeaderPK3, @branchPK, 'BH03', 'NC5', 'D', '2025-01-03', 1, GETUTCDATE(), 'AFU', 'AFU');

INSERT INTO CusInBondMoveHeader
	(BM_PK, BM_BH, BM_CustomsStatus, BM_PaperlessInbondNum, BM_SystemCreateTimeUtc, BM_SystemCreateUser, BM_SystemLastEditTimeUtc, BM_SystemLastEditUser, BM_EntryDate, BM_ValuationDate)
VALUES
	(@cusInBondMoveHeaderPK, @cusInBondHeaderPK, 'REL', @paperlessInbondNum, '2025-01-01', 'AFU', GETUTCDATE(), 'AFU', '2025-01-01', '2025-01-01'),
	(@cusInBondMoveHeaderPK2, @cusInBondHeaderPK2, 'REL', @paperlessInbondNum2, '2025-01-02', 'AFU', GETUTCDATE(), 'AFU', '2025-01-02', '2025-01-02'),
	(@cusInBondMoveHeaderPK3, @cusInBondHeaderPK3, 'REL', @paperlessInbondNum3, '2025-01-01', 'AFU', GETUTCDATE(), 'AFU', '2025-01-01', '2025-01-01');

INSERT INTO JobDocAddress
	(E2_PK, E2_IsValid, E2_AddressType, E2_OA_Address, E2_ParentID, E2_ParentTableCode, E2_SystemCreateTimeUtc, E2_SystemCreateUser, E2_SystemLastEditTimeUtc, E2_SystemLastEditUser)
VALUES 
	(NEWID(), 1, 'PRC', @addressPk, @cusInBondHeaderPK, 'BH', '2025-01-01 17:12:00', 'AFU', '2025-01-01 17:12:00', 'AFU'),
	(NEWID(), 1, 'PRC', @addressPk, @cusInBondHeaderPK2, 'BH', '2025-01-01 17:12:00', 'AFU', '2025-01-02 17:12:00', 'AFU'),
	(NEWID(), 1, 'PRC', @addressPk, @cusInBondHeaderPK3, 'BH', '2025-01-01 17:12:00', 'AFU', '2025-01-03 17:12:00', 'AFU');

INSERT INTO EDIMessage
	(EM_PK, EM_GB, EM_GE, EM_LinkTable, EM_LinkUniqueID, EM_ApplicationCode, EM_MessageType, EM_MessageNum, EM_MessageSubType, EM_ReceiveTransmit, EM_Status, EM_SystemCreateTimeUtc, EM_SystemCreateUser, EM_SystemLastEditTimeUtc, EM_SystemLastEditUser)
VALUES
	(NEWID(), @branchPK, @departmentPK, 'CusInBondHeader', @cusInBondHeaderPK, 'FRC', 'TP5', '001', '029', 'RCV', 'PRS', '2025-01-01', 'AFU', '2025-01-01', 'AFU'),
	(NEWID(), @branchPK, @departmentPK, 'CusInBondHeader', @cusInBondHeaderPK2, 'FRC', 'TP5', '111', '015', 'TRX', 'PRS', '2025-01-01', 'AFU', '2025-01-01', 'AFU'),
	(NEWID(), @branchPK, @departmentPK, 'CusInBondHeader', @cusInBondHeaderPK2, 'FRC', 'TP5', @ediMessageNum, '029', 'RCV', 'PRS', '2025-01-02', 'AFU', '2025-01-02', 'AFU'),
	(NEWID(), @branchPK, @departmentPK, 'CusInBondHeader', @cusInBondHeaderPK2, 'FRC', 'TP5', '202', '029', 'RCV', 'PRS', '2025-01-02', 'AFU', '2025-01-02', 'AFU'),
	(NEWID(), @branchPK, @departmentPK, 'CusInBondHeader', @cusInBondHeaderPK2, 'FRC', 'TP5', '203', '029', 'RCV', 'PRS', '2025-01-02', 'AFU', '2025-01-02', 'AFU'),
	(NEWID(), @branchPK, @departmentPK, 'CusInBondHeader', @cusInBondHeaderPK3, 'FRC', 'TP5', '301', '009', 'RCV', 'PRS', '2025-01-01', 'AFU', '2025-01-01', 'AFU');

INSERT INTO CusEntryNum
	(CE_PK, CE_IsValid, CE_ParentID, CE_ParentTable, CE_EntryNum, CE_Category, CE_EntryType, CE_EntryLineReference, CE_IssueDate, CE_RN_NKCountryCode, CE_SystemCreateTimeUtc, CE_SystemCreateUser, CE_SystemLastEditTimeUtc, CE_SystemLastEditUser)
VALUES
	(NEWID(), 1, @cusInBondHeaderPK, 'CusInBondHeader', '001', 'CUS', 'MRN', 'TA-PRCHEA', @transactionDate, 'FR', '2025-01-01 17:44:00', 'E', '2025-01-01 17:44:00', 'AFU'),
	(NEWID(), 1, @cusInBondHeaderPK2, 'CusInBondHeader', '002', 'CUS', 'MRN', 'TA-PRCHEA', @transactionDate2, 'FR', '2025-01-02 17:44:00', 'E', '2025-01-02 17:44:00', 'AFU'),
	(NEWID(), 1, @cusInBondHeaderPK3, 'CusInBondHeader', '003', 'CUS', 'MRN', 'TA-PRCHEA', '2025-01-02 13:11:00', 'FR', '2025-01-02 13:13:00', 'E', '2025-01-02 15:11:00', 'AFU');

INSERT INTO
	CusPermitHeader (CPH_PK, CPH_RN_NKCountryCode, CPH_OH_PermitHolder, CPH_Number, CPH_ApplicationCode, CPH_Type, CPH_StartDate, CPH_EndDate, CPH_SystemCreateTimeUtc, CPH_SystemCreateUser, CPH_SystemLastEditTimeUtc, CPH_SystemLastEditUser)
VALUES
    (@cusPermitHeaderPK,  'FR', @guaranteeHolderPK, @CPH_Number1, 'GUA', 'COD', '2024-12-15 14:00:00', '2024-12-31 14:00:00', GETUTCDATE(), 'AFU', GETUTCDATE(), 'AFU'),
    (@cusPermitHeaderPK2, 'FR', @guaranteeHolderPK, @CPH_Number2, 'GUA', 'COD', '2024-12-01 14:00:00', '2024-12-14 14:00:00', GETUTCDATE(), 'AFU', GETUTCDATE(), 'AFU'),
    (@cusPermitHeaderPK21, 'FR', @guaranteeHolderPK, @CPH_Number2, 'GUA', 'COD', GETUTCDATE(), GETUTCDATE()+1, GETUTCDATE(), 'AFU', GETUTCDATE(), 'AFU'),
    (@cusPermitHeaderPK22, 'FR', @guaranteeHolderPK, @CPH_Number2, 'GUA', 'COD', '2024-12-15 14:00:00', '2024-12-31 14:00:00', GETUTCDATE(), 'AFU', GETUTCDATE(), 'AFU'),
    (@cusPermitHeaderPK3, 'FR', @guaranteeHolderPK, @CPH_Number3, 'GUA', 'COD', GETUTCDATE(), GETUTCDATE()+1, GETUTCDATE(), 'AFU', GETUTCDATE(), 'AFU');

INSERT INTO
	CusBondDetail (PW_PK, PW_BondNumber, PW_ParentTableCode, PW_ParentID, PW_ApplicationCode, PW_BondAmount, PW_SystemCreateTimeUtc, PW_SystemLastEditTimeUtc, PW_SystemCreateUser, PW_SystemLastEditUser)
VALUES
    (@cusBondDetailPK1, @CPH_Number1, 'BM', @cusInBondMoveHeaderPK, 'NCT', 888.50, '2025-01-01', '2025-01-01 01:01:00', 'AFU', 'AFU'),
    (@cusBondDetailPK2, @CPH_Number2, 'BM', @cusInBondMoveHeaderPK2, 'NCT', @bondAmountForMissingTransaction, '2025-01-11', '2025-01-15 01:01:00', 'AFU', 'AFU'),
    (@cusBondDetailPK4, @CPH_Number3, 'BM', @cusInBondMoveHeaderPK3, 'NCT', 999.90, '2025-01-03', '2025-01-03 01:01:00', 'AFU', 'AFU');

INSERT INTO	
	CusPermitLineTransaction (CPL_PK, CPL_CPH_PermitHeader, CPL_TranQty, CPL_TranValue, CPL_Reference, CPL_Comment, CPL_TransactionDate, CPL_TransactionType, CPL_TransactionCategory, CPL_AppId, CPL_TransactionStatus, CPL_IsAggregated, CPL_Procedure, CPL_ReferenceNumberLine, CPL_SystemCreateTimeUtc, CPL_SystemCreateUser, CPL_SystemLastEditTimeUtc, CPL_SystemLastEditUser)
VALUES 
	(NEWID(), @cusPermitHeaderPK, 100, 888.50, CAST(@paperlessInbondNum as varchar), 'CusPermitLineTransaction1', '2025-02-13 14:00:00', 'TRA', 'CUM', 'App001', 'CON', 0, 'Proc001', 10, '2025-02-13 14:00:00', 'AFU', '2025-02-13 14:00:00', 'AFU'),
	(NEWID(), @cusPermitHeaderPK3, 50, 999.90, CAST(@paperlessInbondNum3 as varchar), 'CusPermitLineTransaction2', '2025-02-14 14:00:00', 'TRA', 'CUM', 'App002', 'CON', 0, 'Proc002', 10, '2025-02-14 14:00:00', 'AFU', '2025-02-14 14:00:00', 'AFU');
";

			var cmd = Db.Connection.Command(sqlText);
			cmd.AddParameter("@branchPK", SqlDbType.UniqueIdentifier, branchPK);
			cmd.AddParameter("@guaranteeHolderPK", SqlDbType.UniqueIdentifier, guaranteeHolderPK);
			cmd.AddParameter("@departmentPK", SqlDbType.UniqueIdentifier, departmentPK);
			cmd.AddParameter("@companyPK", SqlDbType.UniqueIdentifier, companyPK);
			cmd.AddParameter("@entryHeaderPK", SqlDbType.UniqueIdentifier, entryHeaderPK);
			cmd.AddParameter("@jobDeclarationPK", SqlDbType.UniqueIdentifier, jobDeclarationPK);
			cmd.AddParameter("@cusInBondHeaderPK", SqlDbType.UniqueIdentifier, cusInBondHeaderPK);
			cmd.AddParameter("@cusInBondHeaderPK2", SqlDbType.UniqueIdentifier, cusInBondHeaderPK2);
			cmd.AddParameter("@cusInBondHeaderPK3", SqlDbType.UniqueIdentifier, cusInBondHeaderPK3);
			cmd.AddParameter("@cusInBondMoveHeaderPK", SqlDbType.UniqueIdentifier, cusInBondMoveHeaderPK);
			cmd.AddParameter("@cusInBondMoveHeaderPK2", SqlDbType.UniqueIdentifier, cusInBondMoveHeaderPK2);
			cmd.AddParameter("@cusInBondMoveHeaderPK3", SqlDbType.UniqueIdentifier, cusInBondMoveHeaderPK3);
			cmd.AddParameter("@cusPermitHeaderPK", SqlDbType.UniqueIdentifier, cusPermitHeaderPK);
			cmd.AddParameter("@cusPermitHeaderPK2", SqlDbType.UniqueIdentifier, cusPermitHeaderPK2);
			cmd.AddParameter("@cusPermitHeaderPK21", SqlDbType.UniqueIdentifier, cusPermitHeaderPK21);
			cmd.AddParameter("@cusPermitHeaderPK22", SqlDbType.UniqueIdentifier, cusPermitHeaderPK22);
			cmd.AddParameter("@cusPermitHeaderPK3", SqlDbType.UniqueIdentifier, cusPermitHeaderPK3);
			cmd.AddParameter("@cusPermitHeaderPK4", SqlDbType.UniqueIdentifier, cusPermitHeaderPK4);
			cmd.AddParameter("@cusBondDetailPK1", SqlDbType.UniqueIdentifier, cusBondDetailPK1);
			cmd.AddParameter("@cusBondDetailPK2", SqlDbType.UniqueIdentifier, cusBondDetailPK2);
			cmd.AddParameter("@cusBondDetailPK3", SqlDbType.UniqueIdentifier, cusBondDetailPK3);
			cmd.AddParameter("@cusBondDetailPK4", SqlDbType.UniqueIdentifier, cusBondDetailPK4);
			cmd.AddParameter("@bondAmountForMissingTransaction", SqlDbType.Decimal, bondAmountForMissingTransaction);
			cmd.AddParameter("@bondAmountForMissingTransaction2", SqlDbType.Decimal, bondAmountForMissingTransaction2);
			cmd.AddParameter("@ediMessageNum", SqlDbType.VarChar, ediMessageNum);
			cmd.AddParameter("@paperlessInbondNum", SqlDbType.VarChar, paperlessInbondNum);
			cmd.AddParameter("@paperlessInbondNum2", SqlDbType.VarChar, paperlessInbondNum2);
			cmd.AddParameter("@paperlessInbondNum3", SqlDbType.VarChar, paperlessInbondNum3);
			cmd.AddParameter("@transactionDate", SqlDbType.SmallDateTime, transactionDate);
			cmd.AddParameter("@transactionDate2", SqlDbType.SmallDateTime, transactionDate2);
			cmd.AddParameter("@CPH_Number1", SqlDbType.VarChar, CPH_Number1);
			cmd.AddParameter("@CPH_Number2", SqlDbType.VarChar, CPH_Number2);
			cmd.AddParameter("@CPH_Number3", SqlDbType.VarChar, CPH_Number3);
			cmd.AddParameter("@addressPK", SqlDbType.UniqueIdentifier, addressPK);
			cmd.ExecuteNonQuery();
		}
	}
}
