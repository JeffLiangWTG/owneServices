using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common.KR;
using Enterprise.Customs.KR.Messaging;
using NUnit.Framework;

namespace Enterprise.Customs.KR.Business.Testing
{
	sealed class VersionNumberExtensionMethodsTest : TestCaseWithFactory
	{
		public void TestRecordVersionNumberFromCH_VersionID()
		{
			var entry = Factory.New<CusEntryHeader>();
			entry.CH_VersionID = 1;

			var message = entry.Messages.AddNew();
			AssertEquals(ZString.Empty, message.EM_ApplicationReference);

			message.RecordVersionNumberFromCH_VersionID(entry);
			AssertEquals("2", message.EM_ApplicationReference);
		}

		public void TestRecordVersionNumberFromCusEntryNum()
		{
			var entry = Factory.New<CusEntryHeader>();
			entry.CH_VersionID = 1;
			var entryNum = entry.EntryNumbers.AddNew();
			entryNum.CE_EntryType = ElectronicDocumentTypeList.Codes._5SC;
			entryNum.CE_EntryLineReference = "2";

			var message = entry.Messages.AddNew();
			AssertEquals(ZString.Empty, message.EM_ApplicationReference);

			message.RecordVersionNumberFromCusEntryNum(entry, ElectronicDocumentTypeList.Codes._5SC);
			AssertEquals("3", message.EM_ApplicationReference);
		}

		public void TestUpdateEntryVersionID()
		{
			var entry = Factory.New<CusEntryHeader>();
			AssertEquals(ZShort.Zero, entry.CH_VersionID);

			var message = entry.Messages.AddNew();
			AssertEquals(ZString.Empty, message.EM_ApplicationReference);

			entry.UpdateEntryVersionID(message);
			AssertEquals(1u, entry.CH_VersionID);

			message.EM_ApplicationReference = "2";
			entry.UpdateEntryVersionID(message);
			AssertEquals(2u, entry.CH_VersionID);
		}

