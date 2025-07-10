using CargoWise.Types;

namespace Enterprise.Customs.EU.NCTS.Messaging
{
	public interface ICustomsOffice
	{
		ZString ArrivalTime { get; }
		ZString ReferenceNumber { get; }
	}
}
