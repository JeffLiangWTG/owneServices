using System;
using CargoWise.Customs.DE.MessageDefinitions.ATLASVersion10_1;
using Enterprise.Customs.DE.Messaging.Testing;
using NUnit.Framework;
using WTG.NUnit;

namespace Enterprise.Customs.DE.Messaging.ATLASVersion10_1.Testing
{
	[TestedType(typeof(CUSNOAProvider))]
	sealed class CUSNOAProviderTest : InboundDataProviderTestCase<ICUSNOA, CUSNOAProvider>
	{
		public void TestConstructorThrowsArgumentException()
		{
			AssertExceptionThrown<ArgumentException>(() => new CUSNOAProvider(null));
		}

		[ExpectNoExceptions]
		public void TestMessageIdentifier()
		{
			message.MetaData.MessageIdentifier = "CUSNOA58750000000375302250219160047";
			NUnit.Framework.Assert.That(dataProvider.MessageIdentifier, Is.EqualTo("CUSNOA58750000000375302250219160047"));
		}

		[ExpectNoExceptions]
		public void TestReferenceNumber()
		{
			message.Header.ReferenceNumber = "ATB150000620520195880";
			NUnit.Framework.Assert.That(dataProvider.ReferenceNumber, Is.EqualTo("ATB150000620520195880").Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestLocalReferenceNumber()
		{
			message.Header.LocalReferenceNumber = "MAS/22/11/22028";
			NUnit.Framework.Assert.That(dataProvider.LocalReferenceNumber, Is.EqualTo("MAS/22/11/22028").Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestGoodsItems()
		{
			message.Body = new[]
			{
				new GCNOADGoodsItem(),
				new GCNOADGoodsItem()
			};

			CombineAssertions(() =>
			{
				var goodsItems = dataProvider.GoodsItems;
				NUnit.Framework.Assert.That(goodsItems.Count, Is.EqualTo(2), "Count");
				NUnit.Framework.Assert.That(dataProvider.GoodsItems, Is.SameAs(goodsItems), "Cached");
			});
		}

		[ExpectNoExceptions]
		public void TestGoodsItems_Empty()
		{
			CombineAssertions(() =>
			{
				var goodsItems = dataProvider.GoodsItems;
				NUnit.Framework.Assert.That(goodsItems.Count, Is.EqualTo(0), "Count");
				NUnit.Framework.Assert.That(dataProvider.GoodsItems, Is.SameAs(goodsItems), "Cached");
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			message = new GCNOAD()
			{
				MetaData = new GCNOADMetaData(),
				Header = new GCNOADHeader()
			};
			dataProvider = new CUSNOAProvider(message);
		}
		GCNOAD message;
		ICUSNOA dataProvider;

		protected override CUSNOAProvider GetProvider() => (CUSNOAProvider)dataProvider;
	}
}
