using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.DE.Business;
using Enterprise.Customs.DE.Business.Testing;
using Enterprise.Customs.DE.Messaging;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Messaging.Business;
using Moq;

namespace Enterprise.Customs.DE.NCTS.Business.Testing
{
	sealed class NctsECWINFMessageProcessorTest : MessageProcessorAbstractTest<NctsECWINFMessageProcessor, AtlasInboundEDIMessage<IECWINF>>
	{
		public void TestDocumentsAttachedAsEDocs()
		{
			var eDocsSupporter = nctsHeader as IDocManagerSupportBase;
			AssertEquals("PreReq", 0, eDocsSupporter.DocManagerInfo().AllEDocs.Count);

			messageMock.Setup(x => x.AttachedDocuments).Returns(SampleAttachedDocument);
			ProcessMessage(message);
			AssertEquals("eDoc attached", 1, eDocsSupporter.DocManagerInfo().AllEDocs.Count);
		}

		public void TestGetLinkedObject()
		{
			ProcessMessage(message);
			CombineAssertions(() =>
			{
				AssertType("Type is NctsDepartureMovementHeader", typeof(NctsDepartureMovementHeader), message.EM_LinkedObject);
				AssertEquals("Correct LinkedObject obtained", nctsHeader.MovementHeader, message.EM_LinkedObject);
			});
		}

		public void TestNoLinkedObject()
		{
			dataProviderMock.Setup(m => m.ReferenceNumber).Returns("ATB0000000000000");

			ProcessMessage(message);
			CombineAssertions(() =>
			{
				AssertNull("No LinkedObject", message.EM_LinkedObject);
				AssertEquals("EM_Status", Enterprise.Messaging.Integration.EDIMessageStatusList.Codes.Error, message.EM_Status);
			});
		}

		public void TestDataProviderNullDueToInvalidMessage()
		{
			messageMock.Setup(m => m.DataProvider).Returns((IECWINF)null);
			AssertNoExceptionThrown(() => ProcessMessage(message));
		}

		public void TestProcessMessage()
		{
			ProcessMessage(message);

			AssertEquals("EM_Status", Enterprise.Messaging.Integration.EDIMessageStatusList.Codes.ProcessedOK, message.EM_Status);
		}

		public void TestPopulateLogbookRegNum()
		{
			ProcessMessage(message);
			AssertEquals("23DE00000000123456, ATH9456789456789789, ATH9456789456789777, ATH9456789456789780", message.GetLogbookRegistrationNumber());
		}

		public void TestLocalReferenceNumberInLogbook()
		{
			ProcessMessage(message);
			AssertEquals("LogbookLocalReferenceNumber", "WTG1234", message.GetLogbookLocalReferenceNumber());
		}

		public void TestEMailNotification_TableWithLocalRefNum_AdditionalRegNum()
		{
			var user = Factory.NewWithValidTestData<GlbStaff>();
			user.GS_EmailAddress = "test@mail.com";
			outgoingMessage.EM_SystemCreateUser = user.GS_Code;

			ProcessMessage(message);

			var reference = nctsHeader.BH_JobReference;
			var email = Env.OutgoingCustomsMailManager.EmailsCreated.FirstOrDefault(x => x.Subject.Contains(reference));

			var subject = $"NCTS ECWINF - Bonded Warehouse Completion Information Response for {reference}";
			var bodyMessageTitle = $"<title>{subject}</title>";
			var bodyMessageHeader = $@"<strong>NCTS ECWINF - Bonded Warehouse Completion Information Response for <a href=""edient:Command=ShowEditForm&LicenceCode={GlbCompany.CurrentCompany.LicenceKeyIdentifier}&ControllerID=NctsMovementController&BusinessEntityPK=" + nctsHeader.PK;
			var bodyMessageSummary = $@"Your NCTS Departure Declaration for Job {reference} received a Bonded Warehouse Completion Information. For details please follow the link to the job.";
			var bodyMessageTable = @"<table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table"">"
								   + "<tr><td>Additional Registration Number</td><td>23DE00000000123456</td></tr>"
								   + "<tr><td>Local Reference Number</td><td>WTG1234</td></tr>"
								   + "</table>";
			AssertEmailForSingleRecipientWithTable(ZString.Empty, email, "test@mail.com", subject, bodyMessageTitle, bodyMessageHeader, bodyMessageSummary, bodyMessageTable);
		}

