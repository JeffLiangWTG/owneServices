using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.Common.KR;
using Enterprise.Customs.Common.Shared;
using Enterprise.Customs.KR.Messaging;

namespace Enterprise.Customs.KR.Business.Testing
{
	sealed class GOVCBR5ULMessageSenderTest : TestCaseWithFactory
	{
		public void TestSendingObjects()
		{
			AssertEquals(2, MessageSendingObjects.Count());
			MessageSendingObjects.First().ShouldSend = true;
			MessageSendingObjects.Last().ShouldSend = true;

			var sender = GetMessageSender();
			sender.Send();
			var sendingObjects = MessageSendingObjects;
			var entry = MessageSendingObjects.First().Header;
			var entryNumbers = entry.EntryNumbers.Cast<CusEntryNumber>().Where(x => x.CE_EntryType == ElectronicDocumentTypeList.Codes._5UL).OrderBy(x => x.CE_EntryLineReference).ToList();
			var messages = entry.Messages.Cast<EDIMessage>().Where(x => x.EM_MessageType == ElectronicDocumentTypeList.Codes._5UL).OrderBy(x => x.EM_ApplicationReference).ToList();

			AssertEquals("Four entry num rows are created", 4, entryNumbers.Count);
			AssertEquals("Two 5UL message rows are created", 2, messages.Count);
		}

		public void TestSend5ULFor5FE()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = KRJobMessageTypeList.Codes.Import;
			CreateCusEntryNumAndCustomsDisbursementBills("1111", "I", "0127030112200237051");
			CreateCusEntryNumAndCustomsDisbursementBills("2222", "D", "0127030112200237052");
			CreateCusEntryNumAndCustomsDisbursementBills("3333", "D", "0127030112200237053");
			Factory.Save();

			var sendingObjectParent = new PenaltyRefundRequestMessageSendingObjectParent(declaration);
			AssertEquals(3, sendingObjectParent.SendingObjectsCollection.Count);

			var sendingObject1 = sendingObjectParent.SendingObjectsCollection[0];
			sendingObject1.Amendment5WNNumber = ZShort.Zero;
			sendingObject1.ShouldSend = true;

			var sendingObject2 = sendingObjectParent.SendingObjectsCollection[1];
			sendingObject2.Amendment5WNNumber = 1;
			sendingObject2.ShouldSend = true;

			var sendingObject3 = sendingObjectParent.SendingObjectsCollection[2];
			sendingObject3.Amendment5WNNumber = 2;
			sendingObject3.ShouldSend = true;
			new GOVCBR5ULSender(sendingObjectParent.SendingObjectsCollection.Cast<PenaltyRefundRequestMessageSendingObject>(), Factory).Send();
			Factory.Save();

			AssertRefundSessionalData(sendingObject1, false);
			AssertRefundSessionalData(sendingObject2, true);
			AssertRefundSessionalData(sendingObject3, false);

			void AssertRefundSessionalData(PenaltyRefundRequestMessageSendingObject sendingObject, bool isFor5FE)
			{
				var instruction = sendingObject.Header.EntryInstruction;
				AssertEquals(1, instruction.RefundSessionalDataCollection.Count);
				AssertEquals(1, instruction.AmendmentSessionalDataCollection.Count);
				AssertEquals(isFor5FE, instruction.AmendmentSessionalDataCollection[0].VersionNumber5WN == sendingObject.Amendment5WNNumber);
				AssertEquals(isFor5FE, instruction.AmendmentSessionalDataCollection[0].PK == instruction.RefundSessionalDataCollection[0].CSI_CSI_SupportingInfo);
			}

			void CreateCusEntryNumAndCustomsDisbursementBills(string entryNum, string statementType, string statementNumber)
			{
				var entry = declaration.CustomsEntryHeaders.AddNew();
				entry.EntryNumber = entryNum;

				var instruction = declaration.CustomsEntryInstructions.AddNew();
				var amendmentSessionalData = instruction.AmendmentSessionalDataCollection.AddNew();
				amendmentSessionalData.CSI_ItemNumber = 1;
				entry.CH_CEI_Instruction = instruction.PK;

				var statementHeader = Factory.New<CusStatementHeader>();
				statementHeader.B2_StatementType = statementType;
				statementHeader.B2_StatementNumber = statementNumber;
				statementHeader.B2_PaymentStatus = StatementHeaderPaymentStatusList.Codes.PYC;
				var statementLine = statementHeader.StatementLines.AddNew();
				statementLine.B3_EntryType = KRJobMessageTypeList.Codes.Import;
				statementLine.B3_AssociatedEntry = statementNumber;
				statementLine.B3_EntryNum = entryNum;
			}
		}

