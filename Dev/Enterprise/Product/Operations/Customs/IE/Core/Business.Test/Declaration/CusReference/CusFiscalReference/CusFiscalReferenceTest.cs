using Enterprise.Customs.Common.EU;
using NUnit.Framework;

namespace Enterprise.Customs.IE.Business.Declaration.Testing
{
	[TestedType(typeof(CusFiscalReference))]
	class CusFiscalReferenceTest : EU.Business.Declaration.Testing.CusFiscalReferenceAbstractTest<CusFiscalReference>
	{
		public void TestIsImportInvoiceLine()
		{
			var testDec = Factory.New<JobDeclaration>();
			testDec.JE_MessageType = EUJobMessageTypeList.Codes.Import;

			var entryInstruction = testDec.CustomsEntryInstructions.AddNew();
			var entryInstructionFiscalReference = entryInstruction.FiscalReferences.AddNew();
			Assert("Parent IS NOT an InvoiceLine", !entryInstructionFiscalReference.IsImportInvoiceLine());

			var invoiceLine = testDec.Invoices.AddNew().InvoiceLines.AddNew();
			var invoiceLineFiscalReference = invoiceLine.FiscalReferences.AddNew();
			Assert("Parent IS an InvoiceLine", invoiceLineFiscalReference.IsImportInvoiceLine());

			testDec.JE_MessageType = EUJobMessageTypeList.Codes.Export;
			Assert("The declaration is not an import", !invoiceLineFiscalReference.IsImportInvoiceLine());
		}

		public void TestIsImportEntryInstruction()
		{
			var testDec = Factory.New<JobDeclaration>();
			testDec.JE_MessageType = EUJobMessageTypeList.Codes.Import;

			var invoiceLine = testDec.Invoices.AddNew().InvoiceLines.AddNew();
			var invoiceLineFiscalReference = invoiceLine.FiscalReferences.AddNew();
			Assert("Parent IS NOT an EntryInstruction", !invoiceLineFiscalReference.IsImportEntryInstruction());

			var entryInstruction = testDec.CustomsEntryInstructions.AddNew();
			var entryInstructionFiscalReference = entryInstruction.FiscalReferences.AddNew();
			Assert("Parent IS an EntryInstruction", entryInstructionFiscalReference.IsImportEntryInstruction());

			testDec.JE_MessageType = EUJobMessageTypeList.Codes.Export;
			Assert("The declaration is not an import", !entryInstructionFiscalReference.IsImportEntryInstruction());
		}
	}
}
