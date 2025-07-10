using System;
using System.Collections.Generic;
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
	[TestedType(typeof(TemporaryStorageCUSCANMessageProcessor))]
	sealed class TemporaryStorageCUSCANMessageProcessorTest : MessageProcessorAbstractTest<TemporaryStorageCUSCANMessageProcessor, AtlasInboundEDIMessage<ICUSCAN>>
	{
		public void TestRegisterLineNotFound()
		{
			SetupCUSCANGoodsItems("05", "05");
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

		public void TestDataProviderNullDueToInvalidMessage()
		{
			messageMock.Setup(x => x.DataProvider).Returns(() => null);
			AssertNoExceptionThrown(() => ProcessMessage(message));
		}

		public void TestGetLinkedObjectFromReferenceNumber()
		{
			dataProviderMock.Setup(m => m.GoodsItems).Returns(Array.Empty<ICUSCANGoodsItem>());
			ProcessMessage(message);
			AssertEquals(regHeader, message.EM_LinkedObject);
		}

		public void TestGetLinkedObjectFromMRN()
		{
			regHeader.SRH_Reference = MRN;
			dataProviderMock.Setup(m => m.GoodsItems).Returns(Array.Empty<ICUSCANGoodsItem>());
			dataProviderMock.Setup(m => m.MRN).Returns(MRN);
			ProcessMessage(message);

			AssertEquals(regHeader, message.EM_LinkedObject);
		}

		public void TestRegisterHeaderAndRegLineTransactionAndLinesStatusFIN()
		{
			var regLine1 = regHeader.CusTempStorageRegLines.AddNew();
			regLine1.SRL_LineNumber = 1;

			var regLine2 = regHeader.CusTempStorageRegLines.AddNew();
			regLine2.SRL_LineNumber = 2;

			SetupCUSCANGoodsItems("04", "04");
			AddRegLineTransaction(regLine1, TransactionTypes.Codes.OpeningBalance, 4);
			AddRegLineTransaction(regLine1, TransactionTypes.Codes.Adjustment, 2);

			AddRegLineTransaction(regLine2, TransactionTypes.Codes.OpeningBalance, 9);

			Factory.Save();

			ProcessMessage(message);

			CombineAssertions(() =>
			{
				AssertEquals("Header Status", CustomsStatusList.Codes.FIN, regHeader.SRH_Status);

				AssertRegLine("Line 1", regLine1, CustomsStatusList.Codes.FIN);
				AssertRegLineTransaction("Line 1", TempStorageTestHelpers.GetRegLineTransaction(regLine1, TransactionTypes.Codes.Transaction), -6);

				AssertRegLine("Line 2", regLine2, CustomsStatusList.Codes.FIN);
				AssertRegLineTransaction("Line 2", TempStorageTestHelpers.GetRegLineTransaction(regLine2, TransactionTypes.Codes.Transaction), -9);
			});
		}

		public void TestRegisterHeaderAndLinesStatusMixedFINAndDEL()
		{
			var regLine1 = regHeader.CusTempStorageRegLines.AddNew();
			regLine1.SRL_LineNumber = 1;

			var regLine2 = regHeader.CusTempStorageRegLines.AddNew();
			regLine2.SRL_LineNumber = 2;

			SetupCUSCANGoodsItems("04", "05");
			ProcessMessage(message);
			CombineAssertions(() =>
			{
				AssertEquals("Header Status", CustomsStatusList.Codes.FIN, regHeader.SRH_Status);
				AssertRegLine("Line 1", regLine1, CustomsStatusList.Codes.FIN);
				AssertRegLine("Line 2", regLine2, CustomsStatusList.Codes.DEL);
			});
		}

		public void TestRegisterHeaderAndLinesStatusAllDEL()
		{
			var regLine1 = regHeader.CusTempStorageRegLines.AddNew();
			regLine1.SRL_LineNumber = 1;

			var regLine2 = regHeader.CusTempStorageRegLines.AddNew();
			regLine2.SRL_LineNumber = 2;

			SetupCUSCANGoodsItems("05", "05");
			ProcessMessage(message);
			CombineAssertions(() =>
			{
				AssertEquals("Header Status", CustomsStatusList.Codes.DEL, regHeader.SRH_Status);
				AssertRegLine("Line 1", regLine1, CustomsStatusList.Codes.DEL);
				AssertRegLine("Line 2", regLine2, CustomsStatusList.Codes.DEL);
			});
		}

		public void TestRegisterHeaderStatusIsNotUpdatedWhenALineStatusIsEmpty()
		{
			var regLine1 = regHeader.CusTempStorageRegLines.AddNew();
			regLine1.SRL_LineNumber = 1;

			var regLine2 = regHeader.CusTempStorageRegLines.AddNew();
			regLine2.SRL_LineNumber = 2;

			var regLine3 = regHeader.CusTempStorageRegLines.AddNew();
			regLine3.SRL_LineNumber = 3;

			SetupCUSCANGoodsItems("04", "05");
			ProcessMessage(message);
			CombineAssertions(() =>
			{
				AssertEquals("Header Status", ZString.Empty, regHeader.SRH_Status);
				AssertRegLine("Line 1", regLine1, CustomsStatusList.Codes.FIN);
				AssertRegLine("Line 2", regLine2, CustomsStatusList.Codes.DEL);
				AssertRegLine("Line 3", regLine3, ZString.Empty);
			});
		}

		public void TestPopulateLogbookRegNum()
		{
			SetupCUSCANGoodsItems("04", "05");
			dataProviderMock.Setup(m => m.MRN).Returns(MRN);

			ProcessMessage(message);
			AssertContainsExactElementsInAnyOrder(new[] { "ATB150000620520195875", "24DE123050554788M5" }, message.GetLogbookRegistrationNumbers());
		}

		public void TestUnsolicitedEMail_NominatedGroup()
		{
			dataProviderMock.Setup(m => m.GoodsItems).Returns((IReadOnlyCollection<ICUSCANGoodsItem>)Enumerable.Empty<ICUSCANGoodsItem>());
			dataProviderMock.Setup(m => m.MRN).Returns(MRN);

			var groupNotificationRegistryItem = new EU.Registry.TemporaryStorageUnsolicitedGroupNotification(Core.Constants.EmailTo.NominatedGroup, TempStorageTestHelpers.CreateGlbGroup(Factory).PK);
			using (EU.Registry.EUCustomsDataRegistry.Instance.SendTemporaryStorageUnsolicited.SetTemporaryValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, groupNotificationRegistryItem))
			{
				ProcessMessage(message);

				var email = Env.OutgoingCustomsMailManager.EmailsCreated.Single();
				var reference = regHeader.SRH_Reference;
				var subject = $"SumA CUSCAN – Customs Cancellation Information Response for {reference}";
				var bodyMessageTitle = $"<title>{subject}</title>";
				var bodyMessageHeader = $@"<strong>SumA CUSCAN – Customs Cancellation Information Response for <a href=""edient:Command=ShowEditForm&LicenceCode={GlbCompany.CurrentCompany.LicenceKeyIdentifier}&ControllerID=DESumARegister&BusinessEntityPK={regHeader.PK}";
				var bodyMessageSummary = $"Your SumA Register {reference} received a Customs Cancellation Information. For details please follow the link to the register.";
				var bodyMessageTable = @"<table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table"">"
									   + $"<tr><td>Registration Number</td><td>{ReferenceNumber}</td></tr>"
									   + $"<tr><td>MRN</td><td>{MRN}</td></tr>"
									   + $"<tr><td>Reason</td><td>{Reason}</td></tr>"
									   + "</table>";
				CombineAssertions(() => AssertEmailWithTable(ZString.Empty, email, new ZString[] { "staff1@group-suma.com", "staff2@group-suma.com" }, subject, bodyMessageTitle, bodyMessageHeader, bodyMessageSummary, bodyMessageTable));
			}
		}

		protected override ZString MessageFriendlyName => "Temporary Storage CUSCAN Message Processor";

		protected override DEBranchCustomsApplicationTypeMessageProcessor<AtlasInboundEDIMessage<ICUSCAN>> Processor => new TemporaryStorageCUSCANMessageProcessor(logger);

		protected override void SetUp()
		{
			base.SetUp();

			regHeader = Factory.NewWithValidTestData<CusTempStorageRegHeader>();
			regHeader.SRH_Reference = ReferenceNumber;

			dataProviderMock = new Mock<ICUSCAN> { CallBase = true };
			dataProviderMock.Setup(m => m.MessageIdentifier).Returns(MessageIdentifier);
			dataProviderMock.Setup(m => m.ReferenceNumber).Returns(ReferenceNumber);
			dataProviderMock.Setup(m => m.Reason).Returns(Reason);

			messageMock = Factory.NewMoq<AtlasInboundEDIMessage<ICUSCAN>>();
			messageMock.Setup(m => m.DataProvider).Returns(dataProviderMock.Object);
			message = messageMock.Object;

			Factory.Save();
		}
		CusTempStorageRegHeader regHeader;
		AtlasInboundEDIMessage<ICUSCAN> message;

		Mock<ICUSCAN> dataProviderMock;
		Mock<AtlasInboundEDIMessage<ICUSCAN>> messageMock;

		const string MessageIdentifier = "CUSCAN58750000000518175240519105043";
		const string ReferenceNumber = "ATB150000620520195875";
		const string MRN = "24DE123050554788M5";
		const string Reason = "This is the reason";

		void AssertRegLineTransaction(ZString testCase, CusTempStorageRegLineTransaction regLineTransaction, ZInt packageQty)
		{
			AssertEquals(testCase + "->SRT_PackageQty", packageQty, regLineTransaction.SRT_PackageQty);
			AssertEquals(testCase + "->SRT_GrossWeight", ZDecimal.Zero, regLineTransaction.SRT_GrossWeight);
			AssertEquals(testCase + "->SRT_ReferenceType", TransactionReferenceTypes.Codes.MANU, regLineTransaction.SRT_ReferenceType);
			AssertEquals(testCase + "->SRT_Comments", "Canceled by Customs (CUSCAN)", regLineTransaction.SRT_Comments);
			AssertEquals(testCase + "->SRT_TransactionType", TransactionTypes.Codes.Transaction, regLineTransaction.SRT_TransactionType);
			AssertEquals(testCase + "->SRT_InternalReferenceNumber", MessageIdentifier, regLineTransaction.SRT_InternalReferenceNumber);
		}

		void AssertRegLine(ZString testCase, CusTempStorageRegLine line, ZString expectedCustomsStatus)
		{
			AssertEquals(testCase + "->SRL_CustomsStatus", expectedCustomsStatus, line.SRL_CustomsStatus);
			AssertEquals(testCase + "->SRL_PackagesRemaining", ZInt.Zero, line.SRL_PackagesRemaining);
		}

		void AddRegLineTransaction(CusTempStorageRegLine line, ZString transactionType, ZInt packageQty)
		{
			var regLineTransaction = line.CusTempStorageRegLineTransactions.AddNew();
			regLineTransaction.SRT_TransactionType = transactionType;
			regLineTransaction.SRT_GrossWeight = ZDecimal.Zero;
			regLineTransaction.SRT_PackageQty = packageQty;
		}

		void SetupCUSCANGoodsItems(ZString goodsItem1Status, ZString goodsItem2Status)
		{
			var goodsItemDataProvider1Mock = new Mock<ICUSCANGoodsItem>();
			goodsItemDataProvider1Mock.Setup(m => m.SequenceNumber).Returns("1");
			goodsItemDataProvider1Mock.Setup(m => m.CustomsGoodsStatus).Returns(goodsItem1Status);

			var goodsItemDataProvider2Mock = new Mock<ICUSCANGoodsItem>();
			goodsItemDataProvider2Mock.Setup(m => m.SequenceNumber).Returns("2");
			goodsItemDataProvider2Mock.Setup(m => m.CustomsGoodsStatus).Returns(goodsItem2Status);

			dataProviderMock.Setup(m => m.GoodsItems).Returns(new ICUSCANGoodsItem[] { goodsItemDataProvider1Mock.Object, goodsItemDataProvider2Mock.Object });
			Factory.Save();
		}
	}
}
