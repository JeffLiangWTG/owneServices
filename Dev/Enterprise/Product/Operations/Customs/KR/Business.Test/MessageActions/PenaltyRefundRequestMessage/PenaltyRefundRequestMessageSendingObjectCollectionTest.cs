using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common.KR;
using Enterprise.Customs.Common.Shared;
using Enterprise.Customs.KR.Messaging;
using Enterprise.Messaging.Integration;
using NUnit.Framework;

namespace Enterprise.Customs.KR.Business.Testing
{
	[TestedType(typeof(PenaltyRefundRequestMessageSendingObjectCollection))]
	sealed class PenaltyRefundRequestMessageSendingObjectCollectionTest : NonPersistentBusinessObjectCollectionTestCase<PenaltyRefundRequestMessageSendingObjectCollection>
	{
		protected override PenaltyRefundRequestMessageSendingObjectCollection GetCollectionToTest()
		{
			return new PenaltyRefundRequestMessageSendingObjectCollection(Declaration);
		}

		JobDeclaration Declaration => declaration ?? (declaration = Factory.New<JobDeclaration>());
		JobDeclaration declaration;

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			var entry = Declaration.CustomsEntryHeaders.AddNew();
			var instruction = declaration.CustomsEntryInstructions.AddNew();
			entry.CH_CEI_Instruction = instruction.PK;
			var amendmentSessionalData = instruction.AmendmentSessionalDataCollection.AddNew();
			var refundSessionalData = amendmentSessionalData.RefundSessionalDataCollection.AddNew();

			var entryNumber = entry.EntryNumbers.AddNew();
			entryNumber.CE_EntryType = ElectronicDocumentTypeList.Codes._5UL;
			entryNumber.CE_EntryNum = "1234500001";

			var statement = Factory.New<CusStatementHeader>();
			statement.B2_StatementType = StatementHeaderTypeList.Codes.Invoice;
			statement.B2_PaymentStatus = StatementHeaderPaymentStatusList.Codes.PYC;
			var statementLine = statement.StatementLines.AddNew();
			statementLine.B3_EntryNum = $"1234500001";
			statementLine.B3_EntryType = SharedJobMessageTypeList.Codes.Import;
			Factory.Save();

			return new PenaltyRefundRequestMessageSendingObject(entry, "11111", entryNumber);
		}

		public void TestSendingObjectPopulate()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = KRJobMessageTypeList.Codes.Import;
			declaration.JE_TaxOffice = "613";

			var instruction = declaration.CustomsEntryInstructions.AddNew();
			var entry = declaration.CustomsEntryHeaders.AddNew();
			entry.CH_CEI_Instruction = instruction.PK;
			entry.EntryNumber = "1";
			var entryLine = entry.MergedLines.AddNew();

			var refundEntryNumber = entry.EntryNumbers.AddNew();
			refundEntryNumber.CE_EntryType = ElectronicDocumentTypeList.Codes._5UL;

			var statement = Factory.New<CusStatementHeader>();
			statement.B2_StatementType = StatementHeaderTypeList.Codes.CustomsDisbursementBill;
			statement.B2_StatementNumber = "11111";
			statement.B2_Status = StatementHeaderStatusList.Codes.A;

			var statementLine = statement.StatementLines.AddNew();
			statementLine.B3_EntryType = SharedJobMessageTypeList.Codes.Import;
			statementLine.B3_EntryNum = entry.EntryNumber;

			Factory.Save();

			var collection = new PenaltyRefundRequestMessageSendingObjectCollection(declaration);
			AssertEquals(0, collection.Count);

			refundEntryNumber.CE_EntryNum = "AAA1111";
			statement.B2_PaymentStatus = StatementHeaderPaymentStatusList.Codes.PYC;

			var amendmentSessionalData = instruction.AmendmentSessionalDataCollection.AddNew();
			amendmentSessionalData.CSI_ItemNumber = 111111;
			var refundSessionalData = amendmentSessionalData.RefundSessionalDataCollection.AddNew();
			refundSessionalData.CSI_ReferenceNumber = "AAA1111";
			refundSessionalData.CSI_ReferenceNumber2 = "11111";

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

			statement.B2_IncomingMessageNo = message5FK.EM_MessageNum;
			Factory.Save();

			collection = new PenaltyRefundRequestMessageSendingObjectCollection(declaration);
			AssertEquals(1, collection.Count);

			var declaration2 = Factory.New<JobDeclaration>();
			declaration2.JE_MessageType = KRJobMessageTypeList.Codes.Import;
			declaration2.JE_TaxOffice = "613";

			var instruction2 = declaration2.CustomsEntryInstructions.AddNew();
			var entry2 = declaration2.CustomsEntryHeaders.AddNew();
			entry2.CH_CEI_Instruction = instruction2.PK;
			entry2.EntryNumber = "2";

			var refundEntryNumber2 = entry2.EntryNumbers.AddNew();
			refundEntryNumber2.CE_EntryType = ElectronicDocumentTypeList.Codes._5UL;

			var statement2 = Factory.New<CusStatementHeader>();
			statement2.B2_StatementType = StatementHeaderTypeList.Codes.CustomsDisbursementBill;
			statement2.B2_StatementNumber = "22222";
			statement2.B2_Status = StatementHeaderStatusList.Codes.A;
			statement2.B2_PaymentStatus = StatementHeaderPaymentStatusList.Codes.PYC;
			statement2.B2_GC = declaration2.JE_GC;
			statement2.B2_ProcessDate = new ZDateTime(2025, 03, 01);

			var statementLine2 = statement2.StatementLines.AddNew();
			statementLine2.B3_EntryNum = entry2.EntryNumber;
			statementLine2.B3_EntryType = SharedJobMessageTypeList.Codes.Import;

			var statement3 = Factory.New<CusStatementHeader>();
			statement3.B2_StatementType = StatementHeaderTypeList.Codes.CustomsDisbursementBill;
			statement3.B2_StatementNumber = "33333";
			statement3.B2_GC = declaration2.JE_GC;
			statement3.B2_ProcessDate = new ZDateTime(2025, 02, 01); 

			var statementLine3 = statement3.StatementLines.AddNew();
			statementLine3.B3_EntryNum = entry2.EntryNumber;
			statementLine3.B3_EntryType = SharedJobMessageTypeList.Codes.Import;

			Factory.Save();

			var collection2 = new PenaltyRefundRequestMessageSendingObjectCollection(declaration2);
			AssertEquals(1, collection2.Count); 
		}
	}
}
