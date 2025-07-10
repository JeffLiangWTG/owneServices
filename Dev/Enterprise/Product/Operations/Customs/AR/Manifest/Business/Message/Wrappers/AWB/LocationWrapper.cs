using CargoWise.Customs.AR.MessageContracts;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.AR.Manifest.Business
{
	internal class LocationWrapper : ILocation
	{
		internal LocationWrapper(BusinessObjectFactory factory, ZString location)
		{
			airportInfo = ARHelperClass.AirportInfo(factory, location);
		}
		readonly (ZString id, ZString name) airportInfo;

		string ILocation.ID => airportInfo.id;

		string ILocation.Name => airportInfo.name;
	}
}
