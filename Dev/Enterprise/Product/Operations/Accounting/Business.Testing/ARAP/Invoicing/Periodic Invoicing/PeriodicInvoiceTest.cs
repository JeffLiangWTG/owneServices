using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing.Periodic_Invoicing;
using Enterprise.Accounting.Business.ARAP.Invoicing.Printing;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Core;
using Enterprise.DocumentEngine;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CountryCompliance;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;
using static Enterprise.Core.Constants;

namespace Enterprise.Accounting.Business.ARAP.Invoicing.Testing
{
	[TestedType(typeof(PeriodicInvoice))]
	public class PeriodicInvoiceTest : PeriodicInvoiceBaseTest
	{
		public void TestPreparePeriodicInvoiceInNewFactory()
		{
			var (periodicInvoice, jobs, _) = CreatePeriodicInvoiceWithJobsAndMiscInvoices(jobCount: 3);
			periodicInvoice.Jobs[0].IncludeInThePeriodicInvoice = true;
			periodicInvoice.Jobs[1].IncludeInThePeriodicInvoice = false;
			periodicInvoice.Jobs[2].IncludeInThePeriodicInvoice = true;

			var periodicInvoiceInNewFactory = periodicInvoice.PreparePeriodicInvoiceInNewFactoryForAuthorizationCheck();
			AssertNotEquals(periodicInvoice.Factory, periodicInvoiceInNewFactory.Factory);
			AssertEquals(periodicInvoice.DebtorPK, periodicInvoiceInNewFactory.DebtorPK);
			AssertEquals(periodicInvoice.PostDate, periodicInvoiceInNewFactory.PostDate);
			AssertEquals(periodicInvoice.InvoiceType, periodicInvoiceInNewFactory.InvoiceType);
			AssertEquals(periodicInvoice.InvoiceDate, periodicInvoiceInNewFactory.InvoiceDate);
			AssertEquals(periodicInvoice.CurrencyNK, periodicInvoiceInNewFactory.CurrencyNK);
			AssertEquals(2, periodicInvoiceInNewFactory.Jobs.Count);
			periodicInvoiceInNewFactory.Jobs.Select(x => x.PK).ContainsSameElementsInAnyOrder(new ZGuid[] { jobs[0].PK, jobs[2].PK });

			Assert("Factory context should be absent because PreSaveValidation hasn't run", !periodicInvoiceInNewFactory.Factory.HasContext(BusinessContext.PeriodicInvoiceHasAlreadyBeenValidatedInAnotherFactory));
			Assert("Validation should be active because PreSaveValidation hasn't run", !periodicInvoiceInNewFactory.Factory.IsValidationSuspended);

			periodicInvoice.RunPreSaveValidation();
			Assert("Precondition: no validation errors.", !periodicInvoice.HasErrors);

			var periodicInvoiceInNewFactory_AfterGoodValidation = periodicInvoice.PreparePeriodicInvoiceInNewFactoryForAuthorizationCheck();
			Assert("Factory context should be present because PreSaveValidation has run", periodicInvoiceInNewFactory_AfterGoodValidation.Factory.HasContext(BusinessContext.PeriodicInvoiceHasAlreadyBeenValidatedInAnotherFactory));
			Assert("Validation should be suspended because PreSaveValidation has run", periodicInvoiceInNewFactory_AfterGoodValidation.Factory.IsValidationSuspended);

			periodicInvoice.Debtor.CompanyData.InvoiceTypes.RemoveAndDeleteAll();
			Factory.Save();

			periodicInvoice.RunPreSaveValidation();
			Assert("Precondition: has validation errors.", periodicInvoice.HasErrors);

			var periodicInvoiceInNewFactory_AfterBadValidation = periodicInvoice.PreparePeriodicInvoiceInNewFactoryForAuthorizationCheck();
			Assert("Factory context should be absent because PreSaveValidation failed", !periodicInvoiceInNewFactory_AfterBadValidation.Factory.HasContext(BusinessContext.PeriodicInvoiceHasAlreadyBeenValidatedInAnotherFactory));
			Assert("Validation should be active because PreSaveValidation failed", !periodicInvoiceInNewFactory_AfterBadValidation.Factory.IsValidationSuspended);
		}

		public void TestCreateTransactions()
		{
			var periodHelper = new AccountingPeriodTestHelper(new BusinessObjectFactory());
			periodHelper.SetupPeriods();

			var testObjectCreator = new TestObjectCreator(Factory);

			var shipment = testObjectCreator.CreateJobPlugIn(JobInvoicingConsumerTypes.Shipment);
			var job = testObjectCreator.CreateJob(shipment, testObjectCreator.ABIGAS, 10, testObjectCreator.ZECTRA, 10);
			job.JH_UniqueJobInvoiceNumber = 6;

			var orgInvoiceType = testObjectCreator.ABIGAS.CompanyData.InvoiceTypes.AddNew();
			orgInvoiceType.PI_Module = JobInvoicingConsumerTypes.Shipment.Code;
			orgInvoiceType.PI_Interval = InvoiceTypeBillingInterval.Codes.MTH;
			orgInvoiceType.PI_StartDay = InvoiceTypeMonthCommencement.Codes.LMH;
			orgInvoiceType.PI_Type = InvoiceTypeLayoutList.Codes.INV;
			orgInvoiceType.PI_RS_NKServiceLevel = "STD";

			var orgInvTypeDeferredCharges = orgInvoiceType.DeferredCharges.AddNew();
			orgInvTypeDeferredCharges.PO_AC = testObjectCreator.MRG100.PK;

			Factory.Save();

			var charge = testObjectCreator.CreateCharge(job, testObjectCreator.MRG100, "Test Charge", null, 0, null, testObjectCreator.AUD, 300, testObjectCreator.ABIGAS);
			charge.JR_InvoiceType = InvoiceTypesList.Codes.DestinationChargesInvoice_Batching;
			charge.JR_APInvoiceDate = charge.JR_PaymentDate = ZDateTime.Empty;

			Factory.Save();

			var periodicInvoice = new PeriodicInvoice(Factory);
			periodicInvoice.CurrencyNK = "AUD";
			periodicInvoice.DebtorPK = testObjectCreator.ABIGAS.PK;
			periodicInvoice.InvoiceType = InvoiceTypesList.Codes.DestinationChargesInvoice_Batching;
			periodicInvoice.LoadJobs();

			Assert(periodicInvoice.CreateTransactions());
			AssertNotNull(periodicInvoice.PostManager.Poster.PostedInvoices);
			AssertEquals(1, periodicInvoice.PostManager.Poster.PostedInvoices.Count);
		}

		public void TestValidationFlag_IsTrue_AfterPreSaveValidation()
		{
			var (periodicInvoice, _, _) = CreatePeriodicInvoiceWithJobsAndMiscInvoices(jobCount: 0);
			AssertEquals("Precondition: validation has not yet been run", false, periodicInvoice.ValidationHasRun);

			periodicInvoice.RunPreSaveValidation();
			AssertEquals("A flag should indicate when validation has run", true, periodicInvoice.ValidationHasRun);
		}

		public void TestValidationFlag_IsFalse_AfterLoadJobs()
		{
			var (periodicInvoice, _, _) = CreatePeriodicInvoiceWithJobsAndMiscInvoices(jobCount: 1);
			periodicInvoice.RunPreSaveValidation();
			AssertEquals("Precondition: validation has run", true, periodicInvoice.ValidationHasRun);

			periodicInvoice.LoadJobs();
			AssertEquals("Loading jobs can influence the validation result, so the flag should be false", false, periodicInvoice.ValidationHasRun);
		}

		public void TestValidationFlag_IsFalse_AfterMiscInvoices()
		{
			var (periodicInvoice, _, _) = CreatePeriodicInvoiceWithJobsAndMiscInvoices(jobCount: 0, invoiceCount: 1);
			periodicInvoice.RunPreSaveValidation();
			AssertEquals("Precondition: validation has run", true, periodicInvoice.ValidationHasRun);

			periodicInvoice.LoadMiscInvoices();
			AssertEquals("Loading invoices can influence the validation result, so the flag should be false", false, periodicInvoice.ValidationHasRun);
		}

		public void TestValidationFlag_WhenChargeIncludedFlagChanged()
		{
			var (periodicInvoice, _, _) = CreatePeriodicInvoiceWithJobsAndMiscInvoices(jobCount: 1);
			periodicInvoice.RunPreSaveValidation();
			AssertEquals("Precondition: validation has run", true, periodicInvoice.ValidationHasRun);

			periodicInvoice.Jobs[0].IncludeInThePeriodicInvoice = !periodicInvoice.Jobs[0].IncludeInThePeriodicInvoice;
			AssertEquals("Changing IncludeInThePeriodicInvoice can influence the validation result, so the flag should be false", false, periodicInvoice.ValidationHasRun);
		}

		public void TestValidationFlag_WhenMiscInvoiceIncludedFlagChanged()
		{
			var (periodicInvoice, _, _) = CreatePeriodicInvoiceWithJobsAndMiscInvoices(jobCount: 0, invoiceCount: 1);
			periodicInvoice.RunPreSaveValidation();
			AssertEquals("Precondition: validation has run", true, periodicInvoice.ValidationHasRun);

			periodicInvoice.MiscInvoices[0].IncludeInThePeriodicInvoice = !periodicInvoice.MiscInvoices[0].IncludeInThePeriodicInvoice;
			AssertEquals("Changing IncludeInThePeriodicInvoice can influence the validation result, so the flag should be false", false, periodicInvoice.ValidationHasRun);
		}

		public void TestPostManagerValidationDoesNotRun_InDifferentFactory_WhenValidationAlreadyRanInAnotherFactory()
		{
			var (periodicInvoice, jobs, _) = CreatePeriodicInvoiceWithJobsAndMiscInvoices(jobCount: 1);
			periodicInvoice.Jobs[0].IncludeInThePeriodicInvoice = true;

			periodicInvoice.RunPreSaveValidation();

			periodicInvoice.Debtor.CompanyData.InvoiceTypes.RemoveAndDeleteAll();  // This should generate a validation error.
			periodicInvoice.Debtor.OH_IsDebtor = false;
			Factory.Save();

			var periodicInvoiceInNewFactory = periodicInvoice.PreparePeriodicInvoiceInNewFactoryForAuthorizationCheck();

			var postManagerError = string.Empty;
			periodicInvoiceInNewFactory.PostManager.OnCriticalPostError += new EventHandler<CriticalPostingErrorEventArgs>((sender, e) => postManagerError = e.ErrorMessage);
			var result = periodicInvoiceInNewFactory.CreateTransactions();

			CombineAssertions("PostManager should not run validation because validation was already run in different factory bizo", () =>
			{
				Assert(result);
				AssertNullOrEmpty(postManagerError);
			});
		}

		public void TestPostManagerValidationDoesRun_InDifferentFactory_WhenValidationWasNotRunInAnotherFactory()
		{
			var (periodicInvoice, jobs, _) = CreatePeriodicInvoiceWithJobsAndMiscInvoices(jobCount: 1);
			periodicInvoice.Jobs[0].IncludeInThePeriodicInvoice = true;

			periodicInvoice.Debtor.CompanyData.InvoiceTypes.RemoveAndDeleteAll();  // This should generate a validation error.
			periodicInvoice.Debtor.OH_IsDebtor = false;
			Factory.Save();

			var periodicInvoiceInNewFactory = periodicInvoice.PreparePeriodicInvoiceInNewFactoryForAuthorizationCheck();

			var postManagerError = string.Empty;
			periodicInvoiceInNewFactory.PostManager.OnCriticalPostError += new EventHandler<CriticalPostingErrorEventArgs>((sender, e) => postManagerError = e.ErrorMessage);
			var result = periodicInvoiceInNewFactory.CreateTransactions();

			CombineAssertions("PostManager should run validation because validation was not run in different factory bizo", () =>
			{
				Assert(!result);
				AssertNotNullOrEmpty(postManagerError);
			});
		}

		public void TestDefaultTaxBranch()
		{
			using (AccountingMasterFilesRegistry.Instance.EnableTaxBranchReporting.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, true))
			{
				var currentBranchPK = GlbBranch.CurrentBranch.PK;

				Assert("TaxBranch is empty", TestPeriodicInvoice.TaxBranch.IsEmpty);

				TestObjectCreator.TestOrganisation.CompanyData.SetARTaxApplicable(true);
				TestPeriodicInvoice.DebtorPK = TestObjectCreator.TestOrganisation.PK;

				AssertEquals("TaxBranch should be default to current branch", currentBranchPK, TestPeriodicInvoice.TaxBranch);

				TestObjectCreator.TestOrganisation.CompanyData.SetARTaxApplicable(false);
				TestPeriodicInvoice.DebtorPK = ZGuid.Empty;
				TestPeriodicInvoice.DebtorPK = TestObjectCreator.TestOrganisation.PK;
				Assert("TaxBranch should be default to empty", TestPeriodicInvoice.TaxBranch.IsEmpty);
			}
		}

		public void TestClearJobsAfterTaxBranchChanged()
		{
			using (var job = TestObjectCreator.CreateJob(TestObjectCreator.CreateShipment("S001")))
			{
				var charge1 = job.Charges.AddNew();
				var charge2 = job.Charges.AddNew();

				TestPeriodicInvoice.Jobs.Add(job);
				TestPeriodicInvoice.Charges.Add(charge1);
				TestPeriodicInvoice.Charges.Add(charge2);

				AssertEquals(1, TestPeriodicInvoice.Jobs.Count);

				TestPeriodicInvoice.TaxBranch = TestObjectCreator.NonCurrentBranch.PK;

				AssertEquals("Jobs should be cleared after tax branch changed.", 0, TestPeriodicInvoice.Jobs.Count);
			}
		}

		public void TestFilterChargesAndJobsAfterTaxBranchChanged()
		{
			var branch1 = TestObjectCreator.CreateBranch("001", GlbCompany.CurrentCompany);
			var branch2 = TestObjectCreator.CreateBranch("002", GlbCompany.CurrentCompany);

			TestObjectCreator.CreateTestPeriods(ZDateTime.Today);
			TestObjectCreator.CreateJobShipmentWithFIDCharge("S001", TestObjectCreator.LocalClient, TestObjectCreator.CC1, 100M, 1000M, true);
			TestObjectCreator.CreateJobShipmentWithFIDCharge("S002", TestObjectCreator.LocalClient, TestObjectCreator.CC1, 200M, 2000M, true);

			var newFactory = new BusinessObjectFactory();
			var bulkPeriodicInvoice = new PeriodicInvoiceBulk(newFactory);
			bulkPeriodicInvoice.CurrencyNK = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;
			bulkPeriodicInvoice.LoadJobs();
			TestObjectCreator.LocalClient.CompanyData.ClearInvoiceTypeCache_ForTestOnly();

			AssertEquals(1, bulkPeriodicInvoice.PeriodicInvoices.Count);

			var periodicInvoice = bulkPeriodicInvoice.PeriodicInvoices[0];
			AssertEquals(4, periodicInvoice.Charges.Count);
			AssertEquals(2, periodicInvoice.Jobs.Count);
			AssertEquals(6600M, periodicInvoice.LocalTotalAmount);
			AssertEquals(6600M, bulkPeriodicInvoice.LocalTotalAmount);

			var job1 = periodicInvoice.Jobs.Cast<PeriodicInvoiceSelectableJob>().First(x => x.JH_JobNum == "S001").Parent;
			AssertEquals(2, job1.Charges.Count);

			var job2 = periodicInvoice.Jobs.Cast<PeriodicInvoiceSelectableJob>().First(x => x.JH_JobNum == "S002").Parent;
			AssertEquals(2, job2.Charges.Count);

			var charge1 = job1.Charges[0];
			var charge2 = job1.Charges[1];
			charge1.JR_GB_SellTaxBranch = branch1.PK;
			charge2.JR_GB_SellTaxBranch = branch2.PK;

			newFactory.Save();

			periodicInvoice.TaxBranch = branch1.PK;

			AssertEquals(1, periodicInvoice.Charges.Count);
			AssertEquals(1, periodicInvoice.Jobs.Count);
			AssertEquals(1100M, periodicInvoice.LocalTotalAmount);
			AssertEquals(1100M, bulkPeriodicInvoice.LocalTotalAmount);

			periodicInvoice.TaxBranch = ZGuid.Empty;

			AssertEquals(4, periodicInvoice.Charges.Count);
			AssertEquals(2, periodicInvoice.Jobs.Count);

			periodicInvoice.TaxBranch = ZGuid.BrettsGuid;

			AssertEquals(0, periodicInvoice.Charges.Count);
			AssertEquals(0, periodicInvoice.Jobs.Count);
		}

