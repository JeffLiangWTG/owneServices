using System.Linq;
using CargoWise.Customs.DE.MessageContracts.AES;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;
using Enterprise.Customs.DE.Business.Testing;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.Universal.Testing;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.DE.Business.Declaration.Testing
{
	[TestedType(typeof(ExportEXPREJMessageProcessor))]
	sealed class ExportEXPREJMessageProcessorTest : MessageProcessorAbstractTest<ExportEXPREJMessageProcessor, AesInboundEDIMessage<IEXPREJ>>
		, ITestEntryLinesLockedAfterProcessing
	{
		public void TestGetLinkedObjectFromOriginalMessage()
		{
			ProcessMessage(message);
			AssertEquals(entryHeader, message.EM_LinkedObject);
		}

		public void TestGetLinkedObjectFromMRN()
		{
			var entryNum = Factory.NewWithValidTestData<CusEntryNumber>();
			entryNum.CE_EntryNum = "00DE000000000000E0";
			entryNum.CE_EntryType = CusEntryNumberTypes.Standard.MovementReferenceNumber;
			entryNum.Parent = entryHeader;

			messageMock.Setup(x => x.DataProvider.ReferencedMessageIdentifier).Returns("INVALID_CODE");
			messageMock.Setup(x => x.DataProvider.MovementReferenceNumber).Returns("00DE000000000000E0");

			ProcessMessage(message);

			AssertEquals(entryHeader, message.EM_LinkedObject);
		}

		public void TestDataProviderNullDueToInvalidMessage()
		{
			messageMock.Reset();
			messageMock.Setup(m => m.DataProvider).Returns((IEXPREJ)null);
			AssertNoExceptionThrown(() => ProcessMessage(message));
		}

		public void TestValidMessage()
		{
			ProcessMessage(message);
			CombineAssertions(() =>
			{
				AssertEquals("EM_Status", Enterprise.Messaging.Integration.EDIMessageStatusList.Codes.ProcessedOK, message.EM_Status);
				AssertEquals("EntryHeaderStatusDescription", "Anmeldung nicht angenommen", entryHeader.EntryHeaderStatusDescription);
				AssertEquals("CH_EntryStatus", "515", entryHeader.CH_EntryStatus);
				AssertEquals("MovementReferenceNumber", "12DE123456789012E0", entryHeader.MovementReferenceNumber);
				var reference = entryHeader.Declaration.JE_DeclarationReference;
				var email = Env.OutgoingCustomsMailManager.EmailsCreated.Single();
				var lrn = entryHeader.LocalReferenceNumber;
				var subject = $"AES EXP Rejection Message Response for {reference} - LRN: {lrn}";
				var titleSubject = $"AES EXP Rejection Message Response for {reference}";
				var bodyMessageTitle = $"<title>{titleSubject}</title>";
				var bodyMessageHeader = $@"<strong>AES EXP Rejection Message Response for <a href=""edient:Command=ShowEditForm&LicenceCode={GlbCompany.CurrentCompany.LicenceKeyIdentifier}&ControllerID=JobDeclaration&BusinessEntityPK=" + entryHeader.Declaration.PK;
				var bodyMessageSummary = $@"Your Export Declaration Message for {reference} has a Rejection Message. For details please follow the Link to the Job<br /><br />";
				var bodyMessageTable = @"<table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table"">"
						+ "<tr><td>MRN:</td><td>12DE123456789012E0</td></tr>"
						+ "<tr><td>Status:</td><td>515</td></tr>"
						+ "<tr><td>Status Text:</td><td>Anmeldung nicht angenommen</td></tr>"
						+ "</table>";
				AssertEmailForSingleRecipientWithTable(ZString.Empty, email, "Dummy@dummy.com", subject, bodyMessageTitle, bodyMessageHeader, bodyMessageSummary, bodyMessageTable);
			});
		}

		public void TestPopulateLogbookRegNum()
		{
			ProcessMessage(message);
			AssertEquals("12DE123456789012E0", message.GetLogbookRegistrationNumber());
		}

		public void TestPopulateLogbookLocalReferenceNumber()
		{
			ProcessMessage(message);
			AssertEquals("Reference1234567abcd", message.GetLogbookLocalReferenceNumber());
		}

		public void TestAddCustomsEntryStatusLog()
		{
			ProcessMessage(message);
			AssertEquals("515", entryHeader.GetCustomsEntryStatusEventReference());
		}

		protected override ZString MessageFriendlyName => "Export EXPREJ Message Processor";

		protected override DEBranchCustomsApplicationTypeMessageProcessor<AesInboundEDIMessage<IEXPREJ>> Processor => new ExportEXPREJMessageProcessor(logger);

		public string DeclarationMessageType => Common.Shared.SharedJobMessageTypeList.Codes.Export;

		EDIMessage ITestEntryLinesLockedAfterProcessing.PrepareMessagesAndGetMessageToProcessForEntryLinesLockedTest(CusEntryHeader entryHeader)
		{
			var outgoingMessage = CreateOriginalMessageLinkedToParent<AesEDIMessage>(entryHeader, "1122334455");
			outgoingMessage.EM_ApplicationCode = "DEE";
			outgoingMessage.EM_MessageType = Messaging.EDIMessageTypeList.Codes.AES;
			outgoingMessage.EM_MessageSubType = MessageTypeList.Codes.Export;
			outgoingMessage.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			entryHeader.Messages.Add(outgoingMessage);
			dataProviderMock.Setup(x => x.ReferencedMessageIdentifier).Returns("1122334455");
			Factory.Save();

			return message;
		}

		protected override void SetUp()
		{
			base.SetUp();

			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ExportCustomsStatus, "Customs Status Export AES-EXP");
			helper.CreateCusCodeList(Core.Constants.CountryCodes.Germany, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ExportCustomsStatus, "515", "Anmeldung nicht angenommen", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);

			var staff = Factory.LoadFromNaturalKey<GlbStaff>(GlbStaffSchema.GS_Code, GlbStaff.CurrentUser.GS_Code);
			staff.GS_EmailAddress = "Dummy@dummy.com";

			var groupPK = EnvProxy.Instance.Registry.PostMasterGroup;
			var group = Factory.Load<GlbGroup>(groupPK);
			group.Staff.Add(staff);

			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Export;
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
			entryHeader = (CusEntryHeader)declaration.ActiveEntryHeaders.AddNew();
			entryHeader.CH_BGMReference = "ATB150000620520195875";

			outgoingMessage = CreateOriginalMessageLinkedToParent<AesEDIMessage>(entryHeader, "EXPREJ5875");
			outgoingMessage.EM_SystemCreateUser = staff.GS_Code;

			dataProviderMock = new Mock<IEXPREJ>();
			dataProviderMock.Setup(m => m.MessageIdentifier).Returns("1234567890");
			dataProviderMock.Setup(m => m.ReferencedMessageIdentifier).Returns("EXPREJ5875");
			dataProviderMock.Setup(m => m.ExportStatus).Returns("515");
			dataProviderMock.Setup(m => m.MovementReferenceNumber).Returns("12DE123456789012E0");
			dataProviderMock.Setup(m => m.LocalReferenceNumber).Returns("Reference1234567abcd");

			messageMock = Factory.NewMoq<AesInboundEDIMessage<IEXPREJ>>();
			messageMock.Setup(m => m.DataProvider).Returns(dataProviderMock.Object);
			message = messageMock.Object;
			Factory.Save();
		}
		CusEntryHeader entryHeader;
		Mock<IEXPREJ> dataProviderMock;
		Mock<AesInboundEDIMessage<IEXPREJ>> messageMock;
		EDIMessage outgoingMessage;
		AesInboundEDIMessage<IEXPREJ> message;
	}
}
