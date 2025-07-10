using Enterprise.Customs.Business;
using Enterprise.Customs.Common.BR;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.BR.Business.Testing
{
	[TestedType(typeof(InvoiceCharge))]
	class InvoiceChargeTest : Customs.Business.Testing.BaseInvoiceChargeTest
	{
		public void TestJ7_PrepaidCollect_ReadOnly()
		{
			var invoiceCharge = Factory.New<InvoiceCharge>();
			invoiceCharge.J7_ChargeType = ImportCustomsChargeTypeList.Codes.OverseasFreightPrepaid;
			AssertEquals("J7_PrepaidCollect_ReadOnly should be", true, invoiceCharge.J7_PrepaidCollect_ReadOnly);

			invoiceCharge.J7_ChargeType = ImportCustomsChargeTypeList.Codes.FreightInNationalTerritory;
			AssertEquals("J7_PrepaidCollect_ReadOnly should be", false, invoiceCharge.J7_PrepaidCollect_ReadOnly);

			invoiceCharge.J7_ChargeType = ImportCustomsChargeTypeList.Codes.OverseasFreightCollect;
			AssertEquals("J7_PrepaidCollect_ReadOnly should be", true, invoiceCharge.J7_PrepaidCollect_ReadOnly);
		}

		public void TestPrepaidOrCollect()
		{
			var invoiceCharge = Factory.New<InvoiceCharge>();
			invoiceCharge.J7_ChargeType = ImportCustomsChargeTypeList.Codes.OverseasFreightPrepaid;
			AssertEquals("J7_PrepaidCollect should be", Core.Constants.PaymentType.Prepaid, invoiceCharge.J7_PrepaidCollect);

			invoiceCharge.J7_ChargeType = ImportCustomsChargeTypeList.Codes.OverseasFreightCollect;
			AssertEquals("J7_PrepaidCollect should be", Core.Constants.PaymentType.Collect, invoiceCharge.J7_PrepaidCollect);
		}

		public void TestTypeDecider()
		{
			Assert("Update BaseInvoiceChargeTypeDecider to include a decider for this class", Factory.New(typeof(BaseInvoiceCharge)).GetType() == GetExpectedBusinessObjectType());
		}

		public void TestImportLicenseDistributeBy()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportLicense;
			var invoiceHeader = declaration.Invoices.AddNew();
			invoiceHeader.JZ_RX_NKInvoice_Currency = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;

			var charge = invoiceHeader.Charges.AddNew(ImportCustomsChargeTypeList.Codes.FreightInNationalTerritory, 100m, invoiceHeader.Invoice_Currency.RX_Code);
			AssertEquals("Distribute by should be FOB", ChargeDistributeByList.Codes.NetWeight, charge.J7_DistributeBy);

			charge.J7_ChargeType = ImportCustomsChargeTypeList.Codes.OverseasFreightCollect;
			AssertEquals("Distribute by should be FOB", ChargeDistributeByList.Codes.NetWeight, charge.J7_DistributeBy);

			charge.J7_ChargeType = ImportCustomsChargeTypeList.Codes.OverseasFreightPrepaid;
			AssertEquals("Distribute by should be FOB", ChargeDistributeByList.Codes.NetWeight, charge.J7_DistributeBy);

			charge.J7_ChargeType = Common.CustomsChargeTypeList.Codes.OverseasInsurance;
			AssertEquals("Distribute by should be FOB", ChargeDistributeByList.Codes.FOB, charge.J7_DistributeBy);
		}

		public void TestDefaultCurrencyAndReadOnly()
		{
			var invoiceCharge = Factory.New<InvoiceCharge>();

			invoiceCharge.J7_RX_NKCurrency = Core.Constants.CurrencyCodes.Zimbabwe;
			invoiceCharge.J7_ChargeType = ImportCustomsChargeTypeList.Codes.FreightInNationalTerritory;
			Assert("J7_RX_NKCurrencyInfo must NOT be ReadOnly for FNT", !invoiceCharge.J7_RX_NKCurrencyInfo.ReadOnly);
			AssertEquals("J7_RX_NKCurrency must be BRL for FCO", Core.Constants.CurrencyCodes.Zimbabwe, invoiceCharge.J7_RX_NKCurrency);

			invoiceCharge.J7_ChargeType = ImportCustomsChargeTypeList.Codes.FreightComponents;
			Assert("J7_RX_NKCurrency must NOT be ReadOnly for FCO", !invoiceCharge.J7_RX_NKCurrencyInfo.ReadOnly);
			AssertEquals("J7_RX_NKCurrency must be BRL for FCO", Core.Constants.CurrencyCodes.Brazil, invoiceCharge.J7_RX_NKCurrency);

			invoiceCharge.J7_RX_NKCurrency = Core.Constants.CurrencyCodes.Zimbabwe;
			invoiceCharge.J7_ChargeType = ImportCustomsChargeTypeList.Codes.OtherExpensesICMS;
			Assert("J7_RX_NKCurrency must be ReadOnly for EIC", invoiceCharge.J7_RX_NKCurrencyInfo.ReadOnly);
			AssertEquals("J7_RX_NKCurrency must be BRL for EIC", Core.Constants.CurrencyCodes.Brazil, invoiceCharge.J7_RX_NKCurrency);
		}

		public void TestIsFreightComponents()
		{
			var invoiceCharge = Factory.New<InvoiceCharge>();

			invoiceCharge.J7_ChargeType = ImportCustomsChargeTypeList.Codes.FreightInNationalTerritory;
			Assert("IsFreightComponents must be False", !invoiceCharge.IsFreightComponents);

			invoiceCharge.J7_ChargeType = ImportCustomsChargeTypeList.Codes.FreightComponents;
			Assert("IsFreightComponents must be True", invoiceCharge.IsFreightComponents);
		}

		public void TestIsOtherExpensesICMS()
		{
			var invoiceCharge = Factory.New<InvoiceCharge>();

			invoiceCharge.J7_ChargeType = ImportCustomsChargeTypeList.Codes.FreightInNationalTerritory;
			Assert("IsOtherExpensesICMS must be False", !invoiceCharge.IsOtherExpensesICMS);

			invoiceCharge.J7_ChargeType = ImportCustomsChargeTypeList.Codes.OtherExpensesICMS;
			Assert("IsOtherExpensesICMS must be True", invoiceCharge.IsOtherExpensesICMS);
		}

		protected override void SetupAllTestObjects()
		{
			testDec = base.Factory.New<JobDeclaration>();
			testDec.JE_MessageType = BRJobMessageTypeList.Codes.MiscellaneousCustoms;
			invoice = testDec.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			testCharge = invoice.Charges.AddNew();
		}
	}
}