		public void TestEMailNotification_TableWithAdditionalRegNum()
		{
			var user = Factory.NewWithValidTestData<GlbStaff>();
			user.GS_EmailAddress = "test@mail.com";
			outgoingMessage.EM_SystemCreateUser = user.GS_Code;

			dataProviderMock.Setup(m => m.LocalReferenceNumber).Returns(ZString.Empty);

			ProcessMessage(message);

			var reference = nctsHeader.BH_JobReference;
			var email = Env.OutgoingCustomsMailManager.EmailsCreated.FirstOrDefault(x => x.Subject.Contains(reference));

			var subject = $"NCTS ECWINF - Bonded Warehouse Completion Information Response for {reference}";
			var bodyMessageTitle = $"<title>{subject}</title>";
			var bodyMessageHeader = $@"<strong>NCTS ECWINF - Bonded Warehouse Completion Information Response for <a href=""edient:Command=ShowEditForm&LicenceCode={GlbCompany.CurrentCompany.LicenceKeyIdentifier}&ControllerID=NctsMovementController&BusinessEntityPK=" + nctsHeader.PK;
			var bodyMessageSummary = $@"Your NCTS Departure Declaration for Job {reference} received a Bonded Warehouse Completion Information. For details please follow the link to the job.";
			var bodyMessageTable = @"<table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table"">"
								   + "<tr><td>Additional Registration Number</td><td>23DE00000000123456</td></tr>"
								   + "</table>";
			AssertEmailForSingleRecipientWithTable(ZString.Empty, email, "test@mail.com", subject, bodyMessageTitle, bodyMessageHeader, bodyMessageSummary, bodyMessageTable);
		}

		protected override ZString MessageFriendlyName => "NCTS ECWINF Message Processor";

		protected override DEBranchCustomsApplicationTypeMessageProcessor<AtlasInboundEDIMessage<IECWINF>> Processor => new NctsECWINFMessageProcessor(logger);

		protected override void SetUp()
		{
			base.SetUp();

			nctsHeader = Factory.NewWithValidTestData<NctsHeader>();
			nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
			var mrn = CusEntryNumber.LoadOrCreate(nctsHeader, CusEntryNumberTypes.Standard.MovementReferenceNumber, Core.Constants.CountryCodes.Germany);
			mrn.CE_EntryNum = MovementReferenceNumber;

			outgoingMessage = CreateOriginalMessageLinkedToParent<AtlasEDIMessage>(nctsHeader.MovementHeader, MovementReferenceNumber);
			nctsHeader.MovementHeader.Messages.Add(outgoingMessage);
			outgoingMessage.EM_ApplicationCode = EDIMessage.ApplicationCodes.DECustomsAtlasSystem;
			outgoingMessage.EM_MessageType = EDIMessageTypeList.Codes.Import;
			outgoingMessage.EM_MessageSubType = ImportMessageSubTypeList.Codes.FreeCirculationSingleDeclaration;
			outgoingMessage.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;

			dataProviderMock = new Mock<IECWINF>();
			dataProviderMock.Setup(m => m.ReferenceNumber).Returns(MovementReferenceNumber);
			dataProviderMock.Setup(m => m.LocalReferenceNumber).Returns("WTG1234");

			var goodsItem = GetGoodsItem("ATH9456789456789789");
			var goodsItem2 = GetGoodsItem("ATH9456789456789777");
			var goodsItem3 = GetGoodsItem("ATH9456789456789780");

			dataProviderMock = new Mock<IECWINF>();
			dataProviderMock.Setup(x => x.MessageIdentifier).Returns("FINTAX20833294803129238170736212505");
			dataProviderMock.Setup(x => x.ReferenceNumber).Returns(MovementReferenceNumber);
			dataProviderMock.Setup(x => x.LocalReferenceNumber).Returns("WTG1234");
			dataProviderMock.Setup(m => m.GoodsItems).Returns(new[] { goodsItem.Object, goodsItem2.Object, goodsItem3.Object });

			messageMock = Factory.NewMoq<AtlasInboundEDIMessage<IECWINF>>();
			messageMock.Setup(m => m.DataProvider).Returns(dataProviderMock.Object);
			message = messageMock.Object;

			Factory.Save();
		}

		Mock<IECWINF> dataProviderMock;
		Mock<AtlasInboundEDIMessage<IECWINF>> messageMock;
		AtlasInboundEDIMessage<IECWINF> message;
		EDIMessage outgoingMessage;

		NctsHeader nctsHeader;

		const string MovementReferenceNumber = "23DE00000000123456";

		Mock<IECWINFGoodsItem> GetGoodsItem(ZString registrationNumber)
		{
			var goodsItem = new Mock<IECWINFGoodsItem>();
			goodsItem.Setup(x => x.ReferencedRegistrationNumber).Returns(registrationNumber);

			return goodsItem;
		}
	}
}
