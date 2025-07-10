using System;
using System.Collections.Generic;
using System.Data;
using CargoWise.Data;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.TaxFramework.Business;
using Enterprise.Accounting.Utility.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;
using static Enterprise.Accounting.Utility.Testing.TaxFrameworkTestObjectCreator;

namespace Enterprise.Accounting.Business.Testing.ScriptTests.TaxFramework
{
	class Report_TaxAnalysisDetailedOtherTaxesTest : ScriptTest
	{
		[TestDate(2020, 06, 15)]
		[SuspendCriticalValidation]
		public void TestTaxAnalysisDetailed_Columns()
		{
			var companyABC = TestObjectCreator.CreateNewCompany("ABC");

			var taxSystem = TaxFrameworkTestObjectCreator.CreateTaxSystem("TS");
			var taxAuthority = TaxFrameworkTestObjectCreator.CreateTaxAuthority("NSW");
			var taxConfiguration = TaxFrameworkTestObjectCreator.CreateTaxConfiguration(companyABC, taxAuthority, taxSystem);
			var taxControlAccount = Factory.NewWithValidTestData<AccGLHeader>();
			var taxPendingControlAccount = Factory.NewWithValidTestData<AccGLHeader>();
			var taxExpenseAccount = Factory.NewWithValidTestData<AccGLHeader>();
			var taxMessage = TestObjectCreator.TaxMsg1;

			taxControlAccount.AG_AccountNum = "TaxControl";
			taxPendingControlAccount.AG_AccountNum = "TaxPndCtrl";
			taxExpenseAccount.AG_AccountNum = "TaxExpense";

			taxConfiguration.ETC_Code = "XXX";
			taxConfiguration.ETC_Description = "Test configuration";
			taxConfiguration.ETC_AG_TaxControlAccount = taxControlAccount.PK;
			taxConfiguration.ETC_AG_TaxExpenseAccount = taxExpenseAccount.PK;
			taxConfiguration.ETC_AG_TaxPendingControlAccount = taxPendingControlAccount.PK;

			var invoice = TestObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "011011", TestObjectCreator.AUD, 1M, 100M, 0M, 100M, 0M, TestObjectCreator.ABIGAS, TestObjectCreator.FRT.PK);
			invoice.AH_ComplianceSubType = "AST";
			invoice.AH_TransactionReference = "TRANREF";
			invoice.AH_FullyPaidDate = ZDateTime.Today;

			var taxTransaction1 = TaxFrameworkTestObjectCreator.CreateTaxTransaction(new CreateTaxTransactionParameters { Company = companyABC, Ledger = LedgerTypes.AccountsReceivable, LocalTaxAmount = 10m, RealisationDate = ZDate.Today, TaxConfiguration = taxConfiguration, TransactionHeader = invoice, DoesNotCreateGLMovemetsOnSaving = true });
			var taxTransaction2 = TaxFrameworkTestObjectCreator.CreateTaxTransaction(new CreateTaxTransactionParameters { Company = companyABC, Ledger = LedgerTypes.AccountsReceivable, LocalTaxAmount = 10m, RealisationDate = ZDate.Today, TaxConfiguration = taxConfiguration, TransactionHeader = invoice, DoesNotCreateGLMovemetsOnSaving = true });
			taxTransaction1.ATT_A9_TaxMessage = taxMessage.PK;
			taxTransaction2.ATT_A9_TaxMessage = taxMessage.PK;

			taxTransaction1.ATT_AH_MatchTransaction = invoice.PK;
			taxTransaction2.ATT_AH_MatchTransaction = invoice.PK;

			Factory.Save();

