using System.Collections.Specialized;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.KR.Messaging;
using Enterprise.Messaging.MessageProcessors;
using WTG.StaticAnalysis.Annotation;
using static Enterprise.Customs.KR.Messaging.Constants;

namespace Enterprise.Customs.KR.Business
{
	[CodeAlive("An instance is created via reflection using the attribute MessageType")]
	[MessageType(ElectronicDocumentTypeList.Codes._RE7)]
	class GOVCBRRE7Processor : IMessageProcessor
	{
		public void Process(EDIMessage message)
		{
			using (var textReader = message.GetEM_MessageTextReader())
			{
				var messageData = new GOVCBRRE7DataProvider().GetMessageData(textReader);
				var (entryNum5UL, entry, refundDeclaration, outgoingMessage) = MessageLinkedObjectManager.GetLinkedRefundObject(message.Factory, message.Company, messageData.ApplicationNumber, ElectronicDocumentTypeList.Codes._5UL);

				if (entry != null)
				{
					message.EM_LinkedObject = entry;
					var instruction = entry.EntryInstruction;
					if (instruction != null)
					{
						var amendmentSessionalData = instruction.AmendmentSessionalDataCollection.Cast<AmendmentSessionalData>().FirstOrDefault(x => x.RefundDeclarationNumber == messageData.ApplicationNumber);
						if (amendmentSessionalData != null)
						{
							amendmentSessionalData.RefundSessionalData.CSI_DateOfIssue = messageData.AcceptDateTime;
						}
					}
				}
				else if (refundDeclaration != null)
				{
					message.EM_LinkedObject = refundDeclaration;
					refundDeclaration.CRD_MessageStatus = CustomsMessageStatusTypeList.Codes.OriginalAccepted;
				}

				if (entryNum5UL != null)
				{
					entryNum5UL.CE_EntryStatus = CustomsMessageStatusTypeList.Codes.OriginalAccepted;
					entryNum5UL.CE_IssueDate = messageData.AcceptDateTime;
				}
				message.EM_ApplicationReference = outgoingMessage?.EM_MessageNum ?? ZString.Empty;
				message.EM_MessageInterpretation = CreateUserFriendlyMessageInterpretation(message, messageData);
				if (refundDeclaration != null)
				{
					SendNotificationReconDeclaration(message, refundDeclaration, messageData);
				}
				else
				{
					SendNotification(message, entry, messageData);
				}
			}
		}

		#region SuppressResourceStringsCheckRegion

		void SendNotification(EDIMessage message, CusEntryHeader entry, IGOVCBRRE7MessageData messageData)
		{
			var declaration = entry?.Declaration;
			var notificationData = new DeclarationEntryNotificationData()
			{
				Entry = entry,
				Branch = declaration?.Branch ?? message.Branch,
				EmailGroup = KRCustomsRegistry.Instance.ImportEmailGroup,
				EmailBody = CreateUserFriendlyMessageInterpretation(message, messageData),
				OriginalMessageTypes = new ZString[] { ElectronicDocumentTypeList.Codes._5UL },
				MessageTypeDescription = $"[{ElectronicDocumentTypeList.Descriptions._RE7}]",
				EntryNumber = messageData.ApplicationNumber
			};
			NotificationSender.SendNotification(notificationData);
		}

		void SendNotificationReconDeclaration(EDIMessage message, CusReconDeclaration reconDeclaration, IGOVCBRRE7MessageData messageData)
		{
			var notificationData = NotificationSender.GetReconDeclarationNotificationData(reconDeclaration, messageData.ApplicationNumber);
			notificationData.EmailBody = CreateUserFriendlyMessageInterpretation(message, messageData);
			notificationData.MessageTypeDescription = $"[{ElectronicDocumentTypeList.Descriptions._RE7}]";
			NotificationSender.SendNotification(notificationData);
		}

		string CreateUserFriendlyMessageInterpretation(EDIMessage message, IGOVCBRRE7MessageData messageData)
		{
			var tableContents = new HtmlTableCreator(System.Array.Empty<string>());
			tableContents.WriteRowWithFormatting(new CellWithFormatting("항 목", new NameValueCollection { { "width", "200" } }, true), new CellWithFormatting("내 용", new NameValueCollection { { "width", "350" } }, true));
			tableContents.WriteRow(new string[] { "제출문서", "과오납 및 계약상이 환급신청서" });
			tableContents.WriteRow(new string[] { "통보일시", messageData.NoticeDateTime.ToString(DateFormatType.DateTimeKorean) });
			tableContents.WriteRow(new string[] { "신청문서 제출번호", MessageFunctions.DeclarationNumberFormat(messageData.ApplicationNumber) });
			tableContents.WriteRow(new string[] { "신청문서 수신일시", messageData.AcceptDateTime.ToString(DateFormatType.DateTimeKorean) });
			tableContents.WriteRow(new string[] { "접수 세관(과)", messageData.DeclarationOffice.IsEmpty ? string.Empty : "[" + messageData.DeclarationOffice + "] " + MessageFunctions.GetCustomsOfficeAndDivision(message.Factory, messageData.DeclarationOffice) });
			tableContents.WriteRow(new string[] { "특이사항", messageData.ContentDescription });

			return tableContents.ToHtml();
		}
		#endregion
	}
}
