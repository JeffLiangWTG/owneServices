using System;
using CargoWise.Customs.DE.MessageDefinitions.ATLASVersion10_1;
using Enterprise.Customs.DE.Messaging.Testing;
using NUnit.Framework;
using WTG.NUnit;

namespace Enterprise.Customs.DE.Messaging.ATLASVersion10_1.Testing
{
	[TestedType(typeof(CUSCANGoodsItemProvider))]
	sealed class CUSCANGoodsItemProviderTest : InboundDataProviderTestCase<ICUSCANGoodsItem, CUSCANGoodsItemProvider>
	{
		public void TestConstructorThrowsArgumentException()
		{
			AssertExceptionThrown<ArgumentException>(() => new CUSCANGoodsItemProvider(null));
		}

		[ExpectNoExceptions]
		public void TestSequenceNumber()
		{
			goodsItem.SequenceNumber = "12";
			NUnit.Framework.Assert.That(dataProvider.SequenceNumber, Is.EqualTo("12").Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestCustomsGoodsStatus()
		{
			goodsItem.CustomsGoodsStatus = "05";
			NUnit.Framework.Assert.That(dataProvider.CustomsGoodsStatus, Is.EqualTo("05").Using(CustomComparers.TypeComparison));
		}

		protected override void SetUp()
		{
			base.SetUp();
			goodsItem = new SCCANEGoodsItem();
			dataProvider = new CUSCANGoodsItemProvider(goodsItem);
		}
		SCCANEGoodsItem goodsItem;
		ICUSCANGoodsItem dataProvider;

		protected override CUSCANGoodsItemProvider GetProvider() => (CUSCANGoodsItemProvider)dataProvider;
	}
}
