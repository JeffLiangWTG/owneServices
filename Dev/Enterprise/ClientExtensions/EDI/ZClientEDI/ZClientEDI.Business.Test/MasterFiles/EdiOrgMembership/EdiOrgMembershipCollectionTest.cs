using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Client.EDI.MasterFiles.Business.Test
{
	[TestedType(typeof(EdiOrgMembershipCollection))]
	public class EdiOrgMembershipCollectionTest : ActiveBusinessObjectCollectionTestCase<EdiOrgMembershipCollection>
	{
		public void TestCreateAdhocCollection()
		{
			var collection = EdiOrgMembershipCollection.CreateAdhocCollection(Factory);
			var item1 = Factory.New<EdiOrgMembership>();
			var item2 = Factory.New<EdiOrgMembership>();
			collection.Add(item1);
			collection.Add(item2);
			AssertEquals(2, collection.Count);
		}

		protected override EdiOrgMembershipCollection GetCollectionToTest()
		{
			var org = Factory.New<EDIOrgHeader>();
			var collection = org.Memberships;
			return collection;
		}
	}
}
