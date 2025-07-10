using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.Common.Shared;
using Enterprise.Customs.KR.Messaging;
using Enterprise.Messaging.MessageProcessors;
using WTG.StaticAnalysis.Annotation;
using static Enterprise.Customs.KR.Messaging.Constants;

namespace Enterprise.Customs.KR.Business
{
	[CodeAlive("An instance is created via reflection using the attribute MessageType")]
	[MessageType(ElectronicDocumentTypeList.Codes._5AF)]
	public class GOVCBR5AFProcessor : IMessageProcessor
	{
		public void Process(EDIMessage message)
		{
			using (var textReader = message.GetEM_MessageTextReader())
			{
				var messageData = new GOVCBR5AFDataProvider().GetMessageData(textReader);
				var entry = MessageLinkedObjectManager.GetLinkedObject(message.Factory, message.Company, messageData.ExportDeclarationNumber, SharedJobMessageTypeList.Codes.Export);
				if (entry != null)
				{
					message.EM_LinkedObject = entry;

					var originalMessage = entry.Messages.GetLastMessage(EDIMessage.ApplicationCodes.KRCustoms, messageData.DeclarationType);
					message.EM_ApplicationReference = originalMessage?.EM_MessageNum ?? ZString.Empty;

					if (messageData.DeclarationType == ElectronicDocumentTypeList.Codes._830)
					{
						entry.CH_Status = CustomsMessageStatusTypeList.Codes.OriginalAccepted;
						entry.EntryNumbers.UpdateCusEntryNumIfExists(SharedJobMessageTypeList.Codes.Export, CusEntryNumber.Schema.CE_IssueDate, messageData.AcceptDateTime);
					}
					else if (messageData.DeclarationType == ElectronicDocumentTypeList.Codes._5AS)
					{
						entry.CH_Status = CustomsMessageStatusTypeList.Codes.AmendmentAccepted;
					}
					else if (messageData.DeclarationType == ElectronicDocumentTypeList.Codes._DKJ)
					{
						entry.CH_Status = CustomsMessageStatusTypeList.Codes.CancellationAccepted;
					}
					entry.UpdateEntryVersionID((EDIMessage)originalMessage);
				}
				message.EM_MessageInterpretation = CreateUserFriendlyMessageInterpretation(messageData);
				SendNotification(message, entry, messageData);
			}
		}

		#region SuppressResourceStringsCheckRegion

		void SendNotification(EDIMessage message, CusEntryHeader entry, IGOVCBR5AFMessageData messageData)
		{
			var declaration = entry?.Declaration;
			var notificationData = new DeclarationEntryNotificationData()
			{
				Entry = entry,
				Branch = declaration?.Branch ?? message.Branch,
				EmailGroup = KRCustomsRegistry.Instance.ExportEmailGroup,
				EmailBody = CreateUserFriendlyMessageInterpretation(messageData),
				OriginalMessageTypes = new ZString[] { messageData.DeclarationType },
				MessageTypeDescription = $"[{ElectronicDocumentTypeList.Descriptions._5AF}]",
				EntryNumber = messageData.ExportDeclarationNumber
			};
			NotificationSender.SendNotification(notificationData);
		}
		string CreateUserFriendlyMessageInterpretation(IGOVCBR5AFMessageData messageData)
		{
			var result = new ZStringBuilder();
			var tableContents = new HtmlTableCreator(new string[] { "항 목", "내 용" });
			tableContents.WriteRow(new string[] { "제출문서", new ElectronicDocumentTypeList().GetDescriptionFromCode(messageData.DeclarationType) });
			tableContents.WriteRow(new string[] { "접수 통보일시", messageData.NoticeDateTime.ToString(DateFormatType.DateTimeKorean) });
			tableContents.WriteRow(new string[] { "접수 신고번호", MessageFunctions.DeclarationNumberFormat(messageData.ExportDeclarationNumber) });
			tableContents.WriteRow(new string[] { "접수일시", messageData.AcceptDateTime.ToString(DateFormatType.DateTimeKorean) });
			tableContents.WriteRow(new string[] { "세관 담당자", messageData.CustomsPersonID + " " + messageData.CustomsPersonName });
			tableContents.WriteRow(new string[] { "심사 담당자", messageData.CustomsManagerID + " " + messageData.CustomsManagerName });
			tableContents.WriteRow(new string[] { "정정 접수 결과내역", messageData.AmendAcceptResult });
			result.Append(tableContents.ToHtml());

			return result.ToString();
		}
		#endregion
	}
}
