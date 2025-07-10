using System.Xml;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.KR.Messaging;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Schema;
using WTG.StaticAnalysis.Annotation;
using static Enterprise.Customs.KR.Messaging.Constants;

namespace Enterprise.Customs.KR.Business
{
	[CodeAlive("An instance is created via reflection using the attribute MessageType")]
	[MessageType(EDIInterchangeType.XER)]
	public class XERMessageProcessor : IMessageProcessor
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Xerf message")]
		public void Process(EDIMessage message)
		{
			const string reasonNodeName = "UniversalInterchange/Body/UniversalEvent/Event/EventParameters/Reason";

			var xmldoc = new XmlDocument();
			xmldoc.LoadXml(MessageEncoding.UTF8WithoutBOM.GetString(message.EM_MessageData));

			var xPathReason = StringUtils.ConvertToXPath(reasonNodeName);
			var reason = xmldoc.SelectSingleNode(xPathReason)?.InnerXml;

			if (!string.IsNullOrEmpty(reason))
			{
				message.EM_MessageData = MessageEncoding.UTF8WithoutBOM.GetBytes(reason);
			}

			if (message.Interchange != null)
			{
				var query = new ZQuery(EDIInterchangeSchema.EI_ReceiveTransmit, EDIInterchange.Direction.Transmit);
				query.AddToFilter(EDIInterchangeSchema.EI_SessionGUID, message.Interchange.EI_SessionGUID);
				var outEdiInterchange = message.Factory.LoadTop1<EDIInterchange>(query);

				var emailSent = false;

				if (outEdiInterchange?.ContainedMessages.Count > 0)
				{
					var outgoingEdiMessage = outEdiInterchange.ContainedMessages[0];
					var linkedObject = outgoingEdiMessage.EM_LinkedObject as IEDIMessageCollectionProviderWithID;
					linkedObject?.MarkAsFailed();

					if (linkedObject != null)
					{
						message.EM_LinkedObject = (BusinessObject)linkedObject;

						NotificationData notificationData = null;

						switch (linkedObject)
						{
							case CusMiscRequestHeader requestHeader:
								notificationData = NotificationSender.GetNotificationData(message, requestHeader, NotificationSender.XERNotificationEmailBody);
								break;
							case CusEntryHeader entryHeader:
								notificationData = NotificationSender.GetNotificationData(message, entryHeader, NotificationSender.XERNotificationEmailBody);
								break;
							default:
								break;
						}

						if (notificationData != null)
						{
							notificationData.OriginalMessageTypes = new ZString[] { outgoingEdiMessage.EM_MessageType };
							notificationData.MessageTypeDescription = message.EM_MessageType;
							notificationData.EmailGroup = ElectronicDocumentTypeList.IsExportOrLocalExportOutgoingMessage(outgoingEdiMessage.EM_MessageType) ? KRCustomsRegistry.Instance.ExportEmailGroup : KRCustomsRegistry.Instance.ImportEmailGroup;
							NotificationSender.ErrorSendNotification(notificationData, HtmlResponseEmailGenerator.ShownBelow);
							emailSent = true;
						}
					}
				}

				if (!emailSent)
				{
					var outgoingEDIMessage = outEdiInterchange?.ContainedMessages[0];
					var emailGroup = ElectronicDocumentTypeList.IsExportOrLocalExportOutgoingMessage(outgoingEDIMessage?.EM_MessageType ?? string.Empty) ? KRCustomsRegistry.Instance.ExportEmailGroup : KRCustomsRegistry.Instance.ImportEmailGroup;
					NotificationSender.ErrorSendNotification(message, NotificationSender.XERNotificationEmailBody, "시스템 에러 수신", emailGroup, HtmlResponseEmailGenerator.ShownBelow);
				}
			}
		}
	}
}
