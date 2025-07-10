using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.Common.KR;
using Enterprise.Customs.KR.Messaging;
using NUnit.Framework;

namespace Enterprise.Customs.KR.Business.Testing
{
	[TestedType(typeof(AmendmentSessionalData))]
	sealed class AmendmentSessionalDataTest : CusSupportingInfoTest<AmendmentSessionalData>
	{
		[TestDate(2025, 01, 16)]
		public void TestFields()
		{
			SetUpSessionalData();
			entry.Charges.SetAmount(ChargeTypeList.Codes.LiquorTax, 2000m);
			entry.Charges.SetAmount(ChargeTypeList.Codes.AgricultureTax, 4000m);
			entry.Charges.SetAmount(ChargeTypeList.Codes.TransportationTax, 8000m);
			entry.Charges.SetAmount(ChargeTypeList.Codes.SpecialConsumptionTax, 16000m);
			entry.Charges.SetAmount(ChargeTypeList.Codes.EducationTax, 32000m);
			entry.Charges.SetAmount(ChargeTypeList.Codes.VAT, 64000m);
			var message5FE = entry.Messages.AddNew();
			message5FE.EM_MessageType = ElectronicDocumentTypeList.Codes._5FE;
			message5FE.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			message5FE.EM_MessageNum = "1";
			message5FE.EM_ApplicationReference = "2";
			message5FE.EM_SystemCreateUser = "ORG";
			Factory.Save();

			sessionalData.CSI_Code = DutyTaxCorrectionCodeList.Codes.A;
			sessionalData.CSI_LineNo = 2;
			sessionalData.CSI_Status = CustomsEntryStatusTypeList.Codes.ANT;
			sessionalData.CSI_DateOfIssue = new ZDateTime(2025, 01, 01);
			sessionalData.CSI_ItemNumber = 2;
			sessionalData.CSI_SubType = TaxPenaltyTypeCodeList.Codes._01;
			sessionalData.CSI_Procedure = TaxPenaltyTypeCodeList.Codes._02;

			AssertEquals("A", sessionalData.AmendmentType);
			AssertEquals(2, sessionalData.AmendmentCW1VersionNo);
			AssertEquals((ZShort)1, sessionalData.AmendmentCustomsVersionNo);
			AssertEquals("1", sessionalData.MessageNum5FE);
			AssertEquals("ANT", sessionalData.AmendmentEntryStatus);
			AssertEquals(new ZDateTime(2025, 01, 01), sessionalData.AcceptanceDate5FE);
			AssertEquals(2, sessionalData.VersionNumber5WN);
			AssertEquals(0m, sessionalData.DutyDifference);
			AssertEquals("01", sessionalData.DutyPenaltyCause);
			AssertEquals(126000m, sessionalData.TaxDifference);
			AssertEquals("02", sessionalData.TaxPenaltyCause);

			Assert(sessionalData.DutyPenaltyCauseInfo.ReadOnly);
			Assert(!sessionalData.TaxPenaltyCauseInfo.ReadOnly);

			entry.Charges.SetAmount(ChargeTypeList.Codes.Duty, 1000m);

			AssertEquals(1000m, sessionalData.DutyDifference);
			Assert(!sessionalData.DutyPenaltyCauseInfo.ReadOnly);
			Assert(!sessionalData.TaxPenaltyCauseInfo.ReadOnly);

			entry.CH_Status = CustomsMessageStatusTypeList.Codes.AmendmentSent;
			Assert(sessionalData.DutyPenaltyCauseInfo.ReadOnly);
			Assert(sessionalData.TaxPenaltyCauseInfo.ReadOnly);

			entry.CH_Status = CustomsMessageStatusTypeList.Codes.AmendmentAccepted;
			Assert(sessionalData.DutyPenaltyCauseInfo.ReadOnly);
			Assert(sessionalData.TaxPenaltyCauseInfo.ReadOnly);

			entry.CH_Status = CustomsMessageStatusTypeList.Codes.AmendmentRejected;
			Assert(!sessionalData.DutyPenaltyCauseInfo.ReadOnly);
			Assert(!sessionalData.TaxPenaltyCauseInfo.ReadOnly);

			var incomingMessage = entry.Messages.AddNew();
			incomingMessage.EM_MessageType = ElectronicDocumentTypeList.Codes._R99;
			incomingMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			incomingMessage.EM_MessageNum = "2";
			incomingMessage.EM_ApplicationReference = message5FE.EM_MessageNum;
			incomingMessage.EM_SystemCreateUser = "ORG";

			entry.CH_VersionID = 2;
			Factory.Save();

			Assert(sessionalData.DutyPenaltyCauseInfo.ReadOnly);
			Assert(sessionalData.TaxPenaltyCauseInfo.ReadOnly);

			message5FE = entry.Messages.AddNew();
			message5FE.EM_MessageType = ElectronicDocumentTypeList.Codes._5FE;
			message5FE.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			message5FE.EM_MessageNum = "3";
			message5FE.EM_ApplicationReference = "3";
			message5FE.EM_SystemCreateUser = "ORG";

			incomingMessage = entry.Messages.AddNew();
			incomingMessage.EM_MessageType = ElectronicDocumentTypeList.Codes._R20;
			incomingMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			incomingMessage.EM_MessageNum = "4";
			incomingMessage.EM_ApplicationReference = message5FE.EM_MessageNum;
			incomingMessage.EM_SystemCreateUser = "ORG";
			entry.CH_VersionID = 3;
			sessionalData.CSI_LineNo = 3;
			Factory.Save();

			Assert(!sessionalData.DutyPenaltyCauseInfo.ReadOnly);
			Assert(!sessionalData.TaxPenaltyCauseInfo.ReadOnly);

			entry.Statement929.B2_DueDate = new ZDateTime(2025, 02, 01);

			Assert(sessionalData.DutyPenaltyCauseInfo.ReadOnly);
			Assert(sessionalData.TaxPenaltyCauseInfo.ReadOnly);

			entry.Statement929.B2_DueDate = ZDateTime.Empty;

			Assert(sessionalData.DutyPenaltyCauseInfo.ReadOnly);
			Assert(sessionalData.TaxPenaltyCauseInfo.ReadOnly);
		}

