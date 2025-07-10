using CargoWise.Types;
using Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects;

namespace Enterprise.Customs.GB.Ccsuk.AirCargoInventory.Messaging
{
	class CuscarWrapperFromCusMawbUFO : CuscarWrapperFromCusMawb, ICuscar
	{
		public CuscarWrapperFromCusMawbUFO(CusMAWB mawbToWrap)
			: base(mawbToWrap)
		{
		}

		ZString ICuscar.AirportOfArrival
		{
			get { return mawb.CargoTerminalOperatorAirport; }
		}
	}
}
