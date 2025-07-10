using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Xml;
using CargoWise.Common;
using CargoWise.Customs.CH.MessageDefinitions;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.CH.Business;

public abstract class BaseInboundMessageCreator : IInboundMessageCreator
{
	void IInboundMessageCreator.CreateMessagesForInterchange(EDIInterchange interchange)
	{
		Argument.NotNull(interchange, nameof(interchange));
		GenerateMessageFromInterchange(interchange);
	}

	public BaseInboundMessageCreator(LoggingInformation logger)
	{
		LoggingInformation = logger;
	}

	protected LoggingInformation LoggingInformation { get; }

	public virtual bool RemoveSoapEnvelope => false;

	void GenerateMessageFromInterchange(EDIInterchange interchange)
	{
		var parsingResults = GetResponseParsingResults(interchange).ToArray();
		if (parsingResults.Any())
		{
			var messageNumber = 1;
			foreach (var parsingResult in parsingResults)
			{
				if (CreateMessageResponse(interchange, parsingResult, messageNumber))
				{
					messageNumber += 1;
				}
			}
		}
		else
		{
			interchange.EI_Status = EDIInterchange.Status.Error;
			interchange.Logs.AddNew(Events.ErrorReport, "NO CH CUSTOMS DATA");
		}
	}

	protected virtual IEnumerable<ResponseParsingResult> GetResponseParsingResults(EDIInterchange interchange)
	{
		using (var reader = interchange.GetEI_BodyTextReader())
		{
			return MIMETypeXTParser.ParseTextHttp(reader);
		}
	}

	bool CreateMessageResponse(EDIInterchange interchange, ResponseParsingResult parsingResult, int messageNumber)
	{
		if (!parsingResult.IsEmpty)
		{
			if (parsingResult.IsXml)
			{
				return CreateMessageResponseForXml(interchange, parsingResult, messageNumber);
			}
			else if (parsingResult.IsPdf)
			{
				return CreateMessageResponseForPdf(interchange, parsingResult, messageNumber);
			}
			else if (parsingResult.IsJson)
			{
				return CreateMessageResponseForJson(interchange, parsingResult, messageNumber);
			}
			else if (messageNumber == 1 && !string.IsNullOrEmpty(parsingResult.BodyText))
			{
				return CreateMessage(interchange, null, messageNumber, parsingResult.BodyText, (NoResString)"Unrecognized response message type");
			}
		}

		return false;
	}

	bool CreateMessageResponseForPdf(EDIInterchange interchange, ResponseParsingResult response, int messageNumber)
	{
		ZString messageText = ZString.Empty;
		ZString messageSubType = MessageSubTypeCodeList.Codes.Document;
		ZString errorMessage = ZString.Empty;

		try
		{
			var attachedDocument = new AttachedDocument()
			{
				FileName = response.Description.Trim() + ".pdf",
				ImageData = response.BodyData,
				Type = new AttachedDocumentType()
				{
					Code = Core.Constants.RefDocTypes.CustomsAuthority,
					Description = Core.Constants.RefDocTypeDescriptions.CustomsAuthority
				}
			};
			messageText = CargoWise.Customs.Shared.MessageContracts.XmlObjectSerializer.SerializeDefaultSettingsWithoutNamespaces(attachedDocument);
		}
		catch (Exception ex) when (!ex.IsCriticalException())
		{
			errorMessage = FormattableString.Invariant($"Message {messageNumber}: error serializing attached document xml");
			ErrorReporter.ReportOnce("CH.InboundMessageCreator.CreateMessageResponseForPdf", errorMessage, ex);
		}

		return CreateMessage(interchange, messageSubType, messageNumber, messageText, errorMessage);
	}

	bool CreateMessageResponseForXml(EDIInterchange interchange, ResponseParsingResult response, int messageNumber)
	{
		ZString xmlResponse = ZString.Empty;
		ZString messageSubType = ZString.Empty;
		ZString errorMessage = ZString.Empty;
		ZString messageText = ZString.Empty;

		try
		{
			xmlResponse = RemoveSoapEnvelope ? MIMETypeXTParser.ParseSoapEnvelope(response.BodyText) ?? response.BodyText : response.BodyText;
		}
		catch (XmlException ex)
		{
			errorMessage = FormattableString.Invariant($"Message {messageNumber}: error reading xml");
			LoggingInformation?.LogError(errorMessage);
			ErrorReporter.ReportOnce("CH.InboundMessageCreator.CreateMessageResponseForXml", errorMessage, ex);
		}

		if (!xmlResponse.IsEmpty)
		{
			try
			{
				messageSubType = GetMessageSubTypeFromResponse(xmlResponse);
				if (!messageSubType.IsEmpty)
				{
					messageText = xmlResponse;
				}
			}
			catch (NotSupportedException ex) when (!ex.IsCriticalException())
			{
				errorMessage = FormattableString.Invariant($"Message {messageNumber}: Not Supported XML Schema");
				LoggingInformation?.LogError(errorMessage);
				ErrorReporter.ReportOnce("CH.InboundMessageCreator.ResponseAnalyzingError", errorMessage, ex);
			}
		}

		return CreateMessage(interchange, messageSubType, messageNumber, messageText, errorMessage);
	}

	bool CreateMessageResponseForJson(EDIInterchange interchange, ResponseParsingResult response, int messageNumber)
	{
		ZString messageSubType = ZString.Empty;
		ZString errorMessage = ZString.Empty;
		var messageText = response.BodyText;

		if (!string.IsNullOrEmpty(messageText))
		{
			messageSubType = GetMessageSubTypeFromResponse(messageText);
			try
			{
				var jsonResponse = JsonNode.Parse(messageText);
				messageText = jsonResponse?.ToString();
			}
			catch (JsonException ex)
			{
				errorMessage = FormattableString.Invariant($"Message {messageNumber}: error reading JSON");
				LoggingInformation?.LogError(errorMessage);
				ErrorReporter.ReportOnce("CH.InboundMessageCreator.CreateMessageResponseForJson", errorMessage, ex);
			}
		}
		return CreateMessage(interchange, messageSubType, messageNumber, messageText, errorMessage);
	}

	internal static bool CreateMessage(EDIInterchange interchange, ZString messageSubType, int messageNumber, ZString messageText, string errorMessage = null, string messageType = null)
	{
		var newMessage = interchange.ContainedMessages.AddNew();
		newMessage.EM_ApplicationCode = interchange.EI_ApplicationCode;
		newMessage.EM_MessageType = messageType ?? interchange.EI_InterchangeType;
		newMessage.EM_MessageSubType = messageSubType;
		newMessage.EM_ReceiveTransmit = ReceiveTransmitList.Codes.Receive;
		newMessage.EM_Status = GetMessageStatus(messageSubType);
		newMessage.EM_MessageNum = messageNumber.ToString().PadLeft(4, '0');
		newMessage.EM_MessageText = messageText;

		if (!string.IsNullOrEmpty(errorMessage))
		{
			newMessage.Logs.AddNew(Events.ErrorReport, errorMessage);
		}
		return true;
	}

	static ZString GetMessageStatus(ZString messageSubType) => messageSubType.IsEmpty ? EDIMessageStatusList.Codes.Discarded : EDIMessageStatusList.Codes.Queued;

	protected virtual ZString GetMessageSubTypeFromResponse(string xmlResponse) => MessageSubTypeCodeList.Codes.Undefined;
}
