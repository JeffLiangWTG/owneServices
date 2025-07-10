using Enterprise.Customs.EU.Business;

namespace Enterprise.Customs.GB.Ccsuk.AirCargoInventory.Messaging
{
	abstract class CIMFFM : CargoImpBase
	{
		public CIMFFM(ErrorCollector ec)
			: base(ec)
		{ }
	}
}
