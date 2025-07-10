using System.Collections.Specialized;
using CargoWise.Types;
using Enterprise.Customs.Common.Shared;
using Enterprise.Customs.KR.Messaging;
using Enterprise.Messaging.MessageProcessors;
using WTG.StaticAnalysis.Annotation;
using static Enterprise.Customs.KR.Messaging.Constants;

namespace Enterprise.Customs.KR.Business
{
	[CodeAlive("An instance is created via reflection using the attribute MessageType")]
	[MessageType(ElectronicDocumentTypeList.Codes._5TV)]
	class GOVCBR5TVProcessor : IMessageProcessor
	{
		public void Process(EDIMessage message)
		{
			using (var textReader = message.GetEM_MessageTextReader())
			{
				var messageData = new GOVCBR5TVDataProvider().GetMessageData(message.Factory, textReader);
				var entry = MessageLinkedObjectManager.GetLinkedObject(message.Factory, message.Company, messageData.ImportDeclarationNumber, SharedJobMessageTypeList.Codes.Import);
				if (entry != null)
				{
					message.EM_LinkedObject = entry;

					var cusEntryNum = entry.EntryNumbers.GetOrCreateCusEntryNum(ElectronicDocumentTypeList.Codes._5TV);
					cusEntryNum.CE_EntryNum = messageData.NoticeNumber;
					cusEntryNum.CE_IssueDate = messageData.NoticeDate;
					cusEntryNum.CE_ExpiryDate = messageData.CorrectionDate;
				}
				message.EM_MessageInterpretation = CreateUserFriendlyMessageInterpretation(message, messageData);
				SendNotification(message, entry, messageData);
			}
		}

		#region SuppressResourceStringsCheckRegion
		void SendNotification(EDIMessage message, CusEntryHeader entry, IGOVCBR5TVMessageData messageData)
		{
			var declaration = entry?.Declaration;
			var notificationData = new DeclarationEntryNotificationData()
			{
				Entry = entry,
				Branch = declaration?.Branch ?? message.Branch,
				EmailGroup = KRCustomsRegistry.Instance.ImportEmailGroup,
				EmailBody = CreateUserFriendlyMessageInterpretation(message, messageData),
				OriginalMessageTypes = new ZString[] { ElectronicDocumentTypeList.Codes._5FE },
				MessageTypeDescription = $"[{ElectronicDocumentTypeList.Descriptions._5TV}]",
				EntryNumber = messageData.ImportDeclarationNumber
			};
			NotificationSender.SendNotification(notificationData);
		}

		string CreateUserFriendlyMessageInterpretation(EDIMessage message, IGOVCBR5TVMessageData messageData)
		{
			var result = new ZStringBuilder();

			var tableContents = new HtmlTableCreator(new string[] { "항 목", "내 용" });
			tableContents.WriteRow(new string[] { "제출문서", "수입(납세)신고 정정 신청서" });
			tableContents.WriteRow(new string[] { "통지번호", messageData.FormattedNoticeNumber });
			tableContents.WriteRow(new string[] { "담당 세관(과)", "[" + messageData.CustomsOfficeAndCustomsDivision + "] " + MessageFunctions.GetCustomsOfficeAndDivision(message.Factory, messageData.CustomsOfficeAndCustomsDivision) });
			tableContents.WriteRow(new string[] { "담당과장", messageData.CustomsManagerName });
			tableContents.WriteRow(new string[] { "담당자", messageData.CustomsPersonName });
			tableContents.WriteRow(new string[] { "담당자 연락처", messageData.CustomsPersonPhoneNumber });
			tableContents.WriteRow(new string[] { "수입신고번호", MessageFunctions.DeclarationNumberFormat(messageData.ImportDeclarationNumber) });
			tableContents.WriteRow(new string[] { "수입신고일자", messageData.ImportDeclarationDate.ToString(DateFormatType.DateKorean) });
			tableContents.WriteRow(new string[] { "보정란수", messageData.EntryLineCount.ToString() });
			tableContents.WriteRow(new string[] { "증감세액 총합", messageData.TotalCustomsDisbursementDifferenceAmount.ToString() });
			tableContents.WriteRow(new string[] { "보정사유", messageData.CorrectionReason });
			tableContents.WriteRow(new string[] { "보정기한일자", messageData.CorrectionDate.ToString(DateFormatType.DateKorean) });
			tableContents.WriteRow(new string[] { "통지일자", messageData.NoticeDate.ToString(DateFormatType.DateKorean) });
			tableContents.WriteRow(new string[] { "통지 세관", "[" + messageData.NoticeCustomsOffice + "] " + MessageFunctions.GetCustomsOffice(message.Factory, messageData.NoticeCustomsOffice) });
			tableContents.WriteRow(new string[] { "납세의무자 상호", messageData.PayerCompanyName });
			tableContents.WriteRow(new string[] { "납세의무자 성명", messageData.PayerRepresentativeName });
			tableContents.WriteRow(new string[] { "첨부서류", messageData.AttachedDocumentName });
			tableContents.WriteRow(new string[] { "신고인부호", messageData.DeclarantID });
			tableContents.WriteRowWithFormatting(new CellWithFormatting("보정품목 내역은 프로그램에서 확인 하십시오.", new NameValueCollection { { "align", "left" }, { "colspan", "2" } }, false));
			result.Append(tableContents.ToHtml());

			return result.ToString();
		}
		#endregion
	}
}
