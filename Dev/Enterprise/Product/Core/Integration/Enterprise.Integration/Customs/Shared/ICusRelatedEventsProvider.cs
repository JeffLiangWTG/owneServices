using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Integration
{
	public static partial class Customs
	{
		public static partial class Shared
		{
			public interface ICusRelatedEventsProvider
			{
				BusinessObject[] CusRelatedBusinessObjects(Forwarding.IForwardingShipment shipment, ZString referenceNumber);
			}
		}
	}
}