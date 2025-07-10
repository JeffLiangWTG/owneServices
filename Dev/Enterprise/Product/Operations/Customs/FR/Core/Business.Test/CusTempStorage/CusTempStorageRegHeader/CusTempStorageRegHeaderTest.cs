using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.NCTS.Business.Testing;
using Enterprise.Customs.FR.Business.MessagesWrappers.Send;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using TransactionTypes = Enterprise.Customs.EFTA.TemporaryStorageRegister.Business.CusTempStorageRegLineTransactionTypeList;

namespace Enterprise.Customs.FR.Business.CusTempStorage.Testing
{
	[TestedType(typeof(CusTempStorageRegHeader))]
	sealed class CusTempStorageRegHeaderTest : EnterpriseBusinessObjectTestCase
	{
		public void TestGetStorageRegLineTypeCore()
			=> AssertEquals(typeof(CusTempStorageRegLine), header.GetStorageRegLineType());

		public void TestShouldBeClosed()
		{
			var header = Factory.New<CusTempStorageRegHeader>();
			var regLine1 = header.CusTempStorageRegLines.AddNew();
			var transaction11 = regLine1.CusTempStorageRegLineTransactions.AddNew();
			transaction11.SRT_PackageQty = 10;
			transaction11.SRT_TransactionType = TransactionTypes.Codes.OpeningBalance;
			AssertEquals("Temp. Storage Register Header should not be closed", false, header.ShouldBeClosed);

			var transaction12 = regLine1.CusTempStorageRegLineTransactions.AddNew();
			transaction12.SRT_PackageQty = -10;
			transaction12.SRT_TransactionType = TransactionTypes.Codes.Transaction;
			AssertEquals("Temp. Storage Register Header should be closed", true, header.ShouldBeClosed);

			var regLine2 = header.CusTempStorageRegLines.AddNew();
			var transaction21 = regLine2.CusTempStorageRegLineTransactions.AddNew();
			transaction21.SRT_PackageQty = 2;
			transaction21.SRT_TransactionType = TransactionTypes.Codes.OpeningBalance;
			AssertEquals("Temp. Storage Register Header should not be closed", false, header.ShouldBeClosed);

			var transaction22 = regLine2.CusTempStorageRegLineTransactions.AddNew();
			transaction22.SRT_PackageQty = -2;
			transaction22.SRT_TransactionType = TransactionTypes.Codes.Adjustment;
			AssertEquals("Temp. Storage Register Header should not be closed", true, header.ShouldBeClosed);
		}

		public void TestIsDeclarationClosed()
		{
			var header = Factory.New<CusTempStorageRegHeader>();
			header.SRH_Status = TempStorageDeclarationStatusList.Codes.Open;
			AssertEquals("Temp. Storage Register Header is not closed", false, header.IsDeclarationClosed);

			header.SRH_Status = TempStorageDeclarationStatusList.Codes.Closed;
			AssertEquals("Temp. Storage Register Header is closed", true, header.IsDeclarationClosed);
		}

		public void TestSRH_Reference()
		{
			CombineAssertions(() =>
			{
				AssertEquals("Caption", "TSD Number", DataBoundResourceStrings.GetDataForProperty(header.SRH_ReferenceInfo).Caption);
				AssertEquals("ReadOnly", true, header.SRH_ReferenceInfo.ReadOnly);
			});
		}

		public void TestSRH_InternalReference()
		{
			AssertEquals("Caption", "Job Reference", DataBoundResourceStrings.GetDataForProperty(header.SRH_InternalReferenceInfo).Caption);
			AssertEquals("ReadOnly", true, header.SRH_InternalReferenceInfo.ReadOnly);
		}

		public void TestSRH_ArrivalDate()
		{
			AssertEquals("ReadOnly", true, header.SRH_ArrivalDateInfo.ReadOnly);
		}

		public void TestSRH_PresentationDate()
		{
			AssertEquals("ReadOnly", true, header.SRH_PresentationDateInfo.ReadOnly);
		}

		public void TestSRH_PreviousReferenceType()
		{
			AssertEquals("ReadOnly", true, header.SRH_PreviousReferenceTypeInfo.ReadOnly);
		}

		public void TestSRH_PreviousReference()
		{
			AssertEquals("ReadOnly", true, header.SRH_PreviousReferenceInfo.ReadOnly);
		}

