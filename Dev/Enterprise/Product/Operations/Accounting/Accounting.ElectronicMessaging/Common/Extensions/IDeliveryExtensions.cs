using Enterprise.MasterFiles.Business.MessageDelivery;
using Enterprise.Messaging.Integration;

namespace Enterprise.Accounting.ElectronicMessaging.Common
{
	public static class IDeliveryExtensions
	{
		public static IXmlEDIInterchange GetCreatedInterchange(this IDelivery delivery)
			=> (delivery is EServicesDelivery eSrvcDelivery) ? eSrvcDelivery.InterchangeCreated : null;
	}
}
