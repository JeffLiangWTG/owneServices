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
	[MessageType(ElectronicDocumentTypeList.Codes._5GV)]
	class GOVCBR5GVProcessor : IMessageProcessor
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Not a code smell")]
		internal const string removeText = "보완";

		public void Process(EDIMessage message)
		{
			using (var textReader = message.GetEM_MessageTextReader())
			{
				var messageData = new GOVCBR5GVDataProvider().GetMessageData(message.Factory, textReader);
				var entry = MessageLinkedObjectManager.GetLinkedObject(message.Factory, message.Company, messageData.ImportDeclarationNumber, SharedJobMessageTypeList.Codes.Import);
				if (entry != null)
				{
					message.EM_LinkedObject = entry;

					if (messageData.ComplementReasonCode == ComplementReasonCodeList.Codes._1)
					{
						entry.CH_EntryStatus = CustomsEntryStatusTypeList.Codes.MDC;
					}
					else if (messageData.ComplementReasonCode == ComplementReasonCodeList.Codes._2)
					{
						entry.CH_EntryStatus = CustomsEntryStatusTypeList.Codes.MFD;
					}
					else if (messageData.ComplementReasonCode == ComplementReasonCodeList.Codes._3)
					{
						entry.CH_EntryStatus = CustomsEntryStatusTypeList.Codes.CGD;
					}
					else if (messageData.ComplementReasonCode == ComplementReasonCodeList.Codes._4)
					{
						entry.CH_EntryStatus = CustomsEntryStatusTypeList.Codes.CGP;
					}
					else
					{
						entry.CH_EntryStatus = CustomsEntryStatusTypeList.Codes.MFI;
					}

					var entryNum = entry.EntryNumbers.GetOrCreateCusEntryNum(ElectronicDocumentTypeList.Codes._5GV);
					entryNum.CE_IssueDate = messageData.NoticeDate;
					entryNum.CE_ExpiryDate = messageData.ComplementByDate;
					entryNum.CE_EntryNum = messageData.ComplementNumber.Replace("-", "").Replace(removeText, "");
				}
				message.EM_MessageInterpretation = CreateUserFriendlyMessageInterpretation(messageData, MessageFunctions.MessageInterpretationMode.FullView);
				SendNotification(message, entry, messageData);
			}
		}

		#region SuppressResourceStringsCheckRegion
		void SendNotification(EDIMessage message, CusEntryHeader entry, GOVCBR5GVMessageData messageData)
		{
			var declaration = entry?.Declaration;
			var notificationData = new DeclarationEntryNotificationData()
			{
				Entry = entry,
				Branch = declaration?.Branch ?? message.Branch,
				EmailGroup = KRCustomsRegistry.Instance.ImportEmailGroup,
				EmailBody = CreateUserFriendlyMessageInterpretation(messageData, MessageFunctions.MessageInterpretationMode.Email),
				OriginalMessageTypes = new ZString[] { ElectronicDocumentTypeList.Codes._929 },
				MessageTypeDescription = $"[{ElectronicDocumentTypeList.Descriptions._5GV}]",
				EntryNumber = messageData.ImportDeclarationNumber
			};
			NotificationSender.SendNotification(notificationData);
		}

		string CreateUserFriendlyMessageInterpretation(GOVCBR5GVMessageData messageData, MessageFunctions.MessageInterpretationMode interpretationMode)
		{
			var result = new ZStringBuilder();

			var tableContents = new HtmlTableCreator(new string[] { "항 목", "내 용" });
			tableContents.WriteRow(new string[] { "제출문서", "수입신고서" });
			tableContents.WriteRow(new string[] { "발행일자", messageData.NoticeDate.ToString(DateFormatType.DateKorean) });
			tableContents.WriteRow(new string[] { "수입신고번호", MessageFunctions.DeclarationNumberFormat(messageData.ImportDeclarationNumber) });
			tableContents.WriteRow(new string[] { "보완요구통보구분명", messageData.ComplementReasonName });
			tableContents.WriteRow(new string[] { "보완요구기한일자", messageData.ComplementByDate.ToString(DateFormatType.DateKorean) });
			tableContents.WriteRow(new string[] { "담당자 성명", messageData.CustomsManagerName });
			tableContents.WriteRow(new string[] { "담당자 전화번호", messageData.CustomsManagerPhoneNumber });
			tableContents.WriteRow(new string[] { "보완요구사유", messageData.ComplementDescription });
			result.Append(tableContents.ToHtml());

			if (messageData.Lines != null && messageData.Lines.Count > 0)
			{
				var detailContents = new HtmlTableCreator(System.Array.Empty<string>()) { EnableHTMLEncoding = false };

				var firstLine = (GOVCBR5GVLineMessageData)messageData.Lines.FirstOrDefault();
				if (messageData.Lines.Count == 1 && (firstLine.SecondLineNo.IsEmpty && firstLine.SecondLineDataItemID.IsEmpty && firstLine.SecondLineDataItemIDDescription.IsEmpty))
				{
					detailContents.WriteRowWithFormatting(new CellWithFormatting("보완요구 항목", new NameValueCollection { { "align", "left" }, { "colspan", "3" } }, true));
					detailContents.WriteRowWithFormatting(new CellWithFormatting("란번호", new NameValueCollection { { "align", "center" }, { "width", "100" } }), new CellWithFormatting("항목번호", new NameValueCollection { { "align", "center" }, { "width", "100" } }), new CellWithFormatting("항목명", new NameValueCollection { { "align", "center" }, { "width", "300" } }));
					detailContents.WriteRow(new string[] { firstLine.FirstLineNo.ToString(), firstLine.FirstLineDataItemID, firstLine.FirstLineDataItemIDDescription });
				}
				else
				{
					detailContents.WriteRowWithFormatting(new CellWithFormatting("보완요구 항목", new NameValueCollection { { "align", "left" }, { "colspan", "6" } }, true));
					detailContents.WriteRowWithFormatting(new CellWithFormatting("란번호", new NameValueCollection { { "align", "center" }, { "width", "50" } }), new CellWithFormatting("항목번호", new NameValueCollection { { "align", "center" }, { "width", "50" } }), new CellWithFormatting("항목명", new NameValueCollection { { "align", "center" }, { "width", "150" } })
													, new CellWithFormatting("란번호", new NameValueCollection { { "align", "center" }, { "width", "50" } }), new CellWithFormatting("항목번호", new NameValueCollection { { "align", "center" }, { "width", "50" } }), new CellWithFormatting("항목명", new NameValueCollection { { "align", "center" }, { "width", "150" } }));

					var i = 0;
					foreach (GOVCBR5GVLineMessageData line in messageData.Lines)
					{
						if (interpretationMode == MessageFunctions.MessageInterpretationMode.Email && i > 9)
						{
							detailContents.WriteRowWithFormatting(new CellWithFormatting("나머지 내역은 프로그램에서 확인 하십시오.", new NameValueCollection { { "colspan", "6" } }));
							break;
						}
						var additionalInformationCodeList = new AdditionalInformationCodeList();
						detailContents.WriteRow(new string[] { line.FirstLineNo.ToString(), line.FirstLineDataItemID, line.FirstLineDataItemIDDescription, line.SecondLineNo.ToString(), line.SecondLineDataItemID, line.SecondLineDataItemIDDescription });
						i += 1;
					}
				}

				result.Append(detailContents.ToHtml());
			}

			return result.ToString();
		}
		#endregion
	}
}
