using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Registry.Business;
using NUnit.Framework;

namespace Enterprise.CommissionManagement.Business.Testing
{
	[TestedType(typeof(TopLevelCommissionFinalizerLineItemGroupingCollection))]
	internal class TopLevelCommissionFinalizerLineItemGroupingCollectionTest : NonPersistentBusinessObjectCollectionTestCase<TopLevelCommissionFinalizerLineItemGroupingCollection>
	{
		public void TestGroupers_ShowForAllCompany()
		{
			OrganisationsDataRegistry.Instance.OnlyShowCommissionsForCurrentLoginCompany.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);

			var lineItemCollection = new CommissionFinalizerLineItemCollection(Factory);
			var collection = new TopLevelCommissionFinalizerLineItemGroupingCollection(lineItemCollection);
			collection.Init();

			AssertContainsExactElementsInAnyOrder(
				new[]
				{
					typeof(ViewCommissionLineRecipientAndPreferredCompanyGrouper<CommissionFinalizerLineItem>),
					typeof(ViewCommissionLineLocalCompanyGrouper<CommissionFinalizerLineItem>),
					typeof(ViewCommissionLineSourceAndStatusGrouper<CommissionFinalizerLineItem>)
				},
				collection.Groupers.Select(x => x.GetType()));
		}

		public void TestGroupers_ShowForCurrentLoginCompany()
		{
			OrganisationsDataRegistry.Instance.OnlyShowCommissionsForCurrentLoginCompany.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			var lineItemCollection = new CommissionFinalizerLineItemCollection(Factory);
			var collection = new TopLevelCommissionFinalizerLineItemGroupingCollection(lineItemCollection);
			collection.Init();

			AssertContainsExactElementsInAnyOrder(
				new[]
				{
					typeof(ViewCommissionLineRecipientAndPreferredCompanyGrouper<CommissionFinalizerLineItem>),
					typeof(ViewCommissionLineSourceAndStatusGrouper<CommissionFinalizerLineItem>)
				},
				collection.Groupers.Select(x => x.GetType()));
		}

		#region Implementation

		protected override TopLevelCommissionFinalizerLineItemGroupingCollection GetCollectionToTest()
		{
			var lineItemCollection = new CommissionFinalizerLineItemCollection(Factory);
			var collection = new TopLevelCommissionFinalizerLineItemGroupingCollection(lineItemCollection);
			collection.Init();
			return collection;
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			var grouping = new CommissionFinalizerLineItemGrouping(Factory);
			grouping.Init(new[] { new CommissionFinalizerLineItem(Factory.New<ViewCommissionLine>()) });
			return grouping;
		}

		#endregion
	}
}
