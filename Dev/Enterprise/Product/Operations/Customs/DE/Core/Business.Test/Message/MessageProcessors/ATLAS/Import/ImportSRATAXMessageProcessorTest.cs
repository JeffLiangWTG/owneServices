using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Common;
using Enterprise.Customs.DE.Business.Testing;
using Enterprise.Customs.DE.Messaging;
using Enterprise.Customs.DE.Registry;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.DE.Business.Import.Testing
{
	[TestedType(typeof(ImportSRATAXMessageProcessor))]
	sealed class ImportSRATAXMessageProcessorTest : MessageProcessorAbstractTest<ImportSRATAXMessageProcessor, AtlasInboundEDIMessage<ISRATAX>>
	{
		public void TestGetAttachedDocuments()
		{
			messageMock.Setup(m => m.AttachedDocuments).Returns(new List<AttachedDocument>
			{
				new AttachedDocument
				{
					FileName = "file1.pdf",
					Type = new DocumentType { Code = "AAA", Description = "AAA Desc" },
					ImageData = (SubStreamableStream)new MemoryStream(Convert.FromBase64String("XXX="))
				}
			});
			var processor = new ImportSRATAXMessageProcessorForTest(logger);
			AssertEquals(1, processor.GetAttachedDocumentsExposed(message).Count(x => x.FileName.Equals("file1.pdf")));
			message.AttachedDocuments[0].ImageData.Dispose();
		}

		public void TestDataProviderNullDueToInvalidMessage()
		{
			messageMock.Setup(m => m.DataProvider).Returns((ISRATAX)null);
			AssertNoExceptionThrown(() => ProcessMessage(message));
		}

		public void TestProcessMessage()
		{
			var originalMessageEM_GB = message.EM_GB;
			ProcessMessage(message);
			CombineAssertions(() =>
			{
				AssertEquals("EM_MessageNum", "SRATAX52395580308888770311173073312", message.EM_MessageNum);
				AssertEquals("EM_Status", Enterprise.Messaging.Integration.EDIMessageStatusList.Codes.ProcessedOK, message.EM_Status);
				AssertNull("EM_LinkedObject", message.EM_LinkedObject);
				AssertEquals("EM_GB not updated", originalMessageEM_GB, message.EM_GB);
			});
		}

		public void TestDocumentsAttachedToMessage()
		{
			ProcessMessage(message);
			AssertDocumentLinkingSubscribers(new BusinessObject[] { message });
		}

		public void TestCusEntryNumCreatedAndLinkedToMessage_ReferenceNumber()
		{
			ProcessMessage(message);
			var cusEntryNumber = CusEntryNumber.Load(message, CusEntryNumberTypes.Standard.MovementReferenceNumber, Core.Constants.CountryCodes.Germany);
			CombineAssertions(() =>
			{
				AssertEquals("CE_EntryNum", "ATS000009360920213450", cusEntryNumber.CE_EntryNum);
				AssertEquals("CE_EntryStatus", TaxChangeAssessmentStatusCodeList.Codes.OPN, cusEntryNumber.CE_EntryStatus);
				AssertEquals("CE_IssueDate", new ZDateTime(2021, 09, 21), cusEntryNumber.CE_IssueDate);
				AssertEquals("CE_ExpiryDate", new ZDateTime(2021, 09, 30), cusEntryNumber.CE_ExpiryDate);
			});
		}

		public void TestCusEntryNumCreatedAndLinkedToMessage_MRN()
		{
			dataProviderMock.Setup(x => x.ReferenceNumber).Returns((string)null);
			dataProviderMock.Setup(x => x.MRN).Returns("23DE586601055987B7");
			ProcessMessage(message);
			var cusEntryNumber = CusEntryNumber.Load(message, CusEntryNumberTypes.Standard.MovementReferenceNumber, Core.Constants.CountryCodes.Germany);
			CombineAssertions(() =>
			{
				AssertEquals("CE_EntryNum", "23DE586601055987B7", cusEntryNumber.CE_EntryNum);
				AssertEquals("CE_EntryStatus", TaxChangeAssessmentStatusCodeList.Codes.OPN, cusEntryNumber.CE_EntryStatus);
				AssertEquals("CE_IssueDate", new ZDateTime(2021, 09, 21), cusEntryNumber.CE_IssueDate);
				AssertEquals("CE_ExpiryDate", new ZDateTime(2021, 09, 30), cusEntryNumber.CE_ExpiryDate);
			});
		}

		public void TestStmNoteCreatedAndLinkedToMessage()
		{
			ProcessMessage(message);
			var note = GetStmNote(message.PK, message.TableName, TaxChangeAssessment.Schema.TaxChangeAssessmentType);
			CombineAssertions(() =>
			{
				AssertEquals("ST_NoteType", "INT", note.ST_NoteType);
				AssertEquals("ST_Description", "TaxChangeAssessmentType", note.ST_Description);
				AssertEquals("ST_NoteText", ImportTaxChangeAssessmentTypeList.Codes.A, note.ST_NoteText);
			});
		}

		public void TestGenerateEmail()
		{
			dataProviderMock.Setup(x => x.MRN).Returns("23DE586601055987B7");
			var sendToRegistryCollection = ATLASMessageHelper.CreateSendAcknowledgementsRegistryCollection(Factory);
			using (DECustomsDataRegistry.Instance.SendTaxChangeAcknowledgements.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, sendToRegistryCollection))
			{
				ProcessMessage(message);

				var reference = message.DataProvider.ReferenceNumber;
				var email = Env.OutgoingCustomsMailManager.EmailsCreated.FirstOrDefault(x => x.Subject.Contains(reference));

				var subject = $"Import SRATAX – Tax Change Assessment Response for {reference}";
				var bodyMessageTitle = $"<title>{subject}</title>";
				var bodyMessageHeader = @"<strong>Import SRATAX – Tax Change Assessment Response for <a href=""edient:Command=ShowEditForm&LicenceCode=EDIEDIDAT&ControllerID=DETaxChangeAssessment&BusinessEntityPK=" + message.PK;
				var bodyMessageSummary = "For details please follow the Link to the Job.";
				var bodyMessageTable = @"<table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table"">"
											+ "<tr><td>Tax Change Assessment Type</td><td>Post-clearance</td></tr>"
											+ "<tr><td>MRN</td><td>23DE586601055987B7</td></tr>"
											+ "<tr><td>Reference Number</td><td>ATS000009360920213450</td></tr>"
											+ "<tr><td>Local Reference Number</td><td>ASV-Nr. 754-06-2021-3450</td></tr>"
											+ "<tr><td>Creation Date</td><td>21.09.2021</td></tr>"
											+ "<tr><td>Maturity Date</td><td>30.09.2021</td></tr></table>";
				AssertEmailForSingleRecipientWithTable(string.Empty, email, "test@mail.com", subject, bodyMessageTitle, bodyMessageHeader, bodyMessageSummary, bodyMessageTable);
			}
		}

		public void TestGenerateEmail_EmptyFieldsNotShown()
		{
			var sendToRegistryCollection = ATLASMessageHelper.CreateSendAcknowledgementsRegistryCollection(Factory);
			using (DECustomsDataRegistry.Instance.SendTaxChangeAcknowledgements.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, sendToRegistryCollection))
			{
				CombineAssertions(() =>
				{
					dataProviderMock.Setup(m => m.ReferenceNumber).Returns((string)null);
					dataProviderMock.Setup(m => m.TaxAssessmentCreationDate).Returns((DateTime?)null);
					dataProviderMock.Setup(m => m.MaturityDate).Returns((DateTime?)null);
					dataProviderMock.Setup(m => m.LocalReferenceNumber).Returns((string)null);
					ProcessMessage(message);
					var email = Env.OutgoingCustomsMailManager.EmailsCreated.FirstOrDefault(x => x.Subject.Contains("SRATAX"));
					AssertNotContains("Creation Date", "<tr><td>Creation Date</td>", email.Body);
					AssertNotContains("Maturity Date", "<tr><td>Maturity Date</td>", email.Body);
					AssertNotContains("Reference Number", "<tr><td>Reference Number</td>", email.Body);
					AssertNotContains("MRN", "<tr><td>MRN</td>", email.Body);
					AssertNotContains("Local Reference Number", "<tr><td>Local Reference Number</td>", email.Body);
				});
			}
		}

		public void TestGenerateEmail_NoInterchangeRecipientEBS()
		{
			var sendToRegistryCollection = ATLASMessageHelper.CreateSendAcknowledgementsRegistryCollection(Factory);
			using (DECustomsDataRegistry.Instance.SendTaxChangeAcknowledgements.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, sendToRegistryCollection))
			{
				dataProviderMock.Setup(m => m.InterchangeRecipientEBS).Returns((string)null);
				ProcessMessage(message);
				AssertNull(Env.OutgoingCustomsMailManager.EmailsCreated.FirstOrDefault(x => x.Subject.Contains(message.DataProvider.ReferenceNumber)));
			}
		}

		public void TestGenerateEmail_InterchangeRecipientEBSNotCapture()
		{
			var sendToRegistryCollection = ATLASMessageHelper.CreateSendAcknowledgementsRegistryCollection(Factory);
			using (DECustomsDataRegistry.Instance.SendTaxChangeAcknowledgements.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, sendToRegistryCollection))
			{
				dataProviderMock.Setup(m => m.InterchangeRecipientEBS).Returns("0002");
				ProcessMessage(message);
				AssertNull(Env.OutgoingCustomsMailManager.EmailsCreated.FirstOrDefault(x => x.Subject.Contains(message.DataProvider.ReferenceNumber)));
			}
		}

		public void TestPopulateLogbookRegNum()
		{
			dataProviderMock.Setup(x => x.MRN).Returns("23DE586601055987B7");
			ProcessMessage(message);
			AssertContainsExactElementsInAnyOrder(new[] { "ATS000009360920213450", "23DE586601055987B7" }, message.GetLogbookRegistrationNumbers());
		}

		protected override ZString MessageFriendlyName => "Import SRATAX Message Processor";

		protected override DEBranchCustomsApplicationTypeMessageProcessor<AtlasInboundEDIMessage<ISRATAX>> Processor => new ImportSRATAXMessageProcessor(logger);

		protected override bool ExpectedMustHaveLinkedObject => false;

		protected override bool ExpectedNeedAttachDocumentsToMessage => true;

		protected override bool ExpectedNeedAttachDocumentsToLinkedObject => false;

		protected override void SetUp()
		{
			base.SetUp();

			dataProviderMock = new Mock<ISRATAX>();
			dataProviderMock.Setup(x => x.MessageIdentifier).Returns("SRATAX52395580308888770311173073312");
			dataProviderMock.Setup(x => x.InterchangeRecipientEBS).Returns("0001");
			dataProviderMock.Setup(x => x.TaxChangeAssessmentType).Returns(ImportTaxChangeAssessmentTypeList.Codes.A);
			dataProviderMock.Setup(x => x.ReferenceNumber).Returns("ATS000009360920213450");
			dataProviderMock.Setup(x => x.LocalReferenceNumber).Returns("ASV-Nr. 754-06-2021-3450");
			dataProviderMock.Setup(x => x.TaxAssessmentCreationDate).Returns(new DateTime(2021, 09, 21));
			dataProviderMock.Setup(m => m.MaturityDate).Returns(new DateTime(2021, 09, 30));
			messageMock = Factory.NewMoq<AtlasInboundEDIMessage<ISRATAX>>();
			messageMock.Setup(m => m.DataProvider).Returns(dataProviderMock.Object);
			message = messageMock.Object;

			Factory.Save();
		}
		Mock<ISRATAX> dataProviderMock;
		Mock<AtlasInboundEDIMessage<ISRATAX>> messageMock;
		AtlasInboundEDIMessage<ISRATAX> message;

		class ImportSRATAXMessageProcessorForTest : ImportSRATAXMessageProcessor
		{
			public ImportSRATAXMessageProcessorForTest(LoggingInformation logger) : base(logger)
			{
			}

			public List<AttachedDocument> GetAttachedDocumentsExposed(AtlasInboundEDIMessage<ISRATAX> message) => GetAttachedDocuments(message);
		}
	}
}
