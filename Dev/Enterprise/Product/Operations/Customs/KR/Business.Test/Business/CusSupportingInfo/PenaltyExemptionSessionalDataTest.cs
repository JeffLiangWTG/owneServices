using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.Common.KR;
using Enterprise.Customs.KR.Messaging;
using NUnit.Framework;

namespace Enterprise.Customs.KR.Business.Testing
{
	[TestedType(typeof(PenaltyExemptionSessionalData))]
	sealed class PenaltyExemptionSessionalDataTest : CusSupportingInfoTest<PenaltyExemptionSessionalData>
	{
		protected override BusinessObject GetNewBusinessObject() => CreateSessionalData(Factory);
		protected override IEnumerable<PenaltyExemptionSessionalData> GetBizObjsForCorrectlyTypeDecideTest(BusinessObjectFactory factory)
		{
			yield return CreateSessionalData(factory);
		}
		protected override BusinessObject GetBusinessObjectForFetchForLoad()
		{
			var businessObj = (PenaltyExemptionSessionalData)base.GetBusinessObjectForFetchForLoad();
			businessObj.CSI_Type = CusSupportingInfoTypeList.Codes._5UA;
			return businessObj;
		}

		public void TestPenaltyExemptionSessionalData()
		{
			var penaltyExemptionSessionalData = CreateSessionalData(Factory);
			penaltyExemptionSessionalData.CSI_Procedure = "N";
			penaltyExemptionSessionalData.CSI_Code = "Y";
			penaltyExemptionSessionalData.CSI_LineNo = 1;
			penaltyExemptionSessionalData.CSI_Value = 1000m;
			penaltyExemptionSessionalData.CSI_DateOfIssue = new ZDateTime(2025, 01, 01);
			penaltyExemptionSessionalData.CSI_Status  = CustomsEntryStatusTypeList.Codes.ANT;

			AssertEquals("N", penaltyExemptionSessionalData.ApplyDutyPenaltyReduction);
			AssertEquals("Y", penaltyExemptionSessionalData.PenaltyExemptionCode);
			AssertEquals(1, penaltyExemptionSessionalData.PenaltyExemptionRequestVersionNo);
			AssertEquals(1000m, penaltyExemptionSessionalData.PenaltyExemptionAmount);
			AssertEquals(new ZDateTime(2025, 01, 01), penaltyExemptionSessionalData.AcceptanceDate5UA);
			AssertEquals(CustomsEntryStatusTypeList.Codes.ANT, penaltyExemptionSessionalData.EntryStatus5UA);
		}

