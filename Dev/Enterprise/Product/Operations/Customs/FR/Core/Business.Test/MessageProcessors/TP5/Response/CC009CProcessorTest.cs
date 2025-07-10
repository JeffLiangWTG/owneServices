using System.Linq;
using CargoWise.Customs.FR.MessageDefinitions.TP5.CC009C;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.EFTA.TemporaryStorageRegister.Business;
using Enterprise.Customs.EU.NCTS.Business.CodeDescriptionPairLists;
using Enterprise.Customs.EU.NCTS.Business.Testing;
using Enterprise.Customs.FR.Business.CusTempStorage;
using Enterprise.Customs.FR.Business.EdiMessages;
using Enterprise.Customs.FR.Business.NCTS;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using CusTempStorageRegHeader = Enterprise.Customs.FR.Business.CusTempStorage.CusTempStorageRegHeader;
using CusTempStorageRegLine = Enterprise.Customs.FR.Business.CusTempStorage.CusTempStorageRegLine;
using NctsMovementHeaderTransactionStatusList = Enterprise.Customs.EU.NCTS.Business.NctsMovementHeaderTransactionStatusList;

namespace Enterprise.Customs.FR.Business.MessageProcessors.Testing
{
	sealed class CC009CProcessorTest : TP5BaseProcessorTest<Cc009CType, CC009CProcessor>
	{
		protected override ZString GetMessageSubType() => TP5ResponseMessageSubTypeList.Codes.InvalidationDecision;

		protected override ZString GetMessageText() => resourceRetriever.Value.GetString("Enterprise.Customs.FR.Business.Testing.EDIMessage.TestFiles.NCTS_CC009CResponseMessage.xml");

		ZString GetMessageTextWithInvalidationDecision0() => resourceRetriever.Value.GetString("Enterprise.Customs.FR.Business.Testing.EDIMessage.TestFiles.NCTS_CC009CResponseMessageWithInvalidationDecision0.xml");

		protected override ZString ExpectedMRN => "MRN1";

		new public void TestDepartureCustomsStatus()
		{
			var header = GetNCTSHeader();
			var processor = GetNCTSBaseProcessor();

			var message = GetMessage(header);
			processor.ProcessMessage(message);
			ZString expectedCustomsStatus = NCTS5DepartureCustomsStatusList.Codes.Cancelled;
			AssertEquals("CustomsStatus should be set depending the response.", expectedCustomsStatus, header.MovementHeader.BM_CustomsStatus);

			header.MovementHeader.BM_CustomsStatus = "XYZ";
			var message1 = GetMessage(header, false);
			processor.ProcessMessage(message1);
			AssertEquals("No change in CustomsStatus when the decision tag is 0", "XYZ", header.MovementHeader.BM_CustomsStatus);
		}

		new public void TestMessageStatusIsUpdated()
		{
			var header = GetNCTSHeader();
			var processor = GetNCTSBaseProcessor();

			var message = GetMessage(header);
			processor.ProcessMessage(message);
			ZString expectedMessageStatus = LogicalStatusList.Codes.Accepted;
			AssertEquals("MessageStatus should be set depending the response.", expectedMessageStatus, header.MovementHeader.BM_MessageStatus);

			var message1 = GetMessage(header, false);
			processor.ProcessMessage(message1);
			expectedMessageStatus = LogicalStatusList.Codes.Invalid;
			AssertEquals("MessageStatus should be set depending the response.", expectedMessageStatus, header.MovementHeader.BM_MessageStatus);
		}

		new public void TestPhaseIdIsUpdated()
		{
			var header = GetNCTSHeader();

			var processor = GetNCTSBaseProcessor();

			var message = GetMessage(header);
			processor.ProcessMessage(message);
			ZString expectedPhaseId = NctsMovementHeaderTransactionStatusList.Codes.Cancellation;
			AssertEquals("PhaseId should be set depending the response.", expectedPhaseId, header.MovementHeader.BM_Phase);

			var message1 = GetMessage(header, false);
			processor.ProcessMessage(message1);
			expectedPhaseId = NctsMovementHeaderTransactionStatusList.Codes.Declaration;
			AssertEquals("PhaseId should be set depending the response.", expectedPhaseId, header.MovementHeader.BM_Phase);
		}

