using System.Linq;
using CargoWise.Customs.DE.MessageContracts.NCTS;
using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.DE.NCTS.Business.Testing
{
	sealed class NCTSEventSealsProviderTest : Customs.Business.Testing.DataProviderTestCase<NCTSEventSealsProvider>
	{
		public void TestNew()
		{
			CombineAssertions(() =>
			{
				AssertNull("NULL", NCTSEventSealsProvider.NewOrNull(null));
				AssertNotNull("Not NULL", dataProvider);
			});
		}

		public void TestNumber()
		{
			AssertEquals("Default", 0, dataProvider.Number);
		}

		public void TestNumber_Entered()
		{
			seals.BN_NoOfSeals = 2;
			AssertEquals("Entered", 2, dataProvider.Number);
		}

		public void TestIdentities()
		{
			CombineAssertions(() =>
			{
				var identities = dataProvider.Identities;
				AssertEquals("Default", 0, identities.Count);
				AssertSame("Cached", identities, dataProvider.Identities);
			});
		}

		public void TestIdentities_Entered()
		{
			CombineAssertions(() =>
			{
				seals.SealContainers.AddNew().BC_Seal1 = "seal1";
				var identities = dataProvider.Identities;
				AssertEquals("Count", 1, identities.Count);
				AssertEquals("Identity", "seal1", identities.Single());
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			seals = Factory.New<EnRouteSeal>();
			dataProvider = NCTSEventSealsProvider.NewOrNull(seals);
		}
		EnRouteSeal seals;
		INCTSSeals dataProvider;

		protected override NCTSEventSealsProvider GetProvider() => (NCTSEventSealsProvider)dataProvider;
	}
}
