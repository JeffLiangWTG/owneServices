using System.Collections.Generic;
using System.Linq;
using CargoWise.Billing.Collectors.Accounting;
using Enterprise.Integration.Billing;
using NUnit.Framework;

namespace Enterprise.Billing.StlCollector.Retriever.Scripts.Accounting
{
	[TestedType(typeof(AccBankUsageCollector))]
	sealed class AccBankUsageCollectorTest : RefStlScriptWithDefaultsTest
	{
		protected override IDateTimeRange TestDateTimeRange => AusydMonthRange.New(2024, 1);

		protected override bool IsMandatoryForMilestones => false;

		protected override void PrepareTestData()
		{
			var sql = @"declare @SYDCompanyPK UNIQUEIDENTIFIER = '00000001-0000-0000-0000-000000000001';
						declare @TYOCompanyPK UNIQUEIDENTIFIER = '00000001-0000-0000-0000-000000000002';
						declare @EGPCompanyPK UNIQUEIDENTIFIER = '00000001-0000-0000-0000-000000000003';
						declare @MELCompanyPK UNIQUEIDENTIFIER = '00000001-0000-0000-0000-000000000004';

						declare @SYDOrgProxyPK UNIQUEIDENTIFIER = '00000002-0000-0000-0000-000000000001';
						declare @TYOOrgProxyPK UNIQUEIDENTIFIER = '00000002-0000-0000-0000-000000000002';
						declare @EGPOrgProxyPK UNIQUEIDENTIFIER = '00000002-0000-0000-0000-000000000003';
						declare @MELOrgProxyPK UNIQUEIDENTIFIER = '00000002-0000-0000-0000-000000000004';

						declare @SYDBranchPK UNIQUEIDENTIFIER = '00000003-0000-0000-0000-000000000001';
						declare @TYOBranchPK UNIQUEIDENTIFIER = '00000003-0000-0000-0000-000000000002';
						declare @EGPBranchPK UNIQUEIDENTIFIER = '00000003-0000-0000-0000-000000000003';
						declare @MELBranchPK UNIQUEIDENTIFIER = '00000003-0000-0000-0000-000000000004';

						declare @SYDHeaderPK UNIQUEIDENTIFIER = '00000004-0000-0000-0000-000000000001';
						declare @SYD2HeaderPK UNIQUEIDENTIFIER = '00000004-0000-0000-0000-000000000011';
						declare @SYD3HeaderPK UNIQUEIDENTIFIER = '00000004-0000-0000-0000-000000000021';
						declare @TYOHeaderPK UNIQUEIDENTIFIER = '00000004-0000-0000-0000-000000000002';
						declare @EGPHeaderPK UNIQUEIDENTIFIER = '00000004-0000-0000-0000-000000000003';
						declare @MELHeaderPK UNIQUEIDENTIFIER = '00000004-0000-0000-0000-000000000004';

						declare @SYDDepartmentPK UNIQUEIDENTIFIER = '00000005-0000-0000-0000-000000000001';
						declare @TYODepartmentPK UNIQUEIDENTIFIER = '00000005-0000-0000-0000-000000000002';
						declare @EGPDepartmentPK UNIQUEIDENTIFIER = '00000005-0000-0000-0000-000000000003';
						declare @MELDepartmentPK UNIQUEIDENTIFIER = '00000005-0000-0000-0000-000000000004';

						DECLARE @SYDBankAccountPK UNIQUEIDENTIFIER  = '11111111-1111-1111-1111-111111111111';
						DECLARE @SYD2BankAccountPK UNIQUEIDENTIFIER = '11111111-1111-1111-1111-111111111112';
						DECLARE @SYD3BankAccountPK UNIQUEIDENTIFIER = '11111111-1111-1111-1111-111111111113';
						DECLARE @TYOBankAccountPK UNIQUEIDENTIFIER  = '22222222-2222-2222-2222-222222222222';
						DECLARE @EGPBankAccountPK UNIQUEIDENTIFIER  = '33333333-3333-3333-3333-333333333333';
						DECLARE @MELBankAccountPK UNIQUEIDENTIFIER  = '44444444-4444-4444-4444-444444444444';

						INSERT dbo.OrgHeader (OH_PK, OH_Code) VALUES
										(@SYDOrgProxyPK, 'SYD100'),
										(@TYOOrgProxyPK, 'TOKYOJ'),
										(@EGPOrgProxyPK, 'EGYPT1'),
										(@MELOrgProxyPK, 'MEL101');

						INSERT dbo.GlbCompany (GC_PK, GC_Code, GC_Name, GC_RX_NKLocalCurrency, GC_RN_NKCountryCode, GC_OH_OrgProxy) VALUES
										(@SYDCompanyPK, 'SD1', 'Test SYD Company', 'AUD', 'AU', @SYDOrgProxyPK),
										(@TYOCompanyPK, 'JTK', 'Test TYO Company', 'JPY', 'JP', @TYOOrgProxyPK),
										(@EGPCompanyPK, 'DEG', 'Test EGD Company', 'EGP', 'EG', @EGPOrgProxyPK),
										(@MELCompanyPK, 'MEL', 'Test MEL Company', 'AUD', 'AU', @MELOrgProxyPK);

						INSERT dbo.GlbBranch (GB_PK, GB_Code, GB_GC) VALUES
										(@SYDBranchPK, 'SD1',  @SYDCompanyPK),
										(@TYOBranchPK, 'TK1',  @TYOCompanyPK),
										(@EGPBranchPK, 'EG1',  @EGPCompanyPK),
										(@MELBranchPK, 'ML1',  @MELCompanyPK);

						INSERT dbo.AccGLHeader (AG_PK, AG_AccountType, AG_AccountNum, AG_DebitCredit) VALUES
										(@SYDHeaderPK,  'XXX', 10, 'DR'),
										(@SYD2HeaderPK, 'XXX', 11, 'DR'),
										(@SYD3HeaderPK, 'XXX', 12, 'DR'),
										(@TYOHeaderPK,  'XXX', 20, 'DR'),
										(@EGPHeaderPK,  'XXX', 30, 'DR'),
										(@MELHeaderPK,  'XXX', 40, 'DR');

						INSERT dbo.GlbDepartment (GE_PK, GE_Code) VALUES 
										(@SYDDepartmentPK, 'DE1'),
										(@TYODepartmentPK, 'DE2'),
										(@EGPDepartmentPK, 'DE3'),
										(@MELDepartmentPK, 'DE4');

						INSERT dbo.AccBankAccount (AB_PK, AB_GC, AB_GB, AB_AG, AB_Code, AB_AccountNum, AB_AccountType, AB_IsActive, AB_Desc, AB_BankName, AB_SWIFT, AB_BSB, AB_RN_NKBankAccountCountry, AB_RX_NKAccountCurrency, AB_SystemCreateUser, AB_LastReconcileDate, AB_SystemLastEditUser, AB_SystemLastEditTimeUtc) VALUES
									(@SYDBankAccountPK,  @SYDCompanyPK, @SYDBranchPK, @SYDHeaderPK,  'SYDBA', 1, 'BNK', 1, 'SYD BANK ACCOUNT',  'BANK OF AUSTRALIA',      '12345', '063111', 'AU', 'AUD', 'UT1', '2024-01-01 00:00:00', 'UT2', '2024-01-04 00:00:00'),
									(@SYD2BankAccountPK, @SYDCompanyPK, @SYDBranchPK, @SYD2HeaderPK, 'SYDBB', 1, 'BNK', 1, 'SYD2 BANK ACCOUNT', 'BANK OF AUSTRALIA',      '12345', '063112', 'AU', 'AUD', 'UT1', '2024-01-01 00:00:00', 'UT2', '2024-01-04 00:00:00'),
									(@SYD3BankAccountPK, @SYDCompanyPK, @SYDBranchPK, @SYD3HeaderPK, 'SYDBC', 1, 'BNK', 1, 'SYD3 BANK ACCOUNT', 'BANK OF AUSTRALIA',      '12345', '063113', 'AU', 'AUD', 'UT1', '2024-01-01 00:00:00', 'UT2', '2024-01-04 00:00:00'),
									(@TYOBankAccountPK,  @TYOCompanyPK, @TYOBranchPK, @TYOHeaderPK,  'TYOBA', 2, 'LNK', 1, 'TYO BANK ACCOUNT',  'BANK OF JAPAN',          '22345', '163123', 'JP', 'JPY', 'UT2', '2024-01-02 00:00:00', 'UT3', '2024-01-05 00:00:00'),
									(@EGPBankAccountPK,  @EGPCompanyPK, @EGPBranchPK, @EGPHeaderPK,  'EGPBA', 3, 'BNK', 1, 'EGP BANK ACCOUNT',  'NATIONAL BANK OF EGYPT', '32345', '263222', 'EG', 'EGP', 'UT3', '2024-01-03 00:00:00', 'UT1', '2024-01-06 00:00:00'),
									(@MELBankAccountPK,  @MELCompanyPK, @MELBranchPK, @MELHeaderPK,  'MLDBA', 4, 'LNK', 0, 'MEL BANK ACCOUNT',  'BANK OF MELBOURNE',      '42345', '163111', 'AU', 'AUD', 'UT4', '2024-01-04 00:00:00', 'UT5', '2024-01-07 00:00:00');

						-- Transactions outside date range
						INSERT INTO dbo.AccTransactionHeader (AH_PK, AH_GC, AH_GB, AH_GE, AH_AB, AH_InvoiceDate, AH_Ledger, AH_TransactionType, AH_TransactionNum, AH_SystemCreateTimeUtc, AH_SystemCreateUser, AH_RX_NKTransactionCurrency, AH_OSTotal, AH_IsCancelled, AH_ReceiptType) VALUES
						(NEWID(), @SYDCompanyPK, @SYDBranchPK, @SYDDepartmentPK, @SYDBankAccountPK,  '2023-12-13', 'CB', 'DPY', '011', '2023-12-24', 'UT2', 'AUD', 411.00, 0, 'EFT');
						INSERT INTO dbo.AccTransactionHeader (AH_PK, AH_GC, AH_GB, AH_GE, AH_AB, AH_InvoiceDate, AH_Ledger, AH_TransactionType, AH_TransactionNum, AH_SystemCreateTimeUtc, AH_SystemCreateUser, AH_RX_NKTransactionCurrency, AH_OSTotal, AH_IsCancelled, AH_ReceiptType) VALUES
						(NEWID(), @SYDCompanyPK, @SYDBranchPK, @SYDDepartmentPK, @SYDBankAccountPK,  '2023-12-15', 'CB', 'DPY', '012', '2023-12-26', 'UT3', 'AUD', 412.00, 0, 'EFT');

						-- Single transaction
						INSERT INTO dbo.AccTransactionHeader (AH_PK, AH_GC, AH_GB, AH_GE, AH_AB, AH_InvoiceDate, AH_Ledger, AH_TransactionType, AH_TransactionNum, AH_SystemCreateTimeUtc, AH_SystemCreateUser, AH_RX_NKTransactionCurrency, AH_OSTotal, AH_IsCancelled, AH_ReceiptType) VALUES
						(NEWID(), @TYOCompanyPK, @TYOBranchPK, @TYODepartmentPK, @TYOBankAccountPK,  '2024-01-03', 'CB', 'TRF', '002', '2024-01-04', 'UT2', 'JPY', 402.00, 0, 'EFT');

						-- Grouping by IsCanceled
						INSERT INTO dbo.AccTransactionHeader (AH_PK, AH_GC, AH_GB, AH_GE, AH_AB, AH_InvoiceDate, AH_Ledger, AH_TransactionType, AH_TransactionNum, AH_SystemCreateTimeUtc, AH_SystemCreateUser, AH_RX_NKTransactionCurrency, AH_OSTotal, AH_IsCancelled, AH_ReceiptType) VALUES
						(NEWID(), @SYDCompanyPK, @SYDBranchPK, @SYDDepartmentPK, @SYD2BankAccountPK, '2024-01-01', 'CB', 'TRF', '001', '2024-01-02', 'UT1', 'AUD', 401.00, 1, 'EFT');
						INSERT INTO dbo.AccTransactionHeader (AH_PK, AH_GC, AH_GB, AH_GE, AH_AB, AH_InvoiceDate, AH_Ledger, AH_TransactionType, AH_TransactionNum, AH_SystemCreateTimeUtc, AH_SystemCreateUser, AH_RX_NKTransactionCurrency, AH_OSTotal, AH_IsCancelled, AH_ReceiptType) VALUES
						(NEWID(), @SYDCompanyPK, @SYDBranchPK, @SYDDepartmentPK, @SYD2BankAccountPK, '2024-01-01', 'CB', 'TRF', '102', '2024-01-02', 'UT1', 'AUD', 401.01, 1, 'EFT');
						INSERT INTO dbo.AccTransactionHeader (AH_PK, AH_GC, AH_GB, AH_GE, AH_AB, AH_InvoiceDate, AH_Ledger, AH_TransactionType, AH_TransactionNum, AH_SystemCreateTimeUtc, AH_SystemCreateUser, AH_RX_NKTransactionCurrency, AH_OSTotal, AH_IsCancelled, AH_ReceiptType) VALUES
						(NEWID(), @SYDCompanyPK, @SYDBranchPK, @SYDDepartmentPK, @SYD2BankAccountPK, '2024-01-01', 'CB', 'TRF', '103', '2024-01-02', 'UT1', 'AUD', 401.02, 0, 'EFT');

						-- Grouping by Transaction Type
						INSERT INTO dbo.AccTransactionHeader (AH_PK, AH_GC, AH_GB, AH_GE, AH_AB, AH_InvoiceDate, AH_Ledger, AH_TransactionType, AH_TransactionNum, AH_SystemCreateTimeUtc, AH_SystemCreateUser, AH_RX_NKTransactionCurrency, AH_OSTotal, AH_IsCancelled, AH_ReceiptType) VALUES
						(NEWID(), @EGPCompanyPK, @EGPBranchPK, @EGPDepartmentPK, @EGPBankAccountPK,  '2024-01-05', 'CB', 'TRF', '003', '2024-01-06', 'UT3', 'EGP', 403.00, 0, 'EFT');
						INSERT INTO dbo.AccTransactionHeader (AH_PK, AH_GC, AH_GB, AH_GE, AH_AB, AH_InvoiceDate, AH_Ledger, AH_TransactionType, AH_TransactionNum, AH_SystemCreateTimeUtc, AH_SystemCreateUser, AH_RX_NKTransactionCurrency, AH_OSTotal, AH_IsCancelled, AH_ReceiptType) VALUES
						(NEWID(), @EGPCompanyPK, @EGPBranchPK, @EGPDepartmentPK, @EGPBankAccountPK,  '2024-01-07', 'CB', 'TRF', '004', '2024-01-08', 'UT4', 'EGP', 404.00, 0, 'EFT');
						INSERT INTO dbo.AccTransactionHeader (AH_PK, AH_GC, AH_GB, AH_GE, AH_AB, AH_InvoiceDate, AH_Ledger, AH_TransactionType, AH_TransactionNum, AH_SystemCreateTimeUtc, AH_SystemCreateUser, AH_RX_NKTransactionCurrency, AH_OSTotal, AH_IsCancelled, AH_ReceiptType) VALUES
						(NEWID(), @EGPCompanyPK, @EGPBranchPK, @EGPDepartmentPK, @EGPBankAccountPK,  '2024-01-09', 'CB', 'TRF', '005', '2024-01-10', 'UT5', 'EGP', 405.00, 0, 'EFT');
						INSERT INTO dbo.AccTransactionHeader (AH_PK, AH_GC, AH_GB, AH_GE, AH_AB, AH_InvoiceDate, AH_Ledger, AH_TransactionType, AH_TransactionNum, AH_SystemCreateTimeUtc, AH_SystemCreateUser, AH_RX_NKTransactionCurrency, AH_OSTotal, AH_IsCancelled, AH_ReceiptType) VALUES
						(NEWID(), @EGPCompanyPK, @EGPBranchPK, @EGPDepartmentPK, @EGPBankAccountPK,  '2024-01-11', 'CB', 'TRF', '006', '2024-01-12', 'UT6', 'EGP', 406.00, 0, 'EFT');
						INSERT INTO dbo.AccTransactionHeader (AH_PK, AH_GC, AH_GB, AH_GE, AH_AB, AH_InvoiceDate, AH_Ledger, AH_TransactionType, AH_TransactionNum, AH_SystemCreateTimeUtc, AH_SystemCreateUser, AH_RX_NKTransactionCurrency, AH_OSTotal, AH_IsCancelled, AH_ReceiptType) VALUES
						(NEWID(), @EGPCompanyPK, @EGPBranchPK, @EGPDepartmentPK, @EGPBankAccountPK,  '2024-01-13', 'CB', 'DPY', '007', '2024-01-14', 'UT7', 'EGP', 407.00, 0, 'EFT');
						INSERT INTO dbo.AccTransactionHeader (AH_PK, AH_GC, AH_GB, AH_GE, AH_AB, AH_InvoiceDate, AH_Ledger, AH_TransactionType, AH_TransactionNum, AH_SystemCreateTimeUtc, AH_SystemCreateUser, AH_RX_NKTransactionCurrency, AH_OSTotal, AH_IsCancelled, AH_ReceiptType) VALUES
						(NEWID(), @EGPCompanyPK, @EGPBranchPK, @EGPDepartmentPK, @EGPBankAccountPK,  '2024-01-15', 'CB', 'DPY', '008', '2024-01-16', 'UT8', 'EGP', 408.00, 0, 'EFT');
						INSERT INTO dbo.AccTransactionHeader (AH_PK, AH_GC, AH_GB, AH_GE, AH_AB, AH_InvoiceDate, AH_Ledger, AH_TransactionType, AH_TransactionNum, AH_SystemCreateTimeUtc, AH_SystemCreateUser, AH_RX_NKTransactionCurrency, AH_OSTotal, AH_IsCancelled, AH_ReceiptType) VALUES
						(NEWID(), @EGPCompanyPK, @EGPBranchPK, @EGPDepartmentPK, @EGPBankAccountPK,  '2024-01-17', 'CB', 'RCB', '009', '2024-01-18', 'UT9', 'EGP', 409.00, 0, 'EFT');
						INSERT INTO dbo.AccTransactionHeader (AH_PK, AH_GC, AH_GB, AH_GE, AH_AB, AH_InvoiceDate, AH_Ledger, AH_TransactionType, AH_TransactionNum, AH_SystemCreateTimeUtc, AH_SystemCreateUser, AH_RX_NKTransactionCurrency, AH_OSTotal, AH_IsCancelled, AH_ReceiptType) VALUES
						(NEWID(), @EGPCompanyPK, @EGPBranchPK, @EGPDepartmentPK, @EGPBankAccountPK,  '2024-01-19', 'CB', 'RCB', '010', '2024-01-20', 'UT0', 'EGP', 410.00, 0, 'EFT');
						INSERT INTO dbo.AccTransactionHeader (AH_PK, AH_GC, AH_GB, AH_GE, AH_AB, AH_InvoiceDate, AH_Ledger, AH_TransactionType, AH_TransactionNum, AH_SystemCreateTimeUtc, AH_SystemCreateUser, AH_RX_NKTransactionCurrency, AH_OSTotal, AH_IsCancelled, AH_ReceiptType) VALUES
						(NEWID(), @MELCompanyPK, @MELBranchPK, @MELDepartmentPK, @MELBankAccountPK,  '2024-01-11', 'CB', 'DPY', '011', '2024-01-22', 'UT1', 'AUD', 411.00, 0, 'EFT');

						-- Grouping by Receipt / Payment Type
						INSERT INTO dbo.AccTransactionHeader (AH_PK, AH_GC, AH_GB, AH_GE, AH_AB, AH_InvoiceDate, AH_Ledger, AH_TransactionType, AH_TransactionNum, AH_SystemCreateTimeUtc, AH_SystemCreateUser, AH_RX_NKTransactionCurrency, AH_OSTotal, AH_IsCancelled, AH_ReceiptType) VALUES
						(NEWID(), @SYDCompanyPK, @SYDBranchPK, @SYDDepartmentPK, @SYD3BankAccountPK, '2024-01-01', 'CB', 'TRF', '201', '2024-01-02', 'UT1', 'AUD', 441.00, 0, 'EFT');
						INSERT INTO dbo.AccTransactionHeader (AH_PK, AH_GC, AH_GB, AH_GE, AH_AB, AH_InvoiceDate, AH_Ledger, AH_TransactionType, AH_TransactionNum, AH_SystemCreateTimeUtc, AH_SystemCreateUser, AH_RX_NKTransactionCurrency, AH_OSTotal, AH_IsCancelled, AH_ReceiptType) VALUES
						(NEWID(), @SYDCompanyPK, @SYDBranchPK, @SYDDepartmentPK, @SYD3BankAccountPK, '2024-01-01', 'CB', 'TRF', '202', '2024-01-02', 'UT1', 'AUD', 442.00, 0, 'CHQ');
						INSERT INTO dbo.AccTransactionHeader (AH_PK, AH_GC, AH_GB, AH_GE, AH_AB, AH_InvoiceDate, AH_Ledger, AH_TransactionType, AH_TransactionNum, AH_SystemCreateTimeUtc, AH_SystemCreateUser, AH_RX_NKTransactionCurrency, AH_OSTotal, AH_IsCancelled, AH_ReceiptType) VALUES
						(NEWID(), @SYDCompanyPK, @SYDBranchPK, @SYDDepartmentPK, @SYD3BankAccountPK, '2024-01-01', 'CB', 'TRF', '203', '2024-01-02', 'UT1', 'AUD', 443.00, 0, 'CSH');
						INSERT INTO dbo.AccTransactionHeader (AH_PK, AH_GC, AH_GB, AH_GE, AH_AB, AH_InvoiceDate, AH_Ledger, AH_TransactionType, AH_TransactionNum, AH_SystemCreateTimeUtc, AH_SystemCreateUser, AH_RX_NKTransactionCurrency, AH_OSTotal, AH_IsCancelled, AH_ReceiptType) VALUES
						(NEWID(), @SYDCompanyPK, @SYDBranchPK, @SYDDepartmentPK, @SYD3BankAccountPK, '2024-01-01', 'CB', 'TRF', '204', '2024-01-02', 'UT1', 'AUD', 444.00, 0, 'CSH');
";

			TestConnection.ExecuteNonQuery(sql);
		}

