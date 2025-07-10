using System.Linq;
using System.Xml;
using System.Xml.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.IT.Business;

public static class MessageProcessorHelper
{
	public static ZString RetrieveValueOfXmlNode(ZString content, ZString tagName, bool throwExceptionIfOccurs = false)
	{
		try
		{
			var doc = XDocument.Parse(content);
			return doc?.Descendants().SingleOrDefault(node => node.Name.LocalName == tagName)?.Value ?? ZString.Empty;
		}
		catch (XmlException)
		{
			if (throwExceptionIfOccurs)
			{
				throw;
			}
			return ZString.Empty;
		}
	}

	public static ITEDIInterchange GetSentInterchangeWithTrackingId(ZGuid eHubTrackingID, BusinessObjectFactory factory)
	{
		var query = new ZQuery(EDIInterchangeSchema.EI_SessionGUID, eHubTrackingID);
		query.AddToFilter(EDIInterchangeSchema.EI_ReceiveTransmit, EDIInterchange.Direction.Transmit);
		return factory.LoadTop1<ITEDIInterchange>(query);
	}

	public static string UnableToLocateTheRelatedSentInterchangeWithSessionGuid(ZGuid trackingId)
		=> Res.GetString("6235E266-02ED-4567-927D-617D14BCD59A", "Unable to locate the related sent interchange with Session ID = {0}.", trackingId.IsEmpty ? (NoResString)"<empty>" : trackingId.ToString());
}
