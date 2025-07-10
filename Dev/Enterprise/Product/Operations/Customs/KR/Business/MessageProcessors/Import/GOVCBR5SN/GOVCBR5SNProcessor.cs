using CargoWise.Types;
using Enterprise.Customs.KR.Messaging;
using Enterprise.Messaging.MessageProcessors;
using Enterprise.ZArchitecture.Business;
using WTG.StaticAnalysis.Annotation;
using static Enterprise.Customs.KR.Messaging.Constants;

namespace Enterprise.Customs.KR.Business
{
	[CodeAlive("An instance is created via reflection using the attribute MessageType")]
	[MessageType(ElectronicDocumentTypeList.Codes._5SN)]
	class GOVCBR5SNProcessor : IMessageProcessor
	{
		public void Process(EDIMessage message)
		{
			using (var textReader = message.GetEM_MessageTextReader())
			{
				var messageData = new GOVCBR5SNDataProvider().GetMessageData(textReader);
				var entry = MessageLinkedObjectManager.GetLinkedObject(message.Factory, message.Company, messageData.ValueDeclarationTemplateNumber, ElectronicDocumentTypeList.Codes._5SM);
				if (entry != null)
				{
					message.EM_LinkedObject = entry;

					using (entry.SuspendCESLog())
					{
						if (messageData.ResultType == NameCodeList.Codes._2)
						{
							entry.CH_EntryStatus = CustomsEntryStatusTypeList.Codes.DMS;
							entry.CH_Status = CustomsMessageStatusTypeList.Codes.CancellationApprovedByCustoms;
							message.EM_MessageOwner = CustomsEntryStatusTypeList.Codes.DMS;
						}
						else
						{
							entry.CH_EntryStatus = CustomsEntryStatusTypeList.Codes.ANT;
							message.EM_MessageOwner = CustomsEntryStatusTypeList.Codes.ANT;
						}
					}
					entry.Logs.AddNew(new EventValue(Events.CustomsEntryStatus, eventTime: messageData.ApprovalDate.ToZDateTime().ToOffset(), reference: entry.CH_EntryStatus));

					if (!messageData.ResultReason.IsEmpty)
					{
						entry.CH_CustomsMessageRemarks = messageData.ResultReason;
					}
					entry.CH_BGMReference = messageData.IdentificationNumber;

					var entryNumber = entry.EntryNumbers.GetOrCreateCusEntryNum(ElectronicDocumentTypeList.Codes._5SM);
					entryNumber.CE_ExpiryDate = messageData.EffectiveToDate;

					var outgoingMessage = entry.Messages.GetLastMessage(EDIMessage.ApplicationCodes.KRCustoms, ElectronicDocumentTypeList.Codes._5SM);
					message.EM_ApplicationReference = outgoingMessage?.EM_MessageNum ?? ZString.Empty;
				}

				message.EM_MessageSubType = ElectronicDocumentTypeList.Codes._5SM;
				message.EM_MessageInterpretation = CreateUserFriendlyMessageInterpretation(message, messageData);
				SendNotification(message, entry, messageData);
			}
		}

		#region SuppressResourceStringsCheckRegion
		void SendNotification(EDIMessage message, CusEntryHeader entry, IGOVCBR5SNMessageData messageData)
		{
			var declaration = entry?.Declaration;
			var notificationData = new DeclarationEntryNotificationData()
			{
				Entry = entry,
				Branch = declaration?.Branch ?? message.Branch,
				EmailGroup = KRCustomsRegistry.Instance.ImportEmailGroup,
				EmailBody = CreateUserFriendlyMessageInterpretation(message, messageData),
				OriginalMessageTypes = new ZString[] { ElectronicDocumentTypeList.Codes._5SM },
				MessageTypeDescription = $"[{ElectronicDocumentTypeList.Descriptions._5SN}]",
				EntryNumber = messageData.ValueDeclarationTemplateNumber
			};
			NotificationSender.SendNotification(notificationData);
		}

		string CreateUserFriendlyMessageInterpretation(EDIMessage message, IGOVCBR5SNMessageData messageData)
		{
			var result = new ZStringBuilder();

			var tableContents = new HtmlTableCreator(new string[] { "항 목", "내 용" });
			tableContents.WriteRow(new string[] { "제출문서", "포괄적 가격신고서" });
			tableContents.WriteRow(new string[] { "제출번호", MessageFunctions.DeclarationNumberFormat(messageData.ValueDeclarationTemplateNumber) });
			tableContents.WriteRow(new string[] { "처리일자", messageData.ApprovalDate.ToString(DateFormatType.DateKorean) });
			tableContents.WriteRow(new string[] { "유효만료일자", messageData.EffectiveToDate.ToString(DateFormatType.DateKorean) });
			var nameCodeList = new NameCodeList();
			tableContents.WriteRow(new string[] { "처리결과코드", "[" + messageData.ResultType + "] " + nameCodeList.GetDescriptionFromCode(messageData.ResultType) ?? ZString.Empty });
			tableContents.WriteRow(new string[] { "처리세관(과)", "[" + messageData.DeclarationOffice + "] " + MessageFunctions.GetCustomsOfficeAndDivision(message.Factory, messageData.DeclarationOffice) });
			tableContents.WriteRow(new string[] { "처리세관 담당자명", messageData.CustomsPersonName });
			tableContents.WriteRow(new string[] { "처리내역", messageData.ResultReason });
			if (messageData.IdentificationNumber.Length >= 12)
			{
				tableContents.WriteRow(new string[] { "특이사항", messageData.IdentificationNumber.Substring(0,3) + "-" +
																	  messageData.IdentificationNumber.Substring(3,2) + "-" +
																	  messageData.IdentificationNumber.Substring(5,2) + "-" +
																	  messageData.IdentificationNumber.Substring(7) });
			}
			else
			{
				tableContents.WriteRow(new string[] { "특이사항", messageData.IdentificationNumber });
			}
			result.Append(tableContents.ToHtml());

			return result.ToString();
		}
		#endregion
	}
}