		public void TestTemporaryStorageRollBack_WhenInvalidationFlagIsEnabled()
		{
			var nctsHeader = SetupStorage();
			CusTempStorageRegHeader reg1Reloaded, reg2Reloaded;
			EU.TemporaryStorage.Business.CusTempStorageRegLine regLine1Reloaded, regLine2Reloaded;

			var message = GetMessage(nctsHeader);
			var processor = GetNCTSBaseProcessor();
			processor.ProcessMessage(message);
			Factory.Save();

			reg1Reloaded = new BusinessObjectFactory().LoadTop1<CusTempStorageRegHeader>(new ZQuery(CusTempStorageRegHeaderSchema.SRH_InternalReference, "FRJ_IST1"));
			regLine1Reloaded = reg1Reloaded.CusTempStorageRegLines.Cast<CusTempStorageRegLine>().Single();
			AssertEquals("The register should be refilled with the departure declaration packages when the declaration is cancelled", 100, regLine1Reloaded.SRL_PackagesRemaining);

			reg2Reloaded = new BusinessObjectFactory().LoadTop1<CusTempStorageRegHeader>(new ZQuery(CusTempStorageRegHeaderSchema.SRH_InternalReference, "FRJ_IST2"));
			regLine2Reloaded = reg2Reloaded.CusTempStorageRegLines.Cast<CusTempStorageRegLine>().Single();
			AssertEquals("The register should be refilled with the departure declaration packages when the declaration is cancelled", 200, regLine2Reloaded.SRL_PackagesRemaining);

			var mails = Env.OutgoingCustomsMailManager.EmailsCreated;
			AssertEquals("No error emails should be sent", 0, mails.Count);
		}

		public void TestTemporaryStorageRollBack_WhenInvalidationFlagIsDisabled()
		{
			var nctsHeader = SetupStorage();
			CusTempStorageRegHeader reg1Reloaded, reg2Reloaded;
			EU.TemporaryStorage.Business.CusTempStorageRegLine regLine1Reloaded, regLine2Reloaded;

			var message = GetMessage(nctsHeader, false);
			var processor = GetNCTSBaseProcessor();
			processor.ProcessMessage(message);
			Factory.Save();

			reg1Reloaded = new BusinessObjectFactory().LoadTop1<CusTempStorageRegHeader>(new ZQuery(CusTempStorageRegHeaderSchema.SRH_InternalReference, "FRJ_IST1"));
			regLine1Reloaded = reg1Reloaded.CusTempStorageRegLines.Cast<CusTempStorageRegLine>().Single();
			AssertEquals("The number of remaining packages should remain as it was since no cancellation occurs", 20, regLine1Reloaded.SRL_PackagesRemaining);

			reg2Reloaded = new BusinessObjectFactory().LoadTop1<CusTempStorageRegHeader>(new ZQuery(CusTempStorageRegHeaderSchema.SRH_InternalReference, "FRJ_IST2"));
			regLine2Reloaded = reg2Reloaded.CusTempStorageRegLines.Cast<CusTempStorageRegLine>().Single();
			AssertEquals("The number of remaining packages should remain as it was since no cancellation occurs", 130, regLine2Reloaded.SRL_PackagesRemaining);

			var mails = Env.OutgoingCustomsMailManager.EmailsCreated;
			AssertEquals("No error emails should be sent", 0, mails.Count);
		}

