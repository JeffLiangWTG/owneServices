using System.Linq;
using Enterprise.Customs.IE.Business.Testing;

namespace Enterprise.Customs.IE.Business.AIS.Testing
{
	sealed class CustomsValuationProviderTest : Customs.Business.Testing.DataProviderTestCase<CustomsValuationProvider>
	{
		public void TestValuationMethod()
		{
			var invoiceLine = EntryLineWrapper.RandomInvoiceLine;
			invoiceLine.JI_ValuationCode = "A";

			AssertEquals("ValuationMethod", "A", GetProvider().ValuationMethod);
		}

		public void TestItemAmount()
		{
			var invoiceLine = EntryLineWrapper.RandomInvoiceLine;
			invoiceLine.JI_LinePrice = 12;
			invoiceLine.InvoiceHeader.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.EuropeanUnion;

			AssertEquals("ItemAmount", 12m, GetProvider().ItemAmount);
		}

		public void TestPreference()
		{
			var invoiceLine = EntryLineWrapper.RandomInvoiceLine;
			invoiceLine.JI_PrimaryPreference = "ref";

			AssertEquals("Preference", "ref", GetProvider().Preference);
		}

		public void TestPostalvalueAmount()
		{
			AssertEquals("PostalvalueAmount", decimal.Zero, GetProvider().PostalvalueAmount);
		}

		public void TestPostalvalueCurrency()
		{
			AssertNull("PostalvalueCurrency", GetProvider().PostalvalueCurrency);
		}

		public void TestAdditionsAndDeductions()
		{
			var invoiceLine = EntryLineWrapper.RandomInvoiceLine;
			var invoiceHeader = invoiceLine.InvoiceHeader;
			invoiceHeader.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.EuropeanUnion;
			var charge = invoiceLine.Charges.AddNew();
			charge.J7_ChargeType = "CT1";
			charge.J7_Amount = 10m;

			var charge2 = invoiceLine.Charges.AddNew();
			charge2.J7_ChargeType = "CT1";
			charge2.J7_Amount = 20m;

			var apportionedCharge1 = invoiceLine.ApportionedCharges.AddNew();
			apportionedCharge1.J7_ChargeType = "CT1";
			apportionedCharge1.J7_Amount = 1m;
			apportionedCharge1.J7_RX_NKCurrency = Core.Constants.CurrencyCodes.EuropeanUnion;

			var apportionedCharge2 = invoiceLine.ApportionedCharges.AddNew();
			apportionedCharge2.J7_ChargeType = "CT2";
			apportionedCharge2.J7_Amount = 2m;
			apportionedCharge2.J7_RX_NKCurrency = Core.Constants.CurrencyCodes.EuropeanUnion;

			CombineAssertions(() =>
			{
				AssertEquals("AdditionsAndDeductions", 2, Provider.AdditionsAndDeductions.Count);
				AssertContainsExactElementsInAnyOrder("There should be 2 AdditionsAndDeductions elements as both Charges and Apportioned Charges should be merged by charge type.", new string[] { "CT1|31", "CT2|2" }, Provider.AdditionsAndDeductions.Select(x => x.Code + "|" + x.Amount.ToString()));
			});
		}

		protected override CustomsValuationProvider GetProvider() => CreateProvider(EntryLineWrapper);
		CustomsValuationProvider CreateProvider(EntryLineWrapper wrapper) => new CustomsValuationProvider(wrapper);

		void SetupData()
		{
			(_, entryLineWrapper) = MessageProviderTestHelper.SetupBasicTestBizObjs(Factory);
		}

		EntryLineWrapper EntryLineWrapper
		{
			get
			{
				if (entryLineWrapper == null)
				{
					SetupData();
				}
				return entryLineWrapper;
			}
		}
		EntryLineWrapper entryLineWrapper;
	}
}
