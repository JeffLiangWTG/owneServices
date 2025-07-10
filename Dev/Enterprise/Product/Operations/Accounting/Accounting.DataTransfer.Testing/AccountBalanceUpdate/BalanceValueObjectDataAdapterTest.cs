using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.ARAP.Journal;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.DataTransfer.DataAdapters;
using Enterprise.DataTransfer.Xml.Testing;
using Enterprise.DataTransfer.Xml.XsdVersion1;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Accounting.DataTransfer.AccountBalanceUpdate.Testing
{
	[TestedType(typeof(BalanceValueObjectDataAdapter))]
	class BalanceValueObjectDataAdapterTest : ValueObjectDataAdapterTest<Journal, Balance>
	{
		public void TestSaveARJournal_WillNotTiggerCriticalValidationError_CreateTransactionRestriction()
		{
			var currentCompanyData = NewOrg.CompanyData;
			currentCompanyData.OB_ARTransactionCreationRestriction = Enterprise.Core.Constants.TransactionCreationRestriction.All;

			var nonCurrentCompanyData = NewOrg.GetCompanyDataForGlbCompany(company);
			nonCurrentCompanyData.OB_ARTransactionCreationRestriction = Enterprise.Core.Constants.TransactionCreationRestriction.None;

			Factory.Save();

			SystemDataRegistry.Instance.OrganisationMatchnigTypeItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "ENT");

			var arJournalOtherCompany = Adapter.CreateOrUpdateFromValueObject(ValueObj.Balance[2], Context) as ARJournal;
			AssertNoExceptionThrown("Will not show Critical Validation Error: CreateTransactionRestriction", () => Factory.Save());
			AssertEquals(true, arJournalOtherCompany.IsInDatabase);
			AssertEquals(nonCurrentCompanyData.Company.PK, arJournalOtherCompany.Company.PK);
		}

		public void TestSaveAPJournal_WillNotTiggerCriticalValidationError_CreateTransactionRestriction()
		{
			var currentCompanyData = NewOrg.CompanyData;
			currentCompanyData.OB_APTransactionCreationRestriction = Enterprise.Core.Constants.TransactionCreationRestriction.All;

			var nonCurrentCompanyData = NewOrg.GetCompanyDataForGlbCompany(company);
			nonCurrentCompanyData.OB_APTransactionCreationRestriction = Enterprise.Core.Constants.TransactionCreationRestriction.None;
			nonCurrentCompanyData.OB_IsCreditor = true;

			Factory.Save();

			SystemDataRegistry.Instance.OrganisationMatchnigTypeItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "ENT");

			ValueObj.Balance[2].Ledger = TxnLedgerType.AP;
			var apJournalOtherCompany = Adapter.CreateOrUpdateFromValueObject(ValueObj.Balance[2], Context) as APJournal;
			AssertNoExceptionThrown("Will not show Critical Validation Error: CreateTransactionRestriction", () => Factory.Save());
			AssertEquals(true, apJournalOtherCompany.IsInDatabase);
			AssertEquals(nonCurrentCompanyData.Company.PK, apJournalOtherCompany.Company.PK);
		}

		[TestDate(2015, 1, 15)]
		public void TestCreateOrUpdateFromValueObject_SavingInCompanyContext()
		{
			// Set up additional AP Balance for other company
			CreateXsdBalance(ValueObj, 1600.0m, branch2.GB_Code, TxnLedgerType.AP, CurrentOrg.OH_Code);
			SetBalance(CurrentOrg, 1100.0m, ZArchitecture.Core.LedgerTypes.AccountsPayable, branch2);

			Factory.Save();

			AssertEquals(0, Factory.ChildFactories.Count);

			TestCreateOrUpdateFromValueObject_EntepriseCodeOrgMatching();

			AssertEquals(2, Factory.ChildFactories.Count);

			// Assert additional Journal
			APJournal aPJournalOtherCompany = Adapter.CreateOrUpdateFromValueObject(ValueObj.Balance[3], Context) as APJournal;
			AssertNotNull(aPJournalOtherCompany);
			Assert(!aPJournalOtherCompany.IsDeleted);
			AssertEquals(2700m, aPJournalOtherCompany.AH_OutstandingAmount);
			AssertEquals(2700m, aPJournalOtherCompany.AH_InvoiceAmount);
			AssertEquals(2700m, aPJournalOtherCompany.AH_OSTotal);
			AssertEquals(Core.Constants.DebitCredit.Credit, aPJournalOtherCompany.DebitCreditSign);

			AssertEquals("ZZZ", aPJournalOtherCompany.Branch.GB_Code);
			AssertEquals("JPY", aPJournalOtherCompany.AH_RX_NKTransactionCurrency);
			AssertEquals(GlbDepartment.CurrentDepartment.GE_Code, aPJournalOtherCompany.Department.GE_Code);

			AssertEquals(2, Factory.ChildFactories.Count);
			BusinessObjectFactory childFactory = Factory.ChildFactories[0];
			AssertNotNull(childFactory);

			Factory.Save();
		}

		[TestDate(2015, 1, 15)]
		public void TestCreateOrUpdateFromValueObject_EntepriseCodeOrgMatching()
		{
			SystemDataRegistry.Instance.OrganisationMatchnigTypeItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "ENT");

			ARJournal = Adapter.CreateOrUpdateFromValueObject(ValueObj.Balance[0], Context) as ARJournal;
			AssertNotNull(ARJournal);
			Assert(!ARJournal.IsDeleted);
			AssertEquals(-200.0m, ARJournal.AH_OutstandingAmount);
			AssertEquals(-200.0m, ARJournal.AH_InvoiceAmount);
			AssertEquals(-200.0m, ARJournal.AH_OSTotal);

			AssertEquals("AAA", ARJournal.Branch.GB_Code);
			AssertEquals("AUD", ARJournal.AH_RX_NKTransactionCurrency);
			AssertEquals(GlbDepartment.CurrentDepartment.GE_Code, ARJournal.Department.GE_Code);

			APJournal = Adapter.CreateOrUpdateFromValueObject(ValueObj.Balance[1], Context) as APJournal;
			AssertNotNull(APJournal);
			Assert(!APJournal.IsDeleted);
			AssertEquals(-9200.0m, APJournal.AH_OutstandingAmount);
			AssertEquals(-9200.0m, APJournal.AH_InvoiceAmount);
			AssertEquals(-9200.0m, APJournal.AH_OSTotal);

			AssertEquals("AAA", APJournal.Branch.GB_Code);
			AssertEquals("AUD", APJournal.AH_RX_NKTransactionCurrency);
			AssertEquals(GlbDepartment.CurrentDepartment.GE_Code, ARJournal.Department.GE_Code);

			var arJournalOtherCompany = Adapter.CreateOrUpdateFromValueObject(ValueObj.Balance[2], Context) as ARJournal;
			AssertNotNull(arJournalOtherCompany);
			Assert(!arJournalOtherCompany.IsDeleted);
			AssertEquals(-600.0m, arJournalOtherCompany.AH_OutstandingAmount);
			AssertEquals(-600.0m, arJournalOtherCompany.AH_InvoiceAmount);
			AssertEquals(-600.0m, arJournalOtherCompany.AH_OSTotal);
			AssertEquals("ZZZ", arJournalOtherCompany.Branch.GB_Code);
			AssertEquals("JPY", arJournalOtherCompany.AH_RX_NKTransactionCurrency);
			AssertEquals(GlbDepartment.CurrentDepartment.GE_Code, arJournalOtherCompany.Department.GE_Code);
		}

		[TestDate(2015, 1, 15)]
		public void TestCreateOrUpdateFromValueObject_LegacyCodeOrgMatching()
		{
			ARJournal = Adapter.CreateOrUpdateFromValueObject(ValueObj.Balance[0], Context) as ARJournal;
			AssertNotNull(ARJournal);
			Assert(ARJournal.IsDeleted);
			AssertContains("Could not match organization AAASYD. Account Balance will not be updated.", Notifications.AsString);

			ValueObj.Balance[0].OrganisationCode = "legacy";
			OrgCusCode legacyCode = CurrentOrg.CustomsCodes.AddNew();
			legacyCode.OK_CodeType = "LSC";
			legacyCode.OK_CustomsRegNo = "legacy";

			ARJournal = Adapter.CreateOrUpdateFromValueObject(ValueObj.Balance[0], Context) as ARJournal;
			AssertEquals(-200.0m, ARJournal.AH_OutstandingAmount);
			AssertEquals(-200.0m, ARJournal.AH_InvoiceAmount);
			AssertEquals(-200.0m, ARJournal.AH_OSTotal);
			AssertEquals("AAA", ARJournal.Branch.GB_Code);
			AssertEquals("AUD", ARJournal.AH_RX_NKTransactionCurrency);
			AssertEquals(GlbDepartment.CurrentDepartment.GE_Code, ARJournal.Department.GE_Code);
		}

		public void TestCreateOrUpdateFromValueObject_BranchCodeNotMatched()
		{
			SystemDataRegistry.Instance.OrganisationMatchnigTypeItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "ENT");

			ValueObj.Balance[0].BranchCode = "UUU";
			ARJournal aRJournal = Adapter.CreateOrUpdateFromValueObject(ValueObj.Balance[0], Context) as ARJournal;
			AssertNotNull(aRJournal);
			Assert(aRJournal.IsDeleted);
			Assert(Notifications.AsString.Contains("Could not match branch UUU. Account Balance will not be updated."));
		}

		public void TestCreateOrUpdateFromValueObject_BalanceAlreadyCorrect()
		{
			SystemDataRegistry.Instance.OrganisationMatchnigTypeItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "ENT");

			ValueObj.Balance[0].BalanceValue = 1200.0m;
			ARJournal = Adapter.CreateOrUpdateFromValueObject(ValueObj.Balance[0], Context) as ARJournal;
			AssertNotNull(ARJournal);
			Assert(ARJournal.IsDeleted);
			AssertContains("AR Journal for organization AAASYD in branch AAA: balance not updated as it is already correct.", Notifications.AsString);
		}

		public void TestCreateOrUpdateFromValueObject_DoesNotImportInvalidJournal_BranchIsInvalid()
		{
			var inactiveBranch = Factory.NewWithValidTestData<GlbBranch>();
			inactiveBranch.GB_Code = "BAD";
			inactiveBranch.GB_GC = GlbCompany.CurrentCompany.PK;
			inactiveBranch.GB_IsActive = false;
			Factory.Save();

			SystemDataRegistry.Instance.OrganisationMatchnigTypeItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "ENT");

			var arJournalBizObj = ValueObj.Balance[0];
			arJournalBizObj.BranchCode = "BAD";

			using (Env.SetTemporaryUserContext(Env.CurrentUser.LoginName, inactiveBranch.PK.ToGuid(), Env.CurrentDepartment.PK))
			{
				ARJournal = Adapter.CreateOrUpdateFromValueObject(arJournalBizObj, Context) as ARJournal;
			}

			var expectedErrorMessage = "This Branch is inactive - it may not be used";
			Assert(Notifications.HasErrors);
			AssertContains(expectedErrorMessage, Notifications.AsString);

			AssertNotNull(ARJournal);
			Assert(ARJournal.IsDeleted);

			Notifications.Clear();
			var apJournalBizObj = ValueObj.Balance[1];
			apJournalBizObj.BranchCode = "BAD";

			using (Env.SetTemporaryUserContext(Env.CurrentUser.LoginName, inactiveBranch.PK.ToGuid(), Env.CurrentDepartment.PK))
			{
				APJournal = Adapter.CreateOrUpdateFromValueObject(apJournalBizObj, Context) as APJournal;
			}

			AssertContains(expectedErrorMessage, Notifications.AsString);
			AssertNotNull(APJournal);
			Assert(APJournal.IsDeleted);
		}

		public void TestCreateOrUpdateFromValueObject_DoesNotImportInvalidJournal_OrgIsValid()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_Code = "BAD";
			org.OH_IsDebtor = false;
			org.OH_IsCreditor = false;
			Factory.Save();

			SystemDataRegistry.Instance.OrganisationMatchnigTypeItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "ENT");

			var arJournalBizObj = ValueObj.Balance[0];
			arJournalBizObj.OrganisationCode = "BAD";
			ARJournal = Adapter.CreateOrUpdateFromValueObject(arJournalBizObj, Context) as ARJournal;

			var expectedErrorMessage = "The Organization must have an Organization Type of Receivables selected.";
			AssertContains(expectedErrorMessage, Notifications.AsString);
			AssertNotNull(ARJournal);
			Assert(ARJournal.IsDeleted);

			Notifications.Clear();

			var apJournalBizObj = ValueObj.Balance[1];
			apJournalBizObj.OrganisationCode = "BAD";
			APJournal = Adapter.CreateOrUpdateFromValueObject(apJournalBizObj, Context) as APJournal;

			expectedErrorMessage = "The Organization must have an Organization Type of Payables selected.";
			AssertContains(expectedErrorMessage, Notifications.AsString);
			AssertNotNull(APJournal);
			Assert(APJournal.IsDeleted);
		}

		#region Set Up

		BalanceValueObjectDataAdapter Adapter;
		Balances ValueObj;
		ValueObjectImportContext Context;
		NotificationBuffer Notifications;
		OrgHeader CurrentOrg;
		OrgHeader NewOrg;
		GlbCompany company;
		GlbBranch branch2;
		ARJournal ARJournal;
		APJournal APJournal;
		TestObjectCreator Creator;

		Balance CreateXsdBalance(Balances valueObj, ZDecimal balanceValue, ZString branchCode, TxnLedgerType ledger, ZString organizationCode)
		{
			Balance journal = valueObj.Balance.AddNew();
			journal.BalanceValue = balanceValue;
			journal.BranchCode = branchCode;
			journal.Ledger = ledger;
			journal.OrganisationCode = organizationCode;
			journal.DepartmentCode = Env.CurrentDepartment.Code;

			return journal;
		}

		protected override void SetUp()
		{
			base.SetUp();

			Adapter = new BalanceValueObjectDataAdapter();
			Notifications = new NotificationBuffer();
			Context = new ValueObjectImportContext(Factory, Notifications);
			ValueObj = new Balances();
			Creator = new TestObjectCreator(Factory);

			GlbBranch branch1 = Factory.NewWithValidTestData<GlbBranch>();
			branch1.GB_Code = "AAA";
			branch1.GB_GC = GlbCompany.CurrentCompany.PK;

			branch2 = Factory.NewWithValidTestData<GlbBranch>();
			branch2.GB_Code = "ZZZ";
			company = Factory.NewWithValidTestData<GlbCompany>();
			company.GC_RN_NKCountryCode = "JP";
			company.GC_RX_NKLocalCurrency = "JPY";
			company.GC_OH_OrgProxy = Factory.NewWithValidTestData<OrgHeader>().PK;
			branch2.GB_GC = company.PK;

			Factory.Save();

			var year = ZDateTime.Now.Year;
			var periodTestHelper = new AccountingPeriodTestHelper();
			periodTestHelper.PostPeriodsForEntireYear(year, company.PK, AccountingPeriodTestHelper.CalendarType.CalendarYear);
			periodTestHelper.PostPeriodsForEntireYear(year + 1, company.PK, AccountingPeriodTestHelper.CalendarType.CalendarYear);
			periodTestHelper.PostPeriodsForEntireYear(year, GlbCompany.CurrentCompany.PK, AccountingPeriodTestHelper.CalendarType.CalendarYear);
			periodTestHelper.PostPeriodsForEntireYear(year + 1, GlbCompany.CurrentCompany.PK, AccountingPeriodTestHelper.CalendarType.CalendarYear);

			CurrentOrg = Factory.NewWithValidTestData<OrgHeader>();
			CurrentOrg.OH_Code = "AAASYD";
			CurrentOrg.CompanyData.OB_IsDebtor = true;
			CurrentOrg.CompanyData.OB_IsCreditor = true;
			NewOrg = Factory.NewWithValidTestData<OrgHeader>();
			NewOrg.OH_Code = "AAASYD1";
			var companyDataForNewOrg = NewOrg.GetCompanyDataForGlbCompany(company);
			companyDataForNewOrg.OB_IsDebtor = true;

			var companyDataForCurrentOrg = CurrentOrg.GetCompanyDataForGlbCompany(company);
			companyDataForCurrentOrg.OB_IsCreditor = true;

			CreateXsdBalance(ValueObj, 1000.0m, branch1.GB_Code, TxnLedgerType.AR, CurrentOrg.OH_Code);
			CreateXsdBalance(ValueObj, -10000.0m, branch1.GB_Code, TxnLedgerType.AP, CurrentOrg.OH_Code);
			CreateXsdBalance(ValueObj, 1200.0m, branch2.GB_Code, TxnLedgerType.AR, NewOrg.OH_Code);

			SetBalance(CurrentOrg, 1200.0m, ZArchitecture.Core.LedgerTypes.AccountsReceivable, branch1);
			SetBalance(CurrentOrg, 800.0m, ZArchitecture.Core.LedgerTypes.AccountsPayable, branch1);
			SetBalance(NewOrg, 1800.0m, ZArchitecture.Core.LedgerTypes.AccountsReceivable, branch2);

			Factory.Save();
		}

		void SetBalance(OrgHeader currentOrg, ZDecimal balance, ZString ledger, GlbBranch branch)
		{
			AccTransactionHeader headerToMatch = Factory.NewWithValidTestData<AccTransactionHeader>();
			Invoice invoice = null;
			if (ledger == LedgerTypes.AccountsReceivable)
			{
				invoice = (ARInvoice)Creator.CreateInvoiceWithLine(typeof(ARInvoice), CurrentOrg.OH_Code + TestObjectCreator.GetRandomString(10), Creator.AUD, 1.0m, balance * 2, 0m, balance * 2, 0m);
				invoice.AH_OH = currentOrg.PK;
				invoice.AH_GB = branch.PK;
				invoice.Lines[0].AL_GB = branch.PK;

				AccTransactionMatchLink matchLink = ((IMatching)invoice).CurrentMatchGroup.AddNew();
				matchLink.AP_Amount = balance;
				matchLink.AP_AH = invoice.PK;
				matchLink.TransactionHeader.AH_GB = branch.PK;

				invoice.AH_OutstandingAmount = balance;
				headerToMatch.AH_InvoiceAmount = -balance;

				AccTransactionMatchLink linkToMatch = ((IMatching)invoice).CurrentMatchGroup.AddNew();
				linkToMatch.AP_AH = headerToMatch.PK;
				linkToMatch.AP_Amount = -balance;
				linkToMatch.TransactionHeader.AH_GB = branch.PK;
				TestObjectCreator.SetupMatchLinkMatchDate(invoice);
			}
			if (ledger == LedgerTypes.AccountsPayable)
			{
				invoice = (APInvoice)Creator.CreateInvoiceWithLine(typeof(APInvoice), CurrentOrg.OH_Code + TestObjectCreator.GetRandomString(10), Creator.AUD, 1.0m, balance * 2, 0m, balance * 2, 0m);
				invoice.AH_OH = currentOrg.PK;
				invoice.AH_GB = branch.PK;
				invoice.Lines[0].AL_GB = branch.PK;

				AccTransactionMatchLink matchLink = ((IMatching)invoice).CurrentMatchGroup.AddNew();
				matchLink.AP_Amount = -balance;
				matchLink.AP_AH = invoice.PK;
				matchLink.TransactionHeader.AH_GB = branch.PK;

				invoice.AH_OutstandingAmount = -balance;
				headerToMatch.AH_InvoiceAmount = balance;

				AccTransactionMatchLink linkToMatch = ((IMatching)invoice).CurrentMatchGroup.AddNew();
				linkToMatch.AP_AH = headerToMatch.PK;
				linkToMatch.AP_Amount = balance;
				linkToMatch.TransactionHeader.AH_GB = branch.PK;
				TestObjectCreator.SetupMatchLinkMatchDate(invoice);
			}
		}

		#endregion

		#region Overrides

		protected override string ExpectedRootCollectionElementName
		{
			get { return "Balances"; }
		}

		protected override string ExpectedRootElementName
		{
			get { return "Balance"; }
		}

		protected override BusinessObjectAndExpectedOutputFileName GetEmptyBizObjSample()
		{
			return null;
		}

		protected override BusinessObjectAndExpectedOutputFileName GetPopulatedBizObjWithEmptyFieldsSample()
		{
			return null;
		}

		protected override BusinessObjectAndExpectedOutputFileName GetFullyPopulatedBizObjSample()
		{
			return null;
		}

		protected override BusinessObjectAndExpectedOutputFileName[] GetMiscSampleBusinessObjects()
		{
			return null;
		}

		protected override ValueObjectDataAdapter<Journal, Balance> GetNewBizObjXmlDataAdapter()
		{
			return new BalanceValueObjectDataAdapter();
		}

		protected override Journal NewBusinessObject()
		{
			return Factory.NewWithValidTestData<APJournal>();
		}

		protected override Type GetDataAdapterType()
		{
			return typeof(BalanceValueObjectDataAdapter);
		}

		protected override bool IsExportToValueObjectSupported
		{
			get { return false; }
		}

		protected override bool IsImportFromValueObjectSupported
		{
			get { return true; }
		}

		#endregion
	}
}
