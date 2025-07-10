using System;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.MasterFiles.Business.AccountingMasterFilesTaxFrameworkConstants;

namespace Enterprise.Accounting.TaxFramework.Business.Testing
{
	class GLMovementCreatorTest : TestCaseWithFactory
	{
		public void TestGLAccountMovementUseThisClassAsDependency()
		{
			var glMovementCreator = new GLMovementProcessor();
			AssertType<GLMovementCreator>(glMovementCreator.GLMovementCreator_ExposedForTestOnly);
		}

		#region CreateNormalRecord

		public void TestCreateNormalRecord_InvalidDateSource()
		{
			var glMovementCreator = (IGLMovementCreator)new GLMovementCreator();
			var taxRecord = CreateTaxRecord(TaxBasisList.Posting.Code, ZDate.Today, ZDate.Today, 10, LedgerControlAccount, TaxControlAccount, TaxExpenseAccount, null);
#if NETFRAMEWORK
			AssertExceptionThrown<ArgumentException>("", "'100' is invalid value.\r\nParameter name: dateSource", () => glMovementCreator.CreateNormalRecord(taxRecord, (GLMovementDateSource)100));
#else
			var ex = AssertExceptionThrown<ArgumentException>(() => glMovementCreator.CreateNormalRecord(taxRecord, (GLMovementDateSource)100));
			Assert(ex.Message.Equals("'100' is invalid value. (Parameter 'dateSource')"));
			Assert(ex.ParamName.Equals("dateSource"));
#endif
		}

		public void TestCreateNormalRecord_LedgerControlAndTaxControlAccounts_PostDate()
		{
			AssertCreateNormalRecord_LedgerControlAndTaxControlAccounts(GLMovementDateSource.TaxRecordPostDate);
		}

		public void TestCreateNormalRecord_LedgerControlAndTaxControlAccounts_RealisationDate()
		{
			AssertCreateNormalRecord_LedgerControlAndTaxControlAccounts(GLMovementDateSource.TaxRecordRealisationDate);
		}

		void AssertCreateNormalRecord_LedgerControlAndTaxControlAccounts(GLMovementDateSource dateSource)
		{
			var glMovementCreator = (IGLMovementCreator)new GLMovementCreator();
			var expectedDate = ZDate.Today.AddDays(-3);
			var postDate = dateSource == GLMovementDateSource.TaxRecordPostDate ? expectedDate : ZDate.Empty;
			var realisationDate = dateSource == GLMovementDateSource.TaxRecordRealisationDate ? expectedDate : ZDate.Empty;

			var taxRecord = CreateTaxRecord(TaxBasisList.Posting.Code, postDate, realisationDate, 10, LedgerControlAccount, TaxControlAccount, TaxExpenseAccount, null);
			AssertExceptionThrown<TaxFrameworkUnknownConfigurationValueException>("", "'TTT' tax configuration has conflicting combination of GL Accounts.", () => glMovementCreator.CreateNormalRecord(taxRecord, dateSource));
			DeleteGLMovements(taxRecord);

			taxRecord.ATT_AG_TaxExpenseAccount = ZGuid.Empty;
			glMovementCreator.CreateNormalRecord(taxRecord, dateSource);
			var glMovements = LoadGLMovements(taxRecord);
			AssertEquals("Created GLMovement count for tax record with correct number of GL accounts", 1, glMovements.Length);
			AssertGLMovement(nameof(taxRecord), glMovements[0], TaxGLMovementTypeList.Normal.Code, taxRecord.PK, LedgerControlAccount, TaxControlAccount, 10, expectedDate);

			var taxRecord2 = CreateTaxRecord(TaxBasisList.Posting.Code, postDate, realisationDate, -10, LedgerControlAccount, TaxControlAccount, null, TaxPendingControlAccount);
			AssertExceptionThrown<TaxFrameworkUnknownConfigurationValueException>("", "'TTT' tax configuration has conflicting combination of GL Accounts.", () => glMovementCreator.CreateNormalRecord(taxRecord2, dateSource));
			DeleteGLMovements(taxRecord2);

			taxRecord2.ATT_AG_TaxPendingControlAccount = ZGuid.Empty;
			glMovementCreator.CreateNormalRecord(taxRecord2, dateSource);
			glMovements = LoadGLMovements(taxRecord2);
			AssertEquals($"Created GLMovement count for {nameof(taxRecord2)}", 1, glMovements.Length);
			AssertGLMovement(nameof(taxRecord2), glMovements[0], TaxGLMovementTypeList.Normal.Code, taxRecord2.PK, TaxControlAccount, LedgerControlAccount, 10, expectedDate);
		}

