using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Universal;

namespace Enterprise.Customs.CN.Business.Testing
{
	class ValidationHelperTests : TestCaseWithFactory
	{
		public void TestCheckConditionForInvoiceLine()
		{
			var declaration = Factory.New<JobDeclaration>();
			var instruction = declaration.CustomsEntryInstructions.AddNew();
			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine = (JobComInvoiceLine)invoiceHeader.InvoiceLines.AddNew();
			var doc08 = invoiceLine.CusSupportingDocuments.AddNew();
			doc08.CSI_Code = "08";
			doc08.CSI_ReferenceNumber = "08";
			var doc09 = invoiceLine.CusSupportingDocuments.AddNew();
			doc09.CSI_Code = "09";
			doc09.CSI_ReferenceNumber = "09";
			invoiceLine.JI_CEI = instruction.PK;

			var testValueType_PROH = Factory.New<RefCusConditionValueType>();
			testValueType_PROH.ZX4_ValueType = Core.Constants.Customs.Universal.RefCusConditionValueTypes.Codes.Prohibitation;
			var testValueType_DOC = Factory.New<RefCusConditionValueType>();
			testValueType_DOC.ZX4_ValueType = Core.Constants.Customs.Universal.RefCusConditionValueTypes.Codes.PresentationOfSupportingDoc;
			var testValueType_UNK = Factory.New<RefCusConditionValueType>();
			testValueType_UNK.ZX4_ValueType = "UNK";

			CombineAssertions(() =>
			{
				AssertEquals("Test 1", true, invoiceLine.CheckConditionForInvoiceLine("CTRL", "PROH", "08"));
				AssertEquals("Test 2", true, invoiceLine.CheckConditionForInvoiceLine("CTRL", "PROH", "XX"));
				AssertEquals("Test 3", true, invoiceLine.CheckConditionForInvoiceLine("CTRL", "DOC", "08"));
				AssertEquals("Test 4", true, invoiceLine.CheckConditionForInvoiceLine("CTRL", "DOC", "09"));
				AssertEquals("Test 5", false, invoiceLine.CheckConditionForInvoiceLine("CTRL", "DOC", "XX"));
				AssertEquals("Test 6", false, invoiceLine.CheckConditionForInvoiceLine("CTRL", "UNK", "09"));
				AssertEquals("Test 7", false, invoiceLine.CheckConditionForInvoiceLine("CTRL", "UNK", "XX"));
				AssertEquals("Test 8", false, invoiceLine.CheckConditionForInvoiceLine("", "", "09"));
				AssertEquals("Test 9", false, invoiceLine.CheckConditionForInvoiceLine("", "", "XX"));
			});
		}
	}
}
