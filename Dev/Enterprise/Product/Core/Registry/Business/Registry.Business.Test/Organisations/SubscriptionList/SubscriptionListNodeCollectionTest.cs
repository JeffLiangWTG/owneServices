using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(SubscriptionListNodeCollection))]
	sealed class SubscriptionListNodeCollectionTest : NonPersistentBusinessObjectCollectionTestCase<SubscriptionListNodeCollection>
	{
		public void TestCreateNonPersistentBusinessObjectShouldCreateHRMLookupWhenIsHRCampaign()
		{
			const string codeA = "AAA";
			const string codeB = "BBB";
			var categoryAList = new CodeDescriptionBoolCollection();
			categoryAList.Add(codeA, (NoResString)"Category AAA");
			var categoryBList = new CodeDescriptionBoolCollection();
			categoryBList.Add(codeB, (NoResString)"Category BBB");

			OrganisationsDataRegistry.Instance.CampaignCategory1List.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, categoryAList);
			OrganisationsDataRegistry.Instance.HRCampaignCategory1List.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, categoryBList);

			var collection = new SubscriptionListNodeCollectionForTest(true);
			var subProperty1 = collection.CreateNonPersistentBusinessObjectExposed();
			AssertEquals("Should use CRM registry list since IsHRCampaign defaults to false", true, subProperty1.Lookups.MediaCategoryWithAllList.ContainsCode(codeA));
			AssertEquals("Should use CRM registry list since IsHRCampaign defaults to false", false, subProperty1.Lookups.MediaCategoryWithAllList.ContainsCode(codeB));

			collection.IsHRCampaign = true;
			var subProperty2 = collection.CreateNonPersistentBusinessObjectExposed();
			AssertEquals("Should use HRM registry list since IsHRCampaign is true", false, subProperty2.Lookups.MediaCategoryWithAllList.ContainsCode(codeA));
			AssertEquals("Should use HRM registry list since IsHRCampaign is true", true, subProperty2.Lookups.MediaCategoryWithAllList.ContainsCode(codeB));
		}

		#region Implementation

		protected override SubscriptionListNodeCollection GetCollectionToTest()
		{
			return new SubscriptionListNodeCollection(true);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new SubscriptionProperties();
		}

		#endregion
	}
}
