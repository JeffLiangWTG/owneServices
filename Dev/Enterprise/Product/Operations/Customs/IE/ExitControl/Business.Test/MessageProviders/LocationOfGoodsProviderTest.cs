namespace Enterprise.Customs.IE.ExitControl.Business.AES.Testing
{
	class LocationOfGoodsProviderTest : Customs.Business.Testing.DataProviderTestCase<LocationOfGoodsProvider, CargoWise.Customs.IE.MessageContracts.AES.Interfaces.ILocationOfGoods>
	{
		public void TestLocationCodeType()
		{
			locationCodeType = "ABC";
			AssertEquals("LocationCodeType", "ABC", IProvider.LocationCodeType);
		}

		public void TestUNLocode()
		{
			unlocode = "AUSYD";
			AssertEquals("UNLocode", "AUSYD", IProvider.UNLocode);
		}

		protected override LocationOfGoodsProvider GetProvider() => new LocationOfGoodsProvider(locationCodeType, unlocode);
		string locationCodeType;
		string unlocode;
	}
}
