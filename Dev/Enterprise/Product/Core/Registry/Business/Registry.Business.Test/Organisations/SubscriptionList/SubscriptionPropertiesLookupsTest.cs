using System;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Registry.Business.Testing
{
	sealed class SubscriptionPropertiesLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestMediaCategoryAndTypeListLookups()
		{
			var collectionA = new CodeDescriptionBoolCollection
			{
				{ "AAA", (NoResString)"Category A", true }
			};

			var collectionB = new CodeDescriptionBoolCollection
			{
				{ "BBB", (NoResString)"Category B", true }
			};

			var collectionC = new CodeDescriptionBoolCollection
			{
				{ "CCC", (NoResString)"Category C", true }
			};

			var collectionD = new CodeDescriptionBoolCollection
			{
				{ "DDD", (NoResString)"Category D", true }
			};

			OrganisationsDataRegistry.Instance.CampaignCategory2List.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, collectionA);
			OrganisationsDataRegistry.Instance.HRCampaignCategory2List.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, collectionB);
			OrganisationsDataRegistry.Instance.CampaignCategory1List.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, collectionC);
			OrganisationsDataRegistry.Instance.HRCampaignCategory1List.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, collectionD);

			var collection = new SubscriptionListNodeCollection(true);
			var node1 = collection.AddNew();
			node1.IsHRCampaign = true;
			var lookups = new SubscriptionPropertiesLookups(node1);
			Assert("Collection should use the HR list from the registry", lookups.MediaTypeWithAllList.ContainsCode(collectionB[0].Code));
			Assert("Collection should use the HR list from the registry", lookups.MediaCategoryWithAllList.ContainsCode(collectionD[0].Code));

			var node2 = collection.AddNew();
			node2.IsHRCampaign = false;
			var lookups2 = new SubscriptionPropertiesLookups(node2);
			Assert("Collection should use the CRM list from the registry", lookups2.MediaTypeWithAllList.ContainsCode(collectionA[0].Code));
			Assert("Collection should use the CRM list from the registry", lookups2.MediaCategoryWithAllList.ContainsCode(collectionC[0].Code));
		}

		public void TestChangingIsHRCampaignShouldChangeLookups()
		{
			var collectionA = new CodeDescriptionBoolCollection
			{
				{ "AAA", (NoResString)"Category A", true }
			};

			var collectionB = new CodeDescriptionBoolCollection
			{
				{ "BBB", (NoResString)"Category B", true }
			};

			var collectionC = new CodeDescriptionBoolCollection
			{
				{ "CCC", (NoResString)"Category C", true }
			};

			var collectionD = new CodeDescriptionBoolCollection
			{
				{ "DDD", (NoResString)"Category D", true }
			};

			OrganisationsDataRegistry.Instance.CampaignCategory2List.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, collectionA);
			OrganisationsDataRegistry.Instance.HRCampaignCategory2List.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, collectionB);
			OrganisationsDataRegistry.Instance.CampaignCategory1List.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, collectionC);
			OrganisationsDataRegistry.Instance.HRCampaignCategory1List.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, collectionD);

			var collection = new SubscriptionListNodeCollection(true);
			var node1 = collection.AddNew();
			node1.IsHRCampaign = true;

			var lookups = new SubscriptionPropertiesLookups(node1);
			Assert("Collection should use the HR list from the registry", lookups.MediaTypeWithAllList.ContainsCode(collectionB[0].Code));
			Assert("Collection should use the HR list from the registry", lookups.MediaCategoryWithAllList.ContainsCode(collectionD[0].Code));

			node1.IsHRCampaign = false;
			Assert("Collection should use the CRM list from the registry", lookups.MediaTypeWithAllList.ContainsCode(collectionA[0].Code));
			Assert("Collection should use the CRM list from the registry", lookups.MediaCategoryWithAllList.ContainsCode(collectionC[0].Code));
		}
	}
}
