namespace Enterprise.Customs.IE.Business.AIS.Testing
{
	sealed class TransportEquipmentProviderTest : Customs.Business.Testing.DataProviderTestCase<TransportEquipmentProvider>
	{
		public void TestContainerId()
		{
			AssertEquals("ContainerId", "ID", Provider.ContainerId);
		}

		public void TestGoodsReferences()
		{
			var goodsReferences = new[] { "DGIN001" };
			var provider = new TransportEquipmentProvider("ID", goodsReferences);
			AssertSame(goodsReferences, provider.GoodsReferences);
		}

		protected override TransportEquipmentProvider GetProvider() => new TransportEquipmentProvider("ID", new[] { "DGIN001" });
	}
}
