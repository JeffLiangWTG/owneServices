using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Client.EDI.MarketingManager.Business.Test
{
	[TestedType(typeof(BusinessObjectCollection))]
	class EDIGlbCompanyCampaignItemCollectionTest : BusinessObjectCollectionTestCase
	{
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			EDIGlbCompanyCampaign campaign = Factory.New<EDIGlbCompanyCampaign>();
			return new EDIGlbCompanyCampaignItemCollection(campaign);
		}

		public void TestElementOfCorrectType()
		{
			AssertEquals(typeof(EDIGlbCompanyCampaignItem), Collection.TypeOfElements);
		}
	}
}
