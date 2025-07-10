using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.Business.AccumulativeAmendment;
using Enterprise.Customs.Common.KR;
using Enterprise.Customs.Common.Shared;
using Enterprise.Customs.KR.Messaging;
using Enterprise.Messaging.MessageProcessors;
using Enterprise.ZArchitecture.Business;
using WTG.StaticAnalysis.Annotation;
using static Enterprise.Customs.KR.Messaging.Constants;

namespace Enterprise.Customs.KR.Business
{
	[CodeAlive("An instance is created via reflection using the attribute MessageType")]
	[MessageType(ElectronicDocumentTypeList.Codes._5FK)]
	class GOVCBR5FKProcessor : IMessageProcessor
	{
		public void Process(EDIMessage message)
		{
			using (var textReader = message.GetEM_MessageTextReader())
			{
				var messageData = new GOVCBR5FKDataProvider().GetMessageData(message.Factory, textReader);
				var entry = MessageLinkedObjectManager.GetLinkedObject(message.Factory, message.Company, messageData.ImportDeclarationNumber, SharedJobMessageTypeList.Codes.Import);
				if (entry != null)
				{
					message.EM_LinkedObject = entry;
					message.EM_MessageInterpretation = CreateUserFriendlyMessageInterpretation(messageData);

					var noticeCode = messageData.NoticeCode;
					var noticeDate = messageData.NoticeDate.ToOffset();

					var entryStatus = string.Empty;
					using (noticeDate.IsEmpty ? null : entry.SuspendCESLog())
					{
						if (noticeCode == ImportAmendmentResultList.Codes._11)
						{
							entryStatus = CustomsEntryStatusTypeList.Codes.ANT;

							if (!messageData.NoticeNumber.IsEmpty && StatementHeaderStatusList.IsResultingFromAmendment(messageData.TransactionNatureCode))
							{
								CreateOrUpdateCusStatementHeader(messageData, entry);
							}
						}
						else if (noticeCode == ImportAmendmentResultList.Codes._12)
						{
							entryStatus = CustomsEntryStatusTypeList.Codes.RJC;
							entry.MarkLodgedSnapshotAsDeleted(ElectronicDocumentTypeList.Codes._929);
						}
						else if (noticeCode == ImportAmendmentResultList.Codes._13)
						{
							entryStatus = CustomsEntryStatusTypeList.Codes.CCL;
							entry.MarkLodgedSnapshotAsDeleted(ElectronicDocumentTypeList.Codes._929);
						}
						else if (noticeCode == ImportAmendmentResultList.Codes._14)
						{
							entryStatus = CustomsEntryStatusTypeList.Codes.DMS;
							entry.MarkLodgedSnapshotAsDeleted(ElectronicDocumentTypeList.Codes._929);
						}

						var originalMessage = entry.Messages.GetLastMessage(EDIMessage.ApplicationCodes.KRCustoms, ElectronicDocumentTypeList.Codes._5FE);
						message.EM_ApplicationReference = originalMessage?.EM_MessageNum ?? ZString.Empty;
						message.EM_MessageSubType = ElectronicDocumentTypeList.Codes._5FE;

						if (!string.IsNullOrEmpty(entryStatus))
						{
							entry.CH_EntryStatus = entryStatus;
							message.EM_MessageOwner = entry.CH_EntryStatus;

							var amendmentSessionalData = entry.LoadAmendmentSessionalData((ZShort)messageData.AmendSequence, messageData.SubmissionDate);
							if (amendmentSessionalData != null)
							{
								amendmentSessionalData.CSI_Status = entry.CH_EntryStatus;
								if (amendmentSessionalData.ValidPenaltyExemptionSessionalData != null)
								{
									amendmentSessionalData.ValidPenaltyExemptionSessionalData.CSI_Value = messageData.TotalInterestAndPenalty;
								}
							}
						}
					}

					if (!noticeDate.IsEmpty)
					{
						entry.AddLog(Events.CustomsEntryStatus, entry.CH_EntryStatus, noticeDate);
					}

					TaxTypeMatching(entry, TaxType.TBD, messageData.InDepositAmount);
					TaxTypeMatching(entry, TaxType.TAD, messageData.DelayPaymentAmount);
					TaxTypeMatching(entry, TaxType.TPA, messageData.TotalInterestAndPenalty);
				}
				SendNotification(message, entry, messageData);
			}

			void CreateOrUpdateCusStatementHeader(GOVCBR5FKMessageData messageData, CusEntryHeader entry)
			{
				var statement = new CusStatementHeader.Loader(message.Factory).Load(messageData.NoticeNumber, entry.Declaration.JE_GC, StatementHeaderTypeList.Codes.CustomsDisbursementBill);
				if (statement == null)
				{
					statement = message.Factory.New<CusStatementHeader>();
					statement.B2_StatementNumber = messageData.NoticeNumber;
					statement.B2_GC = entry.Declaration.JE_GC;
					statement.B2_StatementType = StatementHeaderTypeList.Codes.CustomsDisbursementBill;
				}
				else
				{
					statement.StatementLines.DeleteAll();
				}
				statement.B2_IncomingMessageNo = message.EM_MessageNum;
				statement.B2_ProcessDate = messageData.NoticeDate;
				statement.B2_StatementAmount = GetStatementAmount(messageData, entry);
				statement.B2_Status = messageData.TransactionNatureCode;
				statement.B2_PaymentStatus = StatementHeaderPaymentStatusList.Codes.PYI;
				statement.B2_ProcessPort = entry.Declaration.JE_CustomsOffice.SubstringSafe(0, 3);
				statement.B2_OH_Importer = entry.Declaration.JE_OH_DutyPayer;

				if (messageData.TransactionNatureCode != StatementHeaderStatusList.Codes.O)
				{
					statement.B2_PrintDate = messageData.SubmissionDate;
					statement.B2_DueDate = messageData.SubmissionDate.AddDays(1);
				}

				var statementLine = statement.StatementLines.AddNew();
				statementLine.B3_EntryType = KRJobMessageTypeList.Codes.Import;
				statementLine.B3_EntryNum = messageData.ImportDeclarationNumber;
				statementLine.B3_SequenceNumber = 1;

				foreach (var charge in messageData.Charges.Cast<GOVCBR5FKChargeMessageData>().Where(x => x.Amount != 0))
				{
					statementLine.B3_CustomsFeesTotal += charge.Amount;

					var chargeType = ChargeTypeList.GetCorrespondingChargeType(charge.TypeCode);
					var statementLineCharge = statementLine.Charges.FirstOrDefault(x => x.B4_ChargeType == chargeType);
					if (statementLineCharge == null)
					{
						statementLineCharge = statementLine.Charges.AddNew();
						statementLineCharge.B4_ChargeType = chargeType;
					}
					statementLineCharge.B4_ChargeAmount += charge.Amount;
				}

				if (messageData.TotalInterestAndPenalty != 0)
				{
					statementLine.B3_CustomsFeesTotal += messageData.TotalInterestAndPenalty;

					var statementLineCharge = statementLine.Charges.AddNew();
					statementLineCharge.B4_ChargeType = ChargeTypeList.GetCorrespondingChargeType(EntryTaxTypeList.Codes._5AT);
					statementLineCharge.B4_ChargeAmount = messageData.TotalInterestAndPenalty;
				}

				message.CloneIncludingInterpretation(statement);
			}

			ZDecimal GetStatementAmount(GOVCBR5FKMessageData messageData, CusEntryHeader entry)
			{
				ZDecimal result = messageData.InDepositAmount;
				if (messageData.TransactionNatureCode == StatementHeaderStatusList.Codes.O)
				{
					var snapshot = entry.Snapshots.GetLatestSnapshotIn(ElectronicDocumentTypeList.Codes._929, EntrySnapshotStatus.Lodged);
					if (snapshot != null)
					{
						var header = snapshot.DataProviderObject as IImportEntryHeader;
						if ((header?.PaymentType ?? ZString.Empty) == PaymentMethodCodeList.Codes._33)
						{
							result = messageData.VATPayable;
						}
					}
				}
				return result;
			}
		}

