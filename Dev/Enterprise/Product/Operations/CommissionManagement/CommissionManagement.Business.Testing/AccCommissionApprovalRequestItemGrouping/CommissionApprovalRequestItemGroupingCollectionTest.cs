using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.CommissionManagement.Business.Testing
{
	[TestedType(typeof(CommissionApprovalRequestItemGroupingCollection))]
	internal class CommissionApprovalRequestItemGroupingCollectionTest : NonPersistentBusinessObjectCollectionTestCase<CommissionApprovalRequestItemGroupingCollection>
	{
		#region Implementation

		protected override CommissionApprovalRequestItemGroupingCollection GetCollectionToTest()
		{
			return new CommissionApprovalRequestItemGroupingCollection(Factory);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			var line = Factory.NewWithValidTestData<AccCommissionLine>();
			Factory.Save();

			var requestItem = ApprovalRequest.Items.AddNew();
			requestItem.CRI_CL0 = line.PK;
			var grouping = new CommissionApprovalRequestItemGrouping(Factory);
			grouping.Init(new[] { requestItem });
			return grouping;
		}

		AccCommissionApprovalRequest ApprovalRequest
		{
			get
			{
				if (approvalRequest == null)
				{
					approvalRequest = Factory.NewWithValidTestData<AccCommissionApprovalRequest>();
				}

				return approvalRequest;
			}
		}
		AccCommissionApprovalRequest approvalRequest;

		#endregion
	}
}
