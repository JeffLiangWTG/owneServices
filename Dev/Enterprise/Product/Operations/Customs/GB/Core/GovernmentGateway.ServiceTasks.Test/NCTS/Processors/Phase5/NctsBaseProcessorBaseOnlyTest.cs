using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Common;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.GB.GovernmentGateway.ServiceTasks.NCTS.Testing;
using Enterprise.Integration;
using Enterprise.Messaging.Business;
using ServiceManager.Integration.ServiceTasks.CW.Test;
using NctsHeader = Enterprise.Customs.GB.Business.NctsHeader;

namespace Enterprise.Customs.GB.GovernmentGateway.ServiceTasks.Ncts.Processors.Phase5.Testing
{
	sealed class NctsBaseProcessorBaseOnlyTest : TestCaseWithFactory
	{
		public void TestFindNctsHeaderUsingDepartureIdOrArrivalId_DepartureLinkedToHeader()
		{
			var (header, outgoingMessage) = SetupTestFindNctsHeaderUsingDepartureIdOrArrivalId(NctsMovementType.Codes.Departure);
			header.Messages.Add(outgoingMessage);

			var incomingMessage = responseHelper.CreateDefaultIncomingMessage(Factory, header, string.Empty, ZDateTime.Now, incomingApplicationReference: "APPREF1");

			AssertEquals(header, processor.FindNctsHeaderUsingDepartureIdOrArrivalId(incomingMessage));
			AssertContains("Matched to BH_12340001 header using arrival/departure ID 'APPREF1' on outgoing message OUT1", serviceLogger.ToString());
			AssertEquals("EM_LinkedObject", header, incomingMessage.EM_LinkedObject);
		}

		public void TestFindNctsHeaderUsingDepartureIdOrArrivalId_DepartureLinkedToMovementHeader()
		{
			var (header, outgoingMessage) = SetupTestFindNctsHeaderUsingDepartureIdOrArrivalId(NctsMovementType.Codes.Departure);
			header.MovementHeader.Messages.Add(outgoingMessage);

			var incomingMessage = responseHelper.CreateDefaultIncomingMessage(Factory, header, string.Empty, ZDateTime.Now, incomingApplicationReference: "APPREF1");

			AssertEquals(header, processor.FindNctsHeaderUsingDepartureIdOrArrivalId(incomingMessage));
			AssertContains("Matched to BH_12340001 departure using ID 'APPREF1' on outgoing message OUT1", serviceLogger.ToString());
			AssertEquals("EM_LinkedObject", header.MovementHeader, incomingMessage.EM_LinkedObject);
		}

		public void TestFindNctsHeaderUsingDepartureIdOrArrivalId_ArrivalLinkedToHeader()
		{
			var (header, outgoingMessage) = SetupTestFindNctsHeaderUsingDepartureIdOrArrivalId(NctsMovementType.Codes.Arrival);
			header.Messages.Add(outgoingMessage);

			var incomingMessage = responseHelper.CreateDefaultIncomingMessage(Factory, header, string.Empty, ZDateTime.Now, incomingApplicationReference: "APPREF1");

			AssertEquals(header, processor.FindNctsHeaderUsingDepartureIdOrArrivalId(incomingMessage));
			AssertContains("Matched to BH_12340001 header using arrival/departure ID 'APPREF1' on outgoing message OUT1", serviceLogger.ToString());
			AssertEquals("EM_LinkedObject", header, incomingMessage.EM_LinkedObject);
		}

		public void TestFindNctsHeaderUsingDepartureIdOrArrivalId_NotFound()
		{
			var (header, outgoingMessage) = SetupTestFindNctsHeaderUsingDepartureIdOrArrivalId(NctsMovementType.Codes.Departure);
			header.Messages.Add(outgoingMessage);

			var incomingMessage = responseHelper.CreateDefaultIncomingMessage(Factory, header, string.Empty, ZDateTime.Now, incomingApplicationReference: "APPREF2");

			AssertNull("Result from FindNctsHeaderUsingDepartureIdOrArrivalId", processor.FindNctsHeaderUsingDepartureIdOrArrivalId(incomingMessage));
			AssertContains("Could not find a suitable outgoing message with the departure/arrival ID 'APPREF2'", serviceLogger.ToString());
			AssertNull("EM_LinkedObject", incomingMessage.EM_LinkedObject);
		}

