using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.CashBook.Transfer.Testing
{
	public static class BankTranferExtensionsForTest
	{
		static void DoAssert(BankTransfer transfer)
		{
			Assertion.AssertEquals("Invoice amounts balance to 0", transfer.TransferRowFrom.AH_InvoiceAmount, -transfer.TransferRowTo.AH_InvoiceAmount);
			Assertion.AssertEquals("Local amounts are matched", transfer.TransferRowFrom.AH_LocalExTaxAmount, transfer.TransferRowTo.AH_LocalExTaxAmount);
		}

		public static BankTransfer SetBuyAmountWithAsserts(this BankTransfer transfer, decimal amount)
		{
			transfer.BuyAmount = amount;
			DoAssert(transfer);
			return transfer;
		}

		public static BankTransfer SetSellAmountWithAsserts(this BankTransfer transfer, ZDecimal amount)
		{
			transfer.SellAmount = amount;
			DoAssert(transfer);
			return transfer;
		}

		public static BankTransfer SetLocalBuyAmountWithAsserts(this BankTransfer transfer, ZDecimal amount)
		{
			transfer.LocalBuyAmount = amount;
			DoAssert(transfer);
			return transfer;
		}

		public static BankTransfer SetLocalSellAmountWithAsserts(this BankTransfer transfer, ZDecimal amount)
		{
			transfer.LocalSellAmount = amount;
			DoAssert(transfer);
			return transfer;
		}

		public static BankTransfer SetFromBankAccountWithAsserts(this BankTransfer transfer, ZGuid bankPK)
		{
			transfer.BankTransferFromPK = bankPK;
			DoAssert(transfer);
			return transfer;
		}

		public static BankTransfer SetToBankAccountWithAsserts(this BankTransfer transfer, ZGuid bankPK)
		{
			transfer.BankTransferToPK = bankPK;
			DoAssert(transfer);
			return transfer;
		}

		public static BankTransfer SetBuyExchangeRateWithAsserts(this BankTransfer transfer, ZDecimal rate)
		{
			transfer.BuyExchangeRate = rate;
			DoAssert(transfer);
			return transfer;
		}

		public static BankTransfer SetSellExchangeRateWithAsserts(this BankTransfer transfer, ZDecimal rate)
		{
			transfer.SellExchangeRate = rate;
			DoAssert(transfer);
			return transfer;
		}

		public static BankTransfer SetBuyAmount(this BankTransfer transfer, ZDecimal amount)
		{
			transfer.BuyAmount = amount;
			return transfer;
		}

		public static BankTransfer SetSellAmount(this BankTransfer transfer, ZDecimal amount)
		{
			transfer.SellAmount = amount;
			return transfer;
		}

		public static BankTransfer SetLocalBuyAmount(this BankTransfer transfer, ZDecimal amount)
		{
			transfer.LocalBuyAmount = amount;
			return transfer;
		}

		public static BankTransfer SetLocalSellAmount(this BankTransfer transfer, ZDecimal amount)
		{
			transfer.LocalSellAmount = amount;
			return transfer;
		}

		public static BankTransfer SetBuyExchangeRate(this BankTransfer transfer, ZDecimal rate)
		{
			transfer.BuyExchangeRate = rate;
			return transfer;
		}

		public static BankTransfer SetSellExchangeRate(this BankTransfer transfer, ZDecimal rate)
		{
			transfer.SellExchangeRate = rate;
			return transfer;
		}

		public static void AssertSellProperties(this BankTransfer transfer,
			ZDecimal sellAmount, ZDecimal sellExchangeRate, ZDecimal sellLocal)
		{
			Assertion.AssertEquals(sellAmount, transfer.SellAmount);
			Assertion.AssertEquals(sellExchangeRate, transfer.SellExchangeRate);
			Assertion.AssertEquals(sellLocal, transfer.LocalSellAmount);
		}

		public static void AssertBuyProperties(this BankTransfer transfer,
			ZDecimal buyAmount, ZDecimal buyExchangeRate, ZDecimal buyLocal)
		{
			Assertion.AssertEquals(buyAmount, transfer.BuyAmount);
			Assertion.AssertEquals(buyExchangeRate, transfer.BuyExchangeRate);
			Assertion.AssertEquals(buyLocal, transfer.LocalBuyAmount);
		}

		public static void AssertRowTo(this BankTransfer transfer,
			ZDecimal buyAmount, ZDecimal buyExchangeRate, ZDecimal buyLocal)
		{
			Assertion.AssertEquals(buyAmount, transfer.TransferRowTo.AH_OSExTaxAmount);
			Assertion.AssertEquals(buyExchangeRate, transfer.TransferRowTo.AH_ExchangeRate);
			Assertion.AssertEquals(buyLocal, transfer.TransferRowTo.AH_LocalExTaxAmount);
		}

		public static void AssertRowFrom(this BankTransfer transfer,
			ZDecimal sellAmount, ZDecimal sellExchangeRate, ZDecimal sellLocal)
		{
			Assertion.AssertEquals(sellAmount, transfer.TransferRowFrom.AH_OSExTaxAmount);
			Assertion.AssertEquals(sellExchangeRate, transfer.TransferRowFrom.AH_ExchangeRate);
			Assertion.AssertEquals(sellLocal, transfer.TransferRowFrom.AH_LocalExTaxAmount);
		}

		public static void AssertSellPropertiesAndRowFrom(this BankTransfer transfer,
			ZDecimal sellAmount, ZDecimal sellExchangeRate, ZDecimal sellLocal)
		{
			AssertSellProperties(transfer, sellAmount, sellExchangeRate, sellLocal);
			AssertRowFrom(transfer, sellAmount, sellExchangeRate, sellLocal);
		}

		public static void AssertBuyPropertiesAndRowTo(this BankTransfer transfer,
			ZDecimal buyAmount, ZDecimal buyExchangeRate, ZDecimal buyLocal)
		{
			AssertBuyProperties(transfer, buyAmount, buyExchangeRate, buyLocal);
			AssertRowTo(transfer, buyAmount, buyExchangeRate, buyLocal);
		}
	}
}
