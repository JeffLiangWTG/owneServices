using System;
using System.Data;
using System.Linq;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Accounting.Business.WIPAccrual;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Core;
using Enterprise.Environment;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.ARAP.Invoicing.Testing
{
	public class InvoicingBasePostingTest : TestCaseWithFactory
	{
		public void TestOnSavingApprovalLogIsNotAddedWhenThereIsNoRequestAndNoApprovingUser()
		{
			//Case - Save Invoice without any approval request and approving user - assert log is not added to the invoice
			var invoice = Factory.NewWithValidTestData<APInvoice>();
			AssertNull("Precondition: no approval request", invoice.APInvoiceTransactionRelatedApprovalRequest);
			AssertEquals("Precondition: no approving user", ZGuid.Empty, invoice.ApprovingUserPK);

			Factory.Save();
			var invoiceApprovedLog = invoice.Logs.GetAllLogs().Cast<StmALog>().FirstOrDefault(x => x.SL_SE_NKEvent == Events.TransactionApprovalActioned.Code);
			AssertNull("No log should be added as there is no request and no approving user", invoiceApprovedLog);
		}

		public void TestOnSavingApprovalLogIsNotAddedForOnlyApprovedRequest()
		{
			//Case - Only Approved -- Assert no log is added

			var invoice = TestObjectCreator.CreateAPInvoiceForApprovalRequest<APInvoice>(TestObjectCreator.AALSHI, 100);
			var approvalRequestInOtherFactory = TestObjectCreator.CreateAndSaveApprovalRequestForInvoiceInNewFactoryAsInProduction(invoice);

			AssertEquals("Precondition: approval request status should be requested", Constants.GenApprovalRequestApprovalStatus.Requested, approvalRequestInOtherFactory.XP_ApprovalStatus);
			var invoiceApprovedLog = invoice.Logs.GetAllLogs().Cast<StmALog>().FirstOrDefault(x => x.SL_SE_NKEvent == Events.TransactionApprovalActioned.Code);
			Assert("Precondition: Invoice saved as incomplete", invoice.IsIncompleteInvoice);
			AssertNull("No log should be added as the request status is not posted", invoiceApprovedLog);

			approvalRequestInOtherFactory.XP_ApprovalStatus = Constants.GenApprovalRequestApprovalStatus.Approved;
			approvalRequestInOtherFactory.Factory.Save();

			invoiceApprovedLog = invoice.Logs.GetAllLogs().Cast<StmALog>().FirstOrDefault(x => x.SL_SE_NKEvent == Events.TransactionApprovalActioned.Code);
			AssertNotNull("Log should be added as the request status is approved", invoiceApprovedLog);
			AssertEquals("IN|INI|Approved For Posting", invoiceApprovedLog.SL_Reference);
		}

		public void TestOnSavingApprovalLogIsNotAddedForApprovedThenCanceledRequest()
		{
			//Case - Invoice Approval request approved and then the request is canceled and after that invoice is posted --  Assert no log is added

			var invoice = TestObjectCreator.CreateAPInvoiceForApprovalRequest<APInvoice>(TestObjectCreator.AALSHI, 100);
			var approvalRequestInOtherFactory = TestObjectCreator.CreateAndSaveApprovalRequestForInvoiceInNewFactoryAsInProduction(invoice);

			AssertEquals("Precondition: approval request status should be requested", Constants.GenApprovalRequestApprovalStatus.Requested, approvalRequestInOtherFactory.XP_ApprovalStatus);
			var invoiceApprovedLog = invoice.Logs.GetAllLogs().Cast<StmALog>().FirstOrDefault(x => x.SL_SE_NKEvent == Events.TransactionApprovalActioned.Code);
			Assert("Precondition: Invoice saved as incomplete", invoice.IsIncompleteInvoice);
			AssertNull("No log should be added as the request status is not posted", invoiceApprovedLog);

			approvalRequestInOtherFactory.XP_ApprovalStatus = Constants.GenApprovalRequestApprovalStatus.Approved;
			approvalRequestInOtherFactory.Factory.Save();

			AssertEquals("Precondition: Invoice Factory request status is approved", Constants.GenApprovalRequestApprovalStatus.Approved, invoice.APInvoiceTransactionRelatedApprovalRequest.XP_ApprovalStatus);
			invoiceApprovedLog = invoice.Logs.GetAllLogs().Cast<StmALog>().FirstOrDefault(x => x.SL_SE_NKEvent == Events.TransactionApprovalActioned.Code);
			AssertNotNull("Log should be added as the request status is approved", invoiceApprovedLog);
			AssertEquals("IN|INI|Approved For Posting", invoiceApprovedLog.SL_Reference);

			approvalRequestInOtherFactory.Factory.RefreshEnabled = false;
			approvalRequestInOtherFactory.XP_ApprovalStatus = Constants.GenApprovalRequestApprovalStatus.Cancelled;
			approvalRequestInOtherFactory.SetContext(BusinessContext.CancelApprovalRequestByUser);
			approvalRequestInOtherFactory.Factory.Save();

			AssertEquals("Precondition: Invoice Factory request status is still approved", Constants.GenApprovalRequestApprovalStatus.Approved, invoice.APInvoiceTransactionRelatedApprovalRequest.XP_ApprovalStatus);
			invoice.MoveFromIncompleteToPayableLedger();
			Factory.Save();

			AssertEquals("Invoice should be posted", true, invoice.IsPosted);
			invoiceApprovedLog = invoice.Logs.GetAllLogs().Cast<StmALog>().Where(x => x.SL_SE_NKEvent == Events.TransactionApprovalActioned.Code).Skip(1).FirstOrDefault();
			AssertNull("No log should be added as the request status is not posted even if invoice is posted", invoiceApprovedLog);
		}

		[TestDate(2016, 11, 09)]
		public void TestOnSavingApprovalLogIsAddedForInvoiceWithApprovingUserPK()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_Code = "ABC";
			Factory.Save();

			var invoiceWithoutApprovalRequest = Factory.NewWithValidTestData<APInvoice>();
			invoiceWithoutApprovalRequest.ApprovingUserPK = staff.PK;
			Factory.Save();

			var invoiceApprovedLog = invoiceWithoutApprovalRequest.Logs.GetAllLogs().FirstOrDefault(x => ((StmALog)x).SL_SE_NKEvent == Events.TransactionApprovalActioned.Code) as StmALog;
			AssertNotNull("Invoice approved log should be added", invoiceApprovedLog);
			AssertEquals("Approving Reference", "AP|INV|Approved and Posted", invoiceApprovedLog.SL_Reference);
			AssertEquals("Approving DateTime", ZDateTime.Today.Date, invoiceApprovedLog.SL_EventTime.Date);
			AssertEquals("Approving user", "ABC", invoiceApprovedLog.SL_GS_NKUser);
		}

		public void TestOnSavingApprovalLogIsAddedForPostedRequestInAnotherFactory()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_Code = "ABC";
			Factory.Save();

			var invoiceWithApprovalRequest = TestObjectCreator.CreateAPInvoiceForApprovalRequest<APInvoice>(TestObjectCreator.AALSHI, 100);
			var approvalRequestInOtherFactory = TestObjectCreator.CreateAndSaveApprovalRequestForInvoiceInNewFactoryAsInProduction(invoiceWithApprovalRequest);

			AssertEquals("Precondition: approval request status should be requested", Constants.GenApprovalRequestApprovalStatus.Requested, approvalRequestInOtherFactory.XP_ApprovalStatus);
			var invoiceApprovedLog = invoiceWithApprovalRequest.Logs.GetAllLogs().Cast<StmALog>().FirstOrDefault(x => x.SL_SE_NKEvent == Events.TransactionApprovalActioned.Code);
			Assert("Precondition: Invoice saved as incomplete", invoiceWithApprovalRequest.IsIncompleteInvoice);
			AssertNull("No log should be added as the request status is not posted", invoiceApprovedLog);

			approvalRequestInOtherFactory.XP_ApprovalStatus = Constants.GenApprovalRequestApprovalStatus.Approved;
			approvalRequestInOtherFactory.XP_GS_NKApprovingUser1 = staff.GS_Code;
			approvalRequestInOtherFactory.XP_ApprovalDate = new ZDateTime(2016, 3, 15);
			approvalRequestInOtherFactory.Factory.Save();

			AssertEquals("Precondition: Invoice Factory request status is approved", Constants.GenApprovalRequestApprovalStatus.Approved, invoiceWithApprovalRequest.APInvoiceTransactionRelatedApprovalRequest.XP_ApprovalStatus);
			invoiceApprovedLog = invoiceWithApprovalRequest.Logs.GetAllLogs().Cast<StmALog>().FirstOrDefault(x => x.SL_SE_NKEvent == Events.TransactionApprovalActioned.Code);
			AssertNotNull("Log should be added as the request status is approved", invoiceApprovedLog);
			AssertEquals("IN|INI|Approved For Posting", invoiceApprovedLog.SL_Reference);

			approvalRequestInOtherFactory.Factory.RefreshEnabled = false;
			approvalRequestInOtherFactory.XP_ApprovalStatus = Constants.GenApprovalRequestApprovalStatus.Posted;
			approvalRequestInOtherFactory.Factory.Save();

			AssertEquals("Precondition: Invoice Factory request status is still approved", Constants.GenApprovalRequestApprovalStatus.Approved, invoiceWithApprovalRequest.APInvoiceTransactionRelatedApprovalRequest.XP_ApprovalStatus);
			invoiceWithApprovalRequest.MoveFromIncompleteToPayableLedger();
			Factory.Save();

			invoiceApprovedLog = invoiceWithApprovalRequest.Logs.GetAllLogs().Cast<StmALog>().Where(x => x.SL_SE_NKEvent == Events.TransactionApprovalActioned.Code).Skip(1).FirstOrDefault();
			AssertNotNull("Invoice approved log should be added", invoiceApprovedLog);
			AssertEquals("Approving Reference", "AP|INV|Approved and Posted", invoiceApprovedLog.SL_Reference);
			AssertEquals("Approving DateTime", new ZDateTime(2016, 3, 15), invoiceApprovedLog.SL_EventTime);
			AssertEquals("Approving user", "ABC", invoiceApprovedLog.SL_GS_NKUser);
		}

		public void TestOnSavingApprovalLogIsAddedForPostedRequestInSameFactory()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_Code = "ABC";
			Factory.Save();

			var invoiceWithApprovalRequest = TestObjectCreator.CreateAPInvoiceForApprovalRequest<APInvoice>(TestObjectCreator.AALSHI, 100);
			var approvalRequestInOtherFactory = TestObjectCreator.CreateAndSaveApprovalRequestForInvoiceInNewFactoryAsInProduction(invoiceWithApprovalRequest);
			var requestInCurrentFactory = Factory.Load<APInvoiceChargesApprovalRequest>(approvalRequestInOtherFactory.PK);

			AssertEquals("Precondition: approval request status should be requested", Constants.GenApprovalRequestApprovalStatus.Requested, approvalRequestInOtherFactory.XP_ApprovalStatus);
			var invoiceApprovedLog = invoiceWithApprovalRequest.Logs.GetAllLogs().Cast<StmALog>().FirstOrDefault(x => x.SL_SE_NKEvent == Events.TransactionApprovalActioned.Code);
			Assert("Precondition: Invoice saved as incomplete", invoiceWithApprovalRequest.IsIncompleteInvoice);
			AssertNull("No log should be added as the request status is not posted", invoiceApprovedLog);

			approvalRequestInOtherFactory.XP_ApprovalStatus = Constants.GenApprovalRequestApprovalStatus.Approved;
			approvalRequestInOtherFactory.XP_GS_NKApprovingUser1 = staff.GS_Code;
			approvalRequestInOtherFactory.XP_ApprovalDate = new ZDateTime(2016, 3, 15);
			approvalRequestInOtherFactory.Factory.Save();

			AssertEquals("Precondition: Invoice Factory request status is approved", Constants.GenApprovalRequestApprovalStatus.Approved, invoiceWithApprovalRequest.APInvoiceTransactionRelatedApprovalRequest.XP_ApprovalStatus);
			invoiceApprovedLog = invoiceWithApprovalRequest.Logs.GetAllLogs().Cast<StmALog>().FirstOrDefault(x => x.SL_SE_NKEvent == Events.TransactionApprovalActioned.Code);
			AssertNotNull("Log should be added as the request status is approved", invoiceApprovedLog);
			AssertEquals("IN|INI|Approved For Posting", invoiceApprovedLog.SL_Reference);

			approvalRequestInOtherFactory.XP_ApprovalStatus = Constants.GenApprovalRequestApprovalStatus.Posted;
			approvalRequestInOtherFactory.Factory.Save();

			AssertEquals("Precondition: Invoice Factory request status is posted", Constants.GenApprovalRequestApprovalStatus.Posted, invoiceWithApprovalRequest.APInvoiceTransactionRelatedApprovalRequest.XP_ApprovalStatus);
			invoiceWithApprovalRequest.MoveFromIncompleteToPayableLedger();
			Factory.Save();

			invoiceApprovedLog = invoiceWithApprovalRequest.Logs.GetAllLogs().Cast<StmALog>().Where(x => x.SL_SE_NKEvent == Events.TransactionApprovalActioned.Code).Skip(1).FirstOrDefault();
			AssertNotNull("Invoice approved log should be added", invoiceApprovedLog);
			AssertEquals("Approving Reference", "AP|INV|Approved and Posted", invoiceApprovedLog.SL_Reference);
			AssertEquals("Approving DateTime", new ZDateTime(2016, 3, 15), invoiceApprovedLog.SL_EventTime);
			AssertEquals("Approving user", "ABC", invoiceApprovedLog.SL_GS_NKUser);
		}

		public void TestCreateWIPWhenCostIsGreaterThanAccrualWithRegistryFlagSet()
		{
			AccountingConfigurationRegistry.Instance.CreateWIPWhenCostIsGreaterThanAccrual.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, true);

			SetupWIPsAndAccrualsForWIPAccrualCreationTests();
			SetupAndSaveInvoiceMediatorForWIPAccrualCreationTests(200.00m, LedgerTypes.AccountsPayable);

			Factory.Save();

			AssertAccrualsAreReversedCorrectly();

			ZQuery wIPFilter = new ZQuery(AccTransactionLinesSchema.AL_LineType, TransactionLineTypes.WIP);
			WIPAccrualCollection loadedWIPs = new WIPAccrualCollection(Factory, wIPFilter);
			loadedWIPs.Load();
			AssertEquals("Should only be one WIP created", 1, loadedWIPs.Count);

			AssertEquals("Amount WIP was created for should be difference between existing Accruals and actual cost entered", 45.00m, loadedWIPs[0].AL_LocalExTaxAmount);
			AssertEquals("WIP Post Date should be the same as AP Invoice Post Date", APInvoicePostDate.Date, loadedWIPs[0].AL_PostDate.ToDateTime());
			AssertEquals("WIP should have same job as invoice line", loadedWIPs[0].AL_JH, Job1.PK);
			AssertEquals("WIP should have same charge code as invoice line", loadedWIPs[0].AL_AC, MarginChargeCode.PK.ToGuid());
			AssertEquals("WIP should have same department as invoice line", loadedWIPs[0].AL_GE, CEADepartment.PK.ToGuid());
			AssertEquals("WIP should have same branch as invoice line", loadedWIPs[0].AL_GB, Env.CurrentBranch.PK);
		}

		public void TestAccrualCreationWhenCostIsLessThanExistingAccruals()
		{
			SetupWIPsAndAccrualsForWIPAccrualCreationTests();
			SetupAndSaveInvoiceMediatorForWIPAccrualCreationTests(100.00m, LedgerTypes.AccountsPayable);

			Factory.Save();

			AssertAccrualsAreReversedCorrectly();

			WIPAccrualCollection loadedAccruals = GetNewlyCreatedAccrualsAfterReversingAllApplicable();

			AssertEquals("Should only be one unreversed accrual", 1, loadedAccruals.Count);
			Accrual createdAccrual = loadedAccruals[0] as Accrual;
			AssertEquals("Created accrual should be for 55 dollars", 55.00m, createdAccrual.AL_LocalExTaxAmount);
			AssertEquals("Created date on accrual should be same as AP Invoice being posted", APInvoicePostDate.Date, createdAccrual.AL_PostDate.Date.ToDateTime());
			AssertEquals("Created accrual's branch should be same branch as line", Env.CurrentBranch.PK, createdAccrual.AL_GB.ToGuid());
			AssertEquals("Created accrual's department should be the same as line", CEADepartment.PK, createdAccrual.AL_GE);
			AssertEquals("Created accrual's charge code should be the same as line", MarginChargeCode.PK, createdAccrual.AL_AC);
			AssertEquals("Created accrual's job should be same as line", Job1.PK, createdAccrual.AL_JH.ToGuid());
		}

		public void TestIsFinalCreatesNoNewAccruals()
		{
			AccountingConfigurationRegistry.Instance.CreateWIPWhenCostIsGreaterThanAccrual.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, true);

			SetupWIPsAndAccrualsForWIPAccrualCreationTests();
			SetupAndSaveInvoiceMediatorForWIPAccrualCreationTests(100.00m, LedgerTypes.AccountsPayable);

			TestInvoice.Lines[0].AL_IsFinalCharge = ZBool.True;
			Factory.Save();
			AssertAccrualsAreReversedCorrectly();

			WIPAccrualCollection createdAccruals = GetNewlyCreatedAccrualsAfterReversingAllApplicable();
			AssertEquals("Should be no accruals created when is final is ticked for line", 0, createdAccruals.Count);

			WIPAccrualCollection createdWIPS = GetNewlyCreatedWIPsAfterReversingAllApplicable();
			AssertEquals("Should be no WIPS created either, when is final is ticked for line", 0, createdWIPS.Count);
		}

		public void TestCreateWIPWhenNoAccrualsExist()
		{
			AccountingConfigurationRegistry.Instance.CreateWIPWhenCostIsGreaterThanAccrual.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, true);

			SetupAndSaveInvoiceMediatorForWIPAccrualCreationTests(100.00m, LedgerTypes.AccountsPayable);
			Factory.Save();

			ZQuery wIPFilter = new ZQuery(AccTransactionLinesSchema.AL_LineType, TransactionLineTypes.WIP);
			wIPFilter.AddToFilter(AccTransactionLinesSchema.AL_GC, GlbCompany.CurrentCompany.PK);
			WIPAccrualCollection newlyCreatedWIPs = new WIPAccrualCollection(Factory, wIPFilter);
			newlyCreatedWIPs.Load();

			AssertEquals("Should be one WIP only created", 1, newlyCreatedWIPs.Count);
			WIP newlyCreatedWIP = newlyCreatedWIPs[0] as WIP;
			AssertEquals("WIP Amount should be (LineCost / MarginPercentage) * 100", 111.11m, newlyCreatedWIP.AL_LocalExTaxAmount);
			AssertEquals("WIP Post date should be same as AP Invoice being posted", APInvoicePostDate.Date, newlyCreatedWIP.AL_PostDate);
		}

		public void TestDontCreateWIPWhenNoAccrualsExistANDIsFinalIsTrue()
		{
			AccountingConfigurationRegistry.Instance.CreateWIPWhenCostIsGreaterThanAccrual.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, true);
			AccountingConfigurationRegistry.Instance.CreateWIPWhenCostIsFinal.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, true);

			SetupAndSaveInvoiceMediatorForWIPAccrualCreationTests(100.00m, LedgerTypes.AccountsPayable);
			TestInvoice.Lines[0].AL_IsFinalCharge = ZBool.True;
			Factory.Save();

			ZQuery wIPFilter = new ZQuery(AccTransactionLinesSchema.AL_LineType, TransactionLineTypes.WIP);
			wIPFilter.AddToFilter(AccTransactionLinesSchema.AL_GC, GlbCompany.CurrentCompany.PK);
			WIPAccrualCollection newlyCreatedWIPs = new WIPAccrualCollection(Factory, wIPFilter);
			newlyCreatedWIPs.Load();

			AssertEquals("Should be a WIP created", 1, newlyCreatedWIPs.Count);
		}

		public void TestDontSetSellAmountWhenNoAccrualsExistANDIsFinalIsTrue()
		{
			AccountingConfigurationRegistry.Instance.CreateWIPWhenCostIsGreaterThanAccrual.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, true);
			AccountingConfigurationRegistry.Instance.CreateWIPWhenCostIsFinal.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, true);

			SetupAndSaveInvoiceMediatorForWIPAccrualCreationTests(100.00m, LedgerTypes.AccountsPayable);
			TestInvoice.Lines[0].AL_IsFinalCharge = ZBool.True;
			Factory.Save();

			ChargeCollection charges = GetCreatedJobCharges(Job1);
			AssertEquals("1 Job Charge should be created.", 1, charges.Count);

			AssertEquals("Sell Amount should be 0.", 111.11m, charges[0].JR_OSSellAmt);
		}

		protected WIPAccrualCollection GetNewlyCreatedAccrualsAfterReversingAllApplicable()
		{
			ZQuery accrualFilter = new ZQuery(AccTransactionLinesSchema.AL_LineType, TransactionLineTypes.Accrual);
			accrualFilter.AddToFilter(AccTransactionLinesSchema.AL_GB, SQLComparisonOperator.Equal, GlbBranch.CurrentBranch.PK.ToGuid());
			accrualFilter.AddToFilter(AccTransactionLinesSchema.AL_GE, SQLComparisonOperator.Equal, CEADepartment.PK);
			accrualFilter.AddToFilter(AccTransactionLinesSchema.AL_ReverseDate, SQLComparisonOperator.Equal, null);
			accrualFilter.AddToFilter(AccTransactionLinesSchema.AL_JH, SQLComparisonOperator.Equal, Job1PK);
			accrualFilter.AddToFilter(AccTransactionLinesSchema.AL_LineType, SQLComparisonOperator.Equal, TransactionLineTypes.Accrual);

			WIPAccrualCollection result = new WIPAccrualCollection(Factory, accrualFilter);
			result.Load();

			return result;
		}

		protected WIPAccrualCollection GetNewlyCreatedWIPsAfterReversingAllApplicable()
		{
			ZQuery accrualFilter = new ZQuery(AccTransactionLinesSchema.AL_LineType, TransactionLineTypes.WIP);
			accrualFilter.AddToFilter(AccTransactionLinesSchema.AL_GB, SQLComparisonOperator.Equal, GlbBranch.CurrentBranch.PK.ToGuid());
			accrualFilter.AddToFilter(AccTransactionLinesSchema.AL_GE, SQLComparisonOperator.Equal, CEADepartment.PK);
			accrualFilter.AddToFilter(AccTransactionLinesSchema.AL_ReverseDate, SQLComparisonOperator.Equal, null);
			accrualFilter.AddToFilter(AccTransactionLinesSchema.AL_JH, SQLComparisonOperator.Equal, Job1PK);

			WIPAccrualCollection result = new WIPAccrualCollection(Factory, accrualFilter);
			result.Load();

			return result;
		}

		protected ChargeCollection GetCreatedJobCharges(Job job)
		{
			ChargeCollection result = new ChargeCollection(job);
			result.Load();

			return result;
		}

		#region Implementation

		GlbDepartment CEADepartment;
		GlbDepartment FEADepartment;
		OrgHeader TestOrganisation;
		Guid Job1PK;
		Job Job1;
		Job Job2;
		AccChargeCode MarginChargeCode;
		APInvoice TestInvoice;
		WIPAccrualCollection ApplicableAccruals;
		WIPAccrualCollection NonApplicableAccruals;
		ZDateTime APInvoicePostDate;
		AccountingPeriodTestHelper PeriodHelper;
		TestObjectCreator TestObjectCreator;

		protected override void SetUp()
		{
			base.SetUp();
			TestObjectCreator = new TestObjectCreator(Factory);

			ZQuery fEADepartmentFilter = new ZQuery(GlbDepartmentSchema.GE_Code, "FEA");
			FEADepartment = Factory.LoadTop1<GlbDepartment>(fEADepartmentFilter);

			ZQuery cEADepartmentFilter = new ZQuery(GlbDepartmentSchema.GE_Code, "CEA");
			CEADepartment = Factory.LoadTop1<GlbDepartment>(cEADepartmentFilter);

			TestOrganisation = Factory.LoadTop1<OrgHeader>(new ZQuery());
			TestOrganisation.CompanyData.OB_IsCreditor = true;
			TestOrganisation.CompanyData.OB_IsDebtor = true;

			Job1 = Factory.NewJobWithValidTestDataForTesting<Job>();
			Job2 = Factory.NewJobWithValidTestDataForTesting<Job>();

			Job1.JH_GE = GlbDepartment.CurrentDepartment.PK;
			Job2.JH_GE = GlbDepartment.CurrentDepartment.PK;

			Job1.JH_GB = GlbBranch.CurrentBranch.PK;
			Job2.JH_GB = GlbBranch.CurrentBranch.PK;

			Job1.LocalChargesPK = TestOrganisation.PK;
			Job2.LocalChargesPK = TestOrganisation.PK;

			Job1PK = Job1.PK.ToGuid();

			MarginChargeCode = TestObjectCreator.InsertMarginChargeCode(90);
			MarginChargeCode.Factory.Save();

			PeriodHelper = new AccountingPeriodTestHelper();
			PeriodHelper.SetupPeriods();

			APInvoicePostDate = PeriodHelper.PreviousOpenPeriod.AM_StartDate.ToDateTime();
		}

		protected void SetupWIPsAndAccrualsForWIPAccrualCreationTests()
		{
			ChargeCollection applicableAccrualCharges = new ChargeCollection(Job1);
			ChargeCollection nonApplicableAccrualCharges = new ChargeCollection(Job2);

			applicableAccrualCharges.Add(AddCharge(50.00m, 0m, GlbBranch.CurrentBranch.PK.ToGuid(), CEADepartment.PK.ToGuid(), Job1, MarginChargeCode.PK.ToGuid()));
			applicableAccrualCharges.Add(AddCharge(60.00m, 0m, GlbBranch.CurrentBranch.PK.ToGuid(), CEADepartment.PK.ToGuid(), Job1, MarginChargeCode.PK.ToGuid()));
			applicableAccrualCharges.Add(AddCharge(45.00m, 0m, GlbBranch.CurrentBranch.PK.ToGuid(), CEADepartment.PK.ToGuid(), Job1, MarginChargeCode.PK.ToGuid()));
			nonApplicableAccrualCharges.Add(AddCharge(35.78m, 0m, GlbBranch.CurrentBranch.PK.ToGuid(), CEADepartment.PK.ToGuid(), Job2, MarginChargeCode.PK.ToGuid()));
			nonApplicableAccrualCharges.Add(AddCharge(3500.00m, 0m, GlbBranch.CurrentBranch.PK.ToGuid(), FEADepartment.PK.ToGuid(), Job2, MarginChargeCode.PK.ToGuid()));

			Factory.Save();

			ApplicableAccruals = new WIPAccrualCollection(Factory, new ZQuery());
			NonApplicableAccruals = new WIPAccrualCollection(Factory, new ZQuery());
			foreach (Charge applicableAccrualCharge in applicableAccrualCharges)
			{
				ApplicableAccruals.Add(applicableAccrualCharge.Accrual);
			}
			foreach (Charge nonApplicableAccrualCharge in nonApplicableAccrualCharges)
			{
				NonApplicableAccruals.Add(nonApplicableAccrualCharge.Accrual);
			}
		}

		protected void SetupAndSaveInvoiceMediatorForWIPAccrualCreationTests(decimal firstLineAmount, string ledger)
		{
			TestInvoice = Factory.New<APInvoice>();

			TestInvoice.AH_TransactionType = TransactionTypes.Invoice;
			TestInvoice.AH_PostDate = APInvoicePostDate;
			TestInvoice.AH_DueDate = ZDateTime.Now;
			TestInvoice.AH_OH = TestOrganisation.PK;
			TestInvoice.AH_TransactionNum = "INVONE";
			TestInvoice.AH_GE = CEADepartment.PK;

			TestInvoice.Lines.AddNew();
			TestInvoice.Lines[0].AL_LineType = TransactionLineTypes.Cost;
			TestInvoice.Lines[0].AL_JH = Job1PK;
			TestInvoice.Lines[0].AL_GE = CEADepartment.PK;

			TestInvoice.Lines[0].AL_AC = MarginChargeCode.PK;
			TestInvoice.Lines[0].AL_OSExTaxAmount = firstLineAmount;
			TestInvoice.Lines[0].AL_PostDate = APInvoicePostDate;

			TestInvoice.SubmittedFromInvoicingForm = true;
		}

		protected WIPAccrualCollection GetLoadedAccruals()
		{
			ZQuery accrualFilter = new ZQuery(AccTransactionLinesSchema.AL_LineType, TransactionLineTypes.Accrual);
			accrualFilter.AddToFilter(AccTransactionLinesSchema.AL_GC, GlbCompany.CurrentCompany.PK);
			WIPAccrualCollection loadedAccruals = new WIPAccrualCollection(Factory, accrualFilter);
			loadedAccruals.Load();
			return loadedAccruals;
		}

		protected void AssertAccrualsAreReversedCorrectly()
		{
			WIPAccrualCollection loadedAccruals = GetLoadedAccruals();
			foreach (BaseWIPAccrual wIPAccrual in loadedAccruals)
			{
				if (ApplicableAccruals.Contains(wIPAccrual.PK))
				{
					Assert("Accrual of amount" + wIPAccrual.AL_LocalExTaxAmount.ToString() + " was not reversed. All applicable accruals should have been reversed", wIPAccrual.IsReversed);
					AssertEquals("Accrual posted date should be same as AP Invoice Post Date", APInvoicePostDate.Date, wIPAccrual.AL_ReverseDate.ToDateTime().Date);
				}
				else
				{
					Assert("Accrual of amount" + wIPAccrual.AL_LocalExTaxAmount.ToString() + " was reversed. All non applicable accruals should not have been reversed", !wIPAccrual.IsReversed);
				}
			}
		}

		protected Guid GetFirstRevenueChargeCode()
		{
			string sQL = "SELECT TOP 1 " + AccChargeCodeSchema.Constants.PK + " FROM " + AccChargeCodeSchema.Constants.SqlSchemaName + "." + AccChargeCodeSchema.Constants.TableName + " WHERE " + AccChargeCodeSchema.Constants.AC_ChargeType + " = @RevenueChargeType";
			DbCommand command = Db.Connection.Command(sQL); // This code is used extensively for test. We need to keep the code as they were in order to keep compatibility
			command.AddParameter("@RevenueChargeType", SqlDbType.Char, Core.Constants.ChargeType.Revenue);
			return Utilities.GetGuidFromObject(command.ExecuteScalar());
		}

		protected void SetJobStatusToClosedForPK(Guid jobHeaderPK)
		{
			string sQL = string.Format("UPDATE {0} SET {1} = @Status WHERE {2} = @PK", JobHeaderSchema.Constants.TableName, JobHeaderSchema.Constants.JH_Status, JobHeaderSchema.Constants.PK);
			DbCommand command = Db.Connection.Command(sQL); // This code is used extensively for test. We need to keep the code as they were in order to keep compatibility
			command.AddParameter("@Status", SqlDbType.Char, 3, JobHeaderStatus.Closed.Code);
			command.AddParameter("@PK", SqlDbType.UniqueIdentifier, jobHeaderPK);

			command.ExecuteNonQuery();
		}

		protected WIP AddWIP(decimal amount, Guid branch, Guid department, Guid jobPK, Guid chargeCodePK)
		{
			WIP wIP = Factory.New<WIP>();
			AddWIPAccrual(wIP, amount, branch, department, jobPK, chargeCodePK);
			return wIP;
		}

		protected Accrual AddAccrual(decimal amount, Guid branch, Guid department, Guid jobPK, Guid chargeCodePK)
		{
			Accrual accrual = Factory.New<Accrual>();
			AddWIPAccrual(accrual, amount, branch, department, jobPK, chargeCodePK);
			return accrual;
		}

		protected void AddWIPAccrual(BaseWIPAccrual wIPAccrual, decimal amount, Guid branch, Guid department, Guid jobPK, Guid chargeCodePK)
		{
			wIPAccrual.AL_LocalExTaxAmount = amount;
			wIPAccrual.AL_GB = branch;
			wIPAccrual.AL_JH = jobPK;
			wIPAccrual.AL_GE = department;
			wIPAccrual.AL_AC = chargeCodePK;
		}

		protected Charge AddCharge(decimal costAmount, decimal revenueAmount, Guid branch, Guid department, Job job, Guid chargeCodePK)
		{
			Charge charge = job.Charges.AddNew();
			charge.JR_AC = chargeCodePK;
			charge.JR_GB = branch;
			charge.JR_GE = department;
			charge.JR_OSCostAmt = costAmount;
			charge.JR_OSSellAmt = revenueAmount;
			return charge;
		}

		#endregion
	}
}
