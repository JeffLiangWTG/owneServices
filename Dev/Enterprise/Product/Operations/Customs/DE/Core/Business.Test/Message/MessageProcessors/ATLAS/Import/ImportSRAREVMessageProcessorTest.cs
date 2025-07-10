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
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.MasterFiles.Integration;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.DE.Business.Import.Testing
{
	[TestedType(typeof(ImportSRAREVMessageProcessor))]
	sealed class ImportSRAREVMessageProcessorTest : MessageProcessorAbstractTest<ImportSRAREVMessageProcessor, AtlasInboundEDIMessage<ISRAREV>>
	{
		public void TestGetLinkedObject_CancelledReferenceNumber()
		{
			ProcessMessage(message);
			AssertSame(originalMessage, message.EM_LinkedObject);
		}

		public void TestGetLinkedObject_MRN()
		{
			dataProviderMock.Setup(m => m.CancelledReferenceNumber).Returns((string)null);
			dataProviderMock.Setup(m => m.CancelledMRN).Returns(ReferenceNumber);
			ProcessMessage(message);
			AssertSame(originalMessage, message.EM_LinkedObject);
		}

		public void TestNoLinkedObject()
		{
			dataProviderMock.Setup(m => m.CancelledReferenceNumber).Returns("ATB0000000000000");
			ProcessMessage(message);
			CombineAssertions(() =>
			{
				AssertNull("No LinkedObject", message.EM_LinkedObject);
				AssertEquals("Message status == error", Enterprise.Messaging.Integration.EDIMessageStatusList.Codes.Error, message.EM_Status);
			});
		}

		public void TestGetCorrectBranchPK()
		{
			var processor = new ImportSRAREVMessageProcessorForTest(logger);
			AssertEquals(originalMessage.EM_GB, processor.GetCorrectBranchPKExposed(originalMessage));
		}

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
			var processor = new ImportSRAREVMessageProcessorForTest(logger);
			AssertEquals(1, processor.GetAttachedDocumentsExposed(message).Count(x => x.FileName.Equals("file1.pdf")));
			message.AttachedDocuments[0].ImageData.Dispose();
		}

		public void TestDataProviderNullDueToInvalidMessage()
		{
			messageMock.Setup(m => m.DataProvider).Returns((ISRAREV)null);
			AssertNoExceptionThrown(() => ProcessMessage(message));
		}

		public void TestProcessMessage()
		{
			ProcessMessage(message);
			CombineAssertions(() =>
			{
				AssertEquals("EM_Status", Enterprise.Messaging.Integration.EDIMessageStatusList.Codes.ProcessedOK, message.EM_Status);
				AssertEquals("CE_EntryStatus", TaxChangeAssessmentStatusCodeList.Codes.CAN, mrnEntryNumber.CE_EntryStatus);
			});
		}

		public void TestProcessMessage_MultipleCusEntryNumbers()
		{
			var secondOriginalMessage = Factory.New<TaxChangeAssessment>();
			secondOriginalMessage.EM_ApplicationReference = "NSTAXJ";
			secondOriginalMessage.EM_Status = Enterprise.Messaging.Integration.EDIMessageStatusList.Codes.Sent;

			var secondMrnEntryNumber = CusEntryNumber.LoadOrCreate(secondOriginalMessage, CusEntryNumberTypes.Standard.MovementReferenceNumber, Core.Constants.CountryCodes.Germany);
			secondMrnEntryNumber.CE_EntryNum = ReferenceNumber;

			Factory.Save();

			messageMock.Setup(m => m.AttachedDocuments).Returns(new List<AttachedDocument>
			{
				new AttachedDocument
				{
					FileName = "file1.pdf",
					Type = new DocumentType { Code = "AAA", Description = "AAA Desc" },
					ImageData = (SubStreamableStream)new MemoryStream(Convert.FromBase64String("MkE5UlZFQUMtQVpQMlU0RjItSkhKRVNDUkMtWllSSFpOUEYtTFREODQ2TFYtMzlRRDZGTk0tM1Y3VzZNRDgtUTdVMjk5VTI="))
				}
			});

			ProcessMessage(message);
			CombineAssertions(() =>
			{
				AssertEquals("EM_Status", Enterprise.Messaging.Integration.EDIMessageStatusList.Codes.ProcessedOK, message.EM_Status);
				AssertEquals("mrnEntryNumber.CE_EntryStatus", TaxChangeAssessmentStatusCodeList.Codes.CAN, mrnEntryNumber.CE_EntryStatus);
				AssertEquals("secondMrnEntryNumber.CE_EntryStatus", TaxChangeAssessmentStatusCodeList.Codes.CAN, secondMrnEntryNumber.CE_EntryStatus);

				AssertEquals("originalMessage.Files count", 1, originalMessage.DocManagerInfo.Files.ToList<IeDoc>().Count(x => x.FileName.Equals("file1.pdf")));
				AssertEquals("secondOriginalMessage.Files count", 1, secondOriginalMessage.DocManagerInfo.Files.ToList<IeDoc>().Count(x => x.FileName.Equals("file1.pdf")));
			});

			message.AttachedDocuments[0].ImageData.Dispose();
		}

		public void TestGenerateEmail()
		{
			var sendToRegistryCollection = ATLASMessageHelper.CreateSendAcknowledgementsRegistryCollection(Factory);
			using (DECustomsDataRegistry.Instance.SendTaxChangeAcknowledgements.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, sendToRegistryCollection))
			{
				dataProviderMock.Setup(m => m.CancelledMRN).Returns("CancelledMRN");
				ProcessMessage(message);

				var reference = originalMessage.ReferenceNumber;
				var email = Env.OutgoingCustomsMailManager.EmailsCreated.FirstOrDefault(x => x.Subject.Contains(reference));

				var subject = $"Import SRAREV – Reversal Tax Change Assessment Response for {reference}";
				var bodyMessageTitle = $"<title>{subject}</title>";
				var bodyMessageHeader = @"<strong>Import SRAREV – Reversal Tax Change Assessment Response for <a href=""edient:Command=ShowEditForm&LicenceCode=EDIEDIDAT&ControllerID=DETaxChangeAssessment&BusinessEntityPK=" + originalMessage.PK;
				var bodyMessageSummary = "For details please follow the Link to the Job.";
				var bodyMessageTable = @"<table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table"">"
										+ $"<tr><td>Canceled Reference Number</td><td>{ReferenceNumber}</td></tr>"
										+ "<tr><td>Canceled MRN</td><td>CancelledMRN</td></tr></table>";
				AssertEmailForSingleRecipientWithTable(string.Empty, email, "test@mail.com", subject, bodyMessageTitle, bodyMessageHeader, bodyMessageSummary, bodyMessageTable);
			}
		}

		public void TestGenerateEmail_NoInterchangeRecipientEBS()
		{
			var sendToRegistryCollection = ATLASMessageHelper.CreateSendAcknowledgementsRegistryCollection(Factory);
			using (DECustomsDataRegistry.Instance.SendTaxChangeAcknowledgements.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, sendToRegistryCollection))
			{
				dataProviderMock.Setup(m => m.InterchangeRecipientEBS).Returns(string.Empty);
				ProcessMessage(message);

				var reference = originalMessage.ReferenceNumber;
				AssertNull(Env.OutgoingCustomsMailManager.EmailsCreated.FirstOrDefault(x => x.Subject.Contains(reference)));
			}
		}

		public void TestGenerateEmail_InterchangeRecipientEBSNotCapture()
		{
			var sendToRegistryCollection = ATLASMessageHelper.CreateSendAcknowledgementsRegistryCollection(Factory);
			using (DECustomsDataRegistry.Instance.SendTaxChangeAcknowledgements.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, sendToRegistryCollection))
			{
				dataProviderMock.Setup(m => m.InterchangeRecipientEBS).Returns("0002");
				ProcessMessage(message);

				var reference = originalMessage.ReferenceNumber;
				AssertNull(Env.OutgoingCustomsMailManager.EmailsCreated.FirstOrDefault(x => x.Subject.Contains(reference)));
			}
		}

		public void TestGenerateEmail_EmptyFieldsNotShown()
		{
			var sendToRegistryCollection = ATLASMessageHelper.CreateSendAcknowledgementsRegistryCollection(Factory);
			using (DECustomsDataRegistry.Instance.SendTaxChangeAcknowledgements.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, sendToRegistryCollection))
			{
				CombineAssertions(() =>
				{
					ProcessMessage(message);
					var email = Env.OutgoingCustomsMailManager.EmailsCreated.Single();
					AssertNotContains("No Cancelled MRN", "<td>Cancelled MRN</td>", email.Body);

					dataProviderMock.Setup(m => m.CancelledReferenceNumber).Returns((string)null);
					dataProviderMock.Setup(m => m.CancelledMRN).Returns(ReferenceNumber);
					ProcessMessage(message);
					email = Env.OutgoingCustomsMailManager.EmailsCreated.Last();
					AssertNotContains("No Cancelled ReferenceNumber", "<td>Cancelled Reference Number</td>", email.Body);
				});
			}
		}

		public void TestPopulateLogbookRegNum()
		{
			dataProviderMock.Setup(m => m.CancelledMRN).Returns("CancelledMRN");
			ProcessMessage(message);
			AssertContainsExactElementsInAnyOrder(new[] { ReferenceNumber, "CancelledMRN" }, message.GetLogbookRegistrationNumbers());
		}

		protected override ZString MessageFriendlyName => "Import SRAREV Message Processor";

		protected override DEBranchCustomsApplicationTypeMessageProcessor<AtlasInboundEDIMessage<ISRAREV>> Processor => new ImportSRAREVMessageProcessor(logger);

		protected override void SetUp()
		{
			base.SetUp();

			originalMessage = Factory.New<TaxChangeAssessment>();
			originalMessage.EM_ApplicationReference = "NSTAXJ";
			originalMessage.EM_Status = Enterprise.Messaging.Integration.EDIMessageStatusList.Codes.Sent;

			mrnEntryNumber = CusEntryNumber.LoadOrCreate(originalMessage, CusEntryNumberTypes.Standard.MovementReferenceNumber, Core.Constants.CountryCodes.Germany);
			mrnEntryNumber.CE_EntryNum = ReferenceNumber;

			dataProviderMock = new Mock<ISRAREV>();
			dataProviderMock.Setup(x => x.MessageIdentifier).Returns("SRAREV52395580308888770311173073312");
			dataProviderMock.Setup(x => x.CancelledReferenceNumber).Returns(ReferenceNumber);
			dataProviderMock.Setup(m => m.InterchangeRecipientEBS).Returns("0001");
			messageMock = Factory.NewMoq<AtlasInboundEDIMessage<ISRAREV>>();
			messageMock.Setup(m => m.DataProvider).Returns(dataProviderMock.Object);
			message = messageMock.Object;

			Factory.Save();
		}
		TaxChangeAssessment originalMessage;
		CusEntryNumber mrnEntryNumber;
		Mock<ISRAREV> dataProviderMock;
		Mock<AtlasInboundEDIMessage<ISRAREV>> messageMock;
		AtlasInboundEDIMessage<ISRAREV> message;
		const string ReferenceNumber = "ATR673055920820113428";

		class ImportSRAREVMessageProcessorForTest : ImportSRAREVMessageProcessor
		{
			public ImportSRAREVMessageProcessorForTest(LoggingInformation logger) : base(logger)
			{
			}

			public ZGuid GetCorrectBranchPKExposed(BusinessObject linkedObject) => GetCorrectBranchPK(linkedObject);

			public List<AttachedDocument> GetAttachedDocumentsExposed(AtlasInboundEDIMessage<ISRAREV> message) => GetAttachedDocuments(message);
		}
	}
}
