using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Accounting.Business.WIPAccrual;
using Enterprise.Accounting.Registry.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using static Enterprise.Core.Constants;
using static Enterprise.Registry.Business.ComplianceSubTypeCodesAndLists.CodesAndDescriptions;

namespace Enterprise.Accounting.Business.ARAP.Invoicing.Testing
{
	[TestedType(typeof(APBulkInvoicePoster))]
	public class APBulkInvoicePosterTest : NonPersistentBusinessObjectTestCase
	{
		public void TestApportionAccruals_TaxDate()
		{
			var org = Factory.New<OrgHeader>();
			org.OH_Code = "TestOrg";
			org.CompanyData.SetAPTaxApplicable(true);
			var taxRate = Factory.NewWithValidTestData<AccTaxRate>();
			taxRate.AT_Code = "Test";
			taxRate.AT_RN_NKCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			taxRate.SetRateNumerator_ForTestOnly(10);
			Factory.Save();
			var jH_S00001001 = TestObjectCreator.CreateJob(org, 0, null, 0);
			var jH_S00001002 = TestObjectCreator.CreateJob(org, 0, null, 0);

			var accrual01 = TestObjectCreator.CreateAccrual(jH_S00001001, TestObjectCreator.CC1, 1, "S00001001", 0, 100);
			var accrual02 = TestObjectCreator.CreateAccrual(jH_S00001002, TestObjectCreator.CC1, 1, "S00001002", 0, 200);
			Factory.Save();

			var invoice1 = TestObjectCreator.CreateAPInvoiceForBulkPoster(typeof(APInvoiceForBulkPoster), "1", TestObjectCreator.AUD, 1m, 800m, 80m, 0m, 800m, 80m, 0m, taxRate.PK, org.PK);
			var invoice2 = TestObjectCreator.CreateAPInvoiceForBulkPoster(typeof(APInvoiceForBulkPoster), "2", TestObjectCreator.USD, 0.8995m, 1079.40m, 50m, 0, 1200m, 55.59m, 0, taxRate.PK, org.PK);
			var expectedDate = ZDate.Today.AddDays(-4);
			invoice2.TaxDate = expectedDate;

			var poster = new APBulkInvoicePoster(Factory);
			poster.Accruals.Add(accrual01);
			poster.Invoices.Add(invoice1);
			poster.Invoices.Add(invoice2);

			poster.ApportionAccruals();

			foreach (APInvoiceForBulkPoster invoice in poster.Invoices)
			{
				AssertEquals(String.Format("Number of lines in Invoice {0}.", invoice.AH_TransactionNum), 2, invoice.Lines.Count);
			}

			AssertEquals(taxRate.PK, invoice1.Lines[1].AL_AT);
			AssertEquals(ZDate.Today, invoice1.Lines[1].AL_TaxDate);
			AssertEquals(taxRate.PK, invoice2.Lines[1].AL_AT);
			AssertEquals(expectedDate, invoice2.Lines[1].AL_TaxDate);
		}

		[SuspendCriticalValidation]
		public void TestApportionAccruals_ComplianceErrors()
		{
			var currComp = GlbCompany.CurrentCompany;
			using (currComp.TemporarilySetCountry(CountryCodes.Italy))
			{
				var newFactory = new BusinessObjectFactory();
				const string subType = "APS";
				var sequence = newFactory.NewWithValidTestData<AccComplianceSequence>();
				sequence.XD_SequenceClass = subType;
				sequence.XD_Code = "APS1";
				sequence.XD_StartNumber = 1;
				sequence.XD_EndNumber = 99;
				sequence.XD_NextNumber = 25;
				sequence.XD_MaximumNumberDigits = 9;
				sequence.XD_GC_Company = currComp.PK;
				sequence.XD_GB_BranchOwner = GlbBranch.CurrentBranch.PK;
				sequence.XD_Prefix = "APS-";

				Factory.Save();

				var collection = new ComplianceSubTypeAttributionRuleConfigurationCollection();
				var config = collection.AddNew();
				config.Country = currComp.Country.Code;
				config.SubType = subType;
				config.LedgerType = LedgerTypes.AccountsPayable;
				config.InvoiceType = TransactionTypes.Invoice;
				config.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsAnAmountOfTax;
				config.DisbursementRule = DisbursementRuleCodes.AllTransactions;
				config.OriginalRule = OriginalRuleCodes.AllTransactions;

				var org = Factory.New<OrgHeader>();
				org.OH_Code = "TestOrg";
				org.OH_IsCreditor = true;
				org.CompanyData.SetAPTaxApplicableIgnoringRegistrySetting(true);

				var taxRate = TestObjectCreator.CreateTaxRate("Test", "Not reportable", AccTaxRate.Types.NotReportable, 10, ZString.Empty, 0, 1, currComp.GC_RN_NKCountryCode);

				Factory.Save();

				var jH_S00001001 = TestObjectCreator.CreateJob(org, 0, null, 0);
				var accrual01 = TestObjectCreator.CreateAccrual(jH_S00001001, TestObjectCreator.CC1, 1, "S00001001", 0, 100);
				var fesDepartment = Factory.LoadFromNaturalKey<GlbDepartment>(GlbDepartmentSchema.GE_Code, "FES");
				accrual01.AL_GE = fesDepartment.PK;
				Factory.Save();

				var poster = TestObjectCreator.CreateAPBulkInvoicePoster(new[] { accrual01 }, true);
				var invoice1 = TestObjectCreator.CreateAPInvoiceForBulkPoster(typeof(APInvoiceForBulkPoster), "1", TestObjectCreator.EUR, 1m, 800m, 80m, 0m, 800m, 80m, 0m, taxRate.PK, org.PK);
				poster.Invoices.Add(invoice1);

				var registry = AccountingMasterFilesRegistry.Instance;
				var currCompGuid = currComp.PK.ToGuid();
				using (registry.ComplianceSubTypeAttributionRuleConfiguration.SetTemporaryValue(currCompGuid, Guid.Empty, Guid.Empty, collection))
				using (registry.ComplianceNumberAllocationDate_AP.SetTemporaryValue(currCompGuid, Guid.Empty, Guid.Empty, AccountingMasterFilesConstants.ComplianceNumberAllocationDateOptions.PostDate.Code))
				using (registry.ComplianceDocumentNumberAllocation_Payables.SetTemporaryValue(currCompGuid, Guid.Empty, Guid.Empty, AccComplianceSequenceLookups.ComplianceDocumentNumberAllocationSettingCodes.Post))
				{
					Assert(currComp.Country.SupportComplianceSubType);

					sequence.XD_ExpiryDate = ZDateTime.Today.AddDays(-1);
					newFactory.Save();
					AssertExceptionThrown(typeof(APBulkInvoiceComplianceSequenceRelatedException), () => Factory.Save());
					AssertHasRowErrorContaining(invoice1, ComplianceSequenceNumberAllocationErrorMessages.FailedToFindComplianceSequenceMessage);

					invoice1.RemoveRowError(ComplianceSequenceNumberAllocationErrorMessages.FailedToFindComplianceSequenceMessage);

					sequence.XD_ExpiryDate = ZDateTime.Empty;
					newFactory.Save();
					AssertNoExceptionThrown(() => Factory.Save());
					AssertNoRowErrorContaining(invoice1, "Compliance");
				}
			}
		}

