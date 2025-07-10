
using CargoWise.Types;
namespace Enterprise.Customs.GB.Ccsuk.AirCargoInventory.Messaging
{
	interface IFsn
	{
		ZString SplitNumber { get; }
		ZString Pieces { get; }
		ZString Date { get; }
		ZString AgentReference { get; }
		ZString CAC { get; }
		ZString CAT { get; }
		ZString AwbSerialNumber { get; }
	}
}
