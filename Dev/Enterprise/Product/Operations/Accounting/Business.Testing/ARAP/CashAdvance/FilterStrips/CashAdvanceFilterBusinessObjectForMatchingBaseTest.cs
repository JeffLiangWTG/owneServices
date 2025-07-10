using System;
using Enterprise.Accounting.Business.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Accounting.Business.Base.Matching.Testing
{
	public abstract class CashAdvanceFilterBusinessObjectForMatchingBaseTest : CashAdvanceFilterBusinessObjectTest
	{
		public override void TestStatusFilter()
		{
			var filter = (ModuleTextFilter)FilterBO["Status"];
			AssertEquals(CashAdvanceStatusCodes.RequestHeader.Requested, filter.Property);
			AssertEquals(FilterVisibility.AlwaysAppliedAndHidden, filter.Visibility);

			FilterCollection.Load(FilterBO.Filter);

			AssertEquals(2, FilterCollection.Count);
			AssertContainsExactElementsInAnyOrder(new AccCashAdvanceRequestHeader[] { CashAdvanceRequestHeader1, CashAdvanceRequestHeader2 }, FilterCollection);
			AssertCollectionNotContains(CashAdvanceRequestHeader3, FilterCollection);
		}

		public override void TestOSAmountFilter()
		{
			CashAdvanceRequestHeader3.CAH_GC_Company = GlbCompany.CurrentCompany.PK;

			Factory.Save();

			ModuleNumberRangeFilter filter = (ModuleNumberRangeFilter)FilterBO["OS Amount"];

			filter.Property1 = 0.0;
			filter.Property2 = 10.0;
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			AssertEquals(1, FilterCollection.Count);
			AssertCollectionContains(FormattableString.Invariant($"Cash Advance headers that are in REQ status and created for {ObjectCreator.LocalClient.OH_Code} with OS amount mmore between 0 and 10"), CashAdvanceRequestHeader1, FilterCollection);
			AssertCollectionNotContains(CashAdvanceRequestHeader2, FilterCollection);
			AssertCollectionNotContains(CashAdvanceRequestHeader3, FilterCollection);

			filter.Property1 = 100.0;
			filter.Property2 = 150.0;
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			AssertEquals(1, FilterCollection.Count);
			AssertCollectionNotContains(CashAdvanceRequestHeader1, FilterCollection);
			AssertContainsExactElementsInAnyOrder(FormattableString.Invariant($"Cash Advance headers that are in REQ status and created for {ObjectCreator.LocalClient.OH_Code} with OS amount mmore between 100 and 150"), new AccCashAdvanceRequestHeader[] { CashAdvanceRequestHeader2 }, FilterCollection);
		}

		protected override void SetUp()
		{
			MatchingFilterBizO.PrimaryOrganization = ObjectCreator.LocalClient.PK;
			base.SetUp();
		}

		protected MatchingFilterBusinessObject MatchingFilterBizO => matchingFilterBizO ?? (matchingFilterBizO = GetMatchingFilterBusinessObject());
		MatchingFilterBusinessObject matchingFilterBizO;

		protected abstract MatchingFilterBusinessObject GetMatchingFilterBusinessObject();

		protected override void SetupOrgFilter()
		{
			MatchingFilterBizO.PrimaryOrganization = Organisation1.PK;
		}
	}
}
