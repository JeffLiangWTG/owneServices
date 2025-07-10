using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business.Testing;
using Enterprise.Accounting.DataTransfer.Universal;
using Enterprise.Accounting.Export;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Core;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Integration;
using Enterprise.Integration.DocumentEngine;
using Enterprise.LogWalker.Test;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.Registry.Business;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Accounting;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.WorkflowManager.ServiceTasks;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using static Enterprise.Core.Constants;
using static Enterprise.Registry.Business.AmountBasedMultiLevelAuthorisationRequirement;
using AuthorisationCodes = Enterprise.Registry.Business.AmountBasedMultiLevelAuthorisationRequirement.AuthorisationRequirementCodes;
using UniversalCodeDescriptionPair = Enterprise.UniversalDataBuss.DataObjects.Universal.CodeDescriptionPair;

namespace Enterprise.Accounting.Business.ARAP.Invoicing.Testing
{
	[TestDate(2019, 10, 20, 12, 12, 12, 0)]
	public class TransactionAllocationAndPosterTest : TestCaseWithFactory
	{
		public void TestAllocationAndPosterNoSecurityUserSet()
		{
			Notifications.Clear();
			new AccountingPeriodTestHelper().SetupPeriods();
			AccountingConfigurationRegistry.Instance.SecurityRightsForTransactionAllocateAndPostTrigger.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, Guid.Empty);

			var apInvoices = Factory.Load<APInvoice>(new ZQuery(new ZQuery(AccTransactionHeaderSchema.AH_Ledger, LedgerTypes.AccountsPayable).AddToFilter(AccTransactionHeaderSchema.AH_GC, GlbCompany.CurrentCompany.PK)));
			var invoicePendingAllocation = CreateAndTriggerPostFromXML(UniversalTransactionXML, 60, Notifications);

			var apInvoicesUpdated = LoadAPInvoiceInDBInANewFactory(GlbCompany.CurrentCompany.PK);
			AssertEquals("not set up", apInvoices.Length, apInvoicesUpdated.Length);

			var errorMsg = @"Error - Accounts Payable Invoice: Please ensure you have set a user for security set at 'Accounting -> Payable Defaults -> Security for trigger allocation and post' before attempting to post.";
			AssertEquals(errorMsg, Notifications.AsString.Trim());
			AssertStmNote("Check Error Msg", invoicePendingAllocation.Notes.FindByDescription(PredefinedNoteTypes.Instance.TransactionAllocationAndPosterLogNote.Description), $"[2019-10-20T12:12:12.0000000Z]{errorMsg}", 1);
		}

		public void TestAllocationAndPosterWithInactiveUser()
		{
			Notifications.Clear();
			new AccountingPeriodTestHelper().SetupPeriods();

			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_IsActive = false;
			AccountingConfigurationRegistry.Instance.SecurityRightsForTransactionAllocateAndPostTrigger.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, staff.PK.ToGuid());

			var invoicePendingAllocation = CreateAndTriggerPostFromXML(UniversalTransactionXML, 60, Notifications, staff);

