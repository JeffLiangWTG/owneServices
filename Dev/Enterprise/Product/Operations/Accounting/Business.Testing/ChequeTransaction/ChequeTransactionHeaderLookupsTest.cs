using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Accounting.Business.ChequeTransaction.Testing
{
	public class ChequeTransactionHeaderLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestChequeTransactionTypesList()
		{
			var accPaymentBatch = Factory.New<ChequeTransactionHeader>();
			var expectedValues = new string[] {
									"CET", "COC", "CBC",
									"CBG", "CCB", "BAB",
									"CRB", "CRR", "CCP",
									"BCP", "CEC", "CRC",
									"BCR", "UCC", "CJP" };

			AssertEquals(expectedValues.Length, accPaymentBatch.Lookups.PaymentTypeList.Count);
			AssertContainsExactElementsInAnyOrder(expectedValues, accPaymentBatch.Lookups.PaymentTypeList.Cast<CodeDescriptionPair>().Select(x => x.Code).ToArray());
		}

		public void TestBankAccounts()
		{
			PaymentBatch.Lookups.BankAccounts.Load();

			AssertEquals("BankAccounts Count", 1, PaymentBatch.Lookups.BankAccounts.Count);
			AssertEquals("BankAccounts contains BankAccount", true, PaymentBatch.Lookups.BankAccounts.Contains(BankAccount.PK));
			AssertEquals("BankAccounts does not contain CashAccount", true, !PaymentBatch.Lookups.BankAccounts.Contains(CashAccount.PK));
		}

		public void TestFundingBankAccounts()
		{
			PaymentBatch.Lookups.FundingBankAccounts.Load();

			AssertEquals("BankAccounts Count", 1, PaymentBatch.Lookups.FundingBankAccounts.Count);
			AssertEquals("BankAccounts contains CashAccount", true, PaymentBatch.Lookups.FundingBankAccounts.Contains(CashAccount.PK));
			AssertEquals("BankAccounts does not contain BankAccount", true, !PaymentBatch.Lookups.FundingBankAccounts.Contains(BankAccount.PK));
		}

		protected override void SetUp()
		{
			base.SetUp();

			TestObjectCreator = new TestObjectCreator(Factory);

			CashAccount = TestObjectCreator.CreateBankAccount("CSH", "CashAccount", TestObjectCreator.USD, TestObjectCreator.GLHeader1, "CSH");
			BankAccount = TestObjectCreator.CreateBankAccount("BNK", "BankAccount", TestObjectCreator.TRY, TestObjectCreator.GLHeader2, "BNK");
			Factory.Save();
		}

		ChequeTransactionHeader PaymentBatch => paymentBatch ??= Factory.New<ChequeTransactionHeader>();
		ChequeTransactionHeader paymentBatch;

		TestObjectCreator TestObjectCreator;
		AccBankAccount CashAccount;
		AccBankAccount BankAccount;
	}
}


