using System;
using CargoWise.Customs.DE.MessageDefinitions.ATLASVersion10_1;
using Enterprise.Customs.DE.Messaging.Testing;
using NUnit.Framework;
using WTG.NUnit;

namespace Enterprise.Customs.DE.Messaging.ATLASVersion10_1.Testing
{
	[TestedType(typeof(ECWINFGoodsItemProvider))]
	sealed class ECWINFGoodsItemProviderTest : InboundDataProviderTestCase<IECWINFGoodsItem, ECWINFGoodsItemProvider>
	{
		public void TestConstructorThrowsArgumentException()
		{
			AssertExceptionThrown<ArgumentException>(() => new ECWINFGoodsItemProvider(null));
		}

		[ExpectNoExceptions]
		public void TestReferencedRegistrationNumber()
		{
			goodsItem.ReferencedRegistrationNumber = "RefNumber12345";
			NUnit.Framework.Assert.That(dataProvider.ReferencedRegistrationNumber, Is.EqualTo("RefNumber12345").Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestMRN()
		{
			NUnit.Framework.Assert.That(goodsItem.MRN, Is.EqualTo(default(string)), "Precondition - should be [null]");
			NUnit.Framework.Assert.That(dataProvider.MRN, Is.EqualTo(default(string)));

			goodsItem.MRN = "123";
			NUnit.Framework.Assert.That(dataProvider.MRN, Is.EqualTo("123"));
		}

		protected override void SetUp()
		{
			base.SetUp();
			goodsItem = new LECWIFCustomsWarehouseGoodsItem();
			dataProvider = new ECWINFGoodsItemProvider(goodsItem);
		}
		LECWIFCustomsWarehouseGoodsItem goodsItem;
		ECWINFGoodsItemProvider dataProvider;

		protected override ECWINFGoodsItemProvider GetProvider() => dataProvider;
	}
}