			var dataTable = RunScript(companyABC.PK, "ALL", "ALL");
			var dateOnlyColumns = new[] { "ATT_PostDate", "ATT_RealisationDate", "ATT_TaxDate" };
			var pkReplacment = new List<Tuple<ZGuid, string>>(new[] { new Tuple<ZGuid, string>(taxTransaction1.PK, "TaxTransaction1PK"), new Tuple<ZGuid, string>(taxTransaction2.PK, "TaxTransaction2PK") });
			var expectedResult = @"
ATT_PK                               ATT_TaxSuperType ATT_TaxSystemCode    ETC_Code                       ETC_TaxAuthorityCode ETC_Description                                                                                                                                                                                                                                                  ATT_Ledger ATT_Basis Branch ATT_GB                               Dept ATT_GE_Department                    ATT_PostDate          ATT_RealisationDate   IsRealized MatchTransactionNumber       ATT_TaxDate         TaxRate                      ATT_TaxAuthorityServiceCode AT_Code    A9_Code    A9_EnglishMsg                                                                                                                                                                                                                                                   ATT_RX_NKOSTaxCurrency ATT_OSTaxBaseAmount   ATT_LocalTaxBaseAmount ATT_OSTaxAmount       ATT_LocalTaxAmount    ATT_IsCancelled ATT_AffectsSourceTransactionTotal RealizedGL ExpenseGL  PendingGL  AH_Ledger AH_TransactionType AH_TransactionCategory AH_TransactionNum                      AH_ConsolidatedInvoiceRef              AH_ComplianceSubType AH_TransactionReference OH_Code      OH_FullName                                                                                          ParentTransactionBranch ParentTransactionDept AH_PostDate             AH_InvoiceDate          AH_DueDate              AH_FullyPaidDate        AH_Desc
------------------------------------ ---------------- -------------------- ------------------------------ -------------------- ---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- ---------- --------- ------ ------------------------------------ ---- ------------------------------------ --------------------- --------------------- ---------- ---------------------------- ------------------- ---------------------------- --------------------------- ---------- ---------- --------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- ---------------------- --------------------- ---------------------- --------------------- --------------------- --------------- --------------------------------- ---------- ---------- ---------- --------- ------------------ ---------------------- -------------------------------------- -------------------------------------- -------------------- ----------------------- ------------ ---------------------------------------------------------------------------------------------------- ----------------------- --------------------- ----------------------- ----------------------- ----------------------- ----------------------- --------------------------------------------------------------------------------------------------------------------------------
TaxTransaction1PK                    PER              TS                   XXX                            NSW                  Test configuration                                                                                                                                                                                                                                               AR         PST       BNE    27a55065-ac88-4ec3-8bed-e575e79172cb BRN  86bb1c22-0865-4685-996e-d56cbd136491 2020-06-15			 2020-06-15			   Y          00001000                     2020-06-15		   0.00                                                     WC0PBHU16S MSG1       English Msg 1                                                                                                                                                                                                                                                   AUD                    0.00                  0.00                   0.00                  10.00                 0               1                                 TaxControl TaxExpense TaxPndCtrl AR        INV                FIN                    00001000                                                                      AST					TRANREF					ABIGAS       ABI GAS & TOOLS                                                                                      BNE                     BRN                   2020-06-15 00:00:00     2020-06-15 00:00:00     2020-06-15 00:00:00     2020-06-15 00:00:00     Test Invoice
TaxTransaction2PK                    PER              TS                   XXX                            NSW                  Test configuration                                                                                                                                                                                                                                               AR         PST       BNE    27a55065-ac88-4ec3-8bed-e575e79172cb BRN  86bb1c22-0865-4685-996e-d56cbd136491 2020-06-15			 2020-06-15			   Y          00001000                     2020-06-15		   0.00                                                     H73CO36JKW MSG1       English Msg 1                                                                                                                                                                                                                                                   AUD                    0.00                  0.00                   0.00                  10.00                 0               1                                 TaxControl TaxExpense TaxPndCtrl AR        INV                FIN                    00001000                                                                      AST					TRANREF					ABIGAS       ABI GAS & TOOLS                                                                                      BNE                     BRN                   2020-06-15 00:00:00     2020-06-15 00:00:00     2020-06-15 00:00:00     2020-06-15 00:00:00     Test Invoice
";
			AssertTableAsTextFromSQLServerManagenentStudio("", dataTable, expectedResult, new List<string>() { }, dateOnlyColumns, pkReplacment);
		}

