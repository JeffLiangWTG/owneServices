using CargoWise.Types;

namespace Enterprise.ZArchitecture.GUI.BrowserInterop
{
	public interface IUpdateFilterData
	{
	}

	public class CarrierUpdateFilterData : IUpdateFilterData
	{
		public ZDateTime startDate, expiryDate;
		public ZGuid serviceProviderPK;
		public ZString contractID, transportMode, containerType;
		public ZBool isCommodityHazardousRequired;
	}

	public class ClientUpdateFilterData : IUpdateFilterData
	{
		public ZDateTime startDate, expiryDate;
		public ZGuid clientPK;
		public ZString contractID;
	}
}
