using CargoWise.Types;
using Enterprise.Customs.Common.Shared;
using Enterprise.Customs.KR.Messaging;
using Enterprise.Messaging.MessageProcessors;
using WTG.StaticAnalysis.Annotation;
using static Enterprise.Customs.KR.Messaging.Constants;

namespace Enterprise.Customs.KR.Business
{
	[CodeAlive("An instance is created via reflection using the attribute MessageType")]
	[MessageType(ElectronicDocumentTypeList.Codes._5WJ)]
	class GOVCBR5WJProcessor : IMessageProcessor
	{
		public void Process(EDIMessage message)
		{
			using (var textReader = message.GetEM_MessageTextReader())
			{
				var messageData = new GOVCBR5WJDataProvider().GetMessageData(textReader);
				var entry = MessageLinkedObjectManager.GetLinkedObject(message.Factory, message.Company, messageData.ImportDeclarationNumber, SharedJobMessageTypeList.Codes.Import);
				if (entry != null)
				{
					message.EM_LinkedObject = entry;
				}
				message.EM_MessageInterpretation = CreateUserFriendlyMessageInterpretation(messageData);
				SendNotification(message, entry, messageData);
			}
		}

		#region SuppressResourceStringsCheckRegion
		void SendNotification(EDIMessage message, CusEntryHeader entry, IGOVCBR5WJMessageData messageData)
		{
			var declaration = entry?.Declaration;
			var notificationData = new DeclarationEntryNotificationData()
			{
				Entry = entry,
				Branch = declaration?.Branch ?? message.Branch,
				EmailGroup = KRCustomsRegistry.Instance.ImportEmailGroup,
				EmailBody = CreateUserFriendlyMessageInterpretation(messageData),
				OriginalMessageTypes = new ZString[] { ElectronicDocumentTypeList.Codes._929 },
				MessageTypeDescription = $"[{ElectronicDocumentTypeList.Descriptions._5WJ}]",
				EntryNumber = messageData.ImportDeclarationNumber
			};
			NotificationSender.SendNotification(notificationData);
		}

		string CreateUserFriendlyMessageInterpretation(IGOVCBR5WJMessageData messageData)
		{
			var result = new ZStringBuilder();

			var tableContents = new HtmlTableCreator(new string[] { "항 목", "내 용" });
			tableContents.WriteRow(new string[] { "제출문서", "수입신고서" });
			tableContents.WriteRow(new string[] { "안내일자", messageData.NoticeDate.ToString(DateFormatType.DateKorean) });
			tableContents.WriteRow(new string[] { "신고품명", messageData.HSDescription });
			tableContents.WriteRow(new string[] { "거래품명", messageData.TradeName });
			tableContents.WriteRow(new string[] { "상표명", messageData.BrandName });
			tableContents.WriteRow(new string[] { "모델규격", messageData.ModelName });
			tableContents.WriteRow(new string[] { "분류의견", messageData.ClassificationReason });
			tableContents.WriteRow(new string[] { "참고사항1", messageData.ContentDescription1 });
			tableContents.WriteRow(new string[] { "참고사항2", messageData.ContentDescription2 });
			tableContents.WriteRow(new string[] { "신고인 상호", messageData.DeclarantCompanyName });
			tableContents.WriteRow(new string[] { "화주 상호", messageData.ImporterCompanyName });
			tableContents.WriteRow(new string[] { "분석회보 문서번호", MessageFunctions.AnalysisNumberFormat(messageData.AnalysisNumber) });
			tableContents.WriteRow(new string[] { "수입신고번호", MessageFunctions.DeclarationNumberFormat(messageData.ImportDeclarationNumber) });
			tableContents.WriteRow(new string[] { "란번호", messageData.EntryLineNo.ToString() });
			tableContents.WriteRow(new string[] { "규격번호", messageData.InvoiceLineNo.ToString() });
			tableContents.WriteRow(new string[] { "신고일자", messageData.DeclarationDate.ToString(DateFormatType.DateKorean) });
			tableContents.WriteRow(new string[] { "결정세번", messageData.DecisionHSCode });
			tableContents.WriteRow(new string[] { "신고세번", messageData.HSCode });
			result.Append(tableContents.ToHtml());

			return result.ToString();
		}
		#endregion
	}
}
