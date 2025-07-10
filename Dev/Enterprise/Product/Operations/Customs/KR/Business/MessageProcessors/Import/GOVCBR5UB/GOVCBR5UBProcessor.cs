using CargoWise.Types;
using Enterprise.Customs.Common.KR;
using Enterprise.Customs.Common.Shared;
using Enterprise.Customs.KR.Messaging;
using Enterprise.Messaging.MessageProcessors;
using WTG.StaticAnalysis.Annotation;
using static Enterprise.Customs.KR.Messaging.Constants;

namespace Enterprise.Customs.KR.Business
{
	[CodeAlive("An instance is created via reflection using the attribute MessageType")]
	[MessageType(ElectronicDocumentTypeList.Codes._5UB)]
	class GOVCBR5UBProcessor : IMessageProcessor
	{
		public void Process(EDIMessage message)
		{
			using (var textReader = message.GetEM_MessageTextReader())
			{
				var messageData = new GOVCBR5UBDataProvider().GetMessageData(message.Factory, textReader);
				var entry = MessageLinkedObjectManager.GetLinkedObject(message.Factory, message.Company, messageData.ImportDeclarationNumber, SharedJobMessageTypeList.Codes.Import);
				ZString reviewResult = ZString.Empty;
				PenaltyExemptionSessionalData penaltyExemptionSessionalData = null;
				if (entry != null)
				{
					message.EM_LinkedObject = entry;
					var versionNumber = messageData.PenaltyExemptionReqSequence.ToString();
					var outgoingMessage = entry.Messages.GetLastMessageMatching(x => x.EM_MessageType == ElectronicDocumentTypeList.Codes._5FE && x.EM_MessageOwner == versionNumber);
					if (outgoingMessage != null)
					{
						message.EM_MessageSubType = ElectronicDocumentTypeList.Codes._5FE;
					}
					else
					{
						outgoingMessage = entry.Messages.GetLastMessageMatching(x => x.EM_MessageType == ElectronicDocumentTypeList.Codes._5UA && x.EM_ApplicationReference == versionNumber);
						message.EM_MessageSubType = ElectronicDocumentTypeList.Codes._5UA;
					}
					penaltyExemptionSessionalData = entry.LoadPenaltyExemptionSessionalDataRelated5UA((ZShort)messageData.PenaltyExemptionReqSequence);
					message.EM_ApplicationReference = outgoingMessage?.EM_MessageNum ?? ZString.Empty;
				}
				message.EM_MessageInterpretation = CreateUserFriendlyMessageInterpretation(message, messageData);

				if (messageData.ResultType == AdditionalTexReductionResultTypeList.Codes.C)
				{
					reviewResult = CustomsEntryStatusTypeList.Codes.ANT;
				}
				else if (messageData.ResultType == AdditionalTexReductionResultTypeList.Codes.E)
				{
					reviewResult = CustomsEntryStatusTypeList.Codes.DMS;

					var statementHeader = new CusStatementHeader.Loader(message.Factory).Load(messageData.NoticeNumber, entry.Declaration.JE_GC, StatementHeaderTypeList.Codes.CustomsDisbursementBill);
					if (statementHeader == null)
					{
						statementHeader = message.Factory.New<CusStatementHeader>();
						statementHeader.B2_StatementNumber = messageData.NoticeNumber;
						statementHeader.B2_Status = StatementHeaderStatusList.Codes.U;
						statementHeader.B2_StatementType = StatementHeaderTypeList.Codes.CustomsDisbursementBill;
						statementHeader.B2_PaymentStatus = StatementHeaderPaymentStatusList.Codes.PYI;
					}
					else
					{
						statementHeader.StatementLines.DeleteAll();
					}

					statementHeader.B2_ProcessPort = entry.Declaration.JE_CustomsOffice;
					statementHeader.B2_GC = entry.Declaration.JE_GC;
					statementHeader.B2_OH_Importer = entry.Declaration.JE_OH_DutyPayer;
					statementHeader.B2_CheckNo = message.EM_MessageNum;
					statementHeader.B2_StatementAmount = messageData.PenaltyExemptionAmount;
					statementHeader.B2_ProcessDate = messageData.NoticeDateTime;
					statementHeader.B2_PrintDate = messageData.ApprovalDate;
					statementHeader.B2_DueDate = messageData.ApprovalDate.AddDays(1);

					var statementLine = statementHeader.StatementLines.AddNew();
					statementLine.B3_EntryNum = messageData.ImportDeclarationNumber;
					statementLine.B3_EntryType = KRJobMessageTypeList.Codes.Import;
					statementLine.B3_SequenceNumber = 1;
					statementLine.B3_CustomsFeesTotal = messageData.PenaltyExemptionAmount;

					var statementCharge = statementLine.Charges.AddNew();
					statementCharge.B4_ChargeType = ChargeTypeList.Codes.PenaltyAndInterest;
					statementCharge.B4_ChargeAmount = messageData.PenaltyExemptionAmount;

					message.CloneIncludingInterpretation(statementHeader);
				}

				if (!reviewResult.IsEmpty)
				{
					message.EM_MessageOwner = reviewResult;
					if (penaltyExemptionSessionalData != null)
					{
						penaltyExemptionSessionalData.CSI_Status = reviewResult;
					}
				}

				SendNotification(message, entry, messageData);
			}
		}

