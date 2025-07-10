using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.KR.Messaging;
using Enterprise.Messaging.MessageProcessors;
using WTG.StaticAnalysis.Annotation;
using static Enterprise.Customs.KR.Messaging.Constants;

namespace Enterprise.Customs.KR.Business
{
	[CodeAlive("An instance is created via reflection using the attribute MessageType")]
	[MessageType(ElectronicDocumentTypeList.Codes._RCA)]
	class GOVCBRRCAProcessor : IMessageProcessor
	{
		public void Process(EDIMessage message)
		{
			using (var textReader = message.GetEM_MessageTextReader())
			{
				var messageData = new GOVCBRRCADataProvider().GetMessageData(textReader);
				var (entryNum5UL, entry, refundDeclaration, outgoingMessage) = MessageLinkedObjectManager.GetLinkedRefundObject(message.Factory, message.Company, messageData.RefundDeclarationNumber, ElectronicDocumentTypeList.Codes._5UL);

				var status = CustomsEntryStatusTypeList.Codes.PFL;
				if (messageData.NoticeType == TransactionNatureCodeList.Codes.B)
				{
					status = CustomsEntryStatusTypeList.Codes.NDC;
				}
				else if (messageData.NoticeType == TransactionNatureCodeList.Codes.C)
				{
					status = CustomsEntryStatusTypeList.Codes.DMS;
				}
				else if (messageData.NoticeType == TransactionNatureCodeList.Codes.D)
				{
					status = CustomsEntryStatusTypeList.Codes.ANT;
				}
				else if (messageData.NoticeType == TransactionNatureCodeList.Codes.E)
				{
					status = CustomsEntryStatusTypeList.Codes.PNR;
				}

				if (entry != null)
				{
					if (status == CustomsEntryStatusTypeList.Codes.DMS)
					{
						entry.MarkLodgedSnapshotAsDeleted(ElectronicDocumentTypeList.Codes._5UL);
					}
					message.EM_LinkedObject = entry;
					var refundSessionalData = entry.EntryInstruction?.RefundSessionalDataCollection.Cast<RefundSessionalData>().FirstOrDefault(x => x.CSI_ReferenceNumber == messageData.RefundDeclarationNumber);
					if (refundSessionalData != null)
					{
						refundSessionalData.CSI_Status = status;
					}
				}
				else if (refundDeclaration != null)
				{
					if (status == CustomsEntryStatusTypeList.Codes.DMS)
					{
						refundDeclaration.CRD_MessageStatus = CustomsMessageStatusTypeList.Codes.CancellationByCustoms;
					}
					message.EM_LinkedObject = refundDeclaration;
					refundDeclaration.CRD_CustomsStatus = status;
				}

				message.EM_MessageOwner = status;
				message.EM_ApplicationReference = outgoingMessage?.EM_MessageNum ?? ZString.Empty;
				message.EM_MessageInterpretation = CreateUserFriendlyMessageInterpretation(message, messageData);
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
		void SendNotification(EDIMessage message, CusEntryHeader entry, IGOVCBRRCAMessageData messageData)
		{
			var declaration = entry?.Declaration;
			var notificationData = new DeclarationEntryNotificationData()
			{
				Entry = entry,
				Branch = declaration?.Branch ?? message.Branch,
				EmailGroup = KRCustomsRegistry.Instance.ImportEmailGroup,
				EmailBody = CreateUserFriendlyMessageInterpretation(message, messageData),
				OriginalMessageTypes = new ZString[] { ElectronicDocumentTypeList.Codes._5UL },
				MessageTypeDescription = $"[{ElectronicDocumentTypeList.Descriptions._RCA}]",
				EntryNumber = messageData.RefundDeclarationNumber
			};
			NotificationSender.SendNotification(notificationData);
		}

		void SendNotificationReconDeclaration(EDIMessage message, CusReconDeclaration reconDeclaration, IGOVCBRRCAMessageData messageData)
		{
			var notificationData = NotificationSender.GetReconDeclarationNotificationData(reconDeclaration, messageData.RefundDeclarationNumber);
			notificationData.EmailBody = CreateUserFriendlyMessageInterpretation(message, messageData);
			notificationData.MessageTypeDescription = $"[{ElectronicDocumentTypeList.Descriptions._RCA}]";
			NotificationSender.SendNotification(notificationData);
		}

		string CreateUserFriendlyMessageInterpretation(EDIMessage message, IGOVCBRRCAMessageData messageData)
		{
			var result = new ZStringBuilder();

			var tableContents = new HtmlTableCreator(new string[] { "항 목", "내 용" });
			tableContents.WriteRow(new string[] { "제출문서", "과오납 및 계약상이 환급신청서" });
			tableContents.WriteRow(new string[] { "환급신청번호", MessageFunctions.DeclarationNumberFormat(messageData.RefundDeclarationNumber) });
			tableContents.WriteRow(new string[] { "처리일시", messageData.NoticeDateTime.ToString(DateFormatType.DateTimeKorean) });
			tableContents.WriteRow(new string[] { "통보구분", "[" + messageData.NoticeType + "] " + new TransactionNatureCodeList().GetDescriptionFromCode(messageData.NoticeType) ?? ZString.Empty });
			tableContents.WriteRow(new string[] { "세관담당자명", messageData.CustomsPersonName });
			tableContents.WriteRow(new string[] { "처리 세관(과)", "[" + messageData.DeclarationOffice + "] " + MessageFunctions.GetCustomsOfficeAndDivision(message.Factory, messageData.DeclarationOffice) });
			tableContents.WriteRow(new string[] { "처리결과", messageData.ResultReason });
			tableContents.WriteRow(new string[] { "특이사항", messageData.ContentDescription + " " + messageData.ContentDescription2 });
			result.Append(tableContents.ToHtml());

			return result.ToString();
		}
		#endregion
	}
}
