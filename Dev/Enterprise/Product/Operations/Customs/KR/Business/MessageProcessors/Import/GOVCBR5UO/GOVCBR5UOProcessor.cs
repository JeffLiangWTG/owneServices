using System.Collections.Specialized;
using CargoWise.Types;
using Enterprise.Customs.KR.Messaging;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.MessageProcessors;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using WTG.StaticAnalysis.Annotation;
using static Enterprise.Customs.KR.Messaging.Constants;

namespace Enterprise.Customs.KR.Business
{
	[CodeAlive("An instance is created via reflection using the attribute MessageType")]
	[MessageType(ElectronicDocumentTypeList.Codes._5UO)]
	class GOVCBR5UOProcessor : IMessageProcessor
	{
		public void Process(EDIMessage message)
		{
			using (var textReader = message.GetEM_MessageTextReader())
			{
				var messageData = new GOVCBR5UODataProvider().GetMessageData(message.Factory, textReader);
				var (entryNum5UL, entry, refundDeclaration, outgoingMessage) = MessageLinkedObjectManager.GetLinkedRefundObject(message.Factory, message.Company, messageData.RefundDeclarationNumber, ElectronicDocumentTypeList.Codes._5UL);
				if (entry != null)
				{
					message.EM_LinkedObject = entry;
					message.EM_MessageOwner = CustomsEntryStatusTypeList.Codes.ANT;
					entry.AddLog(Events.Authorised, reference: (NoResString)"TYP=Refund|REF=" + messageData.RefundDeclarationNumber, eventTime: messageData.ApprovalDate.ToZDateTime().ToOffset());

					var cusEntryNum = entry.EntryNumbers.GetOrCreateCusEntryNum(ElectronicDocumentTypeList.Codes._5UO);
					cusEntryNum.CE_EntryNum = messageData.ApprovalNo;
					cusEntryNum.CE_IssueDate = messageData.ApprovalDate;
					cusEntryNum.CE_EntryLineReference = messageData.RefundDeclarationNumber;
				}
				else if (refundDeclaration != null)
				{
					message.EM_LinkedObject = refundDeclaration;
					message.EM_MessageOwner = CustomsEntryStatusTypeList.Codes.ANT;

					var cusEntryNum = refundDeclaration.EntryNumbers.GetOrCreateCusEntryNum(ElectronicDocumentTypeList.Codes._5UO);
					cusEntryNum.CE_EntryNum = messageData.ApprovalNo;
					cusEntryNum.CE_IssueDate = messageData.ApprovalDate;
					cusEntryNum.CE_EntryLineReference = messageData.RefundDeclarationNumber;
				}
				message.EM_ApplicationReference = outgoingMessage?.EM_MessageNum ?? ZString.Empty;
				message.EM_MessageInterpretation = CreateUserFriendlyMessageInterpretation(messageData);
				if (refundDeclaration != null)
				{
					SendNotificationReconDeclaration(message, refundDeclaration, messageData);
				}
				else
				{
					SendNotification(message, entry, messageData);
				}
			}
		}

		#region SuppressResourceStringsCheckRegion
		void SendNotification(EDIMessage message, CusEntryHeader entry, GOVCBR5UOMessageData messageData)
		{
			var declaration = entry?.Declaration;
			var notificationData = new DeclarationEntryNotificationData()
			{
				Entry = entry,
				Branch = declaration?.Branch ?? message.Branch,
				EmailGroup = KRCustomsRegistry.Instance.ImportEmailGroup,
				EmailBody = CreateUserFriendlyMessageInterpretation(messageData),
				OriginalMessageTypes = new ZString[] { ElectronicDocumentTypeList.Codes._5UL },
				MessageTypeDescription = $"[{ElectronicDocumentTypeList.Descriptions._5UO}]",
				EntryNumber = messageData.RefundDeclarationNumber
			};
			NotificationSender.SendNotification(notificationData);
		}

		void SendNotificationReconDeclaration(EDIMessage message, CusReconDeclaration reconDeclaration, GOVCBR5UOMessageData messageData)
		{
			var notificationData = NotificationSender.GetReconDeclarationNotificationData(reconDeclaration, messageData.RefundDeclarationNumber);
			notificationData.EmailBody = CreateUserFriendlyMessageInterpretation(messageData);
			notificationData.MessageTypeDescription = $"[{ElectronicDocumentTypeList.Descriptions._5UO}]";
			NotificationSender.SendNotification(notificationData);
		}

