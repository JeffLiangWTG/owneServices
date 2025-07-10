using System;
using Enterprise.Customs.Business.Testing;

namespace Enterprise.Customs.IE.H7.Business.Messaging.V1.Test
{
	sealed class IM432HeaderProviderTest : DataProviderTestCase<IM432HeaderProvider>
	{
		public void TestConstructor()
		{
			AssertExceptionThrown<ArgumentException>("Message sending object missing", () => new IM432HeaderProvider(null));
		}

		public void TestDeclaration()
		{
			var declaration = Provider.Declaration;
			CombineAssertions("Declaration", () =>
			{
				Assert("Declaration is IM432DeclarationTypeProvider", declaration is IM432DeclarationTypeProvider);
				AssertSame("Is cached", declaration, Provider.Declaration);
			});
		}

		public void TestGoodsShipment()
		{
			var goodsShipment = Provider.GoodsShipment;
			CombineAssertions("GoodsShipment", () =>
			{
				Assert("GoodsShipment is IM432GoodsShipmentTypeProvider", goodsShipment is IM432GoodsShipmentTypeProvider);
				AssertSame("Is cached", goodsShipment, Provider.GoodsShipment);
			});
		}

		protected override IM432HeaderProvider GetProvider()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();

			var sendingObject = new MessageSendingObject(bill);
			return new IM432HeaderProvider(sendingObject);
		}
	}
}
