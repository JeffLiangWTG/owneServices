using System;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.IE.Business.CusTempStorage;

namespace Enterprise.Customs.IE.Business.AIS.UCC5.CusTempStorage.Testing
{
	sealed class TS313MessageProviderTest : DataProviderTestCase<TS313MessageProvider>
	{
		public void TestConstructor()
		{
			AssertExceptionThrown<ArgumentException>("Should throw ArgumentException with null header.", () => new TS313MessageProvider(null));
		}

		public void TestDeclaration()
		{
			AssertType<TS313DeclarationTypeProvider>(Provider.Declaration);
		}

		public void TestGoodsShipment()
		{
			AssertType<TS313AndTS315GoodsShipmentTypeProvider>(Provider.GoodsShipment);
		}

		protected override TS313MessageProvider GetProvider() => new TS313MessageProvider(new TemporaryStorageMessageSendingObject(header));

		protected override void SetUp()
		{
			base.SetUp();
			header = Factory.NewWithValidTestData<TemporaryStorageHeader>();
		}
		TemporaryStorageHeader header;
	}
}
