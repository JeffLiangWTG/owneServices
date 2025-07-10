using System;
using Enterprise.Client.EDI.IdentityTenant.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Client.EDI.IdentityTenant.Module.Testing
{
	[TestedType(typeof(EdiIdentityTenantFilterBusinessObject))]
	internal class EdiIdentityTenantFilterBusinessObjectTest : FilterStripBusinessObjectTestCase
	{
		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject()
		{
			return new EdiIdentityTenantFilterBusinessObject();
		}

		public void TestTenantIdQuery()
		{
			var tenant1 = Factory.NewWithValidTestData<EdiIdentityTenant>();
			var tenantId1 = Guid.NewGuid().ToString();
			tenant1.IDT_TenantId = tenantId1;
			var tenant2 = Factory.NewWithValidTestData<EdiIdentityTenant>();
			var tenantId2 = Guid.NewGuid().ToString();
			tenant2.IDT_TenantId = tenantId2;
			Factory.Save();
			var filter = new EdiIdentityTenantFilterBusinessObject();
			((ModuleTextFilter)filter["Tenant Id"]).Property = tenantId1;
			((ModuleTextFilter)filter["Tenant Id"]).IsActive = true;
			var collection = new EdiIdentityTenantCollection(Factory);
			collection.AdditionalFilter = filter.Filter;
			AssertCollectionContains("Should have Tenant1", tenant1, collection);
			AssertCollectionNotContains("Should not have Tenant2", tenant2, collection);

			((ModuleTextFilter)filter["Tenant Id"]).Property = tenantId2;
			collection.AdditionalFilter = filter.Filter;
			AssertCollectionNotContains("Should not have Tenant1", tenant1, collection);
			AssertCollectionContains("Should have Tenant2", tenant2, collection);

			((ModuleTextFilter)filter["Tenant Id"]).Property = Guid.NewGuid().ToString();
			collection.AdditionalFilter = filter.Filter;
			AssertCollectionNotContains("Should not have Tenant1", tenant1, collection);
			AssertCollectionNotContains("Should not have Tenant2", tenant2, collection);
		}

		public void TestClientIdQuery()
		{
			var tenant1 = Factory.NewWithValidTestData<EdiIdentityTenant>();
			var clientId1 = Guid.NewGuid().ToString();
			tenant1.IDT_OidcClientId = clientId1;
			var tenant2 = Factory.NewWithValidTestData<EdiIdentityTenant>();
			var clientId2 = Guid.NewGuid().ToString();
			tenant2.IDT_OidcClientId = clientId2;
			Factory.Save();
			var filter = new EdiIdentityTenantFilterBusinessObject();
			((ModuleTextFilter)filter["OIDC Client Id"]).Property = clientId1;
			((ModuleTextFilter)filter["OIDC Client Id"]).IsActive = true;
			var collection = new EdiIdentityTenantCollection(Factory);
			collection.AdditionalFilter = filter.Filter;
			AssertCollectionContains("Should have Tenant1", tenant1, collection);
			AssertCollectionNotContains("Should not have Tenant2", tenant2, collection);

			((ModuleTextFilter)filter["OIDC Client Id"]).Property = clientId2;
			collection.AdditionalFilter = filter.Filter;
			AssertCollectionNotContains("Should not have Tenant1", tenant1, collection);
			AssertCollectionContains("Should have Tenant2", tenant2, collection);

			((ModuleTextFilter)filter["OIDC Client Id"]).Property = Guid.NewGuid().ToString();
			collection.AdditionalFilter = filter.Filter;
			AssertCollectionNotContains("Should not have Tenant1", tenant1, collection);
			AssertCollectionNotContains("Should not have Tenant2", tenant2, collection);
		}

		public void TestGraphClientIdQuery()
		{
			var tenant1 = Factory.NewWithValidTestData<EdiIdentityTenant>();
			var graphClientId1 = Guid.NewGuid().ToString();
			tenant1.IDT_GraphClientId = graphClientId1;
			var tenant2 = Factory.NewWithValidTestData<EdiIdentityTenant>();
			var graphClientId2 = Guid.NewGuid().ToString();
			tenant2.IDT_GraphClientId = graphClientId2;
			Factory.Save();
			var filter = new EdiIdentityTenantFilterBusinessObject();
			((ModuleTextFilter)filter["Graph Client Id"]).Property = graphClientId1;
			((ModuleTextFilter)filter["Graph Client Id"]).IsActive = true;
			var collection = new EdiIdentityTenantCollection(Factory);
			collection.AdditionalFilter = filter.Filter;
			AssertCollectionContains("Should have Tenant1", tenant1, collection);
			AssertCollectionNotContains("Should not have Tenant2", tenant2, collection);

			((ModuleTextFilter)filter["Graph Client Id"]).Property = graphClientId2;
			collection.AdditionalFilter = filter.Filter;
			AssertCollectionNotContains("Should not have Tenant1", tenant1, collection);
			AssertCollectionContains("Should have Tenant2", tenant2, collection);

			((ModuleTextFilter)filter["Graph Client Id"]).Property = Guid.NewGuid().ToString();
			collection.AdditionalFilter = filter.Filter;
			AssertCollectionNotContains("Should not have Tenant1", tenant1, collection);
			AssertCollectionNotContains("Should not have Tenant2", tenant2, collection);
		}

		public void TestTenantNameQuery()
		{
			var tenant1 = Factory.NewWithValidTestData<EdiIdentityTenant>();
			tenant1.IDT_Name = "Match1";
			var tenant2 = Factory.NewWithValidTestData<EdiIdentityTenant>();
			tenant2.IDT_Name = "Match2";
			Factory.Save();
			var filter = new EdiIdentityTenantFilterBusinessObject();
			((ModuleTextFilter)filter["Name"]).Property = "Match1";
			((ModuleTextFilter)filter["Name"]).IsActive = true;
			var collection = new EdiIdentityTenantCollection(Factory);
			collection.AdditionalFilter = filter.Filter;
			AssertCollectionContains("Should have Tenant1", tenant1, collection);
			AssertCollectionNotContains("Should not have Tenant2", tenant2, collection);

			((ModuleTextFilter)filter["Name"]).Property = "Match2";
			collection.AdditionalFilter = filter.Filter;
			AssertCollectionNotContains("Should not have Tenant1", tenant1, collection);
			AssertCollectionContains("Should have Tenant2", tenant2, collection);

			((ModuleTextFilter)filter["Name"]).Property = "Non-Match";
			collection.AdditionalFilter = filter.Filter;
			AssertCollectionNotContains("Should not have Tenant1", tenant1, collection);
			AssertCollectionNotContains("Should not have Tenant2", tenant2, collection);
		}

		public void TestOnBoardingFilter()
		{
			var onBoardingDbo = Factory.NewWithValidTestData<EdiIdentityTenant>();
			onBoardingDbo.IDT_Onboarding = true;
			var notOnBoardingDbo = Factory.NewWithValidTestData<EdiIdentityTenant>();
			notOnBoardingDbo.IDT_Onboarding = false;
			Factory.Save();
			var filter = new EdiIdentityTenantFilterBusinessObject();
			var onBoardingStatusFilter = (ModuleTextFilter)filter["Onboarding Status"];
			onBoardingStatusFilter.Property = EdiIdentityTenantFilterBusinessObject.StatusOnboarding;
			onBoardingStatusFilter.IsActive = true;
			var collection = new EdiIdentityTenantCollection(Factory);
			collection.AdditionalFilter = filter.Filter;
			AssertContainsExactElementsInAnyOrder(onBoardingDbo, collection);
			onBoardingStatusFilter.Property = EdiIdentityTenantFilterBusinessObject.StatusNotOnboarding;
			collection.AdditionalFilter = filter.Filter;
			AssertContainsExactElementsInAnyOrder(notOnBoardingDbo, collection);
			onBoardingStatusFilter.Property = FilterStripBusinessObject.StatusAll;
			collection.AdditionalFilter = filter.Filter;
			AssertContainsExactElementsInAnyOrder(new EdiIdentityTenant[] { onBoardingDbo, notOnBoardingDbo }, collection);
		}
	}
}
