using System;
using CargoWise.Customs.DE.MessageDefinitions.ATLASVersion10_1;
using Enterprise.Customs.DE.Messaging.Testing;
using NUnit.Framework;
using WTG.NUnit;

namespace Enterprise.Customs.DE.Messaging.ATLASVersion10_1.Testing
{
	[TestedType(typeof(CUSSTPGoodsItemProvider))]
	sealed class CUSSTPGoodsItemProviderTest : InboundDataProviderTestCase<ICUSSTPGoodsItem, CUSSTPGoodsItemProvider>
	{
		public void TestConstructorThrowsArgumentException()
		{
			AssertExceptionThrown<ArgumentException>(() => new CUSSTPGoodsItemProvider(null));
		}

		[ExpectNoExceptions]
		public void TestSequenceNumber()
		{
			goodsItem.CustomsIntervention = new SCSTPCGoodsItemCustomsIntervention()
			{
				SequenceNumber = "12"
			};
			NUnit.Framework.Assert.That(dataProvider.SequenceNumber, Is.EqualTo("12").Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestCustomsGoodsStatus()
		{
			goodsItem.CustomsIntervention = new SCSTPCGoodsItemCustomsIntervention()
			{
				Code = "05"
			};
			NUnit.Framework.Assert.That(dataProvider.CustomsGoodsStatus, Is.EqualTo("05").Using(CustomComparers.TypeComparison));
		}

		protected override void SetUp()
		{
			base.SetUp();
			goodsItem = new SCSTPCGoodsItem();
			dataProvider = new CUSSTPGoodsItemProvider(goodsItem);
		}
		SCSTPCGoodsItem goodsItem;
		ICUSSTPGoodsItem dataProvider;

		protected override CUSSTPGoodsItemProvider GetProvider() => (CUSSTPGoodsItemProvider)dataProvider;
	}
}
