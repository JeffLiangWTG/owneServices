namespace Enterprise.Customs.EU.NCTS.Business;

public class CusTransportMeansConfiguration
{
	public ICusTransportMeansValidationDecider GetValidationDecider(NctsHeader header) => GetValidationDeciderCore(header);

	protected virtual ICusTransportMeansValidationDecider GetValidationDeciderCore(NctsHeader header)
	{
		return header switch
		{
			{ IsPhase5Departure: true } => GetDepartureCusTransportMeansPhase5ValidationDecider(),
			_ => null,
		};
	}

	protected virtual IDepartureCusTransportMeansPhase5ValidationDecider GetDepartureCusTransportMeansPhase5ValidationDecider() => new DepartureCusTransportMeansPhase5ValidationDecider();
}
