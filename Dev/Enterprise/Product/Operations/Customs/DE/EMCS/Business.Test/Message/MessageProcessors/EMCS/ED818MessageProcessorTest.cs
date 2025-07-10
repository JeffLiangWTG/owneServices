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
	[TestedType(typeof(ED818MessageProcessor))]
	class ED818MessageProcessorTest : MessageProcessorAbstractTest<ED818MessageProcessor, EmcsInboundEDIMessage<IED818>>
	{
		public void TestDataProviderNullDueToInvalidMessage()
		{
			messageMock.Setup(m => m.DataProvider).Returns((IED818)null);
			AssertNoExceptionThrown(() => ProcessMessage(message));
		}

		public void TestGetLinkedObject()
		{
			ProcessMessage(message);
			AssertEquals(declaration, message.EM_LinkedObject);
		}

		public void TestProcessMessageCore_DeclarationForEADNotFound()
		{
			CombineAssertions(() =>
			{
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
				AssertEquals("Entry Status", EU.EMCS.Business.EntryStatusList.Codes.COM, declaration.JE_EntryStatus);
				AssertEquals("Message Status on Declaration", ZString.Empty, declaration.JE_MessageStatus);
				AssertEquals("Message Status", EDIMessage.Status.ProcessedOK, message.EM_Status);
			});
		}

		public void TestProcessMessageCore_MessageProcessed_Consignee()
		{
			CombineAssertions(() =>
			{
				declaration.JE_DeclarantType = EU.EMCS.Business.EMCSEntryTypeList.Codes.Consignee;
				dataProviderMock.Setup(m => m.MessageGroup).Returns(EmcsMessageSubTypeList.Codes.Emb);
				Factory.Save();

				ProcessMessage(message);
				AssertEquals("Entry Status", EU.EMCS.Business.EntryStatusList.Codes.COM, declaration.JE_EntryStatus);
				AssertEquals("Message Status on Declaration", EDIMessage.Status.Received, declaration.JE_MessageStatus);
				AssertEquals("Message Status", EDIMessage.Status.ProcessedOK, message.EM_Status);
			});
		}

		public void TestProcessMessageCore_MessageProcessed_Consignor()
		{
			var line1 = declaration.InvoiceLines.AddNew();
			line1.JI_LineNo = 1;
			line1.JI_CustomsQuantity = 150;
			line1.ZG_DeclaredValue = 150;

			var line2 = declaration.InvoiceLines.AddNew();
			line2.JI_LineNo = 2;
			line2.JI_CustomsQuantity = 200;
			line2.ZG_DeclaredValue = 200;

			CombineAssertions(() =>
			{
				ProcessMessage(message);

				AssertEquals("Entry Status", EU.EMCS.Business.EntryStatusList.Codes.COM, declaration.JE_EntryStatus);
				AssertEquals("Message Status", EDIMessage.Status.ProcessedOK, message.EM_Status);
				AssertReportOfReceipt();
			});
		}

		void AssertReportOfReceipt()
		{
			AssertEquals("Has 2 lines", 2, message.DataProvider.ReportOfReceipts.Count);

			var reportOfReceipt1 = declaration.InvoiceLines.Cast<EU.EMCS.Business.EMCSJobComInvoiceLine>().Single(x => x.JI_LineNo == 1);
			AssertEquals("1st line, Customs Quantity", 140m, reportOfReceipt1.JI_CustomsQuantity);
			AssertEquals("1st line, Refused Quantity", 20m, reportOfReceipt1.Outturn.C5_RejectedQuantity);
			AssertEquals("2nd line, Observed Quantity", -10m, reportOfReceipt1.Outturn.ObservedDifference);

			var reasons1 = reportOfReceipt1.Outturn.ReportOfReceiptReasons;
			AssertEquals("1st line, 1st reason, Reason Code", "3", reasons1[0].CY_Code);
			AssertEquals("1st line, 1st reason, Reason Description", "Goods were damaged during transport", reasons1[0].CY_Data);
			AssertEquals("1st line, 2nd reason, Reason Code", "2", reasons1[1].CY_Code);
			AssertEquals("1st line, 2nd reason, Reason Description", "Quantity is less than what was reported", reasons1[1].CY_Data);

			var reportOfReceipt2 = declaration.InvoiceLines.Cast<EU.EMCS.Business.EMCSJobComInvoiceLine>().Single(x => x.JI_LineNo == 2);
			AssertEquals("2nd line, Customs Quantity", 230m, reportOfReceipt2.JI_CustomsQuantity);
			AssertEquals("2nd line, Refused Quantity", 20m, reportOfReceipt2.Outturn.C5_RejectedQuantity);
			AssertEquals("2nd line, Observed Quantity", 30m, reportOfReceipt2.Outturn.ObservedDifference);

			var reasons2 = reportOfReceipt2.Outturn.ReportOfReceiptReasons;
			AssertEquals("2nd line, 1st reason, Reason Code", "3", reasons2[0].CY_Code);
			AssertEquals("2nd line, 1st reason, Reason Description", "Goods were damaged during transport", reasons2[0].CY_Data);
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

		public void TestGenerateEmail_NoLines()
		{
			var lastOutgoingMessage = CreateOriginalMessageLinkedToParent<EmcsEDIMessage>(declaration, message.EM_MessageNum);
			lastOutgoingMessage.EM_SystemCreateUser = user.GS_Code;
			declaration.Messages.Add(lastOutgoingMessage);

			ProcessMessage(message);
			AssertEmail("No Lines");
		}

		public void TestGenerateEmail_MultipleLines()
		{
			var lastOutgoingMessage = CreateOriginalMessageLinkedToParent<EmcsEDIMessage>(declaration, message.EM_MessageNum);
			lastOutgoingMessage.EM_SystemCreateUser = user.GS_Code;
			declaration.Messages.Add(lastOutgoingMessage);

			ProcessMessage(message);

			var additionalBodyMessageTable = @"<br />"
											+ @"<table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table"">"
											+ @"<thead><tr class=""tableheadings""><th>Line No.</th><th>Unsatisfactory Reason</th><th>Information</th></tr></thead>"
											+ "<tr><td>1</td><td>3 - Goods Damaged</td><td>Goods were damaged during transport</td></tr>"
											+ "<tr><td>1</td><td>2 - Shortage</td><td>Quantity is less than what was reported</td></tr>"
											+ "<tr><td>2</td><td>3 - Goods Damaged</td><td>Goods were damaged during transport</td></tr>"
											+ "</table>";
			AssertEmail("Multiple Lines", additionalBodyMessageTable);
		}

		public void TestPopulateLogbookRegistrationNumber()
		{
			ProcessMessage(message);
			AssertEquals("LogbookRegistrationNumber", "MRN98761234", message.GetLogbookRegistrationNumber());
		}

		protected override ZString MessageFriendlyName => "EMCS ED818 Message Processor";

		protected override DEBranchCustomsApplicationTypeMessageProcessor<EmcsInboundEDIMessage<IED818>> Processor => new ED818MessageProcessor(logger);

		protected override void SetUp()
		{
			base.SetUp();
			declaration = Factory.CreateDeclarationWithEadReference("MRN98761234", "1", EU.EMCS.Business.EMCSEntryTypeList.Codes.Consignor);

			eventMock = new Mock<IEMCSEvent>();
			eventMock.Setup(e => e.AdministrativeReferenceCode).Returns("MRN98761234");
			eventMock.Setup(m => m.SequenceNumber).Returns("1");

			var reason1 = new Mock<IED818UnsatisfactoryReason>();
			reason1.Setup(x => x.ReasonCode).Returns("3");
			reason1.Setup(m => m.ComplementaryInformation).Returns("Goods were damaged during transport");

			var reason2 = new Mock<IED818UnsatisfactoryReason>();
			reason2.Setup(x => x.ReasonCode).Returns("2");
			reason2.Setup(m => m.ComplementaryInformation).Returns("Quantity is less than what was reported");

			var reportOfReceipt1 = new Mock<IED818ReportOfReceipt>();
			reportOfReceipt1.Setup(x => x.LineNumber).Returns("1");
			reportOfReceipt1.Setup(x => x.IndicatorOfShortageOrExcess).Returns("S");
			reportOfReceipt1.Setup(x => x.ObservedQuantity).Returns(10);
			reportOfReceipt1.Setup(x => x.RefusedQuantity).Returns(20);
			reportOfReceipt1.Setup(m => m.UnsatisfactoryReasons).Returns(new[] { reason1.Object, reason2.Object });

			var reportOfReceipt2 = new Mock<IED818ReportOfReceipt>();
			reportOfReceipt2.Setup(x => x.LineNumber).Returns("2");
			reportOfReceipt2.Setup(x => x.IndicatorOfShortageOrExcess).Returns("E");
			reportOfReceipt2.Setup(x => x.ObservedQuantity).Returns(30);
			reportOfReceipt2.Setup(x => x.RefusedQuantity).Returns(20);
			reportOfReceipt2.Setup(m => m.UnsatisfactoryReasons).Returns(new[] { reason1.Object });

			dataProviderMock = new Mock<IED818>();
			dataProviderMock.Setup(p => p.MessageIdentifier).Returns("DE90003480001005");
			dataProviderMock.Setup(p => p.MessageGroup).Returns(EmcsMessageSubTypeList.Codes.Eme);
			dataProviderMock.Setup(p => p.ExciseMovement).Returns(eventMock.Object);
			dataProviderMock.Setup(p => p.GlobalConclusionOfReceipt).Returns("4");
			dataProviderMock.Setup(m => m.ReportOfReceipts).Returns(new[] { reportOfReceipt1.Object, reportOfReceipt2.Object });

			messageMock = Factory.NewMoq<EmcsInboundEDIMessage<IED818>>();
			messageMock.Setup(m => m.DataProvider).Returns(dataProviderMock.Object);

			message = messageMock.Object;

			user = Factory.NewWithValidTestData<GlbStaff>();
			user.GS_EmailAddress = "test@mail.com";

			Factory.Save();
		}

		EMCSJobDeclaration declaration;
		Mock<IEMCSEvent> eventMock;
		Mock<IED818> dataProviderMock;
		Mock<EmcsInboundEDIMessage<IED818>> messageMock;
		EmcsInboundEDIMessage<IED818> message;
		GlbStaff user;

		void AssertEmail(string testCase, string additionalBodyMessageTable = "")
		{
			var reference = declaration.JE_DeclarationReference;
			var email = Env.OutgoingCustomsMailManager.EmailsCreated.FirstOrDefault(x => x.Subject.Contains(reference));

			var subject = $"EMCS Report of Receipt Response for {reference}";
			var bodyMessageTitle = $"<title>{subject}</title>";
			var bodyMessageHeader = @"<strong>EMCS Report of Receipt Response for <a href=""edient:Command=ShowEditForm&LicenceCode=EDIEDIDAT&ControllerID=EMCS&BusinessEntityPK=" + declaration.PK;
			var bodyMessageSummary = $"Your EMCS Declaration for Job {reference} received a report of receipt. For details please follow the link to the Job.";
			var bodyMessageTable = @"<table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table"">"
									+ "<tr><td>ARC</td><td>MRN98761234</td></tr>"
									+ "<tr><td>Global Conclusion of Receipt</td><td>4 - Waren beim Empfang teilweise zur&#252;ckgewiesen</td></tr>"
									+ "</table>"
									+ additionalBodyMessageTable;
			AssertEmailForSingleRecipientWithTable(testCase, email, "test@mail.com", subject, bodyMessageTitle, bodyMessageHeader, bodyMessageSummary, bodyMessageTable);
		}
	}
}
