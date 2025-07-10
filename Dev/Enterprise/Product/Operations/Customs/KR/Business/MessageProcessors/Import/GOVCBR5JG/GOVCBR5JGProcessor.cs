using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.KR.Messaging;
using Enterprise.Messaging.MessageProcessors;
using WTG.StaticAnalysis.Annotation;
using static Enterprise.Customs.KR.Messaging.Constants;
using static Enterprise.Messaging.Business.HtmlResponseEmailGenerator;

namespace Enterprise.Customs.KR.Business
{
	[CodeAlive("An instance is created via reflection using the attribute MessageType")]
	[MessageType(ElectronicDocumentTypeList.Codes._5JG)]
	class GOVCBR5JGProcessor : IMessageProcessor
	{
		public void Process(EDIMessage message)
		{
			using (var textReader = message.GetEM_MessageTextReader())
			{
				var messageData = new GOVCBR5JGDataProvider().GetMessageData(textReader);

				#region statementHeader
				var statementHeader = new CusStatementHeader.Loader(message.Factory).Load(messageData.PaymentNumber, message.Branch.GB_GC, StatementHeaderTypeList.Codes.Normal);
				var existingLines = new List<CusStatementLine>();
				var existingLineCharges = new List<CusStatementLineCharge>();
				if (statementHeader == null)
				{
					statementHeader = message.Factory.New<CusStatementHeader>();
					statementHeader.B2_StatementNumber = messageData.PaymentNumber;
					statementHeader.B2_GC = message.Branch.GB_GC;
					statementHeader.B2_StatementType = StatementHeaderTypeList.Codes.Normal;
				}
				else
				{
					if (statementHeader.StatementLines.Count != ZInt.Zero)
					{
						existingLines.AddRange(statementHeader.StatementLines.Cast<CusStatementLine>());
					}
				}
				statementHeader.B2_DueDate = messageData.PaymentDate.AddDays(15);
				statementHeader.B2_ProcessDate = messageData.PaymentDate;
				statementHeader.B2_StatementAmount = messageData.NoticeAmount;
				statementHeader.B2_PaymentStatus = CustomsEntryStatusTypeList.Codes.PYI;
				statementHeader.B2_ProcessPort = messageData.PaymentNumber.Substring(0, 3);
				statementHeader.B2_IsMonthlyStatement = true;
				statementHeader.B2_AccountNo = messageData.BankAccountNumber;
				statementHeader.B2_PeriodStartDate = messageData.PaymentDate.AddMonths(-1).AddDays(1 - messageData.PaymentDate.Day);
				statementHeader.B2_PeriodEndDate = messageData.PaymentDate.AddDays(0 - messageData.PaymentDate.Day);
				statementHeader.PayerFromCustoms = messageData.ImportCompanyName + "\r\n" + messageData.ImportRepresentativeName + "\r\n" + messageData.ImportAddressLine1;
				#endregion

				#region statementLine & LineCharge
				var statementLine = statementHeader.StatementLines.FirstOrDefault(x => x.B3_EntryNum == messageData.PaymentNumber);
				if (statementLine == null)
				{
					statementLine = statementHeader.StatementLines.AddNew();
					statementLine.B3_EntryNum = messageData.PaymentNumber;
				}
				else
				{
					existingLines.Remove(statementLine);
					if (statementLine.Charges.Count != ZInt.Zero)
					{
						existingLineCharges.AddRange(statementLine.Charges.Cast<CusStatementLineCharge>());
					}
				}
				statementLine.B3_EntryType = Constants.EntryTypeForStatementLine.OtherImportCost;
				statementLine.B3_CustomsFeesTotal = messageData.NoticeAmount;

				var dutyTaxFeeList = new Dictionary<ZString, ZDecimal>();
				dutyTaxFeeList.Add(ChargeTypeList.Codes.DIF, messageData.TemporaryOpeningFee);
				dutyTaxFeeList.Add(ChargeTypeList.Codes.PAF, messageData.InspectionFee);
				dutyTaxFeeList.Add(ChargeTypeList.Codes.TOF, messageData.PermissionApplicationFee);

				foreach (var dutyTax in dutyTaxFeeList)
				{
					if (dutyTax.Value > ZInt.Zero)
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
				existingLineCharges.ForEach(x => x.Delete());
				existingLines.ForEach(x => x.Charges.DeleteAll());
				existingLines.ForEach(x => x.Delete());
				#endregion

				message.EM_LinkedObject = statementHeader;
				message.EM_MessageInterpretation = CreateUserFriendlyMessageInterpretation(messageData, false);
				SendNotification(statementHeader, message, messageData);
			}
		}

		#region SuppressResourceStringsCheckRegion
		void SendNotification(CusStatementHeader statementHeader, EDIMessage message, IGOVCBR5JGMessageData messageData)
		{
			var notificationData = new NotificationData()
			{
				ControllerIDProvider = statementHeader,
				MessagesParent = statementHeader,
				JobNumberDescription = $"발행번호: {messageData.PaymentNumber}",
				Branch = message.Branch,
				EmailGroup = KRCustomsRegistry.Instance.ImportStatementEmailGroup,
				EmailBody = CreateUserFriendlyMessageInterpretation(messageData),
				MessageTypeDescription = $"[{ElectronicDocumentTypeList.Descriptions._5JG}]",
				EntryNumber = messageData.PaymentNumber,
			};
			NotificationSender.SendNotification(notificationData);
		}

		string CreateUserFriendlyMessageInterpretation(IGOVCBR5JGMessageData messageData, bool isUsingEmail = true)
		{
			var caption = "Caption: 수입 납부고지서 및 세금계산서 수신 그룹.";
			var message = "Message: 수입 납부고지 및 세금계산서 & 경비내역서 알림을 받을 직원 그룹.";
			var nonTaxIncomeFeeType = new NonTaxIncomeFeeTypeCodeList();

			var tableContents = new HtmlTableCreator(new string[] { "항 목", "내 용" });
			tableContents.WriteRow(new string[] { "고지번호", MessageFunctions.GetFormattedNumber(messageData.PaymentNumber, new int[] { 0, 3, 5, 7 }) });
			tableContents.WriteRow(new string[] { "납부자 상호", messageData.ImportCompanyName });
			tableContents.WriteRow(new string[] { "납부자 성명", messageData.ImportRepresentativeName });
			tableContents.WriteRow(new string[] { "납부자 주소", messageData.ImportAddressLine1 });
			tableContents.WriteRow(new string[] { "수입대체경비 징수관서", messageData.CustomsOffice });
			tableContents.WriteRow(new string[] { "통지일자", messageData.PaymentDate.ToString(DateFormatType.DateKorean) });
			tableContents.WriteRow(new string[] { "세외수입수수료구분", nonTaxIncomeFeeType.GetDescriptionFromCode(messageData.DutyTaxFeeType) });
			tableContents.WriteRow(new string[] { "관세납부전용 계좌번호", messageData.BankAccountNumber });
			tableContents.WriteRow(new string[] { "세입징수관 계좌번호", messageData.CustomsOfficeBankAccountNumber });
			tableContents.WriteRow(new string[] { "고지금액", messageData.NoticeAmount.ToString() });
			tableContents.WriteRow(new string[] { "임시개청수수료", messageData.TemporaryOpeningFee.ToString() });
			tableContents.WriteRow(new string[] { "파출검사수수료", messageData.InspectionFee.ToString() });
			tableContents.WriteRow(new string[] { "허가신청수수료", messageData.PermissionApplicationFee.ToString() });
			tableContents.WriteRow(new string[] { "전자납부번호", MessageFunctions.GetFormattedNumber(messageData.ElectronNoticeNumber, new int[] { 0, 4, 7, 9, 11 }) });
			tableContents.WriteRow(new string[] { "선사항공사부호", messageData.CarrierID });
			tableContents.WriteRow(new string[] { "보세운송업자부호", messageData.AgentID });
			tableContents.WriteRow(new string[] { "신고자", messageData.DeclarantID });

			if (isUsingEmail)
			{
				return caption + HtmlConstants.Br + message + tableContents.ToHtml();
			}
			else
			{
				return tableContents.ToHtml();
			}
		}
		#endregion
	}
}
