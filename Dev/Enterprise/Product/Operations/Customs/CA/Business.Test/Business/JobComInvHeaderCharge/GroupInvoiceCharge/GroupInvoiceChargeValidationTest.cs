using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.CA.Business.Testing
{
	sealed class GroupInvoiceChargeValidationTest : TestCaseWithFactory
	{
		public void TestMessageValidation()
		{
			GroupInvoiceCharge groupCharge = Factory.New<GroupInvoiceCharge>();
			AssertEquals(typeof(ExternalMessageValidation), groupCharge.Validation.MessageValidation.GetType());
		}

		public void TestIsCIFComponentUsed()
		{
			Assert(new GroupInvoiceChargeValidationHelper(Factory.New<GroupInvoiceCharge>()).IsCIFComponentUsedExposed);
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

		public void TestValidateGroupInvoiceChargeForLVSJob()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.LowValueShipments;
			declaration.Invoices.AddNew();
			var charge = declaration.TopGroupInvoice.Charges.AddNew();
			charge.Validation.ValidateAll();
			AssertNoRowWarningContaining(charge, "This charge will only be apportioned over LVS shipments manually entered on this consolidation");

			var lvxJob = Factory.New<JobDeclaration>();
			lvxJob.JE_MessageType = JobMessageTypeList.Codes.LVSForConsolidation;
			LVXJobsConsolidateHelper.AttachToConsolidatedLVSDeclaration(lvxJob.LVXInvoiceHeader, declaration);
			charge.Validation.ValidateAll();
			AssertHasRowWarningContaining(charge, "This charge will only be apportioned over LVS shipments manually entered on this consolidation");
		}
	}
}