		[TestDate(2020, 06, 15)]
		[SuspendCriticalValidation]
		public void TestTaxAnalysisDetailed_Filters()
		{
			var companyABC = TestObjectCreator.CreateNewCompany("ABC");
			var companyXYZ = TestObjectCreator.CreateNewCompany("XYZ");
			var taxTransaction1 = TaxFrameworkTestObjectCreator.CreateTaxTransaction(new CreateTaxTransactionParameters { Company = companyABC, Ledger = LedgerTypes.AccountsReceivable, LocalTaxAmount = 10m, RealisationDate = ZDate.Empty, DoesNotCreateGLMovemetsOnSaving = true });
			var taxTransaction2 = TaxFrameworkTestObjectCreator.CreateTaxTransaction(new CreateTaxTransactionParameters { Company = companyABC, Ledger = LedgerTypes.AccountsReceivable, RealisationDate = ZDate.Today, DoesNotCreateGLMovemetsOnSaving = true });
			var taxTransaction3 = TaxFrameworkTestObjectCreator.CreateTaxTransaction(new CreateTaxTransactionParameters { Company = companyABC, Ledger = LedgerTypes.AccountsPayable, LocalTaxAmount = 10m, RealisationDate = ZDate.Empty, DoesNotCreateGLMovemetsOnSaving = true });
			var taxTransaction4 = TaxFrameworkTestObjectCreator.CreateTaxTransaction(new CreateTaxTransactionParameters { Company = companyABC, Ledger = LedgerTypes.AccountsPayable, RealisationDate = ZDate.Today, DoesNotCreateGLMovemetsOnSaving = true });

			var taxTransaction5 = TaxFrameworkTestObjectCreator.CreateTaxTransaction(new CreateTaxTransactionParameters { Company = companyXYZ, Ledger = LedgerTypes.AccountsReceivable, LocalTaxAmount = 10m, RealisationDate = ZDate.Empty, DoesNotCreateGLMovemetsOnSaving = true });
			var taxTransaction6 = TaxFrameworkTestObjectCreator.CreateTaxTransaction(new CreateTaxTransactionParameters { Company = companyXYZ, Ledger = LedgerTypes.AccountsReceivable, RealisationDate = ZDate.Today, DoesNotCreateGLMovemetsOnSaving = true });
			var taxTransaction7 = TaxFrameworkTestObjectCreator.CreateTaxTransaction(new CreateTaxTransactionParameters { Company = companyXYZ, Ledger = LedgerTypes.AccountsPayable, LocalTaxAmount = 10m, RealisationDate = ZDate.Empty, DoesNotCreateGLMovemetsOnSaving = true });
			var taxTransaction8 = TaxFrameworkTestObjectCreator.CreateTaxTransaction(new CreateTaxTransactionParameters { Company = companyXYZ, Ledger = LedgerTypes.AccountsPayable, RealisationDate = ZDate.Today, DoesNotCreateGLMovemetsOnSaving = true });

			Factory.Save();
			AssertTaxTransactionsForCompany(companyABC, taxTransaction1, taxTransaction2, taxTransaction3, taxTransaction4);
			AssertTaxTransactionsForCompany(companyXYZ, taxTransaction5, taxTransaction6, taxTransaction7, taxTransaction8);
		}

		void AssertTaxTransactionsForCompany(GlbCompany company, AccTaxTransaction taxTransaction1, AccTaxTransaction taxTransaction2, AccTaxTransaction taxTransaction3, AccTaxTransaction taxTransaction4)
		{
			var dataTable = RunScript(company.PK, "ALL", "ALL");
			AssertEquals(4, dataTable.Rows.Count);
			AssertTaxRecordExists(taxTransaction1);
			AssertTaxRecordExists(taxTransaction2);
			AssertTaxRecordExists(taxTransaction3);
			AssertTaxRecordExists(taxTransaction4);

			dataTable = RunScript(company.PK, "ALL", "N");
			AssertEquals(2, dataTable.Rows.Count);
			AssertTaxRecordExists(taxTransaction1);
			AssertTaxRecordExists(taxTransaction3);

			dataTable = RunScript(company.PK, LedgerTypes.AccountsReceivable, "N");
			AssertEquals(1, dataTable.Rows.Count);
			AssertTaxRecordExists(taxTransaction1);

			dataTable = RunScript(company.PK, LedgerTypes.AccountsPayable, "N");
			AssertEquals(1, dataTable.Rows.Count);
			AssertTaxRecordExists(taxTransaction3);

			dataTable = RunScript(company.PK, "ALL", "Y");
			AssertEquals(2, dataTable.Rows.Count);
			AssertTaxRecordExists(taxTransaction2);
			AssertTaxRecordExists(taxTransaction4);

			dataTable = RunScript(company.PK, LedgerTypes.AccountsReceivable, "Y");
			AssertEquals(1, dataTable.Rows.Count);
			AssertTaxRecordExists(taxTransaction2);

			dataTable = RunScript(company.PK, LedgerTypes.AccountsPayable, "Y");
			AssertEquals(1, dataTable.Rows.Count);
			AssertTaxRecordExists(taxTransaction4);

			void AssertTaxRecordExists(AccTaxTransaction taxTransaction)
			{
				var result = dataTable.Select($"ATT_PK='{taxTransaction.PK}'");
				AssertNotNull(result);
			}
		}

		DataTable RunScript(ZGuid companyPK, ZString ledger, string isRealized)
		{
			return DataUtils.GetDataTableFromQuery(Db.Connection, $@"
SELECT * 
FROM Report_TaxAnalysisDetailedOtherTaxes(
	'{companyPK}',
	'{ledger}',
	'{isRealized}'
)"
			);
		}

		TaxFrameworkTestObjectCreator TaxFrameworkTestObjectCreator => taxFrameworkTestObjectCreator ?? (taxFrameworkTestObjectCreator = new TaxFrameworkTestObjectCreator(Factory));
		TaxFrameworkTestObjectCreator taxFrameworkTestObjectCreator;
	}
}
