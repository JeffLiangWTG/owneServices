using System.Collections.Specialized;
using CargoWise.Types;
using Enterprise.Customs.KR.Messaging;
using Enterprise.Messaging.MessageProcessors;
using WTG.StaticAnalysis.Annotation;
using static Enterprise.Customs.KR.Messaging.Constants;

namespace Enterprise.Customs.KR.Business
{
	[CodeAlive("An instance is created via reflection using the attribute MessageType")]
	[MessageType(ElectronicDocumentTypeList.Codes._5UN)]
	class GOVCBR5UNProcessor : IMessageProcessor
	{
		public void Process(EDIMessage message)
		{
			using (var textReader = message.GetEM_MessageTextReader())
			{
				var messageData = new GOVCBR5UNDataProvider().GetMessageData(message.Factory, textReader);
				var (entryNum5UL, entry, refundDeclaration, outgoingMessage) = MessageLinkedObjectManager.GetLinkedRefundObject(message.Factory, message.Company, messageData.RefundDeclarationNumber, ElectronicDocumentTypeList.Codes._5UL);
				if (entry != null)
				{
					message.EM_LinkedObject = entry;
				}
				else if (refundDeclaration != null)
				{
					message.EM_LinkedObject = refundDeclaration;
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

		void SendNotification(EDIMessage message, CusEntryHeader entry, IGOVCBR5UNMessageData messageData)
		{
			var declaration = entry?.Declaration;
			var notificationData = new DeclarationEntryNotificationData()
			{
				Entry = entry,
				Branch = declaration?.Branch ?? message.Branch,
				EmailGroup = KRCustomsRegistry.Instance.ImportEmailGroup,
				EmailBody = CreateUserFriendlyMessageInterpretation(messageData),
				OriginalMessageTypes = new ZString[] { ElectronicDocumentTypeList.Codes._5UL },
				MessageTypeDescription = $"[{ElectronicDocumentTypeList.Descriptions._5UN}]",
				EntryNumber = messageData.RefundDeclarationNumber
			};
			NotificationSender.SendNotification(notificationData);
		}

		void SendNotificationReconDeclaration(EDIMessage message, CusReconDeclaration reconDeclaration, IGOVCBR5UNMessageData messageData)
		{
			var notificationData = NotificationSender.GetReconDeclarationNotificationData(reconDeclaration, messageData.RefundDeclarationNumber);
			notificationData.EmailBody = CreateUserFriendlyMessageInterpretation(messageData);
			notificationData.MessageTypeDescription = $"[{ElectronicDocumentTypeList.Descriptions._5UN}]";
			NotificationSender.SendNotification(notificationData);
		}

		string CreateUserFriendlyMessageInterpretation(IGOVCBR5UNMessageData messageData)
		{
			var result = new ZStringBuilder();

			var tableContents = new HtmlTableCreator(System.Array.Empty<string>());
			tableContents.WriteRowWithFormatting(new CellWithFormatting("항 목", new NameValueCollection { { "width", "200" } }, true), new CellWithFormatting("내 용", new NameValueCollection { { "width", "300" } }, true));
			tableContents.WriteRow(new string[] { "제출문서", "과오납 환급 신청서" });
			tableContents.WriteRow(new string[] { "환급신청번호", messageData.RefundDeclarationNumber });
			tableContents.WriteRow(new string[] { "업체 상호", messageData.ImportCompanyName });
			tableContents.WriteRow(new string[] { "업체 대표자성명", messageData.ImportRepresentativeName });
			tableContents.WriteRow(new string[] { "기본주소", messageData.ImportAddressLine1 });
			tableContents.WriteRow(new string[] { "상세주소", messageData.ImportAddressLine2 });
			tableContents.WriteRow(new string[] { "충당세관명", messageData.CustomsOfficeName });
			tableContents.WriteRow(new string[] { "충당과명", messageData.CustomsDivisionName });
			tableContents.WriteRow(new string[] { "환급신청일자", messageData.SubmissionDate.ToString(DateFormatType.DateKorean) });
			tableContents.WriteRow(new string[] { "환급결의일자", messageData.ApprovalDate.ToString(DateFormatType.DateKorean) });
			tableContents.WriteRow(new string[] { "충당일자", messageData.PaymentDate.ToString(DateFormatType.DateKorean) });
			tableContents.WriteRow(new string[] { "환급결의번호", messageData.ApprovalNo });
			tableContents.WriteRow(new string[] { "국고계좌번호", messageData.BankAccountNumber });
			tableContents.WriteRow(new string[] { "충당고지번호", messageData.NoticeNumber });
			result.Append(tableContents.ToHtml());

			if (messageData.DutyTax != null)
			{
				var detailContents = new HtmlTableCreator(System.Array.Empty<string>()) { EnableHTMLEncoding = false };
				detailContents.WriteRowWithFormatting(new CellWithFormatting("세액 내용", new NameValueCollection { { "align", "left" }, { "colspan", "4" } }, true));

				detailContents.WriteRowWithFormatting(new CellWithFormatting("세액구분", new NameValueCollection { { "align", "center" }, { "width", "100" } }), new CellWithFormatting("결정액", new NameValueCollection { { "align", "center" }, { "width", "100" } }), new CellWithFormatting("충당액", new NameValueCollection { { "align", "center" }, { "width", "100" } }), new CellWithFormatting("잔액", new NameValueCollection { { "align", "center" }, { "width", "100" } }));

				var taxTypeList = new EntryTaxTypeList();
				foreach (var duty in messageData.DutyTax)
				{
					var taxType = duty.Key;
					if (taxType != EntryTaxTypeList.Codes._5CZ)
					{
						detailContents.WriteRowWithFormatting(new CellWithFormatting(taxTypeList.GetDescriptionFromCode(taxType), new NameValueCollection { { "align", "left" } }), new CellWithFormatting(duty.Value.OriginalAmount.ToString(), new NameValueCollection { { "align", "right" } }), new CellWithFormatting(duty.Value.SupplementaryAmount.ToString(), new NameValueCollection { { "align", "right" } }), new CellWithFormatting(duty.Value.DifferenceAmount.ToString(), new NameValueCollection { { "align", "right" } }));
					}
				}

				IDutyTaxFee total;
				if (messageData.DutyTax.TryGetValue(EntryTaxTypeList.Codes._5CZ, out total))
				{
					var endCell1 = new CellWithFormatting(taxTypeList.GetDescriptionFromCode(total.DutyTaxType), new NameValueCollection { { "align", "left" } });
					var endCell2 = new CellWithFormatting(total.OriginalAmount.ToString(), new NameValueCollection { { "align", "right" } });
					var endCell3 = new CellWithFormatting(total.SupplementaryAmount.ToString(), new NameValueCollection { { "align", "right" } });
					var endCell4 = new CellWithFormatting(total.DifferenceAmount.ToString(), new NameValueCollection { { "align", "right" } });
					detailContents.WriteRowWithFormatting(endCell1, endCell2, endCell3, endCell4);
				}
				result.Append(detailContents.ToHtml());
			}

			return result.ToString();
		}

		#endregion
	}
}
