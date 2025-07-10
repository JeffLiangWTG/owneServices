using System.Collections.Specialized;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.Common.Shared;
using Enterprise.Customs.KR.Messaging;
using Enterprise.Messaging.MessageProcessors;
using WTG.StaticAnalysis.Annotation;
using static Enterprise.Customs.KR.Messaging.Constants;

namespace Enterprise.Customs.KR.Business
{
	[CodeAlive("An instance is created via reflection using the attribute MessageType")]
	[MessageType(ElectronicDocumentTypeList.Codes._R60)]
	class GOVCBRR60Processor : IMessageProcessor
	{
		public void Process(EDIMessage message)
		{
			using (var textReader = message.GetEM_MessageTextReader())
			{
				var messageData = new GOVCBRR60DataProvider().GetMessageData(textReader);
				var entry = MessageLinkedObjectManager.GetLinkedObject(message.Factory, message.Company, messageData.ExportDeclarationNumber, SharedJobMessageTypeList.Codes.Export);
				if (entry != null)
				{
					message.EM_LinkedObject = entry;
					entry.CH_EntryStatus = CustomsEntryStatusTypeList.Codes.AMI;
				}
				message.EM_MessageInterpretation = CreateUserFriendlyMessageInterpretation(messageData, MessageFunctions.MessageInterpretationMode.FullView);
				SendNotification(message, entry, messageData);
			}
		}

		#region SuppressResourceStringsCheckRegion

		void SendNotification(EDIMessage message, CusEntryHeader entry, IGOVCBRR60MessageData messageData)
		{
			var declaration = entry?.Declaration;
			var notificationData = new DeclarationEntryNotificationData()
			{
				Entry = entry,
				Branch = declaration?.Branch ?? message.Branch,
				EmailGroup = KRCustomsRegistry.Instance.ExportEmailGroup,
				EmailBody = CreateUserFriendlyMessageInterpretation(messageData, MessageFunctions.MessageInterpretationMode.Email),
				OriginalMessageTypes = new ZString[] { ElectronicDocumentTypeList.Codes._830 },
				MessageTypeDescription = $"[{ElectronicDocumentTypeList.Descriptions._R60}]",
				EntryNumber = messageData.ExportDeclarationNumber
			};
			NotificationSender.SendNotification(notificationData);
		}

		string CreateUserFriendlyMessageInterpretation(IGOVCBRR60MessageData messageData, MessageFunctions.MessageInterpretationMode mode)
		{
			var result = new ZStringBuilder();
			var tableContents = new HtmlTableCreator(System.Array.Empty<string>());
			tableContents.WriteRowWithFormatting(new CellWithFormatting("항 목", new NameValueCollection { { "width", "150" } }, true), new CellWithFormatting("내 용", new NameValueCollection { { "width", "350" } }, true));
			tableContents.WriteRow(new string[] { "제출문서", "수출신고서" });
			tableContents.WriteRow(new string[] { "보완통보일시", messageData.NoticeDateTime.ToString(DateFormatType.DateTimeKorean) });
			tableContents.WriteRow(new string[] { "수출신고번호", MessageFunctions.DeclarationNumberFormat(messageData.ExportDeclarationNumber) });
			tableContents.WriteRow(new string[] { "수출화주", messageData.SupplierName });
			result.Append(tableContents.ToHtml());

			if (messageData.Commodity.Any())
			{
				var detailContents = new HtmlTableCreator(System.Array.Empty<string>()) { EnableHTMLEncoding = false };
				detailContents.WriteRowWithFormatting(new CellWithFormatting("자동보완 내역", new NameValueCollection { { "colspan", "4" } }, true));
				detailContents.WriteRow(new string[] { "검증 모델명", "자동보완 내역" });
				var i = 0;

				foreach (var commodity in messageData.Commodity)
				{
					if (mode == MessageFunctions.MessageInterpretationMode.Email && i > 9)
					{
						detailContents.WriteRowWithFormatting(new CellWithFormatting("나머지 내역은 프로그램에서 확인 하십시오.", new NameValueCollection { { "colspan", "4" } }));
						break;
					}
					detailContents.WriteRowWithFormatting(new CellWithFormatting(commodity.ModelName, new NameValueCollection { { "width", "150" } }),
																  new CellWithFormatting(commodity.ComplementDescription, new NameValueCollection { { "width", "350" } }));
					i += 1;
				}
				result.Append(detailContents.ToHtml());
			}
			return result.ToString();
		}
		#endregion
	}
}
