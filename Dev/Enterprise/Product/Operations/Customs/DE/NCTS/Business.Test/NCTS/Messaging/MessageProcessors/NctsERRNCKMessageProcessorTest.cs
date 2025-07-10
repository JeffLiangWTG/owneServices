using System.Linq;
using CargoWise.Customs.DE.MessageContracts;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.Common.Shared;
using Enterprise.Customs.DE.Business;
using Enterprise.Customs.DE.Business.Testing;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.EU.NCTS.Business.Testing;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Business;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.DE.NCTS.Business.Testing
{
	[TestedType(typeof(NctsERRNCKMessageProcessor))]
	sealed class NctsERRNCKMessageProcessorTest : MessageProcessorAbstractTest<NctsERRNCKMessageProcessor, AtlasInboundEDIMessage<IERRNCK>>
	{
		public void TestDocumentsAttachedAsEDocs_LinkedObjectIsHeader()
		{
			CreateOriginalMessageLinkedToParent<AtlasEDIMessage>(nctsHeader, ReferencedMessageIdentifier);

			var eDocsSupporter = nctsHeader as IDocManagerSupportBase;
			AssertEquals("PreReq", 0, eDocsSupporter.DocManagerInfo().AllEDocs.Count);

			messageMock.Setup(x => x.AttachedDocuments).Returns(SampleAttachedDocument);
			ProcessMessage(message);
			AssertEquals("eDoc attached", 1, eDocsSupporter.DocManagerInfo().AllEDocs.Count);
		}

		public void TestDocumentsAttachedAsEDocs_LinkedObjectIsMovementHeader()
		{
			nctsHeader.BH_HeaderType = EU.NCTS.Business.NctsMovementType.Codes.Departure;
			CreateOriginalMessageLinkedToParent<AtlasEDIMessage>(nctsHeader.MovementHeader, ReferencedMessageIdentifier);

			var eDocsSupporter = nctsHeader as IDocManagerSupportBase;
			AssertEquals("PreReq", 0, eDocsSupporter.DocManagerInfo().AllEDocs.Count);

			messageMock.Setup(x => x.AttachedDocuments).Returns(SampleAttachedDocument);
			ProcessMessage(message);
			AssertEquals("eDoc attached", 1, eDocsSupporter.DocManagerInfo().AllEDocs.Count);
		}

		public void TestEventsCreatedDeparture()
		{
			nctsHeader.BH_HeaderType = EU.NCTS.Business.NctsMovementType.Codes.Departure;
			CreateOriginalMessageLinkedToParent<AtlasEDIMessage>(nctsHeader.MovementHeader, ReferencedMessageIdentifier);

			ProcessMessage(message);
			Factory.Save();

			AssertEquals("Event Reference", LogicalStatusList.Codes.Error, nctsHeader.MovementHeader.Logs.MostRecentLogByEventTime(Events.MessageStatusChange)?.SL_Reference);
		}

		public void TestEventsCreatedArrival()
		{
			nctsHeader.BH_HeaderType = EU.NCTS.Business.NctsMovementType.Codes.Arrival;
			CreateOriginalMessageLinkedToParent<AtlasEDIMessage>(nctsHeader, ReferencedMessageIdentifier);

			ProcessMessage(message);
			Factory.Save();

			AssertEquals("Event Reference", LogicalStatusList.Codes.Error, nctsHeader.ArrivalMovementHeader.Logs.MostRecentLogByEventTime(Events.MessageStatusChange)?.SL_Reference);
		}

		public void TestLinkedObjectNotFound()
		{
			var outgoingMessage = CreateOriginalMessageLinkedToParent<AtlasEDIMessage>(nctsHeader, ReferencedMessageIdentifier);
			outgoingMessage.EM_MessageNum = "NOTORIGINALMSG";

			ProcessMessage(message);

			AssertEquals(EDIMessage.Status.Error, message.EM_Status);
		}

		public void TestGetLinkedObjectFromOriginalMessage()
		{
			CreateOriginalMessageLinkedToParent<AtlasEDIMessage>(nctsHeader, ReferencedMessageIdentifier);

			ProcessMessage(message);

			AssertEquals(nctsHeader, message.EM_LinkedObject);
		}

		public void TestDataProviderNullDueToInvalidMessage()
		{
			messageMock.Setup(m => m.DataProvider).Returns((IERRNCK)null);
			AssertNoExceptionThrown(() => ProcessMessage(message));
		}

		public void TestProcessMessage()
		{
			CreateOriginalMessageLinkedToParent<AtlasEDIMessage>(nctsHeader, ReferencedMessageIdentifier);

			ProcessMessage(message);

			AssertEquals("EM_Status", EDIMessage.Status.ProcessedOK, message.EM_Status);
		}

		public void TestStatusIsSetDeparture()
		{
			nctsHeader.BH_HeaderType = EU.NCTS.Business.NctsMovementType.Codes.Departure;
			CreateOriginalMessageLinkedToParent<AtlasEDIMessage>(nctsHeader.MovementHeader, ReferencedMessageIdentifier);

			ProcessMessage(message);

			AssertEquals("BM_Phase", EU.NCTS.Business.NctsMovementHeaderTransactionStatusList.Codes.Declaration, nctsHeader.MovementHeader.BM_Phase);
			AssertEquals("BM_MessageStatus", LogicalStatusList.Codes.Error, nctsHeader.MovementHeader.BM_MessageStatus);
		}

		public void TestStatusIsSetToREJArrival()
		{
			nctsHeader.BH_HeaderType = EU.NCTS.Business.NctsMovementType.Codes.Arrival;
			CreateOriginalMessageLinkedToParent<AtlasEDIMessage>(nctsHeader, ReferencedMessageIdentifier);

			ProcessMessage(message);
			AssertEquals(LogicalStatusList.Codes.Error, nctsHeader.EffectiveMessageStatus);
		}

		public void TestMessageIsAddedToMessagesTabDeparture()
		{
			nctsHeader.BH_HeaderType = EU.NCTS.Business.NctsMovementType.Codes.Departure;
			CreateOriginalMessageLinkedToParent<AtlasEDIMessage>(nctsHeader.MovementHeader, ReferencedMessageIdentifier);

			ProcessMessage(message);

			nctsHeader.MovementHeader.Messages.Load();
			Assert("message found", nctsHeader.MovementHeader.Messages.Cast<EDIMessage>().Any(m => m.EM_MessageNum == MessageIdentifier));
		}

		public void TestStatusIsSetToMessagesTabArrival()
		{
			nctsHeader.BH_HeaderType = EU.NCTS.Business.NctsMovementType.Codes.Arrival;
			CreateOriginalMessageLinkedToParent<AtlasEDIMessage>(nctsHeader, ReferencedMessageIdentifier);

			ProcessMessage(message);

			Assert("message found", nctsHeader.Messages.Cast<EDIMessage>().Any(m => m.EM_MessageNum == MessageIdentifier));
		}

		public void XTestEDIMessageIsUpdatedCorrectly() // no value in this test, b/c it has to be filled to get here?
		{
			nctsHeader.BH_HeaderType = EU.NCTS.Business.NctsMovementType.Codes.Arrival;
			ProcessMessage(message);
			AssertEquals("DEA", message.EM_ApplicationCode);
			AssertEquals("NCT", message.EM_MessageType);
			AssertEquals("RCV", message.EM_ReceiveTransmit);
		}

		public void TestMailIsSentToUser()
		{
			CreateOriginalMessageLinkedToParent<AtlasEDIMessage>(nctsHeader, ReferencedMessageIdentifier, "bob@where.com");

			ProcessMessage(message);

			var email = Env.OutgoingCustomsMailManager.EmailsCreated.Single();
			var subject = "NCTS Declaration Message Status Response for REFERENCE1";
			var bodyMessageTitle = $"<title>{subject}</title>";
			var bodyMessageHeader = ZString.Empty;
			var bodyMessageSummary = "Your NCTS Declaration Message for Job REFERENCE1 has been rejected.";
			var bodyMessageTable = "<tr class=\"tableheadings\"><th>Error Code</th><th>Pointer</th><th>Text</th><th>Original Value</th></tr></thead>"
				+ "<tr><td>VEE00701</td><td>/DETBRC/KOPF</td><td>Die Beendigung zu diesem Vorgang wurde bereits von der Dienststelle abgeschlossen.</td><td>Original Value Header.</td></tr>"
				+ "<tr><td>VEE00702</td><td>/DETBRC/POS/1</td><td>Positionsfehlerbeschreibungstext.</td><td>Original Value Line.</td></tr>";
			CombineAssertions(() =>
			{
				AssertEmailForSingleRecipientWithTable(ZString.Empty, email, "bob@where.com", subject, bodyMessageTitle, bodyMessageHeader, bodyMessageSummary, bodyMessageTable);
			});
		}

		public void TestPopulateLogbookRegistrationNumber()
		{
			CreateOriginalMessageLinkedToParent<AtlasEDIMessage>(nctsHeader, ReferencedMessageIdentifier);

			ProcessMessage(message);

			AssertEquals(ReferenceNumber, message.GetLogbookRegistrationNumber());
		}

		public void TestPopulateLogbookRegistrationNumber_ShouldUseReferencedMessageIdentifier_WhenReferenceNumberIsNullOrEmpty()
		{
			CreateOriginalMessageLinkedToParent<AtlasEDIMessage>(nctsHeader, ReferencedMessageIdentifier);
			var testCases = new[] { string.Empty, null };
			foreach (var testCase in testCases)
			{
				dataProviderMock.Setup(m => m.ReferenceNumber).Returns(testCase);

				ProcessMessage(message);

				AssertEquals($"{nameof(IERRNCK.ReferenceNumber)} = \"{testCase}\"", ReferencedMessageIdentifier, message.GetLogbookRegistrationNumber());
			}
		}

		public void TestPopulateLogbookLocalReferenceNumber()
		{
			CreateOriginalMessageLinkedToParent<AtlasEDIMessage>(nctsHeader, ReferencedMessageIdentifier);

			ProcessMessage(message);

			AssertEquals(LocalReferenceNumber, message.GetLogbookLocalReferenceNumber());
		}

		public void TestGuaranteeTransactionsSetToDEL()
		{
			NCTSTestHelper.SetupC0009ForEuAndCtCountries(Factory);

			nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
			nctsHeader.MovementHeader.BM_PaperlessInbondNum = LocalReferenceNumber;

			CreateOriginalMessageLinkedToParent<AtlasEDIMessage>(nctsHeader.MovementHeader, ReferencedMessageIdentifier);

			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			var guaranteeHeader = Factory.NewWithValidTestData<CusGuaranteeHeader>();
			guaranteeHeader.CPH_ApplicationCode = CusPermitHeaderApplicationCodeList.Codes.Guarantee;
			guaranteeHeader.CPH_Number = "GUA1";
			guaranteeHeader.CPH_StartDate = ZDate.Today.AddMonths(-1);
			guaranteeHeader.CPH_EndDate = ZDate.Today.AddMonths(1);
			guaranteeHeader.CPH_SystemCreateTimeUtc = ZDate.Today;
			guaranteeHeader.CPH_Type = "TRA";
			guaranteeHeader.CPH_RN_NKCountryCode = Core.Constants.CountryCodes.Germany;
			guaranteeHeader.CPH_Balance = 1000.0m;
			guaranteeHeader.CPH_OH_PermitHolder = org1.PK;
			var transaction = guaranteeHeader.AddTransaction("OPENING", "OPENING", ZString.Empty, ZString.Empty, 1000.0m, 0, transactionType: PermitTransactionTypeList.Codes.OBL, isAggregated: true);

			nctsHeader.Principal.E2_OA_Address = org1.MainAddress.PK;
			nctsHeader.MovementHeader.BM_PaperlessInbondNum = "LRN123456789";

			var guarantee = nctsHeader.MovementHeader.Guarantees.AddNew();
			guarantee.PW_BondType = NctsGuaranteeTypeList.Codes._1;
			guarantee.PW_BondNumber = "GUA1";
			guarantee.PW_BondAmount = 145.0m;
			guarantee.PW_CPH_Guarantee = guaranteeHeader.PK;

			guaranteeHeader.AddTransaction(nctsHeader.MovementHeader.BM_PaperlessInbondNum,
					"NCTS write-off " + nctsHeader.MovementHeader.BM_PaperlessInbondNum + " [" + nctsHeader.MovementReferenceNumber + "]",
					LocalReferenceNumber,
					ZString.Empty,
					guarantee.PW_BondAmount * -1,
					0,
					status: PermitTransactionStatusList.Codes.Pending);

			CombineAssertions(() => {
				AssertEquals("Initial situation GUA1", 2, guaranteeHeader.GetTransactions().Count());
				ProcessMessage(message);
				AssertEquals("After processing GUA1", 2, guaranteeHeader.GetTransactions().Count());
				AssertEquals("After processing GUA1", PermitTransactionStatusList.Codes.Deleted, guaranteeHeader.GetTransactions().Last().CPL_TransactionStatus);
			});
		}

		protected override ZString MessageFriendlyName => "NCTS ERRNCK Message Processor";

		protected override DEBranchCustomsApplicationTypeMessageProcessor<AtlasInboundEDIMessage<IERRNCK>> Processor => new NctsERRNCKMessageProcessor(logger);

		protected override void SetUp()
		{
			base.SetUp();

			nctsHeader = Factory.NewWithValidTestData<NctsHeader>();
			nctsHeader.BH_JobReference = "REFERENCE1";

			dataProviderMock = new Mock<IERRNCK>();

			dataProviderMock.Setup(p => p.MessageIdentifier).Returns(MessageIdentifier);
			dataProviderMock.Setup(p => p.ReferencedMessageIdentifier).Returns(ReferencedMessageIdentifier);
			dataProviderMock.Setup(m => m.ReferenceNumber).Returns(ReferenceNumber);
			dataProviderMock.Setup(m => m.LocalReferenceNumber).Returns(LocalReferenceNumber);
			dataProviderMock.Setup(p => p.Errors).Returns(() =>
			{
				var iErrorArray = new Mock<IERRNCKError>[]
				{
					new Mock<IERRNCKError>(),
					new Mock<IERRNCKError>()
				};

				iErrorArray[0].Setup(e => e.Code).Returns("VEE00701");
				iErrorArray[0].Setup(e => e.Pointer).Returns("/DETBRC/KOPF");
				iErrorArray[0].Setup(e => e.Text).Returns("Die Beendigung zu diesem Vorgang wurde bereits von der Dienststelle abgeschlossen.");
				iErrorArray[0].Setup(e => e.OriginalValue).Returns("Original Value Header.");

				iErrorArray[1].Setup(e => e.Code).Returns("VEE00702");
				iErrorArray[1].Setup(e => e.Pointer).Returns("/DETBRC/POS/1");
				iErrorArray[1].Setup(e => e.Text).Returns("Positionsfehlerbeschreibungstext.");
				iErrorArray[1].Setup(e => e.OriginalValue).Returns("Original Value Line.");

				return iErrorArray.Select(x => x.Object).ToArray();
			});

			messageMock = Factory.NewMoq<AtlasInboundEDIMessage<IERRNCK>>();
			messageMock.Setup(m => m.DataProvider).Returns(dataProviderMock.Object);
			message = messageMock.Object;
			Factory.Save();
		}
		Mock<IERRNCK> dataProviderMock;
		Mock<AtlasInboundEDIMessage<IERRNCK>> messageMock;
		AtlasInboundEDIMessage<IERRNCK> message;
		NctsHeader nctsHeader;

		const string ReferencedMessageIdentifier = "DE441715100000000000000000000477553";
		const string MessageIdentifier = "ERRNCK58750000000381119050419125839";
		const string ReferenceNumber = "22DE000000001234E0";
		const string LocalReferenceNumber = "19DE485154386041M4";
	}
}
