using System;
using System.Xml.Linq;
using System.Xml.XPath;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Messaging.Business;
using Enterprise.xTMessaging.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.CH.Business;

public class UniversalEventInboundMessageCreator : IInboundMessageCreator
{
	public UniversalEventInboundMessageCreator(LoggingInformation logger)
	{
		LoggingInformation = logger;
	}

	protected LoggingInformation LoggingInformation { get; }

	void IInboundMessageCreator.CreateMessagesForInterchange(EDIInterchange interchange)
	{
		Argument.NotNull(interchange, nameof(interchange));

		ZString messageSubType = ZString.Empty;
		ZString errorMessage = ZString.Empty;
		var bodyText = interchange.EI_BodyText;
		string outgoingInterchangeType = null;

		if (bodyText.IsEmpty)
		{
			interchange.EI_Status = EDIInterchange.Status.Error;
			interchange.Logs.AddNew(Events.ErrorReport, "NO CH CUSTOMS DATA");
		}
		else
		{
			try
			{
				if (!bodyText.IsEmpty && bodyText.Contains("<UniversalInterchange ")
					&& XDocument.Parse(bodyText)?.Root?.XPathSelectElement((NoResString)"//*[local-name()='UniversalInterchange']//*[local-name()='Body']")?.FirstNode is XElement bodyNode)
				{
					bodyText = bodyNode.ToString();
				}

				messageSubType = GetMessageSubTypeFromUniversalEvent(new UniversalEventWrapper(bodyText));
				if (messageSubType.IsEmpty)
				{
					errorMessage = (NoResString)"Unrecognized response message type";
				}

				if (interchange.EI_InterchangeType == MessageTypeCodeList.Codes.XER)
				{
					outgoingInterchangeType = interchange.Factory.GetOutgoingInterchangeFromSessionId(interchange.EI_SessionGUID)?.EI_InterchangeType;
				}
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				errorMessage = FormattableString.Invariant($"Interchange {interchange.EI_InterchangeNum}: error reading Universal Event response");
				LoggingInformation?.LogError(errorMessage);
				ErrorReporter.ReportOnce("CH.UniversalEventInboundMessageCreator.CreateMessageForUniversalEvent", errorMessage, ex);
			}

			BaseInboundMessageCreator.CreateMessage(interchange, messageSubType, 1, bodyText, errorMessage, outgoingInterchangeType);
		}
	}

	protected virtual ZString GetMessageSubTypeFromUniversalEvent(UniversalEventWrapper eventData)
	{
		switch (eventData.EventType)
		{
			case AutoEvents.InterchangeAcknowledgedCode:
				return MessageSubTypeCodeList.Codes.Acknowledged;
			case AutoEvents.InterchangeRejectedCode:
				return MessageSubTypeCodeList.Codes.Rejected;
			default:
				return ZString.Empty;
		}
	}
}
