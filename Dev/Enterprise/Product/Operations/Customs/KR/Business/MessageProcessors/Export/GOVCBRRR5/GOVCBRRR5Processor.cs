using CargoWise.Types;
using Enterprise.Customs.Common.Shared;
using Enterprise.Customs.KR.Messaging;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.MessageProcessors;
using Enterprise.ZArchitecture.Business;
using WTG.StaticAnalysis.Annotation;
using static Enterprise.Customs.KR.Messaging.Constants;

namespace Enterprise.Customs.KR.Business
{
	[CodeAlive("An instance is created via reflection using the attribute MessageType")]
	[MessageType(ElectronicDocumentTypeList.Codes._RR5)]
	public class GOVCBRRR5Processor : IMessageProcessor
	{
		public void Process(EDIMessage message)
		{
			using (var textReader = message.GetEM_MessageTextReader())
			{
				var messageData = new GOVCBRRR5DataProvider().GetMessageData(textReader);
				var entry = MessageLinkedObjectManager.GetLinkedObject(message.Factory, message.Company, messageData.ExportDeclarationNumber, SharedJobMessageTypeList.Codes.Export);
				if (entry != null)
				{
					message.EM_LinkedObject = entry;

					var reasonType = messageData.ChangeReasonType;
					var inspectionType = messageData.InspectionChangeType;
					var noticeDate = messageData.NoticeDateTime.ToOffset();

					using (noticeDate.IsEmpty ? null : entry.SuspendCESLog())
					{
						if (reasonType == ExportChangingProcessReasonCodeList.Codes._02)
						{
							entry.CH_EntryStatus = CustomsEntryStatusTypeList.Codes.PRN;
						}
						else if (reasonType == ExportChangingProcessReasonCodeList.Codes._05)
						{
							if (inspectionType == ExportInspectionChangingNotificationCodeList.Codes._1)
							{
								entry.CH_EntryStatus = CustomsEntryStatusTypeList.Codes.BER;
							}
							else if (inspectionType == ExportInspectionChangingNotificationCodeList.Codes._2)
							{
								entry.CH_EntryStatus = CustomsEntryStatusTypeList.Codes.PER;
							}
							else if (inspectionType == ExportInspectionChangingNotificationCodeList.Codes._3)
							{
								entry.CH_EntryStatus = CustomsEntryStatusTypeList.Codes.EER;
							}
							else if (inspectionType == ExportInspectionChangingNotificationCodeList.Codes._5)
							{
								entry.CH_EntryStatus = CustomsEntryStatusTypeList.Codes.LER;
							}
						}
						else if (reasonType == ExportChangingProcessReasonCodeList.Codes._36)
						{
							entry.CH_EntryStatus = CustomsEntryStatusTypeList.Codes.CCG;
						}
					}

					if (!noticeDate.IsEmpty)
					{
						entry.AddLog(Events.CustomsEntryStatus, entry.CH_EntryStatus, noticeDate);
					}

					if (!messageData.CustomsManagerID.IsEmpty || !messageData.CustomsManagerName.IsEmpty)
					{
						entry.CustomsOfficers.CreateOrUpdate(CustomsOfficerTypeList.Codes.ResponsibleCustomsOfficer, messageData.CustomsManagerID, messageData.CustomsManagerName, message.EM_MessageDateTime);
					}
					else if (!messageData.SubCustomsManagerID.IsEmpty || !messageData.SubCustomsManagerName.IsEmpty)
					{
						entry.CustomsOfficers.CreateOrUpdate(CustomsOfficerTypeList.Codes.ResponsibleCustomsOfficer, messageData.SubCustomsManagerID, messageData.SubCustomsManagerName, message.EM_MessageDateTime);
					}
				}
				message.EM_MessageInterpretation = CreateUserFriendlyMessageInterpretation(messageData);
				SendNotification(message, entry, messageData);
			}
		}

		#region SuppressResourceStringsCheckRegion
		void SendNotification(EDIMessage message, CusEntryHeader entry, IGOVCBRRR5MessageData messageData)
		{
			var declaration = entry?.Declaration;
			var notificationData = new DeclarationEntryNotificationData()
			{
				Entry = entry,
				Branch = declaration?.Branch ?? message.Branch,
				EmailGroup = KRCustomsRegistry.Instance.ExportEmailGroup,
				EmailBody = CreateUserFriendlyMessageInterpretation(messageData),
				OriginalMessageTypes = new ZString[] { ElectronicDocumentTypeList.Codes._830 },
				MessageTypeDescription = $"[{ElectronicDocumentTypeList.Descriptions._RR5}]",
				EntryNumber = messageData.ExportDeclarationNumber
			};
			NotificationSender.SendNotification(notificationData);
		}

		ZString CreateUserFriendlyMessageInterpretation(IGOVCBRRR5MessageData messageData)
		{
			var result = new ZStringBuilder();
			var tableContents = new HtmlTableCreator(new string[] { "항 목", "내 용" });
			tableContents.WriteRow(new string[] { "제출문서", "수출신고서" });
			tableContents.WriteRow(new string[] { "처리결과통보일시", messageData.NoticeDateTime.ToString(DateFormatType.DateTimeKorean) });
			tableContents.WriteRow(new string[] { "수출신고번호", MessageFunctions.DeclarationNumberFormat(messageData.ExportDeclarationNumber) });
			tableContents.WriteRow(new string[] { "변경결과사유", new ExportChangingProcessReasonCodeList().GetDescriptionFromCode(messageData.ChangeReasonType) });
			if (messageData.ChangeReasonDescription2 != ZString.Empty)
			{
				tableContents.WriteRow(new string[] { "변경사유내용", messageData.ChangeReasonDescription + HtmlResponseEmailGenerator.HtmlConstants.Br + messageData.ChangeReasonDescription2 });
			}
			else
			{
				tableContents.WriteRow(new string[] { "변경사유내용", messageData.ChangeReasonDescription });
			}

			tableContents.WriteRow(new string[] { "검사결과통보", new ExportInspectionChangingNotificationCodeList().GetDescriptionFromCode(messageData.InspectionChangeType) });
			tableContents.WriteRow(new string[] { "(변경)심사 담당자", messageData.CustomsManagerID + " " + messageData.CustomsManagerName });
			result.Append(tableContents.ToHtml());

			return result.ToString();
		}
		#endregion
	}
}