		[TestDate(2023, 11, 10)]
		public void TestCalculation5FEVersionNumber()
		{
			var entry = Factory.NewWithValidTestData<CusEntryHeader>();
			var originalMessage = CreateEDIMessage(entry, "1", ElectronicDocumentTypeList.Codes._929, ZString.Empty, EDIMessage.Direction.Transmit, "1");
			var receiveR99Message = CreateEDIMessage(entry, "2", ElectronicDocumentTypeList.Codes._R99, ElectronicDocumentTypeList.Codes._929, EDIMessage.Direction.Receive, "1");
			entry.CH_VersionID = ZShort.Parse(originalMessage.EM_ApplicationReference);
			AssertEquals(1u, entry.CH_VersionID);
			AssertEquals(1u, entry.CalculateNextCustoms5FEVersionNumber());

			var firstAmendMessage = CreateEDIMessage(entry, "3", ElectronicDocumentTypeList.Codes._5FE, ZString.Empty, EDIMessage.Direction.Transmit, "2");
			var receiveR99Message1 = CreateEDIMessage(entry, "4", ElectronicDocumentTypeList.Codes._R99, ElectronicDocumentTypeList.Codes._5FE, EDIMessage.Direction.Receive, "3");
			entry.CH_VersionID = ZShort.Parse(firstAmendMessage.EM_ApplicationReference);
			AssertEquals(2u, entry.CH_VersionID);
			AssertEquals(2u, entry.CalculateNextCustoms5FEVersionNumber());

			var secondAmendMessage = CreateEDIMessage(entry, "5", ElectronicDocumentTypeList.Codes._5FE, ZString.Empty, EDIMessage.Direction.Transmit, "3");
			var receiveR20Message1 = CreateEDIMessage(entry, "6", ElectronicDocumentTypeList.Codes._R20, ElectronicDocumentTypeList.Codes._5FE, EDIMessage.Direction.Receive, "5");
			AssertEquals(2u, entry.CH_VersionID);
			AssertEquals(2u, entry.CalculateNextCustoms5FEVersionNumber());

			var thirdAmendMessage = CreateEDIMessage(entry, "7", ElectronicDocumentTypeList.Codes._5FE, ZString.Empty, EDIMessage.Direction.Transmit, "3");
			var receiveR99Message2 = CreateEDIMessage(entry, "8", ElectronicDocumentTypeList.Codes._R99, ElectronicDocumentTypeList.Codes._5FE, EDIMessage.Direction.Receive, "7");
			entry.CH_VersionID = ZShort.Parse(thirdAmendMessage.EM_ApplicationReference);
			AssertEquals(3u, entry.CH_VersionID);
			AssertEquals(3u, entry.CalculateNextCustoms5FEVersionNumber());

			var fourthAmendMessage = CreateEDIMessage(entry, "9", ElectronicDocumentTypeList.Codes._5FE, ZString.Empty, EDIMessage.Direction.Transmit, "4");
			var receiveR20Message2 = CreateEDIMessage(entry, "10", ElectronicDocumentTypeList.Codes._R20, ElectronicDocumentTypeList.Codes._5FE, EDIMessage.Direction.Receive, "9");
			AssertEquals(3u, entry.CH_VersionID);
			AssertEquals(3u, entry.CalculateNextCustoms5FEVersionNumber());

			CombineAssertions("Test case. If sent 5FE on a previous date", () =>
			{
				originalMessage.EM_SystemCreateTimeUtc = new ZDateTime("2023-11-09 09:00:00");
				receiveR99Message.EM_SystemCreateTimeUtc = new ZDateTime("2023-11-09 09:30:00");
				firstAmendMessage.EM_SystemCreateTimeUtc = new ZDateTime("2023-11-09 10:00:00");
				receiveR99Message1.EM_SystemCreateTimeUtc = new ZDateTime("2023-11-09 10:30:00");
				secondAmendMessage.EM_SystemCreateTimeUtc = new ZDateTime("2023-11-09 11:00:00");
				receiveR20Message1.EM_SystemCreateTimeUtc = new ZDateTime("2023-11-09 11:30:00");
				thirdAmendMessage.EM_SystemCreateTimeUtc = new ZDateTime("2023-11-09 12:00:00");
				receiveR99Message2.EM_SystemCreateTimeUtc = new ZDateTime("2023-11-09 12:30:00");
				fourthAmendMessage.EM_SystemCreateTimeUtc = new ZDateTime("2023-11-09 13:00:00");
				receiveR20Message2.EM_SystemCreateTimeUtc = new ZDateTime("2023-11-09 13:30:00");
				AssertEquals(3u, entry.CH_VersionID);
				AssertEquals(1u, entry.CalculateNextCustoms5FEVersionNumber());

				var fifthAmendMessage = CreateEDIMessage(entry, "11", ElectronicDocumentTypeList.Codes._5FE, ZString.Empty, EDIMessage.Direction.Transmit, "4");
				var receiveR99Message3 = CreateEDIMessage(entry, "12", ElectronicDocumentTypeList.Codes._R99, ElectronicDocumentTypeList.Codes._5FE, EDIMessage.Direction.Receive, "11");
				entry.CH_VersionID = ZShort.Parse(fifthAmendMessage.EM_ApplicationReference);
				AssertEquals(4u, entry.CH_VersionID);
				AssertEquals(2u, entry.CalculateNextCustoms5FEVersionNumber());
			});
		}

		[TestDate(2024, 02, 10)]
		public void TestSendingObject5FEAmendmentVersion()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = KRJobMessageTypeList.Codes.Import;
			var entry = declaration.CustomsEntryHeaders.AddNew();

			var instruction = declaration.CustomsEntryInstructions.AddNew();
			instruction.CEI_DataModel = Core.Constants.CountryCodes.KoreaSouth;
			entry.CH_CEI_Instruction = instruction.PK;

			Factory.Save();

			CreateEDIMessage(entry, "1", ElectronicDocumentTypeList.Codes._929, "", EDIMessage.Direction.Transmit, "1", new ZDateTime("2024-02-08 10:00:00"));
			CreateEDIMessage(entry, "2", ElectronicDocumentTypeList.Codes._R99, ElectronicDocumentTypeList.Codes._929, EDIMessage.Direction.Receive, "1", new ZDateTime("2024-02-08 10:00:05"));

			var sessionalData5FE1 = CreateSessionalData(entry, "3", ElectronicDocumentTypeList.Codes._5FE, "BX", EDIMessage.Direction.Transmit, "2", new ZDateTime("2024-02-08 11:00:00"));
			CreateEDIMessage(entry, "4", ElectronicDocumentTypeList.Codes._R99, ElectronicDocumentTypeList.Codes._5FE, EDIMessage.Direction.Receive, "3", new ZDateTime("2024-02-08 11:00:05"));

