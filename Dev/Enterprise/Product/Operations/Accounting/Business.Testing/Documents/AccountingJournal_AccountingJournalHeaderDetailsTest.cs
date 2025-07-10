using CargoWise.EntityFramework.Testing;
using Enterprise.DocumentEngine.Business;
using Enterprise.DocumentEngine.DocBuilder;
using Enterprise.DocumentEngine.Testing.UtilityClasses;

namespace Enterprise.Accounting.Business.Testing
{
	class AccountingJournal_AccountingJournalHeaderDetailsTest : TestCaseWithFactory
	{
		public void TestAccountingJournalHeaderDetailsSectionContains_REFERENCE_Macros()
		{
			var systemTemplate = StmTemplateBase.GetDocBuilderTemplate(Factory, DocBuilderTemplateType.System);
			var excelWorkSheet = ConfigurableTemplateTestHelper.GetExcelWorkSheetFromSection(systemTemplate, "Accounting Journal Header Details");

			var referenceContent =
@"{BU}-[<HideRowIf(""<GenericTransactionHeader.ConsolidatedInvoiceRef>"" == """" && ""<GenericTransactionHeader.ComplianceSubType>"" == """" && ""<GenericTransactionHeader.TransactionReference>"" == """")>]
{C}-[REFERENCE]   {J}-[<GenericTransactionHeader.ConsolidatedInvoiceRef>  <GenericTransactionHeader.ComplianceSubType>  <GenericTransactionHeader.TransactionNumber>  <GenericTransactionHeader.TransactionReference>]   {BU}-[<HideRowIf(""<GenericTransactionHeader.ConsolidatedInvoiceRef>"" == """" && ""<GenericTransactionHeader.ComplianceSubType>"" == """" && ""<GenericTransactionHeader.TransactionReference>"" == """")>]";

			var excelWorkSheetContent = excelWorkSheet.ToString();

			AssertContainsInOrder("The section should contain REFERENCE macros", excelWorkSheetContent, "[<GenericTransactionHeader.AJOptionField3Caption>]", referenceContent, "[REQUESTER]");
		}
	}
}
