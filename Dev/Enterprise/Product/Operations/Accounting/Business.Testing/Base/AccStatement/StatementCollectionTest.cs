using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.Base.AccStatement.Testing
{
	[TestedType(typeof(StatementCollection))]
	public class StatementCollectionTest : BusinessObjectCollectionTestCase
	{
		public void TestInstantitation()
		{
			ZGuid bankPK1 = InsertBank(1);

			BusinessObjectFactory testFactory = new BusinessObjectFactory();
			BankStatement testBank = testFactory.Load(typeof(BankStatement), bankPK1) as BankStatement;

			StatementCollection testCollection = new StatementCollection(testBank, testFactory);
			AssertNotNull(testCollection);
		}

		public void TestCollectionAddNew()
		{
			ZGuid bankPK1 = InsertBank(1);

			BusinessObjectFactory testFactory = new BusinessObjectFactory();
			BankStatement testBank = testFactory.Load(typeof(BankStatement), bankPK1) as BankStatement;

			StatementCollection testCollection = new StatementCollection(testBank, testFactory);
			testCollection.AddNew();

			AssertEquals(1, testCollection.Count);
		}

		public void TestCollection()
		{
			ZGuid bankPK1 = InsertBank(1);

			BusinessObjectFactory testFactory = new BusinessObjectFactory();
			BankStatement testBank = testFactory.Load(typeof(BankStatement), bankPK1) as BankStatement;

			StatementCollection testCollection = new StatementCollection(testBank, testFactory);

			Statement statement1 = testCollection.AddNew();
			Statement statement2 = testCollection.AddNew();

			AssertEquals(statement1, testCollection[0]);
			AssertEquals(statement2, testCollection[1]);
		}

		public void TestOnRemovingOnRemoved()
		{
			ZGuid bankPK1 = InsertBank(1);

			BusinessObjectFactory testFactory = new BusinessObjectFactory();
			BankStatement testBank = testFactory.Load(typeof(BankStatement), bankPK1) as BankStatement;

			Statement statement1 = testBank.AddNewStatement();
			Statement statement2 = testBank.AddNewStatement();
			Statement statement3 = testBank.AddNewStatement();
			Statement statement4 = testBank.AddNewStatement();
			Statement statement5 = testBank.AddNewStatement();
			Statement statement6 = testBank.AddNewStatement();

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

			testBank.GetStatements_ForTestOnly().Remove(statement1.PK);
			testBank.CurrentPageFilter = 0;
			testBank.ApplyFilter(true);
			AssertEquals("150.00 (2 Transactions)", testBank.PageDebitTotal);
			AssertEquals("370.00 (3 Transactions)", testBank.PageCreditTotal);
			AssertEquals("150.00 (2 Transactions)", testBank.DebitTotal);
			AssertEquals("370.00 (3 Transactions)", testBank.CreditTotal);
			AssertEquals(220m, testBank.BalanceAmount);
			AssertEquals(220m, testBank.PageBalanceAmount);

			Statement statement7 = testBank.AddNewStatement();
			statement7.AS_PageNumber = 2;
			statement7.AS_DebitCredit = Statement.DEBIT;
			statement7.AS_Amount = 100d;
			AssertEquals("250.00 (3 Transactions)", testBank.PageDebitTotal);
			AssertEquals("370.00 (3 Transactions)", testBank.PageCreditTotal);
			AssertEquals("250.00 (3 Transactions)", testBank.DebitTotal);
			AssertEquals("370.00 (3 Transactions)", testBank.CreditTotal);
			AssertEquals(120m, testBank.BalanceAmount);
			AssertEquals(120m, testBank.PageBalanceAmount);
			testBank.GetStatements_ForTestOnly().Remove(statement7.PK);

			testBank.CurrentPageFilter = 0;
			testBank.ApplyFilter(true);
			AssertEquals("150.00 (2 Transactions)", testBank.PageDebitTotal);
			AssertEquals("370.00 (3 Transactions)", testBank.PageCreditTotal);
			AssertEquals("150.00 (2 Transactions)", testBank.DebitTotal);
			AssertEquals("370.00 (3 Transactions)", testBank.CreditTotal);
			AssertEquals(220m, testBank.BalanceAmount);
			AssertEquals(220m, testBank.PageBalanceAmount);
		}

		public void TestPageNo()
		{
			ZGuid bankPK1 = InsertBank(1);

			BusinessObjectFactory testFactory = new BusinessObjectFactory();
			BankStatement testBank = testFactory.Load(typeof(BankStatement), bankPK1) as BankStatement;

			StatementCollection testCollection = new StatementCollection(testBank, testFactory);

			testCollection.AddNew(typeof(Statement));

			AssertEquals((short)1, testCollection[0].AS_PageNumber);

			testCollection.AddNew(typeof(Statement));
			AssertEquals((short)1, testCollection[1].AS_PageNumber);

			testCollection[1].AS_PageNumber = (ZShort)2;

			testCollection.AddNew(typeof(Statement));
			AssertEquals((short)2, testCollection[2].AS_PageNumber);
		}

		public void TestSetDefaultStatementDateForNewChild()
		{
			ZGuid bankPK1 = InsertBank(1);

			BusinessObjectFactory testFactory = new BusinessObjectFactory();
			BankStatement testBank = testFactory.Load(typeof(BankStatement), bankPK1) as BankStatement;

			StatementCollection testCollection = new StatementCollection(testBank, testFactory);

			testCollection.AddNew(typeof(Statement));
			AssertEquals("AS_StatementDate", testBank.AB_LastStatementDate, testCollection[0].AS_StatementDate);
		}

		#region Implementation

		protected ZGuid InsertBank(int gLCount)
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
			testBankAccount.AB_LastReconcileDate = new ZDateTime(2006, 11, 20);
			testBankAccount.AB_LastStatementDate = new ZDateTime(2006, 11, 21);

			testFactory.Save();
			return testBankAccount.PK;
		}

		protected string GetRandomString(int stringLength)
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

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new StatementCollection(Factory.New(typeof(BankStatement)) as BankStatement, Factory);
		}

		#endregion
	}
}
