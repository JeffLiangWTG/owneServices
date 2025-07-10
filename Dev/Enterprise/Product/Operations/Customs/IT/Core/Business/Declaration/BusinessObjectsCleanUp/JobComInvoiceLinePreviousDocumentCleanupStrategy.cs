using CargoWise.Types;

namespace Enterprise.Customs.IT.Business.Declaration;

sealed class JobComInvoiceLinePreviousDocumentCleanupStrategy : PreviousDocumentCleanUpStrategy
{
	public JobComInvoiceLinePreviousDocumentCleanupStrategy(PreviousDocument previousDocument) : base(previousDocument)
	{
	}

	protected override void CleanupCore()
	{
		base.CleanupCore();

		if (Declaration.IsUCC6AndIsExport)
		{
			ClearNotApplicableForUcc6ExportPreviousDocumentsFields();
		}
	}

	void ClearNotApplicableForUcc6ExportPreviousDocumentsFields()
	{
		PreviousDocument.CSI_Procedure = ZString.Empty;
		PreviousDocument.CSI_SubType = ZString.Empty;
		PreviousDocument.CSI_UnitOfQuantity3 = ZString.Empty;
	}
}
