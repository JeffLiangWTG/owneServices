using System;
using System.Linq;
using CargoWise.Customs.DE.MessageDefinitions.ATLASVersion10_1;
using Enterprise.Customs.DE.Messaging.Testing;
using NUnit.Framework;
using WTG.NUnit;

namespace Enterprise.Customs.DE.Messaging.ATLASVersion10_1.Testing
{
	[TestedType(typeof(CUSCANProvider))]
	sealed class CUSCANProviderTest : InboundDataProviderTestCase<ICUSCAN, CUSCANProvider>
	{
		public void TestConstructorThrowsArgumentException()
		{
			AssertExceptionThrown<ArgumentException>(() => new CUSCANProvider(null));
		}

		[ExpectNoExceptions]
		public void TestMessageIdentifier()
		{
			message.MetaData = new SCCANEMetaData()
			{
				MessageIdentifier = "CUSCAN58750000000518175240519105043"
			};
			NUnit.Framework.Assert.That(dataProvider.MessageIdentifier, Is.EqualTo("CUSCAN58750000000518175240519105043"));
		}

		[ExpectNoExceptions]
		public void TestReferenceNumber()
		{
			message.Header = new SCCANEHeader()
			{
				ReferenceNumber = "ATB150000620520195875"
			};
			NUnit.Framework.Assert.That(dataProvider.ReferenceNumber, Is.EqualTo("ATB150000620520195875").Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestMRN()
		{
			message.Header = new SCCANEHeader()
			{
				MRN = "24DE123050554788M5"
			};
			NUnit.Framework.Assert.That(dataProvider.MRN, Is.EqualTo("24DE123050554788M5"));
		}

		[ExpectNoExceptions]
		public void TestReason()
		{
			message.Header = new SCCANEHeader()
			{
				Reason = "This is the reason"
			};
			NUnit.Framework.Assert.That(dataProvider.Reason, Is.EqualTo("This is the reason").Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestGoodsItems()
		{
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(dataProvider.GoodsItems.Count, Is.EqualTo(2), "Collection Contains");
				NUnit.Framework.Assert.That(typeof(ICUSCANGoodsItem).IsAssignableFrom(dataProvider.GoodsItems.First().GetType()), Is.EqualTo(true), "Is ICUSCANGoodsItem");
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			message = new SCCANE()
			{
				Body = new SCCANEGoodsItem[]
				{
					new SCCANEGoodsItem(),
					new SCCANEGoodsItem()
				}
			};
			dataProvider = new CUSCANProvider(message);
		}
		SCCANE message;
		ICUSCAN dataProvider;

		protected override CUSCANProvider GetProvider() => (CUSCANProvider)dataProvider;
	}
}
