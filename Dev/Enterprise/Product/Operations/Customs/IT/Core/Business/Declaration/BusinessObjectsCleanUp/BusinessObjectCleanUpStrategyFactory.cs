namespace Enterprise.Customs.IT.Business.Declaration;

public sealed class BusinessObjectCleanUpStrategyFactory : IBusinessObjectCleanUpStrategyFactory
{
	ICleanUpStrategy IBusinessObjectCleanUpStrategyFactory.CreateForDeclaration(JobDeclaration declaration)
		=> new JobDeclarationCleanUpStrategy(declaration);

	ICleanUpStrategy IBusinessObjectCleanUpStrategyFactory.CreateForDeclarationTransportModeInland(JobDeclaration declaration)
		=> new JobDeclarationTransportModeInlandCleanUpStrategy(declaration);

	ICleanUpStrategy IBusinessObjectCleanUpStrategyFactory.CreateForEntryInstruction(CusEntryInstruction entryInstruction)
		=> new CusEntryInstructionCleanUpStrategy(entryInstruction);

	ICleanUpStrategy IBusinessObjectCleanUpStrategyFactory.CreateForInvoice(JobComInvoiceHeader invoiceHeader)
		=> new JobComInvoiceHeaderCleanUpStrategy(invoiceHeader);

	ICleanUpStrategy IBusinessObjectCleanUpStrategyFactory.CreateForInvoiceLine(JobComInvoiceLine invoiceLine)
		=> new JobComInvoiceLineCleanUpStrategy(invoiceLine);

	ICleanUpStrategy IBusinessObjectCleanUpStrategyFactory.CreateForPreviousDocument(PreviousDocument previousDocument)
		=> previousDocument.ParentIsJobComInvoiceLine
			? new JobComInvoiceLinePreviousDocumentCleanupStrategy(previousDocument)
			: new PreviousDocumentCleanUpStrategy(previousDocument);

	ICleanUpStrategy IBusinessObjectCleanUpStrategyFactory.CreateForSupportingDocument(SupportingDocument supportingDocument)
		=> new SupportingDocumentCleanUpStrategy(supportingDocument);

	ICleanUpStrategy IBusinessObjectCleanUpStrategyFactory.CreateForDeclarationShipmentIncoTerm(JobDeclaration declaration)
		=> new JobDeclarationShipmentIncoTermCleanUpStrategy(declaration);
}
