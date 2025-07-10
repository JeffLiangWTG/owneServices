using CargoWise.Types;

namespace Enterprise.Integration.Freight
{
	public interface IPortHubSelection
	{
		ZGuid TY_OA_DepotAddress { get; set; }
		ZString TY_Direction { get; set; }
	}
}
