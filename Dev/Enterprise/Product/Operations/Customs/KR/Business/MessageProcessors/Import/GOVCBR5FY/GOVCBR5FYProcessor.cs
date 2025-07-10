using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.Common.Shared;
using Enterprise.Customs.KR.Messaging;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.MessageProcessors;
using WTG.StaticAnalysis.Annotation;
using static Enterprise.Customs.KR.Messaging.Constants;

namespace Enterprise.Customs.KR.Business
{
	[CodeAlive("An instance is created via reflection using the attribute MessageType")]
	[MessageType(ElectronicDocumentTypeList.Codes._5FY)]
	class GOVCBR5FYProcessor : IMessageProcessor
	{
		public void Process(EDIMessage message)
		{
			using (var textReader = message.GetEM_MessageTextReader())
			{
				var messageData = new GOVCBR5FYDataProvider().GetMessageData(textReader);

				var statementHeader = new CusStatementHeader.Loader(message.Factory).Load(messageData.SumPaymentNumber, message.Branch.GB_GC, StatementHeaderTypeList.Codes.Invoice);
				var existingLines = new List<CusStatementLine>();

				if (statementHeader == null)
				{
					statementHeader = message.Factory.New<CusStatementHeader>();
					statementHeader.B2_StatementNumber = messageData.SumPaymentNumber;
					statementHeader.B2_GC = message.Branch.GB_GC;
					statementHeader.B2_StatementType = StatementHeaderTypeList.Codes.Invoice;
				}
				else
				{
					if (statementHeader.StatementLines.Count != 0)
					{
						existingLines.AddRange(statementHeader.StatementLines.Cast<CusStatementLine>());
					}
				}

				statementHeader.B2_DueDate = messageData.ExpirationDate;
				statementHeader.B2_ProcessDate = messageData.NoticeDate;
				statementHeader.B2_StatementAmount = messageData.TotalPayableAmount;
				statementHeader.B2_PaymentStatus = StatementHeaderPaymentStatusList.Codes.PYI;
				statementHeader.B2_ProcessPort = messageData.SumPaymentNumber.Substring(5, 3);
				statementHeader.B2_IsMonthlyStatement = true;
				statementHeader.B2_PeriodStartDate = messageData.NoticeDate.AddMonths(-1).AddDays(1 - messageData.NoticeDate.Day);
				statementHeader.B2_PeriodEndDate = messageData.NoticeDate.AddDays(0 - messageData.NoticeDate.Day);

				var codeType = messageData.ImporterType == "04" ? (ZString)IdentificationType.BusinessRegNo : messageData.ImporterType;

				statementHeader.B2_ImporterCustomsID = messageData.ImporterID;
				var brokerNum = GlbBranch.CurrentBranch?.OrgProxy?.CustomsCodes?.GetCustomsRegNo(codeType) ??
								GlbCompany.CurrentCompany.OrgProxy.CustomsCodes.GetCustomsRegNo(codeType);
				if (brokerNum == messageData.ImporterID)
				{
					statementHeader.B2_PaymentParty = PaymentPartyList.Codes.BRK;
					statementHeader.B2_OH_Importer = GlbBranch.CurrentBranch?.OrgProxy?.PK ?? GlbCompany.CurrentCompany.OrgProxy.PK;
				}
				else
				{
					statementHeader.B2_PaymentParty = PaymentPartyList.Codes.OWN;

					var cusCode = new OrgCusCode.Loader(message.Factory).Load(Core.Constants.CountryCodes.KoreaSouth, codeType, messageData.ImporterID);

					if (cusCode != null && cusCode.Length == 1)
					{
						statementHeader.B2_OH_Importer = cusCode[0].OK_OH;
					}
					else if (messageData.TotalDutyTax != null)
					{
						var entry = MessageLinkedObjectManager.GetLinkedObject(message.Factory, message.Company, messageData.TotalDutyTax.FirstOrDefault().ImportDeclarationNumber, SharedJobMessageTypeList.Codes.Import);

						statementHeader.B2_OH_Importer = entry?.Declaration?.JE_OH_DutyPayer ?? ZGuid.Empty;
					}
				}

				#region statementLine & LineCharge
				if (messageData.TotalDutyTax != null)
				{
					var chargeTypeList = new ChargeTypeList();
					foreach (var goodshipment in messageData.TotalDutyTax)
					{
						var statementLine = statementHeader.StatementLines.FirstOrDefault(x => x.B3_EntryNum == goodshipment.ImportDeclarationNumber);
						var existingLineCharges = new List<CusStatementLineCharge>();

						if (statementLine == null)
						{
							statementLine = statementHeader.StatementLines.AddNew();
							statementLine.B3_EntryNum = goodshipment.ImportDeclarationNumber;
						}
						else
						{
							statementLine.B3_CustomsFeesTotal = ZDecimal.Zero;
							existingLines.Remove(statementLine);

							existingLineCharges.AddRange(statementLine.Charges.Cast<CusStatementLineCharge>());
						}
						statementLine.B3_SequenceNumber = goodshipment.SequenceNumber;
						statementLine.B3_AssociatedEntry = goodshipment.PaymentReferenceNumber;
						statementLine.B3_EntryType = SharedJobMessageTypeList.Codes.Import;

						if (goodshipment.DutyTaxes != null)
						{
							foreach (var charge in goodshipment.DutyTaxes)
							{
								var statementCharge = statementLine.Charges.FirstOrDefault(x => x.B4_ChargeType == ChargeTypeList.GetCorrespondingChargeType(charge.DutyTaxType));

								if (statementCharge == null)
								{
									statementCharge = statementLine.Charges.AddNew();
									statementCharge.B4_ChargeType = ChargeTypeList.GetCorrespondingChargeType(charge.DutyTaxType);
								}
								else
								{
									existingLineCharges.Remove(statementCharge);
								}

								statementCharge.B4_ChargeAmount = charge.DutyTaxFee;
								statementLine.B3_CustomsFeesTotal += charge.DutyTaxFee;
							}
						}
						existingLineCharges.ForEach(x => x.Delete());
					}
				}
				existingLines.ForEach(x => x.Charges.DeleteAll());
				existingLines.ForEach(x => x.Delete());
				#endregion

				message.EM_LinkedObject = statementHeader;
				message.EM_MessageInterpretation = CreateUserFriendlyMessageInterpretation(messageData, MessageFunctions.MessageInterpretationMode.FullView);
				SendNotification(message, statementHeader, messageData);
			}
		}

