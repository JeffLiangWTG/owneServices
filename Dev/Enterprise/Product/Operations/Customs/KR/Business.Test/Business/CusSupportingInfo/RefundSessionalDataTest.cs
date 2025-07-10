using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.Common.KR;
using Enterprise.Customs.KR.Messaging;
using NUnit.Framework;

namespace Enterprise.Customs.KR.Business.Testing
{
	[TestedType(typeof(RefundSessionalData))]
	sealed class RefundSessionalDataTest : CusSupportingInfoTest<RefundSessionalData>
	{
		[TestDate(2025, 01, 01)]
		public void Test5ULFields()
		{
			SetUpSessionalData();

			refundSessionalData.CSI_CSI_SupportingInfo = ZGuid.Empty;
			AssertEquals(null, refundSessionalData.AmendmentSessionalData);
			Assert(refundSessionalData.RefundRequestYNInfo.ReadOnly);

			Assert(!refundSessionalData.RefundCauseCodeInfo.ReadOnly);
			Assert(!refundSessionalData.RefundReasonCodeInfo.ReadOnly);
			Assert(!refundSessionalData.RefundTypeInfo.ReadOnly);
			Assert(!refundSessionalData.CustomsDisbursementBillInfo.ReadOnly);

			refundSessionalData.CSI_CSI_SupportingInfo = amendmentSessionalData.PK;
			AssertNotNull(refundSessionalData.AmendmentSessionalData);
			Assert(!refundSessionalData.RefundRequestYNInfo.ReadOnly);

			var message5FE = entry.Messages.AddNew();
			message5FE.EM_MessageType = ElectronicDocumentTypeList.Codes._5FE;
			message5FE.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			message5FE.EM_MessageNum = "1";
			message5FE.EM_ApplicationReference = "2";
			message5FE.EM_SystemCreateUser = "ORG";
			message5FE.EM_SystemCreateTimeUtc = new ZDateTime(2024, 12, 31);

			var message5UL = entry.Messages.AddNew();
			message5UL.EM_MessageType = ElectronicDocumentTypeList.Codes._5UL;
			message5UL.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			message5UL.EM_MessageNum = "1";
			message5UL.EM_ApplicationReference = "2";
			message5UL.EM_MessageOwner = "82455884";
			message5UL.EM_SystemCreateUser = "ORG";
			message5UL.EM_SystemCreateTimeUtc = new ZDateTime(2024, 12, 31);

			var entryNum5UL = entry.EntryNumbers.GetOrCreateCusEntryNumWithMatchingVersionNumber(ElectronicDocumentTypeList.Codes._5UL, "1234567890123456789", null);
			entryNum5UL.CE_EntryNum = "82455884";
			entryNum5UL.CE_EntryStatus = CustomsMessageStatusTypeList.Codes.OriginalAccepted;
			Factory.Save();

			amendmentSessionalData.CSI_LineNo = 2;
			refundSessionalData.CSI_Code = YesNoList.Codes.No;
			refundSessionalData.CSI_ReferenceNumber = "82455884";
			refundSessionalData.CSI_Procedure = RefundCauseCodeList.Codes._01;
			refundSessionalData.CSI_IssuerType = RefundReasonCodeList.Codes._02;
			refundSessionalData.CSI_SubType = RefundTypeList.Codes.A;
			refundSessionalData.CSI_DateOfIssue = new ZDateTime(2025, 01, 01);
			refundSessionalData.CSI_Status = CustomsEntryStatusTypeList.Codes.ANT;
			refundSessionalData.CSI_ReferenceNumber2 = "1234567890123456789";
			refundSessionalData.CSI_DateOfExpiry = new ZDateTime(2025, 01, 10);
			refundSessionalData.CSI_Tariff = "123456";
			refundSessionalData.CSI_Value = 1000m;

			AssertEquals("N", refundSessionalData.RefundRequestYN);
			AssertEquals("82455884", refundSessionalData.RefundRequestNumber);
			AssertEquals("01", refundSessionalData.RefundCauseCode);
			AssertEquals("02", refundSessionalData.RefundReasonCode);
			AssertEquals("A", refundSessionalData.RefundType);
			AssertEquals(new ZDateTime(2025, 01, 01), refundSessionalData.AcceptanceDate5UL);
			AssertEquals("ANT", refundSessionalData.EntryStatus5UL);
			AssertEquals("승인통보", refundSessionalData.EntryStatus5ULDescription);
			AssertEquals("1234567890123456789", refundSessionalData.CustomsDisbursementBill);
			AssertEquals("1234-567-89-01-2-345678-9", refundSessionalData.FormattedCustomsDisbursementBill);
			AssertEquals(new ZDateTime(2025, 01, 10), refundSessionalData.RefundApprovalDate);
			AssertEquals("123456", refundSessionalData.RefundApprovalNumber);
			AssertEquals(1000m, refundSessionalData.RefundAmount);

			var messageRCA = entry.Messages.AddNew();
			messageRCA.EM_ApplicationCode = EDIMessage.ApplicationCodes.KRCustoms;
			messageRCA.EM_MessageType = ElectronicDocumentTypeList.Codes._RCA;
			messageRCA.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			messageRCA.EM_MessageNum = "3";
			messageRCA.EM_ApplicationReference = "1";
			messageRCA.EM_MessageOwner = CustomsEntryStatusTypeList.Codes.ANT;
			messageRCA.EM_SystemCreateTimeUtc = new ZDateTime(2024, 12, 31);

			var incomingMessage = entry.Messages.AddNew();
			incomingMessage.EM_MessageType = ElectronicDocumentTypeList.Codes._R99;
			incomingMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			incomingMessage.EM_MessageNum = "3";
			incomingMessage.EM_ApplicationReference = message5FE.EM_MessageNum;
			incomingMessage.EM_SystemCreateUser = "ORG";

			Assert(refundSessionalData.RefundCauseCodeInfo.ReadOnly);
			Assert(refundSessionalData.RefundReasonCodeInfo.ReadOnly);
			Assert(refundSessionalData.RefundTypeInfo.ReadOnly);
			Assert(refundSessionalData.CustomsDisbursementBillInfo.ReadOnly);
			entryNum5UL.CE_EntryStatus = CustomsMessageStatusTypeList.Codes.OriginalRejected;
			Assert(!refundSessionalData.RefundCauseCodeInfo.ReadOnly);
			Assert(!refundSessionalData.RefundReasonCodeInfo.ReadOnly);
			Assert(!refundSessionalData.RefundTypeInfo.ReadOnly);
			Assert(!refundSessionalData.CustomsDisbursementBillInfo.ReadOnly);

			entry.CH_VersionID = 1;
			entry.CH_Status = CustomsMessageStatusTypeList.Codes.AmendmentAccepted;
			Assert(refundSessionalData.RefundRequestYNInfo.ReadOnly);
			entry.CH_Status = CustomsMessageStatusTypeList.Codes.AmendmentRejected;
			Assert(!refundSessionalData.RefundRequestYNInfo.ReadOnly);
			entry.CH_VersionID = 2;
			Assert(refundSessionalData.RefundRequestYNInfo.ReadOnly);

			message5FE = entry.Messages.AddNew();
			message5FE.EM_MessageType = ElectronicDocumentTypeList.Codes._5FE;
			message5FE.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			message5FE.EM_MessageNum = "1";
			message5FE.EM_ApplicationReference = "2";
			message5FE.EM_SystemCreateUser = "ORG";
			message5FE.EM_SystemCreateTimeUtc = new ZDateTime(2025, 01, 01);

			incomingMessage = entry.Messages.AddNew();
			incomingMessage.EM_MessageType = ElectronicDocumentTypeList.Codes._R20;
			incomingMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			incomingMessage.EM_MessageNum = "4";
			incomingMessage.EM_ApplicationReference = message5FE.EM_MessageNum;
			incomingMessage.EM_SystemCreateUser = "ORG";

			Assert(!refundSessionalData.RefundRequestYNInfo.ReadOnly);

			message5UL = entry.Messages.AddNew();
			message5UL.EM_MessageType = ElectronicDocumentTypeList.Codes._5UL;
			message5UL.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			message5UL.EM_MessageNum = "2";
			message5UL.EM_ApplicationReference = "3";
			message5UL.EM_MessageOwner = "82455884";
			message5UL.EM_SystemCreateUser = "ORG";
			message5UL.EM_SystemCreateTimeUtc = new ZDateTime(2025, 01, 01);

			messageRCA = entry.Messages.AddNew();
			messageRCA.EM_ApplicationCode = EDIMessage.ApplicationCodes.KRCustoms;
			messageRCA.EM_MessageType = ElectronicDocumentTypeList.Codes._RCA;
			messageRCA.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			messageRCA.EM_MessageNum = "4";
			messageRCA.EM_ApplicationReference = "1";
			messageRCA.EM_MessageOwner = CustomsEntryStatusTypeList.Codes.DMS;
			messageRCA.EM_SystemCreateTimeUtc = new ZDateTime(2025, 01, 01);

			amendmentSessionalData.CSI_Code = DutyTaxCorrectionCodeList.Codes.B;
			refundSessionalData.CSI_CSI_SupportingInfo = amendmentSessionalData.PK;
			Assert(refundSessionalData.RefundCauseCodeInfo.ReadOnly);
			Assert(refundSessionalData.RefundReasonCodeInfo.ReadOnly);
			Assert(refundSessionalData.RefundTypeInfo.ReadOnly);
			Assert(refundSessionalData.CustomsDisbursementBillInfo.ReadOnly);
			Assert(refundSessionalData.CustomsDisbursementBillInfo.ReadOnly);
		}
		protected override BusinessObject GetNewBusinessObject() => CreateRefundSessionalData(Factory);
		protected override IEnumerable<RefundSessionalData> GetBizObjsForCorrectlyTypeDecideTest(BusinessObjectFactory factory)
		{
			yield return CreateRefundSessionalData(factory);
		}
		protected override BusinessObject GetBusinessObjectForFetchForLoad()
		{
			var businessObj = (RefundSessionalData)base.GetBusinessObjectForFetchForLoad();
			businessObj.CSI_Type = CusSupportingInfoTypeList.Codes._5UL;
			return businessObj;
		}