		#region SuppressResourceStringsCheckRegion
		void SendNotification(EDIMessage message, CusEntryHeader entry, GOVCBR5UBMessageData messageData)
		{
			var declaration = entry?.Declaration;
			var notificationData = new DeclarationEntryNotificationData()
			{
				Entry = entry,
				Branch = declaration?.Branch ?? message.Branch,
				EmailGroup = KRCustomsRegistry.Instance.ImportEmailGroup,
				EmailBody = CreateUserFriendlyMessageInterpretation(message, messageData),
				OriginalMessageTypes = new ZString[] { ElectronicDocumentTypeList.Codes._5UA },
				MessageTypeDescription = $"[{ElectronicDocumentTypeList.Descriptions._5UB}]",
				EntryNumber = messageData.ImportDeclarationNumber
			};
			NotificationSender.SendNotification(notificationData);
		}

		string CreateUserFriendlyMessageInterpretation(EDIMessage message, GOVCBR5UBMessageData messageData)
		{
			var result = new ZStringBuilder();

			var tableContents = new HtmlTableCreator(new string[] { "항 목", "내 용" });
			tableContents.WriteRow(new string[] { "제출문서", "가산세(보정이자) 감면 신청서" });
			tableContents.WriteRow(new string[] { "신고번호", MessageFunctions.DeclarationNumberFormat(messageData.ImportDeclarationNumber) });
			tableContents.WriteRow(new string[] { "담당직원명", messageData.CustomsPersonName });
			tableContents.WriteRow(new string[] { "처리세관(과)", "[" + messageData.DeclarationOffice + "] " + MessageFunctions.GetCustomsOfficeAndDivision(message.Factory, messageData.DeclarationOffice) });
			tableContents.WriteRow(new string[] { "처리일자", messageData.ApprovalDate.ToString(DateFormatType.DateKorean) });
			tableContents.WriteRow(new string[] { "통보일시", messageData.NoticeDateTime.ToString(DateFormatType.DateTimeKorean) });
			tableContents.WriteRow(new string[] { "면제종류", messageData.PenaltyExemptionCode + " " + new PenaltyExemptionCodeList().GetDescriptionFromCode(messageData.PenaltyExemptionCode) ?? ZString.Empty });
			tableContents.WriteRow(new string[] { "처리결과", messageData.ResultType + " " + messageData.ResultTypeDescription });
			tableContents.WriteRow(new string[] { "처리내역", messageData.ResultReason });
			tableContents.WriteRow(new string[] { "고지번호", MessageFunctions.NoticeNumberFormat(messageData.NoticeNumber) });
			tableContents.WriteRow(new string[] { "면제신청차수", messageData.PenaltyExemptionReqSequence.ToString() });
			tableContents.WriteRow(new string[] { "면제대상금액", decimal.ToInt32(messageData.PenaltyExemptionAmount).ToString() });
			result.Append(tableContents.ToHtml());

			return result.ToString();
		}
		#endregion
	}
}