		public void TestUpdateGuaranteeTransactions_WhenInvalidationFlagIsEnabled()
		{
			NCTSTestHelper.SetupC0009ForCountries(Factory, Core.Constants.CountryCodes.France);

			var eoriCode = "123456789000";
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			orgHeader.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, eoriCode, GlbCompany.CurrentCompany.GC_RN_NKCountryCode);

			var guaranteeHeader1 = GetGuaranteeHeader(orgHeader, "19860101");
			guaranteeHeader1.AddTransaction("Opening Bal", "Opening Bal", "111", ZString.Empty, 200m, 0, PermitTransactionStatusList.Codes.Confirmed, transactionType: PermitTransactionTypeList.Codes.OBL, isAggregated: true);
			guaranteeHeader1.AddTransaction("2595950308700261001902", "CMT-CON", "111", "", -50m, 0, PermitTransactionStatusList.Codes.Confirmed, isAggregated: true);
			guaranteeHeader1.AddTransaction("2595950308700261001902", "CMT-DEL-1", "111", ZString.Empty, -10, 0, PermitTransactionStatusList.Codes.Pending, isAggregated: true);
			guaranteeHeader1.AddTransaction("2595950308700261001902", "CMT-DEL-2", "111", ZString.Empty, -20, 0, PermitTransactionStatusList.Codes.Pending, isAggregated: true);

			var guaranteeHeader2 = GetGuaranteeHeader(Factory.NewWithValidTestData<OrgHeader>(), "GRN_NotPresentInDeclaration");
			guaranteeHeader2.AddTransaction("Opening Bal", "Opening Bal for GRN_NotPresentInDeclaration", ZString.Empty, ZString.Empty, 1000m, 0, PermitTransactionStatusList.Codes.Confirmed, transactionType: PermitTransactionTypeList.Codes.OBL, isAggregated: true);
			guaranteeHeader2.AddTransaction("2595950308700261001902", "CMT-CON", "111", "", -50m, 0, PermitTransactionStatusList.Codes.Confirmed, isAggregated: true);
			guaranteeHeader2.AddTransaction("2595950308700261001902", "NCTS departure for GRN_NotPresentInDeclaration", ZString.Empty, ZString.Empty, -500m, 0, PermitTransactionStatusList.Codes.Pending, isAggregated: true);
			Factory.Save();

			var nctsHeader = GetNCTSHeader();
			nctsHeader.BH_HeaderType = EU.NCTS.Business.NctsMovementType.Codes.Departure;
			nctsHeader.MovementHeader.BM_PaperlessInbondNum = "2595950308700261001902";
			nctsHeader.Principal.E2_OA_Address = guaranteeHeader1.PermitHolder.MainAddress.PK;

			nctsHeader.GetEffectiveGuarantees().DeleteAll();
			var guarantee = nctsHeader.GetEffectiveGuarantees().AddNew();
			guarantee.PW_BondAmount = 150m;
			guarantee.PW_BondNumber = guaranteeHeader1.CPH_Number;
			nctsHeader.ApportionedAmountToGuaranteesLiabilityAmount();

			AssertEquals("Prerequisite: Guarantee should be linked to the nctsHeader", nctsHeader.GetEffectiveGuarantees().First().CusGuarantee.PK, guarantee.CusGuarantee.PK);

			var outgoingMessage = GetNCTSFREDIMessage(nctsHeader);
			outgoingMessage.EM_Status = EDIMessage.Status.Received;
			outgoingMessage.EM_ReceiveTransmit = ReceiveTransmitList.Codes.Transmit;
			outgoingMessage.EM_MessageSubType = TP5MessageTypeList.Codes.CC015C;
			nctsHeader.MovementHeader.Messages.Add(outgoingMessage);

			var incomingMessage = GetNCTSFREDIMessage(nctsHeader);
			nctsHeader.Messages.Add(incomingMessage);

			var processor = GetNCTSBaseProcessor();
			processor.ProcessMessage(incomingMessage);
			Factory.Save();

