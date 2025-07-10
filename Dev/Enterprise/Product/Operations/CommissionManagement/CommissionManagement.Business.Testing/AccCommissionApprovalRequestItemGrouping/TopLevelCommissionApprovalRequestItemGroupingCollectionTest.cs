using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Registry.Business;
using NUnit.Framework;

namespace Enterprise.CommissionManagement.Business.Testing
{
	[TestedType(typeof(TopLevelCommissionApprovalRequestItemGroupingCollection))]
	internal class TopLevelCommissionApprovalRequestItemGroupingCollectionTest : NonPersistentBusinessObjectCollectionTestCase<TopLevelCommissionApprovalRequestItemGroupingCollection>
	{
		public void TestCommissionApprovalRequestItemGroupingCollection_ShowForAllCompany()
		{
			OrganisationsDataRegistry.Instance.OnlyShowCommissionsForCurrentLoginCompany.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);

			var request = Factory.New<AccCommissionApprovalRequest>();
			var innerCollection = new AccCommissionApprovalRequestItemCollection(request);
			var collection = new TopLevelCommissionApprovalRequestItemGroupingCollection(innerCollection);
			collection.Init();

			AssertContainsExactElementsInAnyOrder(
				new[]
				{
					typeof(ViewCommissionLineRecipientAndPreferredCompanyGrouper<AccCommissionApprovalRequestItem>),
					typeof(ViewCommissionLineLocalCompanyGrouper<AccCommissionApprovalRequestItem>),
					typeof(ViewCommissionLineSourceAndStatusAndApprovalRequestGrouper<AccCommissionApprovalRequestItem>)
				},
				collection.Groupers.Select(x => x.GetType()));
		}

		public void TestCommissionApprovalRequestItemGroupingCollection_ShowForCurrentLoginCompany()
		{
			OrganisationsDataRegistry.Instance.OnlyShowCommissionsForCurrentLoginCompany.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			var request = Factory.New<AccCommissionApprovalRequest>();
			var innerCollection = new AccCommissionApprovalRequestItemCollection(request);
			var collection = new TopLevelCommissionApprovalRequestItemGroupingCollection(innerCollection);
			collection.Init();

			AssertContainsExactElementsInAnyOrder(
				new[]
				{
					typeof(ViewCommissionLineRecipientAndPreferredCompanyGrouper<AccCommissionApprovalRequestItem>),
					typeof(ViewCommissionLineSourceAndStatusAndApprovalRequestGrouper<AccCommissionApprovalRequestItem>)
				},
				collection.Groupers.Select(x => x.GetType()));
		}

		#region Implementation

		protected override TopLevelCommissionApprovalRequestItemGroupingCollection GetCollectionToTest()
		{
			var lineItemCollection = new AccCommissionApprovalRequestItemCollection(Factory);
			return new TopLevelCommissionApprovalRequestItemGroupingCollection(lineItemCollection);
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
