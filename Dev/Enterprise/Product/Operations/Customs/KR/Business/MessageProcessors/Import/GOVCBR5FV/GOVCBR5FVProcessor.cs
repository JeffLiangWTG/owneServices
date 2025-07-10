using CargoWise.Types;
using Enterprise.Customs.Common.Shared;
using Enterprise.Customs.KR.Messaging;
using Enterprise.Messaging.MessageProcessors;
using WTG.StaticAnalysis.Annotation;
using static Enterprise.Customs.KR.Messaging.Constants;

namespace Enterprise.Customs.KR.Business
{
	[CodeAlive("Soon to be used")]
	[MessageType(ElectronicDocumentTypeList.Codes._5FV)]
	class GOVCBR5FVProcessor : IMessageProcessor
	{
		public void Process(EDIMessage message)
		{
			using (var textReader = message.GetEM_MessageTextReader())
			{
				var messageData = new GOVCBR5FVDataProvider().GetMessageData(message.Factory, textReader);
				var entry = MessageLinkedObjectManager.GetLinkedObject(message.Factory, message.Company, messageData.ImportDeclarationNumber, SharedJobMessageTypeList.Codes.Import);
				if (entry != null)
				{
					message.EM_LinkedObject = entry;

					var statement = new CusStatementHeader.Loader(message.Factory).Load(messageData.NoticeNumber, entry.Declaration.JE_GC, StatementHeaderTypeList.Codes.CustomsDisbursementBill);
					if (statement != null)
					{
						statement.B2_PaymentAuthorizationDate = messageData.PaymentDate;
						statement.B2_PaymentStatus = StatementHeaderPaymentStatusList.Codes.PYC;
					}
				}
				message.EM_MessageInterpretation = CreateUserFriendlyMessageInterpretation(message, messageData);
				SendNotification(message, entry, messageData);
			}
		}

		#region SuppressResourceStringsCheckRegion
		void SendNotification(EDIMessage message, CusEntryHeader entry, IGOVCBR5FVMessageData messageData)
		{
			var declaration = entry?.Declaration;
			var notificationData = new DeclarationEntryNotificationData()
			{
				Entry = entry,
				Branch = declaration?.Branch ?? message.Branch,
				EmailGroup = KRCustomsRegistry.Instance.ImportEmailGroup,
				EmailBody = CreateUserFriendlyMessageInterpretation(message, messageData),
				OriginalMessageTypes = new ZString[] { ElectronicDocumentTypeList.Codes._929 },
				MessageTypeDescription = $"[{ElectronicDocumentTypeList.Descriptions._5FV}]",
				EntryNumber = messageData.ImportDeclarationNumber
			};
			NotificationSender.SendNotification(notificationData);
		}

		string CreateUserFriendlyMessageInterpretation(EDIMessage message, IGOVCBR5FVMessageData messageData)
		{
			var result = new ZStringBuilder();

			var tableContents = new HtmlTableCreator(new string[] { "항 목", "내 용" });
			tableContents.WriteRow(new string[] { "제출문서", "수입신고서" });

			var invNumber = messageData.TaxInvoiceNumber;
			if (!invNumber.IsEmpty && invNumber.Length == 13)
			{
				tableContents.WriteRow(new string[] { "세금계산서 번호", invNumber.Substring(0, 3) + "-" +
																			invNumber.Substring(3, 2) + "-" +
																			invNumber.Substring(5) });
			}
			else
			{
				tableContents.WriteRow(new string[] { "세금계산서 번호", invNumber });
			}
			tableContents.WriteRow(new string[] { "수입신고번호", MessageFunctions.DeclarationNumberFormat(messageData.ImportDeclarationNumber) });
			tableContents.WriteRow(new string[] { "고지번호", messageData.NoticeNumber });
			var refundApprovalNo = messageData.RefundApprovalNo;
			if (!refundApprovalNo.IsEmpty && refundApprovalNo.Length == 12)
			{
				tableContents.WriteRow(new string[] { "환급결의번호", refundApprovalNo.Substring(0, 3) + "-" +
																		 refundApprovalNo.Substring(3, 2) + "-" +
																		 refundApprovalNo.Substring(5, 2) + "-" +
																		 refundApprovalNo.Substring(7) });
			}
			else
			{
				tableContents.WriteRow(new string[] { "환급결의번호", refundApprovalNo });
			}
			tableContents.WriteRow(new string[] { "수입자 사업자(주민)등록번호", messageData.ImporterID });
			tableContents.WriteRow(new string[] { "수입자 상호", messageData.ImporterCompanyName });
			tableContents.WriteRow(new string[] { "수입자 성명", messageData.ImporterRepresentativeName });
			tableContents.WriteRow(new string[] { "수입자 주소", messageData.ImporterAddressLine });
			tableContents.WriteRow(new string[] { "납부일자", messageData.PaymentDate.ToString(DateFormatType.DateYYKorean) });
			tableContents.WriteRow(new string[] { "공란수", messageData.BlankCount });
			tableContents.WriteRow(new string[] { "과세표준", messageData.CustomsValue.ToString() });
			tableContents.WriteRow(new string[] { "세액", messageData.Tax.ToString() });
			tableContents.WriteRow(new string[] { "세금계산서 정정일자", messageData.AmendDate.ToString(DateFormatType.DateKorean) });
			tableContents.WriteRow(new string[] { "세금계산서 교부사유", "[" + messageData.IssueReasonCode + "] " + new IssueReasonCodeList().GetDescriptionFromCode(messageData.IssueReasonCode) });
			tableContents.WriteRow(new string[] { "세금계산서 유형", "[" + messageData.TaxInvoiceType + "] " + new TaxInvoiceTypeList().GetDescriptionFromCode(messageData.TaxInvoiceType) });
			tableContents.WriteRow(new string[] { "환급사유 구분", "[" + messageData.RefundType + "] " + new RefundTransactionNatureCodeList().GetDescriptionFromCode(messageData.RefundType) });
			tableContents.WriteRow(new string[] { "재발행여부", messageData.ReIssueYN });
			result.Append(tableContents.ToHtml());

			return result.ToString();
		}
		#endregion
	}
}