		public void TestCreateNormalRecord_LedgerControlAndTaxExpenseAccounts_PostDate()
		{
			AssertCreateNormalRecord_LedgerControlAndTaxExpenseAccounts(GLMovementDateSource.TaxRecordPostDate);
		}
		public void TestCreateNormalRecord_LedgerControlAndTaxExpenseAccounts_RealisationDate()
		{
			AssertCreateNormalRecord_LedgerControlAndTaxExpenseAccounts(GLMovementDateSource.TaxRecordRealisationDate);
		}

		void AssertCreateNormalRecord_LedgerControlAndTaxExpenseAccounts(GLMovementDateSource dateSource)
		{
			var glMovementCreator = (IGLMovementCreator)new GLMovementCreator();
			var expectedDate = ZDate.Today.AddDays(-4);
			var postDate = dateSource == GLMovementDateSource.TaxRecordPostDate ? expectedDate : ZDate.Empty;
			var realisationDate = dateSource == GLMovementDateSource.TaxRecordRealisationDate ? expectedDate : ZDate.Empty;

			var taxRecord = CreateTaxRecord(TaxBasisList.Posting.Code, postDate, realisationDate, 20, LedgerControlAccount, TaxControlAccount, TaxExpenseAccount, null);
			AssertExceptionThrown<TaxFrameworkUnknownConfigurationValueException>("", "'TTT' tax configuration has conflicting combination of GL Accounts.", () => glMovementCreator.CreateNormalRecord(taxRecord, dateSource));
			DeleteGLMovements(taxRecord);

			taxRecord.ATT_AG_TaxControlAccount = ZGuid.Empty;
			glMovementCreator.CreateNormalRecord(taxRecord, dateSource);
			var glMovements = LoadGLMovements(taxRecord);
			AssertEquals("Created GLMovement count for tax record with correct number of GL accounts", 1, glMovements.Length);
			AssertGLMovement(nameof(taxRecord), glMovements[0], TaxGLMovementTypeList.Normal.Code, taxRecord.PK, LedgerControlAccount, TaxExpenseAccount, 20, expectedDate);

			var taxRecord2 = CreateTaxRecord(TaxBasisList.Posting.Code, postDate, realisationDate, -20, LedgerControlAccount, null, TaxExpenseAccount, TaxPendingControlAccount);
			AssertExceptionThrown<TaxFrameworkUnknownConfigurationValueException>("", "'TTT' tax configuration has conflicting combination of GL Accounts.", () => glMovementCreator.CreateNormalRecord(taxRecord2, dateSource));
			DeleteGLMovements(taxRecord2);

			taxRecord2.ATT_AG_TaxPendingControlAccount = ZGuid.Empty;
			glMovementCreator.CreateNormalRecord(taxRecord2, dateSource);
			glMovements = LoadGLMovements(taxRecord2);
			AssertEquals($"Created GLMovement count for {nameof(taxRecord2)}", 1, glMovements.Length);
			AssertGLMovement(nameof(taxRecord2), glMovements[0], TaxGLMovementTypeList.Normal.Code, taxRecord2.PK, TaxExpenseAccount, LedgerControlAccount, 20, expectedDate);
		}

		public void TestCreateNormalRecord_TaxControlAndTaxExpenseAccounts_PostDate()
		{
			AssertCreateNormalRecord_TaxControlAndTaxExpenseAccounts(GLMovementDateSource.TaxRecordPostDate);
		}
		public void TestCreateNormalRecord_TaxControlAndTaxExpenseAccounts_RealisationDate()
		{
			AssertCreateNormalRecord_TaxControlAndTaxExpenseAccounts(GLMovementDateSource.TaxRecordRealisationDate);
		}

