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
	[MessageType(ElectronicDocumentTypeList.Codes._5GZ)]
	class GOVCBR5GZProcessor : IMessageProcessor
	{
		public void Process(EDIMessage message)
		{
			using (var textReader = message.GetEM_MessageTextReader())
			{
				var messageData = new GOVCBR5GZDataProvider().GetMessageData(textReader);
				var entry = MessageLinkedObjectManager.GetLinkedObject(message.Factory, message.Company, messageData.ImportDeclarationNumber, SharedJobMessageTypeList.Codes.Import);
				if (entry != null)
				{
					message.EM_LinkedObject = entry;

					entry.CH_Status = CustomsEntryStatusTypeList.Codes.NDM;
				}
				message.EM_MessageInterpretation = CreateUserFriendlyMessageInterpretation(messageData, MessageFunctions.MessageInterpretationMode.FullView);
				SendNotification(message, entry, messageData);
			}
		}

		#region SuppressResourceStringsCheckRegion

		void SendNotification(EDIMessage message, CusEntryHeader entry, IGOVCBR5GZMessageData messageData)
		{
			var declaration = entry?.Declaration;
			var notificationData = new DeclarationEntryNotificationData()
			{
				Entry = entry,
				Branch = declaration?.Branch ?? message.Branch,
				EmailGroup = KRCustomsRegistry.Instance.ImportEmailGroup,
				EmailBody = CreateUserFriendlyMessageInterpretation(messageData, MessageFunctions.MessageInterpretationMode.Email),
				OriginalMessageTypes = new ZString[] { ElectronicDocumentTypeList.Codes._929 },
				MessageTypeDescription = $"[{ElectronicDocumentTypeList.Descriptions._5GZ}]",
				EntryNumber = messageData.ImportDeclarationNumber
			};
			NotificationSender.SendNotification(notificationData);
		}

		string CreateUserFriendlyMessageInterpretation(IGOVCBR5GZMessageData messageData, MessageFunctions.MessageInterpretationMode mode)
		{
			var result = new ZStringBuilder();
			var tableContents = new HtmlTableCreator(System.Array.Empty<string>());
			tableContents.WriteRowWithFormatting(new CellWithFormatting("항 목", new NameValueCollection { { "width", "200" } }, true), new CellWithFormatting("내 용", new NameValueCollection { { "width", "400" } }, true));
			tableContents.WriteRow(new string[] { "제출문서", "수입신고서" });
			tableContents.WriteRow(new string[] { "처리결과 통보일시", messageData.NoticeDateTime.ToString(DateFormatType.DateTimeKorean) });
			tableContents.WriteRow(new string[] { "수입신고번호", MessageFunctions.DeclarationNumberFormat(messageData.ImportDeclarationNumber) });
			tableContents.WriteRow(new string[] { "심사담당자 성명", messageData.CustomsManagerName });
			result.Append(tableContents.ToHtml());

			var tableReasonContents = new HtmlTableCreator(System.Array.Empty<string>());
			tableReasonContents.WriteRowWithFormatting(new CellWithFormatting("미 결 사 유", new NameValueCollection { { "colspan", "2" } }, true));
			tableReasonContents.WriteRow(new string[] { "미결사유", "미결사유 상세내용" });

			var unsettledReasonTypeList = new UnsettledReasonTypeList();
			var i = 0;
			foreach (var kvp in messageData.Unsettled)
			{
				if (i > 9 && mode == MessageFunctions.MessageInterpretationMode.Email)
				{
					tableReasonContents.WriteRowWithFormatting(new CellWithFormatting("나머지 내역은 프로그램에서 확인 하십시오.", new NameValueCollection { { "colspan", "2" } }));
					break;
				}
				tableReasonContents.WriteRow(new string[] { "[" + kvp.Key + "] " + unsettledReasonTypeList.GetDescriptionFromCode(kvp.Key), kvp.Value });
				i += 1;
			}
			result.Append(tableReasonContents.ToHtml());

			return result.ToString();
		}
		#endregion
	}
}
