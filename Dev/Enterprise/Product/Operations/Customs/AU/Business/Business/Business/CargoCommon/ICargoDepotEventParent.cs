using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public interface ICargoDepotEventParent
	{
		LogsForNominatedEvent CargoReceivedAtDepotLogs { get; }
		LogsForNominatedEvent CargoAvailableAtDepotLogs { get; }
		LogsForNominatedEvent ReadyForLocalDeliveryLogs { get; }
		bool IsCargoStatusClear { get; }
		bool IsHeldAtOutturn { get; set; }
	}
}
