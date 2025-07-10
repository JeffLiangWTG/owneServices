using System;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.Base.Interfaces.Testing;
using Enterprise.Accounting.Registry.Business;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.CashBook.ExchangeDifference.Testing
{
	[TestedType(typeof(NewCashbookExchangeDiff))]
	public class NewCashbookExchangeDiffTest : CashbookExchangeDiffTest
	{
		#region Properties

		public void TestInclude()
		{
			var newCashbookExchangeDiff = GetNewCashbookExchangeDiff();
			newCashbookExchangeDiff.Include = true;
			newCashbookExchangeDiff.AH_ExchangeRate = 0;
			AssertEquals("NewCashbookExchangeDiff has errors when include is ticked", true, newCashbookExchangeDiff.HasErrors);
			newCashbookExchangeDiff.Include = false;
			AssertEquals("NewCashbookExchangeDiff has no errors when include is unticked", false, newCashbookExchangeDiff.HasErrors);
		}

		[TestDate(2020, 2, 6)]
		public void TestAH_InvoiceDate()
		{
			var header = Factory.New<NewCashbookExchangeDiffHeader>();
			var newCashbookExchangeDiff1 = header.NewCashbookExchangeDiffCollection.AddNew();

			var expectedInvoiceDate1 = ZDateTime.Now.AddDays(1);
			var expectedInvoiceDate2 = ZDateTime.Now.AddDays(2);

			header.AH_InvoiceDate = expectedInvoiceDate1;
			AssertEquals("Invoice date will changed from header", expectedInvoiceDate1, newCashbookExchangeDiff1.AH_InvoiceDate);

			newCashbookExchangeDiff1.AH_InvoiceDate = expectedInvoiceDate2;
			AssertEquals("Invoice date can't change", expectedInvoiceDate1, newCashbookExchangeDiff1.AH_InvoiceDate);

			var newCashbookExchangeDiff2 = Factory.New<NewCashbookExchangeDiff>();
			newCashbookExchangeDiff2.AH_InvoiceDate = expectedInvoiceDate2;
			AssertEquals("Invoice date without parent", expectedInvoiceDate2, newCashbookExchangeDiff2.AH_InvoiceDate);
		}

		[TestDate(2020, 2, 6)]
		public void TestAH_PostDate()
		{
			var header = Factory.New<NewCashbookExchangeDiffHeader>();
			var newCashbookExchangeDiff1 = header.NewCashbookExchangeDiffCollection.AddNew();

			var expectedPostDate1 = ZDateTime.Now.AddDays(1);
			var expectedPostDate2 = ZDateTime.Now.AddDays(2);

			header.AH_PostDate = expectedPostDate1;
			AssertEquals("post date will changed from header", expectedPostDate1, newCashbookExchangeDiff1.AH_PostDate);

			newCashbookExchangeDiff1.AH_PostDate = expectedPostDate2;
			AssertEquals("post date can't change", expectedPostDate1, newCashbookExchangeDiff1.AH_PostDate);

			var newCashbookExchangeDiff2 = Factory.New<NewCashbookExchangeDiff>();
			newCashbookExchangeDiff2.AH_PostDate = expectedPostDate2;
			AssertEquals("post date without parent", expectedPostDate2, newCashbookExchangeDiff2.AH_PostDate);

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.China))
			{
				var newCashbookExchangeDiff3 = header.NewCashbookExchangeDiffCollection.AddNew();
				var bank = TestObjectCreator.CreateBankAccount("TST", "Test Bank", TestObjectCreator.AUD, TestObjectCreator.GLHeader1);
				newCashbookExchangeDiff3.AH_AB = bank.PK;
				TestObjectCreator.CreateExchangeRate(TestObjectCreator.AUD, "BUY", 4.6m, new ZDateTime(2020, 2, 1), new ZDateTime(2020, 2, 29));
				header.AH_PostDate = ZDateTime.Now;
				AssertEquals("exchange rate should not be 0", 4.6m, newCashbookExchangeDiff3.AH_ExchangeRate);

				header.AH_PostDate = new ZDateTime(2020, 3, 1);
				AssertEquals("exchange rate not found", 0m, newCashbookExchangeDiff3.AH_ExchangeRate);
			}
		}

		public void TestAH_Desc()
		{
			var header = Factory.New<NewCashbookExchangeDiffHeader>();
			var newCashbookExchangeDiff1 = header.NewCashbookExchangeDiffCollection.AddNew();

			var expectedDesc1 = "Test1";
			var expectedDesc2 = "Test2";

			header.AH_Desc = expectedDesc1;
			AssertEquals("description will changed from header", expectedDesc1, newCashbookExchangeDiff1.AH_Desc);

			newCashbookExchangeDiff1.AH_Desc = expectedDesc2;
			AssertEquals("description can't change", expectedDesc1, newCashbookExchangeDiff1.AH_Desc);

			var newCashbookExchangeDiff2 = Factory.New<NewCashbookExchangeDiff>();
			newCashbookExchangeDiff2.AH_Desc = expectedDesc2;
			AssertEquals("description without parent", expectedDesc2, newCashbookExchangeDiff2.AH_Desc);
		}

		public void TestReadOnly()
		{
			var newCashbookExchangeDiff = GetNewCashbookExchangeDiff();
			AssertEquals("AH_TransactionNum should be readonly", true, newCashbookExchangeDiff.AH_TransactionNumInfo.ReadOnly);
			AssertEquals("AH_AB should be readonly", true, newCashbookExchangeDiff.AH_ABInfo.ReadOnly);
		}

		public void TestBank()
		{
			var header = Factory.New<NewCashbookExchangeDiffHeader>();
			var newCashbookExchangeDiff = header.NewCashbookExchangeDiffCollection.AddNew();

			var bank = TestObjectCreator.CreateBankAccount("TSTBNK1", "Test Bank Account", TestObjectCreator.USD, TestObjectCreator.GLHeader1);
			AssertEquals("Pre-condition", ZString.Empty, newCashbookExchangeDiff.BankCurrency);
			AssertEquals("Pre-condition", ZString.Empty, newCashbookExchangeDiff.BankAccountDescription);

			newCashbookExchangeDiff.AH_AB = bank.PK;
			AssertEquals("bank currency", TestObjectCreator.USD.Code, newCashbookExchangeDiff.BankCurrency);
			AssertEquals("bank account description", "Test Bank Account", newCashbookExchangeDiff.BankAccountDescription);
		}

		[TestDate(2020, 2, 20)]
		public void TestNewExchangeRateWithFallBackToPreviousExchangeRate()
		{
			using (AccountingConfigurationRegistry.Instance.FallBackToPreviousExchangeRate.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var periodManagements = Factory.Load<AccPeriodManagement>(new ZQuery());
				periodManagements.ForEach((x) => { x.Delete(); });
				TestObjectCreator.CreateTestPeriodsForEntireYear(2020);

				var bank = TestObjectCreator.USDBankAccount;
				var header = Factory.New<NewCashbookExchangeDiffHeader>();
				header.AH_PostDate = ZDateTime.Today.AddMonths(-1);
				var cashbookExchangeDiff = header.NewCashbookExchangeDiffCollection.AddNew();
				cashbookExchangeDiff.AH_AB = bank.PK;

				AssertEquals("Pre-condition: registry setting is PER (default)", "PER", AccountingConfigurationRegistry.Instance.BankCurrencyAdjustmentExchangeRateType.Value);
				AssertEquals("pre-condition", 0m, cashbookExchangeDiff.GetNewExchangeRate());

				TestObjectCreator.CreateExchangeRate(TestObjectCreator.USD, "PER", 0.8289m, new ZDateTime(2020, 1, 1), new ZDateTime(2020, 1, 31));
				TestObjectCreator.CreateExchangeRate(TestObjectCreator.USD, "BUY", 0.8288m, ZDateTime.Today, ZDateTime.Today);
				AssertEquals("Use 'PER' rate", 0.8289m, cashbookExchangeDiff.GetNewExchangeRate());
			}
		}

		#endregion

		#region overrides

		public void TestDefaultValues()
		{
			var newCashbookExchangeDiff = GetNewCashbookExchangeDiff();
			AssertEquals(true, newCashbookExchangeDiff.Include);
		}

		public void TestIsSavedByFactoryWithInclude()
		{
			var newCashbookExchangeDiff = GetNewCashbookExchangeDiff();
			AssertEquals("Pre-condition", true, newCashbookExchangeDiff.IsSavedByFactory);
			newCashbookExchangeDiff.Include = false;
			AssertEquals("IsSavedByFactory", false, newCashbookExchangeDiff.IsSavedByFactory);
		}
		NewCashbookExchangeDiff GetNewCashbookExchangeDiff() => (NewCashbookExchangeDiff)GetNewBusinessObject();

		#endregion

		#region Implementation

		protected override Type TypeOfValidation
		{
			get { return typeof(NewCashbookExchangeDiffValidation); }
		}

		#endregion
	}

	[TestedType(typeof(NewCashbookExchangeDiff))]
	public class NewCashbookExchangeDiffIUnmatchDateSupporterTest : BaseITransactionTestCase
	{
	}
}
