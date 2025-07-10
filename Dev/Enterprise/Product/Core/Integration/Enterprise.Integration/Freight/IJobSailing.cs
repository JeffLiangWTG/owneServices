using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Integration.Freight
{
	public interface IJobSailing : IBusiness
	{
		ZGuid PK { get; }
		ZString JX_UniqueReference { get; set; }
		ZString JX_ServiceString { get; set; }
		ZGuid JX_JA { get; set; }
		ZGuid JX_JB { get; set; }
		IVoyageOrigin Origin { get; }
		IVoyageDestination Destination { get; }
	}
}
