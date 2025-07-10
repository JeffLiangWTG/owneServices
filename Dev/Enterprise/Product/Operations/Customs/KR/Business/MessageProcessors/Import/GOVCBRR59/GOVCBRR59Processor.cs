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
	[MessageType(ElectronicDocumentTypeList.Codes._R59)]
	class GOVCBRR59Processor : IMessageProcessor
	{
		public void Process(EDIMessage message)
		{
			using (var textReader = message.GetEM_MessageTextReader())
			{
				var messageData = new GOVCBRR59DataProvider().GetMessageData(textReader);
				var entry = MessageLinkedObjectManager.GetLinkedObject(message.Factory, message.Company, messageData.ImportDeclarationNumber, SharedJobMessageTypeList.Codes.Import);
				if (entry != null)
				{
					message.EM_LinkedObject = entry;
				}
				var messageInterpretation = CreateUserFriendlyMessageInterpretation(message, messageData);
				message.EM_MessageInterpretation = messageInterpretation;
				SendNotification(message, entry, messageData, messageInterpretation);
			}
		}

		#region SuppressResourceStringsCheckRegion
		void SendNotification(EDIMessage message, CusEntryHeader entry, IGOVCBRR59MessageData messageData, string messageInterpretation)
		{
			var declaration = entry?.Declaration;
			var notificationData = new DeclarationEntryNotificationData()
			{
				Entry = entry,
				Branch = declaration?.Branch ?? message.Branch,
				EmailGroup = KRCustomsRegistry.Instance.ImportEmailGroup,
				EmailBody = messageInterpretation,
				OriginalMessageTypes = new ZString[] { ElectronicDocumentTypeList.Codes._929 },
				MessageTypeDescription = $"[{ElectronicDocumentTypeList.Descriptions._R59}]",
				EntryNumber = messageData.ImportDeclarationNumber
			};
			NotificationSender.SendNotification(notificationData);
		}

		string CreateUserFriendlyMessageInterpretation(EDIMessage message, IGOVCBRR59MessageData messageData)
		{
			var result = new ZStringBuilder();

			var tableContents = new HtmlTableCreator(new string[] { "항 목", "내 용" });
			tableContents.WriteRow(new string[] { "제출문서", "수입신고서" });
			tableContents.WriteRow(new string[] { "수입신고번호", MessageFunctions.DeclarationNumberFormat(messageData.ImportDeclarationNumber) });
			tableContents.WriteRow(new string[] { "신청일자", messageData.DeclarationDate.ToString(DateFormatType.DateKorean) });
			tableContents.WriteRow(new string[] { "처리일시", messageData.NoticeDateTime.ToString(DateFormatType.DateTimeKorean) });
			tableContents.WriteRow(new string[] { "처리결과", new ResultTypeList().GetDescriptionFromCode(messageData.ResultType) });
			tableContents.WriteRow(new string[] { "처리담당자명", messageData.CustomsManagerName });
			tableContents.WriteRow(new string[] { "처리세관", messageData.CustomsOffice.IsEmpty ? string.Empty : "[" + messageData.CustomsOffice + "] " + MessageFunctions.GetCustomsOfficeAndDivision(message.Factory, messageData.CustomsOffice) });
			tableContents.WriteRowWithFormatting(new CellWithFormatting("[처리내역]", new NameValueCollection { { "align", "left" }, { "colspan", "2" } }));
			tableContents.WriteRow(new string[] { "란번호", messageData.EntryLineNo.ToString() });
			tableContents.WriteRow(new string[] { "HS 부호", MessageFunctions.HSCodeFormat(messageData.HSCode) });
			tableContents.WriteRow(new string[] { "이행 수량", string.Format("{0:#,##0}", messageData.Quantity) + " " + messageData.QuantityUnit });
			tableContents.WriteRow(new string[] { "이행 순중량", string.Format("{0:#,##0.000}", messageData.NetWeightInKG) + " " + Core.Constants.Weight.Kilograms });
			result.Append(tableContents.ToHtml());

			return result.ToString();
		}
		#endregion
	}
}