		GOVCBR5ULSender GetMessageSender() => new GOVCBR5ULSender(MessageSendingObjects, Factory);
		IEnumerable<PenaltyRefundRequestMessageSendingObject> MessageSendingObjects => messageSendingObjects ??= new PenaltyRefundRequestMessageSendingObjectParent(declaration).SendingObjectsCollection.Cast<PenaltyRefundRequestMessageSendingObject>();
		IEnumerable<PenaltyRefundRequestMessageSendingObject> messageSendingObjects;

		JobDeclaration declaration;

		protected override void SetUp()
		{
			base.SetUp();
			declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = KRJobMessageTypeList.Codes.Import;

			var instruction = declaration.CustomsEntryInstructions.AddNew();
			var amendmentSessionalData = instruction.AmendmentSessionalDataCollection.AddNew();
			amendmentSessionalData.CSI_ItemNumber = 111111;
			var entry = declaration.ActiveEntryHeaders.AddNew();
			entry.CH_CEI_Instruction = instruction.PK;
			entry.EntryNumber = "1234520000045M";

			var entryNumber5UL1 = entry.EntryNumbers.AddNew();
			entryNumber5UL1.CE_EntryType = ElectronicDocumentTypeList.Codes._5UL;
			entryNumber5UL1.CE_EntryLineReference = "030112000771025";

			var entryNumber5UL2 = entry.EntryNumbers.AddNew();
			entryNumber5UL2.CE_EntryType = ElectronicDocumentTypeList.Codes._5UL;
			entryNumber5UL2.CE_EntryLineReference = "030112000771026";

			var entryNumber5UL3 = entry.EntryNumbers.AddNew();
			entryNumber5UL3.CE_EntryType = ElectronicDocumentTypeList.Codes._5UL;
			entryNumber5UL3.CE_EntryLineReference = "030112000771027";

			var entryNumber5UL4 = entry.EntryNumbers.AddNew();
			entryNumber5UL4.CE_EntryType = ElectronicDocumentTypeList.Codes._5UL;
			entryNumber5UL4.CE_EntryLineReference = "030112000771028";

			var statement = Factory.New<CusStatementHeader>();
			statement.B2_StatementType = StatementHeaderTypeList.Codes.Invoice;
			statement.B2_PaymentAuthorizationDate = new ZDateTime(2024, 02, 20);
			statement.B2_PaymentStatus = StatementHeaderPaymentStatusList.Codes.PYC;
			var statementLine = statement.StatementLines.AddNew();
			statementLine.B3_EntryNum = "1234520000045M";
			statementLine.B3_EntryType = SharedJobMessageTypeList.Codes.Import;
			statementLine.B3_AssociatedEntry = "030112000771025";

			var statement2 = Factory.New<CusStatementHeader>();
			statement2.B2_StatementType = StatementHeaderTypeList.Codes.Normal;
			statement2.B2_PaymentAuthorizationDate = new ZDateTime(2024, 02, 27);
			statement2.B2_PaymentStatus = StatementHeaderPaymentStatusList.Codes.PYC;
			statement2.B2_StatementNumber = "030112000771026";
			var statementLine2 = statement2.StatementLines.AddNew();
			statementLine2.B3_EntryNum = "1234520000045M";
			statementLine2.B3_EntryType = SharedJobMessageTypeList.Codes.Import;

			var statement3 = Factory.New<CusStatementHeader>();
			statement3.B2_StatementType = StatementHeaderTypeList.Codes.CustomsDisbursementBill;
			statement3.B2_PaymentAuthorizationDate = new ZDateTime(2024, 02, 28);
			statement3.B2_PaymentStatus = StatementHeaderPaymentStatusList.Codes.PYC;
			statement3.B2_StatementNumber = "030112000771027";
			var statementLine3 = statement3.StatementLines.AddNew();
			statementLine3.B3_EntryNum = "1234520000045M";
			statementLine3.B3_EntryType = SharedJobMessageTypeList.Codes.Import;

			var statement4 = Factory.New<CusStatementHeader>();
			statement4.B2_StatementType = StatementHeaderTypeList.Codes.CustomsDisbursementBill;
			statement4.B2_PaymentAuthorizationDate = new ZDateTime(2024, 02, 28);
			statement4.B2_PaymentStatus = StatementHeaderPaymentStatusList.Codes.PYI;
			statement4.B2_StatementNumber = "030112000771028";
			var statementLine4 = statement4.StatementLines.AddNew();
			statementLine4.B3_EntryNum = "1234520000045M";
			statementLine4.B3_EntryType = SharedJobMessageTypeList.Codes.Import;
			Factory.Save();
		}
	}
}
