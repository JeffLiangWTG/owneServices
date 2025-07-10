using System;
using CargoWise.Customs.DE.MessageDefinitions.ATLASVersion10_1;
using Enterprise.Customs.DE.Messaging.Testing;
using NUnit.Framework;
using WTG.NUnit;

namespace Enterprise.Customs.DE.Messaging.ATLASVersion10_1.Testing
{
	[TestedType(typeof(NFFTAXGoodsItemProvider))]
	sealed class NFFTAXGoodsItemProviderTest : InboundDataProviderTestCase<INFFTAXGoodsItem, NFFTAXGoodsItemProvider>
	{
		public void TestConstructorThrowsArgumentException()
		{
			AssertExceptionThrown<ArgumentException>(() => new NFFTAXGoodsItemProvider(null));
		}

		[ExpectNoExceptions]
		public void TestSequenceNumber()
		{
			goodsItem.SequenceNumber = "1";
			NUnit.Framework.Assert.That(dataProvider.SequenceNumber, Is.EqualTo("1").Using(CustomComparers.TypeComparison));
		}

		protected override void SetUp()
		{
			base.SetUp();
			goodsItem = new GNTAXKGoodsItem();
			dataProvider = new NFFTAXGoodsItemProvider(goodsItem);
		}
		GNTAXKGoodsItem goodsItem;
		INFFTAXGoodsItem dataProvider;

		protected override NFFTAXGoodsItemProvider GetProvider() => (NFFTAXGoodsItemProvider)dataProvider;
	}
}
