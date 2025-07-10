using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using AccountingBusiness = Enterprise.Accounting.Business;

namespace Enterprise.Accounting.Business.Testing
{
	[TestedType(typeof(CreditControlledDocumentsApprovalCollection))]
	public class CreditControlledDocumentsApprovalCollectionTest : ActiveBusinessObjectCollectionTestCase<CreditControlledDocumentsApprovalCollection>
	{
		public void TestRelationshipFilterToLoadOnlyCurrentCompanyRecords()
		{
			var menutItem = Factory.New<StmMenuItem>();
			menutItem.SU_MenuPath = "menu/path/newbizo";
			menutItem.SU_MenuName = "name";
			var businessObject = Factory.NewJobWithValidTestDataForTesting<JobHeader>();

			var branchForNonCurentCompany = Factory.LoadTop1<GlbBranch>(new ZQuery(GlbBranchSchema.GB_GC, SQLComparisonOperator.NotEqual, GlbCompany.CurrentCompany.PK));
			var nonCurrentBranchFilter = new ZQuery(GlbBranchSchema.GB_GC, GlbCompany.CurrentCompany.PK);
			nonCurrentBranchFilter.AddToFilter(GlbBranchSchema.PK, SQLComparisonOperator.NotEqual, GlbBranch.CurrentBranch.PK);
			var nonCurrentBranchForCurentCompany = Factory.LoadTop1<GlbBranch>(nonCurrentBranchFilter);
			var approvalRequest1 = Factory.NewWithValidTestData<AccountingBusiness.CreditControlledDocumentsApproval>();
			approvalRequest1.Initialize(businessObject, menutItem.PK);
			approvalRequest1.XP_GB_RequestingBranch = branchForNonCurentCompany.PK;
			var approvalRequest2 = Factory.NewWithValidTestData<AccountingBusiness.CreditControlledDocumentsApproval>();
			approvalRequest2.Initialize(businessObject, menutItem.PK);
			approvalRequest2.XP_GB_RequestingBranch = GlbBranch.CurrentBranch.PK;
			var approvalRequest3 = Factory.NewWithValidTestData<AccountingBusiness.CreditControlledDocumentsApproval>();
			approvalRequest3.Initialize(businessObject, menutItem.PK);
			approvalRequest3.XP_GB_RequestingBranch = nonCurrentBranchForCurentCompany.PK;
			Factory.Save();

			var collection = GetCollectionToTest();
			AssertEquals(2, collection.Count);
			AssertCollectionContains(approvalRequest2, collection);
			AssertCollectionContains(approvalRequest3, collection);
		}

		public void TestRelationshipFilter()
		{
			var menutItem = Factory.New<StmMenuItem>();
			menutItem.SU_MenuPath = "menu/path/newbizo";
			menutItem.SU_MenuName = "name";
			var businessObject = Factory.NewJobWithValidTestDataForTesting<JobHeader>();

			var approvalRequest1 = Factory.NewWithValidTestData<AccountingBusiness.CreditControlledDocumentsApproval>();
			approvalRequest1.Initialize(businessObject, menutItem.PK);
			var approvalRequest2 = Factory.NewWithValidTestData<AccountingBusiness.CreditControlledDocumentsApproval>();
			approvalRequest2.Initialize(businessObject, menutItem.PK);
			var approvalRequest3 = Factory.NewWithValidTestData<AccountingBusiness.CreditControlledDocumentsApproval>();
			approvalRequest3.Initialize(businessObject, menutItem.PK);
			Factory.Save();

			var collection = GetCollectionToTest();
			AssertEquals(3, collection.Count);
			AssertCollectionContains(approvalRequest1, collection);
			AssertCollectionContains(approvalRequest2, collection);
			AssertCollectionContains(approvalRequest3, collection);

			approvalRequest1.XP_SubSystem = "";
			approvalRequest1.XP_ApprovalType = "";
			approvalRequest2.XP_SubSystem = "XXB";
			approvalRequest3.XP_ApprovalType = "BBB";
			var approvalRequest4 = Factory.NewWithValidTestData<AccountingBusiness.CreditControlledDocumentsApproval>();
			approvalRequest4.Initialize(businessObject, menutItem.PK);
			Factory.Save();
			AssertEquals(1, collection.Count);
			AssertCollectionContains(approvalRequest4, collection);
		}
	}
}
