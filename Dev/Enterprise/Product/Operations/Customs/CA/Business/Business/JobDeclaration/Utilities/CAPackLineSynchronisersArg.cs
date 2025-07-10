using System;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.CA.Business
{
	class CAPackLineSynchronisersArg : PackLineSynchronisersArg
	{
		public Func<CargoControlNumberCollectionSynchroniser> GetCargoControlNumberSynchroniser;
	}
}
