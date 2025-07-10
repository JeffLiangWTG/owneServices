using NUnit.Framework;

namespace Enterprise.Customs.BR.Business.Testing
{
	[TestedType(typeof(InvoiceLineCharge))]
	class InvoiceLineChargeTest : Customs.Business.Testing.BaseInvoiceLineChargeTest
	{
		public void TestTypeDecider()
		{
			Assert("Update BaseInvoiceChargeTypeDecider to include a decider for this class", Factory.New(typeof(Customs.Business.BaseInvoiceLineCharge)).GetType() == GetExpectedBusinessObjectType());
		}

		public void TestDefaultCurrencyAndReadOnly()
		{
			var lineCharge = Factory.New<InvoiceLineCharge>();

			lineCharge.J7_RX_NKCurrency = Core.Constants.CurrencyCodes.Zimbabwe;
			lineCharge.J7_ChargeType = ImportCustomsChargeTypeList.Codes.FreightInNationalTerritory;
			Assert("J7_RX_NKCurrency must NOT be ReadOnly for FNT", !lineCharge.J7_RX_NKCurrencyInfo.ReadOnly);
			AssertEquals("J7_RX_NKCurrency must be BRL for FCO", Core.Constants.CurrencyCodes.Zimbabwe, lineCharge.J7_RX_NKCurrency);

			lineCharge.J7_ChargeType = ImportCustomsChargeTypeList.Codes.FreightComponents;
			Assert("J7_RX_NKCurrency must NOT be ReadOnly for FCO", !lineCharge.J7_RX_NKCurrencyInfo.ReadOnly);
			AssertEquals("J7_RX_NKCurrency must be BRL for FCO", Core.Constants.CurrencyCodes.Brazil, lineCharge.J7_RX_NKCurrency);

			lineCharge.J7_RX_NKCurrency = Core.Constants.CurrencyCodes.Zimbabwe;
			lineCharge.J7_ChargeType = ImportCustomsChargeTypeList.Codes.OtherExpensesICMS;
			Assert("J7_RX_NKCurrency must be ReadOnly for EIC", lineCharge.J7_RX_NKCurrencyInfo.ReadOnly);
			AssertEquals("J7_RX_NKCurrency must be BRL for EIC", Core.Constants.CurrencyCodes.Brazil, lineCharge.J7_RX_NKCurrency);
		}

		public void TestIsFreightComponents()
		{
			var lineCharge = Factory.New<InvoiceLineCharge>();

			lineCharge.J7_ChargeType = ImportCustomsChargeTypeList.Codes.FreightInNationalTerritory;
			Assert("IsFreightComponents must be False", !lineCharge.IsFreightComponents);

			lineCharge.J7_ChargeType = ImportCustomsChargeTypeList.Codes.FreightComponents;
			Assert("IsFreightComponents must be True", lineCharge.IsFreightComponents);
		}

		public void TestIsOtherExpensesICMS()
		{
			var lineCharge = Factory.New<InvoiceLineCharge>();

			lineCharge.J7_ChargeType = ImportCustomsChargeTypeList.Codes.FreightInNationalTerritory;
			Assert("IsOtherExpensesICMS must be False", !lineCharge.IsOtherExpensesICMS);

			lineCharge.J7_ChargeType = ImportCustomsChargeTypeList.Codes.OtherExpensesICMS;
			Assert("IsOtherExpensesICMS must be True", lineCharge.IsOtherExpensesICMS);
		}
	}
}
