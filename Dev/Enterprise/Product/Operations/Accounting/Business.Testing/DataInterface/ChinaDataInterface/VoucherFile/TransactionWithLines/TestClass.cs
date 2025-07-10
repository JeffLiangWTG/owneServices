using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Core;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Moq;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.DataInterface.ChinaDataInterface.VoucherFile
{
	[TestedType(typeof(InvoiceCreditAdjustmentVoucherProvider))]
	public class TestClass : TransactionWithLinesTestCase
	{
		public override void TestCompanyName()
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
				fInvoiceToTest = null;
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

		public override void TestBranchCode()
		{
			var company = TestObjectCreator.CreateNewCompany("DCN", Core.Constants.CountryCodes.China);
			var branch = TestObjectCreator.CreateBranch("SHA", "Shanghai", company);
			var loginBranch = TestObjectCreator.CreateBranch("BEI", "Beijing", company);

			Factory.Save();

			AccTransactionHeader transaction;
			using (Env.SetTemporaryUserContext(Env.CurrentUser.LoginName, branch.PK.ToGuid(), Env.CurrentDepartment.PK))
			{
				fInvoiceToTest = null;
				transaction = GetTestTransaction();
				Factory.Save();
			}

			using (Env.SetTemporaryUserContext(Env.CurrentUser.LoginName, loginBranch.PK.ToGuid(), Env.CurrentDepartment.PK))
			{
				var provider = GetVoucherProvider(transaction);
				AssertEquals(branch.GB_Code, provider.BranchCode);
			}
		}

		public void TestLineNumbers()
		{
			AssertEquals(3, TestInvoiceCreditAdjustmentVoucher.VoucherLines.Length);
		}

		public void TestLineNumbersWithGST()
		{
			InvoiceToTest.AH_GSTAmount = 17.0m;
			AssertEquals(4, TestInvoiceCreditAdjustmentVoucher.VoucherLines.Length);
		}

		public void TestLineAccountNumberDetails()
		{
			LocalAccountNumber1 = "1000.10.10";
			LocalAccountNumber2 = "1000.10.20";
			LocalControlAccount = "1000.10.30";
			LocalGST = "1000.10.40";
			GLAccount1 = TestObjectCreator.InsertGLHeader();
			GLAccount2 = TestObjectCreator.InsertGLHeader();
			ControlAccount = TestObjectCreator.InsertGLHeader();
			GSTAccount = TestObjectCreator.InsertGLHeader();
			ChargeCode1 = TestObjectCreator.InsertChargeCode(Constants.ChargeType.Margin);
			ChargeCode1.AC_AG_RevenueAccount = GLAccount1.PK;
			ChargeCode1.AC_AG_CostAccount = GLAccount2.PK;
			GLAccountDescriptor1 = SetupAccountDescriptor(GLAccount1, LocalAccountNumber1);
			GLAccountDescriptor2 = SetupAccountDescriptor(GLAccount2, LocalAccountNumber2);
			GLAccountDescriptor3 = SetupAccountDescriptor(ControlAccount, LocalControlAccount);
			GLAccountDescriptor4 = SetupAccountDescriptor(GSTAccount, LocalGST);
			LineAmount1 = 120m;
			LineAmount2 = 70m;
			GSTAmount1 = 12.0m;
			GSTAmount2 = 7.0m;
			fInvoiceToTest = null;
			SetUpInvoice();
			TestInvoiceCreditAdjustmentVoucher = new InvoiceCreditAdjustmentVoucherProvider(InvoiceToTest);
			AssertEquals(LocalAccountNumber1, TestInvoiceCreditAdjustmentVoucher.VoucherLines[1].AccountNumber);
			AssertEquals(LocalAccountNumber2, TestInvoiceCreditAdjustmentVoucher.VoucherLines[2].AccountNumber);
		}

		public void TestLineVoucherNumberDetailsForAR()
		{
			InvoiceToTest.AH_TransactionNum = "1234";
			AssertEquals(InvoiceToTest.AH_TransactionNum, TestInvoiceCreditAdjustmentVoucher.VoucherLines[0].VoucherNumber);
			AssertEquals(InvoiceToTest.AH_TransactionNum, TestInvoiceCreditAdjustmentVoucher.VoucherLines[1].VoucherNumber);
		}

		public void TestLineVoucherNumberDetailsForAP()
		{
			InvoiceToTest.AH_Ledger = LedgerTypes.AccountsPayable;
			InvoiceToTest.AH_TransactionNum = "ABCD";
			InvoiceToTest.AH_TransactionReference = "1234567890";
			InvoiceToTest.AH_ConsolidatedInvoiceRef = "XYZ";
			AssertEquals(InvoiceToTest.AH_ConsolidatedInvoiceRef, TestInvoiceCreditAdjustmentVoucher.VoucherLines[0].VoucherNumber);
			AssertEquals(InvoiceToTest.AH_ConsolidatedInvoiceRef, TestInvoiceCreditAdjustmentVoucher.VoucherLines[1].VoucherNumber);
		}

		public void TestTaiwanLineVoucherNumberDetailsForAP()
		{
			GlbCompany.CurrentCompany.GC_RN_NKCountryCode = Core.Constants.CountryCodes.Taiwan;
			InvoiceToTest.AH_Ledger = LedgerTypes.AccountsPayable;
			InvoiceToTest.AH_TransactionNum = "ABCD";
			InvoiceToTest.AH_TransactionReference = "1234567890";
			InvoiceToTest.AH_ConsolidatedInvoiceRef = "C10000001";
			AssertEquals("For Taiwan voucher number is AH_ConsolidatedInvoiceRef of Invoice", InvoiceToTest.AH_ConsolidatedInvoiceRef, TestInvoiceCreditAdjustmentVoucher.VoucherLines[0].VoucherNumber);
			AssertEquals("For Taiwan voucher number is AH_ConsolidatedInvoiceRef of Invoice", InvoiceToTest.AH_ConsolidatedInvoiceRef, TestInvoiceCreditAdjustmentVoucher.VoucherLines[1].VoucherNumber);
		}

		public void TestGSTAmount()
		{
			InvoiceToTest.AH_GSTAmount = 17.0m;
			AssertEquals(0.0m, TestInvoiceCreditAdjustmentVoucher.VoucherLines[3].DebitAmount);
			AssertEquals(17.0m, TestInvoiceCreditAdjustmentVoucher.VoucherLines[3].CreditAmount);
		}

		public void TestControlAmount()
		{
			InvoiceToTest.AH_GSTAmount = 17.0m;
			InvoiceToTest.AH_InvoiceAmount = 170.0m;
			AssertEquals(187.0m, TestInvoiceCreditAdjustmentVoucher.VoucherLines[0].DebitAmount);
			AssertEquals(0.0m, TestInvoiceCreditAdjustmentVoucher.VoucherLines[0].CreditAmount);
		}

		public void TestGSTLine()
		{
			LocalAccountNumber1 = "1000.10.10";
			LocalAccountNumber2 = "1000.10.20";
			LocalControlAccount = "1000.10.30";
			LocalGST = "1000.10.40";
			GLAccount1 = TestObjectCreator.InsertGLHeader();
			GLAccount2 = TestObjectCreator.InsertGLHeader();
			ControlAccount = TestObjectCreator.InsertGLHeader();
			GSTAccount = TestObjectCreator.InsertGLHeader();
			ChargeCode1 = TestObjectCreator.InsertChargeCode(Constants.ChargeType.Margin);
			ChargeCode1.AC_AG_RevenueAccount = GLAccount1.PK;
			ChargeCode1.AC_AG_CostAccount = GLAccount2.PK;
			GLAccountDescriptor1 = SetupAccountDescriptor(GLAccount1, LocalAccountNumber1);
			GLAccountDescriptor2 = SetupAccountDescriptor(GLAccount2, LocalAccountNumber2);
			GLAccountDescriptor3 = SetupAccountDescriptor(ControlAccount, LocalControlAccount);
			GLAccountDescriptor4 = SetupAccountDescriptor(GSTAccount, LocalGST);
			LineAmount1 = 120m;
			LineAmount2 = 70m;
			GSTAmount1 = 12.0m;
			GSTAmount2 = 7.0m;
			var mockControlAccount = new Mock<IControlAccountProvider>();
			mockControlAccount.Setup(m => m.SetTransaction(It.IsAny<AccTransactionHeader>()));
			mockControlAccount.Setup(m => m.PK).Returns(ControlAccount.PK);
			mockControlAccount.Setup(m => m.GST).Returns(GSTAccount.PK);
			InvoiceToTest.AH_GSTAmount = 12.0m;
			InvoiceCreditAdjustmentVoucherProvider testProvider = new InvoiceCreditAdjustmentVoucherProvider(InvoiceToTest, mockControlAccount.Object);
			AssertEquals(LocalControlAccount, testProvider.VoucherLines[0].AccountNumber);
			AssertEquals(LocalGST, testProvider.VoucherLines[3].AccountNumber);
			AssertEquals(1m, testProvider.VoucherLines[3].ExchangeRate);
			AssertEquals(0.0m, testProvider.VoucherLines[3].DebitAmount);
			AssertEquals(12.0m, testProvider.VoucherLines[3].CreditAmount);
			AssertEquals(0.0m, testProvider.VoucherLines[3].OSDebitAmount);
			AssertEquals(12.0m, testProvider.VoucherLines[3].OSCreditAmount);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return new InvoiceCreditAdjustmentVoucherProvider(Factory.NewWithValidTestData(typeof(AccTransactionHeader)) as AccTransactionHeader);
		}

		protected override AccTransactionHeader GetTestTransaction()
		{
			return InvoiceToTest;
		}

		protected override void SetLineDescription(AccTransactionHeader header)
		{
			base.SetLineDescription(header);
			InvoicingBase headerWithLines = Factory.Load<InvoicingBase>(header.PK);
			headerWithLines.AH_Desc = "TRANSACTION_HEADER_DESCRIPTION";
			foreach (AccTransactionLines line in headerWithLines.Lines)
			{
				line.AL_Desc = "TRANSACTION_LINE_DESCRIPTION";
			}
		}

		protected override ZString GetExpectedLineDescriptionForFirstLine()
		{
			return "TRANSACTION_LINE_DESCRIPTION";
		}

		protected override ZString GetExpectedLineDescriptionForControlLine()
		{
			return "TRANSACTION_HEADER_DESCRIPTION";
		}

		protected override void SetJobNumber(AccTransactionHeader header)
		{
			base.SetJobNumber(header);
			InvoicingBase headerWithLines = Factory.Load<InvoicingBase>(header.PK);
			headerWithLines.AH_JH = Job.PK;
			foreach (AccTransactionLines line in headerWithLines.Lines)
			{
				line.AL_JH = Job.PK;
			}
		}

		protected override void SetOrganisationCode(AccTransactionHeader header)
		{
			base.SetOrganisationCode(header);
			header.AH_OH = TestObjectCreator.AALSHI.PK;
		}

		protected override ZString GetExpectedOrganisationCodeForControlLine()
		{
			return TestObjectCreator.AALSHI.OH_Code;
		}

		protected override ZString GetExpectedOrganisationCodeForFirstLine()
		{
			return TestObjectCreator.AALSHI.OH_Code;
		}

		protected override bool JobNumberIsApplicableToThisVoucher
		{
			get
			{
				return true;
			}
		}

		protected override VoucherProvider GetVoucherProvider(AccTransactionHeader transaction)
		{
			return new InvoiceCreditAdjustmentVoucherProvider(transaction);
		}

		protected override ZInt GetExpectedAttachementNo()
		{
			return 0;
		}

		protected override bool ControlLineDescriptionIsApplicableForThisVoucher
		{
			get
			{
				return true;
			}
		}

		protected override void PrepareDataForTestForNewFieldsTest()
		{
			InvoiceToTest.AH_InvoiceAmount = -170.0m;
			InvoiceToTest.Lines[0].AL_LineAmount = -100m;
			InvoiceToTest.Lines[0].AL_GSTVAT = 0m;
			InvoiceToTest.Lines[0].AL_OSAmount = -100m;
			InvoiceToTest.Lines[1].AL_LineAmount = -70m;
			InvoiceToTest.Lines[1].AL_GSTVAT = 0m;
			InvoiceToTest.Lines[1].AL_OSGSTAmount = 0m;
			InvoiceToTest.Lines[1].AL_OSAmount = -70m;
		}
	}
}
