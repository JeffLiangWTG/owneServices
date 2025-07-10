using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Customs.FR.MessageDefinitions.DeltaT.CC029B;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.Common;
using Enterprise.Customs.EFTA.TemporaryStorageRegister.Business;
using Enterprise.Customs.EU.Business.CodeDescriptionPairLists;
using Enterprise.Customs.FR.Business;
using Enterprise.Customs.FR.Business.CusTempStorage;
using Enterprise.Customs.FR.Registry;
using Enterprise.DocumentEngine.FlexCelInterface;
using Enterprise.DocumentEngine.Scheduler.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using CusTempStorageRegHeader = Enterprise.Customs.FR.Business.CusTempStorage.CusTempStorageRegHeader;
using CusTempStorageRegLineTransaction = Enterprise.Customs.FR.Business.CusTempStorage.CusTempStorageRegLineTransaction;
using CusTempStorageRegLineTransactionReferenceTypeList = Enterprise.Customs.FR.Business.CusTempStorageRegLineTransactionReferenceTypeList;
using NctsHeader = Enterprise.Customs.FR.Business.NCTS.NctsHeader;
using NctsTransitStatusList = Enterprise.Customs.EU.NCTS.Business.NctsTransitStatusList;

namespace Enterprise.Customs.FR.NCTS.ServiceTask.Testing
{
	class DTCC029BProcessorTest : DTBaseProcessorTest<Cc029BType>
	{
		public void TestTheTadDocumentGeneratedWhenProcessing()
		{
			var helper = new FRNctsResponseProcessingTests();
			var permitHelper = new PermitTestDataHelper(Factory);
			var eoriCode1 = "123456789000";
			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			org1.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, eoriCode1, GlbCompany.CurrentCompany.GC_RN_NKCountryCode);
			var cusGuaranteeHeaderList = new List<CusGuaranteeHeader>();
			cusGuaranteeHeaderList.Add(GenerateGuaranteeHeader(org1, "19860101", "NCT00050167", "111", 100m));
			cusGuaranteeHeaderList.Add(GenerateGuaranteeHeader(org1, "19860102", "NCT00050167", "112", 300m));
			Factory.Save();

			var nctsheader = Factory.New<NctsHeader>();
			nctsheader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
			nctsheader.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Departure);

			var goodsItem = nctsheader.MovementHeader.GoodsItems.AddNew();
			goodsItem.BY_GrossWeight = 20m;
			var package1 = goodsItem.Packages.AddNew();
			package1.B5_UnitCount = 40;

			helper.GetProcessReceivedMessageHeader("DT029B_MESSAGE.xml", cusGuaranteeHeaderList, org1, nctsheader);

			var queuedPrintJobs = Factory.Load<StmPrintJob>(new ZQuery(StmPrintJobSchema.SP_ParentGuid, nctsheader.PK));
			AssertEquals("There should be a TAD/TSAD in the transit declaration print job queue.", 1, queuedPrintJobs.Length);

