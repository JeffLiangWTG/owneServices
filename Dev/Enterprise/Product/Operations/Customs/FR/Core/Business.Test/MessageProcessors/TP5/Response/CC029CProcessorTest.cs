using System;
using System.Linq;
using CargoWise.Customs.FR.MessageDefinitions.TP5.CC029C;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.EFTA.TemporaryStorageRegister.Business;
using Enterprise.Customs.EU.NCTS.Business.CodeDescriptionPairLists;
using Enterprise.Customs.EU.NCTS.Business.Testing;
using Enterprise.Customs.FR.Business.CusTempStorage;
using Enterprise.Customs.FR.Business.EdiMessages;
using Enterprise.Customs.FR.Registry;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using CusTempStorageRegHeader = Enterprise.Customs.FR.Business.CusTempStorage.CusTempStorageRegHeader;
using CusTempStorageRegLineTransaction = Enterprise.Customs.FR.Business.CusTempStorage.CusTempStorageRegLineTransaction;

namespace Enterprise.Customs.FR.Business.MessageProcessors.Testing
{
	sealed class CC029CProcessorTest : TP5BaseProcessorTest<Cc029CType, CC029CProcessor>
	{
		protected override ZString GetMessageSubType() => TP5ResponseMessageSubTypeList.Codes.ReleasedForTransit;

		protected override ZString GetMessageText() => resourceRetriever.Value.GetString("Enterprise.Customs.FR.Business.Testing.EDIMessage.TestFiles.NCTS_CC029CResponseMessage.xml");

		protected override ZString ExpectedMRN => "MRN1";

		protected override ZDateTime ExpectedReleaseDate => new ZDateTime(2021, 05, 01);

		protected override ZString ExpectedMessageStatus => Common.EU.LogicalStatusList.Codes.Accepted;

		protected override ZString ExpectedPhaseId => "015";

		protected override ZString ExpectedDepartureCustomsStatus => NCTS5DepartureCustomsStatusList.Codes.ReleasedForTransit;

		[TestDate(2024, 12, 18, 0, 0, 0)]
		public void TestUpdateTemporaryStorageIfApplicable()
		{
			var ist1 = CusTempStorageJobHeader.New(Factory, FRConstants.TemporaryStorage.AppCodeIST);
			ist1.SJH_JobReference = "FRJ_IST1";
			ist1.SJH_CustomsOffice = "FR001";
			ist1.SJH_OH_Customer = Factory.NewWithValidTestData<OrgHeader>().PK;
			ist1.SJH_ArrivalDate = new ZDate(2021, 08, 01);
			ist1.SJH_PresentationDate = new ZDate(2021, 08, 02);
			ist1.SJH_TempStorageEndDateUtc = new ZDate(2021, 08, 03);
			ist1.SJH_PreviousReferenceType = CusTempStorageRegLineTransactionReferenceTypeList.Codes.IST;
			ist1.SJH_PreviousReferenceNumber = "FRJ_IST1";

			var line1 = ist1.CusTempStorageDec.CusTempStorageLines.AddNew();
			line1.TSL_OwnerReferenceType = "AWB";
			line1.TSL_OwnerReferenceNumber = "OWNREF001";
			line1.TSL_UnionStatus = "T1";
			line1.TSL_LocationOfGoods = "FR002300";
			line1.TSL_PackageQty = 100;
			line1.TSL_PackageType = "1A";
			line1.TSL_GrossWeight = 1000;
			line1.TSL_GrossWeightUQ = Core.Constants.Weight.Kilograms;

			var group = SetUpStaffAndGroup();

			Factory.Save();
			FRCustomsDataRegistry.Instance.DeltaTResponseNotificationGroup.SetValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, new Enterprise.Registry.Business.Customs.ManifestGroupNotification("ESG", group.PK, false));
			var logger = new LoggingInformation();

			var regheader = Factory.New<CusTempStorageRegHeader>();
			regheader.SRH_Reference = "DDTNUMBER";
			regheader.SRH_InternalReference = ist1.SJH_JobReference;
			var regline1 = regheader.CusTempStorageRegLines.AddNew();
			regline1.FillWithValidTestData();
			regline1.SRL_LineNumber = 1;
			regline1.SRL_PackagesRemaining = 100;

