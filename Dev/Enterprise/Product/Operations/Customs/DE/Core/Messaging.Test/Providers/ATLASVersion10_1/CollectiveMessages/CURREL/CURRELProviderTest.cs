using System;
using System.Linq;
using CargoWise.Customs.DE.MessageDefinitions.ATLASVersion10_1;
using Enterprise.Customs.DE.Messaging.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.DE.Messaging.ATLASVersion10_1.Testing
{
	[TestedType(typeof(CURRELProvider))]
	sealed class CURRELProviderTest : InboundDataProviderTestCase<ICURREL, CURRELProvider>
	{
		public void TestConstructorThrowsArgumentException()
		{
			AssertExceptionThrown<ArgumentException>(() => new CURRELProvider(null));
		}

		[ExpectNoExceptions]
		public void TestMessageIdentifier()
		{
			xmlObject.MetaData = new GCRELGMetaData()
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
		public void TestTemporaryReferenceNumber()
		{
			xmlObject.Header = new GCRELGHeader()
			{
				TemporaryReferenceNumber = "123456789"
			};
			NUnit.Framework.Assert.That(dataProvider.TemporaryReferenceNumber, Is.EqualTo("123456789"));
		}

		[ExpectNoExceptions]
		public void TestTemporaryReferenceNumber_NoHeader()
		{
			NUnit.Framework.Assert.That(dataProvider.TemporaryReferenceNumber, Is.EqualTo(default(string)));
		}

		[ExpectNoExceptions]
		public void TestReferenceNumber()
		{
			xmlObject.Header = new GCRELGHeader()
			{
				ReferenceNumber = "987654321"
			};
			NUnit.Framework.Assert.That(dataProvider.ReferenceNumber, Is.EqualTo("987654321"));
		}

		[ExpectNoExceptions]
		public void TestReferenceNumber_NoHeader()
		{
			NUnit.Framework.Assert.That(dataProvider.ReferenceNumber, Is.EqualTo(default(string)));
		}

		[ExpectNoExceptions]
		public void TestLocalReferenceNumber()
		{
			xmlObject.Header = new GCRELGHeader()
			{
				LRN = "MAS/22/11/22027"
			};
			NUnit.Framework.Assert.That(dataProvider.LocalReferenceNumber, Is.EqualTo("MAS/22/11/22027"));
		}

		[ExpectNoExceptions]
		public void TestLocalReferenceNumber_NoHeader()
		{
			NUnit.Framework.Assert.That(dataProvider.LocalReferenceNumber, Is.EqualTo(default(string)));
		}

		[ExpectNoExceptions]
		public void TestMRN()
		{
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(dataProvider.MRN, Is.EqualTo(default(string)));

				xmlObject.Header = new GCRELGHeader { MRN = "23DE12345678901234" };

				NUnit.Framework.Assert.That(dataProvider.MRN, Is.EqualTo("23DE12345678901234"));
			});
		}

		[ExpectNoExceptions]
		public void TestPresentationModalitiesNotification()
		{
			xmlObject.Header = new GCRELGHeader()
			{
				PresentationModalitiesNotification = "Notification for test"
			};
			NUnit.Framework.Assert.That(dataProvider.PresentationModalitiesNotification, Is.EqualTo("Notification for test"));
		}

		[ExpectNoExceptions]
		public void TestPresentationModalitiesNotification_NoHeader()
		{
			NUnit.Framework.Assert.That(dataProvider.PresentationModalitiesNotification, Is.EqualTo(default(string)));
		}

		[ExpectNoExceptions]
		public void TestGoodsItems()
		{
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(dataProvider.GoodsItems.Count, Is.EqualTo(2), "Collection Count");
				NUnit.Framework.Assert.That(typeof(ICURRELGoodsItem).IsAssignableFrom(dataProvider.GoodsItems.First().GetType()), Is.EqualTo(true), "Is ICURRELGoodsItem");
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			xmlObject = new GCRELG()
			{
				Body = new GCRELGGoodsItem[]
				{
					new GCRELGGoodsItem(),
					new GCRELGGoodsItem()
				}
			};
			dataProvider = new CURRELProvider(xmlObject);
		}
		GCRELG xmlObject;
		CURRELProvider dataProvider;

		protected override CURRELProvider GetProvider() => dataProvider;
	}
}
