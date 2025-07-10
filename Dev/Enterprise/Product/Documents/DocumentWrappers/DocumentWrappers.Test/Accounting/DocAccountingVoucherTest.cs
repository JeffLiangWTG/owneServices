using System;
using System.Data;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.ARAP;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.DataInterface.ChinaDataInterface;
using Enterprise.Accounting.Business.DataInterface.ChinaDataInterface.VoucherFile;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Core;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers
{
	[TestedType(typeof(DocAccountingVoucher))]
	sealed class DocAccountingVoucherTest : DocumentWrapperTestCase
	{
		public void TestDocAccountingVoucherWrappedObject()
		{
			VoucherProvider testVoucherProvider = new VoucherProviderFactory(Factory).GetProvider(TestTransaction);
			DocAccountingVoucher docAccountingVoucher = DocAccountingVoucher.New(testVoucherProvider, Factory);
			AssertEquals("Wrapped Object should be TestVoucherProvider", testVoucherProvider, docAccountingVoucher.WrappedObject);
		}

		public override DocumentWrapper[] GetDocumentWrappers()
		{
			VoucherProvider testVoucherProvider = new VoucherProviderFactory(Factory).GetProvider(TestTransaction);
			return new DocumentWrapper[] { DocAccountingVoucher.New(testVoucherProvider, Factory) };
		}

		public void TestVoucherPeriod()
		{
			AssertEquals(1, TestWrapper.Period);
		}

		public void TestVoucherPeriodYear()
		{
			AssertEquals(TestPeriodYear, TestWrapper.PeriodYear);
		}

		public void TestVoucherVoucherNumberForCTR()
		{
			AccGLHeader aPControl = Factory.NewWithValidTestData(typeof(AccGLHeader)) as AccGLHeader;
			AccGLHeader aRControl = Factory.NewWithValidTestData(typeof(AccGLHeader)) as AccGLHeader;

			aPControl.AG_DebitCredit = Constants.DebitCredit.Debit;
			aRControl.AG_DebitCredit = Constants.DebitCredit.Debit;

			SetupAccountDescriptor(aRControl, "1000", "LocalAccountDescription1",
								   AccountTypeComboBoxConstants.BalanceSheetAccount);
			SetupAccountDescriptor(aPControl, "2000", "LocalAccountDescription3",
								   AccountTypeComboBoxConstants.BalanceSheetAccount);

			AccountingConfigurationRegistry.Instance.ARControlAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty,
																			   aRControl.PK.ToGuid());
			AccountingConfigurationRegistry.Instance.APControlAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty,
																			   aPControl.PK.ToGuid());
			Factory.Save();
			Contra testContra = Contra.New(Factory);
			testContra.ARRow.AH_TransactionNum = TestVoucherNum;
			testContra.APRow.AH_TransactionNum = TestVoucherNum;
			VoucherProvider testVoucherProvider = new ARAPContraVoucherProvider(testContra.APRow);
			fTestWrapper = DocAccountingVoucher.New(testVoucherProvider, Factory);
			AssertEquals("CTR" + TestVoucherNum, TestWrapper.VoucherNumber);
		}

		AccGLAccountDescriptor SetupAccountDescriptor(AccGLHeader gLAccount, string localAccountNum,
																string localAccountDescription, string reportCategory)
		{
			AccGLAccountDescriptor gLAccountDescriptor = Factory.New(typeof(AccGLAccountDescriptor)) as AccGLAccountDescriptor;
			gLAccountDescriptor.AJ_Language = DataInterfaceUtils.GetLocalLanguage();
			gLAccountDescriptor.AJ_RN_NKCountryOfCompliance = Constants.CountryCodes.China;
			gLAccountDescriptor.AJ_ReportType = AccGLAccountDescriptor.ReportTypeCOA;
			gLAccountDescriptor.AJ_ReportCategory = reportCategory;
			gLAccountDescriptor.AJ_LocalAccountNumber = localAccountNum;
			gLAccountDescriptor.AJ_AccountDescription = localAccountDescription;
			gLAccountDescriptor.ParentGLHeaderPK = gLAccount.PK;
			gLAccountDescriptor.AJ_DebitCredit = gLAccount.AG_DebitCredit;
			return gLAccountDescriptor;
		}

		DataRow GetDummyDataRow(Guid gLHeader, string type, decimal amount)
		{
			DataTable testTable = new DataTable();
			testTable.Columns.Add("BranchCode", typeof(string));
			testTable.Columns.Add("DepartmentCode", typeof(string));
			testTable.Columns.Add("GLHeader", typeof(Guid));
			testTable.Columns.Add("TransactionType", typeof(string));
			testTable.Columns.Add("Amount", typeof(decimal));
			testTable.Columns.Add("OSAmount", typeof(decimal));
			testTable.Columns.Add("VoucherNumber", typeof(string));

			return testTable.Rows.Add(new object[] { GlbBranch.CurrentBranch.GB_Code, GlbDepartment.CurrentDepartment.GE_Code, gLHeader, type, amount, amount, TestVoucherNum });
		}

		public void TestVoucherVoucherNumberForJC()
		{
			VoucherDataSource lineToReturn = new VoucherDataSource(Factory, GetDummyDataRow(Guid.Empty, "", 0));

			var mockCollection = new Mock<WIPAccrualDataSourceCollection>();
			mockCollection.Setup(m => m.GetCount()).Returns(3);
			mockCollection.Setup(m => m.GetVoucherData(0)).Returns(lineToReturn);
			mockCollection.Setup(m => m.GetVoucherData(1)).Returns(lineToReturn);
			mockCollection.Setup(m => m.GetVoucherData(2)).Returns(lineToReturn);

			WIPVoucherProvider testProvider = new WIPVoucherProvider(mockCollection.Object, 0);

			fTestWrapper = DocAccountingVoucher.New(testProvider, Factory);

			AssertEquals("WIP" + TestVoucherNum, TestWrapper.VoucherNumber);
		}

		public void TestVoucherVoucherNumberWithPrefixForJC()
		{
			AccountingMasterFilesRegistry.Instance.PrintGLVoucherBasedOnTransactionLineBranch.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			VoucherDataSource lineToReturn = new VoucherDataSource(Factory, GetDummyDataRow(Guid.Empty, "", 0));

			var mockCollection = new Mock<WIPAccrualDataSourceCollection>();
			mockCollection.Setup(m => m.GetCount()).Returns(3);
			mockCollection.Setup(m => m.GetVoucherData(0)).Returns(lineToReturn);
			mockCollection.Setup(m => m.GetVoucherData(1)).Returns(lineToReturn);
			mockCollection.Setup(m => m.GetVoucherData(2)).Returns(lineToReturn);

			WIPVoucherProvider testProvider = new WIPVoucherProvider(mockCollection.Object, 0);

			fTestWrapper = DocAccountingVoucher.New(testProvider, Factory);

			AssertEquals(GlbBranch.CurrentBranch.GB_Code + "WIP" + TestVoucherNum, TestWrapper.VoucherNumber);
		}

		public void TestVoucherVoucherNumberForAR()
		{
			TestTransaction.AH_TransactionNum = TestVoucherNum;
			TestTransaction.AH_TransactionReference = "";

			TestTransaction.AH_Ledger = LedgerTypes.AccountsReceivable;
			TestTransaction.AH_TransactionType = TransactionTypes.Payment;

			AssertEquals("ARPAY" + TestVoucherNum, TestWrapper.VoucherNumber);
		}

		public void TestVoucherVoucherNumberForAP()
		{
			TestTransaction.AH_TransactionReference = TestVoucherNum;
			TestTransaction.AH_TransactionNum = "";
			TestTransaction.AH_ConsolidatedInvoiceRef = "xyz123";

			TestTransaction.AH_Ledger = LedgerTypes.AccountsPayable;
			TestTransaction.AH_TransactionType = TransactionTypes.Invoice;

			AssertEquals("APINVxyz123", TestWrapper.VoucherNumber);
		}

		public void TestPostDate()
		{
			TestTransaction.AH_PostDate = TestDate;

			AssertEquals(TestDate, TestWrapper.VoucherPostDate);
			AssertEquals(TestDate.Year.ToString(), TestWrapper.VoucherPostDateYear);
			AssertEquals(TestDate.Month.ToString(), TestWrapper.VoucherPostDateMonth);
			AssertEquals(TestDate.Day.ToString(), TestWrapper.VoucherPostDateDay);
		}

		public void TestTransactionBranchCode()
		{
			AssertEquals("TransactionBranchCode", GlbBranch.CurrentBranch.GB_Code, TestWrapper.TransactionBranchCode);

			var branchSHA = TestObjectCreator.CreateBranch("SHA", "Shanghai", GlbCompany.CurrentCompany);
			TestTransaction.AH_GB = branchSHA.PK;
			AssertEquals("TransactionBranchCode should equal to the transaction branch rather than the login branch", "SHA", TestWrapper.TransactionBranchCode);
		}

		public void TestTransactionCompanyName()
		{
			GlbCompany.CurrentCompany.GC_RN_NKCountryCode = Core.Constants.CountryCodes.China;
			AssertEquals("TransactionCompanyName", "EDI CUSTOMS BROKERS", TestWrapper.TransactionCompanyName);

			var branchOrgProxy = TestObjectCreator.CreateOrgHeader("BRNPROXY", true, true);
			var branchSHA = TestObjectCreator.CreateBranch("SHA", "Shanghai", GlbCompany.CurrentCompany, branchOrgProxy);
			var branchOrgProxyARMAddressCHS = TestObjectCreator.CreateAddress(branchOrgProxy, OrgAddressType.Receivables, true);
			branchOrgProxyARMAddressCHS.OA_Language = Core.SharedConstants.Languages.ChineseSimplified;
			branchOrgProxyARMAddressCHS.CompanyName = "Branch > Org Proxy > Main ARM Address’s Company Name in CHS";

			TestTransaction.AH_GB = branchSHA.PK;
			AssertEquals("TransactionCompanyName", branchOrgProxyARMAddressCHS.CompanyName, TestWrapper.TransactionCompanyName);
		}

		public void TestPostedBy()
		{
			TestTransaction = Factory.NewWithValidTestData(typeof(ARInvoice)) as AccTransactionHeader;
			Factory.Save();
			ARInvoice invoiceToRead = new BusinessObjectFactory().Load(typeof(ARInvoice), TestTransaction.PK) as ARInvoice;
			AssertEquals(invoiceToRead.CreatingUser, TestWrapper.PostedBy);
		}

		public void TestEnterBy()
		{
			GlbStaff staff = Factory.LoadTop1<GlbStaff>(new ZQuery(GlbStaffSchema.GS_Code, "E"));
			Factory.Save();
			ARInvoice invoiceToRead = new BusinessObjectFactory().Load(typeof(ARInvoice), TestTransaction.PK) as ARInvoice;
			AssertEquals(invoiceToRead.CreatingUser, TestWrapper.EnterBy);
		}

		public void TestReviewer()
		{
			GlbStaff staff = Factory.LoadTop1<GlbStaff>(new ZQuery(GlbStaffSchema.GS_Code, "E"));
			AccountingConfigurationRegistry.Instance.VoucherAppointedPartiesReviewerDefault.SetValue(Guid.Empty, BranchPK, Guid.Empty, staff.PK.ToGuid());
			GlbStaff newStaff = TestObjectCreator.CreateStaff("JNS");
			newStaff.GS_FullName = "John Snow";
			Factory.Save();

			TestTransaction.AH_TransactionType = TransactionTypes.Receipt;
			TestTransaction.AH_GS_NKAuditedBy = newStaff.GS_Code;
			AssertEquals("Should be from GS_FullName", newStaff.GS_FullName, TestWrapper.Reviewer);
		}

		public void TestCashier()
		{
			GlbStaff staff = Factory.LoadTop1<GlbStaff>(new ZQuery(GlbStaffSchema.GS_Code, "E"));
			AccountingConfigurationRegistry.Instance.VoucherAppointedPartiesCashierDefault.SetValue(Guid.Empty, BranchPK, Guid.Empty, staff.PK.ToGuid());
			GlbStaff newStaff = TestObjectCreator.CreateStaff("JNS");
			newStaff.GS_FullName = "John Snow";
			Factory.Save();

			TestTransaction.AH_TransactionType = TransactionTypes.Receipt;
			TestTransaction.AH_GS_NKCashier = newStaff.GS_Code;
			AssertEquals("Should be from GS_FullName", newStaff.GS_FullName, TestWrapper.Cashier);
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

			var voucher = DocAccountingVoucher.New(provider, Factory);

			AssertEquals(25m, voucher.TotalDebitAmount);
			AssertEquals(25m, voucher.TotalCreditAmount);
		}

		public void TestNumberOfSupportingDocuments()
		{
			TestTransaction.AH_NumberOfSupportingDocuments = 1;
			AssertEquals(1, TestWrapper.NumberOfSupportingDocuments.ToZInt());
		}

		Guid BranchPK { get { return TestTransaction != null && TestTransaction.Branch != null ? TestTransaction.Branch.PK.ToGuid() : Guid.Empty; } }

		public void TestPrintedBy()
		{
			AssertEquals(GlbStaff.CurrentUser.GS_FullName, TestWrapper.VoucherPrintedBy);
		}

		public void TestGetPeriod()
		{
			AssertEquals(12, TestWrapper.GetPeriodNoOnly(200412));
			AssertEquals(13, TestWrapper.GetPeriodNoOnly(13));
			AssertEquals(1, TestWrapper.GetPeriodNoOnly(1));
			AssertEquals(0, TestWrapper.GetPeriodNoOnly(-1));
			AssertEquals(0, TestWrapper.GetPeriodNoOnly(0));
		}

		public void TestVoucherLineCount()
		{
			TestWrapper.VoucherProvider = MakeTestWrapperForTestVoucherLineCount(true);

			AssertEquals(3, TestWrapper.VoucherLines.Count);
		}

		public void TestVoucherPrint_WhenOnlyAmountZeroVoucherLine_ShouldNotCreateVoucherLine()
		{ 
			TestWrapper.VoucherProvider = MakeTestWrapperForTestVoucherLineCount(false);

			AssertEquals(0, TestWrapper.VoucherLines.Count);
		}

		VoucherProvider MakeTestWrapperForTestVoucherLineCount(bool isPositiveNumber)
		{
			var lines = new VoucherLine[3];
			lines[0] = new VoucherLine();
			lines[1] = new VoucherLine();
			lines[2] = new VoucherLine();

			if (isPositiveNumber)
			{
				lines[0].CreditAmount = 1;
				lines[1].CreditAmount = 2;
				lines[2].DebitAmount = 3;
			}

			var voucherMock = new Mock<InvoiceCreditAdjustmentVoucherProvider>(TestTransaction) { CallBase = true };
			voucherMock.Setup(m => m.VoucherLines).Returns(lines);

			return voucherMock.Object;
		}

		public void TestVoucherLinesGroupByGLAccountCurrencyDescription()
		{
			var line1 = CreateVoucherLine(TestObjectCreator.GLHeader1.PK, "Group 1", "a", Constants.CurrencyCodes.China, 100m, 0m, 100m, 0m);
			var line2 = CreateVoucherLine(TestObjectCreator.GLHeader1.PK, "Group 1", "b", Constants.CurrencyCodes.China, 100m, 0m, 100m, 0m);
			var line3 = CreateVoucherLine(TestObjectCreator.GLHeader2.PK, "Group 1", "c", Constants.CurrencyCodes.China, 20m, 0m, 20m, 0m);
			var line4 = CreateVoucherLine(TestObjectCreator.GLHeader1.PK, "Group 1", "d", Constants.CurrencyCodes.Taiwan, 10m, 0m, 10m, 0m);
			var line5 = CreateVoucherLine(TestObjectCreator.GLHeader2.PK, "Group 2", "e", Constants.CurrencyCodes.Taiwan, 75m, 0m, 75m, 0m);
			var line6 = CreateVoucherLine(TestObjectCreator.GLHeader2.PK, "Group 2", "f", Constants.CurrencyCodes.Taiwan, 75m, 0m, 75m, 0m);
			var line7 = CreateVoucherLine(ZGuid.Empty, "Non-Group", "g", Constants.CurrencyCodes.China, 5m, 0m, 5m, 0m);
			var line8 = CreateVoucherLine(TestObjectCreator.GLHeader1.PK, "Credit", "h", Constants.CurrencyCodes.China, 0m, 385m, 0m, 385m);

			var lines = new VoucherLine[] { line1, line2, line3, line4, line5, line6, line7, line8 };
			var voucherProvider = new VoucherProviderFactory(Factory).GetProvider(TestTransaction);
			var accountingVoucher = DocAccountingVoucher.New(voucherProvider, Factory);
			voucherProvider.VoucherLines = lines;
			var voucherLines = accountingVoucher.VoucherLinesGroupByGLAccountCurrencyDescription;

			AssertEquals(6, voucherLines.Count);
			AssertEquals(385m, voucherLines.Cast<DocAccountingVoucherLine>().Sum(x => x.CreditAmount));
			AssertEquals(385m, voucherLines.Cast<DocAccountingVoucherLine>().Sum(x => x.DebitAmount));

			AssertEquals(385m, voucherLines.Cast<DocAccountingVoucherLine>().Last().CreditAmount);
			AssertEquals(0m, voucherLines.Cast<DocAccountingVoucherLine>().Last().DebitAmount);
		}

		VoucherLine CreateVoucherLine(ZGuid accountPk, ZString desc, ZString additionDesc, ZString currencyCode, ZDecimal dbtAmt, ZDecimal cdtAmt, ZDecimal osDbtAmt, ZDecimal osCdtAmt)
		{
			var line = new VoucherLine(TestTransaction);
			line.AccountPK = accountPk;
			line.Description = desc;
			line.AdditionalAccountDescription = additionDesc;
			line.CurrencyCode = currencyCode;
			line.CreditAmount = cdtAmt;
			line.DebitAmount = dbtAmt;
			line.OSCreditAmount = osCdtAmt;
			line.OSDebitAmount = osDbtAmt;
			return line;
		}

		AccTransactionHeader TestTransaction;
		int TestPeriod;
		int TestPeriodYear;
		DocAccountingVoucher fTestWrapper;
		ZDateTime TestDate;
		ZString TestVoucherNum;

		DocAccountingVoucher TestWrapper
		{
			get
			{
				if (fTestWrapper == null)
				{
					VoucherProvider testVoucherProvider = new VoucherProviderFactory(Factory).GetProvider(TestTransaction);
					fTestWrapper = DocAccountingVoucher.New(testVoucherProvider, Factory);
				}
				return fTestWrapper;
			}
		}

		protected override void SetUp()
		{
			TestPeriod = 200401;
			TestPeriodYear = 2004;
			TestVoucherNum = "ABC123";
			TestDate = new ZDateTime(2004, 1, 15);

			AccountingPeriodTestHelper testHelper = new AccountingPeriodTestHelper(Factory);
			testHelper.SetupSinglePeriod(TestPeriod, new ZDateTime(2004, 1, 1), new ZDateTime(2004, 1, 31));

			TestTransaction = Factory.NewWithValidTestData(typeof(AccTransactionHeader)) as AccTransactionHeader;
			TestTransaction.AH_PostDate = TestDate;
			TestTransaction.AH_Ledger = LedgerTypes.AccountsPayable;
			TestTransaction.AH_TransactionType = TransactionTypes.Invoice;
			base.SetUp();
		}
	}
}
