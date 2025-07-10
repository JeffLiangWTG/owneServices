using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class AURollAddInfoLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestRollsList()
		{
			var roll = Factory.New<Roll>();
			AssertEquals("Rolls", typeof(CMRClientRolls), roll.AddInfoLookups.ClientRolls.GetType());
		}
	}
}
