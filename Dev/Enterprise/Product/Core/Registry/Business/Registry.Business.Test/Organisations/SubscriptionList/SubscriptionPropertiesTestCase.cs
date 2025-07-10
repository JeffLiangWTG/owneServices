using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(SubscriptionProperties))]
	sealed class SubscriptionPropertiesTestCase : NonPersistentBusinessObjectTestCase
	{
		public void TestValidateIsOrgLevel()
		{
			var collection = new SubscriptionListNodeCollection(true);
			var node1 = collection.AddNew();

			node1.IsSubscribed = true;
			node1.IsOrgLevel = false;

			AssertNoWarnings(node1.IsOrgLevelInfo);

			node1.IsSubscribed = false;
			AssertNoWarnings(node1.IsOrgLevelInfo);

			node1.IsSubscribed = true;
			node1.IsOrgLevel = true;
			AssertNoWarnings(node1.IsOrgLevelInfo);

			node1.IsSubscribed = false;
			AssertHasWarning(node1.IsOrgLevelInfo, "Un-subscribing this contact's Organization from campaign categories/types will overwrite existing related Subscription Preferences for all of the Organization's Contacts.");

			node1.IsSubscribed = true;
			AssertNoWarnings(node1.IsOrgLevelInfo);

			node1.IsSubscribed = false;
			node1.IsOrgLevel = false;
			AssertNoWarnings(node1.IsOrgLevelInfo);

			node1.IsOrgLevel = true;
			AssertHasWarning(node1.IsOrgLevelInfo, "Un-subscribing this contact's Organization from campaign categories/types will overwrite existing related Subscription Preferences for all of the Organization's Contacts.");
		}

		#region Implementation

		protected override BusinessObject GetNewBusinessObject()
		{
			return new SubscriptionPropertiesTest();
		}
		#endregion
	}
}