		public void TestSRH_Status()
		{
			AssertEquals("ReadOnly", true, header.SRH_StatusInfo.ReadOnly);
		}

		public void TestLookups()
		{
			AssertType<CusTempStorageRegHeaderLookups>(header.Lookups);
		}

		public void TestValidation()
		{
			AssertType<CusTempStorageRegHeaderValidation>(header.Validation);
		}

		public void TestCusTempStorageRegLines()
		{
			CombineAssertions(() =>
			{
				AssertType<CusTempStorageRegLineCollection>("Type", header.CusTempStorageRegLines);
				AssertEquals("Lines are read-only", true, header.CusTempStorageRegLines.ReadOnly);

				var line = header.CusTempStorageRegLines.AddNew();
				line.SRL_LineNumber = 1;
				AssertEquals("Transactions are editable", true, line.CusTempStorageRegLineTransactions.ReadOnly);

				header.SRH_Status = TempStorageDeclarationStatusList.Codes.Open;
				header.CusTempStorageRegLines.DeleteAll();
				line = header.CusTempStorageRegLines.AddNew();
				line.SRL_LineNumber = 1;
				AssertEquals("Transactions are editable", false, line.CusTempStorageRegLineTransactions.ReadOnly);
			});
		}

		public void TestHumanReadableName()
		{
			header.SRH_Reference = "ATB150000010320006001";
			header.SRH_InternalReference = "FRJ000020";
			AssertEquals("Temp. Storage Register ATB150000010320006001/FRJ000020", header.HumanReadableName);
		}

		public void TestSetDefaultValues()
		{
			CombineAssertions(() =>
			{
				AssertEquals("SRH_AppCode", FRConstants.TemporaryStorage.AppCodeIST, header.SRH_AppCode);
				AssertEquals("SRH_Status", TempStorageDeclarationStatusList.Codes.Closed, header.SRH_Status);
			});
		}

		public void TestReOpenHeader()
		{
			var header = Factory.New<CusTempStorageRegHeader>();
			header.SRH_Reference = "Job001";
			var regLine1 = header.CusTempStorageRegLines.AddNew();
			regLine1.SRL_LineNumber = 1;
			var transaction1 = regLine1.CusTempStorageRegLineTransactions.AddNew();
			transaction1.SRT_PackageQty = 10;
			transaction1.SRT_TransactionType = TransactionTypes.Codes.OpeningBalance;
			var transaction2 = regLine1.CusTempStorageRegLineTransactions.AddNew();
			transaction2.SRT_PackageQty = -10;
			transaction2.SRT_TransactionType = TransactionTypes.Codes.Transaction;
			AssertEquals("Pre-requisite : Temp. Storage Register Header should be CLS.", TempStorageDeclarationStatusList.Codes.Closed, header.SRH_Status);
			Env.Security.FRTempStorageRegisterReOpening.IsAllowed = true;

			header.ReOpen();

			AssertEquals("Header is open", "OPN", header.SRH_Status);
			Assert("Header is marked as re-opened", header.IsReopened);

			var transactionManuallyAdjust = regLine1.CusTempStorageRegLineTransactions.AddNew();
			transactionManuallyAdjust.SRT_PackageQty = 1;
			transactionManuallyAdjust.SRT_TransactionType = TransactionTypes.Codes.Adjustment;
			Factory.Save();

			Assert("UCK log is correctly added to this Temp. Reg. Header.", header.Logs.GetAllLogs().Cast<StmALog>().Any(x => x.SL_SE_NKEvent == "UCK"));
		}

