using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.Accounting.Testing
{
	[TestedType(typeof(DocCheque))]
	sealed class DocChequeTestCase : NonPersistentBusinessObjectTestCase
	{
		protected override void SetUp()
		{
			base.SetUp();
			CurrentCurrencySymbol = System.Threading.Thread.CurrentThread.CurrentCulture.NumberFormat.CurrencySymbol;
		}
		string CurrentCurrencySymbol;

		protected override void TearDown()
		{
			AssertEquals("Currency symbol has changed during test and not been reset", CurrentCurrencySymbol, System.Threading.Thread.CurrentThread.CurrentCulture.NumberFormat.CurrencySymbol);
			base.TearDown();
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return new DocCheque("", 0M, null, 0);
		}

		public void TestNegativeAmount()
		{
			var aUD = Factory.LoadTop1<RefCurrency>(new ZQuery(RefCurrencySchema.RX_Code, "AUD"));
			DocCurrency aUDWrapper = DocCurrency.New(aUD, Factory);

			AssertExceptionThrown("Should throw exception if negative amount is passed", typeof(InvalidDocumentWrapperParameterException), "Cheque to \'Me\' has negative amount: -2345.67. Please confirm this is a valid payment.", () => new DocCheque("Me", -2345.67M, aUDWrapper, 70));
		}

		public void TestChequeAmountInWords()
		{
			var aUD = Factory.LoadTop1<RefCurrency>(new ZQuery(RefCurrencySchema.RX_Code, "AUD"));
			DocCurrency aUDWrapper = DocCurrency.New(aUD, Factory);

			DocCheque docCheque = new DocCheque("Me", 2345.67M, aUDWrapper, 40);
			AssertEquals("TWO THOUSAND, THREE HUNDRED AND FORTY FIVE DOLLARS AND 67 CENTS", docCheque.ChequeAmountInWords);
			AssertEquals("TWO THOUSAND, THREE HUNDRED AND FORTY", docCheque.FirstLineAmountInWords);
			AssertEquals("FIVE DOLLARS AND 67 CENTS", docCheque.SecondLineAmountInWords);

			docCheque = new DocCheque("Me", 2345.67M, aUDWrapper, 70);
			AssertEquals("TWO THOUSAND, THREE HUNDRED AND FORTY FIVE DOLLARS AND 67 CENTS", docCheque.ChequeAmountInWords);
			AssertEquals("TWO THOUSAND, THREE HUNDRED AND FORTY FIVE DOLLARS AND 67 CENTS", docCheque.FirstLineAmountInWords);
			AssertEquals("", docCheque.SecondLineAmountInWords);
		}

		public void TestChequeAmountInWordsWithSmallWidth()
		{
			var aUD = Factory.LoadTop1<RefCurrency>(new ZQuery(RefCurrencySchema.RX_Code, "AUD"));
			DocCurrency aUDWrapper = DocCurrency.New(aUD, Factory);

			DocCheque docCheque = new DocCheque("Me", 3M, aUDWrapper, 2);
			AssertEquals("THREE DOLLARS ONLY", docCheque.FirstLineAmountInWords);
			AssertEquals("", docCheque.SecondLineAmountInWords);

			docCheque = new DocCheque("Me", 3M, aUDWrapper, 5);
			AssertEquals("THREE", docCheque.FirstLineAmountInWords);
			AssertEquals("DOLLARS ONLY", docCheque.SecondLineAmountInWords);

			docCheque = new DocCheque("Me", 3M, aUDWrapper, 6);
			AssertEquals("THREE", docCheque.FirstLineAmountInWords);
			AssertEquals("DOLLARS ONLY", docCheque.SecondLineAmountInWords);
		}

		public void TestPaymentCurrency()
		{
			DocCheque docCheque = new DocCheque("", 0M, null, 40);
			AssertNull(docCheque.PaymentCurrency);

			var aUD = Factory.LoadTop1<RefCurrency>(new ZQuery(RefCurrencySchema.RX_Code, "AUD"));
			DocCurrency aUDWrapper = DocCurrency.New(aUD, Factory);

			docCheque = new DocCheque("Me", 2345.67M, aUDWrapper, 40);
			AssertEquals("AUD", docCheque.PaymentCurrency.Code);
		}

		public void TestChequePayTo()
		{
			DocCheque docCheque = new DocCheque("Me", 2345.67M, null, 40);
			AssertEquals("Me", docCheque.ChequePayTo);
			AssertEquals("Me", docCheque.FirstLineChequePayTo);
			AssertEquals("", docCheque.SecondLineChequePayTo);

			docCheque = new DocCheque("Testing ChequePayTo field is split to 2 lines", 1000M, null, 40);
			AssertEquals("Testing ChequePayTo", docCheque.FirstLineChequePayTo);
			AssertEquals("field is split to 2 lines", docCheque.SecondLineChequePayTo);
		}

		public void TestChequeAmount()
		{
			DocCheque docCheque = new DocCheque("Me", 100M, null, 40);
			AssertEquals(100M, docCheque.ChequeAmount);
		}

		public void TestChequeAmountFormatted()
		{
			var aUD = Factory.LoadTop1<RefCurrency>(new ZQuery(RefCurrencySchema.RX_Code, "AUD"));
			DocCurrency aUDWrapper = DocCurrency.New(aUD, Factory);
			DocCheque docCheque = new DocCheque("Me", 21600.95m, aUDWrapper, 40);
			aUD.RX_SubUnitRatio = 1000;
			AssertEquals("***21,600.950", docCheque.ChequeAmountFormatted);
			aUD.RX_SubUnitRatio = 1;
			AssertEquals("***21,601", docCheque.ChequeAmountFormatted);

			using (new CurrentCompanyCountryCultureChanger(Core.Constants.CountryCodes.Iceland))
			{
				AssertEquals("Formatting for Iceland", "***21.601", docCheque.ChequeAmountFormatted);
			}
		}

		public void TestTodaysDateSplit()
		{
			DocCheque docCheque = new DocCheque("Me", 100M, null, 40);
			ZString todaysDate = ZDateTime.Today.ToString("ddMMyy");
			AssertEquals(todaysDate[0] + "   " + todaysDate[1] + "   " +
							todaysDate[2] + "   " + todaysDate[3] + "   " +
							todaysDate[4] + "   " + todaysDate[5], docCheque.TodaysDateSplit);
		}

		public void TestTodaysDateSplitDDMMYYYY()
		{
			DocCheque docCheque = new DocCheque("Me", 100M, null, 40);
			ZString todaysDate = ZDateTime.Today.ToString("ddMMyyyy");
			AssertEquals(todaysDate[0] + " " + todaysDate[1] + " " +
							todaysDate[2] + " " + todaysDate[3] + " " +
							todaysDate[4] + " " + todaysDate[5] + " " +
							todaysDate[6] + " " + todaysDate[7], docCheque.TodaysDateSplitDDMMYYYY);
		}

		public void TestNumberOfPowersOfTen()
		{
			DocCheque cheque = new DocCheque("Me", 9801234.56m, null, 40);
			AssertEquals("Should be 9 x 1,000,000", "nine", cheque.Millions);
			AssertEquals("Should be 8 x 100,000", "eight", cheque.HundredThousands);
			AssertEquals("Should be 0 x 10,000", "zero", cheque.TenThousands);
			AssertEquals("Should be 1 x 1,000", "one", cheque.Thousands);
			AssertEquals("Should be 2 x 100", "two", cheque.Hundreds);
			AssertEquals("Should be 3 x 10", "three", cheque.Tens);
			AssertEquals("Should be 4 x 1", "four", cheque.Ones);
		}

		public void TestCents()
		{
			DocCheque cheque = new DocCheque("Me", 9801234.56m, null, 40);
			AssertEquals("Should be 56", "56", cheque.Cents);

			cheque = new DocCheque("Me", 9801234.5665m, null, 40);
			AssertEquals("Should be 56", "56", cheque.Cents);

			cheque = new DocCheque("Me", 9801234.5m, null, 40);
			AssertEquals("Should be 50", "50", cheque.Cents);

			cheque = new DocCheque("Me", 9801234m, null, 40);
			AssertEquals("Should be 00", "00", cheque.Cents);
		}
	}
}
