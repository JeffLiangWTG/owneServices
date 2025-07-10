namespace Enterprise.Customs.CA.Business.Testing
{
	sealed class InvoiceChargeValidationTest : Customs.Business.Testing.InvoiceChargeValidationTest
	{
		public void TestInvoiceCharge()
		{
			InvoiceCharge parent = Factory.New<InvoiceCharge>();
			AssertEquals(parent.Validation.InvoiceCharge, parent);
		}

		public void TestMessageValidation()
		{
			InvoiceCharge parent = Factory.New<InvoiceCharge>();
			AssertEquals(typeof(ExternalMessageValidation), parent.Validation.MessageValidation.GetType());
		}

		public void TestIsCIFComponentUsedOFT()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			var oFT = invoice.Charges.AddNew();
			oFT.J7_ChargeType = Enterprise.Customs.Business.CustomsChargeTypeList.Codes.OverseasFreight;
			oFT.J7_Amount = 100m;
			oFT.J7_RX_NKCurrency = JobDeclaration.LocalCurrencyConstantCode;
			oFT.J7_IsGSTApplicable = false;
			AssertHasMessageErrorContaining(oFT.J7_IsGSTApplicableInfo, "is a CIF component");
			oFT.J7_IsGSTApplicable = true;
			AssertNoMessageErrorContaining(oFT.J7_IsGSTApplicableInfo, "is a CIF component");
		}

		public void TestIsCIFComponentUsedLHC()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			var lHC = invoice.Charges.AddNew();
			lHC.J7_ChargeType = Enterprise.Customs.Business.CustomsChargeTypeList.Codes.LandingCharges;
			lHC.J7_Amount = 100m;
			lHC.J7_RX_NKCurrency = JobDeclaration.LocalCurrencyConstantCode;
			lHC.J7_IsGSTApplicable = true;
			lHC.J7_IsGSTApplicable = false;
			AssertNoMessageErrorContaining(lHC.J7_IsGSTApplicableInfo, "is not a CIF component");
			lHC.J7_IsGSTApplicable = true;
			AssertHasMessageErrorContaining(lHC.J7_IsGSTApplicableInfo, "is not a CIF component");
		}

		public void TestIsCIFComponentUsed()
		{
			Assert(new InvoiceChargeValidationHelper(Factory.New<InvoiceCharge>()).IsCIFComponentUsedExposed);
		}

		public void TestCheckJ7_Amount()
		{
			var invoice = Factory.New<JobComInvoiceHeader>();
			var oFT = invoice.Charges.AddNew();
			oFT.J7_ChargeType = Enterprise.Customs.Business.CustomsChargeTypeList.Codes.OverseasFreight;
			oFT.J7_Amount = 99999.00;
			oFT.J7_RX_NKCurrency = Core.Constants.CurrencyCodes.Canada;
			oFT.J7_ExchangeRate = 1;
			AssertNoMessageErrorContaining(oFT.J7_AmountInfo, "Amount for overseas freight may not exceed CAD 99,999");
			oFT.J7_Amount = 99999.01;
			AssertHasMessageErrorContaining(oFT.J7_AmountInfo, "Amount for overseas freight may not exceed CAD 99,999");
			oFT.J7_RX_NKCurrency = Core.Constants.CurrencyCodes.BurkinaFaso;
			oFT.J7_Amount = 999990.00;
			oFT.J7_ExchangeRate = 10;
			AssertNoMessageErrorContaining(oFT.J7_AmountInfo, "Amount for overseas freight may not exceed CAD 99,999");
			oFT.J7_Amount = 1000000.00;
			AssertHasMessageErrorContaining(oFT.J7_AmountInfo, "Amount for overseas freight may not exceed CAD 99,999");
		}
	}
}
