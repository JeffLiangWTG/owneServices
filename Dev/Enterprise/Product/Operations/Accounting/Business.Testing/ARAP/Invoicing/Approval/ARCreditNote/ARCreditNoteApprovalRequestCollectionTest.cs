using Enterprise.Accounting.Business.TransactionApproval.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.ARAP.Invoicing.Testing
{
	[TestedType(typeof(ARCreditNoteApprovalRequestCollection))]
	public class ARCreditNoteApprovalRequestCollectionTest : TransactionApprovalRequestCollectionTest<ARCreditNoteApprovalRequestCollection, ARCreditNoteApprovalRequest>
	{
		public override void TestRelationshipFilter()
		{
			var approvalRequest1 = CreateARCreditNoteApprovalRequest(false);
			var approvalRequest2 = CreateARCreditNoteApprovalRequest(false);
			var approvalRequest3 = CreateARCreditNoteApprovalRequest(true);
			Factory.Save();

			var collection = GetCollectionToTest();
			AssertEquals(3, collection.Count);
			AssertNotNull(collection.FindByPK(approvalRequest1.PK));
			AssertNotNull(collection.FindByPK(approvalRequest2.PK));
			AssertNotNull(collection.FindByPK(approvalRequest3.PK));

			approvalRequest1.XP_SubSystem = "";
			approvalRequest1.XP_ApprovalType = "";
			approvalRequest2.XP_SubSystem = "XXB";
			approvalRequest3.XP_ApprovalType = "BBB";
			var approvalRequest4 = CreateARCreditNoteApprovalRequest(false);
			var approvalRequest5 = CreateARCreditNoteApprovalRequest(true);
			Factory.Save();

			AssertEquals(2, collection.Count);
			AssertNotNull(collection.FindByPK(approvalRequest4.PK));
			AssertNotNull(collection.FindByPK(approvalRequest5.PK));
		}

		ARCreditNoteApprovalRequest CreateARCreditNoteApprovalRequest(bool isForReversal)
		{
			var request = (ARCreditNoteApprovalRequest)base.GetNewElementToAddToTheCollection();
			if (isForReversal)
			{
				request.ChangeApprovalTypeForInvoiceReversal();
			}
			return request;
		}
	}
}
