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
	[MessageType(ElectronicDocumentTypeList.Codes._5GU)]
	class GOVCBR5GUProcessor : IMessageProcessor
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Not a code smell")]
		internal const string removeText = "시정";

		public void Process(EDIMessage message)
		{
			using (var textReader = message.GetEM_MessageTextReader())
			{
				var messageData = new GOVCBR5GUDataProvider().GetMessageData(message.Factory, textReader, message);
				var entry = MessageLinkedObjectManager.GetLinkedObject(message.Factory, message.Company, messageData.ImportDeclarationNumber, SharedJobMessageTypeList.Codes.Import);
				if (entry != null)
				{
					message.EM_LinkedObject = entry;

					var entryNum = entry.EntryNumbers.GetOrCreateCusEntryNum(ElectronicDocumentTypeList.Codes._5GU);
					entryNum.CE_IssueDate = messageData.CustomsRegistryDate;
					entryNum.CE_ExpiryDate = messageData.CorrectionOrderDeadline;
					entryNum.CE_EntryNum = messageData.ComplementNumber.Replace("-", "").Replace(removeText, "");
				}

				message.EM_MessageInterpretation = CreateUserFriendlyMessageInterpretation(message, messageData, MessageFunctions.MessageInterpretationMode.FullView);
				SendNotification(message, entry, messageData);
			}
		}

		#region SuppressResourceStringsCheckRegion
		void SendNotification(EDIMessage message, CusEntryHeader entry, GOVCBR5GUMessageData messageData)
		{
			var declaration = entry?.Declaration;
			var notificationData = new DeclarationEntryNotificationData()
			{
				Entry = entry,
				Branch = declaration?.Branch ?? message.Branch,
				EmailGroup = KRCustomsRegistry.Instance.ImportEmailGroup,
				EmailBody = CreateUserFriendlyMessageInterpretation(message, messageData, MessageFunctions.MessageInterpretationMode.Email),
				OriginalMessageTypes = new ZString[] { ElectronicDocumentTypeList.Codes._929 },
				MessageTypeDescription = $"[{ElectronicDocumentTypeList.Descriptions._5GU}]",
				EntryNumber = messageData.ImportDeclarationNumber
			};
			NotificationSender.SendNotification(notificationData);
		}

		string CreateUserFriendlyMessageInterpretation(EDIMessage message, GOVCBR5GUMessageData messageData, MessageFunctions.MessageInterpretationMode mode)
		{
			var result = new ZStringBuilder();

			var tableContents = new HtmlTableCreator(System.Array.Empty<string>());
			tableContents.WriteRowWithFormatting(new CellWithFormatting("항 목", new NameValueCollection { { "width", "200" } }, true), new CellWithFormatting("내 용", new NameValueCollection { { "width", "300" } }, true));
			tableContents.WriteRow(new string[] { "제출문서", "수입신고서" });
			tableContents.WriteRow(new string[] { "신고번호", MessageFunctions.DeclarationNumberFormat(messageData.ImportDeclarationNumber) });
			tableContents.WriteRow(new string[] { "세관등록일자", messageData.CustomsRegistryDate.ToString(DateFormatType.DateKorean) });
			tableContents.WriteRow(new string[] { "시정명령기한", messageData.CorrectionOrderDeadline.ToString(DateFormatType.DateKorean) });
			tableContents.WriteRow(new string[] { "세관(과)", "[" + messageData.DeclarationOffice + "] " + MessageFunctions.GetCustomsOfficeAndDivision(message.Factory, messageData.DeclarationOffice) });
			tableContents.WriteRow(new string[] { "세관담당자명", messageData.CustomsPersonName });
			tableContents.WriteRow(new string[] { "문서번호", messageData.ComplementNumber });
			tableContents.WriteRow(new string[] { "전화번호", messageData.CustomsPersonPhoneNumber });
			result.Append(tableContents.ToHtml());

			if (messageData.Corrections.Count > 0)
			{
				var detailContents = new HtmlTableCreator(System.Array.Empty<string>()) { EnableHTMLEncoding = false };
				detailContents.WriteRowWithFormatting(new CellWithFormatting("위반 내용", new NameValueCollection { { "align", "left" }, { "colspan", "4" } }, true));
				detailContents.WriteRowWithFormatting(new CellWithFormatting("란번호", new NameValueCollection { { "align", "center" }, { "width", "50" } }), new CellWithFormatting("위반유형코드", new NameValueCollection { { "align", "center" }, { "width", "100" } }),
													  new CellWithFormatting("위반내용", new NameValueCollection { { "align", "center" }, { "width", "150" } }), new CellWithFormatting("시정방법", new NameValueCollection { { "align", "center" }, { "width", "200" } }));
				var i = 0;
				foreach (GOVCBR5GULineMessageData correction in messageData.Corrections)
				{
					if (mode == MessageFunctions.MessageInterpretationMode.Email && i > 9)
					{
						detailContents.WriteRowWithFormatting(new CellWithFormatting("나머지 내역은 프로그램에서 확인 하십시오.", new NameValueCollection { { "colspan", "4" } }));
						break;
					}
					detailContents.WriteRowWithFormatting(new CellWithFormatting(correction.EntryLineNo.ToString()),
																  new CellWithFormatting(correction.ViolationCode),
																  new CellWithFormatting(correction.ViolationName),
																  new CellWithFormatting(correction.CorrectionMethod));
					i += 1;
				}
				result.Append(detailContents.ToHtml());
			}

			return result.ToString();
		}
		#endregion
	}
}
