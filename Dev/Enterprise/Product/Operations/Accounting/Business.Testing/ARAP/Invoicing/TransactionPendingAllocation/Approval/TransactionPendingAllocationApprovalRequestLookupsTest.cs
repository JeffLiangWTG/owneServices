using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Core;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Accounting.Business.Testing.ARAP.Invoicing.Testing
{
	public class TransactionPendingAllocationApprovalRequestLookupsTest : GenApprovalRequestLookupsTest
	{
		public override void TestApprovalStatusList()
		{
			var expectedList = ExpectedList;
			AssertApprovalStatusList(expectedList);
		}

		public void TestApprovalStatusList_WhenITransactionPendingAllocationApprovalEInvoicingRequestsFunctionalityProviderImplemented()
		{
			var expectedList = ExpectedList;
			AddElectronicRequestPairs(expectedList);

			TestObjectCreator.MockCountryFactoryForTPAAeInvoicing();
			AssertApprovalStatusList(expectedList);
		}

		void AssertApprovalStatusList(CodeDescriptionPairList expectedList)
		{
			var approvalRequest = Factory.New<TransactionPendingAllocationApprovalRequest>();
			var testLookups = new TransactionPendingAllocationApprovalRequestLookups(approvalRequest);
			AssertContainsExactElementsInAnyOrder(expectedList, testLookups.ApprovalStatusList);
		}

		public void TestTransactionPendingAllocationApprovalStatusCodeDescriptionList()
		{
			var expectedList = ExpectedList;
			AssertTransactionPendingAllocationApprovalStatusCodeDescriptionList(expectedList);
		}

		public void TestTransactionPendingAllocationApprovalStatusCodeDescriptionList_WhenITransactionPendingAllocationApprovalEInvoicingRequestsFunctionalityProviderImplemented()
		{
			var expectedList = ExpectedList;
			AddElectronicRequestPairs(expectedList);

			TestObjectCreator.MockCountryFactoryForTPAAeInvoicing();
			AssertTransactionPendingAllocationApprovalStatusCodeDescriptionList(expectedList);
		}

		void AssertTransactionPendingAllocationApprovalStatusCodeDescriptionList(CodeDescriptionPairList expectedList)
		{
			var list = TransactionPendingAllocationApprovalRequestLookups.TransactionPendingAllocationApprovalStatusCodeDescriptionList(Factory, Env.CurrentCompany.Country.Code);
			AssertContainsExactElementsInAnyOrder(expectedList, list);
		}

		public static CodeDescriptionPairList ExpectedList
		{
			get
			{
				var list = new CodeDescriptionPairList();
				list.AddPair(Constants.GenApprovalRequestApprovalStatus.Requested, "Requested");
				list.AddPair(Constants.GenApprovalRequestApprovalStatus.Cancelled, "Canceled");
				list.AddPair(Constants.GenApprovalRequestApprovalStatus.Rejected, "Rejected");
				list.AddPair(Constants.GenApprovalRequestApprovalStatus.Approved, "Approved");
				list.AddPair(Constants.GenApprovalRequestApprovalStatus.Posted, "Posted");
				list.AddPair(Constants.GenApprovalRequestApprovalStatus.Error, "Validation Errors");

				return list;
			}
		}

		void AddElectronicRequestPairs(CodeDescriptionPairList list)
		{
			list.AddPair(Constants.GenApprovalRequestApprovalStatus.ApprovalRequested, "Approval Requested");
			list.AddPair(Constants.GenApprovalRequestApprovalStatus.RejectionRequested, "Rejection Requested");
		}
	}
}
