using System.Linq;
using System.Text.RegularExpressions;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.CommissionManagement.Business.Testing
{
	[TestedType(typeof(AccCommissionApprovalRequest))]
	internal class AccCommissionApprovalRequestTest : EnterpriseBusinessObjectTestCase
	{
		#region Properties

		[TestDate(2002, 2, 2)]
		[TestUtcOffset(10, 0, 0)]
		public void TestApproveStatus1Description()
		{
			var approvalRequest = Factory.New<AccCommissionApprovalRequest>();
			approvalRequest.CRQ_GS_NKApprovingStaff1 = ZString.Empty;
			AssertEquals(ZString.Empty, approvalRequest.ApproveStatus1Description);

			approvalRequest.CRQ_GS_NKApprovingStaff1 = "ADL";
			AssertEquals(CommissionApprovalRequestApproveStatusList.Descriptions.Pending, approvalRequest.ApproveStatus1Description);

			approvalRequest.ApproveStaff1();
			Factory.Save();
			AssertEquals(CommissionApprovalRequestApproveStatusList.Descriptions.Approved + ": 02-Feb-02 10:00", approvalRequest.ApproveStatus1Description);
		}

		[TestDate(2002, 2, 2)]
		[TestUtcOffset(10, 0, 0)]
		public void TestApproveStatus2Description()
		{
			var approvalRequest = Factory.New<AccCommissionApprovalRequest>();
			approvalRequest.CRQ_GS_NKApprovingStaff2 = ZString.Empty;
			AssertEquals(ZString.Empty, approvalRequest.ApproveStatus2Description);

			approvalRequest.CRQ_GS_NKApprovingStaff2 = "ADL";
			AssertEquals(CommissionApprovalRequestApproveStatusList.Descriptions.Pending, approvalRequest.ApproveStatus2Description);

			approvalRequest.ApproveStaff2();
			Factory.Save();
			AssertEquals(CommissionApprovalRequestApproveStatusList.Descriptions.Approved + ": 02-Feb-02 10:00", approvalRequest.ApproveStatus2Description);
		}

		public void TestCRQ_GS_NKApprovingStaff_ReadOnly()
		{
			var approvalRequest = Factory.NewWithValidTestData<AccCommissionApprovalRequest>();

			AssertEquals(false, approvalRequest.CRQ_GS_NKApprovingStaff1Info.ReadOnly);
			AssertEquals(false, approvalRequest.CRQ_GS_NKApprovingStaff2Info.ReadOnly);

			Factory.Save();

			AssertEquals("Should be readonly once saved.", true, approvalRequest.CRQ_GS_NKApprovingStaff1Info.ReadOnly);
			AssertEquals("Should be readonly once saved.", true, approvalRequest.CRQ_GS_NKApprovingStaff2Info.ReadOnly);
		}

		public void TestStatus()
		{
			var approval = Factory.New<AccCommissionApprovalRequest>();
			var item = approval.Items.AddNew();

			approval.CRQ_GS_NKApprovingStaff1 = ZString.Empty;
			approval.CRQ_GS_NKApprovingStaff2 = ZString.Empty;
			approval.CRQ_Staff1HasApproved = false;
			approval.CRQ_Staff2HasApproved = false;
			AssertEquals(CommissionApprovalRequestStatusList.Codes.Approved, approval.Status);

			approval.CRQ_GS_NKApprovingStaff1 = "ADL";
			approval.CRQ_Staff1HasApproved = false;
			AssertEquals(CommissionApprovalRequestStatusList.Codes.Pending, approval.Status);

			approval.CRQ_Staff1HasApproved = true;
			AssertEquals(CommissionApprovalRequestStatusList.Codes.Approved, approval.Status);

			approval.CRQ_GS_NKApprovingStaff2 = "SCW";
			approval.CRQ_Staff2HasApproved = false;
			AssertEquals(CommissionApprovalRequestStatusList.Codes.Pending, approval.Status);

			approval.CRQ_Staff2HasApproved = true;
			AssertEquals(CommissionApprovalRequestStatusList.Codes.Approved, approval.Status);

			item.Delete();
			AssertEquals(CommissionApprovalRequestStatusList.Codes.Canceled, approval.Status);
		}

		[TestDate(2002, 2, 2)]
		public void TestApprove()
		{
			var commissionLine1 = Factory.NewWithValidTestData<AccCommissionLine>();
			var commissionLine2 = Factory.NewWithValidTestData<AccCommissionLine>();
			var commissionLine3 = Factory.NewWithValidTestData<AccCommissionLine>();

			var approvalRequest = Factory.NewWithValidTestData<AccCommissionApprovalRequest>();
			approvalRequest.CRQ_GS_NKApprovingStaff1 = "ADL";
			approvalRequest.CRQ_GS_NKApprovingStaff2 = "SCW";

			var item1 = approvalRequest.Items.AddNew();
			item1.CRI_CL0 = commissionLine1.PK;
			item1.CRI_IsSelected = true;

			var item2 = approvalRequest.Items.AddNew();
			item2.CRI_CL0 = commissionLine2.PK;
			item2.CRI_IsSelected = true;

			var item3 = approvalRequest.Items.AddNew();
			item3.CRI_CL0 = commissionLine3.PK;
			item3.CRI_IsSelected = false;

			Factory.Save();

			var viewCommissionLine1 = item1.ViewCommissionLine;
			var viewCommissionLine2 = item2.ViewCommissionLine;
			var viewCommissionLine3 = item3.ViewCommissionLine;

			approvalRequest.ApproveStaff1();
			AssertEquals("Should have overriden the approving staff with the actual approving staff", GlbStaff.CurrentUser.GS_Code, approvalRequest.CRQ_GS_NKApprovingStaff1);
			CombineAssertions("Shouldn't flag lines as approved until both staff have approved", () =>
			{
				AssertEquals("commissionLine1", ZDateTime.Empty, commissionLine1.CL0_ApprovedDateTimeUtc);
				AssertEquals("commissionLine2", ZDateTime.Empty, commissionLine2.CL0_ApprovedDateTimeUtc);
				AssertEquals("commissionLine3", ZDateTime.Empty, commissionLine3.CL0_ApprovedDateTimeUtc);
			});

			var anotherStaff = Factory.New<GlbStaff>();
			anotherStaff.GS_Code = "RIS";

			using (Env.SetTemporaryUserContext(new UserContext(anotherStaff, Env.CurrentBranch.PK, Env.CurrentDepartment.PK)))
			{
				approvalRequest.ApproveStaff2();
				AssertEquals("Should have overriden the approving staff with the actual approving staff", "RIS", approvalRequest.CRQ_GS_NKApprovingStaff2);
				CombineAssertions("Should only flag lines that are 'selected' as approved", () =>
				{
					AssertEquals("commissionLine1", new ZDateTime(2002, 2, 2), commissionLine1.CL0_ApprovedDateTimeUtc);
					AssertEquals("commissionLine2", new ZDateTime(2002, 2, 2), commissionLine2.CL0_ApprovedDateTimeUtc);
					AssertEquals("commissionLine3", ZDateTime.Empty, commissionLine3.CL0_ApprovedDateTimeUtc);
				});

				Factory.Save();

				CombineAssertions("Views should have been updated after factory save", () =>
				{
					AssertEquals("viewCommissionLine1", new ZDateTime(2002, 2, 2), viewCommissionLine1.VCL_ApprovedDateTimeUtc);
					AssertEquals("viewCommissionLine2", new ZDateTime(2002, 2, 2), viewCommissionLine2.VCL_ApprovedDateTimeUtc);
					AssertEquals("viewCommissionLine3", ZDateTime.Empty, viewCommissionLine3.VCL_ApprovedDateTimeUtc);
				});

				AssertNoExceptionThrown("A Factory Save after ApprovedDateTime has been set should not cause an exception", () => Factory.Save());
			}
		}

		public void TestHasAtLeastOneStaffWhoHasApproved()
		{
			var staff1 = Factory.New<GlbStaff>();
			staff1.GS_Code = "ADL";
			var staff2 = Factory.New<GlbStaff>();
			staff2.GS_Code = "SCW";

			var approval = Factory.New<AccCommissionApprovalRequest>();
			approval.CRQ_GS_NKApprovingStaff1 = "ADL";
			approval.CRQ_GS_NKApprovingStaff2 = "SCW";

			AssertEquals(false, approval.HasAtLeastOneStaffWhoHasApproved);

			approval.CRQ_Staff1HasApproved = true;
			approval.CRQ_Staff2HasApproved = false;
			AssertEquals(true, approval.HasAtLeastOneStaffWhoHasApproved);

			approval.CRQ_Staff1HasApproved = false;
			approval.CRQ_Staff2HasApproved = true;
			AssertEquals(true, approval.HasAtLeastOneStaffWhoHasApproved);

			approval.CRQ_Staff1HasApproved = true;
			approval.CRQ_Staff2HasApproved = true;
			AssertEquals(true, approval.HasAtLeastOneStaffWhoHasApproved);
		}

		#endregion

		#region Save

		public void TestBatchNumberSetOnSaving()
		{
			var approvalRequest = Factory.NewWithValidTestData<AccCommissionApprovalRequest>();
			approvalRequest.CRQ_BatchNumber = "";

			var expectedBatchNumber = Env.NumberFountains.CommissionApprovalRequestBatchNo.PeekPreliminaryFormatted(Factory);
			Factory.Save();
			AssertEquals("Should have been populated", expectedBatchNumber, approvalRequest.CRQ_BatchNumber);

			approvalRequest.CRQ_Staff1HasApproved = true;
			Factory.Save();
			AssertEquals("Should not have changed", expectedBatchNumber, approvalRequest.CRQ_BatchNumber);
		}

		public void TestDetachCommissionLinesFromAllOtherApprovalRequestsOnSaving()
		{
			var lineA = Factory.NewWithValidTestData<AccCommissionLine>();
			var lineB = Factory.NewWithValidTestData<AccCommissionLine>();
			var initialRequest = Factory.NewWithValidTestData<AccCommissionApprovalRequest>();
			var itemA = initialRequest.Items.AddNew();
			itemA.CRI_CL0 = lineA.PK;
			var itemB = initialRequest.Items.AddNew();
			itemB.CRI_CL0 = lineB.PK;

			Factory.Save();

			CombineAssertions(() =>
			{
				AssertEquals("lineA", false, lineA.IsDeleted);
				AssertEquals("lineB", false, lineB.IsDeleted);
				AssertEquals("itemA", false, itemA.IsDeleted);
				AssertEquals("itemB", false, itemB.IsDeleted);
			});

			var secondRequest = Factory.NewWithValidTestData<AccCommissionApprovalRequest>();
			var secondRequestItemA = secondRequest.Items.AddNew();
			secondRequestItemA.CRI_CL0 = lineA.PK;

			Factory.Save();

			CombineAssertions(() =>
			{
				AssertEquals("lineA", false, lineA.IsDeleted);
				AssertEquals("lineB", false, lineB.IsDeleted);
				AssertEquals("itemA", true, itemA.IsDeleted);
				AssertEquals("itemB", false, itemB.IsDeleted);
				AssertEquals("secondRequestItemA", false, secondRequestItemA.IsDeleted);
			});
		}

		public void TestDetachCommissionLinesFromAllOtherApprovalRequests_ShouldUseTableValuedParameters()
		{
			var lineA = Factory.NewWithValidTestData<AccCommissionLine>();
			var lineB = Factory.NewWithValidTestData<AccCommissionLine>();
			var initialRequest = Factory.NewWithValidTestData<AccCommissionApprovalRequest>();
			var itemA = initialRequest.Items.AddNew();
			itemA.CRI_CL0 = lineA.PK;
			var itemB = initialRequest.Items.AddNew();
			itemB.CRI_CL0 = lineB.PK;

			using (Db.Connection.TrackExecutedCommands())
			{
				using (var settings = TestEntityFrameworkSettings.Get())
				{
					settings.TVPRule = new TVPRule("0");
					Factory.Save();

					var executedCommand = Db.Connection.ExecutedCommands.FirstOrDefault(c => c.StartsWith("SELECT \r\nCRI_PK") && c.Contains("FROM dbo.AccCommissionApprovalRequestItem"));
					AssertNotNull(executedCommand);
					AssertContains("Should use TVPs", "(CRI_CL0 in (SELECT Value FROM", executedCommand);
					Assert("Should not use explicit values", !Regex.IsMatch(executedCommand, @"\(CRI_CL0 in \((@(.*?),)*?@(.*?)\)"));
				}
			}
		}

		public void TestRefreshApproveStatus1DescriptionInfoOnSaved()
		{
			var request = Factory.New<AccCommissionApprovalRequest>();
			request.CRQ_GS_NKApprovingStaff1 = "ADL";
			Factory.Save();

			var propertyInfoRefreshed = false;
			request.ApproveStatus1DescriptionInfo.ValueChanged += (sender, e) =>
				{
					propertyInfoRefreshed = true;
				};

			request.ApproveStaff1();
			Factory.Save();

			Assert(propertyInfoRefreshed);
		}

		public void TestRefreshApproveStatus2DescriptionInfoOnSaved()
		{
			var request = Factory.New<AccCommissionApprovalRequest>();
			request.CRQ_GS_NKApprovingStaff2 = "ADL";
			Factory.Save();

			var propertyInfoRefreshed = false;
			request.ApproveStatus2DescriptionInfo.ValueChanged += (sender, e) =>
			{
				propertyInfoRefreshed = true;
			};

			request.ApproveStaff2();
			Factory.Save();

			Assert(propertyInfoRefreshed);
		}

		public void TestShowRowErrorsWhenApprovalRequestDoesntHaveAmendedTransaction_Job()
		{
			var request = Factory.NewWithValidTestData<AccCommissionApprovalRequest>();

			var jobHeader = Factory.NewJobForTesting<JobHeader>();
			jobHeader.JH_JobNum = "S0001005";

			var accHeaderInvoice = Factory.NewWithValidTestData<AccTransactionHeader>();
			accHeaderInvoice.AH_Ledger = LedgerTypes.AccountsReceivable;
			accHeaderInvoice.AH_TransactionType = TransactionTypes.Invoice;

			accHeaderInvoice.AH_GB = GlbBranch.CurrentBranch.PK;
			accHeaderInvoice.AH_GE = GlbDepartment.CurrentDepartment.PK;
			accHeaderInvoice.AH_JH = jobHeader.PK;
			accHeaderInvoice.AH_RX_NKTransactionCurrency = Core.Constants.CurrencyCodes.Australia;
			accHeaderInvoice.AH_PostDate = ZDateTime.Today.AddDays(-1);
			accHeaderInvoice.AH_Desc = "Invoice Transaction";

			var accHeaderInvoice2 = Factory.NewWithValidTestData<AccTransactionHeader>();
			accHeaderInvoice2.AH_Ledger = LedgerTypes.AccountsReceivable;
			accHeaderInvoice2.AH_TransactionType = TransactionTypes.Invoice;

			accHeaderInvoice2.AH_GB = GlbBranch.CurrentBranch.PK;
			accHeaderInvoice2.AH_GE = GlbDepartment.CurrentDepartment.PK;
			accHeaderInvoice2.AH_JH = jobHeader.PK;
			accHeaderInvoice2.AH_RX_NKTransactionCurrency = Core.Constants.CurrencyCodes.Australia;
			accHeaderInvoice2.AH_PostDate = ZDateTime.Today.AddDays(-1);
			accHeaderInvoice2.AH_Desc = "Invoice Transaction2";

			var accHeaderCreditNote = Factory.NewWithValidTestData<AccTransactionHeader>();
			accHeaderCreditNote.AH_Ledger = LedgerTypes.AccountsReceivable;
			accHeaderCreditNote.AH_TransactionType = TransactionTypes.CreditNote;

			accHeaderCreditNote.AH_GB = GlbBranch.CurrentBranch.PK;
			accHeaderCreditNote.AH_GE = GlbDepartment.CurrentDepartment.PK;
			accHeaderCreditNote.AH_JH = jobHeader.PK;
			accHeaderCreditNote.AH_RX_NKTransactionCurrency = Core.Constants.CurrencyCodes.Australia;
			accHeaderCreditNote.AH_PostDate = ZDateTime.Today;
			accHeaderCreditNote.AH_Desc = "Credit Note Transaction";
			accHeaderCreditNote.AH_TransactionBelongsToGroup = accHeaderInvoice.PK;

			var commissionHeaderJobInvoice = Factory.NewWithValidTestData<AccCommissionHeader>();
			commissionHeaderJobInvoice.CH0_GroupingSourceTableCode = jobHeader.TablePrefix;
			commissionHeaderJobInvoice.CH0_GroupingSourceID = jobHeader.PK;
			commissionHeaderJobInvoice.CH0_AH_Source = accHeaderInvoice.PK;
			commissionHeaderJobInvoice.CH0_JobNumber = jobHeader.JH_JobNum;
			commissionHeaderJobInvoice.CH0_GC = GlbCompany.CurrentCompany.PK;

			var invoiceJobLine = Factory.NewWithValidTestData<AccCommissionLine>();
			invoiceJobLine.CL0_ParentID = commissionHeaderJobInvoice.PK;
			invoiceJobLine.CL0_ParentTableCode = AccCommissionHeaderSchema.Constants.Prefix;

			var commissionHeaderJobInvoice2 = Factory.NewWithValidTestData<AccCommissionHeader>();
			commissionHeaderJobInvoice2.CH0_GroupingSourceTableCode = jobHeader.TablePrefix;
			commissionHeaderJobInvoice2.CH0_GroupingSourceID = jobHeader.PK;
			commissionHeaderJobInvoice2.CH0_AH_Source = accHeaderInvoice2.PK;
			commissionHeaderJobInvoice2.CH0_JobNumber = jobHeader.JH_JobNum;
			commissionHeaderJobInvoice2.CH0_GC = GlbCompany.CurrentCompany.PK;

			var invoiceJobLine2 = Factory.NewWithValidTestData<AccCommissionLine>();
			invoiceJobLine2.CL0_ParentID = commissionHeaderJobInvoice2.PK;
			invoiceJobLine2.CL0_ParentTableCode = AccCommissionHeaderSchema.Constants.Prefix;

			var commissionHeaderJobCreditNote = Factory.NewWithValidTestData<AccCommissionHeader>();
			commissionHeaderJobCreditNote.CH0_GroupingSourceTableCode = jobHeader.TablePrefix;
			commissionHeaderJobCreditNote.CH0_GroupingSourceID = jobHeader.PK;
			commissionHeaderJobCreditNote.CH0_AH_Source = accHeaderCreditNote.PK;
			commissionHeaderJobCreditNote.CH0_JobNumber = jobHeader.JH_JobNum;
			commissionHeaderJobCreditNote.CH0_GC = GlbCompany.CurrentCompany.PK;

			var creditNoteJobLine = Factory.NewWithValidTestData<AccCommissionLine>();
			creditNoteJobLine.CL0_ParentID = commissionHeaderJobCreditNote.PK;
			creditNoteJobLine.CL0_ParentTableCode = AccCommissionHeaderSchema.Constants.Prefix;

			var item1 = request.Items.AddNew();
			item1.CRI_CL0 = invoiceJobLine.PK;

			var item2 = request.Items.AddNew();
			item2.CRI_CL0 = invoiceJobLine2.PK;

			Factory.Save();

			AssertEquals("Amendment Made", true, request.AmendmentMadeToTransactionAfterApprovalRequest());
			AssertRowErrorOnCorrectLeaf(request, $"{GlbCompany.CurrentCompany.PK}-{jobHeader.JH_JobNum}");
		}

		public void TestShowRowErrorsWhenApprovalRequestDoesntHaveAmendedTransaction_Transaction()
		{
			var request = Factory.NewWithValidTestData<AccCommissionApprovalRequest>();

			var accHeaderInvoice = Factory.NewWithValidTestData<AccTransactionHeader>();
			accHeaderInvoice.AH_Ledger = LedgerTypes.AccountsReceivable;
			accHeaderInvoice.AH_TransactionType = TransactionTypes.Invoice;

			accHeaderInvoice.AH_GB = GlbBranch.CurrentBranch.PK;
			accHeaderInvoice.AH_GE = GlbDepartment.CurrentDepartment.PK;
			accHeaderInvoice.AH_RX_NKTransactionCurrency = Core.Constants.CurrencyCodes.Australia;
			accHeaderInvoice.AH_PostDate = ZDateTime.Today.AddDays(-1);
			accHeaderInvoice.AH_Desc = "Invoice Transaction";

			var accHeaderInvoice2 = Factory.NewWithValidTestData<AccTransactionHeader>();
			accHeaderInvoice2.AH_Ledger = LedgerTypes.AccountsReceivable;
			accHeaderInvoice2.AH_TransactionType = TransactionTypes.Invoice;

			accHeaderInvoice2.AH_GB = GlbBranch.CurrentBranch.PK;
			accHeaderInvoice2.AH_GE = GlbDepartment.CurrentDepartment.PK;
			accHeaderInvoice2.AH_RX_NKTransactionCurrency = Core.Constants.CurrencyCodes.Australia;
			accHeaderInvoice2.AH_PostDate = ZDateTime.Today.AddDays(-1);
			accHeaderInvoice2.AH_Desc = "Invoice Transaction2";

			var accHeaderCreditNote = Factory.NewWithValidTestData<AccTransactionHeader>();
			accHeaderCreditNote.AH_Ledger = LedgerTypes.AccountsReceivable;
			accHeaderCreditNote.AH_TransactionType = TransactionTypes.CreditNote;

			accHeaderCreditNote.AH_GB = GlbBranch.CurrentBranch.PK;
			accHeaderCreditNote.AH_GE = GlbDepartment.CurrentDepartment.PK;
			accHeaderCreditNote.AH_RX_NKTransactionCurrency = Core.Constants.CurrencyCodes.Australia;
			accHeaderCreditNote.AH_PostDate = ZDateTime.Today;
			accHeaderCreditNote.AH_Desc = "Credit Note Transaction";
			accHeaderCreditNote.AH_TransactionBelongsToGroup = accHeaderInvoice.PK;

			var commissionHeaderJobInvoice = Factory.NewWithValidTestData<AccCommissionHeader>();
			commissionHeaderJobInvoice.CH0_GroupingSourceTableCode = accHeaderInvoice.TablePrefix;
			commissionHeaderJobInvoice.CH0_GroupingSourceID = accHeaderInvoice.PK;
			commissionHeaderJobInvoice.CH0_AH_Source = accHeaderInvoice.PK;

			var invoiceJobLine = Factory.NewWithValidTestData<AccCommissionLine>();
			invoiceJobLine.CL0_ParentID = commissionHeaderJobInvoice.PK;
			invoiceJobLine.CL0_ParentTableCode = AccCommissionHeaderSchema.Constants.Prefix;

			var commissionHeaderJobInvoice2 = Factory.NewWithValidTestData<AccCommissionHeader>();
			commissionHeaderJobInvoice2.CH0_GroupingSourceTableCode = accHeaderInvoice2.TablePrefix;
			commissionHeaderJobInvoice2.CH0_GroupingSourceID = accHeaderInvoice2.PK;
			commissionHeaderJobInvoice2.CH0_AH_Source = accHeaderInvoice2.PK;

			var invoiceJobLine2 = Factory.NewWithValidTestData<AccCommissionLine>();
			invoiceJobLine2.CL0_ParentID = commissionHeaderJobInvoice2.PK;
			invoiceJobLine2.CL0_ParentTableCode = AccCommissionHeaderSchema.Constants.Prefix;

			var commissionHeaderJobCreditNote = Factory.NewWithValidTestData<AccCommissionHeader>();
			commissionHeaderJobCreditNote.CH0_GroupingSourceTableCode = accHeaderCreditNote.TablePrefix;
			commissionHeaderJobCreditNote.CH0_GroupingSourceID = accHeaderCreditNote.PK;
			commissionHeaderJobCreditNote.CH0_AH_Source = accHeaderCreditNote.PK;

			var creditNoteJobLine = Factory.NewWithValidTestData<AccCommissionLine>();
			creditNoteJobLine.CL0_ParentID = commissionHeaderJobCreditNote.PK;
			creditNoteJobLine.CL0_ParentTableCode = AccCommissionHeaderSchema.Constants.Prefix;

			var item1 = request.Items.AddNew();
			item1.CRI_CL0 = invoiceJobLine.PK;

			var item2 = request.Items.AddNew();
			item2.CRI_CL0 = invoiceJobLine2.PK;

			Factory.Save();

			AssertEquals("Amendment Made", true, request.AmendmentMadeToTransactionAfterApprovalRequest());
			AssertRowErrorOnCorrectLeaf(request, $"{accHeaderInvoice.PK}");
		}

		public void TestShowRowErrorsWhenApprovalRequestDoesntHaveAmendedTransaction_ArchivedJob()
		{
			var request = Factory.NewWithValidTestData<AccCommissionApprovalRequest>();

			var jobHeader = Factory.NewJobForTesting<JobHeader>();
			jobHeader.JH_JobNum = "S0001005";

			var accHeaderInvoice = Factory.NewWithValidTestData<AccTransactionHeader>();
			accHeaderInvoice.AH_Ledger = LedgerTypes.AccountsReceivable;
			accHeaderInvoice.AH_TransactionType = TransactionTypes.Invoice;

			var accHeaderInvoice2 = Factory.NewWithValidTestData<AccTransactionHeader>();
			accHeaderInvoice2.AH_Ledger = LedgerTypes.AccountsReceivable;
			accHeaderInvoice2.AH_TransactionType = TransactionTypes.Invoice;

			accHeaderInvoice2.AH_GB = GlbBranch.CurrentBranch.PK;
			accHeaderInvoice2.AH_GE = GlbDepartment.CurrentDepartment.PK;
			accHeaderInvoice2.AH_JH = jobHeader.PK;
			accHeaderInvoice2.AH_RX_NKTransactionCurrency = Core.Constants.CurrencyCodes.Australia;
			accHeaderInvoice2.AH_PostDate = ZDateTime.Today.AddDays(-1);
			accHeaderInvoice2.AH_Desc = "Invoice Transaction2";

			var accHeaderCreditNote = Factory.NewWithValidTestData<AccTransactionHeader>();
			accHeaderCreditNote.AH_Ledger = LedgerTypes.AccountsReceivable;
			accHeaderCreditNote.AH_TransactionType = TransactionTypes.CreditNote;

			accHeaderCreditNote.AH_GB = GlbBranch.CurrentBranch.PK;
			accHeaderCreditNote.AH_GE = GlbDepartment.CurrentDepartment.PK;
			accHeaderCreditNote.AH_JH = jobHeader.PK;
			accHeaderCreditNote.AH_RX_NKTransactionCurrency = Core.Constants.CurrencyCodes.Australia;
			accHeaderCreditNote.AH_PostDate = ZDateTime.Today;
			accHeaderCreditNote.AH_Desc = "Credit Note Transaction";
			accHeaderCreditNote.AH_TransactionBelongsToGroup = accHeaderInvoice.PK;

			var commissionHeaderJobInvoice = Factory.NewWithValidTestData<AccCommissionHeader>();
			commissionHeaderJobInvoice.CH0_GroupingSourceTableCode = jobHeader.TablePrefix;
			commissionHeaderJobInvoice.CH0_AH_Source = accHeaderInvoice.PK;
			commissionHeaderJobInvoice.CH0_JobNumber = jobHeader.JH_JobNum;
			commissionHeaderJobInvoice.CH0_GC = GlbCompany.CurrentCompany.PK;

			var invoiceJobLine = Factory.NewWithValidTestData<AccCommissionLine>();
			invoiceJobLine.CL0_ParentID = commissionHeaderJobInvoice.PK;
			invoiceJobLine.CL0_ParentTableCode = AccCommissionHeaderSchema.Constants.Prefix;

			var commissionHeaderJobInvoice2 = Factory.NewWithValidTestData<AccCommissionHeader>();
			commissionHeaderJobInvoice2.CH0_GroupingSourceTableCode = jobHeader.TablePrefix;
			commissionHeaderJobInvoice2.CH0_AH_Source = accHeaderInvoice2.PK;
			commissionHeaderJobInvoice2.CH0_JobNumber = jobHeader.JH_JobNum;
			commissionHeaderJobInvoice2.CH0_GC = GlbCompany.CurrentCompany.PK;

			var invoiceJobLine2 = Factory.NewWithValidTestData<AccCommissionLine>();
			invoiceJobLine2.CL0_ParentID = commissionHeaderJobInvoice2.PK;
			invoiceJobLine2.CL0_ParentTableCode = AccCommissionHeaderSchema.Constants.Prefix;

			var commissionHeaderJobCreditNote = Factory.NewWithValidTestData<AccCommissionHeader>();
			commissionHeaderJobCreditNote.CH0_GroupingSourceTableCode = jobHeader.TablePrefix;
			commissionHeaderJobCreditNote.CH0_AH_Source = accHeaderCreditNote.PK;
			commissionHeaderJobCreditNote.CH0_JobNumber = jobHeader.JH_JobNum;
			commissionHeaderJobCreditNote.CH0_GC = GlbCompany.CurrentCompany.PK;

			var creditNoteJobLine = Factory.NewWithValidTestData<AccCommissionLine>();
			creditNoteJobLine.CL0_ParentID = commissionHeaderJobCreditNote.PK;
			creditNoteJobLine.CL0_ParentTableCode = AccCommissionHeaderSchema.Constants.Prefix;

			var item1 = request.Items.AddNew();
			item1.CRI_CL0 = invoiceJobLine.PK;

			var item2 = request.Items.AddNew();
			item2.CRI_CL0 = invoiceJobLine2.PK;

			Factory.Save();

			AssertEquals("Amendment Made", true, request.AmendmentMadeToTransactionAfterApprovalRequest());
			AssertRowErrorOnCorrectLeaf(request, $"{GlbCompany.CurrentCompany.PK}-{jobHeader.JH_JobNum}");
		}

		void AssertRowErrorOnCorrectLeaf(AccCommissionApprovalRequest request, ZString sourceUniqueId)
		{
			void AssertRowErrorOnCorrectLeaf(CommissionApprovalRequestItemGrouping grouping)
			{
				if (grouping.IsLeaf)
				{
					AssertNotEquals("Group should have a source", ZString.Empty, grouping.SourceUniqueId);

					var label = $"Table Code: {grouping.SourceTableCode}, Id: {grouping.SourceUniqueId}";
					if (sourceUniqueId == grouping.SourceUniqueId)
					{
						AssertHasRowError(label, grouping, "Amendment done since approval request.");
					}
					else
					{
						AssertNoRowErrors(label, grouping);
					}
				}
				else
				{
					foreach (var subGrouping in grouping.SubGroupings)
					{
						AssertRowErrorOnCorrectLeaf(subGrouping);
					}
				}
			}

			foreach (var grouping in request.CommissionApprovalRequestItemGroupingCollection.Cast<CommissionApprovalRequestItemGrouping>())
			{
				AssertRowErrorOnCorrectLeaf(grouping);
			}
		}
	}

	#endregion
}
