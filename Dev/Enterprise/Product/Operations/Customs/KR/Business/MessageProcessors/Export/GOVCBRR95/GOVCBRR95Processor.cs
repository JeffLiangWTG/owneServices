using CargoWise.Types;
using Enterprise.Customs.Common.Shared;
using Enterprise.Customs.KR.Messaging;
using Enterprise.Messaging.MessageProcessors;
using WTG.StaticAnalysis.Annotation;
using static Enterprise.Customs.KR.Messaging.Constants;

namespace Enterprise.Customs.KR.Business
{
	[CodeAlive("An instance is created via reflection using the attribute MessageType")]
	[MessageType(ElectronicDocumentTypeList.Codes._R95)]
	public class GOVCBRR95Processor : IMessageProcessor
	{
		public void Process(EDIMessage message)
		{
			using (var textReader = message.GetEM_MessageTextReader())
			{
				var messageData = new GOVCBRR95DataProvider().GetMessageData(textReader);
				var entry = MessageLinkedObjectManager.GetLinkedObject(message.Factory, message.Company, messageData.ExportDeclarationNumber, SharedJobMessageTypeList.Codes.Export);
				if (entry != null)
				{
					message.EM_LinkedObject = entry;
					entry.CH_EntryStatus = CustomsEntryStatusTypeList.Codes.CLR;
				}
				message.EM_MessageInterpretation = CreateUserFriendlyMessageInterpretation(messageData, message);
				SendNotification(message, entry, messageData);
			}
		}

		#region SuppressResourceStringsCheckRegion

		void SendNotification(EDIMessage message, CusEntryHeader entry, IGOVCBRR95MessageData messageData)
		{
			var declaration = entry?.Declaration;
			var notificationData = new DeclarationEntryNotificationData()
			{
				Entry = entry,
				Branch = declaration?.Branch ?? message.Branch,
				EmailGroup = KRCustomsRegistry.Instance.ExportEmailGroup,
				EmailBody = CreateUserFriendlyMessageInterpretation(messageData, message),
				OriginalMessageTypes = new ZString[] { ElectronicDocumentTypeList.Codes._830 },
				MessageTypeDescription = $"[{ElectronicDocumentTypeList.Descriptions._R95}]",
				EntryNumber = messageData.ExportDeclarationNumber
			};
			NotificationSender.SendNotification(notificationData);
		}
		string CreateUserFriendlyMessageInterpretation(IGOVCBRR95MessageData messageData, EDIMessage message)
		{
			var result = new ZStringBuilder();
			var customsofficeAnddivision = MessageFunctions.GetCustomsOfficeAndDivision(message.Factory, messageData.CustomsOfficeAndDivision);

			var tableContents = new HtmlTableCreator(new string[] { "항 목", "내 용" });
			tableContents.WriteRow(new string[] { "제출문서", "수출신고서" });
			tableContents.WriteRow(new string[] { "검사일자", messageData.InspectionDate.ToString(DateFormatType.DateKorean) });
			tableContents.WriteRow(new string[] { "수출신고번호", MessageFunctions.DeclarationNumberFormat(messageData.ExportDeclarationNumber) });
			tableContents.WriteRow(new string[] { "검사차수", messageData.InspectionSequenceNo.ToString() });
			tableContents.WriteRow(new string[] { "신고인 상호", messageData.DeclarantCompanyName });
			tableContents.WriteRow(new string[] { "검사 세관(과)", "[" + messageData.CustomsOfficeAndDivision + "] " + customsofficeAnddivision });
			tableContents.WriteRow(new string[] { "검사 담당자명", messageData.InspectionPersonName });
			tableContents.WriteRow(new string[] { "세관 담당자 전화번호", messageData.CustomsPersonPhoneNumber });
			result.Append(tableContents.ToHtml());

			return result.ToString();
		}
		#endregion
	}
}
