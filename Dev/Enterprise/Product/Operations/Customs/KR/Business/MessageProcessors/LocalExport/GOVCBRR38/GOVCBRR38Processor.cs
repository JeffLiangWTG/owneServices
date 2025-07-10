using CargoWise.Types;
using Enterprise.Customs.KR.Messaging;
using Enterprise.Messaging.MessageProcessors;
using WTG.StaticAnalysis.Annotation;
using static Enterprise.Customs.KR.Messaging.Constants;

namespace Enterprise.Customs.KR.Business
{
	[CodeAlive("An instance is created via reflection using the attribute MessageType")]
	[MessageType(ElectronicDocumentTypeList.Codes._R38)]
	public class GOVCBRR38Processor : IMessageProcessor
	{
		public void Process(EDIMessage message)
		{
			using (var textReader = message.GetEM_MessageTextReader())
			{
				var messageData = new GOVCBRR38DataProvider().GetMessageData(textReader);
				var typeCode = messageData.DeclarationType.Substring(6);
				var supporter = new MultiPurposeResponseSupporterProvider().GetSupporterForR38(typeCode);
				if (supporter != null)
				{
					var entryNumType = supporter.EntryType;
					var entry = MessageLinkedObjectManager.GetLinkedObject(message.Factory, message.Company, messageData.DeclarationNumber, entryNumType);
					if (entry != null)
					{
						message.EM_LinkedObject = entry;
						supporter.UpdateParent(entry, typeCode, messageData);

						var outgoingMessage = entry.Messages.GetLastMessage(EDIMessage.ApplicationCodes.KRCustoms, typeCode);
						message.EM_ApplicationReference = outgoingMessage?.EM_MessageNum ?? ZString.Empty;
						message.EM_MessageSubType = typeCode;

						entry.UpdateEntryVersionID((EDIMessage)outgoingMessage);
					}
					message.EM_MessageInterpretation = CreateUserFriendlyMessageInterpretation(message, messageData, typeCode);
					SendNotification(message, entry, messageData, typeCode);
				}
			}
		}

		#region SuppressResourceStringsCheckRegion

		void SendNotification(EDIMessage message, CusEntryHeader entry, IGOVCBRR38MessageData messageData, ZString typeCode)
		{
			var declaration = entry?.Declaration;
			var notificationData = new DeclarationEntryNotificationData()
			{
				Entry = entry,
				Branch = declaration?.Branch ?? message.Branch,
				EmailGroup = KRCustomsRegistry.Instance.LocalExportEmailGroup,
				EmailBody = CreateUserFriendlyMessageInterpretation(message, messageData, typeCode),
				OriginalMessageTypes = new ZString[] { typeCode },
				MessageTypeDescription = $"[{ElectronicDocumentTypeList.Descriptions._R38}]",
				EntryNumber = messageData.DeclarationNumber,
			};
			NotificationSender.SendNotification(notificationData);
		}

		string CreateUserFriendlyMessageInterpretation(EDIMessage message, IGOVCBRR38MessageData messageData, ZString typeCode)
		{
			var electronicDocumentTypeList = new ElectronicDocumentTypeList();
			var tableContents = new HtmlTableCreator(new string[] { "항 목", "내 용" });
			tableContents.WriteRow(new string[] { "제출문서", electronicDocumentTypeList.GetDescriptionFromCode(typeCode) });
			tableContents.WriteRow(new string[] { "접수통보일시", messageData.NoticeDateTime.ToString(DateFormatType.DateTimeKorean) });
			tableContents.WriteRow(new string[] { "접수 세관/과", "[" + messageData.CustomsOfficeAndDivision + "] " + MessageFunctions.GetCustomsOfficeAndDivision(message.Factory, messageData.CustomsOfficeAndDivision) });
			tableContents.WriteRow(new string[] { "신청서 문서번호", MessageFunctions.DeclarationNumberFormat(messageData.DeclarationNumber) });
			tableContents.WriteRow(new string[] { "참조번호", messageData.ConfirmNumber });
			tableContents.WriteRow(new string[] { "접수내역", messageData.ContentDescription });

			return tableContents.ToHtml();
		}
		#endregion
	}
}
