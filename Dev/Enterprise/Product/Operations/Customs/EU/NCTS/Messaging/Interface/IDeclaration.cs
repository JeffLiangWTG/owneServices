using CargoWise.Types;

namespace Enterprise.Customs.EU.NCTS.Messaging
{
	public interface IDeclaration
	{
		ZBool IsProduction { get; }
		ZString LocalReferenceNumber { get; }
		ZString MovementReferenceNumber { get; }
		ITrader Principal { get; }
		ITrader Consignee { get; }
		ZString DepartureCustomsOfficeReferenceNumber { get; }
		ZString DestinationCustomsOfficeReferenceNumber { get; }
	}
}
