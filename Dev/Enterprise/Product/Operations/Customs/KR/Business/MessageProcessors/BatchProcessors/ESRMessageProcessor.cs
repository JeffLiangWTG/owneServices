using System;
using System.Text.Json.Nodes;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.KR.Messaging;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using WTG.StaticAnalysis.Annotation;
using static Enterprise.Customs.KR.Messaging.Constants;

namespace Enterprise.Customs.KR.Business
{
	[CodeAlive("An instance is created via reflection using the attribute MessageType")]
	[MessageType(EDIInterchangeType.ESR)]
	public class ESRMessageProcessor : IMessageProcessor
	{
		public void Process(EDIMessage message)
		{
			var messageData = message.EM_MessageText;
			var headers = JsonNode.Parse(messageData);
			var status = (string)headers[Constants.InterchangeJsonKeys.CustomStatus];
			var reason = (string)headers[Constants.InterchangeJsonKeys.CustomStatusDesc];

			if (reason != null)
			{
				var reasonDecoded = MessageEncoding.UTF8WithoutBOM.GetString(Convert.FromBase64String(reason));
				message.EM_MessageText = messageData.Replace(reason, reasonDecoded);
				reason = reasonDecoded;
			}

			if (CustomsErrorCodeList.IsInvalidGlbExternalPassword(status))
			{
				var wrapper = GlbCompanyWrapper.GetWrapper<GlbCompanyWrapper>(message.Branch.Company);
				var glbExternalPassword = wrapper.GetGlbExternalPassword<GlbExternalPassword>(PasswordTypesList.Codes.KRB);
				if (glbExternalPassword != null)
				{
					glbExternalPassword = message.Factory.Load<GlbExternalPassword>(glbExternalPassword.PK);
					glbExternalPassword.GP_PasswordStatus = PasswordStatusList.Codes.Invalid;
					glbExternalPassword.GP_StatusReason = reason;
				}
			}

			if (message.Interchange != null && status != CustomsErrorCodeList.Codes.C901)
			{
				var query = new ZQuery(EDIInterchangeSchema.EI_ReceiveTransmit, EDIInterchange.Direction.Transmit);
				query.AddToFilter(EDIInterchangeSchema.EI_SessionGUID, message.Interchange.EI_SessionGUID);
				var outEdiInterchange = message.Factory.LoadTop1<EDIInterchange>(query);
				var emailBody = NotificationSender.ESRNotificationEmailBody + MessageEncoding.UTF8WithoutBOM.GetString(message.EM_MessageData);

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
								notificationData = NotificationSender.GetNotificationData(message, requestHeader, emailBody);
								break;
							case CusEntryHeader entryHeader:
								notificationData = NotificationSender.GetNotificationData(message, entryHeader, emailBody);
								break;
							default:
								break;
						}

						if (notificationData != null)
						{
							notificationData.OriginalMessageTypes = new ZString[] { outgoingEdiMessage.EM_MessageType };
							notificationData.MessageTypeDescription = message.EM_MessageType;
							notificationData.EmailGroup = ElectronicDocumentTypeList.IsExportOrLocalExportOutgoingMessage(outgoingEdiMessage.EM_MessageType) ? KRCustomsRegistry.Instance.ExportEmailGroup : KRCustomsRegistry.Instance.ImportEmailGroup;
							NotificationSender.ErrorSendNotification(notificationData);
							emailSent = true;
						}
					}
				}

				if (!emailSent)
				{
					var outgoingEDIMessage = outEdiInterchange?.ContainedMessages[0];
					var emailGroup = ElectronicDocumentTypeList.IsExportOrLocalExportOutgoingMessage(outgoingEDIMessage?.EM_MessageType ?? string.Empty) ? KRCustomsRegistry.Instance.ExportEmailGroup : KRCustomsRegistry.Instance.ImportEmailGroup;
					NotificationSender.ErrorSendNotification(message, emailBody, (NoResString)"에러 수신", emailGroup);
				}
			}
		}
	}
}
