using System;
using CargoWise.Customs.DE.MessageContracts.AES;
using CargoWise.Customs.DE.MessageDefinitions.AESVersion3_0;
using Enterprise.Customs.DE.Messaging.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.DE.Messaging.AESVersion3_0.Testing
{
	[TestedType(typeof(EXPRELProvider))]
	sealed class EXPRELProviderTest : InboundDataProviderTestCase<IEXPREL, EXPRELProvider>
	{
		public void TestConstructorThrowsArgumentException()
		{
			AssertExceptionThrown<ArgumentException>(() => new EXPRELProvider(null));
		}

		[ExpectNoExceptions]
		public void TestMovementReferenceNumber()
		{
			message.ExportOperation = new DEXPRFExportOperation
			{
				MRN = "00DE000000000000E0"
			};
			NUnit.Framework.Assert.That(dataProvider.MovementReferenceNumber, Is.EqualTo("00DE000000000000E0"));
		}

		[ExpectNoExceptions]
		public void TestLocalReferenceNumber()
		{
			message.ExportOperation = new DEXPRFExportOperation
			{
				LRN = "local reference number"
			};
			NUnit.Framework.Assert.That(dataProvider.LocalReferenceNumber, Is.EqualTo("local reference number"));
		}

		[ExpectNoExceptions]
		public void TestLocalReferenceNumber_Null()
		{
			message.ExportOperation = new DEXPRFExportOperation();
			NUnit.Framework.Assert.That(dataProvider.LocalReferenceNumber, Is.EqualTo(default(string)));
		}

		[ExpectNoExceptions]
		public void TestIssuingDateTime()
		{
			message.ExportOperation = new DEXPRFExportOperation
			{
				releaseDateAndTime = new DateTime(2021, 06, 30, 12, 12, 12)
			};
			NUnit.Framework.Assert.That(dataProvider.IssuingDateTime, Is.EqualTo(new DateTime(2021, 06, 30, 12, 12, 12)));
		}

		[ExpectNoExceptions]
		public void TestMessageIdentifier()
		{
			message.messageIdentification = "0000000001";
			NUnit.Framework.Assert.That(dataProvider.MessageIdentifier, Is.EqualTo("0000000001"));
		}

		[ExpectNoExceptions]
		public void TestReferencedMessageIdentifier()
		{
			message.correlationIdentifier = "CUSCAN58750000000518175240519105043";
			NUnit.Framework.Assert.That(dataProvider.ReferencedMessageIdentifier, Is.EqualTo("CUSCAN58750000000518175240519105043"));
		}

		protected override void SetUp()
		{
			base.SetUp();
			message = new DEXPRF();
			dataProvider = new EXPRELProvider(message);
		}
		DEXPRF message;
		IEXPREL dataProvider;

		protected override EXPRELProvider GetProvider() => (EXPRELProvider)dataProvider;
	}
}
