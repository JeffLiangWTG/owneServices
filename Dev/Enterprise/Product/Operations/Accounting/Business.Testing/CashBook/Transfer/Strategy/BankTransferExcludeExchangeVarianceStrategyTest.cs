using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Accounting.Business.CashBook.Transfer.Testing
{
	public class BankTransferExcludeExchangeVarianceStrategyTest : TestCaseWithFactory
	{
		#region Sell

		public void TestSellAmount()
		{
			BankTransferForeignToLocal.SetSellAmountWithAsserts(100m);
			BankTransferForeignToLocal.AssertSellPropertiesAndRowFrom(100m, 0m, 0m);
			BankTransferForeignToLocal.AssertBuyPropertiesAndRowTo(0m, 1m, 0m);

			BankTransferForeignToForeign.SetSellAmountWithAsserts(100m);
			BankTransferForeignToForeign.AssertSellPropertiesAndRowFrom(100m, 0m, 0m);
			BankTransferForeignToForeign.AssertBuyPropertiesAndRowTo(0m, 0m, 0m);

			BankTransferLocalToForeign.SetSellAmountWithAsserts(100m);
			BankTransferLocalToForeign.AssertSellPropertiesAndRowFrom(100m, 1m, 100m);
			BankTransferLocalToForeign.AssertBuyPropertiesAndRowTo(0m, 0m, 100m);

			BankTransferLocalToLocal.SetSellAmountWithAsserts(100m);
			BankTransferLocalToLocal.AssertSellPropertiesAndRowFrom(100m, 1m, 100m);
			BankTransferLocalToLocal.AssertBuyPropertiesAndRowTo(100m, 1m, 100m);
		}

		public void TestSellExchangeRate()
		{
			AssertEquals("Precondition", false, BankTransferForeignToLocal.SellZExchangeRate.RateInfo.ReadOnly);
			AssertEquals("Precondition", false, BankTransferForeignToForeign.SellZExchangeRate.RateInfo.ReadOnly);
			AssertEquals("Precondition", true, BankTransferLocalToForeign.SellZExchangeRate.RateInfo.ReadOnly);
			AssertEquals("Precondition", true, BankTransferLocalToLocal.SellZExchangeRate.RateInfo.ReadOnly);

			BankTransferForeignToLocal.SetSellAmountWithAsserts(100m).SetSellExchangeRateWithAsserts(2m);
			BankTransferForeignToLocal.AssertSellPropertiesAndRowFrom(100m, 2m, 50m);
			BankTransferForeignToLocal.AssertBuyPropertiesAndRowTo(50m, 1m, 50m);

			BankTransferForeignToForeign.SetSellAmountWithAsserts(100m).SetSellExchangeRateWithAsserts(2m);
			BankTransferForeignToForeign.AssertSellPropertiesAndRowFrom(100m, 2m, 0m);
			BankTransferForeignToForeign.AssertBuyPropertiesAndRowTo(0m, 0m, 0m);
		}

		public void TestLocalSellAmount()
		{
			AssertEquals("Precondition", false, BankTransferForeignToLocal.LocalSellAmountInfo.ReadOnly);
			AssertEquals("Precondition", false, BankTransferForeignToForeign.LocalSellAmountInfo.ReadOnly);
			AssertEquals("Precondition", true, BankTransferLocalToForeign.LocalSellAmountInfo.ReadOnly);
			AssertEquals("Precondition", true, BankTransferLocalToLocal.LocalSellAmountInfo.ReadOnly);

			BankTransferForeignToLocal.SetSellAmountWithAsserts(100m).SetSellExchangeRateWithAsserts(2m).SetLocalSellAmountWithAsserts(300m);
			BankTransferForeignToLocal.AssertSellPropertiesAndRowFrom(100m, 0.333333m, 300m);
			BankTransferForeignToLocal.AssertBuyPropertiesAndRowTo(300m, 1m, 300m);

			BankTransferForeignToForeign.SetSellAmountWithAsserts(100m).SetSellExchangeRateWithAsserts(2m).SetLocalSellAmountWithAsserts(300m);
			BankTransferForeignToForeign.AssertSellPropertiesAndRowFrom(100m, 0.333333m, 300m);
			BankTransferForeignToForeign.AssertBuyPropertiesAndRowTo(0m, 0m, 300m);
		}

		#endregion

		#region Buy

		public void TestBuyAmount()
		{
			BankTransferForeignToLocal.SetSellAmountWithAsserts(100m).SetSellExchangeRateWithAsserts(2m).SetLocalSellAmountWithAsserts(300m);
			BankTransferForeignToLocal.SetBuyAmountWithAsserts(200m);
			BankTransferForeignToLocal.AssertSellPropertiesAndRowFrom(100m, 0.5m, 200m);
			BankTransferForeignToLocal.AssertBuyPropertiesAndRowTo(200m, 1m, 200m);

			BankTransferForeignToForeign.SetSellAmountWithAsserts(100m).SetSellExchangeRateWithAsserts(2m).SetLocalSellAmountWithAsserts(300m);
			BankTransferForeignToForeign.SetBuyAmountWithAsserts(200m);
			BankTransferForeignToForeign.AssertSellPropertiesAndRowFrom(100m, 0.333333m, 300m);
			BankTransferForeignToForeign.AssertBuyPropertiesAndRowTo(200m, 0.666667m, 300m);

			BankTransferLocalToForeign.SetSellAmountWithAsserts(300m);
			BankTransferLocalToForeign.SetBuyAmountWithAsserts(200m);
			BankTransferLocalToForeign.AssertSellPropertiesAndRowFrom(300m, 1m, 300m);
			BankTransferLocalToForeign.AssertBuyPropertiesAndRowTo(200m, 0.666667m, 300m);

			BankTransferLocalToLocal.SetSellAmountWithAsserts(300m);
			BankTransferLocalToLocal.SetBuyAmountWithAsserts(200m);
			BankTransferLocalToLocal.AssertSellPropertiesAndRowFrom(200m, 1m, 200m);
			BankTransferLocalToLocal.AssertBuyPropertiesAndRowTo(200m, 1m, 200m);
		}

		public void TestBuyExchangeRate()
		{
			AssertEquals("Precondition", true, BankTransferForeignToLocal.BuyZExchangeRate.RateInfo.ReadOnly);
			AssertEquals("Precondition", false, BankTransferForeignToForeign.BuyZExchangeRate.RateInfo.ReadOnly);
			AssertEquals("Precondition", false, BankTransferLocalToForeign.BuyZExchangeRate.RateInfo.ReadOnly);
			AssertEquals("Precondition", true, BankTransferLocalToLocal.BuyZExchangeRate.RateInfo.ReadOnly);

			BankTransferForeignToForeign.SetSellAmountWithAsserts(100m).SetSellExchangeRateWithAsserts(2m).SetLocalSellAmountWithAsserts(300m);
			BankTransferForeignToForeign.SetBuyAmountWithAsserts(200m).SetBuyExchangeRateWithAsserts(3m);
			BankTransferForeignToForeign.AssertSellPropertiesAndRowFrom(100m, 0.333333m, 300m);
			BankTransferForeignToForeign.AssertBuyPropertiesAndRowTo(900m, 3m, 300m);

			BankTransferLocalToForeign.SetSellAmountWithAsserts(300m);
			BankTransferLocalToForeign.SetBuyAmountWithAsserts(200m).SetBuyExchangeRateWithAsserts(3m);
			BankTransferLocalToForeign.AssertSellPropertiesAndRowFrom(300m, 1m, 300m);
			BankTransferLocalToForeign.AssertBuyPropertiesAndRowTo(900m, 3m, 300m);
		}

		public void TestLocalBuyAmount()
		{
			AssertEquals("Precondition", true, BankTransferForeignToLocal.LocalBuyAmountInfo.ReadOnly);
			AssertEquals("Precondition", false, BankTransferForeignToForeign.LocalBuyAmountInfo.ReadOnly);
			AssertEquals("Precondition", false, BankTransferLocalToForeign.LocalBuyAmountInfo.ReadOnly);
			AssertEquals("Precondition", true, BankTransferLocalToLocal.LocalBuyAmountInfo.ReadOnly);

			BankTransferForeignToForeign.SetSellAmountWithAsserts(100m).SetSellExchangeRateWithAsserts(2m).SetLocalSellAmountWithAsserts(300m);
			BankTransferForeignToForeign.SetBuyAmountWithAsserts(200m).SetBuyExchangeRateWithAsserts(3m).SetLocalBuyAmountWithAsserts(400m);
			BankTransferForeignToForeign.AssertSellPropertiesAndRowFrom(100m, 0.25m, 400m);
			BankTransferForeignToForeign.AssertBuyPropertiesAndRowTo(1200m, 3m, 400m);

			BankTransferLocalToForeign.SetSellAmountWithAsserts(300m);
			BankTransferLocalToForeign.SetBuyAmountWithAsserts(200m).SetBuyExchangeRateWithAsserts(3m).SetLocalBuyAmountWithAsserts(400m);
			BankTransferLocalToForeign.AssertSellPropertiesAndRowFrom(400m, 1m, 400m);
			BankTransferLocalToForeign.AssertBuyPropertiesAndRowTo(1200m, 3m, 400m);
		}

		#endregion

		public void TestGenerateReverseTransaction()
		{
			BankTransferForeignToForeign.SetSellAmountWithAsserts(4470.98m).SetLocalSellAmountWithAsserts(34683.39m);
			BankTransferForeignToForeign.SetBuyAmountWithAsserts(4029.80m);

			BankTransferForeignToForeign.AssertSellPropertiesAndRowFrom(4470.98m, 0.128908m, 34683.39m);
			BankTransferForeignToForeign.AssertBuyPropertiesAndRowTo(4029.80m, 0.116188m, 34683.39m);

			var reversal = GenerateReverseTransaction(BankTransferForeignToForeign);

			reversal.AssertSellPropertiesAndRowFrom(4029.80m, 0.116188m, 34683.39m);
			reversal.AssertBuyPropertiesAndRowTo(4470.98m, 0.128908m, 34683.39m);
		}

		BankTransfer GenerateReverseTransaction(BankTransfer bankTransfer)
		{
			bankTransfer.GenerateReverseTransaction(true);
			return bankTransfer.ReverseTransaction as BankTransfer;
		}

		protected override void SetUp()
		{
			var year = ZDateTime.Now.Year;
			TestObjectCreator.CreateTestPeriodsForEntireYear(year);
			TestObjectCreator.CreateTestPeriodsForEntireYear(year + 1);

			BankTransferForeignToLocal = new BankTransfer(Factory, null);
			BankTransferForeignToLocal.BankTransferFromPK = TestObjectCreator.USDBankAccount.PK;
			BankTransferForeignToLocal.BankTransferToPK = TestObjectCreator.AUDBankAccount.PK;

			BankTransferForeignToForeign = new BankTransfer(Factory, null);
			BankTransferForeignToForeign.BankTransferFromPK = TestObjectCreator.USDBankAccount.PK;
			BankTransferForeignToForeign.BankTransferToPK = TestObjectCreator.GBPBankAccount.PK;

			BankTransferLocalToForeign = new BankTransfer(Factory, null);
			BankTransferLocalToForeign.BankTransferFromPK = TestObjectCreator.AUDBankAccount.PK;
			BankTransferLocalToForeign.BankTransferToPK = TestObjectCreator.GBPBankAccount.PK;

			BankTransferLocalToLocal = new BankTransfer(Factory, null);
			BankTransferLocalToLocal.BankTransferFromPK = TestObjectCreator.AUDBankAccount.PK;
			BankTransferLocalToLocal.BankTransferToPK = TestObjectCreator.AUDBankAccount2.PK;
		}

		BankTransfer BankTransferForeignToLocal;
		BankTransfer BankTransferForeignToForeign;
		BankTransfer BankTransferLocalToForeign;
		BankTransfer BankTransferLocalToLocal;

		TestObjectCreator TestObjectCreator => testObjectCreator ?? (testObjectCreator = new TestObjectCreator(Factory));
		TestObjectCreator testObjectCreator;
	}
}