			AssertEquals("PermitAppId of LastOutgoingMessage should be equal to MessageReferenceNumber", "111", FRPermitHelper.GetPermitAppIdForMessage(nctsHeader.GetOutgoingMessage(incomingMessage)));
			AssertEquals("MessageSubType of incoming message should be 009", TP5ResponseMessageSubTypeList.Codes.InvalidationDecision, nctsHeader.Messages.LastIncomingMessage.EM_MessageSubType);

			var permitHelper = new PermitTestDataHelper(Factory);
			var query = permitHelper.GetPermitLineTransactionQuery("2595950308700261001902", $"NCTS write-off {FRPermitHelper.GetPermitAppIdForMessage(incomingMessage)} [{ExpectedMRN}]", nctsHeader.Messages.LastIncomingMessage.EM_MessageNum, PermitTransactionCategoryList.Codes.CUM, PermitTransactionTypeList.Codes.TRA);
			var transactionRequested = new BusinessObjectFactory().Load<BaseCusPermitLineTransaction>(query);
			AssertEquals("There should be two write-off transactions created, one from each guaranteee header", 2, transactionRequested.Length);

			query = permitHelper.GetPermitLineTransactionQuery("2595950308700261001902", "CMT-DEL-1", "111", PermitTransactionCategoryList.Codes.CUM, PermitTransactionTypeList.Codes.TRA);
			transactionRequested = new BusinessObjectFactory().Load<BaseCusPermitLineTransaction>(query);
			AssertEquals("There should be one transaction present for the CMT-DEL-1 query", 1, transactionRequested.Length);
			AssertEquals("Transaction status should be deleted", PermitTransactionStatusList.Codes.Deleted, transactionRequested[0].CPL_TransactionStatus);

			query = permitHelper.GetPermitLineTransactionQuery("2595950308700261001902", "CMT-DEL-2", "111", PermitTransactionCategoryList.Codes.CUM, PermitTransactionTypeList.Codes.TRA);
			transactionRequested = new BusinessObjectFactory().Load<BaseCusPermitLineTransaction>(query);
			AssertEquals("There should be one transaction present for the CMT-DEL-2 query", 1, transactionRequested.Length);
			AssertEquals("Transaction status should be deleted", PermitTransactionStatusList.Codes.Deleted, transactionRequested[0].CPL_TransactionStatus);

			query = permitHelper.GetPermitLineTransactionQuery("2595950308700261001902", "NCTS departure for GRN_NotPresentInDeclaration", ZString.Empty, PermitTransactionCategoryList.Codes.CUM, PermitTransactionTypeList.Codes.TRA);
			transactionRequested = new BusinessObjectFactory().Load<BaseCusPermitLineTransaction>(query);
			AssertEquals("There should be one transaction present for this query", 1, transactionRequested.Length);
			AssertEquals("Transaction status should be deleted", PermitTransactionStatusList.Codes.Deleted, transactionRequested[0].CPL_TransactionStatus);
		}

		public void TestUpdateGuaranteeTransactions_WhenInvalidationFlagIsDisabled()
		{
			NCTSTestHelper.SetupC0009ForCountries(Factory, Core.Constants.CountryCodes.France);

			var eoriCode = "123456789000";
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			orgHeader.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, eoriCode, GlbCompany.CurrentCompany.GC_RN_NKCountryCode);

			var guaranteeHeader1 = GetGuaranteeHeader(orgHeader, "19860101");
			guaranteeHeader1.AddTransaction("2595950308700261001902", "Opening Bal", "111", ZString.Empty, 200m, 0, PermitTransactionStatusList.Codes.Confirmed, transactionType: PermitTransactionTypeList.Codes.OBL, isAggregated: true);
			guaranteeHeader1.AddTransaction("2595950308700261001902", "CMT-DEL-1", "111", ZString.Empty, -10, 0, PermitTransactionStatusList.Codes.Pending, isAggregated: true);
			guaranteeHeader1.AddTransaction("2595950308700261001902", "CMT-DEL-2", "111", ZString.Empty, -20, 0, PermitTransactionStatusList.Codes.Pending, isAggregated: true);

