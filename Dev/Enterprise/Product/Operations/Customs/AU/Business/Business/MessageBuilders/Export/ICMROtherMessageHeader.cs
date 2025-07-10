using CargoWise.Types;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public interface ICMROtherMessageHeader
	{
		ZString DepotEstablishmentID { get; }
		ZString DestinationEstablishmentID { get; }
		ZString CAN { get; }
		EDIMessageCollection Messages { get; }
	}
}