			var transaction1 = regline1.CusTempStorageRegLineTransactions.AddNew();
			transaction1.SRT_TransactionType = CusTempStorageRegLineTransactionTypeList.Codes.OpeningBalance;
			transaction1.SRT_GrossWeight = 1m;
			transaction1.SRT_PackageQty = 100;
			transaction1.SRT_ReferenceType = CusTempStorageRegLineTransactionReferenceTypeList.Codes.IST;
			transaction1.SRT_Reference = "FRJ_IST1";

			TestDateAttribute.AddMinutes(1);

			var nctsHeader = GetNCTSHeader();
			nctsHeader.BH_HeaderType = EU.NCTS.Business.NctsMovementType.Codes.Departure;
			nctsHeader.MovementHeader.BM_PaperlessInbondNum = "2595950308700261001902";

			var goodsItem = nctsHeader.Bills.AddNew().GoodsItems.AddNew();
			var previousDoc = goodsItem.PreviousDocuments.AddNew();
			previousDoc.CSI_Quantity = 20m;
			previousDoc.CSI_Quantity2 = 40m;
			previousDoc.CSI_Code = FRConstants.PreviousDocuments.N337;
			previousDoc.CSI_ReferenceNumber = "FRJ_IST1";
			previousDoc.CSI_ItemNumber = 1;

			var message = GetNCTSFREDIMessage(nctsHeader);
			var processor = GetNCTSBaseProcessor();
			processor.ProcessMessage(message);
			Factory.Save();

			var regHeader = new BusinessObjectFactory().LoadTop1<CusTempStorageRegHeader>(new ZQuery(CusTempStorageRegHeaderSchema.SRH_InternalReference, "FRJ_IST1"));
			var regLines = regHeader.CusTempStorageRegLines;
			AssertEquals(1, regLines.Count);

			var regLineFinal = regHeader.CusTempStorageRegLines.First(x => x.SRL_LineNumber == 1);
			var transaction = (CusTempStorageRegLineTransaction)regLineFinal.CusTempStorageRegLineTransactions.FirstOrDefault(x => x.SRT_TransactionType == CusTempStorageRegLineTransactionTypeList.Codes.Transaction);

			CombineAssertions("Transaction values", () =>
			{
				AssertEquals(CusTempStorageRegLineTransactionTypeList.Codes.Transaction, transaction.SRT_TransactionType);
				AssertEquals("Transaction", transaction.TransactionTypeDescription);
				AssertEquals("2595950308700261001902", transaction.SRT_InternalReferenceNumber);
				AssertEquals("MRN1", transaction.SRT_Reference);
				AssertEquals(-40, transaction.SRT_PackageQty);
				AssertEquals(TempStorageTransactionRefTypeList.Codes.NctsHeader, transaction.SRT_ReferenceType);
				AssertEquals(20m, transaction.SRT_GrossWeight);
				AssertEquals("", transaction.SRT_Comments);
			});
		}

		public void TestUpdateTemporaryStorageIfApplicable_MultiplePreviousDocument()
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
			transaction2A.SRT_PackageQty = 100;

			var nctsHeader = GetNCTSHeader();
			nctsHeader.BH_HeaderType = EU.NCTS.Business.NctsMovementType.Codes.Departure;

			var bill1 = nctsHeader.Bills.AddNew();
			var goodsItem1 = bill1.GoodsItems.AddNew();
			var pd1 = goodsItem1.PreviousDocuments.AddNew();
			pd1.CSI_Quantity = 10m;
			pd1.CSI_Quantity2 = 60m;
			pd1.CSI_Code = FRConstants.PreviousDocuments.N337;
			pd1.CSI_ReferenceNumber = "FRJ_IST1";
			pd1.CSI_ItemNumber = 1;

			var goodsItem2 = bill1.GoodsItems.AddNew();
			var pd2 = goodsItem2.PreviousDocuments.AddNew();
			pd2.CSI_Quantity = 10m;
			pd2.CSI_Quantity2 = 20m;
			pd2.CSI_Code = FRConstants.PreviousDocuments.N337;
			pd2.CSI_ReferenceNumber = "FRJ_IST1";
			pd2.CSI_ItemNumber = 1;

			var goods2 = nctsHeader.Bills.AddNew().GoodsItems.AddNew();
			var pd3 = goods2.PreviousDocuments.AddNew();
			pd3.CSI_Quantity = 40m;
			pd3.CSI_Quantity2 = 70m;
			pd3.CSI_Code = FRConstants.PreviousDocuments.N337;
			pd3.CSI_ReferenceNumber = "FRJ_IST2";
			pd3.CSI_ItemNumber = 1;

