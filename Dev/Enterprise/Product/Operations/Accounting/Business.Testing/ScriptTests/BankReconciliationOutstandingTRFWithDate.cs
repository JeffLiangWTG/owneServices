using System;
using System.Data;
using System.Linq;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.Base.AccStatement;
using Enterprise.Accounting.Utility.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.Testing.ScriptTests
{
	class BankReconciliationOutstandingTRFWithDate : ScriptTest
	{
		[TestDate(2014, 07, 10)]
		public void TestStatements()
		{
			var statementTypes = new BusinessObjectFactory().New<Statement>().Lookups.AS_Type_List;
			var amountDifference = 4m;
			var amount = 10;
			foreach (CodeDescriptionPair pair in statementTypes)
			{
				SetStatement("DR", pair.Code, amount, "TEST1", TestBank, Factory);
				SetStatement("CR", pair.Code, amount + amountDifference, "TEST2", TestBank, Factory);
				amount += 10;
			}
			Factory.Save();

			var results = RunScript();
			var expectedResult = @"
Source    Date                    TransactionType TransactionNum       ReceiptType Reference                 Amount1               StatementAmount       BatchPayment                                                                                         Ledger BranchCode
--------- ----------------------- --------------- -------------------- ----------- ------------------------- --------------------- --------------------- ---------------------------------------------------------------------------------------------------- ------ ----------
STATEMENT 2014-07-10 00:00:00                                          TRF         TEST1                     0.00                  -90.00
STATEMENT 2014-07-10 00:00:00                                          TRF         TEST2                     0.00                  94.00
";
			AssertTableAsTextFromSQLServerManagenentStudio("", results, expectedResult, Enumerable.Empty<string>(), Enumerable.Empty<Tuple<ZGuid, string>>());
		}

		#region Implementation

		AccBankAccount TestBank
		{
			get
			{
				if (testBank == null)
				{
					testBank = new BusinessObjectFactory().New<AccBankAccount>();

					testBank.AB_AccountNum = "Bank123";
					testBank.AB_GC = GlbCompany.CurrentCompany.PK;
					testBank.AB_RX_NKAccountCurrency = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;
					testBank.AB_AG = Factory.LoadTop1(typeof(AccGLHeader), new ZQuery()).PK;
					testBank.AB_Code = "ABCBANK";
					testBank.Factory.Save();

					testBank = Factory.Load<AccBankAccount>(testBank.PK);
				}

				return testBank;
			}
		}
		AccBankAccount testBank;

		Statement SetStatement(string debitCredit, string type, decimal amount, string reference, AccBankAccount fBankAccount, BusinessObjectFactory factory)
		{
			Statement testStatment = factory.New(typeof(Statement)) as Statement;

			testStatment.AS_AB = fBankAccount.PK;
			testStatment.AS_DebitCredit = debitCredit;
			testStatment.AS_Type = type;
			testStatment.AS_Amount = amount;
			testStatment.AS_ChequeOrReference = reference;
			testStatment.AS_StatementDate = ZDateTime.Now;

			return testStatment;
		}

		DataTable RunScript()
		{
			return RunScript(TestBank, ZDateTime.Now.AddMinutes(1), ZDateTime.Now.AddMinutes(1));
		}

		DataTable RunScript(AccBankAccount bank, ZDateTime reconcileDate, ZDateTime satementDate)
		{
			var sql = string.Format(@"
						SELECT
							Source
							,Date
							,TransactionType
							,TransactionNum
							,ReceiptType
							,Reference
							,Amount1
							,StatementAmount
							,BatchPayment
							,Ledger
							,BranchCode
						FROM 
							BankReconciliationOutstandingTRFWithDate(
								'{0}' --@Bank 
								,'{1}' --@Company
								,'{2}' --@ReconcileDate
								,'{3}'  --@StatementDate
							)
						ORDER BY ReceiptType, Reference",
					bank.PK
					,GlbCompany.CurrentCompany.PK
					,reconcileDate.ToISO8601String()
					,satementDate.ToISO8601String()
				);
			return DataUtils.GetDataTableFromQuery(Db.Connection, sql);
		}

		#endregion
	}
}