		public void TestApportionAccruals()
		{
			// Setup
			bool oldIsGSTRegistered = GlbCompany.CurrentCompany.GC_IsGSTRegistered;
			try
			{
				GlbCompany.CurrentCompany.GC_IsGSTRegistered = true;

				OrgHeader org = Factory.New<OrgHeader>();
				org.OH_Code = "TestOrg";
				org.CompanyData.SetAPTaxApplicable(true);
				AccTaxRate taxRate = Factory.NewWithValidTestData<AccTaxRate>();
				taxRate.AT_Code = "Test";
				taxRate.AT_RN_NKCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
				taxRate.SetRateNumerator_ForTestOnly(10);
				Factory.Save();
				Job jH_S00001001 = TestObjectCreator.CreateJob(org, 0, null, 0);
				Job jH_S00001002 = TestObjectCreator.CreateJob(org, 0, null, 0);
				Job jH_S00001003 = TestObjectCreator.CreateJob(org, 0, null, 0);
				Job jH_S00001004 = TestObjectCreator.CreateJob(org, 0, null, 0);
				Job jH_S00001005 = TestObjectCreator.CreateJob(org, 0, null, 0);

				Accrual accrual01 = TestObjectCreator.CreateAccrual(jH_S00001001, TestObjectCreator.CC1, 1, "S00001001", 0, 100);
				Accrual accrual02 = TestObjectCreator.CreateAccrual(jH_S00001002, TestObjectCreator.CC1, 1, "S00001002", 0, 200);
				Accrual accrual03 = TestObjectCreator.CreateAccrual(jH_S00001003, TestObjectCreator.CC1, 1, "S00001003", 0, 300);
				Accrual accrual04 = TestObjectCreator.CreateAccrual(jH_S00001004, TestObjectCreator.CC1, 1, "S00001004", 0, 400);
				Accrual accrual05 = TestObjectCreator.CreateAccrual(jH_S00001005, TestObjectCreator.CC1, 1, "S00001005", 0, 500);
				Accrual accrual06 = TestObjectCreator.CreateAccrual(jH_S00001001, TestObjectCreator.CC2, 1, "S00001001", 0, 600);
				Accrual accrual07 = TestObjectCreator.CreateAccrual(jH_S00001002, TestObjectCreator.CC2, 1, "S00001002", 0, 700);
				Accrual accrual08 = TestObjectCreator.CreateAccrual(jH_S00001003, TestObjectCreator.CC2, 1, "S00001003", 0, 800);
				Accrual accrual09 = TestObjectCreator.CreateAccrual(jH_S00001004, TestObjectCreator.CC2, 1, "S00001004", 0, 900);
				Accrual accrual10 = TestObjectCreator.CreateAccrual(jH_S00001005, TestObjectCreator.CC2, 1, "S00001005", 0, 1000);
				accrual01.AL_GE = accrual02.AL_GE = accrual03.AL_GE = accrual04.AL_GE = accrual05.AL_GE = TestObjectCreator.NonCurrentDepartment.PK;

				Accrual accrual01_bad = TestObjectCreator.CreateAccrual(jH_S00001001, TestObjectCreator.CC1, 1, "S00001001", 0, 100);
				Accrual accrual02_bad = TestObjectCreator.CreateAccrual(jH_S00001002, TestObjectCreator.CC1, 1, "S00001002", 0, 200);
				Accrual accrual03_bad = TestObjectCreator.CreateAccrual(jH_S00001003, TestObjectCreator.CC1, 1, "S00001003", 0, 300);
				Accrual accrual04_bad = TestObjectCreator.CreateAccrual(jH_S00001004, TestObjectCreator.CC1, 1, "S00001004", 0, 400);
				Accrual accrual05_bad = TestObjectCreator.CreateAccrual(jH_S00001005, TestObjectCreator.CC1, 1, "S00001005", 0, 500);
				Accrual accrual06_bad = TestObjectCreator.CreateAccrual(jH_S00001001, TestObjectCreator.CC2, 1, "S00001001", 0, 600);
				Accrual accrual07_bad = TestObjectCreator.CreateAccrual(jH_S00001002, TestObjectCreator.CC2, 1, "S00001002", 0, 700);
				Accrual accrual08_bad = TestObjectCreator.CreateAccrual(jH_S00001003, TestObjectCreator.CC2, 1, "S00001003", 0, 800);
				Accrual accrual09_bad = TestObjectCreator.CreateAccrual(jH_S00001004, TestObjectCreator.CC2, 1, "S00001004", 0, 900);
				Accrual accrual10_bad = TestObjectCreator.CreateAccrual(jH_S00001005, TestObjectCreator.CC2, 1, "S00001005", 0, 1000);

				accrual01_bad.ShouldReverese = false;
				accrual02_bad.ShouldReverese = false;
				accrual03_bad.ShouldReverese = false;
				accrual04_bad.ShouldReverese = false;
				accrual05_bad.ShouldReverese = false;
				accrual06_bad.ShouldReverese = false;
				accrual07_bad.ShouldReverese = false;
				accrual08_bad.ShouldReverese = false;
				accrual09_bad.ShouldReverese = false;
				accrual10_bad.ShouldReverese = false;

				APInvoiceForBulkPoster invoice1 = TestObjectCreator.CreateAPInvoiceForBulkPoster(typeof(APInvoiceForBulkPoster), "1", TestObjectCreator.AUD, 1m, 800m, 80m, 0m, 800m, 80m, 0m, taxRate.PK, org.PK);
				APInvoiceForBulkPoster invoice2 = TestObjectCreator.CreateAPInvoiceForBulkPoster(typeof(APInvoiceForBulkPoster), "2", TestObjectCreator.USD, 0.8995m, 1079.40m, 50m, 0, 1200m, 55.59m, 0, taxRate.PK, org.PK);
				APInvoiceForBulkPoster invoice3 = (APInvoiceForBulkPoster)(TestObjectCreator.CreateAPInvoice<APInvoiceForBulkPoster>("3", TestObjectCreator.AUD, 1m, 1500m, 0, 0, 1500m, 0, 0));
				APInvoiceForBulkPoster invoice4 = (APInvoiceForBulkPoster)(TestObjectCreator.CreateAPInvoice<APInvoiceForBulkPoster>("4", TestObjectCreator.AUD, 1m, 900m, 0, 0, 900m, 0, 0));
				APInvoiceForBulkPoster invoice5 = (APInvoiceForBulkPoster)(TestObjectCreator.CreateAPInvoice<APInvoiceForBulkPoster>("5", TestObjectCreator.USD, 0.9m, 990m, 0, 0, 1100m, 0, 0));

				Factory.Save();

				APBulkInvoicePoster poster = new APBulkInvoicePoster(Factory);
				poster.Accruals.Add(accrual01);
				poster.Accruals.Add(accrual02);
				poster.Accruals.Add(accrual03);
				poster.Accruals.Add(accrual04);
				poster.Accruals.Add(accrual05);
				poster.Accruals.Add(accrual06);
				poster.Accruals.Add(accrual07);
				poster.Accruals.Add(accrual08);
				poster.Accruals.Add(accrual09);
				poster.Accruals.Add(accrual10);
				poster.Accruals.Add(accrual01_bad);
				poster.Accruals.Add(accrual02_bad);
				poster.Accruals.Add(accrual03_bad);
				poster.Accruals.Add(accrual04_bad);
				poster.Accruals.Add(accrual05_bad);
				poster.Accruals.Add(accrual06_bad);
				poster.Accruals.Add(accrual07_bad);
				poster.Accruals.Add(accrual08_bad);
				poster.Accruals.Add(accrual09_bad);
				poster.Accruals.Add(accrual10_bad);
				poster.Invoices.Add(invoice1);
				poster.Invoices.Add(invoice2);
				poster.Invoices.Add(invoice3);
				poster.Invoices.Add(invoice4);
				poster.Invoices.Add(invoice5);

				poster.ApportionAccruals();

				foreach (APInvoiceForBulkPoster invoice in poster.Invoices)
				{
					AssertEquals(String.Format("Number of lines in Invoice {0}.", invoice.AH_TransactionNum), 10, invoice.Lines.Count);
				}

				AssertAccrualsAreApportionedCorrectly(poster.Invoices, new ZDecimal[] { 100, 200, 300, 400, 500, 600, 700, 800, 900, 1000 });

				AssertInvoiceLines(invoice1, new ZDecimal[] { 14.55, 29.09, 43.64, 58.18, 72.73, 87.27, 101.82, 116.36, 130.91, 145.45 }, 800m, new ZDecimal[] { 1.46, 2.91, 4.36, 5.82, 7.27, 8.73, 10.18, 11.64, 13.09, 14.54 }, 80m);
				AssertInvoiceLines(invoice2, new ZDecimal[] { 19.63, 39.25, 58.88, 78.50, 98.13, 117.75, 137.38, 157.00, 176.63, 196.25 }, 1079.4m, new ZDecimal[] { 0.91, 1.82, 2.73, 3.64, 4.55, 5.45, 6.36, 7.27, 8.18, 9.09 }, 50m);
				AssertInvoiceLines(invoice3, new ZDecimal[] { 27.27, 54.54, 81.81, 109.10, 136.36, 163.64, 190.90, 218.19, 245.46, 272.73 }, 1500m, new ZDecimal[] { 0.00, 0.00, 0.00, 0.00, 0.00, 0.00, 0.00, 0.00, 0.00, 0.00 }, 0m);
				AssertInvoiceLines(invoice4, new ZDecimal[] { 16.36, 32.73, 49.09, 65.45, 81.82, 98.18, 114.55, 130.91, 147.27, 163.64 }, 900m, new ZDecimal[] { 0.00, 0.00, 0.00, 0.00, 0.00, 0.00, 0.00, 0.00, 0.00, 0.00 }, 0m);
				AssertInvoiceLines(invoice5, new ZDecimal[] { 18.00, 36.00, 54.00, 72.00, 90.00, 108.00, 126.00, 144.00, 162.00, 180.00 }, 990m, new ZDecimal[] { 0.00, 0.00, 0.00, 0.00, 0.00, 0.00, 0.00, 0.00, 0.00, 0.00 }, 0m);
			}
			finally
			{
				GlbCompany.CurrentCompany.GC_IsGSTRegistered = oldIsGSTRegistered;
			}
		}

