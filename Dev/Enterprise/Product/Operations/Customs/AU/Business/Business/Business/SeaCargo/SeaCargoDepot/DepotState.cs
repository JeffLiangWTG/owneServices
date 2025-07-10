
namespace Enterprise.Customs.AU.Declaration.Business
{
	public enum DepotState
	{
		Unknown,
		ImpendingCargo,
		ImpendingCargoCancelled,
		CargoArrived,
		CargoArrivedError,
		CargoUnpacked,
		CargoUnpackedError,
		CargoDelivered,
		CargoDeliveredError,
		CargoClear
	}
}
