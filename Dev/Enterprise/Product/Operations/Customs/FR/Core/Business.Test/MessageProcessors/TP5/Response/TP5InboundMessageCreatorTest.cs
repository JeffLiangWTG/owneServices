using System;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.FR.Business.EdiMessages;
using Enterprise.Customs.FR.Business.EdiMessages.Testing;
using Enterprise.Customs.FR.Business.NCTS;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.FR.Business.MessageProcessors.Testing
{
	class TP5InboundMessageCreatorTest : TestCaseWithFactory
	{
		[TestDate(2025, 01, 02, 03, 04, 05)]
		public void TestGetLinkedObjectWhenMultipleNctsHeadersHaveTheSameCorrelationId()
		{
			var nctsHeader1 = Factory.New<NctsHeader>();
			nctsHeader1.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			nctsHeader1.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Departure);
			var entryNumber1 = CusEntryNumber.New<CusEntryNumber>(nctsHeader1, CusEntryNumberTypes.EU.CorrelationIdentifier, nctsHeader1.CountryCode);
			entryNumber1.CE_EntryNum = "0000007735";

			var interchange1 = Factory.NewWithValidTestData<EDIInterchange>();
			interchange1.EI_InterchangeNum = "123";
			var message1 = Factory.New<TestEDIMessage>();
			nctsHeader1.MovementHeader.Messages.Add(message1);
			message1.EM_EI = interchange1.PK;
			Factory.Save();
			TestDateAttribute.AddMinutes(1);

			var nctsHeader2 = Factory.New<NctsHeader>();
			nctsHeader2.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			nctsHeader2.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Departure);
			var entryNumber2 = CusEntryNumber.New<CusEntryNumber>(nctsHeader2, CusEntryNumberTypes.EU.CorrelationIdentifier, nctsHeader2.CountryCode);
			entryNumber2.CE_EntryNum = "0000007735";

			var interchange2 = Factory.NewWithValidTestData<EDIInterchange>();
			interchange2.EI_InterchangeNum = "321";
			var message2 = Factory.New<TestEDIMessage>();
			nctsHeader2.MovementHeader.Messages.Add(message2);
			message2.EM_EI = interchange2.PK;

			var interchange = Factory.NewWithValidTestData<EDIInterchange>();
			interchange.EI_From = "EASYLOG2TEST_EAD";
			interchange.EI_To = "HYEDFRCMT";
			interchange.EI_ApplicationCode = ApplicationCodeList.Codes.FRCustomsMessage;
			interchange.EI_InterchangeType = GenericMessageDeliveryInterchangeTypeList.Codes.FRCustomsTP5;
			interchange.EI_InterchangeNum = "321.1.2";
			interchange.EI_ReceiveTransmit = EDIInterchange.Direction.Receive;
			interchange.EI_Status = EDIInterchange.Status.Queued;
			interchange.EI_IsActive = true;
			interchange.EI_GB = GlbBranch.CurrentBranch.PK;
			interchange.EI_BodyText = GetMessageText;
			Factory.Save();

			var creator = new TP5InboundMessageCreator() as IInboundMessageCreator;
			creator.CreateMessagesForInterchange(interchange);

			var messages = Factory.Load<EDIMessage>(new ZQuery(EDIMessageSchema.EM_EI, interchange.PK));
			AssertEquals(1, messages.Length);

			var message = messages[0];
			AssertType<NCTSFREDIMessage>(messages[0]);
			AssertEquals(1, interchange.ContainedMessages.Count);
			AssertEquals(message.PK, interchange.ContainedMessages[0].PK);
			AssertEquals("message linked to interchange", interchange.PK, message.EM_EI);
			AssertEquals("When looking for NCTS jobs by transaction ID, if multiple matching NCTS jobs are found, the target NCTS job will be located based on the interchange number.", nctsHeader2, message.EM_LinkedObject);

			AssertContains("If we find more than 1 matched NCTS jobs, report an error silently", "When looking for NCTS jobs by transaction ID, multiple matching NCTS jobs are found.", ErrorReporter.LastKeyReported);
			AssertContains("Expected message", "More than one CusEntryNum shares the same entry number, they are:", ErrorReporter.LastMessageReported);
			AssertContains("Expected message", "CE_EntryNum: 0000007735, CE_SystemCreateTimeUtc: 02-Jan-25 03:04", ErrorReporter.LastMessageReported);
			AssertContains("Expected message", "CE_EntryNum: 0000007735, CE_SystemCreateTimeUtc: 02-Jan-25 03:05", ErrorReporter.LastMessageReported);
			ErrorReporter.Clear();
		}

		public void TestNoteAddedWhenParentNotFound()
		{
			var interchange = Factory.NewWithValidTestData<EDIInterchange>();
			interchange.EI_From = "EASYLOG2TEST_EAD";
			interchange.EI_To = "HYEDFRCMT";
			interchange.EI_ApplicationCode = ApplicationCodeList.Codes.FRCustomsMessage;
			interchange.EI_InterchangeType = GenericMessageDeliveryInterchangeTypeList.Codes.FRCustomsTP5;
			interchange.EI_InterchangeNum = "237";
			interchange.EI_ReceiveTransmit = EDIInterchange.Direction.Receive;
			interchange.EI_Status = EDIInterchange.Status.Queued;
			interchange.EI_IsActive = true;
			interchange.EI_GB = GlbBranch.CurrentBranch.PK;
			interchange.EI_BodyText = GetMessageText;
			Factory.Save();

			var creator = new TP5InboundMessageCreator() as IInboundMessageCreator;
			creator.CreateMessagesForInterchange(interchange);
			var message = Factory.Load<EDIMessage>(new ZQuery(EDIMessageSchema.EM_EI, interchange.PK))[0];
			AssertEquals("Message should be flagged as discarded.", EDIMessageStatusList.Codes.Discarded, message.EM_Status);
			AssertEquals("A note should have been added with an explicit text.", "Couldn't locate NctsHeader using provided correlationId 0000007735.", (message.Notes.GetAllNotes().First() as StmNote).ST_NoteText);
		}

		public void TestCreateMessagesForInterchange()
		{
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			nctsHeader.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Departure);
			var entryNumber = CusEntryNumber.New<CusEntryNumber>(nctsHeader, CusEntryNumberTypes.EU.CorrelationIdentifier, nctsHeader.CountryCode);
			entryNumber.CE_EntryNum = "0000007735";

			var interchange = Factory.NewWithValidTestData<EDIInterchange>();
			interchange.EI_From = "EASYLOG2TEST_EAD";
			interchange.EI_To = "HYEDFRCMT";
			interchange.EI_ApplicationCode = ApplicationCodeList.Codes.FRCustomsMessage;
			interchange.EI_InterchangeType = GenericMessageDeliveryInterchangeTypeList.Codes.FRCustomsTP5;
			interchange.EI_InterchangeNum = "237";
			interchange.EI_ReceiveTransmit = EDIInterchange.Direction.Receive;
			interchange.EI_Status = EDIInterchange.Status.Queued;
			interchange.EI_IsActive = true;
			interchange.EI_GB = GlbBranch.CurrentBranch.PK;
			interchange.EI_BodyText = GetMessageText;
			Factory.Save();

			var creator = new TP5InboundMessageCreator() as IInboundMessageCreator;
			creator.CreateMessagesForInterchange(interchange);

			var messages = Factory.Load<EDIMessage>(new ZQuery(EDIMessageSchema.EM_EI, interchange.PK));
			AssertEquals(1, messages.Length);

			var message = messages[0];
			AssertType<NCTSFREDIMessage>(messages[0]);
			AssertEquals(1, interchange.ContainedMessages.Count);
			AssertEquals(message.PK, interchange.ContainedMessages[0].PK);
			AssertEquals("message linked to interchange", interchange.PK, message.EM_EI);
			AssertEquals("EM_MessageType", MessageTypeList.Codes.TP5, message.EM_MessageType);
			AssertEquals("EM_MessageSubType", "004", message.EM_MessageSubType);
			AssertEquals("EM_MessageText body text", GetMessageText, message.EM_MessageText);
			AssertEquals("EM_LinkedObject", nctsHeader, message.EM_LinkedObject);
			AssertEquals("Message should be flagged as Queued.", EDIMessageStatusList.Codes.Queued, message.EM_Status);
			AssertEquals("No note should be added when a linked object whose correlationId matches the interchange transactionId is found.", 0, message.Notes.GetAllNotes().Count);
		}

		ZString GetMessageText => resourceRetriever.Value.GetString("Enterprise.Customs.FR.Business.Testing.EDIMessage.TestFiles.NCTS_CC004CResponseMessage.xml");

		readonly Lazy<EmbeddedResourceRetriever> resourceRetriever = new Lazy<EmbeddedResourceRetriever>(() => new EmbeddedResourceRetriever());
	}
}
