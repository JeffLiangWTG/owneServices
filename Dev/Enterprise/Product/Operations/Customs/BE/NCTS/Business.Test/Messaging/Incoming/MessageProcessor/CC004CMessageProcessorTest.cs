using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Customs.BE.MessageContracts.Interfaces;
using CargoWise.Types;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.EU.NCTS.Business.CodeDescriptionPairLists;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Business;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.BE.NCTS.Business.Testing
{
	[TestedType(typeof(CC004CMessageProcessor))]
	sealed class CC004CMessageProcessorTest : NctsMessageProcessorTestCase<CC004CMessageProcessor, ICC004CDataProvider>
	{
		public void TestProcessCC004CMessageDeclaration()
		{
			var currentDateTime = DateTime.Now;
			MockProvider.Setup(x => x.EntryDate).Returns(currentDateTime);
			MockProvider.Setup(x => x.MRN).Returns("22BE000000000012J1");
			MockProvider.Setup(x => x.LRN).Returns("22045281480600000001");
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
			nctsHeader.EffectiveMessageStatus = LogicalStatusList.Codes.Sent;
			var movementHeader = nctsHeader.MovementHeader;
			movementHeader.BM_PaperlessInbondNum = "22045281480600000001";
			movementHeader.BM_Phase = NctsMovementHeaderTransactionStatusList.Codes.Declaration;
			movementHeader.BM_CustomsStatus = NCTS5DepartureCustomsStatusList.Codes.AmendmentRequested;
			movementHeader.BM_AdditionalDeclarationType = NctsTypeOfAdditionalDeclarationList.Codes.A;
			Extensions.CreateMovementReferenceNumber(nctsHeader, "22BE000000000012J1");
			Factory.Save();
			Processor.PreProcessMessage(incomingMessage);
			Processor.ProcessMessage(incomingMessage);
			Factory.Save();

			CombineAssertions(() =>
			{
				AssertEquals($"EDIMEssage Status Should BE {EDIMessageStatusList.Codes.ProcessedOK}", EDIMessageStatusList.Codes.ProcessedOK, incomingMessage.EM_Status);
				AssertEquals("CusInBondMoveHeader Customs Status Should BE MRN", NCTS5DepartureCustomsStatusList.Codes.MrnAllocated, movementHeader.BM_CustomsStatus);
				AssertEquals("CusInBondMoveHeader Phase Should BE '015'", NctsMovementHeaderTransactionStatusList.Codes.Declaration, movementHeader.BM_Phase);
				AssertEquals("Message Status Should BE 'ACC'", LogicalStatusList.Codes.Accepted, nctsHeader.EffectiveMessageStatus);
				AssertNotNull("CusInBondMoveHeader Should have a CES event logged", movementHeader.Logs.Find(x => x.SL_SE_NKEvent == Events.CustomsEntryStatus.Code && x.SL_Reference == movementHeader.BM_CustomsStatus).SingleOrDefault());
				AssertEquals("Only 1 CES-event should have been created", 1, movementHeader.Logs.Find(x => x.SL_SE_NKEvent == Events.CustomsEntryStatus.Code && x.SL_Reference == movementHeader.BM_CustomsStatus).Count());
				AssertContains("ST_NoteText", "Amendment acceptance", incomingMessage.EM_MessageInterpretation);
			});
		}

		public void TestProcessCC004CMessageDeclaration_WithReleaseDate()
		{
			var currentDateTime = DateTime.Now;
			MockProvider.Setup(x => x.EntryDate).Returns(currentDateTime);
			MockProvider.Setup(x => x.MRN).Returns("22BE000000000012J1");
			MockProvider.Setup(x => x.LRN).Returns("22045281480600000001");
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Departure);
			nctsHeader.EffectiveMessageStatus = LogicalStatusList.Codes.Sent;
			var movementHeader = nctsHeader.MovementHeader;
			movementHeader.BM_PaperlessInbondNum = "22045281480600000001";
			movementHeader.BM_Phase = NctsMovementHeaderTransactionStatusList.Codes.Declaration;
			movementHeader.BM_CustomsStatus = NCTS5DepartureCustomsStatusList.Codes.AmendmentRequested;
			movementHeader.BM_AdditionalDeclarationType = NctsTypeOfAdditionalDeclarationList.Codes.A;
			Extensions.CreateMovementReferenceNumber(nctsHeader, "22BE000000000012J1");
			nctsHeader.MovementReferenceEntryNumber.CE_IssueDate = currentDateTime;
			Factory.Save();
			Processor.PreProcessMessage(incomingMessage);
			Processor.ProcessMessage(incomingMessage);
			Factory.Save();

			CombineAssertions(() =>
			{
				AssertEquals($"EDIMEssage Status Should BE {EDIMessageStatusList.Codes.ProcessedOK}", EDIMessageStatusList.Codes.ProcessedOK, incomingMessage.EM_Status);
				AssertEquals("CusInBondMoveHeader Customs Status Should BE REL", NCTS5DepartureCustomsStatusList.Codes.ReleasedForTransit, movementHeader.BM_CustomsStatus);
				AssertEquals("CusInBondMoveHeader Phase Should BE '015'", NctsMovementHeaderTransactionStatusList.Codes.Declaration, movementHeader.BM_Phase);
				AssertEquals("Message Status Should BE 'ACC'", LogicalStatusList.Codes.Accepted, nctsHeader.EffectiveMessageStatus);
				AssertNotNull("CusInBondMoveHeader Should have a CES event logged", movementHeader.Logs.Find(x => x.SL_SE_NKEvent == Events.CustomsEntryStatus.Code && x.SL_Reference == movementHeader.BM_CustomsStatus).SingleOrDefault());
				AssertContains("ST_NoteText", "Amendment acceptance", incomingMessage.EM_MessageInterpretation);
			});
		}

		public void TestProcessCC004CMessageDeclaration_NoReleaseDate_WithoutMRN()
		{
			var currentDateTime = DateTime.Now;
			MockProvider.Setup(x => x.EntryDate).Returns(currentDateTime);
			MockProvider.Setup(x => x.MRN).Returns(string.Empty);
			MockProvider.Setup(x => x.LRN).Returns("22045281480600000001");
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Departure);
			nctsHeader.EffectiveMessageStatus = LogicalStatusList.Codes.Sent;
			var movementHeader = nctsHeader.MovementHeader;
			movementHeader.BM_PaperlessInbondNum = "22045281480600000001";
			movementHeader.BM_Phase = NctsMovementHeaderTransactionStatusList.Codes.Declaration;
			movementHeader.BM_CustomsStatus = NCTS5DepartureCustomsStatusList.Codes.AmendmentRequested;
			movementHeader.BM_AdditionalDeclarationType = NctsTypeOfAdditionalDeclarationList.Codes.A;
			Factory.Save();
			Processor.PreProcessMessage(incomingMessage);
			Processor.ProcessMessage(incomingMessage);
			Factory.Save();

			CombineAssertions(() =>
			{
				AssertEquals($"EDIMEssage Status Should BE {EDIMessageStatusList.Codes.ProcessedOK}", EDIMessageStatusList.Codes.ProcessedOK, incomingMessage.EM_Status);
				AssertEquals("CusInBondMoveHeader Customs Status Should BE REL", NCTS5DepartureCustomsStatusList.Codes.AmendmentRequested, movementHeader.BM_CustomsStatus);
				AssertEquals("CusInBondMoveHeader Phase Should BE '015'", NctsMovementHeaderTransactionStatusList.Codes.Declaration, movementHeader.BM_Phase);
				AssertEquals("Message Status Should BE 'ACC'", LogicalStatusList.Codes.Accepted, nctsHeader.EffectiveMessageStatus);
				AssertNotNull("CusInBondMoveHeader Should have a CES event logged", movementHeader.Logs.Find(x => x.SL_SE_NKEvent == Events.CustomsEntryStatus.Code && x.SL_Reference == movementHeader.BM_CustomsStatus).FirstOrDefault());
				AssertContains("ST_NoteText", "Amendment acceptance", incomingMessage.EM_MessageInterpretation);
			});
		}

		public void TestDiscardedStatusAndNote()
		{
			var currentDateTime = DateTime.Now;
			MockProvider.Setup(x => x.EntryDate).Returns(currentDateTime);
			MockProvider.Setup(x => x.MRN).Returns(string.Empty);
			MockProvider.Setup(x => x.LRN).Returns("22045281480600000001");
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Departure);
			nctsHeader.EffectiveMessageStatus = LogicalStatusList.Codes.Sent;
			var movementHeader = nctsHeader.MovementHeader;
			movementHeader.BM_PaperlessInbondNum = "22045281480600000001";
			movementHeader.BM_Phase = NctsMovementHeaderTransactionStatusList.Codes.Declaration;
			movementHeader.BM_CustomsStatus = NCTS5DepartureCustomsStatusList.Codes.AdditionalDocumentsRequest;
			movementHeader.BM_AdditionalDeclarationType = NctsTypeOfAdditionalDeclarationList.Codes.A;
			Factory.Save();
			Processor.PreProcessMessage(incomingMessage);

			CombineAssertions(() =>
			{
				AssertEquals($"EDIMessage status should be {EDIMessageStatusList.Codes.Discarded}", EDIMessageStatusList.Codes.Discarded, incomingMessage.EM_Status);
				var note = incomingMessage.Notes.FindByDescription(Constants.MessageProcessingNotes.ProcessingLog).Single();
				AssertContains("Note text", "The message was discarded, because the Status at Customs of the declaration is different from ACK, PRE, MRN, AMR or GIV. In case of GIV, phase status needs to be 013 and message status SNT or ACK", note.ST_NoteText);
			});
		}

		protected override NctsHeader SetupAndProcessMessage(List<BE.Business.CusGuaranteeHeader> guaranteeHeaderList, OrgHeader org, NctsHeader nctsHeader)
		{
			nctsHeader.Logs.CreateRecreateOrUpdateEventLog(new EventValue(Events.CustomsEntryStatus, eventTime: DateTime.Now, reference: "REL"));
			return base.SetupAndProcessMessage(guaranteeHeaderList, org, nctsHeader);
		}

		protected override bool SupportsSearchByLRN => true;

		protected override bool SupportsSearchByMRN => true;

		protected override Mock<ICC004CDataProvider> MockProvider => mockProvider ?? (mockProvider = new Mock<ICC004CDataProvider>());
		Mock<ICC004CDataProvider> mockProvider;

		protected override Mock<CC004CMessageProcessor> MockProcessor => mockProcessor ?? (mockProcessor = new Mock<CC004CMessageProcessor>(new BatchProcessor.LoggingInformation()));
		Mock<CC004CMessageProcessor> mockProcessor;

		protected override ZString MessageTypeToInclude => BEIncomingMessageTypes.Codes.CC004C;

		protected override string ExpectedMessageFriendlyName => BEIncomingMessageTypes.Descriptions.CC004C;

		protected override ZString[] PreProcessedOKCustomsStatus => new ZString[] { NCTS5DepartureCustomsStatusList.Codes.Acknowledged };

		protected override Type ExpectedMessageInterpreterType => typeof(CC004CMessageInterpreter);

		protected override string MovementType => NctsMovementType.Codes.Departure;

		protected override ZBool MessageConfirmsGuaranteeTransactions => true;
	}
}
