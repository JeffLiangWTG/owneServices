namespace Enterprise.Customs.EU.NCTS.Business;

public class NctsPreviousDocumentConfiguration
{
	public INctsPreviousDocumentValidationDecider GetValidationDecider(NctsPreviousDocument previousDocument) => GetValidationDeciderCore(previousDocument);

	protected virtual INctsPreviousDocumentValidationDecider GetValidationDeciderCore(NctsPreviousDocument previousDocument) =>
		previousDocument switch
		{
			{ IsPhase5Departure: true } => GetNctsPreviousDocumentDeparturePhase5ValidationDecider(),
			{ IsPhase5: true } => GetNctsPreviousDocumentArrivalPhase5ValidationDecider(),
			_ => null
		};

	protected virtual INctsPreviousDocumentArrivalPhase5ValidationDecider GetNctsPreviousDocumentArrivalPhase5ValidationDecider() =>
		new NctsPreviousDocumentArrivalPhase5ValidationDecider();

	protected virtual INctsPreviousDocumentDeparturePhase5ValidationDecider GetNctsPreviousDocumentDeparturePhase5ValidationDecider() =>
		new NctsPreviousDocumentDeparturePhase5ValidationDecider();
}

