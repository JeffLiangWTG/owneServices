using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.ARAP.Invoicing.TaxFramework;
using Enterprise.Accounting.Utility.Testing;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business.Testing.Accounting.Helpers;
using Enterprise.ZArchitecture.Core;
using static Enterprise.Accounting.Utility.Testing.TaxFrameworkTestObjectCreator;
using static Enterprise.MasterFiles.Business.AccountingMasterFilesTaxFrameworkConstants;

namespace Enterprise.Accounting.TaxFramework.Business.Testing
{
	public class WHTAmountCalculatorTest : TestCaseWithFactory
	{
		public void TestCalculation()
		{
			var company = ObjectCreator.CreateNewCompany("CO1");
			var branch = ObjectCreator.CreateBranch("BR1", company);
			var currency = ObjectCreator.USD;

			using (Env.SetTemporaryUserContext(Env.CurrentUserPK, branch.PK.ToGuid(), Env.CurrentDepartmentPK))
			{
				ObjectCreator.CreateTestPeriods(ZDate.Today.AddMonths(-1));
			}

			AccObjectCreator.ConfigureTaxFrameworkAtCompanyLevel(company, LedgerTypes.AccountsPayable);
			var taxConfig = company.AccTaxConfigurations[0];
			taxConfig.ETC_AG_TaxControlAccount = ObjectCreator.GSTInputControlAccount().PK;

			var invoice1 = ObjectCreator.CreateInvoice(typeof(APInvoice), currency, 2M);
			var accTaxRecord1 = TaxFrameworkTestObjectCreator.CreateTaxTransaction(new CreateTaxTransactionParameters { TransactionHeader = invoice1, CompanyPK = company.PK, BranchPK = branch.PK, TaxConfiguration = taxConfig, Currency = currency, LocalTaxAmount = 200M, OsTaxAmount = 100M, TaxId = ObjectCreator.GST1, LocalTaxBaseAmount = 100M, OsTaxBaseAmount = 100M, TaxBasis = TaxBasisList.PostingOnMatching.Code, TaxSuperType = TaxSuperTypeList.StandardPaymentRetention.Code });
			var accTaxRecord1Realized = TaxFrameworkTestObjectCreator.CreateTaxTransaction(new CreateTaxTransactionParameters { TransactionHeader = invoice1, CompanyPK = company.PK, BranchPK = branch.PK, TaxConfiguration = taxConfig, Currency = currency, LocalTaxAmount = 300M, OsTaxAmount = 150M, TaxId = ObjectCreator.GST11, LocalTaxBaseAmount = 100M, OsTaxBaseAmount = 100M, TaxBasis = TaxBasisList.PostingOnMatching.Code, TransactionHeaderPK = accTaxRecord1.ATT_AH, RealisationDate = ZDate.Today, TaxSuperType = TaxSuperTypeList.StandardPaymentRetention.Code });

			var invoice2 = ObjectCreator.CreateInvoice(typeof(APInvoice), currency, 2M);
			var accTaxRecord2 = TaxFrameworkTestObjectCreator.CreateTaxTransaction(new CreateTaxTransactionParameters { TransactionHeader = invoice2, CompanyPK = company.PK, BranchPK = branch.PK, TaxConfiguration = taxConfig, Currency = currency, LocalTaxAmount = 50M, OsTaxAmount = 25M, LocalTaxBaseAmount = 100M, OsTaxBaseAmount = 100M, TaxBasis = TaxBasisList.PostingOnMatching.Code, TaxSuperType = TaxSuperTypeList.StandardPaymentRetention.Code });
			var accTaxRecord2Cancelled = TaxFrameworkTestObjectCreator.CreateTaxTransaction(new CreateTaxTransactionParameters { TransactionHeader = invoice2, CompanyPK = company.PK, BranchPK = branch.PK, TaxConfiguration = taxConfig, Currency = currency, LocalTaxAmount = 75M, OsTaxAmount = 40M, IsCancelled = true, LocalTaxBaseAmount = 100M, OsTaxBaseAmount = 100M, TaxBasis = TaxBasisList.PostingOnMatching.Code, TransactionHeaderPK = accTaxRecord2.ATT_AH, TaxSuperType = TaxSuperTypeList.StandardPaymentRetention.Code });

			var invoice3 = ObjectCreator.CreateInvoice(typeof(APInvoice), currency, 2M);
			var accTaxRecord3NonSPR = TaxFrameworkTestObjectCreator.CreateTaxTransaction(new CreateTaxTransactionParameters { TransactionHeader = invoice3, CompanyPK = company.PK, BranchPK = branch.PK, TaxConfiguration = taxConfig, Currency = currency, LocalTaxAmount = 75M, OsTaxAmount = 40M, IsCancelled = true, LocalTaxBaseAmount = 100M, OsTaxBaseAmount = 100M, TaxBasis = TaxBasisList.PostingOnMatching.Code, TaxSuperType = TaxSuperTypeList.SalesTax.Code });

			var transactionWithoutWHT = ObjectCreator.CreateAPInvoice<APInvoice>("10001", ObjectCreator.AUD, 1.0M, 250M, 21M, 0M, 250M, 21M, 0M);
			var line = ObjectCreator.CreateInvoiceLine(transactionWithoutWHT, ObjectCreator.AUD, 1.0M, 250M);

			var invoice4 = ObjectCreator.CreateInvoice(typeof(ARInvoice), currency, 1.5M);
			var accTaxRecordAR = TaxFrameworkTestObjectCreator.CreateTaxTransaction(new CreateTaxTransactionParameters { TransactionHeader = invoice4, CompanyPK = company.PK, BranchPK = branch.PK, TaxConfiguration = taxConfig, Currency = currency, LocalTaxAmount = 200M, OsTaxAmount = 100M, TaxId = ObjectCreator.GST1, LocalTaxBaseAmount = 100M, OsTaxBaseAmount = 100M, TaxBasis = TaxBasisList.PostingOnMatching.Code, TaxSuperType = TaxSuperTypeList.StandardPaymentRetention.Code });
			accTaxRecordAR.ATT_Ledger = LedgerTypes.AccountsReceivable;

			TaxFrameworkTestObjectCreator.CreateTaxTransactionLinePivot(accTaxRecord1.PK, TaxFrameworkObjectFactory.GetInvoicingLineBaseTaxable(line));
			TaxFrameworkTestObjectCreator.CreateTaxTransactionLinePivot(accTaxRecord1Realized.PK, TaxFrameworkObjectFactory.GetInvoicingLineBaseTaxable(line));
			TaxFrameworkTestObjectCreator.CreateTaxTransactionLinePivot(accTaxRecord2.PK, TaxFrameworkObjectFactory.GetInvoicingLineBaseTaxable(line));
			var pivot = TaxFrameworkTestObjectCreator.CreateTaxTransactionLinePivot(accTaxRecord2Cancelled.PK, TaxFrameworkObjectFactory.GetInvoicingLineBaseTaxable(line));
			pivot.ATP_LocalTaxAmount = 75M;
			pivot = TaxFrameworkTestObjectCreator.CreateTaxTransactionLinePivot(accTaxRecord3NonSPR.PK, TaxFrameworkObjectFactory.GetInvoicingLineBaseTaxable(line));
			pivot.ATP_LocalTaxAmount = 75M;
			TaxFrameworkTestObjectCreator.CreateTaxTransactionLinePivot(accTaxRecordAR.PK, TaxFrameworkObjectFactory.GetInvoicingLineBaseTaxable(line));

			Factory.Save();

			var amounts = new WHTAmountCalculator().Calculate(accTaxRecord1.ATT_AH, accTaxRecord2.ATT_AH, accTaxRecord3NonSPR.ATT_AH, transactionWithoutWHT.PK, accTaxRecordAR.ATT_AH);
			AssertWHTAmount(accTaxRecord1.ATT_AH, 200M, 300M);
			AssertWHTAmount(accTaxRecord2.ATT_AH, 50M, 0M);
			AssertWHTAmount(accTaxRecord3NonSPR.ATT_AH, 0M, 0M);
			AssertWHTAmount(transactionWithoutWHT.PK, 0M, 0M);
			AssertWHTAmount(accTaxRecordAR.ATT_AH, 0M, 0M);

			void AssertWHTAmount(ZGuid transactionPK, ZDecimal expectedNotionalAmount, ZDecimal expectedRealizedAmount)
			{
				var amount = amounts.FirstOrDefault(x => x.TransactionPK == transactionPK);
				AssertNotNull(amount);
				AssertEquals("Notional Amount", expectedNotionalAmount, amount.NotionalAmount);
				AssertEquals("Realized Amount", expectedRealizedAmount, amount.RealizedAmount);
			}
		}

		TestObjectCreator ObjectCreator => objectCreator ?? (objectCreator = new TestObjectCreator(Factory));
		TestObjectCreator objectCreator;

		TaxFrameworkTestObjectCreator TaxFrameworkTestObjectCreator => taxFrameworkTestObjectCreator ?? (taxFrameworkTestObjectCreator = new TaxFrameworkTestObjectCreator(Factory));
		TaxFrameworkTestObjectCreator taxFrameworkTestObjectCreator;

		AccountingTestObjectCreator AccObjectCreator => accObjectCreator ?? (accObjectCreator = new AccountingTestObjectCreator(Factory));
		AccountingTestObjectCreator accObjectCreator;
	}
}
