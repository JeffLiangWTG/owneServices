using System;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(OpportunityClosedReasonsRegistryItem))]
	sealed class OpportunityLostReasonsRegistryItemTest : StronglyTypedRegistryItemTestCase<OpportunityClosedReasonsCollection>
	{
		protected override StronglyTypedRegistryItem<OpportunityClosedReasonsCollection, OpportunityClosedReasonsCollection> GetNewRegistryItem()
		{
			return new OpportunityClosedReasonsRegistryItem("", null, null, null, RegistryStorageFlags.Company, RegistryOptions.PreserveTestValue, new OpportunityClosedReasonsCollection());
		}

		public void TestGetValueWithoutFallbackCore()
		{
			var opportunityClosedReasonsRegistryItem = GetNewRegistryItem();
			var companyPK = Guid.NewGuid();
			var collection = opportunityClosedReasonsRegistryItem.GetValueWithoutFallback(companyPK, Guid.Empty, Guid.Empty);
			AssertEquals(companyPK, collection.CurrentFallbackLevel.CompanyPK(false));
			AssertEquals(Guid.Empty, collection.CurrentFallbackLevel.BranchPK);
			AssertEquals(Guid.Empty, collection.CurrentFallbackLevel.DepartmentPK);
		}
	}
}
