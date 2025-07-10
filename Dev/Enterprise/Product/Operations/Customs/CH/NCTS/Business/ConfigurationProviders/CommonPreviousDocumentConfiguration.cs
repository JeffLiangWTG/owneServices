using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.CH.NCTS.Business;

public sealed class CommonPreviousDocumentConfiguration : EU.NCTS.Business.CommonPreviousDocumentConfiguration
{
	protected override ICommonPreviousDocumentValidationDecider GetCommonPreviousDocumentValidationDecider()
		=> new CommonPreviousDocumentValidationDecider();

	protected override ICommonPreviousDocumentArrivalValidationDecider GetCommonPreviousDocumentArrivalValidationDecider()
		=> new CommonPreviousDocumentArrivalValidationDecider();

	protected override ICommonPreviousDocumentDepartureValidationDecider GetCommonPreviousDocumentDepartureValidationDecider()
		=> new CommonPreviousDocumentDepartureValidationDecider();
}
