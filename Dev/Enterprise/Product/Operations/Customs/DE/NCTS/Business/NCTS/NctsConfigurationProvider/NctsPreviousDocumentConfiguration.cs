using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.DE.NCTS.Business;

public sealed class NctsPreviousDocumentConfiguration : Enterprise.Customs.EU.NCTS.Business.NctsPreviousDocumentConfiguration
{
	protected override INctsPreviousDocumentDeparturePhase5ValidationDecider GetNctsPreviousDocumentDeparturePhase5ValidationDecider() => new NctsPreviousDocumentDeparturePhase5ValidationDecider();
}

