using CargoWise.Types;
using Enterprise.Customs.KR.Messaging;
using Enterprise.Messaging.MessageProcessors;
using WTG.StaticAnalysis.Annotation;
using static Enterprise.Customs.KR.Messaging.Constants;

namespace Enterprise.Customs.KR.Business
{
	[CodeAlive("An instance is created via reflection using the attribute MessageType")]
	[MessageType(ElectronicDocumentTypeList.Codes._R76)]
	class GOVCBRR76Processor : IMessageProcessor
	{
		public void Process(EDIMessage message)
		{
			using (var textReader = message.GetEM_MessageTextReader())
			{
				var messageData = new GOVCBRR76DataProvider().GetMessageData(textReader);
				var requestHeader = MessageLinkedObjectManager.GetLinkedMiscRequestHeader(message.Factory, message.Company, messageData.ApplicationNumber, messageData.DeclarationType);
				if (requestHeader != null)
				{
					var outgoingMessage = requestHeader.Messages.GetLastMessage(EDIMessage.ApplicationCodes.KRCustoms, messageData.DeclarationType);
					message.EM_ApplicationReference = outgoingMessage?.EM_MessageNum ?? ZString.Empty;
					message.EM_LinkedObject = requestHeader;

					requestHeader.CMR_Status = CustomsMessageStatusTypeList.Codes.OriginalAccepted;
				}
				message.EM_MessageInterpretation = CreateUserFriendlyMessageInterpretation(messageData, message);
				SendNotification(message, requestHeader, messageData);
			}
		}

		#region SuppressResourceStringsCheckRegion

		void SendNotification(EDIMessage message, CusMiscRequestHeader requestHeader, IGOVCBRR76MessageData messageData)
		{
			var notificationData = new NotificationData()
			{
				ControllerIDProvider = requestHeader,
				MessagesParent = requestHeader,
				JobNumberDescription = $"Declaration Number: {requestHeader?.CMR_JobNumber ?? ZString.Empty} / 제출번호: {messageData.ApplicationNumber}",
				Branch = requestHeader?.Branch ?? message.Branch,
				EmailGroup = messageData.DeclarationType == ElectronicDocumentTypeList.Codes._5GW ? KRCustomsRegistry.Instance.ImportEmailGroup : KRCustomsRegistry.Instance.ExportEmailGroup,
				EmailBody = CreateUserFriendlyMessageInterpretation(messageData, message),
				OriginalMessageTypes = new ZString[] { messageData.DeclarationType },
				MessageTypeDescription = $"[{ElectronicDocumentTypeList.Descriptions._R76}]",
				EntryNumber = messageData.ApplicationNumber,
				AlternativeRecipientStaff = requestHeader?.Broker
			};
			NotificationSender.SendNotification(notificationData);
		}

		string CreateUserFriendlyMessageInterpretation(IGOVCBRR76MessageData messageData, EDIMessage message)
		{
			var tableContents = new HtmlTableCreator(new string[] { "항 목", "내 용" });
			tableContents.WriteRow(new string[] { "제출문서", new ElectronicDocumentTypeList().GetDescriptionFromCode(messageData.DeclarationType) });
			tableContents.WriteRow(new string[] { "통보일시", messageData.NoticeDateTime.ToString(DateFormatType.DateTimeKorean) });
			tableContents.WriteRow(new string[] { "신청문서 제출번호", messageData.ApplicationNumber });
			tableContents.WriteRow(new string[] { "신청문서 수신일시", messageData.AcceptDateTime.ToString(DateFormatType.DateTimeKorean) });
			tableContents.WriteRow(new string[] { "통보세관(과)", messageData.CustomsOfficeAndDivision.IsEmpty ? string.Empty : "[" + messageData.CustomsOfficeAndDivision + "] " + MessageFunctions.GetCustomsOfficeAndDivision(message.Factory, messageData.CustomsOfficeAndDivision) });

			return tableContents.ToHtml();
		}
		#endregion
	}
}