		protected override void AssertResultSet(IEnumerable<IStlTransaction> transactions)
		{
			var expectedDataForUnusedAccount = @"{""AB_PK"":""11111111-1111-1111-1111-111111111111"",""AB_RN_NKBankAccountCountry"":""AU"",""AB_RX_NKAccountCurrency"":""AUD"",""AB_Desc"":""SYD BANK ACCOUNT"",""AB_BankName"":""BANK OF AUSTRALIA"",""AB_BSB"":""063111"",""AB_LastReconcileDate"":""2024-01-01T00:00:00"",""AB_SystemLastEditUser"":""UT2"",""AB_SystemLastEditTimeUtc"":""2024-01-04T00:00:00"",""TransactionCount"":0}";
			var expectedDataForOneTransaction = @"{""AB_PK"":""22222222-2222-2222-2222-222222222222"",""AB_RN_NKBankAccountCountry"":""JP"",""AB_RX_NKAccountCurrency"":""JPY"",""AB_Desc"":""TYO BANK ACCOUNT"",""AB_BankName"":""BANK OF JAPAN"",""AB_BSB"":""163123"",""AB_LastReconcileDate"":""2024-01-02T00:00:00"",""AH_TransactionType"":""TRF"",""AB_SystemLastEditUser"":""UT3"",""AB_SystemLastEditTimeUtc"":""2024-01-05T00:00:00"",""TransactionCount"":1}";
			var expectedDataForCanceledTransactions = @"{""AB_PK"":""11111111-1111-1111-1111-111111111112"",""AB_RN_NKBankAccountCountry"":""AU"",""AB_RX_NKAccountCurrency"":""AUD"",""AB_Desc"":""SYD2 BANK ACCOUNT"",""AB_BankName"":""BANK OF AUSTRALIA"",""AB_BSB"":""063112"",""AB_LastReconcileDate"":""2024-01-01T00:00:00"",""AH_TransactionType"":""TRF"",""AB_SystemLastEditUser"":""UT2"",""AB_SystemLastEditTimeUtc"":""2024-01-04T00:00:00"",""TransactionCount"":3}";
			var expectedDataForTransactionTypeDPY = @"{""AB_PK"":""33333333-3333-3333-3333-333333333333"",""AB_RN_NKBankAccountCountry"":""EG"",""AB_RX_NKAccountCurrency"":""EGP"",""AB_Desc"":""EGP BANK ACCOUNT"",""AB_BankName"":""NATIONAL BANK OF EGYPT"",""AB_BSB"":""263222"",""AB_LastReconcileDate"":""2024-01-03T00:00:00"",""AH_TransactionType"":""DPY"",""AB_SystemLastEditUser"":""UT1"",""AB_SystemLastEditTimeUtc"":""2024-01-06T00:00:00"",""TransactionCount"":2}";
			var expectedDataForTransactionTypeRCB = @"{""AB_PK"":""33333333-3333-3333-3333-333333333333"",""AB_RN_NKBankAccountCountry"":""EG"",""AB_RX_NKAccountCurrency"":""EGP"",""AB_Desc"":""EGP BANK ACCOUNT"",""AB_BankName"":""NATIONAL BANK OF EGYPT"",""AB_BSB"":""263222"",""AB_LastReconcileDate"":""2024-01-03T00:00:00"",""AH_TransactionType"":""RCB"",""AB_SystemLastEditUser"":""UT1"",""AB_SystemLastEditTimeUtc"":""2024-01-06T00:00:00"",""TransactionCount"":2}";
			var expectedDataForTransactionTypeTRF = @"{""AB_PK"":""33333333-3333-3333-3333-333333333333"",""AB_RN_NKBankAccountCountry"":""EG"",""AB_RX_NKAccountCurrency"":""EGP"",""AB_Desc"":""EGP BANK ACCOUNT"",""AB_BankName"":""NATIONAL BANK OF EGYPT"",""AB_BSB"":""263222"",""AB_LastReconcileDate"":""2024-01-03T00:00:00"",""AH_TransactionType"":""TRF"",""AB_SystemLastEditUser"":""UT1"",""AB_SystemLastEditTimeUtc"":""2024-01-06T00:00:00"",""TransactionCount"":4}";
			var expectedDataForReceipt = @"{""AB_PK"":""11111111-1111-1111-1111-111111111113"",""AB_RN_NKBankAccountCountry"":""AU"",""AB_RX_NKAccountCurrency"":""AUD"",""AB_Desc"":""SYD3 BANK ACCOUNT"",""AB_BankName"":""BANK OF AUSTRALIA"",""AB_BSB"":""063113"",""AB_LastReconcileDate"":""2024-01-01T00:00:00"",""AH_TransactionType"":""TRF"",""AB_SystemLastEditUser"":""UT2"",""AB_SystemLastEditTimeUtc"":""2024-01-04T00:00:00"",""TransactionCount"":4}";

			AssertEquals("Expecting 7 data points, matching above local variables", 7, transactions.Count());

			var unusedBankAccount = transactions.Single(t => t.Reference5 == "11111111-1111-1111-1111-111111111111");
			AssertEquals("An active bank account with no transactions in date range should have 0 transaction count", expectedDataForUnusedAccount, unusedBankAccount.AdditionalRefs);

			var oneTransactionBankAccount = transactions.Single(t => t.Reference5 == "22222222-2222-2222-2222-222222222222");
			AssertEquals("When an account only has one transaction, transaction count should be 1", expectedDataForOneTransaction, oneTransactionBankAccount.AdditionalRefs);

			var cancelledTransactionBankAccount = transactions.Single(t => t.Reference5 == "11111111-1111-1111-1111-111111111112");
			AssertEquals(expectedDataForCanceledTransactions, cancelledTransactionBankAccount.AdditionalRefs);

			CombineAssertions("When an account has transactions under different transaction types, number of transactions will be counted for each transaction type", () =>
			{
				transactions.Single(t => t.Reference5 == "33333333-3333-3333-3333-333333333333" && t.AdditionalRefs == expectedDataForTransactionTypeDPY);
				transactions.Single(t => t.Reference5 == "33333333-3333-3333-3333-333333333333" && t.AdditionalRefs == expectedDataForTransactionTypeRCB);
				transactions.Single(t => t.Reference5 == "33333333-3333-3333-3333-333333333333" && t.AdditionalRefs == expectedDataForTransactionTypeTRF);
			});

			var eftTypeBankAccount = transactions.Single(t => t.Reference5 == "11111111-1111-1111-1111-111111111113");
			AssertEquals(expectedDataForReceipt, eftTypeBankAccount.AdditionalRefs);

			var inactiveBankAccount = transactions.FirstOrDefault(t => t.Reference5 == "44444444-4444-4444-4444-444444444444");
			AssertNull("When an account is inactive, the account information should not be collected", inactiveBankAccount);
		}
	}
}