		public void TestFindNctsHeaderUsingDepartureIdOrArrivalId_FindNullLink()
		{
			var (header, _) = SetupTestFindNctsHeaderUsingDepartureIdOrArrivalId(NctsMovementType.Codes.Departure);

			var incomingMessage = responseHelper.CreateDefaultIncomingMessage(Factory, header, string.Empty, ZDateTime.Now, incomingApplicationReference: "APPREF1");

			AssertNull("Result from FindNctsHeaderUsingDepartureIdOrArrivalId", processor.FindNctsHeaderUsingDepartureIdOrArrivalId(incomingMessage));
			AssertContains("Could not find a suitable outgoing message with the departure/arrival ID 'APPREF1'; found outgoing message OUT1 linked to an unexpected object type ''", serviceLogger.ToString());
			AssertNull("EM_LinkedObject", incomingMessage.EM_LinkedObject);
		}

		public void TestFindNctsHeaderUsingDepartureIdOrArrivalId_MultipleMatchesForDifferentThings()
		{
			var (header1, outgoingMessage1) = SetupTestFindNctsHeaderUsingDepartureIdOrArrivalId(NctsMovementType.Codes.Departure);
			header1.Messages.Add(outgoingMessage1);
			var (header2, outgoingMessage2) = SetupTestFindNctsHeaderUsingDepartureIdOrArrivalId(NctsMovementType.Codes.Departure);
			header2.Messages.Add(outgoingMessage2);

			var incomingMessage = responseHelper.CreateDefaultIncomingMessage(Factory, header1, string.Empty, ZDateTime.Now, incomingApplicationReference: "APPREF1");

			AssertNull("Result from FindNctsHeaderUsingDepartureIdOrArrivalId", processor.FindNctsHeaderUsingDepartureIdOrArrivalId(incomingMessage));
			AssertContains("Could not find a suitable outgoing message with the departure/arrival ID 'APPREF1'; found multiple outgoing messages linked to different objects", serviceLogger.ToString());
			AssertNull("EM_LinkedObject", incomingMessage.EM_LinkedObject);
		}

		public void TestFindNctsHeaderUsingDepartureIdOrArrivalId_MultipleMatchesForSameThing()
		{
			var (header, outgoingMessage1) = SetupTestFindNctsHeaderUsingDepartureIdOrArrivalId(NctsMovementType.Codes.Departure);
			outgoingMessage1.EM_SystemCreateTimeUtc = ZDateTime.UtcNow.AddDays(-1);
			var outgoingMessage2 = CreateOutgoingMessage();
			outgoingMessage2.EM_MessageNum = "OUT2";
			header.Messages.Add(outgoingMessage1);
			header.Messages.Add(outgoingMessage2);

			var incomingMessage = responseHelper.CreateDefaultIncomingMessage(Factory, header, string.Empty, ZDateTime.Now, incomingApplicationReference: "APPREF1");

			AssertEquals(header, processor.FindNctsHeaderUsingDepartureIdOrArrivalId(incomingMessage));
			AssertContains("Should match to header using newest message", "Matched to BH_12340001 header using arrival/departure ID 'APPREF1' on outgoing message OUT2", serviceLogger.ToString());
			AssertEquals("EM_LinkedObject", header, incomingMessage.EM_LinkedObject);
		}

		public void TestLocateHeaderByLRN_Found()
		{
			var header = CreateHeader(NctsMovementType.Codes.Departure);
			header.MovementHeader.BM_PaperlessInbondNum = "LRN1234";
			Factory.Save();

			processor.ExposedLRN = header.MovementHeader.BM_PaperlessInbondNum;
			var foundHeader = processor.LocateHeaderByLRN(Factory);

			AssertEquals(header, foundHeader);
			AssertContains("Found a movement with the LRN 'LRN1234'", serviceLogger.ToString());
		}

