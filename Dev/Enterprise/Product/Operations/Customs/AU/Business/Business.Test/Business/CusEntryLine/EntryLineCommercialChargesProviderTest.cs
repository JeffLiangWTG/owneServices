using Enterprise.Customs.Business;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	public class EntryLineCommercialChargesProviderTest : ChargesProviderTest
	{
		public override void TestCommission()
		{
			invoice.Charges.AddNew(CustomsChargeTypeList.Codes.Commission, 100m, JobDeclaration.LocalCurrencyConstantCode);

			invoiceLine1.JI_LinePrice = 6000m;
			invoiceLine2.JI_LinePrice = 3900m;
			testDec.ResumeApportionment();
			AssertEquals("Commission", 100m, chargesProvider.Commission.Amount);
			AssertEquals("Commission", JobDeclaration.LocalCurrencyConstantCode, chargesProvider.Commission.Currency.Code);
		}

		public override void TestDiscount()
		{
			invoice.Charges.AddNew(CustomsChargeTypeList.Codes.Discount, 100m, JobDeclaration.LocalCurrencyConstantCode);

			invoiceLine1.JI_LinePrice = 6000m;
			invoiceLine2.JI_LinePrice = 4100m;
			testDec.ResumeApportionment();
			AssertEquals("Discount", 100m, chargesProvider.Discount.Amount);
			AssertEquals("Discount", JobDeclaration.LocalCurrencyConstantCode, chargesProvider.Discount.Currency.Code);
		}

		public override void TestForeignInlandFreight()
		{
			invoice.Charges.AddNew(CustomsChargeTypeList.Codes.ForeignInlandFreight, 100m, JobDeclaration.LocalCurrencyConstantCode);

			invoiceLine1.JI_LinePrice = 6000m;
			invoiceLine2.JI_LinePrice = 3900m;
			testDec.ResumeApportionment();
			AssertEquals("ForeignInlandFreight", 100m, chargesProvider.ForeignInlandFreight.Amount);
			AssertEquals("ForeignInlandFreight", JobDeclaration.LocalCurrencyConstantCode, chargesProvider.ForeignInlandFreight.Currency.Code);
		}

		public override void TestInvoiceTotal()
		{
			invoiceLine1.JI_LinePrice = 6000m;
			invoiceLine2.JI_LinePrice = 3900m;

			AssertEquals("Invoice total", 9900m, chargesProvider.InvoiceTotal.Amount);
			AssertEquals("Invoice total currency", JobDeclaration.LocalCurrencyConstantCode, chargesProvider.InvoiceTotal.Currency.Code);
		}

		public override void TestLandingCharges()
		{
			BaseJobComInvHeaderCharge lCH = invoice.Charges.AddNew(CustomsChargeTypeList.Codes.LandingCharges, 100m, JobDeclaration.LocalCurrencyConstantCode);
			lCH.J7_IsIncludedInITOT = true;

			invoiceLine1.JI_LinePrice = 6000m;
			invoiceLine2.JI_LinePrice = 3900m;
			testDec.ResumeApportionment();
			AssertEquals("LandingCharges", 100m, chargesProvider.LandingCharges.Amount);
			AssertEquals("LandingCharges", JobDeclaration.LocalCurrencyConstantCode, chargesProvider.LandingCharges.Currency.Code);
		}

		public override void TestOtherCharges1()
		{
			invoice.Charges.AddNew(CustomsChargeTypeList.Codes.OtherCharges, 100m, JobDeclaration.LocalCurrencyConstantCode);

			invoiceLine1.JI_LinePrice = 6000m;
			invoiceLine2.JI_LinePrice = 3900m;
			testDec.ResumeApportionment();
			AssertEquals("OtherCharges", 100m, chargesProvider.OtherCharges1.Amount);
			AssertEquals("OtherCharges", JobDeclaration.LocalCurrencyConstantCode, chargesProvider.OtherCharges1.Currency.Code);
		}

		public override void TestOtherCharges2()
		{
			BaseJobComInvHeaderCharge oTH = invoice.Charges.AddNew(CustomsChargeTypeList.Codes.OtherCharges, 100m, JobDeclaration.LocalCurrencyConstantCode);
			oTH.J7_IsDutiable = false;
			oTH.J7_IsGSTApplicable = false;
			oTH.J7_IsIncludedInITOT = true;

			invoiceLine1.JI_LinePrice = 6000m;
			invoiceLine2.JI_LinePrice = 3900m;
			testDec.ResumeApportionment();
			AssertEquals("OtherCharges2", 100m, chargesProvider.OtherCharges2.Amount);
			AssertEquals("OtherCharges2", JobDeclaration.LocalCurrencyConstantCode, chargesProvider.OtherCharges2.Currency.Code);
		}

		public override void TestPackingCosts()
		{
			invoice.Charges.AddNew(CustomsChargeTypeList.Codes.PackingCost, 100m, JobDeclaration.LocalCurrencyConstantCode);

			invoiceLine1.JI_LinePrice = 6000m;
			invoiceLine2.JI_LinePrice = 3900m;
			testDec.ResumeApportionment();
			AssertEquals("PackingCost", 100m, chargesProvider.PackingCosts.Amount);
			AssertEquals("PackingCost", JobDeclaration.LocalCurrencyConstantCode, chargesProvider.PackingCosts.Currency.Code);
		}

		protected override ICommercialChargesProvider GetChargesProvider()
		{
			return new EntryLineCommercialChargesProvider(entryLine);
		}
	}
}
