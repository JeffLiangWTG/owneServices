using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.EFTA.TemporaryStorageRegister.Business;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.FR.Business.CusTempStorage;
using Enterprise.Customs.Universal.Testing;
using Enterprise.DocumentEngine.Scheduler.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using CusTempStorageRegHeader = Enterprise.Customs.FR.Business.CusTempStorage.CusTempStorageRegHeader;
using CusTempStorageRegLineTransaction = Enterprise.Customs.FR.Business.CusTempStorage.CusTempStorageRegLineTransaction;

namespace Enterprise.Customs.FR.Business.MessagesWrappers.Send.Testing
{
	public class SendWrapperHelperTest : TestCaseWithFactory
	{
		#region SendEmailForDdt
		public void TestSendDDT_CreatesEdoc()
		{
			var errorCollector = new ErrorCollector();
			var header = CusTempStorageJobHeader.New(Factory);
			header.SJH_JobReference = "FRJ000020";
			header.DDTNumber = "DDT123456";
			header.SJH_OH_Customer = Factory.NewWithValidTestData<OrgHeader>().PK;
			SetUpCustomsOffice();
			header.SJH_CustomsOffice = "FR001";
			var storageDec = header.CusTempStorageDec;
			storageDec.STH_OwnerReferenceNumber = "TST1";

			AssertEquals(ZDate.Empty, header.SJH_TempStorageEndDateUtc.Date);
			errorCollector.WipeErrors();
			SendWrapperHelper.SendDDT(Factory, header, errorCollector);
			var printJobs = Factory.Load<StmPrintJob>(new ZQuery(StmPrintJobSchema.SP_ParentGuid, header.PK));
			AssertEquals("Sending of DDT message should have added an eDoc.", 1, printJobs.Length);
		}

		[TestDate(2022, 03, 25, 14, 17, 33)]
		public void TestSendDDT_SJH_TempStorageEndDateUtc()
		{
			var errorCollector = new ErrorCollector();
			var header = CusTempStorageJobHeader.New(Factory);
			header.SJH_JobReference = "FRJ000020";
			header.DDTNumber = "DDT123456";
			header.SJH_OH_Customer = Factory.NewWithValidTestData<OrgHeader>().PK;
			SetUpCustomsOffice();
			header.SJH_CustomsOffice = "FR001";
			var storageDec = header.CusTempStorageDec;
			storageDec.STH_OwnerReferenceNumber = "TST1";

			AssertEquals(ZDate.Empty, header.SJH_TempStorageEndDateUtc.Date);
			errorCollector.WipeErrors();
			var message = SendWrapperHelper.SendDDT(Factory, header, errorCollector);
			AssertEquals(ZDate.Today.AddDays(90), header.SJH_TempStorageEndDateUtc.Date);
		}

