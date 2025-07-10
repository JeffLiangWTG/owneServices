namespace Enterprise.Customs.DE.NCTS.Business
{
	public interface INctsPreviousProcedureParentProvider
	{
		EU.NCTS.Business.INctsPreviousDocumentCollection<NctsPreviousDocument> PreviousProcedures { get; }

		EU.NCTS.Business.INctsPreviousDocumentCollection<NctsPreviousDocument> PreviousDocuments { get; }

		NctsHeader NctsHeader { get; }
	}
}
