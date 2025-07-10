using System;
using CargoWise.Customs.DE.MessageContracts.AES;
using CargoWise.Customs.DE.MessageDefinitions.AESVersion3_0;
using Enterprise.Customs.DE.Messaging.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.DE.Messaging.AESVersion3_0.Testing
{
	[TestedType(typeof(EXQSTAProvider))]
	sealed class EXQSTAProviderTest : InboundDataProviderTestCase<IEXQSTA, EXQSTAProvider>
	{
		public void TestConstructorThrowsArgumentException()
		{
			AssertExceptionThrown<ArgumentException>(() => new EXQSTAProvider(null));
		}

		[ExpectNoExceptions]
		public void TestMessageIdentifier()
		{
			message.messageIdentification = "Test1234";
			NUnit.Framework.Assert.That(dataProvider.MessageIdentifier, Is.EqualTo("Test1234"));
		}

		[ExpectNoExceptions]
		public void TestReferencedMessageIdentifier()
		{
			message.correlationIdentifier = "Test5678";
			NUnit.Framework.Assert.That(dataProvider.ReferencedMessageIdentifier, Is.EqualTo("Test5678"));
		}

		[ExpectNoExceptions]
		public void TestMovementReferenceNumber()
		{
			message.ExportOperation = new DEXQSBExportOperation
			{
				MRN = "22DE789087654"
			};
			NUnit.Framework.Assert.That(dataProvider.MovementReferenceNumber, Is.EqualTo("22DE789087654"));
		}

		protected override void SetUp()
		{
			base.SetUp();
			message = new DEXQSB();
			dataProvider = new EXQSTAProvider(message);
		}
		DEXQSB message;
		IEXQSTA dataProvider;

		protected override EXQSTAProvider GetProvider() => (EXQSTAProvider)dataProvider;
	}
}