		void AssertCreateNormalRecord_TaxControlAndTaxExpenseAccounts(GLMovementDateSource dateSource)
		{
			var glMovementCreator = (IGLMovementCreator)new GLMovementCreator();
			var expectedDate = ZDate.Today.AddDays(-5);
			var postDate = dateSource == GLMovementDateSource.TaxRecordPostDate ? expectedDate : ZDate.Empty;
			var realisationDate = dateSource == GLMovementDateSource.TaxRecordRealisationDate ? expectedDate : ZDate.Empty;

			var taxRecord = CreateTaxRecord(TaxBasisList.Posting.Code, postDate, realisationDate, 30, LedgerControlAccount, TaxControlAccount, TaxExpenseAccount, null);
			AssertExceptionThrown<TaxFrameworkUnknownConfigurationValueException>("", "'TTT' tax configuration has conflicting combination of GL Accounts.", () => glMovementCreator.CreateNormalRecord(taxRecord, dateSource));
			DeleteGLMovements(taxRecord);

			taxRecord.ATT_AG_LedgerControlAccount = ZGuid.Empty;
			glMovementCreator.CreateNormalRecord(taxRecord, dateSource);
			var glMovements = LoadGLMovements(taxRecord);
			AssertEquals("Created GLMovement count for tax record with correct number of GL accounts", 1, glMovements.Length);
			AssertGLMovement(nameof(taxRecord), glMovements[0], TaxGLMovementTypeList.Normal.Code, taxRecord.PK, TaxControlAccount, TaxExpenseAccount, 30, expectedDate);

			var taxRecord2 = CreateTaxRecord(TaxBasisList.Posting.Code, postDate, realisationDate, -30, null, TaxControlAccount, TaxExpenseAccount, TaxPendingControlAccount);
			AssertExceptionThrown<TaxFrameworkUnknownConfigurationValueException>("", "'TTT' tax configuration has conflicting combination of GL Accounts.", () => glMovementCreator.CreateNormalRecord(taxRecord2, dateSource));
			DeleteGLMovements(taxRecord2);

			taxRecord2.ATT_AG_TaxPendingControlAccount = ZGuid.Empty;
			glMovementCreator.CreateNormalRecord(taxRecord2, dateSource);
			glMovements = LoadGLMovements(taxRecord2);
			AssertEquals($"Created GLMovement count for {nameof(taxRecord2)}", 1, glMovements.Length);
			AssertGLMovement(nameof(taxRecord2), glMovements[0], TaxGLMovementTypeList.Normal.Code, taxRecord2.PK, TaxExpenseAccount, TaxControlAccount, 30, expectedDate);
		}

#endregion

		#region CreatePendingRecord

		public void TestCreatePendingRecord_NoReleaseDate_NotInDB()
		{
			var glMovementCreator = (IGLMovementCreator)new GLMovementCreator();
			var expectedDate = ZDate.Today.AddDays(-10);
			var taxRecord = CreateTaxRecord(TaxBasisList.Matching.Code, expectedDate, ZDate.Empty, 100, LedgerControlAccount, TaxControlAccount, TaxExpenseAccount, TaxPendingControlAccount);
			AssertExceptionThrown<TaxFrameworkUnknownConfigurationValueException>("", "'TTT' tax configuration has conflicting combination of GL Accounts.", () => glMovementCreator.CreatePendingRecord(taxRecord));
			DeleteGLMovements(taxRecord);

			taxRecord.ATT_AG_TaxExpenseAccount = ZGuid.Empty;
			glMovementCreator.CreatePendingRecord(taxRecord);
			var glMovements = LoadGLMovements(taxRecord);
			AssertEquals("Created GLMovement count for tax record with correct number of GL accounts", 1, glMovements.Length);
			AssertGLMovement(nameof(taxRecord), glMovements[0], TaxGLMovementTypeList.Pending.Code, taxRecord.PK, LedgerControlAccount, TaxPendingControlAccount, 100, expectedDate);

			var taxRecord2 = CreateTaxRecord(TaxBasisList.Matching.Code, expectedDate, ZDate.Empty, -100, LedgerControlAccount, TaxControlAccount, null, TaxPendingControlAccount);
			glMovementCreator.CreatePendingRecord(taxRecord2);
			glMovements = LoadGLMovements(taxRecord2);
			AssertEquals($"Created GLMovement count for {nameof(taxRecord2)}", 1, glMovements.Length);
			AssertGLMovement(nameof(taxRecord2), glMovements[0], TaxGLMovementTypeList.Pending.Code, taxRecord2.PK, TaxPendingControlAccount, LedgerControlAccount, 100, expectedDate);
		}

