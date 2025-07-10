using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Customs.EU.MessageContracts;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.ASYCUDA.Business;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.EU.Manifest.ICS2.Business
{
	public class EUManifestMessageSender
	{
		public EUManifestMessageSender(AsycudaManifestHeader header)
		{
			ManifestHeader = Argument.NotNull(header, nameof(header));
		}

		AsycudaManifestHeader ManifestHeader { get; }

		public string SendFilingMessage()
		{
			var messageType = ManifestHeader.GetCustomsMessageType(false);
			return SendMessageCore(messageType, null);
		}

		public string SendAmendmentMessage(ICS2AmendedItemsHeader amendedItemsHeader)
		{
			var messageType = amendedItemsHeader?.MessageType ?? ManifestHeader.GetCustomsMessageType(true);
			return SendMessageCore(messageType, amendedItemsHeader);
		}

		public string SendMessage(string messageType)
		{
			return SendMessageCore(messageType, null);
		}

		string SendMessageCore(string messageType, ICS2AmendedItemsHeader amendedItemsHeader)
		{
			var result = Res.GetString("64AC98BE-7F7D-4B91-B970-AAFAD76F1414", "Failed to send message.");

			if (ManifestHeader.IsAwaitingResponse())
			{
				result = Res.GetString("F7A8DF43-7B53-4FF7-ACAA-20E072AC518A",
					"There are messages waiting for a response. You are unable to send a message until a valid response is received. If a response has been received, exit the job, then re-open to refresh the status");
			}
			else
			{
				var messagesForSending = new List<EDIMessage>();
				var requestHeaders = new Dictionary<RequestHeader, IZType>();

				var requestStatus = GetRequestStatusFromMessageType(messageType);

				foreach (var messageBuilder in GetMessageBuilders(messageType, amendedItemsHeader))
				{
					if (messageBuilder.Builder != null)
					{
						var message = GetEDIMessage();
						message.EM_MessageType = messageType;
						message.EM_MessageText = messageBuilder.Builder.GetXMLMessage();
						ManifestHeader.Messages.Add(message);

						messagesForSending.Add(message);

						foreach (var requestHeader in messageBuilder.AmendedItems.Select(c => c.RequestHeader))
						{
							if (requestHeader != null)
							{
								requestHeaders[requestHeader] = requestHeader.EUS_StatusInfo.OriginalValue;
								requestHeader.EUS_Status = requestStatus;
							}
						}
					}
				}

				var count = messagesForSending.Count;
				if (count > 0)
				{
					var oldMessageStatus = ManifestHeader.AMA_MessageStatus;
					var oldLRN = ManifestHeader.LocalReferenceNumber;
					ManifestHeader.AMA_MessageStatus = MessageStatusCodeList.Codes.Awaiting;

					try
					{
						ManifestHeader.Factory.Save();

						result = count > 1
							? Res.GetString("E69E2525-E579-4E8D-9BD0-D2E73E60A274", "{0} messages have been sent.", count)
							: Res.GetString("29E3DB4F-C97B-41F1-96A8-889645A4C457", "The message has been sent.");
					}
					catch (ZSaveException ex)
					{
						messagesForSending.ForEach(c => c.Delete());
						requestHeaders.ForEach(c => c.Key.EUS_StatusInfo.Value = c.Value);

						ManifestHeader.AMA_MessageStatus = oldMessageStatus;
						ManifestHeader.RollBackLocalReferenceNumberOnSavingFailed(oldLRN);
						ZExceptionReporting.HandleSaveException(ex);
					}
				}
			}

			return result;
		}

		IEnumerable<(IMessageBuilder Builder, IEnumerable<IAmendedItem> AmendedItems)> GetMessageBuilders(string messageType, ICS2AmendedItemsHeader amendedItemsHeader)
		{
			if (amendedItemsHeader?.HasAmendedItems() ?? false)
			{
				var groups = GetAmendedItemGroups(messageType, amendedItemsHeader.AmendedItems.Cast<ICS2AmendedItem>().Where(c => c.IsSelected));

				foreach (var group in groups)
				{
					yield return (EUICS2MessageBuilderLoader.Instance.GetMessageBuilder(messageType, ManifestHeader, group.ToArray()), group);
				}
			}
			else
			{
				yield return (EUICS2MessageBuilderLoader.Instance.GetMessageBuilder(messageType, ManifestHeader, Array.Empty<IAmendedItem>()), Array.Empty<IAmendedItem>());
			}
		}

		ICS2OutboundEDIMessage GetEDIMessage()
		{
			var message = ManifestHeader.Factory.New<ICS2OutboundEDIMessage>();
			message.EM_LinkedObject = ManifestHeader;

			var credential = ManifestHeader.ICS2Credential;
			if (credential != null)
			{
				message.EM_GP = credential.PK;
			}

			return message;
		}

		IEnumerable<IGrouping<ZString, ICS2AmendedItem>> GetAmendedItemGroups(string messageType, IEnumerable<ICS2AmendedItem> items)
		{
			switch (messageType)
			{
				case MessageTypes.Codes.R02:
				case MessageTypes.Codes.R03:
					{
						return items.GroupBy(c => c.ResponsibleMemberState);
					}

				default:
					{
						return items.GroupBy(c => c.Identifier);
					}
			}
		}

		string GetRequestStatusFromMessageType(string messageType)
		{
			switch (messageType)
			{
				case MessageTypes.Codes.A14:
				case MessageTypes.Codes.A16:
				case MessageTypes.Codes.A17:
				case MessageTypes.Codes.A22:
				case MessageTypes.Codes.A24:
				case MessageTypes.Codes.A26:
				case MessageTypes.Codes.A40:
					{
						return MessageStatusCodeList.Codes.Sent;
					}

				case MessageTypes.Codes.R02:
				case MessageTypes.Codes.R03:
					{
						return MessageStatusCodeList.Codes.Awaiting;
					}

				default:
					{
						return MessageStatusCodeList.Codes.Unknown;
					}
			}
		}
	}
}
