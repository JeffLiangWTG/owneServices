using System;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing.Periodic_Invoicing;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using static Enterprise.Core.Constants;

namespace Enterprise.Accounting.Business.ARAP.Invoicing.Testing
{
	[TestedType(typeof(PeriodicInvoiceBulk))]
	public class PeriodicInvoiceBulkTest : PeriodicInvoiceBaseTest
	{
		public void TestFilterBusinessObject()
		{
			AssertEquals("FilterBusinessObject should be PeriodicInvoiceBulkJobsFilterBusinessObject", typeof(PeriodicInvoiceBulkJobsFilterBusinessObject), TestPeriodicInvoice.JobsFilter.GetType());
		}

		AuthorizationModeAndSettings SetAuthorizationLevelSettings()
		{
			var registryValue = TestObjectCreator.CreateAuthorizationModeAndSettings(100m, 200m);
			AccountingConfigurationRegistry.Instance.ReceivableAuthorizationModeAndSettings.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, registryValue);
			return registryValue;
		}

		public void TestCreateBulkPeriodicInvoiceAndCheckSecurityRights_UserHasRights()
		{
			TestObjectCreator.CreateTestPeriods(ZDateTime.Today);
			TestObjectCreator.CreateJobShipmentWithFIDCharge("S001", TestObjectCreator.LocalClient, TestObjectCreator.CC1, 100M, -1000M);

			var testPeriodicInvoice = new PeriodicInvoiceBulk(Factory);
			testPeriodicInvoice.CurrencyNK = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;
			testPeriodicInvoice.LoadJobs();
			TestObjectCreator.LocalClient.CompanyData.ClearInvoiceTypeCache_ForTestOnly();

			AssertEquals(1, testPeriodicInvoice.PeriodicInvoices.Count);
			Assert(!testPeriodicInvoice.CheckLevelSecurityRightsForPeriodicCreditNotes());
			AssertNoRowError(testPeriodicInvoice.PeriodicInvoices[0], PeriodicInvoiceBulk.unauthorizedCreditNoteErrorMessage);

			using (AccountingConfigurationRegistry.Instance.ReceivableAuthorizationModeAndSettings.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, new AuthorizationModeAndSettings { AuthorizationMode = AuthorizationMode.Codes.TwoApprovers }))
			{
				Assert(testPeriodicInvoice.CheckLevelSecurityRightsForPeriodicCreditNotes());
				AssertHasRowError(testPeriodicInvoice.PeriodicInvoices[0], PeriodicInvoiceBulk.enforceTwoUserApprovalEnabledErrorMessage);
			}
		}

		public void TestInvoiceDate_ReadOnly()
		{
			Env.Security.NewReceivablesBulkPeriodicInvoiceDate.IsAllowed = true;
			Assert("Should not be readonly as the security right is true.", !TestPeriodicInvoice.InvoiceDateInfo.ReadOnly);

			Env.Security.NewReceivablesBulkPeriodicInvoiceDate.IsAllowed = false;
			Assert("Should be readonly as the security right is false.", TestPeriodicInvoice.InvoiceDateInfo.ReadOnly);

			using (AccountingConfigurationRegistry.Instance.InvAndPstDateDefaultingBehaviour.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, AccountingConstants.InvAndPstDateDefaultingRuleTypes.MonthEndSuspension.Code))
			{
				Env.Security.NewReceivablesBulkPeriodicInvoiceDate.IsAllowed = true;
				Assert("Should be readonly as the registry is set to MTH and then it doesn't matter to the security right.", TestPeriodicInvoice.InvoiceDateInfo.ReadOnly);

				Env.Security.NewReceivablesBulkPeriodicInvoiceDate.IsAllowed = false;
				Assert("Should be readonly as the registry is set to MTH and then it doesn't matter to the security right.", TestPeriodicInvoice.InvoiceDateInfo.ReadOnly);
			}
		}

		public void TestCreateBulkPeriodicInvoiceAndCheckSecurityRights_UserHasNoRights()
		{
			var originalFirstAmountLevelAllows = Env.Security.CreditAdjustmentNotePostingApprovalFirstLevelApproval.IsAllowed;
			var originalSecondAmountLevelAllows = Env.Security.CreditAdjustmentNotePostingApprovalSecondLevelApproval.IsAllowed;
			try
			{
				var registry = SetAuthorizationLevelSettings();
				Env.Security.CreditAdjustmentNotePostingApprovalFirstLevelApproval.IsAllowed = false;
				Env.Security.CreditAdjustmentNotePostingApprovalSecondLevelApproval.IsAllowed = false;

				TestObjectCreator.LocalClient.CompanyData.OB_ARVATConfig = AccountingMasterFilesConstants.OrganisationTaxConfiguartionTypes.Default.Code;
				TestObjectCreator.LocalClient2.CompanyData.OB_ARVATConfig = AccountingMasterFilesConstants.OrganisationTaxConfiguartionTypes.Default.Code;
				TestObjectCreator.ABIGAS.CompanyData.OB_ARVATConfig = AccountingMasterFilesConstants.OrganisationTaxConfiguartionTypes.Default.Code;

				TestObjectCreator.CreateTestPeriods(ZDateTime.Today);
				TestObjectCreator.CreateJobShipmentWithFIDCharge("S001", TestObjectCreator.LocalClient, TestObjectCreator.CC1, 100M, -1000M);
				TestObjectCreator.CreateJobShipmentWithFIDCharge("S002", TestObjectCreator.LocalClient2, TestObjectCreator.CC1, 100M, -50M);
				TestObjectCreator.CreateJobShipmentWithFIDCharge("S003", TestObjectCreator.ABIGAS, TestObjectCreator.CC1, 100M, -900M);

				var testPeriodicInvoice = new PeriodicInvoiceBulk(Factory);
				testPeriodicInvoice.CurrencyNK = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;
				testPeriodicInvoice.LoadJobs();
				TestObjectCreator.LocalClient.CompanyData.ClearInvoiceTypeCache_ForTestOnly();
				TestObjectCreator.LocalClient2.CompanyData.ClearInvoiceTypeCache_ForTestOnly();
				TestObjectCreator.ABIGAS.CompanyData.ClearInvoiceTypeCache_ForTestOnly();

				AssertEquals(3, testPeriodicInvoice.PeriodicInvoices.Count);
				Assert(testPeriodicInvoice.CheckLevelSecurityRightsForPeriodicCreditNotes());
				var invoiceForLocalClient = testPeriodicInvoice.PeriodicInvoices.First(x => ((PeriodicInvoice)x).DebtorPK == TestObjectCreator.LocalClient.PK);
				Assert(invoiceForLocalClient.HasRowErrors);
				Assert(invoiceForLocalClient.RowErrors.Contains(PeriodicInvoiceBulk.unauthorizedCreditNoteErrorMessage));
				var invoiceForLocalClient2 = testPeriodicInvoice.PeriodicInvoices.First(x => ((PeriodicInvoice)x).DebtorPK == TestObjectCreator.LocalClient2.PK);
				AssertNoRowError(invoiceForLocalClient2, PeriodicInvoiceBulk.unauthorizedCreditNoteErrorMessage);
				var invoiceForABIGAS = testPeriodicInvoice.PeriodicInvoices.First(x => ((PeriodicInvoice)x).DebtorPK == TestObjectCreator.ABIGAS.PK);
				Assert(invoiceForABIGAS.HasRowErrors);
				Assert(invoiceForABIGAS.RowErrors.Contains(PeriodicInvoiceBulk.unauthorizedCreditNoteErrorMessage));

				((PeriodicInvoice)invoiceForLocalClient).IncludeInThePeriodicInvoice = false;
				AssertNoRowError(invoiceForLocalClient, PeriodicInvoiceBulk.unauthorizedCreditNoteErrorMessage);
				Assert(testPeriodicInvoice.CheckLevelSecurityRightsForPeriodicCreditNotes());
				AssertNoRowError(invoiceForLocalClient, PeriodicInvoiceBulk.unauthorizedCreditNoteErrorMessage);
				AssertNoRowError(invoiceForLocalClient2, PeriodicInvoiceBulk.unauthorizedCreditNoteErrorMessage);
				Assert(invoiceForABIGAS.HasRowErrors);
				Assert(invoiceForABIGAS.RowErrors.Contains(PeriodicInvoiceBulk.unauthorizedCreditNoteErrorMessage));

				((PeriodicInvoice)invoiceForABIGAS).IncludeInThePeriodicInvoice = false;
				AssertNoRowError(invoiceForABIGAS, PeriodicInvoiceBulk.unauthorizedCreditNoteErrorMessage);
				Assert(!testPeriodicInvoice.CheckLevelSecurityRightsForPeriodicCreditNotes());
				AssertNoRowError(invoiceForLocalClient, PeriodicInvoiceBulk.unauthorizedCreditNoteErrorMessage);
				AssertNoRowError(invoiceForLocalClient2, PeriodicInvoiceBulk.unauthorizedCreditNoteErrorMessage);
				AssertNoRowError(invoiceForABIGAS, PeriodicInvoiceBulk.unauthorizedCreditNoteErrorMessage);

				registry.AuthorizationMode = AuthorizationMode.Codes.TwoApprovers;
				using (AccountingConfigurationRegistry.Instance.ReceivableAuthorizationModeAndSettings.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, registry))
				{
					((PeriodicInvoice)invoiceForABIGAS).IncludeInThePeriodicInvoice = true;
					((PeriodicInvoice)invoiceForLocalClient).IncludeInThePeriodicInvoice = false;
					((PeriodicInvoice)invoiceForLocalClient2).IncludeInThePeriodicInvoice = false;
					Assert(testPeriodicInvoice.CheckLevelSecurityRightsForPeriodicCreditNotes());
					AssertHasRowError(invoiceForABIGAS, PeriodicInvoiceBulk.enforceTwoUserApprovalEnabledErrorMessage);
					AssertNoRowError(invoiceForLocalClient, PeriodicInvoiceBulk.enforceTwoUserApprovalEnabledErrorMessage);
					AssertNoRowError(invoiceForLocalClient2, PeriodicInvoiceBulk.enforceTwoUserApprovalEnabledErrorMessage);
				}

				registry.AuthorizationMode = AuthorizationMode.Codes.SequentialApprovers;
				using (AccountingConfigurationRegistry.Instance.ReceivableAuthorizationModeAndSettings.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, registry))
				{
					((PeriodicInvoice)invoiceForABIGAS).IncludeInThePeriodicInvoice = true;
					((PeriodicInvoice)invoiceForLocalClient).IncludeInThePeriodicInvoice = false;
					((PeriodicInvoice)invoiceForLocalClient2).IncludeInThePeriodicInvoice = false;
					Assert(testPeriodicInvoice.CheckLevelSecurityRightsForPeriodicCreditNotes());
					AssertHasRowError(invoiceForABIGAS, PeriodicInvoiceBulk.enforceSequentialUserApprovalEnabledErrorMessage);
					AssertNoRowError(invoiceForLocalClient, PeriodicInvoiceBulk.enforceSequentialUserApprovalEnabledErrorMessage);
					AssertNoRowError(invoiceForLocalClient2, PeriodicInvoiceBulk.enforceSequentialUserApprovalEnabledErrorMessage);
				}
			}
			finally
			{
				Env.Security.CreditAdjustmentNotePostingApprovalFirstLevelApproval.IsAllowed = originalFirstAmountLevelAllows;
				Env.Security.CreditAdjustmentNotePostingApprovalSecondLevelApproval.IsAllowed = originalSecondAmountLevelAllows;
			}
		}

		public void TestCreateBulkPeriodicInvoiceAndCheckSecurityRights_UserHasNoRights_HasMiscInvoices()
		{
			var originalFirstAmountLevelAllows = Env.Security.CreditAdjustmentNotePostingApprovalFirstLevelApproval.IsAllowed;
			var originalSecondAmountLevelAllows = Env.Security.CreditAdjustmentNotePostingApprovalSecondLevelApproval.IsAllowed;
			try
			{
				var registry = SetAuthorizationLevelSettings();
				Env.Security.CreditAdjustmentNotePostingApprovalFirstLevelApproval.IsAllowed = false;
				Env.Security.CreditAdjustmentNotePostingApprovalSecondLevelApproval.IsAllowed = false;

				TestObjectCreator.CreateTestPeriods(ZDateTime.Today);
				var arInvoice = Factory.NewWithValidTestData<ARInvoice>();
				arInvoice.AH_OSTotalAmount = 2000M;
				arInvoice.AH_OH = TestObjectCreator.LocalClient.PK;

				var arCreditNote = Factory.NewWithValidTestData<ARCreditNote>();
				arCreditNote.AH_OSTotalAmount = 1000M;
				arCreditNote.AH_OH = TestObjectCreator.LocalClient2.PK;
				Factory.Save();

				var testPeriodicInvoice = new PeriodicInvoiceBulk(Factory);
				testPeriodicInvoice.CurrencyNK = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;
				testPeriodicInvoice.LoadMiscInvoices();
				TestObjectCreator.LocalClient.CompanyData.ClearInvoiceTypeCache_ForTestOnly();
				testPeriodicInvoice.MiscInvoices.Add(arInvoice);
				testPeriodicInvoice.MiscInvoices.Add(arCreditNote);

				AssertEquals(2, testPeriodicInvoice.PeriodicInvoices.Count);
				Assert(!testPeriodicInvoice.CheckLevelSecurityRightsForPeriodicCreditNotes());
				AssertNoRowError(testPeriodicInvoice.PeriodicInvoices[0], PeriodicInvoiceBulk.unauthorizedCreditNoteErrorMessage);
				AssertNoRowError(testPeriodicInvoice.PeriodicInvoices[1], PeriodicInvoiceBulk.unauthorizedCreditNoteErrorMessage);
			}
			finally
			{
				Env.Security.CreditAdjustmentNotePostingApprovalFirstLevelApproval.IsAllowed = originalFirstAmountLevelAllows;
				Env.Security.CreditAdjustmentNotePostingApprovalSecondLevelApproval.IsAllowed = originalSecondAmountLevelAllows;
			}
		}

		public void TestCreateBulkPeriodicInvoiceAndCheckSecurityRights_BasedOnLineLevel()
		{
			var registry = SetAuthorizationLevelSettings();
			AccountingConfigurationRegistry.Instance.EnableLineLevelApprovalRequestForARCreditNote.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			var staff = TestObjectCreator.CreateStaffWithSecurityRights("newuser", "tst", Env.Security.APInvoiceApproval.Code, "password", false);
			TestObjectCreator.SetUpCreditAdjustmentNotePostingApprovalLevelForBranchDepartment(GlbDepartment.CurrentDepartment.PK.ToGuid(), GlbBranch.CurrentBranch.PK.ToGuid(), staff.PK, true);
			TestObjectCreator.SetUpCreditAdjustmentNotePostingApprovalLevelForBranchDepartment(TestObjectCreator.FIADepartment.PK.ToGuid(), TestObjectCreator.NonCurrentBranch.PK.ToGuid(), staff.PK, false);

			var staff2 = TestObjectCreator.CreateStaffWithSecurityRights("newuser2", "ts2", Env.Security.APInvoiceApproval.Code, "password2", false);
			TestObjectCreator.SetUpCreditAdjustmentNotePostingApprovalLevelForBranchDepartment(GlbDepartment.CurrentDepartment.PK.ToGuid(), GlbBranch.CurrentBranch.PK.ToGuid(), staff2.PK, true);
			TestObjectCreator.SetUpCreditAdjustmentNotePostingApprovalLevelForBranchDepartment(TestObjectCreator.FIADepartment.PK.ToGuid(), TestObjectCreator.NonCurrentBranch.PK.ToGuid(), staff2.PK, true);

			TestObjectCreator.CreateTestPeriods(ZDateTime.Today);
			TestObjectCreator.CreateJobShipmentWithFIDCharge("S001", TestObjectCreator.LocalClient, TestObjectCreator.CC1, 100M, -700M, true);
			TestObjectCreator.CreateJobShipmentWithFIDCharge("S002", TestObjectCreator.LocalClient2, TestObjectCreator.CC1, 100M, -900M, true);

			var testPeriodicInvoice = new PeriodicInvoiceBulk(Factory);
			testPeriodicInvoice.CurrencyNK = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;
			testPeriodicInvoice.LoadJobs();
			TestObjectCreator.LocalClient.CompanyData.ClearInvoiceTypeCache_ForTestOnly();
			TestObjectCreator.LocalClient2.CompanyData.ClearInvoiceTypeCache_ForTestOnly();

			using (Env.SetTemporaryUserContext(staff.GS_LoginName, GlbBranch.CurrentBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
			{
				AssertEquals(2, testPeriodicInvoice.PeriodicInvoices.Count);
				Assert(testPeriodicInvoice.CheckLevelSecurityRightsForPeriodicCreditNotes());
				var invoiceForLocalClient = testPeriodicInvoice.PeriodicInvoices.First(x => ((PeriodicInvoice)x).DebtorPK == TestObjectCreator.LocalClient.PK);
				AssertHasRowError(invoiceForLocalClient, PeriodicInvoiceBulk.unauthorizedCreditNoteErrorMessage);
				var invoiceForLocalClient2 = testPeriodicInvoice.PeriodicInvoices.First(x => ((PeriodicInvoice)x).DebtorPK == TestObjectCreator.LocalClient2.PK);
				AssertHasRowError(invoiceForLocalClient2, PeriodicInvoiceBulk.unauthorizedCreditNoteErrorMessage);
			}

			using (Env.SetTemporaryUserContext(staff2.GS_LoginName, GlbBranch.CurrentBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
			{
				AssertEquals(2, testPeriodicInvoice.PeriodicInvoices.Count);
				Assert(!testPeriodicInvoice.CheckLevelSecurityRightsForPeriodicCreditNotes());
				var invoiceForLocalClient = testPeriodicInvoice.PeriodicInvoices.First(x => ((PeriodicInvoice)x).DebtorPK == TestObjectCreator.LocalClient.PK);
				AssertNoRowError(invoiceForLocalClient, PeriodicInvoiceBulk.unauthorizedCreditNoteErrorMessage);
				var invoiceForLocalClient2 = testPeriodicInvoice.PeriodicInvoices.First(x => ((PeriodicInvoice)x).DebtorPK == TestObjectCreator.LocalClient2.PK);
				AssertNoRowError(invoiceForLocalClient2, PeriodicInvoiceBulk.unauthorizedCreditNoteErrorMessage);
			}
		}

		public void TestAuthorizationNotRequiredWhenNoCreditNotesExist()
		{
			var originalFirstAmountLevelAllows = Env.Security.CreditAdjustmentNotePostingApprovalFirstLevelApproval.IsAllowed;
			var originalSecondAmountLevelAllows = Env.Security.CreditAdjustmentNotePostingApprovalSecondLevelApproval.IsAllowed;
			try
			{
				var registry = SetAuthorizationLevelSettings();
				Env.Security.CreditAdjustmentNotePostingApprovalFirstLevelApproval.IsAllowed = false;
				Env.Security.CreditAdjustmentNotePostingApprovalSecondLevelApproval.IsAllowed = false;

				TestObjectCreator.CreateTestPeriods(ZDateTime.Today);
				TestObjectCreator.CreateJobShipmentWithFIDCharge("S001", TestObjectCreator.LocalClient, TestObjectCreator.CC1, 100M, 1000M);

				var testPeriodicInvoice = new PeriodicInvoiceBulk(Factory);
				testPeriodicInvoice.CurrencyNK = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;
				testPeriodicInvoice.LoadJobs();
				TestObjectCreator.LocalClient.CompanyData.ClearInvoiceTypeCache_ForTestOnly();

				AssertEquals(1, testPeriodicInvoice.PeriodicInvoices.Count);
				Assert("No Authoirization should be required", !testPeriodicInvoice.CheckLevelSecurityRightsForPeriodicCreditNotes());
				AssertNoRowError(testPeriodicInvoice.PeriodicInvoices[0], PeriodicInvoiceBulk.unauthorizedCreditNoteErrorMessage);

				Env.Security.CreditAdjustmentNotePostingApprovalFirstLevelApproval.IsAllowed = true;
				Env.Security.CreditAdjustmentNotePostingApprovalSecondLevelApproval.IsAllowed = true;

				Assert("No Authoirization should be required", !testPeriodicInvoice.CheckLevelSecurityRightsForPeriodicCreditNotes());
				AssertNoRowError(testPeriodicInvoice.PeriodicInvoices[0], PeriodicInvoiceBulk.unauthorizedCreditNoteErrorMessage);

				registry.AuthorizationMode = AuthorizationMode.Codes.TwoApprovers;
				using (AccountingConfigurationRegistry.Instance.ReceivableAuthorizationModeAndSettings.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, registry))
				{
					Assert("No Authoirization should be required", !testPeriodicInvoice.CheckLevelSecurityRightsForPeriodicCreditNotes());
					AssertNoRowError(testPeriodicInvoice.PeriodicInvoices[0], PeriodicInvoiceBulk.unauthorizedCreditNoteErrorMessage);

					Env.Security.CreditAdjustmentNotePostingApprovalFirstLevelApproval.IsAllowed = false;
					Env.Security.CreditAdjustmentNotePostingApprovalSecondLevelApproval.IsAllowed = false;

					Assert("No Authoirization should be required", !testPeriodicInvoice.CheckLevelSecurityRightsForPeriodicCreditNotes());
					AssertNoRowError(testPeriodicInvoice.PeriodicInvoices[0], PeriodicInvoiceBulk.unauthorizedCreditNoteErrorMessage);
				}
			}
			finally
			{
				Env.Security.CreditAdjustmentNotePostingApprovalFirstLevelApproval.IsAllowed = originalFirstAmountLevelAllows;
				Env.Security.CreditAdjustmentNotePostingApprovalSecondLevelApproval.IsAllowed = originalSecondAmountLevelAllows;
			}
		}

		public void TestPeriodicInvoiceGeneration()
		{
			var newFactory = new BusinessObjectFactory();
			var newTestObjectCreator = new TestObjectCreator(newFactory);
			TestPeriodicInvoice.CurrencyNK = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;
			ARInvoice invoice = newFactory.NewWithValidTestData<ARInvoice>();
			ARInvoice invoice2 = newFactory.NewWithValidTestData<ARInvoice>();
			invoice.AH_TransactionCategory = "FIN";
			invoice.AH_OH = newTestObjectCreator.Agent.PK;
			invoice2.AH_TransactionCategory = "FIN";
			invoice2.AH_OH = newTestObjectCreator.LocalClient.PK;
			newTestObjectCreator.CreateInvoiceLine(invoice, newTestObjectCreator.AUD, 1M, 50M).AL_OSTaxAmount = 60M;
			newTestObjectCreator.CreateInvoiceLine(invoice2, newTestObjectCreator.AUD, 1M, 500M).AL_OSTaxAmount = 600M;

			using (Job job = newTestObjectCreator.CreateJob(newTestObjectCreator.CreateShipment("S001")))
			{
				Charge charge = newFactory.NewWithValidTestData<Charge>();
				Charge charge2 = newFactory.NewWithValidTestData<Charge>();
				charge.JR_JH = job.PK;
				charge2.JR_JH = job.PK;
				charge.JR_OH_SellAccount = newTestObjectCreator.LocalClient.PK;
				charge2.JR_OH_SellAccount = newTestObjectCreator.LocalClient2.PK;
				charge.JR_InvoiceType = InvoiceTypesList.Codes.FinalInvoice_Batching;
				charge2.JR_InvoiceType = InvoiceTypesList.Codes.FinalInvoice_Batching;
				charge.JR_AT_SellGSTRate = newTestObjectCreator.GST1.PK;
				charge2.JR_AT_SellGSTRate = newTestObjectCreator.GST1.PK;
				charge.JR_OSSellAmt = 30;
				charge2.JR_OSSellAmt = 300;

				newFactory.Save();

				int jobDbHits = Factory.GetTableHitCount(Job.Schema.TableName);
				int chargeDbHits = Factory.GetTableHitCount(Charge.Schema.TableName);
				int miscInvoiceDbHits = Factory.GetTableHitCount(ARInvoice.Schema.TableName);

				TestPeriodicInvoice.LoadJobs();
				TestPeriodicInvoice.LoadMiscInvoices();

				AssertEquals("Job table db hits", 1, Factory.GetTableHitCount(Job.Schema.TableName) - jobDbHits);
				AssertEquals("Charge table db hits", 1, Factory.GetTableHitCount(Charge.Schema.TableName) - chargeDbHits);
				AssertEquals("Misc Invoice db hits", 1, Factory.GetTableHitCount(ARInvoice.Schema.TableName) - miscInvoiceDbHits);

				AssertEquals(1, TestPeriodicInvoice.Jobs.Count);
				AssertEquals(2, TestPeriodicInvoice.Charges.Count);
				AssertEquals(2, TestPeriodicInvoice.MiscInvoices.Count);
				AssertEquals(3, TestPeriodicInvoice.PeriodicInvoices.Count);

				AssertEquals(1, TestPeriodicInvoice.PeriodicInvoices[0].Jobs.Count);
				AssertEquals(1, TestPeriodicInvoice.PeriodicInvoices[0].Charges.Count);
				AssertEquals(1, TestPeriodicInvoice.PeriodicInvoices[0].MiscInvoices.Count);

				AssertEquals(1, TestPeriodicInvoice.PeriodicInvoices[1].Jobs.Count);
				AssertEquals(1, TestPeriodicInvoice.PeriodicInvoices[1].Charges.Count);
				AssertEquals(0, TestPeriodicInvoice.PeriodicInvoices[1].MiscInvoices.Count);

				AssertEquals(0, TestPeriodicInvoice.PeriodicInvoices[2].Jobs.Count);
				AssertEquals(0, TestPeriodicInvoice.PeriodicInvoices[2].Charges.Count);
				AssertEquals(1, TestPeriodicInvoice.PeriodicInvoices[2].MiscInvoices.Count);

				AssertEquals("All invoices should be in the same factory to reduce memory usage", TestPeriodicInvoice.PeriodicInvoices[0].Factory, TestPeriodicInvoice.PeriodicInvoices[1].Factory);
				AssertEquals("All invoices should be in the same factory to reduce memory usage", TestPeriodicInvoice.PeriodicInvoices[0].Factory, TestPeriodicInvoice.PeriodicInvoices[2].Factory);
			}
		}

		public override void TestReloadChargesByJob()
		{
			using (var job = TestObjectCreator.CreateJob(TestObjectCreator.CreateShipment("S00001234")))
			{
				var charge = CreateCharge(job, TestObjectCreator.CC1, TestObjectCreator.AUD, 30m, TestObjectCreator.ABIGAS, TestObjectCreator.GSTWithExtraRate, InvoiceTypesList.Codes.FinalInvoice_Batching);
				var charge2 = CreateCharge(job, TestObjectCreator.CC1, TestObjectCreator.AUD, 300m, TestObjectCreator.ABIGAS, TestObjectCreator.GSTWithExtraRate, InvoiceTypesList.Codes.FinalInvoice_Batching);
				Factory.Save();

				TestPeriodicInvoice.CurrencyNK = "AUD";
				SetupPeriodicInvoiceWithJobsAndMiscInvoices(TestPeriodicInvoice, new ZGuid[] { job.PK }, new ZGuid[] { charge.PK, charge2.PK }, new ZGuid[] { Guid.Empty, Guid.Empty }); // This method is used here as an action rather than as a setup. Please call appropriate TestPeriodicInvoice.LoadXXX() method when this test is next modified.

				AssertEquals(1, TestPeriodicInvoice.Jobs.Count);
				AssertEquals(2, TestPeriodicInvoice.Charges.Count);
				AssertEquals(0, TestPeriodicInvoice.MiscInvoices.Count);
				AssertEquals(1, TestPeriodicInvoice.PeriodicInvoices.Count);

				var periodicInvoice = TestPeriodicInvoice.PeriodicInvoices[0];

				AssertEquals("OSExTaxAmount", 330m, periodicInvoice.OSExTaxAmount);
				AssertEquals("OSTaxAmount", 40.79m, periodicInvoice.OSTaxAmount);
				AssertEquals("OSTotalAmount", 370.79m, periodicInvoice.OSTotalAmount);
				AssertEquals("OSExtraTaxAmount", 1.19m, periodicInvoice.OSExtraTaxAmount);
				AssertEquals("LocalExTaxAmount", 330m, periodicInvoice.LocalExTaxAmount);
				AssertEquals("LocalTaxAmount", 40.79m, periodicInvoice.LocalTaxAmount);
				AssertEquals("LocalTotalAmount", 370.79m, periodicInvoice.LocalTotalAmount);
				AssertEquals("LocalExtraTaxAmount", 1.19m, periodicInvoice.LocalExtraTaxAmount);

				var newFactory = new BusinessObjectFactory();
				var loadedCharge = newFactory.Load<JobCharge>(charge2.PK);
				loadedCharge.JR_OSSellAmt = 500;
				loadedCharge.JR_LocalSellAmt = 500;
				newFactory.Save();

				TestPeriodicInvoice.ReloadChargesByJob(job.PK);

				AssertEquals("OSExTaxAmount", 530m, periodicInvoice.OSExTaxAmount);
				AssertEquals("OSTaxAmount", 65.51m, periodicInvoice.OSTaxAmount);
				AssertEquals("OSTotalAmount", 595.51m, periodicInvoice.OSTotalAmount);
				AssertEquals("OSExtraTaxAmount", 1.91m, periodicInvoice.OSExtraTaxAmount);
				AssertEquals("LocalExTaxAmount", 530m, periodicInvoice.LocalExTaxAmount);
				AssertEquals("LocalTaxAmount", 65.51m, periodicInvoice.LocalTaxAmount);
				AssertEquals("LocalTotalAmount", 595.51m, periodicInvoice.LocalTotalAmount);
				AssertEquals("LocalExtraTaxAmount", 1.91m, periodicInvoice.LocalExtraTaxAmount);

				AssertEquals("OSExTaxAmount", 530m, TestPeriodicInvoice.OSExTaxAmount);
				AssertEquals("OSTaxAmount", 65.51m, TestPeriodicInvoice.OSTaxAmount);
				AssertEquals("OSTotalAmount", 595.51m, TestPeriodicInvoice.OSTotalAmount);
				AssertEquals("OSExtraTaxAmount", 1.91m, TestPeriodicInvoice.OSExtraTaxAmount);
				AssertEquals("LocalExTaxAmount", 530m, TestPeriodicInvoice.LocalExTaxAmount);
				AssertEquals("LocalTaxAmount", 65.51m, TestPeriodicInvoice.LocalTaxAmount);
				AssertEquals("LocalTotalAmount", 595.51m, TestPeriodicInvoice.LocalTotalAmount);
				AssertEquals("LocalExtraTaxAmount", 1.91m, TestPeriodicInvoice.LocalExtraTaxAmount);
			}
		}

		public override void TestReloadChargesByJobWithNoCharge()
		{
			using (var job = TestObjectCreator.CreateJob(TestObjectCreator.CreateShipment("S00001235")))
			{
				var charge = CreateCharge(job, TestObjectCreator.CC1, TestObjectCreator.AUD, 50m, TestObjectCreator.ABIGAS, TestObjectCreator.GSTWithExtraRate, InvoiceTypesList.Codes.FinalInvoice_Batching);
				Factory.Save();

				TestPeriodicInvoice.CurrencyNK = "AUD";
				SetupPeriodicInvoiceWithJobsAndMiscInvoices(TestPeriodicInvoice, new ZGuid[] { job.PK }, new ZGuid[] { charge.PK }, new ZGuid[] { Guid.Empty, Guid.Empty }); // This method is used here as an action rather than as a setup. Please call appropriate TestPeriodicInvoice.LoadXXX() method when this test is next modified.

				AssertEquals(1, TestPeriodicInvoice.Jobs.Count);
				AssertEquals(1, TestPeriodicInvoice.Charges.Count);
				AssertEquals(0, TestPeriodicInvoice.MiscInvoices.Count);
				AssertEquals(1, TestPeriodicInvoice.PeriodicInvoices.Count);

				var periodicInvoice = TestPeriodicInvoice.PeriodicInvoices[0];

				AssertEquals("OSExTaxAmount", 50m, periodicInvoice.OSExTaxAmount);
				AssertEquals("OSTaxAmount", 6.18m, periodicInvoice.OSTaxAmount);
				AssertEquals("OSTotalAmount", 56.18m, periodicInvoice.OSTotalAmount);
				AssertEquals("OSExtraTaxAmount", 0.18m, periodicInvoice.OSExtraTaxAmount);
				AssertEquals("LocalExTaxAmount", 50m, periodicInvoice.LocalExTaxAmount);
				AssertEquals("LocalTaxAmount", 6.18m, periodicInvoice.LocalTaxAmount);
				AssertEquals("LocalTotalAmount", 56.18m, periodicInvoice.LocalTotalAmount);
				AssertEquals("LocalExtraTaxAmount", 0.18m, periodicInvoice.LocalExtraTaxAmount);

				var newFactory = new BusinessObjectFactory();
				var loadedCharge = newFactory.Load<JobCharge>(charge.PK);
				loadedCharge.JR_InvoiceType = InvoiceTypesList.Codes.FinalInvoice;
				newFactory.Save();

				TestPeriodicInvoice.ReloadChargesByJob(job.PK);

				AssertEquals("OSExTaxAmount", 0m, periodicInvoice.OSExTaxAmount);
				AssertEquals("OSTaxAmount", 0m, periodicInvoice.OSTaxAmount);
				AssertEquals("OSTotalAmount", 0m, periodicInvoice.OSTotalAmount);
				AssertEquals("OSExtraTaxAmount", 0m, periodicInvoice.OSExtraTaxAmount);
				AssertEquals("LocalExTaxAmount", 0m, periodicInvoice.LocalExTaxAmount);
				AssertEquals("LocalTaxAmount", 0m, periodicInvoice.LocalTaxAmount);
				AssertEquals("LocalTotalAmount", 0m, periodicInvoice.LocalTotalAmount);
				AssertEquals("LocalExtraTaxAmount", 0m, periodicInvoice.LocalExtraTaxAmount);
			}
		}

		public void TestFilterJobsAndChargesWithTaxBranchOnAfterChangeLines()
		{
			using (AccountingMasterFilesRegistry.Instance.EnableTaxBranchReporting.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, true))
			{
				var branch1 = TestObjectCreator.CreateBranch("001", GlbCompany.CurrentCompany);
				var branch2 = TestObjectCreator.CreateBranch("002", GlbCompany.CurrentCompany);

				TestObjectCreator.CreateTestPeriods(ZDateTime.Today);
				TestObjectCreator.CreateJobShipmentWithFIDCharge("S001", TestObjectCreator.LocalClient, TestObjectCreator.CC1, 100M, 1000M, true);
				TestObjectCreator.CreateJobShipmentWithFIDCharge("S002", TestObjectCreator.LocalClient, TestObjectCreator.CC1, 200M, 2000M, true);

				var job1 = Factory.LoadTop1<Job>(new ZQuery(JobHeaderSchema.JH_JobNum, "S001"));
				var job2 = Factory.LoadTop1<Job>(new ZQuery(JobHeaderSchema.JH_JobNum, "S002"));

				job1.JH_GB_TaxBranch = branch1.PK;
				job2.JH_GB_TaxBranch = branch2.PK;

				var charge1 = job1.Charges[0];
				var charge2 = job1.Charges[1];
				var charge3 = job2.Charges[0];
				var charge4 = job2.Charges[1];

				charge1.JR_GB_SellTaxBranch = branch1.PK;

				Factory.Save();

				AssertEquals(branch1.PK, charge1.JR_GB_SellTaxBranch);
				AssertEquals(GlbBranch.CurrentBranch.PK, charge2.JR_GB_SellTaxBranch);
				AssertEquals(GlbBranch.CurrentBranch.PK, charge3.JR_GB_SellTaxBranch);
				AssertEquals(GlbBranch.CurrentBranch.PK, charge4.JR_GB_SellTaxBranch);

				var newFactory = new BusinessObjectFactory();
				var bulkPeriodicInvoice = new PeriodicInvoiceBulk(newFactory);
				bulkPeriodicInvoice.CurrencyNK = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;
				var filter = (ModuleGuidFilter)bulkPeriodicInvoice.JobsFilter["Job Header Tax Branch"];
				filter.Property = branch1.PK;
				filter.IsActive = true;

				bulkPeriodicInvoice.LoadJobs();
				TestObjectCreator.LocalClient.CompanyData.ClearInvoiceTypeCache_ForTestOnly();

				AssertEquals(1, bulkPeriodicInvoice.PeriodicInvoices.Count);

				var periodicInvoice = bulkPeriodicInvoice.PeriodicInvoices[0];

				AssertEquals(2, periodicInvoice.InitializedCharges.Count);
				Assert(periodicInvoice.InitializedCharges.Any(x => x.PK == charge1.PK));
				Assert(periodicInvoice.InitializedCharges.Any(x => x.PK == charge2.PK));

				AssertEquals(1, periodicInvoice.Charges.Count);
				AssertEquals(periodicInvoice.Charges[0].PK, charge2.PK);

				AssertEquals(1, periodicInvoice.Jobs.Count);
				AssertEquals(1100M, periodicInvoice.LocalTotalAmount);
				AssertEquals(1100M, bulkPeriodicInvoice.LocalTotalAmount);
			}
		}

		public void TestBulkInvoiceFieldsPopulatesToChildPeriodicInvoicesOnChange()
		{
			TestPeriodicInvoice.CurrencyNK = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;
			ARInvoice invoice = Factory.NewWithValidTestData<ARInvoice>();
			invoice.AH_TransactionCategory = "FIN";
			invoice.AH_OH = TestObjectCreator.Agent.PK;
			TestObjectCreator.CreateInvoiceLine(invoice, TestObjectCreator.AUD, 1M, 50M).AL_OSTaxAmount = 60M;

			using (Job job = TestObjectCreator.CreateJob(TestObjectCreator.CreateShipment("S001")))
			{
				Charge charge = Factory.NewWithValidTestData<Charge>();
				charge.JR_JH = job.PK;
				charge.JR_OH_SellAccount = TestObjectCreator.LocalClient.PK;
				charge.JR_InvoiceType = InvoiceTypesList.Codes.FinalInvoice_Batching;
				charge.JR_AT_SellGSTRate = TestObjectCreator.GST1.PK;
				charge.JR_OSSellAmt = 30;

				Factory.Save();

				TestPeriodicInvoice.LoadJobs();
				TestPeriodicInvoice.LoadMiscInvoices();

				AssertEquals("Precondition: ", 2, TestPeriodicInvoice.PeriodicInvoices.Count);

				TestPeriodicInvoice.InvoiceDate = ZDateTime.Now.AddDays(-2);
				TestPeriodicInvoice.PostDate = ZDateTime.Now.AddDays(-1);

				AssertEquals("InvoiceDate must be populated to all invoices in a bulk.", TestPeriodicInvoice.InvoiceDate, TestPeriodicInvoice.PeriodicInvoices[0].InvoiceDate);
				AssertEquals("InvoiceDate must be populated to all invoices in a bulk.", TestPeriodicInvoice.InvoiceDate, TestPeriodicInvoice.PeriodicInvoices[1].InvoiceDate);
				AssertEquals("PostDate must be populated to all invoices in a bulk.", TestPeriodicInvoice.PostDate, TestPeriodicInvoice.PeriodicInvoices[0].PostDate);
				AssertEquals("PostDate must be populated to all invoices in a bulk.", TestPeriodicInvoice.PostDate, TestPeriodicInvoice.PeriodicInvoices[1].PostDate);
			}
		}

		[TestDate(2013, 02, 01)]
		public void TestIncludeInThePeriodicInvoiceIsDifferentPerInvoiceForSameJob()
		{
			TestObjectCreator.CreateTestPeriods(new ZDateTime(2013, 01, 01));
			foreach (var client in new[] { TestObjectCreator.ABIGAS, TestObjectCreator.LocalClient })
			{
				var periodicInvoiceType = client.CompanyData.InvoiceTypes.AddNew();
				periodicInvoiceType.PI_Module = JobInvoicingConsumerTypes.Shipment.Code;
				periodicInvoiceType.PI_Interval = "MTH";
				periodicInvoiceType.PI_StartDay = "LMH";
				periodicInvoiceType.PI_Type = "CHG";
				periodicInvoiceType.PI_RS_NKServiceLevel = "STD";
			}

			TestPeriodicInvoice.CurrencyNK = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;
			var job1 = TestObjectCreator.CreateJob(TestObjectCreator.CreateShipment("S001"));
			var job1charge_localClient = TestObjectCreator.CreateCharge(job1, TestObjectCreator.CC1, "", TestObjectCreator.AUD, 10m, TestObjectCreator.AALSHI, TestObjectCreator.AUD, 30m, TestObjectCreator.LocalClient);
			job1charge_localClient.JR_InvoiceType = InvoiceTypesList.Codes.FinalInvoice_Batching;
			var job1charge_ABIGAS = TestObjectCreator.CreateCharge(job1, TestObjectCreator.CC1, "", TestObjectCreator.AUD, 10m, TestObjectCreator.AALSHI, TestObjectCreator.AUD, 60m, TestObjectCreator.ABIGAS);
			job1charge_ABIGAS.JR_InvoiceType = InvoiceTypesList.Codes.FinalInvoice_Batching;
			var job2 = TestObjectCreator.CreateJob(TestObjectCreator.CreateShipment("S002"));
			var job2charge_localClient = TestObjectCreator.CreateCharge(job2, TestObjectCreator.CC1, "", TestObjectCreator.AUD, 10m, TestObjectCreator.AALSHI, TestObjectCreator.AUD, 45m, TestObjectCreator.LocalClient);
			job2charge_localClient.JR_InvoiceType = InvoiceTypesList.Codes.FinalInvoice_Batching;

			Factory.Save();

			TestPeriodicInvoice.InvoiceDate = ZDateTime.Now.AddDays(-2);
			TestPeriodicInvoice.PostDate = ZDateTime.Now.AddDays(-1);
			TestPeriodicInvoice.LoadJobs();
			TestPeriodicInvoice.LoadMiscInvoices();

			AssertEquals("Precondition: number of periodic invoices", 2, TestPeriodicInvoice.PeriodicInvoices.Count);

			var localClientInvoice = FindPeriodInvoiceForDebtor(TestPeriodicInvoice, TestObjectCreator.LocalClient.PK);
			var aBIGASInvoice = FindPeriodInvoiceForDebtor(TestPeriodicInvoice, TestObjectCreator.ABIGAS.PK);

			AssertEquals("Precondition: localClientInvoice has 2 job rows ", 2, localClientInvoice.Jobs.Count);
			AssertEquals("Precondition: ABIGASInvoice has 1 job row ", 1, aBIGASInvoice.Jobs.Count);
			AssertEquals("Precondition: selectable jobs selected by default", true, localClientInvoice.Jobs[0].IncludeInThePeriodicInvoice);
			AssertEquals("Precondition: selectable jobs selected by default", true, localClientInvoice.Jobs[1].IncludeInThePeriodicInvoice);
			AssertEquals("Precondition: selectable jobs selected by default", true, aBIGASInvoice.Jobs[0].IncludeInThePeriodicInvoice);

			var localClientJob1Row = FindSelectableJobForActualJob(localClientInvoice, job1.PK);
			var localClientJob2Row = FindSelectableJobForActualJob(localClientInvoice, job2.PK);
			var aBIGASJob1Row = FindSelectableJobForActualJob(aBIGASInvoice, job1.PK);

			AssertEquals("Both selectable jobs reference the same job in memory", localClientJob1Row.Parent, aBIGASJob1Row.Parent);
			Assert("Selectable jobs are distinct entities", localClientJob1Row != aBIGASJob1Row);
			localClientJob1Row.IncludeInThePeriodicInvoice = false;
			AssertEquals("localClientJob1Row now unticked", false, localClientJob1Row.IncludeInThePeriodicInvoice);
			AssertEquals("Other rows unaffected by the unticking of localClientJob1Row", true, localClientJob2Row.IncludeInThePeriodicInvoice);
			AssertEquals("Other rows unaffected by the unticking of localClientJob1Row", true, aBIGASJob1Row.IncludeInThePeriodicInvoice);

			var periodicInvoiceBulkPoster = TestPeriodicInvoice.CreatePeriodicInvoiceBulkPoster();
			periodicInvoiceBulkPoster.Post();

			var f2 = new BusinessObjectFactory();
			var allInvoices = f2.Load<ARInvoice>(new ZQuery());
			var localClientPostedInvoice = f2.LoadTop1<ARInvoice>(new ZQuery(AccTransactionHeaderSchema.AH_OH, TestObjectCreator.LocalClient.PK));
			var aBIGASPostedInvoice = f2.LoadTop1<ARInvoice>(new ZQuery(AccTransactionHeaderSchema.AH_OH, TestObjectCreator.ABIGAS.PK));
			var job1charge_localClientReloaded = f2.Load<Charge>(job1charge_localClient.PK);
			var job1charge_ABIGASReloaded = f2.Load<Charge>(job1charge_ABIGAS.PK);
			var job2charge_localClientReloaded = f2.Load<Charge>(job2charge_localClient.PK);

			AssertEquals("Only 2 invoices were created", 2, allInvoices.Length);
			AssertEquals("The invoice posted for localClient has 1 line only", 1, localClientPostedInvoice.Lines.Count);
			AssertEquals("The invoice posted for localClient has correct amount", 45m, localClientPostedInvoice.AH_InvoiceAmount);
			AssertEquals("The invoice posted for ABIGAS has 1 line only", 1, aBIGASPostedInvoice.Lines.Count);
			AssertEquals("The invoice posted for ABIGAS has correct amount", 60m, aBIGASPostedInvoice.AH_InvoiceAmount);

			AssertEquals("The unselected charge was not posted", null, job1charge_localClientReloaded.ARLine.TransactionHeader);
			AssertEquals("The selected charges were posted", aBIGASPostedInvoice.PK, job1charge_ABIGASReloaded.ARLine.TransactionHeader.PK);
			AssertEquals("The selected charges were posted", localClientPostedInvoice.PK, job2charge_localClientReloaded.ARLine.TransactionHeader.PK);
		}

		public void TestPeriodicInvoiceBulkSplitting()
		{
			using (Job job = TestObjectCreator.CreateJob(TestObjectCreator.CreateShipment("S001")))
			{
				TestObjectCreator.CreateOrgInvoiceType(TestObjectCreator.LocalClient.CompanyData, "ALL", "ALL", "ALL", ZString.Empty, "INV", "INV");
				TestObjectCreator.CreateOrgInvoiceType(TestObjectCreator.LocalClient2.CompanyData, "ALL", "ALL", "ALL", ZString.Empty, "INV", "INV");

				var address = TestObjectCreator.CreateAddress(TestObjectCreator.LocalClient, OrgAddressType.Office, true);
				var address2 = TestObjectCreator.CreateAddress(TestObjectCreator.LocalClient2, OrgAddressType.Office, true);

				var charge = TestObjectCreator.CreateCharge(job, TestObjectCreator.CC1, 250M, 500M);
				charge.JR_OH_SellAccount = TestObjectCreator.LocalClient.PK;
				charge.JR_OA_SellInvoiceAddress = address.PK;
				charge.JR_InvoiceType = InvoiceTypesList.Codes.FinalInvoice_Batching;

				charge = TestObjectCreator.CreateCharge(job, TestObjectCreator.CC2, 900M, 900M);
				charge.JR_OH_SellAccount = TestObjectCreator.LocalClient.PK;
				charge.JR_OA_SellInvoiceAddress = address.PK;
				charge.JR_InvoiceType = InvoiceTypesList.Codes.FinalInvoice_Batching;

				charge = TestObjectCreator.CreateCharge(job, TestObjectCreator.CC3, 150M, 320M);
				charge.JR_OH_SellAccount = TestObjectCreator.LocalClient2.PK;
				charge.JR_OA_SellInvoiceAddress = address2.PK;
				charge.JR_InvoiceType = InvoiceTypesList.Codes.FinalInvoice_Batching;

				charge = TestObjectCreator.CreateCharge(job, TestObjectCreator.FRT, 350M, 700M);
				charge.JR_OH_SellAccount = TestObjectCreator.LocalClient2.PK;
				charge.JR_OA_SellInvoiceAddress = address2.PK;
				charge.JR_InvoiceType = InvoiceTypesList.Codes.FinalInvoice_Batching;

				Factory.Save();
			}

			var periodHelper = new AccountingPeriodTestHelper(Factory);
			periodHelper.SetupPeriods();

			AccountingConfigurationRegistry.Instance.JobInvoiceMaximumNumberOfChargesOfSplittingRules.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 0);
			AccountingConfigurationRegistry.Instance.JobInvoiceMaximumValueOfSplittingRules.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 1000);
			AccountingConfigurationRegistry.Instance.JobInvoiceAddressCountry.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "SAM");

			var periodicInvoiceBulk = new PeriodicInvoiceBulk(new BusinessObjectFactory());
			periodicInvoiceBulk.CurrencyNK = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;
			periodicInvoiceBulk.LoadJobs();

			AssertEquals(1, periodicInvoiceBulk.Jobs.Count);
			AssertEquals(4, periodicInvoiceBulk.Charges.Count);
			AssertEquals(2, periodicInvoiceBulk.PeriodicInvoices.Count);

			AssertEquals(1, periodicInvoiceBulk.PeriodicInvoices[0].Jobs.Count);
			AssertEquals(2, periodicInvoiceBulk.PeriodicInvoices[0].Charges.Count);

			AssertEquals(1, periodicInvoiceBulk.PeriodicInvoices[1].Jobs.Count);
			AssertEquals(2, periodicInvoiceBulk.PeriodicInvoices[1].Charges.Count);

			var query = new ZQuery(AccTransactionHeaderSchema.AH_TransactionType, TransactionTypes.Invoice);
			query.AddToFilter(AccTransactionHeaderSchema.AH_Ledger, LedgerTypes.AccountsReceivable);

			var beforePosting = Factory.GetDatabaseCount(typeof(TransactionHeader), query);
			AssertEquals(0, beforePosting);

			var poster = periodicInvoiceBulk.CreatePeriodicInvoiceBulkPoster();
			poster.Post();

			var afterPosting = Factory.GetDatabaseCount(typeof(TransactionHeader), query);
			AssertEquals(4, afterPosting);
		}

		[TestDate(2020, 10, 23)]
		public void TestPostedInvoiceLineTaxDate_UseInvoiceDate()
		{
			AssertPostedInvoiceLineTaxDate(TaxDateDefaultingOption.Code.InvoiceDate);
		}

		[TestDate(2020, 10, 23)]
		public void TestPostedInvoiceLineTaxDate_UseTodayDate()
		{
			AssertPostedInvoiceLineTaxDate(TaxDateDefaultingOption.Code.Today);
		}

		[TestDate(2020, 10, 23)]
		public void TestPostedInvoiceLineTaxDate_UseJobDate()
		{
			AssertPostedInvoiceLineTaxDate(TaxDateDefaultingOption.Code.EstimatedArrivalDate);
		}

		public void AssertPostedInvoiceLineTaxDate(string taxDateDefaultingOption)
		{
			TestObjectCreator.CreateTestPeriods(new ZDateTime(2020, 01, 01));
			foreach (var client in new[] { TestObjectCreator.ABIGAS, TestObjectCreator.LocalClient })
			{
				var periodicInvoiceType = client.CompanyData.InvoiceTypes.AddNew();
				periodicInvoiceType.PI_Module = JobInvoicingConsumerTypes.Shipment.Code;
				periodicInvoiceType.PI_Interval = "MTH";
				periodicInvoiceType.PI_StartDay = "LMH";
				periodicInvoiceType.PI_Type = "CHG";
				periodicInvoiceType.PI_RS_NKServiceLevel = "STD";
			}

			TestPeriodicInvoice.CurrencyNK = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;
			TestObjectCreator.LocalClient.CompanyData.OB_ARVATConfig = AccountingMasterFilesConstants.OrganisationTaxConfiguartionTypes.Default.Code;
			TestObjectCreator.ABIGAS.CompanyData.OB_ARVATConfig = AccountingMasterFilesConstants.OrganisationTaxConfiguartionTypes.Default.Code;
			var shipment1 = TestObjectCreator.CreateShipment("S00001003");
			shipment1.JS_E_ARV = new ZDateTime(2020, 10, 15);
			var job1 = TestObjectCreator.CreateJob(shipment1, TestObjectCreator.ABIGAS, 10M, TestObjectCreator.ZECTRA, 10M);
			var job1charge_localClient = TestObjectCreator.CreateCharge(job1, TestObjectCreator.CC1, "", TestObjectCreator.AUD, 10m, TestObjectCreator.AALSHI, TestObjectCreator.AUD, 30m, TestObjectCreator.LocalClient);
			job1charge_localClient.JR_AT_SellGSTRate = TestObjectCreator.GST1.PK;
			job1charge_localClient.JR_InvoiceType = InvoiceTypesList.Codes.FinalInvoice_Batching;
			var job1charge_ABIGAS = TestObjectCreator.CreateCharge(job1, TestObjectCreator.CC1, "", TestObjectCreator.AUD, 10m, TestObjectCreator.AALSHI, TestObjectCreator.AUD, 60m, TestObjectCreator.ABIGAS);
			job1charge_ABIGAS.JR_AT_SellGSTRate = TestObjectCreator.GST1.PK;
			job1charge_ABIGAS.JR_InvoiceType = InvoiceTypesList.Codes.FinalInvoice_Batching;
			var shipment2 = TestObjectCreator.CreateShipment("S00001004");
			shipment2.JS_E_ARV = new ZDateTime(2020, 10, 16);
			var job2 = TestObjectCreator.CreateJob(shipment2, TestObjectCreator.ABIGAS, 10M, TestObjectCreator.ZECTRA, 10M);
			var job2charge_localClient = TestObjectCreator.CreateCharge(job2, TestObjectCreator.CC1, "", TestObjectCreator.AUD, 10m, TestObjectCreator.AALSHI, TestObjectCreator.AUD, 45m, TestObjectCreator.LocalClient);
			job2charge_localClient.JR_AT_SellGSTRate = TestObjectCreator.GST1.PK;
			job2charge_localClient.JR_InvoiceType = InvoiceTypesList.Codes.FinalInvoice_Batching;

			Factory.Save();

			TestPeriodicInvoice.InvoiceDate = new ZDateTime(2020, 10, 20);
			TestPeriodicInvoice.PostDate = new ZDateTime(2020, 10, 15);
			TestPeriodicInvoice.LoadJobs();
			TestPeriodicInvoice.LoadMiscInvoices();

			AssertEquals("Precondition: number of periodic invoices", 2, TestPeriodicInvoice.PeriodicInvoices.Count);

			var collection = new TaxDateDefaultingOptionCollection();
			var taxDateOption = collection.AddNew();
			taxDateOption.JobType = "SHP";
			taxDateOption.DirectionCode = "ALL";
			taxDateOption.Mode = "ALL";
			taxDateOption.Ledger = "AR";
			taxDateOption.TaxDateOption = taxDateDefaultingOption;
			using (AccountingConfigurationRegistry.Instance.TaxDateDefaultingOption.SetTemporaryValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, collection))
			{
				var periodicInvoiceBulkPoster = TestPeriodicInvoice.CreatePeriodicInvoiceBulkPoster();
				periodicInvoiceBulkPoster.Post();

				var newFactory = new BusinessObjectFactory();
				var allInvoices = newFactory.Load<ARInvoice>(new ZQuery());
				var localClientPostedInvoice = newFactory.LoadTop1<ARInvoice>(new ZQuery(AccTransactionHeaderSchema.AH_OH, TestObjectCreator.LocalClient.PK));
				var aBIGASPostedInvoice = newFactory.LoadTop1<ARInvoice>(new ZQuery(AccTransactionHeaderSchema.AH_OH, TestObjectCreator.ABIGAS.PK));

				AssertEquals("Only 2 invoices were created", 2, allInvoices.Length);
				AssertEquals("The invoice posted for localClient has 2 lines", 2, localClientPostedInvoice.Lines.Count);
				AssertEquals("The invoice posted for ABIGAS has 1 line only", 1, aBIGASPostedInvoice.Lines.Count);
				switch (taxDateDefaultingOption)
				{
					case TaxDateDefaultingOption.Code.InvoiceDate:
						AssertEquals("Should use invoice date", new ZDate(2020, 10, 20), localClientPostedInvoice.Lines[0].AL_TaxDate);
						AssertEquals("Should use invoice date", new ZDate(2020, 10, 20), localClientPostedInvoice.Lines[1].AL_TaxDate);
						AssertEquals("Should use invoice date", new ZDate(2020, 10, 20), aBIGASPostedInvoice.Lines[0].AL_TaxDate);
						break;
					case TaxDateDefaultingOption.Code.Today:
						AssertEquals("Should use today's date", new ZDate(2020, 10, 23), localClientPostedInvoice.Lines[0].AL_TaxDate);
						AssertEquals("Should use today's date", new ZDate(2020, 10, 23), localClientPostedInvoice.Lines[1].AL_TaxDate);
						AssertEquals("Should use today's date", new ZDate(2020, 10, 23), aBIGASPostedInvoice.Lines[0].AL_TaxDate);
						break;
					case TaxDateDefaultingOption.Code.EstimatedArrivalDate:
						AssertEquals("Should use ETA from job S00001003", new ZDate(2020, 10, 15), localClientPostedInvoice.Lines.Cast<InvoicingLineBase>().First(x => x.Job.JH_JobNum == "S00001003").AL_TaxDate);
						AssertEquals("Should use ETA from job S00001004", new ZDate(2020, 10, 16), localClientPostedInvoice.Lines.Cast<InvoicingLineBase>().First(x => x.Job.JH_JobNum == "S00001004").AL_TaxDate);
						AssertEquals("Should use ETA from job S00001003", new ZDate(2020, 10, 15), aBIGASPostedInvoice.Lines.Cast<InvoicingLineBase>().First(x => x.Job.JH_JobNum == "S00001003").AL_TaxDate);
						break;
					default:
						Assert("Invalid tax date default option", false);
						break;
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

		public void TestPeriodicInvoiceBulkOnHold()
		{
			TestObjectCreator.CreateOrgInvoiceType(TestObjectCreator.LocalClient.CompanyData, "ALL", "ALL", "ALL", ZString.Empty, "INV", "INV");
			(new AccountingPeriodTestHelper(Factory)).SetupPeriods();

			TestPeriodicInvoice.CurrencyNK = TestObjectCreator.AUD.RX_Code;
			var expectedMessage = "Jobs with status 'Invoice on hold' or 'Work on hold' cannot be included in the invoice.";

			var job1 = TestPeriodicInvoiceOnHold_CreateJob("S001", JobHeaderStatus.WorkOnHold);
			TestPeriodicInvoice.LoadJobs();

			AssertEquals("One invoice loaded", 1, TestPeriodicInvoice.PeriodicInvoices.Count);

			var invoice = TestPeriodicInvoice.PeriodicInvoices[0];

			AssertEquals("One job loaded", 1, invoice.Jobs.Count);
			Assert("The job is excluded", !invoice.Jobs[0].IncludeInThePeriodicInvoice);
			AssertNoError(invoice.Jobs[0].IncludeInThePeriodicInvoiceInfo, expectedMessage);
			AssertHasWarning(invoice.Jobs[0].IncludeInThePeriodicInvoiceInfo, expectedMessage);
			AssertEquals("The invoice total is zero", ZDecimal.Zero, invoice.OSTotalAmount);
			AssertEquals("The job amount is zero", ZDecimal.Zero, invoice.Jobs[0].JH_OSAmountForPeriodicBilling);
			AssertEquals("The total is zero", ZDecimal.Zero, TestPeriodicInvoice.OSTotalAmount);

			invoice.Jobs[0].IncludeInThePeriodicInvoice = true;
			AssertHasError(invoice.Jobs[0].IncludeInThePeriodicInvoiceInfo, expectedMessage);
			AssertNoWarning(invoice.Jobs[0].IncludeInThePeriodicInvoiceInfo, expectedMessage);
			AssertEquals("The invoice total is updated", 120M, invoice.OSTotalAmount);
			AssertEquals("The job amount is updated after user action", 120M, invoice.Jobs[0].JH_OSAmountForPeriodicBilling);
			AssertEquals("The total is updated", 120M, TestPeriodicInvoice.OSTotalAmount);

			TestPeriodicInvoice.RunPreSaveValidation();
			AssertHasError(invoice.Jobs[0].IncludeInThePeriodicInvoiceInfo, expectedMessage);

			invoice.Jobs[0].IncludeInThePeriodicInvoice = false;
			AssertNoError(invoice.Jobs[0].IncludeInThePeriodicInvoiceInfo, expectedMessage);
			AssertNoWarning(invoice.Jobs[0].IncludeInThePeriodicInvoiceInfo, expectedMessage);

			var job2 = TestPeriodicInvoiceOnHold_CreateJob("S002", JobHeaderStatus.Working);
			TestPeriodicInvoice.LoadJobs();

			AssertEquals("One invoice loaded", 1, TestPeriodicInvoice.PeriodicInvoices.Count);

			invoice = TestPeriodicInvoice.PeriodicInvoices[0];

			AssertEquals("Two jobs loaded", 2, invoice.Jobs.Count);

			var selectableJobs = invoice.Jobs.Cast<PeriodicInvoiceSelectableJob>();
			var jobWHL = selectableJobs.Single(t => t.Parent.PK == job1.PK);
			var jobWRK = selectableJobs.Single(t => t.Parent.PK == job2.PK);

			Assert("The WHL job is excluded", !jobWHL.IncludeInThePeriodicInvoice);
			Assert("The WRK job is included", jobWRK.IncludeInThePeriodicInvoice);
			AssertNoError(jobWHL.IncludeInThePeriodicInvoiceInfo, expectedMessage);
			AssertHasWarning(jobWHL.IncludeInThePeriodicInvoiceInfo, expectedMessage);
			AssertEquals("The invoice total takes into account the excluded job", 120M, invoice.OSTotalAmount);
			AssertEquals("The WHL job has Zero charges", ZDecimal.Zero, jobWHL.JH_OSAmountForPeriodicBilling);
			AssertEquals("The WRK job is included", 120M, jobWRK.JH_OSAmountForPeriodicBilling);
			AssertEquals("The total is 120", 120M, TestPeriodicInvoice.OSTotalAmount);

			jobWHL.IncludeInThePeriodicInvoice = true;
			AssertHasError(jobWHL.IncludeInThePeriodicInvoiceInfo, expectedMessage);
			AssertNoWarning(jobWHL.IncludeInThePeriodicInvoiceInfo, expectedMessage);
			AssertEquals("The invoice total is updated", 240M, invoice.OSTotalAmount);
			AssertEquals("The WHL job has now charges after user action", 120M, jobWHL.JH_OSAmountForPeriodicBilling);
			AssertEquals("The total is updated", 240M, TestPeriodicInvoice.OSTotalAmount);

			TestPeriodicInvoice.RunPreSaveValidation();
			AssertHasError(jobWHL.IncludeInThePeriodicInvoiceInfo, expectedMessage);

			jobWHL.IncludeInThePeriodicInvoice = false;
			AssertNoError(jobWHL.IncludeInThePeriodicInvoiceInfo, expectedMessage);
			AssertHasWarning(jobWHL.IncludeInThePeriodicInvoiceInfo, expectedMessage);

			jobWRK.IncludeInThePeriodicInvoice = false;
			AssertNoError(jobWHL.IncludeInThePeriodicInvoiceInfo, expectedMessage);
			AssertNoWarning(jobWHL.IncludeInThePeriodicInvoiceInfo, expectedMessage);

			invoice.IncludeInThePeriodicInvoice = true;
			Assert("The WHL job is excluded", !jobWHL.IncludeInThePeriodicInvoice);
			Assert("The WRK job is included", jobWRK.IncludeInThePeriodicInvoice);
			AssertNoError(jobWHL.IncludeInThePeriodicInvoiceInfo, expectedMessage);
			AssertHasWarning(jobWHL.IncludeInThePeriodicInvoiceInfo, expectedMessage);

			var poster = TestPeriodicInvoice.CreatePeriodicInvoiceBulkPoster();
			poster.PostingProgress += TestPeriodicInvoiceOnHold_PostingProgress;
			TestPeriodicInvoiceOnHold_PostingProgress_Valid = false;
			poster.Post();
			Assert("The posting progress form should contain warning", TestPeriodicInvoiceOnHold_PostingProgress_Valid);
		}

		bool TestPeriodicInvoiceOnHold_PostingProgress_Valid;
		void TestPeriodicInvoiceOnHold_PostingProgress(int arg1, string arg2, bool arg3, Exception arg4)
		{
			if (arg2 == "Invoice ZLOCCLT, AUD, FID: AR INV 00001000 was posted.\r\nWarning! The following charges were excluded:\r\n\t - SHP S001 WHL")
			{
				TestPeriodicInvoiceOnHold_PostingProgress_Valid = true;
			}
		}

		public void TestPeriodicInvoiceBulk_CheckExporterExemption()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.Italy))
			using (AccountingMasterFilesRegistry.Instance.ValidateTaxIDApplicationForExporterExemptionItaly.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			{
				SetupDataForExporterExemption();

				CreateShipmentWithJobAndCharge(1, "Test charge 1", 100, dichIntTaxRate.PK);
				CreateShipmentWithJobAndCharge(2, "Test charge 2", 100, dichIntTaxRate.PK);
				TestPeriodicInvoice.CurrencyNK = TestObjectCreator.EUR.RX_Code;
				TestPeriodicInvoice.PostDate = ZDateTime.Now;
				TestPeriodicInvoice.LoadJobs();
				AssertEquals("One invoice loaded", 1, TestPeriodicInvoice.PeriodicInvoices.Count);
				var poster = TestPeriodicInvoice.CreatePeriodicInvoiceBulkPoster();
				poster.PostingProgress += TestExporterExemptionCeilingLimit_PostingProgress;
				poster.Post();

				AssertEquals("Invoice posted, no Ceiling Limit error", false, TestCeilingLimitErrorIsPresent);

				CreateShipmentWithJobAndCharge(3, "Test charge 3", 2000, dichIntTaxRate.PK);
				TestPeriodicInvoice.LoadJobs();
				poster = TestPeriodicInvoice.CreatePeriodicInvoiceBulkPoster();
				poster.PostingProgress += TestExporterExemptionCeilingLimit_PostingProgress;
				poster.Post();

				AssertEquals("Invoice not posted, Ceiling Limit error", true, TestCeilingLimitErrorIsPresent);
			}
		}

		bool TestCeilingLimitErrorIsPresent;
		void TestExporterExemptionCeilingLimit_PostingProgress(int arg1, string arg2, bool arg3, Exception arg4)
		{
			var expectedError = @"Invoice ZDebtor, EUR, FID: Transaction was not posted.
ZDebtor: There are errors that need to be corrected before saving the current Accounts Receivable Invoice.
TAX ID: DICH.INT, based on the Registry [Accounting -> Receivable Defaults -> Default Settings -> Validate Tax ID Application for Exporter Exemption (Italy)] it is not possible to proceed with the post because you are posting a transaction on a debtor that has one or more EXV-VAT/GST Exporter Exemption document with CEILING LIMIT.
The sum of the transactions that contain DICH.INT Tax ID including this one you are posting, exceeds the CEILING LIMIT by EUR 900.00.
To proceed with the post please fix Tax ID Code or save a new EXV-VAT/GST Exporter Exemption in the Debtor Organization eDocs, with an higher CEILING LIMIT or disable the Registry.";

			if (arg3 && arg2 == expectedError && arg4 == null)
			{
				TestCeilingLimitErrorIsPresent = true;
			}
		}

		public void TestPeriodicInvoiceBulk_CheckExporterExemption_NotValid()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.Italy))
			using (AccountingMasterFilesRegistry.Instance.ValidateTaxIDApplicationForExporterExemptionItaly.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			{
				SetupDataForExporterExemption(expired: true);

				CreateShipmentWithJobAndCharge(1, "Test charge 1", 100, dichIntTaxRate.PK);
				CreateShipmentWithJobAndCharge(2, "Test charge 2", 100, dichIntTaxRate.PK);

				TestPeriodicInvoice.CurrencyNK = TestObjectCreator.EUR.RX_Code;
				TestPeriodicInvoice.PostDate = ZDateTime.Now;
				TestPeriodicInvoice.LoadJobs();

				var poster = TestPeriodicInvoice.CreatePeriodicInvoiceBulkPoster();
				poster.PostingProgress += TestExporterExceptionNotValid_PostingProgress;
				poster.Post();

				AssertEquals("Invoice not posted, No EXV Documents valid Error", true, TestExporterExemptionNotValidIsPresent);
			}
		}

		bool TestExporterExemptionNotValidIsPresent;
		void TestExporterExceptionNotValid_PostingProgress(int arg1, string arg2, bool arg3, Exception arg4)
		{
			var expectedError = @"Invoice ZDebtor, EUR, FID: Transaction was not posted.
ZDebtor: If [Accounting -> Receivable Defaults -> Default Settings -> Validate Tax ID Application for Exporter Exemption (Italy)] is set to Yes, the DICH.INT Tax ID can only be used for a Debtor with a valid Exporter Exemption Certificate, where the certificate Ceiling Limit has not been exceeded.";

			if (arg3 && arg2 == expectedError && arg4 == null)
			{
				TestExporterExemptionNotValidIsPresent = true;
			}
		}

		public void TestPeriodicInvoiceBulk_CheckExporterExemption_StillAvailableAmount()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.Italy))
			using (AccountingMasterFilesRegistry.Instance.ValidateTaxIDApplicationForExporterExemptionItaly.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			{
				SetupDataForExporterExemption();

				CreateShipmentWithJobAndCharge(1, "Test charge 1", 50, TestObjectCreator.GST2.PK);

				TestPeriodicInvoice.CurrencyNK = TestObjectCreator.EUR.RX_Code;
				TestPeriodicInvoice.PostDate = ZDateTime.Now;
				TestPeriodicInvoice.LoadJobs();
				AssertEquals("One invoice loaded", 1, TestPeriodicInvoice.PeriodicInvoices.Count);
				var poster = TestPeriodicInvoice.CreatePeriodicInvoiceBulkPoster();
				poster.PostingProgress += TestExporterExemptionStillAvailableAmount_PostingProgress;
				poster.Post();

				AssertEquals("Invoice not posted, Still Available amount error", true, TestStillAvailableAmountErrorIsPresent);
			}
		}

		bool TestStillAvailableAmountErrorIsPresent;
		void TestExporterExemptionStillAvailableAmount_PostingProgress(int arg1, string arg2, bool arg3, Exception arg4)
		{
			var expectedError = @"Invoice ZDebtor, EUR, FID: Transaction was not posted.
ZDebtor: ZDebtor: There are errors that need to be corrected before this Accounts Receivable Invoice can be saved.
TAX ID: ZZGST2 based on the registry [Accounting -> Receivable Defaults -> Default Settings -> Validate Tax ID Application for Exporter Exemption (Italy)] it is not possible to proceed with the post because you are posting a transaction on a debtor that has one or more EXV-VAT/GST Exporter Exemption documents that still have available EUR 1300.00 CEILING LIMIT.
To proceed with the post fix Tax ID Code or disable the above registry item.";

			if (arg3 && arg2 == expectedError && arg4 == null)
			{
				TestStillAvailableAmountErrorIsPresent = true;
			}
		}

		void CreateShipmentWithJobAndCharge(ZShort jobInvoiceNum, string chargeDescription, decimal sellAmount, ZGuid taxRatePK)
		{
			var shipment = TestObjectCreator.CreateJobPlugIn(JobInvoicingConsumerTypes.Shipment);

			var job = TestObjectCreator.CreateJob(shipment, TestObjectCreator.Debtor, 10, TestObjectCreator.ZECTRA, 10);
			job.JH_UniqueJobInvoiceNumber = jobInvoiceNum;

			var charge = TestObjectCreator.CreateCharge(job, TestObjectCreator.MRG100, chargeDescription, null, 0, null, TestObjectCreator.EUR, sellAmount, TestObjectCreator.Debtor);
			charge.JR_InvoiceType = InvoiceTypesList.Codes.FinalInvoice_Batching;
			charge.JR_APInvoiceDate = charge.JR_PaymentDate = ZDateTime.Empty;
			charge.JR_AT_SellGSTRate = taxRatePK;
			charge.JR_SellTaxDate = ZDate.Today;
			Factory.Save();
		}

		[TestDate(2020, 10, 23)]
		public void TestPeriodicInvoiceBulkPostWithComplianceSequenceRelatedException()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.Portugal))
			using (AccountingMasterFilesRegistry.Instance.ComplianceNumberAllocationDate_AR.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, AccountingMasterFilesConstants.ComplianceNumberAllocationDateOptions.NoControl.Code))
			{
				var menuPK = TestObjectCreator.SetupComplianceMenuAndPivot("Govt Compliance Inv");
				var book = TestObjectCreator.SetupComplianceSequence(menuPK, "TXI", "ABC", 1, 100, 1);
				book.XD_PrintingAuthorizationNumber = "Test";
				Factory.Save();

				var invoice1 = TestObjectCreator.SetupComplianceInvoice("TXI", false, ZDateTime.Now.AddHours(1));
				Factory.Save();

				TestObjectCreator.CreateTestPeriods(new ZDateTime(2020, 01, 01));
				foreach (var client in new[] { TestObjectCreator.LocalClient })
				{
					var periodicInvoiceType = client.CompanyData.InvoiceTypes.AddNew();
					periodicInvoiceType.PI_Module = JobInvoicingConsumerTypes.Shipment.Code;
					periodicInvoiceType.PI_Interval = "MTH";
					periodicInvoiceType.PI_StartDay = "LMH";
					periodicInvoiceType.PI_Type = "CHG";
					periodicInvoiceType.PI_RS_NKServiceLevel = "STD";
				}

				TestPeriodicInvoice.CurrencyNK = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;
				TestObjectCreator.LocalClient.CompanyData.OB_ARVATConfig = AccountingMasterFilesConstants.OrganisationTaxConfiguartionTypes.Default.Code;
				var shipment1 = TestObjectCreator.CreateShipment("S00001003");
				shipment1.JS_E_ARV = new ZDateTime(2020, 10, 15);
				var job1 = TestObjectCreator.CreateJob(shipment1, TestObjectCreator.ABIGAS, 10M, TestObjectCreator.ZECTRA, 10M);
				var job1charge_localClient = TestObjectCreator.CreateCharge(job1, TestObjectCreator.CC1, "", TestObjectCreator.AUD, 10m, TestObjectCreator.AALSHI, TestObjectCreator.AUD, 30m, TestObjectCreator.LocalClient);
				job1charge_localClient.JR_AT_SellGSTRate = TestObjectCreator.GST1.PK;
				job1charge_localClient.JR_InvoiceType = InvoiceTypesList.Codes.FinalInvoice_Batching;

				Factory.Save();

				TestPeriodicInvoice.InvoiceDate = new ZDateTime(2020, 10, 20);
				TestPeriodicInvoice.PostDate = new ZDateTime(2020, 10, 15);
				TestPeriodicInvoice.LoadJobs();
				TestPeriodicInvoice.LoadMiscInvoices();

				AssertEquals("Precondition: number of periodic invoices", 2, TestPeriodicInvoice.PeriodicInvoices.Count);

				var collection = new TaxDateDefaultingOptionCollection();
				var taxDateOption = collection.AddNew();
				taxDateOption.JobType = "SHP";
				taxDateOption.DirectionCode = "ALL";
				taxDateOption.Mode = "ALL";
				taxDateOption.Ledger = "AR";
				taxDateOption.TaxDateOption = TaxDateDefaultingOption.Code.Today;
				using (AccountingConfigurationRegistry.Instance.TaxDateDefaultingOption.SetTemporaryValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, collection))
				{
					var periodicInvoiceBulkPoster = TestPeriodicInvoice.CreatePeriodicInvoiceBulkPoster();
					periodicInvoiceBulkPoster.PostingProgress += TestComplianceSequenceRelatedException_PostingProgress;
					periodicInvoiceBulkPoster.Post();

					AssertEquals(true, TestComplianceSequenceRelatedException_PostingProgress_Valid);
				}
			}
		}

		bool TestComplianceSequenceRelatedException_PostingProgress_Valid;
		void TestComplianceSequenceRelatedException_PostingProgress(int arg1, string arg2, bool arg3, Exception arg4)
		{
			if (arg3 && arg2 == "Invoice date must be equal or higher than previous document." && arg4 == null)
			{
				TestComplianceSequenceRelatedException_PostingProgress_Valid = true;
			}
		}

		void SetupDataForExporterExemption(bool expired = false)
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

			Factory.Save();

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

		AccTaxRate dichIntTaxRate;

		PeriodicInvoiceBulk TestPeriodicInvoice
		{
			get { return (PeriodicInvoiceBulk)testPeriodicInvoice_internalValue; }
		}

		#region Helper Methods

		protected static PeriodicInvoice FindPeriodInvoiceForDebtor(PeriodicInvoiceBulk bulkInvoice, ZGuid debtorPK)
		{
			return (from PeriodicInvoice pi in bulkInvoice.PeriodicInvoices where pi.DebtorPK == debtorPK select pi).Single();
		}

		protected static PeriodicInvoiceSelectableJob FindSelectableJobForActualJob(PeriodicInvoice invoice, ZGuid actualJobPK)
		{
			return (from PeriodicInvoiceSelectableJob j in invoice.Jobs where j.Parent.PK == actualJobPK select j).Single();
		}

		#endregion

	}
}