		public void TestCreatePendingRecord_WithReleaseDate_NotInDB()
		{
			var expectedPostDate = ZDate.Today.AddDays(-11);
			var expectedRealiseDate = expectedPostDate.AddDays(3);
			var taxRecord = CreateTaxRecord(TaxBasisList.Matching.Code, expectedPostDate, expectedRealiseDate, 200, LedgerControlAccount, TaxControlAccount, null, TaxPendingControlAccount);
			var glMovementCreator = (IGLMovementCreator)new GLMovementCreator();
			glMovementCreator.CreatePendingRecord(taxRecord);
			var glMovements = LoadGLMovements(taxRecord);
			AssertEquals("Created GLMovement count for tax record with correct number of GL accounts", 1, glMovements.Length);
			var glMovement = glMovements[0];
			AssertEquals("ATM_Type of created GL movements", TaxGLMovementTypeList.Pending.Code, glMovement.ATM_Type);
			AssertGLMovement(nameof(taxRecord) + " Pending", glMovement, TaxGLMovementTypeList.Pending.Code, taxRecord.PK, LedgerControlAccount, TaxPendingControlAccount, 200, expectedPostDate);

			var taxRecord2 = CreateTaxRecord(TaxBasisList.Matching.Code, expectedPostDate, expectedRealiseDate, -200, LedgerControlAccount, TaxControlAccount, null, TaxPendingControlAccount);
			glMovementCreator.CreatePendingRecord(taxRecord2);
			glMovements = LoadGLMovements(taxRecord2);
			AssertEquals($"Created GLMovement count for {nameof(taxRecord2)}", 1, glMovements.Length);
			glMovement = glMovements[0];
			AssertEquals($"ATM_Type of created GL movements for {nameof(taxRecord2)}", TaxGLMovementTypeList.Pending.Code, glMovement.ATM_Type);
			AssertGLMovement(nameof(taxRecord) + " Pending", glMovement, TaxGLMovementTypeList.Pending.Code, taxRecord2.PK, TaxPendingControlAccount, LedgerControlAccount, 200, expectedPostDate);
		}

		public void TestCreateRealisedRecord_WithReleaseDate_NotInDB()
		{
			var expectedPostDate = ZDate.Today.AddDays(-11);
			var expectedRealiseDate = expectedPostDate.AddDays(3);
			var taxRecord = CreateTaxRecord(TaxBasisList.Matching.Code, expectedPostDate, expectedRealiseDate, 200, LedgerControlAccount, TaxControlAccount, null, TaxPendingControlAccount);
			var glMovementCreator = (IGLMovementCreator)new GLMovementCreator();
			glMovementCreator.CreateRealisedRecord(taxRecord);
			var glMovements = LoadGLMovements(taxRecord);
			AssertEquals("Created GLMovement count for tax record with correct number of GL accounts", 1, glMovements.Length);
			var glMovement = glMovements[0];
			AssertEquals("ATM_Type of created GL movements", TaxGLMovementTypeList.Realised.Code, glMovement.ATM_Type);
			AssertGLMovement(nameof(taxRecord) + " Realised", glMovement, TaxGLMovementTypeList.Realised.Code, taxRecord.PK, TaxPendingControlAccount, TaxControlAccount, 200, expectedRealiseDate);

			var taxRecord2 = CreateTaxRecord(TaxBasisList.Matching.Code, expectedPostDate, expectedRealiseDate, -200, LedgerControlAccount, TaxControlAccount, null, TaxPendingControlAccount);
			glMovementCreator.CreateRealisedRecord(taxRecord2);
			glMovements = LoadGLMovements(taxRecord2);
			AssertEquals($"Created GLMovement count for {nameof(taxRecord2)}", 1, glMovements.Length);
			glMovement = glMovements[0];
			AssertEquals($"ATM_Type of created GL movements for {nameof(taxRecord2)}", TaxGLMovementTypeList.Realised.Code, glMovement.ATM_Type);
			AssertGLMovement(nameof(taxRecord) + " Realised", glMovement, TaxGLMovementTypeList.Realised.Code, taxRecord2.PK, TaxControlAccount, TaxPendingControlAccount, 200, expectedRealiseDate);
		}

		#endregion