		[TestDate(2022, 03, 25, 14, 17, 33)]
		public void TestSendDDT()
		{
			var errorCollector = new ErrorCollector();
			var header = CusTempStorageJobHeader.New(Factory);
			header.SJH_JobReference = "FRJ000020";
			header.DDTNumber = "DDT123456";
			header.SJH_OH_Customer = Factory.NewWithValidTestData<OrgHeader>().PK;
			var storageDec = header.CusTempStorageDec;
			storageDec.STH_OwnerReferenceNumber = "TST1";
			var permitQuery = new ZDBOnlySubQuery(typeof(BaseCusPermitHeader), CusPermitLineTransactionSchema.CPL_CPH_PermitHeader);
			permitQuery.AddToFilter(CusPermitHeaderSchema.CPH_RN_NKCountryCode, Core.Constants.CountryCodes.France);

			var query = new ZDBOnlyQuery(typeof(BaseCusPermitLineTransaction));
			query.AddToFilter(CusPermitLineTransactionSchema.CPL_Reference, header.SJH_JobReference);
			query.AddToFilter(CusPermitLineTransactionSchema.CPL_TransactionStatus, PermitTransactionStatusList.Codes.Confirmed);
			query.AddSubQuery(permitQuery, JoinCondition.And);

			var transactions = Factory.Load<BaseCusPermitLineTransaction>(query);
			AssertEquals(0, transactions.Length);

			SetUpCustomsOffice();
			header.SJH_CustomsOffice = "FR001";
			transactions = Factory.Load<BaseCusPermitLineTransaction>(query);
			AssertEquals(0, transactions.Length);

			errorCollector.WipeErrors();
			var message = SendWrapperHelper.SendDDT(Factory, header, errorCollector);
			AssertEquals(0, errorCollector.ErrorCount);
			AssertEquals("Temp. Storage Register DDT123456/FRJ000020 was created.", message);
			var printJobQuery = new ZQuery(StmPrintJobSchema.SP_EmailSubjectLine, SQLComparisonOperator.Contains, "Advice Of Placement");
			AssertEquals("There should be a print job added to the queue.", 1, Factory.GetDatabaseCount(typeof(StmPrintJob), printJobQuery));

			transactions = Factory.Load<BaseCusPermitLineTransaction>(query);
			AssertEquals(0, transactions.Length);

			var line = storageDec.CusTempStorageLines.AddNew();
			var lineItem = line.CusTempStorageLineItems.AddNew();
			lineItem.TSI_GuaranteedValue = 300m;

			var orgHeader = Factory.New<OrgHeader>();
			orgHeader.OH_Code = "TST";
			var guaranteeHeader = Factory.New<EU.Business.CusGuaranteeHeader>();
			guaranteeHeader.CPH_Type = GuaranteeTypeList.Codes.AI2;
			guaranteeHeader.CPH_Number = "0000001";
			guaranteeHeader.CPH_StartDate = ZDate.Today;
			guaranteeHeader.CPH_OH_PermitHolder = orgHeader.PK;
			var transaction1 = guaranteeHeader.CusGuaranteeLineTransactions.AddNew();
			transaction1.CPL_Reference = "Entry Number";
			transaction1.CPL_TranValue = 1000m;
			transaction1.CPL_TransactionType = PermitTransactionTypeList.Codes.CUS;
			transaction1.CPL_TransactionStatus = PermitTransactionStatusList.Codes.Confirmed;
			transaction1.CPL_TransactionCategory = PermitTransactionCategoryList.Codes.CUM;
			transaction1.CPL_AppId = "Entry Reference";
			transaction1.CPL_Comment = "Instruction Desc.";
			transaction1.CPL_Procedure = "AAA";
			header.SJH_CPH_Guarantee = guaranteeHeader.PK;

			errorCollector.WipeErrors();
			SendWrapperHelper.SendDDT(Factory, header, errorCollector);
			AssertEquals(0, errorCollector.ErrorCount);
			transactions = Factory.Load<BaseCusPermitLineTransaction>(query);
			AssertEquals(1, transactions.Length);
			AssertEquals(-300m, transactions[0].CPL_TranValue);

			lineItem.TSI_GuaranteedValue = 500m;
			Factory.Save();
			transactions = Factory.Load<BaseCusPermitLineTransaction>(query);
			AssertEquals(1, transactions.Length);
			AssertEquals(-300m, transactions[0].CPL_TranValue);

			errorCollector.WipeErrors();
			SendWrapperHelper.SendDDT(Factory, header, errorCollector);
			AssertEquals(0, errorCollector.ErrorCount);
			transactions = Factory.Load<BaseCusPermitLineTransaction>(query);
			AssertEquals(2, transactions.Length);
			AssertEquals(-300m, transactions[0].CPL_TranValue);
			AssertEquals(-200m, transactions[1].CPL_TranValue);
		}

		public void TestSendDDT_ShouldAllocateNewDDTNumber()
		{
			SetUpCustomsOffice();

			var customer = Factory.NewWithValidTestData<OrgHeader>();
			var authorisation = Factory.New<CusAuthorisationHeader>();
			authorisation.CPH_Type = CusAuthorizationHeaderTypeList.Codes.TemporaryStorage;
			authorisation.CPH_Number = "TST_ATH_001";
			authorisation.CPH_OH_PermitHolder = customer.PK;
			authorisation.CPH_StartDate = ZDate.Today.AddMonths(-1);
			authorisation.CPH_EndDate = ZDate.Today.AddMonths(1);
			var rule = authorisation.CusAuthorisationRules.AddNew();
			rule.CPR_RuleCode = CusAuthorisationRuleTypeList.Codes.USE;
			rule.CPR_ValueFrom = FRConstants.TemporaryStorage.AppCodeIST;

			var errorCollector = new ErrorCollector();

			var storageHeader = CusTempStorageJobHeader.New(Factory, FRConstants.TemporaryStorage.AppCodeIST);
			storageHeader.SJH_OH_Customer = customer.PK;
			storageHeader.SJH_CustomsOffice = "FR001";

			storageHeader.SJH_CustomsProfile = "A001";
			SendWrapperHelper.SendDDT(Factory, storageHeader, errorCollector);
			AssertEquals("Invalid customs profile.", "Please select one valid customs profile.", errorCollector.GetErrorsAsString());
			AssertEquals(ZString.Empty, storageHeader.DDTNumber);
			errorCollector.WipeErrors();

			storageHeader.SJH_CustomsProfile = "TST_ATH_001";
			SendWrapperHelper.SendDDT(Factory, storageHeader, errorCollector);
			AssertEquals("No DDT number range.", "There is no valid DDT number ranges defined.\r\nPlease go to Authorization > Number Ranges to create one.", errorCollector.GetErrorsAsString());
			AssertEquals(ZString.Empty, storageHeader.DDTNumber);
			errorCollector.WipeErrors();

			var stmNums = authorisation.CustomsNumberProvider.CustomsNumbers.AddNew();
			stmNums.SN_Type = CusAuthorisationHeaderCustomsNumberRangeTypeList.Codes.TemporaryStorageInstallationDdtNumberFrance;
			stmNums.SN_FountainName = "JAC";
			stmNums.SN_MinimumValue = 1;
			stmNums.SN_Count = 100;
			stmNums.SN_Value = 12;
			Factory.Save();
			SendWrapperHelper.SendDDT(Factory, storageHeader, errorCollector);
			AssertEquals("DDT number allocated.", 0, errorCollector.ErrorCount);
			AssertEquals("JAC000012", storageHeader.DDTNumber);
			errorCollector.WipeErrors();

			SendWrapperHelper.SendDDT(Factory, storageHeader, errorCollector);
			AssertEquals(0, errorCollector.ErrorCount);
			AssertEquals("Should not allocate new number if DDTNumber is not empty.", "JAC000012", storageHeader.DDTNumber);
		}

