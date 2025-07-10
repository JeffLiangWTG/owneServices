using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Accounting.Business.CashBook.Transfer.Testing
{
	public class BankTransferIncludeExchangeVarianceInRowFromStrategyTest : TestCaseWithFactory
	{
		public void TestGenerateReverseTransaction_ForeignToLocal()
		{
			SetupBankTransfers();
			BankTransferForeignToLocal.SetSellAmount(100m).SetSellExchangeRate(2m);
			BankTransferForeignToLocal.SetBuyAmount(200m);

			BankTransferForeignToLocal.AssertSellProperties(100m, 2m, 50m);
			BankTransferForeignToLocal.AssertBuyProperties(200m, 1m, 200m);
			AssertEquals(150m, BankTransferForeignToLocal.ExRateGainLoss);
			BankTransferForeignToLocal.AssertRowFrom(100m, 2m, 50m);
			BankTransferForeignToLocal.AssertRowTo(50m, 1m, 50m);

			var reversal = GenerateReverseTransaction(BankTransferForeignToLocal);

			reversal.AssertSellProperties(200m, 1m, 200m);
			reversal.AssertBuyPropertiesAndRowTo(100m, 2m, 50m);
			AssertEquals(-150m, reversal.ExRateGainLoss);
			reversal.AssertRowFrom(50m, 1m, 50m);
			reversal.AssertRowTo(100m, 2m, 50m);
		}

		public void TestGenerateReverseTransaction_LocalToForeign()
		{
			SetupBankTransfers();
			BankTransferLocalToForeign.SetSellAmount(50m);
			BankTransferLocalToForeign.SetBuyAmount(200m).SetBuyExchangeRate(2m);

			BankTransferLocalToForeign.AssertSellProperties(50m, 1m, 50m);
			BankTransferLocalToForeign.AssertBuyProperties(200m, 2m, 100m);
			AssertEquals(50m, BankTransferLocalToForeign.ExRateGainLoss);
			BankTransferLocalToForeign.AssertRowFrom(50m, 1m, 50m);
			BankTransferLocalToForeign.AssertRowTo(200m, 4m, 50m);

			var reversal = GenerateReverseTransaction(BankTransferLocalToForeign);

			reversal.AssertSellProperties(200m, 2m, 100m);
			reversal.AssertBuyPropertiesAndRowTo(50m, 1m, 50m);
			AssertEquals(-50m, reversal.ExRateGainLoss);
			reversal.AssertRowFrom(200m, 4m, 50m);
			reversal.AssertRowTo(50m, 1m, 50m);
		}

		public void TestGenerateReverseTransaction_ForeignToForeign()
		{
			SetupBankTransfers();
			BankTransferForeignToForeign.SetSellAmount(100m).SetSellExchangeRate(2m);
			BankTransferForeignToForeign.SetBuyAmount(200m).SetBuyExchangeRate(3m);

			BankTransferForeignToForeign.AssertSellProperties(100m, 2m, 50m);
			BankTransferForeignToForeign.AssertBuyProperties(200m, 3m, 66.67m);
			AssertEquals(16.67m, BankTransferForeignToForeign.ExRateGainLoss);
			BankTransferForeignToForeign.AssertRowFrom(100m, 2m, 50m);
			BankTransferForeignToForeign.AssertRowTo(200m, 4m, 50m);

			var reversal = GenerateReverseTransaction(BankTransferForeignToForeign);

			reversal.AssertSellProperties(200m, 2.999850m, 66.67m);
			reversal.AssertBuyPropertiesAndRowTo(100m, 2m, 50m);
			AssertEquals(-16.67m, reversal.ExRateGainLoss);
			reversal.AssertRowFrom(200m, 4m, 50m);
			reversal.AssertRowTo(100m, 2m, 50m);
		}

		public void TestGenerateReverseTransaction_WithExchangeRate_ForeignToLocal()
		{
			TestObjectCreator.CreateExchangeRate(TestObjectCreator.USD, 7m);

			TestGenerateReverseTransaction_ForeignToLocal();
		}

		public void TestGenerateReverseTransaction_WithExchangeRate_LocalToForeign()
		{
			TestObjectCreator.CreateExchangeRate(TestObjectCreator.USD, 7m);

			TestGenerateReverseTransaction_LocalToForeign();
		}

		public void TestGenerateReverseTransaction_WithExchangeRate_ForeignToForeign()
		{
			TestObjectCreator.CreateExchangeRate(TestObjectCreator.USD, 7m);
			TestObjectCreator.CreateExchangeRate(TestObjectCreator.GBP, 8m);

			TestGenerateReverseTransaction_ForeignToForeign();
		}

		protected override void SetUp()
		{
			var year = ZDateTime.Now.Year;
			TestObjectCreator.CreateTestPeriodsForEntireYear(year);
			TestObjectCreator.CreateTestPeriodsForEntireYear(year + 1);
		}

		BankTransfer GenerateReverseTransaction(BankTransfer bankTransfer)
		{
			bankTransfer.GenerateReverseTransaction(true);
			return bankTransfer.ReverseTransaction as BankTransfer;
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