		#region Setup and helpers

		AccTaxTransaction CreateTaxRecord(string basis, ZDate postDate, ZDate realisationDate, ZDecimal amount, AccGLHeader ledgerControlAccount, AccGLHeader taxControlAccount, AccGLHeader taxExpenseAccount, AccGLHeader taxPendingControlAccount)
		{
			var invoice = TestObjectCreator.CreateInvoice(typeof(APInvoice), TestObjectCreator.AUD, 1M, TestObjectCreator.AALSHI);
			var taxRecord = Factory.NewWithValidTestData<AccTaxTransaction>();
			taxRecord.ATT_AH = invoice.PK;
			taxRecord.TaxConfiguration.ETC_Code = "TTT";
			taxRecord.ATT_Basis = basis;
			taxRecord.ATT_PostDate = postDate;
			taxRecord.ATT_RealisationDate = realisationDate;
			taxRecord.ATT_LocalTaxAmount = amount;
			taxRecord.ATT_AG_LedgerControlAccount = (ledgerControlAccount?.PK).GetValueOrDefault();
			taxRecord.ATT_AG_TaxControlAccount = (taxControlAccount?.PK).GetValueOrDefault();
			taxRecord.ATT_AG_TaxExpenseAccount = (taxExpenseAccount?.PK).GetValueOrDefault();
			taxRecord.ATT_AG_TaxPendingControlAccount = (taxPendingControlAccount?.PK).GetValueOrDefault();

			return taxRecord;
		}

		static void AssertGLMovement(string message, AccTaxGLMovement glMovement, ZString type, ZGuid taxRecordPK, AccGLHeader debitAccount, AccGLHeader creditAccount, ZDecimal amount, ZDate date)
		{
			CombineAssertions(() =>
			{
				AssertEquals(message + ": ATM_Type", type, glMovement.ATM_Type);
				AssertEquals(message + ": ATM_ATT_TaxTransaction", taxRecordPK, glMovement.ATM_ATT_TaxTransaction);
				AssertEquals(message + ": ATM_AG_DebitAccount", debitAccount.AG_AccountNum, (glMovement.DebitAccount?.AG_AccountNum).GetValueOrDefault());
				AssertEquals(message + ": ATM_AG_CreditAccount", creditAccount.AG_AccountNum, (glMovement.CreditAccount?.AG_AccountNum).GetValueOrDefault());
				AssertEquals(message + ": ATM_Amount", amount, glMovement.ATM_Amount);
				AssertEquals(message + ": ATM_Date", date, glMovement.ATM_Date);
			});
		}

		AccTaxGLMovement[] LoadGLMovements(AccTaxTransaction taxRecord) => Factory.Load<AccTaxGLMovement>(new ZQuery(AccTaxGLMovementSchema.ATM_ATT_TaxTransaction, taxRecord.PK) { FetchOnlyFromLocalCache = true });

		void DeleteGLMovements(AccTaxTransaction taxRecord) => LoadGLMovements(taxRecord).ForEach(x => x.Delete());

		protected TestObjectCreator TestObjectCreator
		{
			get { return testObjectCreator ?? (testObjectCreator = new TestObjectCreator(Factory)); }
		}
		TestObjectCreator testObjectCreator;

		protected override void SetUp()
		{
			base.SetUp();

			TestObjectCreator.CreateTestPeriods(ZDateTime.Today.AddMonths(-6));

			LedgerControlAccount = Factory.NewWithValidTestData<AccGLHeader>();
			LedgerControlAccount.AG_AccountNum = "LedgerCtrl";
			TaxControlAccount = Factory.NewWithValidTestData<AccGLHeader>();
			TaxControlAccount.AG_AccountNum = "TaxControl";
			TaxPendingControlAccount = Factory.NewWithValidTestData<AccGLHeader>();
			TaxPendingControlAccount.AG_AccountNum = "TaxPndCtrl";
			TaxExpenseAccount = Factory.NewWithValidTestData<AccGLHeader>();
			TaxExpenseAccount.AG_AccountNum = "TaxExpense";
			Factory.Save();
		}

		AccGLHeader LedgerControlAccount;
		AccGLHeader TaxControlAccount;
		AccGLHeader TaxPendingControlAccount;
		AccGLHeader TaxExpenseAccount;

		#endregion
	}
}
