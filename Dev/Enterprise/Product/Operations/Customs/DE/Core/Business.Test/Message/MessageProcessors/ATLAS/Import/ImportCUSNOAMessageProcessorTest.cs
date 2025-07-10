using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.DE.Business.Testing;
using Enterprise.Customs.DE.Messaging;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.DE.Business.Declaration.Testing
{
	[TestedType(typeof(ImportCUSNOAMessageProcessor))]
	class ImportCUSNOAMessageProcessorTest : ImportMessageProcessorAffectingDeclarationAbstractTest<ImportCUSNOAMessageProcessor, AtlasInboundEDIMessage<ICUSNOA>>
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
			messageMock.Setup(m => m.DataProvider).Returns((ICUSNOA)null);
			AssertNoExceptionThrown(() => ProcessMessage(Message));
		}

		public void TestDocumentsAttached()
		{
			ProcessMessage(Message);
			AssertDocumentLinkingSubscribers(new BusinessObject[] { entryHeader });
		}

		public void TestGenerateEmail()
		{
			var goodsItemMock1 = new Mock<ICUSNOAGoodsItem>();
			goodsItemMock1.Setup(x => x.SequenceNumber).Returns("2");
			goodsItemMock1.Setup(x => x.DocumentType).Returns("9BBL");
			goodsItemMock1.Setup(x => x.DocumentReference).Returns("AD357-001");
			goodsItemMock1.Setup(m => m.CancellationWriteOffFlag).Returns("N");

			var goodsItemMock2 = new Mock<ICUSNOAGoodsItem>();
			goodsItemMock2.Setup(x => x.SequenceNumber).Returns("17");
			goodsItemMock2.Setup(x => x.DocumentType).Returns("Y100");
			goodsItemMock2.Setup(x => x.DocumentReference).Returns("002/34689 - 002");
			goodsItemMock2.Setup(m => m.CancellationWriteOffFlag).Returns("Y");

			dataProviderMock.Setup(m => m.GoodsItems).Returns(new[] { goodsItemMock1.Object, goodsItemMock2.Object });

			ProcessMessage(Message);

			var declaration = entryHeader.Declaration;
			var reference = declaration.JE_DeclarationReference;
			var email = Env.OutgoingCustomsMailManager.EmailsCreated.FirstOrDefault(x => x.Subject.Contains(reference));

			var subject = $"Import CUSNOA – Customs Notification of Attribution Response for {reference}";
			var bodyMessageTitle = $"<title>{subject}</title>";
			var bodyMessageHeader = $@"<strong>Import CUSNOA – Customs Notification of Attribution Response for <a href=""edient:Command=ShowEditForm&LicenceCode={GlbCompany.CurrentCompany.LicenceKeyIdentifier}&ControllerID=JobDeclaration&BusinessEntityPK=" + declaration.PK;
			var bodyMessageSummary = $@"Your Import Declaration for Job {reference} received a Customs Notification of Attribution. For details please follow the link to the job.";
			var bodyMessageTable = @"<table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table"">"
								   + "<tr><td>Registration Number</td><td>ATB150000620520195875</td></tr>"
								   + "<tr><td>Local Reference Number</td><td>MAS/22/11/22027</td></tr>"
								   + "</table>";
			var goodsItemsTable = @"<table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table"">"
								   + "<tr><td>Line Number</td><td>Document Type</td><td>Document Reference</td><td>Cancellation?</td></tr>"
								   + "<tr><td>2</td><td>9BBL</td><td>AD357-001</td><td>N</td></tr>"
								   + "<tr><td>17</td><td>Y100</td><td>002/34689 - 002</td><td>Y</td></tr>"
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

			ProcessMessage(Message);

			var email = Env.OutgoingCustomsMailManager.EmailsCreated.Single();
			CombineAssertions(() =>
			{
				AssertNotContains("No Local Reference Number", "<td>Local Reference Number</td>", email.Body);
				AssertNotContains("GoodsItems Table is not shown when it has no rows", @"<tr><td>Line Number</td><td>Document Type</td><td>Document Reference</td><td>Cancellation?</td></tr>", email.Body);
			});
		}

		public void TestPopulateLogbookRegNum()
		{
			ProcessMessage(Message);
			AssertEquals("ATB150000620520195875", Message.GetLogbookRegistrationNumber());
		}

		protected override ZString MessageFriendlyName => "Import CUSNOA Message Processor";

		protected override DEBranchCustomsApplicationTypeMessageProcessor<AtlasInboundEDIMessage<ICUSNOA>> Processor => new ImportCUSNOAMessageProcessor(logger);

		protected override AtlasInboundEDIMessage<ICUSNOA> Message => messageMock.Object;

		protected override void SetUp()
		{
			base.SetUp();

			entryHeader = (CusEntryHeader)declaration.ActiveEntryHeaders.AddNew();
			var mrnEntryNumber = CusEntryNumber.LoadOrCreate(entryHeader, Core.Constants.CountryCodes.Germany);
			mrnEntryNumber.CE_EntryNum = "ATB150000620520195875";
			mrnEntryNumber.CE_EntryType = CusEntryNumberTypes.Standard.MovementReferenceNumber;

			var user = Factory.NewWithValidTestData<GlbStaff>();
			user.GS_EmailAddress = "test@mail.com";

			outgoingMessage = CreateOriginalMessageLinkedToParent<AtlasEDIMessage>(entryHeader, "outgoing");
			outgoingMessage.EM_ApplicationCode = EDIMessage.ApplicationCodes.DECustomsAtlasSystem;
			outgoingMessage.EM_MessageType = EDIMessageTypeList.Codes.Import;
			outgoingMessage.EM_MessageSubType = ImportMessageSubTypeList.Codes.FreeCirculationSingleDeclaration;
			outgoingMessage.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			outgoingMessage.EM_SystemCreateUser = user.GS_Code;

			dataProviderMock = new Mock<ICUSNOA>();
			dataProviderMock.Setup(x => x.MessageIdentifier).Returns("0000000009");
			dataProviderMock.Setup(x => x.ReferenceNumber).Returns("ATB150000620520195875");
			dataProviderMock.Setup(m => m.LocalReferenceNumber).Returns("MAS/22/11/22027");

			messageMock = Factory.NewMoq<AtlasInboundEDIMessage<ICUSNOA>>();
			messageMock.Setup(m => m.DataProvider).Returns(dataProviderMock.Object);
			Factory.Save();
		}
		CusEntryHeader entryHeader;
		Mock<ICUSNOA> dataProviderMock;
		Mock<AtlasInboundEDIMessage<ICUSNOA>> messageMock;
		EDIMessage outgoingMessage;
	}
}
