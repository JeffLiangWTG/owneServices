namespace Enterprise.Customs.EU.NCTS.Business.Interfaces
{
	public interface ICanSupportPhase5
	{
		bool IsPhase5 { get; }

		bool IsPhase5Arrival { get; }

		bool IsPhase5Departure { get; }
	}
}
