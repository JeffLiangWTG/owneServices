using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.DataInterface.ChinaDataInterface.VoucherFile
{
	[TestedType(typeof(CBTransferVoucherProvider))]
	public class CBTransferVoucherProviderTest : TransactionWithoutLinesVoucherProviderTest
	{
		public override void TestCompanyName()
		{
			var newFactory = new BusinessObjectFactory();
			var creator = new TestObjectCreator(newFactory);

			var companyOrgProxy = creator.CreateOrgHeader("COMPROXY", true, true);
			var companyOrgProxyOFCAddressCHS = creator.CreateAddress(companyOrgProxy, OrgAddressType.Office, true);
			companyOrgProxyOFCAddressCHS.OA_Language = Core.SharedConstants.Languages.ChineseSimplified;
			companyOrgProxyOFCAddressCHS.OA_CompanyNameOverride = "Company > Org Proxy > Main OFC Address’s Company Name in CHS";

			var branchOrgProxy = creator.CreateOrgHeader("BRNPROXY", true, true);
			var branchOrgProxyOFCAddressCHS = creator.CreateAddress(branchOrgProxy, OrgAddressType.Office, true);
			branchOrgProxyOFCAddressCHS.OA_Language = Core.SharedConstants.Languages.ChineseSimplified;
			branchOrgProxyOFCAddressCHS.OA_CompanyNameOverride = "Branch > Org Proxy > Main OFC Address’s Company Name in CHS";

			var company = creator.CreateNewCompany("DCN", Core.Constants.CountryCodes.China, orgProxy: companyOrgProxy);
			var branch = creator.CreateBranch("SHA", "Shanghai", company, branchOrgProxy);
			var loginBranch = creator.CreateBranch("BEI", "Beijing", company);

			newFactory.Save();

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
				newFactory.Save();
				AssertEquals(companyOrgProxy.OH_FullName, provider.CompanyName);

				var companyOrgProxyARMAddressENG = TestObjectCreator.CreateAddress(companyOrgProxy, OrgAddressType.Receivables, true);
				companyOrgProxyARMAddressENG.OA_Language = Core.SharedConstants.Languages.English;
				companyOrgProxyARMAddressENG.OA_CompanyNameOverride = "Company > Org Proxy > Main ARM Address’s Company Name in ENG";
				var translatedAddressCHSForCompanyOrgProxyARMAddressENG = TestObjectCreator.CreateTranslatedAddress(companyOrgProxyARMAddressENG, Core.SharedConstants.Languages.ChineseSimplified, "Company > Org Proxy > Main ARM Address’s Company Name in ENG -> Translated Address in CHS", "OTA_Address1");
				newFactory.Save();
				AssertEquals(translatedAddressCHSForCompanyOrgProxyARMAddressENG.CompanyName, provider.CompanyName);

				var companyOrgProxyARMAddressCHS = TestObjectCreator.CreateAddress(companyOrgProxy, OrgAddressType.Receivables, true);
				companyOrgProxyARMAddressCHS.OA_Language = Core.SharedConstants.Languages.ChineseSimplified;
				companyOrgProxyARMAddressCHS.OA_CompanyNameOverride = "Company > Org Proxy > Main ARM Address’s Company Name in CHS";
				newFactory.Save();
				AssertEquals(companyOrgProxyARMAddressCHS.CompanyName, provider.CompanyName);

				var branchOrgProxyARMAddressENG = TestObjectCreator.CreateAddress(branchOrgProxy, OrgAddressType.Receivables, true);
				branchOrgProxyARMAddressENG.OA_Language = Core.SharedConstants.Languages.English;
				branchOrgProxyARMAddressENG.OA_CompanyNameOverride = "Branch > Org Proxy > Main ARM Address’s Company Name in ENG";
				var translatedAddressCHSForBranchOrgProxyARMAddressENG = TestObjectCreator.CreateTranslatedAddress(branchOrgProxyARMAddressENG, Core.SharedConstants.Languages.ChineseSimplified, "Branch > Org Proxy > Main ARM Address’s Company Name in ENG -> Translated Address in CHS", "OTA_Address1");
				newFactory.Save();
				AssertEquals(translatedAddressCHSForBranchOrgProxyARMAddressENG.CompanyName, provider.CompanyName);

				var branchOrgProxyARMAddressCHS = TestObjectCreator.CreateAddress(branchOrgProxy, OrgAddressType.Receivables, true);
				branchOrgProxyARMAddressCHS.OA_Language = Core.SharedConstants.Languages.ChineseSimplified;
				branchOrgProxyARMAddressCHS.OA_CompanyNameOverride = "Branch > Org Proxy > Main ARM Address’s Company Name in CHS";
				newFactory.Save();
				AssertEquals(branchOrgProxyARMAddressCHS.CompanyName, provider.CompanyName);
			}
		}

		public override void TestBranchCode()
		{
			var newFactory = new BusinessObjectFactory();
			var creator = new TestObjectCreator(newFactory);

			var company = creator.CreateNewCompany("DCN", Core.Constants.CountryCodes.China);
			var branch = creator.CreateBranch("SHA", "Shanghai", company);
			var loginBranch = creator.CreateBranch("BEI", "Beijing", company);

			newFactory.Save();

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

		public override void TestDebitInFirstRow()
		{
			TestTransaction.AH_RX_NKTransactionCurrency = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;
			TestTransaction.AH_InvoiceAmount = 120m;
			AssertEquals(120m, TestProvider.VoucherLines[0].DebitAmount);
			AssertEquals(0m, TestProvider.VoucherLines[0].CreditAmount);
			TestTransaction.AH_InvoiceAmount = -120m;
			AssertEquals(120m, TestProvider.VoucherLines[0].DebitAmount);
			AssertEquals(0m, TestProvider.VoucherLines[0].CreditAmount);
		}

		public void TestLineNoGenerated()
		{
			AssertEquals(2, TestProvider.VoucherLines.Length);
		}

		public void TestAccountNumber()
		{
			TestTransaction.AH_AB = Bank1.PK;
			AssertEquals(LocalAccountNumber1, TestProvider.VoucherLines[0].AccountNumber);
		}

		public void TestVoucherType()
		{
			//string ExpectedVoucherType = "应收帐款-收款单";
			string expectedVoucherType = "CB-TRF";
			AssertEquals(expectedVoucherType, TestProvider.VoucherLines[0].VoucherType);
		}

		public void TestVoucherNumber()
		{
			TestTransaction.AH_TransactionNum = "TESTTRAN001";
			AssertEquals(TestTransaction.AH_TransactionNum, TestProvider.VoucherLines[0].VoucherNumber);
		}

		public void TestVoucherAmount()
		{
			TestTransaction.AH_RX_NKTransactionCurrency = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;
			TestTransaction.AH_InvoiceAmount = 120m;
			TestTransaction.AH_ExchangeRate = 1m;
			TestTransaction.AH_OSTotal = 120m;
			AssertEquals(120m, TestProvider.VoucherLines[0].DebitAmount);
			AssertEquals(0m, TestProvider.VoucherLines[0].CreditAmount);
			AssertEquals(1m, TestProvider.VoucherLines[0].ExchangeRate);
			AssertEquals(120m, TestProvider.VoucherLines[0].ForeignCurrencyAmount);
			TestTransaction.AH_RX_NKTransactionCurrency = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;
			TestTransaction.AH_InvoiceAmount = -120m;
			TestTransaction.AH_ExchangeRate = 1m;
			TestTransaction.AH_OSTotal = -120m;
			TestProvider = GetVoucherProvider(TestTransaction) as TransactionWithoutLinesVoucherProvider;
			AssertEquals(0m, TestProvider.VoucherLines[1].DebitAmount);
			AssertEquals(120m, TestProvider.VoucherLines[1].CreditAmount);
			AssertEquals(1m, TestProvider.VoucherLines[1].ExchangeRate);
			AssertEquals(120m, TestProvider.VoucherLines[1].ForeignCurrencyAmount);
			AssertEquals(GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency, TestProvider.VoucherLines[1].CurrencyCode);
		}

		protected override VoucherProvider GetVoucherProvider(AccTransactionHeader transaction)
		{
			return new CBTransferVoucherProvider(transaction);
		}

		protected override AccTransactionHeader GetTestTransaction()
		{
			AccTransactionHeader transaction = Factory.NewWithValidTestData(typeof(AccTransactionHeader)) as AccTransactionHeader;
			transaction.AH_TransactionType = TransactionTypes.Transfer;
			transaction.AH_Ledger = LedgerTypes.CashBook;
			transaction.AH_TransactionCount = 1;
			transaction.AH_GC = GlbCompany.CurrentCompany.PK;
			transaction.AH_TransactionBelongsToGroup = ZGuid.NewZGuid();
			AccTransactionHeader relatedTransaction = Factory.NewWithValidTestData(typeof(AccTransactionHeader)) as AccTransactionHeader;
			relatedTransaction.AH_TransactionType = transaction.AH_TransactionType;
			relatedTransaction.AH_Ledger = transaction.AH_Ledger;
			relatedTransaction.AH_TransactionNum = transaction.AH_TransactionNum;
			relatedTransaction.AH_TransactionCount = 2;
			relatedTransaction.AH_GC = transaction.AH_GC;
			relatedTransaction.AH_ExchangeRate = transaction.AH_ExchangeRate;
			relatedTransaction.AH_TransactionBelongsToGroup = transaction.AH_TransactionBelongsToGroup;
			return transaction;
		}

		protected override void UpdateRelatedTransactionFields(AccTransactionHeader transaction)
		{
			ZQuery transferRowFilter = new ZQuery(AccTransactionHeaderSchema.AH_Ledger, transaction.AH_Ledger);
			transferRowFilter.AddToFilter(AccTransactionHeaderSchema.AH_TransactionType, ZArchitecture.Core.TransactionTypes.Transfer);
			transferRowFilter.AddToFilter(AccTransactionHeaderSchema.AH_TransactionNum, transaction.AH_TransactionNum);
			transferRowFilter.AddToFilter(AccTransactionHeaderSchema.PK, SQLComparisonOperator.NotEqual, transaction.PK);
			TransactionHeaderCollection relatedTransactions = new TransactionHeaderCollection(Factory, transferRowFilter);
			relatedTransactions.Load();
			if (relatedTransactions.Count > 0)
			{
				relatedTransactions[0].AH_RX_NKTransactionCurrency = transaction.AH_RX_NKTransactionCurrency;
				relatedTransactions[0].AH_ExchangeRate = transaction.AH_ExchangeRate;
				relatedTransactions[0].AH_OSTotal = transaction.AH_OSTotal;
			}
		}

		protected override ZInt GetExpectedAttachementNo()
		{
			return 1;
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return new CBTransferVoucherProvider(Factory.NewWithValidTestData(typeof(AccTransactionHeader)) as AccTransactionHeader);
		}

		protected override void SetUp()
		{
			base.SetUp();
			TestProvider = GetVoucherProvider(TestTransaction) as TransactionWithoutLinesVoucherProvider;
		}

		protected override void AssertValuesForControlLine(VoucherProvider standardProvider, VoucherProvider providerWithOptionalFields)
		{
			Assert(true);
		}

		protected override void AssertValuesForFirstLine(VoucherProvider standardProvider, VoucherProvider providerWithOptionalFields)
		{
			Assert(true);
		}

		protected override bool OrganisationCodeIsApplicableToThisVoucher
		{
			get
			{
				return false;
			}
		}
	}
}
