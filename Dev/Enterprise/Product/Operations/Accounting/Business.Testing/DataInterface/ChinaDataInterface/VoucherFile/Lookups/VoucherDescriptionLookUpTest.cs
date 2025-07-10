using CargoWise.EntityFramework.Testing;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Accounting.Business.DataInterface.ChinaDataInterface.VoucherFile
{
	public class VoucherDescriptionLookUpTest : TestCaseWithFactory
	{
		public void TestDescriptionIfNonChina()
		{
			GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.Australia);
			APInvoice aPInvoice = Factory.NewWithValidTestData<APInvoice>();
			VoucherDescriptionLookUp lookUp = new VoucherDescriptionLookUp(aPInvoice);
			string result = lookUp.GetDescription();
			AssertEquals(aPInvoice.AH_Desc, result);
		}

		public void TestDescriptionIfChina()
		{
			GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.China);
			APInvoice aPInvoice = Factory.NewWithValidTestData<APInvoice>();
			VoucherDescriptionLookUp lookUp = new VoucherDescriptionLookUp(aPInvoice);
			string result = lookUp.GetDescription();
			AssertEquals("营业成本", result);
		}

		public void TestDescriptionIfChinaARInv()
		{
			GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.China);
			string result = GetVoucherDescription(LedgerTypes.AccountsReceivable, TransactionTypes.Invoice);
			AssertEquals("营业收入", result);
		}

		public void TestDescriptionIfChinaAPInv()
		{
			GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.China);
			string result = GetVoucherDescription(LedgerTypes.AccountsPayable, TransactionTypes.Invoice);
			AssertEquals("营业成本", result);
		}

		public void TestDescriptionIfChinaDirectPayment()
		{
			GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.China);
			string result = GetVoucherDescription(LedgerTypes.CashBook, TransactionTypes.DirectPayment);
			AssertEquals("直接付款单", result);
		}

		public void TestDescriptionIfChinaJobRevenueJournal()
		{
			GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.China);
			string result = GetVoucherDescription(LedgerTypes.JobCosting, TransactionTypes.JobRevenueJournal);
			AssertEquals("营业凭证", result);
		}

		string GetVoucherDescription(string ledger, string type)
		{
			AccTransactionHeader testTransaction = SetTransaction(ledger, type);
			VoucherDescriptionLookUp lookUp = new VoucherDescriptionLookUp(testTransaction);
			return lookUp.GetDescription();
		}

		AccTransactionHeader SetTransaction(string ledger, string transactionType)
		{
			AccTransactionHeader testTransaction = Factory.NewWithValidTestData(typeof(AccTransactionHeader)) as AccTransactionHeader;
			testTransaction.AH_Ledger = ledger;
			testTransaction.AH_TransactionType = transactionType;
			return testTransaction;
		}
	}
}