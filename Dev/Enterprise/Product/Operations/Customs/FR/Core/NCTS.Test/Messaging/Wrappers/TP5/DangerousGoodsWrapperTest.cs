using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.FR.NCTS.Messaging.TP5.Testing
{
	class DangerousGoodsWrapperTest : Customs.Business.Testing.DataProviderTestCase<DangerousGoodsWrapper>
	{
		public void TestUNNumber()
		{
			AssertEquals("UNNumber shoul equal the item substance code.", "0004b", Provider.UNNumber);
		}

		protected override DangerousGoodsWrapper GetProvider()
		{
			var substance = Factory.New<UNDGSubstance>();
			substance.DG_Code = "0004b";

			var undgItem = Factory.New<UNDGDataItem>();
			undgItem.DI_DG = substance.PK;

			return DangerousGoodsWrapper.New(undgItem);
		}
	}
}
