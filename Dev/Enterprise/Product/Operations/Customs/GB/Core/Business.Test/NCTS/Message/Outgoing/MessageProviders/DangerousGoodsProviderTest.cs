using Enterprise.Customs.Business.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.GB.Business.NCTS.Testing
{
	class DangerousGoodsProviderTest : DataProviderTestCase<DangerousGoodsProvider>
	{
		public void TestSequenceNumber()
		{
			AssertEquals(3, Provider.SequenceNumber);
		}

		public void TestUNNumber()
		{
			AssertEquals("xyz", Provider.UNNumber);
		}

		protected override DangerousGoodsProvider GetProvider()
		{
			var substance = Factory.New<UNDGSubstance>();
			substance.DG_UNNO = "xyz";
			var undg = Factory.New<UNDGDataItem>();
			undg.DI_DG = substance.PK;
			return new DangerousGoodsProvider(undg, 3);
		}
	}
}
