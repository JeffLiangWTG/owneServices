using System;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Accounting.Module.Testing;
using Enterprise.Core;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Accounting.Module.TransactionApproval.Testing
{
	[TestedType(typeof(TransactionApprovalFilterBusinessObject))]
	public class TransactionApprovalFilterBusinessObjectTest : AccountingFilterStripBusinessObjectTestCase
	{
		public virtual void TestReasonCodeFilterList()
		{
			var filterBO = this.GetNewFilterStripBusinessObject();
			var filter = (ModuleTextFilter)filterBO["Reason Code"];

			var list = (CodeDescriptionPairList)filter.List;
			AssertEquals("Count", GenApprovalRequestHelper.GetAllReasonCodeList.Count, list.Count);
			AssertEquals("DAM, DSC, IAM, ICH, IDA, IDE, IJD, IOB, IRA, LDL, TXT, WOR", list.CodesAsString);
		}

		public virtual void TestApprovalStatusFilterList()
		{
			var filterBO = this.GetNewFilterStripBusinessObject();
			var filter = (ModuleTextFilter)filterBO["Approval Status"];

			var list = (ICodeDescriptionPairList)filter.List;
			AssertEquals("Count", GenApprovalRequestLookups.ApprovalStatusCodeDescriptionList.Count - 1, list.Count);
			Assert("Contains code Requested", list.ContainsCode(Constants.GenApprovalRequestApprovalStatus.Requested));
			Assert("Contains code Cancelled", list.ContainsCode(Constants.GenApprovalRequestApprovalStatus.Cancelled));
			Assert("Contains code Rejected", list.ContainsCode(Constants.GenApprovalRequestApprovalStatus.Rejected));
			Assert("Contains code Approved", list.ContainsCode(Constants.GenApprovalRequestApprovalStatus.Approved));
			Assert("Contains code Posted", list.ContainsCode(Constants.GenApprovalRequestApprovalStatus.Posted));
			Assert("Doesn't contains code Error", !list.ContainsCode(Constants.GenApprovalRequestApprovalStatus.Error));
		}

		#region TestJobBranch Department Filter
		public void TestJobBranchFilter()
		{
			var branch1 = Factory.NewWithValidTestData<GlbBranch>();
			branch1.GB_AccountingGroupCode = "BRA";
			branch1.GB_GC = GlbCompany.CurrentCompany.PK;

			var branch2 = Factory.NewWithValidTestData<GlbBranch>();
			branch2.GB_AccountingGroupCode = "TST";
			branch2.GB_GC = TestObjectCreator.NonCurrentCompany.PK;

			var job1 = Factory.NewJobWithValidTestDataForTesting<Job>();
			var approvalRequest1 = Factory.NewWithValidTestData<GenApprovalRequest>();
			approvalRequest1.XP_ParentID = job1.PK;
			approvalRequest1.XP_GB_JobBranch = branch1.PK;

			var job2 = Factory.NewJobWithValidTestDataForTesting<Job>();
			var approvalRequest2 = Factory.NewWithValidTestData<GenApprovalRequest>();
			approvalRequest2.XP_ParentID = job2.PK;
			approvalRequest2.XP_GB_JobBranch = branch2.PK;

			Factory.Save();

			var filterBO = this.GetNewFilterStripBusinessObject();
			var jobBranchGuidFilter = (ModuleGuidFilter)filterBO["Job Branch Code"];
			jobBranchGuidFilter.Property = branch1.PK;
			jobBranchGuidFilter.IsActive = true;

			var collection = new ActiveBusinessObjectCollection<GenApprovalRequest>(Factory);
			collection.AdditionalFilter = filterBO.Filter;

			AssertContainsExactElementsInAnyOrder("Collection should Contain approvalRequest1", new[] { approvalRequest1 }, collection);
			AssertCollectionNotContains("Collection shouldn't contain approvalRequest2", new[] { approvalRequest2 }, collection);
		}

		public void TestJobDepartmentFilter()
		{
			var department = Factory.NewWithValidTestData<GlbDepartment>();
			department.GE_Code = "ABC";

			var department2 = Factory.NewWithValidTestData<GlbDepartment>();
			department2.GE_Code = "EFG";

			var job1 = Factory.NewJobWithValidTestDataForTesting<Job>();
			var approvalRequest1 = Factory.NewWithValidTestData<GenApprovalRequest>();
			approvalRequest1.XP_ParentID = job1.PK;
			approvalRequest1.XP_GE_JobDepartment = department.PK;

			var job2 = Factory.NewJobWithValidTestDataForTesting<Job>();
			var approvalRequest2 = Factory.NewWithValidTestData<GenApprovalRequest>();
			approvalRequest2.XP_ParentID = job2.PK;
			approvalRequest2.XP_GE_JobDepartment = department2.PK;

			Factory.Save();

			var filterBO = this.GetNewFilterStripBusinessObject();
			var jobBranchGuidFilter = (ModuleGuidFilter)filterBO["Job Department Code"];
			jobBranchGuidFilter.Property = department.PK;
			jobBranchGuidFilter.IsActive = true;

			var collection = new ActiveBusinessObjectCollection<GenApprovalRequest>(Factory);
			collection.AdditionalFilter = filterBO.Filter;

			AssertContainsExactElementsInAnyOrder("Collection should Contain approvalRequest1", new[] { approvalRequest1 }, collection);
			AssertCollectionNotContains("Collection shouldn't contain approvalRequest2", new[] { approvalRequest2 }, collection);
		}
		#endregion

		#region TestBranchManagementCodeFilter

		public void TestBranchManagementCodeFilter()
		{
			var codeCollection = new BranchManagementCodeDescriptionBoolCollection();
			codeCollection.Add("BRA", null, true);
			codeCollection.Add("BRB", null, true);
			AccountingMasterFilesRegistry.Instance.BranchManagementCodes.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, codeCollection);

			var branch1 = Factory.NewWithValidTestData<GlbBranch>();
			branch1.GB_AccountingGroupCode = "BRA";
			branch1.GB_GC = GlbCompany.CurrentCompany.PK;
			var branch2 = Factory.NewWithValidTestData<GlbBranch>();
			branch2.GB_GC = GlbCompany.CurrentCompany.PK;
			branch2.GB_AccountingGroupCode = "BRB";
			Factory.Save();

			var job1 = Factory.NewJobWithValidTestDataForTesting<Job>();
			var approvalRequest1 = Factory.NewWithValidTestData<GenApprovalRequest>();
			approvalRequest1.XP_ParentID = job1.PK;
			approvalRequest1.XP_GB_RequestingBranch = branch1.PK;

			var job2 = Factory.NewJobWithValidTestDataForTesting<Job>();
			var approvalRequest2 = Factory.NewWithValidTestData<GenApprovalRequest>();
			approvalRequest2.XP_ParentID = job2.PK;
			approvalRequest2.XP_GB_RequestingBranch = branch2.PK;

			Factory.Save();

			var filterBO = this.GetNewFilterStripBusinessObject();
			var branchManagementCodeFilter = (ModuleTextFilter)filterBO["Branch Management Code"];
			branchManagementCodeFilter.Property = "BRA";
			branchManagementCodeFilter.IsActive = true;
			var collection = new ActiveBusinessObjectCollection<GenApprovalRequest>(Factory);
			collection.AdditionalFilter = filterBO.Filter;
			AssertContainsExactElementsInAnyOrder("Collection should Contain approvalRequest1", new[] { approvalRequest1 }, collection);

			branchManagementCodeFilter.Property = "BRB";
			collection.AdditionalFilter = filterBO.Filter;
			AssertContainsExactElementsInAnyOrder("Collection should Contain approvalRequest2", new[] { approvalRequest2 }, collection);
		}

		#endregion

		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject()
		{
			return new TransactionApprovalFilterBusinessObject();
		}
	}
}
