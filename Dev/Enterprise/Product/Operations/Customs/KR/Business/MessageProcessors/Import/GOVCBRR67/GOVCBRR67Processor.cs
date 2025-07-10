using System.Collections.Specialized;
using CargoWise.Types;
using Enterprise.Customs.KR.Messaging;
using Enterprise.Messaging.MessageProcessors;
using WTG.StaticAnalysis.Annotation;
using static Enterprise.Customs.KR.Messaging.Constants;

namespace Enterprise.Customs.KR.Business
{
	[CodeAlive("An instance is created via reflection using the attribute MessageType")]
	[MessageType(ElectronicDocumentTypeList.Codes._R67)]
	class GOVCBRR67Processor : IMessageProcessor
	{
		public void Process(EDIMessage message)
		{
			using (var textReader = message.GetEM_MessageTextReader())
			{
				var messageData = new GOVCBRR67DataProvider().GetMessageData(textReader);
				var entry = MessageLinkedObjectManager.GetLinkedMiscRequestHeader(message.Factory, message.Company, messageData.ApplicationNumber, ElectronicDocumentTypeList.Codes._5SG);
				if (entry != null)
				{
					message.EM_LinkedObject = entry;

					entry.CMR_Status = CustomsMessageStatusTypeList.Codes.OriginalAccepted;
					var outgoingMessage = (EDIMessage)entry.Messages.GetLastMessage(EDIMessage.ApplicationCodes.KRCustoms, ElectronicDocumentTypeList.Codes._5SG);
					message.EM_ApplicationReference = outgoingMessage?.EM_MessageNum ?? ZString.Empty;
				}
				message.EM_MessageInterpretation = CreateUserFriendlyMessageInterpretation(messageData);
				SendNotification(message, entry, messageData);
			}
		}

		#region SuppressResourceStringsCheckRegion

		void SendNotification(EDIMessage message, CusMiscRequestHeader requestHeader, IGOVCBRR67MessageData messageData)
		{
			var notificationData = new NotificationData()
			{
				ControllerIDProvider = requestHeader,
				MessagesParent = requestHeader,
				JobNumberDescription = $"Declaration Number: {requestHeader?.CMR_JobNumber ?? ZString.Empty} / 제출번호: {messageData.ApplicationNumber}",
				Branch = requestHeader?.Branch ?? message.Branch,
				EmailGroup = KRCustomsRegistry.Instance.ImportEmailGroup,
				EmailBody = CreateUserFriendlyMessageInterpretation(messageData),
				OriginalMessageTypes = new ZString[] { ElectronicDocumentTypeList.Codes._5SG },
				MessageTypeDescription = $"[{ElectronicDocumentTypeList.Descriptions._R67}]",
				EntryNumber = messageData.ApplicationNumber,
				AlternativeRecipientStaff = requestHeader?.Broker
			};
			NotificationSender.SendNotification(notificationData);
		}

		string CreateUserFriendlyMessageInterpretation(IGOVCBRR67MessageData messageData)
		{
			var tableContents = new HtmlTableCreator(System.Array.Empty<string>());
			tableContents.WriteRowWithFormatting(new CellWithFormatting("항 목", new NameValueCollection { { "width", "200" } }, true), new CellWithFormatting("내 용", new NameValueCollection { { "width", "300" } }, true));
			tableContents.WriteRow(new string[] { "제출문서", "확정가격신고 기간연장신청" });
			tableContents.WriteRow(new string[] { "통보일시	", messageData.NoticeDateTime.ToString(DateFormatType.DateTimeKorean) });
			if (messageData.ApplicationNumber.Length >= 19)
			{
				tableContents.WriteRow(new string[] { "신청문서 제출번호", messageData.ApplicationNumber.Substring(0,3) + "-" +
																		messageData.ApplicationNumber.Substring(3,5) + "-" +
																		messageData.ApplicationNumber.Substring(8,4) + "-" +
																		messageData.ApplicationNumber.Substring(12,7) });
			}
			else
			{
				tableContents.WriteRow(new string[] { "신청문서 제출번호", messageData.ApplicationNumber });
			}
			tableContents.WriteRow(new string[] { "신청문서 제출차수", messageData.SequenceNo.ToString() });
			tableContents.WriteRow(new string[] { "신청문서 수신일시", messageData.AcceptDateTime.ToString(DateFormatType.DateTimeKorean) });
			tableContents.WriteRow(new string[] { "접수내역", messageData.NoticeDescription });

			return tableContents.ToHtml();
		}
		#endregion
	}
}
