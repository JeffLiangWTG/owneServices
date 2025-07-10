using System;
using CargoWise.Customs.DE.MessageDefinitions.ATLASVersion10_1;
using Enterprise.Customs.DE.Messaging.Testing;
using NUnit.Framework;
using WTG.NUnit;

namespace Enterprise.Customs.DE.Messaging.ATLASVersion10_1.Testing
{
	[TestedType(typeof(CURRELGoodsItemProvider))]
	sealed class CURRELGoodsItemProviderTest : InboundDataProviderTestCase<ICURRELGoodsItem, CURRELGoodsItemProvider>
	{
		public void TestConstructorThrowsArgumentException()
		{
			AssertExceptionThrown<ArgumentException>(() => new CURRELGoodsItemProvider(null));
		}

		[ExpectNoExceptions]
		public void TestSequenceNumber()
		{
			goodsItem.SequenceNumber = "12";
			NUnit.Framework.Assert.That(dataProvider.SequenceNumber, Is.EqualTo("12").Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestAcceptanceFlag()
		{
			goodsItem.AcceptanceFlag = "A";
			NUnit.Framework.Assert.That(dataProvider.AcceptanceFlag, Is.EqualTo("A").Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestDirectiveFlag()
		{
			goodsItem.DirectiveFlag = "Z";
			NUnit.Framework.Assert.That(dataProvider.DirectiveFlag, Is.EqualTo("Z").Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestRejectionFlag()
		{
			goodsItem.RejectionFlag = "A";
			NUnit.Framework.Assert.That(dataProvider.RejectionFlag, Is.EqualTo("A").Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestIssuingFlag()
		{
			goodsItem.IssuingFlag = "Z";
			NUnit.Framework.Assert.That(dataProvider.IssuingFlag, Is.EqualTo("Z").Using(CustomComparers.TypeComparison));
		}

		protected override void SetUp()
		{
			base.SetUp();
			goodsItem = new GCRELGGoodsItem();
			dataProvider = new CURRELGoodsItemProvider(goodsItem);
		}
		GCRELGGoodsItem goodsItem;
		CURRELGoodsItemProvider dataProvider;

		protected override CURRELGoodsItemProvider GetProvider() => dataProvider;
	}
}
