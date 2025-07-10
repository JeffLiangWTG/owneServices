using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.DE.Business.Declaration;
using Enterprise.Customs.DE.Business.Testing;
using Enterprise.Customs.DE.Messaging;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.DE.Business.Import.Testing
{
	[TestedType(typeof(ImportECWINFMessageProcessor))]
	sealed class ImportECWINFMessageProcessorTest : MessageProcessorAbstractTest<ImportECWINFMessageProcessor, AtlasInboundEDIMessage<IECWINF>>
	{
		public void TestGetLinkedObject()
		{
			ProcessMessage(message);
			AssertSame(entryHeader, message.EM_LinkedObject);
		}

		public void TestGetLinkedObject_MRN()
		{
			var mrnEntryNumber = CusEntryNumber.LoadOrCreate(entryHeader, CusEntryNumberTypes.Standard.MovementReferenceNumber, Core.Constants.CountryCodes.Germany);
			mrnEntryNumber.CE_EntryNum = "MRN123456789";
			dataProviderMock.Setup(x => x.MRN).Returns("MRN123456789");

			ProcessMessage(message);

			AssertSame(entryHeader, message.EM_LinkedObject);
		}

		public void TestNoLinkedObject()
		{
			dataProviderMock.Setup(m => m.ReferenceNumber).Returns("ATB0000000000000");

			ProcessMessage(message);
			CombineAssertions(() =>
			{
				AssertNull("No LinkedObject", message.EM_LinkedObject);
				AssertEquals("EM_Status", Enterprise.Messaging.Integration.EDIMessageStatusList.Codes.Error, message.EM_Status);
				AssertEquals("CH_EntryStatus", ZString.Empty, entryHeader.CH_EntryStatus);
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
			AssertEquals("ATC996151771020016389, ATH9456789456789789, ATH9456789456789780", message.GetLogbookRegistrationNumber());
			AssertContainsExactElementsInAnyOrder(new[] { "ATC996151771020016389", "ATH9456789456789789", "ATH9456789456789780" }, message.GetLogbookRegistrationNumbers());
		}

		public void TestPopulateLogbookRegNum_MRN()
		{
			dataProviderMock.Setup(x => x.MRN).Returns("MRN123456789");
			var goodsItem1Mock = Mock.Get(dataProviderMock.Object.GoodsItems.First());
			goodsItem1Mock.Setup(m => m.MRN).Returns("MRN98765");
			var goodsItem2Mock = Mock.Get(dataProviderMock.Object.GoodsItems.ElementAt(1));
			goodsItem2Mock.Setup(m => m.MRN).Returns("MRN4321");

			ProcessMessage(message);

			AssertContainsExactElementsInAnyOrder(new[] { "ATC996151771020016389", "MRN123456789", "ATH9456789456789789", "ATH9456789456789780", "MRN98765", "MRN4321" }, message.GetLogbookRegistrationNumbers());
		}

		public void TestLocalReferenceNumberInLogbook()
		{
			ProcessMessage(message);
			AssertEquals("LogbookLocalReferenceNumber", "WTG1234", message.GetLogbookLocalReferenceNumber());
		}

		public void TestEMailNotification()
		{
			var user = Factory.NewWithValidTestData<GlbStaff>();
			user.GS_EmailAddress = "test@mail.com";
			outgoingMessage.EM_SystemCreateUser = user.GS_Code;

			dataProviderMock.Setup(x => x.MRN).Returns("MRN123456789");

			ProcessMessage(message);

			var declaration = entryHeader.Declaration;
			var reference = declaration.JE_DeclarationReference;
			var email = Env.OutgoingCustomsMailManager.EmailsCreated.FirstOrDefault(x => x.Subject.Contains(reference));

			var subject = $"Import ECWINF – Bonded Warehouse Completion Information Response for {reference}";
			var bodyMessageTitle = $"<title>{subject}</title>";
			var bodyMessageHeader = $@"<strong>Import ECWINF – Bonded Warehouse Completion Information Response for <a href=""edient:Command=ShowEditForm&LicenceCode={GlbCompany.CurrentCompany.LicenceKeyIdentifier}&ControllerID=JobDeclaration&BusinessEntityPK=" + declaration.PK;
			var bodyMessageSummary = $@"Your Import Declaration for Job {reference} received a Bonded Warehouse Completion Information. For details please follow the link to the job.";
			var bodyMessageTable = @"<table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table"">"
									+ "<tr><td>Additional Registration Number</td><td>ATC996151771020016389</td></tr>"
									+ "<tr><td>MRN</td><td>MRN123456789</td></tr>"
									+ "<tr><td>Local Reference Number</td><td>WTG1234</td></tr>"
									+ "</table>";
			AssertEmailForSingleRecipientWithTable(ZString.Empty, email, "test@mail.com", subject, bodyMessageTitle, bodyMessageHeader, bodyMessageSummary, bodyMessageTable);
		}

		public void TestGenerateEmail_EmptyFieldsNotShown()
		{
			CombineAssertions(() =>
			{
				var user = Factory.NewWithValidTestData<GlbStaff>();
				user.GS_EmailAddress = "test@mail.com";
				outgoingMessage.EM_SystemCreateUser = user.GS_Code;

				ProcessMessage(message);
				var email = Env.OutgoingCustomsMailManager.EmailsCreated.Last();
				AssertNotContains("No MRN", "<td>MRN</td>", email.Body);

				dataProviderMock.Setup(m => m.ReferenceNumber).Returns(ZString.Empty);
				dataProviderMock.Setup(x => x.MRN).Returns("MRN123456789");
				dataProviderMock.Setup(m => m.LocalReferenceNumber).Returns(ZString.Empty);
				entryHeader.MovementReferenceNumberSetter("MRN123456789");
				ProcessMessage(message);
				email = Env.OutgoingCustomsMailManager.EmailsCreated.Last();
				AssertNotContains("No ReferenceNumberr", "<td>Additional Registration Number</td>", email.Body);
				AssertNotContains("No LocalReferenceNumber", "<td>Local Reference Number</td>", email.Body);
			});
		}

		protected override ZString MessageFriendlyName => "Import ECWINF Message Processor";

		protected override DEBranchCustomsApplicationTypeMessageProcessor<AtlasInboundEDIMessage<IECWINF>> Processor => new ImportECWINFMessageProcessor(logger);

		protected override void SetUp()
		{
			base.SetUp();

			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
			entryHeader = (CusEntryHeader)declaration.ActiveEntryHeaders.AddNew();
			var mrnEntryNumber = CusEntryNumber.LoadOrCreate(entryHeader, CusEntryNumberTypes.Standard.MovementReferenceNumber, Core.Constants.CountryCodes.Germany);
			mrnEntryNumber.CE_EntryNum = "ATC996151771020016389";
			entryLine = entryHeader.AllEntryLines.AddNew();
			entryLine.CL_LineNumber = 1;

			outgoingMessage = CreateOriginalMessageLinkedToParent<AtlasEDIMessage>(entryHeader, "outgoing");
			outgoingMessage.EM_ApplicationCode = EDIMessage.ApplicationCodes.DECustomsAtlasSystem;
			outgoingMessage.EM_MessageType = EDIMessageTypeList.Codes.Import;
			outgoingMessage.EM_MessageSubType = ImportMessageSubTypeList.Codes.FreeCirculationSingleDeclaration;
			outgoingMessage.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;

			var goodsItem = GetGoodsItem("ATH9456789456789789");
			var goodsItem2 = GetGoodsItem("ATH9456789456789789");
			var goodsItem3 = GetGoodsItem("ATH9456789456789780");

			dataProviderMock = new Mock<IECWINF>();
			dataProviderMock.Setup(x => x.MessageIdentifier).Returns("FINTAX20833294803129238170736212505");
			dataProviderMock.Setup(x => x.ReferenceNumber).Returns("ATC996151771020016389");
			dataProviderMock.Setup(x => x.LocalReferenceNumber).Returns("WTG1234");
			dataProviderMock.Setup(m => m.GoodsItems).Returns(new[] { goodsItem.Object, goodsItem3.Object, goodsItem3.Object });

			messageMock = Factory.NewMoq<AtlasInboundEDIMessage<IECWINF>>();
			messageMock.Setup(m => m.DataProvider).Returns(dataProviderMock.Object);
			message = messageMock.Object;

			Factory.Save();
		}
		CusEntryHeader entryHeader;
		CusEntryLine entryLine;
		Mock<IECWINF> dataProviderMock;
		Mock<AtlasInboundEDIMessage<IECWINF>> messageMock;
		AtlasInboundEDIMessage<IECWINF> message;
		EDIMessage outgoingMessage;

		Mock<IECWINFGoodsItem> GetGoodsItem(ZString registrationNumber)
		{
			var goodsItem = new Mock<IECWINFGoodsItem>();
			goodsItem.Setup(x => x.ReferencedRegistrationNumber).Returns(registrationNumber);

			return goodsItem;
		}
	}
}