		public void TestRefundDeclarationNumber()
		{
			SetUpSessionalData();
			AssertNull(sessionalData.RefundSessionalData);
			Assert(sessionalData.RefundDeclarationNumber.IsEmpty);

			var refundSessionalData = entry.EntryInstruction.RefundSessionalDataCollection.AddNew();
			refundSessionalData.CSI_ReferenceNumber2 = "030123456789012";
			refundSessionalData.CSI_ReferenceNumber = "6N00220000076M";
			refundSessionalData.CSI_CSI_SupportingInfo = sessionalData.PK;
			Factory.Save();

			var factory = new BusinessObjectFactory();
			var loadSessionalData = factory.Load<AmendmentSessionalData>(sessionalData.PK);
			AssertNotNull(loadSessionalData.RefundSessionalData);
			AssertEquals("6N00220000076M", loadSessionalData.RefundDeclarationNumber);
			AssertEquals("030123456789012", loadSessionalData.RefundSessionalData.CSI_ReferenceNumber2);
		}

		protected override BusinessObject GetNewBusinessObject() => CreateAmendmentSessionalData(Factory);

		protected override IEnumerable<AmendmentSessionalData> GetBizObjsForCorrectlyTypeDecideTest(BusinessObjectFactory factory)
		{
			yield return CreateAmendmentSessionalData(factory);
		}

		protected override BusinessObject GetBusinessObjectForFetchForLoad()
		{
			var businessObj = (AmendmentSessionalData)base.GetBusinessObjectForFetchForLoad();
			businessObj.CSI_Type = CusSupportingInfoTypeList.Codes._5FE;
			return businessObj;
		}
		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory) => CreateAmendmentSessionalData(factory);

		AmendmentSessionalData CreateAmendmentSessionalData(BusinessObjectFactory factory)
		{
			var declaration = factory.New<JobDeclaration>();
			declaration.JE_MessageType = KRJobMessageTypeList.Codes.Import;
			var instruction = declaration.CustomsEntryInstructions.AddNew();
			var amendmentSessionDataCollection = new AmendmentSessionalDataCollection(instruction);
			return amendmentSessionDataCollection.AddNew();
		}

		void SetUpSessionalData()
		{
			sessionalData = CreateAmendmentSessionalData(Factory);
			var declaration = sessionalData.Parent.JobDeclaration;
			entry = (CusEntryHeader)declaration.CustomsEntryHeaders.AddNew();
			entry.CH_VersionID = 1;
			entry.CH_CEI_Instruction = sessionalData.Parent.PK;
			entry.EntryNumber = "1234525000001M";
			var statement929 = Factory.New<CusStatementHeader>();
			statement929.B2_StatementType = StatementHeaderTypeList.Codes.CustomsDisbursementBill;
			statement929.B2_StatementNumber = "030123456789012";
			statement929.B2_ProcessDate = new ZDateTime(2025, 01, 01);
			statement929.B2_DueDate = new ZDateTime(2025, 01, 15);

			var statementLine = statement929.StatementLines.AddNew();
			statementLine.B3_EntryType = KRJobMessageTypeList.Codes.Import;
			statementLine.B3_EntryNum = entry.EntryNumber;
			Factory.Save();
		}

		CusEntryHeader entry;
		AmendmentSessionalData sessionalData;
	}
}