			var errorMsg = @"Error - Accounts Payable Invoice: Please ensure you have set an active user in 'Accounting -> Payable Defaults -> Security for trigger allocation and post' before attempting to post.";
			AssertEquals(errorMsg, Notifications.AsString.Trim());
			AssertStmNote("Check Error Msg", invoicePendingAllocation.Notes.FindByDescription(PredefinedNoteTypes.Instance.TransactionAllocationAndPosterLogNote.Description), $"[2019-10-20T12:12:12.0000000Z]{errorMsg}", 1);
		}

		public void TestNeedSendEmailIfProcessHasError()
		{
			Notifications.Clear();
			new AccountingPeriodTestHelper().SetupPeriods();
			TestObjectCreator.GS1.ResetBranchAndDepartment();
			AccountingConfigurationRegistry.Instance.SecurityRightsForTransactionAllocateAndPostTrigger.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, TestObjectCreator.GS1.PK.ToGuid());
			var group = TestObjectCreator.CreateRecipientGroup("TTT", "XXX", "TTT@test.com");
			NotificationDataRegistry.Instance.TransactionsPendingAllocationXMLPostingFailureNotificationGroup.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, group.PK.ToGuid());

			AssertEquals("Pre-condition", 0, Env.OutgoingMailManager.EmailsCreated.Count);
			CreateAndTriggerPostFromXML(UniversalTransactionXML, 60, Notifications);

			Assert("Pre-condition", Notifications.HasErrors);
			AssertEquals("Email should be created because Notification has error", 1, Env.OutgoingMailManager.EmailsCreated.Count);
		}

		public void TestNotSendEmailIfProcessNoError()
		{
			Notifications.Clear();
			SetApprovalSettings();

			new AccountingPeriodTestHelper().SetupPeriods();
			var group = TestObjectCreator.CreateRecipientGroup("TTT", "XXX", "TTT@test.com");
			NotificationDataRegistry.Instance.TransactionsPendingAllocationXMLPostingFailureNotificationGroup.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, group.PK.ToGuid());

			var staff = TestObjectCreator.GS1;
			staff.GS_GB_HomeBranch = GlbBranch.CurrentBranch.PK;
			staff.GS_GE_HomeDepartment = GlbDepartment.CurrentDepartment.PK;

			var staffSecurity = Factory.New<GlbSecurity>();
			staffSecurity.GU_GS = staff.PK;
			staffSecurity.GU_SecurityRight = Env.Security.APInvoiceApproval_FirstApproval.Code;
			staffSecurity.GU_SecurityItemIsAllowed = true;

			staffSecurity = Factory.New<GlbSecurity>();
			staffSecurity.GU_GS = staff.PK;
			staffSecurity.GU_SecurityRight = Env.Security.APInvoiceApproval_SecondApproval.Code;
			staffSecurity.GU_SecurityItemIsAllowed = true;

			SetupCommonUserSecurity(staff.PK);
			AccountingConfigurationRegistry.Instance.SecurityRightsForTransactionAllocateAndPostTrigger.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, staff.PK.ToGuid());
			CreateAndTriggerPostFromXML(UniversalTransactionXML, 60, Notifications, staff);

			AssertEquals("Pre-condition", false, Notifications.HasErrors);
			AssertEquals("Email should not be created because Notification has no errors", 0, Env.OutgoingMailManager.EmailsCreated.Count);
		}

		public void TestAllocationAndPosterNoSecurityBranchAndDepartmentSet()
		{
			Notifications.Clear();
			new AccountingPeriodTestHelper().SetupPeriods();

			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_Code = "GS";
			staff.GS_FullName = "Sakata Gintoki";
			staff.ResetBranchAndDepartment();

			AccountingConfigurationRegistry.Instance.SecurityRightsForTransactionAllocateAndPostTrigger.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, staff.PK.ToGuid());

			var apInvoices = Factory.Load<APInvoice>(new ZQuery(new ZQuery(AccTransactionHeaderSchema.AH_Ledger, LedgerTypes.AccountsPayable).AddToFilter(AccTransactionHeaderSchema.AH_GC, GlbCompany.CurrentCompany.PK)));
			var invoicePendingAllocation = CreateAndTriggerPostFromXML(UniversalTransactionXML, 60, Notifications, staff);

			var apInvoicesUpdated = LoadAPInvoiceInDBInANewFactory(GlbCompany.CurrentCompany.PK);
			AssertEquals("not set up", apInvoices.Length, apInvoicesUpdated.Length);

			var errorMsg = @"Error - Accounts Payable Invoice: Please ensure that both Home Branch and Home Department are entered for the user set at 'Accounting -> Payable Defaults -> Security for trigger allocation and post' before attempting to post.";
			AssertEquals(errorMsg, Notifications.AsString.Trim());
			AssertStmNote("Check Error Msg", invoicePendingAllocation.Notes.FindByDescription(PredefinedNoteTypes.Instance.TransactionAllocationAndPosterLogNote.Description), $"[2019-10-20T12:12:12.0000000Z]{errorMsg}", 1);
		}

		public void TestAllocationAndPosterNoSecurityUserBranchSet()
		{
			Notifications.Clear();
			new AccountingPeriodTestHelper().SetupPeriods();

			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_Code = "GS";
			staff.GS_FullName = "Sakata Gintoki";
			staff.GS_GB_HomeBranch = ZGuid.Empty;
			staff.GS_GE_HomeDepartment = GlbDepartment.CurrentDepartment.PK;

			AccountingConfigurationRegistry.Instance.SecurityRightsForTransactionAllocateAndPostTrigger.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, staff.PK.ToGuid());

			var apInvoices = Factory.Load<APInvoice>(new ZQuery(new ZQuery(AccTransactionHeaderSchema.AH_Ledger, LedgerTypes.AccountsPayable).AddToFilter(AccTransactionHeaderSchema.AH_GC, GlbCompany.CurrentCompany.PK)));
			var invoicePendingAllocation = CreateAndTriggerPostFromXML(UniversalTransactionXML, 60, Notifications, staff);

			var apInvoicesUpdated = LoadAPInvoiceInDBInANewFactory(GlbCompany.CurrentCompany.PK);
			AssertEquals("not set up", apInvoices.Length, apInvoicesUpdated.Length);

			var errorMsg = @"Error - Accounts Payable Invoice: Please ensure that a Home Branch is entered for the user set at 'Accounting -> Payable Defaults -> Security for trigger allocation and post' before attempting to post.";
			AssertEquals(errorMsg, Notifications.AsString.Trim());
			AssertStmNote("Check Error Msg", invoicePendingAllocation.Notes.FindByDescription(PredefinedNoteTypes.Instance.TransactionAllocationAndPosterLogNote.Description), $"[2019-10-20T12:12:12.0000000Z]{errorMsg}", 1);
		}

		public void TestAllocationAndPosterNoSecurityUserDepartmentSet()
		{
			Notifications.Clear();
			new AccountingPeriodTestHelper().SetupPeriods();

			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_Code = "GS";
			staff.GS_FullName = "Sakata Gintoki";
			staff.GS_GB_HomeBranch = GlbBranch.CurrentBranch.PK;
			staff.GS_GE_HomeDepartment = ZGuid.Empty;

			AccountingConfigurationRegistry.Instance.SecurityRightsForTransactionAllocateAndPostTrigger.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, staff.PK.ToGuid());

			var apInvoices = Factory.Load<APInvoice>(new ZQuery(new ZQuery(AccTransactionHeaderSchema.AH_Ledger, LedgerTypes.AccountsPayable).AddToFilter(AccTransactionHeaderSchema.AH_GC, GlbCompany.CurrentCompany.PK)));

			var invoicePendingAllocation = CreateAndTriggerPostFromXML(UniversalTransactionXML, 60, Notifications, staff);

			var apInvoicesUpdated = LoadAPInvoiceInDBInANewFactory(GlbCompany.CurrentCompany.PK);
			AssertEquals("not set up", apInvoices.Length, apInvoicesUpdated.Length);

			var errorMsg = @"Error - Accounts Payable Invoice: Please ensure that a Home Department is entered for the user set at 'Accounting -> Payable Defaults -> Security for trigger allocation and post' before attempting to post.";
			AssertEquals(errorMsg, Notifications.AsString.Trim());
			AssertStmNote("Check Error Msg", invoicePendingAllocation.Notes.FindByDescription(PredefinedNoteTypes.Instance.TransactionAllocationAndPosterLogNote.Description), $"[2019-10-20T12:12:12.0000000Z]{errorMsg}", 1);
		}

		public void TestApprovalNoRights()
		{
			Notifications.Clear();
			SetApprovalSettings();

			new AccountingPeriodTestHelper().SetupPeriods();

			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_Code = "GS";
			staff.GS_FullName = "Sakata Gintoki";
			staff.GS_GB_HomeBranch = GlbBranch.CurrentBranch.PK;
			staff.GS_GE_HomeDepartment = GlbDepartment.CurrentDepartment.PK;

			var staffSecurity = Factory.New<GlbSecurity>();
			staffSecurity.GU_GS = staff.PK;
			staffSecurity.GU_SecurityRight = Env.Security.APInvoiceApproval_FirstApproval.Code;
			staffSecurity.GU_SecurityItemIsAllowed = false;

			staffSecurity = Factory.New<GlbSecurity>();
			staffSecurity.GU_GS = staff.PK;
			staffSecurity.GU_SecurityRight = Env.Security.APInvoiceApproval_SecondApproval.Code;
			staffSecurity.GU_SecurityItemIsAllowed = false;

			SetupCommonUserSecurity(staff.PK);

			AccountingConfigurationRegistry.Instance.SecurityRightsForTransactionAllocateAndPostTrigger.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, staff.PK.ToGuid());

			var apInvoices = Factory.Load<APInvoice>(new ZQuery(new ZQuery(AccTransactionHeaderSchema.AH_Ledger, LedgerTypes.AccountsPayable).AddToFilter(AccTransactionHeaderSchema.AH_GC, GlbCompany.CurrentCompany.PK)));

			var invoicePendingAllocation = CreateAndTriggerPostFromXML(UniversalTransactionXML, 60, Notifications, staff);

			var apInvoicesUpdated = LoadAPInvoiceInDBInANewFactory(GlbCompany.CurrentCompany.PK);
			AssertEquals("no security rights", apInvoices.Length, apInvoicesUpdated.Length);

			var errorMsg = @"Error - Accounts Payable Invoice: You don't have security right to post this transaction. Please go to Accounting -> Payable Defaults -> Default Settings -> Unapproved Invoices -> Unapproved Invoices Authorization Settings";
			AssertEquals(errorMsg, Notifications.AsString.Trim());
			AssertStmNote("Check Error Msg", invoicePendingAllocation.Notes.FindByDescription(PredefinedNoteTypes.Instance.TransactionAllocationAndPosterLogNote.Description), $"[2019-10-20T12:12:12.0000000Z]{errorMsg}", 1);
		}

		public void TestApprovalRights()
		{
			Notifications.Clear();
			SetApprovalSettings();

			new AccountingPeriodTestHelper().SetupPeriods();

			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_Code = "GS";
			staff.GS_FullName = "Sakata Gintoki";
			staff.GS_GB_HomeBranch = GlbBranch.CurrentBranch.PK;
			staff.GS_GE_HomeDepartment = GlbDepartment.CurrentDepartment.PK;

			var staffSecurity = Factory.New<GlbSecurity>();
			staffSecurity.GU_GS = staff.PK;
			staffSecurity.GU_SecurityRight = Env.Security.APInvoiceApproval_FirstApproval.Code;
			staffSecurity.GU_SecurityItemIsAllowed = true;

			staffSecurity = Factory.New<GlbSecurity>();
			staffSecurity.GU_GS = staff.PK;
			staffSecurity.GU_SecurityRight = Env.Security.APInvoiceApproval_SecondApproval.Code;
			staffSecurity.GU_SecurityItemIsAllowed = true;

			SetupCommonUserSecurity(staff.PK);

			AccountingConfigurationRegistry.Instance.SecurityRightsForTransactionAllocateAndPostTrigger.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, staff.PK.ToGuid());
			var apInvoices = Factory.Load<APInvoice>(new ZQuery(new ZQuery(AccTransactionHeaderSchema.AH_Ledger, LedgerTypes.AccountsPayable).AddToFilter(AccTransactionHeaderSchema.AH_GC, GlbCompany.CurrentCompany.PK)));

			var invoicePendingAllocation = CreateAndTriggerPostFromXML(UniversalTransactionXML, 60, Notifications, staff);

			var apInvoicesUpdated = LoadAPInvoiceInDBInANewFactory(GlbCompany.CurrentCompany.PK);
			AssertEquals("ap was created and saved/posted", apInvoices.Length + 1, apInvoicesUpdated.Length);

			var createdAP = apInvoicesUpdated.Where(x => !apInvoices.Contains(x)).Single();
			AssertEquals("created AP is correct (based on XML)", 1, createdAP.Lines.Count);
			AssertEquals("created AP is correct (based on XML)", (ZDecimal)(-60), createdAP.AH_InvoiceAmount);
			AssertEquals("created AP is correct (based on XML)", LedgerTypes.AccountsPayable, createdAP.AH_Ledger);
			AssertEquals("created AP is correct (based on XML)", TransactionTypes.Invoice, createdAP.AH_TransactionType);

			AssertEquals(0, invoicePendingAllocation.Notes.FindByDescription(PredefinedNoteTypes.Instance.TransactionAllocationAndPosterLogNote.Description).Length);
		}

		public void TestAllocationAndPosterWithValidationErrors()
		{
			Notifications.Clear();
			new AccountingPeriodTestHelper().SetupPeriods();

			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_Code = "GS";
			staff.GS_FullName = "Sakata Gintoki";
			staff.GS_GB_HomeBranch = GlbBranch.CurrentBranch.PK;
			staff.GS_GE_HomeDepartment = GlbDepartment.CurrentDepartment.PK;

			SetupCommonUserSecurity(staff.PK);

			AccountingConfigurationRegistry.Instance.SecurityRightsForTransactionAllocateAndPostTrigger.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, staff.PK.ToGuid());

			var apInvoices = Factory.Load<APInvoice>(new ZQuery(new ZQuery(AccTransactionHeaderSchema.AH_Ledger, LedgerTypes.AccountsPayable).AddToFilter(AccTransactionHeaderSchema.AH_GC, GlbCompany.CurrentCompany.PK)));

			var invoicePendingAllocation = CreateAndTriggerPostFromXML(UniversalTransactionXML, 9999, Notifications, staff);

			var apCreated = LoadAPInvoiceInDBInANewFactory(GlbCompany.CurrentCompany.PK);
			AssertEquals("ap was not created and saved/posted as validation errors - amount not matching up with that declared in the invoicePendingAllocation", apInvoices.Length, apCreated.Length);

			var expectedMessage = @"Error - ExpectedInvoiceTotal: The invoice total of 60.00 AUD does not equal to the expected amount of 9999.00 AUD.
