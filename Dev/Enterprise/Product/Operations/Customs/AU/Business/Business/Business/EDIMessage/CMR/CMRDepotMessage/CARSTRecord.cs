using Enterprise.Freight.CFS.Business;

namespace Enterprise.Customs.AU.Declaration.Business;

public class CARSTRecord
{
	public CFSShipment Shipment;
	public CFSContainer Container;
	public CFSLoadListConsol Consol;
	public ICMRDepotMessageLine Line;
}