			using (var excelInterface = new ExcelInterface())
			{
				excelInterface.LoadExcelFile(queuedPrintJobs[0].SP_CustomProperties);
				AssertContains("Contains labels in english", "A2 - Considered Satisfactory", excelInterface.WorkSheets[0].ToString());
			}
		}

		public void TestNctsguaranteelinkedtoTheConfirmedDepartureMessageAreUpdated()
		{
			var helper = new FRNctsResponseProcessingTests();
			var permitHelper = new PermitTestDataHelper(Factory);
			var eoriCode1 = "123456789000";
			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			org1.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, eoriCode1, GlbCompany.CurrentCompany.GC_RN_NKCountryCode);
			var cusGuaranteeHeaderList = new List<CusGuaranteeHeader>();
			var guaranteeHeader1 = GenerateGuaranteeHeader(org1, "19860101", "NCT00050167", "111", 100m);
			var guaranteeHeader2 = GenerateGuaranteeHeader(org1, "19860102", "NCT00050167", "112", 300m);
			Factory.Save();

			var query = permitHelper.GetPermitLineTransactionQuery("NCT00050167", "CMT-CON", "111", PermitTransactionCategoryList.Codes.CUM, PermitTransactionTypeList.Codes.TRA);
			Factory.Save();
			var transactionRequested = Factory.Load<BaseCusPermitLineTransaction>(query);
			AssertEquals(PermitTransactionStatusList.Codes.Confirmed, transactionRequested[0].CPL_TransactionStatus);

			query = permitHelper.GetPermitLineTransactionQuery("NCT00050167", "CMT-TO-CONF", "111", PermitTransactionCategoryList.Codes.CUM, PermitTransactionTypeList.Codes.OBL);
			transactionRequested = new BusinessObjectFactory().Load<BaseCusPermitLineTransaction>(query);
			AssertEquals(PermitTransactionStatusList.Codes.Pending, transactionRequested[0].CPL_TransactionStatus);

			cusGuaranteeHeaderList.Add(guaranteeHeader1);
			cusGuaranteeHeaderList.Add(guaranteeHeader2);

			var header = helper.GetProcessReceivedMessageHeader("DT029B_MESSAGE.xml", cusGuaranteeHeaderList, org1);

			AssertEquals(true, header.BH_JobReference.Equals("NCT00050167"));
			AssertEquals(true, EU.NCTS.Business.NctsPermitHelper.GetPermitAppIdForMessage(header.Messages.LastOutgoingMessage).Equals("111"));
			AssertEquals(2, header.Messages.Count);
			AssertEquals("029", header.Messages.LastIncomingMessage.EM_MessageType);

			query = permitHelper.GetPermitLineTransactionQuery("NCT00050167", "CMT-CON", "111", PermitTransactionCategoryList.Codes.CUM, PermitTransactionTypeList.Codes.TRA);
			Factory.Save();
			transactionRequested = new BusinessObjectFactory().Load<BaseCusPermitLineTransaction>(query);
			AssertEquals(PermitTransactionStatusList.Codes.Confirmed, transactionRequested[0].CPL_TransactionStatus);

			query = permitHelper.GetPermitLineTransactionQuery("NCT00050167", "CMT-TO-CONF", "111", PermitTransactionCategoryList.Codes.CUM, PermitTransactionTypeList.Codes.OBL);
			Factory.Save();
			transactionRequested = new BusinessObjectFactory().Load<BaseCusPermitLineTransaction>(query);
			AssertEquals(PermitTransactionStatusList.Codes.Confirmed, transactionRequested[0].CPL_TransactionStatus);
			AssertEquals(1, transactionRequested.Length);
		}

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
			Factory.Save();

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

			var helper = new FRNctsResponseProcessingTests();
			var permitHelper = new PermitTestDataHelper(Factory);
			var eoriCode1 = "123456789000";
			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			org1.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, eoriCode1, GlbCompany.CurrentCompany.GC_RN_NKCountryCode);
			var cusGuaranteeHeaderList = new List<CusGuaranteeHeader>
			{
				GenerateGuaranteeHeader(org1, "19860101", "NCT00050167", "111", 100m),
				GenerateGuaranteeHeader(org1, "19860102", "NCT00050167", "112", 300m)
			};
			Factory.Save();

			TestDateAttribute.AddMinutes(1);

			var nctsheader = Factory.New<NctsHeader>();
			nctsheader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
			nctsheader.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Departure);

			var goodsItem = nctsheader.MovementHeader.GoodsItems.AddNew();
			goodsItem.BY_GrossWeight = 20m;
			var package1 = goodsItem.Packages.AddNew();
			package1.B5_UnitCount = 40;

			var pd = goodsItem.PreviousDocuments.AddNew();
			pd.CSI_Code = PreviousDocumentCodeList.Codes._337;
			pd.CSI_ReferenceNumber = "FRJ_IST1";
			pd.CSI_LineNo = 1;

			helper.GetProcessReceivedMessageHeader("DT029B_MESSAGE.xml", cusGuaranteeHeaderList, org1, nctsheader);

			var regHeader = new BusinessObjectFactory().LoadTop1<CusTempStorageRegHeader>(new ZQuery(CusTempStorageRegHeaderSchema.SRH_InternalReference, "FRJ_IST1"));
			var regLines = regHeader.CusTempStorageRegLines;
			AssertEquals(1, regLines.Count);

			var regLineFinal = regHeader.CusTempStorageRegLines.First(x => x.SRL_LineNumber == 1);

			var transaction = (CusTempStorageRegLineTransaction)regLineFinal.CusTempStorageRegLineTransactions.OrderBy(x => x.SRT_SystemCreateTimeUtc).ElementAt(1);

			CombineAssertions("Transaction values", () =>
			{
				AssertEquals(CusTempStorageRegLineTransactionTypeList.Codes.Transaction, transaction.SRT_TransactionType);
				AssertEquals("Transaction", transaction.TransactionTypeDescription);
				AssertEquals("NCT00050167", transaction.SRT_InternalReferenceNumber);
				AssertEquals("", transaction.SRT_Reference);
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

			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
			nctsHeader.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Departure);

			var goods1 = nctsHeader.MovementHeader.GoodsItems.AddNew();
			goods1.BY_GrossWeight = 20m;
			var pack11 = goods1.Packages.AddNew();
			pack11.B5_UnitCount = 50;
			var pack12 = goods1.Packages.AddNew();
			pack12.B5_UnitCount = 30;  // expect a transaction line on reg1 with package -80

			var pd1 = goods1.PreviousDocuments.AddNew();
			pd1.CSI_Code = PreviousDocumentCodeList.Codes._337;
			pd1.CSI_ReferenceNumber = "FRJ_IST1";
			pd1.CSI_LineNo = 1;

			var goods2 = nctsHeader.MovementHeader.GoodsItems.AddNew();
			goods2.BY_GrossWeight = 40m;
			var pack21 = goods2.Packages.AddNew();
			pack21.B5_UnitCount = 70;  // expect a transaction line on reg2 with package -70

			var pd2 = goods2.PreviousDocuments.AddNew();
			pd2.CSI_Code = PreviousDocumentCodeList.Codes._337;
			pd2.CSI_ReferenceNumber = "FRJ_IST2";
			pd2.CSI_LineNo = 1;

			var goods3 = nctsHeader.MovementHeader.GoodsItems.AddNew();
			goods3.BY_GrossWeight = 60m;
			var pack31 = goods3.Packages.AddNew();
			pack31.B5_UnitCount = 60;  // expect no transaction line as FRJ_IST3 is not a valid reference

			var pd3 = goods3.PreviousDocuments.AddNew();
			pd3.CSI_Code = PreviousDocumentCodeList.Codes._337;
			pd3.CSI_ReferenceNumber = "FRJ_IST3";
			pd3.CSI_LineNo = 1;

			Factory.Save();

			var helper = new FRNctsResponseProcessingTests();
			helper.GetProcessReceivedMessageHeader("DT029B_MESSAGE.xml", null, null, nctsHeader);

			var reg1Reloaded = new BusinessObjectFactory().LoadTop1<CusTempStorageRegHeader>(new ZQuery(CusTempStorageRegHeaderSchema.SRH_InternalReference, "FRJ_IST1"));
			var regLine1Reloaded = reg1Reloaded.CusTempStorageRegLines.Single();
			AssertEquals(20, regLine1Reloaded.SRL_PackagesRemaining);
			var transaction1BReloaded = regLine1Reloaded.CusTempStorageRegLineTransactions.Single(x => x.SRT_TransactionType == CusTempStorageRegLineTransactionTypeList.Codes.Transaction);
			AssertEquals(-80, transaction1BReloaded.SRT_PackageQty);

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

			var helper = new FRNctsResponseProcessingTests();
			var permitHelper = new PermitTestDataHelper(Factory);
			var eoriCode1 = "123456789000";
			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			org1.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, eoriCode1, GlbCompany.CurrentCompany.GC_RN_NKCountryCode);
			var cusGuaranteeHeaderList = new List<CusGuaranteeHeader>();
			cusGuaranteeHeaderList.Add(GenerateGuaranteeHeader(org1, "19860101", "NCT00050167", "111", 100m));
			cusGuaranteeHeaderList.Add(GenerateGuaranteeHeader(org1, "19860102", "NCT00050167", "112", 300m));
			Factory.Save();

			var nctsheader = Factory.New<NctsHeader>();
			nctsheader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
			nctsheader.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Departure);
			nctsheader.BH_SystemCreateUser = "~BB";

			var goodsItem = nctsheader.MovementHeader.GoodsItems.AddNew();
			goodsItem.BY_GrossWeight = 20m;
			var package1 = goodsItem.Packages.AddNew();
			package1.B5_UnitCount = 70;

			var pd = goodsItem.PreviousDocuments.AddNew();
			pd.CSI_Code = PreviousDocumentCodeList.Codes._337;
			pd.CSI_ReferenceNumber = "FRJ_IST1";
			pd.CSI_LineNo = 1;

			var ist2 = CusTempStorageJobHeader.New(Factory, FRConstants.TemporaryStorage.AppCodeIST);
			ist2.SJH_JobReference = "FRJ_IST2";
			ist2.SJH_OH_Customer = Factory.NewWithValidTestData<OrgHeader>().PK;

			var frNctsReponseHelper = new FRNctsReponseHelper();
			var realMessage = frNctsReponseHelper.GetEmbeddedResourceFile("DT029B_MESSAGE.xml");
			var departure = helper.SetupAndRunMessageProcessor(EU.NCTS.Business.NctsMovementType.Codes.Departure, realMessage, NctsTransitStatusList.Codes.Unknown, EU.NCTS.Business.NctsMessageStatusList.Codes.DepartureDeclarationSent, true, true, cusGuaranteeHeaderList, org1, nctsheader);
			Factory.Save();

			var regHeader = Factory.LoadTop1<CusTempStorageRegHeader>(new ZQuery(CusTempStorageRegHeaderSchema.SRH_InternalReference, "FRJ_IST1"));
			var regLines = regHeader.CusTempStorageRegLines;
			AssertEquals(1, regLines.Count);

			var regLineFinal = regHeader.CusTempStorageRegLines.First(x => x.SRL_LineNumber == 1);

			AssertEquals(1, regLineFinal.CusTempStorageRegLineTransactions.Count);

			Assert(departure.Logs.GetAllLogs().Cast<StmALog>().Any(x => x.SL_Reference.Contains("Register " + regHeader.SRH_Reference + " has not been updated")));

			var mails = Env.OutgoingCustomsMailManager.EmailsCreated;
			CombineAssertions(() =>
			{
				AssertEquals(1, mails.Count);
				AssertEquals("New Transit response received. Reference: NCT00050167", mails[0].Subject);
				AssertEquals(@"A Transit response has been received. Warning : Register DDTNUMBER has not been updated for transit NCT00050167, there are not enough packages remaining.", mails[0].Body);
			});
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

		public void TestpendingTransactionlinkedtoTheConfirmedDepartureMessageAreConfirmed()
		{
			var helper = new FRNctsResponseProcessingTests();
			var permitHelper = new PermitTestDataHelper(Factory);
			var eoriCode1 = "123456789000";
			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			org1.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, eoriCode1, GlbCompany.CurrentCompany.GC_RN_NKCountryCode);
			var cusGuaranteeHeaderList = new List<CusGuaranteeHeader>();
			var guaranteeHeader1 = GenerateGuaranteeHeader(org1, "19860101", "NCT00050167", "111", 100m);
			Factory.Save();

			var query = permitHelper.GetPermitLineTransactionQuery("NCT00050167", "CMT-CON", "111", PermitTransactionCategoryList.Codes.CUM, PermitTransactionTypeList.Codes.TRA);
			Factory.Save();
			var transactionRequested = Factory.Load<BaseCusPermitLineTransaction>(query);
			AssertEquals(PermitTransactionStatusList.Codes.Confirmed, transactionRequested[0].CPL_TransactionStatus);

			query = permitHelper.GetPermitLineTransactionQuery("NCT00050167", "CMT-TO-CONF", "111", PermitTransactionCategoryList.Codes.CUM, PermitTransactionTypeList.Codes.OBL);
			transactionRequested = new BusinessObjectFactory().Load<BaseCusPermitLineTransaction>(query);
			AssertEquals(PermitTransactionStatusList.Codes.Pending, transactionRequested[0].CPL_TransactionStatus);
			AssertEquals(1, transactionRequested.Length);

			cusGuaranteeHeaderList.Add(guaranteeHeader1);
			var header = helper.GetProcessReceivedMessageHeader("DT029B_MESSAGE.xml", cusGuaranteeHeaderList, org1);

			AssertEquals(true, header.BH_JobReference.Equals("NCT00050167"));
			AssertEquals(true, EU.NCTS.Business.NctsPermitHelper.GetPermitAppIdForMessage(header.Messages.LastOutgoingMessage).Equals("111"));
			AssertEquals(2, header.Messages.Count);
			AssertEquals("029", header.Messages.LastIncomingMessage.EM_MessageType);

			query = permitHelper.GetPermitLineTransactionQuery("NCT00050167", "CMT-CON", "111", PermitTransactionCategoryList.Codes.CUM, PermitTransactionTypeList.Codes.TRA);
			Factory.Save();
			transactionRequested = Factory.Load<BaseCusPermitLineTransaction>(query);
			AssertEquals(PermitTransactionStatusList.Codes.Confirmed, transactionRequested[0].CPL_TransactionStatus);

			query = permitHelper.GetPermitLineTransactionQuery("NCT00050167", "CMT-TO-CONF", "111", PermitTransactionCategoryList.Codes.CUM, PermitTransactionTypeList.Codes.OBL);
			transactionRequested = new BusinessObjectFactory().Load<BaseCusPermitLineTransaction>(query);
			AssertEquals(PermitTransactionStatusList.Codes.Confirmed, transactionRequested[0].CPL_TransactionStatus);
			AssertEquals(1, transactionRequested.Length);

			var bond = header.Guarantees.Cast<CusBondDetail>().FirstOrDefault(x => x.PW_BondNumber == "19860101");
			AssertEquals(header.BH_JobReference, transactionRequested[0].CPL_Reference);
			AssertEquals(100m, transactionRequested[0].CPL_TranValue);
		}

		protected override IEnumerable<DTMessageProcessorTestCase> DTMessageProcessorTestCases
		{
			get
			{
				yield return new DTMessageProcessorTestCase
				{
					IncomingMessageText = frNctsReponseHelper.GetEmbeddedResourceFile("DT029B_MESSAGE.xml"),
					ExpectedNewMessageStatus = EU.NCTS.Business.NctsMessageStatusList.Codes.Ok,
					ExpectedNewDepartureStatus = NctsTransitStatusList.Codes.GoodsReleasedForTransitAtDeparture,
					ExpectedNewDetailedDepartureStatus = ZString.Empty,
					ExpectedNewMessageInterpretation = @"
<p>New departure status: Goods Released For Transit At Departure</p>
<p>Status granted on: 11/04/2019 11:28</p>"
				};
			}
		}

		CusGuaranteeHeader GenerateGuaranteeHeader(OrgHeader org1, ZString pW_BondNumber, ZString transactionReference, ZString transactionAppId, ZDecimal valueTransaction)
		{
			var guaranteeHeader = Factory.NewWithValidTestData<CusGuaranteeHeader>();
			guaranteeHeader.CPH_Number = pW_BondNumber;
			guaranteeHeader.CPH_OH_PermitHolder = org1.PK;
			guaranteeHeader.CPH_StartDate = new ZDate(2020, 7, 15);
			guaranteeHeader.CPH_SystemCreateTimeUtc = new ZDate(2020, 7, 15);
			guaranteeHeader.CPH_Type = EUGuaranteeTypeList.Codes.TRA;
			guaranteeHeader.AddTransaction(transactionReference, "CMT-TO-CONF", transactionAppId, "", valueTransaction, 0, PermitTransactionStatusList.Codes.Pending, transactionType: PermitTransactionTypeList.Codes.OBL, isAggregated: true);
			guaranteeHeader.AddTransaction(transactionReference, "CMT-CON", transactionAppId, "", -10, 0, PermitTransactionStatusList.Codes.Confirmed, isAggregated: true);
			AssertEquals(valueTransaction, guaranteeHeader.CPH_Calc_OpeningBalance);
			return guaranteeHeader;
		}
	}
}
