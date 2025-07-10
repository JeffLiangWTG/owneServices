using System;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.DE.Business.Testing;
using Enterprise.Customs.DE.Messaging;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.DE.Business.CusTempStorage.Testing
{
	[TestedType(typeof(TemporaryStorageCUSSTPMessageProcessor))]
	sealed class TemporaryStorageCUSSTPMessageProcessorTest : MessageProcessorAbstractTest<TemporaryStorageCUSSTPMessageProcessor, AtlasInboundEDIMessage<ICUSSTP>>
	{
		public void TestGetLinkedObject()
		{
			dataProviderMock.Setup(m => m.GoodsItems).Returns(Array.Empty<ICUSSTPGoodsItem>());
			ProcessMessage(message);
			AssertEquals(regHeader, message.EM_LinkedObject);
		}

		public void TestGetLinkedObject_MRN()
		{
			dataProviderMock.Setup(x => x.ReferenceNumber).Returns(ZString.Empty);
			dataProviderMock.Setup(m => m.GoodsItems).Returns(Array.Empty<ICUSSTPGoodsItem>());
			regHeader.SRH_Reference = MRN;

			Factory.Save();

			ProcessMessage(message);
			AssertSame(regHeader, message.EM_LinkedObject);
		}

		public void TestGetLinkedObject_ReferenceNumber()
		{
			dataProviderMock.Setup(x => x.MRN).Returns((string)null);
			dataProviderMock.Setup(m => m.GoodsItems).Returns(Array.Empty<ICUSSTPGoodsItem>());
			regHeader.SRH_Reference = ReferenceNumber;

			Factory.Save();

			ProcessMessage(message);
			AssertSame(regHeader, message.EM_LinkedObject);
		}

		public void TestDataProviderNullDueToInvalidMessage()
		{
			messageMock.Setup(m => m.DataProvider).Returns((ICUSSTP)null);
			AssertNoExceptionThrown(() => ProcessMessage(message));
		}

		public void TestRegisterLineNotFound()
		{
			SetupCUSSTPGoodsItems("0000", "0000");
			ProcessMessage(message);

			var stmNote = GetStmNote(message);
			CombineAssertions(() =>
			{
				AssertEquals("EM_Status", EDIMessage.Status.ProcessedOK, message.EM_Status);
				AssertEquals("Register Record Note",
					"This is the list of Reference Number and Sequence Number system could not locate matching Register records for:\r\nReference Number: AT/B/15/000062/05/2019/5875; Sequence Number: 1\r\nReference Number: AT/B/15/000062/05/2019/5875; Sequence Number: 2",
					stmNote.ST_NoteText);
			});
		}

		public void TestRegisterHeaderAndLinesAllStatuses()
		{
			var goodsItemDataProvider1Mock = new Mock<ICUSSTPGoodsItem>();
			goodsItemDataProvider1Mock.Setup(m => m.SequenceNumber).Returns("1");
			goodsItemDataProvider1Mock.Setup(m => m.CustomsGoodsStatus).Returns("0000");

			var goodsItemDataProvider2Mock = new Mock<ICUSSTPGoodsItem>();
			goodsItemDataProvider2Mock.Setup(m => m.SequenceNumber).Returns("2");
			goodsItemDataProvider2Mock.Setup(m => m.CustomsGoodsStatus).Returns("0100");

			var goodsItemDataProvider3Mock = new Mock<ICUSSTPGoodsItem>();
			goodsItemDataProvider3Mock.Setup(m => m.SequenceNumber).Returns("3");
			goodsItemDataProvider3Mock.Setup(m => m.CustomsGoodsStatus).Returns("0001");

			var goodsItemDataProvider4Mock = new Mock<ICUSSTPGoodsItem>();
			goodsItemDataProvider4Mock.Setup(m => m.SequenceNumber).Returns("4");
			goodsItemDataProvider4Mock.Setup(m => m.CustomsGoodsStatus).Returns("0101");

			var goodsItemDataProvider5Mock = new Mock<ICUSSTPGoodsItem>();
			goodsItemDataProvider5Mock.Setup(m => m.SequenceNumber).Returns("5");
			goodsItemDataProvider5Mock.Setup(m => m.CustomsGoodsStatus).Returns("0102");

			var goodsItemDataProvider6Mock = new Mock<ICUSSTPGoodsItem>();
			goodsItemDataProvider6Mock.Setup(m => m.SequenceNumber).Returns("6");
			goodsItemDataProvider6Mock.Setup(m => m.CustomsGoodsStatus).Returns("0104");

			var goodsItemDataProvider7Mock = new Mock<ICUSSTPGoodsItem>();
			goodsItemDataProvider7Mock.Setup(m => m.SequenceNumber).Returns("7");
			goodsItemDataProvider7Mock.Setup(m => m.CustomsGoodsStatus).Returns("0105");

			dataProviderMock.Setup(m => m.GoodsItems).Returns(new ICUSSTPGoodsItem[] { goodsItemDataProvider1Mock.Object, goodsItemDataProvider2Mock.Object, goodsItemDataProvider3Mock.Object, goodsItemDataProvider4Mock.Object, goodsItemDataProvider5Mock.Object, goodsItemDataProvider6Mock.Object, goodsItemDataProvider7Mock.Object });

			var regLine1 = regHeader.CusTempStorageRegLines.AddNew();
			regLine1.SRL_LineNumber = 1;
			var regLine2 = regHeader.CusTempStorageRegLines.AddNew();
			regLine2.SRL_LineNumber = 2;
			var regLine3 = regHeader.CusTempStorageRegLines.AddNew();
			regLine3.SRL_LineNumber = 3;
			var regLine4 = regHeader.CusTempStorageRegLines.AddNew();
			regLine4.SRL_LineNumber = 4;
			var regLine5 = regHeader.CusTempStorageRegLines.AddNew();
			regLine5.SRL_LineNumber = 5;
			var regLine6 = regHeader.CusTempStorageRegLines.AddNew();
			regLine6.SRL_LineNumber = 6;
			var regLine7 = regHeader.CusTempStorageRegLines.AddNew();
			regLine7.SRL_LineNumber = 7;

			ProcessMessage(message);
			CombineAssertions(() =>
			{
				AssertRegLine("Line 1", regLine1, CustomsStatusList.Codes.TST);
				AssertRegLineTransaction("Line 1 Transaction", TempStorageTestHelpers.GetRegLineTransaction(regLine1, TransactionTypes.Codes.StatusChange), CustomsStatusList.Codes.TST);
				AssertRegLine("Line 2", regLine2, CustomsStatusList.Codes.TST);
				AssertRegLineTransaction("Line 2 Transaction", TempStorageTestHelpers.GetRegLineTransaction(regLine2, TransactionTypes.Codes.StatusChange), CustomsStatusList.Codes.TST);
				AssertRegLine("Line 3", regLine3, CustomsStatusList.Codes.LCK);
				AssertRegLineTransaction("Line 3 Transaction", TempStorageTestHelpers.GetRegLineTransaction(regLine3, TransactionTypes.Codes.StatusChange), CustomsStatusList.Codes.LCK);
				AssertRegLine("Line 4", regLine4, CustomsStatusList.Codes.LCK);
				AssertRegLineTransaction("Line 4 Transaction", TempStorageTestHelpers.GetRegLineTransaction(regLine4, TransactionTypes.Codes.StatusChange), CustomsStatusList.Codes.LCK);
				AssertRegLine("Line 5", regLine5, CustomsStatusList.Codes.LCK);
				AssertRegLineTransaction("Line 5 Transaction", TempStorageTestHelpers.GetRegLineTransaction(regLine5, TransactionTypes.Codes.StatusChange), CustomsStatusList.Codes.LCK);
				AssertRegLine("Line 6", regLine6, CustomsStatusList.Codes.LCK);
				AssertRegLineTransaction("Line 6  Transaction", TempStorageTestHelpers.GetRegLineTransaction(regLine6, TransactionTypes.Codes.StatusChange), CustomsStatusList.Codes.LCK);
				AssertRegLine("Line 7", regLine7, CustomsStatusList.Codes.LCK);
				AssertRegLineTransaction("Line 7 Transaction", TempStorageTestHelpers.GetRegLineTransaction(regLine7, TransactionTypes.Codes.StatusChange), CustomsStatusList.Codes.LCK);
			});
		}

		public void TestUnknownCustomsInterventionCode()
		{
			var goodsItemDataProviderMock = new Mock<ICUSSTPGoodsItem>();
			goodsItemDataProviderMock.Setup(m => m.SequenceNumber).Returns("1");
			goodsItemDataProviderMock.Setup(m => m.CustomsGoodsStatus).Returns("0089");
			dataProviderMock.Setup(m => m.GoodsItems).Returns(new ICUSSTPGoodsItem[] { goodsItemDataProviderMock.Object });

			var regLine1 = regHeader.CusTempStorageRegLines.AddNew();
			regLine1.SRL_LineNumber = 1;

			ProcessMessage(message);
			AssertEquals(true, logger.DebugLogStrings.Cast<string>().Any(x => x.Contains($"We get an unknown Customs Intervention Code '0089' in the message with PK '{message.PK}'.")));
		}

		public void TestPopulateLogbookRegNum()
		{
			dataProviderMock.Setup(m => m.GoodsItems).Returns(Array.Empty<ICUSSTPGoodsItem>());
			ProcessMessage(message);
			AssertContainsExactElementsInAnyOrder(new[] { ReferenceNumber, MRN }, message.GetLogbookRegistrationNumbers());
		}

		public void TestSendUnsolicitedMessageEMail_NominatedGroup()
		{
			var groupNotificationRegistryItem = new EU.Registry.TemporaryStorageUnsolicitedGroupNotification(Core.Constants.EmailTo.NominatedGroup, TempStorageTestHelpers.CreateGlbGroup(Factory).PK);
			using (EU.Registry.EUCustomsDataRegistry.Instance.SendTemporaryStorageUnsolicited.SetTemporaryValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, groupNotificationRegistryItem))
			{
				dataProviderMock.Setup(m => m.GoodsItems).Returns(Array.Empty<ICUSSTPGoodsItem>());
				ProcessMessage(message);
				var email = Env.OutgoingCustomsMailManager.EmailsCreated.Single();
				var reference = regHeader.SRH_Reference;
				var subject = $"SumA CUSSTP – Announcement of a Control Response for {reference}";
				var bodyMessageTitle = $"<title>{subject}</title>";
				var bodyMessageHeader = $@"<strong>SumA CUSSTP – Announcement of a Control Response for <a href=""edient:Command=ShowEditForm&LicenceCode={GlbCompany.CurrentCompany.LicenceKeyIdentifier}&ControllerID=DESumARegister&BusinessEntityPK={regHeader.PK}";
				var bodyMessageSummary = $"Your SumA Register {reference} received an Announcement of a Control. For details please follow the link to the register.";
				var bodyMessageTable = @"<table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table"">"
									   + $"<tr><td>MRN</td><td>{MRN}</td></tr>"
									   + $"<tr><td>Registration Number</td><td>{ReferenceNumber}</td></tr>"
									   + $"<tr><td>Local Reference Number</td><td>{LocalReferenceNumber}</td></tr>"
									   + $"<tr><td>Notification Date/Time</td><td>{notificationDateTime}</td></tr>"
									   + "</table>";
				CombineAssertions(() => AssertEmailWithTable(ZString.Empty, email, new ZString[] { "staff1@group-suma.com", "staff2@group-suma.com" }, subject, bodyMessageTitle, bodyMessageHeader, bodyMessageSummary, bodyMessageTable));
			}
		}

		protected override ZString MessageFriendlyName => "Temporary Storage CUSSTP Message Processor";

		protected override DEBranchCustomsApplicationTypeMessageProcessor<AtlasInboundEDIMessage<ICUSSTP>> Processor => new TemporaryStorageCUSSTPMessageProcessor(logger);

		protected override bool ExpectedMustHaveLinkedObject => true;

		protected override void SetUp()
		{
			base.SetUp();

			regHeader = Factory.NewWithValidTestData<CusTempStorageRegHeader>();
			regHeader.SRH_Reference = ReferenceNumber;

			dataProviderMock = new Mock<ICUSSTP>();
			dataProviderMock.Setup(m => m.MessageIdentifier).Returns(MessageIdentifier);
			dataProviderMock.Setup(m => m.ReferenceNumber).Returns(ReferenceNumber);
			dataProviderMock.Setup(m => m.LocalReferenceNumber).Returns(LocalReferenceNumber);
			dataProviderMock.Setup(m => m.MRN).Returns(MRN);
			dataProviderMock.Setup(m => m.NotificationDateTime).Returns(notificationDateTime);

			messageMock = Factory.NewMoq<AtlasInboundEDIMessage<ICUSSTP>>();
			messageMock.Setup(m => m.DataProvider).Returns(dataProviderMock.Object);
			message = messageMock.Object;
			Factory.Save();
		}

		void SetupCUSSTPGoodsItems(ZString goodsItem1CustomsInterventionCode, ZString goodsItem2CustomsInterventionCode)
		{
			var goodsItemDataProvider1Mock = new Mock<ICUSSTPGoodsItem>();
			goodsItemDataProvider1Mock.Setup(m => m.SequenceNumber).Returns("1");
			goodsItemDataProvider1Mock.Setup(m => m.CustomsGoodsStatus).Returns(goodsItem1CustomsInterventionCode);

			var goodsItemDataProvider2Mock = new Mock<ICUSSTPGoodsItem>();
			goodsItemDataProvider2Mock.Setup(m => m.SequenceNumber).Returns("2");
			goodsItemDataProvider2Mock.Setup(m => m.CustomsGoodsStatus).Returns(goodsItem2CustomsInterventionCode);

			dataProviderMock.Setup(m => m.GoodsItems).Returns(new[] { goodsItemDataProvider1Mock.Object, goodsItemDataProvider2Mock.Object });
			Factory.Save();
		}
		CusTempStorageRegHeader regHeader;
		Mock<ICUSSTP> dataProviderMock;
		Mock<AtlasInboundEDIMessage<ICUSSTP>> messageMock;
		AtlasInboundEDIMessage<ICUSSTP> message;

		const string MessageIdentifier = "CUSSTP58750000000380673030419115545";
		const string ReferenceNumber = "ATB150000620520195875";
		const string LocalReferenceNumber = "19DE587500026775M6";
		const string MRN = "23DE586601055987B7";
		readonly ZDateTime notificationDateTime = new ZDateTime(2022, 11, 08, 10, 11, 00);

		void AssertRegLine(ZString testCase, CusTempStorageRegLine line, ZString expectedCustomsStatus)
		{
			AssertEquals(testCase + "->SRL_CustomsStatus", expectedCustomsStatus, line.SRL_CustomsStatus);
		}

		void AssertRegLineTransaction(ZString testCase, CusTempStorageRegLineTransaction regLineTransaction, ZString customsStatus)
		{
			AssertEquals(testCase + "->SRT_PackageQty", ZInt.Zero, regLineTransaction.SRT_PackageQty);
			AssertEquals(testCase + "->SRT_GrossWeight", ZDecimal.Zero, regLineTransaction.SRT_GrossWeight);
			AssertEquals(testCase + "->SRT_ReferenceType", ZString.Empty, regLineTransaction.SRT_ReferenceType);
			AssertEquals(testCase + "->SRT_Reference", ZString.Empty, regLineTransaction.SRT_Reference);
			AssertEquals(testCase + "->SRT_TransactionType", TransactionTypes.Codes.StatusChange, regLineTransaction.SRT_TransactionType);
			AssertEquals(testCase + "->SRT_InternalReferenceNumber", MessageIdentifier, regLineTransaction.SRT_InternalReferenceNumber);
			AssertEquals(testCase + "->SRT_Comments", "Status Changed to:" + customsStatus, regLineTransaction.SRT_Comments);
		}
	}
}
