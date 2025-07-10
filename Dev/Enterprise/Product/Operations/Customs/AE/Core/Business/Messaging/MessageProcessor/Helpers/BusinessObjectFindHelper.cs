using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.AE.Business;

public static class BusinessObjectFindHelper
{
	public static EDIMessage GetOutboundMessage(this BusinessObjectFactory factory, ZString outboundInterchangeNumber)
	{
		if (outboundInterchangeNumber.IsEmpty)
		{
			return null;
		}

		var query = new ZQuery { OrderBy = EDIInterchangeSchema.EI_SystemCreateTimeUtc.Name + OrderByClause.Descending };
		query.AddToFilter(EDIInterchangeSchema.EI_ApplicationCode, ApplicationCodeList.Codes.UAECustoms);
		query.AddToFilter(EDIInterchangeSchema.EI_InterchangeNum, outboundInterchangeNumber);
		query.AddToFilter(EDIInterchangeSchema.EI_ReceiveTransmit, EDIInterchange.Direction.Transmit);
		query.AddToFilter(EDIInterchangeSchema.EI_Status, EDIInterchangeStatusList.Codes.Sent);
		var interchange = factory.LoadTop1<EDIInterchange>(query);
		return GetOutboundMessageFromOutboundInterchange(factory, interchange);
	}

	public static EDIInterchange GetOutboundInterchangeBySessionGUID(this BusinessObjectFactory factory, ZGuid sessionGUID)
	{
		if (!sessionGUID.IsValid)
		{
			return null;
		}

		var query = new ZQuery { OrderBy = EDIInterchangeSchema.EI_SystemCreateTimeUtc.Name + OrderByClause.Descending };
		query.AddToFilter(EDIInterchangeSchema.EI_ApplicationCode, ApplicationCodeList.Codes.UAECustoms);
		query.AddToFilter(EDIInterchangeSchema.EI_SessionGUID, sessionGUID);
		query.AddToFilter(EDIInterchangeSchema.EI_ReceiveTransmit, EDIInterchange.Direction.Transmit);
		query.AddToFilter(EDIInterchangeSchema.EI_Status, EDIInterchangeStatusList.Codes.Sent);
		return factory.LoadTop1<EDIInterchange>(query);
	}

	public static EDIMessage GetOutboundMessageFromOutboundInterchange(this BusinessObjectFactory factory, EDIInterchange interchange)
	{
		return interchange?.ContainedMessages.Cast<EDIMessage>().SingleOrDefault();
	}

	public static T GetMessageAttachee<T>(this BusinessObjectFactory factory, SchemaStringColumn documentIdColumn, ZString documentId, bool getLatest = true)
		where T : class, IMessageAttachee
	{
		if (documentId.IsEmpty)
		{
			return null;
		}

		var query = new ZQuery();
		if (getLatest)
		{
			var systemCreateTimeColumn = documentIdColumn.ColumnPrefix + "_SystemCreateTimeUtc";
			query.OrderBy = systemCreateTimeColumn + OrderByClause.Descending;
		}
		query.AddToFilter(documentIdColumn, documentId);
		return factory.LoadTop1<T>(query);
	}
}
