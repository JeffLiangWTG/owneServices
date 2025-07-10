using System;
using System.Linq;
using CargoWise.Customs.DE.MessageContracts;
using CargoWise.Customs.DE.MessageDefinitions.ATLASVersion10_1;
using Enterprise.Customs.DE.Messaging.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.DE.Messaging.ATLASVersion10_1.Testing
{
	[TestedType(typeof(ERRNCKProvider))]
	sealed class ERRNCKProviderTest : InboundDataProviderTestCase<IERRNCK, ERRNCKProvider>
	{
		public void TestConstructorThrowsArgumentException()
		{
			AssertExceptionThrown<ArgumentException>(() => new ERRNCKProvider(null));
		}

		[ExpectNoExceptions]
		public void TestMessageIdentifier()
		{
			message.MetaData.MessageIdentifier = "0624532020";
			NUnit.Framework.Assert.That(dataProvider.MessageIdentifier, Is.EqualTo("0624532020"));
		}

		[ExpectNoExceptions]
		public void TestReferencedMessageIdentifier()
		{
			message.Header.ReferencedMessageIdentifier = "TEST12345";
			NUnit.Framework.Assert.That(dataProvider.ReferencedMessageIdentifier, Is.EqualTo("TEST12345"));
		}

		[ExpectNoExceptions]
		public void TestReferenceNumber()
		{
			message.Header.ReferenceNumber = "ATB150000620520195875";
			NUnit.Framework.Assert.That(dataProvider.ReferenceNumber, Is.EqualTo("ATB150000620520195875"));
		}

		[ExpectNoExceptions]
		public void TestLocalReferenceNumber()
		{
			message.Header.LocalReferenceNumber = "LocalReference";
			NUnit.Framework.Assert.That(dataProvider.LocalReferenceNumber, Is.EqualTo("LocalReference"));
		}

		[ExpectNoExceptions]
		public void TestMessageGroup()
		{
			NUnit.Framework.Assert.That(dataProvider.MessageGroup, Is.EqualTo(default(string)));
		}

		[ExpectNoExceptions]
		public void TestErrors()
		{
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(dataProvider.Errors.Count, Is.EqualTo(2), "Collection Contains");
				NUnit.Framework.Assert.That(typeof(IERRNCKError).IsAssignableFrom(dataProvider.Errors.First().GetType()), Is.EqualTo(true), "Is ICUSCANGoodsItem");
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			message = new DEERRF
			{
				MetaData = new DEERRFMetaData(),
				Header = new DEERRFHeader(),
				Error = new DEERRFError[]
				{
					new DEERRFError(),
					new DEERRFError()
				}
			};
			dataProvider = new ERRNCKProvider(message);
		}
		DEERRF message;
		IERRNCK dataProvider;

		protected override ERRNCKProvider GetProvider() => (ERRNCKProvider)dataProvider;
	}
}
