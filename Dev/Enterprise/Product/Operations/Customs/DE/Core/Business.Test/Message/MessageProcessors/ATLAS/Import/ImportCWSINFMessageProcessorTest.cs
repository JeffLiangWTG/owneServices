using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.Customs.DE.Business.Testing;
using Enterprise.Customs.DE.Messaging;
using Enterprise.Customs.DE.Registry;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.ZArchitecture.Environment;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.DE.Business.Import.Testing
{
	[TestedType(typeof(ImportCWSINFMessageProcessor))]
	class ImportCWSINFMessageProcessorTest : MessageProcessorAbstractTest<ImportCWSINFMessageProcessor, AtlasInboundEDIMessage<ICWSINF>>
	{
		public void TestDataProviderNullDueToInvalidMessage()
		{
			messageMock.Setup(m => m.DataProvider).Returns((ICWSINF)null);
			CombineAssertions(() =>
			{
				AssertNoExceptionThrown("No exception", () => ProcessMessage(message));
				AssertEquals("EM_Status", Enterprise.Messaging.Integration.EDIMessageStatusList.Codes.Error, message.EM_Status);
			});
		}

		public void TestProcessMessage()
		{
			ProcessMessage(message);
			AssertEquals("EM_Status", Enterprise.Messaging.Integration.EDIMessageStatusList.Codes.ProcessedOK, message.EM_Status);
		}

		public void TestGenerateEmail_StaffMember()
		{
			var sendToRegistry = new ImportGroupNotification(Core.Constants.EmailTo.StaffMember, ZGuid.Empty);
			using (DECustomsDataRegistry.Instance.SendImportAcknowledgements.SetTemporaryValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, sendToRegistry))
			{
				ProcessMessage(message);
				AssertEquals(false, Env.OutgoingCustomsMailManager.EmailsCreated.Any(x => x.Subject.Contains("CWSINF")));
			}
		}

		public void TestGenerateEmail_NominatedGroup()
		{
			var sendToRegistry = new ImportGroupNotification(Core.Constants.EmailTo.NominatedGroup, CreateGlbGroup().PK);
			using (DECustomsDataRegistry.Instance.SendImportAcknowledgements.SetTemporaryValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, sendToRegistry))
			{
				ProcessMessage(message);
				AssertEquals(true, Env.OutgoingCustomsMailManager.EmailsCreated.Any(x => x.Subject.Contains("CWSINF")));
			}
		}

		public void TestGenerateEmail_StaffMemberAndNominatedGroup()
		{
			var emailGroup = CreateGlbGroup();

			messageMock.Setup(m => m.AttachedDocuments).Returns(new List<AttachedDocument>
			{
				new AttachedDocument
				{
					FileName = "file1.pdf",
					Type = new DocumentType { Code = "AAA", Description = "AAA Desc" },
					ImageData = (SubStreamableStream)new MemoryStream(Convert.FromBase64String("XXX="))
				}
			});

			var sendToRegistry = new ImportGroupNotification(Core.Constants.EmailTo.StaffMemberAndNominatedGroup, emailGroup.PK);
			using (DECustomsDataRegistry.Instance.SendImportAcknowledgements.SetTemporaryValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, sendToRegistry))
			{
				ProcessMessage(message);

				var email = Env.OutgoingCustomsMailManager.EmailsCreated.Single(x => x.Subject.Contains("CWSINF"));
				var subject = "Import CWSINF – Stock Information bonded warehouse Response";
				var bodyMessageTitle = $"<title>{subject}</title>";
				var bodyMessageHeader = $"<strong>{subject}</strong>";
				var bodyMessageSummary = "For details please open the attached report.";
				CombineAssertions(() =>
				{
					AssertEmailForSingleRecipient(ZString.Empty, email, "test@mail.com", subject, bodyMessageTitle, bodyMessageHeader, bodyMessageSummary);
					AssertEquals("File has been attached", 1, email.Attachments.Cast<AttachmentDef>().Count(x => x.DisplayName == "file1.pdf"));
				});
			}
			message.AttachedDocuments[0].ImageData.Dispose();
		}

		public void TestPopulateLogbookRegNum()
		{
			ProcessMessage(message);
			AssertEquals("ABC123456", message.GetLogbookLocalReferenceNumber());
		}

		protected override ZString MessageFriendlyName => "Import CWSINF Message Processor";

		protected override bool ExpectedMustHaveLinkedObject => false;

		protected override bool ExpectedNeedAttachDocumentsToLinkedObject => false;

		protected override DEBranchCustomsApplicationTypeMessageProcessor<AtlasInboundEDIMessage<ICWSINF>> Processor => new ImportCWSINFMessageProcessor(logger);

		protected override void SetUp()
		{
			base.SetUp();

			dataProviderMock = new Mock<ICWSINF>();
			dataProviderMock.Setup(x => x.MessageIdentifier).Returns("0000000009");
			dataProviderMock.Setup(m => m.CurrentProcedure).Returns("ABC123456");

			messageMock = Factory.NewMoq<AtlasInboundEDIMessage<ICWSINF>>();
			messageMock.Setup(m => m.DataProvider).Returns(dataProviderMock.Object);
			message = messageMock.Object;

			Factory.Save();
		}
		Mock<ICWSINF> dataProviderMock;
		Mock<AtlasInboundEDIMessage<ICWSINF>> messageMock;
		AtlasInboundEDIMessage<ICWSINF> message;

		GlbGroup CreateGlbGroup()
		{
			var emailGroup = Factory.NewWithValidTestData<GlbGroup>();
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_EmailAddress = "test@mail.com";
			emailGroup.Staff.Add(staff);
			Factory.Save();

			return emailGroup;
		}
	}
}
