using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CargoWise.Customs.DE.MessageContracts.AES;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;
using Enterprise.Customs.DE.Business.Testing;
using Enterprise.Customs.EU.Business.CodeDescriptionPairLists;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.DE.Business.Declaration.Testing
{
	[TestedType(typeof(ExportEXPNOTMessageProcessor))]
	sealed class ExportEXPNOTMessageProcessorTest : MessageProcessorAbstractTest<ExportEXPNOTMessageProcessor, AesInboundEDIMessage<IEXPNOT>>
	{
		public void TestEmailAttachments()
		{
			var reference = entryHeader.Declaration.JE_DeclarationReference;
			var lrn = entryHeader.LocalReferenceNumber;
			var subject = $"AES EXP Response for {reference} - LRN: {lrn}";
			entryHeader.CH_EntryStatus = "501";
			messageMock.Setup(m => m.AttachedDocuments).Returns(new List<AttachedDocument>
			{
				new AttachedDocument
				{
					FileName = "file1.pdf",
					Type = new DocumentType { Code = "AAA", Description = "AAA Desc" },
					ImageData = (SubStreamableStream)new MemoryStream(Convert.FromBase64String("XXX=")),
				},
				new AttachedDocument
				{
					FileName = "file2.pdf",
					Type = new DocumentType { Code = "BBB", Description = "BBB Desc" },
					ImageData = (SubStreamableStream)new MemoryStream(Convert.FromBase64String("YYY=")),
				}
			});
			ProcessMessage(message);
			var email = Env.OutgoingCustomsMailManager.EmailsCreated.Single(x => x.Subject == subject);
			var emailAttachments = email.Attachments.Cast<AttachmentDef>();
			var file1 = emailAttachments.Single(x => x.DisplayName == "file1.pdf");
			var file2 = emailAttachments.Single(x => x.DisplayName == "file2.pdf");
			CombineAssertions(() =>
			{
				AssertEquals("File1 content", Convert.FromBase64String("XXX="), file1.Data);
				AssertEquals("File2 content", Convert.FromBase64String("YYY="), file2.Data);
			});
		}

		public void TestGetLinkedObjectFromOriginalMessage()
		{
			ProcessMessage(message);
			AssertEquals(entryHeader, message.EM_LinkedObject);
		}

		public void TestGetLinkedObjectFromMRN()
		{
			var entryNum = Factory.NewWithValidTestData<CusEntryNumber>();
			entryNum.CE_EntryNum = "00DE000000000000E0";
			entryNum.CE_EntryType = CusEntryNumberTypes.Standard.MovementReferenceNumber;
			entryNum.Parent = entryHeader;

			messageMock.Setup(x => x.DataProvider.ReferencedMessageIdentifier).Returns("INVALID_CODE");

			ProcessMessage(message);

			AssertEquals(entryHeader, message.EM_LinkedObject);
		}

		public void TestDataProviderNullDueToInvalidMessage()
		{
			messageMock.Reset();
			messageMock.Setup(m => m.DataProvider).Returns((IEXPNOT)null);
			AssertNoExceptionThrown(() => ProcessMessage(message));
		}

		public void TestEntryStatus501()
		{
			entryHeader.CH_EntryStatus = "501";
			ProcessMessage(message);
			CombineAssertions(() =>
			{
				AssertEquals("EM_Status", EDIMessage.Status.ProcessedOK, message.EM_Status);
				AssertEquals("CH_EntryStatus", "541", entryHeader.CH_EntryStatus);
				AssertEquals(ExportExitStatus.Codes.ExitedSatisfactorily, entryHeader.CH_ExitedStatus);
				var reference = entryHeader.Declaration.JE_DeclarationReference;
				var lrn = entryHeader.LocalReferenceNumber;
				var subject = $"AES EXP Response for {reference} - LRN: {lrn}";
				var titleSubject = $"AES EXP Response for {reference}";
				var bodyMessageTitle = $"<title>{titleSubject}</title>";
				var bodyMessageHeader = $@"<strong>AES EXP Response for <a href=""edient:Command=ShowEditForm&LicenceCode={GlbCompany.CurrentCompany.LicenceKeyIdentifier}&ControllerID=JobDeclaration&BusinessEntityPK=" + entryHeader.Declaration.PK;
				var bodyMessageSummary = $@"Your Export Declaration Message for Job {reference} has a Notification for Export. For details please follow the Link to the Job.";
				var bodyMessageTable = @"<table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table"">"
						+ "<tr><td>MRN</td><td>00DE000000000000E0</td></tr>"
						+ "<tr><td>Exit Date - Time</td><td>01.01.2000 - 00:00</td></tr>"
						+ "</table>";
				var email = Env.OutgoingCustomsMailManager.EmailsCreated.Single(x => x.Subject == subject);
				AssertEmailForSingleRecipientWithTable("Message for EntryStatus = 541: ", email, "Dummy@dummy.com", subject, bodyMessageTitle, bodyMessageHeader, bodyMessageSummary, bodyMessageTable);
			});
		}

		public void TestAddCustomsEntryStatusLog_EntryStatus501()
		{
			entryHeader.CH_EntryStatus = "501";
			ProcessMessage(message);
			AssertEquals("541", entryHeader.GetCustomsEntryStatusEventReference());
		}

		public void TestEntryStatus502()
		{
			entryHeader.CH_EntryStatus = "502";
			ProcessMessage(message);
			CombineAssertions(() =>
			{
				AssertEquals("EM_Status", EDIMessage.Status.ProcessedOK, message.EM_Status);
				AssertEquals("CH_EntryStatus", "570", entryHeader.CH_EntryStatus);
				AssertEquals(ExportExitStatus.Codes.ExitedSatisfactorily, entryHeader.CH_ExitedStatus);
				var reference = entryHeader.Declaration.JE_DeclarationReference;
				var lrn = entryHeader.LocalReferenceNumber;
				var subject = $"AES EXP Response for {reference} - LRN: {lrn}";
				var titleSubject = $"AES EXP Response for {reference}";
				var bodyMessageTitle = $"<title>{titleSubject}</title>";
				var bodyMessageHeader = $@"<strong>AES EXP Response for <a href=""edient:Command=ShowEditForm&LicenceCode={GlbCompany.CurrentCompany.LicenceKeyIdentifier}&ControllerID=JobDeclaration&BusinessEntityPK=" + entryHeader.Declaration.PK;
				var bodyMessageSummary = $@"Your Export Declaration Message for Job {reference} has a Notification for Export. For details please follow the Link to the Job.";
				var bodyMessageTable = @"<table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table"">"
						+ "<tr><td>MRN</td><td>00DE000000000000E0</td></tr>"
						+ "<tr><td>Exit Date - Time</td><td>01.01.2000 - 00:00</td></tr>"
						+ "</table>";
				var email = Env.OutgoingCustomsMailManager.EmailsCreated.Single(x => x.Subject == subject);
				AssertEmailForSingleRecipientWithTable("Message for EntryStatus = 570: ", email, "Dummy@dummy.com", subject, bodyMessageTitle, bodyMessageHeader, bodyMessageSummary, bodyMessageTable);
			});
		}

		public void TestAddCustomsEntryStatusLog_EntryStatus502()
		{
			entryHeader.CH_EntryStatus = "502";
			ProcessMessage(message);
			AssertEquals("570", entryHeader.GetCustomsEntryStatusEventReference());
		}

		public void TestEntryStatusNot501Or502()
		{
			entryHeader.CH_EntryStatus = "10";
			entryHeader.CH_ExitedStatus = ExportExitStatus.Codes.UnknownOrNotReported;
			ProcessMessage(message);
			var stmNote = GetStmNote(message);
			CombineAssertions(() =>
			{
				AssertEquals("EM_Status", EDIMessage.Status.Discarded, message.EM_Status);
				AssertEquals("CH_EntryStatus", "10", entryHeader.CH_EntryStatus);
				AssertEquals("CH_ExitedStatus", ExportExitStatus.Codes.UnknownOrNotReported, entryHeader.CH_ExitedStatus);
				AssertEquals("ST_NoteText", "Message discarded: Entry Status '501' or '502' expected.", stmNote.ST_NoteText);
				AssertNull("No Email expected", Env.OutgoingCustomsMailManager.EmailsCreated.SingleOrDefault());
			});
		}

		public void TestAddCustomsEntryStatusLog_Not501Or502()
		{
			entryHeader.CH_EntryStatus = "10";
			ProcessMessage(message);
			AssertEquals(ZString.Empty, entryHeader.GetCustomsEntryStatusEventReference());
		}

		public void TestPopulateLogbookRegNum()
		{
			ProcessMessage(message);
			AssertEquals("00DE000000000000E0", message.GetLogbookRegistrationNumber());
		}

		public void TestPopulateLogbookLocalReferenceNumber()
		{
			ProcessMessage(message);
			AssertEquals("Reference1234567abcd", message.GetLogbookLocalReferenceNumber());
		}

		protected override ZString MessageFriendlyName => "Export EXPNOT Message Processor";

		protected override DEBranchCustomsApplicationTypeMessageProcessor<AesInboundEDIMessage<IEXPNOT>> Processor => new ExportEXPNOTMessageProcessor(logger);

		protected override void SetUp()
		{
			base.SetUp();

			var staff = Factory.LoadFromNaturalKey<GlbStaff>(GlbStaffSchema.GS_Code, GlbStaff.CurrentUser.GS_Code);
			staff.GS_EmailAddress = "Dummy@dummy.com";

			var groupPK = EnvProxy.Instance.Registry.PostMasterGroup;
			var group = Factory.Load<GlbGroup>(groupPK);
			group.Staff.Add(staff);

			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
			entryHeader = (CusEntryHeader)declaration.ActiveEntryHeaders.AddNew();
			entryHeader.CH_BGMReference = "ATB150000620520195875";

			outgoingMessage = CreateOriginalMessageLinkedToParent<AesEDIMessage>(entryHeader, "EXPNOT4387");
			outgoingMessage.EM_SystemCreateUser = staff.GS_Code;

			var dataProviderMock = new Mock<IEXPNOT>();
			dataProviderMock.Setup(m => m.MessageIdentifier).Returns("1289402357");
			dataProviderMock.Setup(m => m.ReferencedMessageIdentifier).Returns("EXPNOT4387");
			dataProviderMock.Setup(m => m.MovementReferenceNumber).Returns("00DE000000000000E0");
			dataProviderMock.Setup(m => m.LocalReferenceNumber).Returns("Reference1234567abcd");
			dataProviderMock.Setup(m => m.ExitDateTime).Returns(new DateTime(2000, 1, 1));

			messageMock = Factory.NewMoq<AesInboundEDIMessage<IEXPNOT>>();
			messageMock.Setup(m => m.DataProvider).Returns(dataProviderMock.Object);
			message = messageMock.Object;
			Factory.Save();
		}
		CusEntryHeader entryHeader;
		EDIMessage outgoingMessage;
		Mock<AesInboundEDIMessage<IEXPNOT>> messageMock;
		AesInboundEDIMessage<IEXPNOT> message;
	}
}
