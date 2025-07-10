using CargoWise.EntityFramework;

namespace Enterprise.Integration
{
	public static partial class Customs
	{
		public static partial class Shared
		{
			public interface ICusRelatedParentEventsProvider
			{
				BusinessObject[] CusRelatedParentBusinessObjects(Forwarding.IForwardingShipment shipment);
			}
		}
	}
}
