using CargoWise.Types;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Client.JAS.Business.Matching.Testing
{
	public class MatchedDataLineTest : TestCase
	{
		public void TestMatchedDataLine()
		{
			string rawString = "AUCORTESTC          23.45+03/10/2000USDINV000This is the denomination          ADJ            M03/10/1981MAWB101       HAWB101       INV00001      P            0.0+AUD";
			MatchedDataLine line = new MatchedDataLine(rawString);
			AssertEquals("AUCOR", line.DestinationSubsidiary);
			AssertEquals("TESTC", line.CounterpartSubsidiary);
			AssertEquals(23.45M, line.Amount);
			AssertEquals(ZBool.False, line.CreditNote);
			AssertEquals(new ZDateTime(2000, 10, 3), line.MaturityDate);
			AssertEquals("USD", line.CurrencyCode);
			AssertEquals("INV000", line.OtherRefNumber);
			AssertEquals("This is the denomination", line.Denomination);
			AssertEquals("M", line.Category);
			AssertEquals(new ZDateTime(1981, 10, 3), line.InvoiceDate);
			AssertEquals("MAWB101", line.MasterBill);
			AssertEquals("HAWB101", line.HouseBill);
			AssertEquals(LedgerTypes.AccountsPayable, line.Ledger);
			AssertEquals(0M, line.ExchangeValue);
			AssertEquals("AUD", line.CurrencyOfExchangeValue);
			AssertEquals(TransactionTypes.AdjustmentNote, line.TransactionType);
		}

		public void TestMatchedDataLine2()
		{
			string rawString = "IDJKTIDBAL         111.23-31/10/2000RUPINV000This is the denomination 2        INV            A30/09/2000MAWB201       HAWB201       INV00002      R          230.0-RUP";
			MatchedDataLine line = new MatchedDataLine(rawString);
			AssertEquals("IDJKT", line.DestinationSubsidiary);
			AssertEquals("IDBAL", line.CounterpartSubsidiary);
			AssertEquals(111.23M, line.Amount);
			AssertEquals(ZBool.True, line.CreditNote);
			AssertEquals(new ZDateTime(2000, 10, 31), line.MaturityDate);
			AssertEquals("RUP", line.CurrencyCode);
			AssertEquals("INV000", line.OtherRefNumber);
			AssertEquals("This is the denomination 2", line.Denomination);
			AssertEquals("A", line.Category);
			AssertEquals(new ZDateTime(2000, 9, 30), line.InvoiceDate);
			AssertEquals("MAWB201", line.MasterBill);
			AssertEquals("HAWB201", line.HouseBill);
			AssertEquals(LedgerTypes.AccountsReceivable, line.Ledger);
			AssertEquals(230M, line.ExchangeValue);
			AssertEquals("RUP", line.CurrencyOfExchangeValue);
			AssertEquals(TransactionTypes.Invoice, line.TransactionType);
		}
	}
}
