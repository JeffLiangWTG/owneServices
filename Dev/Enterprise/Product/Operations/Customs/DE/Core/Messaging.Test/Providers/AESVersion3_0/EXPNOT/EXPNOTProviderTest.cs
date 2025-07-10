using System;
using CargoWise.Customs.DE.MessageContracts.AES;
using CargoWise.Customs.DE.MessageDefinitions.AESVersion3_0;
using Enterprise.Customs.DE.Messaging.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.DE.Messaging.AESVersion3_0.Testing
{
	[TestedType(typeof(EXPNOTProvider))]
	sealed class EXPNOTProviderTest : InboundDataProviderTestCase<IEXPNOT, EXPNOTProvider>
	{
		public void TestConstructorThrowsArgumentException()
		{
			AssertExceptionThrown<ArgumentException>(() => new EXPNOTProvider(null));
		}

		[ExpectNoExceptions]
		public void TestMovementReferenceNumber()
		{
			message.ExportOperation = new DEXPNFExportOperation
			{
				MRN = "00DE000000000000E0"
			};
			NUnit.Framework.Assert.That(dataProvider.MovementReferenceNumber, Is.EqualTo("00DE000000000000E0"));
		}

		[ExpectNoExceptions]
		public void TestLocalReferenceNumber()
		{
			message.ExportOperation = new DEXPNFExportOperation
			{
				LRN = "local reference number"
			};
			NUnit.Framework.Assert.That(dataProvider.LocalReferenceNumber, Is.EqualTo("local reference number"));
		}

		[ExpectNoExceptions]
		public void TestLocalReferenceNumber_Null()
		{
			message.ExportOperation = new DEXPNFExportOperation();
			NUnit.Framework.Assert.That(dataProvider.LocalReferenceNumber, Is.EqualTo(default(string)));
		}

		[ExpectNoExceptions]
		public void TestExitDateTime()
		{
			message.ExportOperation = new DEXPNFExportOperation
			{
				exitDateAndTime = "2021-06-30T23:41:12"
			};
			NUnit.Framework.Assert.That(dataProvider.ExitDateTime, Is.EqualTo(new DateTime(2021, 06, 30, 23, 41, 12)));
		}

		[ExpectNoExceptions]
		public void TestExitDateTime_DateOnly()
		{
			message.ExportOperation = new DEXPNFExportOperation
			{
				exitDateAndTime = "2021-06-30"
			};
			NUnit.Framework.Assert.That(dataProvider.ExitDateTime, Is.EqualTo(new DateTime(2021, 06, 30)));
		}

		[ExpectNoExceptions]
		public void TestExitDateTime_NotSpecified()
		{
			message.ExportOperation = new DEXPNFExportOperation();
			NUnit.Framework.Assert.That(dataProvider.ExitDateTime, Is.Null);
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

		protected override void SetUp()
		{
			base.SetUp();
			message = new DEXPNF();
			dataProvider = new EXPNOTProvider(message);
		}
		DEXPNF message;
		IEXPNOT dataProvider;

		protected override EXPNOTProvider GetProvider() => (EXPNOTProvider)dataProvider;
	}
}
