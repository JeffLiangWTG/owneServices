using CargoWise.Customs.MX.MessageContracts;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.MX.Manifest.Business
{
	internal class LocationWrapper : ILocation
	{
		internal LocationWrapper(BusinessObjectFactory factory, ZString location)
		{
			airportInfo = AWBRequestHelper.AirportInfo(factory, location);
		}
		readonly (ZString id, ZString name) airportInfo;

		string ILocation.ID => airportInfo.id;

		string ILocation.Name => airportInfo.name;
	}
}