		public void TestTaxBranch_ReadOnly()
		{
			using (TestObjectCreator.SetUpTaxBranchRegistry(true))
			{
				TestObjectCreator.ResetSecurityCore();

				AssertTaxBranchReadOnly(true, true);
				AssertTaxBranchReadOnly(false, true);
				AssertTaxBranchReadOnly(true, false);
				AssertTaxBranchReadOnly(false, false);

				void AssertTaxBranchReadOnly(bool isTaxApplicable, bool isSecurityAllowed)
				{
					Env.Security.NewReceivablesOverrideTaxBranchAllows.IsAllowed = isSecurityAllowed;

					TestPeriodicInvoice.DebtorPK = ZGuid.Empty;
					TestPeriodicInvoice.TaxBranch = GlbBranch.CurrentBranch.PK;
					TestObjectCreator.TestOrganisation.CompanyData.SetARTaxApplicable(isTaxApplicable);
					TestPeriodicInvoice.DebtorPK = TestObjectCreator.TestOrganisation.PK;

					var expectedReadOnly = !isTaxApplicable || !isSecurityAllowed;

					AssertEquals(expectedReadOnly, TestPeriodicInvoice.TaxBranchInfo.ReadOnly);
				}
			}
		}

		public void TestInvoiceTerm_ReadOnly()
		{
			AccountingConfigurationRegistry.Instance.AllowModifyInvoiceTerm.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			Env.Security.NewReceivablesPeriodicInvoiceTerm.IsAllowed = true;
			AssertEquals(false, TestPeriodicInvoice.InvoiceTermInfo.ReadOnly);

			AccountingConfigurationRegistry.Instance.AllowModifyInvoiceTerm.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			Env.Security.NewReceivablesPeriodicInvoiceTerm.IsAllowed = true;
			AssertEquals(true, TestPeriodicInvoice.InvoiceTermInfo.ReadOnly);

			AccountingConfigurationRegistry.Instance.AllowModifyInvoiceTerm.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			Env.Security.NewReceivablesPeriodicInvoiceTerm.IsAllowed = false;
			AssertEquals(true, TestPeriodicInvoice.InvoiceTermInfo.ReadOnly);

			AccountingConfigurationRegistry.Instance.AllowModifyInvoiceTerm.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			Env.Security.NewReceivablesPeriodicInvoiceTerm.IsAllowed = false;
			AssertEquals(true, TestPeriodicInvoice.InvoiceTermInfo.ReadOnly);
		}

		public void TestInvoiceTermDays_ReadOnly()
		{
			AccountingConfigurationRegistry.Instance.AllowModifyInvoiceTerm.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			Env.Security.NewReceivablesPeriodicInvoiceTerm.IsAllowed = true;
			TestPeriodicInvoice.InvoiceTerm = Constants.InvoiceTerms.CashOnDelivery;
			AssertEquals(true, TestPeriodicInvoice.InvoiceTermDaysInfo.ReadOnly);

			AccountingConfigurationRegistry.Instance.AllowModifyInvoiceTerm.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			Env.Security.NewReceivablesPeriodicInvoiceTerm.IsAllowed = true;
			TestPeriodicInvoice.InvoiceTerm = Constants.InvoiceTerms.CashOnDelivery;
			AssertEquals(true, TestPeriodicInvoice.InvoiceTermDaysInfo.ReadOnly);

			AccountingConfigurationRegistry.Instance.AllowModifyInvoiceTerm.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			Env.Security.NewReceivablesPeriodicInvoiceTerm.IsAllowed = false;
			TestPeriodicInvoice.InvoiceTerm = Constants.InvoiceTerms.CashOnDelivery;
			AssertEquals(true, TestPeriodicInvoice.InvoiceTermDaysInfo.ReadOnly);

			AccountingConfigurationRegistry.Instance.AllowModifyInvoiceTerm.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			Env.Security.NewReceivablesPeriodicInvoiceTerm.IsAllowed = false;
			TestPeriodicInvoice.InvoiceTerm = Constants.InvoiceTerms.CashOnDelivery;
			AssertEquals(true, TestPeriodicInvoice.InvoiceTermDaysInfo.ReadOnly);

			AccountingConfigurationRegistry.Instance.AllowModifyInvoiceTerm.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			Env.Security.NewReceivablesPeriodicInvoiceTerm.IsAllowed = true;
			TestPeriodicInvoice.InvoiceTerm = Constants.InvoiceTerms.PaymentInAdvance;
			AssertEquals(false, TestPeriodicInvoice.InvoiceTermDaysInfo.ReadOnly);

			AccountingConfigurationRegistry.Instance.AllowModifyInvoiceTerm.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			Env.Security.NewReceivablesPeriodicInvoiceTerm.IsAllowed = true;
			TestPeriodicInvoice.InvoiceTerm = Constants.InvoiceTerms.PaymentInAdvance;
			AssertEquals(true, TestPeriodicInvoice.InvoiceTermDaysInfo.ReadOnly);

			AccountingConfigurationRegistry.Instance.AllowModifyInvoiceTerm.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			Env.Security.NewReceivablesPeriodicInvoiceTerm.IsAllowed = false;
			TestPeriodicInvoice.InvoiceTerm = Constants.InvoiceTerms.PaymentInAdvance;
			AssertEquals(true, TestPeriodicInvoice.InvoiceTermDaysInfo.ReadOnly);

			AccountingConfigurationRegistry.Instance.AllowModifyInvoiceTerm.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			Env.Security.NewReceivablesPeriodicInvoiceTerm.IsAllowed = false;
			TestPeriodicInvoice.InvoiceTerm = Constants.InvoiceTerms.PaymentInAdvance;
			AssertEquals(true, TestPeriodicInvoice.InvoiceTermDaysInfo.ReadOnly);
		}

