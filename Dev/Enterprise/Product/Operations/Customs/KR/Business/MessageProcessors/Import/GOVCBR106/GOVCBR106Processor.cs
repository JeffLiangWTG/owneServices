using CargoWise.Types;
using Enterprise.Customs.Common.Shared;
using Enterprise.Customs.KR.Messaging;
using Enterprise.Messaging.MessageProcessors;
using WTG.StaticAnalysis.Annotation;
using static Enterprise.Customs.KR.Messaging.Constants;

namespace Enterprise.Customs.KR.Business
{
	[CodeAlive("An instance is created via reflection using the attribute MessageType")]
	[MessageType(ElectronicDocumentTypeList.Codes._106)]
	class GOVCBR106Processor : IMessageProcessor
	{
		public void Process(EDIMessage message)
		{
			using (var textReader = message.GetEM_MessageTextReader())
			{
				var messageData = new GOVCBR106DataProvider().GetMessageData(textReader);

				var entry = MessageLinkedObjectManager.GetLinkedObject(message.Factory, message.Company, messageData.ImportDeclarationNumber, SharedJobMessageTypeList.Codes.Import);
				if (entry != null)
				{
					message.EM_LinkedObject = entry;

					var originalType = entry.GetOriginalFTAType();
					var amendmentType = originalType == ElectronicDocumentTypeList.Codes._DHR ? ElectronicDocumentTypeList.Codes._DHS : ElectronicDocumentTypeList.Codes._105;
					
					var outgoingMessage = (EDIMessage)entry.Messages.GetLastMessage(EDIMessage.ApplicationCodes.KRCustoms, amendmentType);
					message.EM_ApplicationReference = outgoingMessage?.EM_MessageNum ?? ZString.Empty;
					message.EM_MessageSubType = outgoingMessage?.EM_MessageType ?? ZString.Empty;

					if (messageData.ResultType == ResultTypeList.Codes.C)
					{
						message.EM_MessageOwner = CustomsEntryStatusTypeList.Codes.ANT;
					}
					else if (messageData.ResultType == ResultTypeList.Codes.E)
					{
						message.EM_MessageOwner = CustomsEntryStatusTypeList.Codes.DMS;
						entry.MarkLodgedSnapshotAsDeleted(originalType);
					}
				}
				message.EM_MessageInterpretation = CreateUserFriendlyMessageInterpretation(messageData);
				SendNotification(message, entry, messageData);
			}
		}

		#region SuppressResourceStringsCheckRegion
		void SendNotification(EDIMessage message, CusEntryHeader entry, IGOVCBR106MessageData messageData)
		{
			var declaration = entry?.Declaration;
			var notificationData = new DeclarationEntryNotificationData()
			{
				Entry = entry,
				Branch = declaration?.Branch ?? message.Branch,
				EmailGroup = KRCustomsRegistry.Instance.ImportEmailGroup,
				EmailBody = CreateUserFriendlyMessageInterpretation(messageData),
				OriginalMessageTypes = new ZString[] { ElectronicDocumentTypeList.Codes._105, ElectronicDocumentTypeList.Codes._DHS },
				MessageTypeDescription = $"[{ElectronicDocumentTypeList.Descriptions._106}]",
				EntryNumber = messageData.ImportDeclarationNumber
			};
			NotificationSender.SendNotification(notificationData);
		}

		string CreateUserFriendlyMessageInterpretation(IGOVCBR106MessageData messageData)
		{
			var result = new ZStringBuilder();

			var tableContents = new HtmlTableCreator(new string[] { "항 목", "내 용" });
			tableContents.WriteRow(new string[] { "제출문서", "협정정정 신고서" });
			tableContents.WriteRow(new string[] { "수입신고번호", MessageFunctions.DeclarationNumberFormat(messageData.ImportDeclarationNumber) });
			tableContents.WriteRow(new string[] { "처리결과", new ResultTypeList().GetDescriptionFromCode(messageData.ResultType) });
			tableContents.WriteRow(new string[] { "담당부서", messageData.CustomsDepartment });
			tableContents.WriteRow(new string[] { "담당자명", messageData.CustomsPersonName });
			tableContents.WriteRow(new string[] { "담당자연락처", messageData.CustomsPersonPhoneNumber });
			tableContents.WriteRow(new string[] { "처리일자", messageData.ApprovalDate.ToString(DateFormatType.DateKorean) });
			tableContents.WriteRow(new string[] { "신청일자", messageData.DeclarationDate.ToString(DateFormatType.DateKorean) });
			tableContents.WriteRow(new string[] { "처리내역", messageData.ContentDescription });
			result.Append(tableContents.ToHtml());

			return result.ToString();
		}
		#endregion
	}
}
