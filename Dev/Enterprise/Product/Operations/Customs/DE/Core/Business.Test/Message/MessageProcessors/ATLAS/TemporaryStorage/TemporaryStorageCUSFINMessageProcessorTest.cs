using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.DE.Business.Testing;
using Enterprise.Customs.DE.Messaging;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.DE.Business.CusTempStorage.Testing
{
	[TestedType(typeof(TemporaryStorageCUSFINMessageProcessor))]
	sealed class TemporaryStorageCUSFINMessageProcessorTest : MessageProcessorAbstractTest<TemporaryStorageCUSFINMessageProcessor, AtlasInboundEDIMessage<ICUSFIN>>
	{
		public void TestGetLinkedObject()
		{
			ProcessMessage(message);
			AssertNull(message.EM_LinkedObject);
		}

		public void TestDataProviderNullDueToInvalidMessage()
		{
			messageMock.Setup(m => m.DataProvider).Returns((ICUSFIN)null);
			AssertNoExceptionThrown(() => ProcessMessage(message));
		}

		public void TestShouldUpdateRegister_RegLinesCustodianIsCW1MessagingOrganisationAndInterchangeRecipient()
		{
			ProcessMessage(message);
			CombineAssertions(() =>
			{
				AssertEquals("Message status", EDIMessage.Status.ProcessedOK, message.EM_Status);
				AssertEquals("LogbookRegNum", ExpectedLogbookRegNumWithAllRegistrationNumbers, message.GetLogbookRegistrationNumber());
				AssertEquals("ReglineTransaction created", 2, regLine.CusTempStorageRegLineTransactions.Count);
				AssertEquals("RegLine status updated", CustomsStatusList.Codes.PAC, regLine.SRL_CustomsStatus);
				AssertDocumentLinkingSubscribers(new BusinessObject[] { regHeader });
			});
		}

		public void TestShouldUpdateRegister_RegLinesCustodianIsNotCW1MessagingOrganisation()
		{
			regLine.SRL_CustodianIdentifier = "FR123456";

			ProcessMessage(message);
			CombineAssertions(() =>
			{
				AssertEquals("Message status", EDIMessage.Status.ProcessedOK, message.EM_Status);
				AssertEquals("LogbookRegNum", ExpectedLogbookRegNumWithAllRegistrationNumbers, message.GetLogbookRegistrationNumber());
				AssertEquals("ReglineTransaction created", 2, regLine.CusTempStorageRegLineTransactions.Count);
				AssertEquals("RegLine status updated", CustomsStatusList.Codes.PAC, regLine.SRL_CustomsStatus);
				AssertDocumentLinkingSubscribers(new BusinessObject[] { regHeader });
			});
		}

		public void TestShouldNotUpdateRegister_RegLinesCustodianIsCW1MessagingOrganisationAndNotInterchangeRecipient()
		{
			CombineAssertions(() =>
			{
				dataProviderMock.Setup(m => m.InterchangeRecipientReferenceNumber).Returns("FR123456");
				ProcessMessage(message);
				AssertEquals("InterchangeRecipients EORINumber does not match RegLineCustodians EORINumber: message status", EDIMessage.Status.ProcessedOK, message.EM_Status);
				AssertEquals("InterchangeRecipients EORINumber does not match RegLineCustodians EORINumber: no reglineTransaction added", 1, regLine.CusTempStorageRegLineTransactions.Count);
				AssertEquals("InterchangeRecipients EORINumber does not match RegLineCustodians EORINumber: LogbookRegNum", ExpectedLogbookRegNumWithAllRegistrationNumbers, message.GetLogbookRegistrationNumber());

				dataProviderMock.Setup(m => m.InterchangeRecipientReferenceNumber).Returns(EoriCode);
				dataProviderMock.Setup(m => m.InterchangeRecipientSubsidiaryNumber).Returns("0002");
				message.EM_Status = ZString.Empty;
				ProcessMessage(message);
				AssertEquals("InterchangeRecipients EORIBranch does not match RegLineCustodians EORIBranch: message status", EDIMessage.Status.ProcessedOK, message.EM_Status);
				AssertEquals("InterchangeRecipients EORIBranch does not match RegLineCustodians EORIBranch: no reglineTransaction added", 1, regLine.CusTempStorageRegLineTransactions.Count);
				AssertEquals("InterchangeRecipients EORIBranch does not match RegLineCustodians EORIBranch: LogbookRegNum", ExpectedLogbookRegNumWithAllRegistrationNumbers, message.GetLogbookRegistrationNumber());
			});
		}

		public void TestUpdateTSL_CustomsStatusOfDeclarationWithMatchingATBNumber()
		{
			var goodsItemMock2 = CreateGoodsItemMock("2", ReferencedRegistrationNumber, 10);
			dataProviderMock.Setup(m => m.GoodsItems).Returns(new ICUSFINGoodsItem[] { goodsItemMock.Object, goodsItemMock2.Object });

			var line2 = cusprlDeclaration.CusTempStorageLines.AddNew();
			line2.TSL_LineNo = 2;
			var regLine2 = regHeader.CusTempStorageRegLines.AddNew();
			regLine2.SRL_LineNumber = 2;

			ProcessMessage(message);
			CombineAssertions(() =>
			{
				AssertEquals("Line2 status updated", CustomsStatusList.Codes.FIN, line2.TSL_CustomsStatus);
				AssertEquals("Line3 status updated", CustomsStatusList.Codes.PAC, defaultLine3.TSL_CustomsStatus);
				AssertEquals("Update Status Log line2", true, logger.DebugLogStrings.Cast<string>().Any(x => x.Contains("Update the status to FIN on line number 2 for Temporary Storage Header DECUSPRL001")));
				AssertEquals("Update Status Log line3", true, logger.DebugLogStrings.Cast<string>().Any(x => x.Contains("Update the status to PAC on line number 3 for Temporary Storage Header DECUSPRL001")));
			});
		}

		public void TestUpdateTSL_CustomsStatusOfDeclarationWithMatchingMRN()
		{
			dataProviderMock.Setup(m => m.AdditionalRegistrationNumber).Returns(ZString.Empty);
			dataProviderMock.Setup(m => m.MRN).Returns(MRN);
			goodsItemMock.Setup(m => m.ReferencedRegistrationNumber).Returns(ZString.Empty);
			goodsItemMock.Setup(m => m.MRN).Returns(MRN);

			var goodsItemMock2 = CreateGoodsItemMock("2", MRN, 10);
			dataProviderMock.Setup(m => m.GoodsItems).Returns(new ICUSFINGoodsItem[] { goodsItemMock.Object, goodsItemMock2.Object });

			cusprlDeclaration.ReferenceNumber = MRN;
			regHeader.SRH_Reference = MRN;
			var line2 = cusprlDeclaration.CusTempStorageLines.AddNew();
			line2.TSL_LineNo = 2;
			var regLine2 = regHeader.CusTempStorageRegLines.AddNew();
			regLine2.SRL_LineNumber = 2;

			ProcessMessage(message);
			CombineAssertions(() =>
			{
				AssertEquals("Line2 status updated", CustomsStatusList.Codes.FIN, line2.TSL_CustomsStatus);
				AssertEquals("Line3 status updated", CustomsStatusList.Codes.PAC, defaultLine3.TSL_CustomsStatus);
				AssertEquals("Update Status Log line2", true, logger.DebugLogStrings.Cast<string>().Any(x => x.Contains("Update the status to FIN on line number 2 for Temporary Storage Header DECUSPRL001")));
				AssertEquals("Update Status Log line3", true, logger.DebugLogStrings.Cast<string>().Any(x => x.Contains("Update the status to PAC on line number 3 for Temporary Storage Header DECUSPRL001")));
			});
		}

		public void TestUpdateTSL_CustomsStatusToFINOfDeclarationsWithMatchingATBNumber_MixedDeclarations()
		{
			var chogffDeclaration = header.CHGOFFCusTempStorageDecs.AddNew();
			chogffDeclaration.ReferenceNumber = ReferencedRegistrationNumber;
			var chgoffLine = chogffDeclaration.CusTempStorageLines.AddNew();
			chgoffLine.TSL_LineNo = 3;

			goodsItemMock.Setup(m => m.Quantity).Returns(10);

			ProcessMessage(message);
			CombineAssertions(() =>
			{
				AssertEquals("CUSPRL Line status updated", CustomsStatusList.Codes.FIN, defaultLine3.TSL_CustomsStatus);
				AssertEquals("CHGOFF Line status not updated", ZString.Empty, chgoffLine.TSL_CustomsStatus);
			});
		}

		public void TestUpdateTSL_CustomsStatusToFINOfDeclarationsWithMatchingATBNumber_MultipleCUSPRL()
		{
			goodsItemMock.Setup(m => m.Quantity).Returns(10);
			var goodsItemMock2 = CreateGoodsItemMock("1", ReferencedRegistrationNumber, 10);
			var goodsItemMock3 = CreateGoodsItemMock("2", ReferencedRegistrationNumber, 10);
			dataProviderMock.Setup(m => m.GoodsItems).Returns(new ICUSFINGoodsItem[] { goodsItemMock.Object, goodsItemMock2.Object, goodsItemMock3.Object });

			var declaration2 = CUSPRLCusTempStorageDec.New(Factory.NewWithValidTestData<CusTempStorageJobHeader>());
			declaration2.ReferenceNumber = ReferencedRegistrationNumber;
			var line2 = declaration2.CusTempStorageLines.AddNew();
			line2.TSL_LineNo = 2;
			var regLine2 = regHeader.CusTempStorageRegLines.AddNew();
			regLine2.SRL_LineNumber = 2;
			var regLineTransaction2 = regLine2.CusTempStorageRegLineTransactions.AddNew();
			regLineTransaction2.SRT_TransactionType = TransactionTypes.Codes.OpeningBalance;
			regLineTransaction2.SRT_PackageQty = 10;
			var line4 = declaration2.CusTempStorageLines.AddNew();
			line4.TSL_LineNo = 4;
			var regLine4 = regHeader.CusTempStorageRegLines.AddNew();
			regLine4.SRL_LineNumber = 4;

			var declaration3 = CUSPRLCusTempStorageDec.New(Factory.NewWithValidTestData<CusTempStorageJobHeader>());
			declaration3.ReferenceNumber = "ATB150002110520195876";
			var differentLine2 = declaration3.CusTempStorageLines.AddNew();
			differentLine2.TSL_LineNo = 2;
			var regHeader1 = Factory.NewWithValidTestData<CusTempStorageRegHeader>();
			regHeader1.SRH_Reference = "ATB150002110520195876";
			var differentRegLine2 = regHeader1.CusTempStorageRegLines.AddNew();
			differentRegLine2.SRL_LineNumber = 2;

			ProcessMessage(message);
			CombineAssertions(() =>
			{
				AssertEquals("Declaration 3 Line2", ZString.Empty, differentLine2.TSL_CustomsStatus);
				AssertEquals("Line2", CustomsStatusList.Codes.FIN, line2.TSL_CustomsStatus);
				AssertEquals("Line3", CustomsStatusList.Codes.FIN, defaultLine3.TSL_CustomsStatus);
				AssertEquals("Line4: Not in Message", ZString.Empty, line4.TSL_CustomsStatus);
			});
		}

		public void TestUpdateTSL_CustomsStatusToFINOfDeclarationsWithMatchingATBNumber_DifferentStorageHeaders()
		{
			var header2 = Factory.New<CusTempStorageJobHeader>();
			header2.SJH_JobReference = "DECUSPRL002";
			var cusprlDeclaration2 = CUSPRLCusTempStorageDec.New(header2);
			cusprlDeclaration2.ReferenceNumber = ReferencedRegistrationNumber;
			var differentLine3 = cusprlDeclaration2.CusTempStorageLines.AddNew();
			differentLine3.TSL_LineNo = 3;

			goodsItemMock.Setup(m => m.Quantity).Returns(10);

			ProcessMessage(message);
			CombineAssertions(() =>
			{
				AssertEquals("Default Line 3 status updated", CustomsStatusList.Codes.FIN, defaultLine3.TSL_CustomsStatus);
				AssertEquals("Different Line 3 status updated", CustomsStatusList.Codes.FIN, differentLine3.TSL_CustomsStatus);
				AssertEquals("Update Status Log for Header: DECUSPRL001", true, logger.DebugLogStrings.Cast<string>().Any(x => x.Contains($"Update the status to FIN on line number 3 for Temporary Storage Header DECUSPRL001")));
				AssertEquals("Update Status Log for Header: DECUSPRL002", true, logger.DebugLogStrings.Cast<string>().Any(x => x.Contains($"Update the status to FIN on line number 3 for Temporary Storage Header DECUSPRL002")));
			});
		}

		public void TestRegLineReferenceAsAdditionalRegistrationNumber()
		{
			var additionalRegistrationNumberCompletionTypes = new ZString[]
			{
				TransactionReferenceTypes.Codes.AUFT,
				TransactionReferenceTypes.Codes.KONS,
				TransactionReferenceTypes.Codes.MANU,
				TransactionReferenceTypes.Codes.ZB,
				TransactionReferenceTypes.Codes.WVV
			};
			AssertRegLineTransactionReference(additionalRegistrationNumberCompletionTypes, AdditionalRegistrationNumber);
		}

		public void TestRegLineReferenceAsAdditionalReferenceNumber()
		{
			var additionalReferenceNumberCompletionTypes = new ZString[]
			{
				TransactionReferenceTypes.Codes.EAS,
				TransactionReferenceTypes.Codes.NCTS,
				"INV"
			};
			AssertRegLineTransactionReference(additionalReferenceNumberCompletionTypes, AdditionalReferenceNumber);
		}

		public void TestRegLineReferenceAsMRN()
		{
			var additionalRegistrationNumberCompletionTypes = new ZString[]
			{
				TransactionReferenceTypes.Codes.AUFT,
				TransactionReferenceTypes.Codes.KONS,
				TransactionReferenceTypes.Codes.MANU,
				TransactionReferenceTypes.Codes.ZB,
				TransactionReferenceTypes.Codes.WVV
			};
			regHeader.SRH_Reference = MRN;
			dataProviderMock.Setup(m => m.MRN).Returns(MRN);
			dataProviderMock.Setup(m => m.AdditionalRegistrationNumber).Returns(ZString.Empty);
			goodsItemMock.Setup(m => m.MRN).Returns(MRN);
			goodsItemMock.Setup(m => m.ReferencedRegistrationNumber).Returns(ZString.Empty);

			AssertRegLineTransactionReference(additionalRegistrationNumberCompletionTypes, MRN);
		}
		public void TestGetCorrectPackageQuantity()
		{
			ProcessMessage(message);
			var transactionLine = TempStorageTestHelpers.GetRegLineTransaction(regLine, TransactionTypes.Codes.Transaction);
			CombineAssertions(() =>
			{
				AssertEquals("Package Quantity is negative value", -1, transactionLine.SRT_PackageQty);
				transactionLine.Delete();
				Factory.Save();

				goodsItemMock.Setup(m => m.CancellationFlag).Returns("J");
				ProcessMessage(message);
				transactionLine = TempStorageTestHelpers.GetRegLineTransaction(regLine, TransactionTypes.Codes.Transaction);
				AssertEquals("Package Quantity is postiive value as cancellation", 1, transactionLine.SRT_PackageQty);
			});
		}

		public void TestSetStatusPartialCompleted()
		{
			ProcessMessage(message);
			AssertRegLineTransactionDetails(-1, 9, CustomsStatusList.Codes.PAC);
			AssertEquals(regHeader.SRH_Status, CustomsStatusList.Codes.PAC);
			AssertDocumentLinkingSubscribers(new BusinessObject[] { regHeader });
		}

		public void TestSetStatusFinalized()
		{
			goodsItemMock.Setup(m => m.Quantity).Returns(10);
			ProcessMessage(message);
			AssertRegLineTransactionDetails(-10, 0, CustomsStatusList.Codes.FIN);
			AssertEquals(regHeader.SRH_Status, CustomsStatusList.Codes.FIN);
		}

		public void TestRegisterLineNotFound()
		{
			regLine.Delete();
			ProcessMessage(message);

			var stmNote = GetStmNote(message);
			CombineAssertions(() =>
			{
				AssertEquals("EM_Status", EDIMessage.Status.ProcessedOK, message.EM_Status);
				AssertEquals("Register Record Note", $"This is the list of Reference Number and Sequence Number system could not locate matching Register records for:\r\nReference Number: ATB150000960420195875; Sequence Number: 3", stmNote.ST_NoteText);
			});
		}

		public void TestRegisterLineNotFound_MRN()
		{
			goodsItemMock.Setup(m => m.ReferencedRegistrationNumber).Returns(ZString.Empty);
			goodsItemMock.Setup(m => m.MRN).Returns(MRN);
			regLine.Delete();
			ProcessMessage(message);

			var stmNote = GetStmNote(message);
			CombineAssertions(() =>
			{
				AssertEquals("EM_Status", EDIMessage.Status.ProcessedOK, message.EM_Status);
				AssertEquals("Register Record Note", $"This is the list of Reference Number and Sequence Number system could not locate matching Register records for:\r\nReference Number: 24DE123050554788M5; Sequence Number: 3", stmNote.ST_NoteText);
			});
		}

		public void TestViolatingSRL_RemainingPackagesConstraint()
		{
			regLineTransaction.Delete();
			ProcessMessage(message);

			var stmNote = GetStmNote(message);

			CombineAssertions(() =>
			{
				AssertEquals("Transaction existing", false, regLine.CusTempStorageRegLineTransactions.Any());
				AssertEquals("SRL_PackagesRemaining should be 0 because the existing transaction was deleted prior to process.", 0, regLine.SRL_PackagesRemaining);
				AssertEquals("MessageStatus", message.EM_Status, EDIMessage.Status.ProcessedOK);
				AssertEquals($"This is the list of Register Lines where Package Qty would cause negative Packages Remaining value:\r\nReference Number: AT/B/15/000096/04/2019/5875; Sequence Number: 3; Packages Remaining: 0; Package Quantity: -1", stmNote.ST_NoteText);
			});
		}

		public void TestPopulateLogBookRegNumWithBothAdditionalNumbersEmpty()
		{
			dataProviderMock.Setup(m => m.AdditionalRegistrationNumber).Returns(ZString.Empty);
			dataProviderMock.Setup(m => m.AdditionalReferenceNumber).Returns(ZString.Empty);
			ProcessMessage(message);
			AssertEquals(ReferencedRegistrationNumber, message.GetLogbookRegistrationNumber());
		}

		public void TestPopulateLogBookRegNumWithAdditionalRegistrationNumberEmpty()
		{
			dataProviderMock.Setup(m => m.AdditionalRegistrationNumber).Returns(ZString.Empty);
			ProcessMessage(message);
			AssertEquals($"{ReferencedRegistrationNumber}, {AdditionalReferenceNumber}", message.GetLogbookRegistrationNumber());
		}

		public void TestPopulateLogBookRegNumWithAdditionalReferenceNumberEmpty()
		{
			dataProviderMock.Setup(m => m.AdditionalReferenceNumber).Returns(ZString.Empty);
			ProcessMessage(message);
			AssertEquals($"{ReferencedRegistrationNumber}, {AdditionalRegistrationNumber}", message.GetLogbookRegistrationNumber());
		}

		public void TestPopulateLogBookRegNumWithMRN()
		{
			dataProviderMock.Setup(m => m.MRN).Returns(MRN);
			goodsItemMock.Setup(m => m.MRN).Returns("24DE123050554788M1");
			ProcessMessage(message);
			AssertContainsExactElementsInAnyOrder(new[] { ReferencedRegistrationNumber, AdditionalReferenceNumber, AdditionalRegistrationNumber, MRN, "24DE123050554788M1" }, message.GetLogbookRegistrationNumbers());
		}

		protected override ZString MessageFriendlyName => "Temporary Storage CUSFIN Message Processor";

		protected override DEBranchCustomsApplicationTypeMessageProcessor<AtlasInboundEDIMessage<ICUSFIN>> Processor => new TemporaryStorageCUSFINMessageProcessor(logger);

		protected override bool ExpectedMustHaveLinkedObject => false;

		protected override void SetUp()
		{
			base.SetUp();

			var custodianOrgHeader = Factory.NewWithValidTestData<OrgHeader>();
			custodianOrgHeader.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, EoriNumber, Core.Constants.CountryCodes.Greece);
			custodianOrgHeader.MainAddress.CustomsCodes.AddNew(GermanyOrgCusCodeInfo.OrgCusCodes.EoriBranchSuffix, BranchNumber, Core.Constants.CountryCodes.Germany);
			custodianOrgHeader.MainAddress.CustomsCodes.AddNew(GermanyOrgCusCodeInfo.OrgCusCodes.ATLASParticipantIdentificationNumber, "API", Core.Constants.CountryCodes.Germany);

			header = Factory.NewWithValidTestData<CusTempStorageJobHeader>();
			header.SJH_JobReference = "DECUSPRL001";
			cusprlDeclaration = CUSPRLCusTempStorageDec.New(header);
			cusprlDeclaration.ReferenceNumber = ReferencedRegistrationNumber;
			defaultLine3 = cusprlDeclaration.CusTempStorageLines.AddNew();
			defaultLine3.TSL_LineNo = 3;

			regHeader = Factory.NewWithValidTestData<CusTempStorageRegHeader>();
			regHeader.SRH_Reference = ReferencedRegistrationNumber;
			regLine = regHeader.CusTempStorageRegLines.AddNew();
			regLine.SRL_LineNumber = 3;
			regLine.SRL_CustodianIdentifier = EoriCode;
			regLine.SRL_CustodianIdentifierBranchNo = BranchNumber;

			regLineTransaction = regLine.CusTempStorageRegLineTransactions.AddNew();
			regLineTransaction.SRT_TransactionType = TransactionTypes.Codes.OpeningBalance;
			regLineTransaction.SRT_PackageQty = 10;

			goodsItemMock = CreateGoodsItemMock("3", ReferencedRegistrationNumber, 1);

			dataProviderMock = new Mock<ICUSFIN>();
			dataProviderMock.Setup(m => m.CustodianReferenceNumber).Returns("DE123456");
			dataProviderMock.Setup(m => m.CustodianSubsidiaryNumber).Returns("0003");
			dataProviderMock.Setup(m => m.InterchangeRecipientReferenceNumber).Returns(EoriCode);
			dataProviderMock.Setup(m => m.InterchangeRecipientSubsidiaryNumber).Returns(BranchNumber);
			dataProviderMock.Setup(m => m.MessageIdentifier).Returns("CUSFIN58750000000381074050419102427");
			dataProviderMock.Setup(m => m.AdditionalRegistrationNumber).Returns(AdditionalRegistrationNumber);
			dataProviderMock.Setup(m => m.AdditionalReferenceNumber).Returns(AdditionalReferenceNumber);
			dataProviderMock.Setup(m => m.CompletionType).Returns(TransactionReferenceTypes.Codes.WVV);
			dataProviderMock.Setup(m => m.GoodsItems).Returns(new ICUSFINGoodsItem[] { goodsItemMock.Object });

			messageMock = Factory.NewMoq<AtlasInboundEDIMessage<ICUSFIN>>();
			messageMock.Setup(m => m.DataProvider).Returns(dataProviderMock.Object);
			message = messageMock.Object;
			Factory.Save();
		}
		CusTempStorageJobHeader header;
		CUSPRLCusTempStorageDec cusprlDeclaration;
		CusTempStorageLine defaultLine3;
		CusTempStorageRegHeader regHeader;
		CusTempStorageRegLine regLine;
		CusTempStorageRegLineTransaction regLineTransaction;
		Mock<ICUSFIN> dataProviderMock;
		Mock<ICUSFINGoodsItem> goodsItemMock;
		Mock<AtlasInboundEDIMessage<ICUSFIN>> messageMock;
		AtlasInboundEDIMessage<ICUSFIN> message;

		Mock<ICUSFINGoodsItem> CreateGoodsItemMock(ZString sequenceNumber, ZString referencedRegistrationNumber, ZInt quantity)
		{
			var result = new Mock<ICUSFINGoodsItem>();
			result.Setup(m => m.Quantity).Returns(quantity);
			result.Setup(m => m.ReferencedRegistrationNumber).Returns(referencedRegistrationNumber);
			result.Setup(m => m.ReferencedSequenceNumber).Returns(sequenceNumber);
			result.Setup(m => m.CancellationFlag).Returns(ZString.Empty);
			return result;
		}

		void AssertRegLineTransactionReference(ZString[] completionTypes, ZString expectedReference)
		{
			CombineAssertions(() =>
			{
				foreach (var completionType in completionTypes)
				{
					dataProviderMock.Setup(m => m.CompletionType).Returns(completionType);

					ProcessMessage(message);
					var transactionLine = TempStorageTestHelpers.GetRegLineTransaction(regLine, TransactionTypes.Codes.Transaction);
					AssertEquals($"{completionType} Reference", expectedReference, transactionLine.SRT_Reference);
					transactionLine.Delete();
					Factory.Save();
				}
			});
		}

		void AssertRegLineTransactionDetails(ZInt expectedPackageQty, ZInt expectedPackagesRemaining, ZString expectedLineCustomsStatus)
		{
			CombineAssertions(() =>
			{
				var transactionLine = TempStorageTestHelpers.GetRegLineTransaction(regLine, TransactionTypes.Codes.Transaction);
				AssertEquals("SRT_PackageQty", expectedPackageQty, transactionLine.SRT_PackageQty);
				AssertEquals("SRT_ReferenceType", "W-VV", transactionLine.SRT_ReferenceType);
				AssertEquals("SRT_InternalReferenceNumber", "CUSFIN58750000000381074050419102427", transactionLine.SRT_InternalReferenceNumber);
				AssertEquals("SRL_PackagesRemaining", expectedPackagesRemaining, regLine.SRL_PackagesRemaining);
				AssertEquals("SRL_CustomsStatus", expectedLineCustomsStatus, regLine.SRL_CustomsStatus);
			});
		}

		const string ReferencedRegistrationNumber = "ATB150000960420195875";
		const string AdditionalReferenceNumber = "ATO310000090420193481";
		const string AdditionalRegistrationNumber = "ATO310000090420195875";
		const string MRN = "24DE123050554788M5";
		const string ExpectedLogbookRegNumWithAllRegistrationNumbers = ReferencedRegistrationNumber + ", " + AdditionalReferenceNumber + ", " + AdditionalRegistrationNumber;
		const string EoriNumber = "8999783";
		const string EoriCode = "GR8999783";
		const string BranchNumber = "0001";
	}
}
