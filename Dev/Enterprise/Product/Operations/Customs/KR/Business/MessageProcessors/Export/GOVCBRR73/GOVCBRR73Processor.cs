using CargoWise.Types;
using Enterprise.Customs.Common.Shared;
using Enterprise.Customs.KR.Messaging;
using Enterprise.Messaging.MessageProcessors;
using Enterprise.ZArchitecture.Core;
using WTG.StaticAnalysis.Annotation;
using static Enterprise.Customs.KR.Messaging.Constants;

namespace Enterprise.Customs.KR.Business
{
	[CodeAlive("An instance is created via reflection using the attribute MessageType")]
	[MessageType(ElectronicDocumentTypeList.Codes._R73)]
	class GOVCBRR73Processor : IMessageProcessor
	{
		public void Process(EDIMessage message)
		{
			using (var textReader = message.GetEM_MessageTextReader())
			{
				var messageData = new GOVCBRR73DataProvider().GetMessageData(textReader);
				var entry = MessageLinkedObjectManager.GetLinkedObject(message.Factory, message.Company, messageData.ExportDeclarationNumber, SharedJobMessageTypeList.Codes.Export);
				if (entry != null)
				{
					message.EM_LinkedObject = entry;

					#region CustomsEntryStatus
					ZString[] documentSubmitDescription = { (NoResString)"\uc804\uc790\uc11c\ub958", (NoResString)"\uc804\uc790\uc81c\ucd9c" };
					if (messageData.ResultType == ExportInspectionResultTypeList.Codes.S)
					{
						entry.CH_EntryStatus = CustomsEntryStatusTypeList.Codes.CLF;
					}
					else if (messageData.ResultType == ExportInspectionResultTypeList.Codes.P)
					{
						if (messageData.DocumentSubmitDescription.Contains(documentSubmitDescription[0]) || messageData.DocumentSubmitDescription.Contains(documentSubmitDescription[1]))
						{
							entry.CH_EntryStatus = CustomsEntryStatusTypeList.Codes.NED;
						}
						else
						{
							entry.CH_EntryStatus = CustomsEntryStatusTypeList.Codes.NDC;
						}
					}
					else if (messageData.ResultType == ExportInspectionResultTypeList.Codes.B)
					{
						entry.CH_EntryStatus = CustomsEntryStatusTypeList.Codes.BAE;
					}
					else
					{
						entry.CH_EntryStatus = CustomsEntryStatusTypeList.Codes.LAE;
					}
					#endregion

					if (!messageData.CustomsOfficerID.IsEmpty || !messageData.CustomsOfficerName.IsEmpty)
					{
						entry.CustomsOfficers.CreateOrUpdate(CustomsOfficerTypeList.Codes.ResponsibleCustomsOfficer, messageData.CustomsOfficerID, messageData.CustomsOfficerName, message.EM_MessageDateTime);
					}
				}
				message.EM_MessageInterpretation = CreateUserFriendlyMessageInterpretation(messageData, entry);
				SendNotification(message, entry, messageData);
			}
		}

		#region SuppressResourceStringsCheckRegion

		void SendNotification(EDIMessage message, CusEntryHeader entry, IGOVCBRR73MessageData messageData)
		{
			var declaration = entry?.Declaration;
			var notificationData = new DeclarationEntryNotificationData()
			{
				Entry = entry,
				Branch = declaration?.Branch ?? message.Branch,
				EmailGroup = KRCustomsRegistry.Instance.ExportEmailGroup,
				EmailBody = CreateUserFriendlyMessageInterpretation(messageData, entry),
				OriginalMessageTypes = new ZString[] { ElectronicDocumentTypeList.Codes._830 },
				MessageTypeDescription = $"[{ElectronicDocumentTypeList.Descriptions._R73}]",
				EntryNumber = messageData.ExportDeclarationNumber
			};
			NotificationSender.SendNotification(notificationData);
		}

		string CreateUserFriendlyMessageInterpretation(IGOVCBRR73MessageData messageData, CusEntryHeader entry)
		{
			var result = new ZStringBuilder();
			var tableContents = new HtmlTableCreator(new string[] { "항 목", "내 용" });
			tableContents.WriteRow(new string[] { "선별결과구분", messageData.ResultType });
			tableContents.WriteRow(new string[] { "제출문서", "수출신고서" });
			tableContents.WriteRow(new string[] { "선별결과 통보일시", messageData.NoticeDateTime.ToString(DateFormatType.DateTimeKorean) });
			tableContents.WriteRow(new string[] { "수출신고번호", MessageFunctions.DeclarationNumberFormat(messageData.ExportDeclarationNumber) });
			var statusTypeList = new CustomsEntryStatusTypeList();
			tableContents.WriteRow(new string[] { "선별결과", statusTypeList.GetDescriptionFromCode(entry?.CH_EntryStatus ?? ZString.Empty) });
			tableContents.WriteRow(new string[] { "서류제출 내용", messageData.DocumentSubmitDescription });
			var officerID = messageData.CustomsOfficerID.IsEmpty ? string.Empty : "[" + messageData.CustomsOfficerID + "] ";
			tableContents.WriteRow(new string[] { "세관 담당자", officerID + messageData.CustomsOfficerName });
			result.Append(tableContents.ToHtml());

			return result.ToString();
		}
		#endregion
	}
}
