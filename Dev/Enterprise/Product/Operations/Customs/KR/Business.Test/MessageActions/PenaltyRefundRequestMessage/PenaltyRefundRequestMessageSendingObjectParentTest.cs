using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common.Shared;
using Enterprise.Customs.KR.Messaging;
using Enterprise.Messaging.Integration;
using NUnit.Framework;

namespace Enterprise.Customs.KR.Business.Testing
{
	[TestedType(typeof(PenaltyRefundRequestMessageSendingObjectParent))]
	sealed class PenaltyRefundRequestMessageSendingObjectParentTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			var declaration = Factory.New<JobDeclaration>();
			return new PenaltyRefundRequestMessageSendingObjectParent(declaration);
		}

		public void TestSendingObjectsCollection_5ULof5FE()
		{
			var declaration1 = Factory.New<JobDeclaration>();
			var objectParent1 = new PenaltyRefundRequestMessageSendingObjectParent(declaration1);
			AssertEquals(0, objectParent1.SendingObjectsCollection.Count);
			AssertEquals(0, objectParent1.ObjectsToSend.Count());

			declaration1.CustomsEntryHeaders.AddNew();
			declaration1.CustomsEntryHeaders.AddNew();
			AssertEquals(0, objectParent1.SendingObjectsCollection.Count);
			AssertEquals(0, objectParent1.ObjectsToSend.Count());

			var declaration2 = Factory.New<JobDeclaration>();

			var entry = declaration2.ActiveEntryHeaders.AddNew();
			entry.EntryNumber = "1234520000045M";

			var instruction = declaration2.CustomsEntryInstructions.AddNew();
			entry.CH_CEI_Instruction = instruction.PK;
			var entryLine = entry.MergedLines.AddNew();

			var amendmentSessionalData = instruction.AmendmentSessionalDataCollection.AddNew();
			amendmentSessionalData.CSI_ItemNumber = 111111;
			var refundSessionalData = amendmentSessionalData.RefundSessionalDataCollection.AddNew();
			refundSessionalData.CSI_Status = CustomsEntryStatusTypeList.Codes.ANT;

			var message5FE = entry.Messages.AddNew();
			message5FE.EM_MessageNum = "10";
			message5FE.EM_MessageType = ElectronicDocumentTypeList.Codes._5FE;
			message5FE.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			message5FE.EM_ApplicationReference = "3";

			var message5FK = entry.Messages.AddNew();
			message5FK.EM_MessageNum = "11";
			message5FK.EM_ReceiveTransmit = ReceiveTransmitList.Codes.Receive;
			message5FK.EM_MessageType = ElectronicDocumentTypeList.Codes._5FK;
			message5FK.EM_MessageOwner = CustomsEntryStatusTypeList.Codes.ANT;
			message5FK.EM_ApplicationReference = message5FE.EM_MessageNum;

			var statement = Factory.New<CusStatementHeader>();
			statement.B2_StatementType = StatementHeaderTypeList.Codes.Invoice;
			statement.B2_PaymentStatus = StatementHeaderPaymentStatusList.Codes.PYC;
			statement.B2_IncomingMessageNo = message5FK.EM_MessageNum;
			var statementLine = statement.StatementLines.AddNew();
			statementLine.B3_EntryNum = "1234520000045M";
			statementLine.B3_EntryType = SharedJobMessageTypeList.Codes.Import;
			Factory.Save();

			var objectParent2 = new PenaltyRefundRequestMessageSendingObjectParent(declaration2);
			AssertEquals(1, objectParent2.SendingObjectsCollection.Count);
			AssertEquals(0, objectParent2.ObjectsToSend.Count());
			
			objectParent2.SendingObjectsCollection[0].ShouldSend = true;
			AssertEquals(1, objectParent2.ObjectsToSend.Count());

			var entry2 = declaration2.ActiveEntryHeaders.AddNew();
			entry2.EntryNumber = "1234520000046M";

			var instruction2 = declaration2.CustomsEntryInstructions.AddNew();
			entry2.CH_CEI_Instruction = instruction.PK;
			var entryLine2 = entry2.MergedLines.AddNew();

			var amendmentSessionalData2 = instruction2.AmendmentSessionalDataCollection.AddNew();
			amendmentSessionalData2.CSI_ItemNumber = 111111;
			var refundSessionalData2 = amendmentSessionalData2.RefundSessionalDataCollection.AddNew();
			refundSessionalData2.CSI_Status = CustomsEntryStatusTypeList.Codes.DMS;

			statement = Factory.New<CusStatementHeader>();
			statement.B2_StatementType = StatementHeaderTypeList.Codes.CustomsDisbursementBill;
			statement.B2_PaymentStatus = StatementHeaderPaymentStatusList.Codes.PYC;
			statementLine = statement.StatementLines.AddNew();
			statementLine.B3_EntryNum = "1234520000046M";
			statementLine.B3_EntryType = SharedJobMessageTypeList.Codes.Import;

			var entry3 = declaration2.ActiveEntryHeaders.AddNew();
			entry3.EntryNumber = "1234520000047M";

			var entryNum = entry3.EntryNumbers.AddNew();
			entryNum.CE_EntryType = ElectronicDocumentTypeList.Codes._5UL;
			entryNum.CE_EntryLineReference = "22222";

			var instruction3 = declaration2.CustomsEntryInstructions.AddNew();
			entry3.CH_CEI_Instruction = instruction3.PK;
			var entryLine3 = entry3.MergedLines.AddNew();

			var amendmentSessionalData3 = instruction3.AmendmentSessionalDataCollection.AddNew();
			amendmentSessionalData3.CSI_ItemNumber = 111111;
			var refundSessionalData3 = amendmentSessionalData3.RefundSessionalDataCollection.AddNew();
			refundSessionalData3.CSI_Status = CustomsEntryStatusTypeList.Codes.PNR;
			refundSessionalData3.CSI_DateOfIssue = ZDateTime.Today;

			var statement2 = Factory.New<CusStatementHeader>();
			statement2.B2_StatementType = StatementHeaderTypeList.Codes.CustomsDisbursementBill;
			statement2.B2_PaymentStatus = StatementHeaderPaymentStatusList.Codes.PYC;
			statement2.B2_IncomingMessageNo = message5FK.EM_MessageNum;
			statement2.B2_StatementNumber = "22222";
			var statementLine2 = statement2.StatementLines.AddNew();
			statementLine2.B3_EntryNum = "1234520000047M";
			statementLine2.B3_EntryType = SharedJobMessageTypeList.Codes.Import;
			Factory.Save();

			objectParent2 = new PenaltyRefundRequestMessageSendingObjectParent(declaration2);
			AssertEquals(3, objectParent2.SendingObjectsCollection.Count);
			AssertEquals(0, objectParent2.ObjectsToSend.Count());
			
			objectParent2.SendingObjectsCollection[0].ShouldSend = true;
			AssertEquals(1, objectParent2.ObjectsToSend.Count());

			objectParent2.SendingObjectsCollection[1].ShouldSend = true;
			AssertEquals(2, objectParent2.ObjectsToSend.Count());

			objectParent2.SendingObjectsCollection[2].ShouldSend = true;
			AssertEquals(3, objectParent2.ObjectsToSend.Count());
		}

		public void TestSendingObjectsCollection_5UL()
		{
			var declaration1 = Factory.New<JobDeclaration>();
			var objectParent1 = new PenaltyRefundRequestMessageSendingObjectParent(declaration1);
			AssertEquals(0, objectParent1.SendingObjectsCollection.Count);
			AssertEquals(0, objectParent1.ObjectsToSend.Count());

			declaration1.CustomsEntryHeaders.AddNew();
			declaration1.CustomsEntryHeaders.AddNew();
			AssertEquals(0, objectParent1.SendingObjectsCollection.Count);
			AssertEquals(0, objectParent1.ObjectsToSend.Count());

			var declaration2 = Factory.New<JobDeclaration>();

			var entry = declaration2.ActiveEntryHeaders.AddNew();
			entry.AllEntryLines.AddNew();
			entry.EntryNumber = "1234520000045M";

			var instruction = declaration2.CustomsEntryInstructions.AddNew();
			entry.CH_CEI_Instruction = instruction.PK;
			var entryLine = entry.MergedLines.AddNew();

			var refundSessionalData = instruction.RefundSessionalDataCollection.AddNew();
			refundSessionalData.CSI_ReferenceNumber2 = "030112000771025";
			var refundSessionalData2 = instruction.RefundSessionalDataCollection.AddNew();
			refundSessionalData2.CSI_ReferenceNumber2 = "030112000771025";

			var statement = Factory.New<CusStatementHeader>();
			statement.B2_StatementType = StatementHeaderTypeList.Codes.CustomsDisbursementBill;
			statement.B2_PaymentStatus = StatementHeaderPaymentStatusList.Codes.PYC;
			statement.B2_StatementNumber = "030112000771025";
			statement.B2_ProcessDate = new ZDateTime(2025, 03, 01);
			var statementLine = statement.StatementLines.AddNew();
			statementLine.B3_EntryNum = entry.EntryNumber;
			statementLine.B3_EntryType = SharedJobMessageTypeList.Codes.Import;
			Factory.Save();

			var objectParent2 = new PenaltyRefundRequestMessageSendingObjectParent(declaration2);
			AssertEquals(1, objectParent2.SendingObjectsCollection.Count);
			AssertEquals(0, objectParent2.ObjectsToSend.Count());

			objectParent2.SendingObjectsCollection[0].ShouldSend = true;
			AssertEquals(1, objectParent2.ObjectsToSend.Count());

			var entry2 = declaration2.ActiveEntryHeaders.AddNew();
			entry2.AllEntryLines.AddNew();
			entry2.EntryNumber = "1234520000046M";

			var instruction2 = declaration2.CustomsEntryInstructions.AddNew();
			entry2.CH_CEI_Instruction = instruction2.PK;
			var entryLine2 = entry2.MergedLines.AddNew();

			var statement2 = Factory.New<CusStatementHeader>();
			statement2.B2_StatementType = StatementHeaderTypeList.Codes.CustomsDisbursementBill;
			statement2.B2_PaymentStatus = StatementHeaderPaymentStatusList.Codes.PYC;
			statement2.B2_ProcessDate = new ZDateTime(2025, 03, 01);
			var statementLine2 = statement2.StatementLines.AddNew();
			statementLine2.B3_EntryNum = entry2.EntryNumber;
			statementLine2.B3_EntryType = SharedJobMessageTypeList.Codes.Import;

			var entry3_notIncluded = declaration2.ActiveEntryHeaders.AddNew();
			entry3_notIncluded.AllEntryLines.AddNew();
			entry3_notIncluded.EntryNumber = "1234520000047M";
			Factory.Save();

			objectParent2 = new PenaltyRefundRequestMessageSendingObjectParent(declaration2);
			AssertEquals(2, objectParent2.SendingObjectsCollection.Count);
			AssertEquals(0, objectParent2.ObjectsToSend.Count());

			objectParent2.SendingObjectsCollection[0].ShouldSend = true;
			AssertEquals(1, objectParent2.ObjectsToSend.Count());
		}

		public void TestGetNewParent()
		{
			var declaration = Factory.New<JobDeclaration>();

			var result = JobDeclarationMessageSendingObjectParent.GetJobDeclarationMessageSendingObjectParent(declaration, ElectronicDocumentTypeList.Codes._5UL, MessageFunctions.MessageFunctionCode.Original);
			AssertEquals(typeof(PenaltyRefundRequestMessageSendingObjectParent), result.GetType());
		}
	}
}