		#region SuppressResourceStringsCheckRegion

		void SendNotification(EDIMessage message, CusStatementHeader statementHeader, IGOVCBR5FYMessageData messageData)
		{
			var notificationData = new NotificationData()
			{
				ControllerIDProvider = statementHeader,
				MessagesParent = statementHeader,
				JobNumberDescription = $"발행번호: {messageData.SumPaymentNumber}",
				Branch = message.Branch,
				EmailGroup = KRCustomsRegistry.Instance.ImportStatementEmailGroup,
				EmailBody = CreateUserFriendlyMessageInterpretation(messageData, MessageFunctions.MessageInterpretationMode.Email),
				MessageTypeDescription = $"[{ElectronicDocumentTypeList.Descriptions._5FY}]",
				EntryNumber = messageData.SumPaymentNumber
			};
			NotificationSender.SendNotification(notificationData);
		}

		string CreateUserFriendlyMessageInterpretation(IGOVCBR5FYMessageData messageData, MessageFunctions.MessageInterpretationMode mode)
		{
			var result = new ZStringBuilder();

			var tableContents = new HtmlTableCreator(System.Array.Empty<string>());
			tableContents.WriteRowWithFormatting(new CellWithFormatting("항 목", new NameValueCollection { { "width", "200" } }, true), new CellWithFormatting("내 용", new NameValueCollection { { "width", "400" } }, true));
			tableContents.WriteRow(new string[] { "일괄납부서 번호", MessageFunctions.GetFormattedNumber(messageData.SumPaymentNumber, new int[] { 0, 4, 7, 9, 11 }) });
			tableContents.WriteRow(new string[] { "수입징수관 계좌번호", messageData.BankAccountNumber });
			tableContents.WriteRow(new string[] { "사업자등록번호(주민번호)", messageData.ImporterID });
			tableContents.WriteRow(new string[] { "사업자등록번호(주민번호)구분", "[" + messageData.ImporterType + "] " + new IdentificationTypeList().GetDescriptionFromCode(messageData.ImporterType) });
			tableContents.WriteRow(new string[] { "상호", messageData.ImporterCompanyName });
			tableContents.WriteRow(new string[] { "성명", messageData.ImporterRepresentativeName });
			tableContents.WriteRow(new string[] { "주소", messageData.ImporterAddressLine1 });
			tableContents.WriteRow(new string[] { "신고인부호", messageData.DeclarantID });
			tableContents.WriteRow(new string[] { "수입징수관서", messageData.CustomsOfficeName });
			tableContents.WriteRow(new string[] { "납부기한", messageData.ExpirationDate.ToString(DateFormatType.DateKorean) });
			tableContents.WriteRow(new string[] { "발행일자", messageData.NoticeDate.ToString(DateFormatType.DateKorean) });
			tableContents.WriteRow(new string[] { "총관세", string.Format("{0:#,##0}", messageData.TotalDutyAmount) });
			tableContents.WriteRow(new string[] { "총부가세", string.Format("{0:#,##0}", messageData.TotalVAT) });
			tableContents.WriteRow(new string[] { "총주세", string.Format("{0:#,##0}", messageData.TotalLiquorTax) });
			tableContents.WriteRow(new string[] { "총농특세", string.Format("{0:#,##0}", messageData.TotalAgricultureTax) });
			tableContents.WriteRow(new string[] { "총개소세", string.Format("{0:#,##0}", messageData.TotalSpecialConsumptionTax) });
			tableContents.WriteRow(new string[] { "총교통세", string.Format("{0:#,##0}", messageData.TotalTransportationTax) });
			tableContents.WriteRow(new string[] { "총교육세", string.Format("{0:#,##0}", messageData.TotalEducationTax) });
			tableContents.WriteRow(new string[] { "총가산세(보정이자)", string.Format("{0:#,##0}", messageData.PenaltyForLateDeclaration) });
			tableContents.WriteRow(new string[] { "총가산금", string.Format("{0:#,##0}", messageData.PenaltyForMissedDeclarationb) });
			tableContents.WriteRow(new string[] { "납기내 금액", string.Format("{0:#,##0}", messageData.TotalPayableAmount) });
			result.Append(tableContents.ToHtml());

			if (messageData.TotalDutyTax != null)
			{
				var tableDutyTaxFeeContents = new HtmlTableCreator(System.Array.Empty<string>()) { EnableHTMLEncoding = false };
				tableDutyTaxFeeContents.WriteRowWithFormatting(new CellWithFormatting("세액 정보", new NameValueCollection { { "align", "left" }, { "colspan", "2" } }, true));
				tableDutyTaxFeeContents.WriteRow(new string[] { "수입신고번호", "세액합계" });
				var i = 0;
				foreach (var dutyTax in messageData.TotalDutyTax)
				{
					if (i > 9 && mode == MessageFunctions.MessageInterpretationMode.Email)
					{
						tableDutyTaxFeeContents.WriteRowWithFormatting(new CellWithFormatting("나머지 내역은 프로그램에서 확인 하십시오.", new NameValueCollection { { "colspan", "2" } }));
						break;
					}
					tableDutyTaxFeeContents.WriteRow(new string[] { MessageFunctions.DeclarationNumberFormat(dutyTax.ImportDeclarationNumber),
																		string.Format("{0:#,##0}", dutyTax.TotalDutyAndTax) });
					i += 1;
				}
				result.Append(tableDutyTaxFeeContents.ToHtml());
			}

			return result.ToString();
		}
		#endregion
	}
}
