using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.CashBook;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.Base.AccStatement.Testing
{
	[TestedType(typeof(Statement))]
	public class StatementTest : EnterpriseBusinessObjectTestCase
	{
		public void TestZDecimalsHaveCorrectDecimalPlacesStatement()
		{
			var statement = Factory.New<Statement>();
			var account = Factory.New<AccBankAccount>();
			statement.AS_AB = account.PK;

			var osList = new List<string>
			{
				nameof(statement.Debit),
				nameof(statement.Credit),
				nameof(statement.AS_Amount),
				nameof(statement.StatementDebit),
				nameof(statement.StatementCredit)
			};

			var tester = new DecimalPlacesAttributeTester(statement);
			tester.CheckNonLocalCurrency(osList, nameof(statement.OSCurrencyDecimals), nameof(statement.BankAccount.AB_RX_NKAccountCurrency), statement.BankAccount);
		}

		public void TestAS_Type_List()
		{
			Statement statement = Factory.New<Statement>();
			AssertNotNull("AS_Type_List", statement.Lookups.AS_Type_List);
			AssertEquals("AS_Type_List.Count", 18, statement.Lookups.AS_Type_List.Count);
		}

		public void TestBankChargeTypes_List()
		{
			Statement statement = Factory.New<Statement>();
			AssertNotNull("BankChargeTypes_List", statement.Lookups.BankChargeTypes_List);
			AssertEquals("LookupEditType", OLookUpEditType.BankChargeTypes, statement.Lookups.BankChargeTypes_List.LookupEditType);
		}

		public void TestAS_Type()
		{
			ShouldCreateDirectTransaction = true;

			BankStatement bankStatement = Factory.New<BankStatement>();
			bankStatement.StatementTypeChanged += new StatementEventHandler(StatementTypeChanged);
			try
			{
				Statement statement = bankStatement.AddNewStatement();
				statement.AS_Type = ZArchitecture.Core.ReceiptTypes.Cheque;
				AssertNull("DirectTransaction", statement.DirectTransaction);

				statement.AS_Type = ZArchitecture.Core.ReceiptTypes.AccountMaintenanceFee;
				AssertNotNull("DirectTransaction", statement.DirectTransaction);
				AssertEquals("AH_ReceiptType", ZArchitecture.Core.ReceiptTypes.AccountMaintenanceFee, statement.DirectTransaction.AH_ReceiptType);

				statement.AS_Type = ZArchitecture.Core.ReceiptTypes.InterestPaid;
				AssertNotNull("DirectTransaction", statement.DirectTransaction);
				AssertEquals("AH_ReceiptType", ZArchitecture.Core.ReceiptTypes.InterestPaid, statement.DirectTransaction.AH_ReceiptType);

				statement.AS_Type = ZArchitecture.Core.ReceiptTypes.CreditCard;
				AssertNull("DirectTransaction", statement.DirectTransaction);
			}
			finally
			{
				bankStatement.StatementTypeChanged -= new StatementEventHandler(StatementTypeChanged);
			}
		}

		public void TestAS_Amount()
		{
			ShouldCreateDirectTransaction = true;

			BankStatement bankStatement = Factory.New<BankStatement>();
			bankStatement.StatementTypeChanged += new StatementEventHandler(StatementTypeChanged);
			try
			{
				Statement statement = bankStatement.AddNewStatement();
				statement.AS_DebitCredit = Statement.CREDIT;
				statement.AS_Type = ZArchitecture.Core.ReceiptTypes.AccountMaintenanceFee;
				AssertNotNull("DirectTransaction", statement.DirectTransaction);

				statement.AS_Amount = 100.00m;
				AssertEquals("AH_OSTotal", 100.00m, statement.DirectTransaction.AH_OSTotal);

				statement.AS_Amount = 200.00m;
				AssertEquals("AH_OSTotal", 200.00m, statement.DirectTransaction.AH_OSTotal);
			}
			finally
			{
				bankStatement.StatementTypeChanged -= new StatementEventHandler(StatementTypeChanged);
			}
		}

		public void TestAS_ChequeOrReference()
		{
			ShouldCreateDirectTransaction = true;

			BankStatement bankStatement = Factory.New<BankStatement>();
			bankStatement.StatementTypeChanged += new StatementEventHandler(StatementTypeChanged);
			try
			{
				Statement statement = bankStatement.AddNewStatement();
				statement.AS_Type = ZArchitecture.Core.ReceiptTypes.AccountMaintenanceFee;
				AssertNotNull("DirectTransaction", statement.DirectTransaction);

				statement.AS_ChequeOrReference = "REF1";
				AssertEquals("AH_OSTotal", "REF1", statement.DirectTransaction.AH_ChequeOrReference);

				statement.AS_ChequeOrReference = "REF2";
				AssertEquals("AH_OSTotal", "REF2", statement.DirectTransaction.AH_ChequeOrReference);
			}
			finally
			{
				bankStatement.StatementTypeChanged -= new StatementEventHandler(StatementTypeChanged);
			}
		}

		public void TestAS_StatementDateReadonly()
		{
			BankStatement bankStatement = Factory.New<BankStatement>();
			AssertEquals("Precondition: Default Value for registry setting", false, AccountingConfigurationRegistry.Instance.AllowManualEntryOfStatementDateWhenEnteringBankStatement.Value);

			Statement statement = bankStatement.AddNewStatement();
			AssertEquals("Statement date should be readonly by default", true, statement.AS_StatementDateInfo.ReadOnly);

			using (AccountingConfigurationRegistry.Instance.AllowManualEntryOfStatementDateWhenEnteringBankStatement.SetTemporaryValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, true))
			{
				AssertEquals("Statement date should be editable when the registry has been overridden", false, statement.AS_StatementDateInfo.ReadOnly);
			}
		}

		public void TestDelete()
		{
			ShouldCreateDirectTransaction = true;

			BankStatement bankStatement = Factory.New<BankStatement>();
			bankStatement.StatementTypeChanged += new StatementEventHandler(StatementTypeChanged);
			try
			{
				Statement statement = bankStatement.AddNewStatement();
				statement.AS_Type = ZArchitecture.Core.ReceiptTypes.AccountMaintenanceFee;
				DirectTransactionHeaderBase directTransaction = statement.DirectTransaction;
				AssertNotNull("DirectTransaction", directTransaction);

				statement.Delete();
				AssertEquals("DirectTransaction is deleted", true, directTransaction.IsDeleted);
			}
			finally
			{
				bankStatement.StatementTypeChanged -= new StatementEventHandler(StatementTypeChanged);
			}
		}

		public void TestHandleDirectTransaction()
		{
			ShouldCreateDirectTransaction = true;
			AccountingConfigurationRegistry.Instance.BankTransactionGLAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, Guid.NewGuid());

			BankStatement bankStatement = Factory.NewWithValidTestData<BankStatement>();
			bankStatement.AB_RX_NKAccountCurrency = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;
			bankStatement.StatementTypeChanged += new StatementEventHandler(StatementTypeChanged);
			try
			{
				bankStatement.AB_LastReconcileDate = ZDateTime.Now;
				Statement statement = bankStatement.AddNewStatement();
				statement.AS_DebitCredit = Statement.CREDIT;
				statement.AS_ChequeOrReference = "REF1";
				statement.AS_Amount = 123.45m;
				statement.AS_Type = ZArchitecture.Core.ReceiptTypes.AccountMaintenanceFee;

				DirectTransactionHeaderBase directTransaction = statement.DirectTransaction;
				AssertNotNull("DirectTransaction", directTransaction);
				AssertEquals("Type of DirectTransaction", typeof(BankReconDirectReceipt), directTransaction.GetType());
				AssertEquals("RelatedStatementPK", statement.PK, directTransaction.RelatedStatementPK);
				AssertEquals("AH_InvoiceDate", statement.AS_StatementDate, directTransaction.AH_InvoiceDate);
				AssertEquals("AH_AB", statement.AS_AB, directTransaction.AH_AB);
				AssertEquals("AH_ReceiptType", statement.AS_Type, directTransaction.AH_ReceiptType);
				AssertEquals("AH_ChequeOrReference", statement.AS_ChequeOrReference, directTransaction.AH_ChequeOrReference);
				AssertEquals("AH_PostDate", statement.AS_StatementDate, directTransaction.AH_PostDate);
				AssertEquals("AH_ChequeDrawer", "", directTransaction.AH_ChequeDrawer);

				AssertEquals("AH_RX_NKTransactionCurrencyInfo.ReadOnly", true, directTransaction.AH_RX_NKTransactionCurrencyInfo.ReadOnly);
				AssertEquals("AH_ExchangeRateInfo.ReadOnly", true, directTransaction.AH_ExchangeRateInfo.ReadOnly);
				AssertEquals("AH_ChequeDrawerInfo.ReadOnly", true, directTransaction.AH_ChequeDrawerInfo.ReadOnly);

				AssertEquals("Lines.Count", 1, directTransaction.Lines.Count);
				DirectTransactionLineBase line = (DirectTransactionLineBase)directTransaction.Lines[0];
				AssertEquals("AL_AG", AccountingConfigurationRegistry.Instance.BankTransactionGLAccount.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty), line.AL_AG);
				AssertEquals("AL_AGInfo.ReadOnly", true, line.AL_AGInfo.ReadOnly);
				AssertEquals("AL_OSExTaxAmount", statement.AS_Amount, line.AL_OSExTaxAmount);
				AssertEquals("AT_Code", "NOTREPORT", line.TaxRate.AT_Code);

				AssertNotNull("RelatedDepositBatch", ((BankReconDirectReceipt)directTransaction).RelatedDepositBatch);

				AssertEquals("Master should contain DirectTransaction", true, bankStatement.DirectTransactions.Headers.Contains(directTransaction));
			}
			finally
			{
				bankStatement.StatementTypeChanged -= new StatementEventHandler(StatementTypeChanged);
			}
		}

		public void TestIsImportingData()
		{
			ShouldCreateDirectTransaction = true;
			AccountingConfigurationRegistry.Instance.BankTransactionGLAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, Guid.NewGuid());

			BankStatement bankStatement = Factory.NewWithValidTestData<BankStatement>();
			bankStatement.AB_RX_NKAccountCurrency = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;
			bankStatement.AB_LastReconcileDate = ZDateTime.Now;
			Statement statement = bankStatement.AddNewStatement();
			statement.AS_DebitCredit = Statement.CREDIT;
			statement.AS_ChequeOrReference = "REF1";
			statement.AS_Amount = 123.45m;

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			statement.IsImportingData = true;
			statement.AS_Type = ZArchitecture.Core.ReceiptTypes.AccountMaintenanceFee;

			Assert("Should not have popped up any messages because system is importing", UnitTestUserNotification.Instance.LastMessage.WasNone);
			DirectTransactionHeaderBase directTransaction = statement.DirectTransaction;
			AssertNotNull("Should have created direct transaction without popping up question to user", directTransaction);
			Assert("Statement must be of type ISupportDataImporting", statement is ISupportDataImporting);
		}

		public void TestDirectTransaction()
		{
			ShouldCreateDirectTransaction = true;

			Statement statement = Factory.New<Statement>();
			statement.AS_Type = ZArchitecture.Core.ReceiptTypes.Cheque;
			AssertNull("DirectTransaction", statement.DirectTransaction);

			statement.AS_Type = ZArchitecture.Core.ReceiptTypes.AccountMaintenanceFee;
			AssertNull("DirectTransaction", statement.DirectTransaction);

			BankStatement bankStatement = Factory.New<BankStatement>();
			bankStatement.StatementTypeChanged += new StatementEventHandler(StatementTypeChanged);
			try
			{
				bankStatement.GetStatements_ForTestOnly().Add(statement);
				AssertNull("DirectTransaction", statement.DirectTransaction);

				statement.AS_Type = ZArchitecture.Core.ReceiptTypes.InterestPaid;
				AssertNotNull("DirectTransaction", statement.DirectTransaction);
			}
			finally
			{
				bankStatement.StatementTypeChanged -= new StatementEventHandler(StatementTypeChanged);
			}
		}

		public void TestShouldCreateDirectTransaction()
		{
			Statement statement = Factory.New<Statement>();
			ShouldCreateDirectTransaction = false;
			AssertEquals("ShouldCreateDirectTransaction", false, statement.ShouldCreateDirectTransaction);

			ShouldCreateDirectTransaction = true;
			AssertEquals("ShouldCreateDirectTransaction", false, statement.ShouldCreateDirectTransaction);

			BankStatement bankStatement = Factory.New<BankStatement>();
			bankStatement.StatementTypeChanged += new StatementEventHandler(StatementTypeChanged);
			try
			{
				bankStatement.GetStatements_ForTestOnly().Add(statement);
				ShouldCreateDirectTransaction = false;
				AssertEquals("ShouldCreateDirectTransaction", false, statement.ShouldCreateDirectTransaction);

				ShouldCreateDirectTransaction = true;
				AssertEquals("ShouldCreateDirectTransaction", true, statement.ShouldCreateDirectTransaction);
			}
			finally
			{
				bankStatement.StatementTypeChanged -= new StatementEventHandler(StatementTypeChanged);
			}
		}

		public void TestShowStatementDirectTransaction()
		{
			Statement statement = Factory.New<Statement>();
			IsDirectTransactionShown = false;
			statement.ShowDirectTransaction();
			AssertEquals("DirectTransaction is not shown", false, IsDirectTransactionShown);

			BankStatement bankStatement = Factory.New<BankStatement>();
			bankStatement.StatementDirectTransactionCreated += new StatementEventHandler(StatementDirectTransactionCreated);
			try
			{
				bankStatement.GetStatements_ForTestOnly().Add(statement);
				IsDirectTransactionShown = false;
				statement.ShowDirectTransaction();
				AssertEquals("DirectTransaction is shown", true, IsDirectTransactionShown);
			}
			finally
			{
				bankStatement.StatementDirectTransactionCreated -= new StatementEventHandler(StatementDirectTransactionCreated);
			}
		}

		public void TestCheckConstant()
		{
			Statement testStatment = (Statement)GetNewBusinessObject();

			AssertNotNull(testStatment.Lookups.AS_DebitCredit_List.GetDescriptionFromCode(Statement.DEBIT));
			AssertNotNull(testStatment.Lookups.AS_DebitCredit_List.GetDescriptionFromCode(Statement.CREDIT));
		}

		public void TestStatementDebitCredit()
		{
			Statement testStatment = (Statement)GetNewBusinessObject();
			testStatment.AS_DebitCredit = Statement.DEBIT;
			testStatment.AS_Amount = 100.0m;

			AssertEquals(100.0m, testStatment.StatementDebit);
			AssertEquals(0m, testStatment.StatementCredit);

			testStatment.AS_DebitCredit = Statement.CREDIT;

			AssertEquals(0m, testStatment.StatementDebit);
			AssertEquals(100.0m, testStatment.StatementCredit);
		}

		public void TestClearedDateTime()
		{
			ZDateTime clearedDate = new ZDateTime(2006, 2, 14);
			Statement testStatment = (Statement)GetNewBusinessObject();
			testStatment.AS_StatementDate = clearedDate;

			testStatment.IsCleared = ZBool.False;
			AssertEquals(ZDateTime.Empty, testStatment.ClearedDate);

			testStatment.IsCleared = ZBool.True;
			AssertEquals(clearedDate, testStatment.ClearedDate);
		}

		public void TestIsCleared_UpdatesBankRecClearedInCurrentSession()
		{
			var masterBankRecon = new BankReconciliation(Factory);
			masterBankRecon.StatementDate = ZDateTime.Now;
			var statement = (Statement)GetNewBusinessObject();
			statement.MasterBankRecon = masterBankRecon;
			AssertEquals("Precondition: IsCleared not set.", false, statement.IsCleared);
			AssertEquals("Precondition: Nothing marked as cleared", 0, masterBankRecon.TransactionIdsClearedInCurrentSession.Count);
			AssertEquals("Precondition: Nothing marked as uncleared", 0, masterBankRecon.TransactionIdsUnclearedInCurrentSession.Count);

			statement.IsCleared = true;
			AssertCollectionContains("Statement is marked as cleared by PK.", statement.PK, masterBankRecon.TransactionIdsClearedInCurrentSession);

			statement.IsCleared = false;
			AssertCollectionNotContains("Statement is unmarked as cleared by PK.", statement.PK, masterBankRecon.TransactionIdsClearedInCurrentSession);
			AssertCollectionNotContains("Statement is not uncleared by PK.", statement.PK, masterBankRecon.TransactionIdsUnclearedInCurrentSession);

			statement.IsCleared = true;
			AssertCollectionContains("Statement is marked as cleared by PK.", statement.PK, masterBankRecon.TransactionIdsClearedInCurrentSession);
			AssertCollectionNotContains("Statement is not uncleared by PK.", statement.PK, masterBankRecon.TransactionIdsUnclearedInCurrentSession);
		}

		public void TestIsCleared_UpdatesBankRecUnclearedInCurrentSession()
		{
			var masterBankRecon = new BankReconciliation(Factory);
			masterBankRecon.StatementDate = ZDateTime.Now;
			var statement = (Statement)GetNewBusinessObject();
			statement.IsCleared = true;
			statement.MasterBankRecon = masterBankRecon;
			AssertEquals("Precondition: IsCleared is set.", true, statement.IsCleared);
			AssertEquals("Precondition: Nothing marked as cleared", 0, masterBankRecon.TransactionIdsClearedInCurrentSession.Count);
			AssertEquals("Precondition: Nothing marked as uncleared", 0, masterBankRecon.TransactionIdsUnclearedInCurrentSession.Count);

			statement.IsCleared = false;
			AssertCollectionContains("Statement is marked as uncleared by PK.", statement.PK, masterBankRecon.TransactionIdsUnclearedInCurrentSession);

			statement.IsCleared = true;
			AssertCollectionNotContains("Statement is unmarked as checked by PK.", statement.PK, masterBankRecon.TransactionIdsUnclearedInCurrentSession);
			AssertCollectionNotContains("Statement is not cleared by PK.", statement.PK, masterBankRecon.TransactionIdsClearedInCurrentSession);

			statement.IsCleared = false;
			AssertCollectionContains("Statement is marked as uncleared by PK.", statement.PK, masterBankRecon.TransactionIdsUnclearedInCurrentSession);
			AssertCollectionNotContains("Statement is not cleared by PK.", statement.PK, masterBankRecon.TransactionIdsClearedInCurrentSession);
		}

		#region Implementation

		bool ShouldCreateDirectTransaction;
		bool IsDirectTransactionShown;

		void StatementTypeChanged(object sender, StatementEventArgs args)
		{
			args.Result = ShouldCreateDirectTransaction;
		}

		void StatementDirectTransactionCreated(object sender, StatementEventArgs args)
		{
			IsDirectTransactionShown = true;
		}

		#endregion
	}
}
