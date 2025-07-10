using System.Collections.Specialized;
using System.Linq;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.Common.Shared;
using Enterprise.Customs.KR.Messaging;
using Enterprise.Messaging.MessageProcessors;
using WTG.StaticAnalysis.Annotation;
using static Enterprise.Customs.KR.Messaging.Constants;

namespace Enterprise.Customs.KR.Business
{
	[CodeAlive("An instance is created via reflection using the attribute MessageType")]
	[MessageType(ElectronicDocumentTypeList.Codes._R43)]
	public class GOVCBRR43Processor : IMessageProcessor
	{
		public void Process(EDIMessage message)
		{
			using (var textReader = message.GetEM_MessageTextReader())
			{
				var messageData = new GOVCBRR43DataProvider().GetMessageData(message.Factory, textReader);
				var entry = MessageLinkedObjectManager.GetLinkedObject(message.Factory, message.Company, messageData.ImportDeclarationNumber, SharedJobMessageTypeList.Codes.Import);

				if (entry != null)
				{
					message.EM_LinkedObject = entry;
					var outgoingMessage = entry.Messages.GetLastMessageWithMatchingVersionNumber(ElectronicDocumentTypeList.Codes._D72, messageData.AmendSequence.ToString());
					message.EM_ApplicationReference = outgoingMessage?.EM_MessageNum ?? ZString.Empty;
					message.EM_MessageSubType = ElectronicDocumentTypeList.Codes._D72;
					if (messageData.ResultType == ResultTypeList.Codes.C)
					{
						message.EM_MessageOwner = (ZString)CustomsEntryStatusTypeList.Codes.ANT;

						if (!messageData.AfterReExportScheduledDate.IsEmpty)
						{
							var entryLines = MessageLinkedObjectManager.GetLinkedObject(message.Factory, message.Branch.Company, messageData.ImportDeclarationNumber, SharedJobMessageTypeList.Codes.Import)?.MergedLines;
							if (entryLines != null)
							{
								foreach (GOVCBRR43LineMessageData invoiceLineData in messageData.InvoiceLines)
								{
									var entryLine = entryLines?.FirstOrDefault(x => x.CL_LineNumber == invoiceLineData.EntryLineNo);
									var invoiceLine = entryLine?.InvoiceLines?.Cast<JobComInvoiceLine>().FirstOrDefault(x => x.JI_SequenceNumber == invoiceLineData.InvoiceLineNo);

									if (invoiceLine != null)
									{
										invoiceLine.JI_ScheduledReExportDate = messageData.AfterReExportScheduledDate.ToZDateTime();
									}
								}
							}
						}
					}
					else if (messageData.ResultType == ResultTypeList.Codes.E)
					{
						message.EM_MessageOwner = (ZString)CustomsEntryStatusTypeList.Codes.DMS;
					}
				}
				message.EM_MessageInterpretation = CreateUserFriendlyMessageInterpretation(message, messageData);
				SendNotification(entry, message, messageData);
			}
		}

		#region SuppressResourceStringsCheckRegion

		void SendNotification(CusEntryHeader entry, EDIMessage message, IGOVCBRR43MessageData messageData)
		{
			var declaration = entry?.Declaration;
			var notificationData = new DeclarationEntryNotificationData()
			{
				Entry = entry,
				Branch = declaration?.Branch ?? message.Branch,
				EmailGroup = KRCustomsRegistry.Instance.ImportEmailGroup,
				EmailBody = CreateUserFriendlyMessageInterpretation(message, messageData),
				OriginalMessageTypes = new ZString[] { ElectronicDocumentTypeList.Codes._D72 },
				MessageTypeDescription = $"[{ElectronicDocumentTypeList.Descriptions._R43}]",
				EntryNumber = messageData.ImportDeclarationNumber
			};
			NotificationSender.SendNotification(notificationData);
		}
		string CreateUserFriendlyMessageInterpretation(EDIMessage message, IGOVCBRR43MessageData messageData)
		{
			var result = new ZStringBuilder();
			var resultType = new ResultTypeList();
			var tableContents = new HtmlTableCreator(System.Array.Empty<string>());
			tableContents.WriteRowWithFormatting(new CellWithFormatting("항목", new NameValueCollection { { "width", "100" } }, true), new CellWithFormatting("내용", new NameValueCollection { { "width", "450" } }, true));
			tableContents.WriteRow(new string[] { "제출문서", "재수출조건부 면세승인 이행기간연장 신청서" });
			tableContents.WriteRow(new string[] { "수입신고번호", MessageFunctions.DeclarationNumberFormat(messageData.ImportDeclarationNumber) });
			tableContents.WriteRow(new string[] { "신청일자", messageData.DeclarationDate.ToString(DateFormatType.DateKorean) });
			tableContents.WriteRow(new string[] { "처리일시", messageData.NoticeDateTime.ToString(DateFormatType.DateTimeKorean) });
			tableContents.WriteRow(new string[] { "연장승인일자", messageData.AfterReExportScheduledDate.ToString(DateFormatType.DateKorean) });
			tableContents.WriteRow(new string[] { "처리결과", resultType.GetDescriptionFromCode(messageData.ResultType) });
			tableContents.WriteRow(new string[] { "처리결과내역", messageData.ResultReason });
			tableContents.WriteRow(new string[] { "처리담당자명", messageData.CustomsManagerName });
			tableContents.WriteRow(new string[] { "처리세관(과)", messageData.CustomsOfficeAndDivision.IsEmpty ? string.Empty : "[" + messageData.CustomsOfficeAndDivision + "] " + MessageFunctions.GetCustomsOfficeAndDivision(message.Factory, messageData.CustomsOfficeAndDivision) });
			tableContents.WriteRow(new string[] { "세관기재란", messageData.CustomsOfficeContent });
			result.Append(tableContents.ToHtml());

			if (!messageData.DutyFulfillment.IsNullOrEmpty())
			{
				var dutyFulfillmentRequestReasonCodeList = new DutyFulfillmentRequestReasonCodeList();
				var dutyFulfillmentContents = new HtmlTableCreator(System.Array.Empty<string>());
				dutyFulfillmentContents.WriteRowWithFormatting(new CellWithFormatting("의무이행요구사항", true));

				foreach (var dutyFulfillment in messageData.DutyFulfillment)
				{
					dutyFulfillmentContents.WriteRowWithFormatting(new CellWithFormatting(dutyFulfillmentRequestReasonCodeList.GetDescriptionFromCode(dutyFulfillment), new NameValueCollection { { "width", "550" } }));
				}
				result.Append(dutyFulfillmentContents.ToHtml());
			}

			return result.ToString();
		}
		#endregion
	}
}