The difference is 9939.00 AUD.";
			AssertEquals(expectedMessage, Notifications.AsString.Trim());
			AssertStmNote("Check Error Msg", invoicePendingAllocation.Notes.FindByDescription(PredefinedNoteTypes.Instance.TransactionAllocationAndPosterLogNote.Description), $"[2019-10-20T12:12:12.0000000Z]{expectedMessage}", 1);
		}

		public void TestMultipleErrorsAreNotDuplicated()
		{
			Notifications.Clear();
			new AccountingPeriodTestHelper().SetupPeriods();

			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_Code = "GS";
			staff.GS_FullName = "Sakata Gintoki";
			staff.GS_GB_HomeBranch = GlbBranch.CurrentBranch.PK;
			staff.GS_GE_HomeDepartment = GlbDepartment.CurrentDepartment.PK;

			SetupCommonUserSecurity(staff.PK);

			AccountingConfigurationRegistry.Instance.SecurityRightsForTransactionAllocateAndPostTrigger.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, staff.PK.ToGuid());

			var apInvoices = Factory.Load<APInvoice>(new ZQuery(new ZQuery(AccTransactionHeaderSchema.AH_Ledger, LedgerTypes.AccountsPayable).AddToFilter(AccTransactionHeaderSchema.AH_GC, GlbCompany.CurrentCompany.PK)));

			var invoicePendingAllocation = CreateAndTriggerPostFromXML(UniversalTransactionXMLWithMultipleLineErrors, 9999, Notifications, staff);

			var apCreated = LoadAPInvoiceInDBInANewFactory(GlbCompany.CurrentCompany.PK);
			AssertEquals("ap was not created and saved/posted as validation errors multiple line errors", apInvoices.Length, apCreated.Length);

			var expectedMessages = new[]
			{
				"Error - AL_AT: Please enter a Tax ID.",
				"Error - AL_Sequence: The Line Sequence Number must be unique.",
				"Error - ExpectedInvoiceTotal: The invoice total of 90.00 AUD does not equal to the expected amount of 9999.00 AUD.\r\nThe difference is 9909.00 AUD.",
			};
			AssertStmNote("Check Error Msg",
				invoicePendingAllocation.Notes.FindByDescription(PredefinedNoteTypes.Instance.TransactionAllocationAndPosterLogNote.Description),
				string.Join(System.Environment.NewLine, expectedMessages.Select(expectedMessage => $"[2019-10-20T12:12:12.0000000Z]{expectedMessage}")),
				1
			);
			AssertEquals("Duplicate errors are not reported", string.Join(System.Environment.NewLine, expectedMessages), Notifications.AsString.Trim());
		}

		public void TestInvoiceNotPostedIfUnauthorisedToReopenJob()
		{
			Notifications.Clear();
			new AccountingPeriodTestHelper().SetupPeriods();
			AccountingMasterFilesRegistry.Instance.EnableAPInvoiceApproval.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);

			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_Code = "GS";
			staff.GS_FullName = "Sakata Gintoki";
			staff.GS_GB_HomeBranch = GlbBranch.CurrentBranch.PK;
			staff.GS_GE_HomeDepartment = GlbDepartment.CurrentDepartment.PK;

			var staffSecurity = Factory.New<GlbSecurity>();
			staffSecurity.GU_GS = staff.PK;
			staffSecurity.GU_SecurityRight = Env.Security.ReopenJob.Code;
			staffSecurity.GU_SecurityItemIsAllowed = false;

			SetupCommonUserSecurity(staff.PK);

			AccountingConfigurationRegistry.Instance.SecurityRightsForTransactionAllocateAndPostTrigger.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, staff.PK.ToGuid());
			var apInvoices = Factory.Load<APInvoice>(new ZQuery(new ZQuery(AccTransactionHeaderSchema.AH_Ledger, LedgerTypes.AccountsPayable).AddToFilter(AccTransactionHeaderSchema.AH_GC, GlbCompany.CurrentCompany.PK)));

			var shipment = TestObjectCreator.CreateShipment("S001");
			var job = TestObjectCreator.CreateJob(shipment, false);
			job.JH_Status = JobHeaderStatus.Closed.Code;
			Factory.Save();

			var invoicePendingAllocation = CreateAndTriggerPostFromXML(XMLWithJobRelatedLines(job, shipment), 40, Notifications, staff);

			var apCreated = LoadAPInvoiceInDBInANewFactory(GlbCompany.CurrentCompany.PK);
			AssertEquals("Cannot reopen job", apInvoices.Length, apCreated.Length);

			var errorMsg = @"Error - Accounts Payable Invoice: Job requires reopening.";
			AssertEquals(errorMsg, Notifications.AsString.Trim());
			AssertStmNote("Check Error Msg", invoicePendingAllocation.Notes.FindByDescription(PredefinedNoteTypes.Instance.TransactionAllocationAndPosterLogNote.Description), $"[2019-10-20T12:12:12.0000000Z]{errorMsg}", 1);

			AssertEquals("job not reopened", job.JH_Status, JobHeaderStatus.Closed.Code);
		}

		public void TestInvoicePostedJobReopenedIfAuthorised()
		{
			Notifications.Clear();
			new AccountingPeriodTestHelper().SetupPeriods();
			AccountingMasterFilesRegistry.Instance.EnableAPInvoiceApproval.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);

			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_Code = "GS";
			staff.GS_FullName = "Sakata Gintoki";
			staff.GS_GB_HomeBranch = GlbBranch.CurrentBranch.PK;
			staff.GS_GE_HomeDepartment = GlbDepartment.CurrentDepartment.PK;

			var staffSecurity = Factory.New<GlbSecurity>();
			staffSecurity.GU_GS = staff.PK;
			staffSecurity.GU_SecurityRight = Env.Security.ReopenJob.Code;
			staffSecurity.GU_SecurityItemIsAllowed = true;

			SetupCommonUserSecurity(staff.PK);

			AccountingConfigurationRegistry.Instance.SecurityRightsForTransactionAllocateAndPostTrigger.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, staff.PK.ToGuid());
			var apInvoices = Factory.Load<APInvoice>(new ZQuery(new ZQuery(AccTransactionHeaderSchema.AH_Ledger, LedgerTypes.AccountsPayable).AddToFilter(AccTransactionHeaderSchema.AH_GC, GlbCompany.CurrentCompany.PK)));

			var shipment = TestObjectCreator.CreateShipment("S001");
			var job = TestObjectCreator.CreateJob(shipment, false);
			job.JH_Status = JobHeaderStatus.Closed.Code;
			Factory.Save();

			var invoicePendingAllocation = CreateAndTriggerPostFromXML(XMLWithJobRelatedLines(job, shipment), 40, Notifications, staff);

			var apInvoicesUpdated = LoadAPInvoiceInDBInANewFactory(GlbCompany.CurrentCompany.PK);

			AssertEquals("ap was created and saved/posted", apInvoices.Length + 1, apInvoicesUpdated.Length);

			var createdAP = apInvoicesUpdated.Where(x => !apInvoices.Contains(x)).Single();
			AssertEquals("created AP is correct (based on XML)", 1, createdAP.Lines.Count);
			AssertEquals("created AP is correct (based on XML)", (ZDecimal)(-40), createdAP.AH_InvoiceAmount);
			AssertEquals("created AP is correct (based on XML)", LedgerTypes.AccountsPayable, createdAP.AH_Ledger);
			AssertEquals("created AP is correct (based on XML)", TransactionTypes.Invoice, createdAP.AH_TransactionType);

			var errorMsg = @"Error - Accounts Payable Invoice: This transaction is linked to approval request but ‘Enable APInvoice Approval’ registry has been set to ‘No’. Please raise an eRequest to request assistance to set this registry to ‘Yes’ before posting the transaction.";
			AssertEquals(errorMsg, Notifications.AsString.Trim());
			AssertStmNote("Check Error Msg", invoicePendingAllocation.Notes.FindByDescription(PredefinedNoteTypes.Instance.TransactionAllocationAndPosterLogNote.Description), $"[2019-10-20T12:12:12.0000000Z]{errorMsg}", 1);

			AssertEquals("job reopened", job.JH_Status, JobHeaderStatus.Working.Code);
		}

		public void TestInvoiceValidationDependsOnUserSet_Standard()
		{
			AssertInvoiceValidationDependsOnUserSet(AccountingMasterFilesConstants.AllowDuplicateInvoiceNumberRule.STD,
				ZDateTime.Today.AddMonths(-AccountingUtils.DuplicateInvoiceNumberPeriodMonths),
				"The transaction number is already in use. Last posted transaction’s invoice date is 20-Oct-18 which is at least 12 months apart. You are not granted security right to Payables > Payables Transactions > New Transactions > Allow Duplicate AP Invoice Number. This transaction number cannot be used. Please enter another one.");
		}

		public void TestInvoiceValidationDependsOnUserSet_Calendar()
		{
			AssertInvoiceValidationDependsOnUserSet(AccountingMasterFilesConstants.AllowDuplicateInvoiceNumberRule.CAL,
				new ZDateTime(ZDateTime.Today.Year - 1, 1, 1),
				"The transaction number is already in use. Last posted transaction’s invoice date is 01-Jan-18 which is in another calendar year. You are not granted security right to Payables > Payables Transactions > New Transactions > Allow Duplicate AP Invoice Number. This transaction number cannot be used. Please enter another one.");
		}

		void AssertInvoiceValidationDependsOnUserSet(string allowDuplicateInvoiceNumberRule, ZDateTime invoiceDate1, string expErrorMessage)
		{
			using (AccountingMasterFilesRegistry.Instance.AllowDuplicateInvoiceNumberDefaultingRule.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, allowDuplicateInvoiceNumberRule))
			{
				Notifications.Clear();
				new AccountingPeriodTestHelper().SetupPeriods();
				AccountingMasterFilesRegistry.Instance.EnableAPInvoiceApproval.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);

				var oldInvoice = TestObjectCreator.CreateAPInvoice<APInvoice>("1", TestObjectCreator.AUD, 1m, 1m, 1m, 1m, 1m, 1m, 1m, TestObjectCreator.AALSHI);
				oldInvoice.AH_InvoiceDate = invoiceDate1;
				oldInvoice.AH_TransactionNum = "1";
				oldInvoice.AH_OH = TestObjectCreator.Creditor1.PK;
				TestObjectCreator.Factory.Save();

				var staff = Factory.NewWithValidTestData<GlbStaff>();
				staff.GS_Code = "GS";
				staff.GS_FullName = "Sakata Gintoki";
				staff.GS_GB_HomeBranch = GlbBranch.CurrentBranch.PK;
				staff.GS_GE_HomeDepartment = GlbDepartment.CurrentDepartment.PK;

				var staffSecurity = Factory.New<GlbSecurity>();
				staffSecurity.GU_GS = staff.PK;
				staffSecurity.GU_SecurityRight = Env.Security.NewPayablesDuplicateInvoiceNumber.Code;
				staffSecurity.GU_SecurityItemIsAllowed = false;

				SetupCommonUserSecurity(staff.PK);

				AccountingConfigurationRegistry.Instance.SecurityRightsForTransactionAllocateAndPostTrigger.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, staff.PK.ToGuid());
				var apInvoices = Factory.Load<APInvoice>(new ZQuery(new ZQuery(AccTransactionHeaderSchema.AH_Ledger, LedgerTypes.AccountsPayable).AddToFilter(AccTransactionHeaderSchema.AH_GC, GlbCompany.CurrentCompany.PK)));

				var shipment = TestObjectCreator.CreateShipment("S001");
				var job = TestObjectCreator.CreateJob(shipment, false);
				Factory.Save();

				var invoicePendingAllocation = CreateAndTriggerPostFromXML(XMLWithJobRelatedLines(job, shipment), 40, Notifications, staff);

				var apCreated = LoadAPInvoiceInDBInANewFactory(GlbCompany.CurrentCompany.PK);

				AssertEquals("Cannot submit duplicate numbers as user has no rights", apInvoices.Length, apCreated.Length);

				var errorMsg = string.Format("Error - AH_TransactionNum: {0}", expErrorMessage);
				AssertEquals(errorMsg, Notifications.AsString.Trim());
				AssertStmNote("Check Error Msg", invoicePendingAllocation.Notes.FindByDescription(PredefinedNoteTypes.Instance.TransactionAllocationAndPosterLogNote.Description), $"[2019-10-20T12:12:12.0000000Z]{errorMsg}", 1);
			}
		}

		public void TestCostVarianceRightsPreventsPosting()
		{
			Notifications.Clear();
			new AccountingPeriodTestHelper().SetupPeriods();
			AccountingMasterFilesRegistry.Instance.EnableAPInvoiceApproval.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);

			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_Code = "GS";
			staff.GS_FullName = "Sakata Gintoki";
			staff.GS_GB_HomeBranch = GlbBranch.CurrentBranch.PK;
			staff.GS_GE_HomeDepartment = GlbDepartment.CurrentDepartment.PK;

			var staffSecurity = Factory.New<GlbSecurity>();
			staffSecurity.GU_GS = staff.PK;
			staffSecurity.GU_SecurityRight = Env.Security.CostVarianceApprovalLevel1.Code;
			staffSecurity.GU_SecurityItemIsAllowed = false;

			var staffSecurity2 = Factory.New<GlbSecurity>();
			staffSecurity2.GU_GS = staff.PK;
			staffSecurity2.GU_SecurityRight = Env.Security.CostVarianceApprovalLevel2.Code;
			staffSecurity2.GU_SecurityItemIsAllowed = false;

			SetupCommonUserSecurity(staff.PK);

			SetCostVarianceSettings();

			AccountingConfigurationRegistry.Instance.SecurityRightsForTransactionAllocateAndPostTrigger.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, staff.PK.ToGuid());

			var shipment = TestObjectCreator.CreateShipment("S001");
			var job = TestObjectCreator.CreateJob(shipment, false);
			job.JH_Status = JobHeaderStatus.Working.Code;
			Factory.Save();

			var apInvoices = Factory.Load<APInvoice>(new ZQuery(new ZQuery(AccTransactionHeaderSchema.AH_Ledger, LedgerTypes.AccountsPayable).AddToFilter(AccTransactionHeaderSchema.AH_GC, GlbCompany.CurrentCompany.PK)));

			var invoicePendingAllocation = CreateAndTriggerPostFromXML(XMLWithJobRelatedLines(job, shipment), 40, Notifications, staff);

			var apCreated = LoadAPInvoiceInDBInANewFactory(GlbCompany.CurrentCompany.PK);
			AssertEquals("ap was not created and saved/posted as invalid security", apInvoices.Length, apCreated.Length);

			var errorMsg = @"Error - Accounts Payable Invoice: You don't have security right to post this transaction.";
			AssertMultilineASCIIEquals(errorMsg, Notifications.AsString.Trim());
			AssertStmNote("Check Error Msg", invoicePendingAllocation.Notes.FindByDescription(PredefinedNoteTypes.Instance.TransactionAllocationAndPosterLogNote.Description), $"[2019-10-20T12:12:12.0000000Z]{errorMsg}", 1);
		}

		[TestDate(2019, 10, 20)]
		public void TestThrowWarningWhenCreateComplianceDocumentsWithNegativeRecords()
		{
			AssertThrowWarningWhenCreateComplianceDocumentsWithNegativeRecords(true);
		}

		[TestDate(2019, 10, 20)]
		public void TestThrowWarningWhenCreateComplianceDocumentsWithNegativeRecords2()
		{
			AssertThrowWarningWhenCreateComplianceDocumentsWithNegativeRecords(false);
		}

		void AssertThrowWarningWhenCreateComplianceDocumentsWithNegativeRecords(bool flag)
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.Taiwan))
			using (AccountingMasterFilesRegistry.Instance.EnableComplianceDocumentModule.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			using (AccountingMasterFilesRegistry.Instance.AllowNegativeComplianceDocumentLines.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, flag))
			using (AccountingMasterFilesRegistry.Instance.ComplianceDocumentNumberAllocation_Payables.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, AccComplianceSequenceLookups.ComplianceDocumentNumberAllocationSettingCodes.Post))
			{
				new AccountingPeriodTestHelper().SetupPeriods();

				var staff = Factory.NewWithValidTestData<GlbStaff>();
				staff.GS_Code = "GS";
				staff.GS_FullName = "Sakata Gintoki";
				staff.GS_GB_HomeBranch = GlbBranch.CurrentBranch.PK;
				staff.GS_GE_HomeDepartment = GlbDepartment.CurrentDepartment.PK;

				var staffSecurity = Factory.New<GlbSecurity>();
				staffSecurity.GU_GS = staff.PK;
				staffSecurity.GU_SecurityRight = Env.Security.CreatePayablesComplianceDocuments.Code;
				staffSecurity.GU_SecurityItemIsAllowed = true;

				var query = new ZQuery(new ZQuery(AccTaxRateSchema.AT_Code, "VAT"), new ZQuery(AccTaxRateSchema.AT_RN_NKCountry, CountryCodes.Taiwan));
				var vat = Factory.LoadTop1<AccTaxRate>(query);
				vat.AT_PostingGroupId = 1;
				vat.SetRateNumerator_ForTestOnly(0);

				query = new ZQuery(new ZQuery(AccTaxRateSchema.AT_Code, "FREEVAT"), new ZQuery(AccTaxRateSchema.AT_RN_NKCountry, CountryCodes.Taiwan));
				var freevat = Factory.LoadTop1<AccTaxRate>(query);
				freevat.AT_PostingGroupId = 2;
				freevat.SetRateNumerator_ForTestOnly(0);

				Factory.Save();

				SetupCommonUserSecurity(staff.PK);
				AccountingConfigurationRegistry.Instance.SecurityRightsForTransactionAllocateAndPostTrigger.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, staff.PK.ToGuid());

				TestObjectCreator.Creditor1.CompanyData.OB_APCreateVATComplianceDocumentOnPosting = "RCC";
				Factory.Save();

				var invoicePendingAllocation = CreateAndTriggerPostFromXML(UniversalTransactionXMLWithNegativeCompliances, 25, Notifications, staff);
				var apCreated = Factory.Load<APInvoice>(new ZQuery(new ZQuery(AccTransactionHeaderSchema.AH_Ledger, LedgerTypes.AccountsPayable).AddToFilter(AccTransactionHeaderSchema.AH_GC, GlbCompany.CurrentCompany.PK)));

				var errorMsg = flag
					? @"Transaction AP INV 1 is posted successfully. However, Compliance Document records could not be created as negative compliance documents are not allowed."
					: @"Transaction AP INV 1 is posted successfully. However, Compliance Document records could not be created as negative compliance document lines are not allowed.";
				AssertMultilineASCIIEquals(errorMsg, Notifications.AsString.Trim());
				AssertStmNote("Check Error Msg", invoicePendingAllocation.Notes.FindByDescription(PredefinedNoteTypes.Instance.TransactionAllocationAndPosterLogNote.Description), $"[2019-10-20T12:12:12.0000000Z]{errorMsg}", 1);

				Notifications.Clear();
			}
		}

		public void TestInvalidPostingGroupsPreventsPosting()
		{
			Notifications.Clear();
			new AccountingPeriodTestHelper().SetupPeriods();
			AccountingMasterFilesRegistry.Instance.EnableAPInvoiceApproval.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);

			var branchLevelPostingConfiguration = new BranchLevelPostingConfiguration() { EnableBranchLevelPosting = true };
			AccountingConfigurationRegistry.Instance.PayableEnforceBranchLevelPosting.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, branchLevelPostingConfiguration);

			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_Code = "GS";
			staff.GS_FullName = "Sakata Gintoki";
			staff.GS_GB_HomeBranch = GlbBranch.CurrentBranch.PK;
			staff.GS_GE_HomeDepartment = GlbDepartment.CurrentDepartment.PK;

			SetupCommonUserSecurity(staff.PK);

			AccountingConfigurationRegistry.Instance.SecurityRightsForTransactionAllocateAndPostTrigger.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, staff.PK.ToGuid());

			var apInvoices = Factory.Load<APInvoice>(new ZQuery(new ZQuery(AccTransactionHeaderSchema.AH_Ledger, LedgerTypes.AccountsPayable).AddToFilter(AccTransactionHeaderSchema.AH_GC, GlbCompany.CurrentCompany.PK)));

			var invoicePendingAllocation = CreateAndTriggerPostFromXML(UniversalTransactionXMLDifferentPostingGroups, 120, Notifications, staff);

			var apCreated = LoadAPInvoiceInDBInANewFactory(GlbCompany.CurrentCompany.PK);
			AssertEquals("ap was not created and saved/posted as validation errors - amount not matching up with that declared in the invoicePendingAllocation", apInvoices.Length, apCreated.Length);

			var errorMsg = @"Error - AL_GB: Please review the charge lines entered and ensure all charges have been entered belong to the same Posting Group. All charges posted in the one transaction must be in the same Posting Group.
