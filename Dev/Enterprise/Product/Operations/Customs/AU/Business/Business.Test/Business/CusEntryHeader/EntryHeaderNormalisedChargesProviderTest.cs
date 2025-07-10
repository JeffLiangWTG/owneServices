using Enterprise.Customs.Business;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	public class EntryHeaderNormalisedChargesProviderTest : ChargesProviderTest
	{
		public override void TestCommission()
		{
			invoice.Charges.AddNew(CustomsChargeTypeList.Codes.Commission, 100m, JobDeclaration.LocalCurrencyConstantCode);

			invoiceLine1.JI_LinePrice = 6000m;
			invoiceLine2.JI_LinePrice = 3900m;

			AssertEquals("Commission", true, chargesProvider.Commission.IsEmpty);
		}

		public override void TestDiscount()
		{
			invoice.Charges.AddNew(CustomsChargeTypeList.Codes.Discount, 100m, JobDeclaration.LocalCurrencyConstantCode);

			invoiceLine1.JI_LinePrice = 6000m;
			invoiceLine2.JI_LinePrice = 4100m;

			AssertEquals("Discount", true, chargesProvider.Discount.IsEmpty);
		}

		public override void TestForeignInlandFreight()
		{
			invoice.Charges.AddNew(CustomsChargeTypeList.Codes.ForeignInlandFreight, 100m, JobDeclaration.LocalCurrencyConstantCode);

			invoiceLine1.JI_LinePrice = 6000m;
			invoiceLine2.JI_LinePrice = 3900m;

			AssertEquals("ForeignInlandFreight", true, chargesProvider.ForeignInlandFreight.IsEmpty);
		}

		public override void TestInvoiceTotal()
		{
			invoice.JZ_IncoTerm = Core.Constants.IncoTerms.CostAndFreight;
			BaseJobComInvHeaderCharge oFT = invoice.Charges.AddNew(AUChargeCodeList.Codes.OverseasFreight, 200m, "AUD");
			oFT.J7_IsIncludedInITOT = true;

			invoiceLine1.JI_LinePrice = 10000m;//FOB:9800m

			JobComInvoiceHeader invoice2 = testDec.Invoices.AddNew();
			invoice2.JZ_IncoTerm = Core.Constants.IncoTerms.FreeOnBoard;
			invoice2.JZ_InvoiceAmount = 3000m;
			invoice2.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;
			invoiceLine2 = invoice2.JobComInvoiceLines.AddNew();
			invoiceLine2.JI_LinePrice = 3000m;
			invoiceLine2.JI_CL = entryLine.PK;
			testDec.ResumeApportionment();
			AssertEquals("Line price1", 10000m, invoiceLine1.JI_LinePrice);
			AssertEquals("Line price2", 3000m, invoiceLine2.JI_LinePrice);
			AssertEquals("InvoiceTotal", 12800m, chargesProvider.InvoiceTotal.Amount);
		}

		public override void TestLandingCharges()
		{
			invoice.Charges.AddNew(CustomsChargeTypeList.Codes.LandingCharges, 100m, JobDeclaration.LocalCurrencyConstantCode);

			invoiceLine1.JI_LinePrice = 6000m;
			invoiceLine2.JI_LinePrice = 3900m;

			AssertEquals("LandingCharges", true, chargesProvider.LandingCharges.IsEmpty);
		}

		public override void TestOtherCharges1()
		{
			invoice.Charges.AddNew(CustomsChargeTypeList.Codes.OtherCharges, 100m, JobDeclaration.LocalCurrencyConstantCode);

			invoiceLine1.JI_LinePrice = 6000m;
			invoiceLine2.JI_LinePrice = 3900m;

			AssertEquals("OtherCharges", true, chargesProvider.OtherCharges1.IsEmpty);
		}

		public override void TestOtherCharges2()
		{
			BaseJobComInvHeaderCharge oTH = invoice.Charges.AddNew(CustomsChargeTypeList.Codes.OtherCharges, 100m, JobDeclaration.LocalCurrencyConstantCode);
			oTH.J7_IsDutiable = false;
			oTH.J7_IsGSTApplicable = false;

			invoiceLine1.JI_LinePrice = 6000m;
			invoiceLine2.JI_LinePrice = 3900m;

			AssertEquals("OtherCharges2", true, chargesProvider.OtherCharges2.IsEmpty);
		}

		public override void TestPackingCosts()
		{
			invoice.Charges.AddNew(CustomsChargeTypeList.Codes.PackingCost, 100m, JobDeclaration.LocalCurrencyConstantCode);

			invoiceLine1.JI_LinePrice = 6000m;
			invoiceLine2.JI_LinePrice = 3900m;

			AssertEquals("PackingCost", true, chargesProvider.PackingCosts.IsEmpty);
		}

		#region Implemetation

		protected override ICommercialChargesProvider GetChargesProvider()
		{
			return new EntryHeaderNormalisedChargesProvider(entryHeader, JobDeclaration.GetLocalCurrency());
		}

		#endregion
	}
}