			var goods3 = nctsHeader.Bills.AddNew().GoodsItems.AddNew();
			var pd4 = goods3.PreviousDocuments.AddNew();
			pd4.CSI_Quantity = 60m;
			pd4.CSI_Quantity2 = 60m;
			pd4.CSI_Code = FRConstants.PreviousDocuments.N337;
			pd4.CSI_ReferenceNumber = "FRJ_IST3"; // expect no transaction line as FRJ_IST3 is not a valid reference
			pd4.CSI_ItemNumber = 1;

			Factory.Save();

			var message = GetNCTSFREDIMessage(nctsHeader);
			var processor = GetNCTSBaseProcessor();
			processor.ProcessMessage(message);
			Factory.Save();

			var reg1Reloaded = new BusinessObjectFactory().LoadTop1<CusTempStorageRegHeader>(new ZQuery(CusTempStorageRegHeaderSchema.SRH_InternalReference, "FRJ_IST1"));
			var regLine1Reloaded = reg1Reloaded.CusTempStorageRegLines.Single();
			AssertEquals(20, regLine1Reloaded.SRL_PackagesRemaining);
			var transaction1BReloaded = regLine1Reloaded.CusTempStorageRegLineTransactions.Where(x => x.SRT_TransactionType == CusTempStorageRegLineTransactionTypeList.Codes.Transaction);
			AssertEquals(-80, transaction1BReloaded.Sum(x => x.SRT_PackageQty));

			var reg2Reloaded = new BusinessObjectFactory().LoadTop1<CusTempStorageRegHeader>(new ZQuery(CusTempStorageRegHeaderSchema.SRH_InternalReference, "FRJ_IST2"));
			var regLine2Reloaded = reg2Reloaded.CusTempStorageRegLines.Single();
			AssertEquals(30, regLine2Reloaded.SRL_PackagesRemaining);
			var transaction2BReloaded = regLine2Reloaded.CusTempStorageRegLineTransactions.Single(x => x.SRT_TransactionType == CusTempStorageRegLineTransactionTypeList.Codes.Transaction);
			AssertEquals(-70, transaction2BReloaded.SRT_PackageQty);

