using System;
using System.Linq;
using CargoWise.Customs.BE.MessageContracts.Interfaces;
using CargoWise.Customs.BE.MessageContracts.MessageProviders;
using CargoWise.Customs.BE.MessageDefinitions.NCTSVersion51_8_2.ctypes;
using CargoWise.Types;
using Enterprise.Customs.Business;
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
	[TestedType(typeof(CC022CMessageProcessor))]
	sealed class CC022CMessageProcessorTest : NctsMessageProcessorTestCase<CC022CMessageProcessor, ICC022CDataProvider>
	{
		public void TestValidate_MatchingMRNWrongSubApplicationCode()
		{
			var nctsHeader = CreateNctsHeader("", NctsTransitStatusList.Codes.DeclarationAccepted);
			nctsHeader.MovementHeader.BM_SubApplicationCode = NctsTypeOfAdditionalDeclarationList.Codes.A;

			Factory.Save();

			Processor.PreProcessMessage(incomingMessage);
			Processor.ProcessMessage(incomingMessage);
			AssertEquals($"EDIMEssage Status Should be '{EDIMessageStatusList.Codes.Failed}'", EDIMessageStatusList.Codes.Failed, incomingMessage.EM_Status);
		}

		[TestDate(2023, 07, 05)]
		public void TestProcessCC022CMessageDeclaration()
		{
			var nctsHeader = CreateNctsHeader("", NCTS5DepartureCustomsStatusList.Codes.MrnAllocated);

			Factory.Save();

			Processor.PreProcessMessage(incomingMessage);
			Processor.ProcessMessage(incomingMessage);
			Factory.Save();

			var departureMovement = nctsHeader.MovementHeader;
			var logEntry = departureMovement.Logs.Find(x => x.SL_SE_NKEvent == Events.CustomsEntryStatus.Code && x.SL_Reference == departureMovement.BM_CustomsStatus).SingleOrDefault();
			var currentDateTime = ZDateTime.Now;

			CombineAssertions(() =>
			{
				AssertEquals(nameof(departureMovement.BM_CustomsStatus), NCTS5DepartureCustomsStatusList.Codes.AmendmentRequested, departureMovement.BM_CustomsStatus);
				AssertEquals(nameof(incomingMessage.EM_Status), EDIMessageStatusList.Codes.ProcessedOK, incomingMessage.EM_Status);
				AssertEquals(nameof(departureMovement.BM_Phase), NctsMovementHeaderTransactionStatusList.Codes.Declaration, departureMovement.BM_Phase);
				AssertEquals(nameof(nctsHeader.EffectiveMessageStatus), LogicalStatusList.Codes.Accepted, nctsHeader.EffectiveMessageStatus);
				AssertNotNull("CES Event not logged", logEntry);
				AssertEquals("Only 1 CES-event should have been created", 1, departureMovement.Logs.Find(x => x.SL_SE_NKEvent == Events.CustomsEntryStatus.Code && x.SL_Reference == departureMovement.BM_CustomsStatus).Count());
				AssertEquals(nameof(logEntry.SL_EventTime), currentDateTime, logEntry.SL_EventTime);
				AssertEquals(nameof(logEntry.SL_GB_NKBranch), GlbBranch.CurrentBranch.DisplayText, logEntry.SL_GB_NKBranch);
				AssertEquals(nameof(logEntry.SL_GE_NKDepartment), GlbDepartment.CurrentDepartment.GE_Code, logEntry.SL_GE_NKDepartment);
				AssertEquals(nameof(logEntry.SL_Parent), departureMovement.PK, logEntry.SL_Parent);
				AssertEquals(nameof(logEntry.SL_Table), CusInBondMoveHeader.Schema.TableName, logEntry.SL_Table);
				AssertEquals(nameof(logEntry.SL_Reference), NCTS5DepartureCustomsStatusList.Codes.AmendmentRequested, logEntry.SL_Reference);
				AssertEquals("ST_NoteText", $"Declaration received a request to amend the declaration on {currentDateTime.ToString("dd-MMM-y HH:mm:ss")}</br>1. Functional error code: 2</br>Reason: No clue</br>Attribute: 3</br>Element in declaration contains now the value: 4</br>", incomingMessage.EM_MessageInterpretation);
			});
		}

		public void TestUnLockEventCreated()
		{
			AssertUnlockEventCreated(NCTS5DepartureCustomsStatusList.Codes.MrnAllocated, "", NCTS5DepartureCustomsStatusList.Codes.ReleasedForTransit, "DEP", NCTS5DepartureCustomsStatusList.Codes.AmendmentRequested, "The tabs are enabled for editing because a Notification to Amend was received.");
		}

		protected override string MovementType => NctsMovementType.Codes.Departure;

		protected override bool SupportsSearchByLRN => false;

		protected override bool SupportsSearchByMRN => true;

		protected override Mock<ICC022CDataProvider> MockProvider => mockProvider ?? (mockProvider = new Mock<ICC022CDataProvider>());
		Mock<ICC022CDataProvider> mockProvider;

		protected override Mock<CC022CMessageProcessor> MockProcessor => mockProcessor ?? (mockProcessor = new Mock<CC022CMessageProcessor>(new BatchProcessor.LoggingInformation()));
		Mock<CC022CMessageProcessor> mockProcessor;

		protected override ZString MessageTypeToInclude => BEIncomingMessageTypes.Codes.CC022C;

		protected override string ExpectedMessageFriendlyName => "Notification to Amend Declaration";

		protected override ZString[] PreProcessedOKCustomsStatus => new ZString[] { NCTS5DepartureCustomsStatusList.Codes.Acknowledged, NCTS5DepartureCustomsStatusList.Codes.MrnAllocated, NCTS5DepartureCustomsStatusList.Codes.PreLodged, NCTS5DepartureCustomsStatusList.Codes.GuaranteeInvalid };

		protected override Type ExpectedMessageInterpreterType => typeof(CC022CMessageInterpreter);

		protected override NctsHeader CreateNctsHeader(string lrn, string customsStatus, string phase = null, string messageStatus = null)
		{
			const string mrn = "22BE000000000012J1";

			var functionalError = FunctionalErrorXmlProvider.New(new FunctionalErrorType01());
			functionalError.SequenceNumber = $"1";
			functionalError.ErrorCode = $"2";
			functionalError.ErrorReason = "No clue";
			functionalError.ErrorPointer = $"3";
			functionalError.OriginalAttributeValue = $"4";

			MockProvider.Setup(x => x.MRN).Returns(mrn);
			MockProvider.Setup(x => x.AmendmentNotificationDateAndTime).Returns(ZDateTime.Now.ToDateTime());
			MockProvider.Setup(x => x.FunctionalErrors).Returns(new[] { functionalError });

			var nctsHeader = base.CreateNctsHeader(lrn, customsStatus, phase, messageStatus);
			Extensions.CreateMovementReferenceNumber(nctsHeader, mrn);
			return nctsHeader;
		}
	}
}
