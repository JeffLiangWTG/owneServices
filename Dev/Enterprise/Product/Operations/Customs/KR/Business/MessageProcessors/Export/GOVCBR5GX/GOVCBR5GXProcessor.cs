using CargoWise.Types;
using Enterprise.Customs.Common.Shared;
using Enterprise.Customs.KR.Messaging;
using Enterprise.Messaging.MessageProcessors;
using WTG.StaticAnalysis.Annotation;
using static Enterprise.Customs.KR.Messaging.Constants;

namespace Enterprise.Customs.KR.Business
{
	[CodeAlive("An instance is created via reflection using the attribute MessageType")]
	[MessageType(ElectronicDocumentTypeList.Codes._5GX)]
	public class GOVCBR5GXProcessor : IMessageProcessor
	{
		public void Process(EDIMessage message)
		{
			using (var textReader = message.GetEM_MessageTextReader())
			{
				var messageData = new GOVCBR5GXDataProvider().GetMessageData(textReader);
				var entry = MessageLinkedObjectManager.GetLinkedObject(message.Factory, message.Company, messageData.ExportDeclarationNumber, SharedJobMessageTypeList.Codes.Export);
				if (entry != null)
				{
					message.EM_LinkedObject = entry;
					entry.CH_EntryStatus = CustomsEntryStatusTypeList.Codes.MFI;
				}
				message.EM_MessageInterpretation = CreateUserFriendlyMessageInterpretation(messageData);
				SendNotification(message, entry, messageData);
			}
		}

		#region SuppressResourceStringsCheckRegion

		void SendNotification(EDIMessage message, CusEntryHeader entry, IGOVCBR5GXMessageData messageData)
		{
			var declaration = entry?.Declaration;
			var notificationData = new DeclarationEntryNotificationData()
			{
				Entry = entry,
				Branch = declaration?.Branch ?? message.Branch,
				EmailGroup = KRCustomsRegistry.Instance.ExportEmailGroup,
				EmailBody = CreateUserFriendlyMessageInterpretation(messageData),
				OriginalMessageTypes = new ZString[] { ElectronicDocumentTypeList.Codes._830 },
				MessageTypeDescription = $"[{ElectronicDocumentTypeList.Descriptions._5GX}]",
				EntryNumber = messageData.ExportDeclarationNumber
			};
			NotificationSender.SendNotification(notificationData);
		}
		string CreateUserFriendlyMessageInterpretation(IGOVCBR5GXMessageData messageData)
		{
			var result = new ZStringBuilder();
			var tableContents = new HtmlTableCreator(new string[] { "항 목", "내 용" });
			tableContents.WriteRow(new string[] { "제출문서", "수출신고서" });
			tableContents.WriteRow(new string[] { "보완 통보일자", messageData.NoticeDate.ToString(DateFormatType.DateKorean) });
			tableContents.WriteRow(new string[] { "수출신고번호", MessageFunctions.DeclarationNumberFormat(messageData.ExportDeclarationNumber) });
			tableContents.WriteRow(new string[] { "보완 완료일자", messageData.ComplementDueDate.ToString(DateFormatType.DateKorean) });
			tableContents.WriteRow(new string[] { "보완 요구사유", messageData.ComplementReasonCode + " " + messageData.ComplementReasonName });
			tableContents.WriteRow(new string[] { "보완 요구사항", messageData.ComplementDescription });
			tableContents.WriteRow(new string[] { "보완 통보세관", messageData.CustomsOffice + " " + messageData.CustomsOfficeName });
			tableContents.WriteRow(new string[] { "신고자", messageData.DeclarantID + " " + messageData.DeclarantName });
			tableContents.WriteRow(new string[] { "보완통보 문서번호", messageData.ComplementNumber.SubstringSafe(0, 2) + "-" +
																		messageData.ComplementNumber.SubstringSafe(2) });
			result.Append(tableContents.ToHtml());

			return result.ToString();
		}
		#endregion
	}
}
