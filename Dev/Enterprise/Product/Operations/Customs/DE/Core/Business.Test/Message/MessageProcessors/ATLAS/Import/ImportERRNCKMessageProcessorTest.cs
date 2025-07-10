using System.Linq;
using CargoWise.Customs.DE.MessageContracts;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;
using Enterprise.Customs.DE.Business.Testing;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Business;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.DE.Business.Declaration.Testing
{
	[TestedType(typeof(ImportERRNCKMessageProcessor))]
	sealed class ImportERRNCKMessageProcessorTest : ImportMessageProcessorAffectingDeclarationAbstractTest<ImportERRNCKMessageProcessor, AtlasInboundEDIMessage<IERRNCK>>
	{
		public void TestGetLinkedObject()
		{
			ProcessMessage(Message);
			CombineAssertions(() =>
			{
				AssertType("Type is CusEntryHeader", typeof(CusEntryHeader), Message.EM_LinkedObject);
				AssertEquals("Correct LinkedObject obtained", entryHeader, Message.EM_LinkedObject);
			});
		}

		public void TestNoLinkedObject()
		{
			outgoingMessage.EM_MessageNum = "IMP002";
			ProcessMessage(Message);
			CombineAssertions(() =>
			{
				AssertNull("No LinkedObject", Message.EM_LinkedObject);
				AssertEquals("Message status == error", EDIMessage.Status.Error, Message.EM_Status);
			});
		}

		public void TestDataProviderNullDueToInvalidMessage()
		{
			messageMock.Setup(m => m.DataProvider).Returns((IERRNCK)null);
			AssertNoExceptionThrown(() => ProcessMessage(Message));
		}

		public void TestProcessMessage()
		{
			ProcessMessage(Message);
			CombineAssertions(() =>
			{
				AssertEquals("EM_Status", AesEDIMessage.Status.ProcessedOK, Message.EM_Status);
				AssertEquals("CH_EntryStatus", UniversalReferenceConstants.EntryStatus.ERR, entryHeader.CH_EntryStatus);
				AssertEquals("CH_Status", EDIMessageStatusList.Codes.Rejected, entryHeader.CH_Status);
			});
		}

		public void TestEventsCreated()
		{
			ProcessMessage(Message);
			var entryStatus = entryHeader.CH_EntryStatus;
			AssertEquals(entryStatus, entryHeader.Logs.MostRecentLogByEventTime(Events.CustomsEntryStatus)?.SL_Reference);
		}

		public void TestEMailNotification()
		{
			var user = Factory.NewWithValidTestData<GlbStaff>();
			user.GS_EmailAddress = "test@wtg.com";
			outgoingMessage.EM_SystemCreateUser = user.GS_Code;

			ProcessMessage(Message);
			var reference = declaration.JE_DeclarationReference;
			var subject = $"Import Declaration Status Response for {reference}";
			var bodyMessageTitle = $"<title>{subject}</title>";
			var bodyMessageHeader = $@"<strong>Import Declaration Status Response for <a href=""edient:Command=ShowEditForm&LicenceCode={GlbCompany.CurrentCompany.LicenceKeyIdentifier}&ControllerID=JobDeclaration&BusinessEntityPK=" + declaration.PK;
			var bodyMessageSummary = $@"Your Import Declaration for Job {reference} has been rejected. For details please follow the Link to the Job.";
			var bodyMessageTable = @"<table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table""><thead><tr class=""tableheadings"">"
					+ "<th>Error Code</th>"
					+ "<th>Pointer</th>"
					+ "<th>Text</th>"
					+ "<th>Original Value</th>"
					+ "</tr></thead>"
					+ "<tr><td>VEE00701</td>"
					+ "<td>CFCDEC/KOPF</td>"
					+ "<td>Die Beendigung zu diesem Vorgang wurde bereits von der Dienststelle abgeschlossen.</td>"
					+ "<td>00000000000006</td>"
					+ "</tr></table>";
			var email = Env.OutgoingCustomsMailManager.EmailsCreated.Single();
			CombineAssertions(() => AssertEmailForSingleRecipientWithTable(ZString.Empty, email, "test@wtg.com", subject, bodyMessageTitle, bodyMessageHeader, bodyMessageSummary, bodyMessageTable));
		}

		public void TestSimplifiedDeclarationSnapshotDeleted()
		{
			var (reconEntry, reconEntryLine, snapshotEntry, snapshotLine) = PrepareReconEntryAndSnapshots();

			ProcessMessage(Message);

			CombineAssertions("SimplifiedDeclarationSnapshotDeleted", () =>
			{
				AssertEquals("CusReconEntry deleted", true, reconEntry.IsDeleted);
				AssertEquals("CusReconEntryLine deleted", true, reconEntryLine.IsDeleted);
				AssertEquals("CusReconSnapshot of CusReconEntry deleted", true, snapshotEntry.IsDeleted);
				AssertEquals("CusReconSnapshot of CusReconEntryLine deleted", true, snapshotLine.IsDeleted);
			});
		}

		public void TestSimplifiedDeclarationSnapshotRemained()
		{
			var mrnEntryNumber = CusEntryNumber.LoadOrCreate(entryHeader, CusEntryNumberTypes.Standard.MovementReferenceNumber, Core.Constants.CountryCodes.Germany);
			mrnEntryNumber.CE_EntryNum = "ATB150000620520195875";

			var (reconEntry, reconEntryLine, snapshotEntry, snapshotLine) = PrepareReconEntryAndSnapshots();

			ProcessMessage(Message);

			CombineAssertions("SimplifiedDeclarationSnapshotRemained", () =>
			{
				AssertEquals("CusReconEntry not deleted", false, reconEntry.IsDeleted);
				AssertEquals("CusReconEntryLine not deleted", false, reconEntryLine.IsDeleted);
				AssertEquals("CusReconSnapshot of CusReconEntry not deleted", false, snapshotEntry.IsDeleted);
				AssertEquals("CusReconSnapshot of CusReconEntryLine not deleted", false, snapshotLine.IsDeleted);
			});
		}

		public void TestPopulateLogbookRegNum()
		{
			ProcessMessage(Message);

			AssertEquals("ATB150000620520195875", Message.GetLogbookRegistrationNumber());
		}

		public void TestPopulateLogbookRegNum_ShouldUseReferencedMessageIdentifier_WhenReferenceNumberIsNullOrEmpty()
		{
			var dataProviderMock = Mock.Get(messageMock.Object.DataProvider);

			var testCases = new[] { string.Empty, null };
			foreach (var testCase in testCases)
			{
				dataProviderMock.Setup(m => m.ReferenceNumber).Returns(testCase);

				ProcessMessage(Message);

				AssertEquals($"{nameof(IERRNCK.ReferenceNumber)} = \"{testCase}\"", ReferencedMessageIdentifier, Message.GetLogbookRegistrationNumber());
			}
		}

		protected override ZString MessageFriendlyName => "Import ERRNCK Message Processor";

		protected override DEBranchCustomsApplicationTypeMessageProcessor<AtlasInboundEDIMessage<IERRNCK>> Processor => new ImportERRNCKMessageProcessor(logger);

		protected override AtlasInboundEDIMessage<IERRNCK> Message => messageMock.Object;

		protected override void SetUp()
		{
			base.SetUp();

			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
			entryHeader = (CusEntryHeader)declaration.ActiveEntryHeaders.AddNew();

			var subMock1 = new Mock<IERRNCK>();
			subMock1.Setup(x => x.MessageIdentifier).Returns("0000000009");
			subMock1.Setup(x => x.ReferenceNumber).Returns("ATB150000620520195875");
			subMock1.Setup(x => x.ReferencedMessageIdentifier).Returns(ReferencedMessageIdentifier);

			var subMock2 = new Mock<IERRNCKError>();
			subMock2.Setup(x => x.Code).Returns("VEE00701");
			subMock2.Setup(x => x.Pointer).Returns("CFCDEC/KOPF");
			subMock2.Setup(x => x.Text).Returns("Die Beendigung zu diesem Vorgang wurde bereits von der Dienststelle abgeschlossen.");
			subMock2.Setup(x => x.OriginalValue).Returns("00000000000006");

			subMock1.Setup(x => x.Errors).Returns(new IERRNCKError[] { subMock2.Object });
			messageMock = Factory.NewMoq<AtlasInboundEDIMessage<IERRNCK>>();
			messageMock.Setup(x => x.DataProvider).Returns(subMock1.Object);

			outgoingMessage = CreateOriginalMessageLinkedToParent<AtlasEDIMessage>(entryHeader, ReferencedMessageIdentifier);
		}
		CusEntryHeader entryHeader;
		Mock<AtlasInboundEDIMessage<IERRNCK>> messageMock;
		EDIMessage outgoingMessage;
		const string ReferencedMessageIdentifier = "IMP001";

		(CusReconEntry, CusReconEntryLine, CusReconSnapshot, CusReconSnapshot) PrepareReconEntryAndSnapshots()
		{
			var reconEntry = Factory.New<CusReconEntry>();
			reconEntry.CRE_CH_OriginalEntry = entryHeader.PK;

			var snapshotEntry = reconEntry.CusReconSnapshots.AddNew();
			var reconEntryLine = reconEntry.CusReconEntryLines.AddNew();
			var snapshotLine = reconEntryLine.CusReconSnapshots.AddNew();

			return (reconEntry, reconEntryLine, snapshotEntry, snapshotLine);
		}
	}
}
