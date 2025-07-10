using System;
using CargoWise.Customs.DE.MessageContracts.AES;
using CargoWise.Customs.DE.MessageDefinitions.AESVersion3_0;
using Enterprise.Customs.DE.Messaging.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.DE.Messaging.AESVersion3_0.Testing
{
	[TestedType(typeof(EXPURGProvider))]
	sealed class EXPURGProviderTest : InboundDataProviderTestCase<IEXPURG, EXPURGProvider>
	{
		public void TestConstructorThrowsArgumentException()
		{
			AssertExceptionThrown<ArgumentException>(() => new EXPURGProvider(null));
		}

		[ExpectNoExceptions]
		public void TestLatestResponseDate()
		{
			message.ExportOperation = new DEXPUCExportOperation
			{
				limitForResponseDate = new DateTime(2021, 06, 30, 12, 12, 12)
			};
			NUnit.Framework.Assert.That(dataProvider.LatestResponseDate, Is.EqualTo(new DateTime(2021, 06, 30, 12, 12, 12)));
		}

		[ExpectNoExceptions]
		public void TestReferencedMessageIdentifier()
		{
			message.correlationIdentifier = "CUSCAN58750000000518175240519105043";
			NUnit.Framework.Assert.That(dataProvider.ReferencedMessageIdentifier, Is.EqualTo("CUSCAN58750000000518175240519105043"));
		}

		[ExpectNoExceptions]
		public void TestMessageIdentifier()
		{
			message.messageIdentification = "0000000001";
			NUnit.Framework.Assert.That(dataProvider.MessageIdentifier, Is.EqualTo("0000000001"));
		}

		[ExpectNoExceptions]
		public void TestMovementReferenceNumber()
		{
			message.ExportOperation.MRN = "22DE1234567";
			NUnit.Framework.Assert.That(dataProvider.MovementReferenceNumber, Is.EqualTo("22DE1234567"));
		}

		[ExpectNoExceptions]
		public void TestLocalReferenceNumber()
		{
			message.ExportOperation.LRN = "local reference number";
			NUnit.Framework.Assert.That(dataProvider.LocalReferenceNumber, Is.EqualTo("local reference number"));
		}

		[ExpectNoExceptions]
		public void TestLocalReferenceNumber_Null()
		{
			NUnit.Framework.Assert.That(dataProvider.LocalReferenceNumber, Is.EqualTo(default(string)));
		}

		protected override void SetUp()
		{
			base.SetUp();
			message = new DEXPUC();
			message.ExportOperation = new DEXPUCExportOperation();
			dataProvider = new EXPURGProvider(message);
		}
		DEXPUC message;
		IEXPURG dataProvider;

		protected override EXPURGProvider GetProvider() => (EXPURGProvider)dataProvider;
	}
}
