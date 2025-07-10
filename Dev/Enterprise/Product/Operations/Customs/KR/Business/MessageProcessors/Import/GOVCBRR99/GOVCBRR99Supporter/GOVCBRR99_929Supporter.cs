using System.Linq;
using System.Reflection;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.Business.AccumulativeAmendment;
using Enterprise.Customs.Common;
using Enterprise.Customs.Common.KR;
using Enterprise.Customs.Common.Shared;
using Enterprise.Customs.KR.Messaging;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.Customs.KR.Business
{
	[CodeAlive("An instance is created via reflection which has a matched value in GOVCBRR99SupportMessageType against EM_MessageSubType")]
	[MultiPurposeResponseSupportMessageType(ElectronicDocumentTypeList.Codes._929)]
	class GOVCBRR99_929Supporter : IGOVCBRR99Supporter
	{
		ZString IGOVCBRR99Supporter.EntryType => SharedJobMessageTypeList.Codes.Import;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Strings already translated")]
		void IGOVCBRR99Supporter.UpdateParent(EDIMessage message, CusEntryHeader entry, ZString typeCode, IGOVCBRR99MessageData messageData, ZStringBuilder emailBuilder)
		{
			var originalMessage = entry.Messages.GetLastMessage(EDIMessage.ApplicationCodes.KRCustoms, typeCode) as EDIMessage;
			message.EM_ApplicationReference = originalMessage?.EM_MessageNum ?? ZString.Empty;
			entry.CH_Status = CustomsMessageStatusTypeList.Codes.OriginalAccepted;
			entry.EntryNumbers.UpdateCusEntryNumIfExists(SharedJobMessageTypeList.Codes.Import, CusEntryNumber.Schema.CE_IssueDate, messageData.AcceptDateTime);
			entry.CH_BGMReference = messageData.NoticeNumber;

			var hasTaxContent = messageData.Content.Any(x => x.ContentType == AdditionalInformationCodeList.Codes._012);
			var payerWrapper = OrgHeaderWrapper.New(entry.Declaration.DutyPayer);
			if (payerWrapper != null)
			{
				if (payerWrapper.ZO_VATDeferment == ZString.Empty)
				{
					if (hasTaxContent)
					{
						payerWrapper.ZO_VATDeferment = VATDefermentCodeList.Codes.Y1;
						emailBuilder.Append("‘부가세 납부유예업체’ 정보가 없는데, 특이사항에 ‘부가세 납부유예 대상 업체’로 통보되어서,");
						emailBuilder.Append("납세의무자 회사 정보에 ‘부가세 납부유예업체’로 변경하였습니다.");
						emailBuilder.Append("확인 바랍니다.");
					}
					else
					{
						payerWrapper.ZO_VATDeferment = VATDefermentCodeList.Codes.N1;
						emailBuilder.Append("‘부가세 납부유예업체’ 정보가 없는데, 특이사항에 ‘부가세 납부유예 대상 업체’로 통보되어 있지 않아서,");
						emailBuilder.Append("납세의무자 회사정보에 ‘부가세 납부유예업체’가 아닌 것으로 변경하였습니다.");
						emailBuilder.Append("확인 바랍니다.");
					}
				}
				else if (payerWrapper.ZO_VATDeferment == VATDefermentCodeList.Codes.Y1)
				{
					if (!hasTaxContent)
					{
						payerWrapper.ZO_VATDeferment = VATDefermentCodeList.Codes.Conflict;
						emailBuilder.Append("‘부가세 납부유예업체’인데, 특이사항에 ‘부가세 납부유예 대상 업체’로 통보되지 않았습니다.");
						emailBuilder.Append("납세의무자 회사 정보를 확인 바랍니다.");
					}
				}
				else if (payerWrapper.ZO_VATDeferment == VATDefermentCodeList.Codes.N1)
				{
					if (hasTaxContent)
					{
						payerWrapper.ZO_VATDeferment = VATDefermentCodeList.Codes.Conflict;
						emailBuilder.Append("‘부가세 납부유예업체’가 아닌데, 특이사항에 ‘부가세 납부유예 대상 업체’로 통보되었습니다.");
						emailBuilder.Append("납세의무자 회사 정보를 확인 바랍니다.");
					}
				}
				else if (payerWrapper.ZO_VATDeferment == VATDefermentCodeList.Codes.Y)
				{
					if (!hasTaxContent)
					{
						emailBuilder.Append("‘부가세 납부유예업체’가 ‘Y’로 설정되어 있지만, 특이사항에 ‘부가세 납부유예 대상 업체’로 통보되지 않았습니다.");
						emailBuilder.Append("납세의무자 회사 정보를 확인 바랍니다.");
					}
				}
				else if (payerWrapper.ZO_VATDeferment == VATDefermentCodeList.Codes.N)
				{
					if (hasTaxContent)
					{
						emailBuilder.Append("‘부가세 납부유예업체’가 ‘N’로 설정되어 있지만, 특이사항에 ‘부가세 납부유예 대상 업체’로 통보되었습니다.");
						emailBuilder.Append("납세의무자 회사 정보를 확인 바랍니다.");
					}
				}

				if (!messageData.NoticeNumber.IsEmpty)
				{
					CusEntrySnapshot snapshot = entry.Snapshots.Cast<CusEntrySnapshot>().FirstOrDefault(x => x.CES_MessageType == ElectronicDocumentTypeList.Codes._929 && x.CES_Status == EntrySnapshotStatus.Lodged && x.CES_VersionNumber == 1);
					if (snapshot != null)
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
						statement.B2_ProcessDate = messageData.NoticeDateTime;
						if (!messageData.DeclarationDate.IsEmpty)
						{
							statement.B2_PrintDate = messageData.DeclarationDate;
							statement.B2_DueDate = messageData.DeclarationDate.AddDays(15);
						}
						statement.B2_Status = StatementHeaderStatusList.Codes.Z;
						statement.B2_PaymentStatus = StatementHeaderPaymentStatusList.Codes.PYI;
						statement.B2_ProcessPort = entry.Declaration.JE_CustomsOffice.SubstringSafe(0, 3);
						statement.B2_OH_Importer = entry.Declaration.JE_OH_DutyPayer;

						ImportEntryHeader dataProvider929 = null;
						using (var textReader = snapshot.GetCES_SnapshotXmlReader())
						{
							dataProvider929 = KRXmlObjectSerializer.DeserializeWithoutSchemaValidation<ImportEntryHeader>(textReader);
						}
						statement.B2_StatementAmount = GetStatementAmount(dataProvider929);

						var statementLine = statement.StatementLines.AddNew();
						statementLine.B3_EntryType = KRJobMessageTypeList.Codes.Import;
						statementLine.B3_EntryNum = messageData.ApplicationNumber;
						statementLine.B3_SequenceNumber = 1;

						CreateStatementLineCharges(statementLine, dataProvider929);

						message.CloneIncludingInterpretation(statement);
					}
				}
			}

			ZDecimal GetStatementAmount(ImportEntryHeader dataProvider929)
			{
				var totalVAT = dataProvider929.TotalVAT;
				var totalPayableAmount = dataProvider929.TotalPayableAmount;

				var result = totalPayableAmount;

				if (payerWrapper.ZO_VATDeferment == VATDefermentCodeList.Codes.Y || payerWrapper.ZO_VATDeferment == VATDefermentCodeList.Codes.Y1)
				{
					result = totalPayableAmount - totalVAT;
				}
				else if (dataProvider929.PaymentType == PaymentMethodCodeList.Codes._33)
				{
					result = totalVAT;
				}
				return result;
			}

			void CreateStatementLineCharges(CusStatementLine statementLine, ImportEntryHeader dataProvider929)
			{
				foreach (var propertyInfo in typeof(IImportEntryHeader).GetProperties())
				{
					ZString customsFeeID = propertyInfo.GetCustomAttribute<DataItemIDAttribute>()?.CustomsFeeID ?? ZString.Empty;
					if (!customsFeeID.IsEmpty)
					{
						ZDecimal amount = ZDecimal.ParseSafe(propertyInfo.GetValue(dataProvider929).ToString(), 0);
						if (amount != 0)
						{
							statementLine.B3_CustomsFeesTotal += amount;

							var chargeType = ChargeTypeList.GetCorrespondingChargeType(customsFeeID);
							if (!chargeType.IsNullOrEmpty())
							{
								var statementLineCharge = statementLine.Charges[chargeType] ?? statementLine.Charges.AddNew();
								statementLineCharge.B4_ChargeType = chargeType;
								statementLineCharge.B4_ChargeAmount += amount;
							}
						}
					}
				}
			}
			entry.UpdateEntryVersionID(originalMessage);
		}
	}
}
