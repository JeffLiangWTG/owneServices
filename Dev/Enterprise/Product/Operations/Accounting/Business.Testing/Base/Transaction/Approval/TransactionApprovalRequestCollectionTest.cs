using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Business.TransactionApproval.Testing
{
	public abstract class TransactionApprovalRequestCollectionTest<CollectionType, RequestType> : ActiveBusinessObjectCollectionTestCase<CollectionType>
			where CollectionType : TransactionApprovalRequestCollection<RequestType>
			where RequestType : GenApprovalRequest
	{
		public void TestRelationshipFilterToLoadOnlyCurrentCompanyRecords()
		{
			GlbBranch branchForNonCurentCompany = Factory.LoadTop1<GlbBranch>(new ZQuery(GlbBranchSchema.GB_GC, SQLComparisonOperator.NotEqual, GlbCompany.CurrentCompany.PK));
			ZQuery nonCurrentBranchFilter = new ZQuery(GlbBranchSchema.GB_GC, GlbCompany.CurrentCompany.PK);
			nonCurrentBranchFilter.AddToFilter(GlbBranchSchema.PK, SQLComparisonOperator.NotEqual, GlbBranch.CurrentBranch.PK);
			GlbBranch nonCurrentBranchForCurentCompany = Factory.LoadTop1<GlbBranch>(nonCurrentBranchFilter);
			var approvalRequest1 = GetNewElementToAddToTheCollectionCasted();
			approvalRequest1.XP_GB_RequestingBranch = branchForNonCurentCompany.PK;
			var approvalRequest2 = GetNewElementToAddToTheCollectionCasted();
			approvalRequest2.XP_GB_RequestingBranch = GlbBranch.CurrentBranch.PK;
			var approvalRequest3 = GetNewElementToAddToTheCollectionCasted();
			approvalRequest3.XP_GB_RequestingBranch = nonCurrentBranchForCurentCompany.PK;
			Factory.Save();

			var collection = GetCollectionToTest();
			AssertEquals(2, collection.Count);
			AssertCollectionContains(approvalRequest2, collection);
			AssertCollectionContains(approvalRequest3, collection);
		}

		public virtual void TestRelationshipFilter()
		{
			var approvalRequest1 = GetNewElementToAddToTheCollectionCasted();
			var approvalRequest2 = GetNewElementToAddToTheCollectionCasted();
			var approvalRequest3 = GetNewElementToAddToTheCollectionCasted();
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
			var approvalRequest4 = GetNewElementToAddToTheCollection();
			Factory.Save();
			AssertEquals(1, collection.Count);
			AssertCollectionContains(approvalRequest4, collection);
		}

		GenApprovalRequest GetNewElementToAddToTheCollectionCasted()
		{
			return (GenApprovalRequest)GetNewElementToAddToTheCollection();
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			var request = (GenApprovalRequest)base.GetNewElementToAddToTheCollection();
			request.FillWithValidTestData();

			return request;
		}

		protected TestObjectCreator TestObjectCreator
		{
			get { return testObjectCreator ?? (testObjectCreator = new TestObjectCreator(Factory)); }
		}
		TestObjectCreator testObjectCreator;
	}
}