			var mails = Env.OutgoingCustomsMailManager.EmailsCreated;
			AssertEquals(0, mails.Count);
		}

		public void TestUpdateTemporaryStorageIfApplicablePackageRemainingShouldBeUnder0()
		{
			var ist1 = CusTempStorageJobHeader.New(Factory, FRConstants.TemporaryStorage.AppCodeIST);
			ist1.SJH_JobReference = "FRJ_IST1";
			ist1.SJH_CustomsOffice = "FR001";
			ist1.SJH_OH_Customer = Factory.NewWithValidTestData<OrgHeader>().PK;
			ist1.SJH_ArrivalDate = new ZDate(2021, 08, 01);
			ist1.SJH_PresentationDate = new ZDate(2021, 08, 02);
			ist1.SJH_TempStorageEndDateUtc = new ZDate(2021, 08, 03);
			ist1.SJH_PreviousReferenceType = CusTempStorageRegLineTransactionReferenceTypeList.Codes.IST;
			ist1.SJH_PreviousReferenceNumber = "FRJ_IST1";

			var line1 = ist1.CusTempStorageDec.CusTempStorageLines.AddNew();
			line1.TSL_OwnerReferenceType = "AWB";
			line1.TSL_OwnerReferenceNumber = "OWNREF001";
			line1.TSL_UnionStatus = "T1";
			line1.TSL_LocationOfGoods = "FR002300";
			line1.TSL_PackageQty = 20;
			line1.TSL_PackageType = "1A";
			line1.TSL_GrossWeight = 1000;
			line1.TSL_GrossWeightUQ = Core.Constants.Weight.Kilograms;
			var group = SetUpStaffAndGroup();

			Factory.Save();
			FRCustomsDataRegistry.Instance.DeltaTResponseNotificationGroup.SetValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, new Enterprise.Registry.Business.Customs.ManifestGroupNotification("ESG", group.PK, false));
			var logger = new LoggingInformation();
			Factory.Save();

			var regheader = Factory.New<CusTempStorageRegHeader>();
			regheader.SRH_Reference = "DDTNUMBER";
			regheader.SRH_InternalReference = ist1.SJH_JobReference;
			var regline1 = regheader.CusTempStorageRegLines.AddNew();
			regline1.FillWithValidTestData();
			regline1.SRL_LineNumber = 1;
			regline1.SRL_PackagesRemaining = 20;

			var transaction1 = regline1.CusTempStorageRegLineTransactions.AddNew();
			transaction1.SRT_TransactionType = CusTempStorageRegLineTransactionTypeList.Codes.OpeningBalance;
			transaction1.SRT_GrossWeight = 1m;
			transaction1.SRT_PackageQty = 30;
			transaction1.SRT_ReferenceType = CusTempStorageRegLineTransactionReferenceTypeList.Codes.IST;
			transaction1.SRT_Reference = "FRJ_IST1";

			var nctsHeader = GetNCTSHeader();
			nctsHeader.BH_HeaderType = EU.NCTS.Business.NctsMovementType.Codes.Departure;
			nctsHeader.MovementHeader.BM_PaperlessInbondNum = "2595950308700261001902";
			nctsHeader.BH_SystemCreateUser = "~BB";

			var goodsItem = nctsHeader.Bills.AddNew().GoodsItems.AddNew();
			var previousDoc = goodsItem.PreviousDocuments.AddNew();
			previousDoc.CSI_Quantity = 20m;
			previousDoc.CSI_Quantity2 = 70m;
			previousDoc.CSI_Code = FRConstants.PreviousDocuments.N337;
			previousDoc.CSI_ReferenceNumber = "FRJ_IST1";
			previousDoc.CSI_ItemNumber = 1;

			var ist2 = CusTempStorageJobHeader.New(Factory, FRConstants.TemporaryStorage.AppCodeIST);
			ist2.SJH_JobReference = "FRJ_IST2";
			ist2.SJH_OH_Customer = Factory.NewWithValidTestData<OrgHeader>().PK;

			var message = GetNCTSFREDIMessage(nctsHeader);
			var processor = GetNCTSBaseProcessor();
			processor.ProcessMessage(message);
			Factory.Save();

			var regHeader = Factory.LoadTop1<CusTempStorageRegHeader>(new ZQuery(CusTempStorageRegHeaderSchema.SRH_InternalReference, "FRJ_IST1"));
			var regLines = regHeader.CusTempStorageRegLines;
			AssertEquals(1, regLines.Count);

			var regLineFinal = regHeader.CusTempStorageRegLines.First(x => x.SRL_LineNumber == 1);

			AssertEquals(1, regLineFinal.CusTempStorageRegLineTransactions.Count);

			Assert(nctsHeader.Logs.GetAllLogs().Cast<StmALog>().Any(x => x.SL_Reference.Contains("Temporary Storage " + regHeader.SRH_Reference + " has not been updated")));

			var mails = Env.OutgoingCustomsMailManager.EmailsCreated;
			CombineAssertions(() =>
			{
				AssertEquals(1, mails.Count);
				AssertEquals("New Transit response received. Reference: 2595950308700261001902", mails[0].Subject);
				AssertEquals(@"An NCTS P5 response for 2595950308700261001902. Temporary Storage DDTNUMBER has not been updated by House Consignment #1. There are not enough packages remaining.", mails[0].Body);
			});
		}

		public void TestUpdateGuaranteeTransactions()
		{
			NCTSTestHelper.SetupC0009ForCountries(Factory, Core.Constants.CountryCodes.France);

			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			orgHeader.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "123456789000", GlbCompany.CurrentCompany.GC_RN_NKCountryCode);

			var guaranteeHeader1 = GetGuaranteeHeader(orgHeader, "GUA1");
			var additionalReference1 = guaranteeHeader1.AdditionalGuaranteeReferences.AddNew();
			additionalReference1.CY_Code = OrgCusAccountDeltaTTypeList.Codes.TR;
			additionalReference1.CY_Data = "GRN1";
			guaranteeHeader1.AddTransaction("Opening Bal", "Opening Bal for GUA1", ZString.Empty, ZString.Empty, 2000m, 0, PermitTransactionStatusList.Codes.Confirmed, transactionType: PermitTransactionTypeList.Codes.OBL, isAggregated: true);
			guaranteeHeader1.AddTransaction("2595950308700261001902", "NCTS departure for GUA1", "111", ZString.Empty, -1000, 0, PermitTransactionStatusList.Codes.Pending, isAggregated: true);
			guaranteeHeader1.CPH_Balance += guaranteeHeader1.GetTransactions().Sum(x => x.CPL_TranValue);

			var guaranteeHeader2 = GetGuaranteeHeader(orgHeader, "GUA2");
			var additionalReference2 = guaranteeHeader2.AdditionalGuaranteeReferences.AddNew();
			additionalReference2.CY_Code = OrgCusAccountDeltaTTypeList.Codes.TR;
			additionalReference2.CY_Data = "GRN2";
			guaranteeHeader2.AddTransaction("Opening Bal", "Opening Bal for GUA2", ZString.Empty, ZString.Empty, 1400m, 0, PermitTransactionStatusList.Codes.Confirmed, transactionType: PermitTransactionTypeList.Codes.OBL, isAggregated: true);
			guaranteeHeader2.AddTransaction("2595950308700261001902", "NCTS departure for GUA2", "111", ZString.Empty, -700, 0, PermitTransactionStatusList.Codes.Pending, isAggregated: true);
			guaranteeHeader2.CPH_Balance += guaranteeHeader2.GetTransactions().Sum(x => x.CPL_TranValue);

			var guaranteeHeader3 = GetGuaranteeHeader(orgHeader, "GUA_NotPresentInIncomingMessage");
			var additionalReference3 = guaranteeHeader3.AdditionalGuaranteeReferences.AddNew();
			additionalReference3.CY_Code = OrgCusAccountDeltaTTypeList.Codes.TR;
			additionalReference3.CY_Data = "GRN3";
			guaranteeHeader3.AddTransaction("Opening Bal", "Opening Bal for GUA2_NotPresentInIncomingMessage", ZString.Empty, ZString.Empty, 1000m, 0, PermitTransactionStatusList.Codes.Confirmed, transactionType: PermitTransactionTypeList.Codes.OBL, isAggregated: true);
			guaranteeHeader3.AddTransaction("2595950308700261001902", "NCTS departure for GUA3_NotPresentInIncomingMessage", "111", ZString.Empty, -500m, 0, PermitTransactionStatusList.Codes.Pending, isAggregated: true);
			guaranteeHeader3.CPH_Balance += guaranteeHeader3.GetTransactions().Sum(x => x.CPL_TranValue);
			Factory.Save();

			var nctsHeader = GetNCTSHeader();
			nctsHeader.BH_HeaderType = EU.NCTS.Business.NctsMovementType.Codes.Departure;
			nctsHeader.MovementHeader.BM_PaperlessInbondNum = "2595950308700261001902";
			nctsHeader.Principal.E2_OA_Address = guaranteeHeader1.PermitHolder.MainAddress.PK;

			var guarantee1 = nctsHeader.GetEffectiveGuarantees().AddNew();
			guarantee1.PW_BondNumber = guaranteeHeader1.CPH_Number;

			var guarantee2 = nctsHeader.GetEffectiveGuarantees().AddNew();
			guarantee2.PW_BondNumber = guaranteeHeader2.CPH_Number;

			var guarantee3 = nctsHeader.GetEffectiveGuarantees().AddNew();
			guarantee3.PW_BondNumber = guaranteeHeader3.CPH_Number;

			AssertEquals("Prerequisite: Guarantee1 should be linked to the nctsHeader", nctsHeader.GetEffectiveGuarantees()[0].CusGuarantee.PK, guarantee1.CusGuarantee.PK);
			AssertEquals("Prerequisite: Guarantee2 should be linked to the nctsHeader", nctsHeader.GetEffectiveGuarantees()[1].CusGuarantee.PK, guarantee2.CusGuarantee.PK);
			AssertEquals("Prerequisite: Guarantee3 should be linked to the nctsHeader", nctsHeader.GetEffectiveGuarantees()[2].CusGuarantee.PK, guarantee3.CusGuarantee.PK);

			var outgoingMessage = GetNCTSFREDIMessage(nctsHeader);
			outgoingMessage.EM_Status = EDIMessage.Status.Received;
			outgoingMessage.EM_ReceiveTransmit = ReceiveTransmitList.Codes.Transmit;
			outgoingMessage.EM_MessageSubType = TP5MessageTypeList.Codes.CC015C;
			outgoingMessage.EM_MessageNum = "111";
			nctsHeader.MovementHeader.Messages.Add(outgoingMessage);

			var incomingMessage = GetNCTSFREDIMessage(nctsHeader);
			incomingMessage.EM_MessageNum = "111";
			nctsHeader.Messages.Add(incomingMessage);

			var messageDataObject = (NCTSMessageDataObject<Cc029CType>)incomingMessage.MessageDataObject;
			AssertEquals("There should be two guarantees in message.", 2, messageDataObject.ResponseMessage.Guarantee.Count);

			var processor = GetNCTSBaseProcessor();
			processor.ProcessMessage(incomingMessage);
			Factory.Save();

			AssertEquals("PermitAppId of LastOutgoingMessage should be equal to MessageReferenceNumber", "111", FRPermitHelper.GetPermitAppIdForMessage(nctsHeader.GetOutgoingMessage(incomingMessage)));
			AssertEquals("MessageSubType of incoming message should be 029", TP5ResponseMessageSubTypeList.Codes.ReleasedForTransit, nctsHeader.Messages.LastIncomingMessage.EM_MessageSubType);

			var permitHelper = new PermitTestDataHelper(Factory);
			var query = permitHelper.GetPermitLineTransactionQuery("2595950308700261001902", $"NCTS departure {nctsHeader.LocalReferenceNumber} adjustment", nctsHeader.Messages.LastIncomingMessage.EM_MessageNum, PermitTransactionCategoryList.Codes.CUM, PermitTransactionTypeList.Codes.TRA);
			var transactionRequested = new BusinessObjectFactory().Load<BaseCusPermitLineTransaction>(query);
			transactionRequested = transactionRequested.OrderByDescending(x => x.CPL_TranValue).ToArray();
			AssertEquals("There should be one adjustment transactions created", 1, transactionRequested.Length);
			AssertEquals("Transaction status should be confirmed", PermitTransactionStatusList.Codes.Confirmed, transactionRequested[0].CPL_TransactionStatus);
			AssertEquals("Transaction CPL_TranValue should be equal to 900 as per adjustment", 900m, transactionRequested[0].CPL_TranValue);
			AssertEquals("PW_Override of guarantee1 should be set to true", true, guarantee1.PW_Override);
			AssertEquals("PW_BondAmount of guarantee1 should be updated to 100", 100m, guarantee1.PW_BondAmount);

			query = permitHelper.GetPermitLineTransactionQuery("2595950308700261001902", "NCTS departure for GUA1", "111", PermitTransactionCategoryList.Codes.CUM, PermitTransactionTypeList.Codes.TRA);
			transactionRequested = new BusinessObjectFactory().Load<BaseCusPermitLineTransaction>(query);
			AssertEquals("There should be one transaction present for this query", 1, transactionRequested.Length);
			AssertEquals("Transaction status should be confirmed", PermitTransactionStatusList.Codes.Confirmed, transactionRequested[0].CPL_TransactionStatus);

			query = permitHelper.GetPermitLineTransactionQuery("2595950308700261001902", "NCTS departure for GUA2", "111", PermitTransactionCategoryList.Codes.CUM, PermitTransactionTypeList.Codes.TRA);
			transactionRequested = new BusinessObjectFactory().Load<BaseCusPermitLineTransaction>(query);
			AssertEquals("There should be one transaction present for this query", 1, transactionRequested.Length);
			AssertEquals("Transaction status should be confirmed", PermitTransactionStatusList.Codes.Confirmed, transactionRequested[0].CPL_TransactionStatus);

			query = permitHelper.GetPermitLineTransactionQuery("2595950308700261001902", "NCTS departure for GUA3_NotPresentInIncomingMessage", "111", PermitTransactionCategoryList.Codes.CUM, PermitTransactionTypeList.Codes.TRA);
			transactionRequested = new BusinessObjectFactory().Load<BaseCusPermitLineTransaction>(query);
			AssertEquals("There should be one transaction present for this query", 1, transactionRequested.Length);
			AssertEquals("Transaction status should be deleted", PermitTransactionStatusList.Codes.Deleted, transactionRequested[0].CPL_TransactionStatus);
		}

		GlbGroup SetUpStaffAndGroup()
		{
			var staff = Factory.New<GlbStaff>();
			staff.FillWithValidTestData();
			staff.GS_Code = "~BB";
			staff.GS_EmailAddress = "test@cargowise.com";

			var group = Factory.NewWithValidTestData<GlbGroup>();
			var groupLink = Factory.NewWithValidTestData<GlbGroupLink>();
			groupLink.GK_GG = group.PK;
			groupLink.GK_GS = staff.PK;
			group.Staff.Load();
			return group;
		}
	}
}