		public void TestInvoiceDate_ReadOnly()
		{
			Env.Security.NewReceivablesPeriodicInvoiceDate.IsAllowed = true;
			Assert("Should not be readonly as the security right is true.", !TestPeriodicInvoice.InvoiceDateInfo.ReadOnly);

			Env.Security.NewReceivablesPeriodicInvoiceDate.IsAllowed = false;
			Assert("Should be readonly as the security right is false.", TestPeriodicInvoice.InvoiceDateInfo.ReadOnly);

			using (AccountingConfigurationRegistry.Instance.InvAndPstDateDefaultingBehaviour.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, AccountingConstants.InvAndPstDateDefaultingRuleTypes.MonthEndSuspension.Code))
			{
				Env.Security.NewReceivablesPeriodicInvoiceDate.IsAllowed = true;
				Assert("Should be readonly as the registry is set to MTH and then it doesn't matter to the security right.", TestPeriodicInvoice.InvoiceDateInfo.ReadOnly);

				Env.Security.NewReceivablesPeriodicInvoiceDate.IsAllowed = false;
				Assert("Should be readonly as the registry is set to MTH and then it doesn't matter to the security right.", TestPeriodicInvoice.InvoiceDateInfo.ReadOnly);
			}
		}

		public void TestJobValidationOnDebtor()
		{
			var periodHelper = new AccountingPeriodTestHelper(Factory);
			periodHelper.SetupPeriods();

			var shipment = TestObjectCreator.CreateShipment("S001");
			var job = TestObjectCreator.CreateJob(shipment, false, false);
			job.JH_OA_LocalChargesAddr = TestObjectCreator.ABIGAS.Addresses.MainAddress.PK;

			var debtor = TestObjectCreator.CreateOrgHeader("TSTORG", false, true);
			OrgInvoiceType type = debtor.CompanyData.InvoiceTypes.AddNew();
			type.PI_Module = JobInvoicingConsumerTypes.Shipment.Code;
			type.PI_Interval = InvoiceTypeBillingInterval.Codes.MTH;
			type.PI_Type = InvoiceTypeLayoutList.Codes.CHG;

			debtor.CompanyData.OB_APVATConfig = AccountingMasterFilesConstants.OrganisationTaxConfiguartionTypes.NotApplicable.Code;

			var charge = TestObjectCreator.CreateCharge(job, TestObjectCreator.CC3, 100M, 120M);
			charge.JR_OH_SellAccount = debtor.PK;
			charge.JR_InvoiceType = "FID";
			charge.JR_RX_NKSellCurrency = TestObjectCreator.AUD.RX_Code;
			charge.JR_JH = job.PK;
			charge.JR_GB = GlbBranch.CurrentBranch.PK;
			charge.JR_AT_SellGSTRate = Guid.Empty;

			Factory.Save();

			debtor.CompanyData.OB_APVATConfig = AccountingMasterFilesConstants.OrganisationTaxConfiguartionTypes.Default.Code;
			Factory.Save();

			var periodicInvoice = TestPeriodicInvoice;
			if (periodicInvoice != null)
			{
				periodicInvoice.DebtorPK = debtor.PK;
			}
			TestPeriodicInvoice.CurrencyNK = "AUD";
			SetupPeriodicInvoiceWithJobsAndMiscInvoices(TestPeriodicInvoice, new ZGuid[] { job.PK }, new ZGuid[] { charge.PK }, null);
			AssertEquals("Precondition: one job should be found", 1, TestPeriodicInvoice.SelectedJobs.Count());

			TestPeriodicInvoice.SelectedJobs.First().MarkLightValidationAsValidForTesting();

			TestPeriodicInvoice.RunPreSaveValidation();
			var expectedMessage = @"You cannot post because job S001 has errors. Please fix errors before posting.
 - Sell Tax ID: Tax IDs on unposted charges conflict with the debtor ""Tax is Applicable"" flag.
One possible way to resolve this is to go into the ""Job Invoicing"" menu and click the ""Reset Unposted lines Tax Default"" option.";

			var errors = TestPeriodicInvoice.NotificationsIncludingChildren.GetErrors().ToList();
			AssertEquals("Should only be the expected error.", 1, errors.Count);
			AssertContains("Expected error message", expectedMessage, errors[0].Message);
		}

		public void TestLoadedJobsInPeriodicInvoiceWhenAutoJobRevenueJournalsIsEnabled()
		{
			var periodHelper = new AccountingPeriodTestHelper(Factory);
			periodHelper.SetupPeriods();

			var shipment = TestObjectCreator.CreateShipment("S001");
			var job = TestObjectCreator.CreateJob(shipment, false, false);
			job.JH_OA_LocalChargesAddr = TestObjectCreator.ABIGAS.Addresses.MainAddress.PK;

			AssertLoadedJobsInPeriodicInvoiceWhenAutoJobRevenueJournalsIsEnabled(job, GlbCompany.CurrentCompany.OrgProxy, false);

			var currentCompanyActiveBranchOrgProxy = TestObjectCreator.CreateOrgHeader("orgA", true, true);
			GlbCompany.CurrentCompany.FirstActiveBranch.GB_OH_OrgProxy = currentCompanyActiveBranchOrgProxy.PK;
			AssertLoadedJobsInPeriodicInvoiceWhenAutoJobRevenueJournalsIsEnabled(job, currentCompanyActiveBranchOrgProxy, false);

			var currentCompanyInactiveBranch = GlbCompany.CurrentCompany.Branches.First();
			currentCompanyInactiveBranch.GB_IsActive = false;
			var currentCompanyInactiveBranchOrgProxy = TestObjectCreator.CreateOrgHeader("orgB", true, true);
			currentCompanyInactiveBranch.GB_OH_OrgProxy = currentCompanyInactiveBranchOrgProxy.PK;
			AssertLoadedJobsInPeriodicInvoiceWhenAutoJobRevenueJournalsIsEnabled(job, currentCompanyInactiveBranchOrgProxy, true);

			var otherCompany = TestObjectCreator.CreateNewCompany("NCC");
			var otherCompanyActiveBranch = TestObjectCreator.CreateNewBranch(otherCompany, "NCB");
			var otherCompanyActiveBranchOrgProxy = TestObjectCreator.CreateOrgHeader("orgC", true, true);
			otherCompanyActiveBranch.GB_OH_OrgProxy = otherCompanyActiveBranchOrgProxy.PK;
			AssertLoadedJobsInPeriodicInvoiceWhenAutoJobRevenueJournalsIsEnabled(job, otherCompanyActiveBranchOrgProxy, true);
		}

		void AssertLoadedJobsInPeriodicInvoiceWhenAutoJobRevenueJournalsIsEnabled(Job job, OrgHeader debtor, bool isJobLoaded)
		{
			AutoJRJRegistryStatusHelper.SetAutoJRJDisabled_ForTestOnly(GlbCompany.CurrentCompany.PK.ToGuid());
			GlbCompany.CurrentCompany.Factory.Save();

			var type = debtor.CompanyData.InvoiceTypes.AddNew();
			type.PI_Module = JobInvoicingConsumerTypes.Shipment.Code;
			type.PI_Interval = InvoiceTypeBillingInterval.Codes.MTH;
			type.PI_Type = InvoiceTypeLayoutList.Codes.CHG;

			debtor.CompanyData.OB_APVATConfig = AccountingMasterFilesConstants.OrganisationTaxConfiguartionTypes.NotApplicable.Code;

			var charge = TestObjectCreator.CreateCharge(job, TestObjectCreator.CC3, 100M, 120M);
			charge.JR_OH_SellAccount = debtor.PK;
			charge.JR_InvoiceType = "FID";
			charge.JR_RX_NKSellCurrency = TestObjectCreator.AUD.RX_Code;
			charge.JR_JH = job.PK;
			charge.JR_GB = GlbBranch.CurrentBranch.PK;
			charge.JR_AT_SellGSTRate = Guid.Empty;

			Factory.Save();

			debtor.CompanyData.OB_APVATConfig = AccountingMasterFilesConstants.OrganisationTaxConfiguartionTypes.Default.Code;
			Factory.Save();

			var periodicInvoice = TestPeriodicInvoice;
			if (periodicInvoice != null)
			{
				periodicInvoice.DebtorPK = debtor.PK;
			}
			TestPeriodicInvoice.CurrencyNK = "AUD";
			SetupPeriodicInvoiceWithJobsAndMiscInvoices(TestPeriodicInvoice, new ZGuid[] { job.PK }, new ZGuid[] { charge.PK }, null); // This method is used here as an action rather than as a setup. Please call appropriate TestPeriodicInvoice.LoadXXX() method when this test is next modified.
			AssertEquals("Precondition: one job should be found", 1, TestPeriodicInvoice.SelectedJobs.Count());

			AutoJRJRegistryStatusHelper.SetAutoJRJEnabled_ForTestOnly(GlbCompany.CurrentCompany.PK.ToGuid());
			SetupPeriodicInvoiceWithJobsAndMiscInvoices(TestPeriodicInvoice, new ZGuid[] { job.PK }, new ZGuid[] { charge.PK }, null); // This method is used here as an action rather than as a setup. Please call appropriate TestPeriodicInvoice.LoadXXX() method when this test is next modified.
			AssertEquals(isJobLoaded, TestPeriodicInvoice.SelectedJobs.Count() == 1);
		}

		public void TestFilterBusinessObject()
		{
			AssertEquals("FilterBusinessObject should be PeriodicInvoiceJobsFilterBusinessObject", typeof(PeriodicInvoiceJobsFilterBusinessObject), TestPeriodicInvoice.JobsFilter.GetType());
		}

		public void TestDefaultCurrencySelection()
		{
			AssertEquals("Precondition: Login company currency is AUD", "AUD", GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency);
			var orgHeader = TestObjectCreator.AALSHI;
			orgHeader.CompanyData.OB_RX_NKAPDefltCurrency = "EUR";

			var periodicInvoice = TestPeriodicInvoice;
			periodicInvoice.DebtorPK = orgHeader.PK;

			AssertEquals("Default currency should be the current login company currency", "AUD", periodicInvoice.CurrencyNK);

			GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.NewZealand);
			AssertEquals("Precondition: Login company currency is NZD", "NZD", GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency);

			periodicInvoice.DebtorPK = Guid.Empty;
			periodicInvoice.DebtorPK = orgHeader.PK;
			AssertEquals("Default currency should be the current login company currency", "NZD", periodicInvoice.CurrencyNK);
		}

		public void TestUpdateDataAfterLinesChanges()
		{
			using (Job job = TestObjectCreator.CreateJob(TestObjectCreator.CreateShipment("S001")))
			{
				Charge charge = job.Charges.AddNew();
				Charge charge2 = job.Charges.AddNew();

				TestPeriodicInvoice.Jobs.Add(job);
				TestPeriodicInvoice.Charges.Add(charge);
				TestPeriodicInvoice.Charges.Add(charge2);

				AssertEquals(2, TestPeriodicInvoice.Charges.Count);
				AssertEquals(0m, TestPeriodicInvoice.OSExTaxAmount);
				AssertEquals(0m, TestPeriodicInvoice.OSTaxAmount);
				AssertEquals(0m, TestPeriodicInvoice.OSTotalAmount);
				AssertEquals(0m, TestPeriodicInvoice.LocalExTaxAmount);
				AssertEquals(0m, TestPeriodicInvoice.LocalTaxAmount);
				AssertEquals(0m, TestPeriodicInvoice.LocalTotalAmount);

				TestObjectCreator.CreateExchangeRate(job, TestObjectCreator.USD, 3M);
				charge.JR_RX_NKSellCurrency = TestObjectCreator.USD.RX_Code;
				charge.JR_OSSellAmt = 30;
				charge.JR_AT_SellGSTRate = TestObjectCreator.GST1.PK;
				charge2.JR_OSSellAmt = 300;
				charge2.JR_AT_SellGSTRate = TestObjectCreator.GST1.PK;

				TestPeriodicInvoice.UpdateDataAfterLinesChanges();

				AssertEquals(2, TestPeriodicInvoice.Charges.Count);
				AssertEquals(310m, TestPeriodicInvoice.OSExTaxAmount);
				AssertEquals(31m, TestPeriodicInvoice.OSTaxAmount);
				AssertEquals(341m, TestPeriodicInvoice.OSTotalAmount);
				AssertEquals(310m, TestPeriodicInvoice.LocalExTaxAmount);
				AssertEquals(31m, TestPeriodicInvoice.LocalTaxAmount);
				AssertEquals(341m, TestPeriodicInvoice.LocalTotalAmount);
			}
		}

		public void TestUpdateDataAfterLinesChangesAddsFetchHints()
		{
			var job1 = TestObjectCreator.CreateJob(TestObjectCreator.CreateShipment("S001"));
			var job2 = TestObjectCreator.CreateJob(TestObjectCreator.CreateShipment("S002"));
			var charge1 = TestObjectCreator.CreateCharge(job1, TestObjectCreator.CC1, "Desc", TestObjectCreator.AUD, 10M, null, TestObjectCreator.AUD, 10M, TestOrg);
			var charge2 = TestObjectCreator.CreateCharge(job2, TestObjectCreator.CC1, "Desc", TestObjectCreator.AUD, 100M, null, TestObjectCreator.AUD, 100M, TestOrg);
			charge1.JR_InvoiceType = "FID";
			charge2.JR_InvoiceType = "FID";
			TestObjectCreator.CreateWIP(charge1);
			TestObjectCreator.CreateWIP(charge2);
			Factory.Save();

			var periodicInvoice = TestPeriodicInvoice;
			if (periodicInvoice != null)
			{
				periodicInvoice.DebtorPK = TestOrg.PK;
			}
			TestPeriodicInvoice.CurrencyNK = "AUD";
			SetupPeriodicInvoiceWithJobsAndMiscInvoices(TestPeriodicInvoice, new ZGuid[] { job1.PK, job2.PK }, new ZGuid[] { charge1.PK, charge2.PK }, null);
			TestPeriodicInvoice.UpdateDataAfterLinesChanges();

			ZQuery cacheOnlyFilter = new ZQuery();
			cacheOnlyFilter.FetchOnlyFromLocalCache = true;
			AssertEquals("Count of GenericJob in Factory", 0, Factory.Load<GenericJob.GenericJob>(cacheOnlyFilter).Length);
			int genericJobDBHits = Factory.GetTableHitCount(GenericJob.GenericJob.Schema.TableName);
			AssertEquals("Precondition: GetLoadedFetchHintCountForTable GenericJob", 0, Factory.GetLoadedFetchHintCountForTable(GenericJob.GenericJob.Schema.TableName));
			var loadGenericJobToTrigerFetchHint = Factory.LoadGenericJob(job1);
			AssertEquals("GenericJob DB Hits", genericJobDBHits + 1, Factory.GetTableHitCount(GenericJob.GenericJob.Schema.TableName));
			if (TestPeriodicInvoice is PeriodicInvoiceLightForCheckSecurityRights)
			{
				AssertEquals("Count of GenericJob in Factory", 1, Factory.Load<GenericJob.GenericJob>(cacheOnlyFilter).Length);
			}
			else
			{
				AssertEquals("Count of GenericJob in Factory", 2, Factory.Load<GenericJob.GenericJob>(cacheOnlyFilter).Length);
			}

			AssertEquals("Count of ShipmentConsolAndMasterBillNumbers in Factory", 0, Factory.Load<ShipmentConsolAndMasterBillNumbers.ShipmentConsolAndMasterBillNumbers>(cacheOnlyFilter).Length);
			int shipmentConsolAndMasterBillNumbersDBHits = Factory.GetTableHitCount(ShipmentConsolAndMasterBillNumbers.ShipmentConsolAndMasterBillNumbers.Schema.TableName);
			AssertEquals("Precondition: GetLoadedFetchHintCountForTable ShipmentConsolAndMasterBillNumbers", 0, Factory.GetLoadedFetchHintCountForTable(ShipmentConsolAndMasterBillNumbers.ShipmentConsolAndMasterBillNumbers.Schema.TableName));
			var loadShipmentConsolAndMasterBillNumbersToTrigerFetchHint = Factory.Load<ShipmentConsolAndMasterBillNumbers.ShipmentConsolAndMasterBillNumbers>(job1.JH_ParentID);
			AssertEquals("ShipmentConsolAndMasterBillNumbers DB Hits", shipmentConsolAndMasterBillNumbersDBHits + 1, Factory.GetTableHitCount(ShipmentConsolAndMasterBillNumbers.ShipmentConsolAndMasterBillNumbers.Schema.TableName));
			AssertEquals("Count of ShipmentConsolAndMasterBillNumbers in Factory", 2, Factory.Load<ShipmentConsolAndMasterBillNumbers.ShipmentConsolAndMasterBillNumbers>(cacheOnlyFilter).Length);
			AssertEquals("GetLoadedFetchHintCountForTable ShipmentConsolAndMasterBillNumbers", 2, Factory.GetLoadedFetchHintCountForTable(ShipmentConsolAndMasterBillNumbers.ShipmentConsolAndMasterBillNumbers.Schema.TableName));

			AssertEquals(2, TestPeriodicInvoice.Jobs.Count);
			AssertEquals(2, TestPeriodicInvoice.Charges.Count);
		}

		public void TestSetDebtorPKClearsJobsAndMiscInvoices()
		{
			var job = TestObjectCreator.CreateJob("J1", TestObjectCreator.LocalClient, 1, TestObjectCreator.Agent, 1);
			Factory.Save();
			TestPeriodicInvoice.Jobs.Add(job);
			ARInvoice arInvoice = Factory.NewWithValidTestData<ARInvoice>();
			arInvoice.AH_OSTotalAmount = 10M;
			TestPeriodicInvoice.MiscInvoices.Add(arInvoice);
			Assert("Precondition: Jobs must not be empty", TestPeriodicInvoice.Jobs.Count > 0);
			Assert("Precondition: MiscInvoices must not be empty", TestPeriodicInvoice.MiscInvoices.Count > 0);

			TestPeriodicInvoice.DebtorPK = TestObjectCreator.ABIGAS.PK;
			Assert("Precondition: Jobs must be empty", TestPeriodicInvoice.Jobs.Count == 0);
			Assert("Precondition: MiscInvoices not be empty", TestPeriodicInvoice.MiscInvoices.Count == 0);
		}

		public void TestSetInvoiceTypeClearsJobsAndMiscInvoices()
		{
			var job = TestObjectCreator.CreateJob("J1", TestObjectCreator.LocalClient, 1, TestObjectCreator.Agent, 1);
			Factory.Save();
			TestPeriodicInvoice.Jobs.Add(job);
			ARInvoice arInvoice = Factory.NewWithValidTestData<ARInvoice>();
			arInvoice.AH_OSTotalAmount = 10M;
			TestPeriodicInvoice.MiscInvoices.Add(arInvoice);
			Assert("Precondition: Jobs must not be empty", TestPeriodicInvoice.Jobs.Count > 0);
			Assert("Precondition: MiscInvoices must not be empty", TestPeriodicInvoice.MiscInvoices.Count > 0);

			TestPeriodicInvoice.InvoiceType = InvoiceTypesList.Codes.FreightInvoice_Batching;
			Assert("Precondition: Jobs must be empty", TestPeriodicInvoice.Jobs.Count == 0);
			Assert("Precondition: MiscInvoices not be empty", TestPeriodicInvoice.MiscInvoices.Count == 0);
		}

		public void TestInitialization()
		{
			PeriodicInvoiceBase periodicInvoiceCopy = (PeriodicInvoiceBase)GetNewBusinessObject();
			periodicInvoiceCopy.CurrencyNK = TestObjectCreator.USD.RX_Code;
			periodicInvoiceCopy.InvoiceDate = ZDateTime.BrettsBirthday;
			periodicInvoiceCopy.PostDate = ZDateTime.BrettsBirthday;
			periodicInvoiceCopy.JobTypeList[1].Value = true;
			periodicInvoiceCopy.JobTypeList[3].Value = true;

			TestPeriodicInvoice.Initialization(periodicInvoiceCopy, TestObjectCreator.AALSHI.PK, "ITD", false);

			AssertEquals(TestObjectCreator.AALSHI.PK, TestPeriodicInvoice.DebtorPK);
			AssertEquals("ITD", TestPeriodicInvoice.InvoiceType);
			AssertEquals(TestObjectCreator.USD.RX_Code, TestPeriodicInvoice.CurrencyNK);
			AssertEquals(ZDateTime.BrettsBirthday, TestPeriodicInvoice.InvoiceDate);
			AssertEquals(ZDateTime.BrettsBirthday, TestPeriodicInvoice.PostDate);
			Assert(!TestPeriodicInvoice.JobTypeList[0].Value);
			Assert(TestPeriodicInvoice.JobTypeList[1].Value);
			Assert(!TestPeriodicInvoice.JobTypeList[2].Value);
			Assert(TestPeriodicInvoice.JobTypeList[3].Value);
		}

		public void TestChangeOfIncludeInThePeriodicInvoiceIsPropagatedOnJobs()
		{
			var job = TestObjectCreator.CreateJob("J1", TestObjectCreator.LocalClient, 1, TestObjectCreator.Agent, 1);
			Factory.Save();
			TestPeriodicInvoice.Jobs.Add(job);

			Assert(TestPeriodicInvoice.IncludeInThePeriodicInvoice);
			Assert(TestPeriodicInvoice.Jobs[0].IncludeInThePeriodicInvoice);

			TestPeriodicInvoice.IncludeInThePeriodicInvoice = false;
			Assert(!TestPeriodicInvoice.IncludeInThePeriodicInvoice);
			Assert(!TestPeriodicInvoice.Jobs[0].IncludeInThePeriodicInvoice);

			TestPeriodicInvoice.IncludeInThePeriodicInvoice = true;
			Assert(TestPeriodicInvoice.IncludeInThePeriodicInvoice);
			Assert(TestPeriodicInvoice.Jobs[0].IncludeInThePeriodicInvoice);
		}

		public void TestPreviewPeriodicInvoice()
		{
			TestObjectCreator.CreateTestPeriods(ZDateTime.Today.AddMonths(-3));
			Factory.Save();

			var mockIPrintTaskUIProvider = new Mock<IPrintTaskUIProvider>();
			mockIPrintTaskUIProvider
				.Setup(m => m.ShowDocDeliveryUI(It.IsAny<PrintTask>(), It.IsAny<DeliveryInstructions>(),
					It.IsAny<ISecurityCheckpoint>()))
				.Returns(false);

			using (new PrintTaskUIProviderFactory.OverriderForTesting(mockIPrintTaskUIProvider.Object))
			{
				var factory = new BusinessObjectFactory();
				var creator = new TestObjectCreator(factory);

				OrgInvoiceType type = creator.ABIGAS.CompanyData.InvoiceTypes.AddNew();
				type.PI_Module = JobInvoicingConsumerTypes.Shipment.Code;
				type.PI_Interval = InvoiceTypeBillingInterval.Codes.MTH;
				type.PI_Type = InvoiceTypeLayoutList.Codes.CHG;
				type.PI_RS_NKServiceLevel = "STD";

				var shipment = creator.CreateShipment("S1");
				shipment.JS_RS_NKServiceLevel = "STD";
				var job = creator.CreateJob(shipment);
				job.LocalChargesPK = creator.ABIGAS.PK;

				var charge = job.Charges.AddNew();
				try
				{
					charge.FillWithValidTestData();
					charge.JR_AC = creator.CC1.PK;
					charge.JR_OH_SellAccount = creator.ABIGAS.PK;
					charge.JR_OSSellAmt = 100m;
					charge.JR_LocalSellAmt = 100m;
					charge.JR_InvoiceType = InvoiceTypesList.Codes.FinalInvoice_Batching;
					factory.Save();

					var periodicInvoice = new PeriodicInvoice(factory);
					periodicInvoice.DebtorPK = creator.ABIGAS.PK;
					periodicInvoice.LoadJobs();
					var previewErrors = periodicInvoice.PreviewPeriodicInvoiceAndReturnErrors();

					AssertNotNull("Should always return string array.", previewErrors);
					AssertEquals(string.Format("Successfully preview should returns no error, now errors are:\r\n{0}", string.Join("\r\n", previewErrors)), 0, previewErrors.Length);
					AssertEquals("Previewing doesn't post a transaction via PostManager", 0, periodicInvoice.PostManager.Poster.PostedInvoices.Count);
					AssertEquals("Previewing create temporary transaction via previewPostManager", 1, periodicInvoice.previewPostManager_ForTestOnly.Poster.PostedInvoices.Count);
					var invoiceCreated = periodicInvoice.previewPostManager_ForTestOnly.Poster.PostedInvoices[0];
					AssertNotNull(invoiceCreated);

					var printTask = new InvoicePrintTask(new InvoicePrintTask.Configuration(invoiceCreated));
					AssertEquals(1, printTask.TaskCount);

					var newFactory = new BusinessObjectFactory();
					var reloadCharge = newFactory.Load<Charge>(charge.PK);
					reloadCharge.Delete();
					newFactory.Save();
					previewErrors = periodicInvoice.PreviewPeriodicInvoiceAndReturnErrors();
					AssertNotNull("Should always return string array.", previewErrors);
					Assert("Charge of Job is deleted, preview should has error.", previewErrors.Length > 0);
					Assert("Should has the charge related errors.", new string[]
{
						"Error - record: Charge count for the invoice was changed from 1 to 0.",
						"Error - record: Total Local Ex Tax Amount of this invoice has changed from $100.00 to $0.00."
}.SequenceEqual(previewErrors));
				}
				finally
				{
					job.Dispose();
				}
			}
		}

		Job TestPeriodicInvoiceOnHold_CreateJob(string name, JobHeaderStatus status)
		{
			var factory = new BusinessObjectFactory();
			var creator = new TestObjectCreator(factory);
			var orgHeader = TestObjectCreator.LocalClient;

			var job = creator.CreateJob(creator.CreateShipment(name), false, false);
			job.JH_OA_LocalChargesAddr = orgHeader.Addresses.MainAddress.PK;
			job.JH_Status = status.Code;

			var charge = creator.CreateCharge(job, creator.CC3, 100M, 120M);
			charge.JR_OH_SellAccount = orgHeader.PK;
			charge.JR_InvoiceType = InvoiceTypesList.Codes.FinalInvoice_Batching;

			factory.Save();
			return job;
		}

		public void TestPeriodicInvoiceOnHold()
		{
			TestPeriodicInvoice.CurrencyNK = TestObjectCreator.AUD.RX_Code;
			TestPeriodicInvoice.DebtorPK = TestObjectCreator.LocalClient.PK;
			var expectedMessage = "Jobs with status 'Invoice on hold' or 'Work on hold' cannot be included in the invoice.";

			var job1 = TestPeriodicInvoiceOnHold_CreateJob("S001", JobHeaderStatus.WorkOnHold);
			TestPeriodicInvoice.LoadJobs();

			AssertEquals("One job loaded", 1, TestPeriodicInvoice.Jobs.Count);
			Assert("The job is excluded", !TestPeriodicInvoice.Jobs[0].IncludeInThePeriodicInvoice);
			AssertNoError(TestPeriodicInvoice.Jobs[0].IncludeInThePeriodicInvoiceInfo, expectedMessage);
			AssertHasWarning(TestPeriodicInvoice.Jobs[0].IncludeInThePeriodicInvoiceInfo, expectedMessage);
			AssertEquals("The invoice total is zero", ZDecimal.Zero, TestPeriodicInvoice.OSTotalAmount);
			AssertEquals("The job amount is zero", ZDecimal.Zero, TestPeriodicInvoice.Jobs[0].JH_OSAmountForPeriodicBilling);

			TestPeriodicInvoice.Jobs[0].IncludeInThePeriodicInvoice = true;
			AssertHasError(TestPeriodicInvoice.Jobs[0].IncludeInThePeriodicInvoiceInfo, expectedMessage);
			AssertNoWarning(TestPeriodicInvoice.Jobs[0].IncludeInThePeriodicInvoiceInfo, expectedMessage);
			AssertEquals("The invoice total is updated", 120M, TestPeriodicInvoice.OSTotalAmount);
			AssertEquals("The job amount is updated after user action", 120M, TestPeriodicInvoice.Jobs[0].JH_OSAmountForPeriodicBilling);

			TestPeriodicInvoice.RunPreSaveValidation();
			AssertHasError(TestPeriodicInvoice.Jobs[0].IncludeInThePeriodicInvoiceInfo, expectedMessage);

			var job2 = TestPeriodicInvoiceOnHold_CreateJob("S002", JobHeaderStatus.Working);
			TestPeriodicInvoice.LoadJobs();

			AssertEquals("Two jobs loaded", 2, TestPeriodicInvoice.Jobs.Count);

			var selectableJobs = TestPeriodicInvoice.Jobs.Cast<PeriodicInvoiceSelectableJob>();
			var jobWHL = selectableJobs.Single(t => t.Parent.PK == job1.PK);
			var jobWRK = selectableJobs.Single(t => t.Parent.PK == job2.PK);

			Assert("The WHL job is excluded", !jobWHL.IncludeInThePeriodicInvoice);
			Assert("The WRK job is included", jobWRK.IncludeInThePeriodicInvoice);
			AssertNoError(jobWHL.IncludeInThePeriodicInvoiceInfo, expectedMessage);
			AssertHasWarning(jobWHL.IncludeInThePeriodicInvoiceInfo, expectedMessage);
			AssertEquals("The invoice total takes into account the excluded job", 120M, TestPeriodicInvoice.OSTotalAmount);
			AssertEquals("The WHL job has zero charge", ZDecimal.Zero, jobWHL.JH_OSAmountForPeriodicBilling);
			AssertEquals("The WRK job has charge", 120M, jobWRK.JH_OSAmountForPeriodicBilling);

			jobWHL.IncludeInThePeriodicInvoice = true;
			AssertHasError(jobWHL.IncludeInThePeriodicInvoiceInfo, expectedMessage);
			AssertNoWarning(jobWHL.IncludeInThePeriodicInvoiceInfo, expectedMessage);
			AssertEquals("The invoice total is updated", 240M, TestPeriodicInvoice.OSTotalAmount);
			AssertEquals("The WHL job has now charge after the user action", 120M, jobWHL.JH_OSAmountForPeriodicBilling);

			TestPeriodicInvoice.RunPreSaveValidation();
			AssertHasError(jobWHL.IncludeInThePeriodicInvoiceInfo, expectedMessage);

			jobWHL.IncludeInThePeriodicInvoice = false;
			AssertNoError(jobWHL.IncludeInThePeriodicInvoiceInfo, expectedMessage);
			AssertHasWarning(jobWHL.IncludeInThePeriodicInvoiceInfo, expectedMessage);

			jobWRK.IncludeInThePeriodicInvoice = false;
			AssertNoError(jobWHL.IncludeInThePeriodicInvoiceInfo, expectedMessage);
			AssertNoWarning(jobWHL.IncludeInThePeriodicInvoiceInfo, expectedMessage);
		}

		#region Validation

		public virtual void TestRunPreSaveValidationCore()
		{
			OrgInvoiceType type = TestObjectCreator.LocalClient.CompanyData.InvoiceTypes.AddNew();
			type.PI_Module = JobInvoicingConsumerTypes.Shipment.Code;
			type.PI_Interval = InvoiceTypeBillingInterval.Codes.MTH;
			type.PI_Type = InvoiceTypeLayoutList.Codes.CHG;
			type.PI_RS_NKServiceLevel = "STD";
			type.Factory.Save();

			ARInvoice invoice1 = Factory.NewWithValidTestData<ARInvoice>();
			ARInvoice invoice2 = Factory.NewWithValidTestData<ARInvoice>();
			invoice1.AH_IsCancelled = true;
			((IMatching)invoice1).CurrentMatchGroup.AddNew().AP_AH = invoice1.PK;
			invoice2.AH_IsCancelled = true;
			((IMatching)invoice2).CurrentMatchGroup.AddNew().AP_AH = invoice2.PK;

			TestObjectCreator.SetupMatchLinkMatchDate(invoice1);
			TestObjectCreator.SetupMatchLinkMatchDate(invoice2);

			TestPeriodicInvoice.InvoiceTerm = "YYY";
			TestPeriodicInvoice.InvoiceType = ZString.Empty;
			TestPeriodicInvoice.DueDate = ZDateTime.Invalid;
			TestPeriodicInvoice.DebtorPK = ZGuid.Empty;

			TestPeriodicInvoice.MiscInvoices.Add(invoice1);
			TestPeriodicInvoice.MiscInvoices.Add(invoice2);

			TestPeriodicInvoice.RunPreSaveValidation();

			AssertHasErrors(TestPeriodicInvoice.InvoiceTermInfo);
			AssertHasErrors(TestPeriodicInvoice.InvoiceTypeInfo);
			AssertHasErrors(TestPeriodicInvoice.DueDateInfo);
			AssertHasErrors(TestPeriodicInvoice.DebtorPKInfo);

			Assert(invoice1.HasRowErrors);
			Assert(invoice2.HasRowErrors);

			invoice1.AH_IsCancelled = false;

			TestPeriodicInvoice.InvoiceTerm = TestPeriodicInvoice.InvoiceTerms_List[0].Code;
			TestPeriodicInvoice.InvoiceType = TestPeriodicInvoice.InvoiceTypeList[0].Code;
			TestPeriodicInvoice.DueDate = ZDateTime.UtcNow;
			TestPeriodicInvoice.DebtorPK = TestObjectCreator.ABIGAS.PK;

			TestPeriodicInvoice.MiscInvoices.Add(invoice1);
			TestPeriodicInvoice.MiscInvoices.Add(invoice2);

			TestPeriodicInvoice.RunPreSaveValidation();

			AssertNoErrors(TestPeriodicInvoice.InvoiceTermInfo);
			AssertNoErrors(TestPeriodicInvoice.InvoiceTypeInfo);
			AssertNoErrors(TestPeriodicInvoice.DueDateInfo);
			AssertNoErrors(TestPeriodicInvoice.DebtorPKInfo);

			Assert(!invoice1.HasRowErrors);
			Assert(invoice2.HasRowErrors);

			TestObjectCreator.CreateTestPeriods(ZDateTime.Today);
			ForwardingShipment shipment1 = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment1.JS_TransportMode = Enterprise.Core.Constants.TransportModes.Air;
			ForwardingShipment shipment2 = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment2.JS_TransportMode = Enterprise.Core.Constants.TransportModes.Air;
			Job job1 = TestObjectCreator.CreateJob("S001", TestObjectCreator.LocalClient, 5M, TestObjectCreator.Agent, 10M);
			job1.Parent = shipment1;
			Job job2 = TestObjectCreator.CreateJob("S002", TestObjectCreator.LocalClient, 5M, TestObjectCreator.Agent, 10M);
			job2.Parent = shipment2;
			job1.LocalChargesPK = ZGuid.Empty;
			job2.LocalChargesPK = ZGuid.Empty;
			Charge charge = TestObjectCreator.CreateCharge(job1, TestObjectCreator.CC1, "Desc", TestObjectCreator.AUD, 10M, TestObjectCreator.Creditor1, TestObjectCreator.AUD, 10M, TestObjectCreator.LocalClient);
			charge.JR_APInvoiceDate = ZDateTime.Empty;
			charge.JR_PaymentDate = ZDateTime.Empty;
			Charge charge2 = TestObjectCreator.CreateCharge(job2, TestObjectCreator.CC1, "Desc", TestObjectCreator.AUD, 20M, TestObjectCreator.Creditor1, TestObjectCreator.AUD, 20M, TestObjectCreator.LocalClient);
			charge2.JR_APInvoiceDate = ZDateTime.Empty;
			charge2.JR_PaymentDate = ZDateTime.Empty;
			charge.JR_InvoiceType = InvoiceTypesList.Codes.FinalInvoice_Batching;

			RevenueRecognitionCollection valuesForTest = new RevenueRecognitionCollection();
			RevenueRecognition setting = valuesForTest.AddNew();
			setting.JobType = "SHP";
			setting.DirectionCode = Constants.FreightShipmentDirection.Code.All;
			setting.Mode = Enterprise.Core.Constants.TransportModes.All;
			setting.RecognitionDateOptionCode = RevenueRecognitionLookups.RecognitionDateOptionCodes.DeliveryDate;
			AccountingConfigurationRegistry.Instance.RevenueRecognitionSetup.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, valuesForTest);
			job1.ClearCacheOfRevenueRecognitionRegistryItems_ForTestsOnly();
			job2.ClearCacheOfRevenueRecognitionRegistryItems_ForTestsOnly();

			TestObjectCreator.CreateJobChargeRevRecognition(job2, "DEL", AccountingConstants.RevenueRecognitionDateConstants.Immediate);

			Factory.Save();

			AssertEquals("Precondition", 0, job1.RevenueRecognitionCollection.Count);
			AssertNotEquals("Precondition", 0, job2.RevenueRecognitionCollection.Count);

			TestPeriodicInvoice.Jobs.Add(job1);
			TestPeriodicInvoice.Jobs.Add(job2);
			TestPeriodicInvoice.Charges.Add(charge);
			TestPeriodicInvoice.Charges.Add(charge2);

			TestPeriodicInvoice.RunPreSaveValidation();

			PeriodicInvoiceSelectableJob selectableJob1 = (from PeriodicInvoiceSelectableJob j in TestPeriodicInvoice.Jobs where j.Parent.PK == job1.PK select j).Single();
			PeriodicInvoiceSelectableJob selectableJob2 = (from PeriodicInvoiceSelectableJob j in TestPeriodicInvoice.Jobs where j.Parent.PK == job2.PK select j).Single();

			Assert(selectableJob1.HasRowErrors);
			Assert(!selectableJob2.HasRowErrors);

			TestObjectCreator.CreateJobChargeRevRecognition(job1, "DEL", AccountingConstants.RevenueRecognitionDateConstants.Immediate);
			Factory.Save();

			TestPeriodicInvoice.RunPreSaveValidation();

			Assert(!selectableJob1.HasRowErrors);
			Assert(!selectableJob2.HasRowErrors);
		}

		public void TestValidateDebtorPK()
		{
			TestPeriodicInvoice.DebtorPK = ZGuid.Empty;
			TestPeriodicInvoice.ValidateDebtorPK();
			AssertHasErrors(TestPeriodicInvoice.DebtorPKInfo);

			TestPeriodicInvoice.DebtorPK = TestObjectCreator.AALSHI.PK;
			TestPeriodicInvoice.ValidateDebtorPK();
			AssertHasErrors("It must be Debtor.", TestPeriodicInvoice.DebtorPKInfo);

			TestPeriodicInvoice.DebtorPK = TestObjectCreator.ABIGAS.PK;
			TestPeriodicInvoice.ValidateDebtorPK();
			AssertNoErrors(TestPeriodicInvoice.DebtorPKInfo);

			TestObjectCreator.CreateOrgInvoiceType(TestObjectCreator.ABIGAS.CompanyData, JobInvoicingConsumerTypes.CFSShipment.Code, "ALL", "ALL", "STD", InvoiceTypeLayoutList.Codes.INV, InvoiceTypeLayoutList.Codes.INV);
			TestObjectCreator.CreateOrgInvoiceType(TestObjectCreator.ABIGAS.CompanyData, JobInvoicingConsumerTypes.Shipment.Code, "ALL", "ALL", "STD", InvoiceTypeLayoutList.Codes.CHG, InvoiceTypeLayoutList.Codes.INV);

			TestPeriodicInvoice.JobTypeList[TestPeriodicInvoice.GetDescriptionForJobTypeList(JobInvoicingConsumerTypes.CFSShipment.Code)].Value = true;
			TestPeriodicInvoice.JobTypeList[TestPeriodicInvoice.GetDescriptionForJobTypeList(JobInvoicingConsumerTypes.Shipment.Code)].Value = true;

			string expectedError = "This debtor does not have configurations for all the selected job types. Review the 'Periodic Invoicing' configuration for this debtor";
			using (TestPeriodicInvoice.GetValidationSuspender())
			{
				TestPeriodicInvoice.JobTypeList[TestPeriodicInvoice.GetDescriptionForJobTypeList(JobInvoicingConsumerTypes.LocalCartage.Code)].Value = true;
				AssertNoError(TestPeriodicInvoice.DebtorPKInfo, expectedError);
				TestPeriodicInvoice.JobTypeList[TestPeriodicInvoice.GetDescriptionForJobTypeList(JobInvoicingConsumerTypes.LocalCartage.Code)].Value = false;
			}

			TestPeriodicInvoice.JobTypeList[TestPeriodicInvoice.GetDescriptionForJobTypeList(JobInvoicingConsumerTypes.LocalCartage.Code)].Value = true;
			AssertHasError(TestPeriodicInvoice.DebtorPKInfo, expectedError);
		}

		public void TestValidateTaxBranch()
		{
			AssertValidateTaxBranch(true, true, true);
			AssertValidateTaxBranch(true, true, false);
			AssertValidateTaxBranch(true, false, false);
			AssertValidateTaxBranch(true, false, true);
			AssertValidateTaxBranch(false, false, false);
			AssertValidateTaxBranch(false, true, true);
			AssertValidateTaxBranch(false, true, false);
			AssertValidateTaxBranch(false, false, true);
			AssertValidateTaxBranch(true, true, true, true, false);
			AssertValidateTaxBranch(true, true, true, true, true);

			void AssertValidateTaxBranch(bool isEnableRegistry, bool isGSTRegistered, bool isARTaxApplicable, bool isPartOfPeriodicInvoiceBulk = false, bool includeInThePeriodicInvoice = false)
			{
				GlbCompany.CurrentCompany.GC_IsGSTRegistered = isGSTRegistered;
				TestObjectCreator.TestOrganisation.CompanyData.SetARTaxApplicable(isARTaxApplicable);
				using (AccountingMasterFilesRegistry.Instance.EnableTaxBranchReporting.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, isEnableRegistry))
				{
					TestObjectCreator.ResetSecurityCore();

					TestPeriodicInvoice.IsPartOfPeriodicInvoiceBulk = isPartOfPeriodicInvoiceBulk;
					TestPeriodicInvoice.IncludeInThePeriodicInvoice = includeInThePeriodicInvoice;
					TestPeriodicInvoice.DebtorPK = ZGuid.Empty;
					TestPeriodicInvoice.DebtorPK = TestObjectCreator.TestOrganisation.PK;

					var hasErrors = isEnableRegistry && isGSTRegistered && isARTaxApplicable && (!isPartOfPeriodicInvoiceBulk || includeInThePeriodicInvoice);

					TestPeriodicInvoice.TaxBranch = ZGuid.Empty;
					AssertErrorMessage(hasErrors, "Please enter a value.");

					var invalidErrorMessage = "Enter a valid selection.";
					TestPeriodicInvoice.TaxBranch = ZGuid.BrettsGuid;
					AssertErrorMessage(hasErrors, invalidErrorMessage);

					TestPeriodicInvoice.TaxBranchInfo.ClearAllNotifications();
					TestPeriodicInvoice.ValidateBeforeFindingJobs();
					AssertErrorMessage(hasErrors, invalidErrorMessage);
				}

				void AssertErrorMessage(bool hasErrors, string errorMessage)
				{
					if (hasErrors)
					{
						AssertHasError(TestPeriodicInvoice.TaxBranchInfo, errorMessage);
					}
					else
					{
						AssertNoErrors(TestPeriodicInvoice.TaxBranchInfo);
					}
				}
			}
		}

		public void TestValidateDebtorPKForBulkPeriodicInvoices()
		{
			TestObjectCreator.CreateTestPeriods(ZDateTime.Today);

			ForwardingShipment shipment1 = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment1.JS_RS_NKServiceLevel = "STD";
			shipment1.JS_TransportMode = Enterprise.Core.Constants.TransportModes.Air;
			ForwardingShipment shipment2 = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment2.JS_RS_NKServiceLevel = "STD";
			shipment2.JS_TransportMode = Enterprise.Core.Constants.TransportModes.Air;

			Job job1 = TestObjectCreator.CreateJob("S001", TestObjectCreator.ABIGAS, 5M, TestObjectCreator.Agent, 10M);
			job1.Parent = shipment1;
			Job job2 = TestObjectCreator.CreateJob("S002", TestObjectCreator.ABIGAS, 5M, TestObjectCreator.Agent, 10M);
			job2.Parent = shipment2;

			Factory.Save();

			PeriodicInvoiceBase periodicInvoiceCopy = (PeriodicInvoiceBase)GetNewBusinessObject();
			periodicInvoiceCopy.CurrencyNK = TestObjectCreator.AUD.RX_Code;
			periodicInvoiceCopy.InvoiceDate = ZDateTime.BrettsBirthday;

			periodicInvoiceCopy.JobTypeList.Where(x => (x.Description.Contains(JobInvoicingConsumerTypes.CFSShipment.Code) || x.Description.Contains(JobInvoicingConsumerTypes.Shipment.Code))).ToList().ForEach(x => x.Value = true);

			TestPeriodicInvoice.Initialization(periodicInvoiceCopy, TestObjectCreator.ABIGAS.PK, "ITD", true);

			TestPeriodicInvoice.Jobs.Add(job1);
			TestPeriodicInvoice.Jobs.Add(job2);

			TestPeriodicInvoice.ValidateDebtorPK();
			AssertHasError(TestPeriodicInvoice.DebtorPKInfo, "This debtor does not have module configurations for all selected jobs. Review the 'Periodic Invoicing' configuration for this debtor");

			TestPeriodicInvoice.IncludeInThePeriodicInvoice = false;
			TestPeriodicInvoice.ValidateDebtorPK();
			AssertNoErrors(TestPeriodicInvoice.DebtorPKInfo);

			TestPeriodicInvoice.IncludeInThePeriodicInvoice = true;
			TestObjectCreator.ABIGAS.CompanyData.InvoiceTypes.AddNew();
			TestObjectCreator.ABIGAS.CompanyData.InvoiceTypes[0].PI_Module = JobInvoicingConsumerTypes.CFSShipment.Code;
			TestObjectCreator.ABIGAS.CompanyData.InvoiceTypes[0].PI_Type = InvoiceTypeLayoutList.Codes.INV;
			TestObjectCreator.ABIGAS.CompanyData.InvoiceTypes[0].PI_RS_NKServiceLevel = "STD";
			TestPeriodicInvoice.ValidateDebtorPK();
			AssertHasError(TestPeriodicInvoice.DebtorPKInfo, "This debtor does not have module configurations for all selected jobs. Review the 'Periodic Invoicing' configuration for this debtor");

			TestObjectCreator.ABIGAS.CompanyData.InvoiceTypes.AddNew();
			TestObjectCreator.ABIGAS.CompanyData.InvoiceTypes[1].PI_Module = JobInvoicingConsumerTypes.Shipment.Code;
			TestObjectCreator.ABIGAS.CompanyData.InvoiceTypes[1].PI_Type = InvoiceTypeLayoutList.Codes.INV;
			TestObjectCreator.ABIGAS.CompanyData.InvoiceTypes[1].PI_RS_NKServiceLevel = "STD";
			TestObjectCreator.ABIGAS.CompanyData.ClearInvoiceTypeCache_ForTestOnly();

			TestPeriodicInvoice.ValidateDebtorPK();
			AssertNoErrors(TestPeriodicInvoice.DebtorPKInfo);
		}

		public void TestPreSaveValidationDoesNotLeaveErrorsOnUntickedJobs()
		{
			TestObjectCreator.CreateTestPeriods(ZDateTime.Today);

			ForwardingShipment shipment1 = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment1.JS_TransportMode = Enterprise.Core.Constants.TransportModes.Air;
			ForwardingShipment shipment2 = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment2.JS_TransportMode = Enterprise.Core.Constants.TransportModes.Air;
			Factory.Save();

			Job job1 = TestObjectCreator.CreateJob("S001", TestObjectCreator.ABIGAS, 5M, TestObjectCreator.Agent, 10M);
			job1.Parent = shipment1;
			Job job2 = TestObjectCreator.CreateJob("S002", TestObjectCreator.ABIGAS, 5M, TestObjectCreator.Agent, 10M);
			job2.Parent = shipment2;

			PeriodicInvoiceBase periodicInvoiceCopy = (PeriodicInvoiceBase)GetNewBusinessObject();
			periodicInvoiceCopy.CurrencyNK = TestObjectCreator.AUD.RX_Code;
			periodicInvoiceCopy.InvoiceDate = ZDateTime.BrettsBirthday;
			periodicInvoiceCopy.JobTypeList[1].Value = true;
			periodicInvoiceCopy.JobTypeList[3].Value = true;

			TestPeriodicInvoice.Initialization(periodicInvoiceCopy, TestObjectCreator.ABIGAS.PK, "ITD", true);

			Factory.Save();

			TestPeriodicInvoice.Jobs.Add(job1);
			TestPeriodicInvoice.Jobs.Add(job2);

			TestPeriodicInvoice.RunPreSaveValidation();

			PeriodicInvoiceSelectableJob selectableJob1 = (from PeriodicInvoiceSelectableJob j in TestPeriodicInvoice.Jobs where j.Parent.PK == job1.PK select j).Single();
			PeriodicInvoiceSelectableJob selectableJob2 = (from PeriodicInvoiceSelectableJob j in TestPeriodicInvoice.Jobs where j.Parent.PK == job2.PK select j).Single();

			AssertHasRowError(selectableJob1, @"Please enter charges before posting.");
			AssertHasRowError(selectableJob2, @"Please enter charges before posting.");

			TestPeriodicInvoice.Jobs[0].IncludeInThePeriodicInvoice = false;
			AssertHasRowError(selectableJob1, @"Please enter charges before posting.");

			TestPeriodicInvoice.RunPreSaveValidation();
			AssertNoRowErrors("Should have no errors", selectableJob1);
			AssertHasRowError(selectableJob2, @"Please enter charges before posting.");
		}

		[ExpectNoExceptions]
		public void TestValidateDebtorPKDoesntThrowException()
		{
			TestPeriodicInvoice.DebtorPK = ZGuid.NewZGuid();
			TestPeriodicInvoice.JobTypeList[TestPeriodicInvoice.GetDescriptionForJobTypeList(JobInvoicingConsumerTypes.Shipment.Code)].Value = true;
			TestPeriodicInvoice.JobTypeList[TestPeriodicInvoice.GetDescriptionForJobTypeList(JobInvoicingConsumerTypes.CFSShipment.Code)].Value = true;
			TestPeriodicInvoice.ValidateDebtorPK();
		}

		public void TestValidateInvoiceTerm()
		{
			TestPeriodicInvoice.InvoiceTerm = "YYY";
			TestPeriodicInvoice.ValidateInvoiceTerm();
			AssertHasErrors(TestPeriodicInvoice.InvoiceTermInfo);

			TestPeriodicInvoice.InvoiceTerm = TestPeriodicInvoice.InvoiceTerms_List[0].Code;
			TestPeriodicInvoice.ValidateInvoiceTerm();
			AssertNoErrors(TestPeriodicInvoice.InvoiceTermInfo);

			TestPeriodicInvoice.DebtorPK = ZGuid.Empty;
			TestPeriodicInvoice.InvoiceTerm = Constants.InvoiceTerms.MonthsFromInvoiceCycleDate;
			TestPeriodicInvoice.ValidateInvoiceTerm();
			AssertHasError(TestPeriodicInvoice.InvoiceTermInfo, TermsAndDueDateCalculationProvider.MonthsFromInvoiceCycleDateError);
		}

		public void TestValidateInvoiceType()
		{
			TestPeriodicInvoice.InvoiceType = ZString.Empty;
			TestPeriodicInvoice.ValidateInvoiceType();
			AssertHasErrors(TestPeriodicInvoice.InvoiceTypeInfo);

			TestPeriodicInvoice.InvoiceType = "YYY";
			TestPeriodicInvoice.ValidateInvoiceType();
			AssertHasErrors(TestPeriodicInvoice.InvoiceTypeInfo);

			TestPeriodicInvoice.InvoiceType = TestPeriodicInvoice.InvoiceTypeList[0].Code;
			TestPeriodicInvoice.ValidateInvoiceType();
			AssertNoErrors(TestPeriodicInvoice.InvoiceTypeInfo);

			TestPeriodicInvoice.InvoiceType = InvoiceTypesList.Codes.DisbursementInForeignCurrency_Batching;
			TestPeriodicInvoice.CurrencyNK = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;
			TestPeriodicInvoice.ValidateInvoiceType();
			AssertHasErrors(TestPeriodicInvoice.InvoiceTypeInfo);

			TestPeriodicInvoice.InvoiceType = InvoiceTypesList.Codes.ForeignCurrencyInvoice_Batching;
			TestPeriodicInvoice.CurrencyNK = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;
			TestPeriodicInvoice.ValidateInvoiceType();
			AssertHasErrors(TestPeriodicInvoice.InvoiceTypeInfo);

			TestPeriodicInvoice.CurrencyNK = TestObjectCreator.USD.RX_Code;
			TestPeriodicInvoice.ValidateInvoiceType();
			AssertNoErrors(TestPeriodicInvoice.InvoiceTypeInfo);

			TestPeriodicInvoice.InvoiceType = InvoiceTypesList.Codes.SelfBillingInvoice_Batching;
			TestPeriodicInvoice.CurrencyNK = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;
			TestPeriodicInvoice.ValidateInvoiceType();
			AssertNoErrors("Self Billing Batching type can be used for local and foreign currencies", TestPeriodicInvoice.InvoiceTypeInfo);

			TestPeriodicInvoice.InvoiceType = InvoiceTypesList.Codes.SelfBillingInvoice_Batching;
			TestPeriodicInvoice.CurrencyNK = TestObjectCreator.USD.RX_Code;
			TestPeriodicInvoice.ValidateInvoiceType();
			AssertNoErrors("Self Billing Batching type can be used for local and foreign currencies", TestPeriodicInvoice.InvoiceTypeInfo);
		}

		public new void TestValidateCurrencyNK()
		{
			TestPeriodicInvoice.CurrencyNK = ZString.Empty;
			TestPeriodicInvoice.ValidateCurrencyNK();
			AssertHasErrors(TestPeriodicInvoice.CurrencyNKInfo);

			TestPeriodicInvoice.CurrencyNK = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;
			TestPeriodicInvoice.ValidateCurrencyNK();
			AssertNoErrors(TestPeriodicInvoice.CurrencyNKInfo);

			TestPeriodicInvoice.InvoiceType = InvoiceTypesList.Codes.DisbursementInForeignCurrency_Batching;
			TestPeriodicInvoice.ValidateCurrencyNK();
			AssertHasErrors(TestPeriodicInvoice.CurrencyNKInfo);

			TestPeriodicInvoice.CurrencyNK = TestObjectCreator.USD.RX_Code;
			TestPeriodicInvoice.ValidateCurrencyNK();
			AssertNoErrors(TestPeriodicInvoice.InvoiceTypeInfo);
		}

		public void TestValidateBranchDepartmentCombination()
		{
			var bbbDepartment = Factory.NewWithValidTestData<GlbDepartment>();
			Factory.Save();

			GlbBranch.CurrentBranch.AllowedDepartments.DeleteAll();
			GlbBranchCombinationValidationTest.SetAllowedBranchDepartmentCombinations(GlbBranch.CurrentBranch, new GlbDepartment[] { bbbDepartment });

			TestPeriodicInvoice.RunPreSaveValidation();
			var expectedError = string.Format(@"The department {0} cannot be used with the branch {1}.
To change this configuration, set up the Branch/Department Combinations in the Edit Branch Window > Departments Tab."
			, Env.CurrentDepartment.Code, Env.CurrentBranch.Code);

			AssertHasRowError(TestPeriodicInvoice, expectedError);
		}

		public void TestValidateDueDate()
		{
			TestPeriodicInvoice.DueDate = ZDateTime.Empty;
			TestPeriodicInvoice.ValidateDueDate();
			AssertNoErrors(TestPeriodicInvoice.DueDateInfo);

			TestPeriodicInvoice.DueDate = ZDateTime.Invalid;
			TestPeriodicInvoice.ValidateDueDate();
			AssertHasErrors(TestPeriodicInvoice.DueDateInfo);

			TestPeriodicInvoice.DueDate = ZDateTime.BrettsBirthday;
			TestPeriodicInvoice.ValidateDueDate();
			AssertHasErrors(TestPeriodicInvoice.DueDateInfo);

			TestPeriodicInvoice.DueDate = ZDateTime.UtcNow;
			TestPeriodicInvoice.ValidateDueDate();
			AssertNoErrors(TestPeriodicInvoice.DueDateInfo);
		}

		public override void TestValidateTotalAmountWhenNotAllowZeroValueARInvoices()
		{
			AccountingConfigurationRegistry.Instance.AllowZeroValueARInvoices.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);

			var periodicInvoiceCopy = (PeriodicInvoiceBase)GetNewBusinessObject();

			var job = TestObjectCreator.CreateJob("J1", TestObjectCreator.LocalClient, 1, TestObjectCreator.Agent, 1);
			Factory.Save();
			TestPeriodicInvoice.Jobs.Add(job);

			var arInvoice = Factory.NewWithValidTestData<ARInvoice>();
			arInvoice.AH_OSTotalAmount = 0M;
			TestPeriodicInvoice.MiscInvoices.Add(arInvoice);

			TestPeriodicInvoice.IsPartOfPeriodicInvoiceBulk = true;
			TestPeriodicInvoice.IncludeInThePeriodicInvoice = false;
			TestPeriodicInvoice.ValidateTotalAmount();
			AssertNoErrors(TestPeriodicInvoice.OSTotalAmountInfo);

			TestPeriodicInvoice.IncludeInThePeriodicInvoice = true;
			TestPeriodicInvoice.ValidateTotalAmount();
			AssertHasErrors("Total Amount cannot be 0. Please select transactions in order to generate Periodic Invoice. This is controlled by the registry: Accounting -> Receivable Defaults -> Default Settings -> Allow Posting of Zero Value AR Invoices.", TestPeriodicInvoice.OSTotalAmountInfo);

			TestPeriodicInvoice.IsPartOfPeriodicInvoiceBulk = false;
			TestPeriodicInvoice.IncludeInThePeriodicInvoice = false;
			TestPeriodicInvoice.ValidateTotalAmount();
			AssertHasErrors("Total Amount cannot be 0. Please select transactions in order to generate Periodic Invoice. This is controlled by the registry: Accounting -> Receivable Defaults -> Default Settings -> Allow Posting of Zero Value AR Invoices.", TestPeriodicInvoice.OSTotalAmountInfo);

			TestPeriodicInvoice.IncludeInThePeriodicInvoice = true;
			TestPeriodicInvoice.ValidateTotalAmount();
			AssertHasErrors("Total Amount cannot be 0. Please select transactions in order to generate Periodic Invoice. This is controlled by the registry: Accounting -> Receivable Defaults -> Default Settings -> Allow Posting of Zero Value AR Invoices.", TestPeriodicInvoice.OSTotalAmountInfo);
		}

		public void TestExcludeInThePeriodicInvoiceRemoveRowErrors()
		{
			var periodicInvoice = (PeriodicInvoice)GetNewBusinessObject();
			periodicInvoice.IncludeInThePeriodicInvoice = true;
			periodicInvoice.AddRowError("test row error");
			Assert(periodicInvoice.HasRowErrors);
			periodicInvoice.IncludeInThePeriodicInvoice = false;
			Assert(!periodicInvoice.HasRowErrors);
		}

		public void TestSetTerms()
		{
			var createJobCharge = new Action<Job>((jb) =>
				{
					JobCharge charge1 = Factory.NewWithValidTestData<JobCharge>();
					charge1.JR_AC = TestObjectCreator.CC1.PK;
					charge1.JR_JH = jb.PK;
					charge1.JR_RX_NKSellCurrency = TestObjectCreator.AUD.RX_Code;
					charge1.JR_OSSellAmt = 10m;
					charge1.JR_LocalSellAmt = 10m;
					charge1.JR_OH_SellAccount = TestObjectCreator.AALSHI.PK;
					charge1.JR_InvoiceType = InvoiceTypesList.Codes.ForeignCurrencyInvoice_Batching;
				}
			);

			var setValuesToInvoice = new Action(() =>
			{
				TestPeriodicInvoice.DebtorPK = TestObjectCreator.AALSHI.PK;
				TestPeriodicInvoice.InvoiceType = InvoiceTypesList.Codes.ForeignCurrencyInvoice_Batching;
				TestPeriodicInvoice.CurrencyNK = "AUD";
			});

			var consol = TestObjectCreator.CreateConsol("AUSYD", "USLAX", "C001");
			var shipment = TestObjectCreator.CreateShipment("S001", "AUSYD", "USLAX");
			shipment.JS_TransportMode = "AIR";
			var job = TestObjectCreator.CreateJob(shipment);
			createJobCharge(job);

			var shipment1 = TestObjectCreator.CreateShipment("S002", "USLAX", "AUSYD");
			shipment1.JS_TransportMode = "AIR";
			var job1 = TestObjectCreator.CreateJob(shipment1);
			createJobCharge(job1);

			var shipment2 = TestObjectCreator.CreateShipment("S003", "AUSYD", "USLAX");
			shipment2.JS_TransportMode = "SEA";
			var job2 = TestObjectCreator.CreateJob(shipment2);
			createJobCharge(job2);

			Factory.Save();

			TestObjectCreator.AALSHI.CompanyData.LoadARTermForAllInvoiceTypes().PY_InvoiceTerm = InvoiceTermsList.FromMonthEnd.Code;
			TestObjectCreator.AALSHI.CompanyData.LoadARTermForAllInvoiceTypes().PY_InvoiceDays = 5;
			TestObjectCreator.AALSHI.CompanyData.CreateOrLoadARTerm(InvoiceTypesList.Codes.ForeignCurrencyInvoice_Batching).PY_InvoiceTerm = InvoiceTermsList.FromInvoiceDate.Code;
			TestObjectCreator.AALSHI.CompanyData.CreateOrLoadARTerm(InvoiceTypesList.Codes.ForeignCurrencyInvoice_Batching).PY_InvoiceDays = 2;

			//Scenario # 0 -- Number of Selected Job Type: 0 , Direction: 0, TransportMode: 0.
			TestPeriodicInvoice.DebtorPK = TestObjectCreator.AALSHI.PK;
			AssertEquals(InvoiceTermsList.FromMonthEnd.Code, TestPeriodicInvoice.InvoiceTerm);
			AssertEquals((short)5, TestPeriodicInvoice.InvoiceTermDays);

			//Scenario # 1 -- Number of Selected Job Type: 0 , Direction: 0, TransportMode: 0, Invoice Type: 1
			setValuesToInvoice();
			AssertEquals(InvoiceTermsList.FromInvoiceDate.Code, TestPeriodicInvoice.InvoiceTerm);
			AssertEquals((short)2, TestPeriodicInvoice.InvoiceTermDays);

			//Add New Invoice Terms Settings in Org Setup
			var term1 = TestObjectCreator.AALSHI.CompanyData.ARTerms.AddNew();
			SetupTermsInfo(term1, JobInvoicingConsumerTypes.Shipment.Code, ZGuid.Empty, ZGuid.Empty, "EXP", "AIR", InvoiceTypesList.Codes.ForeignCurrencyInvoice_Batching, InvoiceTermsList.FromCustomsClearanceDate.Code, 90);

			//Scenario # 2 -- Number of Selected Job Type: 1 , Direction: 1, TransportMode: 1.
			TestPeriodicInvoice.JobTypeList[TestPeriodicInvoice.GetDescriptionForJobTypeList(JobInvoicingConsumerTypes.Shipment.Code)].Value = true;

			var serviceDirectionFilter = TestPeriodicInvoice.JobsFilter[PeriodicInvoiceBaseJobFilterBusinessObject.SERVICE_DIRECTION] as ModuleTextFilter;
			serviceDirectionFilter.IsActive = true;
			serviceDirectionFilter.Property = "EXP";

			var transportModeFilter = TestPeriodicInvoice.JobsFilter[PeriodicInvoiceBaseJobFilterBusinessObject.TRANSPORT_MODE] as ModuleTextFilter;
			transportModeFilter.IsActive = true;
			transportModeFilter.Property = "AIR";

			setValuesToInvoice();
			SelectJobPks(TestObjectCreator.AALSHI.PK, InvoiceTypesList.Codes.ForeignCurrencyInvoice_Batching, new ZGuid[] { job.PK }, job.Charges.Select(x => x.PK).ToArray());
			TestPeriodicInvoice.LoadJobs();
			AssertEquals(InvoiceTermsList.FromCustomsClearanceDate.Code, TestPeriodicInvoice.InvoiceTerm);
			AssertEquals((short)90, TestPeriodicInvoice.InvoiceTermDays);

			//Scenario # 3 -- Number of Selected Job Type: 2 , Direction: 1, TransportMode: 1
			TestPeriodicInvoice.JobTypeList[TestPeriodicInvoice.GetDescriptionForJobTypeList(JobInvoicingConsumerTypes.Shipment.Code)].Value = true;
			TestPeriodicInvoice.JobTypeList[TestPeriodicInvoice.GetDescriptionForJobTypeList(JobInvoicingConsumerTypes.CFSShipment.Code)].Value = true;

			serviceDirectionFilter.IsActive = true;
			serviceDirectionFilter.Property = "EXP";

			transportModeFilter.IsActive = true;
			transportModeFilter.Property = "AIR";

			setValuesToInvoice();
			SelectJobPks(TestObjectCreator.AALSHI.PK, InvoiceTypesList.Codes.ForeignCurrencyInvoice_Batching, new ZGuid[] { job.PK }, job.Charges.Select(x => x.PK).ToArray());
			TestPeriodicInvoice.LoadJobs();
			AssertEquals(InvoiceTermsList.FromInvoiceDate.Code, TestPeriodicInvoice.InvoiceTerm);
			AssertEquals((short)2, TestPeriodicInvoice.InvoiceTermDays);

			//Restoring Scenario #2
			TestPeriodicInvoice.JobTypeList[TestPeriodicInvoice.GetDescriptionForJobTypeList(JobInvoicingConsumerTypes.Shipment.Code)].Value = true;
			TestPeriodicInvoice.JobTypeList[TestPeriodicInvoice.GetDescriptionForJobTypeList(JobInvoicingConsumerTypes.CFSShipment.Code)].Value = false;

			serviceDirectionFilter.Property = "EXP";
			serviceDirectionFilter.IsActive = true;

			transportModeFilter.Property = "AIR";
			transportModeFilter.IsActive = true;

			setValuesToInvoice();
			SelectJobPks(TestObjectCreator.AALSHI.PK, InvoiceTypesList.Codes.ForeignCurrencyInvoice_Batching, new ZGuid[] { job.PK }, job.Charges.Select(x => x.PK).ToArray());
			TestPeriodicInvoice.LoadJobs();
			AssertEquals(InvoiceTermsList.FromCustomsClearanceDate.Code, TestPeriodicInvoice.InvoiceTerm);
			AssertEquals((short)90, TestPeriodicInvoice.InvoiceTermDays);

			//Scenario # 4 -- Number of Selected Job Type: 1 , Direction: 2, TransportMode: 1
			using (AddActiveFilter(serviceDirectionFilter, "IMP", TestPeriodicInvoice.JobsFilter.ModuleFilters))
			{
				TestPeriodicInvoice.JobTypeList[TestPeriodicInvoice.GetDescriptionForJobTypeList(JobInvoicingConsumerTypes.Shipment.Code)].Value = true;
				TestPeriodicInvoice.JobTypeList[TestPeriodicInvoice.GetDescriptionForJobTypeList(JobInvoicingConsumerTypes.CFSShipment.Code)].Value = false;

				serviceDirectionFilter.IsActive = true;
				serviceDirectionFilter.Property = "EXP";

				transportModeFilter.IsActive = true;
				transportModeFilter.Property = "AIR";

				setValuesToInvoice();
				SelectJobPks(TestObjectCreator.AALSHI.PK, InvoiceTypesList.Codes.ForeignCurrencyInvoice_Batching, new ZGuid[] { job.PK, job1.PK }, job.Charges.Select(x => x.PK).ToArray());
				TestPeriodicInvoice.LoadJobs();
				AssertEquals(InvoiceTermsList.FromInvoiceDate.Code, TestPeriodicInvoice.InvoiceTerm);
				AssertEquals((short)2, TestPeriodicInvoice.InvoiceTermDays);
			}

			//Restoring Scenario # 2
			TestPeriodicInvoice.JobTypeList[TestPeriodicInvoice.GetDescriptionForJobTypeList(JobInvoicingConsumerTypes.Shipment.Code)].Value = true;
			TestPeriodicInvoice.JobTypeList[TestPeriodicInvoice.GetDescriptionForJobTypeList(JobInvoicingConsumerTypes.CFSShipment.Code)].Value = false;

			serviceDirectionFilter.Property = "EXP";
			serviceDirectionFilter.IsActive = true;

			transportModeFilter.Property = "AIR";
			transportModeFilter.IsActive = true;

			setValuesToInvoice();
			SelectJobPks(TestObjectCreator.AALSHI.PK, InvoiceTypesList.Codes.ForeignCurrencyInvoice_Batching, new ZGuid[] { job.PK }, job.Charges.Select(x => x.PK).ToArray());
			TestPeriodicInvoice.LoadJobs();
			AssertEquals(InvoiceTermsList.FromCustomsClearanceDate.Code, TestPeriodicInvoice.InvoiceTerm);
			AssertEquals((short)90, TestPeriodicInvoice.InvoiceTermDays);

			//Scenario # 5 -- Number of Selected Job Type: 1 , Direction: 1, TransportMode: 2
			using (AddActiveFilter(transportModeFilter, "SEA", TestPeriodicInvoice.JobsFilter.ModuleFilters))
			{
				TestPeriodicInvoice.JobTypeList[TestPeriodicInvoice.GetDescriptionForJobTypeList(JobInvoicingConsumerTypes.Shipment.Code)].Value = true;
				TestPeriodicInvoice.JobTypeList[TestPeriodicInvoice.GetDescriptionForJobTypeList(JobInvoicingConsumerTypes.CFSShipment.Code)].Value = false;

				serviceDirectionFilter.IsActive = true;
				serviceDirectionFilter.Property = "EXP";

				transportModeFilter.IsActive = true;
				transportModeFilter.Property = "AIR";

				setValuesToInvoice();
				SelectJobPks(TestObjectCreator.AALSHI.PK, InvoiceTypesList.Codes.ForeignCurrencyInvoice_Batching, new ZGuid[] { job.PK, job1.PK, job2.PK }, job.Charges.Select(x => x.PK).ToArray());
				TestPeriodicInvoice.LoadJobs();
				AssertEquals(InvoiceTermsList.FromInvoiceDate.Code, TestPeriodicInvoice.InvoiceTerm);
				AssertEquals((short)2, TestPeriodicInvoice.InvoiceTermDays);
			}
		}

		protected virtual void SelectJobPks(ZGuid debtorPK, ZString invoiceType, ZGuid[] jobPKsToSelect, ZGuid[] chargeCodeToSelect)
		{
		}

		DisposableAction AddActiveFilter(ModuleFilter filter, ZString value, ModuleFilterCollection moduleFilters)
		{
			var filterStrips = new ZArchitecture.Business.Internal.FilterStripCollection(moduleFilters);

			var newFilterStrip = filterStrips.AddNew(filter.Description);
			newFilterStrip.OrCategory = FilterOrCategory.Red;
			filter.OrCategory = FilterOrCategory.Red;

			var addedFilter = newFilterStrip.CurrentModuleFilter as ModuleTextFilter;

			var createAction = new Action(() =>
			{
				addedFilter.IsActive = true;
				addedFilter.Property = value;
			});

			var disposeAction = new Action(() =>
			{
				addedFilter.IsActive = false;
				filterStrips.Remove(newFilterStrip);
			});

			return new DisposableAction(createAction, disposeAction);
		}

		void SetupTermsInfo(OrgARTerms term, string jobType, ZGuid branchPK, ZGuid deptPK, string direction, string transportMode, string invoiceType, string invoiceTerm, int termDays)
		{
			using (term.GetValidationSuspender())
			{
				term.PY_JobType = jobType;
				term.PY_GB_Branch = branchPK;
				term.PY_GE_Department = deptPK;
				term.PY_Direction = direction;
				term.PY_TransportMode = transportMode;
				term.PY_InvoiceClass = invoiceType;
				term.PY_InvoiceTerm = invoiceTerm;
				term.PY_InvoiceDays = (ZByte)termDays;
			}
		}

		#endregion

		#region Overrides

		public override void TestSetDefaultValues()
		{
			AssertEquals("InvoiceDate", ZDateTime.Now.Date, TestPeriodicInvoice.InvoiceDate.Date);
			AssertEquals("PostDate", ZDateTime.Now.Date, TestPeriodicInvoice.PostDate.Date);
			AssertEquals("CurrencyNK", GlbCompany.CurrentCompany.LocalCurrency.RX_Code, TestPeriodicInvoice.CurrencyNK);
			AssertEquals("InvoiceType", TestPeriodicInvoice.InvoiceTypeList[0].Code, TestPeriodicInvoice.InvoiceType);
		}

		#endregion

		public void TestInvoiceTermsList()
		{
			AssertEquals(typeof(ARInvoiceTermsList), TestPeriodicInvoice.InvoiceTerms_List.GetType());
		}

		public void TestInvoiceID()
		{
			TestPeriodicInvoice.DebtorPK = TestObjectCreator.ABIGAS.PK;
			TestPeriodicInvoice.CurrencyNK = TestObjectCreator.USD.RX_Code;
			TestPeriodicInvoice.InvoiceType = InvoiceTypesList.Codes.DisbursementInForeignCurrency_Batching;

			AssertEquals("ABIGAS, USD, DCD", TestPeriodicInvoice.InvoiceID);
		}

		public void TestSellReference()
		{
			TestPeriodicInvoice.SellReference = "Test Sell Reference";
			AssertEquals("Test Sell Reference", TestPeriodicInvoice.SellReference);
			AssertEquals(35, TestPeriodicInvoice.SellReferenceInfo.MaxLength);
		}

		public void TestPeriodicInvoiceSplitting()
		{
			using (Job job = TestObjectCreator.CreateJob(TestObjectCreator.CreateShipment("S001")))
			{
				var addressPK = TestObjectCreator.CreateAddress(TestObjectCreator.LocalClient, OrgAddressType.Office, true).PK;
				TestObjectCreator.CreateOrgInvoiceType(TestObjectCreator.LocalClient.CompanyData, "ALL", "ALL", "ALL", ZString.Empty, "INV", "INV");

				CreateChargeForTest(job, addressPK, TestObjectCreator.CC1, 250M, 500M);
				CreateChargeForTest(job, addressPK, TestObjectCreator.CC2, 900M, 900M);
				CreateChargeForTest(job, addressPK, TestObjectCreator.CC3, 150M, 320M);
				CreateChargeForTest(job, addressPK, TestObjectCreator.FRT, 350M, 700M);

				Factory.Save();
			}

			var periodHelper = new AccountingPeriodTestHelper(Factory);
			periodHelper.SetupPeriods();

			var periodicInvoice = new PeriodicInvoice(new BusinessObjectFactory());
			periodicInvoice.DebtorPK = TestObjectCreator.LocalClient.PK;
			periodicInvoice.LoadJobs();

			AssertEquals(1, periodicInvoice.Jobs.Count);
			AssertEquals(4, periodicInvoice.Charges.Count);

			AccountingConfigurationRegistry.Instance.JobInvoiceMaximumNumberOfChargesOfSplittingRules.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 3);
			AccountingConfigurationRegistry.Instance.JobInvoiceMaximumValueOfSplittingRules.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 0);
			AccountingConfigurationRegistry.Instance.JobInvoiceAddressCountry.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "SAM");

			AssertEquals(0, periodicInvoice.PostManager.Poster.PostedInvoices.Count);

			periodicInvoice.PostManager.CreateTransactions(JobInvoicingPostingOption.Revenue);

			AssertEquals(2, periodicInvoice.PostManager.Poster.PostedInvoices.Count);

			var postedInvoices = periodicInvoice.PostManager.Poster.PostedInvoices.ToArray<Invoice>().OrderBy(x => x.Lines.Count).ToArray();
			AssertEquals(1, postedInvoices[0].Lines.Count);
			AssertEquals(3, postedInvoices[1].Lines.Count);
		}

		public void TestPeriodicInvoiceSplittingWhenPreviewing()
		{
			TestObjectCreator.CreateTestPeriods(ZDateTime.Today.AddMonths(-3));
			Factory.Save();

			var mockIPrintTaskUIProvider = new Mock<IPrintTaskUIProvider>();
			mockIPrintTaskUIProvider
				.Setup(m => m.ShowDocDeliveryUI(It.IsAny<PrintTask>(), It.IsAny<DeliveryInstructions>(),
					It.IsAny<ISecurityCheckpoint>()))
				.Returns(false);

			using (new PrintTaskUIProviderFactory.OverriderForTesting(mockIPrintTaskUIProvider.Object))
			{
				var factory = new BusinessObjectFactory();
				var creator = new TestObjectCreator(factory);

				OrgInvoiceType type = creator.ABIGAS.CompanyData.InvoiceTypes.AddNew();
				type.PI_Module = JobInvoicingConsumerTypes.Shipment.Code;
				type.PI_Interval = InvoiceTypeBillingInterval.Codes.MTH;
				type.PI_Type = InvoiceTypeLayoutList.Codes.CHG;
				type.PI_RS_NKServiceLevel = "STD";

				var shipment = creator.CreateShipment("S1");
				shipment.JS_RS_NKServiceLevel = "STD";
				var job = creator.CreateJob(shipment);
				job.LocalChargesPK = creator.ABIGAS.PK;

				try
				{
					var charge = job.Charges.AddNew();
					charge.FillWithValidTestData();
					charge.JR_AC = creator.CC1.PK;
					charge.JR_OH_SellAccount = creator.ABIGAS.PK;
					charge.JR_OSSellAmt = 100m;
					charge.JR_LocalSellAmt = 100m;
					charge.JR_InvoiceType = InvoiceTypesList.Codes.FinalInvoice_Batching;

					charge = job.Charges.AddNew();
					charge.FillWithValidTestData();
					charge.JR_AC = creator.CC3.PK;
					charge.JR_OH_SellAccount = creator.ABIGAS.PK;
					charge.JR_OSSellAmt = 300m;
					charge.JR_LocalSellAmt = 300m;
					charge.JR_InvoiceType = InvoiceTypesList.Codes.FinalInvoice_Batching;

					factory.Save();

					AccountingConfigurationRegistry.Instance.JobInvoiceMaximumValueOfSplittingRules.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 350m);

					var periodicInvoice = new PeriodicInvoice(factory);
					periodicInvoice.DebtorPK = creator.ABIGAS.PK;
					periodicInvoice.LoadJobs();
					var previewErrors = periodicInvoice.PreviewPeriodicInvoiceAndReturnErrors();

					AssertNotNull("Should always return string array.", previewErrors);
					AssertEquals(string.Format("Successfully preview should returns no error, now errors are:\r\n{0}", string.Join("\r\n", previewErrors)), 0, previewErrors.Length);
					AssertEquals("Previewing doesn't post a transaction via PostManager", 0, periodicInvoice.PostManager.Poster.PostedInvoices.Count);
					AssertEquals("Previewing create temporary transaction via previewPostManager", 2, periodicInvoice.previewPostManager_ForTestOnly.Poster.PostedInvoices.Count);

					var printTask = new InvoicePrintTask(new InvoicePrintTask.Configuration(periodicInvoice.previewPostManager_ForTestOnly.Poster.PostedInvoices.ToArray<TransactionHeader>()));
					AssertEquals(1, printTask.TaskCount);
				}
				finally
				{
					job.Dispose();
				}
			}
		}

		public void TestPeriodicInvoice_CheckExporterExemption()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.Italy))
			using (AccountingMasterFilesRegistry.Instance.ValidateTaxIDApplicationForExporterExemptionItaly.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			{
				var job = SetupDataForExporterExemption();

				CreateChargeWithTaxID(job, "Test Charge 1", 300, dichIntTaxRate.PK);

				var periodicInvoice = new PeriodicInvoice(Factory);
				periodicInvoice.CurrencyNK = "EUR";
				periodicInvoice.DebtorPK = TestObjectCreator.Debtor.PK;
				periodicInvoice.InvoiceType = InvoiceTypesList.Codes.FinalInvoice_Batching;
				periodicInvoice.LoadJobs();

				Assert("Transaction created", periodicInvoice.CreateTransactions());
				AssertNotNull("Posted invoices not null", periodicInvoice.PostManager.Poster.PostedInvoices);
				AssertEquals("Posted invoices count is 1", 1, periodicInvoice.PostManager.Poster.PostedInvoices.Count);

				var periodicInvoice2 = new PeriodicInvoice(Factory);
				periodicInvoice2.CurrencyNK = "EUR";
				periodicInvoice2.DebtorPK = TestObjectCreator.Debtor.PK;
				periodicInvoice2.InvoiceType = InvoiceTypesList.Codes.FinalInvoice_Batching;
				periodicInvoice2.LoadJobs();

				CreateChargeWithTaxID(job, "Test Charge 2", 1400, dichIntTaxRate.PK);
				periodicInvoice2.LoadJobs();

				Assert("Transaction not created", !periodicInvoice2.CreateTransactions());
				var expectedError = @"Error - Accounts Receivable Invoice: There are errors that need to be corrected before saving the current Accounts Receivable Invoice.
TAX ID: DICH.INT, based on the Registry [Accounting -> Receivable Defaults -> Default Settings -> Validate Tax ID Application for Exporter Exemption (Italy)] it is not possible to proceed with the post because you are posting a transaction on a debtor that has one or more EXV-VAT/GST Exporter Exemption document with CEILING LIMIT.
The sum of the transactions that contain DICH.INT Tax ID including this one you are posting, exceeds the CEILING LIMIT by EUR 400.00.
To proceed with the post please fix Tax ID Code or save a new EXV-VAT/GST Exporter Exemption in the Debtor Organization eDocs, with an higher CEILING LIMIT or disable the Registry.";
				AssertEquals("Ceiling Limit Error", true, periodicInvoice2.PostManager.Poster.PostedInvoices[0].Notifications.GetErrors().Contains(expectedError));
			}
		}

		public void TestPeriodicInvoice_CheckExporterExemption_NotValid()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.Italy))
			using (AccountingMasterFilesRegistry.Instance.ValidateTaxIDApplicationForExporterExemptionItaly.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			{
				var job = SetupDataForExporterExemption(expired: true);

				CreateChargeWithTaxID(job, "Test Charge 1", 300, dichIntTaxRate.PK);

				var periodicInvoice = new PeriodicInvoice(Factory);
				periodicInvoice.CurrencyNK = "EUR";
				periodicInvoice.DebtorPK = TestObjectCreator.Debtor.PK;
				periodicInvoice.InvoiceType = InvoiceTypesList.Codes.FinalInvoice_Batching;
				periodicInvoice.LoadJobs();

				Assert("Transaction not created", !periodicInvoice.CreateTransactions());
				var expectedError = "Error - Accounts Receivable Invoice: If [Accounting -> Receivable Defaults -> Default Settings -> Validate Tax ID Application for Exporter Exemption (Italy)] is set to Yes, the DICH.INT Tax ID can only be used for a Debtor with a valid Exporter Exemption Certificate, where the certificate Ceiling Limit has not been exceeded.";
				AssertEquals("No Valid EXV Documents Error", true, periodicInvoice.PostManager.Poster.PostedInvoices[0].Notifications.GetErrors().Contains(expectedError));
			}
		}

		public void TestPeriodicInvoice_CheckExporterExemption_StillAvailableAmount()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.Italy))
			using (AccountingMasterFilesRegistry.Instance.ValidateTaxIDApplicationForExporterExemptionItaly.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			{
				var job = SetupDataForExporterExemption();

				CreateChargeWithTaxID(job, "Test Charge 1", 200, TestObjectCreator.GST2.PK);

				var periodicInvoice = new PeriodicInvoice(Factory);
				periodicInvoice.CurrencyNK = "EUR";
				periodicInvoice.DebtorPK = TestObjectCreator.Debtor.PK;
				periodicInvoice.InvoiceType = InvoiceTypesList.Codes.FinalInvoice_Batching;
				periodicInvoice.LoadJobs();

				Assert("Transaction not created", !periodicInvoice.CreateTransactions());
				var expectedError = @"Error - Accounts Receivable Invoice: ZDebtor: There are errors that need to be corrected before this Accounts Receivable Invoice can be saved.
TAX ID: ZZGST2 based on the registry [Accounting -> Receivable Defaults -> Default Settings -> Validate Tax ID Application for Exporter Exemption (Italy)] it is not possible to proceed with the post because you are posting a transaction on a debtor that has one or more EXV-VAT/GST Exporter Exemption documents that still have available EUR 1300.00 CEILING LIMIT.
To proceed with the post fix Tax ID Code or disable the above registry item.";
				AssertEquals("Still Available Amount Error", true, periodicInvoice.PostManager.Poster.PostedInvoices[0].Notifications.GetErrors().Contains(expectedError));
			}
		}

		void CreateChargeWithTaxID(Job job, string description, decimal sellAmount, ZGuid taxRatePK)
		{
			var charge = TestObjectCreator.CreateCharge(job, TestObjectCreator.MRG100, description, null, 0, null, TestObjectCreator.EUR, sellAmount, TestObjectCreator.Debtor);
			charge.JR_InvoiceType = InvoiceTypesList.Codes.FinalInvoice_Batching;
			charge.JR_APInvoiceDate = charge.JR_PaymentDate = ZDateTime.Empty;
			charge.JR_AT_SellGSTRate = taxRatePK;
			charge.JR_SellTaxDate = ZDate.Today;
			Factory.Save();
		}

		[TestDate(2021, 02, 15)]
		public void TestPeriodicInvoice_CashAdvanceErrors()
		{
			using (AccountingConfigurationRegistry.Instance.EnableReceivablesCashAdvanceFunctionality.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var validationError = string.Empty;
				var setupData = SetupForPeriodInvoiceWithCashAdvance(true);

				var periodicInvoice = new PeriodicInvoice(Factory);
				periodicInvoice.PostManager.OnCriticalPostError += (object sender, CriticalPostingErrorEventArgs e) => validationError = e.ErrorMessage;
				periodicInvoice.DebtorPK = TestObjectCreator.LocalClient.PK;
				periodicInvoice.LoadJobs();
				AssertEquals(1, periodicInvoice.Jobs.Count);
				AssertEquals(3, periodicInvoice.Charges.Count);

				var postManager = periodicInvoice.PostManager;
				AssertEquals(0, postManager.Poster.PostedInvoices.Count);
				postManager.CreateTransactions(JobInvoicingPostingOption.Revenue);

				AssertEquals(true, postManager.CancelPosting);
				AssertEquals($@"One or more charges have an unpaid Advance Payment. However, not all charges linked to the same Advance Payment are being posted. Please post all charges that relate to a single Advance Payment. Otherwise, cancel the request in order to post this charge.
{TestObjectCreator.LocalClient.OH_Code}-{TestObjectCreator.LocalClient.OH_FullName} | {TestObjectCreator.CC4.AC_Code} | {TestObjectCreator.CC4.AC_Desc} | AUD | 700 | {setupData.SecondCAH.CAH_RequestReferenceNumber}
", validationError);
			}
		}

		[TestDate(2021, 02, 15)]
		public void TestPeriodicInvoice_NoCashAdvanceErrors()
		{
			using (AccountingConfigurationRegistry.Instance.EnableReceivablesCashAdvanceFunctionality.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (AccountingConfigurationRegistry.Instance.AllowManualSettingOfReceivablesCashAdvanceRequestStatusToPaid.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var validationError = string.Empty;
				var setupData = SetupForPeriodInvoiceWithCashAdvance();
				setupData.FirstCAH.MarkAsPaid();
				setupData.SecondCAH.MarkAsPaid();
				Factory.Save();

				var periodicInvoice = new PeriodicInvoice(Factory);
				periodicInvoice.PostManager.OnCriticalPostError += (object sender, CriticalPostingErrorEventArgs e) => validationError = e.ErrorMessage;
				periodicInvoice.DebtorPK = TestObjectCreator.LocalClient.PK;
				periodicInvoice.LoadJobs();
				AssertEquals(1, periodicInvoice.Jobs.Count);
				AssertEquals(4, periodicInvoice.Charges.Count);

				var postManager = periodicInvoice.PostManager;
				AssertEquals(0, postManager.Poster.PostedInvoices.Count);
				postManager.CreateTransactions(JobInvoicingPostingOption.Revenue);

				AssertEquals(false, postManager.CancelPosting);
				AssertEquals(string.Empty, validationError);
			}
		}

		[TestDate(2021, 02, 15)]
		public void TestPeriodicInvoice_NoCashAdvanceErrorsWhenRegistryDisabled()
		{
			using (AccountingConfigurationRegistry.Instance.EnableReceivablesCashAdvanceFunctionality.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				var validationError = string.Empty;
				_ = SetupForPeriodInvoiceWithCashAdvance();

				var periodicInvoice = new PeriodicInvoice(Factory);
				periodicInvoice.PostManager.OnCriticalPostError += (object sender, CriticalPostingErrorEventArgs e) => validationError = e.ErrorMessage;
				periodicInvoice.DebtorPK = TestObjectCreator.LocalClient.PK;
				periodicInvoice.LoadJobs();
				AssertEquals(1, periodicInvoice.Jobs.Count);
				AssertEquals(4, periodicInvoice.Charges.Count);

				var postManager = periodicInvoice.PostManager;
				AssertEquals(0, postManager.Poster.PostedInvoices.Count);
				postManager.CreateTransactions(JobInvoicingPostingOption.Revenue);

				AssertEquals(false, postManager.CancelPosting);
				AssertEquals(string.Empty, validationError);
			}
		}

		[TestDate(2021, 02, 15)]
		public void TestPeriodicInvoice_ComplianceBookErrors_PST()
		{
			AssertPeriodicInvoice_ComplianceBookErrors(AccountingMasterFilesConstants.ComplianceNumberAllocationDateOptions.PostDate.Code, new ZDate(2021, 2, 10), new ZDate(2020, 11, 10));
		}

		[TestDate(2021, 02, 15)]
		public void TestPeriodicInvoice_ComplianceBookErrors_INV()
		{
			AssertPeriodicInvoice_ComplianceBookErrors(AccountingMasterFilesConstants.ComplianceNumberAllocationDateOptions.InvoiceDate.Code, new ZDate(2020, 11, 10), new ZDate(2021, 2, 10));
		}

		void AssertPeriodicInvoice_ComplianceBookErrors(string dateOption, ZDateTime postDate, ZDateTime invoiceDate)
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Italy))
			{
				SetComplianceSequence(new ZDate(2021, 1, 31));

				var periodicInvoice = SetupForPeriodInvoiceTest(postDate, invoiceDate);
				var postManager = periodicInvoice.PostManager;

				var registryInstance = AccountingMasterFilesRegistry.Instance;
				var companyPK = GlbCompany.CurrentCompany.PK.ToGuid();

				using (registryInstance.ComplianceDocumentNumberAllocation_Receivables.SetTemporaryValue(companyPK, Guid.Empty, Guid.Empty, AccComplianceSequenceLookups.ComplianceDocumentNumberAllocationSettingCodes.Post))
				using (registryInstance.ComplianceNumberAllocationDate_AR.SetTemporaryValue(companyPK, Guid.Empty, Guid.Empty, dateOption))
				{
					AssertEquals(0, postManager.Poster.PostedInvoices.Count);
					postManager.CreateTransactions(JobInvoicingPostingOption.Revenue);

					AssertEquals(true, postManager.CancelPosting);

					string expectedMessage = @"Please check your Compliance Invoice Book Setups. 
 A Compliance Invoice Book for the relevant Compliance Sub-Type, Branch, Active Status and Start / Expiry Date does not exist.";
					AssertEquals(expectedMessage, lastErrorReportedForPeriodicInvoice);
				}
			}
		}

		[TestDate(2021, 02, 15)]
		public void TestPeriodicInvoice_RegistryNotSetNotErrors()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Italy))
			{
				SetComplianceSequence(new ZDate(2021, 1, 31));

				var periodicInvoice = SetupForPeriodInvoiceTest();
				periodicInvoice.PostDate = new ZDate(2021, 2, 10);
				var postManager = periodicInvoice.PostManager;

				var invoiceForTest = TestObjectCreator.CreateInvoice(typeof(ARInvoice), "INV001", TestObjectCreator.EUR, 1M, TestObjectCreator.ABIGAS);
				invoiceForTest.AH_ComplianceSubType = ItalyComplianceInfo.ComplianceSubTypeCodes.ARI;
				invoiceForTest.AH_PostDate = new ZDate(2021, 1, 10);

				Factory.Save();

				AssertEquals(0, postManager.Poster.PostedInvoices.Count);
				postManager.CreateTransactions(JobInvoicingPostingOption.Revenue);

				AssertEquals(false, postManager.CancelPosting);

				AssertNullOrEmpty(lastErrorReportedForPeriodicInvoice);
			}
		}

		[TestDate(2021, 02, 15)]
		public void TestPeriodicInvoice_PastTransactionsWithBlankComplianceNumber_PST()
		{
			AssertPeriodicInvoice_PastTransactionsWithBlankComplianceNumber(AccountingMasterFilesConstants.ComplianceNumberAllocationDateOptions.PostDate.Code, new ZDate(2021, 1, 10), new ZDate(2020, 11, 10));
		}

		[TestDate(2021, 02, 15)]
		public void TestPeriodicInvoice_PastTransactionsWithBlankComplianceNumber_INV()
		{
			AssertPeriodicInvoice_PastTransactionsWithBlankComplianceNumber(AccountingMasterFilesConstants.ComplianceNumberAllocationDateOptions.InvoiceDate.Code, new ZDate(2020, 11, 10), new ZDate(2021, 1, 10));
		}

		public void AssertPeriodicInvoice_PastTransactionsWithBlankComplianceNumber(string dateOption, ZDateTime postDate, ZDateTime invoiceDate)
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Italy))
			{
				SetComplianceSequence(new ZDate(2021, 12, 31));

				var periodicInvoice = SetupForPeriodInvoiceTest();
				var postManager = periodicInvoice.PostManager;

				var invoiceForTest = TestObjectCreator.CreateInvoice(typeof(ARInvoice), "INV001", TestObjectCreator.EUR, 1M, TestObjectCreator.ABIGAS);
				invoiceForTest.AH_ComplianceSubType = ItalyComplianceInfo.ComplianceSubTypeCodes.ARI;
				invoiceForTest.AH_PostDate = postDate;
				invoiceForTest.AH_InvoiceDate = invoiceDate;

				Factory.Save();

				var registryInstance = AccountingMasterFilesRegistry.Instance;
				var companyPK = GlbCompany.CurrentCompany.PK.ToGuid();

				using (registryInstance.ComplianceDocumentNumberAllocation_Receivables.SetTemporaryValue(companyPK, Guid.Empty, Guid.Empty, AccComplianceSequenceLookups.ComplianceDocumentNumberAllocationSettingCodes.Post))
				using (registryInstance.ComplianceNumberAllocationDate_AR.SetTemporaryValue(companyPK, Guid.Empty, Guid.Empty, dateOption))
				{
					AssertEquals(0, postManager.Poster.PostedInvoices.Count);
					postManager.CreateTransactions(JobInvoicingPostingOption.Revenue);

					AssertEquals(true, postManager.CancelPosting);

					var dateLabel = dateOption == AccountingMasterFilesConstants.ComplianceNumberAllocationDateOptions.InvoiceDate.Code ? "Invoice Date" : "Post Date";

					string expectedMessage = $@"Compliance Numbers cannot be allocated.
 There is some transaction with the same Compliance Sub Type {ItalyComplianceInfo.ComplianceSubTypeCodes.ARI} in earlier {dateLabel} and Compliance Number empty.
 Please allocate Compliance Number to all transactions with {dateLabel} < {ZDate.Today.ToShortDateString()}.";
					AssertEquals(expectedMessage, lastErrorReportedForPeriodicInvoice);
				}
			}
		}

		[TestDate(2021, 02, 15)]
		public void TestPeriodicInvoice_PostDateEarlierThanLastDateUsed_PST()
		{
			AssertPeriodicInvoice_PostDateEarlierThanLastDateUsed(AccountingMasterFilesConstants.ComplianceNumberAllocationDateOptions.PostDate.Code);
		}

		[TestDate(2021, 02, 15)]
		public void TestPeriodicInvoice_PostDateEarlierThanLastDateUsed_INV()
		{
			AssertPeriodicInvoice_PostDateEarlierThanLastDateUsed(AccountingMasterFilesConstants.ComplianceNumberAllocationDateOptions.InvoiceDate.Code);
		}

		void AssertPeriodicInvoice_PostDateEarlierThanLastDateUsed(string dateOption)
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Italy))
			{
				ZDate lastDateUsed = new ZDate(2021, 2, 10);
				var sequence = SetComplianceSequence(new ZDate(2021, 12, 31));

				var previousInvoice = TestObjectCreator.CreateInvoice(typeof(ARInvoice), "INV002", TestObjectCreator.EUR, 1M, TestObjectCreator.Debtor);
				previousInvoice.AH_XD_ComplianceBook = sequence.PK;
				previousInvoice.AH_InvoiceDate = previousInvoice.AH_PostDate = lastDateUsed;

				Factory.Save();

				var periodicInvoice = SetupForPeriodInvoiceTest();

				if (dateOption == AccountingMasterFilesConstants.ComplianceNumberAllocationDateOptions.InvoiceDate.Code)
				{
					periodicInvoice.InvoiceDate = new ZDate(2021, 2, 1);
				}
				else
				{
					periodicInvoice.PostDate = new ZDate(2021, 2, 1);
				}

				var postManager = periodicInvoice.PostManager;

				var registryInstance = AccountingMasterFilesRegistry.Instance;
				var companyPK = GlbCompany.CurrentCompany.PK.ToGuid();

				using (registryInstance.ComplianceDocumentNumberAllocation_Receivables.SetTemporaryValue(companyPK, Guid.Empty, Guid.Empty, AccComplianceSequenceLookups.ComplianceDocumentNumberAllocationSettingCodes.Post))
				using (registryInstance.ComplianceNumberAllocationDate_AR.SetTemporaryValue(companyPK, Guid.Empty, Guid.Empty, dateOption))
				{
					AssertEquals(0, postManager.Poster.PostedInvoices.Count);
					postManager.CreateTransactions(JobInvoicingPostingOption.Revenue);

					AssertEquals(true, postManager.CancelPosting);

					var dateLabel = dateOption == AccountingMasterFilesConstants.ComplianceNumberAllocationDateOptions.PostDate.Code ? "Post" : "Invoice";
					string expectedMessage = $@"Compliance Numbers cannot be allocated.
 Last posted transaction with the same Compliance Sub Type {ItalyComplianceInfo.ComplianceSubTypeCodes.ARI} has {dateLabel} Date = {lastDateUsed.ToShortDateString()}, that is greater than the current one(s).";
					AssertEquals(expectedMessage, lastErrorReportedForPeriodicInvoice);
				}
			}
		}

		AccComplianceSequence SetComplianceSequence(ZDate expiryDate)
		{
			var menuPK = Factory.LoadTop1<StmMenuItem>(new ZQuery(StmMenuItemSchema.SU_MenuName, "Cost Confirmation Document")).PK;

			var complianceSequence = TestObjectCreator.SetupComplianceSequence(menuPK, ItalyComplianceInfo.ComplianceSubTypeCodes.ARI, ItalyComplianceInfo.ComplianceSubTypeCodes.ARI, 1, 100, 2);
			complianceSequence.XD_StartDate = new ZDate(2021, 1, 1);
			complianceSequence.XD_ExpiryDate = expiryDate;
			complianceSequence.XD_IsActive = true;

			Factory.Save();

			return complianceSequence;
		}

		string lastErrorReportedForPeriodicInvoice = string.Empty;

		(Job job, AccCashAdvanceRequestHeader FirstCAH, AccCashAdvanceRequestHeader SecondCAH) SetupForPeriodInvoiceWithCashAdvance(bool shouldHaveMissingLine = false)
		{
			var periodHelper = new AccountingPeriodTestHelper(Factory);
			periodHelper.SetupPeriods();

			var localClient = TestObjectCreator.LocalClient;
			var addressPK = TestObjectCreator.CreateAddress(localClient, OrgAddressType.Office, true).PK;

			using (Job job = TestObjectCreator.CreateJob(TestObjectCreator.CreateShipment("S001")))
			{
				TestObjectCreator.CreateOrgInvoiceType(localClient.CompanyData, "ALL", "ALL", "ALL", ZString.Empty, "INV", "INV");

				var charge1 = CreateChargeForTest(job, addressPK, TestObjectCreator.CC1, 250M, 500M, TestObjectCreator.LocalClient.PK);
				var charge2 = CreateChargeForTest(job, addressPK, TestObjectCreator.CC2, 900M, 900M, TestObjectCreator.LocalClient.PK);
				var charge3 = CreateChargeForTest(job, addressPK, TestObjectCreator.CC3, 150M, 320M, TestObjectCreator.LocalClient.PK);
				var charge4 = CreateChargeForTest(job, addressPK, TestObjectCreator.CC4, 350M, 700M, TestObjectCreator.LocalClient.PK);
				if (shouldHaveMissingLine)
				{
					charge4.JR_InvoiceType = InvoiceTypesList.Codes.FinalInvoice;
				}

				var header1 = TestObjectCreator.CreateCashAdvanceRequestHeader(job, TestObjectCreator.Debtor, LedgerTypes.AccountsReceivable, 250M, 250M, "AUD", "0000001");
				TestObjectCreator.CreateCashAdvanceRequestLine(header1, 250M, 250M, CashAdvanceStatusCodes.RequestLine.Requested, charge1);
				TestObjectCreator.CreateCashAdvanceRequestLine(header1, 900M, 900M, CashAdvanceStatusCodes.RequestLine.Requested, charge2);

				var header2 = TestObjectCreator.CreateCashAdvanceRequestHeader(job, TestObjectCreator.Debtor1, LedgerTypes.AccountsReceivable, 250M, 250M, "AUD", "0000002");
				TestObjectCreator.CreateCashAdvanceRequestLine(header2, 150M, 320M, CashAdvanceStatusCodes.RequestLine.Requested, charge3);
				TestObjectCreator.CreateCashAdvanceRequestLine(header2, 350M, 700M, CashAdvanceStatusCodes.RequestLine.Requested, charge4);

				Factory.Save();
				return (job, header1, header2);
			}
		}

		PeriodicInvoice SetupForPeriodInvoiceTest(ZDateTime? postDate = null, ZDateTime? invoiceDate = null)
		{
			lastErrorReportedForPeriodicInvoice = "";

			GlbBranch.CurrentBranch.GB_RL_NKHomePort = "ITMIL";

			var periodicInvoice = new PeriodicInvoice(Factory);
			periodicInvoice.PostManager.OnCriticalPostError += (object sender, CriticalPostingErrorEventArgs e) => lastErrorReportedForPeriodicInvoice = e.ErrorMessage;

			var localClient = TestObjectCreator.LocalClient;
			var addressPK = TestObjectCreator.CreateAddress(localClient, OrgAddressType.Office, true).PK;

			using (Job job = TestObjectCreator.CreateJob(TestObjectCreator.CreateShipment("S001")))
			{
				TestObjectCreator.CreateOrgInvoiceType(localClient.CompanyData, "ALL", "ALL", "ALL", ZString.Empty, "INV", "INV");

				CreateChargeForTest(job, addressPK, TestObjectCreator.CC1, 250M, 500M);
				CreateChargeForTest(job, addressPK, TestObjectCreator.CC2, 900M, 900M);
				CreateChargeForTest(job, addressPK, TestObjectCreator.CC3, 150M, 320M);
				CreateChargeForTest(job, addressPK, TestObjectCreator.CC4, 350M, 700M);

				Factory.Save();
			}
			var periodHelper = new AccountingPeriodTestHelper(Factory);
			periodHelper.SetupPeriods();

			periodicInvoice.DebtorPK = localClient.PK;

			if (invoiceDate.HasValue)
			{
				periodicInvoice.InvoiceDate = invoiceDate.Value;
			}
			if (postDate.HasValue)
			{
				periodicInvoice.PostDate = postDate.Value;
			}
			periodicInvoice.LoadJobs();

			AssertEquals(1, periodicInvoice.Jobs.Count);
			AssertEquals(4, periodicInvoice.Charges.Count);
			return periodicInvoice;
		}

		Charge CreateChargeForTest(Job job, ZGuid addressPK, AccChargeCode chargeCode, decimal osCostAmt, decimal osSellAmt, ZGuid? debtorPK = null)
		{
			var charge = TestObjectCreator.CreateCharge(job, chargeCode, osCostAmt, osSellAmt);
			charge.JR_OH_SellAccount = debtorPK ?? TestObjectCreator.LocalClient.PK;
			charge.JR_OA_SellInvoiceAddress = addressPK;
			charge.JR_InvoiceType = InvoiceTypesList.Codes.FinalInvoice_Batching;
			return charge;
		}

		Job SetupDataForExporterExemption(bool expired = false)
		{
			var stampDutyRecharge = new StampDutyRecharge
			{
				StampDutyRechargeOrganizationType = StampDutyRechargeOrganizationType.NotRecharging,
				StampDutyRechargeTransactionType = StampDutyRechargeTransactionType.All
			};
			AccountingConfigurationRegistry.Instance.StampDutyRecharge.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, stampDutyRecharge);

			var query = new ZQuery(AccTaxRateSchema.AT_IsActive, true);
			query.AddToFilter(AccTaxRateSchema.AT_RN_NKCountry, CountryCodes.Italy);
			query.AddToFilter(AccTaxRateSchema.AT_Code, "DICH.INT");
			dichIntTaxRate = Factory.LoadTop1<AccTaxRate>(query);
			dichIntTaxRate.SetRate_ForTestOnly(12, 1, ZDate.Today.AddDays(-1), ZDate.Today.AddDays(1));

			var dichInvoice = TestObjectCreator.CreateInvoice(typeof(ARInvoice), "INV002", TestObjectCreator.EUR, 1M, TestObjectCreator.Debtor);
			var line = TestObjectCreator.CreateInvoiceLine(dichInvoice, TestObjectCreator.EUR, 1M, 700M, 70M, 0M);
			line.AL_AT = dichIntTaxRate.PK;

			var dichInvoice1 = TestObjectCreator.CreateInvoice(typeof(APInvoice), "INV003", TestObjectCreator.EUR, 1M, TestObjectCreator.Creditor1);
			var line1 = TestObjectCreator.CreateInvoiceLine(dichInvoice1, TestObjectCreator.EUR, 1M, 700M, 70M, 0M);
			line1.AL_AT = dichIntTaxRate.PK;

			var dateReceived1 = expired ? ZDate.Today.AddDays(-30) : ZDate.Today.AddDays(-1);
			var validToDate1 = expired ? ZDate.Today.AddDays(-1) : ZDate.Today.AddDays(30);
			var validToDate2 = expired ? ZDate.Today.AddDays(-1) : ZDate.Today.AddDays(29);

			CreateEXVDocument(dichInvoice, JobRequiredDocument.DocUsage.Debtor, CountryCodes.Italy, "000001", dateReceived1, validToDate1, "1000");
			CreateEXVDocument(dichInvoice, JobRequiredDocument.DocUsage.Debtor, CountryCodes.Italy, "000002", dateReceived1, validToDate2, "1000");
			CreateEXVDocument(dichInvoice, JobRequiredDocument.DocUsage.Debtor, CountryCodes.UnitedStates, "000003", ZDate.Today.AddDays(-1), ZDate.Today.AddDays(28));
			CreateEXVDocument(dichInvoice1, JobRequiredDocument.DocUsage.Creditor, CountryCodes.Italy, "000004", ZDate.Today.AddDays(-1), ZDate.Today.AddDays(30), "1000");
			CreateEXVDocument(dichInvoice1, JobRequiredDocument.DocUsage.Creditor, CountryCodes.Italy, "000005", ZDate.Today.AddDays(-1), ZDate.Today.AddDays(29), "1000");
			CreateEXVDocument(dichInvoice1, JobRequiredDocument.DocUsage.Creditor, CountryCodes.UnitedStates, "000006", ZDate.Today.AddDays(-1), ZDate.Today.AddDays(28));

			Factory.Save();

			var periodHelper = new AccountingPeriodTestHelper(new BusinessObjectFactory());
			periodHelper.SetupPeriods();

			var orgInvoiceType = TestObjectCreator.Debtor.CompanyData.InvoiceTypes.AddNew();
			orgInvoiceType.PI_Module = JobInvoicingConsumerTypes.Shipment.Code;
			orgInvoiceType.PI_Interval = InvoiceTypeBillingInterval.Codes.MTH;
			orgInvoiceType.PI_StartDay = InvoiceTypeMonthCommencement.Codes.LMH;
			orgInvoiceType.PI_Type = InvoiceTypeLayoutList.Codes.INV;
			orgInvoiceType.PI_RS_NKServiceLevel = "STD";

			var orgInvTypeDeferredCharges = orgInvoiceType.DeferredCharges.AddNew();
			orgInvTypeDeferredCharges.PO_AC = TestObjectCreator.MRG100.PK;

			var shipment = TestObjectCreator.CreateJobPlugIn(JobInvoicingConsumerTypes.Shipment);
			var job = TestObjectCreator.CreateJob(shipment, TestObjectCreator.Debtor, 10, TestObjectCreator.ZECTRA, 10);
			job.JH_UniqueJobInvoiceNumber = 6;

			Factory.Save();

			return job;

			void CreateEXVDocument(InvoicingBase invoice, ZString docUsage, ZString country, ZString docNumber, ZDateTime dateReceived, ZDateTime validToDate, string ceilingLimit = null)
			{
				var doc = invoice.Header.RequiredDocuments.AddNew();
				doc.EQ_DocCategory = ReferenceTypes.ClientSupplierRelationship;
				doc.EQ_DocType = RefDocTypes.VATExporterExemption;
				doc.EQ_DocUsage = docUsage;
				doc.EQ_RN_NKRelatedCountry = country;
				doc.EQ_DocNumber = docNumber;
				doc.EQ_DateReceived = dateReceived.ToDateTimeOffset(null);
				doc.EQ_ValidToDate = validToDate;

				if (ceilingLimit != null)
				{
					var ceilingLimitAttribute = doc.Attributes.AddNew();
					ceilingLimitAttribute.D0_AttribName = JobRequiredDocAttribTypeList.Codes.CeilingLimit;
					ceilingLimitAttribute.D0_AttribValue = ceilingLimit;
				}
			}
		}

		(PeriodicInvoice periodicInvoice, IReadOnlyList<Job> jobs, IReadOnlyList<InvoicingBase>) CreatePeriodicInvoiceWithJobsAndMiscInvoices(int jobCount = 1, int invoiceCount = 0)
		{
			TestObjectCreator.CreateTestPeriods(ZDateTime.Today);

			var debtor = TestObjectCreator.CreateOrgHeader("TSTORG", false, true);
			var type = debtor.CompanyData.InvoiceTypes.AddNew();
			type.PI_Module = "ALL";
			type.PI_Interval = InvoiceTypeBillingInterval.Codes.MTH;
			type.PI_Type = InvoiceTypeLayoutList.Codes.CHG;

			var jobs = new List<Job>();
			for (int i = 0; i < jobCount; i++)
			{
				var shipment = TestObjectCreator.CreateShipment(string.Format("S90001{0}", i.ToString().PadLeft(3, '0')));
				shipment.JS_RS_NKServiceLevel = "STD";
				var job = TestObjectCreator.CreateJob(shipment);
				job.LocalChargesPK = TestObjectCreator.ABIGAS.PK;
				jobs.Add(job);

				var charge = job.Charges.AddNew();
				charge.FillWithValidTestData();
				charge.JR_AC = TestObjectCreator.CC1.PK;
				charge.JR_OH_SellAccount = debtor.PK;
				charge.JR_OSSellAmt = 100m;
				charge.JR_LocalSellAmt = 100m;
				charge.JR_InvoiceType = InvoiceTypesList.Codes.FinalInvoice_Batching;
			}

			var invoices = new List<InvoicingBase>();
			for (int i = 0; i < invoiceCount; i++)
			{
				InvoicingBase arInvoice = Factory.NewWithValidTestData<ARInvoice>();
				arInvoice.AH_OH = debtor.PK;
				arInvoice.AH_RX_NKTransactionCurrency = TestObjectCreator.AUD.RX_Code;
				arInvoice.AH_TransactionCategory = "FIN";
				invoices.Add(arInvoice);

				TestObjectCreator.CreateInvoiceLine(arInvoice, TestObjectCreator.AUD, 1M, 70).AL_OSTaxAmount = 60M;
			}

			Factory.Save();

			var periodicInvoice = new PeriodicInvoice(Factory);
			periodicInvoice.DebtorPK = debtor.PK;
			periodicInvoice.PostDate = ZDateTime.Today;
			periodicInvoice.DueDate = ZDateTime.Today.AddDays(30);
			periodicInvoice.InvoiceDate = ZDateTime.Today;
			periodicInvoice.InvoiceTerm = InvoiceTermsList.CashOnDelivery.Code;
			periodicInvoice.InvoiceTermDays = 30;
			periodicInvoice.InvoiceType = InvoiceTypesList.Codes.FinalInvoice_Batching;
			periodicInvoice.Jobs.AddRange(jobs);
			periodicInvoice.MiscInvoices.AddRange(invoices);

			return (periodicInvoice, jobs, invoices);
		}

		AccTaxRate dichIntTaxRate;

		#region Implementation

		PeriodicInvoice TestPeriodicInvoice
		{
			get { return (PeriodicInvoice)testPeriodicInvoice_internalValue; }
		}

		#endregion
	}
}