		public void TestLocateHeaderByLRN_NotFound()
		{
			_ = CreateHeader(NctsMovementType.Codes.Departure);

			processor.ExposedLRN = "1234";
			var foundHeader = processor.LocateHeaderByLRN(Factory);

			AssertNull(foundHeader);
			AssertContains("Could not find a movement with the LRN '1234'", serviceLogger.ToString());
		}

		public void TestLocateHeaderByMRN_Found()
		{
			var header = CreateHeader(NctsMovementType.Codes.Departure);
			var mrnEntryNumber = CusEntryNumber.New(header, CusEntryNumberTypes.Standard.MovementReferenceNumber, Core.Constants.CountryCodes.UnitedKingdom);
			mrnEntryNumber.CE_EntryNum = "MRN1234";
			Factory.Save();

			processor.ExposedMRN = mrnEntryNumber.CE_EntryNum;
			var foundHeader = processor.LocateHeaderByMRN(Factory);

			AssertEquals(header, foundHeader);
			AssertContains("Found a movement with the MRN 'MRN1234'", serviceLogger.ToString());
		}

		public void TestLocateHeaderByMRN_NotFound()
		{
			_ = CreateHeader(NctsMovementType.Codes.Departure);

			processor.ExposedMRN = "5678";
			var foundHeader = processor.LocateHeaderByMRN(Factory);

			AssertNull(foundHeader);
			AssertContains("Could not find a movement with the MRN '5678'", serviceLogger.ToString());
		}

		(NctsHeader header, EDIMessage outgoingMessage) SetupTestFindNctsHeaderUsingDepartureIdOrArrivalId(string movementType)
		{
			var header = CreateHeader(movementType);
			header.EffectiveMessageStatus = LogicalStatusList.Codes.Sent;
			return (header, CreateOutgoingMessage());
		}

		NctsHeader CreateHeader(string movementType)
		{
			var header = responseHelper.CreateDefaultHeaderForTest(Factory, movementType, string.Empty, jobNo: "BH_12340001");
			header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			return header;
		}

		EDIMessage CreateOutgoingMessage()
		{
			var outgoingMessage = responseHelper.CreateDefaultOutgoingMessage(Factory, "OUT1", string.Empty, ZDateTime.Now, outgoingApplicationReference: "APPREF1");
			outgoingMessage.EM_ApplicationCode = "GBN";
			return outgoingMessage;
		}

		protected override void SetUp()
		{
			base.SetUp();
			serviceLogger = new TestServiceLogger();
			processor = new NctsBaseProcessorForTest(serviceLogger, new LoggingInformation());
		}

		TestServiceLogger serviceLogger;
		NctsBaseProcessorForTest processor;
		readonly CtcNctsResponseHelperTest responseHelper = new() { MessageApplicationCode = EDIInterchange.ApplicationCodes.GbCustomsNCTS };

		class NctsBaseProcessorForTest : NctsBaseProcessor<object>
		{
			public NctsBaseProcessorForTest(ILogger serviceLogger, LoggingInformation loggingInformation) : base(serviceLogger, loggingInformation)
			{
			}

			public new NctsHeader FindNctsHeaderUsingDepartureIdOrArrivalId(EDIMessage inboundMessage) => base.FindNctsHeaderUsingDepartureIdOrArrivalId(inboundMessage);
			public NctsHeader LocateHeaderByLRN(BusinessObjectFactory factory) => base.LocateHeaderByLRN(factory);
			public NctsHeader LocateHeaderByMRN(BusinessObjectFactory factory) => base.LocateHeaderByMRN(factory);

			public string ExposedLRN { get; set; }
			public string ExposedMRN { get; set; }

			protected override ZString LRN => ExposedLRN;

			protected override ZString MRN => ExposedMRN;

			protected override ZString CorrelationIdentifier => ZString.Empty;

			protected override string MessageFriendlyNameCore => throw new System.NotImplementedException();

			protected override string GetMessageId(object messageObject)
			{
				throw new System.NotImplementedException();
			}

			protected override string GetMessageTypeCode(object messageObject)
			{
				throw new System.NotImplementedException();
			}
		}
	}
}
