using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.KR;
using Enterprise.Customs.Common.Shared;
using Enterprise.Customs.KR.Messaging;
using Enterprise.Messaging.Integration;
using NUnit.Framework;

namespace Enterprise.Customs.KR.Business.Testing
{
	[TestedType(typeof(PenaltyRefundRequestMessageSendingObject))]
	public class PenaltyRefundRequestMessageSendingObjectTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			var statement = Factory.New<CusStatementHeader>();
			statement.B2_StatementType = StatementHeaderTypeList.Codes.Invoice;
			statement.B2_PaymentStatus = StatementHeaderPaymentStatusList.Codes.PYC;
			var statementLine = statement.StatementLines.AddNew();
			statementLine.B3_EntryNum = "1234520000045M";
			statementLine.B3_EntryType = SharedJobMessageTypeList.Codes.Import;
			Factory.Save();

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = KRJobMessageTypeList.Codes.Import;
			var entry = declaration.CustomsEntryHeaders.AddNew();
			var entryNumber = entry.EntryNumbers.AddNew();
			entryNumber.CE_EntryType = ElectronicDocumentTypeList.Codes._5UL;
			entryNumber.CE_EntryNum = "AAA111";
			return new PenaltyRefundRequestMessageSendingObject(entry, "11111", entryNumber);
		}

		public void TestAmount()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = KRJobMessageTypeList.Codes.Import;
			var entry = declaration.CustomsEntryHeaders.AddNew();

			var instruction = Factory.New<CusEntryInstruction>();
			instruction.CEI_JE = declaration.PK;
			instruction.CEI_RefundType = RefundTypeList.Codes.A;

			entry.CH_MessageType = JobMessageTypeList.Codes.Import;
			entry.CH_CEI_Instruction = instruction.PK;

			var entryLine = entry.MergedLines.AddNew();

			var entryNumber = entry.EntryNumbers.AddNew();
			entryNumber.CE_EntryType = ElectronicDocumentTypeList.Codes._5UL;
			entryNumber.CE_EntryNum = "AAA111";
			var sendingObject = new PenaltyRefundRequestMessageSendingObject(entry, "11111", entryNumber);
			var refundDetails = new GOVCBR5ULDetails(sendingObject);

			var import5UL = new Import5ULCreator().Create(entry, refundDetails);
			AssertNotNull(import5UL.EntryLines);
			AssertEquals(1, import5UL.EntryLines.Length);
			var import5ULLine = import5UL.EntryLines[0];
			AssertNotNull(import5ULLine.TaxItems);
			var taxItems = import5ULLine.TaxItems.ToList();
			var otherTaxItems = import5ULLine.OtherTaxItems.ToList();
			AssertPenaltyAmount(taxItems, ZInt.Zero);
			AssertTax(taxItems, otherTaxItems, ZInt.Zero);

			entryLine.CL_ValueForVAT = 8000m;
			entryLine.CL_ValueExemptForVAT = 9000m;

			CreateCharge(ChargeTypeList.Codes.Duty, 100);
			CreateCharge(ChargeTypeList.Codes.EducationTax, 200);
			CreateCharge(ChargeTypeList.Codes.AgricultureTax, 300);
			CreateCharge(ChargeTypeList.Codes.VAT, 400);
			CreateCharge(ChargeTypeList.Codes.LiquorTax, 500);
			CreateCharge(ChargeTypeList.Codes.SpecialConsumptionTax, 600);
			CreateCharge(ChargeTypeList.Codes.TransportationTax, 700);
			CreateCharge(ChargeTypeList.Codes.PenaltyForLateDeclaration, 800);
			CreateCharge(ChargeTypeList.Codes.PenaltyForMissedDeclaration, 900);

			Factory.Save();

			sendingObject = new PenaltyRefundRequestMessageSendingObject(entry, "22222", entryNumber);

			AssertEquals(110m, sendingObject.DutyRefundAmount);
			AssertEquals(210m, sendingObject.EDTRefundAmount);
			AssertEquals(310m, sendingObject.AGTRefundAmount);
			AssertEquals(410m, sendingObject.VATRefundAmount);
			AssertEquals(510m, sendingObject.LQTRefundAmount);
			AssertEquals(610m, sendingObject.SCTRefundAmount);
			AssertEquals(710m, sendingObject.TRTRefundAmount);
			AssertEquals(810m, sendingObject.PenaltyForLateDeclarationRefundAmount);
			AssertEquals(910m, sendingObject.PenaltyForMissedDeclarationRefundAmount);

			sendingObject.DutyPenaltyToRefund = 100m;
			sendingObject.EDTPenaltyToRefund = 200m;
			sendingObject.AGTPenaltyToRefund = 300m;
			sendingObject.VATPenaltyToRefund = 400m;
			sendingObject.LQTPenaltyToRefund = 500m;
			sendingObject.SCTPenaltyToRefund = 600m;
			sendingObject.TRTPenaltyToRefund = 700m;

			sendingObject.DutyRefundAmount = 1000m;
			sendingObject.EDTRefundAmount = 2000m;
			sendingObject.AGTRefundAmount = 3000m;
			sendingObject.VATRefundAmount = 4000m;
			sendingObject.LQTRefundAmount = 5000m;
			sendingObject.SCTRefundAmount = 6000m;
			sendingObject.TRTRefundAmount = 7000m;
			sendingObject.PenaltyForLateDeclarationRefundAmount = 10000m;
			sendingObject.PenaltyForMissedDeclarationRefundAmount = 11000m;
			sendingObject.LatePaymentRefundAmount = 12000m;
			sendingObject.NonDutyTaxRefundAmount = 13000m;

			AssertEquals(8000m, sendingObject.ValueForVATRefundAmount);
			AssertEquals(9000m, sendingObject.VATExemptionValueRefundAmount);

			refundDetails = new GOVCBR5ULDetails(sendingObject);
			import5UL = new Import5ULCreator().Create(entry, refundDetails);
			import5ULLine = import5UL.EntryLines[0];
			taxItems = import5ULLine.TaxItems.ToList();
			otherTaxItems = import5ULLine.OtherTaxItems.ToList();
			AssertPenaltyAmount(taxItems, 100);
			AssertTax(taxItems, otherTaxItems, 1000);

			sendingObject.AGTRefundAmount = 0m;
			AssertEquals(0m, sendingObject.AGTRefundAmount);
			refundDetails = new GOVCBR5ULDetails(sendingObject);
			import5UL = new Import5ULCreator().Create(entry, refundDetails);
			import5ULLine = import5UL.EntryLines[0];
			taxItems = import5ULLine.TaxItems.ToList();
			AssertEquals(0m, taxItems.FirstOrDefault(x => x.TaxItem == EntryTaxTypeList.Codes.CAP).Tax);

			void CreateCharge(ZString type, ZDecimal amount)
			{
				var charge = entry.Charges.AddNew();
				charge.C1_ChargeType = type;
				charge.C1_ChargeAmount = amount;

				var charge2 = entry.Charges.AddNew();
				charge2.C1_ChargeType = type;
				charge2.C1_ChargeAmount = 10m;
			}
		}

		readonly string[] otherTaxItemTypeList = { EntryTaxTypeList.Codes._5CQ, EntryTaxTypeList.Codes._5CR, EntryTaxTypeList.Codes._5AC, EntryTaxTypeList.Codes._5AY, EntryTaxTypeList.Codes._5CT, EntryTaxTypeList.Codes._5CS };
		readonly string[] taxItemTypeList = { EntryTaxTypeList.Codes.CUD, EntryTaxTypeList.Codes._5AB, EntryTaxTypeList.Codes.CAP, EntryTaxTypeList.Codes.VAT, EntryTaxTypeList.Codes.ACT, EntryTaxTypeList.Codes.IND, EntryTaxTypeList.Codes.ENV };

		void AssertPenaltyAmount(List<Import5ULTaxItem> taxItems, ZInt taxMultiplier)
		{
			AssertEquals(7, taxItems.Count);
			for(int i = 1; i < taxItemTypeList.Length; i++)
			{
				AssertEquals(i * taxMultiplier, (ZInt)taxItems.FirstOrDefault(x => x.TaxItem == taxItemTypeList[i - 1]).PenaltyAmount);
			}
		}

		void AssertTax(List<Import5ULTaxItem> taxItems, List<Import5ULTaxItem> otherTaxItems, ZInt taxMultiplier)
		{
			AssertEquals(7, taxItems.Count);
			for (int i = 1; i < taxItemTypeList.Length; i++)
			{
				AssertEquals(i * taxMultiplier, (ZInt)taxItems.FirstOrDefault(x => x.TaxItem == taxItemTypeList[i - 1]).Tax);
			}

			AssertEquals(6, otherTaxItems.Count);
			for (int i = 8; i < otherTaxItemTypeList.Length; i++)
			{
				AssertEquals(i * taxMultiplier, (ZInt)otherTaxItems.FirstOrDefault(x => x.TaxItem == otherTaxItemTypeList[i - 8]).Tax);
			}
		}

		[TestDate(2024, 01, 01)]
		public void TestCusStatementAndMessageAndAddInfo()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = KRJobMessageTypeList.Codes.Import;
			declaration.JE_TaxOffice = "613";
			var entry = declaration.CustomsEntryHeaders.AddNew();
			entry.EntryNumber = "1";
			var entryLine = entry.MergedLines.AddNew();

			var refundEntryNumber = entry.EntryNumbers.AddNew();
			refundEntryNumber.CE_EntryType = ElectronicDocumentTypeList.Codes._5UL;
			refundEntryNumber.CE_EntryNum = "AAA111";

			var message5FE = entry.Messages.AddNew();
			message5FE.EM_MessageNum = "10";
			message5FE.EM_MessageType = ElectronicDocumentTypeList.Codes._5FE;
			message5FE.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			message5FE.EM_ApplicationReference = "3";
			Factory.Save();

			var message5FK = entry.Messages.AddNew();
			message5FK.EM_MessageNum = "11";
			message5FK.EM_ReceiveTransmit = ReceiveTransmitList.Codes.Receive;
			message5FK.EM_MessageType = ElectronicDocumentTypeList.Codes._5FK;
			message5FK.EM_MessageOwner = CustomsEntryStatusTypeList.Codes.ANT;
			message5FK.EM_ApplicationReference = message5FE.EM_MessageNum;

			var statement = Factory.New<CusStatementHeader>();
			statement.B2_StatementType = StatementHeaderTypeList.Codes.CustomsDisbursementBill;
			statement.B2_StatementNumber = "11111";
			statement.B2_Status = StatementHeaderStatusList.Codes.A;
			statement.B2_PaymentStatus = StatementHeaderPaymentStatusList.Codes.PYC;
			statement.B2_IncomingMessageNo = message5FK.EM_MessageNum;

			var statementLine = statement.StatementLines.AddNew();
			statementLine.B3_EntryType = SharedJobMessageTypeList.Codes.Import;
			statementLine.B3_EntryNum = entry.EntryNumber;
			statementLine.B3_CustomsFeesTotal = 300m;

			var instruction = declaration.CustomsEntryInstructions.AddNew();
			entry.CH_CEI_Instruction = instruction.PK;

			var amendSessionalData = instruction.AmendmentSessionalDataCollection.AddNew();
			amendSessionalData.CSI_ItemNumber = 1;
			amendSessionalData.CSI_Code = DutyTaxCorrectionCodeList.Codes.C;
			amendSessionalData.CSI_LineNo = 3;
			amendSessionalData.CSI_DateOfIssue = new ZDateTime(2024, 2, 1);
			var refundSessionalData = amendSessionalData.RefundSessionalDataCollection.AddNew();
			Factory.Save();

			var sendingObject = new PenaltyRefundRequestMessageSendingObject(entry, "11111", refundEntryNumber);
			sendingObject.Amendment5WNNumber = 1;
			entry.Reload();
			AssertEquals("A (수입 보정 세액 납부)", sendingObject.BillStatus);
			AssertEquals("PYC (납부완료)", sendingObject.PaymentStatus);
			AssertEquals(300m, sendingObject.PaymentAmount);
			AssertEquals("613", sendingObject.TaxOffice);
			AssertEquals("A", sendingObject.RefundType);

			AssertNotNull(sendingObject.AmendmentSessionalData);
			AssertEquals(new ZDateTime(2024, 2, 1), sendingObject.SubmissionDate);
			AssertEquals("03", sendingObject.AmendmentVersionNoCW1);
			AssertEquals(2u, sendingObject.AmendmentVersionNoCustoms);

			statement.B2_Status = StatementHeaderStatusList.Codes.Z;
			sendingObject = new PenaltyRefundRequestMessageSendingObject(entry, "11111", refundEntryNumber);
			sendingObject.Amendment5WNNumber = 1;
			AssertNotNull(sendingObject.AmendmentSessionalData);
			AssertEquals(new ZDateTime(2024, 2, 1), sendingObject.SubmissionDate);
			AssertEquals("03", sendingObject.AmendmentVersionNoCW1);
			AssertEquals(2u, sendingObject.AmendmentVersionNoCustoms);

			statement.B2_Status = StatementHeaderStatusList.Codes.B;
			sendingObject = new PenaltyRefundRequestMessageSendingObject(entry, "11111", refundEntryNumber);
			sendingObject.Amendment5WNNumber = 1;
			AssertNotNull(sendingObject.AmendmentSessionalData);
			AssertEquals(new ZDateTime(2024, 2, 1), sendingObject.SubmissionDate);
			AssertEquals("03", sendingObject.AmendmentVersionNoCW1);
			AssertEquals(2u, sendingObject.AmendmentVersionNoCustoms);

			var incomingMessage = entry.Messages.AddNew();
			incomingMessage.EM_MessageType = ElectronicDocumentTypeList.Codes._R99;
			incomingMessage.EM_MessageSubType = ElectronicDocumentTypeList.Codes._5FE;
			incomingMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			incomingMessage.EM_MessageNum = "12";
			incomingMessage.EM_ApplicationReference = message5FE.EM_MessageNum;
			Factory.Save();

			sendingObject = new PenaltyRefundRequestMessageSendingObject(entry, "11111", refundEntryNumber);
			sendingObject.Amendment5WNNumber = 1;
			AssertNotNull(sendingObject.AmendmentSessionalData);
			AssertEquals(new ZDateTime(2024, 2, 1), sendingObject.SubmissionDate);
			AssertEquals("03", sendingObject.AmendmentVersionNoCW1);
			AssertEquals(1u, sendingObject.AmendmentVersionNoCustoms);

			sendingObject = new PenaltyRefundRequestMessageSendingObject(entry, "11111", refundEntryNumber);
			sendingObject.Amendment5WNNumber = 2;
			AssertNull(sendingObject.AmendmentSessionalData);
			AssertEquals(ZDateTime.Empty, sendingObject.SubmissionDate);
			AssertEquals(ZString.Empty, sendingObject.AmendmentVersionNoCW1);
			AssertEquals(ZShort.Zero, sendingObject.AmendmentVersionNoCustoms);
		}
		public void TestFormattedCustomsDisbursementBillNumber()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = KRJobMessageTypeList.Codes.Import;
			var entry = declaration.CustomsEntryHeaders.AddNew();
			var instruction = declaration.CustomsEntryInstructions.AddNew();
			entry.CH_CEI_Instruction = instruction.PK;
			var refundSessionalData = entry.EntryInstruction.RefundSessionalDataCollection.AddNew();
			var refundEntryNumber = entry.EntryNumbers.AddNew();
			refundEntryNumber.CE_EntryType = ElectronicDocumentTypeList.Codes._5UL;
			refundEntryNumber.CE_EntryNum = "AAA111";

			var statement = Factory.New<CusStatementHeader>();
			statement.B2_StatementType = StatementHeaderTypeList.Codes.CustomsDisbursementBill;
			statement.B2_StatementNumber = "0127030112200237050";

			var sendingObject = new PenaltyRefundRequestMessageSendingObject(entry, statement.B2_StatementNumber, refundEntryNumber);
			AssertEquals("0127030112200237050", sendingObject.CustomsDisbursementBillNumber);
			AssertEquals("0127-030-11-22-0-023705-0", sendingObject.FormattedCustomsDisbursementBillNumber);
		}

		public void TestCaptions()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = KRJobMessageTypeList.Codes.Import;
			var entry = declaration.CustomsEntryHeaders.AddNew();
			var instruction = declaration.CustomsEntryInstructions.AddNew();
			entry.CH_CEI_Instruction = instruction.PK;
			var refundSessionalData = entry.EntryInstruction.RefundSessionalDataCollection.AddNew();
			var refundEntryNumber = entry.EntryNumbers.AddNew();
			refundEntryNumber.CE_EntryType = ElectronicDocumentTypeList.Codes._5UL;
			refundEntryNumber.CE_EntryNum = "AAA111";

			var statement = Factory.New<CusStatementHeader>();
			statement.B2_StatementType = StatementHeaderTypeList.Codes.CustomsDisbursementBill;
			statement.B2_StatementNumber = "0127030112200237050";

			var sendingObject = new PenaltyRefundRequestMessageSendingObject(entry, statement.B2_StatementNumber, refundEntryNumber);
			AssertHasCustomAttribute<ResourceStringDataAttribute>(sendingObject.GetType(), "FormattedCustomsDisbursementBillNumber", true, attrib => attrib.Caption == "Customs Disbursement Bill #");
			AssertHasCustomAttribute<ResourceStringDataAttribute>(sendingObject.GetType(), "Amendment5WNNumber", true, attrib => attrib.ShortCaption == "5WN Version No.");
			AssertHasCustomAttribute<ResourceStringDataAttribute>(sendingObject.GetType(), "Amendment5WNNumber", true, attrib => attrib.Caption == "5WN Version Number");
			AssertHasCustomAttribute<ResourceStringDataAttribute>(sendingObject.GetType(), "SubmissionDate", true, attrib => attrib.Caption == "Submission Date");
			AssertHasCustomAttribute<ResourceStringDataAttribute>(sendingObject.GetType(), "AmendmentVersionNoCustoms", true, attrib => attrib.ShortCaption == "Amend Version No.");
			AssertHasCustomAttribute<ResourceStringDataAttribute>(sendingObject.GetType(), "AmendmentVersionNoCustoms", true, attrib => attrib.Caption == "Amend Version Number");
			AssertHasCustomAttribute<ResourceStringDataAttribute>(sendingObject.GetType(), "PaymentAmount", true, attrib => attrib.Caption == "Payment Amount");
			AssertHasCustomAttribute<ResourceStringDataAttribute>(sendingObject.GetType(), "RefundType", true, attrib => attrib.Caption == "Refund Type");
			AssertHasCustomAttribute<ResourceStringDataAttribute>(sendingObject.GetType(), "RefundCause", true, attrib => attrib.Caption == "Refund Cause");
			AssertHasCustomAttribute<ResourceStringDataAttribute>(sendingObject.GetType(), "RefundReason", true, attrib => attrib.Caption == "Refund Reason");
			AssertHasCustomAttribute<ResourceStringDataAttribute>(sendingObject.GetType(), "TaxOffice", true, attrib => attrib.Caption == "Tax Office");
		}
		public void TestCusStatementProperties()
		{
			var statement = Factory.New<CusStatementHeader>();
			statement.B2_StatementType = StatementHeaderTypeList.Codes.CustomsDisbursementBill;
			statement.B2_StatementNumber = "11111";
			statement.B2_Status = StatementHeaderStatusList.Codes.A;
			statement.B2_PaymentStatus = StatementHeaderPaymentStatusList.Codes.PYC;

			var statementLine = statement.StatementLines.AddNew();
			statementLine.B3_EntryType = SharedJobMessageTypeList.Codes.Import;
			statementLine.B3_EntryNum = "1";
			statementLine.B3_CustomsFeesTotal = 300m;
			Factory.Save();

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = KRJobMessageTypeList.Codes.Import;
			declaration.JE_TaxOffice = "613";

			var entry_AttachedStatement = declaration.CustomsEntryHeaders.AddNew();
			entry_AttachedStatement.EntryNumber = "1";
			var entry_NotAttachedStatement = declaration.CustomsEntryHeaders.AddNew();
			entry_NotAttachedStatement.EntryNumber = "2";
			AssertProperties(entry_AttachedStatement, "A (수입 보정 세액 납부)", "PYC (납부완료)", 300m);
			AssertProperties(entry_NotAttachedStatement, ZString.Empty, ZString.Empty, ZDecimal.Zero);

			void AssertProperties(CusEntryHeader entry, ZString billStatus, ZString paymentStatus, ZDecimal paymentAmount)
			{
				var sendingObject = new PenaltyRefundRequestMessageSendingObject(entry, "11111", null);
				AssertEquals("613", sendingObject.TaxOffice);
				AssertEquals(billStatus, sendingObject.BillStatus);
				AssertEquals(paymentStatus, sendingObject.PaymentStatus);
				AssertEquals(paymentAmount, sendingObject.PaymentAmount);
			}
		}

		public void TestAmendmentSessionalDataProperties()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = KRJobMessageTypeList.Codes.Import;

			var instruction = declaration.CustomsEntryInstructions.AddNew();
			var amendmentSessionalData = instruction.AmendmentSessionalDataCollection.AddNew();
			amendmentSessionalData.CSI_LineNo = 3;
			amendmentSessionalData.CSI_DateOfIssue = new ZDateTime(2024, 2, 1);
			amendmentSessionalData.CSI_ItemNumber = 2;

			var entry_AttachedInstruction = declaration.CustomsEntryHeaders.AddNew();
			entry_AttachedInstruction.CH_CEI_Instruction = instruction.PK;
			var entry_NotAttachedInstruction = declaration.CustomsEntryHeaders.AddNew();
			AssertProperties(entry_AttachedInstruction, new ZDateTime(2024, 2, 1), "03");
			AssertProperties(entry_NotAttachedInstruction, ZDateTime.Empty, ZString.Empty);

			void AssertProperties(CusEntryHeader entry, ZDateTime submissionDate, ZString amendmentVersionNoCW1)
			{
				var sendingObject = new PenaltyRefundRequestMessageSendingObject(entry, ZString.Empty, null);
				sendingObject.Amendment5WNNumber = (ZShort)2;
				AssertEquals(submissionDate, sendingObject.SubmissionDate);
				AssertEquals(amendmentVersionNoCW1, sendingObject.AmendmentVersionNoCW1);
			}
		}
	}
}
