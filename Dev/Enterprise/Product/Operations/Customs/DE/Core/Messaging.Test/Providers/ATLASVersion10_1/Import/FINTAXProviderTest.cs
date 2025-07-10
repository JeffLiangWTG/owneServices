using System;
using CargoWise.Customs.DE.MessageDefinitions.ATLASVersion10_1;
using Enterprise.Customs.DE.Messaging.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.DE.Messaging.ATLASVersion10_1.Testing
{
	[TestedType(typeof(FINTAXProvider))]
	sealed class FINTAXProviderTest : InboundDataProviderTestCase<IFINTAX, FINTAXProvider>
	{
		[ExpectNoExceptions]
		public void TestMRN() => CombineAssertions(() =>
		{
			xmlObject.Header.MRN = "24DE12345678901234";
			NUnit.Framework.Assert.That(dataProvider.MRN, Is.EqualTo("24DE12345678901234"), "When Header/MRN is set");
			xmlObject.Header = null;
			NUnit.Framework.Assert.That(dataProvider.MRN, Is.EqualTo(default(string)), "When no Header is set - should be [null]");
		});

		public void TestConstructorThrowsArgumentException()
		{
			AssertExceptionThrown<ArgumentException>(() => new FINTAXProvider(null));
		}

		[ExpectNoExceptions]
		public void TestMessageIdentifier()
		{
			xmlObject.MetaData.MessageIdentifier = "0624532020";
			NUnit.Framework.Assert.That(dataProvider.MessageIdentifier, Is.EqualTo("0624532020"));
		}

		[ExpectNoExceptions]
		public void TestMessageIdentifier_NoMetaData()
		{
			xmlObject.MetaData = null;
			NUnit.Framework.Assert.That(dataProvider.MessageIdentifier, Is.EqualTo(default(string)));
		}

		[ExpectNoExceptions]
		public void TestReferenceNumber()
		{
			xmlObject.Header.ReferenceNumber = "987654321";
			NUnit.Framework.Assert.That(dataProvider.ReferenceNumber, Is.EqualTo("987654321"));
		}

		[ExpectNoExceptions]
		public void TestReferenceNumber_NoHeader()
		{
			xmlObject.Header = null;
			NUnit.Framework.Assert.That(dataProvider.ReferenceNumber, Is.EqualTo(default(string)));
		}

		[ExpectNoExceptions]
		public void TestLocalReferenceNumber()
		{
			xmlObject.Header.LRN = "LOCALREFERENCENUMBER";
			NUnit.Framework.Assert.That(dataProvider.LocalReferenceNumber, Is.EqualTo("LOCALREFERENCENUMBER"));
		}

		[ExpectNoExceptions]
		public void TestLocalReferenceNumber_NoHeader()
		{
			xmlObject.Header = null;
			NUnit.Framework.Assert.That(dataProvider.LocalReferenceNumber, Is.EqualTo(default(string)));
		}

		protected override void SetUp()
		{
			base.SetUp();
			xmlObject = new FFTAXE
			{
				MetaData = new FFTAXEMetaData(),
				Header = new FFTAXEHeader()
			};
			dataProvider = new FINTAXProvider(xmlObject);
		}
		FFTAXE xmlObject;
		FINTAXProvider dataProvider;

		protected override FINTAXProvider GetProvider() => dataProvider;
	}
}
