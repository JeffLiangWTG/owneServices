using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
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
	[MessageType(ElectronicDocumentTypeList.Codes._5WN)]
	class GOVCBR5WNProcessor : IMessageProcessor
	{
		public void Process(EDIMessage message)
		{
			using (var textReader = message.GetEM_MessageTextReader())
			{
				var messageData = new GOVCBR5WNDataProvider().GetMessageData(message.Factory, textReader);
				var entry = MessageLinkedObjectManager.GetLinkedObject(message.Factory, message.Company, messageData.ImportDeclarationNumber, SharedJobMessageTypeList.Codes.Import);
				if (entry != null)
				{
					message.EM_LinkedObject = entry;
					message.EM_MessageOwner = messageData.AmendmentSequenceNo;

					if (!messageData.NoticeNumber.IsEmpty && messageData.DutyTaxDifference > 0)
					{
						var statement = new CusStatementHeader.Loader(message.Factory).Load(messageData.NoticeNumber, entry.Declaration.JE_GC, StatementHeaderTypeList.Codes.CustomsDisbursementBill);
						if (statement == null)
						{
							statement = message.Factory.New<CusStatementHeader>();
							statement.B2_StatementNumber = messageData.NoticeNumber;
							statement.B2_GC = entry.Declaration.JE_GC;
							statement.B2_StatementType = StatementHeaderTypeList.Codes.CustomsDisbursementBill;

							var clonedMessage = (EDIMessage)message.Clone();
							clonedMessage.EM_Status = EDIMessage.Status.Received;
							statement.Messages.Add(clonedMessage);
						}
						else
						{
							statement.StatementLines.DeleteAll();
						}
						
						statement.B2_ProcessDate = messageData.NoticeDate;
						statement.B2_PrintDate = messageData.ProcessedDate;
						statement.B2_DueDate = messageData.ProcessedDate.AddDays(15);
						statement.B2_IncomingMessageNo = message.EM_MessageNum;
						statement.B2_StatementAmount = messageData.DutyTaxDifference;
						statement.B2_ProcessPort = entry.Declaration.JE_CustomsOffice.SubstringSafe(0, 3);
						statement.B2_OH_Importer = entry.Declaration.JE_OH_DutyPayer;
						statement.B2_Status = StatementHeaderStatusList.Codes.W;
						statement.B2_PaymentStatus = StatementHeaderPaymentStatusList.Codes.PYI;
						statement.B2_RMNumber = messageData.AmendmentSequenceNo;

						var statementLine = statement.StatementLines.AddNew();
						statementLine.B3_EntryType = KRJobMessageTypeList.Codes.Import;
						statementLine.B3_EntryNum = messageData.ImportDeclarationNumber;
						statementLine.B3_SequenceNumber = 1;
						statementLine.B3_CustomsFeesTotal = messageData.DutyTaxDifference;

						foreach (var tax in messageData.DutyTax.Where(x => x.Value != 0))
						{
							var chargeType = ChargeTypeList.GetCorrespondingChargeType(tax.Key);
							var charge = statementLine.Charges.SingleOrDefault(x => x.B4_ChargeType == chargeType);
							if (charge == null)
							{
								charge = statementLine.Charges.AddNew();
								charge.B4_ChargeType = chargeType;
							}
							charge.B4_ChargeAmount += tax.Value;
						}
					}
					else
					{
						var versionNumber = entry.GetCW1VersionNumberFromCustoms5FEVersionNumber((ZShort)messageData.AmendmentVersionNo, messageData.AmendmentDeclarationDate);
						var amendmentSessionalData = entry.EntryInstruction?.AmendmentSessionalDataCollection.Cast<AmendmentSessionalData>().SingleOrDefault(x => x.AmendmentCW1VersionNo == versionNumber);
						if (amendmentSessionalData != null)
						{
							amendmentSessionalData.CSI_ItemNumber = ZInt.ParseSafe(messageData.AmendmentSequenceNo, 0);
						}
					}
				}
				message.EM_MessageInterpretation = CreateUserFriendlyMessageInterpretation(message, messageData);
				SendNotification(message, entry, messageData);
			}
		}

		#region SuppressResourceStringsCheckRegion
		void SendNotification(EDIMessage message, CusEntryHeader entry, IGOVCBR5WNMessageData messageData)
		{
			var declaration = entry?.Declaration;
			var notificationData = new DeclarationEntryNotificationData()
			{
				Entry = entry,
				Branch = declaration?.Branch ?? message.Branch,
				EmailGroup = KRCustomsRegistry.Instance.ImportEmailGroup,
				EmailBody = CreateUserFriendlyMessageInterpretation(message, messageData),
				OriginalMessageTypes = new ZString[] { ElectronicDocumentTypeList.Codes._929 },
				MessageTypeDescription = $"[{ElectronicDocumentTypeList.Descriptions._5WN}]",
				EntryNumber = messageData.ImportDeclarationNumber
			};
			NotificationSender.SendNotification(notificationData);
		}

		string CreateUserFriendlyMessageInterpretation(EDIMessage message, IGOVCBR5WNMessageData messageData)
		{
			var result = new ZStringBuilder();

			var tableContents = new HtmlTableCreator(new string[] { "항 목", "내 용" });
			tableContents.WriteRow(new string[] { "제출문서", "수입신고서" });
			tableContents.WriteRow(new string[] { "수입신고일자	", messageData.DeclarationDate.ToString(DateFormatType.DateKorean) });
			tableContents.WriteRow(new string[] { "수입신고번호", MessageFunctions.DeclarationNumberFormat(messageData.ImportDeclarationNumber) });
			tableContents.WriteRow(new string[] { "정정신청일자", messageData.AmendmentDeclarationDate.ToString(DateFormatType.DateKorean) });
			tableContents.WriteRow(new string[] { "정정신청차수", messageData.AmendmentVersionNo.ToString() });
			tableContents.WriteRow(new string[] { "통지일자", messageData.NoticeDate.ToString(DateFormatType.DateKorean) });
			tableContents.WriteRow(new string[] { "납세의무자-상호", messageData.PayerCompanyName });
			tableContents.WriteRow(new string[] { "납세의무자-성명", messageData.PayerRepresentativeName });
			tableContents.WriteRow(new string[] { "납세의무자-주소", messageData.PayerAddressLine });
			tableContents.WriteRow(new string[] { "담당부서", messageData.CustomsDepartment });
			tableContents.WriteRow(new string[] { "담당자	", messageData.CustomsPersonName });
			tableContents.WriteRow(new string[] { "담당자연락처", messageData.CustomsPersonPhoneNumber });
			tableContents.WriteRow(new string[] { "경정사유", messageData.TaxAdjustmentReason });
			tableContents.WriteRow(new string[] { "경정통지세관", messageData.CustomsOffice });
			tableContents.WriteRow(new string[] { "납부서번호", messageData.NoticeNumber });
			tableContents.WriteRow(new string[] { "(증감내역)세액합계", messageData.DutyTaxDifference.ToString() });
			result.Append(tableContents.ToHtml());

			if (messageData.DutyTax.Any())
			{
				var detailContents = new HtmlTableCreator(System.Array.Empty<string>()) { EnableHTMLEncoding = false };
				detailContents.WriteRowWithFormatting(new CellWithFormatting("증감내역", new NameValueCollection { { "align", "left" }, { "colspan", "2" } }, true));

				detailContents.WriteRowWithFormatting(new CellWithFormatting("세종부호(산출세액 합계)", new NameValueCollection { { "align", "center" }, { "width", "200" } }), new CellWithFormatting("증감세액", new NameValueCollection { { "align", "center" }, { "width", "300" } }));
				var zeroDuty = new List<ZString>();
				var entryTaxList = new EntryTaxTypeList();
				foreach (var duty in messageData.DutyTax)
				{
					var dutyKey = entryTaxList.GetDescriptionFromCode(duty.Key) ?? duty.Key;
					if (duty.Value != 0)
					{
						detailContents.WriteRowWithFormatting(new CellWithFormatting(dutyKey), new CellWithFormatting(duty.Value.ToString(), new NameValueCollection { { "align", "right" } }));
					}
					else
					{
						zeroDuty.Add(dutyKey);
					}
				}
				ZString line = ZString.Join(",", zeroDuty.ToArray());
				if (!line.IsEmpty)
				{
					detailContents.WriteRowWithFormatting(new CellWithFormatting("증감변동없음", new NameValueCollection { { "align", "left" }, { "width", "200" } }), new CellWithFormatting(line, new NameValueCollection { { "align", "left" }, { "width", "300" } }));
				}

				result.Append(detailContents.ToHtml());
			}

			return result.ToString();
		}
		#endregion
	}
}
