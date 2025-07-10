using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.IT.NCTS.Business;

public sealed class NctsPreviousDocumentConfiguration : EU.NCTS.Business.NctsPreviousDocumentConfiguration
{
	protected override INctsPreviousDocumentDeparturePhase5ValidationDecider GetNctsPreviousDocumentDeparturePhase5ValidationDecider() => new NctsPreviousDocumentDeparturePhase5ValidationDecider();
}
