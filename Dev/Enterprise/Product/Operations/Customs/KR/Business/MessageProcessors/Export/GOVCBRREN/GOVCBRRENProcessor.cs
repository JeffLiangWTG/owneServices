using CargoWise.Types;
using Enterprise.Customs.Common.Shared;
using Enterprise.Customs.KR.Messaging;
using Enterprise.Messaging.MessageProcessors;
using WTG.StaticAnalysis.Annotation;
using static Enterprise.Customs.KR.Messaging.Constants;

namespace Enterprise.Customs.KR.Business
{
	[CodeAlive("An instance is created via reflection using the attribute MessageType")]
	[MessageType(ElectronicDocumentTypeList.Codes._REN)]
	public class GOVCBRRENProcessor : IMessageProcessor
	{
		public void Process(EDIMessage message)
		{
			using (var textReader = message.GetEM_MessageTextReader())
			{
				var messageData = new GOVCBRRENDataProvider().GetMessageData(textReader);
				var entry = MessageLinkedObjectManager.GetLinkedObject(message.Factory, message.Company, messageData.ExportDeclarationNumber, SharedJobMessageTypeList.Codes.Export);
				if (entry != null)
				{
					message.EM_LinkedObject = entry;
					if (messageData.FunctionCode == FunctionCode_08)
					{
						entry.CH_EntryStatus = CustomsEntryStatusTypeList.Codes.SUP;
					}
					else if (messageData.FunctionCode == FunctionCode_39)
					{
						entry.CH_EntryStatus = CustomsEntryStatusTypeList.Codes.CSP;
					}
				}
				message.EM_MessageInterpretation = CreateUserFriendlyMessageInterpretation(entry, messageData);
				SendNotification(message, entry, messageData);
			}
		}

		const string FunctionCode_08 = "08";
		const string FunctionCode_39 = "39";

		#region SuppressResourceStringsCheckRegion
		void SendNotification(EDIMessage message, CusEntryHeader entry, IGOVCBRRENMessageData messageData)
		{
			var declaration = entry?.Declaration;
			var notificationData = new DeclarationEntryNotificationData()
			{
				Entry = entry,
				Branch = declaration?.Branch ?? message.Branch,
				EmailGroup = KRCustomsRegistry.Instance.ExportEmailGroup,
				EmailBody = CreateUserFriendlyMessageInterpretation(entry, messageData),
				OriginalMessageTypes = new ZString[] { ElectronicDocumentTypeList.Codes._830 },
				MessageTypeDescription = $"[{ElectronicDocumentTypeList.Descriptions._REN}]",
				EntryNumber = messageData.ExportDeclarationNumber
			};
			NotificationSender.SendNotification(notificationData);
		}

		ZString CreateUserFriendlyMessageInterpretation(CusEntryHeader entry, IGOVCBRRENMessageData messageData)
		{
			var result = new ZStringBuilder();

			var tableContents = new HtmlTableCreator(new string[] { "항 목", "내 용" });
			tableContents.WriteRow(new string[] { "제출문서", "수출신고서" });
			tableContents.WriteRow(new string[] { "통보일시", messageData.NoticeDateTime.ToString(DateFormatType.DateTimeKorean) });
			tableContents.WriteRow(new string[] { "수출신고번호", messageData.ExportDeclarationNumber });
			tableContents.WriteRow(new string[] { "통관보류차수", messageData.SequenceNo.ToString() });
			tableContents.WriteRow(new string[] { "통관보류구분", new CustomsEntryStatusTypeList().GetDescriptionFromCode(entry?.CH_EntryStatus ?? ZString.Empty) });
			tableContents.WriteRow(new string[] { "수출통관보류종류", new ExportCustomsClearanceDeferTypeCodeList().GetDescriptionFromCode(messageData.SuspendedType) });
			tableContents.WriteRow(new string[] { "수출통관보류구분", new ExportCustomsClearanceDeferReasonCodeList().GetDescriptionFromCode(messageData.SuspendedCode) });
			tableContents.WriteRow(new string[] { "통관보류기타사유", messageData.SuspendedReason });
			tableContents.WriteRow(new string[] { "통관보류시작일시", messageData.StartDateTime.ToString(DateFormatType.DateTimeKorean) });
			tableContents.WriteRow(new string[] { "통관보류종료일시", messageData.EndDateTime.ToString(DateFormatType.DateTimeKorean) });
			tableContents.WriteRow(new string[] { "통관보류(해제)등록직원성명", messageData.CustomsPersonName });
			tableContents.WriteRow(new string[] { "통관보류(해제)등록직원 전화번호", messageData.CustomsPersonPhoneNumber });
			result.Append(tableContents.ToHtml());

			return result.ToString();
		}
		#endregion
	}
}
