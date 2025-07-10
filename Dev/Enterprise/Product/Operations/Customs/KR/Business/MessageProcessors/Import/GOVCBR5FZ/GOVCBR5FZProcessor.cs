using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using CargoWise.Customs.KR.MessageDefinitions;
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
	[MessageType(ElectronicDocumentTypeList.Codes._5FZ)]
	class GOVCBR5FZProcessor : IMessageProcessor
	{
		public void Process(EDIMessage message)
		{
			using (var textReader = message.GetEM_MessageTextReader())
			{
				var messageData = new GOVCBR5FZDataProvider().GetMessageData(textReader);
				var taxInvoiceType = messageData.TaxInvoiceCode == StatementCodeList.Codes.Code1
								? StatementHeaderTypeList.Codes.IndividualCollectionReceipt
								: StatementHeaderTypeList.Codes.MonthlyReceipt;

				var statement = new CusStatementHeader.Loader(message.Factory).Load(messageData.TaxInvoiceNumber, message.Branch.GB_GC, taxInvoiceType);
				var existingLines = new List<CusStatementLine>();

				if (statement == null)
				{
					statement = message.Factory.New<CusStatementHeader>();
					statement.B2_StatementNumber = messageData.TaxInvoiceNumber;
					statement.B2_GC = message.Branch.GB_GC;
					statement.B2_StatementType = taxInvoiceType;
				}
				else
				{
					if (statement.StatementLines.Count != 0)
					{
						existingLines.AddRange(statement.StatementLines.Cast<CusStatementLine>());
					}
				}

				if (messageData.TaxInvoiceCode == StatementCodeList.Codes.Code1)
				{
					statement.B2_PeriodStartDate = messageData.PeriodStartDate;
					statement.B2_PeriodEndDate = messageData.PeriodEndDate;
				}
				statement.B2_PaymentStatus = StatementHeaderPaymentStatusList.Codes.PYC;
				statement.B2_ProcessPort = messageData.TaxInvoiceNumber.Substring(0, 3);
				statement.B2_IsMonthlyStatement = true;
				statement.B2_PaymentType = messageData.TaxInvoiceType == StatementTypeList.Codes.Type01 ? StatementTypeList.Codes.VEP : StatementTypeList.Codes.VPD;
				statement.B2_AccountNo = messageData.SumPaymentNumber;
				statement.B2_PaymentAuthorizationDate = messageData.PaymentDate;
				statement.Remarks = messageData.Remark;
				statement.B2_ProcessDate = message.EM_MessageDateTime.Date;

				var codeType = messageData.ImporterType == nameof(AgencyIdentificationCodeContentType.Ktx)
					? IdentificationType.BusinessRegNo : IdentificationType.KoreanRegNoForResident;
				statement.B2_ImporterCustomsID = messageData.ImporterID;
				var brokerNum = GlbBranch.CurrentBranch?.OrgProxy?.CustomsCodes?.GetCustomsRegNo(codeType) ??
								GlbCompany.CurrentCompany.OrgProxy.CustomsCodes.GetCustomsRegNo(codeType);
				if (brokerNum == messageData.ImporterID)
				{
					statement.B2_PaymentParty = PaymentPartyList.Codes.BRK;
					statement.B2_OH_Importer = GlbBranch.CurrentBranch?.OrgProxy?.PK ?? GlbCompany.CurrentCompany.OrgProxy.PK;
				}
				else
				{
					statement.B2_PaymentParty = PaymentPartyList.Codes.OWN;

					var cusCode = new OrgCusCode.Loader(message.Factory).Load(Core.Constants.CountryCodes.KoreaSouth, codeType, messageData.ImporterID);

					if (cusCode != null && cusCode.Length == 1)
					{
						statement.B2_OH_Importer = cusCode[0].OK_OH;
					}
					else if (messageData.Declarations != null)
					{
						var entry = MessageLinkedObjectManager.GetLinkedObject(message.Factory, message.Company, messageData.Declarations.FirstOrDefault().ImportDeclarationNumber, SharedJobMessageTypeList.Codes.Import);

						statement.B2_OH_Importer = entry?.Declaration?.JE_OH_DutyPayer ?? ZGuid.Empty;
					}
				}

				if (messageData.Declarations != null)
				{
					foreach (var goodsShipment in messageData.Declarations)
					{
						var statementLine = statement.StatementLines?.FirstOrDefault(x => x.B3_EntryNum == goodsShipment.ImportDeclarationNumber);
						if (statementLine == null)
						{
							statementLine = statement.StatementLines.AddNew();
							statementLine.B3_EntryNum = goodsShipment.ImportDeclarationNumber;
						}
						else
						{
							existingLines.Remove(statementLine);
						}

						statementLine.B3_SequenceNumber = goodsShipment.SequenceNumber;
						statementLine.B3_AssociatedEntry = goodsShipment.PaymentNumber;
						statementLine.B3_EntryType = SharedJobMessageTypeList.Codes.Import;
						statementLine.B3_EntryDate = goodsShipment.PaymentDate;
						statementLine.B3_CustomsFeesTotal = goodsShipment.Vat;

						var statementCharge = statementLine.Charges.FirstOrDefault(x => x.B4_ChargeType == ChargeTypeList.Codes.ValueForVAT);

						if (statementCharge == null)
						{
							statementCharge = statementLine.Charges.AddNew();
							statementCharge.B4_ChargeType = ChargeTypeList.Codes.ValueForVAT;
						}
						statementCharge.B4_ChargeAmount = goodsShipment.VATBaseAmount;
					}
				}
				existingLines.ForEach(x => x.Delete());

				if (messageData.TaxInvoiceCode == StatementCodeList.Codes.Code2 && messageData.TaxInvoiceType == StatementTypeList.Codes.Type02)
				{
					var statement5FY = new CusStatementHeader.Loader(message.Factory).Load(statement.B2_AccountNo, message.Branch.GB_GC, StatementHeaderTypeList.Codes.Invoice);
					if (statement5FY != null)
					{
						statement5FY.B2_PaymentStatus = StatementHeaderPaymentStatusList.Codes.PYC;
						statement5FY.B2_PaymentAuthorizationDate = messageData.PaymentDate;
						statement5FY.B2_AccountNo = messageData.TaxInvoiceNumber;
					}
				}
				message.EM_LinkedObject = statement;
				message.EM_MessageInterpretation = CreateUserFriendlyMessageInterpretation(messageData, MessageFunctions.MessageInterpretationMode.FullView);
				SendNotification(message, statement, messageData);
			}
		}

		#region SuppressResourceStringsCheckRegion
		void SendNotification(EDIMessage message, CusStatementHeader statement, IGOVCBR5FZMessageData messageData)
		{
			var notificationData = new NotificationData()
			{
				ControllerIDProvider = statement,
				MessagesParent = statement,
				JobNumberDescription = $"발행번호: {messageData.TaxInvoiceNumber}",
				Branch = message.Branch,
				EmailGroup = KRCustomsRegistry.Instance.ImportStatementEmailGroup,
				EmailBody = CreateUserFriendlyMessageInterpretation(messageData, MessageFunctions.MessageInterpretationMode.Email),
				MessageTypeDescription = $"[{ElectronicDocumentTypeList.Descriptions._5FZ}]",
				EntryNumber = messageData.TaxInvoiceNumber
			};
			NotificationSender.SendNotification(notificationData);
		}

		ZString CreateUserFriendlyMessageInterpretation(IGOVCBR5FZMessageData messageData, MessageFunctions.MessageInterpretationMode mode)
		{
			var result = new ZStringBuilder();

			var tableContents = new HtmlTableCreator(new string[] { "항 목", "내 용" });
			tableContents.WriteRow(new string[] { "계산서 종류", new StatementCodeList().GetDescriptionFromCode(messageData.TaxInvoiceCode) });
			tableContents.WriteRow(new string[] { "계산서 구분", new StatementTypeList().GetDescriptionFromCode(messageData.TaxInvoiceType) });
			tableContents.WriteRow(new string[] { "세금계산서 발행번호", MessageFunctions.GetFormattedNumber(messageData.TaxInvoiceNumber, new int[] { 0, 3, 5 }).ToString() });
			tableContents.WriteRow(new string[] { "월별납부서번호", messageData.SumPaymentNumber });
			tableContents.WriteRow(new string[] { "세관등록번호", messageData.CustomsOfficeID });
			tableContents.WriteRow(new string[] { "세관명", messageData.CustomsOfficeName });
			tableContents.WriteRow(new string[] { "주소", messageData.CustomsOfficeAddressLine1 });
			tableContents.WriteRow(new string[] { "납부자 사업자(주민)등록번호", messageData.ImporterID });
			tableContents.WriteRow(new string[] { "납부자 상호", messageData.ImporterCompanyName });
			tableContents.WriteRow(new string[] { "납부자 대표자명", messageData.ImporterRepresentativeName });
			tableContents.WriteRow(new string[] { "납부자 주소", messageData.ImporterAddressLine });
			tableContents.WriteRow(new string[] { "납부일(수납일)", messageData.PaymentDate.ToString(DateFormatType.DateKorean) });
			tableContents.WriteRow(new string[] { "공란수", messageData.BlankCount });
			tableContents.WriteRow(new string[] { "과세표준", messageData.TotalVATBaseAmount.ToString() });
			tableContents.WriteRow(new string[] { "부가세", messageData.TotalVAT.ToString() });

			if (messageData.PeriodStartDate != ZDate.Empty)
			{
				tableContents.WriteRow(new string[] { "일괄발급기간", messageData.PeriodStartDate.ToString(DateFormatType.DateKorean)
																+ " ~ " + messageData.PeriodEndDate.ToString(DateFormatType.DateKorean) });
			}
			else
			{
				tableContents.WriteRow(new string[] { "일괄발급기간", "" });
			}

			tableContents.WriteRow(new string[] { "비고", messageData.Remark });
			tableContents.WriteRow(new string[] { "총건수", messageData.TotalNumber.ToString() });
			result.Append(tableContents.ToHtml());

			var detailContents = new HtmlTableCreator(System.Array.Empty<string>()) { EnableHTMLEncoding = false };
			detailContents.WriteRowWithFormatting(new CellWithFormatting("개별세금 계산서 내역", new NameValueCollection { { "align", "left" }, { "colspan", "4" } }, true));
			detailContents.WriteRowWithFormatting(new CellWithFormatting("수입신고번호", new NameValueCollection { { "align", "center" }, { "width", "150" } }), new CellWithFormatting("개별납부번호", new NameValueCollection { { "align", "center" }, { "width", "150" } }),
													new CellWithFormatting("과세표준", new NameValueCollection { { "align", "center" }, { "width", "100" } }), new CellWithFormatting("부가세", new NameValueCollection { { "align", "center" }, { "width", "100" } }));
			var i = 0;
			foreach (var vat in messageData.Declarations)
			{
				if (mode == MessageFunctions.MessageInterpretationMode.Email && i > 9)
				{
					detailContents.WriteRowWithFormatting(new CellWithFormatting("나머지 내역은 프로그램에서 확인 하십시오.", new NameValueCollection { { "colspan", "4" } }));
					break;
				}
				detailContents.WriteRowWithFormatting(new CellWithFormatting(MessageFunctions.DeclarationNumberFormat(vat.ImportDeclarationNumber)),
																new CellWithFormatting(vat.PaymentNumber),
																new CellWithFormatting(vat.VATBaseAmount.ToString()),
																new CellWithFormatting(vat.Vat.ToString()));
				i += 1;
			}
			result.Append(detailContents.ToHtml());

			return result.ToString();
		}
		#endregion
	}
}
