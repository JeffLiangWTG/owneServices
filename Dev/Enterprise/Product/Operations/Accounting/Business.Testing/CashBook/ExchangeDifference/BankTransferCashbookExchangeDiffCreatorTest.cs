using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business.CashBook.Transfer;
using Enterprise.Accounting.Registry.Business;
using NUnit.Framework;
using static Enterprise.Core.Constants;

namespace Enterprise.Accounting.Business.CashBook.ExchangeDifference.Testing
{
	public class BankTransferCashbookExchangeDiffCreatorTest : TestCaseWithFactory
	{
		[TestDate(2023, 9, 1)]
		public void TestCreateBankTransferCashbookExchangeDiff_WhenExRateGainLossIsPositive()
		{
			var transfer = CreateBankTransfer(buyLocalAmount: 200m, sellLocalAmount: 100m, toBankAccountPK: TestObjectCreator.USDBankAccount.PK);
			BankTransferCashbookExchangeDiffCreator.Fill(transfer, transfer.ExchangeDiff);

			AssertCashbookExchangeDiff("Exchange Gain on Bank Transfer 00001000 / Test Reference", transfer.ExchangeDiff, 100m, 0m, TestObjectCreator.GLHeader2.PK, TestObjectCreator.USDBankAccount.PK);
		}

		[TestDate(2023, 9, 1)]
		public void TestCreateBankTransferCashbookExchangeDiff_WhenExRateGainLossIsNegative()
		{
			var transfer = CreateBankTransfer(buyLocalAmount: 100m, sellLocalAmount: 200m, toBankAccountPK: TestObjectCreator.USDBankAccount.PK);
			BankTransferCashbookExchangeDiffCreator.Fill(transfer, transfer.ExchangeDiff);

			AssertCashbookExchangeDiff("Exchange Loss on Bank Transfer 00001000 / Test Reference", transfer.ExchangeDiff, -100m, 0m, TestObjectCreator.GLHeader1.PK, TestObjectCreator.USDBankAccount.PK);
		}

		[TestDate(2023, 9, 1)]
		public void TestCreateBankTransferCashbookExchangeDiff_WhenToBankAccountIsLocalCurrency()
		{
			var transfer = CreateBankTransfer(buyLocalAmount: 200m, sellLocalAmount: 100m, toBankAccountPK: TestObjectCreator.AUDBankAccount.PK);
			BankTransferCashbookExchangeDiffCreator.Fill(transfer, transfer.ExchangeDiff);
			AssertCashbookExchangeDiff("Exchange Gain on Bank Transfer 00001000 / Test Reference", transfer.ExchangeDiff, 100m, 100m, TestObjectCreator.GLHeader2.PK, TestObjectCreator.AUDBankAccount.PK);
		}

		protected override void SetUp()
		{
			base.SetUp();

			AccountingConfigurationRegistry.Instance.RealizedExchangeLossAccount.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, TestObjectCreator.GLHeader1.PK.ToGuid());
			AccountingConfigurationRegistry.Instance.RealizedExchangeGainAccount.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, TestObjectCreator.GLHeader2.PK.ToGuid());
		}

		void AssertCashbookExchangeDiff(ZString expectedDesc, CashbookExchangeDiff exchangeDiff, ZDecimal expectedAmount, ZDecimal expectedOSAmount, ZGuid expectedGLAccount, ZGuid expectBankAccount)
		{
			AssertCashbookExchangeDiffCommonProperties(exchangeDiff);
			AssertEquals(expectedDesc, exchangeDiff.AH_Desc);
			AssertEquals(expectedAmount, exchangeDiff.AH_InvoiceAmount);
			AssertEquals(expectedAmount, exchangeDiff.AH_LocalTotal);
			AssertEquals(expectedOSAmount, exchangeDiff.AH_OSTotal);
			AssertEquals(expectedGLAccount, exchangeDiff.AH_AG);
			AssertEquals(expectBankAccount, exchangeDiff.AH_AB);
		}

		void AssertCashbookExchangeDiffCommonProperties(CashbookExchangeDiff exchangeDiff)
		{
			AssertEquals(TransactionCategory.Codes.RealizedExchangeGainLoss, exchangeDiff.AH_TransactionCategory);
			AssertEquals(ZDateTime.Today.AddDays(-1), exchangeDiff.AH_InvoiceDate);
			AssertEquals(ZDateTime.Today.AddDays(-2), exchangeDiff.AH_PostDate);
			AssertEquals(TestObjectCreator.AUD.Code, exchangeDiff.AH_RX_NKTransactionCurrency);
			AssertEquals(1m, exchangeDiff.AH_ExchangeRate);
			AssertEquals(0m, exchangeDiff.AH_OutstandingAmount);
			AssertEquals((byte)7, exchangeDiff.AH_TransactionCount);
		}

		BankTransfer CreateBankTransfer(ZDecimal buyLocalAmount, ZDecimal sellLocalAmount, ZGuid toBankAccountPK)
		{
			var transfer = TestObjectCreator.CreateBankTransfer(ZDateTime.Today, TestObjectCreator.AUDBankAccount.PK, toBankAccountPK, 2500m, 1.0m);
			transfer.ShouldCalculateExchangeVariance = true;
			transfer.LocalBuyAmount = buyLocalAmount;
			transfer.LocalSellAmount = sellLocalAmount;
			transfer.TransactionDate = ZDateTime.Today.AddDays(-1);
			transfer.AH_PostDate = ZDateTime.Today.AddDays(-2);
			transfer.Reference = "Test Reference";
			transfer.TransactionNumber = "00001000";

			return transfer;
		}

		#region Creator

		BankTransferCashbookExchangeDiffCreator BankTransferCashbookExchangeDiffCreator => new BankTransferCashbookExchangeDiffCreator();

		TestObjectCreator TestObjectCreator => testObjectCreator ?? (testObjectCreator = new TestObjectCreator(Factory));
		TestObjectCreator testObjectCreator;

		#endregion
	}
}
