using System;
using System.Linq;
using CargoWise.Customs.DE.MessageDefinitions.ATLASVersion10_1;
using CargoWise.Types;
using Enterprise.Customs.DE.Messaging.Testing;
using NUnit.Framework;
using WTG.NUnit;

namespace Enterprise.Customs.DE.Messaging.ATLASVersion10_1.Testing
{
	[TestedType(typeof(ECWINFProvider))]
	sealed class ECWINFProviderTest : InboundDataProviderTestCase<IECWINF, ECWINFProvider>
	{
		public void TestConstructorThrowsArgumentException()
		{
			AssertExceptionThrown<ArgumentException>(() => new ECWINFProvider(null));
		}

		[ExpectNoExceptions]
		public void TestMessageIdentifier()
		{
			xmlObject.MetaData = new LECWIFMetaData()
			{
				MessageIdentifier = "0624532020"
			};
			NUnit.Framework.Assert.That(dataProvider.MessageIdentifier, Is.EqualTo("0624532020"));
		}

		[ExpectNoExceptions]
		public void TestMessageIdentifier_NoMetaData()
		{
			NUnit.Framework.Assert.That(dataProvider.MessageIdentifier, Is.EqualTo(default(string)));
		}

		[ExpectNoExceptions]
		public void TestReferenceNumber()
		{
			xmlObject.CustomsWarehouse = new LECWIFCustomsWarehouse()
			{
				AdditionalRegistrationNumber = "987654321"
			};
			NUnit.Framework.Assert.That(dataProvider.ReferenceNumber, Is.EqualTo("987654321").Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestMRN()
		{
			NUnit.Framework.Assert.That(xmlObject.CustomsWarehouse, Is.EqualTo(default(LECWIFCustomsWarehouse)), "Precondition - should be [null]");
			NUnit.Framework.Assert.That(dataProvider.MRN, Is.EqualTo(default(string)));

			xmlObject.CustomsWarehouse = new LECWIFCustomsWarehouse { MRN = "123" };
			NUnit.Framework.Assert.That(dataProvider.MRN, Is.EqualTo("123"));
		}

		[ExpectNoExceptions]
		public void TestReferenceNumber_NoCustomsWarehouse()
		{
			NUnit.Framework.Assert.That(dataProvider.ReferenceNumber, Is.EqualTo(ZString.Empty));
		}

		[ExpectNoExceptions]
		public void TestLocalReferenceNumber()
		{
			xmlObject.CustomsWarehouse = new LECWIFCustomsWarehouse()
			{
				LRN = "WTG1234"
			};
			NUnit.Framework.Assert.That(dataProvider.LocalReferenceNumber, Is.EqualTo("WTG1234").Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestLocalReferenceNumber_NoCustomsWarehouse()
		{
			NUnit.Framework.Assert.That(dataProvider.LocalReferenceNumber, Is.EqualTo(ZString.Empty));
		}

		[ExpectNoExceptions]
		public void TestGoodsItems()
		{
			xmlObject.CustomsWarehouse = new LECWIFCustomsWarehouse
			{
				GoodsItem = new LECWIFCustomsWarehouseGoodsItem[]
				{
					new LECWIFCustomsWarehouseGoodsItem(),
					new LECWIFCustomsWarehouseGoodsItem()
				}
			};

			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(dataProvider.GoodsItems.Count, Is.EqualTo(2), "Collection Count");
				NUnit.Framework.Assert.That(typeof(IECWINFGoodsItem).IsAssignableFrom(dataProvider.GoodsItems.First().GetType()), Is.EqualTo(true), "Is IECWINFGoodsItem");
			});
		}

		[ExpectNoExceptions]
		public void TestGoodsItems_NoCustomsWarehouse()
		{
			NUnit.Framework.Assert.That(dataProvider.GoodsItems, Is.EqualTo(Array.Empty<IECWINFGoodsItem>()));
		}

		protected override void SetUp()
		{
			base.SetUp();
			xmlObject = new LECWIF();
			dataProvider = new ECWINFProvider(xmlObject);
		}
		LECWIF xmlObject;
		ECWINFProvider dataProvider;

		protected override ECWINFProvider GetProvider() => dataProvider;
	}
}
