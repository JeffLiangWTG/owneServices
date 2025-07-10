using System.Collections.Generic;
using System.Xml.Linq;
using System.Xml.XPath;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.xTMessaging.Business;

namespace Enterprise.Customs.CH.Business;

public abstract class BaseMessageListAcceptanceMessageProcessor : BaseInboundMessageProcessor<UniversalEventWrapper>
{
	public BaseMessageListAcceptanceMessageProcessor(LoggingInformation logger) : base(logger)
	{
	}

	protected override IReadOnlyList<ZString> MessageTypesToIncludeCore => new ZString[] { MessageTypeCodeList.Codes.MSL };

	protected override IReadOnlyList<ZString> MessageSubTypesToIncludeCore => new ZString[] { MessageSubTypeCodeList.Codes.Acknowledged };

	protected override BusinessObject FindLinkedObject(EDIMessage message, UniversalEventWrapper xmlObject) => FindLinkedObjectByOutgoingSessionID(message);

	protected override UniversalEventWrapper DeserializeResponse(CHEDIMessage message) => message.UniversalEventData;

	protected override void ProcessResponseMessage(CHEDIMessage ediMessage, UniversalEventWrapper eventData)
	{
		if (ediMessage.EM_LinkedObject is GlbCompany company)
		{
			var xmlText = eventData.GetResponseMessage();
			var xmlDoc = XDocument.Parse(xmlText);

			ZString messageId = ZString.Empty;
			ZShort sequenceNumber = 0;
			foreach (var message in xmlDoc.XPathSelectElements("//messages/message/messageId"))
			{
				messageId = message.Value;
				CreateTransactionIfNotExists(company, messageId, ++sequenceNumber);
			}

			if (!messageId.IsEmpty)
			{
				UpdateOrAddLastMessageId(company, messageId);
			}
		}
	}

	void CreateTransactionIfNotExists(GlbCompany company, ZString messageId, ZShort sequenceNumber)
	{
		if (company.LoadTransactionByMessageId(ApplicationCode, messageId) == null)
		{
			company.CreateMessageIdTransaction(ApplicationCode, messageId, sequenceNumber);
		}
	}

	void UpdateOrAddLastMessageId(GlbCompany company, ZString messageId)
	{
		(company.LoadLastMessageIdTransaction(ApplicationCode) ?? company.CreateLastMessageIdTransaction(ApplicationCode)).CPT_TransactionID = messageId;
	}
}
