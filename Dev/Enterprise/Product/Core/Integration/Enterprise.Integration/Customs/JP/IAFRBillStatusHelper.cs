using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Integration
{
	public static partial class Customs
	{
		public static partial class JP
		{
			public static partial class AFR
			{
				public interface IAFRStatusHelper
				{
					ZString GetAFRBillStatus(Forwarding.IForwardingConsol consol);
					ZString GetAFRBillStatus(IJPAFRHeader header);
					ZString GetAFRBillStatus(Forwarding.IForwardingShipment shipment);
					ZString GetAFRBillStatusDescription(BusinessObjectFactory factory, ZString status);
				}
			}
		}
	}
}
