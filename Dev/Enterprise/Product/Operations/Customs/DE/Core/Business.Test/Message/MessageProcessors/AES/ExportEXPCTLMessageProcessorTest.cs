using System.Linq;
using CargoWise.Customs.DE.MessageContracts.AES;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;
using Enterprise.Customs.DE.Business.Testing;
using Enterprise.Customs.DE.Messaging;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.DE.Business.Declaration.Testing
{
	[TestedType(typeof(ExportEXPCTLMessageProcessor))]
	sealed class ExportEXPCTLMessageProcessorTest : MessageProcessorAbstractTest<ExportEXPCTLMessageProcessor, AesInboundEDIMessage<IEXPCTL>>
	{
		protected override ZString MessageFriendlyName => "Export EXPCTL Message Processor";

		protected override DEBranchCustomsApplicationTypeMessageProcessor<AesInboundEDIMessage<IEXPCTL>> Processor => new ExportEXPCTLMessageProcessor(logger);

		public void TestGetLinkedObject()
		{
			ProcessMessage(message);
			CombineAssertions(() =>
			{
				AssertType("Type is CusEntryHeader", typeof(CusEntryHeader), message.EM_LinkedObject);
				AssertEquals("Correct LinkedObject obtained", entryHeader, message.EM_LinkedObject);
			});
		}

		public void TestGetLinkedObject_ReferencedOriginalMessage()
		{
			dataProviderMock.Setup(m => m.MovementReferenceNumber).Returns((string)null);
			dataProviderMock.Setup(e => e.ReferencedMessageIdentifier).Returns("outgoing");

			ProcessMessage(message);
			CombineAssertions(() =>
			{
				AssertSame("Correct LinkedObject obtained", entryHeader, message.EM_LinkedObject);
			});
		}

		public void TestNoLinkedObject()
		{
			dataProviderMock.Setup(m => m.MovementReferenceNumber).Returns("ATB0000000000000");
			ProcessMessage(message);
			CombineAssertions(() =>
			{
				AssertNull("No LinkedObject", message.EM_LinkedObject);
				AssertEquals("Message status == error", AesEDIMessage.Status.Error, message.EM_Status);
			});
		}

		public void TestDataProviderNullDueToInvalidMessage()
		{
			messageMock.Setup(m => m.DataProvider).Returns((IEXPCTL)null);
			AssertNoExceptionThrown(() => ProcessMessage(message));
		}

		public void TestProcessMessage()
		{
			ProcessMessage(message);
			CombineAssertions(() =>
			{
				AssertEquals("EM_Status", AesEDIMessage.Status.ProcessedOK, message.EM_Status);
				AssertEquals("HasExportControlMessageUnreadEDoc", YesNoList.Codes.Yes, declaration.ControlMessageUnreadEDocStatus);
				var reference = entryHeader.Declaration.JE_DeclarationReference;
				var lrn = entryHeader.LocalReferenceNumber;
				var subject = $"AES EXP Control measure Message Response for {reference} - LRN: {lrn}";
				var titleSubject = $"AES EXP Control measure Message Response for {reference}";
				var bodyMessageTitle = $"<title>{titleSubject}</title>";
				var bodyMessageHeader = $@"<strong>AES EXP Control measure Message Response for <a href=""edient:Command=ShowEditForm&LicenceCode={GlbCompany.CurrentCompany.LicenceKeyIdentifier}&ControllerID=JobDeclaration&BusinessEntityPK=" + entryHeader.Declaration.PK;
				var bodyMessageSummary = $@"Your Export Declaration Message for Job {reference} has a control measure request. For details please follow the Link to the Job.<br /><br />MRN: ATB150000620520195875<br /><br />";
				var bodyMessageTable = @"<table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table""><thead><tr class=""tableheadings"">"
						+ "<th>Line No.</th><th>Control Type</th><th>Annotation</th></tr></thead>"
						+ "<tr><td>1</td><td>10 - Dokumentenkontrollen</td><td>ANNOTATION</td></tr></table>";
				var email = Env.OutgoingCustomsMailManager.EmailsCreated.Single();
				AssertEmailForSingleRecipientWithTable(ZString.Empty, email, "Dummy@dummy.com", subject, bodyMessageTitle, bodyMessageHeader, bodyMessageSummary, bodyMessageTable);
			});
		}

		public void TestPopulateLogbookRegNum()
		{
			ProcessMessage(message);
			AssertEquals("ATB150000620520195875", message.GetLogbookRegistrationNumber());
		}

		public void TestPopulateLogbookLocalReferenceNumber()
		{
			ProcessMessage(message);
			AssertEquals("Reference1234567abcd", message.GetLogbookLocalReferenceNumber());
		}

		public void TestAddCustomsEntryStatusLog()
		{
			ProcessMessage(message);
			AssertEquals(UniversalReferenceConstants.EntryStatus.CTL, entryHeader.GetCustomsEntryStatusEventReference());
		}

		protected override void SetUp()
		{
			base.SetUp();

			declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
			entryHeader = (CusEntryHeader)declaration.ActiveEntryHeaders.AddNew();

			var mrnEntryNumber = CusEntryNumber.LoadOrCreate(entryHeader, Core.Constants.CountryCodes.Germany);
			mrnEntryNumber.CE_EntryNum = "ATB150000620520195875";
			mrnEntryNumber.CE_EntryType = CusEntryNumberTypes.Standard.MovementReferenceNumber;

			var staff = Factory.LoadFromNaturalKey<GlbStaff>(GlbStaffSchema.GS_Code, GlbStaff.CurrentUser.GS_Code);
			staff.GS_EmailAddress = "Dummy@dummy.com";
			var groupPK = EnvProxy.Instance.Registry.PostMasterGroup;
			var group = Factory.Load<GlbGroup>(groupPK);
			group.Staff.Add(staff);

			outgoingMessage = CreateOriginalMessageLinkedToParent<AesEDIMessage>(entryHeader, "outgoing");
			outgoingMessage.EM_ApplicationCode = "DEE";
			outgoingMessage.EM_MessageType = EDIMessageTypeList.Codes.AES;
			outgoingMessage.EM_MessageSubType = ExportMessageSubTypeList.Codes.EXP;
			outgoingMessage.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;

			var dataProviderMockLine = new Mock<IEXPCTLLine>();
			dataProviderMockLine.Setup(x => x.Annotation).Returns("ANNOTATION");
			dataProviderMockLine.Setup(x => x.ControlMeasureType).Returns("10");
			dataProviderMockLine.Setup(x => x.SequenceNumber).Returns("1");
			dataProviderMock = new Mock<IEXPCTL>();
			dataProviderMock.Setup(x => x.MessageIdentifier).Returns("0000000009");
			dataProviderMock.Setup(x => x.MovementReferenceNumber).Returns("ATB150000620520195875");
			dataProviderMock.Setup(x => x.LocalReferenceNumber).Returns("Reference1234567abcd");
			dataProviderMock.Setup(x => x.Lines).Returns(new IEXPCTLLine[] { dataProviderMockLine.Object });

			messageMock = Factory.NewMoq<AesInboundEDIMessage<IEXPCTL>>();
			messageMock.Setup(x => x.DataProvider).Returns(dataProviderMock.Object);
			message = messageMock.Object;

			Factory.Save();
		}
		JobDeclaration declaration;
		CusEntryHeader entryHeader;
		Mock<IEXPCTL> dataProviderMock;
		Mock<AesInboundEDIMessage<IEXPCTL>> messageMock;
		EDIMessage outgoingMessage;
		AesInboundEDIMessage<IEXPCTL> message;
	}
}
