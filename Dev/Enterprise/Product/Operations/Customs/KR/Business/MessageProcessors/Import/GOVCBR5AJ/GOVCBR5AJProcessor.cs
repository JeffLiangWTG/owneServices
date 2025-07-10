using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.Common.Shared;
using Enterprise.Customs.KR.Messaging;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.MessageProcessors;
using WTG.StaticAnalysis.Annotation;
using static Enterprise.Customs.KR.Messaging.Constants;

namespace Enterprise.Customs.KR.Business
{
	[CodeAlive("An instance is created via reflection using the attribute MessageType")]
	[MessageType(ElectronicDocumentTypeList.Codes._5AJ)]
	class GOVCBR5AJProcessor : IMessageProcessor
	{
		public void Process(EDIMessage message)
		{
			using (var textReader = message.GetEM_MessageTextReader())
			{
				var messageData = new GOVCBR5AJDataProvider().GetMessageData(textReader);
				var statement = new CusStatementHeader.Loader(message.Factory).Load(messageData.NoticeNumber, message.Branch.GB_GC, StatementHeaderTypeList.Codes.NormalReport);
				var existingLines = new List<CusStatementLine>();
				var existingLineCharges = new List<CusStatementLineCharge>();

				if (statement == null)
				{
					statement = message.Factory.New<CusStatementHeader>();
					statement.B2_StatementNumber = messageData.NoticeNumber;
					statement.B2_GC = message.Branch.GB_GC;
					statement.B2_StatementType = StatementHeaderTypeList.Codes.NormalReport;
				}
				else
				{
					if (statement.StatementLines.Count != ZInt.Zero)
					{
						existingLines.AddRange(statement.StatementLines.Cast<CusStatementLine>());
					}
				}
				statement.B2_PaymentStatus = StatementHeaderPaymentStatusList.Codes.PYC;
				statement.B2_ProcessPort = messageData.NoticeNumber.SubstringSafe(0, 3);
				statement.B2_IsMonthlyStatement = true;
				statement.B2_AccountNo = messageData.PaymentNumber;
				statement.B2_ImporterCustomsID = messageData.ImporterID;
				var entryType = messageData.DeclarationNumberType == ExportImportTypeCodeList.Codes._01 ? SharedJobMessageTypeList.Codes.Import : SharedJobMessageTypeList.Codes.Export;
				var codeType = statement.B2_ImporterCustomsID.Length == 13 ? IdentificationType.KoreanRegNoForResident : IdentificationType.BusinessRegNo;

				var cusCodes = new OrgCusCode.Loader(message.Factory).Load(Core.Constants.CountryCodes.KoreaSouth, codeType, messageData.ImporterID);

				if (cusCodes != null && cusCodes.Length == 1)
				{
					statement.B2_OH_Importer = cusCodes[0].OK_OH;
				}
				else
				{
					var entry = MessageLinkedObjectManager.GetLinkedObject(message.Factory, message.Company, messageData.GoodsShipments.FirstOrDefault().DeclarationNumber, entryType);

					statement.B2_OH_Importer = entry?.Declaration?.JE_OH_DutyPayer ?? ZGuid.Empty;
				}
				statement.B2_ProcessDate = Env.Time.GetLocalTimeFromUtc(message.EM_MessageDateTime.ToDateTime());

				foreach (var goodsShipment in messageData.GoodsShipments)
				{
					var statementLine = statement.StatementLines?.FirstOrDefault(x => x.B3_EntryNum == goodsShipment.DeclarationNumber);
					if (statementLine == null)
					{
						statementLine = statement.StatementLines.AddNew();
						statementLine.B3_EntryNum = goodsShipment.DeclarationNumber;
					}
					else
					{
						existingLines.Remove(statementLine);
						if (statementLine.Charges.Count != ZInt.Zero)
						{
							existingLineCharges.AddRange(statementLine.Charges.Cast<CusStatementLineCharge>());
						}
					}
					statementLine.B3_EntryType = entryType;
					statementLine.B3_CustomsFeesTotal = goodsShipment.TemporaryOpeningFee + goodsShipment.InspectionFee + goodsShipment.OtherFee;
					statement.B2_StatementAmount += statementLine.B3_CustomsFeesTotal;

					var dutyTaxDict = new Dictionary<ZString, ZDecimal>();
					dutyTaxDict.Add(ChargeTypeList.Codes.DIF, goodsShipment.TemporaryOpeningFee);
					dutyTaxDict.Add(ChargeTypeList.Codes.PAF, goodsShipment.InspectionFee);
					dutyTaxDict.Add(ChargeTypeList.Codes.TOF, goodsShipment.OtherFee);

					foreach (var dutyTax in dutyTaxDict)
					{
						if (dutyTax.Value > 0)
						{
							var statementCharge = statementLine.Charges.FirstOrDefault(x => x.B4_ChargeType == dutyTax.Key);
							if (statementCharge == null)
							{
								statementCharge = statementLine.Charges.AddNew();
								statementCharge.B4_ChargeType = dutyTax.Key;
							}
							else
							{
								existingLineCharges.Remove(statementCharge);
							}
							statementCharge.B4_ChargeAmount = dutyTax.Value;
						}
					}
				}
				existingLineCharges.ForEach(x => x.Delete());
				existingLines.ForEach(x => x.Delete());

				var statement5JG = new CusStatementHeader.Loader(message.Factory).Load(messageData.PaymentNumber, message.Branch.GB_GC, StatementHeaderTypeList.Codes.Normal);
				if (statement5JG != null)
				{
					statement5JG.B2_PaymentStatus = StatementHeaderPaymentStatusList.Codes.PYC;
					statement5JG.B2_CheckNo = messageData.NoticeNumber;
				}

				message.EM_LinkedObject = statement;
				message.EM_MessageInterpretation = CreateUserFriendlyMessageInterpretation(messageData, MessageFunctions.MessageInterpretationMode.FullView);
				SendNotification(message, statement, messageData);
			}
		}

