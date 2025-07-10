using Enterprise.CommissionManagement.Business;

namespace Enterprise.DocumentWrappers.Testing
{
	sealed class DocCommissionPaymentTest : DocBaseWrapperTest
	{
		#region Properties

		public void TestBatchNumber()
		{
			AssertEquals("", DocCommissionPayment.BatchNumber);
		}

		#endregion

		#region EntityTotalGroupings

		public void TestEntityTotalGroupings()
		{
			AssertNotNull(DocCommissionPayment.EntityTotalGroupings);
		}

		#endregion

		#region EntitySummaryGroupings

		public void TestEntitySummaryGroupings()
		{
			AssertNotNull(DocCommissionPayment.EntitySummaryGroupings);
		}

		#endregion

		#region CommissionDetails

		public void TestCommissionDetails()
		{
			AssertNotNull(DocCommissionPayment.CommissionDetails);
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

		CommissionPayment CommissionPayment
		{
			get
			{
				if (commissionPayment == null)
				{
					var commissionLine = Factory.NewWithValidTestData<AccCommissionLine>();
					Factory.Save();

					var commissionLineView = Factory.Load<ViewCommissionLine>(commissionLine.PK);
					commissionPayment = new CommissionPayment(Factory, new[] { commissionLineView });
				}

				return commissionPayment;
			}
		}
		CommissionPayment commissionPayment;

		DocCommissionPayment DocCommissionPayment
		{
			get
			{
				if (docCommissionPayment == null)
				{
					docCommissionPayment = DocCommissionPayment.New(CommissionPayment, Factory);
				}

				return docCommissionPayment;
			}
		}
		DocCommissionPayment docCommissionPayment;

		#endregion
	}
}