		string CreateUserFriendlyMessageInterpretation(GOVCBR5UOMessageData messageData)
		{
			var result = new ZStringBuilder();

			var tableContents = new HtmlTableCreator(System.Array.Empty<string>()) { EnableHTMLEncoding = false };
			tableContents.WriteRowWithFormatting(new CellWithFormatting("항 목", new NameValueCollection { { "align", "center" }, { "width", "150" } }, true), new CellWithFormatting("내 용", new NameValueCollection { { "align", "center" }, { "width", "400" } }, true));
			tableContents.WriteRowWithFormatting(new CellWithFormatting("제출문서", new NameValueCollection { { "align", "left" }, { "width", "150" } }), new CellWithFormatting("과오납 환급 신청서", new NameValueCollection { { "align", "left" }, { "width", "400" } }));
			tableContents.WriteRowWithFormatting(new CellWithFormatting("환급신청번호", new NameValueCollection { { "align", "left" }, { "width", "150" } }), new CellWithFormatting(messageData.RefundDeclarationNumber, new NameValueCollection { { "align", "left" }, { "width", "400" } }));
			tableContents.WriteRowWithFormatting(new CellWithFormatting("업체 상호", new NameValueCollection { { "align", "left" }, { "width", "150" } }), new CellWithFormatting(messageData.ImportCompanyName, new NameValueCollection { { "align", "left" }, { "width", "400" } }));
			tableContents.WriteRowWithFormatting(new CellWithFormatting("대표자성명", new NameValueCollection { { "align", "left" }, { "width", "150" } }), new CellWithFormatting(messageData.ImportRepresentativeName, new NameValueCollection { { "align", "left" }, { "width", "400" } }));
			tableContents.WriteRowWithFormatting(new CellWithFormatting("기본주소", new NameValueCollection { { "align", "left" }, { "width", "150" } }), new CellWithFormatting(messageData.FirstImportAddressLine, new NameValueCollection { { "align", "left" }, { "width", "400" } }));
			tableContents.WriteRowWithFormatting(new CellWithFormatting("상세주소", new NameValueCollection { { "align", "left" }, { "width", "150" } }), new CellWithFormatting(messageData.SecondImportAddressLine, new NameValueCollection { { "align", "left" }, { "width", "400" } }));
			tableContents.WriteRowWithFormatting(new CellWithFormatting("통지세관명", new NameValueCollection { { "align", "left" }, { "width", "150" } }), new CellWithFormatting(messageData.CustomsOfficeName, new NameValueCollection { { "align", "left" }, { "width", "400" } }));
			tableContents.WriteRowWithFormatting(new CellWithFormatting("통지과명", new NameValueCollection { { "align", "left" }, { "width", "150" } }), new CellWithFormatting(messageData.CustomsDivisionName, new NameValueCollection { { "align", "left" }, { "width", "400" } }));
			tableContents.WriteRowWithFormatting(new CellWithFormatting("환급신청일자", new NameValueCollection { { "align", "left" }, { "width", "150" } }), new CellWithFormatting(messageData.SubmissionDate.ToString(DateFormatType.DateKorean), new NameValueCollection { { "align", "left" }, { "width", "400" } }));
			tableContents.WriteRowWithFormatting(new CellWithFormatting("환급결의일자", new NameValueCollection { { "align", "left" }, { "width", "150" } }), new CellWithFormatting(messageData.ApprovalDate.ToString(DateFormatType.DateKorean), new NameValueCollection { { "align", "left" }, { "width", "400" } }));
			tableContents.WriteRowWithFormatting(new CellWithFormatting("환급결의번호", new NameValueCollection { { "align", "left" }, { "width", "150" } }), new CellWithFormatting(messageData.ApprovalNo, new NameValueCollection { { "align", "left" }, { "width", "400" } }));
			tableContents.WriteRowWithFormatting(new CellWithFormatting("환급지급은행명", new NameValueCollection { { "align", "left" }, { "width", "150" } }), new CellWithFormatting(messageData.BankCodeName, new NameValueCollection { { "align", "left" }, { "width", "400" } }));
			tableContents.WriteRowWithFormatting(new CellWithFormatting("국고지급은행명", new NameValueCollection { { "align", "left" }, { "width", "150" } }), new CellWithFormatting(messageData.BankCodeName2, new NameValueCollection { { "align", "left" }, { "width", "400" } }));
			tableContents.WriteRowWithFormatting(new CellWithFormatting("환급지급 계좌번호", new NameValueCollection { { "align", "left" }, { "width", "150" } }), new CellWithFormatting(messageData.BankAccountNumber, new NameValueCollection { { "align", "left" }, { "width", "400" } }));
			tableContents.WriteRowWithFormatting(new CellWithFormatting("통보일자", new NameValueCollection { { "align", "left" }, { "width", "150" } }), new CellWithFormatting(messageData.NoticeDate.ToString(DateFormatType.DateKorean), new NameValueCollection { { "align", "left" }, { "width", "400" } }));
			result.Append(tableContents.ToHtml());

			var detailContents = new HtmlTableCreator(System.Array.Empty<string>()) { EnableHTMLEncoding = false };
			detailContents.WriteRowWithFormatting(new CellWithFormatting("세&nbsp;&nbsp;&nbsp;&nbsp;액&nbsp;&nbsp;&nbsp;&nbsp;내&nbsp;&nbsp;&nbsp;&nbsp;용", new NameValueCollection { { "align", "center" }, { "width", "550" }, { "colspan", "3" } }, true));
			detailContents.WriteRowWithFormatting(new CellWithFormatting("관&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;세", new NameValueCollection { { "align", "center" }, { "width", "33%" } }), new CellWithFormatting("교통에너지환경세", new NameValueCollection { { "align", "center" }, { "width", "33%" } }), new CellWithFormatting("개&nbsp;&nbsp;별&nbsp;&nbsp;소&nbsp;&nbsp;비&nbsp;&nbsp;세", new NameValueCollection { { "align", "center" }, { "width", "33%" } }));
			detailContents.WriteRowWithFormatting(new CellWithFormatting("주&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;세", new NameValueCollection { { "align", "center" }, { "width", "33%" } }), new CellWithFormatting("농&nbsp;&nbsp;&nbsp;&nbsp;특&nbsp;&nbsp;&nbsp;&nbsp;세", new NameValueCollection { { "align", "center" }, { "width", "33%" } }), new CellWithFormatting("부&nbsp;&nbsp;가&nbsp;&nbsp;가&nbsp;&nbsp;치&nbsp;&nbsp;세", new NameValueCollection { { "align", "center" }, { "width", "33%" } }));
			detailContents.WriteRowWithFormatting(new CellWithFormatting("교&nbsp;&nbsp;&nbsp;&nbsp;육&nbsp;&nbsp;&nbsp;&nbsp;세", new NameValueCollection { { "align", "center" }, { "width", "33%" } }), new CellWithFormatting("가&nbsp;&nbsp;&nbsp;&nbsp;산&nbsp;&nbsp;&nbsp;&nbsp;금", new NameValueCollection { { "align", "center" }, { "width", "33%" } }), new CellWithFormatting("세&nbsp;&nbsp;&nbsp;&nbsp;외&nbsp;&nbsp;&nbsp;&nbsp;수&nbsp;&nbsp;&nbsp;&nbsp;입", new NameValueCollection { { "align", "center" }, { "width", "33%" } }));
			detailContents.WriteRowWithFormatting(new CellWithFormatting(string.Format("{0:#,##0}", messageData.DutyAmount), new NameValueCollection { { "align", "right" }, { "width", "33%" } }), new CellWithFormatting(string.Format("{0:#,##0}", messageData.TransportationTax), new NameValueCollection { { "align", "right" }, { "width", "33%" } }), new CellWithFormatting(string.Format("{0:#,##0}", messageData.SpecialConsumptionTax), new NameValueCollection { { "align", "right" }, { "width", "33%" } }));
			detailContents.WriteRowWithFormatting(new CellWithFormatting(string.Format("{0:#,##0}", messageData.LiquorTax), new NameValueCollection { { "align", "right" }, { "width", "33%" } }), new CellWithFormatting(string.Format("{0:#,##0}", messageData.AgricultureTax), new NameValueCollection { { "align", "right" }, { "width", "33%" } }), new CellWithFormatting(string.Format("{0:#,##0}", messageData.VAT), new NameValueCollection { { "align", "right" }, { "width", "33%" } }));
			detailContents.WriteRowWithFormatting(new CellWithFormatting(string.Format("{0:#,##0}", messageData.EducationTax), new NameValueCollection { { "align", "right" }, { "width", "33%" } }), new CellWithFormatting(string.Format("{0:#,##0}", messageData.Penalty), new NameValueCollection { { "align", "right" }, { "width", "33%" } }), new CellWithFormatting(string.Format("{0:#,##0}", messageData.NonDutyTaxRevenue), new NameValueCollection { { "align", "right" }, { "width", "33%" } }));
			detailContents.WriteRowWithFormatting(new CellWithFormatting("합&nbsp;계&nbsp;금&nbsp;액", new NameValueCollection { { "align", "center" }, { "colspan", "2" } }), new CellWithFormatting(string.Format("{0:#,##0}", messageData.TotalAmount), new NameValueCollection { { "align", "right" }, { "width", "33%" } }));
			result.Append(HtmlResponseEmailGenerator.HtmlConstants.Br + detailContents.ToHtml());

			return result.ToString();
		}
		#endregion
	}
}
