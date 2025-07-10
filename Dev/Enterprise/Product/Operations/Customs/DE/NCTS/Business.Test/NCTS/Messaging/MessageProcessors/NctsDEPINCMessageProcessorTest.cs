using System;
using System.Linq;
using System.Text;
using CargoWise.Customs.DE.MessageContracts.NCTS;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.DE.Business;
using Enterprise.Customs.DE.Business.Testing;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.EU.NCTS.Business.CodeDescriptionPairLists;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Business;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.DE.NCTS.Business.Testing
{
	[TestedType(typeof(NctsDEPINCMessageProcessor))]
	sealed class NctsDEPINCMessageProcessorTest : MessageProcessorAbstractTest<NctsDEPINCMessageProcessor,
		AtlasInboundEDIMessage<IDEPINC>>
	{
		public void TestDocumentsAttachedAsEDocs()
		{
			var eDocsSupporter = nctsHeader as IDocManagerSupportBase;
			AssertEquals("PreReq", 0, eDocsSupporter.DocManagerInfo().AllEDocs.Count);

			messageMock.Setup(x => x.AttachedDocuments).Returns(SampleAttachedDocument);
			ProcessMessage(message);
			AssertEquals("eDoc attached", 1, eDocsSupporter.DocManagerInfo().AllEDocs.Count);
		}

		public void TestEventsCreated()
		{
			ProcessMessage(message);
			Factory.Save();
			AssertEquals("Event Reference", NCTS5DepartureCustomsStatusList.Codes.IncidentRegistered, nctsHeader.MovementHeader.Logs.MostRecentLogByEventTime(Events.CustomsEntryStatus)?.SL_Reference);
		}

		public void TestGetLinkedObjectFromOriginalMessage()
		{
			ProcessMessage(message);
			AssertEquals(nctsHeader.MovementHeader, message.EM_LinkedObject);
		}

		public void TestDataProviderNullDueToInvalidMessage()
		{
			messageMock.Setup(m => m.DataProvider).Returns((IDEPINC)null);
			AssertNoExceptionThrown(() => ProcessMessage(message));
		}

		public void TestProcessMessage()
		{
			ProcessMessage(message);
			AssertEquals("EM_Status", EDIMessage.Status.ProcessedOK, message.EM_Status);
			AssertEquals("BM_CustomsStatus", NCTS5DepartureCustomsStatusList.Codes.IncidentRegistered, nctsHeader.MovementHeader.BM_CustomsStatus);
			AssertEquals("BM_Phase", NctsMovementHeaderTransactionStatusList.Codes.Declaration, nctsHeader.MovementHeader.BM_Phase);
			AssertEquals("BM_MessageStatus", LogicalStatusList.Codes.Accepted, nctsHeader.MovementHeader.BM_MessageStatus);
		}

		public void TestMailIsSentToUser()
		{
			var staff = Factory.New<GlbStaff>();
			staff.GS_Code = "BOB";
			staff.GS_FullName = "BOB THE BUILDER";
			staff.GS_EmailAddress = "bob@where.com";
			lastOutgoingMessageForTest.EM_SystemCreateUser = staff.GS_Code;

			ProcessMessage(message);

			var email = Env.OutgoingCustomsMailManager.EmailsCreated.Single();

			var title = "NCTS Departure Incident Notification. Response for DEPINCTTEST007";
			var subject = $"{title} LRN: {LocalReferenceNumber}";
			var bodyMessageTitle = $"<title>{title}</title>";
			var bodyMessageHeader = ZString.Empty;
			var bodyMessageSummary =
				"Your NCTS Departure Declaration for Job DEPINCTTEST007 has received an incident notification.";

			var table = new StringBuilder();
			var rowFormat = "<tr><td>{0}</td><td>{1}</td></tr>";
			table.AppendFormat(rowFormat, "MRN", "22DE000000001234E0");
			table.AppendFormat(rowFormat, "Incident Time:", "21-Apr-22 12:56");
			table.AppendFormat(rowFormat, "Incident 1 Type", "1 - Der Bef&#246;rderer ist aus von ihm nicht zu vertretenden Gr&#252;nden gezwungen, von der verbindlichen Bef&#246;rderungsroute gem&#228;&#223; Art. 298 UZK-IA abzuweichen.");
			table.AppendFormat(rowFormat, "Incident 1 Text", "Test 1");
			table.AppendFormat(rowFormat, "Incident 2 Type", "2 - Verschl&#252;sse wurden w&#228;hrend der Bef&#246;rderung aus vom Bef&#246;rderer nicht zu vertretenden Gr&#252;nden verletzt oder manipuliert.");
			table.AppendFormat(rowFormat, "Incident 2 Text", "Test 2");

			CombineAssertions(() =>
			{
				AssertEmailForSingleRecipientWithTable(ZString.Empty, email, "bob@where.com", subject, bodyMessageTitle,
					bodyMessageHeader, bodyMessageSummary, table.ToString());
			});
		}

		public void TestPopulateLogbookRegistrationNumber()
		{
			ProcessMessage(message);
			AssertEquals(MovementReferenceNumber, message.GetLogbookRegistrationNumber());
		}

		protected override ZString MessageFriendlyName => "NCTS DEPINC Message Processor";

		protected override DEBranchCustomsApplicationTypeMessageProcessor<AtlasInboundEDIMessage<IDEPINC>> Processor => new NctsDEPINCMessageProcessor(logger);

		protected override void SetUp()
		{
			base.SetUp();

			nctsHeader = Factory.NewWithValidTestData<NctsHeader>();
			nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			nctsHeader.BH_JobReference = "DEPINCTTEST007";
			nctsHeader.MovementReferenceEntryNumber.CE_EntryNum = MovementReferenceNumber;
			nctsHeader.MovementHeader.BM_PaperlessInbondNum = LocalReferenceNumber;

			lastOutgoingMessageForTest = CreateOriginalMessageLinkedToParent<AtlasEDIMessage>(nctsHeader.MovementHeader, "LASTOUTGOING");
			nctsHeader.MovementHeader.Messages.Add(lastOutgoingMessageForTest);

			messageMock = Factory.NewMoq<AtlasInboundEDIMessage<IDEPINC>>();
			messageMock.Setup(m => m.DataProvider)
				.Returns(Mock.Of<IDEPINC>(i =>
					i.MessageIdentifier == "DEPINCTTEST007" &&
					i.ReferencedMessageIdentifier == "" &&
					i.MovementReferenceNumber == MovementReferenceNumber &&
					i.IncidentTime == IncidentTime &&
					i.Incidents == new[]
					{
						Mock.Of<IDEPINCIncident>(i1 => i1.Text == "Test 1" && i1.Type == "1"),
						Mock.Of<IDEPINCIncident>(i2 => i2.Text == "Test 2" && i2.Type == "2"),
					}));
			message = messageMock.Object;
			Factory.Save();
		}
		Mock<AtlasInboundEDIMessage<IDEPINC>> messageMock;
		AtlasInboundEDIMessage<IDEPINC> message;
		NctsHeader nctsHeader;
		EDIMessage lastOutgoingMessageForTest;

		static readonly DateTime IncidentTime = new DateTime(22, 04, 21, 12, 56, 00);
		const string MovementReferenceNumber = "22DE000000001234E0";
		const string LocalReferenceNumber = "19DE485154386041M4";
	}
}
