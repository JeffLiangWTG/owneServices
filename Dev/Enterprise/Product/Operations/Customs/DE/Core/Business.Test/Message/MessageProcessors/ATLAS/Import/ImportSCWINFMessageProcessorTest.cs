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
	[TestedType(typeof(ImportSCWINFMessageProcessor))]
	sealed class ImportSCWINFMessageProcessorTest : MessageProcessorAbstractTest<ImportSCWINFMessageProcessor, AtlasInboundEDIMessage<ISCWINF>>
	{
		public void TestDataProviderNullDueToInvalidMessage()
		{
			messageMock.Setup(m => m.DataProvider).Returns((ISCWINF)null);
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
				AssertEquals(false, Env.OutgoingCustomsMailManager.EmailsCreated.Any(x => x.Subject.Contains("SCWINF")));
			}
		}

		public void TestGenerateEmail_NominatedGroup()
		{
			var sendToRegistry = new ImportGroupNotification(Core.Constants.EmailTo.NominatedGroup, CreateGlbGroup().PK);
			using (DECustomsDataRegistry.Instance.SendImportAcknowledgements.SetTemporaryValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, sendToRegistry))
			{
				ProcessMessage(message);
				AssertEquals(true, Env.OutgoingCustomsMailManager.EmailsCreated.Any(x => x.Subject.Contains("SCWINF")));
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
				},
			});

			var sendToRegistry = new ImportGroupNotification(Core.Constants.EmailTo.StaffMemberAndNominatedGroup, emailGroup.PK);
			using (DECustomsDataRegistry.Instance.SendImportAcknowledgements.SetTemporaryValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, sendToRegistry))
			{
				ProcessMessage(message);

				var email = Env.OutgoingCustomsMailManager.EmailsCreated.Single(x => x.Subject.Contains("SCWINF"));
				var subject = "Import SCWINF – Transfer Information bonded warehouse Response";
				var bodyMessageTitle = $"<title>{subject}</title>";
				var bodyMessageHeader = "<strong>Import SCWINF – Transfer Information bonded warehouse Response";
				var bodyMessageSummary = "For details please open the attached report.";
				var bodyMessageTable = @"<table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table""><tr><td>MRN</td><td>24DE12345678901234</td></tr><tr><td>Reference Number</td><td>ATR673055920820113428</td></tr></table>";
				CombineAssertions(() =>
				{
					AssertEmailForSingleRecipientWithTable(ZString.Empty, email, "test@mail.com", subject, bodyMessageTitle, bodyMessageHeader, bodyMessageSummary, bodyMessageTable);
					AssertEquals("File has been attached", 1, email.Attachments.Cast<AttachmentDef>().Count(x => x.DisplayName == "file1.pdf"));
				});
			}
			message.AttachedDocuments[0].ImageData.Dispose();
		}

		public void TestPopulateLogbookRegNum()
		{
			ProcessMessage(message);
			CombineAssertions(() =>
			{
				AssertEquals("ATR673055920820113428, 24DE12345678901234", message.GetLogbookRegistrationNumber());
				AssertEquals("ABC123456", message.GetLogbookLocalReferenceNumber());
			});
		}

		protected override ZString MessageFriendlyName => "Import SCWINF Message Processor";

		protected override bool ExpectedMustHaveLinkedObject => false;

		protected override bool ExpectedNeedAttachDocumentsToLinkedObject => false;

		protected override DEBranchCustomsApplicationTypeMessageProcessor<AtlasInboundEDIMessage<ISCWINF>> Processor => new ImportSCWINFMessageProcessor(logger);

		protected override void SetUp()
		{
			base.SetUp();

			dataProviderMock = new Mock<ISCWINF>();
			dataProviderMock.Setup(x => x.MessageIdentifier).Returns("0000000009");
			dataProviderMock.Setup(x => x.ReferenceNumber).Returns("ATR673055920820113428");
			dataProviderMock.Setup(m => m.CurrentProcedure).Returns("ABC123456");
			dataProviderMock.Setup(m => m.MRN).Returns("24DE12345678901234");

			messageMock = Factory.NewMoq<AtlasInboundEDIMessage<ISCWINF>>();
			messageMock.Setup(m => m.DataProvider).Returns(dataProviderMock.Object);
			message = messageMock.Object;

			Factory.Save();
		}
		Mock<ISCWINF> dataProviderMock;
		Mock<AtlasInboundEDIMessage<ISCWINF>> messageMock;
		AtlasInboundEDIMessage<ISCWINF> message;

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
