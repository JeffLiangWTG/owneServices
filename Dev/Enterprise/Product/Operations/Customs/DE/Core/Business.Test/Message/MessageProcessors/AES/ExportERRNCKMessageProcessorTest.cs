using System.Linq;
using CargoWise.Customs.DE.MessageContracts;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;
using Enterprise.Customs.DE.Business.Testing;
using Enterprise.Customs.DE.Messaging;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.DE.Business.Declaration.Testing
{
	[TestedType(typeof(ExportERRNCKMessageProcessor))]
	sealed class ExportERRNCKMessageProcessorTest : MessageProcessorAbstractTest<ExportERRNCKMessageProcessor, AesInboundEDIMessage<IERRNCK>>
	{
		protected override ZString MessageFriendlyName => "Export EXPERR Message Processor";

		protected override DEBranchCustomsApplicationTypeMessageProcessor<AesInboundEDIMessage<IERRNCK>> Processor => new ExportERRNCKMessageProcessor(logger);

		public void TestGetLinkedObjectFromOriginalMessage()
		{
			ProcessMessage(message);
			AssertEquals(entryHeader, message.EM_LinkedObject);
		}

		public void TestProcessMessage()
		{
			ProcessMessage(message);
			CombineAssertions(() =>
			{
				AssertEquals("EM_Status", EDIMessage.Status.ProcessedOK, message.EM_Status);
				AssertEquals("CH_Status", "REJ", entryHeader.CH_Status);

				var reference = entryHeader.Declaration.JE_DeclarationReference;
				var lrn = entryHeader.LocalReferenceNumber;
				var subject = $"Export Declaration Message Status Response for {reference} - LRN: {lrn}";
				var titleSubject = $"Export Declaration Message Status Response for {reference}";
				var bodyMessageTitle = $"<title>{titleSubject}</title>";
				var bodyMessageHeader = $@"<strong>Export Declaration Message Status Response for <a href=""edient:Command=ShowEditForm&LicenceCode={GlbCompany.CurrentCompany.LicenceKeyIdentifier}&ControllerID=JobDeclaration&BusinessEntityPK=" + entryHeader.Declaration.PK;
				var bodyMessageSummary = $@"Your Export Declaration Message for Job {reference} has been rejected. For details please follow the Link to the Job.
Shown below is a summary of relevant information received in the message:";
				var bodyMessageTable = @"<table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table"">"
					+ @"<thead><tr class=""tableheadings"">"
					+ "<th>Error Code</th>"
					+ "<th>Pointer</th>"
					+ "<th>Text</th>"
					+ "<th>Original Value</th>"
					+ "</tr></thead>"
					+ "<tr><td>AAA00000</td>"
					+ "<td>A</td>"
					+ "<td>Text123</td>"
					+ "<td>Original12345</td>"
					+ "</tr></table>";
				var email = Env.OutgoingCustomsMailManager.EmailsCreated.Single();
				AssertEmailForSingleRecipientWithTable(ZString.Empty, email, "Dummy@dummy.com", subject, bodyMessageTitle, bodyMessageHeader, bodyMessageSummary, bodyMessageTable);
			});
		}

		public void TestProcessMessage_MessageGroupIsNotEXP()
		{
			foreach (var group in new ZString[] { "EXQ", "EXT", "???" })
			{
				message.EM_Status = EDIMessage.Status.Queued;
				dataProviderMock.Setup(m => m.MessageGroup).Returns(group);
				ProcessMessage(message);

				CombineAssertions("MessageGroup: " + group, () =>
				{
					AssertEquals("CH_Status", ZString.Empty, entryHeader.CH_Status);
					AssertEquals("EM_Status", EDIMessage.Status.ProcessedOK, message.EM_Status);
					AssertEquals("No Email", false, Env.OutgoingCustomsMailManager.EmailsCreated.Any());
				});
			}
		}

		public void TestPopulateLogbookRegNum()
		{
			ProcessMessage(message);
			AssertEquals("21DE123050554788M5", message.GetLogbookRegistrationNumber());
		}

		public void TestPopulateLogbookRegNum_ShouldUseReferencedMessageIdentifier_WhenReferenceNumberIsNullOrEmpty()
		{
			var testCases = new[] { string.Empty, null };
			foreach (var testCase in testCases)
			{
				dataProviderMock.Setup(m => m.ReferenceNumber).Returns(testCase);

				ProcessMessage(message);

				AssertEquals($"{nameof(IERRNCK.ReferenceNumber)} = \"{testCase}\"", "EXPERRTest", message.GetLogbookRegistrationNumber());
			}
		}

		public void TestPopulateLogbookLocalReferenceNumber()
		{
			ProcessMessage(message);
			AssertEquals("Reference1234567abcd", message.GetLogbookLocalReferenceNumber());
		}

		public void TestAddCustomsEntryStatusLog()
		{
			ProcessMessage(message);
			AssertEquals(UniversalReferenceConstants.EntryStatus.ERR, entryHeader.GetCustomsEntryStatusEventReference());
		}

		protected override void SetUp()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;

			entryHeader = (CusEntryHeader)declaration.ActiveEntryHeaders.AddNew();
			var entryNum = Factory.NewWithValidTestData<CusEntryNumber>();
			entryNum.CE_EntryNum = "00DE000000000000E0";
			entryNum.Parent = entryHeader;

			var staff = Factory.LoadFromNaturalKey<GlbStaff>(GlbStaffSchema.GS_Code, GlbStaff.CurrentUser.GS_Code);
			staff.GS_EmailAddress = "Dummy@dummy.com";
			var groupPK = EnvProxy.Instance.Registry.PostMasterGroup;
			var group = Factory.Load<GlbGroup>(groupPK);
			group.Staff.Add(staff);

			outgoingMessage = CreateOriginalMessageLinkedToParent<AesEDIMessage>(entryHeader, "EXPERRTest");
			outgoingMessage.EM_ApplicationCode = "DEE";
			outgoingMessage.EM_MessageType = EDIMessageTypeList.Codes.AES;
			outgoingMessage.EM_MessageSubType = ExportMessageSubTypeList.Codes.EXP;
			outgoingMessage.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			outgoingMessage.EM_LinkTable = CusEntryHeaderSchema.Constants.TableName;

			dataProviderMock = new Mock<IERRNCK>();
			dataProviderMock.Setup(m => m.MessageIdentifier).Returns("0000000009");
			dataProviderMock.Setup(m => m.MessageGroup).Returns(ExportMessageSubTypeList.Codes.EXP);
			dataProviderMock.Setup(m => m.ReferencedMessageIdentifier).Returns("EXPERRTest");
			dataProviderMock.Setup(m => m.ReferenceNumber).Returns("21DE123050554788M5");
			dataProviderMock.Setup(m => m.LocalReferenceNumber).Returns("Reference1234567abcd");

			var err = new Mock<IERRNCKError>();
			err.Setup(m => m.Code).Returns("AAA00000");
			err.Setup(m => m.Pointer).Returns("A");
			err.Setup(m => m.Text).Returns("Text123");
			err.Setup(m => m.OriginalValue).Returns("Original12345");
			dataProviderMock.Setup(m => m.Errors).Returns(new IERRNCKError[] { err.Object });

			var messageMock = Factory.NewMoq<AesInboundEDIMessage<IERRNCK>>();
			messageMock.Setup(m => m.DataProvider).Returns(dataProviderMock.Object);
			message = messageMock.Object;

			Factory.Save();
		}
		CusEntryHeader entryHeader;
		EDIMessage outgoingMessage;
		AesInboundEDIMessage<IERRNCK> message;
		Mock<IERRNCK> dataProviderMock;
	}
}