		public void TestApportionAccrualsPerformanceOptimisation()
		{
			GlbCompany.CurrentCompany.GC_IsGSTRegistered = true;

			BusinessObjectFactory newFactory = new BusinessObjectFactory();

			OrgHeader org = newFactory.New<OrgHeader>();
			org.OH_Code = "TestOrg";
			org.CompanyData.SetAPTaxApplicable(true);

			OrgHeader sellOrg = newFactory.New<OrgHeader>();
			sellOrg.OH_Code = "SellTestOrg";
			sellOrg.CompanyData.SetAPTaxApplicable(true);

			AccTaxRate taxRate = newFactory.NewWithValidTestData<AccTaxRate>();
			taxRate.AT_Code = "Test";
			taxRate.AT_RN_NKCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			taxRate.SetRateNumerator_ForTestOnly(10);

			newFactory.Save();

			Job jH_S00001001 = TestObjectCreator.CreateJob(org, 0, null, 0);

			Accrual accrual01 = TestObjectCreator.CreateAccrual(jH_S00001001, TestObjectCreator.CC1, 1, "S00001001", 0, 100);
			Accrual accrual02 = TestObjectCreator.CreateAccrual(jH_S00001001, TestObjectCreator.CC1, 1, "S00001001", 0, 200);
			Accrual accrual03 = TestObjectCreator.CreateAccrual(jH_S00001001, TestObjectCreator.CC1, 1, "S00001001", 0, 300);
			Accrual accrual04 = TestObjectCreator.CreateAccrual(jH_S00001001, TestObjectCreator.CC1, 1, "S00001001", 0, 400);
			Accrual accrual05 = TestObjectCreator.CreateAccrual(jH_S00001001, TestObjectCreator.CC1, 1, "S00001001", 0, 500);

			APInvoiceForBulkPoster invoice1 = TestObjectCreator.CreateAPInvoiceForBulkPoster(typeof(APInvoiceForBulkPoster), "1", TestObjectCreator.AUD, 1m, 100m, 10m, 0m, 100m, 10m, 0m, taxRate.PK, org.PK);

			Factory.Save();

			APBulkInvoicePoster poster = new APBulkInvoicePoster(Factory);

			poster.Accruals.Add(accrual01);
			poster.Accruals.Add(accrual02);
			poster.Accruals.Add(accrual03);
			poster.Accruals.Add(accrual04);
			poster.Accruals.Add(accrual05);
			poster.Invoices.Add(invoice1);

			accrual01.RelatedJobCharge.JR_OH_SellAccount = sellOrg.PK;

			var hitsBefore = Factory.GetTableHitCount(OrgMiscServSchema.Constants.TableName);

			poster.ApportionAccruals();

			var hitsAfter = Factory.GetTableHitCount(OrgMiscServSchema.Constants.TableName);

			AssertEquals("Should not be any hits to the OrgMiscServ as a part of updating ReadOnly status on properties because those GUI related actions are suspended", hitsBefore, hitsAfter);
		}

