namespace Enterprise.Customs.EU.NCTS.Business;

public class NctsContainerConfiguration
{
	public INctsContainerValidationDecider GetHeaderValidationDecider(NctsHeader header) => GetHeaderValidationDeciderCore(header);

	protected virtual INctsContainerValidationDecider GetHeaderValidationDeciderCore(NctsHeader header)
	{
		return header switch
		{
			{ IsPhase5Arrival: true } => GetNctsArrivalHeaderContainerPhase5ValidationDecider(),
			{ IsPhase5Departure: true } => GetNctsDepartureHeaderContainerPhase5ValidationDecider(),
			_ => null,
		};
	}

	protected virtual INctsArrivalHeaderContainerPhase5ValidationDecider GetNctsArrivalHeaderContainerPhase5ValidationDecider() => new NctsArrivalHeaderContainerPhase5ValidationDecider();
	protected virtual INctsDepartureHeaderContainerPhase5ValidationDecider GetNctsDepartureHeaderContainerPhase5ValidationDecider() => new NctsDepartureHeaderContainerPhase5ValidationDecider();
}
