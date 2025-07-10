using System;
using System.Linq;
using CargoWise.Customs.DE.MessageContracts;
using CargoWise.Customs.DE.MessageDefinitions.AESVersion3_0;
using Enterprise.Customs.DE.Messaging.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.DE.Messaging.AESVersion3_0.Testing
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
			message.messageIdentification = "0003099984";
			NUnit.Framework.Assert.That(dataProvider.MessageIdentifier, Is.EqualTo("0003099984"));
		}

		[ExpectNoExceptions]
		public void TestReferencedMessageIdentifier()
		{
			message.correlationIdentifier = "00000000000006";
			NUnit.Framework.Assert.That(dataProvider.ReferencedMessageIdentifier, Is.EqualTo("00000000000006"));
		}

		[ExpectNoExceptions]
		public void TestReferenceNumber()
		{
			message.Header.MRN = "00DE000000000000E0";
			NUnit.Framework.Assert.That(dataProvider.ReferenceNumber, Is.EqualTo("00DE000000000000E0"));
		}

		[ExpectNoExceptions]
		public void TestReferenceNumber_HeaderNotSpecified()
		{
			message.Header = null;
			NUnit.Framework.Assert.That(dataProvider.ReferenceNumber, Is.EqualTo(default(string)));
		}

		[ExpectNoExceptions]
		public void TestLocalReferenceNumber()
		{
			message.Header.LRN = "local reference number";
			NUnit.Framework.Assert.That(dataProvider.LocalReferenceNumber, Is.EqualTo("local reference number"));
		}

		[ExpectNoExceptions]
		public void TestLocalReferenceNumber_HeaderNotSpecified()
		{
			message.Header = null;
			NUnit.Framework.Assert.That(dataProvider.LocalReferenceNumber, Is.EqualTo(default(string)));
		}

		[ExpectNoExceptions]
		public void TestMessageGroup()
		{
			message.messageGroup = DEERRGMessageGroup.EXP;
			NUnit.Framework.Assert.That(dataProvider.MessageGroup, Is.EqualTo("EXP"));
		}

		[ExpectNoExceptions]
		public void TestErrors()
		{
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(dataProvider.Errors.Count, Is.EqualTo(2), "Collection Contains");
				NUnit.Framework.Assert.That(typeof(IERRNCKError).IsAssignableFrom(dataProvider.Errors.First().GetType()), Is.EqualTo(true), "Is IERRNCKError");
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			message = new DEERRG
			{
				Header = new DEERRGHeader(),
				Error = new DEERRGError[]
				{
					new DEERRGError(),
					new DEERRGError()
				}
			};
			dataProvider = new ERRNCKProvider(message);
		}
		DEERRG message;
		IERRNCK dataProvider;

		protected override ERRNCKProvider GetProvider() => (ERRNCKProvider)dataProvider;
	}
}
