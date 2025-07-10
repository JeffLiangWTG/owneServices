using System;
using System.Linq;
using CargoWise.Customs.DE.MessageContracts.AES;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;
using Enterprise.Customs.DE.Business.Testing;
using Enterprise.Customs.EU.Business.CodeDescriptionPairLists;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.DE.Business.Declaration.Testing
{
	[TestedType(typeof(ExportEXPFUPMessageProcessor))]
	sealed class ExportEXPFUPMessageProcessorTest : MessageProcessorAbstractTest<ExportEXPFUPMessageProcessor, AesInboundEDIMessage<IEXPFUP>>
	{
		protected override ZString MessageFriendlyName => "Export EXPFUP Message Processor";

		protected override DEBranchCustomsApplicationTypeMessageProcessor<AesInboundEDIMessage<IEXPFUP>> Processor => new ExportEXPFUPMessageProcessor(logger);

		public void TestGetLinkedObjectFromOriginalMessage()
		{
			ProcessMessage(message);
			AssertEquals(entryHeader, message.EM_LinkedObject);
		}

		public void TestGetLinkedObjectFromMRN()
		{
			dataProviderMock.Setup(x => x.ReferencedMessageIdentifier).Returns("INVALID_CODE");
			dataProviderMock.Setup(x => x.MovementReferenceNumber).Returns("00DE000000000000E0");

			ProcessMessage(message);

			AssertEquals(entryHeader, message.EM_LinkedObject);
		}

		public void TestProcessMessage()
		{
			ProcessMessage(message);
			CombineAssertions(() =>
			{
				AssertEquals("EM_Status", EDIMessage.Status.ProcessedOK, message.EM_Status);

				var reference = entryHeader.Declaration.JE_DeclarationReference;
				var lrn = entryHeader.LocalReferenceNumber;
				var subject = $"AES EXP Follow Up Request Response for {reference} - LRN: {lrn}";
				var titleSubject = $"AES EXP Follow Up Request Response for {reference}";
				var bodyMessageTitle = $"<title>{titleSubject}</title>";
				var bodyMessageHeader = $@"<strong>AES EXP Follow Up Request Response for <a href=""edient:Command=ShowEditForm&LicenceCode={GlbCompany.CurrentCompany.LicenceKeyIdentifier}&ControllerID=JobDeclaration&BusinessEntityPK=" + entryHeader.Declaration.PK;
				var bodyMessageSummary = $@"Your Export Declaration Message for Job {reference} has a follow up request. For details please follow the Link to the Job.
Shown below is a summary of relevant information received in the message:";
				var bodyMessageTable = @"<table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table"">"
						+ "<tr><td>Local Reference No.</td><td>Reference1234567abcd</td></tr>"
						+ "<tr><td>MRN</td><td>21DE123050554788M5</td></tr>"
						+ "<tr><td>Request Date</td><td>22.07.2021</td></tr>"
						+ "<tr><td>Response Date</td><td>25.07.2021</td></tr>"
						+ "<tr><td>Presentation Date</td><td>30.07.2021</td></tr>"
						+ "</table>";
				var email = Env.OutgoingCustomsMailManager.EmailsCreated.Single();
				AssertEmailForSingleRecipientWithTable(ZString.Empty, email, "Dummy@dummy.com", subject, bodyMessageTitle, bodyMessageHeader, bodyMessageSummary, bodyMessageTable);
			});
		}

		public void TestPopulateLogbookRegNum()
		{
			ProcessMessage(message);
			AssertEquals("21DE123050554788M5", message.GetLogbookRegistrationNumber());
		}

		public void TestPopulateLogbookLocalReferenceNumber()
		{
			ProcessMessage(message);
			AssertEquals("Reference1234567abcd", message.GetLogbookLocalReferenceNumber());
		}

		public void TestAddCustomsEntryStatusLog()
		{
			ProcessMessage(message);
			AssertEquals(UniversalReferenceConstants.EntryStatus.FUP, entryHeader.GetCustomsEntryStatusEventReference());
		}

		public void TestExitedStatusUpdated()
		{
			ProcessMessage(message);
			AssertEquals(ExportExitStatus.Codes.ReminderForNonExitedGoodsReceived, entryHeader.CH_ExitedStatus);
		}

		protected override void SetUp()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;

			entryHeader = (CusEntryHeader)declaration.ActiveEntryHeaders.AddNew();
			var entryNum = Factory.NewWithValidTestData<CusEntryNumber>();
			entryNum.CE_EntryNum = "00DE000000000000E0";
			entryNum.CE_EntryType = CusEntryNumberTypes.Standard.MovementReferenceNumber;
			entryNum.Parent = entryHeader;

			var staff = Factory.LoadFromNaturalKey<GlbStaff>(GlbStaffSchema.GS_Code, GlbStaff.CurrentUser.GS_Code);
			staff.GS_EmailAddress = "Dummy@dummy.com";
			var groupPK = EnvProxy.Instance.Registry.PostMasterGroup;
			var group = Factory.Load<GlbGroup>(groupPK);
			group.Staff.Add(staff);

			_ = CreateOriginalMessageLinkedToParent<AesEDIMessage>(entryHeader, "EXPFUPTest");

			dataProviderMock = new Mock<IEXPFUP>();
			dataProviderMock.Setup(x => x.MessageIdentifier).Returns("0000000009");
			dataProviderMock.Setup(x => x.ReferencedMessageIdentifier).Returns("EXPFUPTest");
			dataProviderMock.Setup(x => x.LatestPresentationDate).Returns(new DateTime(2021, 7, 30));
			dataProviderMock.Setup(x => x.LatestResponseDate).Returns(new DateTime(2021, 7, 25));
			dataProviderMock.Setup(x => x.RequestDate).Returns(new DateTime(2021, 7, 22));
			dataProviderMock.Setup(x => x.LocalReferenceNumber).Returns("Reference1234567abcd");
			dataProviderMock.Setup(x => x.MovementReferenceNumber).Returns("21DE123050554788M5");

			var messageMock = Factory.NewMoq<AesInboundEDIMessage<IEXPFUP>>();
			messageMock.Setup(x => x.DataProvider).Returns(dataProviderMock.Object);
			message = messageMock.Object;

			Factory.Save();
		}
		CusEntryHeader entryHeader;
		AesInboundEDIMessage<IEXPFUP> message;
		Mock<IEXPFUP> dataProviderMock;
	}
}
