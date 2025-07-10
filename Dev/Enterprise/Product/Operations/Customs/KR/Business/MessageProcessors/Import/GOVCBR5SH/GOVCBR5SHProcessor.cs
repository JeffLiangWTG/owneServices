using System.Collections.Specialized;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.Common.KR;
using Enterprise.Customs.KR.Messaging;
using Enterprise.Messaging.MessageProcessors;
using WTG.StaticAnalysis.Annotation;
using static Enterprise.Customs.KR.Messaging.Constants;

namespace Enterprise.Customs.KR.Business
{
	[CodeAlive("An instance is created via reflection using the attribute MessageType")]
	[MessageType(ElectronicDocumentTypeList.Codes._5SH)]
	class GOVCBR5SHProcessor : IMessageProcessor
	{
		public void Process(EDIMessage message)
		{
			using (var textReader = message.GetEM_MessageTextReader())
			{
				var messageData = new GOVCBR5SHDataProvider().GetMessageData(textReader);
				var miscRequestHeader = MessageLinkedObjectManager.GetLinkedMiscRequestHeader(message.Factory, message.Company, messageData.ApplicationNumber, ElectronicDocumentTypeList.Codes._5SG);
				if (miscRequestHeader != null)
				{
					message.EM_LinkedObject = miscRequestHeader;
					if (messageData.ResultType == ResultTypeList.Codes.C)
					{
						message.EM_MessageOwner = CustomsEntryStatusTypeList.Codes.ANT;

						foreach (var entryDetail in messageData.EntryDetails)
						{
							var entry = MessageLinkedObjectManager.GetLinkedObject(message.Factory, message.Company, entryDetail.ImportDeclarationNumber, KRJobMessageTypeList.Codes.Import);
							var matchingLine = miscRequestHeader.RequestLines.FirstOrDefault(x => x.CML_EntryNumber == entryDetail.ImportDeclarationNumber);
							if (matchingLine != null && entryDetail.EntryReleaseDate.IsValid)
							{
								string lineBreak = string.Empty;
								if (!matchingLine.CML_Remarks.IsEmpty)
								{
									lineBreak = "\r\n";
								}
								matchingLine.CML_Remarks = $"연장수리일자 : {entryDetail.EntryReleaseDate.ToISO8601ShortDateString()}" + lineBreak + matchingLine.CML_Remarks;
							}
							if (entry != null)
							{
								var entryNum934 = entry.EntryNumbers.Cast<CusEntryNumber>().FirstOrDefault(x => x.CE_EntryType == ElectronicDocumentTypeList.Codes._934);
								if (entryNum934 != null)
								{
									entryNum934.CE_ExpiryDate = (ZDateTime)entryDetail.EntryReleaseDate;
								}
							}
						}
					}
					else if (messageData.ResultType == ResultTypeList.Codes.E)
					{
						message.EM_MessageOwner = CustomsEntryStatusTypeList.Codes.DMS;
					}

					var entryNum5SG = miscRequestHeader.CusEntryNumber;
					if (entryNum5SG != null)
					{
						entryNum5SG.CE_IssueDate = (ZDateTime)messageData.ApprovalDate;
					}
				}

				message.EM_MessageInterpretation = CreateUserFriendlyMessageInterpretation(message, messageData, MessageFunctions.MessageInterpretationMode.FullView);
				var outgoingMessage = miscRequestHeader?.Messages.GetLastMessage(EDIMessage.ApplicationCodes.KRCustoms, ElectronicDocumentTypeList.Codes._5SG);
				message.EM_ApplicationReference = outgoingMessage?.EM_MessageNum ?? ZString.Empty;
				SendNotification(message, miscRequestHeader, messageData);
			}
		}

		#region SuppressResourceStringsCheckRegion

		void SendNotification(EDIMessage message, CusMiscRequestHeader requestHeader, IGOVCBR5SHMessageData messageData)
		{
			var notificationData = new NotificationData()
			{
				ControllerIDProvider = requestHeader,
				MessagesParent = requestHeader,
				JobNumberDescription = $"Declaration Number: {requestHeader?.CMR_JobNumber ?? ZString.Empty} / 제출번호: {messageData.ApplicationNumber}",
				Branch = requestHeader?.Branch ?? message.Branch,
				EmailGroup = KRCustomsRegistry.Instance.ImportEmailGroup,
				EmailBody = CreateUserFriendlyMessageInterpretation(message, messageData, MessageFunctions.MessageInterpretationMode.Email),
				OriginalMessageTypes = new ZString[] { ElectronicDocumentTypeList.Codes._5SG },
				MessageTypeDescription = $"[{ElectronicDocumentTypeList.Descriptions._5SH}]",
				EntryNumber = messageData.ApplicationNumber,
				AlternativeRecipientStaff = requestHeader?.Broker
			};
			NotificationSender.SendNotification(notificationData);
		}

		string CreateUserFriendlyMessageInterpretation(EDIMessage message, IGOVCBR5SHMessageData messageData, MessageFunctions.MessageInterpretationMode mode)
		{
			var result = new ZStringBuilder();

			var tableContents = new HtmlTableCreator(new string[] { "항 목", "내 용" });
			tableContents.WriteRow(new string[] { "제출문서", "확정가격신고 기간 연장 신청서" });
			tableContents.WriteRow(new string[] { "확정가격신고기간연장신청번호", messageData.ApplicationNumber });
			tableContents.WriteRow(new string[] { "승인(기각)일자", messageData.ApprovalDate.ToString(DateFormatType.DateKorean) });
			tableContents.WriteRow(new string[] { "처리결과", "[" + messageData.ResultType + "] " + new ResultTypeList().GetDescriptionFromCode(messageData.ResultType) ?? ZString.Empty });
			tableContents.WriteRow(new string[] { "기각사유", messageData.DismissalReason });
			tableContents.WriteRow(new string[] { "담당자명", messageData.CustomsManagerName });
			tableContents.WriteRow(new string[] { "승인세관", "[" + messageData.CustomsOffice + "] " + MessageFunctions.GetCustomsOffice(message.Factory, messageData.CustomsOffice) });
			result.Append(tableContents.ToHtml());

			if (messageData.EntryDetails != null)
			{
				var detailContents = new HtmlTableCreator(System.Array.Empty<string>()) { EnableHTMLEncoding = false };
				detailContents.WriteRowWithFormatting(new CellWithFormatting("연장일 정보", new NameValueCollection { { "align", "center" }, { "colspan", "2" } }, true));
				detailContents.WriteRowWithFormatting(new CellWithFormatting("수입신고번호", new NameValueCollection { { "align", "center" }, { "width", "200" } }), new CellWithFormatting("연장수리일자", new NameValueCollection { { "align", "center" }, { "width", "300" } }));
				var i = 0;
				foreach (var entryDetail in messageData.EntryDetails)
				{
					if (mode == MessageFunctions.MessageInterpretationMode.Email && i > 9)
					{
						detailContents.WriteRowWithFormatting(new CellWithFormatting("나머지 내역은 프로그램에서 확인 하십시오.", new NameValueCollection { { "colspan", "2" } }));
						break;
					}
					detailContents.WriteRow(new string[] { MessageFunctions.DeclarationNumberFormat(entryDetail.ImportDeclarationNumber), entryDetail.EntryReleaseDate.ToString(DateFormatType.DateKorean) });
					i += 1;
				}
				result.Append(detailContents.ToHtml());
			}

			return result.ToString();
		}
		#endregion
	}
}
