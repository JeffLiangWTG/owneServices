using System;
using CargoWise.Customs.IE.MessageContracts.AIS.UCC5.Interfaces;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.IE.Business.CusTempStorage;

namespace Enterprise.Customs.IE.Business.AIS.UCC5.CusTempStorage.Testing
{
	sealed class TS315HeaderProviderTest : DataProviderTestCase<TS315MessageProvider>
	{
		public void TestConstructor()
		{
			AssertExceptionThrown<ArgumentException>("Should throw ArgumentException with null messageSendingObject.", () => new TS315MessageProvider(null));
		}

		public void TestProviderIsTS315Header()
		{
			Assert(Provider is ITS315Header);
		}

		public void TestDeclaration()
		{
			Assert(Provider.Declaration is ITS313AndTS315DeclarationType);
		}

		public void TestGoodsShipment()
		{
			Assert(Provider.GoodsShipment is ITS313AndTS315GoodsShipmentType);
		}

		protected override TS315MessageProvider GetProvider()
		{
			var header = Factory.New<TemporaryStorageHeader>();
			return new TS315MessageProvider(new TemporaryStorageMessageSendingObject(header));
		}
	}
}
