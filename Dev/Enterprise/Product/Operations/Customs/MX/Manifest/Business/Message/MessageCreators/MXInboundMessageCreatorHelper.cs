using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.MX.Manifest.Business
{
	public static class MXInboundMessageCreatorHelper
	{
		public static EDIInterchange GetSentInterchangeWithTrackingId(ZGuid eHubTrackingID, BusinessObjectFactory factory)
		{
			var query = new ZQuery(EDIInterchangeSchema.EI_SessionGUID, eHubTrackingID);
			query.AddToFilter(EDIInterchangeSchema.EI_ReceiveTransmit, EDIInterchange.Direction.Transmit);
			return factory.LoadTop1<EDIInterchange>(query);
		}

		public static EDIMessage GetOriginalMessage(ZGuid interchangePK, BusinessObjectFactory factory)
		{
			var query = new ZQuery(EDIMessageSchema.EM_EI, interchangePK);
			return factory.LoadTop1<EDIMessage>(query);
		}
	}
}