		RefundSessionalData CreateRefundSessionalData(BusinessObjectFactory factory)
		{
			var declaration = factory.New<JobDeclaration>();
			declaration.JE_MessageType = KRJobMessageTypeList.Codes.Import;
			var instruction = declaration.CustomsEntryInstructions.AddNew();
			entry = declaration.ActiveEntryHeaders.AddNew();
			entry.CH_CEI_Instruction = instruction.PK;
			entry.EntryNumber = "1234525000001M";
			amendmentSessionalData = new AmendmentSessionalDataCollection(instruction).AddNew();
			amendmentSessionalData.CSI_Code = DutyTaxCorrectionCodeList.Codes.C;
			return new RefundSessionalDataCollection(instruction, amendmentSessionalData.PK).AddNew();
		}

		void SetUpSessionalData()
		{
			refundSessionalData = CreateRefundSessionalData(Factory);
			var statement929 = Factory.New<CusStatementHeader>();
			statement929.B2_StatementType = StatementHeaderTypeList.Codes.CustomsDisbursementBill;
			statement929.B2_ProcessDate = new ZDateTime(2025, 01, 01);
			statement929.B2_DueDate = new ZDateTime(2025, 01, 15);

			var statementLine = statement929.StatementLines.AddNew();
			statementLine.B3_EntryType = KRJobMessageTypeList.Codes.Import;
			statementLine.B3_EntryNum = entry.EntryNumber;
		}

		CusEntryHeader entry;
		AmendmentSessionalData amendmentSessionalData;
		RefundSessionalData refundSessionalData;
	}
}