		#region SuppressResourceStringsCheckRegion
		void SendNotification(EDIMessage message, CusStatementHeader statement, IGOVCBR5AJMessageData messageData)
		{
			var notificationData = new NotificationData()
			{
				ControllerIDProvider = statement,
				MessagesParent = statement,
				JobNumberDescription = $"발행번호: {messageData.NoticeNumber}",
				Branch = message.Branch,
				EmailGroup = KRCustomsRegistry.Instance.ImportStatementEmailGroup,
				EmailBody = CreateUserFriendlyMessageInterpretation(messageData, MessageFunctions.MessageInterpretationMode.Email),
				MessageTypeDescription = $"[{ElectronicDocumentTypeList.Descriptions._5AJ}]",
				EntryNumber = messageData.NoticeNumber
			};
			NotificationSender.SendNotification(notificationData);
		}

		string CreateUserFriendlyMessageInterpretation(IGOVCBR5AJMessageData messageData, MessageFunctions.MessageInterpretationMode mode)
		{
			var result = new ZStringBuilder();
			var exportImportTypeCodeList = new ExportImportTypeCodeList();
			var nonTaxIncomeFeeTypeCodeList = new NonTaxIncomeFeeTypeCodeList();

			var tableContents = new HtmlTableCreator(new string[] { "항 목", "내 용" });
			tableContents.WriteRow(new string[] { "통지번호", MessageFunctions.GetFormattedNumber(messageData.NoticeNumber, new int[] { 0, 3, 5 }).ToString() });
			tableContents.WriteRow(new string[] { "신고자", messageData.DeclarantID });
			tableContents.WriteRow(new string[] { "화주", messageData.ImporterID });
			tableContents.WriteRow(new string[] { "수출입 구분", exportImportTypeCodeList.GetDescriptionFromCode(messageData.DeclarationNumberType) });
			tableContents.WriteRow(new string[] { "세외수입수수료구분", nonTaxIncomeFeeTypeCodeList.GetDescriptionFromCode(messageData.DutyTaxFreeType) });
			tableContents.WriteRow(new string[] { "임시개청 번호", MessageFunctions.GetFormattedNumber(messageData.ApplicationNumber, new int[] { 0, 5, 7 }).ToString() });
			tableContents.WriteRow(new string[] { "선사항공사부호", messageData.CarrierID });
			tableContents.WriteRow(new string[] { "보세운송업자부호", messageData.AgentID });
			tableContents.WriteRow(new string[] { "고지번호", MessageFunctions.GetFormattedNumber(messageData.PaymentNumber, new int[] { 0, 3, 5, 7 }).ToString() });
			result.Append(tableContents.ToHtml());

			var detailContents = new HtmlTableCreator(System.Array.Empty<string>()) { EnableHTMLEncoding = false };
			detailContents.WriteRowWithFormatting(new CellWithFormatting("대체경비목록", new NameValueCollection { { "align", "left" }, { "colspan", "4" } }, true));
			detailContents.WriteRowWithFormatting(new CellWithFormatting("수입대체경비관련번호", new NameValueCollection { { "align", "center" }, { "width", "100" } }), new CellWithFormatting("임시개청수수료", new NameValueCollection { { "align", "center" }, { "width", "100" } }), new CellWithFormatting("파출검사수수료", new NameValueCollection { { "align", "center" }, { "width", "100" } }), new CellWithFormatting("기타수수료", new NameValueCollection { { "align", "center" }, { "width", "100" } }));
			var i = 0;
			foreach (var goodsShipment in messageData.GoodsShipments)
			{
				if (mode == MessageFunctions.MessageInterpretationMode.Email && i > 9)
				{
					detailContents.WriteRowWithFormatting(new CellWithFormatting("나머지 내역은 프로그램에서 확인 하십시오.", new NameValueCollection { { "colspan", "4" } }));
					break;
				}
				detailContents.WriteRowWithFormatting(new CellWithFormatting(goodsShipment.DeclarationNumber, new NameValueCollection { { "align", "left" } }), new CellWithFormatting(goodsShipment.TemporaryOpeningFee.ToString(), new NameValueCollection { { "align", "right" } }), new CellWithFormatting(goodsShipment.InspectionFee.ToString(), new NameValueCollection { { "align", "right" } }), new CellWithFormatting(goodsShipment.OtherFee.ToString(), new NameValueCollection { { "align", "right" } }));

				i += 1;
			}
			result.Append(detailContents.ToHtml());

			return result.ToString();
		}
		#endregion
	}
}
