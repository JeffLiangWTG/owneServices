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
	[MessageType(ElectronicDocumentTypeList.Codes._RCJ)]
	class GOVCBRRCJProcessor : IMessageProcessor
	{
		public void Process(EDIMessage message)
		{
			using (var textReader = message.GetEM_MessageTextReader())
			{
				var messageData = new GOVCBRRCJDataProvider().GetMessageData(textReader);
				var entry = MessageLinkedObjectManager.GetLinkedObject(message.Factory, message.Company, messageData.ImportDeclarationNumber, SharedJobMessageTypeList.Codes.Import);
				if (entry != null)
				{
					message.EM_LinkedObject = entry;

					if (messageData.SuspendedType == SuspendedTypeList.Codes._1 || messageData.SuspendedType == SuspendedTypeList.Codes._3)
					{
						entry.CH_EntryStatus = CustomsEntryStatusTypeList.Codes.SUP;
					}
					else if (messageData.SuspendedType == SuspendedTypeList.Codes._2 || messageData.SuspendedType == SuspendedTypeList.Codes._4)
					{
						entry.CH_EntryStatus = CustomsEntryStatusTypeList.Codes.CCL;
					}
				}

				message.EM_MessageInterpretation = CreateUserFriendlyMessageInterpretation(message, messageData);
				SendNotification(message, entry, messageData);
			}
		}

		#region SuppressResourceStringsCheckRegion

		void SendNotification(EDIMessage message, CusEntryHeader entry, IGOVCBRRCJMessageData messageData)
		{
			var declaration = entry?.Declaration;
			var notificationData = new DeclarationEntryNotificationData()
			{
				Entry = entry,
				Branch = declaration?.Branch ?? message.Branch,
				EmailGroup = KRCustomsRegistry.Instance.ImportEmailGroup,
				EmailBody = CreateUserFriendlyMessageInterpretation(message, messageData),
				OriginalMessageTypes = new ZString[] { ElectronicDocumentTypeList.Codes._929 },
				MessageTypeDescription = $"[{ElectronicDocumentTypeList.Descriptions._RCJ}]",
				EntryNumber = messageData.ImportDeclarationNumber
			};
			NotificationSender.SendNotification(notificationData);
		}

		string CreateUserFriendlyMessageInterpretation(EDIMessage message, IGOVCBRRCJMessageData messageData)
		{
			var result = new ZStringBuilder();

			var tableContents = new HtmlTableCreator(System.Array.Empty<string>());
			tableContents.WriteRowWithFormatting(new CellWithFormatting("항 목", new NameValueCollection { { "width", "200" } }, true), new CellWithFormatting("내 용", new NameValueCollection { { "width", "300" } }, true));
			tableContents.WriteRow(new string[] { "제출문서", "수입신고서" });
			tableContents.WriteRow(new string[] { "통보일시", messageData.NoticeDateTime.ToString(DateFormatType.DateTimeKorean) });
			tableContents.WriteRow(new string[] { "통관보류번호", messageData.SuspendedNumber });
			tableContents.WriteRow(new string[] { "수입신고번호", MessageFunctions.DeclarationNumberFormat(messageData.ImportDeclarationNumber) });
			tableContents.WriteRow(new string[] { "상위수입신고번호", MessageFunctions.DeclarationNumberFormat(messageData.OriginalDeclarationNumber) });
			tableContents.WriteRow(new string[] { "수입통관보류종류", "[" + messageData.SuspendedType + "] " + new SuspendedTypeList().GetDescriptionFromCode(messageData.SuspendedType) });
			tableContents.WriteRow(new string[] { "통관보류상세내역", messageData.SuspendedReason });
			tableContents.WriteRow(new string[] { "신고일자", messageData.DeclarationDate.ToString(DateFormatType.DateKorean) });
			tableContents.WriteRow(new string[] { "통관보류일자", messageData.SuspendedDate.ToString(DateFormatType.DateKorean) });
			tableContents.WriteRow(new string[] { "통관보류사유", "[" + messageData.SuspendedCode + "] " + new SuspendedCodeList().GetDescriptionFromCode(messageData.SuspendedCode) });
			tableContents.WriteRow(new string[] { "통관보류조치", messageData.SuspendedSolutionCode.IsEmpty ? string.Empty : "[" + messageData.SuspendedSolutionCode + "] " + new SuspendedSolutionCodeList().GetDescriptionFromCode(messageData.SuspendedSolutionCode) });
			tableContents.WriteRow(new string[] { "신고자", messageData.DeclarantCompanyName });
			tableContents.WriteRow(new string[] { "납세의무자 상호", messageData.ImportCompanyName });
			tableContents.WriteRow(new string[] { "납세의무자 성명", messageData.ImportRepresentativeName });
			tableContents.WriteRow(new string[] { "처리세관(부서)", "[" + messageData.DeclarationOffice + "] " + MessageFunctions.GetCustomsOfficeAndDivision(message.Factory, messageData.DeclarationOffice) });
			tableContents.WriteRow(new string[] { "담당자", messageData.CustomsManagerName });
			tableContents.WriteRow(new string[] { "담당자 연락처", messageData.CustomsManagerPhoneNumber });
			tableContents.WriteRow(new string[] { "담당자 팩스번호", messageData.CustomsPersonFaxNumber });
			result.Append(tableContents.ToHtml());

			if (messageData.Consignment != null)
			{
				var detailContents = new HtmlTableCreator(System.Array.Empty<string>()) { EnableHTMLEncoding = false };
				detailContents.WriteRowWithFormatting(new CellWithFormatting("보류품목내역", new NameValueCollection { { "align", "left" }, { "colspan", "4" } }, true));
				detailContents.WriteRowWithFormatting(new CellWithFormatting("란번호", new NameValueCollection { { "align", "center" }, { "width", "100" } }), new CellWithFormatting("품목분류번호", new NameValueCollection { { "align", "center" }, { "width", "100" } }),
													  new CellWithFormatting("거래품명", new NameValueCollection { { "align", "center" }, { "width", "150" } }), new CellWithFormatting("상표명", new NameValueCollection { { "align", "center" }, { "width", "150" } }));
				foreach (var consignment in messageData.Consignment)
				{
					detailContents.WriteRowWithFormatting(new CellWithFormatting(consignment.EntryLineNo.ToString()),
																  new CellWithFormatting(consignment.HSCode),
																  new CellWithFormatting(consignment.InvoiceDescription),
																  new CellWithFormatting(consignment.BrandName));
				}
				result.Append(detailContents.ToHtml());
			}

			return result.ToString();
		}

		#endregion
	}
}
