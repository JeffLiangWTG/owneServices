using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.IT.Business.Declaration.Testing;

sealed class BusinessObjectCleanUpStrategyFactoryTest : TestCaseWithFactory
{
	public void TestCreateForDeclaration()
	{
		AssertType<JobDeclarationCleanUpStrategy>("Type", strategyFactory.CreateForDeclaration(declaration));
	}

	public void TestCreateForDeclarationTransportModeInland()
	{
		AssertType<JobDeclarationTransportModeInlandCleanUpStrategy>("Type", strategyFactory.CreateForDeclarationTransportModeInland(declaration));
	}

	public void TestCreateForEntryInstruction()
	{
		var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
		AssertType<CusEntryInstructionCleanUpStrategy>("Type", strategyFactory.CreateForEntryInstruction(entryInstruction));
	}

	public void TestCreateForInvoice()
	{
		var invoiceHeader = declaration.Invoices.AddNew();
		AssertType<JobComInvoiceHeaderCleanUpStrategy>("Type", strategyFactory.CreateForInvoice(invoiceHeader));
	}

	public void TestCreateForInvoiceLine()
	{
		var invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();
		AssertType<JobComInvoiceLineCleanUpStrategy>("Type", strategyFactory.CreateForInvoiceLine(invoiceLine));
	}

	public void TestCreateForPreviousDocument()
	{
		var previousDocument = declaration.PreviousDocuments.AddNew();
		AssertType<PreviousDocumentCleanUpStrategy>("Type", strategyFactory.CreateForPreviousDocument(previousDocument));

		var invoice = declaration.Invoices.AddNew();
		var invoiceLine = invoice.InvoiceLines.AddNew();
		var jobComInvoiceLinePreviousDocument = invoiceLine.PreviousDocuments.AddNew();
		AssertType<JobComInvoiceLinePreviousDocumentCleanupStrategy>("Type when parent is JobComInvoiceLine", strategyFactory.CreateForPreviousDocument(jobComInvoiceLinePreviousDocument));
	}

	public void TestCreateForSupportingDocument()
	{
		var supportingDocument = declaration.SupportingDocuments.AddNew();
		AssertType<SupportingDocumentCleanUpStrategy>("Type", strategyFactory.CreateForSupportingDocument(supportingDocument));
	}

	public void TestCreateForDeclarationShipmentIncoTerm()
	{
		AssertType<JobDeclarationShipmentIncoTermCleanUpStrategy>("Type", strategyFactory.CreateForDeclarationShipmentIncoTerm(declaration));
	}

	protected override void SetUp()
	{
		base.SetUp();
		declaration = Factory.New<JobDeclaration>();
		strategyFactory = new BusinessObjectCleanUpStrategyFactory();
	}

	JobDeclaration declaration;
	IBusinessObjectCleanUpStrategyFactory strategyFactory;
}
