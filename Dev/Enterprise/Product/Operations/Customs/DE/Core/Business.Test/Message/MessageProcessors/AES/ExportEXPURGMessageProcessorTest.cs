using System;
using System.Linq;
using CargoWise.Customs.DE.MessageContracts.AES;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;
using Enterprise.Customs.DE.Business.Testing;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.DE.Business.Declaration.Testing
{
	[TestedType(typeof(ExportEXPURGMessageProcessor))]
	sealed class ExportEXPURGMessageProcessorTest : MessageProcessorAbstractTest<ExportEXPURGMessageProcessor, AesInboundEDIMessage<IEXPURG>>
	{
		public void TestGetLinkedObjectFromOriginalMessage()
		{
			ProcessMessage(message);
			AssertEquals(entryHeader, message.EM_LinkedObject);
		}

		public void TestGetLinkedObjectFromMRN()
		{
			var entryNum = Factory.NewWithValidTestData<CusEntryNumber>();
			entryNum.CE_EntryNum = "21DE123050554788M5";
			entryNum.CE_EntryType = CusEntryNumberTypes.Standard.MovementReferenceNumber;
			entryNum.Parent = entryHeader;

			messageMock.Setup(x => x.DataProvider.ReferencedMessageIdentifier).Returns("INVALID_CODE");

			ProcessMessage(message);

			AssertEquals(entryHeader, message.EM_LinkedObject);
		}

		public void TestDataProviderNullDueToInvalidMessage()
		{
			messageMock.Setup(x => x.DataProvider).Returns((IEXPURG)null);
			AssertNoExceptionThrown(() => ProcessMessage(message));
		}

		public void TestProcessMessage()
		{
			ProcessMessage(message);
			CombineAssertions(() =>
			{
				AssertEquals("EM_Status", EDIMessage.Status.ProcessedOK, message.EM_Status);
				var reference = entryHeader.Declaration.JE_DeclarationReference;
				var subject = $"AES EXP Response for {reference}";
				var bodyMessageTitle = $"<title>{subject}</title>";
				var bodyMessageHeader = $@"<strong>AES EXP Response for <a href=""edient:Command=ShowEditForm&LicenceCode={GlbCompany.CurrentCompany.LicenceKeyIdentifier}&ControllerID=JobDeclaration&BusinessEntityPK=" + entryHeader.Declaration.PK;
				var bodyMessageSummary = $@"Your Export Declaration Message for Job {reference} has a request to send an AES Entire Message (E_EXP_ENT). For details please follow the Link to the Job.<br/>"
				+ "Date for latest response of AES Entire Message: <b>01.01.2000<b/>";
				var email = Env.OutgoingCustomsMailManager.EmailsCreated.Single();
				AssertEmailForSingleRecipient(ZString.Empty, email, "Dummy@dummy.com", subject, bodyMessageTitle, bodyMessageHeader, bodyMessageSummary);
			});
		}

		public void TestPopulateLogbookRegNum()
		{
			ProcessMessage(message);
			AssertEquals("21DE123050554788M5", message.GetLogbookRegistrationNumber());
		}

		public void TestPopulateLogbookLocalReferenceNumber()
		{
			ProcessMessage(message);
			AssertEquals("Reference1234567abcd", message.GetLogbookLocalReferenceNumber());
		}

		public void TestAddCustomsEntryStatusLog()
		{
			ProcessMessage(message);
			AssertEquals(UniversalReferenceConstants.EntryStatus.URG, entryHeader.GetCustomsEntryStatusEventReference());
		}

		protected override ZString MessageFriendlyName => "Export EXPURG Message Processor";

		protected override DEBranchCustomsApplicationTypeMessageProcessor<AesInboundEDIMessage<IEXPURG>> Processor => new ExportEXPURGMessageProcessor(logger);

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
			outgoingMessage = CreateOriginalMessageLinkedToParent<AesEDIMessage>(entryHeader, "EXPURG4398");
			outgoingMessage.EM_SystemCreateUser = staff.GS_Code;

			var dataProviderMock = new Mock<IEXPURG>();
			dataProviderMock.Setup(m => m.MessageIdentifier).Returns("2402984023");
			dataProviderMock.Setup(m => m.ReferencedMessageIdentifier).Returns("EXPURG4398");
			dataProviderMock.Setup(m => m.MovementReferenceNumber).Returns("21DE123050554788M5");
			dataProviderMock.Setup(m => m.LocalReferenceNumber).Returns("Reference1234567abcd");
			dataProviderMock.Setup(m => m.LatestResponseDate).Returns(new DateTime(2000, 1, 1));

			messageMock = Factory.NewMoq<AesInboundEDIMessage<IEXPURG>>();
			messageMock.Setup(m => m.DataProvider).Returns(dataProviderMock.Object);
			message = messageMock.Object;
			Factory.Save();
		}
		CusEntryHeader entryHeader;
		EDIMessage outgoingMessage;
		Mock<AesInboundEDIMessage<IEXPURG>> messageMock;
		AesInboundEDIMessage<IEXPURG> message;
	}
}
