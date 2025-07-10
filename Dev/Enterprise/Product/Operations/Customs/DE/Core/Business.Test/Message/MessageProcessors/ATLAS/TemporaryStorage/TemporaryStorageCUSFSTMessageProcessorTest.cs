using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.DE.Business.Testing;
using Enterprise.Customs.DE.Messaging;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.DE.Business.CusTempStorage.Testing
{
	[TestedType(typeof(TemporaryStorageCUSFSTMessageProcessor))]
	sealed class TemporaryStorageCUSFSTMessageProcessorTest : MessageProcessorAbstractTest<TemporaryStorageCUSFSTMessageProcessor, AtlasInboundEDIMessage<IUnderCustomsControl>>
	{
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
			messageMock.Setup(m => m.DataProvider).Returns((IUnderCustomsControl)null);
			AssertNoExceptionThrown(() => ProcessMessage(message));
		}

		public void TestNoDeclarationOrRegHeaderLocated()
		{
			ProcessMessage(message);

			CombineAssertions(() =>
			{
				var headerQuery = new ZQuery(CusTempStorageRegHeaderSchema.SRH_Reference, ReferenceNumber);
				var regHeader = Factory.Load<CusTempStorageRegHeader>(headerQuery).Single();
				AssertEquals("regHeader.SRH_ArrivalDate", new ZDate(2020, 2, 17), regHeader.SRH_ArrivalDate);
				AssertEquals("regHeader.SRH_PresentationDate", new ZDate(2019, 05, 22), regHeader.SRH_PresentationDate);
				AssertEquals("regHeader.SRH_PreviousReferenceType", "T-", regHeader.SRH_PreviousReferenceType);
				AssertEquals("regHeader.SRH_PreviousReference", "19DE587500026773M4", regHeader.SRH_PreviousReference);
				AssertEquals("regHeader.SRH_Status", CustomsStatusList.Codes.FIN, regHeader.SRH_Status);
				AssertEquals("regHeader.SRH_CustomsOffice", "DE005875", regHeader.SRH_CustomsOffice);
				AssertEquals("regHeader.SRH_InternalReference", "19DE587500026775M6", regHeader.SRH_InternalReference);
				AssertDocumentLinkingSubscribers(new BusinessObject[] { regHeader });

				var regline = (CusTempStorageRegLine)regHeader.CusTempStorageRegLines.Single();
				AssertEquals("regline.SRL_LineNumber", 11, regline.SRL_LineNumber);
				AssertEquals("regline.SRL_OwnerReferenceType", "ZZZ", regline.SRL_OwnerReferenceType);
				AssertEquals("regline.SRL_OwnerReference", "NCTS-PosNr: 00011;", regline.SRL_OwnerReference);
				AssertEquals("regline.SRL_LocationOfGoods", "2", regline.SRL_LocationOfGoods);
				AssertEquals("regline.SRL_GoodsDescription", "elektronische Untersuchungsgeräte", regline.SRL_GoodsDescription);
				AssertEquals("regline.SRL_PackagesRemaining", 0, regline.SRL_PackagesRemaining);
				AssertEquals("regline.SRL_PackageType", "CS", regline.SRL_PackageType);
				AssertEquals("regline.SRL_CustodianIdentifier", "DE8999783", regline.SRL_CustodianIdentifier);
				AssertEquals("regline.SRL_CustodianIdentifierBranchNo", "0000", regline.SRL_CustodianIdentifierBranchNo);
				AssertEquals("regline.SRL_GoodsOwnerIdentifier", "GR1-Z", regline.SRL_GoodsOwnerIdentifier);
				AssertEquals("regline.SRL_GoodsOwnerIdentifierBranchNo", "0104", regline.SRL_GoodsOwnerIdentifierBranchNo);
				AssertEquals("regline.SRL_GrossWeightUQ", "KGM", regline.SRL_GrossWeightUQ);
				AssertEquals("regline.SRL_CustomsStatus", CustomsStatusList.Codes.FIN, regline.SRL_CustomsStatus);
				AssertEquals("regline.SRL_LimitDate", ZDate.Empty, regline.SRL_LimitDate);
				AssertEquals("regline.SRL_UnionStatus", "C", regline.SRL_UnionStatus);

				var openingTransactionLine = TempStorageTestHelpers.GetRegLineTransaction(regline, TransactionTypes.Codes.OpeningBalance);
				AssertEquals("openingTransactionLine.SRT_GrossWeight", 5645m, openingTransactionLine.SRT_GrossWeight);
				AssertEquals("openingTransactionLine.SRT_PackageQty", 10, openingTransactionLine.SRT_PackageQty);
				AssertEquals("openingTransactionLine.SRT_InternalReferenceNumber", MessageIdentifier, openingTransactionLine.SRT_InternalReferenceNumber);
				AssertEquals("openingTransactionLine.SRT_Comments", "19DE587500026775M6", openingTransactionLine.SRT_Comments);

				var transactionLine = TempStorageTestHelpers.GetRegLineTransaction(regline, TransactionTypes.Codes.Transaction);
				AssertEquals("transactionLine.SRT_GrossWeight", 5645m, transactionLine.SRT_GrossWeight);
				AssertEquals("transactionLine.SRT_PackageQty", -10, transactionLine.SRT_PackageQty);
				AssertEquals("transactionLine.SRT_InternalReferenceNumber", MessageIdentifier, transactionLine.SRT_InternalReferenceNumber);
				AssertEquals("transactionLine.SRT_Comments", "19DE587500026775M6", transactionLine.SRT_Comments);

				AssertEquals("message.EM_LinkedObject", regHeader, message.EM_LinkedObject);
			});
		}

		public void TestExistingDeclaration_NoRegHeader()
		{
			var declaration = CUSPRLCusTempStorageDec.New(Factory.NewWithValidTestData<CusTempStorageJobHeader>());
			CreateOriginalMessageLinkedToParent<AtlasEDIMessage>(declaration, ReferencedMessageIdentifier);
			var line1 = declaration.CusTempStorageLines.AddNew();
			line1.TSL_LineNo = 1;
			var line11 = declaration.CusTempStorageLines.AddNew();
			line11.TSL_LineNo = 11;
			Factory.Save();

			ProcessMessage(message);
			CombineAssertions(() =>
			{
				AssertDocumentLinkingSubscribers(new BusinessObject[] { declaration.StorageHeader, CusTempStorageRegHeader.Load(Factory, ReferenceNumber) });
				AssertEquals("declaration.STH_MessageStatus", EDIMessageStatusList.Codes.ProcessedOK, declaration.STH_MessageStatus);
				AssertEquals("declaration.ReferenceNumber", "AT/B/15/000211/05/2019/5875", declaration.ReferenceNumber);
				AssertEquals("line1.TSL_CustomsStatus", ZString.Empty, line1.TSL_CustomsStatus);
				AssertEquals("line11.TSL_CustomsStatus", CustomsStatusList.Codes.FIN, line11.TSL_CustomsStatus);
				AssertEquals("Update Status Log", true, logger.DebugLogStrings.Cast<string>().Any(x => x.Contains("Update the status to FIN on line number 11")));
			});
		}

		public void TestExistingDeclaration_NoRegHeader_UpdateFromMRN()
		{
			dataProviderMock.Setup(m => m.ReferenceNumber).Returns(ZString.Empty);

			var declaration = CUSPRLCusTempStorageDec.New(Factory.NewWithValidTestData<CusTempStorageJobHeader>());
			CreateOriginalMessageLinkedToParent<AtlasEDIMessage>(declaration, ReferencedMessageIdentifier);
			var line1 = declaration.CusTempStorageLines.AddNew();
			line1.TSL_LineNo = 1;
			var line11 = declaration.CusTempStorageLines.AddNew();
			line11.TSL_LineNo = 11;
			Factory.Save();

			ProcessMessage(message);
			CombineAssertions(() =>
			{
				AssertDocumentLinkingSubscribers(new BusinessObject[] { declaration.StorageHeader, CusTempStorageRegHeader.Load(Factory, MRN) });
				AssertEquals("declaration.STH_MessageStatus", EDIMessageStatusList.Codes.ProcessedOK, declaration.STH_MessageStatus);
				AssertEquals("declaration.ReferenceNumber", MRN, declaration.ReferenceNumber);
				AssertEquals("line1.TSL_CustomsStatus", ZString.Empty, line1.TSL_CustomsStatus);
				AssertEquals("line11.TSL_CustomsStatus", CustomsStatusList.Codes.FIN, line11.TSL_CustomsStatus);
				AssertEquals("Update Status Log", true, logger.DebugLogStrings.Cast<string>().Any(x => x.Contains("Update the status to FIN on line number 11")));
			});
		}

		public void TestUpdateDeclaration_ExistingRegHeaderWithRegLine()
		{
			var declaration = CUSPRLCusTempStorageDec.New(Factory.NewWithValidTestData<CusTempStorageJobHeader>());
			CreateOriginalMessageLinkedToParent<AtlasEDIMessage>(declaration, ReferencedMessageIdentifier);
			var line1 = declaration.CusTempStorageLines.AddNew();
			line1.TSL_LineNo = 1;
			var line11 = declaration.CusTempStorageLines.AddNew();
			line11.TSL_LineNo = 11;
			var regHeader = Factory.NewWithValidTestData<CusTempStorageRegHeader>();
			regHeader.SRH_Reference = ReferenceNumber;
			var regLine = regHeader.CusTempStorageRegLines.AddNew();
			regLine.SRL_LineNumber = 11;
			Factory.Save();

			ProcessMessage(message);
			CombineAssertions(() =>
			{
				AssertEquals("declaration.STH_MessageStatus", EDIMessageStatusList.Codes.ProcessedOK, declaration.STH_MessageStatus);
				AssertEquals("declaration.ReferenceNumber", "AT/B/15/000211/05/2019/5875", declaration.ReferenceNumber);
				AssertEquals("line1.TSL_CustomsStatus", ZString.Empty, line1.TSL_CustomsStatus);
				AssertEquals("line11.TSL_CustomsStatus", CustomsStatusList.Codes.FIN, line11.TSL_CustomsStatus);
				AssertEquals("regLine.SRL_GoodsDescription", "elektronische Untersuchungsgeräte", regLine.SRL_GoodsDescription);
				AssertEquals("Update Status Log", true, logger.DebugLogStrings.Cast<string>().Any(x => x.Contains("Update the status to FIN on line number 11")));
			});
		}

		public void TestUpdateDeclaration_ExistingRegHeaderWithRegLine_UpdateFromMRN()
		{
			dataProviderMock.Setup(m => m.ReferenceNumber).Returns(ZString.Empty);

			var declaration = CUSPRLCusTempStorageDec.New(Factory.NewWithValidTestData<CusTempStorageJobHeader>());
			CreateOriginalMessageLinkedToParent<AtlasEDIMessage>(declaration, ReferencedMessageIdentifier);
			var line1 = declaration.CusTempStorageLines.AddNew();
			line1.TSL_LineNo = 1;
			var line11 = declaration.CusTempStorageLines.AddNew();
			line11.TSL_LineNo = 11;
			var regHeader = Factory.NewWithValidTestData<CusTempStorageRegHeader>();
			regHeader.SRH_Reference = MRN;
			var regLine = regHeader.CusTempStorageRegLines.AddNew();
			regLine.SRL_LineNumber = 11;
			Factory.Save();

			ProcessMessage(message);
			CombineAssertions(() =>
			{
				AssertEquals("declaration.STH_MessageStatus", EDIMessageStatusList.Codes.ProcessedOK, declaration.STH_MessageStatus);
				AssertEquals("declaration.ReferenceNumber", MRN, declaration.ReferenceNumber);
				AssertEquals("line1.TSL_CustomsStatus", ZString.Empty, line1.TSL_CustomsStatus);
				AssertEquals("line11.TSL_CustomsStatus", CustomsStatusList.Codes.FIN, line11.TSL_CustomsStatus);
				AssertEquals("regLine.SRL_GoodsDescription", "elektronische Untersuchungsgeräte", regLine.SRL_GoodsDescription);
				AssertEquals("Update Status Log", true, logger.DebugLogStrings.Cast<string>().Any(x => x.Contains("Update the status to FIN on line number 11")));
			});
		}

		public void TestUpdateTSL_CustomsStatusOfDeclarationsWithMatchingATBNumber_MixedDeclarations()
		{
			var header = Factory.New<CusTempStorageJobHeader>();
			header.SJH_JobReference = "DECUSPRL001";
			var cusprlDeclaration = CUSPRLCusTempStorageDec.New(header);
			cusprlDeclaration.ReferenceNumber = ReferenceNumber;
			var cusprlLine11 = cusprlDeclaration.CusTempStorageLines.AddNew();
			cusprlLine11.TSL_LineNo = 11;

			var chogffDeclaration = header.CHGOFFCusTempStorageDecs.AddNew();
			chogffDeclaration.ReferenceNumber = ReferenceNumber;
			var chgoffLine11 = chogffDeclaration.CusTempStorageLines.AddNew();
			chgoffLine11.TSL_LineNo = 11;

			ProcessMessage(message);
			CombineAssertions(() =>
			{
				AssertEquals("CUSPRL Line status updated", CustomsStatusList.Codes.FIN, cusprlLine11.TSL_CustomsStatus);
				AssertEquals("CHGOFF Line status not uUpdated", ZString.Empty, chgoffLine11.TSL_CustomsStatus);
				AssertEquals("Update Status Log", true, logger.DebugLogStrings.Cast<string>().Any(x => x.Contains("Update the status to FIN on line number 11 for Temporary Storage Header DECUSPRL001")));
			});
		}

		public void TestUpdateTSL_CustomsStatusOfDeclarationsWithMatchingATBNumber_MultipleCUSPRL()
		{
			var goodsItemMock14 = CreateGoodsItemMock("14");
			var goodsItemMock15 = CreateGoodsItemMock("15");
			dataProviderMock.Setup(m => m.GoodsItems).Returns(new IUnderCustomsControlGoodsItem[] { goodsItemMock.Object, goodsItemMock14.Object, goodsItemMock15.Object });

			var declaration1 = CUSPRLCusTempStorageDec.New(Factory.NewWithValidTestData<CusTempStorageJobHeader>());
			CreateOriginalMessageLinkedToParent<AtlasEDIMessage>(declaration1, ReferencedMessageIdentifier);
			var line10 = declaration1.CusTempStorageLines.AddNew();
			line10.TSL_LineNo = 10;
			var line11 = declaration1.CusTempStorageLines.AddNew();
			line11.TSL_LineNo = 11;

			var declaration2 = CUSPRLCusTempStorageDec.New(Factory.NewWithValidTestData<CusTempStorageJobHeader>());
			declaration2.ReferenceNumber = ReferenceNumber;
			var line13 = declaration2.CusTempStorageLines.AddNew();
			line13.TSL_LineNo = 13;

			var declaration3 = CUSPRLCusTempStorageDec.New(Factory.NewWithValidTestData<CusTempStorageJobHeader>());
			declaration3.ReferenceNumber = ReferenceNumber;
			var line14 = declaration3.CusTempStorageLines.AddNew();
			line14.TSL_LineNo = 14;
			var line15 = declaration3.CusTempStorageLines.AddNew();
			line15.TSL_LineNo = 15;

			var declaration4 = CUSPRLCusTempStorageDec.New(Factory.NewWithValidTestData<CusTempStorageJobHeader>());
			declaration4.ReferenceNumber = "ATB150002110520195876";
			var differentReference_line11 = declaration4.CusTempStorageLines.AddNew();
			differentReference_line11.TSL_LineNo = 11;

			ProcessMessage(message);
			CombineAssertions(() =>
			{
				AssertEquals("Linked declaration's ATBNumber matches message's ATBNumber", ReferenceNumber, declaration1.CusEntryNumber.CE_EntryNum);
				AssertEquals("line10", ZString.Empty, line10.TSL_CustomsStatus);
				AssertEquals("Line11", CustomsStatusList.Codes.FIN, line11.TSL_CustomsStatus);
				AssertEquals("Line13", ZString.Empty, line13.TSL_CustomsStatus);
				AssertEquals("Line14", CustomsStatusList.Codes.FIN, line14.TSL_CustomsStatus);
				AssertEquals("Line15", CustomsStatusList.Codes.FIN, line15.TSL_CustomsStatus);
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
			cusprlLine1.TSL_LineNo = 11;

			var header2 = Factory.New<CusTempStorageJobHeader>();
			header2.SJH_JobReference = "DECUSPRL002";
			var cusprlDeclaration2 = CUSPRLCusTempStorageDec.New(header2);
			cusprlDeclaration2.ReferenceNumber = ReferenceNumber;
			var cusprlLine2 = cusprlDeclaration2.CusTempStorageLines.AddNew();
			cusprlLine2.TSL_LineNo = 11;

			ProcessMessage(message);
			CombineAssertions(() =>
			{
				AssertEquals("Line1 status updated", CustomsStatusList.Codes.FIN, cusprlLine1.TSL_CustomsStatus);
				AssertEquals("Line2 status updated", CustomsStatusList.Codes.FIN, cusprlLine2.TSL_CustomsStatus);
				AssertEquals("Update Status Log for Header: DECUSPRL001", true, logger.DebugLogStrings.Cast<string>().Any(x => x.Contains("Update the status to FIN on line number 11 for Temporary Storage Header DECUSPRL001")));
				AssertEquals("Update Status Log for Header: DECUSPRL002", true, logger.DebugLogStrings.Cast<string>().Any(x => x.Contains("Update the status to FIN on line number 11 for Temporary Storage Header DECUSPRL002")));
			});
		}

		[TestDate(2020, 2, 16)]
		public void TestExistingRegHeader()
		{
			var regHeader = Factory.New<CusTempStorageRegHeader>();
			regHeader.SRH_Reference = ReferenceNumber;
			regHeader.SRH_ArrivalDate = ZDate.Today;
			var regLine = regHeader.CusTempStorageRegLines.AddNew();
			regLine.SRL_LineNumber = 11;
			regLine.SRL_LocationOfGoods = Core.Constants.CountryCodes.Germany;
			regLine.SRL_LimitDate = ZDate.Today;
			regLine.SRL_GrossWeightUQ = "KGM";
			var openingBalanceTransaction = regLine.CusTempStorageRegLineTransactions.AddNew();
			openingBalanceTransaction.SRT_GrossWeight = 6598;
			openingBalanceTransaction.SRT_PackageQty = 12;
			openingBalanceTransaction.SRT_TransactionType = TransactionTypes.Codes.OpeningBalance;
			Factory.Save();

			ProcessMessage(message);

			CombineAssertions(() =>
			{
				AssertDocumentLinkingSubscribers(new BusinessObject[] { regHeader });
				AssertEquals("regHeader.SRH_ArrivalDate", new ZDate(2020, 2, 17), regHeader.SRH_ArrivalDate);
				AssertEquals("regHeader.SRH_Status", ZString.Empty, regHeader.SRH_Status);
				AssertEquals("regHeader.SRH_InternalReference", "19DE587500026775M6", regHeader.SRH_InternalReference);

				AssertEquals("regLine.SRL_PackagesRemaining", 12, regLine.SRL_PackagesRemaining);
				AssertEquals("regLine.SRL_PackageType", "CS", regLine.SRL_PackageType);
				AssertEquals("regLine.SRL_CustomsStatus", ZString.Empty, regLine.SRL_CustomsStatus);
				AssertEquals("regLine.CusTempStorageRegLineTransactions.Count", 1, regLine.CusTempStorageRegLineTransactions.Count);

				AssertEquals("openingBalanceTransaction.SRT_PackageQty", 12, openingBalanceTransaction.SRT_PackageQty);
				AssertEquals("openingBalanceTransaction.SRT_GrossWeight", 6598m, openingBalanceTransaction.SRT_GrossWeight);
			});
		}

		public void TestPopulateLogbookRegNum()
		{
			ProcessMessage(message);
			AssertContainsExactElementsInAnyOrder(new[] { ReferenceNumber, MRN }, message.GetLogbookRegistrationNumbers());
		}

		public void TestSendAcknownledgementEMail_StaffMember()
		{
			var jobHeader = Factory.NewWithValidTestData<CusTempStorageJobHeader>();
			jobHeader.SJH_JobReference = "JobReference";
			CreateOriginalMessageLinkedToParent<AtlasEDIMessage>(CUSPRLCusTempStorageDec.New(jobHeader), ReferencedMessageIdentifier, "senduser@mail.com");
			var groupNotificationRegistryItem = new EU.Registry.TemporaryStorageGroupNotification(Core.Constants.EmailTo.StaffMember, ZGuid.Empty);
			using (EU.Registry.EUCustomsDataRegistry.Instance.SendTemporaryStorageAcknowledgements.SetTemporaryValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, groupNotificationRegistryItem))
			{
				ProcessMessage(message);
				AssertAcknowledgeEmail(jobHeader, new ZString[] { "senduser@mail.com" });
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
				AssertAcknowledgeEmail(jobHeader, new ZString[] { "staff1@group-suma.com", "staff2@group-suma.com" });
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
				AssertAcknowledgeEmail(jobHeader, new ZString[] { "senduser@mail.com", "staff1@group-suma.com", "staff2@group-suma.com" });
			}
		}

		public void TestSendUnsolicitedMessageEMail_NominatedGroup()
		{
			var regHeader = Factory.New<CusTempStorageRegHeader>();
			regHeader.SRH_Reference = ReferenceNumber;
			var groupNotificationRegistryItem = new EU.Registry.TemporaryStorageUnsolicitedGroupNotification(Core.Constants.EmailTo.NominatedGroup, TempStorageTestHelpers.CreateGlbGroup(Factory).PK);
			using (EU.Registry.EUCustomsDataRegistry.Instance.SendTemporaryStorageUnsolicited.SetTemporaryValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, groupNotificationRegistryItem))
			{
				ProcessMessage(message);
				AssertUnsolicitedMessageEmail(regHeader, new ZString[] { "staff1@group-suma.com", "staff2@group-suma.com" });
			}
		}

		public void TestSendUnsolicitedMessageEMailForNewlyCreatedRegHeaderAsLinkedObject_NominatedGroup()
		{
			var groupNotificationRegistryItem = new EU.Registry.TemporaryStorageUnsolicitedGroupNotification(Core.Constants.EmailTo.NominatedGroup, TempStorageTestHelpers.CreateGlbGroup(Factory).PK);
			using (EU.Registry.EUCustomsDataRegistry.Instance.SendTemporaryStorageUnsolicited.SetTemporaryValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, groupNotificationRegistryItem))
			{
				ProcessMessage(message);
				var headerQuery = new ZQuery(CusTempStorageRegHeaderSchema.SRH_Reference, ReferenceNumber);
				var regHeader = Factory.Load<CusTempStorageRegHeader>(headerQuery).Single();
				AssertUnsolicitedMessageEmail(regHeader, new ZString[] { "staff1@group-suma.com", "staff2@group-suma.com" });
			}
		}

		protected override ZString MessageFriendlyName => "Temporary Storage CUSFST Message Processor";

		protected override DEBranchCustomsApplicationTypeMessageProcessor<AtlasInboundEDIMessage<IUnderCustomsControl>> Processor => new TemporaryStorageCUSFSTMessageProcessor(logger);

		protected override bool ExpectedMustHaveLinkedObject => false;

		protected override void SetUp()
		{
			base.SetUp();

			goodsItemMock = CreateGoodsItemMock("11");

			dataProviderMock = new Mock<IUnderCustomsControl>();
			dataProviderMock.Setup(m => m.MessageIdentifier).Returns(MessageIdentifier);
			dataProviderMock.Setup(m => m.ArrivalDate).Returns(new ZDate(2020, 2, 17));
			dataProviderMock.Setup(m => m.PresentationDate).Returns(presentationDate);
			dataProviderMock.Setup(m => m.ReferenceNumber).Returns(ReferenceNumber);
			dataProviderMock.Setup(m => m.MRN).Returns(MRN);
			dataProviderMock.Setup(m => m.LocalReferenceNumber).Returns(LocalReferenceNumber);
			dataProviderMock.Setup(m => m.PreviousReferenceType).Returns(PreviousReferenceType.Codes._T);
			dataProviderMock.Setup(m => m.PreviousReferenceNumber).Returns("19DE587500026773M4");
			dataProviderMock.Setup(m => m.CustomsOfficeReferenceNumber).Returns("DE005875");
			dataProviderMock.Setup(m => m.ReferencedMessageIdentifier).Returns(ReferencedMessageIdentifier);
			dataProviderMock.Setup(m => m.GoodsItems).Returns(new IUnderCustomsControlGoodsItem[] { goodsItemMock.Object });

			messageMock = Factory.NewMoq<AtlasInboundEDIMessage<IUnderCustomsControl>>();
			messageMock.Setup(m => m.DataProvider).Returns(dataProviderMock.Object);
			message = messageMock.Object;
			Factory.Save();
		}
		Mock<IUnderCustomsControlGoodsItem> goodsItemMock;
		Mock<IUnderCustomsControl> dataProviderMock;
		Mock<AtlasInboundEDIMessage<IUnderCustomsControl>> messageMock;
		AtlasInboundEDIMessage<IUnderCustomsControl> message;

		const string ReferenceNumber = "ATB150002110520195875";
		const string MRN = "23DE586601055987B7";
		const string LocalReferenceNumber = "19DE587500026775M6";
		const string MessageIdentifier = "CUSFST58750000000514210220519160601";
		const string ReferencedMessageIdentifier = "DE899978300000000812";
		readonly ZDate presentationDate = new ZDate(2019, 05, 22);

		Mock<IUnderCustomsControlGoodsItem> CreateGoodsItemMock(ZString sequenceNumber)
		{
			var result = new Mock<IUnderCustomsControlGoodsItem>();
			result.Setup(m => m.SequenceNumber).Returns(sequenceNumber);
			result.Setup(m => m.OwnerReferenceType).Returns("ZZZ");
			result.Setup(m => m.OwnerReferenceNumber).Returns("NCTS-PosNr: 00011;");
			result.Setup(m => m.LocationOfGoods).Returns("2");
			result.Setup(m => m.GoodsDescription).Returns("elektronische Untersuchungsgeräte");
			result.Setup(m => m.PackageType).Returns("CS");
			result.Setup(m => m.PackageQty).Returns(10);
			result.Setup(m => m.CustodianReferenceNumber).Returns("DE8999783");
			result.Setup(m => m.CustodianSubsidiaryNumber).Returns("0000");
			result.Setup(m => m.DisposalEntitledTraderReferenceNumber).Returns("GR1-Z");
			result.Setup(m => m.DisposalEntitledTraderSubsidiaryNumber).Returns("0104");
			result.Setup(m => m.GrossWeight).Returns(5645);
			result.Setup(m => m.LimitDate).Returns(ZDate.Empty);
			result.Setup(m => m.CustomsGoodsStatus).Returns("C");
			return result;
		}

		void AssertAcknowledgeEmail(CusTempStorageJobHeader jobHeader, ZString[] expectedEmailRecipientsMailAddress)
		{
			var email = Env.OutgoingCustomsMailManager.EmailsCreated.Single();
			var reference = jobHeader.SJH_JobReference;
			var subject = $"SumA CUSFST – Information on Completed C, X, D or Free Zone Goods Response for {reference}";
			var bodyMessageTitle = $"<title>{subject}</title>";
			var bodyMessageHeader = $@"<strong>SumA CUSFST – Information on Completed C, X, D or Free Zone Goods Response for <a href=""edient:Command=ShowEditForm&LicenceCode={GlbCompany.CurrentCompany.LicenceKeyIdentifier}&ControllerID=TemporaryStorage&BusinessEntityPK={jobHeader.PK}";
			var bodyMessageSummary = $"Your SumA Declaration {reference} received an Information on Completed C, X, D or Free Zone Goods. For details please follow the link to the SumA Declaration.";
			var bodyMessageTable = @"<table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table"">"
								   + $"<tr><td>MRN</td><td>{MRN}</td></tr>"
								   + $"<tr><td>Registration Number</td><td>{ReferenceNumber}</td></tr>"
								   + $"<tr><td>Local Reference Number</td><td>{LocalReferenceNumber}</td></tr>"
								   + $"<tr><td>Presentation Date</td><td>{presentationDate.ToShortDateString()}</td></tr>"
								   + "</table>";
			CombineAssertions(() => AssertEmailWithTable(ZString.Empty, email, expectedEmailRecipientsMailAddress, subject, bodyMessageTitle, bodyMessageHeader, bodyMessageSummary, bodyMessageTable));
		}

		void AssertUnsolicitedMessageEmail(CusTempStorageRegHeader regHeader, ZString[] expectedEmailRecipientsMailAddress)
		{
			var email = Env.OutgoingCustomsMailManager.EmailsCreated.Single();
			var reference = regHeader.SRH_Reference;
			var subject = $"SumA CUSFST – Information on Completed C, X, D or Free Zone Goods Response for {reference}";
			var bodyMessageTitle = $"<title>{subject}</title>";
			var bodyMessageHeader = $@"<strong>SumA CUSFST – Information on Completed C, X, D or Free Zone Goods Response for <a href=""edient:Command=ShowEditForm&LicenceCode={GlbCompany.CurrentCompany.LicenceKeyIdentifier}&ControllerID=DESumARegister&BusinessEntityPK={regHeader.PK}";
			var bodyMessageSummary = $"Your SumA Register {reference} received an Information on Completed C, X, D or Free Zone Goods. For details please follow the link to the SumA Register.";
			var bodyMessageTable = @"<table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table"">"
								   + $"<tr><td>MRN</td><td>{MRN}</td></tr>"
								   + $"<tr><td>Registration Number</td><td>{ReferenceNumber}</td></tr>"
								   + $"<tr><td>Local Reference Number</td><td>{LocalReferenceNumber}</td></tr>"
								   + $"<tr><td>Presentation Date</td><td>{presentationDate.ToShortDateString()}</td></tr>"
								   + "</table>";
			CombineAssertions(() => AssertEmailWithTable(ZString.Empty, email, expectedEmailRecipientsMailAddress, subject, bodyMessageTitle, bodyMessageHeader, bodyMessageSummary, bodyMessageTable));
		}
	}
}