Posting is prevented because charges have been entered using a mix of Posting Groups.";
			AssertMultilineASCIIEquals(errorMsg, Notifications.AsString.Trim());
			AssertStmNote("Check Error Msg", invoicePendingAllocation.Notes.FindByDescription(PredefinedNoteTypes.Instance.TransactionAllocationAndPosterLogNote.Description), $"[2019-10-20T12:12:12.0000000Z]{errorMsg}", 1);
		}

		public void TestCostVarianceRightsAllowedPosting()
		{
			Notifications.Clear();
			new AccountingPeriodTestHelper().SetupPeriods();
			AccountingMasterFilesRegistry.Instance.EnableAPInvoiceApproval.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);

			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_Code = "GS";
			staff.GS_FullName = "Sakata Gintoki";
			staff.GS_GB_HomeBranch = GlbBranch.CurrentBranch.PK;
			staff.GS_GE_HomeDepartment = GlbDepartment.CurrentDepartment.PK;

			var staffSecurity = Factory.New<GlbSecurity>();
			staffSecurity.GU_GS = staff.PK;
			staffSecurity.GU_SecurityRight = Env.Security.CostVarianceApprovalLevel1.Code;
			staffSecurity.GU_SecurityItemIsAllowed = true;

			var staffSecurity2 = Factory.New<GlbSecurity>();
			staffSecurity2.GU_GS = staff.PK;
			staffSecurity2.GU_SecurityRight = Env.Security.CostVarianceApprovalLevel2.Code;
			staffSecurity2.GU_SecurityItemIsAllowed = true;

			SetupCommonUserSecurity(staff.PK);

			SetCostVarianceSettings();

			AccountingConfigurationRegistry.Instance.SecurityRightsForTransactionAllocateAndPostTrigger.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, staff.PK.ToGuid());

			var apInvoices = Factory.Load<APInvoice>(new ZQuery(new ZQuery(AccTransactionHeaderSchema.AH_Ledger, LedgerTypes.AccountsPayable).AddToFilter(AccTransactionHeaderSchema.AH_GC, GlbCompany.CurrentCompany.PK)));

			var shipment = TestObjectCreator.CreateShipment("S001");
			var job = TestObjectCreator.CreateJob(shipment, false);
			job.JH_Status = JobHeaderStatus.Working.Code;
			Factory.Save();

			var invoicePendingAllocation = CreateAndTriggerPostFromXML(XMLWithJobRelatedLines(job, shipment), 40, Notifications, staff);

			var apInvoicesUpdated = LoadAPInvoiceInDBInANewFactory(GlbCompany.CurrentCompany.PK);

			AssertEquals("ap was created and saved/posted", apInvoices.Length + 1, apInvoicesUpdated.Length);

			var createdAP = apInvoicesUpdated.Where(x => !apInvoices.Contains(x)).Single();
			AssertEquals("created AP is correct (based on XML)", 1, createdAP.Lines.Count);
			AssertEquals("created AP is correct (based on XML)", (ZDecimal)(-40), createdAP.AH_InvoiceAmount);
			AssertEquals("created AP is correct (based on XML)", LedgerTypes.AccountsPayable, createdAP.AH_Ledger);
			AssertEquals("created AP is correct (based on XML)", TransactionTypes.Invoice, createdAP.AH_TransactionType);

			var errorMsg = @"Error - Accounts Payable Invoice: This transaction is linked to approval request but ‘Enable APInvoice Approval’ registry has been set to ‘No’. Please raise an eRequest to request assistance to set this registry to ‘Yes’ before posting the transaction.";
			AssertEquals(errorMsg, Notifications.AsString.Trim());
			AssertStmNote("Check Error Msg", invoicePendingAllocation.Notes.FindByDescription(PredefinedNoteTypes.Instance.TransactionAllocationAndPosterLogNote.Description), $"[2019-10-20T12:12:12.0000000Z]{errorMsg}", 1);
		}

		public void TestApprovePendingAllocateTransactionWhichWasAlreadyApproved()
		{
			Notifications.Clear();
			new AccountingPeriodTestHelper().SetupPeriods();
			AccountingMasterFilesRegistry.Instance.EnableAPInvoiceApproval.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_Code = "TST";
			staff.GS_FullName = "Test User";
			staff.GS_GB_HomeBranch = GlbBranch.CurrentBranch.PK;
			staff.GS_GE_HomeDepartment = GlbDepartment.CurrentDepartment.PK;
			SetupCommonUserSecurity(staff.PK);
			AccountingConfigurationRegistry.Instance.SecurityRightsForTransactionAllocateAndPostTrigger.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, staff.PK.ToGuid());

			AssertEquals("Pre-condition", 0, LoadAllocatedInvoices().Length);

			var invoicePendingAllocation = TestObjectCreator.CreateTransactionPendingAllocation("1", TestObjectCreator.Creditor1, 60);
			Factory.Save();
			invoicePendingAllocation.TransactionApprovalRequest.Initialize(invoicePendingAllocation, UniversalTransactionXML, false);
			Factory.Save();

			var newFactory = Factory.CreateNewFactory();
			newFactory.RefreshEnabled = false;
			var invoicePendingAllocationReloaded = newFactory.Load<TransactionPendingAllocation>(invoicePendingAllocation.PK);
			var invoiceAllocatedInNewFactory = TransactionAllocationConverter.ConvertUnallocatedToAP(invoicePendingAllocationReloaded).Invoice;
			invoiceAllocatedInNewFactory.Factory.RefreshEnabled = false;
			invoiceAllocatedInNewFactory.Factory.Save();

			var poster = new TransactionAllocationAndPoster(invoicePendingAllocation);
			poster.Process(Notifications);

			AssertEquals("No transaction allocated.", 0, LoadAllocatedInvoices().Length);

			var errorMsg = "Cannot allocate transaction that has been allocated.";
			AssertEquals(errorMsg, Notifications.AsString.Trim());
			AssertStmNote("Check Error Msg", invoicePendingAllocation.Notes.FindByDescription(PredefinedNoteTypes.Instance.TransactionAllocationAndPosterLogNote.Description), $"[2019-10-20T12:12:12.0000000Z]{errorMsg}", 1);

			APInvoice[] LoadAllocatedInvoices() => Factory.Load<APInvoice>(new ZQuery(new ZQuery(AccTransactionHeaderSchema.AH_Ledger, LedgerTypes.AccountsPayable).AddToFilter(AccTransactionHeaderSchema.AH_GC, GlbCompany.CurrentCompany.PK)));
		}

		public void TestRunATP_EDC_ShouldNotThrowConcurrencyError()
		{
			ZArchitecture.Environment.Globals.IsUserInteractive = false;
			new AccountingPeriodTestHelper().SetupPeriods();
			AccountingMasterFilesRegistry.Instance.EnableAPInvoiceApproval.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_Code = "TST";
			staff.GS_FullName = "Test User";
			staff.GS_GB_HomeBranch = GlbBranch.CurrentBranch.PK;
			staff.GS_GE_HomeDepartment = GlbDepartment.CurrentDepartment.PK;
			SetupCommonUserSecurity(staff.PK);
			AccountingConfigurationRegistry.Instance.SecurityRightsForTransactionAllocateAndPostTrigger.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, staff.PK.ToGuid());

			var doc = Factory.LoadTop1<IDocumentCommand>(new DocumentZQuery(CargoWise.Definitions.BusinessContext.APInvoice, "DocBuilder Receipt Document"));
			doc.SU_IsSystemDefined = false;
			doc.SU_FilterList = "";

			var pendingAllocationJob = TestObjectCreator.CreateTransactionPendingAllocation("1", TestObjectCreator.Creditor1, 60);
			Factory.Save();
			pendingAllocationJob.TransactionApprovalRequest.Initialize(pendingAllocationJob, UniversalTransactionXML, false);

			var addRecordTrigger = ((IWorkflowProvider)pendingAllocationJob).WorkflowItems.Triggers.AddNew();
			addRecordTrigger.TriggerConditions.TriggerEventCode = Events.AddedARecordToTheSystemCode;

			var notificationATP = addRecordTrigger.ProcessTaskNotifications.AddNew();
			notificationATP.PQ_TriggerType = "ATP";

			var notificationEDC = addRecordTrigger.ProcessTaskNotifications.AddNew();
			notificationEDC.PQ_TriggerType = "EDC";
			notificationEDC.PQ_SU_Document = doc.PK;

			var log = addRecordTrigger.Logs.GetAllLogs().OfType<StmALog>().Single(l => l.SL_SE_NKEvent == Events.WorkflowTriggerEventCode);
			Factory.Save();

			var subscriber = new WorkflowEventTriggerProcessor();
			var logger = new LoggerForTest();
			var newsTransmitter = new MockNewsTransmitter(subscriber, new[] { subscriber }, new LogWalker.SubscriberParameters { Logger = logger });
			var queuedLog = new QueuedLogForTesting(log, addRecordTrigger);
			queuedLog.SJ_Status = "QUE";
			queuedLog.SJ_PostedTimeUtc = DateTime.UtcNow;

			var apInvoicesInDB = LoadAPInvoiceInDBInANewFactory(GlbCompany.CurrentCompany.PK);
			AssertEquals(0, apInvoicesInDB.Length);

			newsTransmitter.ProcessLogs(new[] { queuedLog });
			AssertCollectionNotContains(logger.LogEntries, x =>
			{
				return x.Contains("ConcurrencyError");
			});
		}

		public void TestApprovePendingAllocateTransaction_WithError()
		{
			AccountingConfigurationRegistry.Instance.SecurityRightsForTransactionAllocateAndPostTrigger.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, TestObjectCreator.GS1.PK.ToGuid());
			var pendingAllocationJob = TestObjectCreator.CreateTransactionPendingAllocation("1", TestObjectCreator.Creditor1, 60);
			var poster = new TransactionAllocationAndPoster(pendingAllocationJob);
			poster.Process(Notifications);

			var apInvoicesInDB = LoadAPInvoiceInDBInANewFactory(GlbCompany.CurrentCompany.PK);
			AssertEquals(0, apInvoicesInDB.Length);
		}

		public void TestUserContextChangedAfterProcessingStarted()
		{
			AccountingConfigurationRegistry.Instance.SecurityRightsForTransactionAllocateAndPostTrigger.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, TestObjectCreator.GS1.PK.ToGuid());
			CreateAndTriggerPostFromXML(UniversalTransactionXML, 60, Notifications, TestObjectCreator.Staff);
			var errorMsg = "The user context was changed after starting processing the ATP trigger. It may due to the value change of registry 'Accounting -> Payable Defaults -> Security for trigger allocation and post'.";
			AssertContains(errorMsg, Notifications.AsString.Trim());
		}

		public void TestLogMessageIsAppenedToStmNoteLast()
		{
			new AccountingPeriodTestHelper().SetupPeriods();
			AccountingConfigurationRegistry.Instance.SecurityRightsForTransactionAllocateAndPostTrigger.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, Guid.Empty);

			var invoicePendingAllocation = TestObjectCreator.CreateTransactionPendingAllocation("1", TestObjectCreator.Creditor1, 60);
			Factory.Save();

			invoicePendingAllocation.TransactionApprovalRequest.Initialize(invoicePendingAllocation, UniversalTransactionXML, false);
			Factory.Save();

			var poster = new TransactionAllocationAndPoster(invoicePendingAllocation);

			poster.Process(Notifications);

			TestDateAttribute.AddHours(1);
			poster.Process(Notifications);

			TestDateAttribute.AddHours(1);
			poster.Process(Notifications);

			TestDateAttribute.AddHours(1);
			poster.Process(Notifications);
			poster.Process(Notifications);

			AssertEquals(5, invoicePendingAllocation.Notes.FindByDescription(PredefinedNoteTypes.Instance.TransactionAllocationAndPosterLogNote.Description).Length);
			CombineAssertions("Keep old note and add new note for every logger message.", () => {
				AssertStmNote("Check Error Msg",
					invoicePendingAllocation.Notes.FindByDescription(PredefinedNoteTypes.Instance.TransactionAllocationAndPosterLogNote.Description),
					"[2019-10-20T12:12:12.0000000Z]Error - Accounts Payable Invoice: Please ensure you have set a user for security set at 'Accounting -> Payable Defaults -> Security for trigger allocation and post' before attempting to post.",
					1
				);
				AssertStmNote("Check Error Msg",
					invoicePendingAllocation.Notes.FindByDescription(PredefinedNoteTypes.Instance.TransactionAllocationAndPosterLogNote.Description),
					"[2019-10-20T13:12:12.0000000Z]Error - Accounts Payable Invoice: Please ensure you have set a user for security set at 'Accounting -> Payable Defaults -> Security for trigger allocation and post' before attempting to post.",
					1
				);
				AssertStmNote("Check Error Msg",
					invoicePendingAllocation.Notes.FindByDescription(PredefinedNoteTypes.Instance.TransactionAllocationAndPosterLogNote.Description),
					"[2019-10-20T14:12:12.0000000Z]Error - Accounts Payable Invoice: Please ensure you have set a user for security set at 'Accounting -> Payable Defaults -> Security for trigger allocation and post' before attempting to post.",
					1
				);
				AssertStmNote("Check Error Msg",
					invoicePendingAllocation.Notes.FindByDescription(PredefinedNoteTypes.Instance.TransactionAllocationAndPosterLogNote.Description),
					"[2019-10-20T15:12:12.0000000Z]Error - Accounts Payable Invoice: Please ensure you have set a user for security set at 'Accounting -> Payable Defaults -> Security for trigger allocation and post' before attempting to post.",
					2
				);
			});
		}

		void SetupCommonUserSecurity(ZGuid staffPK)
		{
			var staffSecurity = Factory.New<GlbSecurity>();
			staffSecurity.GU_GS = staffPK;
			staffSecurity.GU_SecurityRight = Env.Security.NewPayablesAllowGLAccountWithoutMapping.Code;
			staffSecurity.GU_SecurityItemIsAllowed = true;

			staffSecurity = Factory.New<GlbSecurity>();
			staffSecurity.GU_GS = staffPK;
			staffSecurity.GU_SecurityRight = Env.Security.AllowPayablesInvoiceFinalFlag.Code;
			staffSecurity.GU_SecurityItemIsAllowed = true;

			staffSecurity = Factory.New<GlbSecurity>();
			staffSecurity.GU_GS = staffPK;
			staffSecurity.GU_SecurityRight = Env.Security.AllowAPInvoiceDefaultRequisitionDetailsOverride.Code;
			staffSecurity.GU_SecurityItemIsAllowed = true;
		}

		void SetCostVarianceSettings()
		{
			CostVarianceApproval valuesForTest = new CostVarianceApproval();
			valuesForTest.VarianceCalculationStyle = Enterprise.Core.Constants.CostVarianceCalculationStyle.PercentageVariance;
			valuesForTest.VarianceComparisonOption = Enterprise.Core.Constants.CostVarianceComparisonOption.ImportedChargeOrJob;
			CostVarianceApprovalAuthorisationRequirement upTo1 = valuesForTest.AuthorisationRequirements.AddNew();
			upTo1.AuthorisationRequirement = AuthorisationCodes.FirstApprovalRequiredOnly;
			upTo1.Range = RangeCodes.UpTo;
			upTo1.Amount = 30m;

			CostVarianceApprovalAuthorisationRequirement upTo2 = valuesForTest.AuthorisationRequirements.AddNew();
			upTo2.AuthorisationRequirement = AuthorisationCodes.SecondApprovalRequiredOnly;
			upTo2.Range = RangeCodes.Above;
			upTo2.Amount = 30m;

			AccountingConfigurationRegistry.Instance.CostVarianceApproval.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, valuesForTest);
		}

		TransactionPendingAllocation CreateAndTriggerPostFromXML(string xml, decimal amount, NotificationBuffer notifications, GlbStaff userContext = null)
		{
			var invoicePendingAllocation = TestObjectCreator.CreateTransactionPendingAllocation("1", TestObjectCreator.Creditor1, amount);
			Factory.Save();
			invoicePendingAllocation.TransactionApprovalRequest.Initialize(invoicePendingAllocation, xml, false);
			Factory.Save();
			if (userContext?.GS_IsActive == true && userContext?.HomeBranch != null && userContext?.HomeDepartment != null)
			{
				using (Env.SetTemporaryUserContext(userContext.PK.ToGuid(), userContext.HomeBranch.PK.ToGuid(), userContext.HomeDepartment.PK.ToGuid()))
				{
					PostAndProcess(notifications, invoicePendingAllocation);
				}
			}
			else
			{
				PostAndProcess(notifications, invoicePendingAllocation);
			}

			return invoicePendingAllocation;
		}

		void PostAndProcess(NotificationBuffer notifications, TransactionPendingAllocation invoicePendingAllocation)
		{
			var poster = new TransactionAllocationAndPoster(null);
			poster.Process(notifications);

			poster = new TransactionAllocationAndPoster(new BusinessObjectFactory().New<APInvoice>());
			poster.Process(notifications);

			poster = new TransactionAllocationAndPoster(invoicePendingAllocation);
			poster.Process(notifications);

			Factory.Save();
		}

		APInvoice[] LoadAPInvoiceInDBInANewFactory(ZGuid companyPK)
		{
			return new BusinessObjectFactory().Load<APInvoice>(new ZQuery(new ZQuery(AccTransactionHeaderSchema.AH_Ledger, LedgerTypes.AccountsPayable).AddToFilter(AccTransactionHeaderSchema.AH_GC, companyPK)));
		}

		void SetApprovalSettings()
		{
			AccountingMasterFilesRegistry.Instance.EnableAPInvoiceApproval.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			var valuesForTest = new PaymentTwelveLevelAuthorisationSettingsCollection();
			var newSetting = valuesForTest.AddNew();
			newSetting.Amount = 0;
			newSetting.AuthorisationRequirement = AmountBasedMultiLevelAuthorisationRequirement.AuthorisationRequirementCodes.FirstApprovalRequiredOnly;
			newSetting.Range = AmountBasedMultiLevelAuthorisationRequirement.RangeCodes.Above;
			AccountingConfigurationRegistry.Instance.UnapprovedInvoicesAuthorizationSettings.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, valuesForTest);
		}

		protected override void SetUp()
		{
			base.SetUp();

			var rate = AccTaxRate.Helper.FindTaxRate(new BusinessObjectFactory(), AccTaxRate.Helper.MainNotReportableTaxRegistryID, Env.CurrentCompanyPK);
			rate.SetRateNumerator_ForTestOnly(0);
			rate.Factory.Save();

			rate = AccTaxRate.Helper.FindTaxRate(new BusinessObjectFactory(), AccTaxRate.Helper.MainFreeGSTTaxRegistryID, Env.CurrentCompanyPK);
			rate.SetRateNumerator_ForTestOnly(0);
			rate.Factory.Save();
		}

		void AssertStmNote(string comment, StmNote[] stmNotes, string assertingMessage, int expectedCount)
			=> AssertEquals(comment, expectedCount, stmNotes.Where(x => x.ST_NoteText == assertingMessage).Count());

		NotificationBuffer Notifications
		{
			get
			{
				if (fNotifications == null)
				{
					fNotifications = new NotificationBuffer();
				}

				return fNotifications;
			}

			set { fNotifications = value; }
		}

		NotificationBuffer fNotifications;

		TestObjectCreator fTestObjectCreator;
		TestObjectCreator TestObjectCreator
		{
			get { return fTestObjectCreator ?? (fTestObjectCreator = new TestObjectCreator(Factory)); }
		}

		string XMLWithJobRelatedLines(JobHeader job, ForwardingShipment shipment)
		{
			var importer = new TransactionImporter();
			var invoice = new BusinessObjectFactory().New<APInvoice>();
			invoice.AH_TransactionNum = "1";
			invoice.AH_OH = TestObjectCreator.Creditor1.PK;
			invoice.AH_InvoiceDate = ZDateTime.Now;

			var universalTransaction = new TransactionInfo(DefaultDataObjectWriterStrategy.TestInstance);
			universalTransaction.SetPostingJournalCollection(() => new List<PostingJournal>());
			universalTransaction.SetShipmentCollection(() => new List<Shipment>());

			var universalLine = new PostingJournal(DefaultDataObjectWriterStrategy.TestInstance);
			universalLine.Branch = new Branch { Code = TestObjectCreator.NonCurrentBranch.GB_Code };
			universalLine.Department = new Department { Code = TestObjectCreator.NonCurrentDepartment.GE_Code };
			universalLine.ChargeCode = new ChargeCode { Code = TestObjectCreator.CC1.AC_Code };
			universalLine.Description = "Some line text";
			universalLine.IsFinalCharge = true;
			universalLine.Sequence = 3;
			universalLine.OSCurrency = new Currency { Code = "USD" };
			universalLine.LocalCurrency = new Currency { Code = "AUD" };
			universalLine.OSAmount = -80;
			universalLine.LocalAmount = -40;
			universalLine.Job = new EntityReference { Key = "S001", Type = AccountingDataTransferConstants.DataContextTypeString.Job };
			universalTransaction.PostingJournalCollection.Add(universalLine);

			var universalShipment = new Shipment(DefaultDataObjectWriterStrategy.TestInstance);
			universalShipment.DataContext = DataContextFactory.New();
			universalShipment.DataContext.AddDataSource(DataContextType.CFSShipment, "Some job");
			universalTransaction.ShipmentCollection.Add(universalShipment);

			universalLine.Job.Key = universalShipment.DataContext.DataSourceCollection.First().Key;
			invoice.Lines.RemoveAndDeleteAll();

			universalShipment.DataContext = DataContextFactory.New();
			universalShipment.DataContext.AddDataSource(DataContextType.ForwardingShipment, universalLine.Job.Key.Value);
			invoice.Lines.RemoveAndDeleteAll();

			shipment.JS_HouseBill = "Some HouseBill";
			shipment.JS_TransportMode = Constants.TransportModes.Air;

			var parentConsol = new Shipment(DefaultDataObjectWriterStrategy.TestInstance);
			parentConsol.DataContext = DataContextFactory.New();
			parentConsol.DataContext.AddDataSource(DataContextType.ForwardingConsol, "Some consol");
			parentConsol.DataContext.AddDataSource(DataContextType.ForwardingShipment, universalLine.Job.Key.Value);
			universalTransaction.SetShipmentCollection(() => new List<Shipment>());
			universalTransaction.ShipmentCollection.Add(parentConsol);
			parentConsol.SetSubShipmentCollection(() => new DataObjectList<Shipment> { universalShipment });
			Factory.Save();
			universalShipment.WayBillType = new WayBillType { Code = WayBillTypeList.Codes.House };
			universalShipment.WayBillNumber = shipment.JS_HouseBill;
			universalShipment.TransportMode = new UniversalCodeDescriptionPair { Code = Constants.TransportModes.Air };
			invoice.Lines.RemoveAndDeleteAll();
			return universalTransaction.Serialize();
		}

		string UniversalTransactionXML
		{
			get
			{
				return @"
<UniversalTransaction xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.1"">
  <TransactionInfo>

    <Branch>
      <Code>BER</Code>
    </Branch>
    <BranchAddress>
      <AddressType>None</AddressType>
      <OrganizationCode>EDICUS</OrganizationCode>
      <Country>
        <Code>AU</Code>
      </Country>
    </BranchAddress>
    <Department>
      <Code>BRN</Code>
    </Department>
    <Description>AP INVOICE</Description>
    <Ledger>AP</Ledger>
    <LocalExVATAmount>-60.0000</LocalExVATAmount>
    <Number>11112222</Number>
    <NumberOfSupportingDocuments>2</NumberOfSupportingDocuments>
    <OrganizationAddress>
      <AddressType>None</AddressType>
      <OrganizationCode>ABCFRESYD</OrganizationCode>
    </OrganizationAddress>
    <OSCurrency>
      <Code>USD</Code>
      <Description>United States Dollar</Description>
    </OSCurrency>
    <OSExGSTVATAmount>-120.0000</OSExGSTVATAmount>
    <TransactionType>INV</TransactionType>

    <PostingJournalCollection>
      <PostingJournal>
        <Branch>
          <Code>SYD</Code>
        </Branch>
		<GLAccount>
		  <AccountCode>" + TestObjectCreator.GLJournalClearingAccount.AccountNum + @"</AccountCode>
		  <Description>FREIGHT REVENUE ACTUAL</Description>
		</GLAccount>
        <VATTaxID>
          <TaxCode>FREEGST</TaxCode>
		</VATTaxID>        
		<Department>
          <Code>BRN</Code>
        </Department>
        <Description>FREIGHT REVENUE ACTUAL</Description>
        <IsFinalCharge>true</IsFinalCharge>
        <LocalAmount>-60.0000</LocalAmount>
        <OSAmount>-120.00</OSAmount>
        <OSCurrency>
          <Code>USD</Code>
        </OSCurrency>
         <LocalCurrency>
          <Code>AUD</Code>
        </LocalCurrency>
       <Sequence>2</Sequence>
      </PostingJournal>
    </PostingJournalCollection>
  </TransactionInfo>
</UniversalTransaction>";
			}
		}

		string UniversalTransactionXMLWithNegativeCompliances
		{
			get
			{
				return @"
<UniversalTransaction xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.1"">
  <TransactionInfo>
    <BranchAddress>
      <AddressType>OFC</AddressType>
      <Address1>125 MING CHE STREET</Address1>
      <Address2></Address2>
      <AddressOverride>false</AddressOverride>
      <AddressShortCode>125 MING CHE STREET</AddressShortCode>
      <City>Taipei</City>
      <CompanyName>CARGOWISE EDI PTY LTD</CompanyName>
      <Country>
        <Code>TW</Code>
        <Name>Taiwan</Name>
      </Country>
      <Email></Email>
      <Fax></Fax>
      <OrganizationCode>Creditor1</OrganizationCode>
      <Phone>+88680012200</Phone>
      <Port>
        <Code>TWTPE</Code>
        <Name>Taipei</Name>
      </Port>
      <Postcode>10652</Postcode>
      <ScreeningStatus>
        <Code>UNK</Code>
        <Description>Unknown</Description>
      </ScreeningStatus>
      <State></State>
    </BranchAddress>
    <LocalExVATAmount>-20.0000</LocalExVATAmount>
    <LocalTotal>-25.0000</LocalTotal>
    <LocalVATAmount>-5.0000</LocalVATAmount>
    <OSExGSTVATAmount>-20.0000</OSExGSTVATAmount>
    <OSGSTVATAmount>-5</OSGSTVATAmount>
    <OSTotal>-25.0000</OSTotal>
    <OutstandingAmount>-25.0000</OutstandingAmount>
 <PostingJournalCollection>
      <PostingJournal>
        <Branch>
          <Code>SYD</Code>
        </Branch>
		<GLAccount>
		  <AccountCode>" + TestObjectCreator.GLJournalClearingAccount.AccountNum + @"</AccountCode>
		  <Description>FREIGHT REVENUE ACTUAL</Description>
		</GLAccount>
        <VATTaxID>
          <TaxCode>VAT</TaxCode>
          <Description>Standard Rated (Non-Capital)</Description>
          <TaxRate>5</TaxRate>
          <TaxType>
            <Code>RAT</Code>
          </TaxType>
		</VATTaxID>
		<Department>
          <Code>BRN</Code>
        </Department>
        <Description>FREIGHT REVENUE ACTUAL</Description>
        <IsFinalCharge>true</IsFinalCharge>
        <LocalGSTVATAmount>-5.0000</LocalGSTVATAmount>
        <LocalTotalAmount>-105.0000</LocalTotalAmount>
        <LocalAmount>-100.0000</LocalAmount>
        <OSAmount>-100</OSAmount>
        <OSGSTVATAmount>-5</OSGSTVATAmount>
        <OSTotalAmount>-105.0000</OSTotalAmount>
         <LocalCurrency>
          <Code>TWD</Code>
        </LocalCurrency>
       <Sequence>2</Sequence>
      </PostingJournal>
		<PostingJournal>
        <Branch>
          <Code>A01</Code>
        </Branch>
		<GLAccount>
		  <AccountCode>" + TestObjectCreator.ExchangeGainLossControlAccount.AccountNum + @"</AccountCode>
		  <Description>FREIGHT REVENUE ACTUAL</Description>
		</GLAccount>
        <VATTaxID>
          <TaxCode>FREEVAT</TaxCode>
          <Description>Zero Rated</Description>
          <TaxRate>0</TaxRate>
          <TaxType>
            <Code>RAT</Code>
          </TaxType>
		</VATTaxID>
		<Department>
          <Code>BRN</Code>
        </Department>
        <Description>FREIGHT REVENUE ACTUAL</Description>
        <IsFinalCharge>true</IsFinalCharge>
        <LocalAmount>80</LocalAmount>
		<LocalGSTVATAmount>0</LocalGSTVATAmount>
        <LocalTotalAmount>80</LocalTotalAmount>
        <OSAmount>80</OSAmount>
        <OSGSTVATAmount>0</OSGSTVATAmount>
        <OSTotalAmount>80</OSTotalAmount>
         <LocalCurrency>
          <Code>TWD</Code>
        </LocalCurrency>
       <Sequence>5</Sequence>
      </PostingJournal>
    </PostingJournalCollection>
  </TransactionInfo>
</UniversalTransaction>
";
			}
		}

		string UniversalTransactionXMLDifferentPostingGroups
		{
			get
			{
				return @"
<UniversalTransaction xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.1"">
  <TransactionInfo>

    <Branch>
      <Code>BER</Code>
    </Branch>
    <BranchAddress>
      <AddressType>None</AddressType>
      <OrganizationCode>EDICUS</OrganizationCode>
      <Country>
        <Code>AU</Code>
      </Country>
    </BranchAddress>
    <Department>
      <Code>BRN</Code>
    </Department>
    <Description>AP INVOICE</Description>
    <Ledger>AP</Ledger>
    <LocalExVATAmount>-60.0000</LocalExVATAmount>
    <Number>11112222</Number>
    <NumberOfSupportingDocuments>2</NumberOfSupportingDocuments>
    <OrganizationAddress>
      <AddressType>None</AddressType>
      <OrganizationCode>ABCFRESYD</OrganizationCode>
    </OrganizationAddress>
    <OSCurrency>
      <Code>USD</Code>
      <Description>United States Dollar</Description>
    </OSCurrency>
    <OSExGSTVATAmount>-120.0000</OSExGSTVATAmount>
    <TransactionType>INV</TransactionType>

    <PostingJournalCollection>
      <PostingJournal>
        <Branch>
          <Code>SYD</Code>
        </Branch>
		<GLAccount>
		  <AccountCode>" + TestObjectCreator.GLJournalClearingAccount.AccountNum + @"</AccountCode>
		  <Description>FREIGHT REVENUE ACTUAL</Description>
		</GLAccount>
        <VATTaxID>
          <TaxCode>FREEGST</TaxCode>
		</VATTaxID>        
		<Department>
          <Code>BRN</Code>
        </Department>
        <Description>FREIGHT REVENUE ACTUAL</Description>
        <IsFinalCharge>true</IsFinalCharge>
        <LocalAmount>-60.0000</LocalAmount>
        <OSAmount>-120.00</OSAmount>
        <OSCurrency>
          <Code>USD</Code>
        </OSCurrency>
         <LocalCurrency>
          <Code>AUD</Code>
        </LocalCurrency>
       <Sequence>2</Sequence>
      </PostingJournal>
		<PostingJournal>
        <Branch>
          <Code>A01</Code>
        </Branch>
		<GLAccount>
		  <AccountCode>" + TestObjectCreator.GLJournalClearingAccount.AccountNum + @"</AccountCode>
		  <Description>FREIGHT REVENUE ACTUAL</Description>
		</GLAccount>
        <VATTaxID>
          <TaxCode>FREEGST</TaxCode>
		</VATTaxID>
		<Department>
          <Code>BRN</Code>
        </Department>
        <Description>FREIGHT REVENUE ACTUAL</Description>
        <IsFinalCharge>true</IsFinalCharge>
        <LocalAmount>-60.0000</LocalAmount>
        <OSAmount>-120.00</OSAmount>
        <OSCurrency>
          <Code>USD</Code>
        </OSCurrency>
         <LocalCurrency>
          <Code>AUD</Code>
        </LocalCurrency>
       <Sequence>5</Sequence>
      </PostingJournal>
    </PostingJournalCollection>
  </TransactionInfo>
</UniversalTransaction>";
			}
		}

		string UniversalTransactionXMLWithMultipleLineErrors
		{
			get
			{
				return @"
<UniversalTransaction xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.1"">
  <TransactionInfo>

    <Branch>
      <Code>BER</Code>
    </Branch>
    <Department>
      <Code>BRN</Code>
    </Department>
    <Description>AP INVOICE</Description>
    <Ledger>AP</Ledger>
    <LocalExVATAmount>-60.0000</LocalExVATAmount>
    <Number>11112222</Number>
    <NumberOfSupportingDocuments>2</NumberOfSupportingDocuments>
    <OrganizationAddress>
      <AddressType>None</AddressType>
      <OrganizationCode>ABCFRESYD</OrganizationCode>
    </OrganizationAddress>
    <OSCurrency>
      <Code>USD</Code>
      <Description>United States Dollar</Description>
    </OSCurrency>
    <OSExGSTVATAmount>-120.0000</OSExGSTVATAmount>
    <TransactionType>INV</TransactionType>

    <PostingJournalCollection>
      <PostingJournal>
        <Branch>
          <Code>SYD</Code>
        </Branch>
		<GLAccount>
		  <AccountCode>" + TestObjectCreator.GLJournalClearingAccount.AccountNum + @"</AccountCode>
		  <Description>FREIGHT REVENUE ACTUAL</Description>
		</GLAccount>
		<Department>
          <Code>BRN</Code>
        </Department>
        <Description>FREIGHT REVENUE ACTUAL</Description>
        <IsFinalCharge>true</IsFinalCharge>
        <LocalAmount>-60.0000</LocalAmount>
        <OSAmount>-120.00</OSAmount>
        <OSCurrency>
          <Code>USD</Code>
        </OSCurrency>
         <LocalCurrency>
          <Code>AUD</Code>
        </LocalCurrency>
       <Sequence>2</Sequence>
      </PostingJournal>

      <PostingJournal>
        <Branch>
          <Code>SYD</Code>
        </Branch>
		<GLAccount>
		  <AccountCode>" + TestObjectCreator.GLJournalClearingAccount.AccountNum + @"</AccountCode>
		  <Description>FREIGHT REVENUE ACTUAL</Description>
		</GLAccount>
		<Department>
          <Code>BRN</Code>
        </Department>
        <Description>FREIGHT REVENUE ACTUAL</Description>
        <IsFinalCharge>true</IsFinalCharge>
        <LocalAmount>-30.0000</LocalAmount>
        <OSAmount>-60.00</OSAmount>
        <OSCurrency>
          <Code>USD</Code>
        </OSCurrency>
         <LocalCurrency>
          <Code>AUD</Code>
        </LocalCurrency>
       <Sequence>2</Sequence>
      </PostingJournal>
    </PostingJournalCollection>
  </TransactionInfo>
</UniversalTransaction>";
			}
		}
	}
}
