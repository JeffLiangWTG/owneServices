using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Common.BR;

namespace Enterprise.Customs.BR.Business.ImportSiscomex.Testing
{
	class DeclarationProductDetailsProviderTest : TestCaseWithFactory
	{
		public void TestDeclarationProductDetailsProviderTest()
		{
			var oDeclaration = Factory.New<JobDeclaration>();
			oDeclaration.JE_MessageType = BRJobMessageTypeList.Codes.Import;
			var oInvoice = oDeclaration.Invoices.AddNew();
			oInvoice.JZ_IncoTerm = BRIncoTermList.Codes.CIF;
			oInvoice.JZ_InvoiceAmount = 1412.00m;
			oInvoice.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.UnitedStates;
			var oInvoiceLine = oInvoice.InvoiceLines.AddNew();
			oInvoiceLine.JI_LinePrice = 12000m;
			oInvoiceLine.JI_Description = "DESCRIPTION";
			oInvoiceLine.JI_InvoiceQuantity = 100.12313m;
			oInvoiceLine.JI_InvoiceUQ = "KG";
			oInvoiceLine.JI_CustomsQuantity = 100.12313m;

			var charge = oInvoiceLine.Charges.AddNew();
			charge.J7_ChargeType = Common.CustomsChargeTypeList.Codes.AdditionCharge;
			charge.J7_Amount = 50m;
			charge.J7_RX_NKCurrency = Core.Constants.CurrencyCodes.UnitedStates;
			charge.J7_IsDutiable = false;
			charge.J7_Calc_IsIncludedInInvoiceAmount = true;
			oDeclaration.ResumeApportionment();

			var itemProvider = new DeclarationProductDetailsProvider(oInvoiceLine);

			AssertEquals("ProductDescription should be", "DESCRIPTION", itemProvider.ProductDescription);
			AssertEquals("InvoiceQuantityUQDescription should be", "Quilogramas", itemProvider.InvoiceQuantityUQDescription);
			AssertEquals("InvoiceQuantity should be", 100.12313m, itemProvider.InvoiceQuantity);
			AssertEquals("ProductValueAmount should be", 119.85m, itemProvider.ProductValueAmount);
			AssertEquals("CustomsValueAmount should be", 120.3518108m, itemProvider.CustomsValueAmount);

			oInvoiceLine.JI_InvoiceQuantity = 0m;

			itemProvider = new DeclarationProductDetailsProvider(oInvoiceLine);
			AssertEquals("InvoiceQuantity should be", 0m, itemProvider.InvoiceQuantity);
			AssertEquals("ProductValueAmount should be", 0m, itemProvider.ProductValueAmount);
			AssertEquals("CustomsValueAmount should be", 0m, itemProvider.CustomsValueAmount);
		}
	}
}