		[TestDate(2022, 03, 25, 14, 17, 33)]
		public void TestSendDDT_ShouldCreateNewTempStorageRegister()
		{
			SetUpCustomsOffice();

			var errorCollector = new ErrorCollector();
			var header = CusTempStorageJobHeader.New(Factory, FRConstants.TemporaryStorage.AppCodeIST);
			header.SJH_CustomsOffice = "FR001";
			header.SJH_OH_Customer = Factory.NewWithValidTestData<OrgHeader>().PK;
			header.SJH_JobReference = "FRJ00480021";
			header.SJH_ArrivalDate = new ZDate(2021, 08, 01);
			header.SJH_PresentationDate = new ZDate(2021, 08, 02);
			header.DDTNumber = "DDT1";
			header.SJH_PreviousReferenceType = FRConstants.TemporaryStorage.AppCodeIST;
			header.SJH_PreviousReferenceNumber = "FRJ_IST1";
			header.CusTempStorageDec.CusTempStorageLines.RemoveAndDeleteAll();

			var header2 = CusTempStorageJobHeader.New(Factory, FRConstants.TemporaryStorage.AppCodeIST);
			header2.SJH_CustomsOffice = "FR001";
			header2.SJH_OH_Customer = Factory.NewWithValidTestData<OrgHeader>().PK;
			header2.SJH_JobReference = "FRJ_IST1";
			header2.SJH_ArrivalDate = new ZDate(2021, 08, 01);
			header2.SJH_PresentationDate = new ZDate(2021, 08, 02);
			header2.CusTempStorageDec.CusTempStorageLines.RemoveAndDeleteAll();

			var line1 = header.CusTempStorageDec.CusTempStorageLines.AddNew();
			line1.TSL_OwnerReferenceType = "AWB";
			line1.TSL_OwnerReferenceNumber = "OWNREF001";
			line1.TSL_UnionStatus = "T1";
			line1.TSL_LocationOfGoods = "FR002300";
			line1.TSL_PackageQty = 100;
			line1.TSL_PackageType = "1A";
			line1.TSL_GrossWeight = 1000;
			line1.TSL_GrossWeightUQ = Core.Constants.Weight.Grams;
			line1.TSL_ReferenceNumberLine = 1;

			var line2 = header.CusTempStorageDec.CusTempStorageLines.AddNew();
			line2.TSL_OwnerReferenceType = "HWB";
			line2.TSL_OwnerReferenceNumber = "OWNREF002";
			line2.TSL_UnionStatus = "T2";
			line2.TSL_LocationOfGoods = new ZString('F', 35);
			line2.TSL_PackageQty = 200;
			line2.TSL_PackageType = "1B";
			line2.TSL_GrossWeight = 2;
			line2.TSL_GrossWeightUQ = ZString.Empty;
			line2.TSL_ReferenceNumberLine = 2;

			var line3 = header2.CusTempStorageDec.CusTempStorageLines.AddNew();
			line3.TSL_OwnerReferenceType = "AWB";
			line3.TSL_OwnerReferenceNumber = "OWNREF001";
			line3.TSL_UnionStatus = "T1";
			line3.TSL_LocationOfGoods = "FR002300";
			line3.TSL_PackageQty = 300;
			line3.TSL_PackageType = "1A";
			line3.TSL_GrossWeight = 1000;
			line3.TSL_GrossWeightUQ = Core.Constants.Weight.Kilograms;
			line3.TSL_ReferenceNumberLine = 1;

			var line4 = header2.CusTempStorageDec.CusTempStorageLines.AddNew();
			line4.TSL_OwnerReferenceType = "HWB";
			line4.TSL_OwnerReferenceNumber = "OWNREF002";
			line4.TSL_UnionStatus = "T2";
			line4.TSL_LocationOfGoods = new ZString('F', 35);
			line4.TSL_PackageQty = 400;
			line4.TSL_PackageType = "1B";
			line4.TSL_GrossWeight = 2;
			line4.TSL_GrossWeightUQ = Core.Constants.Weight.Kilograms;
			line4.TSL_ReferenceNumberLine = 2;
			Factory.Save();

			var regheader = Factory.New<CusTempStorageRegHeader>();
			regheader.SRH_InternalReference = header2.SJH_JobReference;
			regheader.SRH_Reference = header2.SJH_JobReference;
			var regline1 = regheader.CusTempStorageRegLines.AddNew();
			regline1.FillWithValidTestData();
			regline1.SRL_LineNumber = 1;
			var transaction = regline1.CusTempStorageRegLineTransactions.AddNew();
			transaction.SRT_GrossWeight = new ZWeight(line3.TSL_GrossWeight, line3.TSL_GrossWeightUQ).InKilograms;
			transaction.SRT_PackageQty = line3.TSL_PackageQty;
			transaction.SRT_ReferenceType = CusTempStorageRegLineTransactionReferenceTypeList.Codes.IST;
			transaction.SRT_Reference = header.SJH_JobReference;

			var regline2 = regheader.CusTempStorageRegLines.AddNew();
			regline2.FillWithValidTestData();
			regline2.SRL_LineNumber = 2;
			var transaction2 = regline2.CusTempStorageRegLineTransactions.AddNew();
			transaction2.SRT_GrossWeight = new ZWeight(line4.TSL_GrossWeight, line4.TSL_GrossWeightUQ).InKilograms;
			transaction2.SRT_PackageQty = line4.TSL_PackageQty;
			transaction2.SRT_ReferenceType = CusTempStorageRegLineTransactionReferenceTypeList.Codes.IST;
			transaction2.SRT_Reference = header.SJH_JobReference;

			AssertEquals(true, Factory.Exists(typeof(CusTempStorageRegHeader), new ZQuery(CusTempStorageRegHeaderSchema.SRH_InternalReference, header2.SJH_JobReference)));
			AssertEquals(false, Factory.Exists(typeof(CusTempStorageRegHeader), new ZQuery(CusTempStorageRegHeaderSchema.SRH_InternalReference, header.SJH_JobReference)));
			SendWrapperHelper.SendDDT(Factory, header, errorCollector);
			AssertEquals(0, errorCollector.ErrorCount);

			var regHeader = Factory.LoadTop1<CusTempStorageRegHeader>(new ZQuery(CusTempStorageRegHeaderSchema.SRH_InternalReference, "FRJ00480021"));
			AssertEquals("DDT1", regHeader.SRH_Reference);
			AssertEquals("FRJ00480021", regHeader.SRH_InternalReference);
			AssertEquals(FRConstants.TemporaryStorage.AppCodeIST, regHeader.SRH_AppCode);
			AssertEquals(new ZDate(2021, 08, 01), regHeader.SRH_ArrivalDate);
			AssertEquals(new ZDate(2021, 08, 02), regHeader.SRH_PresentationDate);
			AssertEquals(FRConstants.TemporaryStorage.AppCodeIST, regHeader.SRH_PreviousReferenceType);
			AssertEquals("FRJ_IST1", regHeader.SRH_PreviousReference);
			AssertEquals(TempStorageDeclarationStatusList.Codes.Open, regHeader.SRH_Status);

			var regLines = regHeader.CusTempStorageRegLines;
			AssertEquals(2, regLines.Count);

			var regLine1 = regLines[0];
			AssertEquals(1, regLine1.SRL_LineNumber);
			AssertEquals("AWB", regLine1.SRL_OwnerReferenceType);
			AssertEquals("OWNREF001", regLine1.SRL_OwnerReference);
			AssertEquals("T1", regLine1.SRL_UnionStatus);
			AssertEquals("FR002300", regLine1.SRL_LocationOfGoods);
			AssertEquals(100, regLine1.SRL_PackagesRemaining);
			AssertEquals("1A", regLine1.SRL_PackageType);
			AssertEquals(ZDate.Today.AddDays(90), regLine1.SRL_LimitDate);
			AssertEquals(Core.Constants.Weight.Kilograms, regLine1.SRL_GrossWeightUQ);

			var transaction1 = regLine1.CusTempStorageRegLineTransactions.Single();
			AssertEquals(CusTempStorageRegLineTransactionTypeList.Codes.OpeningBalance, transaction1.SRT_TransactionType);
			AssertEquals(1m, transaction1.SRT_GrossWeight);
			AssertEquals(100, transaction1.SRT_PackageQty);
			AssertEquals(CusTempStorageRegLineTransactionReferenceTypeList.Codes.IST, transaction1.SRT_ReferenceType);
			AssertEquals("DDT1", transaction1.SRT_Reference);
			AssertEquals("FRJ00480021", transaction1.SRT_InternalReferenceNumber);

			var regLine2 = regLines[1];
			AssertEquals(2, regLine2.SRL_LineNumber);
			AssertEquals("HWB", regLine2.SRL_OwnerReferenceType);
			AssertEquals("OWNREF002", regLine2.SRL_OwnerReference);
			AssertEquals("T2", regLine2.SRL_UnionStatus);
			AssertEquals("Should truncate the goods location to correct length.", new ZString('F', 35), regLine2.SRL_LocationOfGoods);
			AssertEquals(200, regLine2.SRL_PackagesRemaining);
			AssertEquals("1B", regLine2.SRL_PackageType);
			AssertEquals(ZDate.Today.AddDays(90), regLine2.SRL_LimitDate);
			AssertEquals(Core.Constants.Weight.Kilograms, regLine2.SRL_GrossWeightUQ);

			var transaction3 = regLine2.CusTempStorageRegLineTransactions.Single();
			AssertEquals(CusTempStorageRegLineTransactionTypeList.Codes.OpeningBalance, transaction3.SRT_TransactionType);
			AssertEquals("If the gross weight unit is empty, we use KG as default.", 2m, transaction3.SRT_GrossWeight);
			AssertEquals(200, transaction3.SRT_PackageQty);
			AssertEquals(CusTempStorageRegLineTransactionReferenceTypeList.Codes.IST, transaction3.SRT_ReferenceType);
			AssertEquals("DDT1", transaction3.SRT_Reference);
			AssertEquals("FRJ00480021", transaction3.SRT_InternalReferenceNumber);
			var transactionregLine2 = regLine2.CusTempStorageRegLineTransactions.Single();
			AssertEquals(CusTempStorageRegLineTransactionTypeList.Codes.OpeningBalance, transactionregLine2.SRT_TransactionType);
			AssertEquals("If the gross weight unit is empty, we use KG as default.", 2m, transactionregLine2.SRT_GrossWeight);
			AssertEquals(200, transactionregLine2.SRT_PackageQty);
			AssertEquals(CusTempStorageRegLineTransactionReferenceTypeList.Codes.IST, transactionregLine2.SRT_ReferenceType);
			AssertEquals("DDT1", transactionregLine2.SRT_Reference);

			var regHeader2 = Factory.LoadTop1<CusTempStorageRegHeader>(new ZQuery(CusTempStorageRegHeaderSchema.SRH_InternalReference, header2.SJH_JobReference));

			var regLines2 = regHeader2.CusTempStorageRegLines;
			AssertEquals(2, regLines2.Count);

			var regLine1Header2 = regHeader2.CusTempStorageRegLines.First(x => x.SRL_LineNumber == 1);
			var regLine2Header2 = regHeader2.CusTempStorageRegLines.First(x => x.SRL_LineNumber == 2);

			var transactionRegLine1Header2 = (CusTempStorageRegLineTransaction)regLine1Header2.CusTempStorageRegLineTransactions.ElementAt(1);
			var transactionRegLine2Header2 = (CusTempStorageRegLineTransaction)regLine2Header2.CusTempStorageRegLineTransactions.ElementAt(1);
			AssertNotNull(transactionRegLine1Header2);
			AssertNotNull(transactionRegLine2Header2);

			CombineAssertions("Transaction in register values", () =>
			{
				AssertEquals(CusTempStorageRegLineTransactionTypeList.Codes.Transaction, transactionRegLine1Header2.SRT_TransactionType);
				AssertEquals(CusTempStorageRegLineTransactionTypeList.Descriptions.Transaction, transactionRegLine1Header2.TransactionTypeDescription);
				AssertEquals(header.SJH_JobReference, transactionRegLine1Header2.SRT_InternalReferenceNumber);
				AssertEquals(header.DDTNumber, transactionRegLine2Header2.SRT_Reference);
				AssertEquals(1m, transactionRegLine1Header2.SRT_GrossWeight);
				AssertEquals(-100, transactionRegLine1Header2.SRT_PackageQty);
				AssertEquals(TempStorageTransactionRefTypeList.Codes.IstHeader, transactionRegLine1Header2.SRT_ReferenceType);
				AssertEquals("", transactionRegLine1Header2.SRT_Comments);
			});

			CombineAssertions("Transaction in register values", () =>
			{
				AssertEquals(CusTempStorageRegLineTransactionTypeList.Codes.Transaction, transactionRegLine2Header2.SRT_TransactionType);
				AssertEquals(CusTempStorageRegLineTransactionTypeList.Descriptions.Transaction, transactionRegLine2Header2.TransactionTypeDescription);
				AssertEquals(header.SJH_JobReference, transactionRegLine1Header2.SRT_InternalReferenceNumber);
				AssertEquals(header.DDTNumber, transactionRegLine2Header2.SRT_Reference);
				AssertEquals(2m, transactionRegLine2Header2.SRT_GrossWeight);
				AssertEquals(-200, transactionRegLine2Header2.SRT_PackageQty);
				AssertEquals(TempStorageTransactionRefTypeList.Codes.IstHeader, transactionRegLine2Header2.SRT_ReferenceType);
				AssertEquals("", transactionRegLine2Header2.SRT_Comments);
			});
		}

