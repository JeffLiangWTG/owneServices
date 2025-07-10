using CargoWise.Types;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	public class EntryLineNormalisedChargesProviderTest : ChargesProviderTest
	{
		public void TestInvoiceTotalReturnsFOBInLocalCurrency()
		{
			ZTestHelper helper = new ZTestHelper(Factory);
			helper.SetExchangeRate(ZDateTime.Today, ZDateTime.Today, 0.50m, helper.USDCurrency);
			invoice.JZ_RX_NKInvoice_Currency = helper.USDCurrency.RX_Code;
			invoice.JZ_InvoiceAmount = 10000m;

			invoice.Charges.AddNew(CustomsChargeTypeList.Codes.ForeignInlandFreight, 100m, "USD");

			invoiceLine1.JI_LinePrice = 6000m;
			invoiceLine2.JI_LinePrice = 3900m;
			testDec.ResumeApportionment();
			AssertEquals("InvoiceTotal in local currency", 20000m, chargesProvider.InvoiceTotal.Amount);
		}

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

			invoiceLine1.JI_LinePrice = 6000m;//FOB:5880
			invoiceLine2.JI_LinePrice = 4000m;//FOB:3920
			testDec.ResumeApportionment();
			AssertEquals("Precondition: Line price1", 6000m, invoiceLine1.JI_LinePrice);
			AssertEquals("Precondition: Line price2", 4000m, invoiceLine2.JI_LinePrice);
			AssertEquals("InvoiceTotal", 9800m, chargesProvider.InvoiceTotal.Amount);
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
			return new EntryLineNormalisedChargesProvider(entryLine, JobDeclaration.GetLocalCurrency());
		}

		#endregion
	}
}