		#region SuppressResourceStringsCheckRegion
		void SendNotification(EDIMessage message, CusEntryHeader entry, IGOVCBR5FKMessageData messageData)
		{
			var declaration = entry?.Declaration;
			var notificationData = new DeclarationEntryNotificationData()
			{
				Entry = entry,
				Branch = declaration?.Branch ?? message.Branch,
				EmailGroup = KRCustomsRegistry.Instance.ImportEmailGroup,
				EmailBody = CreateUserFriendlyMessageInterpretation(messageData),
				OriginalMessageTypes = new ZString[] { ElectronicDocumentTypeList.Codes._5FE },
				MessageTypeDescription = $"[{ElectronicDocumentTypeList.Descriptions._5FK}]",
				EntryNumber = messageData.ImportDeclarationNumber
			};
			NotificationSender.SendNotification(notificationData);
		}

		string CreateUserFriendlyMessageInterpretation(IGOVCBR5FKMessageData messageData)
		{
			var result = new ZStringBuilder();

			var tableContents = new HtmlTableCreator(new string[] { "항 목", "내 용" });
			tableContents.WriteRow(new string[] { "제출문서", "수입신고 정정 신청서" });
			tableContents.WriteRow(new string[] { "신청일자", messageData.SubmissionDate.ToString(DateFormatType.DateKorean) });
			tableContents.WriteRow(new string[] { "처리일시", messageData.NoticeDate.ToString(DateFormatType.DateKorean) });
			tableContents.WriteRow(new string[] { "수입신고번호", MessageFunctions.DeclarationNumberFormat(messageData.ImportDeclarationNumber) });
			tableContents.WriteRow(new string[] { "처리상태코드", new ImportAmendmentResultList().GetDescriptionFromCode(messageData.NoticeCode) });
			tableContents.WriteRow(new string[] { "납기내세액", messageData.InDepositAmount.ToString() });
			tableContents.WriteRow(new string[] { "납기후세액", messageData.DelayPaymentAmount.ToString() });
			tableContents.WriteRow(new string[] { "가산세합계", messageData.TotalInterestAndPenalty.ToString() });
			tableContents.WriteRow(new string[] { "수정신고 전자납부부호", MessageFunctions.NoticeNumberFormat(messageData.NoticeNumber) });
			result.Append(tableContents.ToHtml());

			return result.ToString();
		}

		void TaxTypeMatching(CusEntryHeader entry, ZString chargeType, ZDecimal chargeAmount)
		{
			if (chargeAmount > 0)
			{
				entry.Charges.SetAmount(chargeType, chargeAmount);
			}
		}
		#endregion
	}
}
