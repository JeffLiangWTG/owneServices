using CargoWise.Types;

namespace Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects
{
	public interface IOnwardCarrierProvider
	{
		ZString OnwardCarrier { get; }
		ZString OnwardMode { get; }
	}
}
