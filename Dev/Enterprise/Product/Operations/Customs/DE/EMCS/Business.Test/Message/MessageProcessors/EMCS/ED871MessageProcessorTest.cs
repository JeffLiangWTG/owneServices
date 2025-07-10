using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.Customs.DE.Business;
using Enterprise.Customs.DE.Business.Testing;
using Enterprise.Customs.DE.EMCS.Messaging;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.DE.EMCS.Business.Testing
{
	[TestedType(typeof(ED871MessageProcessor))]
	class ED871MessageProcessorTest : MessageProcessorAbstractTest<ED871MessageProcessor, EmcsInboundEDIMessage<IED871>>
	{
		public void TestDataProviderNullDueToInvalidMessage()
		{
			messageMock.Setup(m => m.DataProvider).Returns((IED871)null);
			AssertNoExceptionThrown(() => ProcessMessage(message));
		}

		public void TestGetLinkedObject()
		{
			ProcessMessage(message);
			AssertEquals(declaration, message.EM_LinkedObject);
		}

		public void TestGetLinkedObject_BranchDoesNotMatch()
		{
			message.EM_GB = Factory.New<GlbCompany>().Branches.AddNew().PK;
			ProcessMessage(message);
			AssertNull(message.EM_LinkedObject);
		}

		public void TestProcessMessageCore_DeclarationForEADNotFound()
		{
			CombineAssertions(() =>
			{
				eventMock.Reset();
				eventMock.Setup(m => m.AdministrativeReferenceCode).Returns("MRN98761236");

				ProcessMessage(message);
				AssertEquals("Entry Status", ZString.Empty, declaration.JE_EntryStatus);
				AssertEquals("Message Status", EDIMessage.Status.Error, message.EM_Status);
			});
		}

		public void TestProcessMessageCore_MessageProcessed()
		{
			CombineAssertions(() =>
			{
				ProcessMessage(message);
				AssertEquals("Entry Status", EU.EMCS.Business.EntryStatusList.Codes.SHR, declaration.JE_EntryStatus);
				AssertEquals("Message Status on Declaration", EDIMessage.Status.Received, declaration.JE_MessageStatus);
				AssertEquals("Message Status", EDIMessage.Status.ProcessedOK, message.EM_Status);
				AssertArrivalInformation();
			});
		}

		public void AssertArrivalInformation()
		{
			AssertEquals("Has 2 lines", 2, message.DataProvider.Lines.Count);

			var line1 = declaration.InvoiceLines.Cast<EU.EMCS.Business.EMCSJobComInvoiceLine>().Single(x => x.JI_LineNo == 1);
			AssertEquals("1st line, ExciseProductCode", "B000", line1.ZG_ExciseProductCode);
			AssertEquals("1st line, Explanation", "Shortage explanation", line1.Outturn.C5_OutturnResultReason);

			var line2 = declaration.InvoiceLines.Cast<EU.EMCS.Business.EMCSJobComInvoiceLine>().Single(x => x.JI_LineNo == 2);
			AssertEquals("2nd line, ExciseProductCode", "S200", line2.ZG_ExciseProductCode);
			AssertEquals("2nd line, Explanation", "Excess explanation", line2.Outturn.C5_OutturnResultReason);
		}

		public void TestDocumentsAttached()
		{
			var attachedDocuments = new List<AttachedDocument>
			{
				new AttachedDocument
				{
					FileName = "file1.pdf",
					Type = new DocumentType { Code = "AAA", Description = "AAA Desc" },
					ImageData = (SubStreamableStream)new MemoryStream(Convert.FromBase64String("XXX="))
				}
			};

			messageMock.Setup(m => m.AttachedDocuments).Returns(attachedDocuments);

			CombineAssertions(() =>
			{
				var docManagerSupport = (IDocManagerSupport)declaration;
				AssertEquals("No eDocs to start with", 0, docManagerSupport.DocManagerInfo.AllEDocs.Count);

				ProcessMessage(message);

				AssertEquals("eDoc attached only once", 1, docManagerSupport.DocManagerInfo.AllEDocs.Count);
				attachedDocuments[0].ImageData.Dispose();
			});
		}

		public void TestGenerateEmail()
		{
			ProcessMessage(message);
			AssertEmail("<br /><br />Global Explanation: Shortage or Excess information");
		}

		public void TestGenerateEmail_GlobalExplanationEmpty()
		{
			dataProviderMock.Setup(m => m.GlobalExplanation).Returns(ZString.Empty);
			ProcessMessage(message);
			AssertEmail(ZString.Empty);
		}

		public void TestGenerateEmail_LinesEmpty()
		{
			dataProviderMock.Setup(m => m.Lines).Returns(Array.Empty<IED871BodyAnalysis>());
			ProcessMessage(message);
			AssertEmail(ZString.Empty, false);
		}

		public void TestPopulateLogbookRegistrationNumber()
		{
			ProcessMessage(message);
			AssertEquals("LogbookRegistrationNumber", "MRN98761234", message.GetLogbookRegistrationNumber());
		}

		protected override ZString MessageFriendlyName => "EMCS ED871 Message Processor";

		protected override DEBranchCustomsApplicationTypeMessageProcessor<EmcsInboundEDIMessage<IED871>> Processor => new ED871MessageProcessor(logger);

		protected override void SetUp()
		{
			base.SetUp();
			declaration = Factory.CreateDeclarationWithEadReference("MRN98761234", "1", EU.EMCS.Business.EMCSEntryTypeList.Codes.Consignor);

			eventMock = new Mock<IEMCSEvent>();
			eventMock.Setup(m => m.AdministrativeReferenceCode).Returns("MRN98761234");
			eventMock.Setup(m => m.SequenceNumber).Returns("1");

			var lineMock1 = new Mock<IED871BodyAnalysis>();
			lineMock1.Setup(m => m.LineNumber).Returns("1");
			lineMock1.Setup(m => m.ExciseProductCode).Returns("B000");
			lineMock1.Setup(m => m.ActualQuantity).Returns(12.123);
			lineMock1.Setup(m => m.Explanation).Returns("Shortage explanation");

			var lineMock2 = new Mock<IED871BodyAnalysis>();
			lineMock2.Setup(m => m.LineNumber).Returns("2");
			lineMock2.Setup(m => m.ExciseProductCode).Returns("S200");
			lineMock2.Setup(m => m.ActualQuantity).Returns(3.55);
			lineMock2.Setup(m => m.Explanation).Returns("Excess explanation");

			dataProviderMock = new Mock<IED871>();
			dataProviderMock.Setup(m => m.MessageIdentifier).Returns("DE90003480001005");
			dataProviderMock.Setup(m => m.MessageGroup).Returns(EmcsMessageSubTypeList.Codes.Eme);
			dataProviderMock.Setup(m => m.GlobalExplanation).Returns("Shortage or Excess information");
			dataProviderMock.Setup(m => m.ExciseMovement).Returns(eventMock.Object);
			dataProviderMock.Setup(m => m.Lines).Returns(new IED871BodyAnalysis[] { lineMock1.Object, lineMock2.Object });

			messageMock = Factory.NewMoq<EmcsInboundEDIMessage<IED871>>();
			messageMock.Setup(m => m.DataProvider).Returns(dataProviderMock.Object);

			message = messageMock.Object;

			user = Factory.NewWithValidTestData<GlbStaff>();
			user.GS_EmailAddress = "test@mail.com";

			lastOutgoingMessage = CreateOriginalMessageLinkedToParent<EmcsEDIMessage>(declaration, message.EM_MessageNum);
			lastOutgoingMessage.EM_SystemCreateUser = user.GS_Code;
			declaration.Messages.Add(lastOutgoingMessage);

			Factory.Save();

			var line1 = declaration.InvoiceLines.AddNew();
			line1.JI_LineNo = 1;
			var line2 = declaration.InvoiceLines.AddNew();
			line2.JI_LineNo = 2;
		}

		EMCSJobDeclaration declaration;
		Mock<IEMCSEvent> eventMock;
		Mock<IED871> dataProviderMock;
		Mock<EmcsInboundEDIMessage<IED871>> messageMock;
		EmcsInboundEDIMessage<IED871> message;
		GlbStaff user;
		EDIMessage lastOutgoingMessage;

		void AssertEmail(ZString globalExplanation, bool emailContainsTable = true)
		{
			var reference = declaration.JE_DeclarationReference;
			var subject = $"EMCS Shortage or Excess Explanation Received Response for {reference}";
			var bodyMessageTitle = $"<title>{subject}</title>";
			var bodyMessageHeader = @"<strong>EMCS Shortage or Excess Explanation Received Response for <a href=""edient:Command=ShowEditForm&LicenceCode=EDIEDIDAT&ControllerID=EMCS&BusinessEntityPK=" + declaration.PK;
			var bodyMessageSummary = $"Your EMCS Declaration for Job {reference} received a Shortage or Excess Explanation. For details please follow the Link to the Job."
				+ "<br /><br />ARC: MRN98761234"
				+ globalExplanation;
			var bodyMessageTable = @"<table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table"">"
			+ @"<thead><tr class=""tableheadings""><th>Position</th><th>Excise Product Code</th><th>Actual Quantity</th><th>Explanation</th></tr></thead>"
			+ "<tr><td>1</td><td>B000</td><td>12.123</td><td>Shortage explanation</td></tr>"
			+ "<tr><td>2</td><td>S200</td><td>3.55</td><td>Excess explanation</td></tr></table>";
			var email = Env.OutgoingCustomsMailManager.EmailsCreated.FirstOrDefault(x => x.Subject.Contains(reference));
			CombineAssertions(() =>
			{
				if (emailContainsTable)
				{
					AssertEmailForSingleRecipientWithTable(ZString.Empty, email, "test@mail.com", subject, bodyMessageTitle, bodyMessageHeader, bodyMessageSummary, bodyMessageTable);
				}
				else
				{
					AssertEmailForSingleRecipient(ZString.Empty, email, "test@mail.com", subject, bodyMessageTitle, bodyMessageHeader, bodyMessageSummary);
					AssertNotContains("No table", bodyMessageTable, email.Body);
				}
			});
		}
	}
}
