using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.GB.Business.Testing;

namespace Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects.Testing
{
	sealed class CusHawbLookupsTests : BusinessObjectLookupsTestCase
	{
		public void TestProfilesLookup()
		{
			DeclarationTestHelper.CreateAgentAndShedBadgesAndCreds();
			Factory.Save();

			var factory2 = new BusinessObjectFactory();
			var mawb2 = factory2.New<CusMAWB>();
			var hawb2 = mawb2.ChildBills.AddNew();
			var profiles = hawb2.Lookups.ProfilesList;
			AssertEquals("Contains CCSUK agent and shed profiles when shed is licenced", 2, profiles.Count);
			Assert(((profiles[0].Code == "CUKFFW98000LXA" && profiles[1].Code == "CUKAIR98LHRCAX") || (profiles[1].Code == "CUKFFW98000LXA" && profiles[0].Code == "CUKAIR98LHRCAX")));
			Assert(((profiles[0].Description == "Agent LXA" && profiles[1].Description == "Shed CAX") || (profiles[1].Description == "Agent LXA" && profiles[0].Description == "Shed CAX")));
		}

		public void TestPresenceOnNetworkList()
		{
			var mawb = Factory.New<CusMAWB>();
			var hawb = mawb.ChildBills.AddNew();
			AssertContains(PresenceOnNetworkList.Codes.NoInformationSendAnFsrWithUpdateToCheck, hawb.Lookups.PresenceOnNetworkList.CodesAsString);
		}

		[StressTest]  // it's not a stress test, but this makes the exception reporter shut up
		public void TestOriginAirport()
		{
			var mawb = Factory.New<CusMAWB>();
			var hawb = mawb.ChildBills.AddNew();
			var nonUkAirports = hawb.Lookups.NonUkAirports;
			CombineAssertions(() =>
			{
				AssertEquals("No GB", false, nonUkAirports.Any(x => x.RL_Code == "GBLHR"));
				AssertEquals("One FR", 1, nonUkAirports.Count(x => x.RL_Code == "FRANT"));
				AssertEquals("One US", 1, nonUkAirports.Count(x => x.RL_Code == "USATL"));
			});
		}
	}
}
