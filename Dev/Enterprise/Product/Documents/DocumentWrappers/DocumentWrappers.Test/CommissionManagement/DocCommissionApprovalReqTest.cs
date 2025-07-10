using Enterprise.CommissionManagement.Business;

namespace Enterprise.DocumentWrappers.Testing
{
	sealed class DocCommissionApprovalReqTest : DocBaseWrapperTest
	{
		#region Properties

		public void TestApprovalRequestPK()
		{
			AssertEquals(ApprovalRequest.PK, DocApprovalRequest.ApprovalRequestPK);
		}

		public void TestBatchNumber()
		{
			ApprovalRequest.CRQ_BatchNumber = "00001001";
			AssertEquals("00001001", DocApprovalRequest.BatchNumber);
		}

		#endregion

		#region EntityTotalGroupings

		public void TestEntityTotalGroupings()
		{
			AssertNotNull(DocApprovalRequest.EntityTotalGroupings);
		}

		#endregion

		#region EntitySummaryGroupings

		public void TestEntitySummaryGroupings()
		{
			AssertNotNull(DocApprovalRequest.EntitySummaryGroupings);
		}

		#endregion

		#region CommissionDetails

		public void TestCommissionDetails()
		{
			AssertNotNull(DocApprovalRequest.CommissionDetails);
		}

		#endregion

		#region Overrides

		protected override DocBaseWrapper GetNewDocumentWrapper()
		{
			var approvalRequest = Factory.New<AccCommissionApprovalRequest>();
			return DocCommissionApprovalReq.New(approvalRequest, Factory);
		}

		#endregion

		#region Implementation

		AccCommissionApprovalRequest ApprovalRequest
		{
			get
			{
				if (approvalRequest == null)
				{
					approvalRequest = Factory.New<AccCommissionApprovalRequest>();
				}

				return approvalRequest;
			}
		}
		AccCommissionApprovalRequest approvalRequest;

		DocCommissionApprovalReq DocApprovalRequest
		{
			get
			{
				if (docApprovalRequest == null)
				{
					docApprovalRequest = DocCommissionApprovalReq.New(ApprovalRequest, Factory);
				}

				return docApprovalRequest;
			}
		}
		DocCommissionApprovalReq docApprovalRequest;

		#endregion
	}
}
