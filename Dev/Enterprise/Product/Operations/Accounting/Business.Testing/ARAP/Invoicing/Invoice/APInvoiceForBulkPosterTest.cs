using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.ARAP.Invoicing.Testing
{
	[TestedType(typeof(APInvoiceForBulkPoster))]
	public class APInvoiceForBulkPosterTest : APInvoiceTest
	{
		public new void TestValidationForIncompleteTransaction()
		{
			Assert(true);
		}

		public void TestAH_LocalExTaxAmountUpdatesPoster()
		{
			APInvoiceForBulkPoster aPInvoiceForBulkPoster = GetNewBusinessObject() as APInvoiceForBulkPoster;
			APBulkInvoicePoster poster = new APBulkInvoicePoster(Factory);
			poster.Invoices.Add(aPInvoiceForBulkPoster);
			AssertEquals(aPInvoiceForBulkPoster.AH_LocalExTaxAmount, poster.Invoices.MasterAPBulkInvoicePoster.TotalLocalExTaxAmount);
			aPInvoiceForBulkPoster.AH_LocalExTaxAmount = 333;
			AssertEquals(aPInvoiceForBulkPoster.AH_LocalExTaxAmount, poster.Invoices.MasterAPBulkInvoicePoster.TotalLocalExTaxAmount);
		}

		public void TestAH_OSExTaxAmountReadOnly()
		{
			var aPInvoiceForBulkPoster = GetNewBusinessObject() as APInvoiceForBulkPoster;
			Assert("Should not be readonly", !aPInvoiceForBulkPoster.AH_OSExTaxAmountInfo.ReadOnly);
		}

		public void TestAH_OSExTaxAmountUpdatesPoster()
		{
			APInvoiceForBulkPoster aPInvoiceForBulkPoster = GetNewBusinessObject() as APInvoiceForBulkPoster;
			APBulkInvoicePoster poster = new APBulkInvoicePoster(Factory);
			poster.Invoices.Add(aPInvoiceForBulkPoster);
			AssertEquals(aPInvoiceForBulkPoster.AH_OSExTaxAmount, poster.Invoices.MasterAPBulkInvoicePoster.TotalOSExTaxAmount);
			aPInvoiceForBulkPoster.AH_OSExTaxAmount = 444;
			AssertEquals(aPInvoiceForBulkPoster.AH_OSExTaxAmount, poster.Invoices.MasterAPBulkInvoicePoster.TotalOSExTaxAmount);
		}

		public void TestAH_LocalTaxAmountUpdatesPoster()
		{
			APInvoiceForBulkPoster aPInvoiceForBulkPoster = GetNewBusinessObject() as APInvoiceForBulkPoster;
			APBulkInvoicePoster poster = new APBulkInvoicePoster(Factory);
			poster.Invoices.Add(aPInvoiceForBulkPoster);
			AssertEquals(aPInvoiceForBulkPoster.AH_OSTaxAmount, poster.Invoices.MasterAPBulkInvoicePoster.TotalOSTaxAmount);
			aPInvoiceForBulkPoster.AH_OSTaxAmount = 333;
			AssertEquals(aPInvoiceForBulkPoster.AH_OSTaxAmount, poster.Invoices.MasterAPBulkInvoicePoster.TotalOSTaxAmount);
		}

		public void TestAH_OSTaxAmountUpdatesPoster()
		{
			APInvoiceForBulkPoster aPInvoiceForBulkPoster = GetNewBusinessObject() as APInvoiceForBulkPoster;
			APBulkInvoicePoster poster = new APBulkInvoicePoster(Factory);
			poster.Invoices.Add(aPInvoiceForBulkPoster);
			AssertEquals(aPInvoiceForBulkPoster.AH_LocalTaxAmount, poster.Invoices.MasterAPBulkInvoicePoster.TotalLocalExTaxAmount);
			aPInvoiceForBulkPoster.AH_OSExTaxAmount = 444;
			AssertEquals(aPInvoiceForBulkPoster.AH_LocalExTaxAmount, poster.Invoices.MasterAPBulkInvoicePoster.TotalLocalExTaxAmount);
		}

		public void TestAH_OSTaxAmount_ValueAndReadOnly()
		{
			ZBool oldIsGSTRegistered = GlbCompany.CurrentCompany.GC_IsGSTRegistered;
			try
			{
				GlbCompany.CurrentCompany.GC_IsGSTRegistered = ZBool.True;

				OrgHeader org = Factory.New<OrgHeader>();
				org.OH_Code = "TestOrg";
				org.CompanyData.SetAPTaxApplicable(true);

				AccTaxRate taxRate = Factory.NewWithValidTestData<AccTaxRate>();
				taxRate.AT_Code = "Test";
				taxRate.AT_RN_NKCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
				taxRate.SetRate_ForTestOnly(10, 1, null, ZDate.Today);
				Factory.Save();

				APInvoiceForBulkPoster invoice = GetNewBusinessObject() as APInvoiceForBulkPoster;
				invoice.AH_OH = org.PK;
				invoice.TaxRate = taxRate.PK;
				invoice.AH_OSTaxAmount = 10m;
				AssertEquals("AH_OSTaxAmount must be editable when company is GST registered, Creditor is flaged for GST, Tax Rate is entered and rate of Tax Rate is not zero", false, invoice.AH_OSTaxAmountInfo.ReadOnly);
				AssertEquals("AH_OSTaxAmount is editable so it allows not zero values", 10m, invoice.AH_OSTaxAmount);

				taxRate = Factory.NewWithValidTestData<AccTaxRate>();
				taxRate.AT_Code = "Test1";
				taxRate.AT_RN_NKCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
				taxRate.SetRate_ForTestOnly(10, 1, null, ZDate.Today.AddDays(-2));

				invoice.AH_OH = org.PK;
				invoice.AH_OSTaxAmount = 11m;
				invoice.TaxRate = taxRate.PK;

				AssertEquals("AH_OSTaxAmount must be read only because Tax Rate has zero rate", true, invoice.AH_OSTaxAmountInfo.ReadOnly);
				AssertEquals("AH_OSTaxAmount must be zero as it is readonly", 0m, invoice.AH_OSTaxAmount);

				var rate2 = taxRate.SetRate_ForTestOnly(0, 1, ZDate.Today.AddDays(-1), ZDate.Today);
				invoice.AH_OH = org.PK;
				invoice.AH_OSTaxAmount = 11m;
				invoice.TaxRate = taxRate.PK;

				AssertEquals("AH_OSTaxAmount must be read only because Tax Rate has zero rate", true, invoice.AH_OSTaxAmountInfo.ReadOnly);
				AssertEquals("AH_OSTaxAmount must be zero as it is readonly", 0m, invoice.AH_OSTaxAmount);

				GlbCompany.CurrentCompany.GC_IsGSTRegistered = ZBool.False;

				rate2.ZAT_EndDate = ZDate.Today.AddDays(-1);
				taxRate.SetRate_ForTestOnly(10, 1, ZDate.Today, null);
				org.CompanyData.SetAPTaxApplicable(true);

				invoice.AH_OH = org.PK;
				invoice.AH_OSTaxAmount = 12m;
				invoice.TaxRate = taxRate.PK;

				AssertEquals("AH_OSTaxAmount must be read only because company is NOT GST registered", true, invoice.AH_OSTaxAmountInfo.ReadOnly);
				AssertEquals("AH_OSTaxAmount must be zero as it is readonly", 0m, invoice.AH_OSTaxAmount);

				GlbCompany.CurrentCompany.GC_IsGSTRegistered = ZBool.True;

				org.CompanyData.SetAPTaxApplicable(false);

				invoice.AH_OH = org.PK;
				invoice.TaxRate = taxRate.PK;
				invoice.AH_OSTaxAmount = 13m;

				((TransactionHeaderValidation)invoice.Validation).ValidateAH_OSTaxAmount();
				AssertEquals("AH_OSTaxAmount must be read only because Creditor is NOT flaged for GST", true, invoice.AH_OSTaxAmountInfo.ReadOnly);
				AssertEquals("AH_OSTaxAmount must be zero as it is readonly", 0m, invoice.AH_OSTaxAmount);

				org.CompanyData.SetAPTaxApplicable(true);
				invoice.AH_OH = org.PK;
				invoice.TaxRate = ZGuid.Empty;
				invoice.AH_OSTaxAmount = 14m;

				((TransactionHeaderValidation)invoice.Validation).ValidateAH_OSTaxAmount();
				AssertEquals("AH_OSTaxAmount must be read only because Tax Rate is NOT entered", true, invoice.AH_OSTaxAmountInfo.ReadOnly);
				AssertEquals("AH_OSTaxAmount must be zero as it is readonly", 0m, invoice.AH_OSTaxAmount);
			}
			finally
			{
				GlbCompany.CurrentCompany.GC_IsGSTRegistered = oldIsGSTRegistered;
			}
		}

		public void TestCheckExchangeRateAfterCurrencyChanged()
		{
			RefCurrency currentCurrency = GlbCompany.CurrentCompany.LocalCurrency;
			RefCurrency anotherCurrency = Factory.LoadTop1<RefCurrency>(new ZQuery(RefCurrencySchema.RX_Code, SQLComparisonOperator.NotEqual, currentCurrency.RX_Code));
			APInvoiceForBulkPoster aPInvoiceForBulkPoster = GetNewBusinessObject() as APInvoiceForBulkPoster;
			aPInvoiceForBulkPoster.ExchangeRate.Currency = anotherCurrency.RX_Code;
			AssertEquals(ZBool.False, aPInvoiceForBulkPoster.ExchangeRate.IsRateReadOnly);
			aPInvoiceForBulkPoster.ExchangeRate.Rate = 2.0M;
			AssertEquals(2.0M, aPInvoiceForBulkPoster.AH_ExchangeRate);
			aPInvoiceForBulkPoster.ExchangeRate.Currency = currentCurrency.RX_Code;
			AssertEquals(ZBool.True, aPInvoiceForBulkPoster.ExchangeRate.IsRateReadOnly);
			AssertEquals("Exchange rate should be equal to 1 when currency is equal to company currency", 1.0M, aPInvoiceForBulkPoster.AH_ExchangeRate);
		}

		public void TestAmountsAfterCurrencyChanged()
		{
			ZBool oldIsGSTRegistered = GlbCompany.CurrentCompany.GC_IsGSTRegistered;
			try
			{
				GlbCompany.CurrentCompany.GC_IsGSTRegistered = ZBool.True;

				OrgHeader org = Factory.New<OrgHeader>();
				org.OH_Code = "TestOrg";
				org.CompanyData.SetAPTaxApplicable(true);

				AccTaxRate taxRate = Factory.NewWithValidTestData<AccTaxRate>();
				taxRate.AT_Code = "Test";
				taxRate.AT_RN_NKCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
				taxRate.SetRateNumerator_ForTestOnly(10);
				Factory.Save();

				APInvoiceForBulkPoster invoice = GetNewBusinessObject() as APInvoiceForBulkPoster;
				invoice.AH_OH = org.PK;
				invoice.TaxRate = taxRate.PK;
				invoice.AH_RX_NKTransactionCurrency = "AUD";
				invoice.AH_OSExTaxAmount = 100M;
				invoice.AH_OSTaxAmount = 10M;
				AssertEquals("Check AH_OSExTaxAmount before curency is changed", 100M, invoice.AH_OSExTaxAmount);
				AssertEquals("Check AH_OSTaxAmount before curency changed", 10M, invoice.AH_OSTaxAmount);

				invoice.AH_RX_NKTransactionCurrency = "USD";
				AssertEquals("AH_OSExTaxAmount must be set to 0 after currency was changed", 0M, invoice.AH_OSExTaxAmount);
				AssertEquals("AH_OSTaxAmount must be set to 0 after currency was changed", 0M, invoice.AH_OSTaxAmount);
			}
			finally
			{
				GlbCompany.CurrentCompany.GC_IsGSTRegistered = oldIsGSTRegistered;
			}
		}

		public void TestLocalAmountsAfterExchangeRateChanged()
		{
			ZBool oldIsGSTRegistered = GlbCompany.CurrentCompany.GC_IsGSTRegistered;
			try
			{
				GlbCompany.CurrentCompany.GC_IsGSTRegistered = ZBool.True;

				OrgHeader org = Factory.New<OrgHeader>();
				org.OH_Code = "TestOrg";
				org.CompanyData.SetAPTaxApplicable(true);

				AccTaxRate taxRate = Factory.NewWithValidTestData<AccTaxRate>();
				taxRate.AT_Code = "Test";
				taxRate.AT_RN_NKCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
				taxRate.SetRateNumerator_ForTestOnly(10);
				Factory.Save();

				APInvoiceForBulkPoster invoice = GetNewBusinessObject() as APInvoiceForBulkPoster;
				invoice.AH_OH = org.PK;
				invoice.AH_RX_NKTransactionCurrency = "USD";
				invoice.AH_ExchangeRate = 1M;
				invoice.TaxRate = taxRate.PK;
				InvoicingLineBase line = (InvoicingLineBase)invoice.Lines.AddNew();
				line.AL_AT = taxRate.PK;
				line.AL_OSExTaxAmount = 100M;
				AssertEquals("Local Charge Amount must be the same as OS Charge Amount as Invoice.AH_ExchangeRate = 1", 100M, invoice.AH_LocalExTaxAmount);
				AssertEquals("Local Tax Amount must be the same as OS Tax Amount as Invoice.AH_ExchangeRate = 1", 10M, invoice.AH_LocalTaxAmount);

				invoice.AH_ExchangeRate = 0.5M;
				AssertEquals("Local Charge Amount must be equal to 2*AH_OSExTaxAmount  as Invoice.AH_ExchangeRate = 0.5", 200M, invoice.AH_LocalExTaxAmount);
				AssertEquals("Local Tax Amount must be equal to 2*AH_OSTaxAmount as Invoice.AH_ExchangeRate = 0.5", 20M, invoice.AH_LocalTaxAmount);

				invoice.AH_ExchangeRate = 4M;
				AssertEquals("Local Charge Amount must be equal to AH_OSExTaxAmount/4  as Invoice.AH_ExchangeRate = 4", 25M, invoice.AH_LocalExTaxAmount);
				AssertEquals("Local Tax Amount must be equal to AH_OSTaxAmount/4 as Invoice.AH_ExchangeRate = 4", 2.5M, invoice.AH_LocalTaxAmount);
			}
			finally
			{
				GlbCompany.CurrentCompany.GC_IsGSTRegistered = oldIsGSTRegistered;
			}
		}

		public void TestTaxRate()
		{
			ZBool oldIsGSTRegistered = GlbCompany.CurrentCompany.GC_IsGSTRegistered;
			try
			{
				GlbCompany.CurrentCompany.GC_IsGSTRegistered = ZBool.True;

				OrgHeader org = Factory.New<OrgHeader>();
				org.OH_Code = "TestOrg";
				org.CompanyData.SetAPTaxApplicable(true);

				AccTaxRate test1TaxRate = Factory.NewWithValidTestData<AccTaxRate>();
				test1TaxRate.AT_Code = "Test1";
				test1TaxRate.AT_RN_NKCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
				test1TaxRate.SetRateNumerator_ForTestOnly(10);

				AccTaxRate test2TaxRate = Factory.NewWithValidTestData<AccTaxRate>();
				test2TaxRate.AT_Code = "Test2";
				test2TaxRate.AT_RN_NKCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
				test2TaxRate.SetRateNumerator_ForTestOnly(0);
				Factory.Save();

				APInvoiceForBulkPoster invoice = GetNewBusinessObject() as APInvoiceForBulkPoster;
				invoice.AH_OH = org.PK;
				invoice.TaxRate = test1TaxRate.PK;
				invoice.AH_OSExTaxAmount = 100M;
				invoice.AH_OSTaxAmount = 10M;

				AssertEquals("TaxRate must be Test1TaxRate PK", test1TaxRate.PK, invoice.TaxRate);
				AssertEquals("Invoice.AH_OSTaxAmount should be equal to 10", 10m, invoice.AH_OSTaxAmount);

				invoice.TaxRate = test2TaxRate.PK;

				AssertEquals("TaxRate must be Test2TaxRate PK", test2TaxRate.PK, invoice.TaxRate);
				AssertEquals("Invoice.AH_OSTaxAmount should be equal to 0 as TaxRate Rate  is equal to 0", 0m, invoice.AH_OSTaxAmount);

				invoice.TaxRate = test1TaxRate.PK;
				invoice.AH_OSTaxAmount = 20M;

				AssertEquals("TaxRate must be Test1TaxRate PK", test1TaxRate.PK, invoice.TaxRate);
				AssertEquals("Invoice.AH_OSTaxAmount should be equal to 20", 20m, invoice.AH_OSTaxAmount);

				invoice.AH_OH = ZGuid.Empty;
				AssertEquals("TaxRate must be readonly", true, invoice.TaxRateInfo.ReadOnly);
				AssertEquals("TaxRate must be empty", ZGuid.Empty, invoice.TaxRate);
				AssertEquals("Invoice.AH_OSTaxAmount should be equal to 0", 0m, invoice.AH_OSTaxAmount);
			}
			finally
			{
				GlbCompany.CurrentCompany.GC_IsGSTRegistered = oldIsGSTRegistered;
			}
		}

		public void TestTaxDate()
		{
			GlbCompany.CurrentCompany.GC_IsGSTRegistered = ZBool.True;

			OrgHeader org = Factory.New<OrgHeader>();
			org.OH_Code = "TestOrg";
			org.CompanyData.SetAPTaxApplicable(true);

			Factory.Save();

			APInvoiceForBulkPoster invoice = GetNewBusinessObject() as APInvoiceForBulkPoster;
			invoice.AH_OH = org.PK;
			invoice.TaxRate = TestObjectCreator.GST1WithDates.PK;
			invoice.AH_OSExTaxAmount = 100M;
			invoice.AH_OSTaxAmount = 10M;

			AssertEquals(false, invoice.TaxDateInfo.ReadOnly);
			AssertEquals(TestObjectCreator.GST1WithDates.PK, invoice.TaxRate);
			AssertEquals(10m, invoice.AH_OSTaxAmount);
			AssertEquals(ZDate.Today, invoice.TaxDate);

			invoice.TaxDate = TestObjectCreator.GST1WithDates_DateWithNoRate;

			AssertEquals(0m, invoice.AH_OSTaxAmount);
			AssertEquals(TestObjectCreator.GST1WithDates_DateWithNoRate, invoice.TaxDate);

			invoice.TaxDate = ZDate.Today.AddDays(5);
			invoice.AH_OSTaxAmount = 10M;

			AssertEquals(10m, invoice.AH_OSTaxAmount);
			AssertEquals(ZDate.Today.AddDays(5), invoice.TaxDate);

			invoice.AH_OH = ZGuid.Empty;
			AssertEquals(true, invoice.TaxDateInfo.ReadOnly);
			AssertEquals(ZGuid.Empty, invoice.TaxRate);
			AssertEquals(0m, invoice.AH_OSTaxAmount);
		}

		public void TestDefaultIsCashInvoiceCore()
		{
			var objectCreator = new TestObjectCreator(Factory);
			var fesDepartment = Factory.LoadFromNaturalKey<GlbDepartment>(GlbDepartmentSchema.GE_Code, "FES");
			var org = objectCreator.Creditor1;
			var job = objectCreator.CreateJob(org, 0, null, 0);
			var accrual01 = objectCreator.CreateAccrual(job, objectCreator.CC1, 1, "S00001001", 110, 100, 10);
			accrual01.AL_GE = fesDepartment.PK;
			Factory.Save();

			var poster = objectCreator.CreateAPBulkInvoicePoster(new[] { accrual01 }, true);
			var invoice = objectCreator.CreateAPInvoiceForBulkPoster(poster, "1", 100m, 10m, org);
			AssertEquals("DefaultIsCashInvoice", false, invoice.DefaultIsCashInvoice);

			org.MiscServ.OM_APPayInvoiceAfterPostingDefault = true;
			Factory.Save();
			invoice = objectCreator.CreateAPInvoiceForBulkPoster(poster, "2", 100m, 10m, org);
			AssertEquals("DefaultIsCashInvoice", false, invoice.DefaultIsCashInvoice);
		}

		[TestedType(typeof(APInvoiceForBulkPoster))]
		public class APInvoiceForBulkPosterMatchingTest : InvoicingBaseMatchingTest
		{
			protected override InvoicingBase GetNewInvoice()
			{
				return Factory.New<APInvoiceForBulkPoster>();
			}
		}

		public void TestValidationType()
		{
			APInvoiceForBulkPoster invoice = GetNewBusinessObject() as APInvoiceForBulkPoster;
			AssertEquals(typeof(APInvoiceForBulkPosterValidation), invoice.Validation.GetType());
		}

		protected override void SetUp()
		{
			base.SetUp();
			TestObjectCreator = new TestObjectCreator(Factory);
		}

		TestObjectCreator TestObjectCreator;

		protected override bool ShouldExpectTaxTotal => true;

		protected override bool IsInvoiceSupportingTaxAmount => false;
	}
}
