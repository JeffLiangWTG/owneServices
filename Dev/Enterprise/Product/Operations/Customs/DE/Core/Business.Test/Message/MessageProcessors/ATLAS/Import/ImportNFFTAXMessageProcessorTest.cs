using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;
using Enterprise.Customs.DE.Business.Testing;
using Enterprise.Customs.DE.Messaging;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.DE.Business.Declaration.Testing
{
	[TestedType(typeof(ImportNFFTAXMessageProcessor))]
	sealed class ImportNFFTAXMessageProcessorTest : ImportMessageProcessorAffectingDeclarationAbstractTest<ImportNFFTAXMessageProcessor, AtlasInboundEDIMessage<INFFTAX>>
	{
		public void TestGetLinkedObject_ReferenceNumber()
		{
			ProcessMessage(Message);
			AssertSame(entryHeader, Message.EM_LinkedObject);
		}

		public void TestGetLinkedObject_MRN()
		{
			dataProviderMock.Setup(m => m.ReferenceNumber).Returns(ZString.Empty);
			dataProviderMock.Setup(m => m.MRN).Returns(ReferenceNumber);
			ProcessMessage(Message);
			AssertSame(entryHeader, Message.EM_LinkedObject);
		}

		public void TestNoLinkedObject()
		{
			dataProviderMock.Setup(m => m.ReferenceNumber).Returns("ATB0000000000000");
			ProcessMessage(Message);
			CombineAssertions(() =>
			{
				AssertNull("No LinkedObject", Message.EM_LinkedObject);
				AssertEquals("Message status == error", EDIMessage.Status.Error, Message.EM_Status);
			});
		}

		public void TestDataProviderNullDueToInvalidMessage()
		{
			messageMock.Setup(m => m.DataProvider).Returns((INFFTAX)null);
			AssertNoExceptionThrown(() => ProcessMessage(Message));
		}

		public void TestProcessMessage()
		{
			ProcessMessage(Message);
			CombineAssertions(() =>
			{
				AssertEquals("EM_Status", AesEDIMessage.Status.ProcessedOK, Message.EM_Status);
				AssertEquals("CH_EntryStatus", UniversalReferenceConstants.EntryStatus.TXR, entryHeader.CH_EntryStatus);
			});
		}

		public void TestEventsCreated()
		{
			ProcessMessage(Message);
			var entryStatus = entryHeader.CH_EntryStatus;
			AssertEquals(entryStatus, entryHeader.Logs.MostRecentLogByEventTime(Events.CustomsEntryStatus)?.SL_Reference);
		}

		public void TestGenerateEmail()
		{
			dataProviderMock.Setup(m => m.MRN).Returns("23DE586601055987B7");

			var goodsItem1 = new Mock<INFFTAXGoodsItem>();
			goodsItem1.Setup(m => m.SequenceNumber).Returns("1");

			var goodsItem2 = new Mock<INFFTAXGoodsItem>();
			goodsItem2.Setup(m => m.SequenceNumber).Returns("2");

			dataProviderMock.Setup(m => m.GoodsItems).Returns(new[] { goodsItem1.Object, goodsItem2.Object });

			ProcessMessage(Message);

			var declaration = entryHeader.Declaration;
			var reference = declaration.JE_DeclarationReference;
			var email = Env.OutgoingCustomsMailManager.EmailsCreated.FirstOrDefault(x => x.Subject.Contains(reference));

			var subject = $"Import NFFTAX – Reasons for not finally fixed Import Taxes Response for {reference}";
			var bodyMessageTitle = $"<title>{subject}</title>";
			var bodyMessageHeader = $@"<strong>Import NFFTAX – Reasons for not finally fixed Import Taxes Response for <a href=""edient:Command=ShowEditForm&LicenceCode={GlbCompany.CurrentCompany.LicenceKeyIdentifier}&ControllerID=JobDeclaration&BusinessEntityPK=" + declaration.PK;
			var bodyMessageSummary = $@"Your Import Declaration for Job {reference} received Reasons for not finally fixed Import Taxes. For details please follow the link to the job.";
			var bodyMessageTable = @"<table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table"">"
								   + $"<tr><td>Registration Number</td><td>{ReferenceNumber}</td></tr>"
								   + "<tr><td>MRN</td><td>23DE586601055987B7</td></tr>"
								   + "<tr><td>Local Reference Number</td><td>KTG/KTG1/80/6835</td></tr>"
								   + "</table>";
			var goodsItemsTable = @"<table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table"">"
								   + "<tr><td>Affected Lines</td></tr>"
								   + "<tr><td>1</td></tr>"
								   + "<tr><td>2</td></tr>"
								   + "</table>";
			CombineAssertions(() =>
			{
				AssertEmailForSingleRecipient("Single", email, "test@mail.com", subject, bodyMessageTitle, bodyMessageHeader, bodyMessageSummary);
				AssertContains("Body-Message-Table", bodyMessageTable, email.Body);
				AssertContains("GoodsItems-Table", goodsItemsTable, email.Body);
			});
		}

		public void TestGenerateEmail_EmptyFieldsNotShown()
		{
			dataProviderMock.Setup(m => m.LocalReferenceNumber).Returns(ZString.Empty);
			CombineAssertions(() =>
			{
				ProcessMessage(Message);
				var email = Env.OutgoingCustomsMailManager.EmailsCreated.Single();
				AssertNotContains("No Local Reference Number", "<td>Local Reference Number</td>", email.Body);
				AssertNotContains("No MRN", "<td>MRN</td>", email.Body);
				AssertNotContains("GoodsItems Table is not shown when it has no rows", @"<td>Affected Lines</td>>", email.Body);

				dataProviderMock.Setup(x => x.ReferenceNumber).Returns((string)null);
				dataProviderMock.Setup(x => x.MRN).Returns(ReferenceNumber);
				ProcessMessage(Message);
				email = Env.OutgoingCustomsMailManager.EmailsCreated.Last();
				AssertNotContains("No Registration Number", "<td>Registration Number</td>", email.Body);
			});
		}

		public void TestPopulateLogbookRegNum()
		{
			dataProviderMock.Setup(m => m.MRN).Returns("23DE586601055987B7");
			ProcessMessage(Message);
			AssertContainsExactElementsInAnyOrder(new[] { ReferenceNumber, "23DE586601055987B7" }, Message.GetLogbookRegistrationNumbers());
		}

		public void TestEmailAttachments()
		{
			var reference = entryHeader.Declaration.JE_DeclarationReference;
			messageMock.Setup(m => m.AttachedDocuments).Returns(new List<AttachedDocument>
			{
				new AttachedDocument
				{
					FileName = "file1.pdf",
					Type = new DocumentType { Code = "AAA", Description = "AAA Desc" },
					ImageData = (SubStreamableStream)new MemoryStream(Convert.FromBase64String("XXX="))
				},
				new AttachedDocument
				{
					FileName = "file2.pdf",
					Type = new DocumentType { Code = "BBB", Description = "BBB Desc" },
					ImageData = (SubStreamableStream)new MemoryStream(Convert.FromBase64String("YYY="))
				}
			});
			ProcessMessage(Message);
			var email = Env.OutgoingCustomsMailManager.EmailsCreated.Single(x => x.Subject.Contains(reference));
			var emailAttachments = email.Attachments.Cast<AttachmentDef>();
			var file1 = emailAttachments.Single(x => x.DisplayName == "file1.pdf");
			var file2 = emailAttachments.Single(x => x.DisplayName == "file2.pdf");
			CombineAssertions(() =>
			{
				AssertEquals("File1 content", Convert.FromBase64String("XXX="), file1.Data);
				AssertEquals("File2 content", Convert.FromBase64String("YYY="), file2.Data);
			});
		}

		protected override ZString MessageFriendlyName => "Import NFFTAX Message Processor";

		protected override DEBranchCustomsApplicationTypeMessageProcessor<AtlasInboundEDIMessage<INFFTAX>> Processor => new ImportNFFTAXMessageProcessor(logger);

		protected override AtlasInboundEDIMessage<INFFTAX> Message => messageMock.Object;

		protected override void SetUp()
		{
			base.SetUp();

			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
			entryHeader = (CusEntryHeader)declaration.ActiveEntryHeaders.AddNew();
			var mrnEntryNumber = CusEntryNumber.LoadOrCreate(entryHeader, Core.Constants.CountryCodes.Germany);
			mrnEntryNumber.CE_EntryNum = ReferenceNumber;
			mrnEntryNumber.CE_EntryType = CusEntryNumberTypes.Standard.MovementReferenceNumber;

			var user = Factory.NewWithValidTestData<GlbStaff>();
			user.GS_EmailAddress = "test@mail.com";

			outgoingMessage = CreateOriginalMessageLinkedToParent<AtlasEDIMessage>(entryHeader, "outgoing");
			outgoingMessage.EM_ApplicationCode = EDIMessage.ApplicationCodes.DECustomsAtlasSystem;
			outgoingMessage.EM_MessageType = EDIMessageTypeList.Codes.Import;
			outgoingMessage.EM_MessageSubType = ImportMessageSubTypeList.Codes.FreeCirculationSingleDeclaration;
			outgoingMessage.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			outgoingMessage.EM_SystemCreateUser = user.GS_Code;

			dataProviderMock = new Mock<INFFTAX>();
			dataProviderMock.Setup(x => x.MessageIdentifier).Returns("0000000009");
			dataProviderMock.Setup(x => x.ReferenceNumber).Returns(ReferenceNumber);
			dataProviderMock.Setup(x => x.LocalReferenceNumber).Returns("KTG/KTG1/80/6835");
			dataProviderMock.Setup(m => m.GoodsItems).Returns(Array.Empty<INFFTAXGoodsItem>());

			messageMock = Factory.NewMoq<AtlasInboundEDIMessage<INFFTAX>>();
			messageMock.Setup(m => m.DataProvider).Returns(dataProviderMock.Object);
			Factory.Save();
		}
		CusEntryHeader entryHeader;
		Mock<INFFTAX> dataProviderMock;
		Mock<AtlasInboundEDIMessage<INFFTAX>> messageMock;
		EDIMessage outgoingMessage;
		const string ReferenceNumber = "ATB150000620520195875";
	}
}
