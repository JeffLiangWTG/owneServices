using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.CashBook;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.Base.AccStatement.Testing
{
	[TestedType(typeof(BankStatement))]
	public class BankStatement_InnerTest : EnterpriseBusinessObjectTestCase
	{
		public void TestZDecimalsHaveCorrectDecimalPlacesBankStatement()
		{
			var statement = Factory.New<BankStatement>();
			AssertNotNull("Company should not be null", statement.Company);

			var osList = new List<string>
				{
					nameof(statement.StatementBalance),
					nameof(statement.OpeningStatementBalance),
					nameof(statement.BalanceAmount),
					nameof(statement.PageBalanceAmount),
					nameof(statement.AmountFilter),
					nameof(statement.AB_StatementBalance)
				};

			var tester = new DecimalPlacesAttributeTester(statement, statement.Company);
			tester.CheckNonLocalCurrency(osList, nameof(statement.OSCurrencyDecimals), nameof(statement.AB_RX_NKAccountCurrency), statement);
		}

		public override void TestSaveAndDeleteBusinessObject()
		{
			Assert("This should be implemented if a client has an issue with deleting", true);
		}

		public void TestClearDirectTransactionsOnFactorySaved()
		{
			BankStatement testStatment = Factory.NewWithValidTestData<BankStatement>();
			BankReconDirectPayment payment = Factory.NewWithValidTestData<BankReconDirectPayment>();
			testStatment.DirectTransactions.Headers.Add(payment);
			AssertEquals(1, testStatment.DirectTransactions.Headers.Count);
			Factory.Save();
			AssertEquals(0, testStatment.DirectTransactions.Headers.Count);
		}

		public void TestStatementGetterShouldReturnEmptyCollection()
		{
			TestCaseHelper.ClearTable(AutoAccBankAccount.Schema.TableName);
			InsertBank(1);
			var query = new ZDBOnlyQuery(typeof(BankStatement));
			var bankStatement = Factory.LoadTop1<BankStatement>(query);
			PrepareTestStatements(bankStatement);

			Factory.Save();

			var newBankStatement = new BusinessObjectFactory().LoadTop1<BankStatement>(query);
			AssertEquals("Should have 0 statements when just created.", 0, newBankStatement.Statements_ForTestOnly.Count);
			AssertEquals("Should have 0 unreconciled statements when just created.", 0, newBankStatement.UnreconciledStatements.Count);
			AssertEquals("Should have 0 reconciled statements when just created.", 0, newBankStatement.ReconciledStatements.Count);

			newBankStatement.Statements_ForTestOnly.Load();
			AssertEquals("Should have 2 statements after loaded.", 2, newBankStatement.Statements_ForTestOnly.Count);
			AssertEquals("Should have 1 unreconciled statements after loaded.", 1, newBankStatement.UnreconciledStatements.Count);
			AssertEquals("Should have 1 reconciled statements after loaded.", 1, newBankStatement.ReconciledStatements.Count);
		}

		void PrepareTestStatements(BankStatement bankStatement)
		{
			bankStatement.Statements_ForTestOnly.AddNew();
			bankStatement.Statements_ForTestOnly.AddNew();

			bankStatement.Statements_ForTestOnly[0].AS_DebitCredit = Core.Constants.DebitCredit.Debit;
			bankStatement.Statements_ForTestOnly[1].AS_DebitCredit = Core.Constants.DebitCredit.Credit;

			bankStatement.Statements_ForTestOnly[0].AS_Amount = 111.11m;
			bankStatement.Statements_ForTestOnly[1].AS_Amount = 222.22m;

			bankStatement.Statements_ForTestOnly[0].AS_StatementDate = new ZDateTime(2004, 3, 2);
			bankStatement.Statements_ForTestOnly[1].AS_StatementDate = new ZDateTime(2004, 2, 24);

			bankStatement.Statements_ForTestOnly[0].AS_ChequeOrReference = "A1";
			bankStatement.Statements_ForTestOnly[1].AS_ChequeOrReference = "A2";

			bankStatement.Statements_ForTestOnly[0].AS_Type = "CHQ";
			bankStatement.Statements_ForTestOnly[1].AS_Type = "RCB";

			bankStatement.Statements_ForTestOnly[0].IsCleared = true;
			bankStatement.Statements_ForTestOnly[1].IsCleared = false;
		}

		public void TestInstantiation()
		{
			BusinessObjectFactory testFactory = new BusinessObjectFactory();
			BankStatement testStatment = testFactory.New(typeof(BankStatement)) as BankStatement;

			AssertNotNull(testStatment);
		}

		public void TestCalculateTotalsAfterStatementDeleted()
		{
			ZGuid bankPK1 = InsertBank(1);

			BusinessObjectFactory testFactory = new BusinessObjectFactory();
			BankStatement testBank = testFactory.Load(typeof(BankStatement), bankPK1) as BankStatement;

			StatementCollection testCollection = testBank.Statements_ForTestOnly;

			Statement statement1 = testCollection.AddNew();
			Statement statement2 = testCollection.AddNew();
			Statement statement3 = testCollection.AddNew();
			Statement statement4 = testCollection.AddNew();
			Statement statement5 = testCollection.AddNew();
			Statement statement6 = testCollection.AddNew();

			statement1.AS_DebitCredit = Statement.DEBIT;
			statement1.AS_PageNumber = 1;
			statement1.AS_Amount = 230d;

			statement2.AS_DebitCredit = Statement.CREDIT;
			statement2.AS_PageNumber = 1;
			statement2.AS_Amount = 150d;

			statement3.AS_DebitCredit = Statement.DEBIT;
			statement3.AS_PageNumber = 2;
			statement3.AS_Amount = 100d;

			statement4.AS_DebitCredit = Statement.CREDIT;
			statement4.AS_PageNumber = 2;
			statement4.AS_Amount = 150d;

			statement5.AS_DebitCredit = Statement.DEBIT;
			statement5.AS_PageNumber = 3;
			statement5.AS_Amount = 50d;

			statement6.AS_DebitCredit = Statement.CREDIT;
			statement6.AS_PageNumber = 3;
			statement6.AS_Amount = 70d;
			testFactory.Save();

			testBank.CurrentPageFilter = 0;
			testBank.ApplyFilter(true);
			AssertEquals("380.00 (3 Transactions)", testBank.PageDebitTotal);
			AssertEquals("370.00 (3 Transactions)", testBank.PageCreditTotal);
			AssertEquals("380.00 (3 Transactions)", testBank.DebitTotal);
			AssertEquals("370.00 (3 Transactions)", testBank.CreditTotal);
			AssertEquals(-10m, testBank.PageBalanceAmount);
			AssertEquals(-10m, testBank.BalanceAmount);

			testBank.CurrentPageFilter = 1;
			testBank.ApplyFilter(true);
			AssertEquals("230.00 (1 Transactions)", testBank.PageDebitTotal);
			AssertEquals("150.00 (1 Transactions)", testBank.PageCreditTotal);
			AssertEquals("380.00 (3 Transactions)", testBank.DebitTotal);
			AssertEquals("370.00 (3 Transactions)", testBank.CreditTotal);
			AssertEquals(-10m, testBank.BalanceAmount);
			AssertEquals(-80m, testBank.PageBalanceAmount);

			testCollection.Remove(statement1.PK);
			testBank.CurrentPageFilter = 0;
			testBank.ApplyFilter(true);
			AssertEquals("150.00 (2 Transactions)", testBank.PageDebitTotal);
			AssertEquals("370.00 (3 Transactions)", testBank.PageCreditTotal);
			AssertEquals("150.00 (2 Transactions)", testBank.DebitTotal);
			AssertEquals("370.00 (3 Transactions)", testBank.CreditTotal);
			AssertEquals(220m, testBank.BalanceAmount);
			AssertEquals(220m, testBank.PageBalanceAmount);

			Statement statement7 = testCollection.AddNew();
			statement7.AS_PageNumber = 2;
			statement7.AS_DebitCredit = Statement.DEBIT;
			statement7.AS_Amount = 100d;
			AssertEquals("250.00 (3 Transactions)", testBank.PageDebitTotal);
			AssertEquals("370.00 (3 Transactions)", testBank.PageCreditTotal);
			AssertEquals("250.00 (3 Transactions)", testBank.DebitTotal);
			AssertEquals("370.00 (3 Transactions)", testBank.CreditTotal);
			AssertEquals(120m, testBank.BalanceAmount);
			AssertEquals(120m, testBank.PageBalanceAmount);
			testCollection.Remove(statement7);

			testBank.CurrentPageFilter = 0;
			testBank.ApplyFilter(true);
			AssertEquals("150.00 (2 Transactions)", testBank.PageDebitTotal);
			AssertEquals("370.00 (3 Transactions)", testBank.PageCreditTotal);
			AssertEquals("150.00 (2 Transactions)", testBank.DebitTotal);
			AssertEquals("370.00 (3 Transactions)", testBank.CreditTotal);
			AssertEquals(220m, testBank.BalanceAmount);
			AssertEquals(220m, testBank.PageBalanceAmount);
		}

		public void TestDirectTransactionsBizObj()
		{
			BankStatement bankStatement = Factory.New<BankStatement>();
			bankStatement.AB_LastStatementDate = new ZDateTime(2006, 10, 11);
			AssertNotNull("DirectTransactions", bankStatement.DirectTransactions);
			AssertEquals("DirectTransactions.StatementDate", bankStatement.AB_LastStatementDate, bankStatement.DirectTransactions.StatementDate);
		}

		public void TestStatementCollection()
		{
			BankStatement testStatment = Factory.New<BankStatement>();
			AssertNotNull("Statements", testStatment.Statements_ForTestOnly);

			testStatment.Statements_ForTestOnly.AddNew();
			AssertEquals("Statements.Count", 1, testStatment.Statements_ForTestOnly.Count);
			AssertEquals("PK.IsValid", true, testStatment.Statements_ForTestOnly[0].PK.IsValid);
			AssertEquals("AS_AB", testStatment.PK, testStatment.Statements_ForTestOnly[0].AS_AB);
		}

		public void TestUnreconciledStatementsCollection()
		{
			BankStatement bankStatement = Factory.New<BankStatement>();
			AssertNotNull("UnreconciledStatements", bankStatement.UnreconciledStatements);

			bankStatement.Statements_ForTestOnly.AddNew();
			AssertEquals("UnreconciledStatements.Count", 1, bankStatement.UnreconciledStatements.Count);
			AssertEquals("AS_AB", bankStatement.PK, bankStatement.UnreconciledStatements[0].AS_AB);
			AssertEquals("AS_IsCleared", false, bankStatement.UnreconciledStatements[0].AS_IsCleared);

			bankStatement.Statements_ForTestOnly[0].AS_IsCleared = true;
			AssertEquals("UnreconciledStatements.Count", 0, bankStatement.UnreconciledStatements.Count);
		}

		public void TestReconciledStatementsCollection()
		{
			BankStatement bankStatement = Factory.New<BankStatement>();
			AssertNotNull("ReconciledStatements", bankStatement.ReconciledStatements);

			bankStatement.Statements_ForTestOnly.AddNew();
			AssertEquals("ReconciledStatements.Count", 0, bankStatement.ReconciledStatements.Count);

			bankStatement.Statements_ForTestOnly[0].AS_IsCleared = true;
			AssertEquals("ReconciledStatements.Count", 1, bankStatement.ReconciledStatements.Count);
			AssertEquals("AS_AB", bankStatement.PK, bankStatement.ReconciledStatements[0].AS_AB);
			AssertEquals("AS_IsCleared", true, bankStatement.ReconciledStatements[0].AS_IsCleared);
		}

		#region TestShouldCreateDirectTransactionForStatement

		public void TestShouldCreateDirectTransactionForStatement()
		{
			BankStatement bankStatement = Factory.New<BankStatement>();
			bankStatement.StatementTypeChanged += new StatementEventHandler(StatementTypeChanged);
			try
			{
				Statement statement = bankStatement.Statements_ForTestOnly.AddNew();
				ShouldCreateDirectTransaction = false;
				AssertEquals("ShouldCreateDirectTransactionForStatement", false, bankStatement.ShouldCreateDirectTransactionForStatement(statement));

				ShouldCreateDirectTransaction = true;
				AssertEquals("ShouldCreateDirectTransactionForStatement", true, bankStatement.ShouldCreateDirectTransactionForStatement(statement));
			}
			finally
			{
				bankStatement.StatementTypeChanged -= new StatementEventHandler(StatementTypeChanged);
			}
		}

		void StatementTypeChanged(object sender, StatementEventArgs args)
		{
			args.Result = ShouldCreateDirectTransaction;
		}

		bool ShouldCreateDirectTransaction;

		#endregion

		#region TestShowStatementDirectTransaction

		public void TestShowStatementDirectTransaction()
		{
			BankStatement bankStatement = Factory.New<BankStatement>();
			bankStatement.StatementDirectTransactionCreated += new StatementEventHandler(StatementDirectTransactionCreated);
			try
			{
				Statement statement = bankStatement.Statements_ForTestOnly.AddNew();
				IsStatementDirectTransactionShown = false;
				bankStatement.ShowStatementDirectTransaction(statement);
				AssertEquals("StatementDirectTransaction is shown", true, IsStatementDirectTransactionShown);
			}
			finally
			{
				bankStatement.StatementDirectTransactionCreated -= new StatementEventHandler(StatementDirectTransactionCreated);
			}
		}

		void StatementDirectTransactionCreated(object sender, StatementEventArgs args)
		{
			IsStatementDirectTransactionShown = true;
		}

		bool IsStatementDirectTransactionShown;

		#endregion

		public void TestGetNumbeOfClearedStatements()
		{
			BankStatement testStatement = Factory.New<BankStatement>();

			testStatement.Statements_ForTestOnly.AddNew();
			testStatement.Statements_ForTestOnly.AddNew();
			testStatement.Statements_ForTestOnly.AddNew();
			testStatement.Statements_ForTestOnly.AddNew();

			testStatement.Statements_ForTestOnly[1].AS_IsCleared = true;
			testStatement.Statements_ForTestOnly[3].AS_IsCleared = true;

			AssertEquals(2, testStatement.ReconciledStatements.Count);
		}

		public void TestDebitTotal()
		{
			BusinessObjectFactory testFactory = new BusinessObjectFactory();
			BankStatement testStatement = testFactory.New(typeof(BankStatement)) as BankStatement;

			testStatement.AB_LastStatementDate = new ZDateTime(2004, 1, 1);

			testStatement.Statements_ForTestOnly.AddNew();
			testStatement.Statements_ForTestOnly.AddNew();
			testStatement.Statements_ForTestOnly.AddNew();
			testStatement.Statements_ForTestOnly.AddNew();
			testStatement.Statements_ForTestOnly.AddNew();

			testStatement.Statements_ForTestOnly[0].AS_DebitCredit = Core.Constants.DebitCredit.Debit;
			testStatement.Statements_ForTestOnly[1].AS_DebitCredit = Core.Constants.DebitCredit.Credit;
			testStatement.Statements_ForTestOnly[2].AS_DebitCredit = Core.Constants.DebitCredit.Debit;
			testStatement.Statements_ForTestOnly[3].AS_DebitCredit = Core.Constants.DebitCredit.Debit;
			testStatement.Statements_ForTestOnly[4].AS_DebitCredit = Core.Constants.DebitCredit.Credit;

			testStatement.Statements_ForTestOnly[0].AS_Amount = 111.11m;
			testStatement.Statements_ForTestOnly[1].AS_Amount = 222.22m;
			testStatement.Statements_ForTestOnly[2].AS_Amount = 123.45m;
			testStatement.Statements_ForTestOnly[3].AS_Amount = 0.0m;
			testStatement.Statements_ForTestOnly[4].AS_Amount = 47.65m;

			testStatement.CalculateTotals();

			AssertEquals(new ZString("234.56 (2 Transactions)"), testStatement.DebitTotal);
			AssertEquals(new ZString("269.87 (2 Transactions)"), testStatement.CreditTotal);
			AssertEquals(35.31m, testStatement.BalanceAmount);
		}

		public void TestAB_Type_List()
		{
			BankStatement bankStatement = Factory.New<BankStatement>();
			AssertNotNull("AB_Type_List", bankStatement.AB_Type_List);
			AssertEquals("AB_Type_List.Count", 17, bankStatement.AB_Type_List.Count);
		}

		[ExpectException(typeof(BankStatementException))]
		public void TestStatementDateFilter()
		{
			BankStatement testStatement = GetTestStatementForFilterTesting();
			testStatement.StatementDateFilter = new ZDateTime(2004, 1, 4);
			testStatement.ApplyFilter(true);
			AssertEquals(0, testStatement.Statements_ForTestOnly.Count);

			testStatement.StatementDateFilter = new ZDateTime(2004, 3, 2);
			testStatement.ApplyFilter(true);
			AssertEquals(2, testStatement.Statements_ForTestOnly.Count);

			testStatement.StatementDateFilter = new ZDateTime(2003, 2, 24);
			testStatement.ApplyFilter(true);
			AssertEquals(1, testStatement.Statements_ForTestOnly.Count);

			testStatement.ClearFilter();
			AssertEquals(0, testStatement.Statements_ForTestOnly.Count);
		}

		public void TestAmountFilter()
		{
			BankStatement testStatement = GetTestStatementForFilterTesting();

			testStatement.ApplyFilter(true);
			AssertEquals(5, testStatement.Statements_ForTestOnly.Count);

			testStatement.AmountFilter = 123.45m;
			testStatement.ApplyFilter(true);
			AssertEquals(1, testStatement.Statements_ForTestOnly.Count);

			testStatement.AmountFilter = 222.22m;
			testStatement.ApplyFilter(true);
			AssertEquals(2, testStatement.Statements_ForTestOnly.Count);
		}

		public void TestDebitCreditFilter()
		{
			BankStatement testStatement = GetTestStatementForFilterTesting();

			testStatement.DebitCreditFilter = Core.Constants.DebitCredit.Credit;
			testStatement.ApplyFilter(true);
			AssertEquals(2, testStatement.Statements_ForTestOnly.Count);

			testStatement.DebitCreditFilter = Core.Constants.DebitCredit.Debit;
			testStatement.ApplyFilter(true);
			AssertEquals(3, testStatement.Statements_ForTestOnly.Count);

			testStatement.DebitCreditFilter = "";
			testStatement.ApplyFilter(true);
			AssertEquals(5, testStatement.Statements_ForTestOnly.Count);
		}

		public void TestTypeFilter()
		{
			BankStatement testStatement = GetTestStatementForFilterTesting();

			testStatement.TypeFilter = "CHQ";
			testStatement.ApplyFilter(true);
			AssertEquals(2, testStatement.Statements_ForTestOnly.Count);

			testStatement.TypeFilter = "RCB";
			testStatement.ApplyFilter(true);
			AssertEquals(2, testStatement.Statements_ForTestOnly.Count);

			testStatement.TypeFilter = "TRF";
			testStatement.ApplyFilter(true);
			AssertEquals(1, testStatement.Statements_ForTestOnly.Count);
		}

		public void TestCurrentPageFilter()
		{
			BankStatement testStatement = GetTestStatementForFilterTesting();
			testStatement.ApplyFilter(true);
			AssertEquals(5, testStatement.Statements_ForTestOnly.Count);

			testStatement.CurrentPageFilter = (ZShort)2;
			testStatement.ApplyFilter(true);
			AssertEquals(2, testStatement.Statements_ForTestOnly.Count);
		}

		public void TestChequeReferenceFilter()
		{
			BankStatement testStatement = GetTestStatementForFilterTesting();

			testStatement.ChequeReferenceFilter = "A";
			testStatement.ApplyFilter(true);
			AssertEquals(2, testStatement.Statements_ForTestOnly.Count);

			testStatement.ChequeReferenceFilter = "B";
			testStatement.ApplyFilter(true);
			AssertEquals(1, testStatement.Statements_ForTestOnly.Count);

			testStatement.ChequeReferenceFilter = "1";
			testStatement.ApplyFilter(true);
			AssertEquals(3, testStatement.Statements_ForTestOnly.Count);
		}

		[ExpectException(typeof(BankStatementException))]
		public void TestCombinedFilter()
		{
			BankStatement testStatement = GetTestStatementForFilterTesting();

			testStatement.AmountFilter = 123.45m;
			testStatement.DebitCreditFilter = Core.Constants.DebitCredit.Debit;
			testStatement.TypeFilter = "TRF";
			testStatement.CurrentPageFilter = 2;
			testStatement.ChequeReferenceFilter = "1";

			testStatement.ApplyFilter(true);
			AssertEquals(1, testStatement.Statements_ForTestOnly.Count);

			testStatement.ChequeReferenceFilter = "12";
			testStatement.ApplyFilter(true);
			AssertEquals(0, testStatement.Statements_ForTestOnly.Count);

			testStatement.ClearFilter();
			AssertEquals(0, testStatement.Statements_ForTestOnly.Count);
			Assert(testStatement.StatementDateFilter.IsEmpty);
			Assert(testStatement.AmountFilter.IsEmpty);
			Assert(testStatement.DebitCreditFilter.IsEmpty);
			Assert(testStatement.TypeFilter.IsEmpty);
			Assert(testStatement.CurrentPageFilter.IsEmpty);
			Assert(testStatement.ChequeReferenceFilter.IsEmpty);
		}

		[ExpectExceptionMessage(typeof(BankStatementException), "Please save statements before filtering.")]
		public void TestValidateFilter_BankStatementNotSaved()
		{
			BankStatement bankStatement = Factory.New<BankStatement>();
			bankStatement.Statements_ForTestOnly.AddNew();
			bankStatement.Statements_ForTestOnly[0].AS_DebitCredit = Statement.DEBIT;
			bankStatement.ApplyFilter();
		}

		[ExpectExceptionMessage(typeof(BankStatementException), "There are errors. Please fix them before filtering.")]
		public void TestValidateFilter_ThereAreErrors()
		{
			BankStatement bankStatement = Factory.NewWithValidTestData<BankStatement>();
			bankStatement.StatementDateFilter = ZDateTime.Empty;
			Factory.Save();
			bankStatement.ApplyFilter();
		}

		public void TestDefaultDate()
		{
			BusinessObjectFactory testFactory = new BusinessObjectFactory();
			BankStatement testStatement = testFactory.New(typeof(BankStatement)) as BankStatement;

			testStatement.AB_LastStatementDate = new ZDateTime(2003, 1, 1);

			testStatement.Statements_ForTestOnly.AddNew();
			testStatement.Statements_ForTestOnly.AddNew();

			AssertEquals(testStatement.AB_LastStatementDate, testStatement.Statements_ForTestOnly[0].AS_StatementDate);
			AssertEquals(testStatement.AB_LastStatementDate, testStatement.Statements_ForTestOnly[1].AS_StatementDate);

			testStatement.Statements_ForTestOnly[0].AS_StatementDate = new ZDateTime(2004, 3, 2);
			testStatement.Statements_ForTestOnly[1].AS_StatementDate = new ZDateTime(2004, 2, 24);

			testStatement = testFactory.Load(typeof(BankStatement), testStatement.PK) as BankStatement;

			AssertEquals(new ZDateTime(2004, 3, 2), testStatement.Statements_ForTestOnly[0].AS_StatementDate);
			AssertEquals(new ZDateTime(2004, 2, 24), testStatement.Statements_ForTestOnly[1].AS_StatementDate);

			testStatement.Statements_ForTestOnly.AddNew();
			AssertEquals(testStatement.AB_LastStatementDate, testStatement.Statements_ForTestOnly[2].AS_StatementDate);
		}

		public void TestGetMaxNo()
		{
			TestCaseHelper.ClearTable(Statement.Schema.TableName);

			InsertBank(1);

			BankStatement testStatement = Factory.LoadTop1(typeof(BankStatement), new ZQuery()) as BankStatement;

			testStatement.Statements_ForTestOnly.AddNew();
			testStatement.Statements_ForTestOnly.AddNew();

			testStatement.Statements_ForTestOnly[0].AS_DebitCredit = Core.Constants.DebitCredit.Debit;
			testStatement.Statements_ForTestOnly[1].AS_DebitCredit = Core.Constants.DebitCredit.Credit;

			testStatement.Statements_ForTestOnly[0].AS_Amount = 111.11m;
			testStatement.Statements_ForTestOnly[1].AS_Amount = 222.22m;

			testStatement.Statements_ForTestOnly[0].AS_StatementDate = new ZDateTime(2004, 3, 2);
			testStatement.Statements_ForTestOnly[1].AS_StatementDate = new ZDateTime(2004, 2, 24);

			testStatement.Statements_ForTestOnly[0].AS_ChequeOrReference = "A1";
			testStatement.Statements_ForTestOnly[1].AS_ChequeOrReference = "A2";

			testStatement.Statements_ForTestOnly[0].AS_Type = "CHQ";
			testStatement.Statements_ForTestOnly[1].AS_Type = "RCB";

			Factory.Save();

			testStatement.Statements_ForTestOnly[0].AS_Sequence = 2;
			testStatement.Statements_ForTestOnly[1].AS_Sequence = 4;

			Factory.Save();

			AssertEquals(3, testStatement.GetMaxSequenceNo_ForTestOnly(new ZDateTime(2004, 3, 2)));
			AssertEquals(5, testStatement.GetMaxSequenceNo_ForTestOnly(new ZDateTime(2004, 2, 24)));
			AssertEquals(0, testStatement.GetMaxSequenceNo_ForTestOnly(new ZDateTime(2005, 2, 24)));
		}

		public void TestSetSequenceNo()
		{
			TestCaseHelper.ClearTable(Statement.Schema.TableName);

			InsertBank(1);

			BankStatement testStatement = Factory.LoadTop1(typeof(BankStatement), new ZQuery()) as BankStatement;

			testStatement.Statements_ForTestOnly.AddNew();
			testStatement.Statements_ForTestOnly.AddNew();

			testStatement.Statements_ForTestOnly[0].AS_DebitCredit = Core.Constants.DebitCredit.Debit;
			testStatement.Statements_ForTestOnly[1].AS_DebitCredit = Core.Constants.DebitCredit.Credit;

			testStatement.Statements_ForTestOnly[0].AS_Amount = 111.11m;
			testStatement.Statements_ForTestOnly[1].AS_Amount = 222.22m;

			testStatement.Statements_ForTestOnly[0].AS_StatementDate = new ZDateTime(2004, 3, 2);
			testStatement.Statements_ForTestOnly[1].AS_StatementDate = new ZDateTime(2004, 2, 24);

			testStatement.Statements_ForTestOnly[0].AS_ChequeOrReference = "A1";
			testStatement.Statements_ForTestOnly[1].AS_ChequeOrReference = "A2";

			testStatement.Statements_ForTestOnly[0].AS_Type = "CHQ";
			testStatement.Statements_ForTestOnly[1].AS_Type = "RCB";

			Factory.Save();

			testStatement.Statements_ForTestOnly[0].AS_Sequence = 2;
			testStatement.Statements_ForTestOnly[1].AS_Sequence = 4;

			Factory.Save();

			testStatement.Statements_ForTestOnly.AddNew();
			testStatement.Statements_ForTestOnly.AddNew();

			testStatement.AB_LastStatementDate = new ZDateTime(2004, 3, 2);
			testStatement.Statements_ForTestOnly[2].AS_StatementDate = new ZDateTime(2004, 3, 2);
			testStatement.Statements_ForTestOnly[3].AS_StatementDate = new ZDateTime(2004, 3, 2);
			testStatement.SetSequenceNo_ForTestOnly();

			AssertEquals(3, testStatement.Statements_ForTestOnly[2].AS_Sequence);
			AssertEquals(4, testStatement.Statements_ForTestOnly[3].AS_Sequence);

			Factory.Save();

			testStatement.AB_LastStatementDate = new ZDateTime(2004, 3, 5);
			testStatement.Statements_ForTestOnly.AddNew();
			testStatement.Statements_ForTestOnly.AddNew();

			testStatement.Statements_ForTestOnly[4].AS_StatementDate = new ZDateTime(2004, 3, 5);
			testStatement.Statements_ForTestOnly[5].AS_StatementDate = new ZDateTime(2004, 3, 5);
			testStatement.SetSequenceNo_ForTestOnly();

			AssertEquals(0, testStatement.Statements_ForTestOnly[4].AS_Sequence);
			AssertEquals(1, testStatement.Statements_ForTestOnly[5].AS_Sequence);
		}

		public void TestGetMaxPage()
		{
			BankStatement testBankStatement = GetTestStatementForFilterTesting();

			// Max Page only works for the date filtered collections
			AssertEquals((short)2, testBankStatement.MaxPage);

			testBankStatement.StatementDateFilter = new ZDateTime(2004, 3, 2);
			AssertEquals((short)2, testBankStatement.MaxPage);
		}

		public void TestIsPageExistForThisCollection()
		{
			BankStatement testBankStatement = GetTestStatementForFilterTesting();

			testBankStatement.StatementDateFilter = new ZDateTime(2004, 3, 2);
			Assert(testBankStatement.IsPageExistForThisCollection(0));

			Assert(testBankStatement.IsPageExistForThisCollection(2));
			Assert(!testBankStatement.IsPageExistForThisCollection(3));
		}

		protected override bool IsDeleteSupported()
		{
			return false;
		}

		string GetRandomString(int stringLength)
		{
			Random generator = new Random();

			string result = "";

			for (int i = 0; i < stringLength; i++)
			{
				int index = generator.Next(65, 90);
				result += (char)index;
			}

			return result;
		}

		BankStatement GetTestStatementForFilterTesting()
		{
			BankStatement testStatement = Factory.New<BankStatement>();

			testStatement.Statements_ForTestOnly.AddNew();
			testStatement.Statements_ForTestOnly[0].AS_StatementDate = new ZDateTime(2004, 3, 2);
			testStatement.Statements_ForTestOnly[0].AS_Amount = 111.11m;
			testStatement.Statements_ForTestOnly[0].AS_DebitCredit = Core.Constants.DebitCredit.Debit;
			testStatement.Statements_ForTestOnly[0].AS_Type = "CHQ";
			testStatement.Statements_ForTestOnly[0].AS_PageNumber = (ZShort)1;
			testStatement.Statements_ForTestOnly[0].AS_ChequeOrReference = "A1";

			testStatement.Statements_ForTestOnly.AddNew();
			testStatement.Statements_ForTestOnly[1].AS_StatementDate = new ZDateTime(2004, 2, 24);
			testStatement.Statements_ForTestOnly[1].AS_Amount = 222.22m;
			testStatement.Statements_ForTestOnly[1].AS_DebitCredit = Core.Constants.DebitCredit.Credit;
			testStatement.Statements_ForTestOnly[1].AS_Type = "RCB";
			testStatement.Statements_ForTestOnly[1].AS_PageNumber = (ZShort)1;
			testStatement.Statements_ForTestOnly[1].AS_ChequeOrReference = "A2";

			testStatement.Statements_ForTestOnly.AddNew();
			testStatement.Statements_ForTestOnly[2].AS_StatementDate = new ZDateTime(2004, 3, 2);
			testStatement.Statements_ForTestOnly[2].AS_Amount = 123.45m;
			testStatement.Statements_ForTestOnly[2].AS_DebitCredit = Core.Constants.DebitCredit.Debit;
			testStatement.Statements_ForTestOnly[2].AS_Type = "TRF";
			testStatement.Statements_ForTestOnly[2].AS_PageNumber = (ZShort)2;
			testStatement.Statements_ForTestOnly[2].AS_ChequeOrReference = "B1";

			testStatement.Statements_ForTestOnly.AddNew();
			testStatement.Statements_ForTestOnly[3].AS_StatementDate = new ZDateTime(2003, 2, 24);
			testStatement.Statements_ForTestOnly[3].AS_Amount = 0.0m;
			testStatement.Statements_ForTestOnly[3].AS_DebitCredit = Core.Constants.DebitCredit.Debit;
			testStatement.Statements_ForTestOnly[3].AS_Type = "RCB";
			testStatement.Statements_ForTestOnly[3].AS_PageNumber = (ZShort)2;
			testStatement.Statements_ForTestOnly[3].AS_ChequeOrReference = "C2";

			testStatement.Statements_ForTestOnly.AddNew();
			testStatement.Statements_ForTestOnly[4].AS_StatementDate = new ZDateTime(2003, 12, 14);
			testStatement.Statements_ForTestOnly[4].AS_Amount = 222.22m;
			testStatement.Statements_ForTestOnly[4].AS_DebitCredit = Core.Constants.DebitCredit.Credit;
			testStatement.Statements_ForTestOnly[4].AS_Type = "CHQ";
			testStatement.Statements_ForTestOnly[4].AS_PageNumber = (ZShort)1;
			testStatement.Statements_ForTestOnly[4].AS_ChequeOrReference = "D1";

			return testStatement;
		}

		ZGuid InsertBank(int gLCount)
		{
			BusinessObjectFactory testFactory = new BusinessObjectFactory();
			AccBankAccount testBankAccount = testFactory.New(typeof(AccBankAccount)) as AccBankAccount;

			testBankAccount.AB_AccountNum = GetRandomString(10);

			testFactory.LoadTop1(typeof(AccGLHeader), new ZQuery());

			testBankAccount.AB_AG = (testFactory.LoadTop1<AccGLHeader>(new ZQuery())).PK;
			testBankAccount.AB_RX_NKAccountCurrency = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;
			testBankAccount.AB_GB = GlbBranch.CurrentBranch.PK;
			testBankAccount.AB_GC = GlbCompany.CurrentCompany.PK;
			testBankAccount.AB_Code = GetRandomString(10);
			testBankAccount.AB_Desc = GetRandomString(10);
			testBankAccount.AB_LastReconcileDate = Env.Time.CurrentLocalDate;
			testBankAccount.AB_LastStatementDate = testBankAccount.AB_LastReconcileDate;

			testFactory.Save();
			return testBankAccount.PK;
		}
	}
}
