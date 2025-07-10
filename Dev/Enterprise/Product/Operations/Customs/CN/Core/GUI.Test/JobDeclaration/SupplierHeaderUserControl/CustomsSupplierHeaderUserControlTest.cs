using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.CN.Business;
using Enterprise.Customs.CN.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.CN.GUI.Testing
{
	[TestedType(typeof(CustomsSupplierHeaderUserControl))]
	class CustomsSupplierHeaderUserControlTest : TestCaseWithFactory
	{
		public void TestCustomsInvoiceLinesBoundGridColumns()
		{
			var testItems = CNCusEntryHeaderHelper.SetupCusEntryHeader(Factory, Factory.Save);
			using var form = new JobDeclarationForm(testItems.JobDeclaration);
			var control = form.FindCustomsSupplierHeaderUserControl();
			AssertNotNull(control?.JobComInvoiceHeadersBoundGrid.InnerGrid.GetColumnStyle(JobComInvoiceHeader.Schema.ContractNumbersAsString));
		}

		public void TestControlsExistence()
		{
			using var control = new CustomsSupplierHeaderUserControl();
			TestUtility.AssertControlExistance(control, "IsJZ_ExchangeRateUserEnterableCheckBox", "Invoices.IsJZ_InvoiceCurrExRateUserEnterable");
			TestUtility.AssertControlExistance(control, "JZ_MarksAndNumbersLongTextBox", "Invoices.JZ_MarksAndNumbers");
			TestUtility.AssertControlExistance(control, "ContractNumbersUserControl", "Invoices");
			TestUtility.AssertControlExistance(control, "FormulaPricingConfirmDropEdit", "Invoices.JZ_Calc_FormulaPricingConfirm");
			TestUtility.AssertControlExistance(control, "PaymentOfRoyaltyConfirmDropEdit", "Invoices.JZ_PaymentOfRoyaltyConfirm");
			TestUtility.AssertControlExistance(control, "PriceAffectConfirmDropEdit", "Invoices.JZ_PriceAffectConfirm");
			TestUtility.AssertControlExistance(control, "SpecialRelationshipConfirmDropEdit", "Invoices.JZ_SpecialRelationshipConfirm");
			TestUtility.AssertControlExistance(control, "TemporaryPricingConfirmDropEdit", "Invoices.JZ_Calc_TemporaryPricingConfirm");
			TestUtility.AssertControlExistance(control, "FormulaPricingConfirmDropEdit", "Invoices.JZ_Calc_FormulaPricingConfirm");
		}

		public void TestGridCaption()
		{
			var testItems = CNCusEntryHeaderHelper.SetupCusEntryHeader(Factory, Factory.Save);
			using var form = new JobDeclarationForm(testItems.JobDeclaration);
			var control = form.FindCustomsSupplierHeaderUserControl();
			AssertEquals("Add to FOB?", control.InvoiceChargesGrid.GetColumnStyle("J7_IsDutiable").Caption);
			AssertEquals("Add to CIF?", control.InvoiceChargesGrid.GetColumnStyle("J7_IsGSTApplicable").Caption);
			AssertEquals("Add to FOB?", control.ApportionedChargesGrid.GetColumnStyle("J7_IsDutiable").Caption);
			AssertEquals("Add to CIF?", control.ApportionedChargesGrid.GetColumnStyle("J7_IsGSTApplicable").Caption);
			AssertEquals("Add to FOB?", control.BaseGroupChargesGrid.GetColumnStyle("J7_IsDutiable").Caption);
			AssertEquals("Add to CIF?", control.BaseGroupChargesGrid.GetColumnStyle("J7_IsGSTApplicable").Caption);
		}

		public void TestGridId()
		{
			using var control = new CustomsSupplierHeaderUserControl();
			AssertEquals("GridLayoutPi3ByCKMr9ZmpQDDAywqWw==", control.JobComInvoiceHeadersBoundGrid.InnerGrid.GridId);
		}
	}
}
