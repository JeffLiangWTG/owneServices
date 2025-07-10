using System.Globalization;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.GB.CDS.Helpers
{
	public static class EventParentFinderHelper
	{
		public static EDIInterchange GetOutboundInterchange(ZGuid eHubTrackingID, BusinessObjectFactory factory)
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

		public static ZString GenericErrorWrapper(ZString? errorType, ZString errorDetail)
		{
			var errType = errorType.HasValue && !errorType.Value.IsEmpty ? errorType.Value : new ZString("GENERAL_ERROR");

			const string xmlErrorFormat = "<errorResponse><code>{0}</code><message>{1}</message></errorResponse>"; // XML Content

			return string.Format(CultureInfo.InvariantCulture, xmlErrorFormat, System.Security.SecurityElement.Escape(errType), System.Security.SecurityElement.Escape(errorDetail));
		}

		public const string ehubRecipient = "eHub";
	}
}
