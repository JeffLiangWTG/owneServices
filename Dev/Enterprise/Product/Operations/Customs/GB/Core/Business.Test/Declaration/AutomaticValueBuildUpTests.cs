using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.GB.Business.Declaration.Testing
{
	class AutomaticValueBuildUpTests : TestCaseWithFactory
	{
		public void TestAutomaticValueBuildUp()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = "IMP";
			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_IncoTerm = "FOB";
			invoice.JZ_InvoiceAmount = 10000;
			invoice.JZ_RX_NKInvoice_Currency = "GBP";
			var topInvoiceCharges = declaration.TopGroupInvoice.Charges;
			var oft = topInvoiceCharges.AddNew();
			oft.J7_ChargeType = ChargesProvider.InternationalFreight.Code;
			oft.J7_RX_NKCurrency = "GBP";
			oft.J7_Amount = 50;
			var dis = topInvoiceCharges.AddNew();
			dis.J7_ChargeType = ChargesProvider.Discount.Code;
			dis.J7_RX_NKCurrency = "GBP";
			dis.J7_Amount = 100;
			var afb = topInvoiceCharges.AddNew();
			afb.J7_ChargeType = ChargesProvider.AirFreight.Code;
			afb.J7_Amount = 200;
			afb.J7_IsDutiable = true;
			afb.J7_IsGSTApplicable = true;
			afb.J7_RX_NKCurrency = "GBP";
			var afa = topInvoiceCharges.AddNew();
			afa.J7_ChargeType = ChargesProvider.AirFreight.Code;
			afa.J7_Amount = 300;
			afa.J7_IsDutiable = false;
			afa.J7_IsGSTApplicable = true;
			afa.J7_RX_NKCurrency = "GBP";
			var ins = topInvoiceCharges.AddNew();
			ins.J7_ChargeType = ChargesProvider.InternationalInsurance.Code;
			ins.J7_Amount = 400;
			ins.J7_IsDutiable = true;
			ins.J7_IsGSTApplicable = true;
			ins.J7_RX_NKCurrency = "GBP";
			var add = topInvoiceCharges.AddNew();
			add.J7_ChargeType = ChargesProvider.AdditionCharge.Code;
			add.J7_Amount = 555;
			add.J7_IsDutiable = true;
			add.J7_IsGSTApplicable = true;
			add.J7_RX_NKCurrency = "GBP";
			var vat = topInvoiceCharges.AddNew();
			vat.J7_ChargeType = ChargesProvider.VATAdjustment.Code;
			vat.J7_Amount = 666;
			vat.J7_IsDutiable = false;
			vat.J7_IsGSTApplicable = true;
			vat.J7_RX_NKCurrency = "GBP";
			var deduction = topInvoiceCharges.AddNew();
			deduction.J7_ChargeType = ChargesProvider.DeductionCharge.Code;
			deduction.J7_Amount = 0.55m;
			deduction.J7_RX_NKCurrency = "GBP";
			declaration.ResumeApportionment();

			declaration.ZG_ManualCalc = true;
			declaration.ZG_ManualCalc = false;
			AssertEquals(100m, declaration.ZG_DiscAmt);
			AssertEquals("200+300=500 OS costs", 500m, declaration.ZG_OSAirTransportAmount);
			AssertEquals("OS costs 500 + OFT 50 = 550", 550m, declaration.ZG_FrtChgAmt);
			AssertEquals(400m, declaration.ZG_InsAmt);
			AssertEquals("Other charges is additions 555 less deduction 0.55 = 554.45 (even though deductions are in lines already)", 554.45m, declaration.ZG_OthChgAmt);
			AssertEquals(666m, declaration.ZG_VATAdjAmt);
		}
	}
}
