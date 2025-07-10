using CargoWise.Types;

namespace Enterprise.Customs.FR.Messaging.Interfaces.ECS
{
	public interface IECSData
	{
		ZString MRN { get; }
		ZString Office { get; }
		ZString EORI { get; }
		ZString Agreement { get; }
		ZString Location { get; }

		ZString SchemaID { get; }
		ZString SchemaVersion { get; }
		ZString PartyId { get; }
		ZString TransactionId { get; }
		ZShort Numseq { get; }
	}
}
