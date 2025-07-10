using CargoWise.Types;

namespace Enterprise.Customs.DE.NCTS.Business.Testing
{
	sealed class NCTSSealsProviderTest : Customs.Business.Testing.DataProviderTestCase<NCTSSealsProvider>
	{
		public void TestNewOrNull()
		{
			AssertNull(NCTSSealsProvider.NewOrNull(null));
		}

		public void TestNumber()
		{
			AssertEquals(2, Provider.Number);
		}

		public void TestIdentities()
		{
			AssertSequencesEqual(new string[] { "111", "222" }, Provider.Identities);
		}

		protected override NCTSSealsProvider GetProvider() => NCTSSealsProvider.NewOrNull(seals);

		protected override void SetUp()
		{
			base.SetUp();
			seals = new ZString[] { "222", "111" };
		}
		ZString[] seals;
	}
}
