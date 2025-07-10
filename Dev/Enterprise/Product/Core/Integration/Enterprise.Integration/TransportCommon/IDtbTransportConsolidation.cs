using CargoWise.Types;

namespace Enterprise.Integration.TransportCommon
{
	public interface IDtbTransportConsolidation
	{
		ZGuid PK { get; }
		ZString KB_ParentTableCode { get; set; }
		ZGuid KB_ParentID { get; set; }
	}
}
