using System.Linq;
using CargoWise.Customs.DE.MessageContracts.NCTS;
using CargoWise.Types;
using Enterprise.Customs.DE.Business;
using Enterprise.Customs.DE.Business.Testing;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.EU.NCTS.Business.CodeDescriptionPairLists;
using Enterprise.Customs.Universal.Testing;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.DE.NCTS.Business.Testing
{
	[TestedType(typeof(NctsTBESTAMessageProcessor))]
	sealed class NctsTBESTAMessageProcessorTest : MessageProcessorAbstractTest<NctsTBESTAMessageProcessor, AtlasInboundEDIMessage<ITBESTA>>
	{
		public void TestLinkedObjectNotFound()
		{
			outgoingMessage.EM_MessageNum = "NOTORIGINALMSG";

			ProcessMessage(message);
			AssertEquals(EDIMessage.Status.Error, message.EM_Status);
		}

		public void TestGetLinkedObjectFromOriginalMessage()
		{
			ProcessMessage(message);
			AssertEquals(nctsHeader, message.EM_LinkedObject);
		}

		public void TestDataProviderNullDueToInvalidMessage()
		{
			messageMock.Setup(x => x.DataProvider).Returns((ITBESTA)null);
			AssertNoExceptionThrown(() => ProcessMessage(message));
			messageMock.Verify(x => x.DataProvider);
		}

		public void TestValidMessageIsSuccessfull()
		{
			nctsHeader.MovementReferenceEntryNumber.CE_EntryNum = "19DE265655002905M6";

			var staff = Factory.New<GlbStaff>();
			staff.GS_Code = "BOB";
			staff.GS_FullName = "BOB THE BUILDER";
			staff.GS_EmailAddress = "bob@thebuilder.com";

			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Germany, "DE", helper.CreateNewOrGetExistingDataGrouping("EUN", "EUN"));
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.NctsCustomsStatus, "Customs Status NCTS Declaration");
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Germany, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.NctsCustomsStatus, "56", "Beendigung abgeschlossen", ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(2));
			Factory.Save();

			outgoingMessage.EM_SystemCreateUser = staff.GS_Code;
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			ProcessMessage(message);
			CombineAssertions(() =>
			{
				AssertEquals("Expected MessageStatus = MAS", NctsMessageStatusList.Codes.ArrivalNotificationSent, nctsHeader.EffectiveMessageStatus);
				AssertEquals("Expected EM_MessageNum", MessageIdentifier, message.EM_MessageNum);
				AssertEquals("Expected ArrivalStatus", NCTS5ArrivalCustomsStatusList.Codes.ClosedFullRelease, nctsHeader.ArrivalMovementHeader.BM_CustomsStatus);
				var reference = nctsHeader.BH_JobReference;
				var subject = $"NCTS TBE Status Message Response for {reference}";
				var bodyMessageTitle = $"<title>{subject}</title>";
				var bodyMessageHeader = $@"<strong>NCTS TBE Status Message Response for <a href=""edient:Command=ShowEditForm&LicenceCode=EDIEDIDAT&ControllerID=NctsMovementController&BusinessEntityPK={nctsHeader.PK}";
				var bodyMessageSummary = $@"Your Declaration {reference} has a Status Message. For details please follow the Link to the Job.";
				var bodyMessageTable = @"<table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table"">"
					+ "<tr><td>MRN</td><td>19DE265655002905M6</td></tr>"
					+ "<tr><td>Status</td><td>56</td></tr>"
					+ "<tr><td>Status Text</td><td>Beendigung abgeschlossen</td></tr>"
					+ "</table>";
				var email = Env.OutgoingCustomsMailManager.EmailsCreated.FirstOrDefault();
				AssertEmailForSingleRecipientWithTable(ZString.Empty, email, "bob@thebuilder.com", subject, bodyMessageTitle, bodyMessageHeader, bodyMessageSummary, bodyMessageTable);
			});
		}

		protected override ZString MessageFriendlyName => "NCTS TBESTA Message Processor";

		protected override DEBranchCustomsApplicationTypeMessageProcessor<AtlasInboundEDIMessage<ITBESTA>> Processor => new NctsTBESTAMessageProcessor(logger);

		protected override void SetUp()
		{
			base.SetUp();

			nctsHeader = Factory.NewWithValidTestData<NctsHeader>();
			nctsHeader.SetMovementType(NctsMovementType.Codes.Arrival);
			outgoingMessage = CreateOriginalMessageLinkedToParent<AtlasEDIMessage>(nctsHeader, ReferencedMessageIdentifier);

			dataProviderMock = new Mock<ITBESTA>();
			dataProviderMock.Setup(m => m.ReferencedMessageIdentifier).Returns(ReferencedMessageIdentifier);
			dataProviderMock.Setup(m => m.MessageIdentifier).Returns(MessageIdentifier);
			dataProviderMock.Setup(m => m.DestinationStatus).Returns("56");
			dataProviderMock.Setup(m => m.Reason).Returns("Reason");

			messageMock = Factory.NewMoq<AtlasInboundEDIMessage<ITBESTA>>();
			messageMock.Setup(m => m.DataProvider).Returns(dataProviderMock.Object);
			message = messageMock.Object;

			Factory.Save();
		}
		Mock<ITBESTA> dataProviderMock;
		Mock<AtlasInboundEDIMessage<ITBESTA>> messageMock;
		AtlasInboundEDIMessage<ITBESTA> message;
		NctsHeader nctsHeader;
		EDIMessage outgoingMessage;
		const string MessageIdentifier = "CUSSTA58750000000381119050419125839";
		const string ReferencedMessageIdentifier = "DE441715100000000000000000000477553";
	}
}
