using System;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.ARAP.ReceiptPayment;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Core;
using Enterprise.DocumentEngineCore.Registry;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Business.DataInterface.ChinaDataInterface.VoucherFile
{
	public abstract class VoucherProviderTestCase : NonPersistentBusinessObjectTestCase
	{
		public virtual void TestCompanyName()
		{
			var companyOrgProxy = TestObjectCreator.CreateOrgHeader("COMPROXY", true, true);
			var companyOrgProxyOFCAddressCHS = TestObjectCreator.CreateAddress(companyOrgProxy, OrgAddressType.Office, true);
			companyOrgProxyOFCAddressCHS.OA_Language = Core.SharedConstants.Languages.ChineseSimplified;
			companyOrgProxyOFCAddressCHS.OA_CompanyNameOverride = "Company > Org Proxy > Main OFC Address’s Company Name in CHS";

			var branchOrgProxy = TestObjectCreator.CreateOrgHeader("BRNPROXY", true, true);
			var branchOrgProxyOFCAddressCHS = TestObjectCreator.CreateAddress(branchOrgProxy, OrgAddressType.Office, true);
			branchOrgProxyOFCAddressCHS.OA_Language = Core.SharedConstants.Languages.ChineseSimplified;
			branchOrgProxyOFCAddressCHS.OA_CompanyNameOverride = "Branch > Org Proxy > Main OFC Address’s Company Name in CHS";

			var company = TestObjectCreator.CreateNewCompany("DCN", Core.Constants.CountryCodes.China, orgProxy: companyOrgProxy);
			var branch = TestObjectCreator.CreateBranch("SHA", "Shanghai", company, branchOrgProxy);
			var loginBranch = TestObjectCreator.CreateBranch("BEI", "Beijing", company);

			Factory.Save();

			AccTransactionHeader transaction;
			using (Env.SetTemporaryUserContext(Env.CurrentUser.LoginName, branch.PK.ToGuid(), Env.CurrentDepartment.PK))
			{
				transaction = GetTestTransaction();
				Factory.Save();
			}

			using (Env.SetTemporaryUserContext(Env.CurrentUser.LoginName, loginBranch.PK.ToGuid(), Env.CurrentDepartment.PK))
			{
				var provider = GetVoucherProvider(transaction);

				companyOrgProxy.OH_FullName = "Company > Org Proxy > Details > Details > Full Name";
				Factory.Save();
				AssertEquals(companyOrgProxy.OH_FullName, provider.CompanyName);

				var companyOrgProxyARMAddressENG = TestObjectCreator.CreateAddress(companyOrgProxy, OrgAddressType.Receivables, true);
				companyOrgProxyARMAddressENG.OA_Language = Core.SharedConstants.Languages.English;
				companyOrgProxyARMAddressENG.OA_CompanyNameOverride = "Company > Org Proxy > Main ARM Address’s Company Name in ENG";
				var translatedAddressCHSForCompanyOrgProxyARMAddressENG = TestObjectCreator.CreateTranslatedAddress(companyOrgProxyARMAddressENG, Core.SharedConstants.Languages.ChineseSimplified, "Company > Org Proxy > Main ARM Address’s Company Name in ENG -> Translated Address in CHS", "OTA_Address1");
				Factory.Save();
				AssertEquals(translatedAddressCHSForCompanyOrgProxyARMAddressENG.CompanyName, provider.CompanyName);

				var companyOrgProxyARMAddressCHS = TestObjectCreator.CreateAddress(companyOrgProxy, OrgAddressType.Receivables, true);
				companyOrgProxyARMAddressCHS.OA_Language = Core.SharedConstants.Languages.ChineseSimplified;
				companyOrgProxyARMAddressCHS.OA_CompanyNameOverride = "Company > Org Proxy > Main ARM Address’s Company Name in CHS";
				Factory.Save();
				AssertEquals(companyOrgProxyARMAddressCHS.CompanyName, provider.CompanyName);

				var branchOrgProxyARMAddressENG = TestObjectCreator.CreateAddress(branchOrgProxy, OrgAddressType.Receivables, true);
				branchOrgProxyARMAddressENG.OA_Language = Core.SharedConstants.Languages.English;
				branchOrgProxyARMAddressENG.OA_CompanyNameOverride = "Branch > Org Proxy > Main ARM Address’s Company Name in ENG";
				var translatedAddressCHSForBranchOrgProxyARMAddressENG = TestObjectCreator.CreateTranslatedAddress(branchOrgProxyARMAddressENG, Core.SharedConstants.Languages.ChineseSimplified, "Branch > Org Proxy > Main ARM Address’s Company Name in ENG -> Translated Address in CHS", "OTA_Address1");
				Factory.Save();
				AssertEquals(translatedAddressCHSForBranchOrgProxyARMAddressENG.CompanyName, provider.CompanyName);

				var branchOrgProxyARMAddressCHS = TestObjectCreator.CreateAddress(branchOrgProxy, OrgAddressType.Receivables, true);
				branchOrgProxyARMAddressCHS.OA_Language = Core.SharedConstants.Languages.ChineseSimplified;
				branchOrgProxyARMAddressCHS.OA_CompanyNameOverride = "Branch > Org Proxy > Main ARM Address’s Company Name in CHS";
				Factory.Save();
				AssertEquals(branchOrgProxyARMAddressCHS.CompanyName, provider.CompanyName);
			}
		}

		public virtual void TestBranchCode()
		{
			var company = TestObjectCreator.CreateNewCompany("DCN", Core.Constants.CountryCodes.China);
			var branch = TestObjectCreator.CreateBranch("SHA", "Shanghai", company);
			var loginBranch = TestObjectCreator.CreateBranch("BEI", "Beijing", company);

			Factory.Save();

			AccTransactionHeader transaction;
			using (Env.SetTemporaryUserContext(Env.CurrentUser.LoginName, branch.PK.ToGuid(), Env.CurrentDepartment.PK))
			{
				transaction = GetTestTransaction();
				Factory.Save();
			}

			using (Env.SetTemporaryUserContext(Env.CurrentUser.LoginName, loginBranch.PK.ToGuid(), Env.CurrentDepartment.PK))
			{
				var provider = GetVoucherProvider(transaction);
				AssertEquals(branch.GB_Code, provider.BranchCode);
			}
		}

		public void TestTotalDebitAndCreditAmount()
		{
			AccTransactionHeader transaction = TestObjectCreator.CreateInvoiceWithLine(typeof(APInvoice), "Test_INV_01", TestObjectCreator.AUD, 1, 25, 0, 25, 0);
			var providerFactory = new VoucherProviderFactory(Factory);
			var provider = providerFactory.GetProvider(transaction);
			var line1 = new VoucherLine(transaction);
			var line2 = new VoucherLine(transaction);

			var line3 = new VoucherLine(transaction);
			var line4 = new VoucherLine(transaction);

			line1.DebitAmount = 10m;
			line2.DebitAmount = 15m;

			line3.CreditAmount = 12m;
			line4.CreditAmount = 13m;

			AssertEquals(25m, provider.TotalDebitAmount);
			AssertEquals(25m, provider.TotalCreditAmount);
		}

		public void TestAttachmentCount()
		{
			AccTransactionHeader transaction = GetTestTransaction();
			VoucherProvider provider = GetVoucherProvider(transaction);
			if (typeof(TransactionLineOnlyVoucherProvider).IsAssignableFrom(provider.GetType()))
			{
				Assert(true);
			}
			else
			{
				AssertEquals(GetExpectedAttachementNo(), provider.VoucherLines[0].AttachmentCount);
			}
		}

		public void TestPostedBy()
		{
			var testTransaction = TestTransaction ?? InsertAPPayment(Factory);
			var provider = GetVoucherProvider(testTransaction);

			AssertEquals("PostedBy", GlbStaff.CurrentUser.GS_FullName, provider.PostedBy);
			Factory.Save();
			AssertEquals("PostedBy", GlbStaff.CurrentUser.GS_FullName, provider.PostedBy);

			var staff = Factory.LoadTop1<GlbStaff>(new ZQuery(GlbStaffSchema.GS_Code, "ZZ"));
			AssertNotEquals(GlbStaff.CurrentUser.GS_FullName, staff.GS_FullName);
			testTransaction.Logs.AddedLog.SL_GS_NKUser = staff.GS_Code;
			Factory.Save();

			if (provider is TransactionLineOnlyVoucherProvider)
			{
				AssertEquals("VoucherProvider of WIP and ACR, the PostedBy should be current user", GlbStaff.CurrentUser.GS_FullName, provider.PostedBy);
			}
			else
			{
				AssertEquals("PostedBy", staff.GS_FullName, provider.PostedBy);
			}
		}

		public void TestNumberOfSupportingDocuments()
		{
			Factory.Save();
			VoucherProvider provider = GetVoucherProvider(TestTransaction);
			if (TestTransaction == null)
			{
				Assert(provider.NumberOfSupportingDocuments.IsEmpty);
			}
			else
			{
				AssertEquals(true, provider.NumberOfSupportingDocuments.ToZInt() >= 0);
			}
		}

		public void TestEnterBy()
		{
			var staff = Factory.LoadTop1<GlbStaff>(new ZQuery(GlbStaffSchema.GS_Code, "E"));
			var testTransaction = TestTransaction ?? InsertAPPayment(Factory);
			Factory.Save();
			var provider = GetVoucherProvider(testTransaction);
			AssertEquals("EnterBy", provider.EnterBy, staff.GS_FullName);
		}

		public void TestReviewer()
		{
			var staff = Factory.LoadTop1<GlbStaff>(new ZQuery(GlbStaffSchema.GS_Code, "E"));
			AccountingConfigurationRegistry.Instance.VoucherAppointedPartiesReviewerDefault.SetValue(Guid.Empty, BranchPK, Guid.Empty, staff.PK.ToGuid());
			var testTransaction = TestTransaction ?? InsertAPPayment(Factory);

			var newStaff = TestObjectCreator.CreateStaff("JNS");
			newStaff.GS_FullName = "John Snow";

			var provider = GetVoucherProvider(testTransaction);

			testTransaction.AH_GS_NKAuditedBy = string.Empty;

			var defaultReviewer = AccountingConfigurationRegistry.Instance.GetVoucherAppointedPartiesDefault(CompanyPK, BranchPK, Guid.Empty, AccountingConstants.VoucherAppointedPartiesCode.VoucherReviewer);
			AssertEquals("Reviewer", provider.Reviewer, provider is TransactionLineOnlyVoucherProvider ? staff.GS_FullName : defaultReviewer);

			testTransaction.AH_GS_NKAuditedBy = newStaff.GS_Code;
			Factory.Save();

			AssertEquals("Reviewer", provider.Reviewer, provider is TransactionLineOnlyVoucherProvider ? staff.GS_FullName : newStaff.GS_FullName);
		}

		public void TestCashier()
		{
			var staff = Factory.LoadTop1<GlbStaff>(new ZQuery(GlbStaffSchema.GS_Code, "E"));
			AccountingConfigurationRegistry.Instance.VoucherAppointedPartiesCashierDefault.SetValue(Guid.Empty, BranchPK, Guid.Empty, staff.PK.ToGuid());
			var testTransaction = TestTransaction ?? InsertAPPayment(Factory);

			var newStaff = TestObjectCreator.CreateStaff("JNS");
			newStaff.GS_FullName = "John Snow";

			var provider = GetVoucherProvider(testTransaction);

			testTransaction.AH_GS_NKCashier = string.Empty;

			var defaultCashier = AccountingConfigurationRegistry.Instance.GetVoucherAppointedPartiesDefault(CompanyPK, BranchPK, Guid.Empty, AccountingConstants.VoucherAppointedPartiesCode.VoucherCasher);
			AssertCashier(provider, testTransaction.AH_Ledger, staff.GS_FullName, defaultCashier);

			testTransaction.AH_GS_NKCashier = newStaff.GS_Code;
			Factory.Save();

			AssertCashier(provider, testTransaction.AH_Ledger, staff.GS_FullName, newStaff.GS_FullName);
		}

		void AssertCashier(VoucherProvider provider, ZString ledger, ZString staffName, ZString cashier)
		{
			switch (TestTransaction?.AH_TransactionType)
			{
				case TransactionTypes.Payment:
				case TransactionTypes.Receipt:
				case TransactionTypes.OpeningPayment:
				case TransactionTypes.OpeningReceipt:
				case TransactionTypes.DirectPayment:
				case TransactionTypes.DirectReceipt:
				case TransactionTypes.Transfer when ledger == LedgerTypes.CashBook:
				case TransactionTypes.ExchangeDifference:
					AssertEquals("Cashier", provider.Cashier, provider is TransactionLineOnlyVoucherProvider ? staffName : cashier);
					break;
				default:
					AssertEquals("Cashier", provider.Cashier, ZString.Empty);
					break;
			}
		}

		public void TestPostPeriod()
		{
			AccountingPeriodTestHelper testHelper = new AccountingPeriodTestHelper(Factory);
			testHelper.SetupSinglePeriod(200401, new ZDateTime(2004, 1, 1), new ZDateTime(2004, 1, 31));
			if (TestTransaction != null)
			{
				TestTransaction.AH_PostDate = new ZDateTime(2004, 1, 15);
			}

			VoucherProvider provider = GetVoucherProvider(TestTransaction);
			Assert(!provider.Period.IsEmpty);
		}

		public void TestForiegnCurrencyTransactions()
		{
			CoreTestForVoucherCurrency(TestObjectCreator.USD);
		}

		public void TestLocalCurrencyTransactions()
		{
			CoreTestForVoucherCurrency(GlbCompany.CurrentCompany.LocalCurrency);
		}

		public void TestOrganisationName()
		{
			var providerFactory = new VoucherProviderFactory(Factory);
			VoucherProvider provider = null;
			AssertExceptionThrown<ArgumentNullException>(() => providerFactory.GetProvider(null));
			var transaction = GetTestTransaction();
			if (OrgNameCondition(transaction))
			{
				provider = providerFactory.GetProvider(transaction);
				AssertNotEquals(ZString.Empty, provider.OrganisationName);
			}

			transaction = GetTestTransaction();
			if (OrgNameCondition(transaction))
			{
				transaction.AH_OH = ZGuid.Empty;
				provider = providerFactory.GetProvider(transaction);
				AssertEquals(ZString.Empty, provider.OrganisationName);
			}

			transaction = GetTestTransaction();
			if (OrgNameCondition(transaction))
			{
				transaction.Header.OH_FullName = null;
				provider = providerFactory.GetProvider(transaction);
				AssertEquals(ZString.Empty, provider.OrganisationName);
			}
		}

		public void TestChineseOrgnizationName()
		{
			var transaction = TestObjectCreator.CreateInvoiceWithLine(typeof(APInvoice), "Test_INV_01", TestObjectCreator.AUD, 1, 300, 30, 300, 30);
			transaction.AH_OH = TestObjectCreator.AALSHI.PK;
			var providerFactory = new VoucherProviderFactory(Factory);
			var provider = providerFactory.GetProvider(transaction);
			var org = TestObjectCreator.AALSHI;
			TestObjectCreator.AALSHI.OH_FullName = "TEST";
			AssertEquals("TEST", provider.OrganisationName);
			var add1 = org.Addresses.AddNew(OrgAddressType.Payables, false);
			add1.CompanyName = "测试公司名 non-main non-CHS non-ARM";
			add1.Language = Core.SharedConstants.Languages.English;
			AssertEquals("TEST", provider.OrganisationName);
			var add2 = org.Addresses.AddNew(OrgAddressType.Payables, true);
			add2.CompanyName = "测试公司名 main non-CHS non-ARM";
			add2.Language = Core.SharedConstants.Languages.English;
			AssertEquals("TEST", provider.OrganisationName);
			var add3 = org.Addresses.AddNew(OrgAddressType.Receivables, true);
			add3.CompanyName = "测试公司名 main non-CHS ARM";
			add3.Language = Core.SharedConstants.Languages.English;
			AssertEquals("TEST", provider.OrganisationName);
			var add4 = org.Addresses.AddNew(OrgAddressType.Receivables, true);
			add4.CompanyName = "测试公司名 main CHS ARM";
			add4.Language = Core.SharedConstants.Languages.ChineseSimplified;
			AssertEquals("测试公司名 main CHS ARM", provider.OrganisationName);
		}

		public void TestBusinessContext()
		{
			DocumentsDataRegistry.Instance.UseNewDocBuilderAccountingVoucher.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			var transaction = TestObjectCreator.CreateInvoiceWithLine(typeof(APInvoice), "Test_INV_01", TestObjectCreator.AUD, 1, 25, 0, 25, 0);

			var provider = GetVoucherProvider(transaction);
			AssertEquals(CargoWise.Definitions.BusinessContext.AccountingVoucher, provider.DocumentSupporter.BusinessContext);

			DocumentsDataRegistry.Instance.UseNewDocBuilderAccountingVoucher.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			provider = GetVoucherProvider(transaction);
			AssertEquals(CargoWise.Definitions.BusinessContext.ARTransaction, provider.DocumentSupporter.BusinessContext);
		}

		protected virtual bool OrgNameCondition(AccTransactionHeader transaction)
		{
			return transaction != null && transaction.Header != null;
		}

		protected virtual void UpdateRelatedTransactionFields(AccTransactionHeader transaction)
		{
			return;
		}

		protected virtual void CoreTestForVoucherCurrency(RefCurrency currency)
		{
			ZDecimal exchangeRate = 1.234M;
			ZDecimal oSAmount = 1000M;
			AccTransactionHeader transaction = GetTestTransaction();
			if (transaction != null)
			{
				transaction.AH_RX_NKTransactionCurrency = currency.RX_Code;
				transaction.AH_ExchangeRate = exchangeRate;
				transaction.AH_OSTotal = oSAmount;
				UpdateRelatedTransactionFields(transaction);
				ZQuery findLinesForHeader = new ZQuery(AccTransactionLinesSchema.AL_AH, transaction.PK);
				findLinesForHeader.AddToFilter(AccTransactionLinesSchema.AL_GC, transaction.AH_GC);
				AccTransactionLinesCollection lines = new AccTransactionLinesCollection(Factory, findLinesForHeader);
				lines.Load();
				foreach (AccTransactionLines line in lines)
				{
					if (line != null)
					{
						line.AL_RX_NKTransactionCurrency = currency.RX_Code;
						line.AL_ExchangeRate = exchangeRate;
						line.AL_OSAmount = oSAmount;
					}
				}
			}

			VoucherProvider provider = GetVoucherProvider(transaction);
			foreach (VoucherLine line in provider.VoucherLines)
			{
				if (line != null)
				{
					if (GlbCompany.CurrentCompany.LocalCurrency.RX_Code == currency.RX_Code)
					{
						AssertEquals("Voucher Exchange Rate", 1m, line.ExchangeRate);
						AssertEquals("Voucher Line Currency", currency.RX_Code, line.CurrencyCode);
						AssertEquals("Voucher Foreign Amount", oSAmount, line.ForeignCurrencyAmount);
					}
					else
					{
						AssertEquals("Voucher Exchange Rate", exchangeRate, line.ExchangeRate);
						AssertEquals("Voucher Line Currency", currency.RX_Code, line.CurrencyCode);
						AssertEquals("Voucher Foreign Amount", oSAmount, line.ForeignCurrencyAmount);
					}
				}
			}
		}

		public void TestNewFieldsAreSetCorrectly()
		{
			PrepareDataForTestForNewFieldsTest();
			using (Job = TestObjectCreator.CreateJob(TestObjectCreator.CreateShipment("S00001234")))
			{
				AccTransactionHeader transaction = GetTestTransaction();
				VoucherProvider standardProvider = GetVoucherProvider(transaction);
				VoucherProvider providerWithOptionalFields = GetVoucherProvider(transaction);
				providerWithOptionalFields.IncludeOrganisationCode = true;
				providerWithOptionalFields.IncludeJobNumber = true;
				SetJobNumber(transaction);
				SetLineDescription(transaction);
				SetOrganisationCode(transaction);
				if (!typeof(TransactionLineOnlyVoucherProvider).IsAssignableFrom(standardProvider.GetType()))
				{
					AssertNotNull("Precondition: Transaction should not be null", TestTransaction);
				}

				AssertNotNull("Precondition: Voucher Provider should not be null", standardProvider);
				AssertNotNull("Precondition: Voucher Provider should not be null", providerWithOptionalFields);
				AssertValuesForFirstLine(standardProvider, providerWithOptionalFields);
				AssertValuesForControlLine(standardProvider, providerWithOptionalFields);
			}
		}

		public virtual void TestSourceIdentifierProvider()
		{
			var transaction = TestObjectCreator.CreateInvoiceWithLine(typeof(APInvoice), "Test_INV_01", TestObjectCreator.AUD, 1, 25, 0, 25, 0);
			var provider = GetVoucherProvider(transaction) as DocumentEngineIntegration.ISourceIdentifierProvider;

			AssertNotNull(provider);
			AssertEquals(transaction.PK, provider.SourceIdentifier);
		}

		protected virtual void AssertValuesForFirstLine(VoucherProvider standardProvider, VoucherProvider providerWithOptionalFields)
		{
			Assert("Voucher Line count should be at least 2", standardProvider.VoucherLines.Length >= 2);
			VoucherLine standardFirstLine = standardProvider.VoucherLines[0];
			AssertNotNull("First Voucher Line should not be null", standardFirstLine);
			VoucherLine firstLineWithOptionalFields = providerWithOptionalFields.VoucherLines[0];
			AssertNotNull("First Voucher Line should not be null", firstLineWithOptionalFields);
			ZString expectedJobNumber = GetExpectedJobNumberForFirstLine();
			ZString expectedLineDescription = GetExpectedLineDescriptionForFirstLine();
			ZString expectedOrganisationCode = GetExpectedOrganisationCodeForFirstLine();
			if (JobNumberIsApplicableToThisVoucher)
			{
				AssertNotContains("Additional Description should NOT contain the Job Number", expectedJobNumber, standardFirstLine.Description);
				AssertContains("Additional Description should contain the Job Number", expectedJobNumber, firstLineWithOptionalFields.Description);
			}
			else
			{
				AssertNotContains("Additional Description should NOT contain the Job Number", expectedJobNumber, standardFirstLine.Description);
				AssertNotContains("Additional Description should NOT contain the Job Number", expectedJobNumber, firstLineWithOptionalFields.Description);
			}

			if (GlbCompany.CurrentCompany.GC_RN_NKCountryCode == Core.Constants.CountryCodes.China || GlbCompany.CurrentCompany.GC_RN_NKCountryCode == Core.Constants.CountryCodes.Taiwan)
			{
				AssertContains("Additional Description should contain the Line Description", expectedLineDescription, standardFirstLine.Description);
			}
			else
			{
				AssertNotContains("Additional Description should NOT contain the Line Description", expectedLineDescription, standardFirstLine.Description);
			}
			AssertContains("Additional Description should contain the Line Description", expectedLineDescription, firstLineWithOptionalFields.Description);
			if (OrganisationCodeForFirstLinesIsApplicableToThisVoucher)
			{
				AssertNotContains("Additional Description should NOT contain the Organisation Code", expectedOrganisationCode, standardFirstLine.AdditionalAccountDescription);
				AssertContains("Additional Description should contain the Organisation Code", expectedOrganisationCode, firstLineWithOptionalFields.AdditionalAccountDescription);
			}
			else
			{
				AssertNotContains("Additional Description should NOT contain the Organisation Code", expectedOrganisationCode, standardFirstLine.AdditionalAccountDescription);
				if (!firstLineWithOptionalFields.VoucherType.Contains("TRF"))
				{
					AssertNotContains("Additional Description should NOT contain the Organisation Code", expectedOrganisationCode, firstLineWithOptionalFields.AdditionalAccountDescription);
				}
			}
		}

		protected virtual void AssertValuesForControlLine(VoucherProvider standardProvider, VoucherProvider providerWithOptionalFields)
		{
			int lastLineIndex = standardProvider.VoucherLines.Length - 1;
			VoucherLine standardLastLine = standardProvider.VoucherLines[lastLineIndex];
			VoucherLine lastLineWithOptionalFields = providerWithOptionalFields.VoucherLines[lastLineIndex];
			Assert("Voucher Line count should be at least 2", standardProvider.VoucherLines.Length >= 2);
			Assert("Voucher Line count should be at least 2", providerWithOptionalFields.VoucherLines.Length >= 2);
			AssertNotNull("Last Voucher Line should not be null", standardLastLine);
			AssertNotNull("Last Voucher Line should not be null", lastLineWithOptionalFields);
			ZString expectedJobNumber = GetExpectedJobNumberForControlLine();
			ZString expectedLineDescription = GetExpectedLineDescriptionForControlLine();
			ZString expectedOrganisationCode = GetExpectedOrganisationCodeForControlLine();
			if (JobNumberIsApplicableToThisVoucher)
			{
				AssertContains("Additional Description should contain the Job Number", expectedJobNumber, lastLineWithOptionalFields.Description);
				AssertNotContains("Additional Description should NOT contain the Job Number", expectedJobNumber, standardLastLine.Description);
			}
			else
			{
				AssertNotContains("Additional Description should NOT contain the Job Number", expectedJobNumber, standardLastLine.Description);
				AssertNotContains("Additional Description should NOT contain the Job Number", expectedJobNumber, lastLineWithOptionalFields.Description);
			}

			if (ControlLineDescriptionIsApplicableForThisVoucher)
			{
				AssertContains("Additional Description should contain the Line Description", expectedLineDescription, lastLineWithOptionalFields.Description);
				if (GlbCompany.CurrentCompany.GC_RN_NKCountryCode == Core.Constants.CountryCodes.China || GlbCompany.CurrentCompany.GC_RN_NKCountryCode == Core.Constants.CountryCodes.Taiwan)
				{
					AssertContains("Additional Description should contain the Line Description", expectedLineDescription, standardLastLine.Description);
				}
				else
				{
					AssertNotContains("Additional Description should NOT contain the Line Description", expectedLineDescription, standardLastLine.Description);
				}
			}

			if (OrganisationCodeIsApplicableToThisVoucher)
			{
				AssertContains("Additional Description should contain the Organisation Code", expectedOrganisationCode, lastLineWithOptionalFields.AdditionalAccountDescription);
				AssertNotContains("Additional Description should NOT contain the Line Organisation Code.", expectedOrganisationCode, standardLastLine.AdditionalAccountDescription);
			}

			Assert(true);
		}

		protected virtual void PrepareDataForTestForNewFieldsTest()
		{
		}

		protected virtual bool JobNumberIsApplicableToThisVoucher
		{
			get
			{
				return false;
			}
		}

		protected virtual void SetJobNumber(AccTransactionHeader header)
		{
		}

		protected virtual bool LineDescriptionIsApplicableToThisVoucher
		{
			get
			{
				return true;
			}
		}

		protected virtual bool ControlLineDescriptionIsApplicableForThisVoucher
		{
			get
			{
				return false;
			}
		}

		protected virtual void SetLineDescription(AccTransactionHeader header)
		{
			if (header != null)
			{
				header.AH_Desc = "LINE DESCRIPTION";
			}
		}

		protected virtual bool OrganisationCodeIsApplicableToThisVoucher
		{
			get
			{
				return true;
			}
		}

		protected virtual bool OrganisationCodeForFirstLinesIsApplicableToThisVoucher
		{
			get
			{
				return false;
			}
		}

		protected virtual void SetOrganisationCode(AccTransactionHeader header)
		{
			if (header != null)
			{
				header.AH_OH = TestObjectCreator.AALSHI.PK;
			}
		}

		protected virtual ZString GetExpectedJobNumberForFirstLine()
		{
			return "Job: " + Job.JH_JobNum;
		}

		protected virtual ZString GetExpectedLineDescriptionForFirstLine()
		{
			return @"- LINE DESCRIPTION";
		}

		protected virtual ZString GetExpectedOrganisationCodeForFirstLine()
		{
			return TestObjectCreator.AALSHI.OH_Code;
		}

		protected virtual ZString GetExpectedJobNumberForControlLine()
		{
			return Job.JH_JobNum;
		}

		protected virtual ZString GetExpectedLineDescriptionForControlLine()
		{
			return ZString.Empty;
		}

		protected virtual ZString GetExpectedOrganisationCodeForControlLine()
		{
			return TestObjectCreator.AALSHI.OH_Code;
		}

		protected AccBankAccount Bank1
		{
			get
			{
				if (fBank1 == null)
				{
					fBank1 = TestObjectCreator.InsertBankAccount(GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency);
					fBank1.AB_AG = GLAccount1.PK;
				}

				return fBank1;
			}
		}

		AccBankAccount fBank1;
		protected AccChargeCode ChargeCode1;
		protected AccGLHeader ControlAccount;
		protected AccGLHeader GLAccount1;
		protected AccGLHeader GLAccount2;
		protected AccGLAccountDescriptor GLAccountDescriptor1;
		protected AccGLAccountDescriptor GLAccountDescriptor2;
		protected AccGLAccountDescriptor GLAccountDescriptorForControlAccount;
		protected ZDecimal LineAmount1;
		protected ZDecimal LineAmount2;
		protected string LocalAccountNumber1;
		protected string LocalAccountNumber2;
		protected string ControlAccountNumber;
		protected string LocalAccountDescription1;
		protected string LocalAccountDescription2;
		protected string LocalAccountDescription3;
		protected VoucherProvider TestProvider;
		protected AccTransactionHeader TestTransaction;
		protected abstract VoucherProvider GetVoucherProvider(AccTransactionHeader transaction);
		protected abstract AccTransactionHeader GetTestTransaction();
		protected abstract ZInt GetExpectedAttachementNo();
		protected Job Job;
		protected Guid BranchPK
		{
			get
			{
				return TestTransaction != null && TestTransaction.Branch != null ? TestTransaction.Branch.PK.ToGuid() : Guid.Empty;
			}
		}

		protected Guid CompanyPK
		{
			get
			{
				return TestTransaction != null && TestTransaction.Company != null ? TestTransaction.Company.PK.ToGuid() : GlbCompany.CurrentCompany.PK.ToGuid();
			}
		}

		protected DataRow GetDummyDataRow(Guid gLHeader, string type, decimal amount)
		{
			DataTable testTable = new DataTable();
			testTable.Columns.Add("BranchCode", typeof(string));
			testTable.Columns.Add("DepartmentCode", typeof(string));
			testTable.Columns.Add("GLHeader", typeof(Guid));
			testTable.Columns.Add("TransactionType", typeof(string));
			testTable.Columns.Add("Amount", typeof(decimal));
			testTable.Columns.Add("OSAmount", typeof(decimal));
			testTable.Columns.Add("VoucherNumber", typeof(string));
			return testTable.Rows.Add(new object[] { GlbBranch.CurrentBranch.GB_Code, GlbDepartment.CurrentDepartment.GE_Code, gLHeader, type, amount, amount, "ABC" });
		}

		protected TransactionHeader InsertAPPayment(BusinessObjectFactory factory)
		{
			TransactionHeader result = factory.NewWithValidTestData<APPayment>();
			result.AH_GB = GlbBranch.CurrentBranch.PK;
			result.AH_GE = GlbDepartment.CurrentDepartment.PK;
			result.AH_PostDate = ZDateTime.Now;
			result.AH_InvoiceDate = ZDateTime.Now;
			result.AH_RX_NKTransactionCurrency = GlbCompany.CurrentCompany.LocalCurrency.RX_Code;
			return result;
		}

		protected TransactionHeader InsertARReceipt(BusinessObjectFactory factory)
		{
			TransactionHeader result = factory.NewWithValidTestData<ARReceipt>();
			result.AH_GB = GlbBranch.CurrentBranch.PK;
			result.AH_GE = GlbDepartment.CurrentDepartment.PK;
			result.AH_PostDate = ZDateTime.Now;
			result.AH_InvoiceDate = ZDateTime.Now;
			result.AH_RX_NKTransactionCurrency = GlbCompany.CurrentCompany.LocalCurrency.RX_Code;
			return result;
		}

		protected override void SetUp()
		{
			base.SetUp();
			OriginalCountryCode = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			GlbCompany.CurrentCompany.GC_RN_NKCountryCode = Core.Constants.CountryCodes.China;
			SetupGLAccount();
			TestTransaction = GetTestTransaction();
		}

		protected override void TearDown()
		{
			GlbCompany.CurrentCompany.GC_RN_NKCountryCode = OriginalCountryCode;
			base.TearDown();
		}

		ZString OriginalCountryCode;
		protected AccGLAccountDescriptor SetupAccountDescriptor(AccGLHeader gLAccount, string localAccountNum, string localAccountDescription, string reportCategory)
		{
			AccGLAccountDescriptor gLAccountDescriptor = Factory.New(typeof(AccGLAccountDescriptor)) as AccGLAccountDescriptor;
			gLAccountDescriptor.AJ_Language = DataInterfaceUtils.GetLocalLanguage();
			gLAccountDescriptor.AJ_RN_NKCountryOfCompliance = Constants.CountryCodes.China;
			gLAccountDescriptor.AJ_ReportType = AccGLAccountDescriptor.ReportTypeCOA;
			gLAccountDescriptor.AJ_ReportCategory = reportCategory;
			gLAccountDescriptor.AJ_LocalAccountNumber = localAccountNum;
			gLAccountDescriptor.AJ_AccountDescription = localAccountDescription;
			gLAccountDescriptor.ParentGLHeaderPK = gLAccount.PK;
			// AccGLAccountDescriptor is not valid with empty AJ_DebitCredit.
			gLAccountDescriptor.AJ_DebitCredit = gLAccount.AG_DebitCredit.IsEmpty ? (ZString)Constants.DebitCredit.Debit : gLAccount.AG_DebitCredit;
			return gLAccountDescriptor;
		}

		protected void SetupGLAccount()
		{
			LocalAccountNumber1 = "1000.10.10";
			LocalAccountNumber2 = "1000.10.20";
			ControlAccountNumber = "1000";
			GLAccount1 = TestObjectCreator.InsertGLHeader();
			GLAccount2 = TestObjectCreator.InsertGLHeader();
			ControlAccount = TestObjectCreator.InsertGLHeader();
			ChargeCode1 = TestObjectCreator.InsertChargeCode(Constants.ChargeType.Margin);
			ChargeCode1.AC_AG_RevenueAccount = GLAccount2.PK;
			GLAccountDescriptor1 = SetupAccountDescriptor(GLAccount1, LocalAccountNumber1, LocalAccountDescription1, AccountTypeComboBoxConstants.ProfitAndLossAccount);
			GLAccountDescriptor2 = SetupAccountDescriptor(GLAccount2, LocalAccountNumber2, LocalAccountDescription2, AccountTypeComboBoxConstants.ProfitAndLossAccount);
			GLAccountDescriptorForControlAccount = SetupAccountDescriptor(ControlAccount, ControlAccountNumber, LocalAccountDescription3, AccountTypeComboBoxConstants.ProfitAndLossAccount);
			Factory.Save();
		}

		OrgHeader fFromAccount;
		protected OrgHeader FromAccount
		{
			get
			{
				if (fFromAccount == null)
				{
					ZDBOnlyQuery query = new ZDBOnlyQuery(typeof(OrgHeader));
					ZDBOnlySubQuery subQuery = new ZDBOnlySubQuery(typeof(OrgCompanyData), OrgCompanyDataSchema.OB_OH);
					ZQuery fromAccountFilter = new ZQuery(OrgCompanyDataSchema.OB_IsCreditor, ZBool.True);
					fromAccountFilter.AddToFilter(OrgCompanyDataSchema.OB_IsDebtor, ZBool.True);
					fromAccountFilter.AddToFilter(OrgCompanyDataSchema.OB_GC, GlbCompany.CurrentCompany.PK);
					subQuery.AddToFilter(fromAccountFilter);
					query.AddSubQuery(subQuery, JoinCondition.And);
					query.AddToFilter(OrgHeaderSchema.PK, ToAccount.PK);
					fFromAccount = Factory.LoadTop1<OrgHeader>(query);
				}

				return fFromAccount;
			}
		}

		OrgHeader fToAccount;
		protected OrgHeader ToAccount
		{
			get
			{
				if (fToAccount == null)
				{
					ZDBOnlyQuery query = new ZDBOnlyQuery(typeof(OrgHeader));
					ZDBOnlySubQuery subQuery = new ZDBOnlySubQuery(typeof(OrgCompanyData), OrgCompanyDataSchema.OB_OH);
					ZQuery toAccountFilter = new ZQuery(OrgCompanyDataSchema.OB_IsCreditor, ZBool.True);
					toAccountFilter.AddToFilter(OrgCompanyDataSchema.OB_IsDebtor, ZBool.True);
					toAccountFilter.AddToFilter(OrgCompanyDataSchema.OB_GC, GlbCompany.CurrentCompany.PK);
					subQuery.AddToFilter(toAccountFilter);
					query.AddSubQuery(subQuery, JoinCondition.And);
					fToAccount = Factory.LoadTop1<OrgHeader>(query);
				}

				return fToAccount;
			}
		}

		protected TestObjectCreator TestObjectCreator
		{
			get
			{
				if (fTestObjectCreator == null)
				{
					fTestObjectCreator = new TestObjectCreator(Factory);
				}

				return fTestObjectCreator;
			}
		}

		protected TestObjectCreator fTestObjectCreator;
	}
}
