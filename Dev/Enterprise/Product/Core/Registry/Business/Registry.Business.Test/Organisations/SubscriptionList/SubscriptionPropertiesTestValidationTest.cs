using CargoWise.EntityFramework.Testing;

namespace Enterprise.Registry.Business.Testing
{
	sealed class SubscriptionPropertiesTestValidationTest : BusinessObjectValidationTestCase
	{
		public void TestValidateType()
		{
			var collection = new SubscriptionListNodeCollection(true);
			var node1 = collection.AddNew();

			node1.MediaCategoryWithAll = "XXX";
			node1.MediaTypeWithAll = "ZZZ";
			node1.PublishedDescription = "DESC";
			node1.PublishedSummary = "";

			AssertMandatoryValidationError(node1.MediaTypeWithAllInfo, false);
			AssertListValidationInvalidCodeError(node1.MediaTypeWithAllInfo, true);
		}
	}
}
