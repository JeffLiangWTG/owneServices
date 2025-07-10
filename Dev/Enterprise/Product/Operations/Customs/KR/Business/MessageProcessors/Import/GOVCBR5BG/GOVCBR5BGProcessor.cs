using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.Common.Shared;
using Enterprise.Customs.KR.Messaging;
using Enterprise.Messaging.MessageProcessors;
using WTG.StaticAnalysis.Annotation;
using static Enterprise.Customs.KR.Messaging.Constants;

namespace Enterprise.Customs.KR.Business
{
	[CodeAlive("An instance is created via reflection using the attribute MessageType")]
	[MessageType(ElectronicDocumentTypeList.Codes._5BG)]
	class GOVCBR5BGProcessor : IMessageProcessor
	{
		public void Process(EDIMessage message)
		{
			using (var textReader = message.GetEM_MessageTextReader())
			{
				var messageData = new GOVCBR5BGDataProvider().GetMessageData(textReader);
				var entry = MessageLinkedObjectManager.GetLinkedObject(message.Factory, message.Company, messageData.ImportDeclarationNumber, SharedJobMessageTypeList.Codes.Import);
				if (entry != null)
				{
					message.EM_LinkedObject = entry;

					if (messageData.ResultType == ResultTypeList.Codes.C)
					{
						entry.CH_EntryStatus = CustomsEntryStatusTypeList.Codes.CCL;
						entry.EntryNumbers.UpdateCusEntryNumIfExists(SharedJobMessageTypeList.Codes.Import, CusEntryNumber.Schema.CE_ExpiryDate, messageData.ApprovalDateTime);
						entry.CH_Status = CustomsMessageStatusTypeList.Codes.CancellationApprovedByCustoms;
						message.EM_MessageOwner = CustomsEntryStatusTypeList.Codes.ANT;
					}
					else if (messageData.ResultType == ResultTypeList.Codes.E)
					{
						entry.CH_Status = CustomsMessageStatusTypeList.Codes.CancellationDeclined;
						message.EM_MessageOwner = CustomsEntryStatusTypeList.Codes.DMS;
					}
					var originalMessage = entry.Messages?.GetLastMessage(EDIMessage.ApplicationCodes.KRCustoms, ElectronicDocumentTypeList.Codes._5BF);
					message.EM_ApplicationReference = originalMessage?.EM_MessageNum ?? ZString.Empty;
				}

				message.EM_MessageSubType = ElectronicDocumentTypeList.Codes._5BF;
				message.EM_MessageInterpretation = CreateUserFriendlyMessageInterpretation(message, messageData);
				SendNotification(message, entry, messageData);
			}
		}

		#region SuppressResourceStringsCheckRegion

		void SendNotification(EDIMessage message, CusEntryHeader entry, IGOVCBR5BGMessageData messageData)
		{
			var declaration = entry?.Declaration;
			var notificationData = new DeclarationEntryNotificationData()
			{
				Entry = entry,
				Branch = declaration?.Branch ?? message.Branch,
				EmailGroup = KRCustomsRegistry.Instance.ImportEmailGroup,
				EmailBody = CreateUserFriendlyMessageInterpretation(message, messageData),
				OriginalMessageTypes = new ZString[] { ElectronicDocumentTypeList.Codes._5BF },
				MessageTypeDescription = $"[{ElectronicDocumentTypeList.Descriptions._5BG}]",
				EntryNumber = messageData.ImportDeclarationNumber
			};
			NotificationSender.SendNotification(notificationData);
		}

		string CreateUserFriendlyMessageInterpretation(EDIMessage message, IGOVCBR5BGMessageData messageData)
		{
			var tableContents = new HtmlTableCreator(new string[] { "항 목", "내 용" });
			tableContents.WriteRow(new string[] { "제출문서", "수입신고서" });
			tableContents.WriteRow(new string[] { "신고번호", MessageFunctions.DeclarationNumberFormat(messageData.ImportDeclarationNumber) });
			tableContents.WriteRow(new string[] { "승인(기각)일자", messageData.ApprovalDateTime.ToString(DateFormatType.DateTimeKoreanNoSecond) });
			tableContents.WriteRow(new string[] { "처리결과부호", messageData.ResultType + " " + new ResultTypeList().GetDescriptionFromCode(messageData.ResultType) ?? ZString.Empty });
			tableContents.WriteRow(new string[] { "귀책사유부호", messageData.FaultParty + " " + new FaultPartyList().GetDescriptionFromCode(messageData.FaultParty) ?? ZString.Empty });
			tableContents.WriteRow(new string[] { "취하사유부호", messageData.ReasonCode + " " + new ReasonCodeList().GetDescriptionFromCode(messageData.ReasonCode) ?? ZString.Empty });
			tableContents.WriteRow(new string[] { "처리세관(과)부호", "[" + messageData.DeclarationOffice + "] " + MessageFunctions.GetCustomsOfficeAndDivision(message.Factory, messageData.DeclarationOffice) });
			tableContents.WriteRow(new string[] { "처리담당자 성명", messageData.CustomsPersonName });
			tableContents.WriteRow(new string[] { "기타사유", messageData.OtherReason });
			tableContents.WriteRow(new string[] { "기각사유", messageData.DismissalReason });

			return tableContents.ToHtml();
		}
		#endregion
	}
}
