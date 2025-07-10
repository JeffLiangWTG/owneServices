using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.IT.NCTS.Business;

public sealed class CommonPreviousDocumentConfiguration : EU.NCTS.Business.CommonPreviousDocumentConfiguration
{
	protected override ICommonPreviousDocumentDepartureValidationDecider GetCommonPreviousDocumentDepartureValidationDecider()
		=> new CommonPreviousDocumentDepartureValidationDecider();
}
