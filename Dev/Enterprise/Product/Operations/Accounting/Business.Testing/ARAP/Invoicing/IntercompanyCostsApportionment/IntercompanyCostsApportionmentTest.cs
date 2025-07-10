using System;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.ARAP.Invoicing.Testing
{
	[TestedType(typeof(IntercompanyCostsApportionment))]
	public class IntercompanyCostsApportionmentTest : NonPersistentBusinessObjectTestCase
	{
		#region Implementation

		TestObjectCreator TestObjectCreator
		{
			get { return TestObjectCreator_internal ?? (TestObjectCreator_internal = new TestObjectCreator(Factory)); }
		}
		TestObjectCreator TestObjectCreator_internal;

		protected override void SetUp()
		{
			base.SetUp();

			testInvoice = new IntercompanyCostsApportionmentInvoice(Factory);
			testInvoiceLine = new IntercompanyCostsApportionmentInvoiceLine(Factory, testInvoice);
			//testApportionment = new IntercompanyCostsApportionment(Factory, testInvoiceLine);
			testApportionment = testInvoiceLine.Apportionments.AddNew();

			ZQuery query = new ZQuery(GlbBranchSchema.GB_GC, SQLComparisonOperator.NotEqual, GlbCompany.CurrentCompany.PK);
			Guid demoCompanyGuid = new Guid("03052ED3-2C64-49AC-97D8-C6079D5015B5");
			query.AddToFilter(GlbBranchSchema.GB_GC, SQLComparisonOperator.NotEqual, demoCompanyGuid);
			intercompanyBranch = Factory.LoadTop1<GlbBranch>(query);

			headerCurrentCompany = TestObjectCreator.CreateCurrentCompanyIntercompanyClearingGLHeader();
			headerIntercompany = TestObjectCreator.CreateIntercompanyIntercompanyClearingGLHeader();

			var gst = Factory.LoadTop1<AccTaxRate>(new ZQuery(AccTaxRateSchema.AT_Code, "GST").AddToFilter(AccTaxRateSchema.AT_RN_NKCountry, Core.Constants.CountryCodes.Australia));
			if (gst != null)
			{
				gst.SetRate_ForTestOnly(10, 1);
			}

			Factory.Save();
		}

		protected IntercompanyCostsApportionmentInvoice testInvoice;
		protected IntercompanyCostsApportionmentInvoiceLine testInvoiceLine;
		protected IntercompanyCostsApportionment testApportionment;
		protected GlbBranch intercompanyBranch;
		protected AccountingPeriodTestHelper PeriodManagementTestHelper;
		protected AccGLHeader headerCurrentCompany;
		protected AccGLHeader headerIntercompany;

		protected override BusinessObject GetNewBusinessObject()
		{
			return new IntercompanyCostsApportionment(Factory, testInvoiceLine);
		}

		#endregion

		#region Helpers

		void setupPeriodManagement(int year)
		{
			TestCaseHelper.ClearTable(AccPeriodManagement.Schema.TableName);
			PeriodManagementTestHelper = new AccountingPeriodTestHelper();
			PeriodManagementTestHelper.PostPeriodsForEntireYear(year, GlbCompany.CurrentCompany.PK);
			PeriodManagementTestHelper.PostPeriodsForEntireYear(year, intercompanyBranch.GB_GC, Enterprise.MasterFiles.Business.AccountingPeriodTestHelper.CalendarType.CalendarYear);
		}

		RefExchangeRate createExchangeRate()
		{
			RefExchangeRate exRate = Factory.New<RefExchangeRate>();
			exRate.RE_GC = intercompanyBranch.GB_GC;
			exRate.RE_ExRateType = nameof(ExchangeRateType.Buy);
			exRate.RE_RX_NKExCurrency = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;
			exRate.RE_StartDate = new ZDateTime(2009, 1, 1);
			exRate.RE_ExpiryDate = new ZDateTime(2009, 12, 31);
			exRate.RE_SellRate = 0.7;
			return exRate;
		}

		#endregion

		#region Tests

		[TestDate(2009, 09, 02)]
		public void TestCompany()
		{
			setupPeriodManagement(2009);
			testInvoice.PostedDate = new ZDateTime(2009, 06, 01);
			TestObjectCreator.SetupIntercompanyClearingConfigurationRegistry(headerCurrentCompany, headerIntercompany, intercompanyBranch.GB_GC);
			testApportionment.Company = intercompanyBranch.GB_GC;
			AssertEquals("Company", intercompanyBranch.GB_GC, testApportionment.Company);
			GlbCompany company = Factory.Load<GlbCompany>(intercompanyBranch.GB_GC);
			AssertEquals("CompanyLocalCurrency", company.GC_RX_NKLocalCurrency, testApportionment.CompanyLocalCurrency);
			AssertEquals("IntercompanyGLAccount", headerIntercompany.PK, testApportionment.IntercompanyGLAccount);
			AssertEquals("CompanyPostToGLAccount", headerCurrentCompany.PK, testApportionment.CompanyPostToGLAccount);
			AssertEquals("AccountingPeriod", 200906, testApportionment.AccountingPeriod);
		}

		public void TestValidateCompany()
		{
			testApportionment.Company = Guid.NewGuid();
			Assert("CompanyInfo", testApportionment.CompanyInfo.HasError("Please enter a valid Company."));
			testApportionment.Company = ZGuid.Empty;
			Assert("CompanyInfo", testApportionment.CompanyInfo.HasError("Please enter a valid Company."));
			testApportionment.Company = intercompanyBranch.GB_GC;
			Assert("CompanyInfo", !testApportionment.CompanyInfo.HasErrors());
		}

		[TestDate(2009, 09, 02)]
		public void TestCompanyLocalCurrency()
		{
			testInvoice.PostedDate = new ZDateTime(2009, 06, 01);
			testApportionment.CompanyLocalCurrency = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;
			AssertEquals("CompanyLocalCurrency", GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency, testApportionment.CompanyLocalCurrency);
			AssertEquals("CompanyLocalExchangeRate", 1.0m, testApportionment.CompanyLocalExchangeRate);

			RefExchangeRate exchangeRate = createExchangeRate();
			Factory.Save();
			ExchangeRateReader.GetReaderInstance().ClearCache();
			GlbCompany company = Factory.Load<GlbCompany>(intercompanyBranch.GB_GC);
			testApportionment.Company = intercompanyBranch.GB_GC;
			AssertEquals("Company", intercompanyBranch.GB_GC, testApportionment.Company);
			testApportionment.CompanyLocalCurrency = company.GC_RX_NKLocalCurrency;
			AssertEquals("CompanyLocalCurrency", company.GC_RX_NKLocalCurrency, testApportionment.CompanyLocalCurrency);
			AssertEquals("CompanyLocalExchangeRate", exchangeRate.RE_SellRate, testApportionment.CompanyLocalExchangeRate);
		}

		public void TestValidateCompanyLocalCurrency()
		{
			testApportionment.CompanyLocalCurrency = "AAA";
			Assert("CompanyLocalCurrencyInfo", testApportionment.CompanyLocalCurrencyInfo.HasError("Please enter a valid Company Local Currency."));
			testApportionment.CompanyLocalCurrency = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;
			Assert("CompanyLocalCurrencyInfo", !testApportionment.CompanyLocalCurrencyInfo.HasErrors());
		}

		public void TestBranch()
		{
			testApportionment.Branch = intercompanyBranch.PK;
			AssertEquals("Branch", intercompanyBranch.PK, testApportionment.Branch);
		}

		public void TestValidateBranch()
		{
			testApportionment.Branch = Guid.NewGuid();
			Assert("BranchInfo", testApportionment.BranchInfo.HasError("Please enter a valid Branch."));
			testApportionment.Branch = ZGuid.Empty;
			Assert("BranchInfo", testApportionment.BranchInfo.HasError("Please enter a valid Branch."));
			testApportionment.Branch = intercompanyBranch.PK;
			Assert("BranchInfo", !testApportionment.BranchInfo.HasErrors());
		}

		public void TestDepartment()
		{
			GlbDepartment department = Factory.LoadTop1<GlbDepartment>(new ZQuery(GlbDepartmentSchema.GE_IsActive, ZBool.True));
			testApportionment.Department = department.PK;
			AssertEquals("Department", department.PK, testApportionment.Department);
		}

		public void TestValidateDepartment()
		{
			testApportionment.Department = Guid.NewGuid();
			Assert("DepartmentInfo", testApportionment.DepartmentInfo.HasError("Please enter a valid Department."));
			testApportionment.Department = ZGuid.Empty;
			Assert("DepartmentInfo", testApportionment.DepartmentInfo.HasError("Please enter a valid Department."));
			GlbDepartment department = Factory.LoadTop1<GlbDepartment>(new ZQuery(GlbDepartmentSchema.GE_IsActive, ZBool.True));
			testApportionment.Department = department.PK;
			Assert("DepartmentInfo", !testApportionment.DepartmentInfo.HasErrors());
		}

		public void TestIntercompanyGLAccount()
		{
			testApportionment.IntercompanyGLAccount = headerIntercompany.PK;
			AssertEquals("IntercompanyGLAccount", headerIntercompany.PK, testApportionment.IntercompanyGLAccount);
		}

		public void TestValidateIntercompanyGLAccount()
		{
			testApportionment.Company = ZGuid.Empty;
			testApportionment.IntercompanyGLAccount = ZGuid.Empty;
			Assert("IntercompanyGLAccountInfo", testApportionment.IntercompanyGLAccountInfo.HasError("Please setup the Intercompany Clearing Configuration in the registry."));
			testApportionment.IntercompanyGLAccount = headerIntercompany.PK;
			Assert("IntercompanyGLAccountInfo", !testApportionment.IntercompanyGLAccountInfo.HasErrors());
			testApportionment.Company = GlbCompany.CurrentCompany.PK;
			testApportionment.IntercompanyGLAccount = ZGuid.Empty;
			Assert("IntercompanyGLAccountInfo", !testApportionment.IntercompanyGLAccountInfo.HasErrors());
		}

		public void TestCompanyPostToGLAccount()
		{
			testApportionment.CompanyPostToGLAccount = headerCurrentCompany.PK;
			AssertEquals("CompanyPostToGLAccount", headerCurrentCompany.PK, testApportionment.CompanyPostToGLAccount);
		}

		public void TestValidateCompanyPostToGLAccount()
		{
			testApportionment.Company = ZGuid.Empty;
			testApportionment.CompanyPostToGLAccount = ZGuid.Empty;
			Assert("CompanyPostToGLAccountInfo", testApportionment.CompanyPostToGLAccountInfo.HasError("Please setup the Intercompany Clearing Configuration in the registry."));
			testApportionment.CompanyPostToGLAccount = headerCurrentCompany.PK;
			Assert("CompanyPostToGLAccountInfo", !testApportionment.CompanyPostToGLAccountInfo.HasErrors());
			testApportionment.Company = GlbCompany.CurrentCompany.PK;
			testApportionment.CompanyPostToGLAccount = ZGuid.Empty;
			Assert("CompanyPostToGLAccountInfo", !testApportionment.CompanyPostToGLAccountInfo.HasErrors());

			var glHeader = Factory.NewWithValidTestData<AccGLHeader>();
			var companyFilter = glHeader.CompanyFilters.AddNew();
			companyFilter.ACF_GC_Company = testApportionment.Company;

			testApportionment.Company = TestObjectCreator.NonCurrentCompany.PK;
			testApportionment.CompanyPostToGLAccount = glHeader.PK;
			AssertNoError(testApportionment.CompanyPostToGLAccountInfo, "This GL Account cannot be used for the company");

			glHeader.AG_IsGlobal = false;
			companyFilter.ACF_GC_Company = Factory.NewWithValidTestData<GlbCompany>().PK;
			testApportionment.ValidateCompanyPostToGLAccount();
			AssertHasError(testApportionment.CompanyPostToGLAccountInfo, "This GL Account cannot be used for the company");
		}

		[TestDate(2009, 09, 02)]
		public void TestLocalAmount()
		{
			testInvoice.PostedDate = new ZDateTime(2009, 06, 01);
			RefExchangeRate exchangeRate = createExchangeRate();
			Factory.Save();
			ExchangeRateReader.GetReaderInstance().ClearCache();
			GlbCompany company = Factory.Load<GlbCompany>(intercompanyBranch.GB_GC);
			RefCurrency currency = RefCurrency.LoadFromCurrencyCode(Factory, company.GC_RX_NKLocalCurrency);
			ExchangeRate exRate = new ExchangeRate(company.GC_IsReciprocal, currency.Decimals, company.PK.ToGuid());
			testApportionment.Company = company.PK;
			ZDecimal invoiceExchangeRate = 1.45m;
			testApportionment.ExchangeRate = invoiceExchangeRate;
			testInvoice.ExchangeRate.Rate = invoiceExchangeRate;
			ZDecimal foreignAmount = 200.00m;
			testApportionment.ForeignApportionedAmount = foreignAmount;
			ZQuery query = new ZQuery(AccTaxRateSchema.AT_RN_NKCountry, GlbCompany.CurrentCompany.GC_RN_NKCountryCode);
			query.AddToFilter(AccTaxRateSchema.AT_IsActive, ZBool.True);
			query.AddToFilter(AccTaxRateSchema.AT_Type, AccTaxRate.Types.Rated);
			AccTaxRate taxRate = Factory.Load<AccTaxRate>(query).First(x => x.GetRate_ForTestOnly() > 0);
			testInvoiceLine.AL_AT = taxRate.PK;
			ZDecimal foreignGST = 20.00m;
			testInvoiceLine.Tax = foreignGST;
			testApportionment.ForeignGST = foreignGST;
			ZDecimal localAmount = Env.CurrentCompany.ExchangeRate.ForeignToLocal(foreignAmount, invoiceExchangeRate);
			testApportionment.LocalAmount = 0.00m;
			testApportionment.LocalAmount = localAmount;
			AssertEquals("LocalAmount", localAmount, testApportionment.LocalAmount);
			AssertEquals("CompanyLocalAmount", exRate.ForeignToLocal(localAmount, exchangeRate.RE_SellRate), testApportionment.CompanyLocalAmount);
			AssertEquals("LocalGST", Env.CurrentCompany.ExchangeRate.ForeignToLocal(foreignGST, invoiceExchangeRate), testApportionment.LocalGST);
		}

		public void TestValidateLocalAmount()
		{
			ZDecimal exchangeRate = 0.75m;
			testInvoice.ExchangeRate.Rate = exchangeRate;
			ZDecimal amount = 1000.00m;
			testInvoiceLine.Amount = amount;
			ZDecimal localAmount = Env.CurrentCompany.ExchangeRate.ForeignToLocal(amount, exchangeRate);
			testApportionment.IsAdjustingLocalAmountSuspended = true;
			testApportionment.LocalAmount = 456.78m;
			testApportionment.IsAdjustingLocalAmountSuspended = false;
			Assert("LocalAmountInfo", testApportionment.LocalAmountInfo.HasError("Local Amount column must sum to Invoice Line Local Amount."));
			testApportionment.LocalAmount = localAmount;
			Assert("LocalAmountInfo", !testApportionment.LocalAmountInfo.HasErrors());
		}

		public void TestForeignApportionedAmount()
		{
			testInvoiceLine.ApportionmentMethod = ZGuid.Empty;
			ZQuery query = new ZQuery(AccTaxRateSchema.AT_RN_NKCountry, GlbCompany.CurrentCompany.GC_RN_NKCountryCode);
			query.AddToFilter(AccTaxRateSchema.AT_IsActive, ZBool.True);
			query.AddToFilter(AccTaxRateSchema.AT_Type, AccTaxRate.Types.Rated);
			AccTaxRate taxRate = Factory.Load<AccTaxRate>(query).First(x => x.GetRate_ForTestOnly() > 0);
			testInvoiceLine.AL_AT = taxRate.PK;
			testInvoice.Currency = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;
			ZDecimal exchangeRate = 1.35m;
			testInvoice.ExchangeRate.Rate = exchangeRate;
			testApportionment.ExchangeRate = exchangeRate;
			ZDecimal foreignAmount = 100.00m;
			testInvoiceLine.Amount = foreignAmount;
			testApportionment.ApportionmentFactor = 100.0m;
			testApportionment.ForeignApportionedAmount = 0.00m;
			testApportionment.ForeignApportionedAmount = foreignAmount;
			AssertEquals("ForeignApportionedAmount", foreignAmount, testApportionment.ForeignApportionedAmount);
			AssertEquals("ForeignGST", testInvoiceLine.Tax, testApportionment.ForeignGST);
			AssertEquals("LocalAmount", Env.CurrentCompany.ExchangeRate.ForeignToLocal(foreignAmount, exchangeRate), testApportionment.LocalAmount);
			AssertEquals("ApportionmentFactor", 100.000m, testApportionment.ApportionmentFactor);
		}

		public void TestValidateForeignApportionedAmount()
		{
			ZDecimal amount = 123.45m;
			testInvoiceLine.Amount = amount;
			testInvoiceLine.ApportionmentMethod = ZGuid.Empty;
			testApportionment.ForeignApportionedAmount = 100.00m;
			Assert("ForeignApportionedAmountInfo", testApportionment.ForeignApportionedAmountInfo.HasError("Foreign Amount column in Apportionment Details must sum to the Invoice Line Amount."));
			testInvoiceLine.ApportionmentMethod = Guid.NewGuid();
			testApportionment.ForeignApportionedAmount = amount;
			Assert("ForeignApportionedAmountInfo", !testApportionment.ForeignApportionedAmountInfo.HasErrors());
			testInvoiceLine.ApportionmentMethod = ZGuid.Empty;
			testApportionment.ForeignApportionedAmount = 0.00m;
			Assert("ForeignApportionedAmountInfo", testApportionment.ForeignApportionedAmountInfo.HasError("Please enter an Amount."));
		}

		public void TestForeignGST()
		{
			testInvoiceLine.ApportionmentMethod = Guid.NewGuid();
			ZDecimal tax = 200.00m;
			testInvoiceLine.Tax = tax;
			testApportionment.ForeignGST = 300.00m;
			AssertEquals("ForeignGST", tax, testApportionment.ForeignGST);
			testApportionment.ForeignGST = tax;
			AssertEquals("ForeignGST", tax, testApportionment.ForeignGST);
		}

		public void TestValidateForeignGST()
		{
			ZDecimal tax = 100.00m;
			testInvoiceLine.Tax = tax;
			testApportionment.IsAdjustingForeignGSTSuspended = true;
			testApportionment.ForeignGST = 123.45m;
			testApportionment.IsAdjustingForeignGSTSuspended = false;
			Assert("ForeignGSTInfo", testApportionment.ForeignGSTInfo.HasError("Foreign GST column must sum to Invoice Line Tax."));
			testApportionment.ForeignGST = tax;
			Assert("ForeignGSTInfo", !testApportionment.ForeignGSTInfo.HasErrors());
		}

		public void TestCompanyLocalAmount()
		{
			ZDecimal companyLocalAmount = 123.45m;
			testApportionment.CompanyLocalAmount = companyLocalAmount;
			AssertEquals("CompanyLocalAmount", companyLocalAmount, testApportionment.CompanyLocalAmount);
		}

		public void TestExchangeRate()
		{
			ZDecimal exchangeRate = 0.75m;
			ZDecimal foreignAmount = 123.45m;
			testApportionment.ForeignApportionedAmount = foreignAmount;
			AssertEquals("ForeignApportionedAmount", foreignAmount, testApportionment.ForeignApportionedAmount);
			AssertEquals("ExchangeRate", 1.00m, testApportionment.ExchangeRate);
			testApportionment.ExchangeRate = exchangeRate;
			AssertEquals("ExchangeRate", exchangeRate, testApportionment.ExchangeRate);
			ZDecimal localAmount = Env.CurrentCompany.ExchangeRate.ForeignToLocal(foreignAmount, exchangeRate);
			AssertEquals("LocalAmount", localAmount, testApportionment.LocalAmount);
		}

		public void TestValidateExchangeRate()
		{
			testInvoice.Currency = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;
			testApportionment.ExchangeRate = 0.75m;
			Assert("ExchangeRateInfo", !testApportionment.ExchangeRateInfo.HasErrors());
			testApportionment.ExchangeRate = 0.00m;
			Assert("ExchangeRateInfo", testApportionment.ExchangeRateInfo.HasError(string.Format("Please make sure there is a buy exchange rate for {0} in the current company.", testInvoice.CurrencyObject.RX_Code)));
		}

		public void TestCompanyLocalExchangeRate()
		{
			ZDecimal exchangeRate = 1.23m;
			testApportionment.CompanyLocalExchangeRate = exchangeRate;
			AssertEquals("CompanyLocalExchangeRate", exchangeRate, testApportionment.CompanyLocalExchangeRate);
		}

		public void TestValidateCompanyLocalExchangeRate()
		{
			testApportionment.Company = intercompanyBranch.GB_GC;
			testApportionment.CompanyLocalExchangeRate = 0.75m;
			Assert("CompanyLocalExchangeRateInfo", !testApportionment.CompanyLocalExchangeRateInfo.HasErrors());
			testApportionment.CompanyLocalExchangeRate = 0.00m;
			GlbCompany company = Factory.Load<GlbCompany>(intercompanyBranch.GB_GC);
			string name = string.Empty;
			if (company != null)
			{
				name = company.GC_Name;
			}
			Assert("CompanyLocalExchangeRateInfo", testApportionment.CompanyLocalExchangeRateInfo.HasError(string.Format("Please make sure there is a buy exchange rate for {0} in this company ({1}).", GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency, name)));
		}

		[TestDate(2009, 09, 02)]
		public void TestAccountingPeriod()
		{
			ZInt period = 200906;
			testApportionment.AccountingPeriod = period;
			AssertEquals("AccountingPeriod", period, testApportionment.AccountingPeriod);
		}

		[TestDate(2009, 09, 02)]
		public void TestValidateAccountingPeriod()
		{
			setupPeriodManagement(2009);
			testApportionment.Company = intercompanyBranch.GB_GC;
			ZInt accountingPeriod = 200906;
			testApportionment.AccountingPeriod = accountingPeriod;
			Assert("AccountingPeriodInfo", !testApportionment.AccountingPeriodInfo.HasErrors());
			testApportionment.AccountingPeriod = 0;
			Assert("AccountingPeriodInfo", testApportionment.AccountingPeriodInfo.HasError("Please make sure Accounting Periods are setup in Period Management for this company, for the Posted Date."));
			ZQuery query = new ZQuery(AccPeriodManagementSchema.AM_GC_Company, SQLComparisonOperator.Equal, intercompanyBranch.GB_GC);
			query.AddToFilter(AccPeriodManagementSchema.AM_Period, SQLComparisonOperator.Equal, accountingPeriod);
			AccPeriodManagement periodManagement = Factory.LoadTop1<AccPeriodManagement>(query);
			if (periodManagement != null)
			{
				periodManagement.AM_IsGeneralLedgerClosed = true;
			}
			Factory.Save();
			testApportionment.AccountingPeriod = accountingPeriod;
			Assert("AccountingPeriodInfo", testApportionment.AccountingPeriodInfo.HasError("The General Ledger Accounting period is closed in this company."));
		}

		public void TestApportionmentFactor()
		{
			ZDecimal amount = 1000.00m;
			testInvoiceLine.Amount = amount;
			testInvoiceLine.ApportionmentMethod = ZGuid.Empty;
			ZDecimal factor = 100.000m;
			testApportionment.ApportionmentFactor = factor;
			AssertEquals("ApportionmentFactor", factor, testApportionment.ApportionmentFactor);
			AssertEquals("ForeignApportionedAmount", amount, testApportionment.ForeignApportionedAmount);
		}

		public void TestValidateApportionmentFactor()
		{
			testApportionment.ApportionmentFactor = -1.000m;
			Assert("ApportionmentFactorInfo", testApportionment.ApportionmentFactorInfo.HasError("Must be greater than 0 and less than or equal to 100."));
			testApportionment.ApportionmentFactor = 100.000m;
			Assert("ApportionmentFactorInfo", !testApportionment.ApportionmentFactorInfo.HasErrors());
			testApportionment.ApportionmentFactor = 101.000m;
			Assert("ApportionmentFactorInfo", testApportionment.ApportionmentFactorInfo.HasError("Must be greater than 0 and less than or equal to 100."));
			testApportionment.ApportionmentFactor = 50.000m;
			Assert("ApportionmentFactorInfo", testApportionment.ApportionmentFactorInfo.HasError("Apportionment Factor column must sum to 100."));
		}

		public void TestLocalGST()
		{
			ZDecimal exchangeRate = 0.75m;
			testInvoice.ExchangeRate.Rate = exchangeRate;
			ZDecimal tax = 100.00m;
			testInvoiceLine.Tax = tax;
			AssertEquals("Tax", tax, testInvoiceLine.Tax);
			ZDecimal localTax = Env.CurrentCompany.ExchangeRate.ForeignToLocal(tax, exchangeRate);
			AssertEquals("LocalTax", localTax, testInvoiceLine.LocalTax);
			testApportionment.LocalGST = 200.00m;
			AssertEquals("LocalGST", localTax, testApportionment.LocalGST);
			testApportionment.LocalGST = tax;
			AssertEquals("LocalGST", localTax, testApportionment.LocalGST);
		}

		public void TestValidateLocalGST()
		{
			testInvoice.ExchangeRate.Rate = 0.75m;
			testInvoiceLine.Tax = 1000.00m;
			testApportionment.IsAdjustingLocalGSTSuspended = true;
			testApportionment.LocalGST = 123.45m;
			testApportionment.IsAdjustingLocalGSTSuspended = false;
			Assert("LocalGSTInfo", testApportionment.LocalGSTInfo.HasError("Local GST column must sum to Invoice Line Local Tax."));
			testApportionment.LocalGST = testInvoiceLine.LocalTax;
			Assert("LocalGSTInfo", !testApportionment.LocalGSTInfo.HasErrors());
		}

		public void TestDescription()
		{
			ZString description = "Description String";
			testApportionment.Description = description;
			AssertEquals("Description", description, testApportionment.Description);
		}

		public void TestTemplateDescription()
		{
			ZString description = "Description String";
			testApportionment.TemplateLineDescription = description;
			AssertEquals("TemplateDescription", description, testApportionment.TemplateLineDescription);
		}

		public void TestBranchDepartmentCombinationValidation_IntercompanyCostsApportionment()
		{
			var bizObj = testApportionment;

			GlbBranchCombinationValidationTest.ValidateBranchDepartmentCombinationsForBizObj(Factory,
				(branch, department) => { bizObj.Branch = branch; bizObj.Department = department; }, bizObj.DepartmentInfo);
		}

		#endregion
	}
}
