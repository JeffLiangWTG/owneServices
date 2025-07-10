using CargoWise.Types;
using Enterprise.Customs.DE.Business.Declaration;
using Enterprise.DocumentWrappers;
using Enterprise.DocumentWrappers.Testing;

namespace Enterprise.Customs.DE.Business.DocumentWrappers.Testing
{
	sealed class CombinedChargesTest : DocBaseWrapperTest
	{
		public void TestNew()
		{
			AssertNull(CombinedCharges.New(null, Factory));
		}

		public void TestType()
		{
			charge.J7_ChargeType = "A0";
			AssertEquals("A0", Wrapper.Type);
		}

		public void TestDescription()
		{
			charge.J7_ChargeType = Enterprise.Customs.Common.CustomsChargeTypeList.Codes.AdditionCharge;

			AssertNotEquals("Precondition", Enterprise.Customs.Common.CustomsChargeTypeList.Descriptions.AdditionCharge, Wrapper.Description);
			AssertEquals("Weitere Zusatzgebühren", Wrapper.Description);
		}

		public void TestAmount()
		{
			charge.J7_Amount = 1.234m;
			AssertEquals("1,23", Wrapper.Amount);
			charge2.J7_Amount = 0.01m;
			AssertEquals("1,24", Wrapper.Amount);
		}

		public void TestCurrency()
		{
			charge.J7_RX_NKCurrency = "EUR";
			AssertEquals("EUR", Wrapper.Currency);
		}

		public void TestAmountEUR()
		{
			CombineAssertions(() =>
			{
				charge.J7_Amount = 1.234m;
				AssertEquals("Rate == 0", "0,00", Wrapper.AmountEUR);

				charge.J7_ExchangeRate = 1.2345678m;
				AssertEquals("Rate > 0", "1,00", Wrapper.AmountEUR);
			});
		}

		public void TestRate()
		{
			charge.J7_ExchangeRate = 1.2345678m;
			AssertEquals("1,234568", Wrapper.Rate);
		}

		public void TestRateDate()
		{
			charge.J7_ExchangeRateDate = new ZDate(2022, 9, 14);
			AssertEquals("14.09.2022", Wrapper.RateDate);
		}

		protected override DocBaseWrapper GetNewDocumentWrapper() => CombinedCharges.New(new[] { charge, charge2 }, Factory);

		new CombinedCharges Wrapper => (CombinedCharges)base.Wrapper;

		protected override void SetUp()
		{
			base.SetUp();
			charge = Factory.NewWithValidTestData<InvoiceLineCharge>();
			charge2 = Factory.NewWithValidTestData<InvoiceLineCharge>();
		}

		InvoiceLineCharge charge;
		InvoiceLineCharge charge2;
	}
}
