namespace Enterprise.Customs.EU.NCTS.Business;

public class NctsSupportingDocumentConfiguration
{
	public INctsSupportingDocumentValidationDecider GetValidationDecider(NctsSupportingDocument supportingDocument) => GetValidationDeciderCore(supportingDocument);

	protected virtual INctsSupportingDocumentValidationDecider GetValidationDeciderCore(NctsSupportingDocument supportingDocument) =>
		supportingDocument switch
		{
			{ IsPhase5Departure: true } => GetNctsSupportingDocumentDeparturePhase5ValidationDecider(),
			_ => null
		};

	protected virtual INctsSupportingDocumentDeparturePhase5ValidationDecider GetNctsSupportingDocumentDeparturePhase5ValidationDecider() =>
		new NctsSupportingDocumentDeparturePhase5ValidationDecider();
}
