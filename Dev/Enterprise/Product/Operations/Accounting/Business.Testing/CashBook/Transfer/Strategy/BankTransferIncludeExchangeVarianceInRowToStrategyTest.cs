using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Accounting.Business.CashBook.Transfer.Testing
{
	public class BankTransferIncludeExchangeVarianceInRowToStrategyTest : TestCaseWithFactory
	{
		#region Sell

		public void TestSellAmount()
		{
			SetupBankTransfers();
			BankTransferForeignToLocal.SetSellAmount(100m);
			BankTransferForeignToLocal.AssertSellPropertiesAndRowFrom(100m, 0m, 0m);
			BankTransferForeignToLocal.AssertBuyProperties(0m, 1m, 0m);
			BankTransferForeignToLocal.AssertRowTo(0m, 1m, 0m);
			AssertEquals(0m, BankTransferForeignToLocal.ExRateGainLoss);

			BankTransferForeignToForeign.SetSellAmount(100m);
			BankTransferForeignToForeign.AssertSellPropertiesAndRowFrom(100m, 0m, 0m);
			BankTransferForeignToForeign.AssertBuyProperties(0m, 0m, 0m);
			BankTransferForeignToForeign.AssertRowTo(0m, 0m, 0m);
			AssertEquals(0m, BankTransferForeignToForeign.ExRateGainLoss);

			BankTransferLocalToForeign.SetSellAmount(100m);
			BankTransferLocalToForeign.AssertSellPropertiesAndRowFrom(100m, 1m, 100m);
			BankTransferLocalToForeign.AssertBuyProperties(0m, 0m, 0m);
			BankTransferLocalToForeign.AssertRowTo(0m, 0m, 0m);
			AssertEquals(-100m, BankTransferLocalToForeign.ExRateGainLoss);
		}

		public void TestSellExchangeRate()
		{
			SetupBankTransfers();
			AssertEquals("Precondition", false, BankTransferForeignToLocal.SellZExchangeRate.RateInfo.ReadOnly);
			AssertEquals("Precondition", false, BankTransferForeignToForeign.SellZExchangeRate.RateInfo.ReadOnly);
			AssertEquals("Precondition", true, BankTransferLocalToForeign.SellZExchangeRate.RateInfo.ReadOnly);
			AssertEquals("Precondition", true, BankTransferLocalToLocal.SellZExchangeRate.RateInfo.ReadOnly);

			BankTransferForeignToLocal.SetSellAmount(100m).SetSellExchangeRate(2m);
			BankTransferForeignToLocal.AssertSellPropertiesAndRowFrom(100m, 2m, 50m);
			BankTransferForeignToLocal.AssertBuyProperties(0m, 1m, 0m);
			BankTransferForeignToLocal.AssertRowTo(0m, 1m, 0m);
			AssertEquals(-50m, BankTransferForeignToLocal.ExRateGainLoss);

			BankTransferForeignToForeign.SetSellAmount(100m).SetSellExchangeRate(2m);
			BankTransferForeignToForeign.AssertSellPropertiesAndRowFrom(100m, 2m, 50m);
			BankTransferForeignToForeign.AssertBuyProperties(0m, 0m, 0m);
			BankTransferForeignToForeign.AssertRowTo(0m, 0m, 0m);
			AssertEquals(-50m, BankTransferForeignToForeign.ExRateGainLoss);
		}

		public void TestLocalSellAmount()
		{
			SetupBankTransfers();
			AssertEquals("Precondition", false, BankTransferForeignToLocal.LocalSellAmountInfo.ReadOnly);
			AssertEquals("Precondition", false, BankTransferForeignToForeign.LocalSellAmountInfo.ReadOnly);
			AssertEquals("Precondition", true, BankTransferLocalToForeign.LocalSellAmountInfo.ReadOnly);
			AssertEquals("Precondition", true, BankTransferLocalToLocal.LocalSellAmountInfo.ReadOnly);

			BankTransferForeignToLocal.SetSellAmount(100m).SetSellExchangeRate(2m).SetLocalSellAmount(300m);
			BankTransferForeignToLocal.AssertSellPropertiesAndRowFrom(100m, 0.333333m, 300m);
			BankTransferForeignToLocal.AssertBuyProperties(0m, 1m, 0m);
			BankTransferForeignToLocal.AssertRowTo(0m, 1m, 0m);
			AssertEquals(-300m, BankTransferForeignToLocal.ExRateGainLoss);

			BankTransferForeignToForeign.SetSellAmount(100m).SetSellExchangeRate(2m).SetLocalSellAmount(300m);
			BankTransferForeignToForeign.AssertSellPropertiesAndRowFrom(100m, 0.333333m, 300m);
			BankTransferForeignToForeign.AssertBuyProperties(0m, 0m, 0m);
			BankTransferForeignToForeign.AssertRowTo(0m, 0m, 0m);
			AssertEquals(-300m, BankTransferForeignToForeign.ExRateGainLoss);
		}

		#endregion

		#region Buy

		public void TestBuyAmount()
		{
			SetupBankTransfers();
			BankTransferForeignToLocal.SetSellAmount(100m).SetSellExchangeRate(2m).SetLocalSellAmount(300m);
			BankTransferForeignToLocal.SetBuyAmount(200m);
			BankTransferForeignToLocal.AssertSellPropertiesAndRowFrom(100m, 0.333333m, 300m);
			BankTransferForeignToLocal.AssertBuyProperties(200m, 1m, 200m);
			BankTransferForeignToLocal.AssertRowTo(300m, 1m, 300m);
			AssertEquals(-100m, BankTransferForeignToLocal.ExRateGainLoss);

			BankTransferForeignToForeign.SetSellAmount(100m).SetSellExchangeRate(2m).SetLocalSellAmount(300m);
			BankTransferForeignToForeign.SetBuyAmount(200m);
			BankTransferForeignToForeign.AssertSellPropertiesAndRowFrom(100m, 0.333333m, 300m);
			BankTransferForeignToForeign.AssertBuyProperties(200m, 0m, 0m);
			BankTransferForeignToForeign.AssertRowTo(200m, 0m, 0m);
			AssertEquals(-300m, BankTransferForeignToForeign.ExRateGainLoss);

			BankTransferLocalToForeign.SetSellAmount(300m);
			BankTransferLocalToForeign.SetBuyAmount(200m);
			BankTransferLocalToForeign.AssertSellPropertiesAndRowFrom(300m, 1m, 300m);
			BankTransferLocalToForeign.AssertBuyProperties(200m, 0m, 0m);
			BankTransferLocalToForeign.AssertRowTo(200m, 0m, 0m);
			AssertEquals(-300m, BankTransferLocalToForeign.ExRateGainLoss);
		}

		public void TestBuyExchangeRate()
		{
			SetupBankTransfers();
			AssertEquals("Precondition", true, BankTransferForeignToLocal.BuyZExchangeRate.RateInfo.ReadOnly);
			AssertEquals("Precondition", false, BankTransferForeignToForeign.BuyZExchangeRate.RateInfo.ReadOnly);
			AssertEquals("Precondition", false, BankTransferLocalToForeign.BuyZExchangeRate.RateInfo.ReadOnly);
			AssertEquals("Precondition", true, BankTransferLocalToLocal.BuyZExchangeRate.RateInfo.ReadOnly);

			BankTransferForeignToForeign.SetSellAmount(100m).SetSellExchangeRate(2m).SetLocalSellAmount(300m);
			BankTransferForeignToForeign.SetBuyAmount(200m).SetBuyExchangeRate(3m);
			BankTransferForeignToForeign.AssertSellPropertiesAndRowFrom(100m, 0.333333m, 300m);
			BankTransferForeignToForeign.AssertBuyProperties(200m, 3m, 66.67m);
			BankTransferForeignToForeign.AssertRowTo(200m, 0.666667m, 300m);
			AssertEquals(-233.33m, BankTransferForeignToForeign.ExRateGainLoss);

			BankTransferLocalToForeign.SetSellAmount(300m);
			BankTransferLocalToForeign.SetBuyAmount(200m).SetBuyExchangeRate(3m);
			BankTransferLocalToForeign.AssertSellPropertiesAndRowFrom(300m, 1m, 300m);
			BankTransferLocalToForeign.AssertBuyProperties(200m, 3m, 66.67m);
			BankTransferLocalToForeign.AssertRowTo(200m, 0.666667m, 300m);
			AssertEquals(-233.33m, BankTransferLocalToForeign.ExRateGainLoss);
		}

		public void TestLocalBuyAmount()
		{
			SetupBankTransfers();
			AssertEquals("Precondition", true, BankTransferForeignToLocal.LocalBuyAmountInfo.ReadOnly);
			AssertEquals("Precondition", false, BankTransferForeignToForeign.LocalBuyAmountInfo.ReadOnly);
			AssertEquals("Precondition", false, BankTransferLocalToForeign.LocalBuyAmountInfo.ReadOnly);
			AssertEquals("Precondition", true, BankTransferLocalToLocal.LocalBuyAmountInfo.ReadOnly);

			BankTransferForeignToForeign.SetSellAmount(100m).SetSellExchangeRate(2m).SetLocalSellAmount(300m);
			BankTransferForeignToForeign.SetBuyAmount(200m).SetBuyExchangeRate(3m).SetLocalBuyAmount(400m);
			BankTransferForeignToForeign.AssertSellPropertiesAndRowFrom(100m, 0.333333m, 300m);
			BankTransferForeignToForeign.AssertBuyProperties(200m, 0.5m, 400m);
			BankTransferForeignToForeign.AssertRowTo(200m, 0.666667m, 300m);
			AssertEquals(100m, BankTransferForeignToForeign.ExRateGainLoss);

			BankTransferLocalToForeign.SetSellAmount(300m);
			BankTransferLocalToForeign.SetBuyAmount(200m).SetBuyExchangeRate(3m).SetLocalBuyAmount(400m);
			BankTransferLocalToForeign.AssertSellPropertiesAndRowFrom(300m, 1m, 300m);
			BankTransferLocalToForeign.AssertBuyProperties(200m, 0.5m, 400m);
			BankTransferLocalToForeign.AssertRowTo(200m, 0.666667m, 300m);
			AssertEquals(100m, BankTransferLocalToForeign.ExRateGainLoss);
		}

		#endregion

		public void TestBankTransferInit_WhenShouldCalculateExchangeVarianceIsUpdated_WithExchangeRate_ForeignToForeign()
		{
			TestObjectCreator.CreateExchangeRate(TestObjectCreator.USD, 2m);
			TestObjectCreator.CreateExchangeRate(TestObjectCreator.GBP, 3m);

			var bankTransfer = new BankTransfer(Factory, null);
			bankTransfer.BankTransferFromPK = TestObjectCreator.USDBankAccount.PK;
			bankTransfer.BankTransferToPK = TestObjectCreator.GBPBankAccount.PK;

			bankTransfer.SetSellAmountWithAsserts(100m);
			bankTransfer.AssertSellPropertiesAndRowFrom(100m, 2m, 50m);
			bankTransfer.AssertBuyPropertiesAndRowTo(150m, 3m, 50m);
			AssertEquals(0m, bankTransfer.ExRateGainLoss);

			bankTransfer.SetSellExchangeRateWithAsserts(4m);
			bankTransfer.AssertSellPropertiesAndRowFrom(100m, 4m, 25m);
			bankTransfer.AssertBuyPropertiesAndRowTo(75m, 3m, 25m);
			AssertEquals(0m, bankTransfer.ExRateGainLoss);

			bankTransfer.SetBuyExchangeRateWithAsserts(5m);
			bankTransfer.AssertSellPropertiesAndRowFrom(100m, 4m, 25m);
			bankTransfer.AssertBuyPropertiesAndRowTo(125m, 5m, 25m);
			AssertEquals(0m, bankTransfer.ExRateGainLoss);

			bankTransfer.ShouldCalculateExchangeVariance = true;

			bankTransfer.AssertSellPropertiesAndRowFrom(100m, 2m, 50m);
			bankTransfer.AssertBuyProperties(125m, 3m, 41.67m);
			bankTransfer.AssertRowTo(125m, 2.5m, 50m);
			AssertEquals(-8.33m, bankTransfer.ExRateGainLoss);

			bankTransfer.ShouldCalculateExchangeVariance = false;

			bankTransfer.AssertSellPropertiesAndRowFrom(100m, 2m, 50m);
			bankTransfer.AssertBuyPropertiesAndRowTo(125m, 2.5m, 50m);
			AssertEquals(0m, bankTransfer.ExRateGainLoss);
		}

		public void TestBankTransferInit_WhenShouldCalculateExchangeVarianceIsUpdated_WithExchangeRate_ForeignToLocal()
		{
			TestObjectCreator.CreateExchangeRate(TestObjectCreator.USD, 2m);

			var bankTransfer = new BankTransfer(Factory, null);
			bankTransfer.BankTransferFromPK = TestObjectCreator.USDBankAccount.PK;
			bankTransfer.BankTransferToPK = TestObjectCreator.AUDBankAccount.PK;

			bankTransfer.SetSellAmountWithAsserts(100m);
			bankTransfer.AssertSellPropertiesAndRowFrom(100m, 2m, 50m);
			bankTransfer.AssertBuyPropertiesAndRowTo(50m, 1m, 50m);
			AssertEquals(0m, bankTransfer.ExRateGainLoss);

			bankTransfer.SetSellExchangeRateWithAsserts(3m);
			bankTransfer.AssertSellPropertiesAndRowFrom(100m, 3m, 33.33m);
			bankTransfer.AssertBuyPropertiesAndRowTo(33.33m, 1m, 33.33m);
			AssertEquals(0m, bankTransfer.ExRateGainLoss);

			bankTransfer.ShouldCalculateExchangeVariance = true;

			bankTransfer.AssertSellPropertiesAndRowFrom(100m, 2m, 50m);
			bankTransfer.AssertBuyProperties(33.33m, 1m, 33.33m);
			bankTransfer.AssertRowTo(50m, 1m, 50m);
			AssertEquals(-16.67m, bankTransfer.ExRateGainLoss);

			bankTransfer.ShouldCalculateExchangeVariance = false;

			bankTransfer.AssertSellPropertiesAndRowFrom(100m, 2m, 50m);
			bankTransfer.AssertBuyPropertiesAndRowTo(50m, 1m, 50m);
			AssertEquals(0m, bankTransfer.ExRateGainLoss);
		}

		public void TestBankTransferInit_WhenShouldCalculateExchangeVarianceIsUpdated_ForeignToForeign()
		{
			var bankTransfer = new BankTransfer(Factory, null);
			bankTransfer.BankTransferFromPK = TestObjectCreator.USDBankAccount.PK;
			bankTransfer.BankTransferToPK = TestObjectCreator.GBPBankAccount.PK;

			bankTransfer.SetSellAmountWithAsserts(100m).SetSellExchangeRateWithAsserts(2m).SetBuyExchangeRateWithAsserts(3m);
			bankTransfer.AssertSellPropertiesAndRowFrom(100m, 2m, 50m);
			bankTransfer.AssertBuyPropertiesAndRowTo(150m, 3m, 50m);
			AssertEquals(0m, bankTransfer.ExRateGainLoss);

			bankTransfer.SetSellExchangeRateWithAsserts(4m);
			bankTransfer.AssertSellPropertiesAndRowFrom(100m, 4m, 25m);
			bankTransfer.AssertBuyPropertiesAndRowTo(75m, 3m, 25m);
			AssertEquals(0m, bankTransfer.ExRateGainLoss);

			bankTransfer.SetBuyExchangeRateWithAsserts(5m);
			bankTransfer.AssertSellPropertiesAndRowFrom(100m, 4m, 25m);
			bankTransfer.AssertBuyPropertiesAndRowTo(125m, 5m, 25m);
			AssertEquals(0m, bankTransfer.ExRateGainLoss);

			bankTransfer.ShouldCalculateExchangeVariance = true;

			bankTransfer.AssertSellPropertiesAndRowFrom(100m, 0m, 0m);
			bankTransfer.AssertBuyProperties(125m, 0m, 0m);
			bankTransfer.AssertRowTo(125m, 0m, 0m);
			AssertEquals(0m, bankTransfer.ExRateGainLoss);

			bankTransfer.ShouldCalculateExchangeVariance = false;

			bankTransfer.AssertSellPropertiesAndRowFrom(100m, 0m, 0m);
			bankTransfer.AssertBuyPropertiesAndRowTo(125m, 0m, 0m);
			AssertEquals(0m, bankTransfer.ExRateGainLoss);
		}

		public void TestBankTransferInit_WhenShouldCalculateExchangeVarianceIsUpdated_ForeignToLocal()
		{
			var bankTransfer = new BankTransfer(Factory, null);
			bankTransfer.BankTransferFromPK = TestObjectCreator.USDBankAccount.PK;
			bankTransfer.BankTransferToPK = TestObjectCreator.AUDBankAccount.PK;

			bankTransfer.SetSellAmountWithAsserts(100m).SetSellExchangeRate(2m);
			bankTransfer.AssertSellPropertiesAndRowFrom(100m, 2m, 50m);
			bankTransfer.AssertBuyPropertiesAndRowTo(50m, 1m, 50m);
			AssertEquals(0m, bankTransfer.ExRateGainLoss);

			bankTransfer.SetSellExchangeRateWithAsserts(3m);
			bankTransfer.AssertSellPropertiesAndRowFrom(100m, 3m, 33.33m);
			bankTransfer.AssertBuyPropertiesAndRowTo(33.33m, 1m, 33.33m);
			AssertEquals(0m, bankTransfer.ExRateGainLoss);

			bankTransfer.ShouldCalculateExchangeVariance = true;

			bankTransfer.AssertSellPropertiesAndRowFrom(100m, 0m, 0m);
			bankTransfer.AssertBuyProperties(33.33m, 1m, 33.33m);
			bankTransfer.AssertRowTo(33.33m, 1m, 33.33m);
			AssertEquals(33.33m, bankTransfer.ExRateGainLoss);

			bankTransfer.ShouldCalculateExchangeVariance = false;

			bankTransfer.AssertSellPropertiesAndRowFrom(100m, 0m, 0m);
			bankTransfer.AssertBuyPropertiesAndRowTo(0m, 1m, 0m);
			AssertEquals(0m, bankTransfer.ExRateGainLoss);
		}

		public void TestBankTransferInit_WhenBankTransferIsPosted_ForeignToForeign()
		{
			var bankTransfer = new BankTransfer(Factory, null);
			bankTransfer.BankTransferFromPK = TestObjectCreator.USDBankAccount.PK;
			bankTransfer.BankTransferToPK = TestObjectCreator.GBPBankAccount.PK;
			bankTransfer.ShouldCalculateExchangeVariance = true;
			bankTransfer.SetSellAmount(100m).SetSellExchangeRate(2m);
			bankTransfer.SetBuyAmount(200m).SetBuyExchangeRateWithAsserts(3m);
			bankTransfer.AssertSellPropertiesAndRowFrom(100m, 2m, 50m);
			bankTransfer.AssertBuyProperties(200m, 3m, 66.67m);
			bankTransfer.AssertRowTo(200m, 4m, 50m);
			AssertEquals(16.67m, bankTransfer.ExRateGainLoss);

			Factory.Save();

			AssertEquals(true, bankTransfer.IsPosted);
			AssertEquals(true, bankTransfer.TransferRowFrom.IsInDatabase);
			AssertEquals(true, bankTransfer.TransferRowTo.IsInDatabase);
			AssertEquals(true, bankTransfer.ExchangeDiff.IsInDatabase);

			var newFactory = new BusinessObjectFactory();
			var reloadedTransferRowFrom = newFactory.Load<BankTransferFromRow>(bankTransfer.TransferRowFrom.PK);
			var reloadedBankTransfer = new BankTransfer(Factory, reloadedTransferRowFrom);
			reloadedBankTransfer.AssertSellPropertiesAndRowFrom(100m, 2m, 50m);
			reloadedBankTransfer.AssertBuyProperties(200m, 2.999850m, 66.67m);
			reloadedBankTransfer.AssertRowTo(200m, 4m, 50m);
			AssertEquals(16.67m, reloadedBankTransfer.ExRateGainLoss);
		}

		public void TestBankTransferInit_WhenBankTransferIsPosted_ForeignToLocal()
		{
			var bankTransfer = new BankTransfer(Factory, null);
			bankTransfer.BankTransferFromPK = TestObjectCreator.USDBankAccount.PK;
			bankTransfer.BankTransferToPK = TestObjectCreator.AUDBankAccount.PK;
			bankTransfer.ShouldCalculateExchangeVariance = true;
			bankTransfer.SetSellAmount(100m).SetSellExchangeRate(2m);
			bankTransfer.SetBuyAmount(200m);
			bankTransfer.AssertSellPropertiesAndRowFrom(100m, 2m, 50m);
			bankTransfer.AssertBuyProperties(200m, 1m, 200m);
			bankTransfer.AssertRowTo(50m, 1m, 50m);
			AssertEquals(150m, bankTransfer.ExRateGainLoss);

			Factory.Save();

			AssertEquals(true, bankTransfer.IsPosted);
			AssertEquals(true, bankTransfer.TransferRowFrom.IsInDatabase);
			AssertEquals(true, bankTransfer.TransferRowTo.IsInDatabase);
			AssertEquals(true, bankTransfer.ExchangeDiff.IsInDatabase);

			var newFactory = new BusinessObjectFactory();
			var reloadedTransferRowFrom = newFactory.Load<BankTransferFromRow>(bankTransfer.TransferRowFrom.PK);
			var reloadedBankTransfer = new BankTransfer(Factory, reloadedTransferRowFrom);
			reloadedBankTransfer.AssertSellPropertiesAndRowFrom(100m, 2m, 50m);
			reloadedBankTransfer.AssertBuyProperties(200m, 1m, 200m);
			reloadedBankTransfer.AssertRowTo(50m, 1m, 50m);
			AssertEquals(150m, reloadedBankTransfer.ExRateGainLoss);
		}

		public void TestOnTransferRowToCurrencyInfoValueChanged()
		{
			TestObjectCreator.CreateExchangeRate(TestObjectCreator.USD, 2m);
			TestObjectCreator.CreateExchangeRate(TestObjectCreator.GBP, 3m);
			TestObjectCreator.CreateExchangeRate(TestObjectCreator.EUR, 4m);

			var bankTransfer = new BankTransfer(Factory, null);
			bankTransfer.BankTransferFromPK = TestObjectCreator.USDBankAccount.PK;
			bankTransfer.BankTransferToPK = TestObjectCreator.GBPBankAccount.PK;
			bankTransfer.ShouldCalculateExchangeVariance = true;
			bankTransfer.SetSellAmount(100m);
			bankTransfer.SetBuyAmountWithAsserts(200m);
			bankTransfer.AssertSellPropertiesAndRowFrom(100m, 2m, 50m);
			bankTransfer.AssertBuyProperties(200m, 3m, 66.67m);
			bankTransfer.AssertRowTo(200m, 4m, 50m);
			AssertEquals(16.67m, bankTransfer.ExRateGainLoss);

			bankTransfer.BankTransferToPK = TestObjectCreator.EURBankAccount.PK;

			bankTransfer.AssertSellPropertiesAndRowFrom(0m, 2m, 0m);
			bankTransfer.AssertBuyProperties(0m, 4m, 0m);
			bankTransfer.AssertRowTo(0m, 4m, 0m);
			AssertEquals(0m, bankTransfer.ExRateGainLoss);
		}

		protected override void SetUp()
		{
			var year = ZDateTime.Now.Year;
			TestObjectCreator.CreateTestPeriodsForEntireYear(year);
			TestObjectCreator.CreateTestPeriodsForEntireYear(year + 1);
		}

		void SetupBankTransfers()
		{
			BankTransferForeignToLocal = new BankTransfer(Factory, null);
			BankTransferForeignToLocal.BankTransferFromPK = TestObjectCreator.USDBankAccount.PK;
			BankTransferForeignToLocal.BankTransferToPK = TestObjectCreator.AUDBankAccount.PK;
			AssertEquals("Precondition", false, BankTransferForeignToLocal.ShouldCalculateExchangeVarianceInfo.ReadOnly);
			BankTransferForeignToLocal.ShouldCalculateExchangeVariance = true;

			BankTransferForeignToForeign = new BankTransfer(Factory, null);
			BankTransferForeignToForeign.BankTransferFromPK = TestObjectCreator.USDBankAccount.PK;
			BankTransferForeignToForeign.BankTransferToPK = TestObjectCreator.GBPBankAccount.PK;
			AssertEquals("Precondition", false, BankTransferForeignToForeign.ShouldCalculateExchangeVarianceInfo.ReadOnly);
			BankTransferForeignToForeign.ShouldCalculateExchangeVariance = true;

			BankTransferLocalToForeign = new BankTransfer(Factory, null);
			BankTransferLocalToForeign.BankTransferFromPK = TestObjectCreator.AUDBankAccount.PK;
			BankTransferLocalToForeign.BankTransferToPK = TestObjectCreator.USDBankAccount.PK;
			AssertEquals("Precondition", false, BankTransferLocalToForeign.ShouldCalculateExchangeVarianceInfo.ReadOnly);
			BankTransferLocalToForeign.ShouldCalculateExchangeVariance = true;

			BankTransferLocalToLocal = new BankTransfer(Factory, null);
			BankTransferLocalToLocal.BankTransferFromPK = TestObjectCreator.AUDBankAccount.PK;
			BankTransferLocalToLocal.BankTransferToPK = TestObjectCreator.AUDBankAccount2.PK;
			AssertEquals("Precondition", true, BankTransferLocalToLocal.ShouldCalculateExchangeVarianceInfo.ReadOnly);
		}

		BankTransfer BankTransferForeignToLocal;
		BankTransfer BankTransferForeignToForeign;
		BankTransfer BankTransferLocalToForeign;
		BankTransfer BankTransferLocalToLocal;

		TestObjectCreator TestObjectCreator => testObjectCreator ?? (testObjectCreator = new TestObjectCreator(Factory));
		TestObjectCreator testObjectCreator;
	}
}