		public void TestApportionDifference()
		{
			// Setup
			bool oldIsGSTRegistered = GlbCompany.CurrentCompany.GC_IsGSTRegistered;
			try
			{
				GlbCompany.CurrentCompany.GC_IsGSTRegistered = true;

				OrgHeader org = Factory.New<OrgHeader>();
				org.OH_Code = "TestOrg";
				org.CompanyData.SetAPTaxApplicable(true);
				AccTaxRate taxRate = Factory.NewWithValidTestData<AccTaxRate>();
				taxRate.AT_Code = "Test";
				taxRate.AT_RN_NKCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
				taxRate.SetRateNumerator_ForTestOnly(10);
				Factory.Save();
				Job jH_S00001001 = TestObjectCreator.CreateJob(org, 0, null, 0);
				Job jH_S00001002 = TestObjectCreator.CreateJob(org, 0, null, 0);
				Job jH_S00001003 = TestObjectCreator.CreateJob(org, 0, null, 0);
				Job jH_S00001004 = TestObjectCreator.CreateJob(org, 0, null, 0);
				Job jH_S00001005 = TestObjectCreator.CreateJob(org, 0, null, 0);

				Accrual accrual01 = TestObjectCreator.CreateAccrual(jH_S00001001, TestObjectCreator.CC1, 1, "S00001001", 0, 100);
				Accrual accrual02 = TestObjectCreator.CreateAccrual(jH_S00001002, TestObjectCreator.CC1, 1, "S00001002", 0, 200);
				Accrual accrual03 = TestObjectCreator.CreateAccrual(jH_S00001003, TestObjectCreator.CC1, 1, "S00001003", 0, 300);
				Accrual accrual04 = TestObjectCreator.CreateAccrual(jH_S00001004, TestObjectCreator.CC1, 1, "S00001004", 0, 400);
				Accrual accrual05 = TestObjectCreator.CreateAccrual(jH_S00001005, TestObjectCreator.CC1, 1, "S00001005", 0, 500);
				Accrual accrual06 = TestObjectCreator.CreateAccrual(jH_S00001001, TestObjectCreator.CC2, 1, "S00001001", 0, 600);
				Accrual accrual07 = TestObjectCreator.CreateAccrual(jH_S00001002, TestObjectCreator.CC2, 1, "S00001002", 0, 700);
				Accrual accrual08 = TestObjectCreator.CreateAccrual(jH_S00001003, TestObjectCreator.CC2, 1, "S00001003", 0, 800);
				Accrual accrual09 = TestObjectCreator.CreateAccrual(jH_S00001004, TestObjectCreator.CC2, 1, "S00001004", 0, 900);
				Accrual accrual10 = TestObjectCreator.CreateAccrual(jH_S00001005, TestObjectCreator.CC2, 1, "S00001005", 0, 1000);
				accrual01.AL_GE = accrual02.AL_GE = accrual03.AL_GE = accrual04.AL_GE = accrual05.AL_GE = TestObjectCreator.NonCurrentDepartment.PK;

				Accrual accrual01_bad = TestObjectCreator.CreateAccrual(jH_S00001001, TestObjectCreator.CC1, 1, "S00001001", 0, 100);
				Accrual accrual02_bad = TestObjectCreator.CreateAccrual(jH_S00001002, TestObjectCreator.CC1, 1, "S00001002", 0, 200);
				Accrual accrual03_bad = TestObjectCreator.CreateAccrual(jH_S00001003, TestObjectCreator.CC1, 1, "S00001003", 0, 300);
				Accrual accrual04_bad = TestObjectCreator.CreateAccrual(jH_S00001004, TestObjectCreator.CC1, 1, "S00001004", 0, 400);
				Accrual accrual05_bad = TestObjectCreator.CreateAccrual(jH_S00001005, TestObjectCreator.CC1, 1, "S00001005", 0, 500);
				Accrual accrual06_bad = TestObjectCreator.CreateAccrual(jH_S00001001, TestObjectCreator.CC2, 1, "S00001001", 0, 600);
				Accrual accrual07_bad = TestObjectCreator.CreateAccrual(jH_S00001002, TestObjectCreator.CC2, 1, "S00001002", 0, 700);
				Accrual accrual08_bad = TestObjectCreator.CreateAccrual(jH_S00001003, TestObjectCreator.CC2, 1, "S00001003", 0, 800);
				Accrual accrual09_bad = TestObjectCreator.CreateAccrual(jH_S00001004, TestObjectCreator.CC2, 1, "S00001004", 0, 900);
				Accrual accrual10_bad = TestObjectCreator.CreateAccrual(jH_S00001005, TestObjectCreator.CC2, 1, "S00001005", 0, 1000);

				accrual01_bad.ShouldReverese = false;
				accrual02_bad.ShouldReverese = false;
				accrual03_bad.ShouldReverese = false;
				accrual04_bad.ShouldReverese = false;
				accrual05_bad.ShouldReverese = false;
				accrual06_bad.ShouldReverese = false;
				accrual07_bad.ShouldReverese = false;
				accrual08_bad.ShouldReverese = false;
				accrual09_bad.ShouldReverese = false;
				accrual10_bad.ShouldReverese = false;

				APInvoiceForBulkPoster invoice1 = TestObjectCreator.CreateAPInvoiceForBulkPoster(typeof(APInvoiceForBulkPoster), "1", TestObjectCreator.AUD, 1m, 810m, 80m, 0m, 810m, 80m, 0m, taxRate.PK, org.PK);
				APInvoiceForBulkPoster invoice2 = TestObjectCreator.CreateAPInvoiceForBulkPoster(typeof(APInvoiceForBulkPoster), "2", TestObjectCreator.USD, 0.8995m, 1079.40m, 50m, 0m, 1200m, 55.59m, 0m, taxRate.PK, org.PK);
				APInvoiceForBulkPoster invoice3 = (APInvoiceForBulkPoster)(TestObjectCreator.CreateAPInvoice<APInvoiceForBulkPoster>("3", TestObjectCreator.AUD, 1m, 1500m, 0, 0m, 1500m, 0, 0m));
				APInvoiceForBulkPoster invoice4 = (APInvoiceForBulkPoster)(TestObjectCreator.CreateAPInvoice<APInvoiceForBulkPoster>("4", TestObjectCreator.AUD, 1m, 900m, 0, 0m, 900m, 0, 0m));
				APInvoiceForBulkPoster invoice5 = (APInvoiceForBulkPoster)(TestObjectCreator.CreateAPInvoice<APInvoiceForBulkPoster>("5", TestObjectCreator.USD, 0.9m, 990m, 0, 0m, 1100m, 0, 0m));

				Factory.Save();

				APBulkInvoicePoster poster = new APBulkInvoicePoster(Factory);
				poster.Accruals.Add(accrual01);
				poster.Accruals.Add(accrual02);
				poster.Accruals.Add(accrual03);
				poster.Accruals.Add(accrual04);
				poster.Accruals.Add(accrual05);
				poster.Accruals.Add(accrual06);
				poster.Accruals.Add(accrual07);
				poster.Accruals.Add(accrual08);
				poster.Accruals.Add(accrual09);
				poster.Accruals.Add(accrual10);
				poster.Accruals.Add(accrual01_bad);
				poster.Accruals.Add(accrual02_bad);
				poster.Accruals.Add(accrual03_bad);
				poster.Accruals.Add(accrual04_bad);
				poster.Accruals.Add(accrual05_bad);
				poster.Accruals.Add(accrual06_bad);
				poster.Accruals.Add(accrual07_bad);
				poster.Accruals.Add(accrual08_bad);
				poster.Accruals.Add(accrual09_bad);
				poster.Accruals.Add(accrual10_bad);
				poster.Invoices.Add(invoice1);
				poster.Invoices.Add(invoice2);
				poster.Invoices.Add(invoice3);
				poster.Invoices.Add(invoice4);
				poster.Invoices.Add(invoice5);

				poster.ApportionAccruals();

				AssertAccrualsAreApportionedCorrectly(poster.Invoices, new ZDecimal[] { 100, 200, 300, 400, 500, 600, 700, 800, 900, 1000 });

				AssertInvoiceLines(invoice1, new ZDecimal[] { 14.70, 29.40, 44.10, 58.80, 73.50, 88.20, 102.90, 117.60, 132.30, 147.01, 1.49 }, 810.00m, new ZDecimal[] { 1.45, 2.90, 4.36, 5.81, 7.26, 8.71, 10.16, 11.61, 13.07, 14.52, 0.15 }, 80m);
				AssertInvoiceLines(invoice2, new ZDecimal[] { 19.59, 39.18, 58.77, 78.36, 97.95, 117.54, 137.13, 156.72, 176.31, 195.90, 1.95 }, 1079.40m, new ZDecimal[] { 0.91, 1.82, 2.72, 3.63, 4.54, 5.45, 6.35, 7.26, 8.17, 9.06, 0.09 }, 50m);
				AssertInvoiceLines(invoice3, new ZDecimal[] { 27.22, 54.45, 81.67, 108.88, 136.12, 163.35, 190.57, 217.79, 245.00, 272.23, 2.72 }, 1500m, new ZDecimal[] { 0.00, 0.00, 0.00, 0.00, 0.00, 0.00, 0.00, 0.00, 0.00, 0.00, 0.00 }, 0m);
				AssertInvoiceLines(invoice4, new ZDecimal[] { 16.33, 32.67, 49.00, 65.34, 81.67, 98.00, 114.34, 130.67, 147.01, 163.34, 1.63 }, 900m, new ZDecimal[] { 0.00, 0.00, 0.00, 0.00, 0.00, 0.00, 0.00, 0.00, 0.00, 0.00, 0.00 }, 0m);
				AssertInvoiceLines(invoice5, new ZDecimal[] { 17.97, 35.93, 53.90, 71.87, 89.84, 107.80, 125.77, 143.74, 161.71, 179.67, 1.80 }, 990m, new ZDecimal[] { 0.00, 0.00, 0.00, 0.00, 0.00, 0.00, 0.00, 0.00, 0.00, 0.00, 0.00 }, 0m);
			}
			finally
			{
				GlbCompany.CurrentCompany.GC_IsGSTRegistered = oldIsGSTRegistered;
			}
		}

