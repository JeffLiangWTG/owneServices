using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Integration.Freight
{
	public interface IJobVoyage : IBusiness
	{
		ZGuid PK { get; }
		ZString JV_RV_NKVessel { get; set; }
		ZString JV_VoyageFlight { get; set; }
	}
}
