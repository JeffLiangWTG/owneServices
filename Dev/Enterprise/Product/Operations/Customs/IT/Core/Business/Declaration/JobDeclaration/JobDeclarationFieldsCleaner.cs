using CargoWise.Common;

namespace Enterprise.Customs.IT.Business.Declaration;

class JobDeclarationFieldsCleaner : IJobDeclarationFieldsCleaner
{
	public JobDeclarationFieldsCleaner(JobDeclaration declaration)
	{
		this.declaration = Argument.NotNull(declaration, nameof(declaration));
		CleanUpStrategyFactory = new BusinessObjectCleanUpStrategyFactory();
	}

	readonly JobDeclaration declaration;

	protected IBusinessObjectCleanUpStrategyFactory CleanUpStrategyFactory { get; private set; }

	void IJobDeclarationFieldsCleaner.CleanUpMessageDependentFieldsIfNoLongerApplicable()
	{
		CleanUpDeclaration();
		CleanUpEntryInstructions();
		CleanUpInvoices();
		CleanUpInvoiceLines();
		CleanUpShipmentIncoTerm();
	}

	void IJobDeclarationFieldsCleaner.CleanUpTransportModeInlandDependentFieldsIfNoLongerApplicable()
	{
		CleanUpStrategyFactory.CreateForDeclarationTransportModeInland(declaration).CleanUp();
	}

	void IJobDeclarationFieldsCleaner.CleanUpShipmentIncoTermFieldsIfNoLongerApplicable()
	{
		CleanUpShipmentIncoTerm();
	}

	#region Implementation

	void CleanUpDeclaration()
	{
		CleanUpStrategyFactory.CreateForDeclaration(declaration).CleanUp();
		CleanUpPreviousDocuments(declaration.PreviousDocuments);
		CleanUpSupportingDocuments(declaration.SupportingDocuments);
	}

	void CleanUpEntryInstructions()
	{
		foreach (CusEntryInstruction entryInstruction in declaration.CustomsEntryInstructions)
		{
			CleanUpStrategyFactory.CreateForEntryInstruction(entryInstruction).CleanUp();
		}
	}

	void CleanUpInvoices()
	{
		foreach (JobComInvoiceHeader invoiceHeader in declaration.Invoices)
		{
			CleanUpStrategyFactory.CreateForInvoice(invoiceHeader).CleanUp();
			CleanUpPreviousDocuments(invoiceHeader.PreviousDocuments);
			CleanUpSupportingDocuments(invoiceHeader.SupportingDocuments);
		}
	}

	void CleanUpInvoiceLines()
	{
		foreach (JobComInvoiceLine invoiceLine in declaration.InvoiceLines)
		{
			CleanUpStrategyFactory.CreateForInvoiceLine(invoiceLine).CleanUp();
			CleanUpPreviousDocuments(invoiceLine.PreviousDocuments);
			CleanUpSupportingDocuments(invoiceLine.SupportingDocuments);
		}
	}

	void CleanUpPreviousDocuments(PreviousDocumentCollection previousDocuments)
	{
		foreach (PreviousDocument document in previousDocuments)
		{
			CleanUpStrategyFactory.CreateForPreviousDocument(document).CleanUp();
		}
	}

	void CleanUpSupportingDocuments(SupportingDocumentCollection supportingDocuments)
	{
		foreach (SupportingDocument document in supportingDocuments)
		{
			CleanUpStrategyFactory.CreateForSupportingDocument(document).CleanUp();
		}
	}

	void CleanUpShipmentIncoTerm()
	{
		CleanUpStrategyFactory.CreateForDeclarationShipmentIncoTerm(declaration).CleanUp();
	}

#if DEBUG

	public void SetCleanUpStrategyFactory(IBusinessObjectCleanUpStrategyFactory cleanUpStrategyFactory)
	{
		CleanUpStrategyFactory = cleanUpStrategyFactory;
	}

#endif

	#endregion
}