		public void TestValidateRetrievedAccrualTotal()
		{
			TestPoster.ValidateRetrievedAccrualTotal();
			AssertHasError(TestPoster.RetrievedAccrualTotalInfo, "Amount can not equal 0.");
		}
		public void TestTotalLocalExTaxAmount()
		{
			APInvoiceForBulkPoster invoice1 = Factory.New<APInvoiceForBulkPoster>();
			invoice1.AH_LocalExTaxAmount = 100m;
			invoice1.AH_LocalTaxAmount = 50m;
			APInvoiceForBulkPoster invoice2 = Factory.New<APInvoiceForBulkPoster>();
			invoice2.AH_LocalExTaxAmount = 400m;
			invoice2.AH_LocalTaxAmount = 200m;
			TestPoster.Invoices.Add(invoice1);
			TestPoster.Invoices.Add(invoice2);
			AssertEquals("TotalLocalExTaxAmount must be sum of AH_LocalExTaxAmount", 500m, TestPoster.TotalLocalExTaxAmount);
		}

		public void TestValidateTotalLocalExTaxAmount()
		{
			TestPoster.ValidateTotalLocalExTaxAmount();
			AssertHasError(TestPoster.TotalLocalExTaxAmountInfo, "Amount can not equal 0.");
		}

		public void TestTotalOSAmount()
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

