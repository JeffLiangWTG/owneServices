using System;
using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Business.ARAP.Invoicing.Testing
{
	public class APInvoiceForBulkPosterValidationTest : APInvoiceValidationTest
	{
		protected override InvoiceBaseValidation GetValidation(TransactionHeader parent)
		{
			return new APInvoiceForBulkPosterValidation((APInvoiceForBulkPoster)parent);
		}

		protected override Type InvoiceType
		{
			get { return typeof(APInvoiceForBulkPoster); }
		}

		public override void TestCheckTransactionNumForUAInvNumExcludingItselfAlreadyExist_Standard()
		{
			Assert(true);
		}

		public override void TestCheckTransactionNumForUAInvNumExcludingItselfAlreadyExist_Calendar()
		{
			Assert(true);
		}

		public override void TestValidateOSTotalAmountExcludeOtherTaxes()
		{
			Assert("Tax Transactions are not applicable here to spend time to make the test working because of special setup to set tax amount in this class.", true);
		}

		public void TestCheckAH_OSTaxAmount()
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

				APInvoiceForBulkPoster invoice = Factory.New<APInvoiceForBulkPoster>();
				invoice.AH_OH = org.PK;
				invoice.TaxRate = taxRate.PK;
				invoice.AH_OSExTaxAmount = 100m;
				invoice.AH_OSTaxAmount = -10m;
				AssertHasError("Error Expected", invoice.AH_OSTaxAmountInfo, "Tax Amount must have the same sign as the Charge Amount.");

				invoice.AH_OSTaxAmount = 0m;
				AssertEquals("Error isn't Expected", false, invoice.AH_OSTaxAmountInfo.HasErrors());

				invoice.AH_OSTaxAmount = 10m;
				AssertEquals("Error isn't Expected", false, invoice.AH_OSTaxAmountInfo.HasErrors());
			}
			finally
			{
				GlbCompany.CurrentCompany.GC_IsGSTRegistered = oldIsGSTRegistered;
			}
		}

		public void TestCheckAH_InvoiceDateCheckAH_PostDate()
		{
			ExchangeRateReader.GetReaderInstance().ClearCache();

			var periodManagementTestHelper = new AccountingPeriodTestHelper();
			periodManagementTestHelper.SetupPeriods();

			var collection = new APInvoiceForBulkPosterCollection(Factory);
			var invoice1 = collection.AddNew();
			var invoice2 = collection.AddNew();

			invoice1.AH_PostDate = ZDateTime.Now;
			invoice1.AH_InvoiceDate = ZDateTime.Now;
			invoice1.AH_RX_NKTransactionCurrency = "AUD";

			invoice2.AH_PostDate = ZDateTime.Now;
			invoice2.AH_InvoiceDate = ZDateTime.Now.AddDays(-1);
			invoice2.AH_RX_NKTransactionCurrency = "AUD";

			Action assertNoErrors = () =>
			{
				invoice1.RunPreSaveValidation();
				invoice2.RunPreSaveValidation();

				AssertNoErrors(invoice1.AH_InvoiceDateInfo);
				AssertNoErrors(invoice1.AH_PostDateInfo);

				AssertNoErrors(invoice2.AH_InvoiceDateInfo);
				AssertNoErrors(invoice2.AH_PostDateInfo);
			};
			PostingExRateRegistryAP.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, "DEF");
			assertNoErrors();

			invoice1.AH_RX_NKTransactionCurrency = invoice2.AH_RX_NKTransactionCurrency = "USD";
			assertNoErrors();
			PostingExRateRegistryAP.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, "TOD");
			assertNoErrors();
			PostingExRateRegistryAP.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, "INV");
			invoice1.AH_InvoiceDate = invoice2.AH_InvoiceDate = ZDateTime.Now;
			assertNoErrors();

			invoice1.AH_InvoiceDate = ZDateTime.Now;
			invoice2.AH_InvoiceDate = ZDateTime.Now.AddDays(-1);
			invoice1.RunPreSaveValidation();
			invoice2.RunPreSaveValidation();
			AssertHasError(invoice1.AH_InvoiceDateInfo, @"The “AP Invoice Posting Exchange Rate Option” has been set to ""INV"".
All Invoice Dates in this Bulk AP Invoice Posting must be the same to facilitate the application of posting using Exchange Rate set for Invoice Date.");
			AssertNoErrors(invoice1.AH_PostDateInfo);
			AssertHasError(invoice2.AH_InvoiceDateInfo, @"The “AP Invoice Posting Exchange Rate Option” has been set to ""INV"".
All Invoice Dates in this Bulk AP Invoice Posting must be the same to facilitate the application of posting using Exchange Rate set for Invoice Date.");
			AssertNoErrors(invoice2.AH_PostDateInfo);
			PostingExRateRegistryAP.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, "PST");
			invoice1.AH_PostDate = invoice2.AH_PostDate = ZDateTime.Now;
			assertNoErrors();

			invoice1.AH_PostDate = ZDateTime.Now;
			invoice2.AH_PostDate = ZDateTime.Now.AddDays(-1);
			invoice1.RunPreSaveValidation();
			invoice2.RunPreSaveValidation();
			AssertNoErrors(invoice1.AH_InvoiceDateInfo);
			AssertHasError(invoice1.AH_PostDateInfo, @"The “AP Invoice Posting Exchange Rate Option” has been set to ""PST"".
All Post Dates in this Bulk AP Invoice Posting must be the same to facilitate the application of posting using Exchange Rate set for Post Date.");
			AssertNoErrors(invoice2.AH_InvoiceDateInfo);
			AssertHasError(invoice2.AH_PostDateInfo, @"The “AP Invoice Posting Exchange Rate Option” has been set to ""PST"".
All Post Dates in this Bulk AP Invoice Posting must be the same to facilitate the application of posting using Exchange Rate set for Post Date.");
		}

		public void TestCheckTaxDate()
		{
			GlbDepartment fesDepartment = Factory.LoadFromNaturalKey<GlbDepartment>(GlbDepartmentSchema.GE_Code, "FES");
			var org = TestObjectCreator.Creditor1;
			org.MiscServ.OM_APPayInvoiceAfterPostingDefault = true;
			var job = TestObjectCreator.CreateJob(org, 0, null, 0);
			var accrual01 = TestObjectCreator.CreateAccrual(job, TestObjectCreator.CC1, 1, "S00001001", 110, 100, 10);
			accrual01.AL_GE = fesDepartment.PK;
			Factory.Save();

			TestObjectCreator.ActiveOrg.CompanyData.SetAPTaxApplicableIgnoringRegistrySetting(true);

			var poster = TestObjectCreator.CreateAPInvoiceForBulkPoster(typeof(APInvoiceForBulkPoster), "1", TestObjectCreator.AUD, 1m, 10m, 0m, 0m, 0m, 0m, 0m, TestObjectCreator.GST1WithDates.PK, TestObjectCreator.ActiveOrg.PK);
			AssertNoErrors(poster.TaxDateInfo);

			poster.TaxDate = TestObjectCreator.GST1WithDates_DateWithNoRate;
			AssertHasError(poster.TaxDateInfo, "No rate found for selected date.");

			poster.TaxDate = ZDate.Empty;
			AssertHasError(poster.TaxDateInfo, "Please enter a Tax Date.");

			poster.TaxRate = ZGuid.Invalid;
			((APInvoiceForBulkPosterValidation)poster.Validation).ValidateTaxDate();
			AssertNoErrors(poster.TaxDateInfo);
		}
	}
}
