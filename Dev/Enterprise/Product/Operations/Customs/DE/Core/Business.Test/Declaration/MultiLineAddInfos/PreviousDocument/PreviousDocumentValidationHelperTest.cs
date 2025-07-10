using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.DE.Business.Declaration.Testing
{
	sealed class PreviousDocumentValidationHelperTest : TestCaseWithFactory
	{
		public void TestPreviousProcedureIsRequiredForInvoiceLine()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine = invoiceHeader.InvoiceLines.AddNew();
			invoiceLine.JI_CEI = entryInstruction.PK;

			CombineAssertions(() =>
			{
				entryInstruction.CEI_SubStyle = "00";
				invoiceLine.JI_Procedure = "3151";
				AssertEquals("JI_Procedure is XX51", expected: true, PreviousDocumentValidationHelper.PreviousProcedureIsRequiredForInvoiceLine(invoiceLine));

				invoiceLine.JI_Procedure = "3171";
				AssertEquals("JI_Procedure is XX71", expected: true, PreviousDocumentValidationHelper.PreviousProcedureIsRequiredForInvoiceLine(invoiceLine));

				invoiceLine.JI_Procedure = "3161";
				AssertEquals("JI_Procedure isn't XX51 or XX71", expected: false, PreviousDocumentValidationHelper.PreviousProcedureIsRequiredForInvoiceLine(invoiceLine));

				invoiceLine.JI_Procedure = "3151";
				entryInstruction.CEI_SubStyle = "10";
				AssertEquals("CEI_SubStyle is 1X", expected: false, PreviousDocumentValidationHelper.PreviousProcedureIsRequiredForInvoiceLine(invoiceLine));

				entryInstruction.CEI_SubStyle = "20";
				AssertEquals("CEI_SubStyle is 2X", expected: false, PreviousDocumentValidationHelper.PreviousProcedureIsRequiredForInvoiceLine(invoiceLine));

				entryInstruction.CEI_SubStyle = "00";
				invoiceLine.JI_CEI = ZGuid.Empty;
				AssertEquals("EntryInstruction is null", expected: false, PreviousDocumentValidationHelper.PreviousProcedureIsRequiredForInvoiceLine(invoiceLine));
			});
		}
	}
}