		public void TestUpdateStatus()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType("CUSOF", "CustomsOffice");
			var codeList = helper.CreateCusCodeList("FR", "CUSOF", "FR001", "FR001 Customs", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingRefCusCodeListAttributeName("EmailAddress", "EmailAddress", "CUSOF", "FR");
			codeList.Attributes.AddNew("EmailAddress", "Check.Yao@wisetechglobal.com");
			Factory.Save();

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

			var stmNums = authorisation.CustomsNumberProvider.CustomsNumbers.AddNew();
			stmNums.SN_Type = CusAuthorisationHeaderCustomsNumberRangeTypeList.Codes.TemporaryStorageInstallationDdtNumberFrance;
			stmNums.SN_FountainName = "JAC";
			stmNums.SN_MinimumValue = 1;
			stmNums.SN_Count = 100;
			stmNums.SN_Value = 12;
			Factory.Save();

			var tempStorageheader = CusTempStorageJobHeader.New(Factory, FRConstants.TemporaryStorage.AppCodeIST);
			tempStorageheader.SJH_CustomsOffice = "FR001";
			tempStorageheader.SJH_OH_Customer = customer.PK;
			tempStorageheader.SJH_JobReference = "FRJ00480021";
			tempStorageheader.SJH_CustomsProfile = "TST_ATH_001";
			tempStorageheader.SJH_ArrivalDate = new ZDate(2021, 08, 01);
			tempStorageheader.SJH_PresentationDate = new ZDate(2021, 08, 02);
			tempStorageheader.SJH_TempStorageEndDateUtc = new ZDate(2021, 08, 03);
			tempStorageheader.SJH_PreviousReferenceType = "820";
			tempStorageheader.SJH_PreviousReferenceNumber = "FRJ00480006";
			var tempStorageLine = tempStorageheader.CusTempStorageDec.CusTempStorageLines.Cast<CusTempStorageLine>().Single();
			tempStorageLine.TSL_PackageQty = 100;
			var tempStorageItem = tempStorageLine.CusTempStorageLineItems.AddNew();
			Factory.Save();

			SendWrapperHelper.SendDDT(Factory, tempStorageheader, new ErrorCollector());
			var regHeader = Factory.LoadTop1<CusTempStorageRegHeader>(new ZQuery(CusTempStorageRegHeaderSchema.SRH_InternalReference, "FRJ00480021"));
			AssertNotNull(regHeader);

			Assert(!regHeader.Logs.GetAllLogs().Cast<StmALog>().Any(x => x.SL_SE_NKEvent == Events.ClearanceCompletedCode));
			var line = regHeader.CusTempStorageRegLines.Single();
			var transaction = (CusTempStorageRegLineTransaction)line.CusTempStorageRegLineTransactions.AddNew();
			transaction.SRT_PackageQty = -20;
			AssertEquals("regHeader.SRH_Status is open", TempStorageDeclarationStatusList.Codes.Open, regHeader.SRH_Status);
			Assert("transaction is not readonly", !transaction.ReadOnly);
			AssertEquals("CusTempStorageDec.STH_DeclarationStatus is open", TempStorageDeclarationStatusList.Codes.Open, tempStorageheader.CusTempStorageDec.STH_DeclarationStatus);

			var transaction2 = line.CusTempStorageRegLineTransactions.AddNew();
			transaction2.SRT_PackageQty = -80;
			AssertEquals("Packages remaining reduced to 0.", 0, line.SRL_PackagesRemaining);
			AssertEquals("regHeader.SRH_Status is closed", TempStorageDeclarationStatusList.Codes.Closed, regHeader.SRH_Status);
			Assert("new transaction is not readonly", !transaction.ReadOnly);
			Assert(!regHeader.Logs.GetAllLogs().Cast<StmALog>().Any(x => x.SL_SE_NKEvent == Events.ClearanceCompletedCode));
			regHeader.Factory.Save();
			Assert("new transaction is readonly", transaction.ReadOnly);
			AssertEquals("CusTempStorageDec.STH_DeclarationStatus is closed", TempStorageDeclarationStatusList.Codes.Closed, tempStorageheader.CusTempStorageDec.STH_DeclarationStatus);
			Assert(regHeader.Logs.GetAllLogs().Cast<StmALog>().Any(x => x.SL_SE_NKEvent == Events.ClearanceCompletedCode));
		}

		protected override BusinessObject GetNewBusinessObject() => GetNewBusinessObject(Factory);

		protected override BusinessObject GetBusinessObjectForFetchForLoad() => GetNewBusinessObject(Factory);

		protected override BusinessObject GetNewBusinessObjectForDefaultLightValidationTest() => GetNewBusinessObject(Factory);

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory) => GetNewBusinessObject(factory);

		protected override void SetUp()
		{
			base.SetUp();
			header = Factory.New<CusTempStorageRegHeader>();
			header.SRH_Reference = "TEST";
		}
		CusTempStorageRegHeader header;

		BusinessObject GetNewBusinessObject(BusinessObjectFactory factory)
		{
			var cusTempStorageRegHeader = factory.New<CusTempStorageRegHeader>();
			cusTempStorageRegHeader.SRH_Reference = "TEST";
			return cusTempStorageRegHeader;
		}
	}
}
