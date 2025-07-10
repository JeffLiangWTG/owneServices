using CargoWise.Types;
using Enterprise.Customs.Common.Shared;
using Enterprise.Customs.KR.Messaging;
using Enterprise.Messaging.MessageProcessors;
using WTG.StaticAnalysis.Annotation;
using static Enterprise.Customs.KR.Messaging.Constants;

namespace Enterprise.Customs.KR.Business
{
	[CodeAlive("An instance is created via reflection using the attribute MessageType")]
	[MessageType(ElectronicDocumentTypeList.Codes._R97)]
	public class GOVCBRR97Processor : IMessageProcessor
	{
		public void Process(EDIMessage message)
		{
			using (var textReader = message.GetEM_MessageTextReader())
			{
				var messageData = new GOVCBRR97DataProvider().GetMessageData(textReader);
				var entry = MessageLinkedObjectManager.GetLinkedObject(message.Factory, message.Company, messageData.ExportDeclarationNumber, SharedJobMessageTypeList.Codes.Export);

				if (entry != null)
				{
					message.EM_LinkedObject = entry;

					if (messageData.AmendType == Exportdeclaration.Correction)
					{
						entry.CH_EntryStatus = CustomsEntryStatusTypeList.Codes.ARS;
					}
					else if (messageData.AmendType == Exportdeclaration.Drop)
					{
						entry.CH_EntryStatus = CustomsEntryStatusTypeList.Codes.CCL;
					}
					else if (messageData.AmendType == Exportdeclaration.Excellency)
					{
						entry.CH_EntryStatus = CustomsEntryStatusTypeList.Codes.RJC;
					}
				}
				message.EM_MessageInterpretation = CreateUserFriendlyMessageInterpretation(message, entry, messageData);
				SendNotification(message, entry, messageData);
			}
		}

		#region SuppressResourceStringsCheckRegion
		void SendNotification(EDIMessage message, CusEntryHeader entry, IGOVCBRR97MessageData messageData)
		{
			var declaration = entry?.Declaration;
			var notificationData = new DeclarationEntryNotificationData()
			{
				Entry = entry,
				Branch = declaration?.Branch ?? message.Branch,
				EmailGroup = KRCustomsRegistry.Instance.ExportEmailGroup,
				EmailBody = CreateUserFriendlyMessageInterpretation(message, entry, messageData),
				OriginalMessageTypes = new ZString[] { ElectronicDocumentTypeList.Codes._830 },
				MessageTypeDescription = $"[{ElectronicDocumentTypeList.Descriptions._R97}]",
				EntryNumber = messageData.ExportDeclarationNumber
			};
			NotificationSender.SendNotification(notificationData);
		}

		ZString CreateUserFriendlyMessageInterpretation(EDIMessage message, CusEntryHeader entry, IGOVCBRR97MessageData messageData)
		{
			var result = new ZStringBuilder();

			var tableContents = new HtmlTableCreator(new string[] { "항 목", "내 용" });
			tableContents.WriteRow(new string[] { "신청문서구분", messageData.DeclarationType });
			tableContents.WriteRow(new string[] { "제출문서", "수출신고수리 정정/취하 승인(신청)서" });
			tableContents.WriteRow(new string[] { "정정일자", messageData.AmendDate.ToString(DateFormatType.DateKorean) });
			tableContents.WriteRow(new string[] { "수출신고번호", MessageFunctions.DeclarationNumberFormat(messageData.ExportDeclarationNumber) });
			tableContents.WriteRow(new string[] { "정정구분", new CustomsEntryStatusTypeList().GetDescriptionFromCode(entry?.CH_EntryStatus ?? ZString.Empty) });
			tableContents.WriteRow(new string[] { "귀책사유", new ExportImputationReasonCodeList().GetDescriptionFromCode(messageData.FaultParty) });
			tableContents.WriteRow(new string[] { "세관(과)", "[" + messageData.CustomsOfficeAndDivision + "] " + MessageFunctions.GetCustomsOfficeAndDivision(message.Factory, messageData.CustomsOfficeAndDivision) });
			tableContents.WriteRow(new string[] { "세관 담당자명", messageData.CustomsPersonName });

			if (entry != null)
			{
				if (entry.CH_EntryStatus == CustomsEntryStatusTypeList.Codes.ARS)
				{
					tableContents.WriteRow(new string[] { "사유코드", new ExportAmendmentReasonCodeList().GetDescriptionFromCode(messageData.ReasonCode) });
				}
				else if (entry.CH_EntryStatus == CustomsEntryStatusTypeList.Codes.CCL)
				{
					tableContents.WriteRow(new string[] { "사유코드", new ExportDeclarationwithdrawReasonCodeList().GetDescriptionFromCode(messageData.ReasonCode) });
				}
				else if (entry.CH_EntryStatus == CustomsEntryStatusTypeList.Codes.RJC)
				{
					tableContents.WriteRow(new string[] { "사유코드", new ExportDeclarationRejectReasonCodeList().GetDescriptionFromCode(messageData.ReasonCode) });
				}
			}

			tableContents.WriteRow(new string[] { "정정/취하/각하사유", messageData.AmendReasonDescription });
			tableContents.WriteRow(new string[] { "수출화주", messageData.SupplierCompanyName });
			result.Append(tableContents.ToHtml());

			return result.ToString();
		}
		#endregion
	}
}