			CreateEDIMessage(entry, "5", ElectronicDocumentTypeList.Codes._5FE, "BX", EDIMessage.Direction.Transmit, "3", new ZDateTime("2024-02-08 12:00:00"));
			CreateEDIMessage(entry, "6", ElectronicDocumentTypeList.Codes._R20, ElectronicDocumentTypeList.Codes._5FE, EDIMessage.Direction.Receive, "5", new ZDateTime("2024-02-08 12:00:02"));

			var sessionalData5FE2 = CreateSessionalData(entry, "7", ElectronicDocumentTypeList.Codes._5FE, "BX", EDIMessage.Direction.Transmit, "3", new ZDateTime("2024-02-08 13:00:00"));
			CreateEDIMessage(entry, "8", ElectronicDocumentTypeList.Codes._R99, ElectronicDocumentTypeList.Codes._5FE, EDIMessage.Direction.Receive, "7", new ZDateTime("2024-02-08 13:00:05"));

			var sendingObject1 = new PenaltyExemptionRequestMessageSendingObject(entry, sessionalData5FE1, 1);
			var sendingObject2 = new PenaltyExemptionRequestMessageSendingObject(entry, sessionalData5FE2, 2);
			AssertEquals(1u, sendingObject1.SessionalData5FE.AmendmentCustomsVersionNo);
			AssertEquals(2u, sendingObject2.SessionalData5FE.AmendmentCustomsVersionNo);

			var sessionalData5FE3 = CreateSessionalData(entry, "9", ElectronicDocumentTypeList.Codes._5FE, "BX", EDIMessage.Direction.Transmit, "4", new ZDateTime("2024-02-09 01:00:00"));
			CreateEDIMessage(entry, "10", ElectronicDocumentTypeList.Codes._R99, ElectronicDocumentTypeList.Codes._5FE, EDIMessage.Direction.Receive, "9", new ZDateTime("2024-02-09 01:00:05"));

			var sessionalData5FE4 = CreateSessionalData(entry, "11", ElectronicDocumentTypeList.Codes._5FE, "BX", EDIMessage.Direction.Transmit, "5", new ZDateTime("2024-02-09 01:30:00"));
			CreateEDIMessage(entry, "12", ElectronicDocumentTypeList.Codes._R99, ElectronicDocumentTypeList.Codes._5FE, EDIMessage.Direction.Receive, "11", new ZDateTime("2024-02-09 01:30:05"));

			var sendingObject3 = new PenaltyExemptionRequestMessageSendingObject(entry, sessionalData5FE3, 3);
			var sendingObject4 = new PenaltyExemptionRequestMessageSendingObject(entry, sessionalData5FE4, 4);
			AssertEquals(1u, sendingObject3.SessionalData5FE.AmendmentCustomsVersionNo);
			AssertEquals(2u, sendingObject4.SessionalData5FE.AmendmentCustomsVersionNo);
		}

		AmendmentSessionalData CreateSessionalData(CusEntryHeader entry, string messageNum, string messageType, string messageSubType, string receiveTransmit, string applicationReference, ZDateTime messageCreateTime = default)
		{
			CreateEDIMessage(entry, messageNum, messageType, messageSubType, receiveTransmit, applicationReference, messageCreateTime);

			var sessionalData = entry.EntryInstruction.AmendmentSessionalDataCollection.AddNew();
			sessionalData.CSI_Code = DutyTaxCorrectionCodeList.Codes.B;
			sessionalData.CSI_LineNo = ZInt.ParseEmptyAsZero(applicationReference);

			return sessionalData;
		}
		EDIMessage CreateEDIMessage(CusEntryHeader entry, string messageNum, string messageType, string messageSubType, string receiveTransmit, string applicationReference, ZDateTime messageCreateTime = default)
		{
			var message = entry.Messages.AddNew();
			message.EM_MessageNum = messageNum;
			message.EM_MessageType = messageType;
			message.EM_MessageSubType = messageSubType;
			message.EM_SystemCreateTimeUtc = messageCreateTime == default ? ZDateTime.Today : messageCreateTime;
			message.EM_ReceiveTransmit = receiveTransmit;
			message.EM_ApplicationReference = applicationReference;
			return message;
		}
	}
}
