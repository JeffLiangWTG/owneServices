using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Integration.Freight
{
	public interface IVoyageDestination : IBusiness
	{
		ZGuid PK { get; }
		ZString JB_RL_NKPortOfDischarge { get; set; }
		ZGuid JB_JV { get; set; }
		ZDateTime JB_E_ARV { get; set; }
		ZDateTime JB_S_ARV { get; set; }
		IJobVoyage Voyage { get; }
	}
}
