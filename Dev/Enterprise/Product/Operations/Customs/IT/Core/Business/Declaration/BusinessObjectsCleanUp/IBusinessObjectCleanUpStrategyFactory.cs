namespace Enterprise.Customs.IT.Business.Declaration;

public interface IBusinessObjectCleanUpStrategyFactory
{
	ICleanUpStrategy CreateForDeclaration(JobDeclaration declaration);

	ICleanUpStrategy CreateForDeclarationTransportModeInland(JobDeclaration declaration);

	ICleanUpStrategy CreateForEntryInstruction(CusEntryInstruction entryInstruction);

	ICleanUpStrategy CreateForInvoice(JobComInvoiceHeader invoiceHeader);

	ICleanUpStrategy CreateForInvoiceLine(JobComInvoiceLine invoiceLine);

	ICleanUpStrategy CreateForPreviousDocument(PreviousDocument previousDocument);

	ICleanUpStrategy CreateForSupportingDocument(SupportingDocument supportingDocument);

	ICleanUpStrategy CreateForDeclarationShipmentIncoTerm(JobDeclaration declaration);
}
