using CargoWise.Types;
using Enterprise.Customs.Common.Shared;
using Enterprise.Customs.KR.Messaging;
using Enterprise.Messaging.MessageProcessors;
using WTG.StaticAnalysis.Annotation;
using static Enterprise.Customs.KR.Messaging.Constants;

namespace Enterprise.Customs.KR.Business
{
	[CodeAlive("An instance is created via reflection using the attribute MessageType")]
	[MessageType(ElectronicDocumentTypeList.Codes._5BE)]
	class GOVCBR5BEProcessor : IMessageProcessor
	{
		public void Process(EDIMessage message)
		{
			using (var textReader = message.GetEM_MessageTextReader())
			{
				var messageData = new GOVCBR5BEDataProvider().GetMessageData(message.Factory, textReader);
				var entry = MessageLinkedObjectManager.GetLinkedObject(message.Factory, message.Company, messageData.ImportDeclarationNumber, SharedJobMessageTypeList.Codes.Import);
				if (entry != null)
				{
					message.EM_LinkedObject = entry;

					var originalMessage = entry.Messages.GetLastMessage(EDIMessage.ApplicationCodes.KRCustoms, ElectronicDocumentTypeList.Codes._5BD);
					message.EM_ApplicationReference = originalMessage?.EM_MessageNum ?? ZString.Empty;

					if (messageData.ResultType == ProcessingResultCodeList.Codes.C || messageData.ResultType == ProcessingResultCodeList.Codes.D)
					{
						message.EM_MessageOwner = CustomsEntryStatusTypeList.Codes.ANT;
					}
					else if (messageData.ResultType == ProcessingResultCodeList.Codes.E || messageData.ResultType == ProcessingResultCodeList.Codes.F)
					{
						message.EM_MessageOwner = CustomsEntryStatusTypeList.Codes.DMS;
					}
					else if (messageData.ResultType == ProcessingResultCodeList.Codes.G || messageData.ResultType == ProcessingResultCodeList.Codes.H)
					{
						message.EM_MessageOwner = CustomsEntryStatusTypeList.Codes.DMS;
					}
				}
				message.EM_MessageInterpretation = CreateUserFriendlyMessageInterpretation(message, messageData);
				message.EM_MessageSubType = ElectronicDocumentTypeList.Codes._5BD;
				SendNotification(message, entry, messageData);
			}
		}

		#region SuppressResourceStringsCheckRegion
		void SendNotification(EDIMessage message, CusEntryHeader entry, IGOVCBR5BEMessageData messageData)
		{
			var declaration = entry?.Declaration;
			var notificationData = new DeclarationEntryNotificationData()
			{
				Entry = entry,
				Branch = declaration?.Branch ?? message.Branch,
				EmailGroup = KRCustomsRegistry.Instance.ImportEmailGroup,
				EmailBody = CreateUserFriendlyMessageInterpretation(message, messageData),
				OriginalMessageTypes = new ZString[] { ElectronicDocumentTypeList.Codes._5BD },
				MessageTypeDescription = $"[{ElectronicDocumentTypeList.Descriptions._5BE}]",
				EntryNumber = messageData.ImportDeclarationNumber
			};
			NotificationSender.SendNotification(notificationData);
		}

		string CreateUserFriendlyMessageInterpretation(EDIMessage message, IGOVCBR5BEMessageData messageData)
		{
			var result = new ZStringBuilder();

			var tableContents = new HtmlTableCreator(new string[] { "항 목", "내 용" });
			tableContents.WriteRow(new string[] { "제출문서", "수입신고 수리전 반출 신고서" });
			tableContents.WriteRow(new string[] { "수입신고번호", MessageFunctions.DeclarationNumberFormat(messageData.ImportDeclarationNumber) });
			tableContents.WriteRow(new string[] { "처리일자", messageData.NoticeDateTime.ToString(DateFormatType.DateTimeKoreanNoSecond) });
			tableContents.WriteRow(new string[] { "납부기한", messageData.PaymentDate.ToString(DateFormatType.DateKorean) });
			var processingResultCodeList = new ProcessingResultCodeList();
			tableContents.WriteRow(new string[] { "처리결과", messageData.ResultType + " " + processingResultCodeList.GetDescriptionFromCode(messageData.ResultType) });
			if (messageData.ResultType == ProcessingResultCodeList.Codes.C || messageData.ResultCode == ProcessingResultCodeList.Codes.D)
			{
				var processingReasonCodeList = new ProcessingReasonCodeList_1();
				tableContents.WriteRow(new string[] { "처리 사유코드", messageData.ResultCode + " " + processingReasonCodeList.GetDescriptionFromCode(messageData.ResultCode) ?? ZString.Empty });
			}
			else if (messageData.ResultType == ProcessingResultCodeList.Codes.G || messageData.ResultCode == ProcessingResultCodeList.Codes.H)
			{
				var processingReasonCodeList = new ProcessingReasonCodeList_2();
				tableContents.WriteRow(new string[] { "처리 사유코드", messageData.ResultCode + " " + processingReasonCodeList.GetDescriptionFromCode(messageData.ResultCode) ?? ZString.Empty });
			}
			else
			{
				tableContents.WriteRow(new string[] { "처리 사유코드", messageData.ResultCode });
			}
			tableContents.WriteRow(new string[] { "처리 상세사유", messageData.ResultReason });
			var approvalCodeList = new ApprovalCodeList();
			tableContents.WriteRow(new string[] { "승인요건", messageData.ApprovalCode + " " + approvalCodeList.GetDescriptionFromCode(messageData.ApprovalCode) ?? ZString.Empty });
			var paymentMethodCodeList = new PaymentMethodCodeList();
			tableContents.WriteRow(new string[] { "변경후 징수형태", messageData.PaymentType.IsEmpty ? string.Empty : "[" + messageData.PaymentType + "] " + paymentMethodCodeList.GetDescriptionFromCode(messageData.PaymentType) ?? ZString.Empty });
			var declarationProcedureTypeCodeList = new DeclarationProcedureTypeCodeList();
			tableContents.WriteRow(new string[] { "변경후 수입종류", messageData.DeclarationProcedureType.IsEmpty ? string.Empty : "[" + messageData.DeclarationProcedureType + "] " + declarationProcedureTypeCodeList.GetDescriptionFromCode(messageData.DeclarationProcedureType) ?? ZString.Empty });
			tableContents.WriteRow(new string[] { "담당자명", messageData.CustomsManagerName });
			tableContents.WriteRow(new string[] { "처리세관(과)부호", "[" + messageData.DeclarationOffice + "] " + MessageFunctions.GetCustomsOfficeAndDivision(message.Factory, messageData.DeclarationOffice) });
			result.Append(tableContents.ToHtml());

			return result.ToString();
		}
		#endregion
	}
}
