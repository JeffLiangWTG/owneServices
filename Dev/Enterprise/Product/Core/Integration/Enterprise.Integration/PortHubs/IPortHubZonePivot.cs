using CargoWise.Types;

namespace Enterprise.Integration.Freight
{
	public interface IPortHubZonePivot
	{
		ZGuid TX_TY_Hub { get; set; }
		ZGuid TX_TZ_Zone { get; set; }
		ZShort TX_PickupCutOffTimeVariance { get; set; }
	}
}
