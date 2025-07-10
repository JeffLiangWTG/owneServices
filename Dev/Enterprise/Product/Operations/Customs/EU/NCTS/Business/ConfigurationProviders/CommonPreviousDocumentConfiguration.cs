namespace Enterprise.Customs.EU.NCTS.Business;

public class CommonPreviousDocumentConfiguration
{
	public ICommonPreviousDocumentValidationDecider GetValidationDecider(CommonPreviousDocument commonPreviousDocument) => GetValidationDeciderCore(commonPreviousDocument);

	protected virtual ICommonPreviousDocumentValidationDecider GetValidationDeciderCore(CommonPreviousDocument commonPreviousDocument) =>
		commonPreviousDocument switch
		{
			{ Header.IsPhase5Departure: true } => GetCommonPreviousDocumentDepartureValidationDecider(),
			{ Header.IsPhase5Arrival: true } => GetCommonPreviousDocumentArrivalValidationDecider(),
			{ Header: not null } => GetCommonPreviousDocumentValidationDecider(),
			_ => null
		};

	protected virtual ICommonPreviousDocumentValidationDecider GetCommonPreviousDocumentValidationDecider() =>
		new CommonPreviousDocumentValidationDecider();

	protected virtual ICommonPreviousDocumentArrivalValidationDecider GetCommonPreviousDocumentArrivalValidationDecider() =>
		new CommonPreviousDocumentArrivalValidationDecider();

	protected virtual ICommonPreviousDocumentDepartureValidationDecider GetCommonPreviousDocumentDepartureValidationDecider() =>
		new CommonPreviousDocumentDepartureValidationDecider();
}
