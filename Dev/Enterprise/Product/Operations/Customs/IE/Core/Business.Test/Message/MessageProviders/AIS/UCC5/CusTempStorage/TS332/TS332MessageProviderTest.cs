using System;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.IE.Business.CusTempStorage;

namespace Enterprise.Customs.IE.Business.AIS.UCC5.CusTempStorage.Testing
{
	sealed class TS332MessageProviderTest : DataProviderTestCase<TS332MessageProvider>
	{
		public void TestConstructor()
		{
			AssertExceptionThrown<ArgumentException>("Should throw ArgumentException with null header.", () => new TS332MessageProvider(null));
		}

		public void TestDeclaration()
		{
			AssertType<TS332DeclarationTypeProvider>(Provider.Declaration);
		}

		public void TestGoodsShipment()
		{
			AssertType<TS332GoodsShipmentTypeProvider>(Provider.GoodsShipment);
		}

		protected override TS332MessageProvider GetProvider() => new TS332MessageProvider(new TemporaryStorageMessageSendingObject(header));

		protected override void SetUp()
		{
			base.SetUp();
			header = Factory.NewWithValidTestData<TemporaryStorageHeader>();
		}
		TemporaryStorageHeader header;
	}
}