				APInvoiceForBulkPoster invoice1 = Factory.New<APInvoiceForBulkPoster>();
				invoice1.AH_OH = org.PK;
				invoice1.TaxRate = taxRate.PK;
				invoice1.AH_OSExTaxAmount = 10m;
				invoice1.AH_OSTaxAmount = 5m;
				APInvoiceForBulkPoster invoice2 = Factory.New<APInvoiceForBulkPoster>();
				invoice2.AH_OH = org.PK;
				invoice2.TaxRate = taxRate.PK;
				invoice2.AH_OSExTaxAmount = 40m;
				invoice2.AH_OSTaxAmount = 20m;
				TestPoster.Invoices.Add(invoice1);
				TestPoster.Invoices.Add(invoice2);
				AssertEquals("TotalOSTaxAmount must be sum of AH_OSTaxAmount", 25m, TestPoster.TotalOSTaxAmount);
				AssertEquals("TotalOSExTaxAmount must be sum of AH_OSExTaxAmount", 50m, TestPoster.TotalOSExTaxAmount);
				AssertEquals("TotalOSAmount must be sum of AH_OSExTaxAmount and AH_OSTaxAmount", 75m, TestPoster.TotalOSAmount);
			}
			finally
			{
				GlbCompany.CurrentCompany.GC_IsGSTRegistered = oldIsGSTRegistered;
			}
		}
		public void TestValidateDifference()
		{
			int oldValue = AccountingConfigurationRegistry.Instance.MaxAccrualVsActualDiscrepancies.Value;
			try
			{
				AccountingConfigurationRegistry.Instance.MaxAccrualVsActualDiscrepancies.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, 15);
				fTestPoster = null;
				APInvoiceForBulkPoster invoice1 = (APInvoiceForBulkPoster)(TestObjectCreator.CreateInvoiceWithLine(typeof(APInvoiceForBulkPoster), "1", TestObjectCreator.AUD, 1m, 10m, 0, 10m, 0));
				TestPoster.Invoices.Add(invoice1);

				TestPoster.ValidateDifference();
				AssertHasWarning(TestPoster.DifferenceInfo, "Invoices and Accruals totals do not match.");

				invoice1.AH_LocalExTaxAmount = 20;
				TestPoster.ValidateDifference();
				AssertHasError(TestPoster.DifferenceInfo, "Difference between Invoices and Accruals totals exceeds maximum value set.");
			}
			finally
			{
				AccountingConfigurationRegistry.Instance.MaxAccrualVsActualDiscrepancies.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, oldValue);
			}
		}

		public void TestValidateExpectedBatchTotal()
		{
			fTestPoster = null;
			APInvoiceForBulkPoster invoice1 = (APInvoiceForBulkPoster)(TestObjectCreator.CreateInvoiceWithLine(typeof(APInvoiceForBulkPoster), "1", TestObjectCreator.AUD, 1m, 10m, 0, 10m, 0));
			TestPoster.Invoices.Add(invoice1);

			TestPoster.ValidateExpectedBatchTotal();
			AssertHasError(TestPoster.ExpectedBatchTotalInfo, "Expected Batch Total should equal the AP Invoice Total.");
		}

		public void TestRetrievedAccrualTotal()
		{
			var job = TestObjectCreator.CreateJobHeader();

			var accrual1 = TestObjectCreator.CreateAccrual(job);
			accrual1.AL_LocalExTaxAmount = 100M;
			accrual1.ShouldReverese = true;

			var accrual2 = TestObjectCreator.CreateAccrual(job);
			accrual2.AL_LocalExTaxAmount = 200M;
			accrual2.ShouldReverese = false;

			var accrual3 = TestObjectCreator.CreateAccrual(job);
			accrual3.AL_LocalExTaxAmount = 300M;
			accrual3.ShouldReverese = true;

			TestPoster.Accruals.Add(accrual1);
			TestPoster.Accruals.Add(accrual2);
			TestPoster.Accruals.Add(accrual3);
			TestPoster.UpdateRetrievedAccrualsTotal();
			AssertEquals("RetrievedAccrualsTotal should be calculated only for accruals with ShouldReverese = true",
				400M, TestPoster.RetrievedAccrualTotal);

			accrual1.ShouldReverese = false;
			AssertEquals("RetrievedAccrualsTotal should be recalculated on changing ShouldReverese value",
				300M, TestPoster.RetrievedAccrualTotal);
		}

		public void TestValidationForPaymentDoesnotGetInvokedDuringSaving()
		{
			GlbDepartment fesDepartment = Factory.LoadFromNaturalKey<GlbDepartment>(GlbDepartmentSchema.GE_Code, "FES");
			var org = TestObjectCreator.Creditor1;
			org.MiscServ.OM_APPayInvoiceAfterPostingDefault = true;
			Job job = TestObjectCreator.CreateJob(org, 0, null, 0);
			Accrual accrual01 = TestObjectCreator.CreateAccrual(job, TestObjectCreator.CC1, 1, "S00001001", 110, 100, 10);
			accrual01.AL_GE = fesDepartment.PK;
			Factory.Save();

			var poster = TestObjectCreator.CreateAPBulkInvoicePoster(new[] { accrual01 }, true);
			var invoice = TestObjectCreator.CreateAPInvoiceForBulkPoster(poster, "1", 100m, 10m, org);
			poster.RunPreSaveValidation();

			AssertNoError(invoice.ReceiptPaymentAH_ABInfo, "Please enter a Bank.");
			AssertNoError(invoice.ReceiptPaymentAH_ChequeOrReferenceInfo, "Please enter a value.");
			AssertNoError(invoice.ReceiptPaymentAK_ABInfo, "Please enter a Check Book.");

			var invoice2 = TestObjectCreator.CreateAPInvoice<APInvoice>("2", TestObjectCreator.AUD, 1.0M, 100m, 10m, 0m, 100m, 10m, 0m);
			invoice2.SubmittedFromInvoicingForm = true;
			invoice2.AH_OH = org.PK;
			invoice2.RunPreSaveValidation();
			AssertHasError(invoice2.ReceiptPaymentAH_ABInfo, "Please enter a Bank.");
			AssertHasError(invoice2.ReceiptPaymentAH_ChequeOrReferenceInfo, "Please enter a value.");
			AssertHasError(invoice2.ReceiptPaymentAK_ABInfo, "Please enter a Check Book.");
		}

		#region Implementation

		void AssertInvoiceLines(APInvoiceForBulkPoster invoice, ZDecimal[] aL_OSExTaxAmounts, ZDecimal aH_OSExTaxAmount, ZDecimal[] aL_OSTaxAmounts, ZDecimal aH_OSTaxAmount)
		{
			ZDecimal aL_OSExTaxAmount = 0;
			ZDecimal aL_OSTaxAmount = 0;
			for (int i = 0; i < invoice.Lines.Count; i++)
			{
				aL_OSExTaxAmount += aL_OSExTaxAmounts[i];
				aL_OSTaxAmount += aL_OSTaxAmounts[i];
				AssertEquals(String.Format("Invoice {0}, line {1}: AL_OSExTaxAmount", invoice.AH_TransactionNum, i), aL_OSExTaxAmounts[i], invoice.Lines[i].AL_OSExTaxAmount);
				AssertEquals(String.Format("Invoice {0}, line {1}: AL_OSTaxAmount", invoice.AH_TransactionNum, i), aL_OSTaxAmounts[i], invoice.Lines[i].AL_OSTaxAmount);
			}
			AssertEquals(String.Format("Invoice {0}: total of AL_OSExTaxAmount", invoice.AH_TransactionNum), aH_OSExTaxAmount, aL_OSExTaxAmount);
			AssertEquals(String.Format("Invoice {0}: total of AL_OSTaxAmount", invoice.AH_TransactionNum), aH_OSTaxAmount, aL_OSTaxAmount);
		}

		void AssertAccrualsAreApportionedCorrectly(APInvoiceForBulkPosterCollection invoices, ZDecimal[] accrualAmounts)
		{
			for (int i = 0; i < accrualAmounts.Length; i++)
			{
				ZDecimal aL_LocalExTaxAmount = 0;
				foreach (APInvoiceForBulkPoster invoice in invoices)
				{
					aL_LocalExTaxAmount += invoice.Lines[i].AL_LocalExTaxAmount;
				}
				AssertEquals("Amount of Accrual doesn't match total of corresponding invoice lines", accrualAmounts[i], aL_LocalExTaxAmount);
			}

			foreach (APInvoiceForBulkPoster invoice in invoices)
			{
				var firstLineDepartment = invoice.Lines[0].Department; // Will be used on Discrepancy line
				foreach (InvoicingLineBase line in invoice.Lines)
				{
					var expectedDepartment = line.AL_AC.IsValid ? (line.AL_AC == TestObjectCreator.CC1.PK ? TestObjectCreator.NonCurrentDepartment : GlbDepartment.CurrentDepartment) : // Apportioned Line
																	firstLineDepartment; // Discrepancy Line
					AssertEquals(string.Format("Invoice {0}, Line {1}. Expected Department {2} but was {3}", invoice.AH_TransactionNum, line.AL_Sequence, expectedDepartment.GE_Code, line.Department.GE_Code), expectedDepartment.PK, line.Department.PK);
				}
			}
		}

		TestObjectCreator fTestObjectCreator;
		TestObjectCreator TestObjectCreator
		{
			get { return fTestObjectCreator ?? (fTestObjectCreator = new TestObjectCreator(Factory)); }
		}

		APBulkInvoicePoster fTestPoster;
		APBulkInvoicePoster TestPoster
		{
			get { return fTestPoster ?? (fTestPoster = new APBulkInvoicePoster(Factory)); }
		}

		#endregion
	}
}
