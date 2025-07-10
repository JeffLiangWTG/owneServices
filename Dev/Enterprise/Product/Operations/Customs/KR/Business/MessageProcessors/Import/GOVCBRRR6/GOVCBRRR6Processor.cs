using CargoWise.Types;
using Enterprise.Customs.KR.Messaging;
using Enterprise.Messaging.MessageProcessors;
using WTG.StaticAnalysis.Annotation;
using static Enterprise.Customs.KR.Messaging.Constants;

namespace Enterprise.Customs.KR.Business
{
	[CodeAlive("An instance is created via reflection using the attribute MessageType")]
	[MessageType(ElectronicDocumentTypeList.Codes._RR6)]
	class GOVCBRRR6Processor : IMessageProcessor
	{
		public void Process(EDIMessage message)
		{
			using (var textReader = message.GetEM_MessageTextReader())
			{
				var messageData = new GOVCBRRR6DataProvider().GetMessageData(textReader);

				var entry = MessageLinkedObjectManager.GetLinkedJobDeclarationD87(message.Factory, message.Company.PK, messageData.CarnetCertificateNumber);
				if (entry != null)
				{
					var outgoingMessage = entry.Messages.GetLastMessage(EDIMessage.ApplicationCodes.KRCustoms, ElectronicDocumentTypeList.Codes._D87);
					message.EM_ApplicationReference = outgoingMessage?.EM_MessageNum ?? ZString.Empty;

					message.EM_LinkedObject = entry;
					entry.CH_Status = CustomsMessageStatusTypeList.Codes.OriginalAccepted;
				}
				message.EM_MessageInterpretation = CreateUserFriendlyMessageInterpretation(message, messageData);
				message.EM_MessageSubType = ElectronicDocumentTypeList.Codes._D87;
				SendNotification(message, entry, messageData);
			}
		}

		#region SuppressResourceStringsCheckRegion
		void SendNotification(EDIMessage message, CusEntryHeader entry, IGOVCBRRR6MessageData messageData)
		{
			var declaration = entry?.Declaration;
			var notificationData = new DeclarationEntryNotificationData()
			{
				Entry = entry,
				Branch = declaration?.Branch ?? message.Branch,
				EmailGroup = KRCustomsRegistry.Instance.ImportEmailGroup,
				EmailBody = CreateUserFriendlyMessageInterpretation(message, messageData),
				OriginalMessageTypes = new ZString[] { ElectronicDocumentTypeList.Codes._D87 },
				MessageTypeDescription = $"[{ElectronicDocumentTypeList.Descriptions._RR6}]",
				EntryNumber = messageData.CarnetCertificateNumber
			};
			NotificationSender.SendNotification(notificationData);
		}

		string CreateUserFriendlyMessageInterpretation(EDIMessage message, IGOVCBRRR6MessageData messageData)
		{
			var result = new ZStringBuilder();

			var tableContents = new HtmlTableCreator(new string[] { "항 목", "내 용" });
			tableContents.WriteRow(new string[] { "제출문서", "까르네 일시수입증서" });
			tableContents.WriteRow(new string[] { "통보일시", messageData.NoticeDateTime.ToString(DateFormatType.DateTimeKorean) });
			tableContents.WriteRow(new string[] { "까르네 증서번호", messageData.CarnetCertificateNumber });
			tableContents.WriteRow(new string[] { "접수 세관(과)", messageData.CustomsOfficeAndDivision.IsEmpty ? string.Empty
													: "[" + messageData.CustomsOfficeAndDivision + "] " + MessageFunctions.GetCustomsOfficeAndDivision(message.Factory, messageData.CustomsOfficeAndDivision) });
			result.Append(tableContents.ToHtml());

			return result.ToString();
		}
		#endregion
	}
}
