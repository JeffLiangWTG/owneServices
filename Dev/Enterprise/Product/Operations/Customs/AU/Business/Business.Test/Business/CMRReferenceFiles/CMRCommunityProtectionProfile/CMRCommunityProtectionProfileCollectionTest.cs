using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	[TestedType(typeof(CMRCommunityProtectionProfileCollection))]
	sealed class CMRCommunityProtectionProfileCollectionTest : BusinessObjectCollectionTestCase
	{
		public void TestContainsRiskID()
		{
			var collection = new CMRCommunityProtectionProfileCollection(Factory);
			var profileWithEmptyStat = collection.AddNew();
			profileWithEmptyStat.CP_CommunityProtectionRiskIdentifier = 400;

			var profileWithNotStat = collection.AddNew();
			profileWithNotStat.CP_CommunityProtectionRiskIdentifier = 401;

			AssertEquals("Contains", true, collection.Contains(400));
			AssertEquals("Contains", true, collection.Contains(401));
			AssertEquals("Contains", false, collection.Contains(402));
		}

		protected override BusinessObjectCollection GetCollectionToTest() => new CMRCommunityProtectionProfileCollection(Factory);

		protected override BusinessObject GetNewElementToAddToTheCollection() => Factory.New(typeof(CMRCommunityProtectionProfile));
	}
}