			var guaranteeHeader2 = GetGuaranteeHeader(Factory.NewWithValidTestData<OrgHeader>(), "GRN_NotPresentInDeclaration");
			guaranteeHeader2.AddTransaction("2595950308700261001902", "Opening Bal for GRN_NotPresentInDeclaration", ZString.Empty, ZString.Empty, 1000m, 0, PermitTransactionStatusList.Codes.Confirmed, transactionType: PermitTransactionTypeList.Codes.OBL, isAggregated: true);
			guaranteeHeader2.AddTransaction("2595950308700261001902", "NCTS departure for GRN_NotPresentInDeclaration", ZString.Empty, ZString.Empty, -500m, 0, PermitTransactionStatusList.Codes.Pending, isAggregated: true);
			Factory.Save();

			var nctsHeader = GetNCTSHeader();
			nctsHeader.BH_HeaderType = EU.NCTS.Business.NctsMovementType.Codes.Departure;
			nctsHeader.MovementHeader.BM_PaperlessInbondNum = "NCT00012345";
			nctsHeader.Principal.E2_OA_Address = guaranteeHeader1.PermitHolder.MainAddress.PK;

			var guarantee = nctsHeader.GetEffectiveGuarantees().AddNew();
			guarantee.PW_BondAmount = 150m;
			guarantee.PW_BondNumber = guaranteeHeader1.CPH_Number;
			nctsHeader.ApportionedAmountToGuaranteesLiabilityAmount();

			AssertEquals("Prerequisite: Guarantee should be linked to the nctsHeader", nctsHeader.GetEffectiveGuarantees().First().CusGuarantee.PK, guarantee.CusGuarantee.PK);

			var outgoingMessage = GetNCTSFREDIMessage(nctsHeader);
			outgoingMessage.EM_Status = EDIMessage.Status.Received;
			outgoingMessage.EM_ReceiveTransmit = ReceiveTransmitList.Codes.Transmit;
			outgoingMessage.EM_MessageSubType = TP5MessageTypeList.Codes.CC015C;
			nctsHeader.MovementHeader.Messages.Add(outgoingMessage);

			var incomingMessage = GetNCTSFREDIMessage(nctsHeader);
			incomingMessage.EM_MessageText = GetMessageTextWithInvalidationDecision0();
			nctsHeader.Messages.Add(incomingMessage);

			var processor = GetNCTSBaseProcessor();
			processor.ProcessMessage(incomingMessage);
			Factory.Save();

			AssertEquals("PermitAppId of LastOutgoingMessage should be equal to MessageReferenceNumber", "111", FRPermitHelper.GetPermitAppIdForMessage(nctsHeader.GetOutgoingMessage(incomingMessage)));
			AssertEquals("MessageSubType of incoming message should be 009", TP5ResponseMessageSubTypeList.Codes.InvalidationDecision, nctsHeader.Messages.LastIncomingMessage.EM_MessageSubType);

			var permitHelper = new PermitTestDataHelper(Factory);
			var query = permitHelper.GetPermitLineTransactionQuery("2595950308700261001902", "CMT-DEL-1", "111", PermitTransactionCategoryList.Codes.CUM, PermitTransactionTypeList.Codes.TRA);
			var transactionRequested = new BusinessObjectFactory().Load<BaseCusPermitLineTransaction>(query);
			AssertEquals("There should be one transaction present for the CMT-DEL-1 query", 1, transactionRequested.Length);
			AssertEquals("Transaction status should be pending", PermitTransactionStatusList.Codes.Pending, transactionRequested[0].CPL_TransactionStatus);

