using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Integration.Freight
{
	public interface IVoyageOrigin : IBusiness
	{
		ZGuid PK { get; }
		ZDateTime JA_E_DEP { get; set; }
		ZDateTime JA_S_DEP { get; set; }
		ZGuid JA_JV { get; set; }
		ZString JA_RL_NKPortOfLoading { get; set; }
		IJobVoyage Voyage { get; }
	}
}