		[TestDate(2025, 01, 16)]
		public void TestReadOnly()
		{
			var penaltyExemptionSessionalData = CreateSessionalData(Factory);
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
			amendmentSessionalData.CSI_Code = DutyTaxCorrectionCodeList.Codes.A;
			amendmentSessionalData.CSI_LineNo = 2;
			amendmentSessionalData.CSI_Status = CustomsEntryStatusTypeList.Codes.ANT;
			amendmentSessionalData.CSI_DateOfIssue = new ZDateTime(2025, 01, 01);
			amendmentSessionalData.CSI_ItemNumber = 2;
			amendmentSessionalData.CSI_SubType = TaxPenaltyTypeCodeList.Codes._01;
			amendmentSessionalData.CSI_Procedure = TaxPenaltyTypeCodeList.Codes._02;

			Assert(!penaltyExemptionSessionalData.PenaltyExemptionCodeInfo.ReadOnly);
			Assert(penaltyExemptionSessionalData.ApplyDutyPenaltyReductionInfo.ReadOnly);

			entry.Charges.SetAmount(ChargeTypeList.Codes.Duty, 1000m);

			AssertEquals(1000m, amendmentSessionalData.DutyDifference);
			Assert(!penaltyExemptionSessionalData.PenaltyExemptionCodeInfo.ReadOnly);
			Assert(!penaltyExemptionSessionalData.ApplyDutyPenaltyReductionInfo.ReadOnly);

			entry.CH_Status = CustomsMessageStatusTypeList.Codes.AmendmentSent;
			Assert(penaltyExemptionSessionalData.PenaltyExemptionCodeInfo.ReadOnly);
			Assert(penaltyExemptionSessionalData.ApplyDutyPenaltyReductionInfo.ReadOnly);

			entry.CH_Status = CustomsMessageStatusTypeList.Codes.AmendmentAccepted;
			Assert(penaltyExemptionSessionalData.PenaltyExemptionCodeInfo.ReadOnly);
			Assert(penaltyExemptionSessionalData.ApplyDutyPenaltyReductionInfo.ReadOnly);

			entry.CH_Status = CustomsMessageStatusTypeList.Codes.AmendmentRejected;
			Assert(!penaltyExemptionSessionalData.PenaltyExemptionCodeInfo.ReadOnly);
			Assert(!penaltyExemptionSessionalData.ApplyDutyPenaltyReductionInfo.ReadOnly);

			var incomingMessage = entry.Messages.AddNew();
			incomingMessage.EM_MessageType = ElectronicDocumentTypeList.Codes._R99;
			incomingMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			incomingMessage.EM_MessageNum = "2";
			incomingMessage.EM_ApplicationReference = message5FE.EM_MessageNum;
			incomingMessage.EM_SystemCreateUser = "ORG";

			entry.CH_VersionID = 2;
			Factory.Save();

			Assert(penaltyExemptionSessionalData.PenaltyExemptionCodeInfo.ReadOnly);
			Assert(penaltyExemptionSessionalData.ApplyDutyPenaltyReductionInfo.ReadOnly);

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
			amendmentSessionalData.CSI_LineNo = 3;
			Factory.Save();

			Assert(!penaltyExemptionSessionalData.PenaltyExemptionCodeInfo.ReadOnly);
			Assert(!penaltyExemptionSessionalData.ApplyDutyPenaltyReductionInfo.ReadOnly);

			entry.Statement929.B2_DueDate = new ZDateTime(2025, 02, 01);

			Assert(penaltyExemptionSessionalData.PenaltyExemptionCodeInfo.ReadOnly);
			Assert(penaltyExemptionSessionalData.ApplyDutyPenaltyReductionInfo.ReadOnly);

			entry.Statement929.B2_DueDate = ZDateTime.Empty;

			Assert(penaltyExemptionSessionalData.PenaltyExemptionCodeInfo.ReadOnly);
			Assert(penaltyExemptionSessionalData.ApplyDutyPenaltyReductionInfo.ReadOnly);
		}

		void SetUpSessionalData()
		{
			var declaration = amendmentSessionalData.Parent.JobDeclaration;
			entry = (CusEntryHeader)declaration.CustomsEntryHeaders.AddNew();
			entry.CH_VersionID = 1;
			entry.CH_CEI_Instruction = amendmentSessionalData.Parent.PK;
			entry.EntryNumber = "1234525000001M";
			var statement929 = Factory.New<CusStatementHeader>();
			statement929.B2_StatementType = StatementHeaderTypeList.Codes.CustomsDisbursementBill;
			statement929.B2_ProcessDate = new ZDateTime(2025, 01, 01);
			statement929.B2_DueDate = new ZDateTime(2025, 01, 15);

			var statementLine = statement929.StatementLines.AddNew();
			statementLine.B3_EntryType = KRJobMessageTypeList.Codes.Import;
			statementLine.B3_EntryNum = entry.EntryNumber;
		}

		PenaltyExemptionSessionalData CreateSessionalData(BusinessObjectFactory factory)
		{
			var declaration = factory.New<JobDeclaration>();
			declaration.JE_MessageType = KRJobMessageTypeList.Codes.Import;
			var instruction = declaration.CustomsEntryInstructions.AddNew();
			amendmentSessionalData = new AmendmentSessionalDataCollection(instruction).AddNew();
			amendmentSessionalData.CSI_Code = "A";
			return amendmentSessionalData.PenaltyExemptionSessionalData;
		}

		CusEntryHeader entry;
		AmendmentSessionalData amendmentSessionalData;
	}
}
