using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.Customs.DE.Business.Testing;
using Enterprise.Customs.DE.Messaging;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Integration;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.DE.Business.CusTempStorage.Testing
{
	[TestedType(typeof(TemporaryStorageCUSTSTMessageProcessor))]
	sealed class TemporaryStorageCUSTSTMessageProcessorTest : MessageProcessorAbstractTest<TemporaryStorageCUSTSTMessageProcessor, AtlasInboundEDIMessage<ICUSTST>>
	{
		public void TestSetRegHeaderStatus()
		{
			var declaration = CUSPRLCusTempStorageDec.New(Factory.NewWithValidTestData<CusTempStorageJobHeader>());
			CreateOriginalMessageLinkedToParent<AtlasEDIMessage>(declaration, ReferencedMessageIdentifier);
			var line = declaration.CusTempStorageLines.AddNew();
			line.TSL_LineNo = 1;
			var regHeader = Factory.NewWithValidTestData<CusTempStorageRegHeader>();
			regHeader.SRH_Reference = ReferenceNumber;
			var regLine = regHeader.CusTempStorageRegLines.AddNew();
			regLine.SRL_LineNumber = 1;
			var regLine2 = regHeader.CusTempStorageRegLines.AddNew();
			regLine2.SRL_LineNumber = 2;
			regLine2.SRL_CustomsStatus = CustomsStatusList.Codes.DEL;
			Factory.Save();

			ProcessMessage(message);

			AssertEquals("regHeader.SRH_Status not updated when line status not updated", "", regHeader.SRH_Status);

			dataProviderMock.Setup(m => m.RecipientReferenceNumber).Returns("DE005876");
			ProcessMessage(message);

			AssertEquals("regHeader.SRH_Status to FIN when all FIN/DEL and line status set to FIN", CustomsStatusList.Codes.FIN, regHeader.SRH_Status);

			var regLine3 = regHeader.CusTempStorageRegLines.AddNew();
			regLine3.SRL_LineNumber = 3;
			regLine3.SRL_CustomsStatus = CustomsStatusList.Codes.TST;

			ProcessMessage(message);

			AssertEquals("regHeader.SRH_Status to PAC when not all FIN/DEL and line status set to FIN", CustomsStatusList.Codes.PAC, regHeader.SRH_Status);
		}

		public void TestExistingRegHeaderLineCustomsStatus_NotSetToFin_WhenEoriMatch_CustodianReferenceNumber()
		{
			AssertExistingRegHeaderLineCustomsStatusSet("DE8999783", "");
		}

		public void TestExistingRegHeaderLineCustomsStatus_NotSetToFin_WhenEoriMatch_DisposalEntitledTraderReferenceNumber()
		{
			AssertExistingRegHeaderLineCustomsStatusSet("DE8999715", "");
		}

		public void TestExistingRegHeaderLineCustomsStatus_SetToFin_WhenNoEoriMatch()
		{
			AssertExistingRegHeaderLineCustomsStatusSet("DE005876", CustomsStatusList.Codes.FIN);
		}

		public void TestNewRegHeaderLineCustomsStatus_NotSetToFin_WhenEoriMatch_CustodianReferenceNumber()
		{
			AssertNewRegHeaderLineCustomsStatusSet("DE8999783", CustomsStatusList.Codes.TST);
		}

		public void TestNewRegHeaderLineCustomsStatus_NotSetToFin_WhenEoriMatch_DisposalEntitledTraderReferenceNumber()
		{
			AssertNewRegHeaderLineCustomsStatusSet("DE8999715", CustomsStatusList.Codes.TST);
		}

		public void TestNewRegHeaderLineCustomsStatus_SetToFin_WhenNoEoriMatch()
		{
			AssertNewRegHeaderLineCustomsStatusSet("DE005875", CustomsStatusList.Codes.FIN);
		}

		public void TestGetLinkedObjectFromOriginalMessage()
		{
			var linkedDec = CUSPRLCusTempStorageDec.New(Factory.NewWithValidTestData<CusTempStorageJobHeader>());
			CreateOriginalMessageLinkedToParent<AtlasEDIMessage>(linkedDec, ReferencedMessageIdentifier);

			ProcessMessage(message);
			AssertEquals(linkedDec, message.EM_LinkedObject);
		}

		public void TestGetLinkedObjectFromReferenceNumber()
		{
			var regHeader = Factory.NewWithValidTestData<CusTempStorageRegHeader>();
			regHeader.SRH_Reference = ReferenceNumber;

			ProcessMessage(message);
			AssertEquals(regHeader, message.EM_LinkedObject);
		}

		public void TestGetLinkedObjectFromMRN()
		{
			var regHeader = Factory.NewWithValidTestData<CusTempStorageRegHeader>();
			regHeader.SRH_Reference = MRN;

			ProcessMessage(message);
			AssertEquals(regHeader, message.EM_LinkedObject);
		}

		public void TestDataProviderNullDueToInvalidMessage()
		{
			messageMock.Setup(m => m.DataProvider).Returns((ICUSTST)null);
			AssertNoExceptionThrown(() => ProcessMessage(message));
		}

		public void TestNoDeclarationOrRegHeaderLocated()
		{
			ProcessMessage(message);

			CombineAssertions(() =>
			{
				var headerQuery = new ZQuery(CusTempStorageRegHeaderSchema.SRH_Reference, ReferenceNumber);
				var regHeader = Factory.Load<CusTempStorageRegHeader>(headerQuery).Single();
				AssertEquals("regHeader.SRH_ArrivalDate", new ZDate(2020, 2, 18), regHeader.SRH_ArrivalDate);
				AssertEquals("regHeader.SRH_PresentationDate", new ZDate(2019, 02, 25), regHeader.SRH_PresentationDate);
				AssertEquals("regHeader.SRH_PreviousReferenceType", PreviousReferenceType.Codes._OHNE, regHeader.SRH_PreviousReferenceType);
				AssertEquals("regHeader.SRH_PreviousReference", ZString.Empty, regHeader.SRH_PreviousReference);
				AssertEquals("regHeader.SRH_Status", CustomsStatusList.Codes.TST, regHeader.SRH_Status);
				AssertEquals("regHeader.SRH_CustomsOffice", "DE005976", regHeader.SRH_CustomsOffice);
				AssertEquals("regHeader.SRH_InternalReference", "19DE587500026775M6", regHeader.SRH_InternalReference);
				AssertDocumentLinkingSubscribers(new BusinessObject[] { regHeader });

				var regline = regHeader.CusTempStorageRegLines.Single();
				AssertEquals("regline.SRL_LineNumber", 1, regline.SRL_LineNumber);
				AssertEquals("regline.SRL_OwnerReferenceType", "ZZZ", regline.SRL_OwnerReferenceType);
				AssertEquals("regline.SRL_OwnerReference", "TEST01", regline.SRL_OwnerReference);
				AssertEquals("regline.SRL_LocationOfGoods", "2", regline.SRL_LocationOfGoods);
				AssertEquals("regline.SRL_GoodsDescription", "Holzspielzeug", regline.SRL_GoodsDescription);
				AssertEquals("regline.SRL_PackagesRemaining", 10, regline.SRL_PackagesRemaining);
				AssertEquals("regline.SRL_PackageType", "PC", regline.SRL_PackageType);
				AssertEquals("regline.SRL_CustodianIdentifier", "DE8999783", regline.SRL_CustodianIdentifier);
				AssertEquals("regline.SRL_CustodianIdentifierBranchNo", "0001", regline.SRL_CustodianIdentifierBranchNo);
				AssertEquals("regline.SRL_GoodsOwnerIdentifier", "DE8999715", regline.SRL_GoodsOwnerIdentifier);
				AssertEquals("regline.SRL_GoodsOwnerIdentifierBranchNo", "0000", regline.SRL_GoodsOwnerIdentifierBranchNo);
				AssertEquals("regline.SRL_GrossWeightUQ", "KGM", regline.SRL_GrossWeightUQ);
				AssertEquals("regline.SRL_CustomsStatus", CustomsStatusList.Codes.TST, regline.SRL_CustomsStatus);
				AssertEquals("regline.SRL_LimitDate", new ZDate(2020, 5, 12), regline.SRL_LimitDate);
				AssertEquals("regline.SRL_UnionStatus", "C", regline.SRL_UnionStatus);

				var openingTransactionLine = regline.CusTempStorageRegLineTransactions.Single(x => x.SRT_TransactionType == TransactionTypes.Codes.OpeningBalance);
				AssertEquals("openingTransactionLine.SRT_GrossWeight", 10.15m, openingTransactionLine.SRT_GrossWeight);
				AssertEquals("openingTransactionLine.SRT_PackageQty", 10, openingTransactionLine.SRT_PackageQty);
				AssertEquals("openingTransactionLine.SRT_InternalReferenceNumber", "CUSTST58750000000375302250219160050", openingTransactionLine.SRT_InternalReferenceNumber);
				AssertEquals("openingTransactionLine.SRT_Comments", ZString.Empty, openingTransactionLine.SRT_Comments);

				AssertEquals("message.EM_LinkedObject", regHeader, message.EM_LinkedObject);
			});
		}

		public void TestExistingDeclaration_NoRegHeader()
		{
			var declaration = CUSPRLCusTempStorageDec.New(Factory.NewWithValidTestData<CusTempStorageJobHeader>());
			CreateOriginalMessageLinkedToParent<AtlasEDIMessage>(declaration, ReferencedMessageIdentifier);
			var line1 = declaration.CusTempStorageLines.AddNew();
			line1.TSL_LineNo = 1;
			var line2 = declaration.CusTempStorageLines.AddNew();
			line2.TSL_LineNo = 2;
			Factory.Save();

			ProcessMessage(message);
			CombineAssertions(() =>
			{
				AssertDocumentLinkingSubscribers(new BusinessObject[] { declaration.StorageHeader, CusTempStorageRegHeader.Load(Factory, ReferenceNumber) });
				AssertEquals("declaration.STH_MessageStatus", EDIMessageStatusList.Codes.ProcessedOK, declaration.STH_MessageStatus);
				AssertEquals("declaration.ReferenceNumber", "AT/B/15/000212/05/2019/5395", declaration.ReferenceNumber);
				AssertEquals("line1.TSL_CustomsStatus", CustomsStatusList.Codes.TST, line1.TSL_CustomsStatus);
				AssertEquals("line2.TSL_CustomsStatus", ZString.Empty, line2.TSL_CustomsStatus);
				AssertEquals("Update Status Log", true, logger.DebugLogStrings.Cast<string>().Any(x => x.Contains("Update the status to TST on line number 1")));
			});
		}

		public void TestExistingDeclaration_NoRegHeader_UpdateFromMRN()
		{
			dataProviderMock.Setup(m => m.ReferenceNumber).Returns(ZString.Empty);

			var declaration = CUSPRLCusTempStorageDec.New(Factory.NewWithValidTestData<CusTempStorageJobHeader>());
			CreateOriginalMessageLinkedToParent<AtlasEDIMessage>(declaration, ReferencedMessageIdentifier);
			var line1 = declaration.CusTempStorageLines.AddNew();
			line1.TSL_LineNo = 1;
			var line2 = declaration.CusTempStorageLines.AddNew();
			line2.TSL_LineNo = 2;
			Factory.Save();

			ProcessMessage(message);
			CombineAssertions(() =>
			{
				AssertDocumentLinkingSubscribers(new BusinessObject[] { declaration.StorageHeader, CusTempStorageRegHeader.Load(Factory, MRN) });
				AssertEquals("declaration.STH_MessageStatus", EDIMessageStatusList.Codes.ProcessedOK, declaration.STH_MessageStatus);
				AssertEquals("declaration.ReferenceNumber", MRN, declaration.ReferenceNumber);
				AssertEquals("line1.TSL_CustomsStatus", CustomsStatusList.Codes.TST, line1.TSL_CustomsStatus);
				AssertEquals("line2.TSL_CustomsStatus", ZString.Empty, line2.TSL_CustomsStatus);
				AssertEquals("Update Status Log", true, logger.DebugLogStrings.Cast<string>().Any(x => x.Contains("Update the status to TST on line number 1")));
			});
		}

		public void TestUpdateDeclaration_ExistingRegHeaderWithRegLine()
		{
			var declaration = CUSPRLCusTempStorageDec.New(Factory.NewWithValidTestData<CusTempStorageJobHeader>());
			CreateOriginalMessageLinkedToParent<AtlasEDIMessage>(declaration, ReferencedMessageIdentifier);
			var line1 = declaration.CusTempStorageLines.AddNew();
			line1.TSL_LineNo = 1;
			var line2 = declaration.CusTempStorageLines.AddNew();
			line2.TSL_LineNo = 2;
			var regHeader = Factory.NewWithValidTestData<CusTempStorageRegHeader>();
			regHeader.SRH_Reference = ReferenceNumber;
			var regLine = regHeader.CusTempStorageRegLines.AddNew();
			regLine.SRL_LineNumber = 1;
			Factory.Save();

			ProcessMessage(message);
			CombineAssertions(() =>
			{
				AssertEquals("declaration.STH_MessageStatus", EDIMessageStatusList.Codes.ProcessedOK, declaration.STH_MessageStatus);
				AssertEquals("line1.TSL_CustomsStatus", CustomsStatusList.Codes.TST, line1.TSL_CustomsStatus);
				AssertEquals("line2.TSL_CustomsStatus", ZString.Empty, line2.TSL_CustomsStatus);
				AssertEquals("regLine.SRL_GoodsDescription", "Holzspielzeug", regLine.SRL_GoodsDescription);
				AssertEquals("Update Status Log", true, logger.DebugLogStrings.Cast<string>().Any(x => x.Contains("Update the status to TST on line number 1")));
			});
		}

		public void TestUpdateDeclaration_ExistingRegHeaderWithRegLine_UpdateFromMRN()
		{
			dataProviderMock.Setup(m => m.ReferenceNumber).Returns(ZString.Empty);

			var declaration = CUSPRLCusTempStorageDec.New(Factory.NewWithValidTestData<CusTempStorageJobHeader>());
			CreateOriginalMessageLinkedToParent<AtlasEDIMessage>(declaration, ReferencedMessageIdentifier);
			var line1 = declaration.CusTempStorageLines.AddNew();
			line1.TSL_LineNo = 1;
			var line2 = declaration.CusTempStorageLines.AddNew();
			line2.TSL_LineNo = 2;
			var regHeader = Factory.NewWithValidTestData<CusTempStorageRegHeader>();
			regHeader.SRH_Reference = MRN;
			var regLine = regHeader.CusTempStorageRegLines.AddNew();
			regLine.SRL_LineNumber = 1;
			Factory.Save();

			ProcessMessage(message);
			CombineAssertions(() =>
			{
				AssertEquals("declaration.STH_MessageStatus", EDIMessageStatusList.Codes.ProcessedOK, declaration.STH_MessageStatus);
				AssertEquals("declaration.ReferenceNumber", MRN, declaration.ReferenceNumber);
				AssertEquals("line1.TSL_CustomsStatus", CustomsStatusList.Codes.TST, line1.TSL_CustomsStatus);
				AssertEquals("line2.TSL_CustomsStatus", ZString.Empty, line2.TSL_CustomsStatus);
				AssertEquals("regLine.SRL_GoodsDescription", "Holzspielzeug", regLine.SRL_GoodsDescription);
				AssertEquals("Update Status Log", true, logger.DebugLogStrings.Cast<string>().Any(x => x.Contains("Update the status to TST on line number 1")));
			});
		}

		public void TestUpdateTSL_CustomsStatusOfDeclarationsWithMatchingATBNumber_MixedDeclarations()
		{
			var header = Factory.New<CusTempStorageJobHeader>();
			header.SJH_JobReference = "DECUSPRL001";
			var cusprlDeclaration = CUSPRLCusTempStorageDec.New(header);
			cusprlDeclaration.ReferenceNumber = ReferenceNumber;
			var cusprlLine1 = cusprlDeclaration.CusTempStorageLines.AddNew();
			cusprlLine1.TSL_LineNo = 1;

			var chogffDeclaration = header.CHGOFFCusTempStorageDecs.AddNew();
			chogffDeclaration.ReferenceNumber = ReferenceNumber;
			var chgoffLine1 = chogffDeclaration.CusTempStorageLines.AddNew();
			chgoffLine1.TSL_LineNo = 1;

			ProcessMessage(message);
			CombineAssertions(() =>
			{
				AssertEquals("CUSPRL Line status updated", CustomsStatusList.Codes.TST, cusprlLine1.TSL_CustomsStatus);
				AssertEquals("CHGOFF Line status not updated", ZString.Empty, chgoffLine1.TSL_CustomsStatus);
				AssertEquals("Update Status Log", true, logger.DebugLogStrings.Cast<string>().Any(x => x.Contains("Update the status to TST on line number 1 for Temporary Storage Header DECUSPRL001")));
			});
		}

		public void TestUpdateTSL_CustomsStatusOfDeclarationsWithMatchingATBNumber_MultipleCUSPRL()
		{
			var goodsItemMock2 = CreateGoodsItemMock("2");
			var goodsItemMock3 = CreateGoodsItemMock("3");
			dataProviderMock.Setup(m => m.GoodsItems).Returns(new ICUSTSTGoodsItem[] { goodsItemMock.Object, goodsItemMock2.Object, goodsItemMock3.Object });

			var declaration1 = CUSPRLCusTempStorageDec.New(Factory.NewWithValidTestData<CusTempStorageJobHeader>());
			CreateOriginalMessageLinkedToParent<AtlasEDIMessage>(declaration1, ReferencedMessageIdentifier);
			var line3 = declaration1.CusTempStorageLines.AddNew();
			line3.TSL_LineNo = 3;
			var line4 = declaration1.CusTempStorageLines.AddNew();
			line4.TSL_LineNo = 4;

			var declaration2 = CUSPRLCusTempStorageDec.New(Factory.NewWithValidTestData<CusTempStorageJobHeader>());
			declaration2.ReferenceNumber = ReferenceNumber;
			var line5 = declaration2.CusTempStorageLines.AddNew();
			line5.TSL_LineNo = 5;

			var declaration3 = CUSPRLCusTempStorageDec.New(Factory.NewWithValidTestData<CusTempStorageJobHeader>());
			declaration3.ReferenceNumber = ReferenceNumber;
			var line1 = declaration3.CusTempStorageLines.AddNew();
			line1.TSL_LineNo = 1;
			var line2 = declaration3.CusTempStorageLines.AddNew();
			line2.TSL_LineNo = 2;

			var declaration4 = CUSPRLCusTempStorageDec.New(Factory.NewWithValidTestData<CusTempStorageJobHeader>());
			declaration4.ReferenceNumber = "ATB150002110520195876";
			var differentReference_line11 = declaration4.CusTempStorageLines.AddNew();
			differentReference_line11.TSL_LineNo = 11;

			ProcessMessage(message);
			CombineAssertions(() =>
			{
				AssertEquals("Linked declaration's ATBNumber matches message's ATBNumber", ReferenceNumber, declaration1.CusEntryNumber.CE_EntryNum);
				AssertEquals("line3", CustomsStatusList.Codes.TST, line3.TSL_CustomsStatus);
				AssertEquals("Line4", ZString.Empty, line4.TSL_CustomsStatus);
				AssertEquals("Line5", ZString.Empty, line5.TSL_CustomsStatus);
				AssertEquals("Line1", CustomsStatusList.Codes.TST, line1.TSL_CustomsStatus);
				AssertEquals("Line2", CustomsStatusList.Codes.TST, line2.TSL_CustomsStatus);
				AssertEquals("Different Reference line11", ZString.Empty, differentReference_line11.TSL_CustomsStatus);
			});
		}

		public void TestUpdateTSL_CustomsStatusOfDeclarationsWithMatchingATBNumber_DifferentStorageHeaders()
		{
			var header1 = Factory.New<CusTempStorageJobHeader>();
			header1.SJH_JobReference = "DECUSPRL001";
			var cusprlDeclaration1 = CUSPRLCusTempStorageDec.New(header1);
			cusprlDeclaration1.ReferenceNumber = ReferenceNumber;
			var cusprlLine1 = cusprlDeclaration1.CusTempStorageLines.AddNew();
			cusprlLine1.TSL_LineNo = 1;

			var header2 = Factory.New<CusTempStorageJobHeader>();
			header2.SJH_JobReference = "DECUSPRL002";
			var cusprlDeclaration2 = CUSPRLCusTempStorageDec.New(header2);
			cusprlDeclaration2.ReferenceNumber = ReferenceNumber;
			var cusprlLine2 = cusprlDeclaration2.CusTempStorageLines.AddNew();
			cusprlLine2.TSL_LineNo = 1;

			ProcessMessage(message);
			CombineAssertions(() =>
			{
				AssertEquals("Line1 status updated", CustomsStatusList.Codes.TST, cusprlLine1.TSL_CustomsStatus);
				AssertEquals("Line2 status updated", CustomsStatusList.Codes.TST, cusprlLine2.TSL_CustomsStatus);
				AssertEquals("Update Status Log for Header: DECUSPRL001", true, logger.DebugLogStrings.Cast<string>().Any(x => x.Contains("Update the status to TST on line number 1 for Temporary Storage Header DECUSPRL001")));
				AssertEquals("Update Status Log for Header: DECUSPRL002", true, logger.DebugLogStrings.Cast<string>().Any(x => x.Contains("Update the status to TST on line number 1 for Temporary Storage Header DECUSPRL002")));
			});
		}

		[TestDate(2020, 2, 18)]
		public void TestExistingRegHeaderPackageCountGreaterThan()
		{
			var regHeader = Factory.New<CusTempStorageRegHeader>();
			regHeader.SRH_Reference = ReferenceNumber;
			regHeader.SRH_ArrivalDate = ZDate.Today;
			var regLine = regHeader.CusTempStorageRegLines.AddNew();
			regLine.SRL_LineNumber = 1;
			regLine.SRL_LocationOfGoods = Core.Constants.CountryCodes.Germany;
			regLine.SRL_LimitDate = ZDate.Today;
			regLine.SRL_GrossWeightUQ = "KGM";
			var openingBalanceTransaction = regLine.CusTempStorageRegLineTransactions.AddNew();
			openingBalanceTransaction.SRT_GrossWeight = 32.65m;
			openingBalanceTransaction.SRT_PackageQty = 12;
			openingBalanceTransaction.SRT_TransactionType = TransactionTypes.Codes.OpeningBalance;
			Factory.Save();

			ProcessMessage(message);

			CombineAssertions(() =>
			{
				AssertDocumentLinkingSubscribers(new BusinessObject[] { regHeader });
				AssertEquals("regHeader.SRH_ArrivalDate", new ZDate(2020, 2, 18), regHeader.SRH_ArrivalDate);
				AssertEquals("regHeader.SRH_Status", ZString.Empty, regHeader.SRH_Status);
				AssertEquals("regHeader.SRH_InternalReference", "19DE587500026775M6", regHeader.SRH_InternalReference);

				AssertEquals("regLine.SRL_PackagesRemaining", 10, regLine.SRL_PackagesRemaining);
				AssertEquals("regLine.SRL_PackageType", "PC", regLine.SRL_PackageType);
				AssertEquals("regLine.SRL_CustomsStatus", ZString.Empty, regLine.SRL_CustomsStatus);
				AssertEquals("regLine.CusTempStorageRegLineTransactions.Count", 2, regLine.CusTempStorageRegLineTransactions.Count);

				AssertEquals("openingBalanceTransaction.SRT_PackageQty", 12, openingBalanceTransaction.SRT_PackageQty);
				AssertEquals("openingBalanceTransaction.SRT_GrossWeight", 32.65m, openingBalanceTransaction.SRT_GrossWeight);

				var adjustmentLine = regLine.CusTempStorageRegLineTransactions.Single(x => x.SRT_TransactionType == TransactionTypes.Codes.Transaction);
				AssertEquals("adjustmentLine.SRT_PackageQty", -2, adjustmentLine.SRT_PackageQty);
				AssertEquals("adjustmentLine.SRT_GrossWeight", 10.15m, adjustmentLine.SRT_GrossWeight);
			});
		}

		public void TestExistingRegHeaderPackageCountLessThan()
		{
			var regHeader = Factory.New<CusTempStorageRegHeader>();
			regHeader.SRH_Reference = ReferenceNumber;
			regHeader.SRH_ArrivalDate = ZDate.Today;
			var regLine = regHeader.CusTempStorageRegLines.AddNew();
			regLine.SRL_LineNumber = 1;
			regLine.SRL_LocationOfGoods = Core.Constants.CountryCodes.Germany;
			regLine.SRL_LimitDate = ZDate.Today;
			regLine.SRL_GrossWeightUQ = "KGM";
			var openingBalanceTransaction = regLine.CusTempStorageRegLineTransactions.AddNew();
			openingBalanceTransaction.SRT_GrossWeight = 8.75m;
			openingBalanceTransaction.SRT_PackageQty = 7;
			openingBalanceTransaction.SRT_TransactionType = TransactionTypes.Codes.OpeningBalance;
			Factory.Save();

			ProcessMessage(message);

			CombineAssertions(() =>
			{
				AssertDocumentLinkingSubscribers(new BusinessObject[] { regHeader });
				AssertEquals("regLine.SRL_PackagesRemaining", 10, regLine.SRL_PackagesRemaining);
				var adjustmentLine = regLine.CusTempStorageRegLineTransactions.Single(x => x.SRT_TransactionType == TransactionTypes.Codes.Transaction);
				AssertEquals("adjustmentLine.SRT_PackageQty", 3, adjustmentLine.SRT_PackageQty);
				AssertEquals("adjustmentLine.SRT_GrossWeight", 10.15m, adjustmentLine.SRT_GrossWeight);
			});
		}

		public void TestLimitDate()
		{
			var expectedLimitDate = ZDate.Today;
			var regHeader = Factory.New<CusTempStorageRegHeader>();
			regHeader.SRH_Reference = ReferenceNumber;
			regHeader.SRH_ArrivalDate = ZDate.Today;
			var regLine = regHeader.CusTempStorageRegLines.AddNew();
			regLine.SRL_LineNumber = 1;
			regLine.SRL_LimitDate = expectedLimitDate;
			Factory.Save();

			goodsItemMock.Setup(m => m.LimitDate).Returns(ZDate.Empty);
			ProcessMessage(message);

			AssertEquals(expectedLimitDate, regLine.SRL_LimitDate);
		}

		public void TestPopulateLogbookRegNum()
		{
			ProcessMessage(message);
			AssertContainsExactElementsInAnyOrder(new[] { ReferenceNumber, MRN }, message.GetLogbookRegistrationNumbers());
		}

		public void TestStmNoteIsGenerated_GoodsItemCustodianReferenceNumberIsEmpty()
		{
			var regHeader = Factory.New<CusTempStorageRegHeader>();
			regHeader.SRH_Reference = ReferenceNumber;
			var regLine = regHeader.CusTempStorageRegLines.AddNew();
			regLine.SRL_LineNumber = 1;
			Factory.Save();

			var custodianAddressMock = new Mock<IUnderCustomsControlGoodsItemAddress>();
			custodianAddressMock.Setup(m => m.Line).Returns("Line 1");
			custodianAddressMock.Setup(m => m.Country).Returns("DE");
			custodianAddressMock.Setup(m => m.Postcode).Returns("123456");
			custodianAddressMock.Setup(m => m.City).Returns("Berlin");
			custodianAddressMock.Setup(m => m.District).Returns("District 1");

			var goodsItemMock = CreateGoodsItemMock("1");
			goodsItemMock.Setup(m => m.CustodianReferenceNumber).Returns(ZString.Empty);
			goodsItemMock.Setup(m => m.CustodianName).Returns("Name 1");
			goodsItemMock.Setup(m => m.CustodianAddress).Returns(custodianAddressMock.Object);
			dataProviderMock.Setup(m => m.GoodsItems).Returns(new ICUSTSTGoodsItem[] { goodsItemMock.Object });

			ProcessMessage(message);
			CombineAssertions(() =>
			{
				var stmNote = GetStmNote(regHeader.PK, EFTA.TemporaryStorageRegister.Business.AutoCusTempStorageRegHeader.Schema.TableName, "Custodian Address for Line 1");
				AssertEquals("ST_NoteText", @"Name 1
Line 1
DE 123456 Berlin
District 1
", stmNote.ST_NoteText);
			});
		}

		public void TestStmNoteIsGenerated_GoodsItemCustodyPlaceCodeIsEmpty()
		{
			var regHeader = Factory.New<CusTempStorageRegHeader>();
			regHeader.SRH_Reference = ReferenceNumber;
			var regLine = regHeader.CusTempStorageRegLines.AddNew();
			regLine.SRL_LineNumber = 1;
			Factory.Save();

			var custodyPlaceAddressMock = new Mock<IUnderCustomsControlGoodsItemAddress>();
			custodyPlaceAddressMock.Setup(m => m.Line).Returns("Line 2");
			custodyPlaceAddressMock.Setup(m => m.Postcode).Returns("234567");
			custodyPlaceAddressMock.Setup(m => m.City).Returns("Munich");
			custodyPlaceAddressMock.Setup(m => m.District).Returns("District 2");

			var goodsItemMock = CreateGoodsItemMock("2");
			goodsItemMock.Setup(m => m.CustodyPlaceCode).Returns(ZString.Empty);
			goodsItemMock.Setup(m => m.CustodyPlaceInformation).Returns("Information 2");
			goodsItemMock.Setup(m => m.CustodyPlaceAddress).Returns(custodyPlaceAddressMock.Object);
			dataProviderMock.Setup(m => m.GoodsItems).Returns(new ICUSTSTGoodsItem[] { goodsItemMock.Object });

			ProcessMessage(message);
			CombineAssertions(() =>
			{
				var stmNote = GetStmNote(regHeader.PK, EFTA.TemporaryStorageRegister.Business.AutoCusTempStorageRegHeader.Schema.TableName, "Custody Place for Line 2");
				AssertEquals("ST_NoteText", @"Information 2
Line 2
234567 Munich
District 2
", stmNote.ST_NoteText);
			});
		}

		public void TestSendUnsolicitedMessageEMail_NominatedGroup()
		{
			messageMock.Setup(m => m.AttachedDocuments).Returns(new List<AttachedDocument>
			{
				new AttachedDocument
				{
					FileName = "file_EPR.pdf",
					Type = new DocumentType { Code = "EPR", Description = "EPR Desc" },
					ImageData = (SubStreamableStream)new MemoryStream(Convert.FromBase64String("XXX="))
				},
				new AttachedDocument
				{
					FileName = "file_AAA.pdf",
					Type = new DocumentType { Code = "AAA", Description = "AAA Desc" },
					ImageData = (SubStreamableStream)new MemoryStream(Convert.FromBase64String("XXX="))
				}
			});
			var regHeader = Factory.New<CusTempStorageRegHeader>();
			regHeader.SRH_Reference = ReferenceNumber;

			var groupNotificationRegistryItem = new EU.Registry.TemporaryStorageUnsolicitedGroupNotification(Core.Constants.EmailTo.NominatedGroup, TempStorageTestHelpers.CreateGlbGroup(Factory).PK);
			using (EU.Registry.EUCustomsDataRegistry.Instance.SendTemporaryStorageUnsolicited.SetTemporaryValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, groupNotificationRegistryItem))
			{
				ProcessMessage(message);
				var email = Env.OutgoingCustomsMailManager.EmailsCreated.Single();
				CombineAssertions(() =>
				{
					AssertUnsolicitedMessageEmail(email, regHeader, new ZString[] { "staff1@group-suma.com", "staff2@group-suma.com" });
					AssertEquals("File has been attached", "file_EPR.pdf", email.Attachments.Cast<AttachmentDef>().SingleOrDefault(x => x.DisplayName.StartsWith("file_")).DisplayName);
				});
			}
			message.AttachedDocuments[0].ImageData.Dispose();
		}

		public void TestSendUnsolicitedMessageEMailForNewlyCreatedRegHeaderAsLinkedObject_NominatedGroup()
		{
			var groupNotificationRegistryItem = new EU.Registry.TemporaryStorageUnsolicitedGroupNotification(Core.Constants.EmailTo.NominatedGroup, TempStorageTestHelpers.CreateGlbGroup(Factory).PK);
			using (EU.Registry.EUCustomsDataRegistry.Instance.SendTemporaryStorageUnsolicited.SetTemporaryValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, groupNotificationRegistryItem))
			{
				ProcessMessage(message);
				var email = Env.OutgoingCustomsMailManager.EmailsCreated.Single();
				var headerQuery = new ZQuery(CusTempStorageRegHeaderSchema.SRH_Reference, ReferenceNumber);
				var regHeader = Factory.Load<CusTempStorageRegHeader>(headerQuery).Single();
				AssertUnsolicitedMessageEmail(email, regHeader, new ZString[] { "staff1@group-suma.com", "staff2@group-suma.com" });
			}
		}

		public void TestSendAcknownledgementEMail_StaffMember()
		{
			messageMock.Setup(m => m.AttachedDocuments).Returns(new List<AttachedDocument>
			{
				new AttachedDocument
				{
					FileName = "file_EPR.pdf",
					Type = new DocumentType { Code = "EPR", Description = "EPR Desc" },
					ImageData = (SubStreamableStream)new MemoryStream(Convert.FromBase64String("XXX="))
				},
				new AttachedDocument
				{
					FileName = "file_AAA.pdf",
					Type = new DocumentType { Code = "AAA", Description = "AAA Desc" },
					ImageData = (SubStreamableStream)new MemoryStream(Convert.FromBase64String("XXX="))
				}
			});

			var jobHeader = Factory.NewWithValidTestData<CusTempStorageJobHeader>();
			jobHeader.SJH_JobReference = "JobReference";
			CreateOriginalMessageLinkedToParent<AtlasEDIMessage>(CUSPRLCusTempStorageDec.New(jobHeader), ReferencedMessageIdentifier, "senduser@mail.com");
			var groupNotificationRegistryItem = new EU.Registry.TemporaryStorageGroupNotification(Core.Constants.EmailTo.StaffMember, ZGuid.Empty);
			using (EU.Registry.EUCustomsDataRegistry.Instance.SendTemporaryStorageAcknowledgements.SetTemporaryValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, groupNotificationRegistryItem))
			{
				ProcessMessage(message);
				var email = Env.OutgoingCustomsMailManager.EmailsCreated.Single();
				CombineAssertions(() =>
				{
					AssertAcknowledgeEmail(email, jobHeader, new ZString[] { "senduser@mail.com" });
					AssertEquals("File has been attached", "file_EPR.pdf", email.Attachments.Cast<AttachmentDef>().SingleOrDefault(x => x.DisplayName.StartsWith("file_")).DisplayName);
				});
			}
		}

		public void TestSendAcknownledgementEMail_NominatedGroup()
		{
			var jobHeader = Factory.NewWithValidTestData<CusTempStorageJobHeader>();
			jobHeader.SJH_JobReference = "JobReference";
			CreateOriginalMessageLinkedToParent<AtlasEDIMessage>(CUSPRLCusTempStorageDec.New(jobHeader), ReferencedMessageIdentifier, "senduser@mail.com");
			var groupNotificationRegistryItem = new EU.Registry.TemporaryStorageGroupNotification(Core.Constants.EmailTo.NominatedGroup, TempStorageTestHelpers.CreateGlbGroup(Factory).PK);
			using (EU.Registry.EUCustomsDataRegistry.Instance.SendTemporaryStorageAcknowledgements.SetTemporaryValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, groupNotificationRegistryItem))
			{
				ProcessMessage(message);
				var email = Env.OutgoingCustomsMailManager.EmailsCreated.Single();
				CombineAssertions(() =>
				{
					AssertAcknowledgeEmail(email, jobHeader, new ZString[] { "staff1@group-suma.com", "staff2@group-suma.com" });
				});
			}
		}

		public void TestSendAcknownledgementEMail_StaffMemberAndNominatedGroup()
		{
			var jobHeader = Factory.NewWithValidTestData<CusTempStorageJobHeader>();
			jobHeader.SJH_JobReference = "JobReference";
			CreateOriginalMessageLinkedToParent<AtlasEDIMessage>(CUSPRLCusTempStorageDec.New(jobHeader), ReferencedMessageIdentifier, "senduser@mail.com");
			var groupNotificationRegistryItem = new EU.Registry.TemporaryStorageGroupNotification(Core.Constants.EmailTo.StaffMemberAndNominatedGroup, TempStorageTestHelpers.CreateGlbGroup(Factory).PK);
			using (EU.Registry.EUCustomsDataRegistry.Instance.SendTemporaryStorageAcknowledgements.SetTemporaryValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, groupNotificationRegistryItem))
			{
				ProcessMessage(message);
				var email = Env.OutgoingCustomsMailManager.EmailsCreated.Single();
				CombineAssertions(() =>
				{
					AssertAcknowledgeEmail(email, jobHeader, new ZString[] { "senduser@mail.com", "staff1@group-suma.com", "staff2@group-suma.com" });
				});
			}
		}

		protected override ZString MessageFriendlyName => "Temporary Storage CUSTST Message Processor";

		protected override DEBranchCustomsApplicationTypeMessageProcessor<AtlasInboundEDIMessage<ICUSTST>> Processor => new TemporaryStorageCUSTSTMessageProcessor(logger);

		protected override bool ExpectedMustHaveLinkedObject => false;

		protected override void SetUp()
		{
			base.SetUp();

			goodsItemMock = CreateGoodsItemMock("1");
			goodsItemMock.Setup(m => m.CustomsGoodsStatus).Returns("C");

			dataProviderMock = new Mock<ICUSTST>();
			dataProviderMock.Setup(m => m.MessageIdentifier).Returns("CUSTST58750000000375302250219160050");
			dataProviderMock.Setup(m => m.ArrivalDate).Returns(new ZDate(2020, 2, 18));
			dataProviderMock.Setup(m => m.PresentationDate).Returns(presentationDate);
			dataProviderMock.Setup(m => m.ReferenceNumber).Returns(ReferenceNumber);
			dataProviderMock.Setup(m => m.MRN).Returns(MRN);
			dataProviderMock.Setup(m => m.LocalReferenceNumber).Returns(LocalReferenceNumber);
			dataProviderMock.Setup(m => m.PreviousReferenceType).Returns(PreviousReferenceType.Codes._OHNE);
			dataProviderMock.Setup(m => m.PreviousReferenceNumber).Returns(ZString.Empty);
			dataProviderMock.Setup(m => m.CustomsOfficeReferenceNumber).Returns("DE005976");
			dataProviderMock.Setup(m => m.RecipientReferenceNumber).Returns("DE8999783");
			dataProviderMock.Setup(m => m.ReferencedMessageIdentifier).Returns(ReferencedMessageIdentifier);
			dataProviderMock.Setup(m => m.GoodsItems).Returns(new ICUSTSTGoodsItem[] { goodsItemMock.Object });
			dataProviderMock.Setup(m => m.LocalReferenceNumber).Returns("19DE587500026775M6");

			messageMock = Factory.NewMoq<AtlasInboundEDIMessage<ICUSTST>>();
			messageMock.Setup(m => m.DataProvider).Returns(dataProviderMock.Object);
			message = messageMock.Object;
			Factory.Save();
		}
		Mock<ICUSTSTGoodsItem> goodsItemMock;
		Mock<ICUSTST> dataProviderMock;
		Mock<AtlasInboundEDIMessage<ICUSTST>> messageMock;
		AtlasInboundEDIMessage<ICUSTST> message;

		const string ReferenceNumber = "ATB150002120520195395";
		const string MRN = "23DE586601055987B7";
		const string ReferencedMessageIdentifier = "DE899978300000000812";
		const string LocalReferenceNumber = "19DE587500026775M6";
		readonly ZDate presentationDate = new ZDate(2019, 02, 25);

		Mock<ICUSTSTGoodsItem> CreateGoodsItemMock(ZString sequenceNumber)
		{
			var result = new Mock<ICUSTSTGoodsItem>();
			result.Setup(m => m.SequenceNumber).Returns(sequenceNumber);
			result.Setup(m => m.OwnerReferenceType).Returns("ZZZ");
			result.Setup(m => m.OwnerReferenceNumber).Returns("TEST01");
			result.Setup(m => m.LocationOfGoods).Returns("2");
			result.Setup(m => m.GoodsDescription).Returns("Holzspielzeug");
			result.Setup(m => m.PackageType).Returns("PC");
			result.Setup(m => m.PackageQty).Returns(10);
			result.Setup(m => m.CustodianReferenceNumber).Returns("DE8999783");
			result.Setup(m => m.CustodianSubsidiaryNumber).Returns("0001");
			result.Setup(m => m.DisposalEntitledTraderReferenceNumber).Returns("DE8999715");
			result.Setup(m => m.DisposalEntitledTraderSubsidiaryNumber).Returns("0000");
			result.Setup(m => m.GrossWeight).Returns(10.15m);
			result.Setup(m => m.LimitDate).Returns(new ZDate(2020, 5, 12));
			result.Setup(m => m.CustomsGoodsStatus).Returns("C");
			result.Setup(m => m.CustodyPlaceCode).Returns("1");
			return result;
		}

		void AssertExistingRegHeaderLineCustomsStatusSet(string eori, string status)
		{
			var declaration = CUSPRLCusTempStorageDec.New(Factory.NewWithValidTestData<CusTempStorageJobHeader>());
			CreateOriginalMessageLinkedToParent<AtlasEDIMessage>(declaration, ReferencedMessageIdentifier);
			var line = declaration.CusTempStorageLines.AddNew();
			line.TSL_LineNo = 1;
			var regHeader = Factory.NewWithValidTestData<CusTempStorageRegHeader>();
			regHeader.SRH_Reference = ReferenceNumber;
			var regLine = regHeader.CusTempStorageRegLines.AddNew();
			regLine.SRL_LineNumber = 1;
			dataProviderMock.Setup(m => m.RecipientReferenceNumber).Returns(eori);
			Factory.Save();

			ProcessMessage(message);

			AssertEquals("regLine.SRL_CustomsStatus eori doesn't match", status, regLine.SRL_CustomsStatus);
		}

		void AssertNewRegHeaderLineCustomsStatusSet(string eori, string status)
		{
			var declaration = CUSPRLCusTempStorageDec.New(Factory.NewWithValidTestData<CusTempStorageJobHeader>());
			CreateOriginalMessageLinkedToParent<AtlasEDIMessage>(declaration, ReferencedMessageIdentifier);
			var line = declaration.CusTempStorageLines.AddNew();
			line.TSL_LineNo = 11;
			dataProviderMock.Setup(m => m.RecipientReferenceNumber).Returns(eori);

			Factory.Save();
			ProcessMessage(message);

			var regHeader = CusTempStorageRegHeader.Load(Factory, ReferenceNumber);
			AssertEquals("SRL_CustomsStatus", status, regHeader.CusTempStorageRegLines.Single().SRL_CustomsStatus);
		}

		void AssertUnsolicitedMessageEmail(EmailDef email, CusTempStorageRegHeader regHeader, ZString[] expectedEmailRecipientsMailAddress)
		{
			var reference = regHeader.SRH_Reference;
			var subject = $"SumA CUSTST - Temporary Storage Response for {reference}";
			var bodyMessageTitle = $"<title>{subject}</title>";
			var bodyMessageHeader = $@"<strong>SumA CUSTST - Temporary Storage Response for <a href=""edient:Command=ShowEditForm&LicenceCode={GlbCompany.CurrentCompany.LicenceKeyIdentifier}&ControllerID=DESumARegister&BusinessEntityPK={regHeader.PK}";
			var bodyMessageSummary = $"Your SumA Register {reference} received a Temporary Storage message. For details please follow the link to the SumA Register.";
			var bodyMessageTable = @"<table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table"">"
								   + $"<tr><td>MRN</td><td>{MRN}</td></tr>"
								   + $"<tr><td>Registration Number</td><td>{ReferenceNumber}</td></tr>"
								   + $"<tr><td>Local Reference Number</td><td>{LocalReferenceNumber}</td></tr>"
								   + $"<tr><td>Presentation Date</td><td>{presentationDate.ToShortDateString()}</td></tr>"
								   + "</table>";
			AssertEmailWithTable("Email", email, expectedEmailRecipientsMailAddress, subject, bodyMessageTitle, bodyMessageHeader, bodyMessageSummary, bodyMessageTable);
		}

		void AssertAcknowledgeEmail(EmailDef email, CusTempStorageJobHeader jobHeader, ZString[] expectedEmailRecipientsMailAddress)
		{
			var reference = jobHeader.SJH_JobReference;
			var subject = $"SumA CUSTST - Temporary Storage Response for {reference}";
			var bodyMessageTitle = $"<title>{subject}</title>";
			var bodyMessageHeader = $@"<strong>SumA CUSTST - Temporary Storage Response for <a href=""edient:Command=ShowEditForm&LicenceCode={GlbCompany.CurrentCompany.LicenceKeyIdentifier}&ControllerID=TemporaryStorage&BusinessEntityPK={jobHeader.PK}";
			var bodyMessageSummary = $"Your SumA Declaration {reference} received a Temporary Storage message. For details please follow the link to the SumA Declaration.";
			var bodyMessageTable = @"<table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table"">"
								   + $"<tr><td>MRN</td><td>{MRN}</td></tr>"
								   + $"<tr><td>Registration Number</td><td>{ReferenceNumber}</td></tr>"
								   + $"<tr><td>Local Reference Number</td><td>{LocalReferenceNumber}</td></tr>"
								   + $"<tr><td>Presentation Date</td><td>{presentationDate.ToShortDateString()}</td></tr>"
								   + "</table>";
			AssertEmailWithTable("Email", email, expectedEmailRecipientsMailAddress, subject, bodyMessageTitle, bodyMessageHeader, bodyMessageSummary, bodyMessageTable);
		}
	}
}
