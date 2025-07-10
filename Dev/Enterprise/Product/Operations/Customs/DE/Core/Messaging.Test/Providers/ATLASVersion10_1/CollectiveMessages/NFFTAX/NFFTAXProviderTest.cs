using System;
using System.Linq;
using CargoWise.Customs.DE.MessageDefinitions.ATLASVersion10_1;
using Enterprise.Customs.DE.Messaging.Testing;
using NUnit.Framework;
using WTG.NUnit;

namespace Enterprise.Customs.DE.Messaging.ATLASVersion10_1.Testing
{
	[TestedType(typeof(NFFTAXProvider))]
	sealed class NFFTAXProviderTest : InboundDataProviderTestCase<INFFTAX, NFFTAXProvider>
	{
		public void TestConstructorThrowsArgumentException()
		{
			AssertExceptionThrown<ArgumentException>(() => new NFFTAXProvider(null));
		}

		[ExpectNoExceptions]
		public void TestMessageIdentifier()
		{
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(dataProvider.MessageIdentifier, Is.EqualTo(default(string)), "Not populated - should be [null]");

				message.MetaData.MessageIdentifier = "MESSAGEIDENTIFIER";
				NUnit.Framework.Assert.That(dataProvider.MessageIdentifier, Is.EqualTo("MESSAGEIDENTIFIER"), "Populated");
			});
		}

		[ExpectNoExceptions]
		public void TestMRN()
		{
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(dataProvider.MRN, Is.EqualTo(default(string)), "Not populated - should be [null]");

				message.Header.MRN = "23DE586601055987B7";
				NUnit.Framework.Assert.That(dataProvider.MRN, Is.EqualTo("23DE586601055987B7"), "Populated");
			});
		}

		[ExpectNoExceptions]
		public void TestReferenceNumber()
		{
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(dataProvider.ReferenceNumber.ToString(), Is.Null.Or.Empty, "Not populated - should be [null] or [empty]");

				message.Header.ReferenceNumber = "REFERENCENUMBER";
				NUnit.Framework.Assert.That(dataProvider.ReferenceNumber, Is.EqualTo("REFERENCENUMBER").Using(CustomComparers.TypeComparison), "Populated");
			});
		}

		[ExpectNoExceptions]
		public void TestLocalReferenceNumber()
		{
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(dataProvider.LocalReferenceNumber.ToString(), Is.Null.Or.Empty, "Not populated - should be [null] or [empty]");

				message.Header.LRN = "LOCALREFERENCENUMBER";
				NUnit.Framework.Assert.That(dataProvider.LocalReferenceNumber, Is.EqualTo("LOCALREFERENCENUMBER").Using(CustomComparers.TypeComparison), "Populated");
			});
		}

		[ExpectNoExceptions]
		public void TestGoodsItems()
		{
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(dataProvider.GoodsItems.Count, Is.EqualTo(0), "Not populated");

				message.Body = new []
				{
					new GNTAXKGoodsItem(),
					new GNTAXKGoodsItem()
				};
				dataProvider = new NFFTAXProvider(message);
				NUnit.Framework.Assert.That(dataProvider.GoodsItems.Count, Is.EqualTo(2), "Populated");
				NUnit.Framework.Assert.That(typeof(INFFTAXGoodsItem).IsAssignableFrom(dataProvider.GoodsItems.First().GetType()), Is.EqualTo(true), "Is INFFTAXGoodsItem");
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			message = new GNTAXK();
			message.MetaData = new GNTAXKMetaData();
			message.Header = new GNTAXKHeader();
			message.Body = Array.Empty<GNTAXKGoodsItem>();
			dataProvider = new NFFTAXProvider(message);
		}
		GNTAXK message;
		INFFTAX dataProvider;

		protected override NFFTAXProvider GetProvider() => (NFFTAXProvider)dataProvider;
	}
}
