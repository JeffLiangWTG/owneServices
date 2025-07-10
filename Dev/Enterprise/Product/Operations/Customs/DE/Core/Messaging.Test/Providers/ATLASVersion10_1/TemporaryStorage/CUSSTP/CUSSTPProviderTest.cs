using System;
using System.Linq;
using CargoWise.Customs.DE.MessageDefinitions.ATLASVersion10_1;
using CargoWise.Types;
using Enterprise.Customs.DE.Messaging.Testing;
using NUnit.Framework;
using WTG.NUnit;

namespace Enterprise.Customs.DE.Messaging.ATLASVersion10_1.Testing
{
	[TestedType(typeof(CUSSTPProvider))]
	sealed class CUSSTPProviderTest : InboundDataProviderTestCase<ICUSSTP, CUSSTPProvider>
	{
		public void TestConstructorThrowsArgumentException()
		{
			AssertExceptionThrown<ArgumentException>(() => new CUSSTPProvider(null));
		}

		[ExpectNoExceptions]
		public void TestMessageIdentifier()
		{
			message.MetaData = new SCSTPCMetaData()
			{
				MessageIdentifier = "CUSCAN58750000000518175240519105043"
			};
			NUnit.Framework.Assert.That(dataProvider.MessageIdentifier, Is.EqualTo("CUSCAN58750000000518175240519105043"));
		}

		[ExpectNoExceptions]
		public void TestReferenceNumber()
		{
			message.Header = new SCSTPCHeader()
			{
				ReferenceNumber = "ATB150000890420195875"
			};
			NUnit.Framework.Assert.That(dataProvider.ReferenceNumber, Is.EqualTo("ATB150000890420195875").Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestGoodsItems()
		{
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(dataProvider.GoodsItems.Count, Is.EqualTo(2), "Collection Contains");
				NUnit.Framework.Assert.That(typeof(ICUSSTPGoodsItem).IsAssignableFrom(dataProvider.GoodsItems.First().GetType()), Is.EqualTo(true), "Is ICUSSTPGoodsItem");
			});
		}

		[ExpectNoExceptions]
		public void TestLocalReferenceNumber()
		{
			message.Header = new SCSTPCHeader()
			{
				LRN = "LocalRefNum"
			};
			NUnit.Framework.Assert.That(dataProvider.LocalReferenceNumber, Is.EqualTo("LocalRefNum").Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestMRN()
		{
			message.Header = new SCSTPCHeader() { MRN = "23DE586601055987B7" };

			NUnit.Framework.Assert.That(dataProvider.MRN, Is.EqualTo("23DE586601055987B7"));
		}

		[ExpectNoExceptions]
		public void TestNotificationDateTime()
		{
			message.Header = new SCSTPCHeader()
			{
				NotificationDateTime = new DateTime(2022, 11, 09, 10, 11, 00)
			};
			NUnit.Framework.Assert.That(dataProvider.NotificationDateTime, Is.EqualTo(new ZDateTime(2022, 11, 09, 10, 11, 00)));
		}

		protected override void SetUp()
		{
			base.SetUp();
			message = new SCSTPC()
			{
				Body = new SCSTPCGoodsItem[]
				{
					new SCSTPCGoodsItem(),
					new SCSTPCGoodsItem()
				}
			};
			dataProvider = new CUSSTPProvider(message);
		}
		SCSTPC message;
		ICUSSTP dataProvider;

		protected override CUSSTPProvider GetProvider() => (CUSSTPProvider)dataProvider;
	}
}
