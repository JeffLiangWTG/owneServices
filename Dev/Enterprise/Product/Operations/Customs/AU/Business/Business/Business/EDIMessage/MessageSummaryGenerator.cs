using System.Linq;
using System.Xml;
using CargoWise.Types;
using Enterprise.Customs.AU.Declaration.Business.RexOwnershipSoap;
using Enterprise.Messaging.Business;
using Enterprise.UniversalDataBuss.Management;
using UniversalEvent = Enterprise.UniversalDataBuss.DataObjects.Universal.Event;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public static class MessageSummaryGenerator
	{
		public static ZString GetMessageSummary(EDIMessage message)
		{
			ZString result;
			if (message is REXMessage rexMessage)
			{
				result = GetSummary(rexMessage);
			}
			else if (message is RFPEDIMessage rfpMessage)
			{
				result = GetSummary(rfpMessage);
			}
			else
			{
				result = message.EM_FormattedMessageText;
			}

			return result;
		}

		static ZString GetSummary(REXMessage message)
		{
			var builder = new ZStringBuilder();

			var xmlDocument = new XmlDocument();
			xmlDocument.LoadXml(message.EM_MessageText);

			if (xmlDocument.DocumentElement != null)
			{
				var localName = xmlDocument.DocumentElement.LocalName;
				if (localName == nameof(RexAcknowledgeOwnership))
				{
					var rexNumberNodeXPath = GetXPath(
						nameof(RexAcknowledgeOwnership),
						nameof(RexAcknowledgeOwnership.identification),
						nameof(RexAcknowledgeOwnership.identification.rexNumber)
					);
					var rexNumberNode = xmlDocument.SelectSingleNode(rexNumberNodeXPath);
					if (rexNumberNode != null)
					{
						var isAcceptedNodeXPath = GetXPath(nameof(RexAcknowledgeOwnership), nameof(RexAcknowledgeOwnership.isAccepted));
						var isAcceptedNode = xmlDocument.SelectSingleNode(isAcceptedNodeXPath);
						if (isAcceptedNode != null && bool.TryParse(isAcceptedNode.InnerText, out var accepted))
						{
							builder.Append(rexNumberNode.InnerText + " Acknowledgement " + (accepted ? "Accepted" : "Rejected"));
						}
					}
				}
				else if (localName == nameof(RexAcknowledgeOwnershipResponse))
				{
					var outcomeNodeXPath = GetXPath(nameof(RexAcknowledgeOwnershipResponse), nameof(RexAcknowledgeOwnershipResponse.outcome));
					var outcomeNode = xmlDocument.SelectSingleNode(outcomeNodeXPath);
					if (outcomeNode != null)
					{
						builder.Append($"Message Status: {outcomeNode.InnerText}");
					}
				}
				else
				{
					var eventDecoder = new NEXDOCEventDecoder(message.GetEM_MessageTextReader().Parse<UniversalEvent>(), null);

					if (!eventDecoder.NotificationTitle.IsEmpty && !eventDecoder.NotificationText.IsEmpty)
					{
						builder.Append("Notification Title: " + eventDecoder.NotificationTitle);
						builder.Append("Notification Text: " + eventDecoder.NotificationText);
					}
					else if (eventDecoder.EventValues.Context.MessageStatus.GetValueOrDefault() == Constants.MessageStatus.Error)
					{
						builder.Append("Error");
						foreach (var contextMessage in eventDecoder.EventValues.Context.Values.Where(c => c.Key.Type.EqualsIgnoringCase("message")))
						{
							builder.Append(contextMessage.Value.ToString());
						}
					}
				}
			}

			return builder.ToStringWithNewLineBetweenAppends();
		}

		static ZString GetXPath(params string[] elementNames)
		{
			var result = new ZStringBuilder();
			for (var idx = 0; idx < elementNames.Length; idx++)
			{
				result.Append(idx == 0 ? "//" : "/");
				result.Append($"*[translate(local-name(), 'ABCDEFGHIJKLMNOPQRSTUVWXYZ', 'abcdefghijklmnopqrstuvwxyz')='{elementNames[idx].ToLower()}']");
			}
			return result.ToString();
		}

		static ZString GetSummary(RFPEDIMessage message)
		{
			var builder = new ZStringBuilder();

			var xmlDocument = new XmlDocument();
			xmlDocument.LoadXml(message.EM_MessageText);
			var eventDecoder = new NEXDOCEventDecoder(message.GetEM_MessageTextReader().Parse<UniversalEvent>(), null);

			if (!eventDecoder.NotificationTitle.IsEmpty && !eventDecoder.NotificationText.IsEmpty)
			{
				builder.Append("Notification Title: " + eventDecoder.NotificationTitle);
				builder.Append("Notification Text: " + eventDecoder.NotificationText);
			}
			else if (eventDecoder.EventValues.Context.MessageStatus.GetValueOrDefault() == Constants.MessageStatus.Error)
			{
				builder.Append("Error");
				foreach (var contextMessage in eventDecoder.EventValues.Context.Values.Where(c => c.Key.Type.EqualsIgnoringCase("message")))
				{
					builder.Append(contextMessage.Value.ToString());
				}
			}

			return builder.ToStringWithNewLineBetweenAppends();
		}
	}
}
