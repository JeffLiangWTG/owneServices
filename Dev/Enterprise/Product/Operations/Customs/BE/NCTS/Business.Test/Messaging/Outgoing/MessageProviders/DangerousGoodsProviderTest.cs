using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.BE.NCTS.Business.Testing
{
	sealed class DangerousGoodsProviderTest : Customs.Business.Testing.DataProviderTestCase<DangerousGoodsProvider>
	{
		public void TestSequenceNumber()
		{
			AssertEquals(3, Provider.SequenceNumber);
		}

		public void TestUNNumber()
		{
			AssertEquals("xyz", Provider.UNNumber);
		}

		protected override void SetUp()
		{
			base.SetUp();

			substance = Factory.New<UNDGSubstance>();
			substance.DG_UNNO = "xyz";
			var undg = Factory.New<UNDGDataItem>();
			undg.DI_DG = substance.PK;
			provider = new DangerousGoodsProvider(undg, 3);
		}

		protected override DangerousGoodsProvider GetProvider() => provider;

		UNDGSubstance substance;
		DangerousGoodsProvider provider;
	}
}