			query = permitHelper.GetPermitLineTransactionQuery("2595950308700261001902", "CMT-DEL-2", "111", PermitTransactionCategoryList.Codes.CUM, PermitTransactionTypeList.Codes.TRA);
			transactionRequested = new BusinessObjectFactory().Load<BaseCusPermitLineTransaction>(query);
			AssertEquals("There should be one transaction present for the CMT-DEL-2 query", 1, transactionRequested.Length);
			AssertEquals("Transaction status should be pending", PermitTransactionStatusList.Codes.Pending, transactionRequested[0].CPL_TransactionStatus);

			query = permitHelper.GetPermitLineTransactionQuery("2595950308700261001902", "NCTS departure for GRN_NotPresentInDeclaration", ZString.Empty, PermitTransactionCategoryList.Codes.CUM, PermitTransactionTypeList.Codes.TRA);
			transactionRequested = new BusinessObjectFactory().Load<BaseCusPermitLineTransaction>(query);
			AssertEquals("There should be one transaction present for this query", 1, transactionRequested.Length);
			AssertEquals("Transaction status should be pending", PermitTransactionStatusList.Codes.Pending, transactionRequested[0].CPL_TransactionStatus);
		}

		FREDIMessage GetMessage(NctsHeader nctsHeader, bool isDecisionTagZero = true)
		{
			var message = Factory.New<NCTSFREDIMessage>();
			message.EM_MessageSubType = GetMessageSubType();
			message.EM_ApplicationCode = EDIMessage.ApplicationCodes.FRCustomsMessage;
			message.EM_Status = EDIMessage.Status.Queued;
			message.EM_ReceiveTransmit = "RCV";
			message.EM_MessageText = isDecisionTagZero ? GetMessageText() : GetMessageTextWithInvalidationDecision0();
			message.EM_LinkedObject = nctsHeader;
			Factory.Save();
			return message;
		}

		NctsHeader SetupStorage()
		{
			var ist1 = CusTempStorageJobHeader.New(Factory, FRConstants.TemporaryStorage.AppCodeIST);
			ist1.SJH_JobReference = "FRJ_IST1";
			ist1.DDTNumber = "DDT1";
			ist1.SJH_OH_Customer = Factory.NewWithValidTestData<OrgHeader>().PK;

			var register1 = Factory.New<CusTempStorageRegHeader>();
			register1.SRH_Reference = "DDT1";
			register1.SRH_InternalReference = "FRJ_IST1";
			var regLine1 = register1.CusTempStorageRegLines.AddNew();
			regLine1.SRL_LineNumber = 1;

			var transaction1A = regLine1.CusTempStorageRegLineTransactions.AddNew();
			transaction1A.SRT_TransactionType = CusTempStorageRegLineTransactionTypeList.Codes.OpeningBalance;
			transaction1A.SRT_PackageQty = 100;

			var ist2 = CusTempStorageJobHeader.New(Factory, FRConstants.TemporaryStorage.AppCodeIST);
			ist2.DDTNumber = "DDT2";
			ist2.SJH_JobReference = "FRJ_IST2";
			ist2.SJH_OH_Customer = Factory.NewWithValidTestData<OrgHeader>().PK;

			var register2 = Factory.New<CusTempStorageRegHeader>();
			register2.SRH_Reference = "DDT2";
			register2.SRH_InternalReference = "FRJ_IST2";
			var regLine2 = register2.CusTempStorageRegLines.AddNew();
			regLine2.SRL_LineNumber = 1;

			var transaction2A = regLine2.CusTempStorageRegLineTransactions.AddNew();
			transaction2A.SRT_TransactionType = CusTempStorageRegLineTransactionTypeList.Codes.OpeningBalance;
			transaction2A.SRT_PackageQty = 200;

			var nctsHeader = GetNCTSHeader();
			nctsHeader.BH_HeaderType = EU.NCTS.Business.NctsMovementType.Codes.Departure;
			nctsHeader.MovementHeader.BM_PaperlessInbondNum = "2595950308700261001902";
			nctsHeader.MovementReferenceEntryNumber.CE_EntryNum = ExpectedMRN;

			var bill1 = nctsHeader.Bills.AddNew();
			var goodsItem1 = bill1.GoodsItems.AddNew();
			var pd1 = goodsItem1.PreviousDocuments.AddNew();
			pd1.CSI_Quantity = 20m;
			pd1.CSI_Quantity2 = 50m;
			pd1.CSI_Code = FRConstants.PreviousDocuments.N337;
			pd1.CSI_ReferenceNumber = "FRJ_IST1";
			pd1.CSI_ItemNumber = 1;

			var pd2 = goodsItem1.PreviousDocuments.AddNew();
			pd2.CSI_Quantity = 40m;
			pd2.CSI_Quantity2 = 30m;
			pd2.CSI_Code = FRConstants.PreviousDocuments.N337;
			pd2.CSI_ReferenceNumber = "FRJ_IST1";
			pd2.CSI_ItemNumber = 1;

			var pd3 = goodsItem1.PreviousDocuments.AddNew();
			pd3.CSI_Quantity = 10m;
			pd3.CSI_Quantity2 = 30m;
			pd3.CSI_Code = FRConstants.PreviousDocuments.N337;
			pd3.CSI_ReferenceNumber = "FRJ_IST1";
			pd3.CSI_ItemNumber = 2; // invalid item number

			var goodsItem2 = nctsHeader.Bills.AddNew().GoodsItems.AddNew();
			var pd4 = goodsItem2.PreviousDocuments.AddNew();
			pd4.CSI_Quantity = 40m;
			pd4.CSI_Quantity2 = 70m;
			pd4.CSI_Code = FRConstants.PreviousDocuments.N337;
			pd4.CSI_ReferenceNumber = "FRJ_IST2";
			pd4.CSI_ItemNumber = 1;

			var logger = new LoggingInformation();

			foreach (var transactionData in goodsItem1.TemporaryStorageRegisterTransactionDataProvider.GetTemporaryStorageRegisterTransactionData())
			{
				register1.AddNewRegisterTransaction(logger, transactionData.RegisterLineNo, transactionData.CustomsReferenceNumber, transactionData.ReferenceType, transactionData.InternalReferenceNumber, transactionData.InternalReferenceType, transactionData.GrossMass, 0 - transactionData.PackageQuantity, transactionData.Comments);
			}

			foreach (var transactionData in goodsItem2.TemporaryStorageRegisterTransactionDataProvider.GetTemporaryStorageRegisterTransactionData())
			{
				register2.AddNewRegisterTransaction(logger, transactionData.RegisterLineNo, transactionData.CustomsReferenceNumber, transactionData.ReferenceType, transactionData.InternalReferenceNumber, transactionData.InternalReferenceType, transactionData.GrossMass, 0 - transactionData.PackageQuantity, transactionData.Comments);
			}

			Factory.Save();

			var reg1Reloaded = new BusinessObjectFactory().LoadTop1<CusTempStorageRegHeader>(new ZQuery(CusTempStorageRegHeaderSchema.SRH_InternalReference, "FRJ_IST1"));
			var regLine1Reloaded = reg1Reloaded.CusTempStorageRegLines.Single();
			AssertEquals("The number of remaining packages should be according to the deducted ones", 20, regLine1Reloaded.SRL_PackagesRemaining);

			var reg2Reloaded = new BusinessObjectFactory().LoadTop1<CusTempStorageRegHeader>(new ZQuery(CusTempStorageRegHeaderSchema.SRH_InternalReference, "FRJ_IST2"));
			var regLine2Reloaded = reg2Reloaded.CusTempStorageRegLines.Single();
			AssertEquals("The number of remaining packages should be according to the deducted ones", 130, regLine2Reloaded.SRL_PackagesRemaining);

			return nctsHeader;
		}
	}
}
