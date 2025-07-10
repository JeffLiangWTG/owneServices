using CargoWise.Types;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.UniversalDataBuss.DataObjects.Universal;

[XsdSchema(UniversalXmlInfo.CommonSchemaName)]
public class GeoLocation : IDataObject
{
	public static GeoLocation New(ZGeography location)
	{
		return location.IsEmpty || !location.IsValid || !location.IsPoint
			? null
			: new GeoLocation { Latitude = location.Latitude, Longitude = location.Longitude, };
	}

	public ZDecimal? Latitude { get; set; }
	public ZDecimal? Longitude { get; set; }
}