		public void TestSendDDT_ShouldAddEvent()
		{
			var header = CusTempStorageJobHeader.New(Factory);
			header.SJH_OH_Customer = Factory.NewWithValidTestData<OrgHeader>().PK;
			SetUpCustomsOffice();
			header.SJH_CustomsOffice = "FR001";
			header.DDTNumber = "DDT123456";
			_ = SendWrapperHelper.SendDDT(Factory, header, new ErrorCollector());
			AssertEquals("There should be a new event added to the declaration.", 1, header.Logs.Find(x => x.SL_SE_NKEvent == Events.InStoreCode).Count());
		}

		void SetUpCustomsOffice()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType("CUSOF", "CustomsOffice");
			var codeList = helper.CreateCusCodeList("FR", "CUSOF", "FR001", "FR001 Customs", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingRefCusCodeListAttributeName("EmailAddress", "EmailAddress", "CUSOF", "FR");
			codeList.Attributes.AddNew("EmailAddress", "Check.Yao@wisetechglobal.com");

			var codeListWithoutEmail = helper.CreateCusCodeList("FR", "CUSOF", "FR002", "FR002 Customs", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			Factory.Save();
		}

		public void TestGetLiquidationWithOnlyCW1Fees()
		{
			var declaration = Factory.New<Declaration.JobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine1 = invoice.InvoiceLines.AddNew();

			var cusEntryHeader = declaration.CustomsEntryHeaders.AddNew();
			cusEntryHeader.CH_BGMReference = "8461132";
			var cusEntryLine = cusEntryHeader.MergedLines.AddNew();
			invoiceLine1.JI_CL = cusEntryLine.PK;

			var fee1 = cusEntryLine.Fees.AddNew();
			fee1.CF_RateOverrideReasonCode = RateOverrideReasonList.Codes.Additional;
			fee1.CF_ChargeType = "B01";
			fee1.NationalFeeTypeCode = "A445";
			fee1.CF_Rate = 5;
			fee1.CF_BaseValue = 10;
			fee1.CF_ChargeAmount = 20;
			fee1.CF_MethodOfPayment = "A";
			var fee2 = cusEntryLine.Fees.AddNew();
			fee2.CF_RateOverrideReasonCode = RateOverrideReasonList.Codes.Override;
			fee2.CF_ChargeType = "A04";
			fee2.NationalFeeTypeCode = "A280";
			fee2.CF_Rate = 2;
			fee2.CF_BaseValue = 30;
			fee2.CF_ChargeAmount = 40;
			fee2.CF_MethodOfPayment = "B";
			var fee3 = cusEntryLine.Fees.AddNew();
			fee3.CF_RateOverrideReasonCode = RateOverrideReasonList.Codes.Additional;
			fee3.CF_ChargeType = "A01";
			fee3.NationalFeeTypeCode = "A325";
			fee3.CF_Rate = 2;
			fee3.CF_BaseValue = 30;
			fee3.CF_ChargeAmount = 40;
			fee3.CF_MethodOfPayment = "B";
			var fee4 = cusEntryLine.Fees.AddNew();
			fee4.CF_RateOverrideReasonCode = ZString.Empty;
			fee4.CF_ChargeType = "A02";
			fee4.NationalFeeTypeCode = "A240";
			fee4.CF_Rate = 3;
			fee4.CF_BaseValue = 40;
			fee4.CF_ChargeAmount = 50;
			fee4.CF_MethodOfPayment = "C";

			invoiceLine1.JI_TariffBypassCode = ZString.Empty;
			var liquidation = SendWrapperHelper.GetLiquidation(cusEntryHeader, true);
			AssertEquals(0, liquidation.Count);

			invoiceLine1.JI_TariffBypassCode = "1";
			liquidation = SendWrapperHelper.GetLiquidation(cusEntryHeader, true);
			AssertEquals(0, liquidation.Count);

			invoiceLine1.JI_TariffBypassCode = ZString.Empty;
			liquidation = SendWrapperHelper.GetLiquidation(cusEntryHeader, false);
			AssertEquals(2, liquidation.Count);

			AssertEquals("override CF_RateOverrideReasonCode", "A280", liquidation.ElementAt(0).TaxDetail.Tax.TaxCode);
			AssertEquals("empty CF_RateOverrideReasonCode", "A240", liquidation.ElementAt(1).TaxDetail.Tax.TaxCode);

			invoiceLine1.JI_TariffBypassCode = "1";
			liquidation = SendWrapperHelper.GetLiquidation(cusEntryHeader, false);
			AssertEquals(1, liquidation.Count);

			AssertEquals("override CF_RateOverrideReasonCode", "A280", liquidation.ElementAt(0).TaxDetail.Tax.TaxCode);
		}

		public void TestGetLiquidationWithOnlyCUSFees()
		{
			var declaration = Factory.New<Declaration.JobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine1 = invoice.InvoiceLines.AddNew();

			var cusEntryHeader = declaration.CustomsEntryHeaders.AddNew();
			cusEntryHeader.CH_BGMReference = "8461132";
			var cusEntryLine = cusEntryHeader.MergedLines.AddNew();
			invoiceLine1.JI_CL = cusEntryLine.PK;

			var fee1 = cusEntryLine.ConfirmedFees.AddNew();
			fee1.CF_RateOverrideReasonCode = RateOverrideReasonList.Codes.Additional;
			fee1.CF_ChargeType = "B01";
			fee1.NationalFeeTypeCode = "A445";
			fee1.CF_Rate = 5;
			fee1.CF_BaseValue = 10;
			fee1.CF_ChargeAmount = 20;
			fee1.CF_MethodOfPayment = "A";
			var fee2 = cusEntryLine.ConfirmedFees.AddNew();
			fee2.CF_RateOverrideReasonCode = RateOverrideReasonList.Codes.Override;
			fee2.CF_ChargeType = "A04";
			fee2.NationalFeeTypeCode = "A280";
			fee2.CF_Rate = 2;
			fee2.CF_BaseValue = 30;
			fee2.CF_ChargeAmount = 40;
			fee2.CF_MethodOfPayment = "B";
			var fee3 = cusEntryLine.ConfirmedFees.AddNew();
			fee3.CF_RateOverrideReasonCode = RateOverrideReasonList.Codes.Additional;
			fee3.CF_ChargeType = "A01";
			fee3.NationalFeeTypeCode = "A325";
			fee3.CF_Rate = 2;
			fee3.CF_BaseValue = 30;
			fee3.CF_ChargeAmount = 40;
			fee3.CF_MethodOfPayment = "B";
			var fee4 = cusEntryLine.ConfirmedFees.AddNew();
			fee4.CF_RateOverrideReasonCode = ZString.Empty;
			fee4.CF_ChargeType = "A02";
			fee4.NationalFeeTypeCode = "A240";
			fee4.CF_Rate = 3;
			fee4.CF_BaseValue = 40;
			fee4.CF_ChargeAmount = 50;
			fee4.CF_MethodOfPayment = "C";

			invoiceLine1.JI_TariffBypassCode = ZString.Empty;
			var liquidation = SendWrapperHelper.GetLiquidation(cusEntryHeader, true);
			AssertEquals(2, liquidation.Count);

			AssertEquals("override CF_RateOverrideReasonCode", "A280", liquidation.ElementAt(0).TaxDetail.Tax.TaxCode);
			AssertEquals("empty CF_RateOverrideReasonCode", "A240", liquidation.ElementAt(1).TaxDetail.Tax.TaxCode);

			invoiceLine1.JI_TariffBypassCode = "1";
			liquidation = SendWrapperHelper.GetLiquidation(cusEntryHeader, true);
			AssertEquals(1, liquidation.Count);

			AssertEquals("override CF_RateOverrideReasonCode", "A280", liquidation.ElementAt(0).TaxDetail.Tax.TaxCode);

			invoiceLine1.JI_TariffBypassCode = ZString.Empty;
			liquidation = SendWrapperHelper.GetLiquidation(cusEntryHeader, false);
			AssertEquals(0, liquidation.Count);

			invoiceLine1.JI_TariffBypassCode = "1";
			liquidation = SendWrapperHelper.GetLiquidation(cusEntryHeader, false);
			AssertEquals(0, liquidation.Count);
		}

		public void TestGetLiquidationWithMixedFees()
		{
			var declaration = Factory.New<Declaration.JobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine1 = invoice.InvoiceLines.AddNew();

			var cusEntryHeader = declaration.CustomsEntryHeaders.AddNew();
			cusEntryHeader.CH_BGMReference = "8461132";
			var cusEntryLine = cusEntryHeader.MergedLines.AddNew();
			invoiceLine1.JI_CL = cusEntryLine.PK;

			var fee1 = cusEntryLine.Fees.AddNew();
			fee1.CF_RateOverrideReasonCode = RateOverrideReasonList.Codes.Additional;
			fee1.CF_ChargeType = "B01";
			fee1.NationalFeeTypeCode = "A445";
			fee1.CF_Rate = 5;
			fee1.CF_BaseValue = 10;
			fee1.CF_ChargeAmount = 20;
			fee1.CF_MethodOfPayment = "A";
			var fee2 = cusEntryLine.Fees.AddNew();
			fee2.CF_RateOverrideReasonCode = RateOverrideReasonList.Codes.Override;
			fee2.CF_ChargeType = "A04";
			fee2.NationalFeeTypeCode = "A280";
			fee2.CF_Rate = 2;
			fee2.CF_BaseValue = 30;
			fee2.CF_ChargeAmount = 40;
			fee2.CF_MethodOfPayment = "B";
			var fee3 = cusEntryLine.ConfirmedFees.AddNew();
			fee3.CF_RateOverrideReasonCode = RateOverrideReasonList.Codes.Additional;
			fee3.CF_ChargeType = "A01";
			fee3.NationalFeeTypeCode = "A325";
			fee3.CF_Rate = 2;
			fee3.CF_BaseValue = 30;
			fee3.CF_ChargeAmount = 40;
			fee3.CF_MethodOfPayment = "B";
			var fee4 = cusEntryLine.ConfirmedFees.AddNew();
			fee4.CF_RateOverrideReasonCode = ZString.Empty;
			fee4.CF_ChargeType = "A02";
			fee4.NationalFeeTypeCode = "A240";
			fee4.CF_Rate = 3;
			fee4.CF_BaseValue = 40;
			fee4.CF_ChargeAmount = 50;
			fee4.CF_MethodOfPayment = "C";

			invoiceLine1.JI_TariffBypassCode = ZString.Empty;
			var liquidation = SendWrapperHelper.GetLiquidation(cusEntryHeader, true);
			AssertEquals(1, liquidation.Count);

			AssertEquals("Confirmed fees empty CF_RateOverrideReasonCode", "A240", liquidation.ElementAt(0).TaxDetail.Tax.TaxCode);

			invoiceLine1.JI_TariffBypassCode = "1";
			liquidation = SendWrapperHelper.GetLiquidation(cusEntryHeader, true);
			AssertEquals(0, liquidation.Count);

			invoiceLine1.JI_TariffBypassCode = ZString.Empty;
			liquidation = SendWrapperHelper.GetLiquidation(cusEntryHeader, false);
			AssertEquals(1, liquidation.Count);

			AssertEquals("Confirmed fees override CF_RateOverrideReasonCode", "A280", liquidation.ElementAt(0).TaxDetail.Tax.TaxCode);

			invoiceLine1.JI_TariffBypassCode = "1";
			liquidation = SendWrapperHelper.GetLiquidation(cusEntryHeader, false);
			AssertEquals(1, liquidation.Count);
			AssertEquals("Confirmed fees override CF_RateOverrideReasonCode", "A280", liquidation.ElementAt(0).TaxDetail.Tax.TaxCode);
		}

		public void TestGetLiquidationWithEntryHeaderCharges_OnlyCW1Charges()
		{
			var cusEntryHeader = prepareEntryHeaderCharges(false, false);

			var liquidation = SendWrapperHelper.GetLiquidation(cusEntryHeader, true);
			AssertEquals("useConfirmedFees and No ConfirmedCharges", 0, liquidation.Count);

			liquidation = SendWrapperHelper.GetLiquidation(cusEntryHeader, false);
			AssertEquals(2, liquidation.Count);
			AssertEquals("Not useConfirmedFees and wrap Calculated Charges: A01", "A01", liquidation.ElementAt(0).TaxDetail.Tax.TaxCode);
			AssertEquals("Not useConfirmedFees and wrap Calculated Charges: A02", "A02", liquidation.ElementAt(1).TaxDetail.Tax.TaxCode);
		}

		public void TestGetLiquidationWithEntryHeaderCharges_OnlyConfirmedCharges()
		{
			var cusEntryHeader = prepareEntryHeaderCharges(true, false);
			var liquidation = SendWrapperHelper.GetLiquidation(cusEntryHeader, true);
			AssertEquals("2 ConfirmedCharges", 2, liquidation.Count);
			AssertEquals("wrap ConfirmedCharges: A01", "A01", liquidation.ElementAt(0).TaxDetail.Tax.TaxCode);
			AssertEquals("wrap ConfirmedCharges: A02", "A02", liquidation.ElementAt(1).TaxDetail.Tax.TaxCode);

			liquidation = SendWrapperHelper.GetLiquidation(cusEntryHeader, false);
			AssertEquals("Not use ConfirmedCharges and no Calculated Charges", 0, liquidation.Count);
		}

		public void TestGetLiquidationWithEntryHeaderCharges_MixedCharges()
		{
			var cusEntryHeader = prepareEntryHeaderCharges(true, true);

			var liquidation = SendWrapperHelper.GetLiquidation(cusEntryHeader, true);
			AssertEquals("2 ConfirmedCharges", 2, liquidation.Count);
			AssertEquals("wrap ConfirmedCharges: A01", "A01", liquidation.ElementAt(0).TaxDetail.Tax.TaxCode);
			AssertEquals("wrap ConfirmedCharges: A02", "A02", liquidation.ElementAt(1).TaxDetail.Tax.TaxCode);

			liquidation = SendWrapperHelper.GetLiquidation(cusEntryHeader, false);
			AssertEquals("1 Charges", 1, liquidation.Count);
			AssertEquals("wrap Calculated Charges: A03", "A03", liquidation.ElementAt(0).TaxDetail.Tax.TaxCode);
		}

		Declaration.CusEntryHeader prepareEntryHeaderCharges(bool useConfirmedCharges, bool useMixedCharges)
		{
			var declaration = Factory.New<Declaration.JobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine1 = invoice.InvoiceLines.AddNew();
			invoiceLine1.JI_TariffBypassCode = ZString.Empty;

			var cusEntryHeader = declaration.CustomsEntryHeaders.AddNew();
			cusEntryHeader.CH_BGMReference = "8461132";
			var cusEntryLine = cusEntryHeader.MergedLines.AddNew();
			cusEntryLine.CL_LineNumber = 1;
			invoiceLine1.JI_CL = cusEntryLine.PK;

			var charge1 = useConfirmedCharges ? cusEntryHeader.ConfirmedCharges.AddNew() : cusEntryHeader.Charges.AddNew();
			charge1.C1_ChargeType = "A01";
			charge1.C1_ChargeAmount = 10m;
			charge1.C1_MethodOfPayment = "1";
			var charge2 = useConfirmedCharges ? cusEntryHeader.ConfirmedCharges.AddNew() : cusEntryHeader.Charges.AddNew();
			charge2.C1_ChargeType = "A02";
			charge2.C1_ChargeAmount = 20m;
			charge2.C1_MethodOfPayment = "2";

			if (useMixedCharges)
			{
				var charge3 = cusEntryHeader.Charges.AddNew();
				charge3.C1_ChargeType = "A03";
				charge3.C1_ChargeAmount = 30m;
				charge3.C1_MethodOfPayment = "3";
			}

			return cusEntryHeader;
		}

		#endregion
	}
}
